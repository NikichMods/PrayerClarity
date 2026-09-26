# Changelog

## PrayerClarity: Rebalanced 0.2.47

- Promotes the runtime-accepted post-0.2.38 presentation work as the new Rebalanced stable line without changing the accepted prayer balance or mechanics.
- Repose now uses one consistent player-facing model across Technology, prayer items, the pulpit and Character -> Temporary Effects; terminal progression states no longer imply that a higher ordinary body-quality tier is still available.
- Bronze/Silver/Gold Repose reliability is stated explicitly, and the terminal Bronze comparison uses separate native quality-star lines for quick scanning.
- Prayer-item tooltips return to the game's standard native width. The 100%-success requirement is centered, while Base Result and On Success content share a clean visible left edge without widening the parchment.
- Preserves the accepted amount + inline-resource wrap repair and all previously accepted 0.2.38 mechanics, requirements, durations, payouts, RNG and balance values.
- Stable publication reuses the exact runtime-tested 0.2.47 DLL without rebuilding.

## PrayerClarity: Rebalanced 0.2.38

- Promotes the runtime-accepted post-0.2.23 presentation and Better Save Soul work as the new Rebalanced stable line.
- Prayer for Donations now uses success-only flat rewards of **+20 / +50 / +100 silver-equivalent**; the Gold pulpit displays the native denomination as **+1 gold**.
- Better Save Soul prayers use the accepted rework: Soul's Repose converts stored Soul Gratitude to Faith **1:1** on success up to **30 / 60 / 90**, Soul Contentment gives **+50% Soul Gratitude** for **45 / 90 / 135 minutes** and prevents passive soul-condition decay while active, and Thorough Cleansing gives **x2 / x3 / x4 Sin Shards** at **q30 / q60 / q120**.
- Prayer-item and Technology presentation, controller tooltip behavior, long-localization pulpit sizing, and live language-font switching include the accepted fixes through 0.2.37.
- Shoots & Roots shows the **95% combined all-sources cap** only on Gold Temporary Effects; Bronze/Silver show only their own prayer reduction.
- The late Bronze Repose reliability clarification is shown as a parenthetical note; legacy Protective Prayer items intentionally keep their historical lore.
- Final 0.2.38 changes are presentation-only over the already accepted mechanics; the exact tested DLL is published without rebuilding.



## PrayerClarity: Vanilla 1.0.33

- Adds the accepted gamepad prayer-Technology carousel without changing stock prayer mechanics or balance.
- In any Technology with multiple visible unlocks and at least one prayer, Left/Right now selects one visible unlock at a time, highlights its icon, and shows only its own tooltip; moving past the first/last unlock returns to normal Technology-tree navigation.
- Mixed prayer Technologies include their non-prayer visible unlocks in the same sequence so icon and tooltip ownership stay one-to-one.
- Mouse/keyboard tooltip behavior and Technologies without prayers remain vanilla.

## PrayerClarity: Rebalanced 0.2.23

- Generalizes the accepted Better Save Soul controller carousel to every prayer-bearing Technology with multiple visible unlocks.
- Uses the same data-driven rule and horizontal boundary behavior as Vanilla 1.0.33.
- Preserves all accepted Rebalanced prayer mechanics, balance values, requirements, BSS effects and save behavior; this release changes only Technology presentation/navigation on gamepad.

## PrayerClarity: Rebalanced 0.2.20 candidate

- Reverts the unaccepted 0.2.19 mixed-BSS grouping/lore-suppression experiment and returns Technology presentation structure to the 0.2.18 baseline.
- Fixes the final Soul's Repose pulpit writer: the last `PulpitPolish` pass now renders the live success-only Soul Gratitude -> Faith transaction and hides the otherwise empty separate Effect row.
- Preserves BSS prayer lore instead of deleting it. Rebalanced normalizes only stale mechanic fragments: Soul Contentment keeps the stock localized prose while its obsolete stock percentage is replaced from the current edition-owned effect value; Thorough Cleansing keeps its sentence while the stock fixed `(x2)` suffix is removed.
- Reuses the historically runtime-verified Technology width lifecycle from PrayerClarity 1.0.13: `UILabel.overflowWidth` controls the native `ResizeFreely` label, then `WidgetsBubbleGUI.UpdateSize()` grows the enclosing parchment. For the combined BSS tooltip, the retained real lore rows and PrayerClarity mechanics row use a finite 520-unit expansion ceiling; ordinary single-prayer Technology tooltips retain the existing content-driven width policy.
- Prayer mechanics, BSS balance values and already-passed 0.2.18 runtime mechanics are unchanged. Acceptance is visual/presentation-only.

