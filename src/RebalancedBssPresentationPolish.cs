using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    // Rebalanced-only final pass for Better Save Soul Technology presentation.
    // It deliberately runs after SecondarySurfacePresentation so Vanilla can keep the
    // exact accepted 1.0.33 bytes while Rebalanced replaces stale stock BSS lore with
    // edition-owned text and adds the accepted Contentment duration emphasis.
    internal static class RebalancedBssPresentationPolish
    {
        private static ManualLogSource _log;
        private static Type _bubbleTextType;
        private static MethodInfo _resolvePrayerCrafts;
        private static MethodInfo _legacyNormalizeLore;
        private static bool _errorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type techUnlock = R.GameType("TechUnlock");
            Type tooltip = R.GameType("Tooltip");
            if (techUnlock == null) throw new MissingMemberException("TechUnlock");
            if (tooltip == null) throw new MissingMemberException("Tooltip");

            MethodInfo getTooltip = R.Method(techUnlock, "GetTooltip", false, new[] { tooltip });
            if (getTooltip == null) throw new MissingMethodException("TechUnlock.GetTooltip(Tooltip)");

            _resolvePrayerCrafts = typeof(SecondarySurfacePresentation).GetMethod(
                "ResolvePrayerCrafts",
                R.Stat,
                null,
                new[] { typeof(object) },
                null);
            _legacyNormalizeLore = typeof(SecondarySurfacePresentation).GetMethod(
                "NormalizeRebalancedBssLore",
                R.Stat,
                null,
                new[] { typeof(string), typeof(string) },
                null);
            _bubbleTextType = R.GameType("BubbleWidgetTextData");

            if (_resolvePrayerCrafts == null)
                throw new MissingMethodException("SecondarySurfacePresentation.ResolvePrayerCrafts(object)");
            if (_legacyNormalizeLore == null)
                throw new MissingMethodException("SecondarySurfacePresentation.NormalizeRebalancedBssLore(string,string)");
            if (_bubbleTextType == null)
                throw new MissingMemberException("BubbleWidgetTextData");

            PatchAfter(
                harmonyId + ".bss-presentation-polish",
                getTooltip,
                nameof(TechUnlockTooltipPostfix),
                harmonyId + ".technology");
        }

        private static void TechUnlockTooltipPostfix(object __instance, object __0)
        {
            try
            {
                if (__instance == null || __0 == null) return;

                List<object> crafts = ResolveCrafts(__instance);
                string family = ResolveSingleFamily(crafts);
                if (string.IsNullOrEmpty(family)) return;

                Localization.UseCurrentGameLanguage();

                if (string.Equals(family, "b_sin_shard", StringComparison.Ordinal))
                {
                    ReplaceLore(
                        __0,
                        crafts,
                        family,
                        "rebalanced.tech.sin_shard_lore");
                    return;
                }

                if (string.Equals(family, "b_grat_points_incr", StringComparison.Ordinal))
                {
                    ReplaceLore(
                        __0,
                        crafts,
                        family,
                        "rebalanced.tech.gratitude_lore");
                    AccentContentmentDurations(__0, crafts);
                }
            }
            catch (Exception ex)
            {
                if (_errorLogged) return;
                _errorLogged = true;
                _log?.LogError(
                    "PrayerClarity Rebalanced BSS presentation polish failed; existing Technology text remains available. " +
                    ex);
            }
        }

        private static List<object> ResolveCrafts(object techUnlock)
        {
            IEnumerable values = _resolvePrayerCrafts.Invoke(null, new[] { techUnlock }) as IEnumerable;
            if (values == null) return new List<object>();

            List<object> result = new List<object>();
            foreach (object value in values)
            {
                if (value != null) result.Add(value);
            }
            return result;
        }

        private static string ResolveSingleFamily(List<object> crafts)
        {
            if (crafts == null || crafts.Count == 0) return null;

            string family = null;
            foreach (object craft in crafts)
            {
                string current = PrayerFamilyFromCraftId(R.Id(craft));
                if (string.IsNullOrEmpty(current)) return null;

                if (family == null) family = current;
                else if (!string.Equals(family, current, StringComparison.Ordinal))
                    return null;
            }

            return family;
        }

        private static string PrayerFamilyFromCraftId(string craftId)
        {
            const string prefix = "pray:";
            if (string.IsNullOrEmpty(craftId) ||
                !craftId.StartsWith(prefix, StringComparison.Ordinal))
                return null;

            int tierSeparator = craftId.LastIndexOf(':');
            if (tierSeparator <= prefix.Length) return null;
            return craftId.Substring(prefix.Length, tierSeparator - prefix.Length);
        }

        private static void ReplaceLore(
            object tooltip,
            List<object> crafts,
            string family,
            string replacementKey)
        {
            string vanilla = R.VanillaLocalize(family + "_d");
            if (string.IsNullOrEmpty(vanilla) ||
                string.Equals(vanilla, family + "_d", StringComparison.Ordinal))
                return;

            string craftId = crafts == null || crafts.Count == 0 ? null : R.Id(crafts[0]);
            string normalized = _legacyNormalizeLore.Invoke(
                null,
                new object[] { craftId, vanilla }) as string;

            string replacement = Localization.F(replacementKey);
            if (string.IsNullOrEmpty(replacement) ||
                string.Equals(replacement, replacementKey, StringComparison.Ordinal))
                return;

            IList rows = GetRows(tooltip);
            if (rows == null) return;

            foreach (object row in rows)
            {
                if (row == null || !_bubbleTextType.IsInstanceOfType(row)) continue;
                string text = R.Get(row, "text") as string;
                if (string.IsNullOrEmpty(text)) continue;

                string updated = ReplaceExact(text, vanilla, replacement);
                if (!string.IsNullOrEmpty(normalized) &&
                    !string.Equals(normalized, vanilla, StringComparison.Ordinal))
                    updated = ReplaceExact(updated, normalized, replacement);

                if (!string.Equals(updated, text, StringComparison.Ordinal))
                    R.Set(row, "text", updated);
            }
        }

        private static string ReplaceExact(string text, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(text) ||
                string.IsNullOrEmpty(oldValue) ||
                text.IndexOf(oldValue, StringComparison.Ordinal) < 0)
                return text;

            return text.Replace(oldValue, newValue);
        }

        private static void AccentContentmentDurations(object tooltip, List<object> crafts)
        {
            IList rows = GetRows(tooltip);
            if (rows == null) return;

            HashSet<string> plainDurations = new HashSet<string>(StringComparer.Ordinal);
            foreach (object craft in crafts)
            {
                PrayerForecast.TierDetails tier = PrayerForecast.BuildTierDetails(craft);
                if (tier == null || !tier.HasSpecialDuration) continue;

                string plain = Localization.F("tech.duration") + ": " +
                               Localization.F("active.timer_days", tier.SpecialDurationDays);
                if (!string.IsNullOrEmpty(plain))
                    plainDurations.Add(plain);
            }

            if (plainDurations.Count == 0) return;

            foreach (object row in rows)
            {
                if (row == null || !_bubbleTextType.IsInstanceOfType(row)) continue;
                string text = R.Get(row, "text") as string;
                if (string.IsNullOrEmpty(text)) continue;

                string updated = text;
                foreach (string plain in plainDurations)
                {
                    if (updated.IndexOf(plain, StringComparison.Ordinal) < 0) continue;
                    string accented = TechnologyTooltipTextStyle.AccentValueAfterColon(plain);
                    updated = updated.Replace(plain, accented);
                }

                if (!string.Equals(updated, text, StringComparison.Ordinal))
                    R.Set(row, "text", updated);
            }
        }

        private static IList GetRows(object tooltip)
        {
            object data = R.Get(tooltip, "data");
            return data == null ? null : R.Get(data, "data_list") as IList;
        }

        private static void PatchAfter(
            string patchOwner,
            MethodInfo target,
            string postfixName,
            string afterOwner)
        {
            Type harmonyType = R.AnyType("HarmonyLib.Harmony");
            Type harmonyMethodType = R.AnyType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony is unavailable.");

            MethodInfo postfixMethod = typeof(RebalancedBssPresentationPolish).GetMethod(
                postfixName,
                R.Stat);
            if (postfixMethod == null)
                throw new MissingMethodException(
                    typeof(RebalancedBssPresentationPolish).FullName,
                    postfixName);

            ConstructorInfo ctor = harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
            object postfix = ctor != null
                ? ctor.Invoke(new object[] { postfixMethod })
                : Activator.CreateInstance(harmonyMethodType);

            if (ctor == null)
            {
                FieldInfo methodField = harmonyMethodType.GetField("method", R.Inst);
                if (methodField == null) throw new MissingMemberException("HarmonyMethod.method");
                methodField.SetValue(postfix, postfixMethod);
            }

            if (!SetHarmonyMember(postfix, harmonyMethodType, "after", new[] { afterOwner }))
                throw new MissingMemberException("HarmonyMethod.after");

            R.TrackPatchOwner(patchOwner);
            object harmony = Activator.CreateInstance(harmonyType, new object[] { patchOwner });
            MethodInfo patch = harmonyType.GetMethods(R.Inst)
                .FirstOrDefault(m =>
                    m.Name == "Patch" &&
                    m.GetParameters().Length >= 5 &&
                    typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = null;
            args[2] = postfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static bool SetHarmonyMember(
            object instance,
            Type type,
            string name,
            object value)
        {
            FieldInfo field = type.GetField(name, R.Inst);
            if (field != null)
            {
                field.SetValue(instance, value);
                return true;
            }

            PropertyInfo property = type.GetProperty(name, R.Inst);
            if (property != null && property.CanWrite)
            {
                property.SetValue(instance, value, null);
                return true;
            }

            return false;
        }
    }
}
