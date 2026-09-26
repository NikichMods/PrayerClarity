using System;
using System.Globalization;

namespace PrayerClarity
{
    internal static class RebalancedPresentationSemantics
    {
        internal static void Install()
        {
            PrayerEditionSemantics.Install(
                TryBuildTierEffect,
                TryBuildActiveEffect,
                TryBuildTechnologyEffect,
                TryGetSoulConversion,
                TryGetEffectivePrayEvent,
                TryResolveLore);
        }

        private static bool TryResolveLore(
            string craftId,
            string vanillaLore,
            out string lore)
        {
            lore = vanillaLore;

            RebalancedPrayerRule rule;
            int tier;
            if (!RebalancedRuleSet.TryParseCraftId(craftId, out rule, out tier))
                return false;

            if (string.Equals(rule.PrayerId, "b_grat_points_incr", StringComparison.Ordinal))
            {
                lore = Localization.F("rebalanced.tech.gratitude_lore");
                return true;
            }

            if (string.Equals(rule.PrayerId, "b_sin_shard", StringComparison.Ordinal))
            {
                lore = Localization.F("rebalanced.tech.sin_shard_lore");
                return true;
            }

            return false;
        }

        private static bool TryGetEffectivePrayEvent(
            string craftId,
            string currentEventId,
            out string effectiveEventId)
        {
            effectiveEventId = currentEventId;
            string mapped;
            if (!RebalancedRuleSet.TryGetEffectivePrayEventId(craftId, out mapped))
                return false;

            effectiveEventId = mapped;
            return true;
        }

        private static bool TryGetSoulConversion(string craftId, out int cap, out int conversion)
        {
            cap = 0;
            conversion = 0;

            RebalancedPrayerRule rule;
            int tier;
            if (!RebalancedRuleSet.TryParseCraftId(craftId, out rule, out tier)) return false;
            if (!string.Equals(rule.PrayerId, "b_souls", StringComparison.Ordinal) ||
                rule.SoulGratitudeFaithCaps == null)
                return false;

            cap = Math.Max(0, rule.TierValue(rule.SoulGratitudeFaithCaps, tier, 0));
            conversion = RebalancedSoulsRepose.GetConversionAmountForCap(cap);
            return cap > 0;
        }

        internal static bool TryBuildTierEffect(string craftId, string buffId, out string text, out string semanticKey)
        {
            text = null;
            semanticKey = null;

            RebalancedPrayerRule rule;
            int tier;
            if (!RebalancedRuleSet.TryParseCraftId(craftId, out rule, out tier)) return false;
            if (rule.ReposeModes != null)
                return TryBuildReposeForecastEffect(rule, tier, out text, out semanticKey);
            return TryBuildRuleEffect(rule, tier, out text, out semanticKey);
        }

        internal static bool TryBuildActiveEffect(string buffId, out string text)
        {
            text = null;
            string semanticKey;
            RebalancedPrayerRule rule;
            int tier;

            switch (buffId)
            {
                case "buff_plant":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.PlantTierParam);
                    if (tier <= 0 || !RebalancedRuleSet.TryGet("b_plant", out rule)) return false;
                    float growthReduction = rule.TierValue(rule.GrowthReduction, tier) * 100f;
                    text = tier >= 3
                        ? Localization.F(
                            "rebalanced.active.plant",
                            growthReduction,
                            RebalancedRoots.MaxCombinedGrowthReduction * 100f)
                        : Localization.F("rebalanced.tech.plant_tier", growthReduction);
                    return true;
                case "buff_sins":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.ConfessionTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_sins", out rule) && TryBuildRuleEffect(rule, tier, out text, out semanticKey);
                case "buff_skull":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.ReposeTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_skull", out rule) && TryBuildReposeActiveEffect(rule, tier, out text, out semanticKey);
                case "buff_sword":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.CombatTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_sword", out rule) && TryBuildRuleEffect(rule, tier, out text, out semanticKey);
                case "buff_star":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.ExcellenceTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_star", out rule) && TryBuildRuleEffect(rule, tier, out text, out semanticKey);
                case "buff_gp_increase":
                    return RebalancedRuleSet.TryGet("b_grat_points_incr", out rule) && TryBuildRuleEffect(rule, 1, out text, out semanticKey);
                case "buff_sin_shard":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.SinShardTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_sin_shard", out rule) && TryBuildRuleEffect(rule, tier, out text, out semanticKey);
                default:
                    return false;
            }
        }

