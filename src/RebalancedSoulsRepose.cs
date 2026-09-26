using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedSoulsRepose
    {
        private sealed class ScopedConversionState
        {
            internal IList Output;
            internal object AddedFaithItem;
            internal int Conversion;
        }

        private static ManualLogSource _log;
        private static Type _prayLogicsType;
        private static ConstructorInfo _itemConstructor;
        private static bool _runtimeErrorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            _prayLogicsType = R.GameType("PrayLogics");
            Type itemType = R.GameType("Item");
            if (_prayLogicsType == null) throw new MissingMemberException("PrayLogics");
            if (itemType == null) throw new MissingMemberException("Item");

            MethodInfo calculate = R.Method(
                _prayLogicsType,
                "CalculatePray",
                true,
                new[] { typeof(string) });
            if (calculate == null)
                throw new MissingMethodException("PrayLogics.CalculatePray(string)");

            _itemConstructor = itemType.GetConstructor(new[] { typeof(string), typeof(int) });
            if (_itemConstructor == null) throw new MissingMethodException("Item(string,int)");

            R.PatchHooks(
                harmonyId + ".soulsrepose.convert",
                typeof(RebalancedSoulsRepose),
                calculate,
                nameof(CalculatePrayPrefix),
                nameof(CalculatePrayPostfix),
                nameof(CalculatePrayFinalizer));
        }

        internal static int GetConversionAmount(RebalancedPrayerRule rule, int tier)
        {
            if (rule == null || rule.SoulGratitudeFaithCaps == null) return 0;
            return GetConversionAmountForCap(rule.TierValue(rule.SoulGratitudeFaithCaps, tier, 0));
        }

        internal static int GetConversionAmountForCap(int cap)
        {
            cap = Math.Max(0, cap);
            if (cap <= 0) return 0;

            int available = (int)Math.Floor(Math.Max(0f, R.PlayerParam("gratitude_points")));
            return Math.Min(available, cap);
        }

        private static void CalculatePrayPrefix(ref string __0, ref ScopedConversionState __state)
        {
            __state = null;
            if (!RebalancedRuntimeState.IsReady) return;
            try
            {
                object craft = GetSelectedPrayerCraft();
                RebalancedPrayerRule rule;
                int tier;
                string craftId = R.Id(craft);
                if (!RebalancedRuleSet.TryParseCraftId(craftId, out rule, out tier)) return;
                if (!string.Equals(rule.PrayerId, "b_souls", StringComparison.Ordinal)) return;

                string effectiveEventId;
                if (!RebalancedRuleSet.TryGetEffectivePrayEventId(craftId, out effectiveEventId))
                    throw new InvalidOperationException("Soul's Repose effective sermon event is unavailable for " + craftId + ".");

                // The pulpit may hold a runtime PrayCraft copy created before the
                // once-per-load CraftDefinition projection. Project only this call's
                // event argument so stock PrayLogics still owns the actual calculation.
                __0 = effectiveEventId;

                int conversion = GetConversionAmount(rule, tier);
                __state = new ScopedConversionState { Conversion = conversion };
                if (conversion <= 0) return;

                IList output = R.Get(craft, "output") as IList;
                if (output == null) throw new MissingMemberException(R.Id(craft) ?? "CraftDefinition", "output");

                object faithItem = _itemConstructor.Invoke(new object[] { "faith", conversion });
                output.Add(faithItem);
                __state.Output = output;
                __state.AddedFaithItem = faithItem;
            }
            catch (Exception ex)
            {
                Restore(__state);
                __state = null;
                LogFailure("PrayerClarity: Rebalanced Soul's Repose conversion projection failed; stock sermon calculation continues. ", ex);
            }
        }

        private static void CalculatePrayPostfix(ScopedConversionState __state)
        {
            if (__state == null || __state.Conversion <= 0) return;

            try
            {
                object prayResult = R.GetStatic(_prayLogicsType, "last_pray_result");
                if (prayResult == null || !Convert.ToBoolean(R.Get(prayResult, "success"))) return;

                object mainGame = R.GetStatic(R.GameType("MainGame"), "me");
                object player = mainGame == null ? null : R.Get(mainGame, "player");
                if (player == null) throw new InvalidOperationException("MainGame.player unavailable after successful sermon.");

                float current = R.Float(R.Get(player, "gratitude_points"));
                R.Set(player, "gratitude_points", Math.Max(0f, current - __state.Conversion));
            }
            catch (Exception ex)
            {
                LogFailure("PrayerClarity: Rebalanced Soul's Repose could not spend Soul Gratitude after success. ", ex);
            }
        }

        private static Exception CalculatePrayFinalizer(Exception __exception, ScopedConversionState __state)
        {
            Restore(__state);
            return __exception;
        }

        private static object GetSelectedPrayerCraft()
        {
            object gui = R.GetStatic(R.GameType("GUIElements"), "me");
            object prayGui = gui == null ? null : R.Get(gui, "pray_craft");
            return prayGui == null ? null : R.Get(prayGui, "pray_craft");
        }

        private static void Restore(ScopedConversionState state)
        {
            if (state == null || state.Output == null || state.AddedFaithItem == null) return;
            try
            {
                for (int i = state.Output.Count - 1; i >= 0; i--)
                {
                    if (!ReferenceEquals(state.Output[i], state.AddedFaithItem)) continue;
                    state.Output.RemoveAt(i);
                    break;
                }
            }
            catch (Exception ex)
            {
                LogFailure("PrayerClarity: Rebalanced Soul's Repose temporary Faith output restoration failed. ", ex);
            }
        }

        private static void LogFailure(string message, Exception ex)
        {
            if (_runtimeErrorLogged) return;
            _runtimeErrorLogged = true;
            _log?.LogError(message + ex);
        }
    }
}