## PrayerClarity: Rebalanced 0.2.19 candidate

- Fixes the Gold Soul's Repose pulpit presentation after runtime identity evidence proved the naturally selected prayer is the exact canonical `pray:b_souls:3` object: the live layout now preserves the success-only Soul Gratitude -> Faith transaction instead of overwriting it with the generic resource row or restoring the old stock effect sentence.
- Keeps the accepted Soul's Repose conversion mechanics unchanged: ordinary church-derived base Faith plus success-only 1:1 Soul Gratitude conversion capped at 30 / 60 / 90.
- Reworks mixed Rebalanced prayer Technology nodes generically by grouping tier snapshots under each localized prayer name, compacting each quality to a single semantic row, and widening only the mixed success-details body when needed.
- Suppresses stale stock BSS lore when Rebalanced owns replacement semantics, removing conflicting `+10%`, old Soul-Gratitude base-scaling wording, and fixed `x2` text while retaining the effective tier values from the live Rebalanced rules.
- Single-family Technology tooltips keep their existing width behavior. No new per-frame polling or broad scans are introduced.
- Reuses already-passed runtime evidence for Soul's Repose conversion arithmetic, Soul Contentment decay protection, and Thorough Cleansing output scaling; the remaining acceptance gate is visual/readability verification only.

## PrayerClarity: Rebalanced 0.2.18 candidate

- Fixes the live Soul's Repose sermon/pulpit path exposed by 0.2.17 runtime testing: the selected prayer now projects its tier's ordinary `default_1/2/3` event directly into the native `PrayLogics.CalculatePray` call, so stale runtime PrayCraft copies cannot fall back to the old Soul-Gratitude base formula.
- The pulpit forecast resolves the same edition-owned effective event and again shows the live success transaction `(gratitude_points) -X -> (faith) +X`.
- Soul's Repose Technology tier rows use a shorter localized “sermon limit” label and keep only the amount + Soul Gratitude icon as an unbreakable unit, preserving normal wrapping for long locales.
- Thorough Cleansing uses tier-neutral explanatory prose; its `×2 / ×3 / ×4` value is highlighted with the existing Technology accent color while the localized label remains wrappable.
- Keeps the 0.2.17 balance values and runtime architecture otherwise unchanged.
- 0.2.17 remains an immutable rejected candidate: its direct conversion probe and Gold Thorough Cleansing path passed, but the natural Soul's Repose pulpit path exposed the stale-event presentation/runtime gap and the Contentment research probe itself had a reflection lookup failure.

## PrayerClarity: Rebalanced 0.2.17 candidate

- Prayer for Donations keeps q20 / q40 / q60 and changes its success-only flat payout to **+20 / +50 / +100 silver**.
- Prayer for Soul's Repose changes to q30 / q60 / q90. Its base Faith now uses the ordinary church sermon event; on success it converts current Soul Gratitude to bonus Faith at **1:1**, capped at **30 / 60 / 90** by prayer quality, and spends only the amount converted.
- Soul's Repose shows the live pending transaction at the pulpit, e.g. `(gratitude_points) -73 -> (faith) +73`.
- Prayer for Soul Contentment keeps q20 / q40 / q60, gives **+50% Soul Gratitude** at all qualities, lasts **45 / 90 / 135 minutes** (1 / 2 / 3 vanilla six-day weeks), and prevents passive soul-condition decay while active, both in the corpse and after extraction. Soul Extractor damage remains vanilla.
- Prayer for Thorough Cleansing changes to q30 / q60 / q120 and scales Sin Shard output to **x2 / x3 / x4** through the game's native `increase_sin_shard_drop` calculation.
- All new player-facing strings are present in the 11 supported locales.
- Runtime acceptance is still required before promotion to `main`.

## PrayerClarity: Rebalanced 0.2.16

- Repose Gold 100%-success requirement changes from **90** to **95 Church Quality**.
- Excellence Gold 100%-success requirement changes from **90** to **95 Church Quality**.
- Bronze/Silver requirements and all prayer effects, durations, payouts, formulas, hooks and runtime behavior are otherwise unchanged.
- Thorough Cleansing remains **30 / 60 / 90**; Soul's Repose remains **30 / 60 / 120**.
- The q95 change is an intentional premium-Gold soft capstone: a practical passive CQ94 church reaches 99%, while a small temporary Church Quality boost or a more specialized passive layout can reach 100%.
- No in-game runtime retest was required because the executable delta is limited to two values in the canonical Rebalanced requirements table, which are projected directly into the already-verified native `CraftDefinition.needs_quality` path.

