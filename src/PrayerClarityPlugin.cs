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
        internal const string PluginVersion = "1.0.53";
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
                    Logger.LogError("Assembly-CSharp is unavailable; PrayerClarity is disabled.");
                    return;
                }

                Guid actualMvid = R.GameAssembly.ManifestModule.ModuleVersionId;
                if (actualMvid != SupportedGameMvid)
                {
                    Logger.LogWarning("Unsupported Graveyard Keeper build (Assembly-CSharp MVID " + actualMvid + "). PrayerClarity is disabled rather than patching an unverified build.");
                    return;
                }

                Localization.Initialize(Assembly.GetExecutingAssembly(), Logger);

                Type prayGui = R.GameType("PrayCraftGUI");
                MethodInfo redraw = R.Method(prayGui, "RedrawTextValues", false, new[] { typeof(float), typeof(float) });
                R.Patch(PluginGuid, typeof(PrayerClarityPlugin), redraw, nameof(RedrawTextValuesPostfix));

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
                    TechnologyPrayerCarousel.Install(PluginGuid, Logger);
                }
                catch (Exception ex)
                {
                    Logger.LogError("PrayerClarity prayer-Technology navigation is disabled; other Clarity surfaces remain active. " + ex);
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

                Logger.LogInfo(PluginName + " " + PluginVersion + " loaded. Clarity-only pulpit, Technology, prayer-item tooltip and Temporary Effects presentation; prayer Technology tooltips receive atomic-anchor content width plus viewport safety; no prayer mechanics are changed.");
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity initialization failed: " + ex);
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
