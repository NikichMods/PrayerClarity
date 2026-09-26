using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace PrayerClarity
{
    internal static class PrayerForecast
    {
        internal enum BonusHighlight
        {
            None,
            Faith,
            Money
        }

        internal sealed class Result
        {
            internal string CraftId;
            internal int QualityTier;

            // Exact resolved values stay available for correctness/tests/balance work.
            // Default pulpit rendering intentionally does not expose these totals.
            internal int BaseFaith;
            internal float BaseMoney;
            internal int BonusFaith;
            internal float BonusMoney;

            // Prayer-owned success contribution, kept separate from resolved payout so
            // presentation can explain the mechanic without spoiling sermon rewards.
            internal float FaithBonusRate;
            internal float MoneyBonusRate;
            internal int FixedFaithBonus;
            internal float FixedMoneyBonus;

            internal int ChancePercent;
            internal float GraveyardQuality;
            internal float SoulGratitude;
            internal bool UsesSoulGratitude;
            internal int SoulGratitudeFaithCap;
            internal int SoulGratitudeConversion;
            internal BonusHighlight Highlight;
            internal string SpecialText;
            // Leading effect icon only. For timed effects this is the exact
            // BuffDefinition.GetIconName() used by vanilla BuffIcon.Draw.
            internal string SpecialIconName;
        }

        internal sealed class TierDetails
        {
            internal string CraftId;
            internal string EventId;
            internal int QualityTier;
            internal int Requirement;
            internal float FaithBonusRate;
            internal float MoneyBonusRate;
            internal int FixedFaithBonus;
            internal float FixedMoneyBonus;
            internal bool UsesSoulGratitude;
            internal int SoulGratitudeFaithCap;
            internal BonusHighlight Highlight;

            // Structured special-effect data for property-first tooltip comparison.
            // SemanticKey excludes duration and is built from raw IDs/values before
            // localization, so identical mechanics can be collapsed safely.
            internal string SpecialSemanticKey;
            internal string SpecialCoreText;
            internal float SpecialDurationDays;
            internal bool HasSpecialDuration;

            // Existing combined representation remains for pulpit/other accepted
            // surfaces so this tooltip polish does not change their grammar.
            internal string SpecialText;
            internal string SpecialIconName;
        }

        private sealed class SpecialInfo
        {
            internal readonly string Text;
            internal readonly string CoreText;
            internal readonly string SemanticKey;
            internal readonly string IconName;
            internal readonly float DurationDays;
            internal readonly bool HasDuration;

            internal SpecialInfo(
                string text,
                string coreText,
                string semanticKey,
                string iconName,
                float durationDays,
                bool hasDuration)
            {
                Text = text;
                CoreText = coreText;
                SemanticKey = semanticKey;
                IconName = iconName;
                DurationDays = durationDays;
                HasDuration = hasDuration;
            }
        }

        private static MethodInfo _fromTimeKToSeconds;

        internal static Result Build(object prayGui, float chance)
        {
            object craft = R.Get(prayGui, "pray_craft");
            TierDetails tier = BuildTierDetails(craft);
            if (tier == null) return null;

            object prayEvent = R.BalanceData(tier.EventId, "PrayEventDefinition", false);
            int baseFaith = Mathf.Max(0, Mathf.RoundToInt(R.SmartFloat(R.Get(prayEvent, "faith"))));
            float baseMoney = Mathf.Max(0f, R.SmartFloat(R.Get(prayEvent, "money")));

            // Preserve the verified exact side-effect-free calculator unchanged.
            // Edition-specific mechanics can contribute through the shared provider
            // without making the Vanilla sibling depend on Rebalanced types.
            int dynamicCap;
            int soulConversion;
            if (!PrayerEditionSemantics.TryGetSoulConversion(tier.CraftId, out dynamicCap, out soulConversion))
            {
                dynamicCap = 0;
                soulConversion = 0;
            }
            int bonusFaith = tier.FixedFaithBonus + Mathf.RoundToInt(baseFaith * tier.FaithBonusRate) + soulConversion;
            float bonusMoney = tier.FixedMoneyBonus + Mathf.Round(baseMoney * tier.MoneyBonusRate * 100f) / 100f;

            return new Result
            {
                CraftId = tier.CraftId,
                QualityTier = tier.QualityTier,
                BaseFaith = baseFaith,
                BaseMoney = baseMoney,
                BonusFaith = bonusFaith,
                BonusMoney = bonusMoney,
                FaithBonusRate = tier.FaithBonusRate,
                MoneyBonusRate = tier.MoneyBonusRate,
                FixedFaithBonus = tier.FixedFaithBonus,
                FixedMoneyBonus = tier.FixedMoneyBonus,
                ChancePercent = Mathf.RoundToInt(Mathf.Clamp01(chance) * 100f),
                GraveyardQuality = R.ZoneQuality("graveyard"),
                SoulGratitude = (tier.UsesSoulGratitude || tier.SoulGratitudeFaithCap > 0)
                    ? R.PlayerParam("gratitude_points")
                    : 0f,
                UsesSoulGratitude = tier.UsesSoulGratitude,
                SoulGratitudeFaithCap = dynamicCap,
                SoulGratitudeConversion = soulConversion,
                Highlight = tier.Highlight,
                SpecialText = tier.SpecialText,
                SpecialIconName = tier.SpecialIconName
            };
        }

        internal static TierDetails BuildTierDetails(object craft)
        {
            if (craft == null) return null;

            string craftId = R.Id(craft) ?? string.Empty;
            if (!craftId.StartsWith("pray:", StringComparison.Ordinal)) return null;

            string eventId = R.Get(craft, "linked_sub_id") as string;
            if (string.IsNullOrEmpty(eventId)) return null;

            string effectiveEventId;
            if (PrayerEditionSemantics.TryGetEffectivePrayEvent(craftId, eventId, out effectiveEventId))
                eventId = effectiveEventId;

            Localization.UseCurrentGameLanguage();

            int fixedFaith = 0;
            float fixedMoney = 0f;
            List<RewardItem> rewards = new List<RewardItem>();
            CollectOutputs(craft, ref fixedFaith, ref fixedMoney, rewards);

            int qualityTier = ParseQualityTier(craftId);
            int soulGratitudeFaithCap;
            int ignoredConversion;
            if (!PrayerEditionSemantics.TryGetSoulConversion(craftId, out soulGratitudeFaithCap, out ignoredConversion))
                soulGratitudeFaithCap = 0;
            SpecialInfo special = BuildSpecial(craft, eventId, rewards, soulGratitudeFaithCap);
            return new TierDetails
            {
                CraftId = craftId,
                EventId = eventId,
                QualityTier = qualityTier,
                Requirement = Mathf.Max(0, Mathf.RoundToInt(R.Float(R.Get(craft, "needs_quality")))),
                FaithBonusRate = R.Float(R.Get(craft, "k_faith")),
                MoneyBonusRate = R.Float(R.Get(craft, "k_money")),
                FixedFaithBonus = fixedFaith,
                FixedMoneyBonus = fixedMoney,
                UsesSoulGratitude = eventId.StartsWith("pray_for_souls_", StringComparison.Ordinal),
                SoulGratitudeFaithCap = soulGratitudeFaithCap,
                Highlight = GetBonusHighlight(craftId),
                SpecialSemanticKey = special == null ? null : special.SemanticKey,
                SpecialCoreText = special == null ? null : special.CoreText,
                SpecialDurationDays = special == null ? 0f : special.DurationDays,
                HasSpecialDuration = special != null && special.HasDuration,
                SpecialText = special == null ? null : special.Text,
                SpecialIconName = special == null ? null : special.IconName
            };
        }

        internal static string BuildActiveBuffText(object playerBuff)
        {
            if (playerBuff == null) return null;
            Localization.UseCurrentGameLanguage();

            object buff = R.Get(playerBuff, "definition");
            if (buff == null) return null;
            string buffId = R.Id(buff) ?? string.Empty;
            object res = R.Get(buff, "res");

            string editionText;
            if (PrayerEditionSemantics.TryBuildActiveEffect(buffId, out editionText))
                return editionText;

            switch (buffId)
            {
                case "buff_sword":
                    return NumberedActiveResEffect("active.sword", res, "add_damage");
                case "buff_shield":
                    return NumberedActiveResEffect("active.shield", res, "add_armor");
                case "buff_skull":
                    return CorpseTierSemantics.StockReposeAddsHigherOrdinaryTier()
                        ? NumberedActiveResEffect("active.skull", res, "body_max")
                        : Localization.F("repose.endpoint");
                case "buff_pen":
                    return Localization.F("active.pen", R.Float(R.Get(buff, "craft_q")));
                case "buff_star":
                    return Localization.F("active.star", R.Float(R.Get(buff, "craft_q")));
                case "buff_plant":
                    return Localization.F("active.plant_inactive");
                case "buff_sins":
                    return Localization.F("active.sins_unverified");
                case "buff_gp_increase":
                    return Localization.F("active.gratitude");
                case "buff_sin_shard":
                    return Localization.F("active.sin_shard");
                default:
                    return null;
            }
        }

        private static int ParseQualityTier(string craftId)
        {
            if (string.IsNullOrEmpty(craftId)) return 0;
            int index = craftId.LastIndexOf(':');
            if (index < 0 || index + 1 >= craftId.Length) return 0;
            int tier;
            return int.TryParse(craftId.Substring(index + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out tier)
                ? tier
                : 0;
        }

        private static BonusHighlight GetBonusHighlight(string craftId)
        {
            if (craftId.StartsWith("pray:b_faith:", StringComparison.Ordinal))
                return BonusHighlight.Faith;
            if (craftId.StartsWith("pray:b_money:", StringComparison.Ordinal))
                return BonusHighlight.Money;
            return BonusHighlight.None;
        }

        private static void CollectOutputs(object craft, ref int fixedFaith, ref float fixedMoney, List<RewardItem> rewards)
        {
            IEnumerable output = R.Get(craft, "output") as IEnumerable;
            if (output == null) return;

            foreach (object item in output)
            {
                if (item == null) continue;
                string id = R.Id(item) ?? string.Empty;
                int value = R.Int(R.Get(item, "value"));
                if (string.Equals(id, "faith", StringComparison.Ordinal)) fixedFaith += value;
                else if (string.Equals(id, "money", StringComparison.Ordinal)) fixedMoney += value / 100f;
                else if (!string.IsNullOrEmpty(id) && value > 0) rewards.Add(new RewardItem(id, value));
            }
        }

        private static SpecialInfo BuildSpecial(object craft, string eventId, List<RewardItem> rewards, int soulGratitudeFaithCap)
        {
            List<string> displayParts = new List<string>();
            List<string> coreParts = new List<string>();
            List<string> semanticParts = new List<string>();
            string iconName = null;
            float durationDays = 0f;
            bool hasDuration = false;

            // The event family is the verified discriminator for the BSS prayer. Keep
            // the player-facing mechanic concise instead of copying the flavor sentence.
            if (eventId.StartsWith("pray_for_souls_", StringComparison.Ordinal))
            {
                string text = Localization.F("buff.souls_repose");
                displayParts.Add(text);
                coreParts.Add(text);
                semanticParts.Add("event:pray_for_souls");
            }

            string craftId = R.Id(craft) ?? string.Empty;

            if (soulGratitudeFaithCap > 0)
            {
                string soulsText = Localization.F("rebalanced.item.souls_repose", soulGratitudeFaithCap);
                displayParts.Add(soulsText);
                coreParts.Add(soulsText);
                semanticParts.Add("rebalanced:souls_conversion_cap=" +
                                  soulGratitudeFaithCap.ToString(CultureInfo.InvariantCulture));
            }

            string buffId = R.Get(craft, "buff") as string;
            float duration = R.Float(R.Get(craft, "dur_parameter"));
            SpecialInfo buff = BuildBuffEffect(craftId, buffId, duration);
            if (buff != null)
            {
                if (!string.IsNullOrEmpty(buff.Text)) displayParts.Add(buff.Text);
                if (!string.IsNullOrEmpty(buff.CoreText)) coreParts.Add(buff.CoreText);
                if (!string.IsNullOrEmpty(buff.SemanticKey)) semanticParts.Add(buff.SemanticKey);
                if (!string.IsNullOrEmpty(buff.IconName)) iconName = buff.IconName;
                if (buff.HasDuration)
                {
                    hasDuration = true;
                    durationDays = buff.DurationDays;
                }
            }

            if (rewards.Count > 0)
            {
                List<string> rewardParts = new List<string>();
                List<string> rewardKeys = new List<string>();
                foreach (RewardItem reward in rewards)
                {
                    rewardParts.Add(BuildRewardText(reward));
                    rewardKeys.Add(reward.Id + "=" + reward.Value.ToString(CultureInfo.InvariantCulture));
                }
                rewardKeys.Sort(StringComparer.Ordinal);

                string rewardText = Localization.F("forecast.reward", string.Join(", ", rewardParts.ToArray()));
                displayParts.Add(rewardText);
                coreParts.Add(rewardText);
                semanticParts.Add("rewards:" + string.Join(",", rewardKeys.ToArray()));
            }

            if (displayParts.Count == 0) return null;
            return new SpecialInfo(
                string.Join(" · ", displayParts.ToArray()),
                string.Join(" · ", coreParts.ToArray()),
                string.Join("|", semanticParts.ToArray()),
                iconName,
                durationDays,
                hasDuration);
        }

        private static string BuildRewardText(RewardItem reward)
        {
            string name = R.VanillaLocalize(reward.Id);
            string amount = " ×" + reward.Value.ToString(CultureInfo.InvariantCulture);

            if (string.Equals(reward.Id, "blessing_commerce", StringComparison.Ordinal))
            {
                string description = R.VanillaLocalize("blessing_commerce_d");
                if (!string.IsNullOrEmpty(description) &&
                    !string.Equals(description, "blessing_commerce_d", StringComparison.Ordinal))
                    return name + amount + " — " + description;
            }

            return name + amount;
        }

        private static SpecialInfo BuildBuffEffect(string craftId, string buffId, float duration)
        {
            if (string.IsNullOrEmpty(buffId)) return null;

            object buff = R.BalanceData(buffId, "BuffDefinition", true);
            object res = buff == null ? null : R.Get(buff, "res");
            string iconName = GetBuffIconName(buff);

            string editionText;
            string editionSemanticKey;
            if (PrayerEditionSemantics.TryBuildTierEffect(craftId, buffId, out editionText, out editionSemanticKey))
            {
                float editionDurationDays = duration > 0.0001f ? DurationParameterToGameDays(duration) : 0f;
                bool editionHasDuration = duration > 0.0001f;
                string editionDisplay = editionText;
                if (editionHasDuration)
                    editionDisplay += " · " + Localization.F("active.timer_days", editionDurationDays);

                return new SpecialInfo(
                    editionDisplay,
                    editionText,
                    editionSemanticKey,
                    iconName,
                    editionDurationDays,
                    editionHasDuration);
            }

            bool showDuration = true;
            string semanticKey = "buff:" + buffId;
            string text;
            switch (buffId)
            {
                case "buff_sword":
                {
                    float value = res == null ? 0f : R.GameResGet(res, "add_damage");
                    text = NumberedActiveResEffect("active.sword", res, "add_damage");
                    semanticKey += ":add_damage=" + value.ToString("R", CultureInfo.InvariantCulture);
                    break;
                }
                case "buff_shield":
                {
                    float value = res == null ? 0f : R.GameResGet(res, "add_armor");
                    text = NumberedActiveResEffect("active.shield", res, "add_armor");
                    semanticKey += ":add_armor=" + value.ToString("R", CultureInfo.InvariantCulture);
                    break;
                }
                case "buff_skull":
                {
                    float value = res == null ? 0f : R.GameResGet(res, "body_max");
                    text = NumberedActiveResEffect("repose.forecast", res, "body_max");
                    semanticKey += ":body_max=" + value.ToString("R", CultureInfo.InvariantCulture);
                    break;
                }
                case "buff_pen":
                {
                    float value = buff == null ? 0f : R.Float(R.Get(buff, "craft_q"));
                    text = buff == null ? null : Localization.F("active.pen", value);
                    semanticKey += ":craft_q=" + value.ToString("R", CultureInfo.InvariantCulture);
                    break;
                }
                case "buff_star":
                {
                    float value = buff == null ? 0f : R.Float(R.Get(buff, "craft_q"));
                    text = buff == null ? null : Localization.F("active.star", value);
                    semanticKey += ":craft_q=" + value.ToString("R", CultureInfo.InvariantCulture);
                    break;
                }
                case "buff_plant":
                    text = Localization.F("active.plant_inactive");
                    showDuration = false;
                    break;
                case "buff_sins":
                    text = Localization.F("active.sins_unverified");
                    showDuration = false;
                    break;
                case "buff_gp_increase":
                    text = Localization.F("active.gratitude");
                    break;
                case "buff_sin_shard":
                    text = Localization.F("active.sin_shard");
                    break;
                default:
                    text = null;
                    break;
            }

            if (string.IsNullOrEmpty(text)) return null;

            float durationDays = 0f;
            bool hasDuration = showDuration && duration > 0.0001f;
            string displayText = text;
            if (hasDuration)
            {
                durationDays = DurationParameterToGameDays(duration);
                displayText += " · " + Localization.F("active.timer_days", durationDays);
            }

            return new SpecialInfo(
                displayText,
                text,
                semanticKey,
                iconName,
                durationDays,
                hasDuration);
        }

        internal static float DurationParameterToGameDays(float durationMinutes)
        {
            if (durationMinutes <= 0.0001f) return 0f;

            if (_fromTimeKToSeconds == null)
            {
                Type timeOfDay = R.GameType("TimeOfDay");
                _fromTimeKToSeconds = R.Method(timeOfDay, "FromTimeKToSeconds", true, new[] { typeof(float) });
            }
            if (_fromTimeKToSeconds == null)
                throw new MissingMethodException("TimeOfDay.FromTimeKToSeconds(float)");

            float secondsPerDay = R.Float(_fromTimeKToSeconds.Invoke(null, new object[] { 1f }));
            if (secondsPerDay <= 0.0001f)
                throw new InvalidOperationException("TimeOfDay.FromTimeKToSeconds(1) returned an invalid day length.");

            // dur_parameter is the verified prayer-buff duration in real-time minutes.
            // Convert it through the game's effective day length. Longer Days patches
            // the same TimeOfDay method, so this automatically reflects that mod with
            // no dependency or per-mod branch.
            return durationMinutes * 60f / secondsPerDay;
        }

        private static string NumberedActiveResEffect(string key, object res, string resKey)
        {
            if (res == null) return null;
            float value = R.GameResGet(res, resKey);
            if (Math.Abs(value) < 0.0001f) return null;
            return Localization.F(key, value);
        }

        private static string GetBuffIconName(object buff)
        {
            if (buff == null) return null;
            MethodInfo method = R.Method(buff.GetType(), "GetIconName", false, 0);
            object value = method == null ? null : method.Invoke(buff, null);
            return value == null ? null : value.ToString();
        }

        private sealed class RewardItem
        {
            internal readonly string Id;
            internal readonly int Value;

            internal RewardItem(string id, int value)
            {
                Id = id;
                Value = value;
            }
        }
    }
}