## PrayerClarity: Rebalanced 0.2.15

- Repose Bronze remains stock-style and Silver keeps the accepted 0.2.14 50/50 reliability behavior.
- Repose Gold now guarantees the **maximum total skull count inside the best actually available ordinary corpse tier**, rather than guaranteeing only that hidden tier.
- The Gold rule is dynamic: it derives the current eligible tier and corpse-part skull values from live game data; it does not hard-code tier 3, body IDs, or 10 skulls.
- Repose requirements change from **20 / 40 / 60** to **20 / 40 / 90**. Durations remain **30 / 42 / 54 minutes**.
- Stock `GameSave.GenerateBody`, RNG, body creation and downstream behavior remain authoritative; the mod scopes the eligible body-definition catalog only for the Gold generation call and restores the exact original reference afterward.
- Runtime evidence on Graveyard Keeper 1.407 directly observed the complete 9-body maximum-score Gold candidate set and ten consecutive real generation calls selecting tier-3, 10-total-skull bodies with catalog restoration after every call.

## PrayerClarity: Rebalanced 0.2.14

- Accepted stable release using the same condition-style **On success:** / **При успехе:** heading already accepted in Vanilla 1.0.32.
- This shortens the heading and makes success scope consistent across the two sibling editions.
- Prayer-item tooltips intentionally remain unchanged and keep **Bonuses on success** / **Бонусы при успехе**.
- No prayer mechanics, balance values, requirements, payouts, durations, stacking, HUD behavior, or other Rebalanced presentation change.

## PrayerClarity: Vanilla 1.0.32

- Accepted stable release refining Technology prayer hierarchy without changing stock Graveyard Keeper 1.407 mechanics.
- Vanilla Technology uses the condition-style header **On success:** / **При успехе:** instead of **Bonuses on success**, making the heading apply naturally to the whole success-only block.
- When a stock prayer has both a shared Faith/donation rider and a shared named effect, those two lines are kept together without a blank gap; tier blocks remain separated below.
- Prayer-item tooltips keep their existing **Bonuses on success** heading and accepted spacing.
- Rebalanced presentation is behaviorally unchanged: its Technology provider keeps the existing Rebalanced heading/grouping.

## PrayerClarity: Rebalanced 0.2.13

- Accepted stable release preserving the 0.2.12 Technology repair and all 0.2.11 specialist/resource-balance changes.
- Soul Contentment now shows its +20% gain with the native Soul Gratitude icon instead of the ambiguous generic word “gratitude”.
- BSS Soul's Repose replaces the redundant shared Technology sentence with a direct scaling hint: more Soul Gratitude means a larger Faith bonus.
- No prayer mechanics, requirements, payouts, durations, stacking, HUD behavior, or shared Vanilla presentation change.

## PrayerClarity: Rebalanced 0.2.12 candidate

- Preserves all 0.2.11 specialist-purity and resource-balance changes.
- Fixes the Technology tooltip regression exposed when a specialist has no generic Faith/money success contribution: PrayerClarity now replaces the stock requirement/lore row directly and inserts **Base result** / **Bonuses on success** before the crafting-location footer even when vanilla omits its `preach_params_2` block.
- Removes the stale stock **+10%** Soul Contentment Technology sentence in Rebalanced, where the effective prayer is **+20%**; the current effect is shown only from Rebalanced mechanics data.
- Prayer-item tooltips, pulpit/HUD, Temporary Effects, balance values, durations and specialist mechanics are unchanged from 0.2.11.

## PrayerClarity: Rebalanced 0.2.11 candidate

- Purifies specialist prayers: non-resource utility prayers no longer carry unrelated prayer-owned Faith/donation percentage riders.
- Removes unrelated fixed Faith/money success outputs from those specialist prayers while preserving their named effects and physical specialist rewards.
- Retunes Prayer for Donations to flat success rewards of **+5 / +15 / +30 silver** at Bronze/Silver/Gold.
- Retunes Combo Prayer donations to **+100 / +200 / +300%** while keeping Combo Faith at **+100 / +150 / +200%**.
- Keeps Prayer for Faith, Prayer for Donations, Combo Prayer, BSS Prayer for Soul's Repose, and Casual Prayer as explicit resource/starter exceptions.
- No changes to accepted specialist effect magnitudes, durations, stacking rules, Repose corpse-quality behavior, Roots cap, or shared Vanilla presentation.

## PrayerClarity: Vanilla 1.0.31 / Rebalanced 0.2.10

