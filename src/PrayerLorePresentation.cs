using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class PrayerLorePresentation
    {
        private const string ExcellenceBaseId = "b_star";
        private const string ExcellenceLoreKey = "b_star_d";

        private static ManualLogSource _log;
        private static bool _errorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type craftDefinition = R.GameType("CraftDefinition");
            if (craftDefinition == null)
                throw new MissingMemberException("CraftDefinition type is unavailable.");

            MethodInfo getDescription = R.Method(craftDefinition, "GetDescription", false, Type.EmptyTypes);
            if (getDescription == null)
                throw new MissingMethodException("CraftDefinition.GetDescription()");

            R.Patch(
                harmonyId + ".prayerlore",
                typeof(PrayerLorePresentation),
                getDescription,
                nameof(GetDescriptionPostfix));
        }

        private static void GetDescriptionPostfix(object __instance, ref string __result)
        {
            try
            {
                string outputId = FirstOutputId(__instance);
                if (!IsExcellenceOutput(outputId)) return;

                string lore = R.VanillaLocalize(ExcellenceLoreKey);
                if (string.IsNullOrEmpty(lore) ||
                    string.Equals(lore, ExcellenceLoreKey, StringComparison.Ordinal))
                    return;

                if (!string.IsNullOrEmpty(__result) &&
                    __result.IndexOf(lore, StringComparison.Ordinal) >= 0)
                    return;

                __result = string.IsNullOrEmpty(__result)
                    ? lore
                    : lore + "\n" + __result;
            }
            catch (Exception ex)
            {
                if (_errorLogged) return;
                _errorLogged = true;
                _log?.LogError("PC_RUNTIME_FALLBACK feature=prayer-lore action=vanilla-description " + ex);
            }
        }

        private static string FirstOutputId(object craft)
        {
            IEnumerable output = craft == null ? null : R.Get(craft, "output") as IEnumerable;
            if (output == null) return null;

            foreach (object item in output)
                return R.Id(item);

            return null;
        }

        private static bool IsExcellenceOutput(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            return string.Equals(itemId, ExcellenceBaseId, StringComparison.Ordinal) ||
                   string.Equals(itemId, ExcellenceBaseId + "_2", StringComparison.Ordinal) ||
                   itemId.StartsWith(ExcellenceBaseId + ":", StringComparison.Ordinal);
        }
    }
}
