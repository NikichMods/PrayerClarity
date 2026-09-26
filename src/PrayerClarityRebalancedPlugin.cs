using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace PrayerClarity
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class PrayerClarityRebalancedPlugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        internal const string PluginName = "PrayerClarity: Rebalanced";
        internal const string PluginVersion = "0.2.47";
        private static readonly Guid SupportedGameMvid = new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");
        private static ManualLogSource _log;
        private static bool _runtimeErrorLogged;

        private void Awake()
        {
            _log = Logger;
            try
            {
                if (!R.BindGameAssembly())
                {
                    Logger.LogError("Assembly-CSharp is unavailable; PrayerClarity: Rebalanced is disabled.");
                    return;
                }

                Guid actualMvid = R.GameAssembly.ManifestModule.ModuleVersionId;
                if (actualMvid != SupportedGameMvid)
                {
                    Logger.LogWarning("Unsupported Graveyard Keeper build (Assembly-CSharp MVID " + actualMvid + "). PrayerClarity: Rebalanced is disabled rather than patching an unverified build.");
                    return;
                }

                RebalancedRuleSet.Validate();
                Localization.Initialize(Assembly.GetExecutingAssembly(), Logger);
                RebalancedPresentationSemantics.Install();

                InstallClarityPresentation();
                RebalancedStaticProjection.Install(PluginGuid, Logger);
                RebalancedTierState.Install(PluginGuid, Logger);
                RebalancedRoots.Install(PluginGuid, Logger);
                RebalancedRepentance.Install(PluginGuid, Logger);
                RebalancedRepose.Install(PluginGuid, Logger);
                RebalancedCombat.Install(PluginGuid, Logger);
                RebalancedExcellence.Install(PluginGuid, Logger);
                RebalancedSoulsRepose.Install(PluginGuid, Logger);
                RebalancedThoroughCleansing.Install(PluginGuid, Logger);
                RebalancedSoulContentment.Install(PluginGuid, Logger);

                Logger.LogInfo(PluginName + " " + PluginVersion + " loaded. Includes the PrayerClarity: Vanilla presentation layer plus the Rebalanced mechanics foundation.");
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity: Rebalanced initialization failed: " + ex);
            }
        }

        private void InstallClarityPresentation()
        {
            Type prayGui = R.GameType("PrayCraftGUI");
            MethodInfo redraw = R.Method(prayGui, "RedrawTextValues", false, new[] { typeof(float), typeof(float) });
            R.Patch(PluginGuid, typeof(PrayerClarityRebalancedPlugin), redraw, nameof(RedrawTextValuesPostfix));

            try
            {
                TechnologyTooltipViewportClamp.Install(PluginGuid, Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity Technology tooltip viewport safety is disabled; other Clarity surfaces remain active. " + ex);
            }

            try
            {
                TechnologyTooltipContentWidth.Install(PluginGuid, Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity Technology tooltip content-width sizing is disabled; Technology text remains available at vanilla/prefab width. " + ex);
            }

            try
            {
                SecondarySurfacePresentation.Install(PluginGuid, Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity secondary Clarity surfaces are disabled; pulpit Clarity remains active. " + ex);
            }

            try
            {
                RebalancedBssPresentationPolish.Install(PluginGuid, Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity Rebalanced BSS presentation polish is disabled; other Rebalanced behavior remains active. " + ex);
            }

            try
            {
                TechnologyPrayerCarousel.Install(PluginGuid, Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity BSS Technology prayer navigation is disabled; other Rebalanced behavior remains active. " + ex);
            }

            try
            {
                ItemTooltipPresentation.Install(PluginGuid, Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity prayer-item tooltip surface is disabled; other Clarity surfaces remain active. " + ex);
            }

            try
            {
                PrayerLorePresentation.Install(PluginGuid, Logger);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity Excellence lore fallback is disabled; other Clarity surfaces remain active. " + ex);
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
                _log?.LogError("PrayerClarity forecast failed; vanilla pulpit UI remains available. " + ex);
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