        internal static bool TryBuildTechnologyEffect(string craftId, out string sharedText, out string tierText)
        {
            sharedText = null;
            tierText = null;

            RebalancedPrayerRule rule;
            int tier;
            if (!RebalancedRuleSet.TryParseCraftId(craftId, out rule, out tier)) return false;

            if (rule.GrowthReduction != null)
            {
                sharedText = Localization.F("rebalanced.tech.plant_intro");
                tierText = Localization.F(
                    "rebalanced.tech.plant_tier",
                    rule.TierValue(rule.GrowthReduction, tier) * 100f);
                return true;
            }

            if (rule.ConfessionProbability != null)
            {
                sharedText = Localization.F("rebalanced.tech.sins_intro");
                tierText = Localization.F("rebalanced.active.sins", rule.TierValue(rule.ConfessionProbability, tier) * 100f);
                return true;
            }

            if (rule.ReposeModes != null)
            {
                // Technology is the comparison surface: state the shared future
                // outcome once, then show only the quality-dependent delta.
                sharedText = Localization.F("repose.forecast");
                ReposeQualityMode mode = rule.TierValue(rule.ReposeModes, tier, ReposeQualityMode.Stock);
                switch (mode)
                {
                    case ReposeQualityMode.Stock:
                        tierText = Localization.F("rebalanced.repose.tier.bronze");
                        return true;
                    case ReposeQualityMode.HalfwayToBest:
                        tierText = Localization.F("rebalanced.repose.tier.silver");
                        return true;
                    case ReposeQualityMode.Best:
                        tierText = Localization.F("rebalanced.repose.tier.gold");
                        return true;
                }
            }

            if (string.Equals(rule.PrayerId, "b_souls", StringComparison.Ordinal) &&
                rule.SoulGratitudeFaithCaps != null)
            {
                int cap = rule.TierValue(rule.SoulGratitudeFaithCaps, tier, 0);
                sharedText = Localization.F("rebalanced.tech.souls_intro");
                tierText = Localization.F("rebalanced.tech.souls_tier", cap);
                return true;
            }

            if (rule.CombatDamage != null && rule.CombatArmor != null && rule.CombatRegenPerSecond != null)
            {
                sharedText = Localization.F("rebalanced.tech.combat_intro");
                tierText = Localization.F(
                    "rebalanced.active.combat",
                    rule.TierValue(rule.CombatDamage, tier),
                    rule.TierValue(rule.CombatArmor, tier),
                    rule.TierValue(rule.CombatRegenPerSecond, tier));
                return true;
            }

            if (rule.CraftQualityBonus != null)
            {
                float value = rule.TierValue(rule.CraftQualityBonus, tier);
                if (string.Equals(rule.PrayerId, "b_pen", StringComparison.Ordinal))
                {
                    sharedText = Localization.F("tech.effect.imagination_intro") + "\n" +
                                 TechnologyTooltipTextStyle.GoldValueAfterColon(Localization.F("active.pen", value));
                    return true;
                }

                if (string.Equals(rule.PrayerId, "b_star", StringComparison.Ordinal))
                {
                    sharedText = Localization.F("tech.effect.excellence_intro");
                    tierText = Localization.F("active.star", value);
                    return true;
                }
            }

            if (rule.SoulGratitudeBonusRate != null)
            {
                sharedText = Localization.F(
                    "rebalanced.tech.gratitude_effect",
                    rule.TierValue(rule.SoulGratitudeBonusRate, tier) * 100f);
                return true;
            }

            if (rule.SinShardMultiplier != null)
            {
                float value = rule.TierValue(rule.SinShardMultiplier, tier);
                sharedText = Localization.F("rebalanced.tech.sin_shard_intro");
                tierText = Localization.F("rebalanced.active.sin_shard", value);
                return true;
            }

            return false;
        }

        private static bool TryBuildReposeForecastEffect(
            RebalancedPrayerRule rule,
            int tier,
            out string text,
            out string semanticKey)
        {
            text = Localization.F("repose.forecast");
            semanticKey = null;

            ReposeQualityMode mode = rule.TierValue(rule.ReposeModes, tier, ReposeQualityMode.Stock);
            switch (mode)
            {
                case ReposeQualityMode.Stock:
                    text = Localization.F("rebalanced.repose.forecast.bronze") + " " +
                           TechnologyTooltipTextStyle.CorpseQualityCue();
                    semanticKey = "rebalanced:repose=stock";
                    return true;
                case ReposeQualityMode.HalfwayToBest:
                    text += "\n" + Localization.F("rebalanced.repose.tier.silver");
                    semanticKey = "rebalanced:repose=halfway";
                    return true;
                case ReposeQualityMode.Best:
                    text += "\n" + Localization.F("rebalanced.repose.tier.gold");
                    semanticKey = "rebalanced:repose=best";
                    return true;
                default:
                    text = null;
                    return false;
            }
        }

