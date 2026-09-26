using System;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedCombat
    {
        private const string DamageParam = "add_damage";
        private const string ArmorParam = "add_armor";
        private const float StockCombatDamage = 5f;

        private sealed class ScopedParamState
        {
            internal object RuntimeEffect;
            internal string Param;
            internal float OriginalValue;
        }

        private static ManualLogSource _log;
        private static MethodInfo _findBuffById;
        private static MethodInfo _runtimeEffectGet;
        private static MethodInfo _runtimeEffectSet;
        private static MethodInfo _runtimeEffectRemoveZeroValues;
        private static bool _runtimeErrorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            ResolveBuffApi();

            Type worldGameObject = R.GameType("WorldGameObject");
            Type damageType = R.GameType("ObjectDefinition+DamageType");
            Type hpAction = R.GameType("HPActionComponent");
            Type gameRes = R.GameType("GameRes");
            if (worldGameObject == null || damageType == null || hpAction == null || gameRes == null)
                throw new MissingMemberException("Combat runtime types are unavailable.");

            MethodInfo getDamage = R.Method(worldGameObject, "GetDamage", false, new[] { damageType });
            MethodInfo decHp = R.Method(hpAction, "DecHP", false, new[] { typeof(float) });
            if (getDamage == null) throw new MissingMethodException("WorldGameObject.GetDamage(DamageType)");
            if (decHp == null) throw new MissingMethodException("HPActionComponent.DecHP(float)");

            _runtimeEffectGet = gameRes.GetMethod(
                "Get",
                R.Inst,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            _runtimeEffectSet = gameRes.GetMethod(
                "Set",
                R.Inst,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            _runtimeEffectRemoveZeroValues = gameRes.GetMethod(
                "RemoveZeroValues",
                R.Inst,
                null,
                Type.EmptyTypes,
                null);

            if (_runtimeEffectGet == null) throw new MissingMethodException("GameRes.Get(string,float)");
            if (_runtimeEffectSet == null) throw new MissingMethodException("GameRes.Set(string,float)");
            if (_runtimeEffectRemoveZeroValues == null) throw new MissingMethodException("GameRes.RemoveZeroValues()");

            ValidateStockCombatBuff();

            R.PatchHooks(
                harmonyId + ".combat.damage.scope",
                typeof(RebalancedCombat),
                getDamage,
                nameof(GetDamagePrefix),
                null,
                nameof(GetDamageFinalizer));
            R.PatchHooks(
                harmonyId + ".combat.armor.scope",
                typeof(RebalancedCombat),
                decHp,
                nameof(DecHpPrefix),
                null,
                nameof(DecHpFinalizer));
        }

        private static void GetDamagePrefix(object __instance, ref ScopedParamState __state)
        {
            __state = null;
            if (!RebalancedRuntimeState.IsReady) return;
            try
            {
                int tier;
                if (!TryGetActiveCombatTier(__instance, out tier)) return;

                RebalancedPrayerRule rule;
                if (!RebalancedRuleSet.TryGet("b_sword", out rule)) return;

                float targetDamage = rule.TierValue(rule.CombatDamage, tier, StockCombatDamage);
                float extra = Math.Max(0f, targetDamage - StockCombatDamage);
                if (extra <= 0.0001f) return;

                BeginProjection(__instance, DamageParam, extra, ref __state);
            }
            catch (Exception ex)
            {
                Restore(__state);
                __state = null;
                LogRuntimeError("Combat outgoing-damage input projection failed closed", ex);
            }
        }

        private static Exception GetDamageFinalizer(Exception __exception, ScopedParamState __state)
        {
            Restore(__state);
            return __exception;
        }

        private static void DecHpPrefix(object __instance, ref ScopedParamState __state)
        {
            __state = null;
            if (!RebalancedRuntimeState.IsReady) return;
            try
            {
                object wgo = R.Get(__instance, "wgo");
                int tier;
                if (!TryGetActiveCombatTier(wgo, out tier)) return;

                RebalancedPrayerRule rule;
                if (!RebalancedRuleSet.TryGet("b_sword", out rule)) return;

                float armor = rule.TierValue(rule.CombatArmor, tier, 0f);
                if (armor <= 0.0001f) return;

                BeginProjection(wgo, ArmorParam, armor, ref __state);
            }
            catch (Exception ex)
            {
                Restore(__state);
                __state = null;
                LogRuntimeError("Combat armor input projection failed closed", ex);
            }
        }

        private static Exception DecHpFinalizer(Exception __exception, ScopedParamState __state)
        {
            Restore(__state);
            return __exception;
        }

        private static void BeginProjection(object wgo, string param, float delta, ref ScopedParamState state)
        {
            object runtimeEffect = R.Get(wgo, "totem_effect");
            if (runtimeEffect == null)
                throw new MissingMemberException("WorldGameObject.totem_effect");

            float original = Convert.ToSingle(
                _runtimeEffectGet.Invoke(runtimeEffect, new object[] { param, 0f }));

            state = new ScopedParamState
            {
                RuntimeEffect = runtimeEffect,
                Param = param,
                OriginalValue = original
            };

            _runtimeEffectSet.Invoke(
                runtimeEffect,
                new object[] { param, original + delta });
        }

        private static void Restore(ScopedParamState state)
        {
            if (state == null || state.RuntimeEffect == null || string.IsNullOrEmpty(state.Param)) return;

            try
            {
                _runtimeEffectSet.Invoke(
                    state.RuntimeEffect,
                    new object[] { state.Param, state.OriginalValue });

                if (Math.Abs(state.OriginalValue) <= 0.0001f)
                    _runtimeEffectRemoveZeroValues.Invoke(state.RuntimeEffect, null);
            }
            catch (Exception ex)
            {
                LogRuntimeError("Combat runtime parameter restoration failed", ex);
            }
        }

        private static bool TryGetActiveCombatTier(object wgo, out int tier)
        {
            tier = 0;
            if (wgo == null || !IsPlayer(wgo) || !HasLiveBuff("buff_sword")) return false;

            tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.CombatTierParam);
            return tier >= 1 && tier <= 3;
        }

        private static bool IsPlayer(object wgo)
        {
            object value = R.Get(wgo, "is_player");
            return value != null && Convert.ToBoolean(value);
        }

        private static void ValidateStockCombatBuff()
        {
            object buff = R.BalanceData("buff_sword", "BuffDefinition", true);
            if (buff == null)
                throw new MissingMemberException("BuffDefinition buff_sword");

            object res = R.Get(buff, "res");
            float stockDamage = R.GameResGet(res, DamageParam);
            float stockArmor = R.GameResGet(res, ArmorParam);

            if (Math.Abs(stockDamage - StockCombatDamage) > 0.0001f)
                throw new InvalidOperationException("buff_sword add_damage changed unexpectedly: " + stockDamage);
            if (Math.Abs(stockArmor) > 0.0001f)
                throw new InvalidOperationException("buff_sword add_armor changed unexpectedly: " + stockArmor);
        }

        private static void ResolveBuffApi()
        {
            if (_findBuffById != null) return;
            Type buffsLogics = R.GameType("BuffsLogics");
            _findBuffById = R.Method(buffsLogics, "FindBuffByID", true, new[] { typeof(string) });
            if (_findBuffById == null)
                throw new MissingMethodException("BuffsLogics.FindBuffByID(string)");
        }

        private static bool HasLiveBuff(string buffId)
        {
            return _findBuffById.Invoke(null, new object[] { buffId }) != null;
        }

        private static void LogRuntimeError(string context, Exception ex)
        {
            if (_runtimeErrorLogged) return;
            _runtimeErrorLogged = true;
            _log?.LogError("PrayerClarity: Rebalanced " + context + ". Stock combat behavior remains active for the affected call. " + ex);
        }
    }
}