- Accepted shared release: replaces the visually ineffective blank-separator spacing attempt from 1.0.30 / 0.2.9 with a controlled leading text line on PrayerClarity-owned **Base result** and **Bonuses on success** title rows in prayer-item tooltips only.
- Keeps Technology comparison tooltips unchanged and vertically compact.
- Preserves the accepted 1.0.30 / 0.2.9 Repose q20/q40/q60 requirement, Prayer for Excellence stock lore fallback, compact Base-result grammar, and all previously accepted mechanics/presentation behavior.
- No prayer mechanics, balance values, durations, stacking rules, payouts, or HUD behavior change in this candidate.

## Shared candidate — Vanilla 1.0.30 / Rebalanced 0.2.9

- Adds native blank spacing before **Base result** in single-prayer item tooltips; Technology comparison tooltips remain vertically compact.
- Restores the stock Prayer for Excellence lore on crafting surfaces when Graveyard Keeper's multi-quality output path omits it, reusing the game's own localized `b_star_d` text rather than adding new copy.
- Rebalanced Prayer for Repose changes only the Gold guaranteed-success Church Quality requirement from **50 to 60**; Bronze/Silver remain 20/40 and the accepted corpse-quality behavior plus 30/42/54-minute durations are unchanged.
- Preserves all runtime-accepted 1.0.29 / 0.2.8 Clarity presentation, including compact Base result parentheses and the Roots 95% cap only on the active Temporary Effects surface.

## Shared candidate — Vanilla 1.0.29 / Rebalanced 0.2.8

- Keeps the runtime-accepted 1.0.28 / 0.2.7 tooltip structure and compact reward rows.
- Shortens Base result dependency wording to parenthetical source labels, e.g. `Faith (Church Quality)` and `Donations (Graveyard Quality)`; Soul's Repose keeps Soul Gratitude in the Faith source.
- Rebalanced Shoots & Roots prayer-selection surfaces now show only the direct tier effect (-20/-30/-40% growth time); the existing 95% combined safety cap remains mechanically unchanged and stays visible on the active Temporary Effects surface.
- Leaves the accepted HUD decimal punctuation behavior unchanged; no heavier punctuation-only workaround is added.
- No prayer balance values, stacking rules, duration rules, or runtime mechanics are changed.

## Shared candidate — Vanilla 1.0.28 / Rebalanced 0.2.7

- Simplifies Faith/donation percentage rows to the resource plus signed percentage, removing the redundant “of base value” suffix.
- Renders single-item prayer rewards as one atomic `localized name ×N` row instead of a separate Quantity line.
- Reuses the existing PrayerClarity-owned content-width mechanism for prayer-item success rows so the 100% success threshold stays visually intact.
- Refreshes the game locale once on the first long-prayer HUD timer render, covering BepInEx initialization before Graveyard Keeper loads GameSettings without adding per-frame language polling.
- Preserves all prayer mechanics and the accepted Base result / Bonuses on success structure; runtime acceptance is still required.

## PrayerClarity: Rebalanced 0.2.4

- Increased Repentance duration from 18 / 36 / 54 to **30 / 42 / 54 minutes** while keeping the accepted 50 / 75 / 100% daily confession probabilities.
- Increased Repose duration from 18 / 36 / 54 to **30 / 42 / 54 minutes** while keeping the accepted Bronze/Silver/Gold corpse-quality behavior unchanged.
- The change targets discrete roughly once-per-day confession and Donkey-delivery opportunities: Bronze now has a credible multi-day window, Silver keeps a distinct intermediate duration step, and Gold remains unchanged.
- No new runtime hooks or polling were added; duration is projected through the existing prayer craft data at the established load-time projection seam.

## PrayerClarity: Rebalanced 0.2.3

- Preserved the accepted Rebalanced prayer balance while closing the remaining runtime architecture audit.
- Shoots & Roots keeps the stock growth expression authoritative and uses the accepted 95% combined growth-time reduction cap.
- Repentance now leaves the stock daily 15% reset and confession RNG flow intact, projecting only the effective 50 / 75 / 100% probability through the native FlowCanvas player-param accessor while the prayer is active.
- Combat damage now follows the native `add_damage` path: Bronze keeps stock +5, Silver/Gold add only the scoped +5 / +10 tier delta during native damage calculation.
- Combat armor now follows native `add_armor` handling through a scoped nonserialized +4 projection during `DecHP`, removing the previous ThreadStatic/global `GetParam` interception.
- Combat regeneration remains on the game's native buff `se_tick` extension point and was runtime-verified at +1 / +2 / +4 HP per second.
- Runtime verification confirmed Repentance 0.50 / 0.75 / 1.00, Combat damage deltas 0 / +5 / +10, exactly +4 armor, and tiered regeneration with no PrayerClarity runtime error.
- Research Test Console diagnostics remain development-only and are not shipped in the release.

