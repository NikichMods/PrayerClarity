using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedRepentance
    {
        private const string StockConfessionExpression = "SetPpar(\"confession_probability\", 0.15)";
        private const string ConfessionProbabilityParam = "confession_probability";
        private const string BuffId = "buff_sins";

        private static ManualLogSource _log;
        private static MethodInfo _findBuffById;
        private static MethodInfo _getRawExpression;
        private static bool _runtimeErrorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            ResolveBuffApi();
            ValidateStockReset();

            Type getterType = R.GameType("FlowCanvas.Nodes.Flow_GetPlayerParam");
            if (getterType == null)
                throw new MissingMemberException("FlowCanvas.Nodes.Flow_GetPlayerParam");

            MethodInfo invoke = R.Method(getterType, "Invoke", false, new[] { typeof(string) });
            if (invoke == null)
                throw new MissingMethodException("Flow_GetPlayerParam.Invoke(string)");

            R.Patch(harmonyId + ".repentance.probability", typeof(RebalancedRepentance), invoke, nameof(InvokePostfix));
        }

        private static void InvokePostfix(string param, ref float __result)
        {
            if (!string.Equals(param, ConfessionProbabilityParam, StringComparison.Ordinal)) return;
            if (!RebalancedRuntimeState.IsReady) return;

            try
            {
                if (!HasLiveBuff(BuffId)) return;

                int tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.ConfessionTierParam);
                if (tier < 1 || tier > 3) return;

                RebalancedPrayerRule rule;
                if (!RebalancedRuleSet.TryGet("b_sins", out rule)) return;

                float probability = rule.TierValue(rule.ConfessionProbability, tier, __result);
                if (probability >= 0f && probability <= 1f)
                    __result = probability;
            }
            catch (Exception ex)
            {
                if (_runtimeErrorLogged) return;
                _runtimeErrorLogged = true;
                _log?.LogError("PrayerClarity: Rebalanced Repentance probability projection failed closed; stock confession probability remains active for the affected read. " + ex);
            }
        }

        private static void ValidateStockReset()
        {
            object logic = R.BalanceData("church_budka_roll", "LogicDefinition", true);
            if (logic == null)
                throw new MissingMemberException("LogicDefinition church_budka_roll");

            IList expressions = R.Get(logic, "execute_expressions") as IList;
            if (expressions == null || expressions.Count != 1)
                throw new InvalidOperationException("church_budka_roll execute_expressions shape changed.");

            object expression = expressions[0];
            if (expression == null)
                throw new InvalidOperationException("church_budka_roll reset expression is unavailable.");

            Type smartExpression = R.GameType("SmartExpression");
            _getRawExpression = smartExpression?.GetMethod(
                "GetRawExpressionString",
                R.Inst,
                null,
                Type.EmptyTypes,
                null);
            if (_getRawExpression == null)
                throw new MissingMethodException("SmartExpression.GetRawExpressionString()");

            string raw = _getRawExpression.Invoke(expression, null) as string;
            if (!string.Equals(raw, StockConfessionExpression, StringComparison.Ordinal))
                throw new InvalidOperationException("church_budka_roll stock reset expression changed: " + raw);
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
    }
}
