using System;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedExcellence
    {
        private static ManualLogSource _log;
        private static bool _runtimeErrorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            Type craftDefinition = R.GameType("CraftDefinition");
            MethodInfo getBuffValue = R.Method(craftDefinition, "GetBuffValue", false, new[] { typeof(string) });
            if (getBuffValue == null) throw new MissingMethodException("CraftDefinition.GetBuffValue(string)");

            R.Patch(harmonyId + ".excellence", typeof(RebalancedExcellence), getBuffValue, nameof(GetBuffValuePostfix));
        }

        private static void GetBuffValuePostfix(string buff_id, ref float __result)
        {
            if (!RebalancedRuntimeState.IsReady) return;
            if (!string.Equals(buff_id, "buff_star", StringComparison.Ordinal) || __result <= 0f) return;

            try
            {
                int tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.ExcellenceTierParam);
                if (tier < 1 || tier > 3) return;

                RebalancedPrayerRule rule;
                if (!RebalancedRuleSet.TryGet("b_star", out rule) || rule.CraftQualityBonus == null) return;
                __result = rule.TierValue(rule.CraftQualityBonus, tier, __result);
            }
            catch (Exception ex)
            {
                if (_runtimeErrorLogged) return;
                _runtimeErrorLogged = true;
                _log?.LogError("PrayerClarity: Rebalanced Excellence quality override failed closed; stock craft_q remains active. " + ex);
            }
        }
    }
}
