using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedStaticProjection
    {
        private sealed class MemberSnapshot
        {
            internal readonly object Target;
            internal readonly MemberInfo Member;
            internal readonly object Value;

            internal MemberSnapshot(object target, MemberInfo member, object value)
            {
                Target = target;
                Member = member;
                Value = value;
            }

            internal bool MatchesCurrent(out string mismatch)
            {
                object current;
                FieldInfo currentField = Member as FieldInfo;
                if (currentField != null)
                    current = currentField.GetValue(Target);
                else
                {
                    PropertyInfo currentProperty = Member as PropertyInfo;
                    if (currentProperty == null)
                    {
                        mismatch = "unsupported-member=" + Member.Name;
                        return false;
                    }
                    current = currentProperty.GetValue(Target, null);
                }

                if (object.Equals(current, Value))
                {
                    mismatch = null;
                    return true;
                }

                mismatch = "member=" + Target.GetType().FullName + "." + Member.Name;
                return false;
            }

            internal void Restore()
            {
                FieldInfo field = Member as FieldInfo;
                if (field != null)
                {
                    field.SetValue(Target, Value);
                    return;
                }

                PropertyInfo property = Member as PropertyInfo;
                if (property != null)
                {
                    property.SetValue(Target, Value, null);
                    return;
                }

                throw new NotSupportedException("Unsupported projection snapshot member: " + Member.MemberType);
            }
        }

        private sealed class ListSnapshot
        {
            internal readonly IList Target;
            internal readonly object[] Items;

            internal ListSnapshot(IList target)
            {
                Target = target;
                Items = new object[target.Count];
                target.CopyTo(Items, 0);
            }

            internal bool MatchesCurrent(out string mismatch)
            {
                if (Target.Count != Items.Length)
                {
                    mismatch = "list-count expected=" + Items.Length + " actual=" + Target.Count;
                    return false;
                }

                for (int i = 0; i < Items.Length; i++)
                {
                    if (!object.Equals(Target[i], Items[i]))
                    {
                        mismatch = "list-item index=" + i;
                        return false;
                    }
                }

                mismatch = null;
                return true;
            }

            internal void Restore()
            {
                Target.Clear();
                foreach (object item in Items)
                    Target.Add(item);
            }
        }

        private sealed class ProjectionSnapshot
        {
            private readonly List<MemberSnapshot> _members = new List<MemberSnapshot>();
            private readonly List<ListSnapshot> _lists = new List<ListSnapshot>();

            internal static ProjectionSnapshot Capture()
            {
                ProjectionSnapshot snapshot = new ProjectionSnapshot();
                snapshot.CaptureAll();
                return snapshot;
            }

            internal bool MatchesCurrent(out string mismatch)
            {
                foreach (ListSnapshot list in _lists)
                    if (!list.MatchesCurrent(out mismatch))
                        return false;

                foreach (MemberSnapshot member in _members)
                    if (!member.MatchesCurrent(out mismatch))
                        return false;

                mismatch = null;
                return true;
            }

            internal bool Restore(out Exception failure)
            {
                failure = null;

                for (int i = _lists.Count - 1; i >= 0; i--)
                {
                    try { _lists[i].Restore(); }
                    catch (Exception ex) { if (failure == null) failure = ex; }
                }

                for (int i = _members.Count - 1; i >= 0; i--)
                {
                    try { _members[i].Restore(); }
                    catch (Exception ex) { if (failure == null) failure = ex; }
                }

                return failure == null;
            }

            private void CaptureAll()
            {
                object combatBuff = R.BalanceData("buff_sword", "BuffDefinition", true);
                if (combatBuff == null) throw new MissingMemberException("BuffDefinition buff_sword");
                CaptureMember(combatBuff, "tick_period");
                CaptureMember(combatBuff, "se_tick");

                for (int tier = 1; tier <= 3; tier++)
                {
                    string craftId = RebalancedRuleSet.CraftId("b_shield", tier);
                    object craft = R.BalanceData(craftId, "CraftDefinition", true);
                    if (craft == null)
                        throw new MissingMemberException("Missing legacy Combat alias prayer craft " + craftId);
                    CaptureMember(craft, "buff");
                }

                object tech = R.BalanceData("Martial skills", "TechDefinition", true);
                if (tech == null) throw new MissingMemberException("TechDefinition Martial skills");
                CaptureRequiredList(tech, "crafts");
                CaptureOptionalList(tech, "_unlocks_list");

                CaptureRequiredMember(R.BalanceData("b_shield", "CraftDefinition", true), "b_shield", "hidden");
                CaptureRequiredMember(R.BalanceData("b_shield_2", "CraftDefinition", true), "b_shield_2", "hidden");

                Type itemType = R.GameType("Item");
                ConstructorInfo itemConstructor = itemType?.GetConstructor(new[] { typeof(string), typeof(int) });
                if (itemConstructor == null) throw new MissingMethodException("Item(string,int)");

                foreach (RebalancedPrayerRule rule in RebalancedRuleSet.All)
                {
                    if (!HasStaticProjection(rule)) continue;

                    for (int tier = 1; tier <= 3; tier++)
                    {
                        string craftId = RebalancedRuleSet.CraftId(rule.PrayerId, tier);
                        object craft = R.BalanceData(craftId, "CraftDefinition", true);
                        if (craft == null)
                        {
                            if (rule.OptionalDlc) continue;
                            throw new MissingMemberException("Missing required prayer craft " + craftId);
                        }

                        if (rule.Requirements != null) CaptureMember(craft, "needs_quality");
                        if (rule.FaithBonusRates != null) CaptureMember(craft, "k_faith");
                        if (rule.MoneyBonusRates != null) CaptureMember(craft, "k_money");
                        if (rule.DurationMinutes != null) CaptureMember(craft, "dur_parameter");
                        if (rule.LinkedPrayEventIds != null) CaptureMember(craft, "linked_sub_id");

                        bool changesOutput =
                            rule.RemoveFixedFaith ||
                            rule.RemoveFixedMoney ||
                            rule.FixedFaithBonuses != null ||
                            rule.FixedMoneyBonusesCents != null ||
                            !string.IsNullOrEmpty(rule.SuccessRewardBaseItemId);
                        if (changesOutput)
                            CaptureRequiredList(craft, "output");
                    }
                }
            }

            private void CaptureRequiredMember(object target, string targetName, string memberName)
            {
                if (target == null) throw new MissingMemberException("CraftDefinition " + targetName);
                CaptureMember(target, memberName);
            }

            private void CaptureMember(object target, string memberName)
            {
                if (target == null) throw new ArgumentNullException(nameof(target));
                MemberInfo member = FindWritableMember(target.GetType(), memberName);
                if (member == null) throw new MissingMemberException(target.GetType().FullName, memberName);
                _members.Add(new MemberSnapshot(target, member, ReadMember(target, member)));
            }

            private void CaptureRequiredList(object target, string memberName)
            {
                IList list = R.Get(target, memberName) as IList;
                if (list == null) throw new MissingMemberException(target.GetType().FullName, memberName);
                _lists.Add(new ListSnapshot(list));
            }

            private void CaptureOptionalList(object target, string memberName)
            {
                IList list = R.Get(target, memberName) as IList;
                if (list != null) _lists.Add(new ListSnapshot(list));
            }

            private static MemberInfo FindWritableMember(Type type, string name)
            {
                for (Type current = type; current != null; current = current.BaseType)
                {
                    FieldInfo field = current.GetField(name, R.Inst);
                    if (field != null) return field;

                    PropertyInfo property = current.GetProperty(name, R.Inst);
                    if (property != null && property.CanRead && property.CanWrite) return property;
                }
                return null;
            }

            private static object ReadMember(object target, MemberInfo member)
            {
                FieldInfo field = member as FieldInfo;
                if (field != null) return field.GetValue(target);

                PropertyInfo property = member as PropertyInfo;
                if (property != null) return property.GetValue(target, null);

                throw new NotSupportedException("Unsupported projection snapshot member: " + member.MemberType);
            }
        }

        private static ManualLogSource _log;
        private static bool _projectedForCurrentLoad;
        private static bool _projectionFailed;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            Type craftComponent = R.GameType("CraftComponent");
            if (craftComponent == null) throw new MissingMemberException("CraftComponent");

            MethodInfo clear = FindMethod(craftComponent, "ClearCraftsListOnGameStart", 0);
            MethodInfo fill = FindMethod(craftComponent, "FillCraftsList", 0);
            if (clear == null) throw new MissingMethodException("CraftComponent.ClearCraftsListOnGameStart()");
            if (fill == null) throw new MissingMethodException("CraftComponent.FillCraftsList()");

            R.Patch(harmonyId + ".projection.reset", typeof(RebalancedStaticProjection), clear, nameof(ClearCraftsListOnGameStartPostfix));
            R.Patch(harmonyId + ".projection.apply", typeof(RebalancedStaticProjection), fill, nameof(FillCraftsListPostfix));
        }

        private static void ClearCraftsListOnGameStartPostfix()
        {
            _projectedForCurrentLoad = false;
            _projectionFailed = false;
            RebalancedRuntimeState.MarkPending();
        }

        private static void FillCraftsListPostfix()
        {
            if (_projectedForCurrentLoad || _projectionFailed) return;

            ProjectionSnapshot snapshot = null;
            try
            {
                RebalancedRoots.ValidateDefinitions();
                snapshot = ProjectionSnapshot.Capture();
                ApplyOnce();

                _projectedForCurrentLoad = true;
                RebalancedRuntimeState.MarkReady();
                _log?.LogDebug("PC_STATIC_PROJECTION_READY edition=rebalanced");
                FaultInjection.OnProjectionReady(_log);
            }
            catch (Exception ex)
            {
                Exception rollbackFailure = null;
                bool rolledBack = snapshot == null || snapshot.Restore(out rollbackFailure);

                _projectionFailed = true;
                RebalancedRuntimeState.Disable();

                string mismatch = null;
                bool snapshotMatches = snapshot != null && snapshot.MatchesCurrent(out mismatch);
                FaultInjection.OnProjectionRollback(
                    _log,
                    rolledBack,
                    snapshotMatches,
                    mismatch);

                _log?.LogError(
                    "PC_STATIC_PROJECTION_FAILED edition=rebalanced rollback=" +
                    (rolledBack ? "success" : "failed") +
                    " runtime=disabled " + ex);

                if (rollbackFailure != null)
                    _log?.LogError("PC_ROLLBACK_FAILED edition=rebalanced phase=static-projection " + rollbackFailure);
            }
        }

        private static void ApplyOnce()
        {
            RebalancedExpressionProjection.Apply();
            ApplyCombatAliasProjection();
            FaultInjection.AfterCombatAliasProjection();
            RetireProtectionCrafting();

            foreach (RebalancedPrayerRule rule in RebalancedRuleSet.All)
            {
                if (!HasStaticProjection(rule)) continue;

                for (int tier = 1; tier <= 3; tier++)
                {
                    string craftId = RebalancedRuleSet.CraftId(rule.PrayerId, tier);
                    object craft = R.BalanceData(craftId, "CraftDefinition", true);
                    if (craft == null)
                    {
                        if (rule.OptionalDlc) continue;
                        throw new MissingMemberException("Missing required prayer craft " + craftId);
                    }

                    ApplyStockOwnedFields(craft, rule, tier);
                }
            }
        }

        private static void ApplyCombatAliasProjection()
        {
            for (int tier = 1; tier <= 3; tier++)
            {
                string craftId = RebalancedRuleSet.CraftId("b_shield", tier);
                object craft = R.BalanceData(craftId, "CraftDefinition", true);
                if (craft == null) throw new MissingMemberException("Missing legacy Combat alias prayer craft " + craftId);

                string currentBuff = Convert.ToString(R.Get(craft, "buff"));
                if (!string.Equals(currentBuff, "buff_shield", StringComparison.Ordinal) &&
                    !string.Equals(currentBuff, "buff_sword", StringComparison.Ordinal))
                    throw new InvalidOperationException(craftId + " buff target changed unexpectedly: " + currentBuff);

                R.Set(craft, "buff", "buff_sword");
            }
        }

        private static void RetireProtectionCrafting()
        {
            object tech = R.BalanceData("Martial skills", "TechDefinition", true);
            if (tech == null) throw new MissingMemberException("TechDefinition Martial skills");

            IList crafts = R.Get(tech, "crafts") as IList;
            if (crafts == null) throw new MissingMemberException("Martial skills", "crafts");
            RemoveStringEntries(crafts, "b_shield", "@b_shield_2");

            IList unlocks = R.Get(tech, "_unlocks_list") as IList;
            if (unlocks != null)
            {
                for (int i = unlocks.Count - 1; i >= 0; i--)
                {
                    string id = Convert.ToString(R.Get(unlocks[i], "id"));
                    if (string.Equals(id, "b_shield", StringComparison.Ordinal) ||
                        string.Equals(id, "b_shield_2", StringComparison.Ordinal))
                        unlocks.RemoveAt(i);
                }
            }

            HideCraftDefinition("b_shield");
            HideCraftDefinition("b_shield_2");
        }

        private static void RemoveStringEntries(IList list, params string[] ids)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                string value = Convert.ToString(list[i]);
                if (ids.Any(id => string.Equals(value, id, StringComparison.Ordinal)))
                    list.RemoveAt(i);
            }
        }

        private static void HideCraftDefinition(string id)
        {
            object craft = R.BalanceData(id, "CraftDefinition", true);
            if (craft == null) throw new MissingMemberException("CraftDefinition " + id);
            object hidden = R.Get(craft, "hidden");
            if (hidden == null) throw new MissingMemberException(id, "hidden");
            R.Set(craft, "hidden", true);
        }

        private static bool HasStaticProjection(RebalancedPrayerRule rule)
        {
            return rule.Requirements != null ||
                   rule.FaithBonusRates != null ||
                   rule.MoneyBonusRates != null ||
                   rule.RemoveFixedFaith ||
                   rule.RemoveFixedMoney ||
                   rule.FixedFaithBonuses != null ||
                   rule.FixedMoneyBonusesCents != null ||
                   rule.DurationMinutes != null ||
                   rule.LinkedPrayEventIds != null ||
                   !string.IsNullOrEmpty(rule.SuccessRewardBaseItemId);
        }

        private static void ApplyStockOwnedFields(object craft, RebalancedPrayerRule rule, int tier)
        {
            if (rule.Requirements != null)
                R.Set(craft, "needs_quality", rule.TierValue(rule.Requirements, tier));
            if (rule.FaithBonusRates != null)
                R.Set(craft, "k_faith", rule.TierValue(rule.FaithBonusRates, tier));
            if (rule.MoneyBonusRates != null)
                R.Set(craft, "k_money", rule.TierValue(rule.MoneyBonusRates, tier));
            if (rule.DurationMinutes != null)
                R.Set(craft, "dur_parameter", rule.TierValue(rule.DurationMinutes, tier));
            if (rule.LinkedPrayEventIds != null)
                R.Set(craft, "linked_sub_id", rule.TierValue(rule.LinkedPrayEventIds, tier));

            bool replaceFaith = rule.RemoveFixedFaith || rule.FixedFaithBonuses != null;
            bool replaceMoney = rule.RemoveFixedMoney || rule.FixedMoneyBonusesCents != null;
            if (replaceFaith || replaceMoney)
                RemovePrayerOwnedFixedOutputs(craft, replaceFaith, replaceMoney);

            if (rule.FixedFaithBonuses != null)
                AddPrayerOwnedFixedOutput(craft, "faith", rule.TierValue(rule.FixedFaithBonuses, tier, 0));
            if (rule.FixedMoneyBonusesCents != null)
                AddPrayerOwnedFixedOutput(craft, "money", rule.TierValue(rule.FixedMoneyBonusesCents, tier, 0));

            ApplySuccessReward(craft, rule, tier);
        }

        private static void RemovePrayerOwnedFixedOutputs(object craft, bool removeFaith, bool removeMoney)
        {
            IList output = RequireOutput(craft);
            for (int i = output.Count - 1; i >= 0; i--)
            {
                object item = output[i];
                string id = R.Id(item) ?? string.Empty;
                if ((removeFaith && string.Equals(id, "faith", StringComparison.Ordinal)) ||
                    (removeMoney && string.Equals(id, "money", StringComparison.Ordinal)))
                    output.RemoveAt(i);
            }
        }

        private static void AddPrayerOwnedFixedOutput(object craft, string itemId, int count)
        {
            if (count <= 0) return;

            Type itemType = R.GameType("Item");
            ConstructorInfo ctor = itemType?.GetConstructor(new[] { typeof(string), typeof(int) });
            if (ctor == null) throw new MissingMethodException("Item(string,int)");

            RequireOutput(craft).Add(ctor.Invoke(new object[] { itemId, count }));
        }

        private static void ApplySuccessReward(object craft, RebalancedPrayerRule rule, int tier)
        {
            if (string.IsNullOrEmpty(rule.SuccessRewardBaseItemId)) return;

            int rewardTier = rule.TierValue(rule.SuccessRewardQualityTiers, tier, 0);
            int count = rule.TierValue(rule.SuccessRewardCounts, tier, 0);
            IList output = RequireOutput(craft);

            for (int quality = 1; quality <= 3; quality++)
            {
                string ownedId = rule.SuccessRewardBaseItemId + ":" + quality;
                for (int i = output.Count - 1; i >= 0; i--)
                    if (string.Equals(R.Id(output[i]), ownedId, StringComparison.Ordinal))
                        output.RemoveAt(i);
            }

            if (rewardTier <= 0 || count <= 0) return;

            string rewardId = rule.SuccessRewardBaseItemId + ":" + rewardTier;
            Type itemType = R.GameType("Item");
            ConstructorInfo ctor = itemType?.GetConstructor(new[] { typeof(string), typeof(int) });
            if (ctor == null) throw new MissingMethodException("Item(string,int)");
            output.Add(ctor.Invoke(new object[] { rewardId, count }));
        }

        private static IList RequireOutput(object craft)
        {
            IList output = R.Get(craft, "output") as IList;
            if (output == null) throw new MissingMemberException(R.Id(craft) ?? "CraftDefinition", "output");
            return output;
        }

        private static MethodInfo FindMethod(Type type, string name, int parameterCount)
        {
            if (type == null) return null;
            return type.GetMethods(R.Inst | R.Stat)
                .FirstOrDefault(method => method.Name == name && method.GetParameters().Length == parameterCount);
        }
    }
}
