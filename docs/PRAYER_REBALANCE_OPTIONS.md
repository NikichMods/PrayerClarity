# PrayerClarity: Rebalanced — stable roster specification

Status: **canonical accepted Rebalanced ruleset for PrayerClarity: Rebalanced 0.2.51**.

Stock Graveyard Keeper 1.407 mechanics remain documented independently in `PRAYER_MECHANICS.md`. Values below are intentional Balance/Rework design unless explicitly identified as a verified repair.

## Current stable runtime identity

- Rebalanced version: **0.2.51**
- frozen accepted ref: `accepted/rebalanced-0.2.51`
- exact accepted runtime/source SHA: `13c85c824bd2932b1175200368d212e03bbae79f`
- canonical DLL: `PrayerClarity.Rebalanced.dll`
- accepted DLL SHA-256: `083168ec6cd76fd55366ca10426e212a30a3bc2c7ca61a9e9de0dc6f656c8ccd`
- supported game: **Graveyard Keeper 1.407**

The sibling Vanilla public stable remains mechanically stock and is documented separately.

Rebalanced 0.2.39–0.2.47 refined presentation, 0.2.48 changed only Combo Prayer donation scaling, and 0.2.49–0.2.51 refined shared Technology presentation. The table below is the current stable 0.2.51 roster.

## Product rules

- **PrayerClarity: Vanilla** and **PrayerClarity: Rebalanced** are alternative sibling editions. Install one, not both.
- Rebalanced uses the shared Clarity presentation layer plus the rules below.
- Failed sermons retain full base donations by permanent project policy; prayer-owned success bonuses and special success outputs still depend on sermon success.
- The pulpit explains dependencies, success probability, prayer-owned modifiers and intrinsic effects without exposing the fully resolved final Faith/donation payout before the sermon animation.
- Mechanics and player-facing surfaces consume the same effective prayer semantics.
- External day-length mods are not balance targets; PrayerClarity derives displayed in-game-day duration from the game's effective day length where applicable.

## Accepted roster

| Family | 100% Church Quality | Current stable Rebalanced behavior |
| --- | ---: | --- |
| Ordinary `b_empty` | **10** | Stock starter baseline. |
| Faith `b_faith` | **20 / 40 / 60** | Success-only flat **+5 / +10 / +20 Faith**. No prayer-owned donation bonus. |
| Donations `b_money` | **20 / 40 / 60** | Success-only flat **+20 / +50 / +100 silver-equivalent**. No prayer-owned Faith bonus. Gold is naturally formatted by the game as **+1 gold** where native money formatting is used. |
| Combo `b_faith_money` | **40 / 60 / 80** | Success-only percentage bonuses: Faith **+100 / +150 / +200%**, donations **+100 / +250 / +500%**. No prayer-owned flat Faith/money. |
| Prosperity `b_village` | **10 / 20 / 30** | Keep **1 / 2 / 3 Commercial Blessings**; remove unrelated prayer-owned Faith/money success outputs. |
| Shoots & Roots `b_plant` | **10 / 30 / 50** | Verified repair of the stock scope mismatch; growth time **-20 / -30 / -40%**. The combined growth-time reduction from all sources is safety-capped at **95%**. |
| Repentance `b_sins` | **20 / 40 / 60** | Daily confession probability **50 / 75 / 100%**. Duration **30 / 42 / 54 min**. |
| Repose `b_skull` | **20 / 40 / 95** | Bronze uses the stock-style expanded pool; Silver is halfway toward best-body certainty; Gold guarantees the **maximum total skull count inside the best actually available ordinary corpse tier**. Duration **30 / 42 / 54 min**. |
| Combat `b_sword` | **20 / 40 / 60** | Damage **+5 / +10 / +15**, armor **+4**, regeneration **1 / 2 / 4 HP/s**. |
| Legacy Protection `b_shield` | **20 / 40 / 60** | Existing legacy items act through the accepted Combat mechanics alias. Protection crafting/Technology is retired. Historical item lore is intentionally preserved. |
| Imagination `b_pen` | **20 / 40 / 60** | Writing quality **+0.7** at all tiers; successful Silver gives **3 Silver Stories**, successful Gold gives **3 Gold Stories**. |
| Excellence `b_star` | **20 / 60 / 95** | Linked-craft quality **+0.2 / +0.5 / +1.0**. |
| BSS Soul's Repose `b_souls` | **30 / 60 / 90** | Uses ordinary church-derived base Faith; on success converts stored Soul Gratitude to bonus Faith **1:1**, capped at **30 / 60 / 90** by prayer quality, spending only the amount converted. |
| BSS Soul Contentment `b_grat_points_incr` | **20 / 40 / 60** | **+50% Soul Gratitude** at all qualities; duration **45 / 90 / 135 min**. While active, passive soul-condition deterioration is suspended both in the corpse and after extraction; direct Soul Extractor damage remains vanilla. |
| BSS Thorough Cleansing `b_sin_shard` | **30 / 60 / 120** | Sin Shard output scales **x2 / x3 / x4** through the game's native Sin Shard calculation. |

## Resource-family grammar

### Faith

Faith is the flat specialist:

- Bronze: **+5 Faith**
- Silver: **+10 Faith**
- Gold: **+20 Faith**

The common sermon base remains church-driven. The prayer-owned flat reward does not scale with the church base.

### Donations

