using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedTierState
    {
        internal const string PlantTierParam = "prayerclarity_rebalanced_plant_tier";
        internal const string PlantReductionParam = "prayerclarity_rebalanced_plant_reduction";
        internal const string ConfessionTierParam = "prayerclarity_rebalanced_confession_tier";
        internal const string ReposeTierParam = "prayerclarity_rebalanced_repose_tier";
        internal const string CombatTierParam = "prayerclarity_rebalanced_combat_tier";
        internal const string CombatRegenParam = "prayerclarity_rebalanced_combat_regen";
        internal const string ExcellenceTierParam = "prayerclarity_rebalanced_excellence_tier";
        internal const string SinShardTierParam = "prayerclarity_rebalanced_sin_shard_tier";

        private static readonly Dictionary<string, string> PrayerToToken =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["b_plant"] = PlantTierParam,
                ["b_sins"] = ConfessionTierParam,
                ["b_skull"] = ReposeTierParam,
                ["b_sword"] = CombatTierParam,
                ["b_shield"] = CombatTierParam,
                ["b_star"] = ExcellenceTierParam,
                ["b_sin_shard"] = SinShardTierParam
            };

        private static ManualLogSource _log;
        private static MethodInfo _setPlayerParam;
        private static MethodInfo _getPlayerParam;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            Type playerComponent = R.GameType("PlayerComponent");
            MethodInfo startPrayAnimation = R.Method(
                playerComponent,
                "StartPrayAnimation",
                false,
                new[] { R.GameType("CraftDefinition"), typeof(bool) });
            if (startPrayAnimation == null)
                throw new MissingMethodException("PlayerComponent.StartPrayAnimation(CraftDefinition,bool)");

            R.Patch(harmonyId + ".tier.capture", typeof(RebalancedTierState), startPrayAnimation, nameof(StartPrayAnimationPostfix));
        }

        private static void StartPrayAnimationPostfix(object __instance)
        {
            try
            {
                if (!RebalancedRuntimeState.IsReady) return;
                if (__instance == null || !Convert.ToBoolean(R.Get(__instance, "_pray_buff_success"))) return;

                object craft = R.Get(__instance, "_pray_craft");
                string craftId = R.Id(craft);
                RebalancedPrayerRule rule;
                int tier;
                if (!RebalancedRuleSet.TryParseCraftId(craftId, out rule, out tier)) return;

                string token;
                if (PrayerToToken.TryGetValue(rule.PrayerId, out token))
                    SetPlayerParam(token, tier);

                switch (rule.PrayerId)
                {
                    case "b_plant":
                        SetPlayerParam(PlantReductionParam, rule.TierValue(rule.GrowthReduction, tier));
                        break;
                    case "b_sword":
                    case "b_shield":
                        SetPlayerParam(CombatRegenParam, rule.TierValue(rule.CombatRegenPerSecond, tier));
                        break;
                }
            }
            catch (Exception ex)
            {
                _log?.LogError("PC_RUNTIME_FALLBACK edition=rebalanced feature=tier-capture action=leave-tier-state-unchanged " + ex);
            }
        }

        internal static int GetCapturedTier(string token)
        {
            float value = GetPlayerParam(token, 0f);
            int tier = (int)Math.Round(value);
            return tier >= 1 && tier <= 3 ? tier : 0;
        }

        internal static int GetCapturedTierForPrayer(string prayerId)
        {
            string token;
            return PrayerToToken.TryGetValue(prayerId ?? string.Empty, out token)
                ? GetCapturedTier(token)
                : 0;
        }

        internal static float GetPlayerParam(string param, float fallback)
        {
            object player = GetPlayer();
            if (player == null) return fallback;

            ResolvePlayerParamMethods(player.GetType());
            if (_getPlayerParam == null) return fallback;
            return Convert.ToSingle(_getPlayerParam.Invoke(player, new object[] { param, fallback }));
        }

        private static void SetPlayerParam(string token, float value)
        {
            object player = GetPlayer();
            if (player == null) throw new InvalidOperationException("MainGame.player unavailable during prayer tier capture.");

            ResolvePlayerParamMethods(player.GetType());
            if (_setPlayerParam == null)
                throw new MissingMethodException("WorldGameObject.SetParam(string,float)");

            _setPlayerParam.Invoke(player, new object[] { token, value });
        }

        private static object GetPlayer()
        {
            object mainGame = R.GetStatic(R.GameType("MainGame"), "me");
            return mainGame == null ? null : R.Get(mainGame, "player");
        }

        private static void ResolvePlayerParamMethods(Type playerType)
        {
            if (_setPlayerParam == null)
                _setPlayerParam = playerType.GetMethod("SetParam", R.Inst, null, new[] { typeof(string), typeof(float) }, null);
            if (_getPlayerParam == null)
                _getPlayerParam = playerType.GetMethod("GetParam", R.Inst, null, new[] { typeof(string), typeof(float) }, null);
        }
    }
}
