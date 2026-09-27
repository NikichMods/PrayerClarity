using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class FaultInjection
    {
        internal const string BaseSource = "9a236d217ca335ac944087dcbb23504bb391fe26";
        private const string OptionalFeature = "technology-carousel";
        private static bool _optionalRollbackObserved;
        private static bool _optionalRollbackSucceeded;
        private static bool _optionalOwnerCleanupSucceeded;

        internal static string Scenario
        {
            get { return string.IsNullOrEmpty(FaultScenario.Value) ? "none" : FaultScenario.Value; }
        }

        internal static bool Is(string scenario)
        {
            return string.Equals(Scenario, scenario, StringComparison.Ordinal);
        }

        internal static void Announce(ManualLogSource log)
        {
            log?.LogWarning(
                "PC_FAULT_TEST scenario=" + Scenario +
                " base_source=" + BaseSource +
                " purpose=research-only");
        }

        internal static bool AdjustCompatibility(bool verified)
        {
            return Is("unverified") ? false : verified;
        }

        internal static void AfterOptionalInstall(string feature)
        {
            if (Is("optional-rollback") &&
                string.Equals(feature, OptionalFeature, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "PC_FAULT_INJECT scenario=optional-rollback point=after-technology-carousel-install");
        }

        internal static void AfterCoreCheckpoint()
        {
            if (Is("core-rollback"))
                throw new InvalidOperationException(
                    "PC_FAULT_INJECT scenario=core-rollback point=after-combat-install");
        }

        internal static void AfterCombatAliasProjection()
        {
            if (Is("projection-rollback"))
                throw new InvalidOperationException(
                    "PC_FAULT_INJECT scenario=projection-rollback point=after-combat-alias-projection");
        }

        internal static void OnOptionalRollback(ManualLogSource log, string feature, bool rolledBack)
        {
            if (!Is("optional-rollback") ||
                !string.Equals(feature, OptionalFeature, StringComparison.Ordinal))
                return;

            _optionalRollbackObserved = true;
            _optionalRollbackSucceeded = rolledBack;

            string details;
            _optionalOwnerCleanupSucceeded = NoOwnerPatches(
                PrayerClarityRebalancedPlugin.PluginGuid + ".technology-prayer-carousel",
                out details);

            log?.LogWarning(
                "PC_TEST_ASSERT scenario=optional-rollback rollback=" + Pass(rolledBack) +
                " owner_cleanup=" + Pass(_optionalOwnerCleanupSucceeded) +
                " details=\"" + details + "\"");
        }

        internal static void OnCoreRollback(ManualLogSource log, bool rolledBack)
        {
            if (!Is("core-rollback")) return;

            string details;
            bool ownerCleanup = NoOwnerPatches(
                PrayerClarityRebalancedPlugin.PluginGuid,
                out details);

            bool semanticsReset = typeof(PrayerEditionSemantics)
                .GetFields(R.Stat)
                .Where(field => typeof(Delegate).IsAssignableFrom(field.FieldType))
                .All(field => field.GetValue(null) == null);

            log?.LogError(
                "PC_TEST_ASSERT scenario=core-rollback rollback=" + Pass(rolledBack) +
                " owner_cleanup=" + Pass(ownerCleanup) +
                " semantics_reset=" + Pass(semanticsReset) +
                " details=\"" + details + "\"");
        }

        internal static void OnReady(ManualLogSource log, GameCompatibilityStatus compatibility)
        {
            if (Is("unverified"))
            {
                log?.LogWarning(
                    "PC_TEST_ASSERT scenario=unverified forced_unverified=" +
                    Pass(compatibility != null && !compatibility.Verified) +
                    " continued_to_ready=PASS");
            }

            if (Is("optional-rollback"))
            {
                log?.LogWarning(
                    "PC_TEST_ASSERT scenario=optional-rollback observed=" + Pass(_optionalRollbackObserved) +
                    " rollback=" + Pass(_optionalRollbackSucceeded) +
                    " owner_cleanup=" + Pass(_optionalOwnerCleanupSucceeded) +
                    " continued_to_ready=PASS");
            }
        }

        internal static void OnProjectionReady(ManualLogSource log)
        {
            if (Is("unverified"))
                log?.LogWarning(
                    "PC_TEST_ASSERT scenario=unverified projection_ready=" +
                    Pass(RebalancedRuntimeState.IsReady));

            if (Is("optional-rollback"))
                log?.LogWarning(
                    "PC_TEST_ASSERT scenario=optional-rollback projection_after_optional_failure=" +
                    Pass(RebalancedRuntimeState.IsReady));
        }

        internal static void OnProjectionRollback(
            ManualLogSource log,
            bool rolledBack,
            bool snapshotMatches,
            string mismatch)
        {
            if (!Is("projection-rollback")) return;

            log?.LogError(
                "PC_TEST_ASSERT scenario=projection-rollback rollback=" + Pass(rolledBack) +
                " snapshot_restore=" + Pass(snapshotMatches) +
                " runtime_disabled=" + Pass(RebalancedRuntimeState.IsDisabled) +
                " mismatch=\"" + (mismatch ?? "none") + "\"");
        }

        private static string Pass(bool value)
        {
            return value ? "PASS" : "FAIL";
        }

        private static bool NoOwnerPatches(string ownerPrefix, out string details)
        {
            try
            {
                Type harmonyType = R.AnyType("HarmonyLib.Harmony");
                if (harmonyType == null)
                {
                    details = "Harmony type unavailable";
                    return false;
                }

                MethodInfo getAll = harmonyType.GetMethods(R.Stat)
                    .FirstOrDefault(method =>
                        method.Name == "GetAllPatchedMethods" &&
                        method.GetParameters().Length == 0);
                MethodInfo getInfo = harmonyType.GetMethods(R.Stat)
                    .FirstOrDefault(method =>
                        method.Name == "GetPatchInfo" &&
                        method.GetParameters().Length == 1 &&
                        typeof(MethodBase).IsAssignableFrom(method.GetParameters()[0].ParameterType));
                if (getAll == null || getInfo == null)
                {
                    details = "Harmony patch inspection API unavailable";
                    return false;
                }

                IEnumerable targets = getAll.Invoke(null, null) as IEnumerable;
                if (targets == null)
                {
                    details = "Harmony target enumeration unavailable";
                    return false;
                }

                foreach (object targetObject in targets)
                {
                    MethodBase target = targetObject as MethodBase;
                    if (target == null) continue;

                    object patches = getInfo.Invoke(null, new object[] { target });
                    if (patches == null) continue;

                    IEnumerable owners = null;
                    PropertyInfo ownersProperty = patches.GetType().GetProperty("Owners", R.Inst);
                    if (ownersProperty != null)
                        owners = ownersProperty.GetValue(patches, null) as IEnumerable;
                    if (owners == null)
                    {
                        FieldInfo ownersField = patches.GetType().GetField("Owners", R.Inst);
                        if (ownersField != null)
                            owners = ownersField.GetValue(patches) as IEnumerable;
                    }
                    if (owners == null) continue;

                    foreach (object ownerObject in owners)
                    {
                        string owner = Convert.ToString(ownerObject);
                        if (!string.IsNullOrEmpty(owner) &&
                            owner.StartsWith(ownerPrefix, StringComparison.Ordinal))
                        {
                            details = "remaining_owner=" + owner +
                                " target=" + target.DeclaringType?.FullName + "." + target.Name;
                            return false;
                        }
                    }
                }

                details = "no_matching_owner_patches";
                return true;
            }
            catch (Exception ex)
            {
                details = "inspection_error=" + ex.GetType().Name + ":" + ex.Message;
                return false;
            }
        }
    }
}
