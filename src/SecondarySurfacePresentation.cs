using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class SecondarySurfacePresentation
    {
        private const int TechnologyTooltipMaxWidth = TechnologyTooltipContentWidth.OwnedMaxWidth;

        private static ManualLogSource _log;
        private static bool _buffErrorLogged;
        private static bool _techErrorLogged;
        private static bool _timerErrorLogged;
        private static bool _hudTimerErrorLogged;
        private static bool _hudTimerLocaleReady;
        private static ConstructorInfo _bubbleTextConstructor;
        private static MethodInfo _tooltipAddDataMethod;
        private static Type _blankSeparatorType;
        private static Type _bubbleTextType;
        private static FieldInfo _playerBuffIdField;
        private static FieldInfo _playerBuffEndTimeField;
        private static MethodInfo _gameTimeGetter;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type perkBuffItemGui = R.GameType("PerkBuffItemGUI");
            Type buffIcon = R.GameType("BuffIcon");
            Type playerBuff = R.GameType("PlayerBuff");
            Type techUnlock = R.GameType("TechUnlock");
            Type tooltip = R.GameType("Tooltip");
            Type mainGame = R.GameType("MainGame");

            MethodInfo drawBuff = R.Method(perkBuffItemGui, "Draw", false, new[] { playerBuff });
            MethodInfo drawBuffIcon = R.Method(buffIcon, "Draw", false, new[] { playerBuff });
            MethodInfo redrawBuffIcon = R.Method(buffIcon, "Redraw", false, 0);
            MethodInfo getTooltip = R.Method(techUnlock, "GetTooltip", false, new[] { tooltip });
            MethodInfo getTimerText = R.Method(playerBuff, "GetTimerText", false, 0);
            if (drawBuff == null) throw new MissingMethodException("PerkBuffItemGUI.Draw(PlayerBuff)");
            if (drawBuffIcon == null) throw new MissingMethodException("BuffIcon.Draw(PlayerBuff)");
            if (redrawBuffIcon == null) throw new MissingMethodException("BuffIcon.Redraw()");
            if (getTooltip == null) throw new MissingMethodException("TechUnlock.GetTooltip(Tooltip)");
            if (getTimerText == null) throw new MissingMethodException("PlayerBuff.GetTimerText()");

            _playerBuffIdField = playerBuff.GetField("buff_id", R.Inst);
            _playerBuffEndTimeField = playerBuff.GetField("end_time", R.Inst);
            PropertyInfo gameTime = mainGame == null ? null : mainGame.GetProperty("game_time", R.Stat);
            _gameTimeGetter = gameTime == null ? null : gameTime.GetGetMethod(true);
            if (_playerBuffIdField == null || _playerBuffEndTimeField == null || _gameTimeGetter == null)
                throw new MissingMemberException("Verified prayer-buff timer state is unavailable.");

            R.Patch(harmonyId + ".activeeffects", typeof(SecondarySurfacePresentation), drawBuff, nameof(PerkBuffDrawPostfix));
            R.PatchPrefix(harmonyId + ".hudprayertimer.locale", typeof(SecondarySurfacePresentation), drawBuffIcon, nameof(BuffIconDrawPrefix));
            R.PatchPrefix(harmonyId + ".hudprayertimer", typeof(SecondarySurfacePresentation), redrawBuffIcon, nameof(BuffIconRedrawPrefix));
            R.Patch(harmonyId + ".technology", typeof(SecondarySurfacePresentation), getTooltip, nameof(TechUnlockTooltipPostfix));
            R.Patch(harmonyId + ".prayertimer", typeof(SecondarySurfacePresentation), getTimerText, nameof(PlayerBuffTimerPostfix));
        }

        private static void PerkBuffDrawPostfix(object __instance, object __0)
        {
            try
            {
                string text = PrayerForecast.BuildActiveBuffText(__0);
                if (string.IsNullOrEmpty(text)) return;

                string buffId = _playerBuffIdField.GetValue(__0) as string;
                float remainingDays;
                if (ShouldShowStrategicDuration(buffId) &&
                    TryGetRemainingPrayerDays(__0, out remainingDays) && remainingDays >= 1f)
                    text += " · " + Localization.F("active.timer_days", remainingDays);

                object description = R.Get(__instance, "txt_descr");
                if (description == null) return;
                R.Set(description, "text", text);

                MethodInfo recalculate = R.Method(__instance.GetType(), "RecalculateDescriptionHeight", false, 0);
                recalculate?.Invoke(__instance, null);
                RepositionTextTable(__instance);
            }
            catch (Exception ex)
            {
                if (_buffErrorLogged) return;
                _buffErrorLogged = true;
                _log?.LogError("PrayerClarity active-effect presentation failed; vanilla Temporary Effects text remains available. " + ex);
            }
        }

        private static void BuffIconDrawPrefix()
        {
            // BuffIcon.Draw is the native icon-binding lifecycle and immediately calls
            // Redraw. Refresh localization here once per icon binding so the compact
            // number uses the game's active locale without polling language every frame.
            Localization.UseCurrentGameLanguage();
        }

        private static bool BuffIconRedrawPrefix(object __instance)
        {
            try
            {
                if (__instance == null) return true;

                object showTimerValue = R.Get(__instance, "show_timer");
                if (!(showTimerValue is bool) || !(bool)showTimerValue) return true;

                object playerBuff = R.Get(__instance, "linked_buff");
                float remainingDays;
                if (!TryGetRemainingPrayerDays(playerBuff, out remainingDays) || remainingDays < 1f)
                    return true;

                object timerLabel = R.Get(__instance, "txt_timer");
                if (timerLabel == null) return true;

                // Plugins initialize before Graveyard Keeper loads GameSettings. If this
                // icon reached Redraw without a later Draw binding, refresh the locale
                // once here after the save/UI lifecycle is definitely live.
                if (!_hudTimerLocaleReady)
                {
                    Localization.UseCurrentGameLanguage();
                    _hudTimerLocaleReady = true;
                }

                string desired = Localization.F("active.timer_days_compact", remainingDays);
                string current = R.Get(timerLabel, "text") as string;
                if (!string.Equals(current, desired, StringComparison.Ordinal))
                    R.Set(timerLabel, "text", desired);

                // BuffsGUI already calls this native Redraw at its own cadence.
                // For long prayer timers the compact day count fully replaces the
                // stock clock text; below one day we return true and preserve stock.
                return false;
            }
            catch (Exception ex)
            {
                if (!_hudTimerErrorLogged)
                {
                    _hudTimerErrorLogged = true;
                    _log?.LogError("PrayerClarity HUD prayer-buff day timer failed; vanilla timer remains available. " + ex);
                }
                return true;
            }
        }

        private static void PlayerBuffTimerPostfix(object __instance, ref string __result)
        {
            try
            {
                float remainingDays;
                if (!TryGetRemainingPrayerDays(__instance, out remainingDays)) return;
                if (remainingDays < 1f) return;
                __result = string.Empty;
            }
            catch (Exception ex)
            {
                if (_timerErrorLogged) return;
                _timerErrorLogged = true;
                _log?.LogError("PrayerClarity prayer-buff day timer failed; vanilla timer remains available. " + ex);
            }
        }

        private static bool TryGetRemainingPrayerDays(object playerBuff, out float remainingDays)
        {
            remainingDays = 0f;
            if (playerBuff == null) return false;

            string buffId = _playerBuffIdField.GetValue(playerBuff) as string;
            if (!IsPrayerTimedBuff(buffId)) return false;

            float endTime = Convert.ToSingle(_playerBuffEndTimeField.GetValue(playerBuff));
            float gameTime = Convert.ToSingle(_gameTimeGetter.Invoke(null, null));
            remainingDays = endTime - gameTime;
            return true;
        }

        private static bool ShouldShowStrategicDuration(string buffId)
        {
            if (!string.Equals(buffId, "buff_plant", StringComparison.Ordinal) &&
                !string.Equals(buffId, "buff_sins", StringComparison.Ordinal))
                return true;

            // Stock Roots/Repentance are intentionally presented as inactive/unverified,
            // so their long timer is not promoted into the description. Rebalanced
            // installs concrete active semantics for these same buff IDs; once present,
            // the duration is meaningful and belongs on the active-effect surface.
            string editionText;
            return PrayerEditionSemantics.TryBuildActiveEffect(buffId, out editionText) &&
                   !string.IsNullOrEmpty(editionText);
        }

        private static bool IsPrayerTimedBuff(string buffId)
        {
            switch (buffId)
            {
                case "buff_sword":
                case "buff_shield":
                case "buff_skull":
                case "buff_pen":
                case "buff_star":
                case "buff_plant":
                case "buff_sins":
                case "buff_gp_increase":
                case "buff_sin_shard":
                    return true;
                default:
                    return false;
            }
        }

        private static void TechUnlockTooltipPostfix(object __instance, object __0)
        {
            try
            {
                List<object> crafts = ResolvePrayerCrafts(__instance);
                if (crafts == null || crafts.Count == 0 || __0 == null) return;

                Localization.UseCurrentGameLanguage();
                TooltipPresentationSections sections = BuildTechnologySections(crafts);
                if (sections == null || !sections.HasContent) return;

                bool preferWideLayout = ShouldPreferWideBssLayout(crafts);
                string vanillaLore = ResolveVanillaPrayerLore(crafts);
                if (!TryReplaceVanillaPrayerMechanics(
                        __0,
                        sections,
                        TechnologyTooltipMaxWidth,
                        vanillaLore,
                        preferWideLayout))
                {
                    bool editionFallbackHandled =
                        PrayerEditionSemantics.HasTechnologyProvider &&
                        TryInsertSectionsAfterVanillaPrayerLore(
                            __0,
                            sections,
                            TechnologyTooltipMaxWidth,
                            vanillaLore,
                            preferWideLayout);

                    if (!editionFallbackHandled)
                        AppendTechnologySections(
                            __0,
                            sections,
                            TechnologyTooltipMaxWidth,
                            preferWideLayout);
                }

                NormalizeRebalancedBssLoreRows(__0, crafts, preferWideLayout);
                PromoteAccumulatedBssWideLayout(__0, crafts);
                NormalizePrayerTechnologyAlignment(__instance, __0);
                TechnologyTooltipViewportClamp.MarkTechnologyTooltip(__0);
            }
            catch (Exception ex)
            {
                if (_techErrorLogged) return;
                _techErrorLogged = true;
                _log?.LogError("PrayerClarity technology-tooltip presentation failed; vanilla technology tooltip remains available. " + ex);
            }
        }

        private static void NormalizePrayerTechnologyAlignment(
            object techUnlock,
            object tooltip)
        {
            if (techUnlock == null || tooltip == null) return;

            object data = R.Get(tooltip, "data");
            IList list = data == null ? null : R.Get(data, "data_list") as IList;
            if (list == null || list.Count == 0) return;

            if (_bubbleTextType == null) _bubbleTextType = R.GameType("BubbleWidgetTextData");
            if (_bubbleTextType == null) return;

            string title = null;
            MethodInfo getData = R.Method(techUnlock.GetType(), "GetData", false, 0);
            if (getData != null)
            {
                object unlockData = getData.Invoke(techUnlock, null);
                title = unlockData == null ? null : R.Get(unlockData, "name") as string;
            }

            string baseHeader = Localization.F("tech.base_result");
            string successHeader = TechnologySuccessHeader();

            for (int i = 0; i < list.Count; i++)
            {
                object row = list[i];
                if (row == null || !_bubbleTextType.IsInstanceOfType(row)) continue;

                string text = R.Get(row, "text") as string;
                if (!string.Equals(text, title, StringComparison.Ordinal) &&
                    !string.Equals(text, baseHeader, StringComparison.Ordinal) &&
                    !string.Equals(text, successHeader, StringComparison.Ordinal))
                    continue;

                object alignment = R.Get(row, "alignment");
                if (alignment != null)
                    R.Set(row, "alignment", Enum.Parse(alignment.GetType(), "Center"));
            }
        }

        private static bool TryReplaceVanillaPrayerMechanics(
            object tooltip,
            TooltipPresentationSections sections,
            int maxWidth,
            string vanillaLore,
            bool preferWideLayout)
        {
            object data = R.Get(tooltip, "data");
            IList list = data == null ? null : R.Get(data, "data_list") as IList;
            if (list == null || list.Count < 2) return false;

            if (_bubbleTextType == null) _bubbleTextType = R.GameType("BubbleWidgetTextData");
            if (_bubbleTextType == null) return false;

            string vanillaHeader = R.VanillaLocalize("preach_params_2");
            int headerIndex = -1;
            for (int i = list.Count - 2; i >= 0; i--)
            {
                object item = list[i];
                if (item == null || !_bubbleTextType.IsInstanceOfType(item)) continue;
                string text = R.Get(item, "text") as string;
                if (string.Equals(text, vanillaHeader, StringComparison.Ordinal))
                {
                    headerIndex = i;
                    break;
                }
            }

            if (headerIndex < 0 || headerIndex + 1 >= list.Count) return false;
            object header = list[headerIndex];
            object body = list[headerIndex + 1];
            if (body == null || !_bubbleTextType.IsInstanceOfType(body)) return false;

            R.Set(header, "text", Localization.F("tech.base_result"));
            list[headerIndex + 1] = CreateTextData(sections.BaseResult, 4, maxWidth);

            int insertIndex = headerIndex + 2;
            if (!string.IsNullOrEmpty(sections.SuccessBonuses))
            {
                object separator = CreateBlankSeparator();
                if (separator != null) list.Insert(insertIndex++, separator);
                list.Insert(insertIndex++, CreateTextData(TechnologySuccessHeader(), 3));
                list.Insert(
                    insertIndex++,
                    CreateTextData(sections.SuccessBonuses, 4, maxWidth, preferWideLayout));
            }

            TooltipTextPolish.NormalizeFollowingCraftingRow(list, insertIndex, _bubbleTextType);

            if (headerIndex > 0)
            {
                object previous = list[headerIndex - 1];
                if (previous != null && _bubbleTextType.IsInstanceOfType(previous))
                {
                    string text = R.Get(previous, "text") as string;
                    if (!string.IsNullOrEmpty(text) &&
                        text.IndexOf("(cross)", StringComparison.Ordinal) >= 0)
                    {
                        string replacement = !string.IsNullOrEmpty(vanillaLore)
                            ? vanillaLore
                            : StripStockRequirementLine(text);
                        if (!string.IsNullOrEmpty(replacement))
                            R.Set(previous, "text", replacement);
                    }
                }
            }

            return true;
        }

        private static bool TryInsertSectionsAfterVanillaPrayerLore(
            object tooltip,
            TooltipPresentationSections sections,
            int maxWidth,
            string vanillaLore,
            bool preferWideLayout)
        {
            object data = R.Get(tooltip, "data");
            IList list = data == null ? null : R.Get(data, "data_list") as IList;
            if (list == null || list.Count == 0) return false;

            if (_bubbleTextType == null) _bubbleTextType = R.GameType("BubbleWidgetTextData");
            if (_bubbleTextType == null) return false;

            int loreIndex = -1;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                object row = list[i];
                if (row == null || !_bubbleTextType.IsInstanceOfType(row)) continue;

                string text = R.Get(row, "text") as string;
                if (string.IsNullOrEmpty(text) ||
                    text.IndexOf("(cross)", StringComparison.Ordinal) < 0)
                    continue;

                string stripped = StripStockRequirementLine(text);
                if (string.Equals(stripped, text, StringComparison.Ordinal)) continue;

                loreIndex = i;
                break;
            }

            if (loreIndex < 0) return false;

            object loreRow = list[loreIndex];
            string current = R.Get(loreRow, "text") as string;
            string replacement = !string.IsNullOrEmpty(vanillaLore)
                ? vanillaLore
                : StripStockRequirementLine(current);
            if (!string.IsNullOrEmpty(replacement))
                R.Set(loreRow, "text", replacement);
            int insertIndex = loreIndex + 1;

            if (!string.IsNullOrEmpty(sections.BaseResult))
            {
                list.Insert(insertIndex++, CreateTextData(Localization.F("tech.base_result"), 3));
                list.Insert(insertIndex++, CreateTextData(sections.BaseResult, 4, maxWidth));
            }

            if (!string.IsNullOrEmpty(sections.SuccessBonuses))
            {
                object separator = CreateBlankSeparator();
                if (separator != null) list.Insert(insertIndex++, separator);
                list.Insert(insertIndex++, CreateTextData(TechnologySuccessHeader(), 3));
                list.Insert(insertIndex++, CreateTextData(sections.SuccessBonuses, 4, maxWidth));
            }

            TooltipTextPolish.NormalizeFollowingCraftingRow(list, insertIndex, _bubbleTextType);
            return true;
        }

        private static TooltipPresentationSections BuildTechnologySections(List<object> crafts)
        {
            if (crafts == null || crafts.Count == 0) return null;

            List<PrayerForecast.TierDetails> tiers = new List<PrayerForecast.TierDetails>();
            foreach (object craft in crafts)
            {
                PrayerForecast.TierDetails tier = PrayerForecast.BuildTierDetails(craft);
                if (tier != null) tiers.Add(tier);
            }
            if (tiers.Count == 0) return null;

            return TechnologyTooltipTierRenderer.BuildSections(tiers);
        }

        private static void AppendTechnologySections(
            object tooltip,
            TooltipPresentationSections sections,
            int maxWidth,
            bool preferWideLayout)
        {
            object blank = CreateBlankSeparator();
            if (blank != null) AddTooltipData(tooltip, blank);

            if (!string.IsNullOrEmpty(sections.BaseResult))
            {
                AddTooltipData(tooltip, CreateTextData(Localization.F("tech.base_result"), 3));
                AddTooltipData(tooltip, CreateTextData(sections.BaseResult, 4, maxWidth));
            }

            if (!string.IsNullOrEmpty(sections.SuccessBonuses))
            {
                object separator = CreateBlankSeparator();
                if (separator != null) AddTooltipData(tooltip, separator);
                AddTooltipData(tooltip, CreateTextData(TechnologySuccessHeader(), 3));
                AddTooltipData(
                    tooltip,
                    CreateTextData(sections.SuccessBonuses, 4, maxWidth, preferWideLayout));
            }
        }

        private static bool ShouldPreferWideBssLayout(List<object> crafts)
        {
            if (!PrayerEditionSemantics.HasTechnologyProvider ||
                crafts == null ||
                crafts.Count < 2)
                return false;

            string firstFamily = null;
            bool multipleFamilies = false;

            foreach (object craft in crafts)
            {
                string family = PrayerFamilyFromCraftId(R.Id(craft));
                if (!IsRebalancedBssFamily(family)) return false;

                if (firstFamily == null) firstFamily = family;
                else if (!string.Equals(firstFamily, family, StringComparison.Ordinal))
                    multipleFamilies = true;
            }

            return multipleFamilies;
        }

        private static bool IsRebalancedBssFamily(string family)
        {
            return string.Equals(family, "b_souls", StringComparison.Ordinal) ||
                   string.Equals(family, "b_grat_points_incr", StringComparison.Ordinal) ||
                   string.Equals(family, "b_sin_shard", StringComparison.Ordinal);
        }

        private static void PromoteAccumulatedBssWideLayout(
            object tooltip,
            List<object> currentCrafts)
        {
            if (!PrayerEditionSemantics.HasTechnologyProvider ||
                tooltip == null ||
                currentCrafts == null ||
                currentCrafts.Count == 0)
                return;

            // The Better Save Soul Technology tooltip is assembled from separate
            // per-family TechUnlock.GetTooltip passes. The 0.2.20 policy looked for
            // multiple BSS families inside one pass, so every pass stayed narrow even
            // though the final Tooltip contained all three prayer blocks.
            //
            // Detect the combined shape from PrayerClarity-owned rows already
            // accumulated in the shared Tooltip. A single prayer contributes exactly
            // two max-width rows (Base Result + success body); two or more prayer
            // blocks therefore produce at least four. Keep the policy gated to BSS
            // families so unrelated combined Technology tooltips retain their current
            // layout.
            foreach (object craft in currentCrafts)
            {
                if (!IsRebalancedBssFamily(PrayerFamilyFromCraftId(R.Id(craft))))
                    return;
            }

            object data = R.Get(tooltip, "data");
            IList list = data == null ? null : R.Get(data, "data_list") as IList;
            if (list == null || list.Count == 0) return;

            if (_bubbleTextType == null) _bubbleTextType = R.GameType("BubbleWidgetTextData");
            if (_bubbleTextType == null) return;

            int ownedBodyRows = 0;
            int baseHeaders = 0;
            string baseHeader = Localization.F("tech.base_result");

            for (int i = 0; i < list.Count; i++)
            {
                object row = list[i];
                if (row == null || !_bubbleTextType.IsInstanceOfType(row)) continue;

                if (R.Int(R.Get(row, "max_width")) == TechnologyTooltipMaxWidth)
                    ownedBodyRows++;

                string text = R.Get(row, "text") as string;
                if (string.Equals(text, baseHeader, StringComparison.Ordinal))
                    baseHeaders++;
            }

            if (ownedBodyRows < 4 || baseHeaders < 2) return;

            for (int i = 0; i < list.Count; i++)
            {
                object row = list[i];
                if (row == null || !_bubbleTextType.IsInstanceOfType(row)) continue;

                if (R.Int(R.Get(row, "max_width")) == TechnologyTooltipMaxWidth)
                {
                    TechnologyTooltipContentWidth.PreferWideLayout(row);
                    continue;
                }

                string text = R.Get(row, "text") as string;
                if (!string.Equals(text, baseHeader, StringComparison.Ordinal) || i <= 0)
                    continue;

                // In the verified BSS block shape the retained lore row is immediately
                // before "Base Result". Let that real visible prose participate in the
                // same native ResizeFreely/overflowWidth path so widening actually
                // reduces the aggregate tooltip height.
                object lore = list[i - 1];
                if (lore == null || !_bubbleTextType.IsInstanceOfType(lore)) continue;

                string loreText = R.Get(lore, "text") as string;
                if (string.IsNullOrEmpty(loreText)) continue;

                R.Set(lore, "max_width", TechnologyTooltipMaxWidth);
                TechnologyTooltipContentWidth.PreferWideLayout(lore);
            }
        }

        private static string PrayerFamilyFromCraftId(string craftId)
        {
            const string prefix = "pray:";
            if (string.IsNullOrEmpty(craftId) || !craftId.StartsWith(prefix, StringComparison.Ordinal))
                return null;

            int tierSeparator = craftId.LastIndexOf(':');
            if (tierSeparator <= prefix.Length || tierSeparator + 1 >= craftId.Length)
                return null;

            return craftId.Substring(prefix.Length, tierSeparator - prefix.Length);
        }

        private static void NormalizeRebalancedBssLoreRows(
            object tooltip,
            List<object> crafts,
            bool preferWideLayout)
        {
            if (!PrayerEditionSemantics.HasTechnologyProvider ||
                tooltip == null ||
                crafts == null ||
                crafts.Count == 0)
                return;

            object data = R.Get(tooltip, "data");
            IList list = data == null ? null : R.Get(data, "data_list") as IList;
            if (list == null || list.Count == 0) return;

            if (_bubbleTextType == null) _bubbleTextType = R.GameType("BubbleWidgetTextData");
            if (_bubbleTextType == null) return;

            Dictionary<string, string> replacements =
                new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (object craft in crafts)
            {
                string craftId = R.Id(craft);
                string family = PrayerFamilyFromCraftId(craftId);
                if (!IsRebalancedBssFamily(family)) continue;

                string key = family + "_d";
                string vanilla = R.VanillaLocalize(key);
                if (string.IsNullOrEmpty(vanilla) ||
                    string.Equals(vanilla, key, StringComparison.Ordinal))
                    continue;

                string normalized = NormalizeRebalancedBssLore(craftId, vanilla);
                replacements[vanilla] = string.IsNullOrEmpty(normalized)
                    ? vanilla
                    : normalized;
            }

            if (replacements.Count == 0) return;

            foreach (object row in list)
            {
                if (row == null || !_bubbleTextType.IsInstanceOfType(row)) continue;
                string text = R.Get(row, "text") as string;
                if (string.IsNullOrEmpty(text)) continue;

                string updated = text;
                bool matchedLore = false;
                foreach (KeyValuePair<string, string> pair in replacements)
                {
                    if (updated.IndexOf(pair.Key, StringComparison.Ordinal) < 0) continue;
                    updated = updated.Replace(pair.Key, pair.Value);
                    matchedLore = true;
                }

                if (!string.Equals(updated, text, StringComparison.Ordinal))
                    R.Set(row, "text", updated);

                if (preferWideLayout && matchedLore)
                {
                    // Make the retained real lore row itself participate in the same
                    // verified content-driven width seam as PrayerClarity-owned rows.
                    // This avoids synthetic spacers: the text the player actually sees
                    // is what naturally widens the combined parchment.
                    R.Set(row, "max_width", TechnologyTooltipMaxWidth);
                    TechnologyTooltipContentWidth.PreferWideLayout(row);
                }
            }
        }

        private static string NormalizeRebalancedBssLore(string craftId, string lore)
        {
            if (string.IsNullOrEmpty(lore)) return lore;

            string family = PrayerFamilyFromCraftId(craftId);
            if (string.Equals(family, "b_sin_shard", StringComparison.Ordinal))
            {
                // Stock lore hard-codes the Bronze x2 multiplier. Rebalanced keeps the
                // sentence but moves the actual x2/x3/x4 value to the tier rows.
                return Regex.Replace(
                    lore,
                    @"\s*[\(（]\s*[x×]\s*2(?:[\.,]0+)?\s*[\)）]",
                    string.Empty,
                    RegexOptions.IgnoreCase);
            }

            if (string.Equals(family, "b_grat_points_incr", StringComparison.Ordinal))
            {
                // Preserve the stock localized lore wording while replacing only its
                // obsolete +10% magnitude with the current edition-owned percentage.
                string shared;
                string tier;
                if (!PrayerEditionSemantics.TryBuildTechnologyEffect(
                        craftId,
                        out shared,
                        out tier) ||
                    string.IsNullOrEmpty(shared))
                    return lore;

                Match current = Regex.Match(shared, @"(?<!\d)(\d+(?:[\.,]\d+)?)\s*[%％]");
                if (!current.Success) return lore;

                string currentNumber = current.Groups[1].Value;
                return Regex.Replace(
                    lore,
                    @"(?<!\d)10(?=\s*[%％])",
                    currentNumber);
            }

            return lore;
        }

        private static string StripStockRequirementLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string normalized = text.Replace("\r\n", "\n");

            int newline = normalized.IndexOf('\n');
            if (newline > 0)
            {
                string firstLine = normalized.Substring(0, newline);
                if (firstLine.IndexOf("(cross)", StringComparison.Ordinal) >= 0)
                    return normalized.Substring(newline + 1).TrimStart();
            }

            int cross = normalized.IndexOf("(cross)", StringComparison.Ordinal);
            if (cross < 0 || cross > 64) return text;

            int end = FindSentenceTerminator(normalized, cross);
            if (end < 0 || end + 1 >= normalized.Length) return text;
            return normalized.Substring(end + 1).TrimStart();
        }

        private static int FindSentenceTerminator(string text, int start)
        {
            for (int i = Math.Max(0, start); i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '.':
                    case '!':
                    case '?':
                    case '。':
                    case '！':
                    case '？':
                        return i;
                }
            }
            return -1;
        }

        private static string ResolveVanillaPrayerLore(List<object> crafts)
        {
            return PrayerLoreResolver.ResolveForCrafts(crafts);
        }

        private static string ResolveBaseLoreKey(string craftId)
        {
            const string prefix = "pray:";
            if (string.IsNullOrEmpty(craftId) || !craftId.StartsWith(prefix, StringComparison.Ordinal)) return null;

            string itemId = craftId.Substring(prefix.Length);
            int tierSeparator = itemId.LastIndexOf(':');
            if (tierSeparator <= 0 || tierSeparator + 1 >= itemId.Length) return null;

            string family = itemId.Substring(0, tierSeparator);
            return family.Length == 0 ? null : family + "_d";
        }

        private static List<object> ResolvePrayerCrafts(object techUnlock)
        {
            List<object> result = new List<object>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            if (techUnlock == null || R.Int(R.Get(techUnlock, "type")) != 0) return result;

            string unlockId = R.Get(techUnlock, "id") as string;
            if (string.IsNullOrEmpty(unlockId)) return result;

            object recipe = R.BalanceData(unlockId, "CraftDefinition", true);
            AddPrayerCraft(result, seen, recipe);
            if (recipe == null) return result;

            IEnumerable output = R.Get(recipe, "output") as IEnumerable;
            if (output == null) return result;

            foreach (object item in output)
            {
                if (item == null) continue;

                IEnumerable multi = R.Get(item, "multiquality_items") as IEnumerable;
                if (multi != null)
                {
                    foreach (object idValue in multi)
                    {
                        string itemId = idValue as string;
                        if (string.IsNullOrEmpty(itemId)) continue;
                        object definition = R.BalanceData(itemId, "ItemDefinition", true);
                        AddLinkedPrayerCraft(result, seen, definition);
                    }
                }

                object directDefinition = R.Get(item, "definition");
                AddLinkedPrayerCraft(result, seen, directDefinition);
            }

            return result;
        }

        private static void AddLinkedPrayerCraft(List<object> result, HashSet<string> seen, object itemDefinition)
        {
            if (itemDefinition == null) return;
            string itemId = R.Id(itemDefinition);
            if (!ItemTooltipPresentation.IsKnownPrayerItemId(itemId)) return;

            object linked = R.Get(itemDefinition, "linked_craft");
            AddPrayerCraft(result, seen, linked);
        }

        private static void AddPrayerCraft(List<object> result, HashSet<string> seen, object craft)
        {
            if (craft == null) return;
            string id = R.Id(craft) ?? string.Empty;
            if (!id.StartsWith("pray:", StringComparison.Ordinal) || !seen.Add(id)) return;
            result.Add(craft);
        }

        private static void RepositionTextTable(object itemGui)
        {
            GameObject go = R.Get(itemGui, "gameObject") as GameObject;
            Transform container = go == null ? null : go.transform.Find("text container");
            Type tableType = R.AnyType("SimpleUITable");
            object table = container == null || tableType == null ? null : container.gameObject.GetComponent(tableType);
            if (table == null) return;
            MethodInfo reposition = R.Method(table.GetType(), "Reposition", false, 0);
            reposition?.Invoke(table, null);
        }

        private static string TechnologySuccessHeader()
        {
            // Both public editions now use the same condition-style Technology heading.
            // Prayer-item tooltips intentionally keep the separate "Bonuses on success"
            // label because their single-quality hierarchy is already accepted.
            return Localization.F("tech.on_success_header");
        }

        private static object CreateBlankSeparator()
        {
            if (_blankSeparatorType == null)
                _blankSeparatorType = R.GameType("BubbleWidgetBlankSeparatorData");
            return _blankSeparatorType == null ? null : Activator.CreateInstance(_blankSeparatorType);
        }

        private static object CreateTextData(
            string text,
            int styleValue,
            int maxWidth = -1,
            bool preferWideLayout = false)
        {
            if (_bubbleTextConstructor == null)
            {
                Type type = R.GameType("BubbleWidgetTextData");
                if (type == null) throw new MissingMemberException("BubbleWidgetTextData");
                foreach (ConstructorInfo constructor in type.GetConstructors(R.Inst))
                {
                    ParameterInfo[] p = constructor.GetParameters();
                    if (p.Length == 4 && p[0].ParameterType == typeof(string) && p[3].ParameterType == typeof(int))
                    {
                        _bubbleTextConstructor = constructor;
                        break;
                    }
                }
                if (_bubbleTextConstructor == null)
                    throw new MissingMethodException("BubbleWidgetTextData(string, TextStyle, Alignment, int)");
            }

            ParameterInfo[] parameters = _bubbleTextConstructor.GetParameters();
            object style = Enum.ToObject(parameters[1].ParameterType, styleValue);
            object alignment = Enum.Parse(parameters[2].ParameterType, "Left");
            object data = _bubbleTextConstructor.Invoke(new object[] { text, style, alignment, maxWidth });
            if (preferWideLayout && maxWidth == TechnologyTooltipMaxWidth)
                TechnologyTooltipContentWidth.PreferWideLayout(data);
            return data;
        }

        private static void AddTooltipData(object tooltip, object data)
        {
            if (tooltip == null || data == null) return;
            if (_tooltipAddDataMethod == null)
                _tooltipAddDataMethod = R.Method(tooltip.GetType(), "AddData", false, 1);
            if (_tooltipAddDataMethod == null) throw new MissingMethodException("Tooltip.AddData(BubbleWidgetData)");
            _tooltipAddDataMethod.Invoke(tooltip, new[] { data });
        }
    }
}
