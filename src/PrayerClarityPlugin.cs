using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace PrayerClarity
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInIncompatibility(RebalancedPluginGuid)]
    public sealed class PrayerClarityPlugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "nikich.graveyardkeeper.prayerclarity";
        internal const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        internal const string PluginName = "PrayerClarity";
        internal const string PluginVersion = BuildVersion.Value;
        private static ManualLogSource _log;
        private static bool _runtimeErrorLogged;

        private void Awake()
        {
            _log = Logger;
            R.PatchTransaction patches = null;
            GameCompatibilityStatus compatibility = null;

            try
            {
                if (!R.BindGameAssembly())
                {
                    Logger.LogError("PC_HOST_MISSING edition=vanilla assembly=Assembly-CSharp action=disabled");
                    return;
                }

                compatibility = GameCompatibility.Inspect(R.GameAssembly);
                Logger.LogInfo(
                    "PC_START edition=vanilla version=" + PluginVersion +
                    " target=\"" + GameCompatibility.VerifiedTarget + "\"" +
                    " game_mvid=" + compatibility.GameMvid +
                    " compatibility=" + compatibility.Mode);

                if (!compatibility.Verified)
                {
                    Logger.LogWarning(
                        "PC_COMPAT_UNVERIFIED edition=vanilla game_mvid=" + compatibility.GameMvid +
                        " action=best-effort required_contracts=exact");
                }

                Localization.Initialize(Assembly.GetExecutingAssembly(), Logger);
                patches = R.BeginPatchTransaction();

                Type prayGui = R.GameType("PrayCraftGUI");
                MethodInfo redraw = R.Method(prayGui, "RedrawTextValues", false, new[] { typeof(float), typeof(float) });
                R.Patch(PluginGuid, typeof(PrayerClarityPlugin), redraw, nameof(RedrawTextValuesPostfix));

                InstallOptional(patches, "technology-viewport",
                    () => TechnologyTooltipViewportClamp.Install(PluginGuid, Logger));
                InstallOptional(patches, "technology-content-width",
                    () => TechnologyTooltipContentWidth.Install(PluginGuid, Logger));
                InstallOptional(patches, "secondary-surfaces",
                    () => SecondarySurfacePresentation.Install(PluginGuid, Logger));
                InstallOptional(patches, "technology-carousel",
                    () => TechnologyPrayerCarousel.Install(PluginGuid, Logger));
                InstallOptional(patches, "prayer-item-tooltip",
                    () => ItemTooltipPresentation.Install(PluginGuid, Logger));
                InstallOptional(patches, "prayer-lore-fallback",
                    () => PrayerLorePresentation.Install(PluginGuid, Logger));

                patches.Commit();
                Logger.LogInfo(
                    "PC_READY edition=vanilla version=" + PluginVersion +
                    " compatibility=" + compatibility.Mode);
            }
            catch (Exception ex)
            {
                Exception rollbackFailure = null;
                bool rolledBack = patches == null || patches.RollbackAll(out rollbackFailure);
                Logger.LogError(
                    "PC_INIT_FAILED edition=vanilla compatibility=" +
                    (compatibility == null ? "unknown" : compatibility.Mode) +
                    " rollback=" + (rolledBack ? "success" : "failed") + " " + ex);

                if (rollbackFailure != null)
                    Logger.LogError("PC_ROLLBACK_FAILED edition=vanilla " + rollbackFailure);
            }
        }

        private void InstallOptional(R.PatchTransaction patches, string feature, Action install)
        {
            int savepoint = patches.Savepoint;
            try
            {
                install();
            }
            catch (Exception ex)
            {
                Exception rollbackFailure;
                bool rolledBack = patches.RollbackTo(savepoint, out rollbackFailure);
                if (rolledBack)
                {
                    Logger.LogWarning(
                        "PC_FEATURE_DISABLED edition=vanilla feature=" + feature +
                        " rollback=success " + ex);
                }
                else
                {
                    Logger.LogError(
                        "PC_FEATURE_DISABLED edition=vanilla feature=" + feature +
                        " rollback=failed " + ex);
                    Logger.LogError(
                        "PC_ROLLBACK_FAILED edition=vanilla feature=" + feature + " " +
                        rollbackFailure);
                    throw new InvalidOperationException(
                        "Optional feature rollback failed: " + feature,
                        rollbackFailure);
                }
            }
        }

        private static void RedrawTextValuesPostfix(object __instance, float chance)
        {
            object label = R.Get(__instance, "l_total_values");
            if (label == null) return;

            try
            {
                PrayerForecast.Result forecast = PrayerForecast.Build(__instance, chance);
                if (forecast == null)
                {
                    PulpitPolish.Restore();
                    PulpitLayoutV4.Restore();
                    PulpitPresentation.Hide(label, __instance);
                    return;
                }

                string vanilla = R.Get(label, "text") as string ?? string.Empty;
                PulpitPresentation.Render(label, __instance, KeepVanillaContext(vanilla), forecast);
                PulpitLayoutV4.Apply(label, __instance, forecast);
                PulpitPolish.Apply(label, __instance, forecast);
            }
            catch (Exception ex)
            {
                PulpitPolish.Restore();
                PulpitLayoutV4.Restore();
                PulpitPresentation.Hide(label, __instance);
                if (_runtimeErrorLogged) return;
                _runtimeErrorLogged = true;
                _log?.LogError("PC_RUNTIME_FALLBACK edition=vanilla feature=pulpit action=vanilla-ui " + ex);
            }
        }

        private static string KeepVanillaContext(string vanilla)
        {
            string normalized = (vanilla ?? string.Empty).Replace("\r\n", "\n");
            string[] lines = normalized.Split(new[] { '\n' }, StringSplitOptions.None);
            if (lines.Length < 3) return vanilla ?? string.Empty;
            return lines[0] + "\n" + lines[1];
        }
    }
}
