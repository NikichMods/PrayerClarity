namespace PrayerClarity
{
    internal delegate bool PrayerTierEffectResolver(string craftId, string buffId, out string text, out string semanticKey);
    internal delegate bool PrayerActiveEffectResolver(string buffId, out string text);
    internal delegate bool PrayerTechnologyEffectResolver(string craftId, out string sharedText, out string tierText);
    internal delegate bool PrayerSoulConversionResolver(string craftId, out int cap, out int conversion);
    internal delegate bool PrayerEventResolver(string craftId, string currentEventId, out string effectiveEventId);
    internal delegate bool PrayerLoreOverrideResolver(string craftId, string vanillaLore, out string lore);

    // Shared presentation seam for sibling editions. Vanilla leaves it unconfigured;
    // Rebalanced installs its provider during plugin initialization. Shared Clarity
    // code therefore remains edition-agnostic and independently buildable.
    internal static class PrayerEditionSemantics
    {
        private static PrayerTierEffectResolver _tierEffect;
        private static PrayerActiveEffectResolver _activeEffect;
        private static PrayerTechnologyEffectResolver _technologyEffect;
        private static PrayerSoulConversionResolver _soulConversion;
        private static PrayerEventResolver _prayEvent;
        private static PrayerLoreOverrideResolver _loreOverride;

        internal static void Install(
            PrayerTierEffectResolver tierEffect,
            PrayerActiveEffectResolver activeEffect,
            PrayerTechnologyEffectResolver technologyEffect = null,
            PrayerSoulConversionResolver soulConversion = null,
            PrayerEventResolver prayEvent = null,
            PrayerLoreOverrideResolver loreOverride = null)
        {
            _tierEffect = tierEffect;
            _activeEffect = activeEffect;
            _technologyEffect = technologyEffect;
            _soulConversion = soulConversion;
            _prayEvent = prayEvent;
            _loreOverride = loreOverride;
        }

        internal static void Reset()
        {
            _tierEffect = null;
            _activeEffect = null;
            _technologyEffect = null;
            _soulConversion = null;
            _prayEvent = null;
            _loreOverride = null;
        }

        internal static bool TryBuildTierEffect(string craftId, string buffId, out string text, out string semanticKey)
        {
            text = null;
            semanticKey = null;
            return _tierEffect != null && _tierEffect(craftId, buffId, out text, out semanticKey);
        }

        internal static bool TryBuildActiveEffect(string buffId, out string text)
        {
            text = null;
            return _activeEffect != null && _activeEffect(buffId, out text);
        }

        internal static bool HasTechnologyProvider
        {
            get { return _technologyEffect != null; }
        }

        internal static bool TryBuildTechnologyEffect(string craftId, out string sharedText, out string tierText)
        {
            sharedText = null;
            tierText = null;
            return _technologyEffect != null && _technologyEffect(craftId, out sharedText, out tierText);
        }

        internal static bool TryGetSoulConversion(string craftId, out int cap, out int conversion)
        {
            cap = 0;
            conversion = 0;
            return _soulConversion != null && _soulConversion(craftId, out cap, out conversion);
        }

        internal static bool TryResolveLoreOverride(
            string craftId,
            string vanillaLore,
            out string lore)
        {
            lore = vanillaLore;
            return _loreOverride != null &&
                   _loreOverride(craftId, vanillaLore, out lore) &&
                   !string.IsNullOrEmpty(lore);
        }

        internal static bool TryGetEffectivePrayEvent(
            string craftId,
            string currentEventId,
            out string effectiveEventId)
        {
            effectiveEventId = currentEventId;
            return _prayEvent != null &&
                   _prayEvent(craftId, currentEventId, out effectiveEventId) &&
                   !string.IsNullOrEmpty(effectiveEventId);
        }
    }
}