        private static bool TryBuildReposeActiveEffect(
            RebalancedPrayerRule rule,
            int tier,
            out string text,
            out string semanticKey)
        {
            text = null;
            semanticKey = null;

            ReposeQualityMode mode = rule.TierValue(rule.ReposeModes, tier, ReposeQualityMode.Stock);
            bool stockAddsHigherTier = CorpseTierSemantics.StockReposeAddsHigherOrdinaryTier();
            bool reliabilityStillChangesDistribution =
                CorpseTierSemantics.BestTierNarrowingChangesDistribution();

            if (!stockAddsHigherTier)
            {
                switch (mode)
                {
                    case ReposeQualityMode.Stock:
                        text = Localization.F("repose.endpoint");
                        semanticKey = "rebalanced:repose=stock";
                        return true;
                    case ReposeQualityMode.HalfwayToBest:
                        text = reliabilityStillChangesDistribution
                            ? Localization.F("rebalanced.repose.active.silver") + " " +
                              TechnologyTooltipTextStyle.CorpseQualityCue()
                            : Localization.F("repose.endpoint");
                        semanticKey = "rebalanced:repose=halfway";
                        return true;
                    case ReposeQualityMode.Best:
                        text = reliabilityStillChangesDistribution
                            ? Localization.F("rebalanced.repose.active.gold") + " " +
                              TechnologyTooltipTextStyle.CorpseQualityCue()
                            : Localization.F("repose.endpoint");
                        semanticKey = "rebalanced:repose=best";
                        return true;
                    default:
                        return false;
                }
            }

            text = Localization.F("active.skull");
            switch (mode)
            {
                case ReposeQualityMode.Stock:
                    semanticKey = "rebalanced:repose=stock";
                    return true;
                case ReposeQualityMode.HalfwayToBest:
                    text += "\n" + Localization.F("rebalanced.repose.active.silver");
                    semanticKey = "rebalanced:repose=halfway";
                    return true;
                case ReposeQualityMode.Best:
                    text += "\n" + Localization.F("rebalanced.repose.active.gold");
                    semanticKey = "rebalanced:repose=best";
                    return true;
                default:
                    text = null;
                    return false;
            }
        }

        private static bool TryBuildRuleEffect(RebalancedPrayerRule rule, int tier, out string text, out string semanticKey)
        {
            text = null;
            semanticKey = null;
            if (rule == null || tier < 1 || tier > 3) return false;

            if (rule.GrowthReduction != null)
            {
                float value = rule.TierValue(rule.GrowthReduction, tier);
                text = Localization.F(
                    "rebalanced.tech.plant_tier",
                    value * 100f);
                semanticKey = "rebalanced:growth=" + Rv(value) +
                              ";cap=" + Rv(RebalancedRoots.MaxCombinedGrowthReduction);
                return true;
            }

            if (rule.ConfessionProbability != null)
            {
                float value = rule.TierValue(rule.ConfessionProbability, tier);
                text = Localization.F("rebalanced.active.sins_detailed", value * 100f);
                semanticKey = "rebalanced:confession=" + Rv(value);
                return true;
            }

            if (rule.ReposeModes != null)
                return TryBuildReposeForecastEffect(rule, tier, out text, out semanticKey);

            if (rule.SoulGratitudeFaithCaps != null &&
                string.Equals(rule.PrayerId, "b_souls", StringComparison.Ordinal))
            {
                int cap = rule.TierValue(rule.SoulGratitudeFaithCaps, tier, 0);
                text = Localization.F("rebalanced.tech.souls_tier", cap);
                semanticKey = "rebalanced:souls_conversion_cap=" + cap.ToString(CultureInfo.InvariantCulture);
                return true;
            }

            if (rule.CombatDamage != null && rule.CombatArmor != null && rule.CombatRegenPerSecond != null)
            {
                float damage = rule.TierValue(rule.CombatDamage, tier);
                float armor = rule.TierValue(rule.CombatArmor, tier);
                float regen = rule.TierValue(rule.CombatRegenPerSecond, tier);
                text = Localization.F("rebalanced.active.combat", damage, armor, regen);
                semanticKey = "rebalanced:combat=" + Rv(damage) + "," + Rv(armor) + "," + Rv(regen);
                return true;
            }

            if (rule.CraftQualityBonus != null)
            {
                float value = rule.TierValue(rule.CraftQualityBonus, tier);
                if (string.Equals(rule.PrayerId, "b_pen", StringComparison.Ordinal))
                    text = Localization.F("active.pen", value);
                else if (string.Equals(rule.PrayerId, "b_star", StringComparison.Ordinal))
                    text = Localization.F("active.star", value);
                else
                    return false;
                semanticKey = "rebalanced:craft_q=" + Rv(value);
                return true;
            }

            if (rule.SoulGratitudeBonusRate != null)
            {
                float value = rule.TierValue(rule.SoulGratitudeBonusRate, tier);
                text = Localization.F("rebalanced.active.gratitude", value * 100f);
                semanticKey = "rebalanced:gratitude=" + Rv(value);
                return true;
            }

            if (rule.SinShardMultiplier != null)
            {
                float value = rule.TierValue(rule.SinShardMultiplier, tier);
                text = Localization.F("rebalanced.active.sin_shard", value);
                semanticKey = "rebalanced:sin_shard=" + Rv(value);
                return true;
            }

            return false;
        }

        private static string Rv(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
