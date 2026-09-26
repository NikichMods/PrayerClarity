using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class TechnologyTooltipViewportClamp
    {
        private const float SafeMarginPixels = 24f;
        private const float SelectedUnlockGapPixels = 12f;
        private const string GamepadTooltipPositionFixHarmonyId = "nikich.gyk.movegamepadtooltips";

        private sealed class Marker
        {
            internal static readonly Marker Instance = new Marker();
            private Marker() { }
        }

        private sealed class BubbleState
        {
            internal readonly Component Root;
            internal readonly object Tooltip;
            internal BubbleState(Component root, object tooltip)
            {
                Root = root;
                Tooltip = tooltip;
            }
        }

        private sealed class AvoidanceState
        {
            internal readonly object Widget;
            internal readonly Component Component;
            internal AvoidanceState(object widget, Component component)
            {
                Widget = widget;
                Component = component;
            }
        }

        private static readonly ConditionalWeakTable<object, Marker> OwnedTooltips = new ConditionalWeakTable<object, Marker>();
        private static readonly ConditionalWeakTable<object, BubbleState> OwnedBubbles = new ConditionalWeakTable<object, BubbleState>();
        private static readonly ConditionalWeakTable<object, AvoidanceState> AvoidanceTargets = new ConditionalWeakTable<object, AvoidanceState>();

        private static ManualLogSource _log;
        private static bool _installed;
        private static bool _runtimeFailed;
        private static Type _uiRootType;
        private static Func<object, object> _linkedTooltipGetter;
        private static Func<object, object> _widgetGetter;
        private static Func<object, int> _widgetWidthGetter;
        private static Func<object, int> _widgetHeightGetter;
        private static Func<object, Vector2> _widgetPivotOffsetGetter;
        private static Func<object, int> _manualHeightGetter;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type tooltipType = R.GameType("Tooltip");
            Type bubbleType = R.GameType("WidgetsBubbleGUI");
            _uiRootType = R.AnyType("UIRoot");
            if (tooltipType == null || bubbleType == null || _uiRootType == null)
                throw new MissingMemberException("Tooltip / WidgetsBubbleGUI / UIRoot type is unavailable.");

            MemberInfo linkedTooltip = RequireMember(tooltipType, "linked_tooltip");
            MemberInfo widget = RequireMember(bubbleType, "widget");
            Type widgetType = MemberType(widget);
            MemberInfo width = RequireMember(widgetType, "width");
            MemberInfo height = RequireMember(widgetType, "height");
            MemberInfo pivotOffset = FindMember(widgetType, "pivotOffset");
            MemberInfo manualHeight = RequireMember(_uiRootType, "manualHeight");

            _linkedTooltipGetter = CompileGetter<object>(tooltipType, linkedTooltip);
            _widgetGetter = CompileGetter<object>(bubbleType, widget);
            _widgetWidthGetter = CompileGetter<int>(widgetType, width);
            _widgetHeightGetter = CompileGetter<int>(widgetType, height);
            if (pivotOffset != null)
                _widgetPivotOffsetGetter = CompileGetter<Vector2>(widgetType, pivotOffset);
            _manualHeightGetter = CompileGetter<int>(_uiRootType, manualHeight);

            MethodInfo tooltipShow = R.Method(tooltipType, "Show", false, new[] { typeof(bool) });
            MethodInfo tooltipClear = R.Method(tooltipType, "ClearData", false, 0);
            MethodInfo bubbleUpdate = R.Method(bubbleType, "Update", false, 0);
            if (tooltipShow == null) throw new MissingMethodException("Tooltip.Show(bool)");
            if (tooltipClear == null) throw new MissingMethodException("Tooltip.ClearData()");
            if (bubbleUpdate == null) throw new MissingMethodException("WidgetsBubbleGUI.Update()");

            R.Patch(harmonyId + ".techviewport.show", typeof(TechnologyTooltipViewportClamp), tooltipShow, nameof(TooltipShowPostfix));
            R.Patch(harmonyId + ".techviewport.clear", typeof(TechnologyTooltipViewportClamp), tooltipClear, nameof(TooltipClearPostfix));
            PatchAfter(
                harmonyId + ".techviewport.update",
                bubbleUpdate,
                nameof(BubbleUpdatePostfix),
                new[] { GamepadTooltipPositionFixHarmonyId });

            _installed = true;
        }

        internal static void MarkTechnologyTooltip(object tooltip)
        {
            if (!_installed || tooltip == null) return;
            OwnedTooltips.Remove(tooltip);
            OwnedTooltips.Add(tooltip, Marker.Instance);
        }

        internal static void SetSelectedUnlockAvoidance(object tooltip, object widget)
        {
            if (!_installed || tooltip == null) return;

            AvoidanceTargets.Remove(tooltip);
            Component component = widget as Component;
            if (widget != null && component != null)
                AvoidanceTargets.Add(tooltip, new AvoidanceState(widget, component));
        }

        internal static void ClearSelectedUnlockAvoidance(object tooltip)
        {
            if (tooltip != null) AvoidanceTargets.Remove(tooltip);
        }

        private static void TooltipClearPostfix(object __instance)
        {
            if (__instance == null) return;
            OwnedTooltips.Remove(__instance);
            AvoidanceTargets.Remove(__instance);
        }

        private static void TooltipShowPostfix(object __instance)
        {
            if (_runtimeFailed || __instance == null) return;

            Marker marker;
            if (!OwnedTooltips.TryGetValue(__instance, out marker)) return;

            try
            {
                object bubble = _linkedTooltipGetter(__instance);
                Component component = bubble as Component;
                if (component == null) return;

                Transform rootTransform = component.transform.root;
                Component root = rootTransform == null ? null : rootTransform.GetComponent(_uiRootType);
                if (root == null) return;

                OwnedBubbles.Remove(bubble);
                OwnedBubbles.Add(bubble, new BubbleState(root, __instance));
            }
            catch (Exception ex)
            {
                DisableAfterRuntimeFailure("linking Technology tooltip bubble", ex);
            }
        }

        private static void BubbleUpdatePostfix(object __instance)
        {
            if (_runtimeFailed || __instance == null) return;

            BubbleState state;
            if (!OwnedBubbles.TryGetValue(__instance, out state)) return;

            try
            {
                Component bubble = __instance as Component;
                if (bubble == null || state.Root == null) return;

                object widget = _widgetGetter(__instance);
                if (widget == null) return;

                int width = _widgetWidthGetter(widget);
                int height = _widgetHeightGetter(widget);
                int manualHeight = _manualHeightGetter(state.Root);
                int screenWidth = Screen.width;
                int screenHeight = Screen.height;
                if (width <= 0 || height <= 0 || manualHeight <= 0 || screenWidth <= 0 || screenHeight <= 0) return;

                float scale = manualHeight / (float)screenHeight;
                Rect safe = Screen.safeArea;
                float halfScreenWidth = screenWidth * 0.5f;
                float halfScreenHeight = screenHeight * 0.5f;

                float left = (safe.xMin - halfScreenWidth) * scale;
                float right = (safe.xMax - halfScreenWidth) * scale;
                float bottom = (safe.yMin - halfScreenHeight) * scale;
                float top = (safe.yMax - halfScreenHeight) * scale;
                float margin = SafeMarginPixels * scale;

                Vector3 position = bubble.transform.localPosition;
                float x = ClampAxis(position.x, left, right, width, margin);
                float y = ClampAxis(position.y, bottom, top, height, margin);

                if (Math.Abs(x - position.x) > 0.01f || Math.Abs(y - position.y) > 0.01f)
                    bubble.transform.localPosition = new Vector3(x, y, position.z);

                AvoidSelectedUnlock(
                    bubble,
                    widget,
                    state,
                    left + margin,
                    right - margin,
                    bottom + margin,
                    top - margin,
                    SelectedUnlockGapPixels * scale);
            }
            catch (Exception ex)
            {
                DisableAfterRuntimeFailure("clamping Technology tooltip bubble", ex);
            }
        }

        private static void AvoidSelectedUnlock(
            Component bubble,
            object bubbleWidget,
            BubbleState state,
            float safeLeft,
            float safeRight,
            float safeBottom,
            float safeTop,
            float gap)
        {
            if (_widgetPivotOffsetGetter == null || state == null || state.Tooltip == null)
                return;

            AvoidanceState target;
            if (!AvoidanceTargets.TryGetValue(state.Tooltip, out target) ||
                target == null ||
                target.Component == null ||
                !target.Component.gameObject.activeInHierarchy)
                return;

            Rect bubbleRect;
            Rect targetRect;
            if (!TryGetRectInRoot(bubbleWidget, bubble, state.Root, out bubbleRect) ||
                !TryGetRectInRoot(target.Widget, target.Component, state.Root, out targetRect))
                return;

            Rect keepOut = Rect.MinMaxRect(
                targetRect.xMin - gap,
                targetRect.yMin - gap,
                targetRect.xMax + gap,
                targetRect.yMax + gap);

            if (!Overlaps(bubbleRect, keepOut)) return;

            Vector2 best = Vector2.zero;
            float bestScore = float.PositiveInfinity;
            bool found = false;

            EvaluateCandidate(
                bubbleRect,
                keepOut,
                new Vector2(keepOut.xMin - bubbleRect.xMax, 0f),
                0f,
                safeLeft,
                safeRight,
                safeBottom,
                safeTop,
                ref found,
                ref best,
                ref bestScore);
            EvaluateCandidate(
                bubbleRect,
                keepOut,
                new Vector2(keepOut.xMax - bubbleRect.xMin, 0f),
                0f,
                safeLeft,
                safeRight,
                safeBottom,
                safeTop,
                ref found,
                ref best,
                ref bestScore);

            // Horizontal separation is the product rule. Vertical separation is only
            // a fallback for narrow aspect ratios / unusually wide localizations.
            EvaluateCandidate(
                bubbleRect,
                keepOut,
                new Vector2(0f, keepOut.yMin - bubbleRect.yMax),
                1000000f,
                safeLeft,
                safeRight,
                safeBottom,
                safeTop,
                ref found,
                ref best,
                ref bestScore);
            EvaluateCandidate(
                bubbleRect,
                keepOut,
                new Vector2(0f, keepOut.yMax - bubbleRect.yMin),
                1000000f,
                safeLeft,
                safeRight,
                safeBottom,
                safeTop,
                ref found,
                ref best,
                ref bestScore);

            if (!found || best.sqrMagnitude <= 0.0001f) return;

            Transform root = state.Root.transform;
            Vector3 currentRoot = root.InverseTransformPoint(bubble.transform.position);
            Vector3 desiredRoot = currentRoot + new Vector3(best.x, best.y, 0f);
            bubble.transform.position = root.TransformPoint(desiredRoot);
        }

        private static void EvaluateCandidate(
            Rect bubble,
            Rect keepOut,
            Vector2 requested,
            float directionPenalty,
            float safeLeft,
            float safeRight,
            float safeBottom,
            float safeTop,
            ref bool found,
            ref Vector2 best,
            ref float bestScore)
        {
            Vector2 delta = requested;
            Rect moved = OffsetRect(bubble, delta);

            if (moved.xMin < safeLeft) delta.x += safeLeft - moved.xMin;
            if (moved.xMax > safeRight) delta.x += safeRight - moved.xMax;
            if (moved.yMin < safeBottom) delta.y += safeBottom - moved.yMin;
            if (moved.yMax > safeTop) delta.y += safeTop - moved.yMax;

            moved = OffsetRect(bubble, delta);
            const float epsilon = 0.01f;
            if (moved.xMin < safeLeft - epsilon ||
                moved.xMax > safeRight + epsilon ||
                moved.yMin < safeBottom - epsilon ||
                moved.yMax > safeTop + epsilon ||
                Overlaps(moved, keepOut))
                return;

            float score = directionPenalty + delta.sqrMagnitude;
            if (found && score >= bestScore) return;

            found = true;
            best = delta;
            bestScore = score;
        }

        private static Rect OffsetRect(Rect rect, Vector2 delta)
        {
            return Rect.MinMaxRect(
                rect.xMin + delta.x,
                rect.yMin + delta.y,
                rect.xMax + delta.x,
                rect.yMax + delta.y);
        }

        private static bool Overlaps(Rect a, Rect b)
        {
            return a.xMin < b.xMax &&
                   a.xMax > b.xMin &&
                   a.yMin < b.yMax &&
                   a.yMax > b.yMin;
        }

        private static bool TryGetRectInRoot(
            object widget,
            Component component,
            Component root,
            out Rect rect)
        {
            rect = new Rect();
            if (widget == null || component == null || root == null || _widgetPivotOffsetGetter == null)
                return false;

            int width = _widgetWidthGetter(widget);
            int height = _widgetHeightGetter(widget);
            if (width <= 0 || height <= 0) return false;

            Vector2 pivot = _widgetPivotOffsetGetter(widget);
            float x0 = -pivot.x * width;
            float y0 = -pivot.y * height;
            float x1 = x0 + width;
            float y1 = y0 + height;

            Transform widgetTransform = component.transform;
            Transform rootTransform = root.transform;

            Vector3 p0 = rootTransform.InverseTransformPoint(widgetTransform.TransformPoint(new Vector3(x0, y0, 0f)));
            Vector3 p1 = rootTransform.InverseTransformPoint(widgetTransform.TransformPoint(new Vector3(x0, y1, 0f)));
            Vector3 p2 = rootTransform.InverseTransformPoint(widgetTransform.TransformPoint(new Vector3(x1, y0, 0f)));
            Vector3 p3 = rootTransform.InverseTransformPoint(widgetTransform.TransformPoint(new Vector3(x1, y1, 0f)));

            float minX = Math.Min(Math.Min(p0.x, p1.x), Math.Min(p2.x, p3.x));
            float maxX = Math.Max(Math.Max(p0.x, p1.x), Math.Max(p2.x, p3.x));
            float minY = Math.Min(Math.Min(p0.y, p1.y), Math.Min(p2.y, p3.y));
            float maxY = Math.Max(Math.Max(p0.y, p1.y), Math.Max(p2.y, p3.y));

            if (maxX <= minX || maxY <= minY) return false;
            rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return true;
        }

        private static float ClampAxis(float value, float low, float high, float size, float margin)
        {
            float min = low + margin + size * 0.5f;
            float max = high - margin - size * 0.5f;
            if (min > max) return value;
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static void DisableAfterRuntimeFailure(string operation, Exception ex)
        {
            if (_runtimeFailed) return;
            _runtimeFailed = true;
            _log?.LogError("PC_RUNTIME_FALLBACK feature=technology-viewport action=vanilla-placement operation=\"" + operation + "\" " + ex);
        }

        private static void PatchAfter(string harmonyId, MethodInfo target, string postfixName, string[] afterOwners)
        {
            Type harmonyType = R.AnyType("HarmonyLib.Harmony");
            Type harmonyMethodType = R.AnyType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null) throw new InvalidOperationException("Harmony unavailable");

            MethodInfo postfix = typeof(TechnologyTooltipViewportClamp).GetMethod(postfixName, R.Stat);
            if (postfix == null) throw new MissingMethodException(typeof(TechnologyTooltipViewportClamp).FullName, postfixName);

            object harmonyPostfix = Activator.CreateInstance(harmonyMethodType, new object[] { postfix });
            bool ordered = false;

            FieldInfo afterField = harmonyMethodType.GetField("after", R.Inst);
            if (afterField != null && afterField.FieldType == typeof(string[]))
            {
                afterField.SetValue(harmonyPostfix, afterOwners);
                ordered = true;
            }
            else
            {
                PropertyInfo afterProperty = harmonyMethodType.GetProperty("after", R.Inst);
                if (afterProperty != null && afterProperty.CanWrite && afterProperty.PropertyType == typeof(string[]))
                {
                    afterProperty.SetValue(harmonyPostfix, afterOwners, null);
                    ordered = true;
                }
            }

            if (!ordered)
                throw new MissingMemberException("HarmonyMethod.after is unavailable; viewport clamp ordering cannot be guaranteed.");

            R.TrackPatchOwner(harmonyId);
            object harmony = Activator.CreateInstance(harmonyType, new object[] { harmonyId });
            MethodInfo patch = harmonyType.GetMethods(R.Inst)
                .FirstOrDefault(m => m.Name == "Patch" && m.GetParameters().Length >= 5 && typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = null;
            args[2] = harmonyPostfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static MemberInfo RequireMember(Type type, string name)
        {
            MemberInfo member = FindMember(type, name);
            if (member != null) return member;
            throw new MissingMemberException(type.FullName, name);
        }

        private static MemberInfo FindMember(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, R.Inst);
                if (field != null) return field;
                PropertyInfo property = current.GetProperty(name, R.Inst);
                if (property != null && property.CanRead) return property;
            }
            return null;
        }

        private static Type MemberType(MemberInfo member)
        {
            FieldInfo field = member as FieldInfo;
            if (field != null) return field.FieldType;
            PropertyInfo property = member as PropertyInfo;
            if (property != null) return property.PropertyType;
            throw new NotSupportedException("Unsupported member type: " + member.MemberType);
        }

        private static Func<object, T> CompileGetter<T>(Type declaringType, MemberInfo member)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            Expression typed = Expression.Convert(instance, declaringType);
            Expression access;

            FieldInfo field = member as FieldInfo;
            if (field != null)
                access = Expression.Field(typed, field);
            else
            {
                PropertyInfo property = member as PropertyInfo;
                if (property == null) throw new NotSupportedException("Unsupported member type: " + member.MemberType);
                access = Expression.Property(typed, property);
            }

            return Expression.Lambda<Func<object, T>>(Expression.Convert(access, typeof(T)), instance).Compile();
        }
    }
}
