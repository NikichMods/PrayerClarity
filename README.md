# PrayerClarity

PrayerClarity is a pair of alternative BepInEx mods for **Graveyard Keeper 1.407**. Both make prayer mechanics clearer; choose the edition that matches how you want the prayers themselves to behave.

## Editions

### PrayerClarity: Vanilla — 1.0.57

*Understand what your prayers actually do — without changing how they work.*

Keeps Graveyard Keeper's stock prayer mechanics and balance intact while improving prayer descriptions, quality comparisons, success requirements, pulpit information, item tooltips, and Character -> Temporary Effects.

[Download PrayerClarity: Vanilla 1.0.57](https://github.com/NikichMods/PrayerClarity/releases/tag/v1.0.57)

### PrayerClarity: Rebalanced — 0.2.51

Uses the same Clarity presentation layer, but intentionally rebalances and repairs the prayer roster so different prayers and qualities create more meaningful choices. Runtime-sensitive prayer effects are implemented through narrow Graveyard Keeper-native seams where verified.

[Download PrayerClarity: Rebalanced 0.2.51](https://github.com/NikichMods/PrayerClarity/releases/tag/rebalanced-v0.2.51)

**Install one edition, not both.**

## Current stable differences

- **Vanilla 1.0.57:** keeps stock 1.407 prayer mechanics/balance. Prayer Technology titles and section headers are centered consistently with mouse and gamepad while mechanics rows remain left-aligned; the accepted prayer-item, pulpit, Temporary Effects, localization and gamepad-navigation presentation is preserved.
- **Rebalanced 0.2.51:** includes the same Clarity layer plus the accepted full-roster rebalance and Better Save Soul rework. Combo Prayer uses Faith **+100 / +150 / +200%** and donations **+100 / +250 / +500%**; the accepted Repose and prayer-item presentation work is retained.

## Shared clarity features

- **Pulpit:** explains guaranteed reward sources, exact success chance, prayer-owned success bonuses, special effects, and effect duration while preserving the sermon itself as the reveal moment for the final Faith/donation payout.
- **Technology tree:** shows shared prayer properties once, then compact Bronze/Silver/Gold tier snapshots with the quality needed for 100% success and the values that actually change by tier. Prayer titles, Base Result and On Success are centered consistently across mouse and gamepad; mechanics content remains left-aligned.
- **Prayer item tooltips:** show the mechanics of the concrete prayer quality you are holding rather than repeating the full three-tier comparison.
- **Character -> Temporary Effects:** shows the actual quantitative effect of active prayer buffs and expresses long remaining durations in in-game days.
- **Localization:** PrayerClarity-owned text is included for all 11 interface languages supported by Graveyard Keeper: English, French, German, Simplified Chinese, Spanish, Brazilian Portuguese, Korean, Japanese, Russian, Italian, and Polish.

## Installation

1. Install **BepInEx 5** for Graveyard Keeper.
2. Download the DLL for the edition you want:
   - Vanilla: `PrayerClarity.dll`
   - Rebalanced: `PrayerClarity.Rebalanced.dll`
3. Put the DLL in `Graveyard Keeper/BepInEx/plugins/PrayerClarity/`.
4. When updating, replace the existing DLL. Do not keep old PrayerClarity versions beside the new one.

Neither edition requires user-facing configuration.

## Compatibility

- Graveyard Keeper **1.407**
- BepInEx 5

Graveyard Keeper **1.407** with the verified `Assembly-CSharp` identity is the tested target. Other storefront/build identities are treated as **unverified**, not automatically rejected: PrayerClarity attempts best-effort activation through the same exact runtime contracts and records the detected MVID/compatibility mode in the BepInEx log. If a required contract is missing, the affected activation is contained rather than guessing another target.

## License

Current PrayerClarity source is released under the [Mozilla Public License 2.0](LICENSE). See [LICENSING.md](LICENSING.md) for historical MIT releases and licensing details.
