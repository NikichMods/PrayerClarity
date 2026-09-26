using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class ItemTooltipPresentation
    {
        private static readonly HashSet<string> PrayerItemFamilies = new HashSet<string>(StringComparer.Ordinal)
        {
            "b_empty", "b_faith", "b_money", "b_faith_money", "b_plant", "b_sins",
            "b_skull", "b_sword", "b_shield", "b_pen", "b_star", "b_village",
            "b_souls", "b_grat_points_incr", "b_sin_shard", "b_ghost", "b_energy",
            "b_random", "b_techpoint_blue", "b_techpoint_green", "b_techpoint_red",
            "b_circle", "b_cross"
        };

        private const int ItemTooltipMaxWidth = TechnologyTooltipContentWidth.OwnedMaxWidth;
        private static ManualLogSource _log;
        private static bool _errorLogged;
        private static Type _bubbleTextType;
        private static ConstructorInfo _bubbleTextConstructor;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            Type itemDefinition = R.GameType("ItemDefinition");
            Type item = R.GameType("Item");
            if (itemDefinition == null || item == null)
                throw new MissingMemberException("ItemDefinition/Item type is unavailable.");

            MethodInfo getTooltipData = R.Method(itemDefinition, "GetTooltipData", false, new[] { item, typeof(bool) });
            if (getTooltipData == null)
                throw new MissingMethodException("ItemDefinition.GetTooltipData(Item, bool)");

            R.Patch(harmonyId + ".itemtooltip", typeof(ItemTooltipPresentation), getTooltipData, nameof(ItemTooltipPostfix));
        }

        private static void ItemTooltipPostfix(object __instance, object __result)
        {
            try
            {
                IList list = __result as IList;
                if (__instance == null || list == null) return;

                object craft = ResolvePrayerCraft(__instance);
                if (craft == null) return;

                Localization.UseCurrentGameLanguage();
                PrayerForecast.TierDetails tier = PrayerForecast.BuildTierDetails(craft);
                TooltipPresentationSections sections = TooltipDetailsRenderer.BuildSingleSections(tier);
                if (sections == null || !sections.HasContent) return;

                int itemQuality = (int)Math.Round(R.Float(R.Get(__instance, "quality")));
                ComposePrayerTooltip(list, craft, itemQuality, sections);
            }
            catch (Exception ex)
            {
                if (_errorLogged) return;
                _errorLogged = true;
                _log?.LogError("PrayerClarity prayer-item tooltip presentation failed; vanilla item tooltip remains available. " + ex);
            }
        }

        private static object ResolvePrayerCraft(object itemDefinition)
        {
            if (itemDefinition == null) return null;
            string itemId = R.Id(itemDefinition);
            if (!IsKnownPrayerItemId(itemId)) return null;

            object craft = R.Get(itemDefinition, "linked_craft");
            if (craft == null) return null;
            string craftId = R.Id(craft) ?? string.Empty;
            return craftId.StartsWith("pray:", StringComparison.Ordinal) ? craft : null;
        }

        internal static bool IsKnownPrayerItemId(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            int separator = itemId.IndexOf(':');
            string family = separator < 0 ? itemId : itemId.Substring(0, separator);
            return PrayerItemFamilies.Contains(family);
        }

        private static void ComposePrayerTooltip(
            IList list,
            object craft,
            int itemQuality,
            TooltipPresentationSections sections)
        {
            if (list == null || list.Count < 2) return;
            if (_bubbleTextType == null) _bubbleTextType = R.GameType("BubbleWidgetTextData");
            if (_bubbleTextType == null) return;

            PrefixTitleWithQualityGlyph(list, itemQuality);

            object descriptionRow = list[1];
            if (descriptionRow == null || !_bubbleTextType.IsInstanceOfType(descriptionRow)) return;

            string lore = PrayerLoreResolver.ResolveForCraftId(R.Id(craft));
            if (!string.IsNullOrEmpty(lore))
                R.Set(descriptionRow, "text", lore);
            else
                R.Set(descriptionRow, "text", StripStockRequirementLine(R.Get(descriptionRow, "text") as string));

            RemoveVanillaPrayerMechanics(list);

            int insertIndex = Math.Min(2, list.Count);
            if (!string.IsNullOrEmpty(sections.BaseResult))
            {
                list.Insert(insertIndex++, CreateTextData("\n" + Localization.F("tech.base_result"), 3, "Center", false));
                list.Insert(insertIndex++, CreateTextData(sections.BaseResult, 4, "Left", true));
            }

            if (!string.IsNullOrEmpty(sections.Requirement))
            {
                list.Insert(insertIndex++, CreateTextData("\n" + Localization.F("item.sermon_success"), 3, "Center", false));
                list.Insert(insertIndex++, CreateTextData(sections.Requirement, 4, "Center", true));
            }

            if (!string.IsNullOrEmpty(sections.SuccessBonuses))
            {
                list.Insert(insertIndex++, CreateTextData("\n" + Localization.F("tech.on_success_header"), 3, "Center", false));
                list.Insert(insertIndex++, CreateTextData(sections.SuccessBonuses, 4, "Left", true));
            }

            TooltipTextPolish.NormalizeFollowingCraftingRow(list, insertIndex, _bubbleTextType);
        }

        private static void PrefixTitleWithQualityGlyph(IList list, int quality)
        {
            if (list == null || list.Count == 0) return;
            if (quality < 1 || quality > 3) return;

            object titleRow = list[0];
            if (titleRow == null || !_bubbleTextType.IsInstanceOfType(titleRow)) return;

            string title = R.Get(titleRow, "text") as string;
            if (string.IsNullOrEmpty(title)) return;

            string prefix;
            switch (quality)
            {
                case 1: prefix = Localization.F("quality.bronze") + " "; break;
                case 2: prefix = Localization.F("quality.silver") + " "; break;
                case 3: prefix = Localization.F("quality.gold") + " "; break;
                default: return;
            }

            if (!title.StartsWith(prefix, StringComparison.Ordinal))
                R.Set(titleRow, "text", prefix + title);
        }

        private static void RemoveVanillaPrayerMechanics(IList list)
        {
            string nativeHeader = R.VanillaLocalize("preach_params_2");
            for (int i = 2; i < list.Count;)
            {
                object row = list[i];
                if (row == null || !_bubbleTextType.IsInstanceOfType(row))
                {
                    i++;
                    continue;
                }

                string text = R.Get(row, "text") as string;
                if (!string.Equals(text, nativeHeader, StringComparison.Ordinal))
                {
                    i++;
                    continue;
                }

                list.RemoveAt(i);
                if (i < list.Count)
                {
                    object body = list[i];
                    if (body != null && _bubbleTextType.IsInstanceOfType(body))
                        list.RemoveAt(i);
                }
            }
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
            if (end < 0 || end + 1 >= normalized.Length) return string.Empty;
            return normalized.Substring(end + 1).TrimStart();
        }

        private static int FindSentenceTerminator(string text, int start)
        {
            for (int i = Math.Max(0, start); i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '.': case '!': case '?': case '。': case '！': case '？':
                        return i;
                }
            }
            return -1;
        }

        private static object CreateTextData(
            string text,
            int styleValue,
            string alignmentName,
            bool compactPrayerItem)
        {
            if (_bubbleTextConstructor == null)
            {
                if (_bubbleTextType == null) _bubbleTextType = R.GameType("BubbleWidgetTextData");
                if (_bubbleTextType == null) throw new MissingMemberException("BubbleWidgetTextData");

                foreach (ConstructorInfo constructor in _bubbleTextType.GetConstructors(R.Inst))
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
            object alignment = Enum.Parse(parameters[2].ParameterType, alignmentName);
            object data = _bubbleTextConstructor.Invoke(new object[] { text, style, alignment, -1 });
            if (compactPrayerItem)
                TechnologyTooltipContentWidth.PreferPrayerItemLayout(data);
            return data;
        }
    }
}
