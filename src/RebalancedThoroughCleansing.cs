using System;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedThoroughCleansing
    {
        private const string StockSinShardParam = "increase_sin_shard_drop";

        private sealed class ScopedState
        {
            internal object RuntimeEffect;
            internal float OriginalValue;
        }

        private static ManualLogSource _log;
        private static MethodInfo _runtimeEffectGet;
        private static MethodInfo _runtimeEffectSet;
        private static MethodInfo _runtimeEffectRemoveZeroValues;
        private static bool _runtimeErrorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type soulHealingWidget = R.GameType("SoulHealingWidget");
            Type gameRes = R.GameType("GameRes");
            if (soulHealingWidget == null) throw new MissingMemberException("SoulHealingWidget");
            if (gameRes == null) throw new MissingMemberException("GameRes");

            MethodInfo heal = R.Method(soulHealingWidget, "OnStartHealButtonPressed", false, Type.EmptyTypes);
            if (heal == null) throw new MissingMethodException("SoulHealingWidget.OnStartHealButtonPressed()");

            _runtimeEffectGet = gameRes.GetMethod(
                "Get", R.Inst, null, new[] { typeof(string), typeof(float) }, null);
            _runtimeEffectSet = gameRes.GetMethod(
                "Set", R.Inst, null, new[] { typeof(string), typeof(float) }, null);
            _runtimeEffectRemoveZeroValues = gameRes.GetMethod(
                "RemoveZeroValues", R.Inst, null, Type.EmptyTypes, null);

            if (_runtimeEffectGet == null) throw new MissingMethodException("GameRes.Get(string,float)");
            if (_runtimeEffectSet == null) throw new MissingMethodException("GameRes.Set(string,float)");
            if (_runtimeEffectRemoveZeroValues == null) throw new MissingMethodException("GameRes.RemoveZeroValues()");

            R.PatchHooks(
                harmonyId + ".thoroughcleansing.scope",
                typeof(RebalancedThoroughCleansing),
                heal,
                nameof(HealPrefix),
                null,
                nameof(HealFinalizer));
        }

        private static void HealPrefix(ref ScopedState __state)
        {
            __state = null;
            if (!RebalancedRuntimeState.IsReady) return;
            try
            {
                float stockActive = RebalancedTierState.GetPlayerParam(StockSinShardParam, 0f);
                if (stockActive <= 0.0001f) return;

                int tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.SinShardTierParam);
                RebalancedPrayerRule rule;
                if (tier <= 0 || !RebalancedRuleSet.TryGet("b_sin_shard", out rule)) return;

                float desiredMultiplier = rule.TierValue(rule.SinShardMultiplier, tier, 2f);
                float additionalPlayerParam = Math.Max(0f, desiredMultiplier - 2f);
                if (additionalPlayerParam <= 0.0001f) return;

                object mainGame = R.GetStatic(R.GameType("MainGame"), "me");
                object player = mainGame == null ? null : R.Get(mainGame, "player");
                if (player == null) return;

                object runtimeEffect = R.Get(player, "totem_effect");
                if (runtimeEffect == null)
                    throw new MissingMemberException("WorldGameObject.totem_effect");

                float original = Convert.ToSingle(
                    _runtimeEffectGet.Invoke(runtimeEffect, new object[] { StockSinShardParam, 0f }));

                __state = new ScopedState
                {
                    RuntimeEffect = runtimeEffect,
                    OriginalValue = original
                };

                _runtimeEffectSet.Invoke(
                    runtimeEffect,
                    new object[] { StockSinShardParam, original + additionalPlayerParam });
            }
            catch (Exception ex)
            {
                Restore(__state);
                __state = null;
                LogFailure("PrayerClarity: Rebalanced Thorough Cleansing tier projection failed; this healing keeps stock shard output. ", ex);
            }
        }

        private static Exception HealFinalizer(Exception __exception, ScopedState __state)
        {
            Restore(__state);
            return __exception;
        }

        private static void Restore(ScopedState state)
        {
            if (state == null || state.RuntimeEffect == null) return;
            try
            {
                _runtimeEffectSet.Invoke(
                    state.RuntimeEffect,
                    new object[] { StockSinShardParam, state.OriginalValue });

                if (Math.Abs(state.OriginalValue) <= 0.0001f)
                    _runtimeEffectRemoveZeroValues.Invoke(state.RuntimeEffect, null);
            }
            catch (Exception ex)
            {
                LogFailure("PrayerClarity: Rebalanced Thorough Cleansing scope restoration failed. ", ex);
            }
        }

        private static void LogFailure(string message, Exception ex)
        {
            if (_runtimeErrorLogged) return;
            _runtimeErrorLogged = true;
            _log?.LogError("PC_RUNTIME_FALLBACK edition=rebalanced feature=thorough-cleansing action=stock " + message + ex);
        }
    }
}
