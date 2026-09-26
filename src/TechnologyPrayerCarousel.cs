using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class TechnologyPrayerCarousel
    {

        private static readonly BindingFlags Inst =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags Stat =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private sealed class State
        {
            internal object Item;
            internal object Tooltip;
            internal readonly List<object> Unlocks = new List<object>();
            internal readonly List<string> PrayerLabels = new List<string>();
            internal readonly List<object> Children = new List<object>();
            internal readonly Dictionary<object, Color> OriginalSpriteColors =
                new Dictionary<object, Color>(ReferenceComparer.Instance);
            internal int SelectedIndex;
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return RuntimeHelpers.GetHashCode(obj); }
        }

        private static ManualLogSource _log;
        private static string _harmonyId;
        private static Type _techTreeGuiType;
        private static Type _tooltipType;
        private static MethodInfo _resolvePrayerCrafts;
        private static MethodInfo _tooltipClearData;
        private static MethodInfo _tooltipsRedraw;
        private static readonly Dictionary<object, State> States =
            new Dictionary<object, State>(ReferenceComparer.Instance);
        private static State _active;
        private static int _pendingHorizontalDirection;
        private static int _pendingHorizontalFrame = -1;
        private static bool _runtimeErrorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            if (string.IsNullOrEmpty(harmonyId)) throw new ArgumentNullException(nameof(harmonyId));
            _harmonyId = harmonyId + ".technology-prayer-carousel";
            _log = log;
            Type techTreeItemType = FindType("TechTreeGUIItem");
            _techTreeGuiType = FindType("TechTreeGUI");
            Type baseGuiType = FindType("BaseGUI");
            _tooltipType = FindType("Tooltip");
            Type tooltipsManagerType = FindType("TooltipsManager");
            Type presentationType = FindType("PrayerClarity.SecondarySurfacePresentation");

            if (techTreeItemType == null) throw new MissingMemberException("TechTreeGUIItem");
            if (_techTreeGuiType == null) throw new MissingMemberException("TechTreeGUI");
            if (baseGuiType == null) throw new MissingMemberException("BaseGUI");
            if (_tooltipType == null) throw new MissingMemberException("Tooltip");
            if (tooltipsManagerType == null) throw new MissingMemberException("TooltipsManager");
            if (presentationType == null)
                throw new MissingMemberException("PrayerClarity.SecondarySurfacePresentation");

            MethodInfo initGamepadTooltip = techTreeItemType.GetMethods(Inst)
                .FirstOrDefault(m =>
                    m.Name == "InitGamepadTooltip" &&
                    m.GetParameters().Length == 1);
            MethodInfo onGamepadOver = techTreeItemType.GetMethod(
                "OnGamepadOver", Inst, null, Type.EmptyTypes, null);
            MethodInfo onGamepadOut = techTreeItemType.GetMethod(
                "OnGamepadOut", Inst, null, Type.EmptyTypes, null);
            MethodInfo left = baseGuiType.GetMethod(
                "OnPressedLeft", Inst, null, Type.EmptyTypes, null);
            MethodInfo right = baseGuiType.GetMethod(
                "OnPressedRight", Inst, null, Type.EmptyTypes, null);

            _resolvePrayerCrafts = presentationType.GetMethod(
                "ResolvePrayerCrafts", Stat, null, new[] { typeof(object) }, null);
            _tooltipClearData = _tooltipType.GetMethod(
                "ClearData", Inst, null, Type.EmptyTypes, null);
            _tooltipsRedraw = tooltipsManagerType.GetMethod(
                "Redraw", Stat, null, Type.EmptyTypes, null);

            if (initGamepadTooltip == null)
                throw new MissingMethodException("TechTreeGUIItem.InitGamepadTooltip");
            if (onGamepadOver == null)
                throw new MissingMethodException("TechTreeGUIItem.OnGamepadOver");
            if (onGamepadOut == null)
                throw new MissingMethodException("TechTreeGUIItem.OnGamepadOut");
            if (left == null)
                throw new MissingMethodException("BaseGUI.OnPressedLeft");
            if (right == null)
                throw new MissingMethodException("BaseGUI.OnPressedRight");
            if (_resolvePrayerCrafts == null)
                throw new MissingMethodException("SecondarySurfacePresentation.ResolvePrayerCrafts(object)");
            if (_tooltipClearData == null)
                throw new MissingMethodException("Tooltip.ClearData()");
            if (_tooltipsRedraw == null)
                throw new MissingMethodException("TooltipsManager.Redraw()");

            Patch(initGamepadTooltip, null, nameof(InitGamepadTooltipPostfix));
            Patch(onGamepadOver, null, nameof(OnGamepadOverPostfix));
            Patch(onGamepadOut, null, nameof(OnGamepadOutPostfix));
            Patch(left, nameof(LeftPrefix), null);
            Patch(right, nameof(RightPrefix), null);

            _log?.LogInfo(
                "PrayerClarity prayer-Technology navigation enabled: on gamepad, Left/Right traverses visible unlocks one at a time in any multi-unlock Technology that contains a prayer, then falls through to native Technology navigation at the outer edges.");
        }

        private static void InitGamepadTooltipPostfix(object __instance, object __0)
        {
            try
            {
                State oldState;
                if (States.TryGetValue(__instance, out oldState))
                {
                    RestoreHighlight(oldState);
                    States.Remove(__instance);
                    if (ReferenceEquals(_active, oldState)) _active = null;
                }

                State state = TryBuildState(__instance, __0);
                if (state == null) return;

                States[__instance] = state;
                RebuildSelectedTooltip(state, false);
            }
            catch (Exception ex)
            {
                LogRuntimeFailure("PC_CAROUSEL_INIT_FAILED ", ex);
            }
        }

        private static void OnGamepadOverPostfix(object __instance)
        {
            try
            {
                State state;
                if (!States.TryGetValue(__instance, out state)) return;

                _active = state;

                if (_pendingHorizontalFrame == Time.frameCount)
                {
                    if (_pendingHorizontalDirection > 0)
                        state.SelectedIndex = 0;
                    else if (_pendingHorizontalDirection < 0)
                        state.SelectedIndex = state.Unlocks.Count - 1;

                    RebuildSelectedTooltip(state, true);
                }

                _pendingHorizontalDirection = 0;
                _pendingHorizontalFrame = -1;

                ApplyHighlight(state);
            }
            catch (Exception ex)
            {
                LogRuntimeFailure("PC_CAROUSEL_FOCUS_FAILED ", ex);
            }
        }

        private static void OnGamepadOutPostfix(object __instance)
        {
            try
            {
                State state;
                if (!States.TryGetValue(__instance, out state)) return;

                RestoreHighlight(state);
                TechnologyTooltipViewportClamp.ClearSelectedUnlockAvoidance(state.Tooltip);
                if (ReferenceEquals(_active, state)) _active = null;
            }
            catch (Exception ex)
            {
                LogRuntimeFailure("PC_CAROUSEL_UNFOCUS_FAILED ", ex);
            }
        }

        private static bool LeftPrefix(object __instance, ref bool __result)
        {
            return HandleHorizontal(__instance, -1, ref __result);
        }

        private static bool RightPrefix(object __instance, ref bool __result)
        {
            return HandleHorizontal(__instance, 1, ref __result);
        }

        private static bool HandleHorizontal(object gui, int direction, ref bool result)
        {
            try
            {
                if (gui == null || _techTreeGuiType == null || !_techTreeGuiType.IsInstanceOfType(gui))
                    return true;

                State state = _active;
                if (state != null && state.Unlocks.Count > 0)
                {
                    Component item = state.Item as Component;
                    if (item == null || !item.gameObject.activeInHierarchy)
                    {
                        RestoreHighlight(state);
                        TechnologyTooltipViewportClamp.ClearSelectedUnlockAvoidance(state.Tooltip);
                        _active = null;
                    }
                    else
                    {
                        int nextIndex = state.SelectedIndex + direction;
                        if (nextIndex >= 0 && nextIndex < state.Unlocks.Count)
                        {
                            state.SelectedIndex = nextIndex;
                            RebuildSelectedTooltip(state, true);
                            ApplyHighlight(state);

                            result = true;
                            return false;
                        }
                    }
                }

                // At an outer child-unlock edge (or when entering from a neighbouring
                // Technology), let Graveyard Keeper run its normal tree navigation.
                // The same-frame direction marker lets OnGamepadOver choose the
                // corresponding boundary unlock when this native move lands on a
                // prayer-bearing Technology.
                _pendingHorizontalDirection = direction;
                _pendingHorizontalFrame = Time.frameCount;
                return true;
            }
            catch (Exception ex)
            {
                LogRuntimeFailure("PC_CAROUSEL_INPUT_FAILED ", ex);
                return true;
            }
        }

        private static State TryBuildState(object item, object visibleUnlocks)
        {
            List<object> unlocks = ToList(visibleUnlocks as IEnumerable);
            if (unlocks.Count < 2) return null;

            var prayerLabels = new List<string>();
            bool containsPrayer = false;
            foreach (object unlock in unlocks)
            {
                string prayerLabel = ResolvePrayerLabel(unlock);
                if (!string.IsNullOrEmpty(prayerLabel))
                    containsPrayer = true;
                prayerLabels.Add(string.IsNullOrEmpty(prayerLabel) ? "-" : prayerLabel);
            }

            if (!containsPrayer) return null;

            Component component = item as Component;
            if (component == null) return null;
            object tooltip = component.gameObject.GetComponent(_tooltipType);
            if (tooltip == null) return null;

            var state = new State
            {
                Item = item,
                Tooltip = tooltip,
                SelectedIndex = 0
            };
            state.Unlocks.AddRange(unlocks);
            state.PrayerLabels.AddRange(prayerLabels);

            IEnumerable childEnumerable = Get(item, "_unlocks") as IEnumerable;
            if (childEnumerable != null)
            {
                foreach (object child in childEnumerable)
                {
                    if (child != null) state.Children.Add(child);
                }
            }

            return state;
        }

        private static string ResolvePrayerLabel(object techUnlock)
        {
            IEnumerable crafts = _resolvePrayerCrafts.Invoke(null, new[] { techUnlock }) as IEnumerable;
            if (crafts == null) return null;

            var families = new List<string>();
            foreach (object craft in crafts)
            {
                string craftId = GetId(craft);
                if (string.IsNullOrEmpty(craftId) ||
                    !craftId.StartsWith("pray:", StringComparison.Ordinal))
                    continue;

                string family = PrayerFamilyFromCraftId(craftId);
                string label = string.IsNullOrEmpty(family) ? craftId : family;
                if (!families.Contains(label))
                    families.Add(label);
            }

            return families.Count == 0 ? null : string.Join("+", families.ToArray());
        }

        private static string PrayerFamilyFromCraftId(string craftId)
        {
            if (string.IsNullOrEmpty(craftId) ||
                !craftId.StartsWith("pray:", StringComparison.Ordinal))
                return null;

            int lastColon = craftId.LastIndexOf(':');
            if (lastColon <= 5) return null;
            return craftId.Substring(5, lastColon - 5);
        }

        private static void RebuildSelectedTooltip(State state, bool redraw)
        {
            if (state == null ||
                state.Tooltip == null ||
                state.SelectedIndex < 0 ||
                state.SelectedIndex >= state.Unlocks.Count)
                return;

            object unlock = state.Unlocks[state.SelectedIndex];
            MethodInfo getTooltip = unlock.GetType().GetMethod(
                "GetTooltip", Inst, null, new[] { _tooltipType }, null);
            if (getTooltip == null)
                throw new MissingMethodException(unlock.GetType().FullName, "GetTooltip(Tooltip)");

            _tooltipClearData.Invoke(state.Tooltip, null);
            getTooltip.Invoke(unlock, new[] { state.Tooltip });

            BindSelectedUnlockAvoidance(state);

            if (redraw)
                _tooltipsRedraw.Invoke(null, null);
        }

        private static void ApplyHighlight(State state)
        {
            if (state == null) return;

            BindSelectedUnlockAvoidance(state);

            for (int i = 0; i < state.Children.Count && i < state.Unlocks.Count; i++)
            {
                object sprite = Get(state.Children[i], "spr");
                if (sprite == null) continue;

                Color original;
                if (!state.OriginalSpriteColors.TryGetValue(sprite, out original))
                {
                    object value = Get(sprite, "color");
                    if (!(value is Color)) continue;
                    original = (Color)value;
                    state.OriginalSpriteColors[sprite] = original;
                }

                float alpha = i == state.SelectedIndex ? original.a : original.a * 0.35f;
                Set(sprite, "color", new Color(original.r, original.g, original.b, alpha));
            }
        }

        private static void RestoreHighlight(State state)
        {
            if (state == null || state.OriginalSpriteColors.Count == 0) return;

            foreach (KeyValuePair<object, Color> pair in state.OriginalSpriteColors)
            {
                if (pair.Key != null)
                    Set(pair.Key, "color", pair.Value);
            }
            state.OriginalSpriteColors.Clear();
        }

        private static void BindSelectedUnlockAvoidance(State state)
        {
            if (state == null || state.Tooltip == null)
                return;

            TechnologyTooltipViewportClamp.MarkTechnologyTooltip(state.Tooltip);

            object sprite = null;
            if (state.SelectedIndex >= 0 && state.SelectedIndex < state.Children.Count)
                sprite = Get(state.Children[state.SelectedIndex], "spr");

            TechnologyTooltipViewportClamp.SetSelectedUnlockAvoidance(state.Tooltip, sprite);
        }

        private static void Patch(MethodInfo target, string prefixName, string postfixName)
        {
            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony is unavailable.");

            R.TrackPatchOwner(_harmonyId);
            object harmony = Activator.CreateInstance(harmonyType, new object[] { _harmonyId });
            object prefix = CreateHarmonyMethod(harmonyMethodType, prefixName);
            object postfix = CreateHarmonyMethod(harmonyMethodType, postfixName);

            MethodInfo patch = harmonyType.GetMethods(Inst)
                .FirstOrDefault(m =>
                    m.Name == "Patch" &&
                    m.GetParameters().Length >= 5 &&
                    typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = prefix;
            args[2] = postfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static object CreateHarmonyMethod(Type harmonyMethodType, string methodName)
        {
            if (string.IsNullOrEmpty(methodName)) return null;

            MethodInfo method = typeof(TechnologyPrayerCarousel)
                .GetMethod(methodName, Stat);
            if (method == null)
                throw new MissingMethodException(
                    typeof(TechnologyPrayerCarousel).FullName,
                    methodName);

            ConstructorInfo ctor = harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
            if (ctor != null) return ctor.Invoke(new object[] { method });

            object value = Activator.CreateInstance(harmonyMethodType);
            if (!Set(value, "method", method))
                throw new MissingMemberException("HarmonyMethod.method");
            return value;
        }

        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type exact = assembly.GetType(name, false);
                    if (exact != null) return exact;

                    Type match = assembly.GetTypes()
                        .FirstOrDefault(t => t != null && t.Name == name);
                    if (match != null) return match;
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Type match = ex.Types
                        .FirstOrDefault(t => t != null && t.Name == name);
                    if (match != null) return match;
                }
                catch
                {
                }
            }
            return null;
        }

        private static object Get(object instance, string name)
        {
            if (instance == null || string.IsNullOrEmpty(name)) return null;
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Inst);
                if (field != null) return field.GetValue(instance);

                PropertyInfo property = type.GetProperty(name, Inst);
                if (property != null && property.CanRead)
                {
                    try { return property.GetValue(instance, null); }
                    catch { return null; }
                }
            }
            return null;
        }

        private static bool Set(object instance, string name, object value)
        {
            if (instance == null || string.IsNullOrEmpty(name)) return false;
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Inst);
                if (field != null)
                {
                    field.SetValue(instance, value);
                    return true;
                }

                PropertyInfo property = type.GetProperty(name, Inst);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(instance, value, null);
                    return true;
                }
            }
            return false;
        }

        private static string GetId(object value)
        {
            return Convert.ToString(Get(value, "id")) ?? string.Empty;
        }

        private static List<object> ToList(IEnumerable values)
        {
            var result = new List<object>();
            if (values == null) return result;
            foreach (object value in values)
                if (value != null) result.Add(value);
            return result;
        }

        private static void LogRuntimeFailure(string prefix, Exception ex)
        {
            if (_runtimeErrorLogged) return;
            _runtimeErrorLogged = true;

            Exception inner = ex is TargetInvocationException && ex.InnerException != null
                ? ex.InnerException
                : ex;
            _log?.LogError(prefix + inner);
        }
    }
}
