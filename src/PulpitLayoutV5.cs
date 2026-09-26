using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PrayerClarity
{
    // 0.1.10 layout layer. The class name stays PulpitLayoutV4 so the already narrow
    // plugin/tuning seam does not need another compatibility hop. The implementation
    // deliberately changes the resize mechanism: the real root window is resized with
    // UIWidget.SetDimensions(), while the stock anchored container is allowed to derive
    // its rectangle from its original anchors via UpdateAnchors().
    internal static class PulpitLayoutV4
    {
        private sealed class WidgetState
        {
            internal Transform Transform;
            internal object Widget;
            internal Vector3 Position;
            internal int Width;
            internal int Height;
        }

        private static readonly Dictionary<string, Sprite> SpecialSpriteCache =
            new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private static MethodInfo _getSpriteMethod;

        private static object _template;
        private static object _gui;
        private static PrayerForecast.Result _forecast;

        private static Transform _window;
        private static Transform _container;
        private static WidgetState _windowState;
        private static WidgetState _containerState;
        private static WidgetState _back;
        private static WidgetState _decoreBack;
        private static WidgetState _header;
        private static WidgetState _pixelLine;
        private static WidgetState _inactiveBack;
        private static Transform _buttonsTips;
        private static Vector3 _buttonsTipsPosition;
        private static Transform _closeButton;
        private static Transform _closeButton2;
        private static Vector3 _closeButtonPosition;
        private static Vector3 _closeButton2Position;

        private static Transform _forecastRoot;
        private static object _resultHeaderLabel;
        private static object _resultRowsLabel;
        private static object _effectLabel;
        private static object _effectIcon;
        private static object _noteLabel;
        private static bool _captured;
        private static int _dynamicExtraHeight;

        internal static void Apply(object template, object gui, PrayerForecast.Result forecast)
        {
            if (template == null || gui == null || forecast == null) return;
            _template = template;
            _gui = gui;
            _forecast = forecast;

            // Recompute content-driven height from the accepted baseline on every
            // host redraw. The native anchored craft button then follows the root
            // window bottom; PrayerClarity does not own its final transform.
            _dynamicExtraHeight = 0;

            Capture(template);
            OverrideSpecialPresentation(gui, forecast);
            CaptureForecastWidgets(template);
            ApplyWindowGeometry();
            ApplyForecastGeometry();
            RefreshResultText();
            RefreshEffectPresentation();
        }

        internal static void ApplyTuning()
        {
            if (!_captured || _template == null || _forecast == null) return;
            ApplyWindowGeometry();
            ApplyForecastGeometry();
            RefreshResultText();
            RefreshEffectPresentation();
        }

        internal static void Restore()
        {
            _dynamicExtraHeight = 0;
            if (!_captured) return;

            // Restore the owner rectangle first. The container is anchored on all four
            // sides, so its stock 274x199 geometry follows automatically from the
            // restored 274x241 owner and the original +3/-35/+7 anchor absolutes.
            SetDimensionsDirect(_windowState, _windowState == null ? 0 : _windowState.Width,
                _windowState == null ? 0 : _windowState.Height);
            UpdateAnchors(_containerState == null ? null : _containerState.Widget);
            if (_containerState != null && _containerState.Transform != null)
                _containerState.Transform.localPosition = _containerState.Position;

            RestoreVisualState(_back);
            RestoreVisualState(_decoreBack);
            RestoreVisualState(_header);
            RestoreVisualState(_pixelLine);
            RestoreVisualState(_inactiveBack);

            if (_buttonsTips != null) _buttonsTips.localPosition = _buttonsTipsPosition;
            if (_closeButton != null) _closeButton.localPosition = _closeButtonPosition;
            if (_closeButton2 != null) _closeButton2.localPosition = _closeButton2Position;

            if (_resultHeaderLabel != null)
            {
                GameObject go = R.Get(_resultHeaderLabel, "gameObject") as GameObject;
                if (go != null) go.SetActive(false);
            }
        }

        private static void Capture(object template)
        {
            GameObject templateGo = R.Get(template, "gameObject") as GameObject;
            Transform container = templateGo == null ? null : templateGo.transform.parent;
            Transform window = container == null ? null : container.parent;
            if (window == null) return;

            if (_captured && ReferenceEquals(_window, window)) return;

            _window = window;
            _container = container;
            _windowState = CaptureWidget(window, "UIWidget");
            _containerState = CaptureWidget(container, "UIWidget");
            _back = CaptureWidget(window.Find("back"), "UI2DSprite");
            _decoreBack = CaptureWidget(window.Find("decore_back"), "UI2DSprite");
            _header = CaptureWidget(window.Find("header"), "UI2DSprite");
            _pixelLine = CaptureWidget(window.Find("header/pixel line"), "UI2DSprite");
            _inactiveBack = CaptureWidget(window.Find("back for inactive stuff"), "UI2DSprite");

            _buttonsTips = window.Find("buttons tips");
            _buttonsTipsPosition = _buttonsTips == null ? Vector3.zero : _buttonsTips.localPosition;
            _closeButton = window.Find("header/close button");
            _closeButton2 = window.Find("header/close button (1)");
            _closeButtonPosition = _closeButton == null ? Vector3.zero : _closeButton.localPosition;
            _closeButton2Position = _closeButton2 == null ? Vector3.zero : _closeButton2.localPosition;
            _captured = true;
        }

        private static WidgetState CaptureWidget(Transform transform, string typeName)
        {
            if (transform == null) return null;
            Type type = R.AnyType(typeName);
            object widget = type == null ? null : transform.gameObject.GetComponent(type);
            if (widget == null) return null;

            return new WidgetState
            {
                Transform = transform,
                Widget = widget,
                Position = transform.localPosition,
                Width = Math.Max(1, R.Int(R.Get(widget, "width"))),
                Height = Math.Max(1, R.Int(R.Get(widget, "height")))
            };
        }

        private static void CaptureForecastWidgets(object template)
        {
            if (_container == null) return;
            Transform root = _container.Find("PrayerClarity.PulpitForecast");
            if (root == null) return;

            if (!ReferenceEquals(_forecastRoot, root))
            {
                _forecastRoot = root;
                _resultHeaderLabel = null;
            }

            _resultRowsLabel = GetLabel(root.Find("PrayerClarity.Result"), template.GetType());
            _effectLabel = GetLabel(root.Find("PrayerClarity.Effect"), template.GetType());
            _noteLabel = GetLabel(root.Find("PrayerClarity.DependencyNote"), template.GetType());
            _effectIcon = GetComponent(root.Find("PrayerClarity.Effect.Icon"), "UI2DSprite");

            if (_resultHeaderLabel == null)
            {
                Transform existing = root.Find("PrayerClarity.ResultHeader");
                _resultHeaderLabel = GetLabel(existing, template.GetType());
                if (_resultHeaderLabel == null)
                {
                    GameObject templateGo = R.Get(template, "gameObject") as GameObject;
                    GameObject go = new GameObject("PrayerClarity.ResultHeader");
                    go.layer = templateGo == null ? root.gameObject.layer : templateGo.layer;
                    go.transform.SetParent(root, false);
                    object label = go.AddComponent(template.GetType());
                    CopyLabelStyle(template, label);
                    TrySet(label, "depth", R.Int(R.Get(template, "depth")) + 1);
                    _resultHeaderLabel = label;
                }
            }

            // ResultHeader is created by this layout layer after PulpitPresentation's
            // font refresh, so explicitly bind it to the current language as well.
            R.EnsureLabelHasCorrectFont(_resultHeaderLabel);

            GameObject headerGo = _resultHeaderLabel == null ? null : R.Get(_resultHeaderLabel, "gameObject") as GameObject;
            if (headerGo != null) headerGo.SetActive(true);
        }

        private static object GetLabel(Transform transform, Type labelType)
        {
            return transform == null || labelType == null ? null : transform.gameObject.GetComponent(labelType);
        }

        private static object GetComponent(Transform transform, string typeName)
        {
            if (transform == null) return null;
            Type type = R.AnyType(typeName);
            return type == null ? null : transform.gameObject.GetComponent(type);
        }

        internal static bool GrowWindowForButtonClearance(float requiredDownward)
        {
            if (!_captured || _windowState == null || requiredDownward <= 0f) return false;

            // The root window is unanchored and grows around its centre. The native
            // craft-button UILabel is bottom-anchored to this window, so increasing
            // total root height by 2*d moves that button down by d while the stock
            // all-sides-anchored content container keeps its centre fixed.
            // Keep a tiny integer-rounding guard and make the request idempotent.
            int growth = Mathf.Max(2, Mathf.CeilToInt(requiredDownward * 2f) + 2);
            _dynamicExtraHeight = Mathf.Max(_dynamicExtraHeight, growth);
            ApplyWindowGeometry();
            return true;
        }

        private static void ApplyWindowGeometry()
        {
            if (!_captured || _windowState == null) return;

            int extraW = Mathf.Max(0, Mathf.RoundToInt(PulpitTuning.WindowExtraWidth.Value));
            int extraH =
                Mathf.Max(0, Mathf.RoundToInt(PulpitTuning.WindowExtraHeight.Value)) +
                Mathf.Max(0, _dynamicExtraHeight);
            float halfW = extraW * 0.5f;
            float halfH = extraH * 0.5f;

            // Critical 0.1.10 change: SetDimensions bypasses the UIWidget width/height
            // anchor-validation setters. 0.1.9 wrote width/height on the anchored
            // container, which permanently changed its anchor absolutes during live
            // tuning. Resize only the unanchored owner, then let stock anchors derive
            // the container rectangle.
            SetDimensionsDirect(_windowState, _windowState.Width + extraW, _windowState.Height + extraH);
            UpdateAnchors(_containerState == null ? null : _containerState.Widget);

            ResizeVisual(_back, extraW, extraH, new Vector3(-halfW, halfH, 0f));
            ResizeVisual(_decoreBack, extraW, extraH, new Vector3(0f, -halfH, 0f));

            if (_header != null)
            {
                SetDimensionsDirect(_header, _header.Width + extraW, _header.Height);
                _header.Transform.localPosition = _header.Position + new Vector3(0f, halfH, 0f);
            }

            if (_pixelLine != null)
                SetDimensionsDirect(_pixelLine, _pixelLine.Width + extraW, _pixelLine.Height);

            if (_inactiveBack != null)
                SetDimensionsDirect(_inactiveBack, _inactiveBack.Width + extraW, _inactiveBack.Height + extraH);

            if (_buttonsTips != null)
                _buttonsTips.localPosition = _buttonsTipsPosition + new Vector3(0f, -halfH, 0f);
            if (_closeButton != null)
                _closeButton.localPosition = _closeButtonPosition + new Vector3(halfW, 0f, 0f);
            if (_closeButton2 != null)
                _closeButton2.localPosition = _closeButton2Position + new Vector3(halfW, 0f, 0f);
        }

        private static void ResizeVisual(WidgetState state, int extraW, int extraH, Vector3 positionDelta)
        {
            if (state == null) return;
            SetDimensionsDirect(state, state.Width + extraW, state.Height + extraH);
            state.Transform.localPosition = state.Position + positionDelta;
        }

        private static void ApplyForecastGeometry()
        {
            int extraW = Mathf.Max(0, Mathf.RoundToInt(PulpitTuning.WindowExtraWidth.Value));
            int containerWidth = CurrentContainerWidth(extraW);

            int contextHeight = 44 + (_forecast != null && (_forecast.UsesSoulGratitude || _forecast.SoulGratitudeFaithCap > 0) ? 16 : 0);
            ConfigureLabel(_template,
                PulpitTuning.ContextX.Value,
                PulpitTuning.ContextY.Value,
                258 + extraW,
                contextHeight,
                "Top",
                "Left",
                "ShrinkContent",
                PulpitTuning.ContextFontSize.Value,
                -2);

            ConfigureLabel(_resultHeaderLabel,
                PulpitTuning.ResultHeaderX.Value,
                PulpitTuning.ResultHeaderY.Value,
                258 + extraW,
                20,
                "Top",
                "Left",
                "ShrinkContent",
                PulpitTuning.ResultHeaderFontSize.Value,
                -1);

            ConfigureLabel(_resultRowsLabel,
                PulpitTuning.ResultX.Value,
                PulpitTuning.ResultY.Value,
                250 + extraW,
                40,
                "Top",
                "Left",
                "ShrinkContent",
                PulpitTuning.ResultFontSize.Value,
                -1);

            if (_effectLabel != null)
            {
                bool iconActive = IsActive(_effectIcon);
                int iconSize = PulpitTuning.EffectIconSize.Value;
                float iconSpace = iconActive ? iconSize + 4f : 0f;
                float labelX = PulpitTuning.EffectX.Value + iconSpace;
                float safeRight = containerWidth * 0.5f - 8f;
                int effectWidth = Math.Max(48, Mathf.FloorToInt(safeRight - labelX));

                ConfigureLabel(_effectLabel,
                    labelX,
                    PulpitTuning.EffectY.Value,
                    effectWidth,
                    20,
                    "TopLeft",
                    "Left",
                    "ResizeHeight",
                    PulpitTuning.EffectFontSize.Value,
                    -1);

                ConfigureWidget(_effectIcon,
                    PulpitTuning.EffectX.Value,
                    PulpitTuning.EffectY.Value - 1f,
                    iconSize,
                    iconSize,
                    "TopLeft");
            }

            ConfigureLabel(_noteLabel,
                PulpitTuning.NoteX.Value,
                PulpitTuning.NoteY.Value,
                266 + extraW,
                20,
                "Top",
                "Center",
                "ShrinkContent",
                PulpitTuning.NoteFontSize.Value,
                -1);
        }

        private static int CurrentContainerWidth(int extraW)
        {
            if (_containerState != null && _containerState.Widget != null)
            {
                int live = R.Int(R.Get(_containerState.Widget, "width"));
                if (live > 0) return live;
                return _containerState.Width + extraW;
            }
            return 274 + extraW;
        }

        private static void RefreshResultText()
        {
            if (_forecast == null) return;
            if (_resultHeaderLabel != null)
                R.Set(_resultHeaderLabel, "text", Localization.F("forecast.result_header"));

            if (_resultRowsLabel != null)
            {
                bool soulsRepose = IsSoulRepose();
                PrayerForecast.BonusHighlight baseHighlight = soulsRepose
                    ? PrayerForecast.BonusHighlight.Faith
                    : PrayerForecast.BonusHighlight.None;

                string guaranteed = "  " + Localization.F("forecast.guaranteed") + ": " +
                                    FormatResources(_forecast.BaseFaith, _forecast.BaseMoney, baseHighlight);
                string success;
                if (_forecast.SoulGratitudeFaithCap > 0)
                {
                    string conversion = _forecast.SoulGratitudeConversion > 0
                        ? Localization.F(
                            "rebalanced.pulpit.souls_conversion",
                            _forecast.SoulGratitudeConversion,
                            _forecast.SoulGratitudeConversion)
                        : Localization.F("rebalanced.pulpit.souls_conversion_empty");
                    success = "  " + Localization.F("forecast.success_bonus", _forecast.ChancePercent) + ": " + conversion;
                }
                else
                {
                    PrayerForecast.BonusHighlight bonusHighlight = soulsRepose
                        ? PrayerForecast.BonusHighlight.Faith
                        : _forecast.Highlight;
                    success = "  " + Localization.F("forecast.success_bonus", _forecast.ChancePercent) + ": " +
                              FormatResources(_forecast.BonusFaith, _forecast.BonusMoney, bonusHighlight);
                }

                R.Set(_resultRowsLabel, "text", guaranteed + "\n" + success);
            }
        }

        private static bool IsSoulRepose()
        {
            object craft = _gui == null ? null : R.Get(_gui, "pray_craft");
            string craftId = R.Id(craft) ?? string.Empty;
            return craftId.StartsWith("pray:b_souls:", StringComparison.Ordinal);
        }

        private static string FormatResources(int faith, float money, PrayerForecast.BonusHighlight highlight)
        {
            List<string> parts = new List<string>();
            if (faith != 0)
            {
                string prefix = highlight == PrayerForecast.BonusHighlight.Faith ? "(up) " : string.Empty;
                parts.Add(prefix + "(faith) " + faith);
            }
            if (Math.Abs(money) >= 0.0001f)
            {
                string prefix = highlight == PrayerForecast.BonusHighlight.Money ? "(up) " : string.Empty;
                parts.Add(prefix + R.FormatMoney(money));
            }
            return parts.Count == 0 ? "—" : string.Join(", ", parts.ToArray());
        }

        private static void OverrideSpecialPresentation(object gui, PrayerForecast.Result forecast)
        {
            object craft = R.Get(gui, "pray_craft");
            if (craft == null) return;
            string craftId = R.Id(craft) ?? string.Empty;
            float duration = R.Float(R.Get(craft, "dur_parameter"));

            if (craftId.StartsWith("pray:b_skull:", StringComparison.Ordinal))
            {
                string rebalancedText;
                string semanticKey;
                bool hasRebalancedSemantics = PrayerEditionSemantics.TryBuildTierEffect(
                    craftId,
                    "buff_skull",
                    out rebalancedText,
                    out semanticKey);

                bool stockAddsHigherTier = CorpseTierSemantics.StockReposeAddsHigherOrdinaryTier();
                bool premiumCanStillChangeDistribution =
                    hasRebalancedSemantics &&
                    CorpseTierSemantics.BestTierNarrowingChangesDistribution();
                bool reliabilityStillChangesDistribution =
                    forecast.QualityTier >= 2 &&
                    premiumCanStillChangeDistribution;

                if (!stockAddsHigherTier)
                {
                    if (hasRebalancedSemantics &&
                        forecast.QualityTier == 1 &&
                        premiumCanStillChangeDistribution)
                    {
                        forecast.SpecialText = Localization.F("repose.endpoint") + "\n" +
                                               Localization.F("rebalanced.repose.endpoint_bronze_hint");
                    }
                    else if (reliabilityStillChangesDistribution)
                    {
                        string deltaKey = forecast.QualityTier == 2
                            ? "rebalanced.repose.terminal.silver"
                            : "rebalanced.repose.terminal.gold";
                        forecast.SpecialText = Localization.F(deltaKey) + " " +
                                               TechnologyTooltipTextStyle.CorpseQualityCue();
                    }
                    else
                        forecast.SpecialText = Localization.F("repose.endpoint");
                }
                else if (!hasRebalancedSemantics)
                    forecast.SpecialText = Localization.F("buff.skull", 1f, duration);

                // Rebalanced tiers already carry their accepted reliability wording
                // and duration in PrayerForecast. Keep that shared semantic text when
                // it still changes the corpse distribution.
                forecast.SpecialIconName = null;
                return;
            }

            if (craftId.StartsWith("pray:b_village:", StringComparison.Ordinal))
            {
                int count = CountOutput(craft, "blessing_commerce");
                string name = R.VanillaLocalize("blessing_commerce");
                string description = R.VanillaLocalize("blessing_commerce_d");
                if (string.IsNullOrEmpty(name) || string.Equals(name, "blessing_commerce", StringComparison.Ordinal))
                {
                    string prayerDescription = R.VanillaLocalize("b_village_d");
                    forecast.SpecialText = string.IsNullOrEmpty(prayerDescription) ? forecast.SpecialText :
                        prayerDescription + (count > 0 ? " ×" + count : string.Empty);
                }
                else
                {
                    forecast.SpecialText = name + (count > 0 ? " ×" + count : string.Empty) +
                        (string.IsNullOrEmpty(description) || string.Equals(description, "blessing_commerce_d", StringComparison.Ordinal)
                            ? string.Empty
                            : " — " + description);
                }
                forecast.SpecialIconName = "i_scroll_3";
                return;
            }

            if (craftId.StartsWith("pray:b_souls:", StringComparison.Ordinal))
            {
                // Rebalanced Soul's Repose owns a success-only SG -> Faith transaction.
                // Keep that transaction in the success row and do not resurrect the
                // stock Soul-Gratitude-dependent effect sentence after the first render.
                if (forecast.SoulGratitudeFaithCap > 0)
                    forecast.SpecialText = null;
                else
                    forecast.SpecialText = Localization.F("buff.souls_repose");

                forecast.SpecialIconName = null;
                return;
            }

            if (craftId.StartsWith("pray:b_sin_shard:", StringComparison.Ordinal))
            {
                string rebalancedText;
                string semanticKey;
                if (PrayerEditionSemantics.TryBuildTierEffect(
                    craftId,
                    "buff_sin_shard",
                    out rebalancedText,
                    out semanticKey))
                {
                    forecast.SpecialText = rebalancedText;
                }
                else
                {
                    forecast.SpecialText = Localization.F("buff.sin_shard", duration);
                }

                forecast.SpecialIconName = "i_sin_shard";
            }
        }

        private static int CountOutput(object craft, string id)
        {
            IEnumerable output = R.Get(craft, "output") as IEnumerable;
            if (output == null) return 0;
            int total = 0;
            foreach (object item in output)
            {
                if (item == null || !string.Equals(R.Id(item), id, StringComparison.Ordinal)) continue;
                total += Math.Max(0, R.Int(R.Get(item, "value")));
            }
            return total;
        }

        private static void RefreshEffectPresentation()
        {
            if (_forecast == null) return;

            if (_effectLabel != null)
            {
                GameObject labelGo = R.Get(_effectLabel, "gameObject") as GameObject;
                bool hasText = !string.IsNullOrEmpty(_forecast.SpecialText);
                if (labelGo != null) labelGo.SetActive(hasText);
                R.Set(_effectLabel, "text", hasText
                    ? Localization.F("forecast.effect_header") + ": " + _forecast.SpecialText
                    : string.Empty);
            }

            if (_effectIcon == null) return;
            GameObject iconGo = R.Get(_effectIcon, "gameObject") as GameObject;
            Sprite sprite = ResolveSpriteDirect(_forecast.SpecialIconName);
            if (sprite != null)
            {
                TrySet(_effectIcon, "sprite2D", sprite);
                if (iconGo != null) iconGo.SetActive(true);
            }
            else if (iconGo != null)
            {
                iconGo.SetActive(false);
            }

            // Icon visibility affects the left edge and therefore the safe wrapping width.
            ApplyForecastGeometry();
        }

        private static Sprite ResolveSpriteDirect(string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return null;

            Sprite cached;
            if (SpecialSpriteCache.TryGetValue(iconName, out cached)) return cached;

            try
            {
                if (_getSpriteMethod == null)
                {
                    Type type = R.GameType("EasySpritesCollection");
                    _getSpriteMethod = R.Method(type, "GetSprite", true,
                        new[] { typeof(string), typeof(bool), typeof(string) });
                }

                Sprite sprite = _getSpriteMethod == null
                    ? null
                    : _getSpriteMethod.Invoke(null, new object[] { iconName, false, string.Empty }) as Sprite;
                if (sprite != null) SpecialSpriteCache[iconName] = sprite;
                return sprite;
            }
            catch { return null; }
        }

        private static void CopyLabelStyle(object source, object target)
        {
            object bitmapFont = R.Get(source, "bitmapFont");
            if (bitmapFont != null) TrySet(target, "bitmapFont", bitmapFont);
            else TrySet(target, "trueTypeFont", R.Get(source, "trueTypeFont"));

            string[] properties =
            {
                "fontStyle", "color", "effectStyle", "effectColor", "effectDistance",
                "supportEncoding", "symbolStyle", "spacingX", "useFloatSpacing",
                "floatSpacingX", "floatSpacingY", "applyGradient", "gradientTop", "gradientBottom"
            };
            foreach (string property in properties)
            {
                try
                {
                    object value = R.Get(source, property);
                    if (value != null) R.Set(target, property, value);
                }
                catch { }
            }
        }

        private static void ConfigureLabel(object label, float x, float y, int width, int height,
            string pivot, string alignment, string overflow, int fontSize, int spacingY)
        {
            if (label == null) return;
            GameObject go = R.Get(label, "gameObject") as GameObject;
            if (go == null) return;

            SetEnum(label, "pivot", pivot);
            TrySet(label, "width", width);
            TrySet(label, "height", height);
            TrySet(label, "fontSize", fontSize);
            TrySet(label, "spacingY", spacingY);
            SetEnum(label, "alignment", alignment);
            SetEnum(label, "overflowMethod", overflow);
            go.transform.localPosition = new Vector3(x, y, 0f);
        }

        private static void ConfigureWidget(object widget, float x, float y, int width, int height, string pivot)
        {
            if (widget == null) return;
            GameObject go = R.Get(widget, "gameObject") as GameObject;
            if (go == null) return;

            SetEnum(widget, "pivot", pivot);
            TrySet(widget, "width", width);
            TrySet(widget, "height", height);
            go.transform.localPosition = new Vector3(x, y, 0f);
        }

        private static void SetDimensionsDirect(WidgetState state, int width, int height)
        {
            if (state == null || state.Widget == null) return;
            int w = Math.Max(1, width);
            int h = Math.Max(1, height);

            MethodInfo method = R.Method(state.Widget.GetType(), "SetDimensions", false,
                new[] { typeof(int), typeof(int) });
            if (method == null)
                throw new MissingMethodException(state.Widget.GetType().FullName, "SetDimensions(int,int)");
            method.Invoke(state.Widget, new object[] { w, h });
        }

        private static void RestoreVisualState(WidgetState state)
        {
            if (state == null) return;
            SetDimensionsDirect(state, state.Width, state.Height);
            if (state.Transform != null) state.Transform.localPosition = state.Position;
        }

        private static void UpdateAnchors(object widget)
        {
            if (widget == null) return;
            MethodInfo method = R.Method(widget.GetType(), "UpdateAnchors", false, 0);
            if (method == null)
                throw new MissingMethodException(widget.GetType().FullName, "UpdateAnchors()");
            method.Invoke(widget, null);
        }

        private static bool IsActive(object widget)
        {
            GameObject go = widget == null ? null : R.Get(widget, "gameObject") as GameObject;
            return go != null && go.activeSelf;
        }

        private static void TrySet(object obj, string name, object value)
        {
            if (obj == null || value == null) return;
            try { R.Set(obj, name, value); }
            catch { }
        }

        private static void SetEnum(object obj, string propertyName, string value)
        {
            if (obj == null) return;
            try
            {
                PropertyInfo property = obj.GetType().GetProperty(propertyName, R.Inst);
                if (property == null || !property.CanWrite || !property.PropertyType.IsEnum) return;
                property.SetValue(obj, Enum.Parse(property.PropertyType, value), null);
            }
            catch { }
        }
    }
}