Donations is the flat money specialist:

- Bronze: **+20 silver-equivalent**
- Silver: **+50 silver-equivalent**
- Gold: **+100 silver-equivalent = 1 gold**

The common donation base remains graveyard-driven. PrayerClarity uses the game's native money formatter where a denomination is displayed.

### Combo

Combo is the percentage generalist:

- Bronze: **+100% Faith, +100% donations**
- Silver: **+150% Faith, +250% donations**
- Gold: **+200% Faith, +500% donations**

No prayer-owned flat Faith or money is added.

## Better Save Soul endgame structure

### Soul's Repose

The old Soul-Gratitude-dependent base-Faith formula is not the Rebalanced behavior.

Accepted Rebalanced behavior:

1. use the ordinary tier-matched sermon event for the normal church-derived base result;
2. on successful sermon, convert stored Soul Gratitude into bonus Faith at **1:1**;
3. conversion cap is **30 / 60 / 90** for Bronze / Silver / Gold;
4. only the amount actually converted is spent.

The pulpit may show the live pending Soul Gratitude -> Faith transaction because that conversion is an intrinsic prayer mechanic; it still does not reveal the entire final sermon payout.

### Soul Contentment

- requirement: **q20 / q40 / q60**
- Soul Gratitude from releasing souls: **+50%** at all tiers
- duration: **45 / 90 / 135 min**
- passive soul-condition decay is suspended in-corpse and after extraction while active
- direct Soul Extractor damage remains vanilla

### Thorough Cleansing

- requirement: **q30 / q60 / q120**
- Sin Shard multiplier: **x2 / x3 / x4**
- the implementation reuses the verified native Sin Shard output path

## Repose terminal behavior

Direct 1.407 evidence establishes that ordinary corpse progression has no higher ordinary BodyDefinition beyond the final existing tier.

Accepted Rebalanced behavior:

- Bronze preserves the stock-style expanded-pool behavior.
- Silver keeps a reliability improvement toward the best actually available body.
- Gold resolves the highest actually existing ordinary tier and then narrows to BodyDefinitions tied for the maximum game-derived total skull count inside that tier.
- Stock `GameSave.GenerateBody`, candidate enumeration, RNG and body construction remain authoritative.
- Do not hard-code a terminal body ID, tier number, or fixed skull target.

At a terminal progression state, Bronze may correctly report that no still-higher corpse quality can be unlocked. Silver/Gold can still change **how reliably the best currently available body is selected**. The exact beginner-facing wording of that reliability note is a separate UX wording question after 0.2.38; mechanics are closed.

## Shoots & Roots cap presentation

The **95%** value is a combined safety cap across the prayer plus other growth-time reduction sources. It is not the prayer's own tier magnitude.

Accepted presentation in 0.2.38:

- Bronze/Silver active effects show only their own **-20% / -30%** prayer reduction.
- Gold active effect additionally states the **95% combined all-sources cap**, because Gold plus the accepted fertilizer interaction can naturally approach that boundary.
- The mechanical 95% cap applies regardless of which tier is active.

## Runtime / performance shape

- No broad Unity scans or background polling.
- Static prayer-definition changes are projected at the verified load/reset boundary.
- Tier-dependent runtime effects use the accepted narrow host/native seams documented in project evidence.
- Repose corpse narrowing is scoped only to the verified ordinary body-generation call and restores the original catalog reference afterward.
- PrayerClarity-owned persistent tier tokens have no gameplay effect without the corresponding live native prayer buff.
- New player-facing strings support all 11 project locales.

## Acceptance history relevant to the current roster

- **0.2.48** accepted the final Combo Prayer donation ladder **+100 / +250 / +500%** while preserving Faith **+100 / +150 / +200%**, q40/q60/q80 and every other prayer balance value.
- **0.2.49–0.2.51** changed shared Technology presentation only. 0.2.51 is the accepted stable alignment model: title / Base Result / On Success centered consistently with mouse and gamepad; mechanics/body rows remain left-aligned.

- **0.2.0–0.2.4** established the initial Rebalanced roster, repaired Roots/Repentance/Combat ownership, and accepted 30/42/54-minute Repentance/Repose durations.
- **0.2.13–0.2.16** established specialist purity, Combo/resource-family grammar, Repose Gold maximum-skull behavior, and q95 premium Gold gates.
- **0.2.17–0.2.24** established the current Donations and Better Save Soul mechanics and closed their live pulpit/Technology presentation paths.
- **0.2.23** generalized the accepted controller carousel to prayer-bearing Technologies.
- **0.2.25–0.2.32** closed selected-unlock clearance and prayer-item/localization presentation defects.
- **0.2.36–0.2.37** accepted native-anchor-driven long-text pulpit growth and current-language font refresh after live language switching.
- **0.2.38** closes the Gold-only Roots cap wording, native Gold denomination for Donations at the pulpit, and parenthetical Repose reliability-note presentation.

Current architecture/save-lifecycle status remains **A — no architecture action** per `POST_AUDIT_VERDICT.md`.

Accepted runtime closure, 2026-09-26:
- real successful Silver and Gold Imagination sermons were observed delivering `story:2 ×3` and `story:3 ×3` respectively. The physical three-Story payout path is therefore closed and no longer an evidence gap.

Any future behavior change requires a new version; do not silently replace the accepted 0.2.51 bytes.