## PrayerClarity: Vanilla 1.0.25

- Added the current Soul Gratitude value to the pulpit context for Soul's Repose.
- Added contextual Repose endpoint wording when stock +1 corpse-tier maximum no longer opens a higher ordinary body tier.
- Kept all prayer mechanics, requirements, rewards, formulas and balance stock Graveyard Keeper 1.407.
- Preserved synchronized Clarity presentation and all 11 supported interface languages.

## PrayerClarity: Rebalanced 0.2.0

- Reworked the main resource family into clearer roles:
  - Faith: flat-only +5 / +10 / +20 Faith, q20 / 40 / 60.
  - Donations: flat-only +5 / +10 / +15 silver, q20 / 40 / 60.
  - Combo: percentage-only +100 / +150 / +200% Faith and donations, q40 / 60 / 80.
- Revised Church Quality ladders across Repentance, Roots, Combat, Imagination, Excellence and Better Save Soul prayers.
- Set BSS Soul's Repose to q30 / 60 / 120 with +50 / +100 / +150% Faith on the verified Soul Gratitude base.
- Added current Soul Gratitude to the Soul's Repose pulpit context.
- Fixed Rebalanced Repose to narrow toward the highest actually existing eligible corpse tier instead of blindly using the raw numeric maximum.
- Added contextual Repose endpoint wording for states where stock/Bronze Repose cannot open a higher ordinary corpse tier.
- Kept Imagination's +0.7 writing-quality effect and its 3 Silver / 3 Gold Story success rewards through the stock sermon-drop pipeline.
- Preserved stock prayer durations and all 11 supported interface languages.

## PrayerClarity: Vanilla 1.0.24

- Refined Technology prayer tooltips around a shared-effect -> tier-specific-value hierarchy.
- Improved semantic emphasis, wrapping, atomic rows, and content-driven width for clearer Bronze/Silver/Gold comparison.
- Removed the redundant shared Prosperity effect line while preserving the tier-specific Commercial Blessing x1/x2/x3 rewards.
- Kept prayer-item, pulpit, and Temporary Effects wording synchronized with the same mechanics semantics.
- Preserved stock Graveyard Keeper 1.407 prayer mechanics and balance.

## PrayerClarity: Rebalanced 0.1.5

- First accepted Rebalanced sibling release.
- Includes the full PrayerClarity presentation layer plus intentional prayer rebalance/rework.
- Uses the accepted specialist Faith/Donations/Soul's Repose progression of +200% / +300% / +400%.
- Rebalances Repentance, Shoots & Roots, Repose, Combat, Imagination, Excellence, Soul Contentment, and Thorough Cleansing with tier-aware effects and durations.
- Synchronizes Technology, prayer-item, pulpit, and Temporary Effects presentation with the effective Rebalanced mechanics.
- Keeps full localization coverage for all 11 supported interface languages.


## 1.0.20

- Added a clear vertical gap before the crafting-location footer in prayer Technology tooltips.
- Replaced punctuation-sensitive parsing of vanilla prayer requirement text with direct localized vanilla lore lookup.
- Fixed the Polish Prayer for Repose Technology tooltip retaining the stock `20–50` requirement text before its description.
- Preserved the existing Clarity-only behavior: no prayer balance or gameplay mechanics changes.

## 1.0.18

- Reworked prayer Technology tooltips into compact shared-details + Bronze/Silver/Gold tier snapshots.
- Added content-driven tooltip width and viewport safety so important tier rows stay readable without long effect text making every tooltip excessively wide.
- Clarified prayer-item tooltips for the concrete quality being inspected.
- Improved active prayer-effect descriptions and long-duration display.
- Refined pulpit information hierarchy while preserving the final sermon payout as an in-sermon reveal.
- Improved wording, spacing, wrapping, and localization across all 11 supported interface languages.
- Removed redundant non-prayer `linked_craft` lookups from prayer-item and Technology discovery paths.
- No prayer balance or gameplay mechanics changes.

## 1.0.1

- Extended Clarity presentation to prayer item tooltips.
- Preserved the existing Clarity-only pulpit, Technology, and Temporary Effects behavior.

## 1.0.0

- First stable Clarity release.
- Added prayer information improvements at the pulpit, in Technology tooltips, and in Character -> Temporary Effects.
- Added all 11 supported interface languages.
