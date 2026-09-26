using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class TechnologyTooltipContentWidth
    {
        internal const int OwnedMaxWidth = 900;
        private const int MinimumAnchorWidth = 150;
        private const int WideLayoutMaxWidth = 520;
        private const string NoBreakSpace = "\u00A0";
        private const int SoftProseWordThreshold = 5;

        private sealed class WideLayoutMarker { }
        private sealed class PrayerItemLayoutMarker { }

        private static readonly ConditionalWeakTable<object, WideLayoutMarker> WideLayoutData =
            new ConditionalWeakTable<object, WideLayoutMarker>();
        private static readonly ConditionalWeakTable<object, PrayerItemLayoutMarker> PrayerItemLayoutData =
            new ConditionalWeakTable<object, PrayerItemLayoutMarker>();

        private static ManualLogSource _log;
        private static bool _errorLogged;
        private static bool _prayerItemAlignmentErrorLogged;

        internal static void PreferWideLayout(object data)
        {
            if (data == null) return;
            WideLayoutData.Remove(data);
            WideLayoutData.Add(data, new WideLayoutMarker());
        }

        internal static void PreferPrayerItemLayout(object data)
        {
            if (data == null) return;
            PrayerItemLayoutData.Remove(data);
            PrayerItemLayoutData.Add(data, new PrayerItemLayoutMarker());
        }

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type bubbleWidgetText = R.GameType("BubbleWidgetText");
            Type bubbleWidgetTextData = R.GameType("BubbleWidgetTextData");
            MethodInfo draw = R.Method(
                bubbleWidgetText,
                "Draw",
                false,
                new[] { bubbleWidgetTextData });

            if (draw == null)
                throw new MissingMethodException("BubbleWidgetText.Draw(BubbleWidgetTextData)");

            R.Patch(
                harmonyId + ".technologycontentwidth",
                typeof(TechnologyTooltipContentWidth),
                draw,
                nameof(BubbleWidgetTextDrawPostfix));

            Type widgetsBubbleGui = R.GameType("WidgetsBubbleGUI");
            MethodInfo updateSizeAndWidgetsPositions = R.Method(
                widgetsBubbleGui,
                "UpdateSizeAndWidgetsPositions",
                false,
                0);
            if (updateSizeAndWidgetsPositions == null)
                throw new MissingMethodException("WidgetsBubbleGUI.UpdateSizeAndWidgetsPositions()");

            R.PatchPrefix(
                harmonyId + ".prayeritemcontentalignment",
                typeof(TechnologyTooltipContentWidth),
                updateSizeAndWidgetsPositions,
                nameof(PrayerItemContentAlignmentPrefix));
        }

        private static void PrayerItemContentAlignmentPrefix(object __instance)
        {
            try
            {
                if (__instance == null) return;

                System.Collections.IEnumerable rows =
                    R.Get(__instance, "bubble_widgets") as System.Collections.IEnumerable;
                if (rows == null) return;

                List<object> leftPrayerRows = new List<object>();
                int nativeMaxWidth = 0;

                foreach (object row in rows)
                {
                    if (row == null) continue;
                    object widget = R.Get(row, "ui_widget");
                    int width = R.Int(R.Get(widget, "width"));
                    if (width > nativeMaxWidth)
                        nativeMaxWidth = width;

                    object data = R.Get(row, "data");
                    PrayerItemLayoutMarker marker;
                    if (data == null || !PrayerItemLayoutData.TryGetValue(data, out marker))
                        continue;

                    object alignment = R.Get(data, "alignment");
                    if (alignment == null ||
                        !string.Equals(alignment.ToString(), "Left", StringComparison.Ordinal))
                        continue;

                    leftPrayerRows.Add(row);
                }

                if (nativeMaxWidth <= 0 || leftPrayerRows.Count == 0)
                    return;

                foreach (object row in leftPrayerRows)
                {
                    object data = R.Get(row, "data");
                    object label = R.Get(row, "_label") ?? R.Get(row, "ui_widget");
                    if (data == null || label == null) continue;

                    string fullText = R.Get(data, "text") as string;
                    if (string.IsNullOrEmpty(fullText)) continue;

                    object overflow = R.Get(label, "overflowMethod");
                    if (overflow != null)
                        R.Set(label, "overflowMethod", Enum.Parse(overflow.GetType(), "ResizeHeight"));

                    // The table has already drawn every native child, so nativeMaxWidth
                    // is exactly the width the stock bubble would choose from those
                    // children. Widen only the PrayerClarity-owned Left content rows to
                    // that existing span; this changes their internal text alignment
                    // without increasing the parchment width.
                    R.Set(label, "width", nativeMaxWidth);
                    R.Set(label, "text", fullText);

                    string processed = R.Get(label, "processedText") as string ?? string.Empty;
                    string repaired = KeepAmountAndInlineSymbolTogether(fullText, processed);
                    if (!string.Equals(repaired, fullText, StringComparison.Ordinal))
                    {
                        R.Set(label, "text", repaired);
                        R.Get(label, "processedText");
                    }
                }
            }
            catch (Exception ex)
            {
                if (_prayerItemAlignmentErrorLogged) return;
                _prayerItemAlignmentErrorLogged = true;
                _log?.LogError(
                    "PrayerClarity prayer-item left-content alignment failed; " +
                    "the native tooltip remains usable. " + ex);
            }
        }

        private static void BubbleWidgetTextDrawPostfix(object __instance, object __0)
        {
            try
            {
                if (__instance == null || __0 == null) return;

                int maxWidth = R.Int(R.Get(__0, "max_width"));

                object label = R.Get(__instance, "_label") ?? R.Get(__instance, "ui_widget");
                if (label == null) return;

                string fullText = R.Get(label, "text") as string;
                if (string.IsNullOrEmpty(fullText)) return;

                PrayerItemLayoutMarker itemMarker;
                if (PrayerItemLayoutData.TryGetValue(__0, out itemMarker))
                {
                    // Prayer-item mechanics follow the game's standard item-tooltip
                    // geometry. PrayerClarity does not write width/height/overflow here.
                    // Keep only the separately accepted final-wrap semantic repair below.
                    string processed = R.Get(label, "processedText") as string ?? string.Empty;

                    // NGUI can wrap between a numeric amount and the following inline
                    // symbol, leaving the icon alone on the next visual line. Runtime
                    // evidence on the 200-unit prayer-item column showed this with
                    // both 1 (faith) and 90 (gratitude_points). Keep that semantic
                    // pair together by moving the wrap opportunity before the amount.
                    string repaired = KeepAmountAndInlineSymbolTogether(fullText, processed);
                    if (!string.Equals(repaired, fullText, StringComparison.Ordinal))
                    {
                        R.Set(label, "text", repaired);
                        R.Get(label, "processedText");
                    }
                    return;
                }

                if (maxWidth != OwnedMaxWidth) return;

                WideLayoutMarker marker;
                if (WideLayoutData.TryGetValue(__0, out marker))
                {
                    // This is the same native NGUI seam runtime-verified in the
                    // 1.0.13 width work: ResizeFreely + overflowWidth lets content
                    // expand naturally, and WidgetsBubbleGUI.UpdateSize() then grows
                    // the enclosing bubble/parchment from the child widget size.
                    //
                    // The 0.2.19 experiment mistakenly treated a larger overflowWidth
                    // as a minimum width. For the combined BSS tooltip we instead use a
                    // real finite expansion ceiling and let its retained long prose
                    // drive the natural width up to that ceiling.
                    R.Set(label, "text", fullText);
                    R.Set(label, "overflowWidth", Math.Min(maxWidth, WideLayoutMaxWidth));
                    R.Get(label, "processedText");
                    return;
                }

                int anchorWidth = MeasureAtomicAnchor(label, fullText, maxWidth);
                if (anchorWidth <= 0) anchorWidth = maxWidth;

                R.Set(label, "text", fullText);
                R.Set(label, "overflowWidth", anchorWidth);
                R.Get(label, "processedText");
            }
            catch (Exception ex)
            {
                if (_errorLogged) return;
                _errorLogged = true;
                _log?.LogError(
                    "PrayerClarity Technology tooltip anchor-width sizing failed; " +
                    "the tooltip remains available at the wide fallback ceiling. " + ex);
            }
        }

        private static string KeepAmountAndInlineSymbolTogether(string raw, string processed)
        {
            if (string.IsNullOrEmpty(raw) || string.IsNullOrEmpty(processed))
                return raw;

            string normalized = processed.Replace("\r\n", "\n");
            string[] lines = normalized.Split(new[] { '\n' }, StringSplitOptions.None);
            List<int> insertionPoints = new List<int>();

            for (int i = 0; i + 1 < lines.Length; i++)
            {
                string previous = (lines[i] ?? string.Empty).TrimEnd();
                string next = (lines[i + 1] ?? string.Empty).TrimStart();
                if (previous.Length == 0 || next.Length == 0 || next[0] != '(')
                    continue;

                int close = next.IndexOf(')');
                if (close <= 1 || close > 48) continue;

                string symbol = next.Substring(0, close + 1);
                if (!IsInlineSymbolToken(symbol)) continue;

                string amount = TrailingAmountToken(previous);
                if (string.IsNullOrEmpty(amount)) continue;

                int clusterAt = FindUniqueAmountSymbolCluster(raw, amount, symbol);
                if (clusterAt <= 0 || raw[clusterAt - 1] == '\n') continue;
                if (!insertionPoints.Contains(clusterAt))
                    insertionPoints.Add(clusterAt);
            }

            if (insertionPoints.Count == 0) return raw;

            insertionPoints.Sort();
            string repaired = raw;
            for (int i = insertionPoints.Count - 1; i >= 0; i--)
                repaired = repaired.Insert(insertionPoints[i], "\n");
            return repaired;
        }

        private static bool IsInlineSymbolToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length < 3 ||
                token[0] != '(' || token[token.Length - 1] != ')')
                return false;

            for (int i = 1; i < token.Length - 1; i++)
            {
                char c = token[i];
                if ((c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9') ||
                    c == '_')
                    continue;
                return false;
            }
            return true;
        }

        private static string TrailingAmountToken(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            int end = text.Length - 1;
            while (end >= 0 && char.IsWhiteSpace(text[end])) end--;
            if (end < 0 || !char.IsDigit(text[end])) return null;

            int start = end;
            while (start > 0)
            {
                char c = text[start - 1];
                if (char.IsDigit(c) || c == '.' || c == ',' ||
                    c == '+' || c == '-' || c == '−')
                {
                    start--;
                    continue;
                }
                break;
            }

            return text.Substring(start, end - start + 1);
        }

        private static int FindUniqueAmountSymbolCluster(string raw, string amount, string symbol)
        {
            string ordinary = amount + " " + symbol;
            int index = raw.IndexOf(ordinary, StringComparison.Ordinal);
            if (index >= 0 && raw.IndexOf(ordinary, index + ordinary.Length, StringComparison.Ordinal) < 0)
                return index;

            string noBreak = amount + NoBreakSpace + symbol;
            index = raw.IndexOf(noBreak, StringComparison.Ordinal);
            if (index >= 0 && raw.IndexOf(noBreak, index + noBreak.Length, StringComparison.Ordinal) < 0)
                return index;

            return -1;
        }

        private static int MeasureAtomicAnchor(object label, string fullText, int measurementCeiling)
        {
            string effectHeader = Localization.F("forecast.effect_header") + ":";
            string[] lines = fullText.Replace("\r\n", "\n").Split('\n');

            float widest = 0f;
            bool previousWasEffectHeader = false;

            R.Set(label, "overflowWidth", measurementCeiling);

            foreach (string raw in lines)
            {
                string line = raw == null ? string.Empty : raw.Trim();
                if (line.Length == 0)
                {
                    previousWasEffectHeader = false;
                    continue;
                }

                string semanticLine = TechnologyTooltipTextStyle.StripColorEncoding(line);
                bool isEffectHeader = string.Equals(semanticLine, effectHeader, StringComparison.Ordinal);
                bool soft = previousWasEffectHeader ||
                            isEffectHeader ||
                            semanticLine.StartsWith(effectHeader + " ", StringComparison.Ordinal) ||
                            semanticLine.IndexOf(" ×", StringComparison.Ordinal) >= 0 ||
                            IsSectionHeading(semanticLine) ||
                            IsLongProse(line, semanticLine);

                previousWasEffectHeader = isEffectHeader;
                if (soft) continue;

                R.Set(label, "text", line);
                R.Get(label, "processedText");
                object printed = R.Get(label, "printedSize");
                if (!(printed is Vector2)) continue;

                float width = ((Vector2)printed).x;
                if (width > widest) widest = width;
            }

            if (widest <= 0f) return measurementCeiling;
            int measured = Mathf.CeilToInt(widest) + 2;
            return Mathf.Clamp(measured, MinimumAnchorWidth, measurementCeiling);
        }

        private static bool IsLongProse(string rawLine, string semanticLine)
        {
            if (string.IsNullOrEmpty(semanticLine)) return false;

            // A fully atomic mechanics row replaces every ordinary space with NBSP.
            // A prose sentence may still contain one smaller atomic cluster (for
            // example ↑ + white skull + red skull); normal spaces around that cluster
            // remain valid wrapping opportunities and the sentence must stay soft.
            bool hasNoBreakSpace = rawLine.IndexOf(NoBreakSpace) >= 0;
            bool hasOrdinarySpace = rawLine.IndexOf(' ') >= 0;
            if (hasNoBreakSpace && !hasOrdinarySpace) return false;

            int words = 1;
            bool inSpace = false;
            for (int i = 0; i < semanticLine.Length; i++)
            {
                bool space = char.IsWhiteSpace(semanticLine[i]);
                if (space && !inSpace) words++;
                inSpace = space;
            }

            return words >= SoftProseWordThreshold;
        }

        private static bool IsSectionHeading(string line)
        {
            return line.EndsWith(":", StringComparison.Ordinal) &&
                   line.IndexOf("100%", StringComparison.Ordinal) < 0;
        }
    }
}
