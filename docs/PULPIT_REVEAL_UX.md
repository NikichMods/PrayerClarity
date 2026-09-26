# Pulpit reward-reveal UX decision

Status: **implemented and runtime-accepted** in the stable PrayerClarity sibling presentation layer. Original design decision approved 2026-09-15; current public stable baselines are Vanilla 1.0.57 and Rebalanced 0.2.51.

This decision was made after reviewing the current pulpit prototype that exposes exact pre-sermon Faith and donation totals.

## UX finding

The exact forecast solves clarity, but it also removes an important reward-reveal moment from the sermon itself.

In stock play, the player chooses a prayer, watches the congregation, then discovers the concrete Faith and donation payout through the sermon animation. Improving the church or using a better prayer can therefore produce a meaningful "that was much better" moment while the reward is visibly distributed.

If the pulpit already says, for example, "guaranteed 4 Faith and 1s24c; +2 Faith and 31c on success", the ceremony no longer reveals anything. The player is mostly waiting for a known result to finish animating.

**UX finding:** PrayerClarity should make the decision understandable without necessarily revealing the exact current sermon payout before the sermon runs.

## Accepted design principle

**Before the sermon, explain the rules, dependencies, modifiers, success conditions, and prayer-specific effects. Let the sermon animation reveal the exact Faith/donation totals.**

This supersedes only the exact-number rendering aspect of the earlier `guaranteed + success bonus + special effect` pulpit model. The semantic separation itself remains useful and should be preserved.

PrayerClarity remains a white-box UX, but "white-box" means the player can understand why one choice is better and what changes the outcome. It does not require turning the pulpit into a final-payout calculator.

## Default pulpit information model

The default pulpit should retain:

1. current Church Quality;
2. current Graveyard Quality where relevant to the stock surface;
3. sermon requirement and exact success probability where useful;
4. the separation between guaranteed/base reward and success-only prayer contribution;
5. exact prayer-specific effects, durations, probabilities, caps, and fixed outputs when they are intrinsic properties of the prayer;
6. enough information to compare prayer qualities and alternative prayers before committing the weekly sermon.

The default pulpit should **not** expose the fully computed current Faith total, donation total, or fully computed success-only Faith/donation delta when those numbers would directly disclose what the ceremony is about to pay out.

## Preferred grammar

For ordinary resource generation, prefer source relationships and prayer modifiers over computed totals.

Example shape:

- `Guaranteed:` Faith — from Church Quality; Donations — from Graveyard Quality.
- `On success (100%):` Faith `+50%`; Donations `+25%`.
- `Effect:` exact special effect and duration.

Use native icons in place of obvious nouns where that remains readable.

The exact wording/localization may change, but the semantic rule should remain: show **what drives the reward** and **what this prayer changes**, not the final current payout.

## What should remain exact

Do not hide numbers merely to create mystery. Exact values are desirable when they describe the prayer itself rather than the final sermon payout.

Examples include:

- success chance;
- effect duration;
- growth-time reduction;
- damage / armor / regeneration values;
- confession probability;
- Soul Gratitude or Sin Shard multipliers;
- Blessing counts or other discrete prayer-owned item outputs;
- fixed prayer-owned Faith/money additions when they are themselves the mechanic, provided the UI still does not collapse everything into the exact final sermon total.

The boundary is therefore **intrinsic mechanic vs final resolved payout**, not simply "numbers are bad".

## Why not `low / medium / high`

A qualitative payout ladder such as `low / medium / high` is not the preferred default.

It creates arbitrary thresholds and can hide real progression. If Church Quality rises enough to improve Faith but both old and new payouts remain inside the same bucket, the UI falsely appears unchanged. The same problem exists for Graveyard Quality and donations.

Source/dependency + modifier presentation preserves comparison without inventing opaque buckets.

## Church-quality and prayer-quality changes

This model deliberately separates two questions:

- **What improved because the church/graveyard changed?** The pulpit shows the current input values and states which base reward each input drives.
- **What improved because the prayer changed?** The pulpit shows the prayer's success requirement and exact modifier/effect.

The player can therefore see both kinds of progression without being shown the final resolved reward in advance.

## Layout consequence

If the source relationship is visible directly in the Guaranteed row, a separate footer sentence such as "guaranteed Faith depends on Church Quality; guaranteed donations depend on Graveyard Quality" is probably redundant and should be removed unless runtime readability proves otherwise.

This may simplify the current enlarged pulpit layout and give more space to prayer effects.

## Internal calculation policy

The semantic/forecast model may still calculate exact values internally when needed for correctness, testing, diagnostics, balance work, or an optional advanced-detail mode.

Default player-facing rendering must not expose those exact current sermon totals merely because the model can compute them.

No side-effecting prayer calculation should be invoked for preview rendering; existing side-effect-free forecast constraints remain unchanged.

## Accepted implementation state

This design is now part of the stable shared Clarity presentation used by both sibling editions. The pulpit preserves the `guaranteed/base dependency -> success-only prayer contribution -> special effect` decomposition while withholding the fully resolved current Faith/donation payout until the sermon animation.

Future pulpit changes should treat this reward-reveal boundary as an accepted product constraint unless new player/runtime evidence explicitly justifies reopening it.
