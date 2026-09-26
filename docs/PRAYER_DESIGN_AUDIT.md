# Prayer Design Audit — Graveyard Keeper 1.407

Status: **current prayer-by-prayer design source of truth**, reconciled 2026-09-26 with PrayerClarity: Vanilla 1.0.33 and stable PrayerClarity: Rebalanced 0.2.47.

Stock mechanics remain canonical in `PRAYER_MECHANICS.md`. Exact current Rebalanced values are canonical in `PRAYER_REBALANCE_OPTIONS.md`. Historical alternatives and earlier coefficient experiments are retained in `PRAYER_POWER_BUDGET.md` and Git history as analysis only.

## Current accepted baselines

- **PrayerClarity: Vanilla 1.0.33** — `accepted/vanilla-1.0.33`, exact source `93b66e747ffe1685003afb894b24f14416edb8c0`, release `v1.0.33`.
- **PrayerClarity: Rebalanced 0.2.47** — `accepted/rebalanced-0.2.47`, exact runtime source `6b3aa5399c8913d368f2b09bab963326db17e7f3`, release `rebalanced-v0.2.47`.

The Rebalanced roster values were established in 0.2.0. Releases 0.2.2 and 0.2.3 changed runtime safety/ownership, 0.2.4 made the accepted Repentance/Repose duration adjustment to 30/42/54 minutes, 0.2.10 raised ordinary Repose Gold from q50 to q60, and 0.2.16 finalized the current q95 premium Gold gate while preserving the accepted effect. The accepted Roots aggregate safety cap remains in force. Rebalanced 0.2.39–0.2.47 changed presentation only and did not alter the accepted roster or balance values.

## Audit rules

- specialization must create a real reason to spend the weekly sermon on the specialist;
- Bronze must already be credible;
- Silver/Gold must justify their higher writing/quality burden;
- a narrow prayer may be deliberately strong in its niche;
- duration is real power only when the underlying activity can exploit the window;
- natural progression obsolescence is acceptable;
- healthy stock behavior should not be nerfed for symmetry;
- Church Quality requirements follow the whole proposition, not a universal ladder;
- failed sermons retain the full vanilla base donation pool by permanent project policy;
- base Faith/donations are distinct from prayer-owned success bonuses;
- gameplay and every UI surface must consume the same effective semantic/rule source.

## Current design matrix

| Prayer/family | Current verdict | Accepted design |
| --- | --- | --- |
| Ordinary | **No change** | Stock starter baseline, q10. |
| Faith | **Accepted specialist rework** | q20/40/60; flat success-only **+5/+10/+20 Faith**; no prayer-owned donation bonus. |
| Donations | **Accepted specialist rework** | q20/40/60; flat success-only **+20/+50/+100 silver-equivalent**; no prayer-owned Faith bonus. |
| Combo | **Accepted generalist rework** | q40/60/80; stable 0.2.47 success-only Faith **+100/+150/+200%**, donations **+100/+200/+300%**; no prayer-owned flat Faith/money. |
| Prosperity | **No balance change** | Stock q10/20/30 and 1/2/3 Commercial Blessings. |
| Shoots & Roots | **Accepted repair + scaling** | q10/30/50; nominal growth time **-20/-30/-40%**, 36/72/108 min; current runtime also enforces the accepted 95% combined reduction safety cap. |
| Repentance | **Accepted rework** | q20/40/60; daily confession probability **50/75/100%**; duration **30/42/54 min**. |
| Repose | **Accepted reliability rework** | q20/40/95; Bronze stock-style pool, Silver remains halfway toward certainty, Gold guarantees the maximum total skull count inside the best actually available ordinary corpse tier; duration **30/42/54 min**. |
| Combat | **Accepted structural/numeric rework** | q20/40/60; damage **+5/+10/+15**, armor **+4**, regen **1/2/4 HP/s**; 36/72/108 min. |
| Imagination | **Accepted premium-output rework** | q20/40/60; writing quality **+0.7** all tiers; successful Silver -> 3 Silver Stories, Gold -> 3 Gold Stories; 18/36/54 min. |
| Excellence | **Accepted magnitude rework** | q20/60/95; linked-craft quality **+0.2/+0.5/+1.0**; 18/36/54 min. |
| BSS Soul's Repose | **Accepted conversion specialist** | q30/60/90; ordinary church-derived base result plus success-only **1:1 Soul Gratitude -> Faith** conversion capped at **30/60/90**. |
| Soul Contentment | **Accepted rework** | q20/40/60; **+50% Soul Gratitude** all tiers; 45/90/135 min; passive soul-condition decay suspended while active. |
| Thorough Cleansing | **Accepted scaling rework** | q30/60/120; **x2/x3/x4 Sin Shards** through the native output path. |

## Faith / Donations / Combo family

Faith and Donations are flat specialists: Faith gives +5/+10/+20 Faith, Donations gives +20/+50/+100 silver-equivalent, and both guarantee at q20/q40/q60.

Combo is a percentage generalist: stable 0.2.47 gives Faith +100/+150/+200% and donations +100/+200/+300%, guaranteed at q40/q60/q80.

This creates the intended progression:

`early focused specialists -> mature specialists / early Combo -> late premium Combo`

The specialists retain early value because their prayer-owned reward does not depend on a large church/graveyard base. Combo becomes increasingly attractive as the underlying sermon economy matures, but pays the higher Church Quality gate and Hard Book recipe.

Do not restore the obsolete +200/+300/+400% specialist ladder or q25/40/70 requirements from the early audit. Those values are historical only.

Pending Rebalanced 0.2.48 candidate: keep Faith at +100/+150/+200% and change Combo donations to **+100/+250/+500%**. This candidate is product-approved but is not part of the accepted stable roster until focused visual acceptance and promotion.

## BSS Soul's Repose

Verified base Faith is:

`Base Faith = (Church Quality + current Soul Gratitude) * 0.1 * EloquenceFactor`

Current Soul Gratitude means the amount held at sermon time, not lifetime healed souls and not capacity.

The accepted q30/60/120 ladder gates a +50/+100/+150% prayer-owned Faith bonus. Gold q120 is intentionally aspirational late-game power. This prayer remains state-dependent rather than being normalized to the ordinary Faith specialist.

## Repentance

The accepted design repairs an otherwise disconnected stock prayer by reusing the game's daily confession scheduler and two-confessional loop.

Current probability ladder is 50% / 75% / 100%. Stable 0.2.14 duration is **30/42/54 minutes**.

Because the effect acts only on once-per-game-day confession rolls, duration must be judged by how many future daily rolls actually occur while the buff is live, not just by the probability itself. The accepted 0.2.4 revision raises Bronze enough to cover about four stock-day opportunities, moves Silver to a distinct intermediate window, and leaves Gold unchanged.

## Imagination

The healthy stock core remains +0.7 writing quality at all tiers.

Premium tiers add a bounded success reward:

- Bronze: no extra Story;
- Silver: 3 Silver Stories on sermon success;
- Gold: 3 Gold Stories on sermon success.

This is a weekly premium output layered onto a writing-planning buff, not intended to replace the broader Story economy. Any future change should compare the three-Story grant against confessionals, player/zombie Story crafting, Journalist/dialogue sources, Better Save Soul Story crafting where available, and finite NPC exchanges.

## No global q normalization

Different prayers pay different unlock, recipe, opportunity-cost and activity-window costs. Their q ladders are deliberately non-uniform. Uniform q would be visual symmetry, not better balance.

## Role-collision check

The current roster keeps distinct jobs:

- Faith / Donations / Combo: focused flat specialists versus scaling generalist;
- Repentance: confession throughput and delayed church-side Faith/Story generation;
- Roots: farming/growth acceleration;
- Repose: corpse-progression reliability;
- Combat: one coherent combat-preparation package;
- Imagination: writing-quality planning plus small premium Story injection;
- Excellence: broader linked-craft quality;
- Prosperity: permanent merchant progression that can naturally become obsolete;
- Soul Contentment / Thorough Cleansing: narrow Better Save Soul workflow accelerators.

A prayer should be reopened when new evidence shows that this role is not actually tempting, materially overlaps another system, or creates a power-budget problem.

## Current architecture consequence

Current stable 0.2.38 keeps Graveyard Keeper authoritative where practical:

- Roots leaves stock growth formulas intact and projects only the native input, with the accepted 95% aggregate cap;
- Repentance leaves the stock daily reset/RNG/loop intact and projects only the effective `confession_probability` read while the native buff is live;
- Combat damage/armor use scoped native `add_damage` / `add_armor` inputs; regeneration uses the native buff tick extension point;
- Repose narrows only the verified ordinary Donkey generation context and still calls stock `GenerateBody`;
- Excellence uses the exact stock `GetBuffValue("buff_star")` semantic seam;
- Soul Contentment retains its accepted narrow live-graph coefficient projection.

`REBALANCED_NATIVE_SEAM_AUDIT.md` and `POST_AUDIT_VERDICT.md` are authoritative for implementation/lifecycle closure.

## Current status

The stable balance/architecture baseline is **Rebalanced 0.2.38**. The accepted 0.2.17+ Better Save Soul/resource changes and the presentation/runtime refinements through 0.2.38 are now canonical; there is no blanket rebalance or architecture task pending.

Known non-blocking evidence gaps remain:

- terminal Repose endpoint wording/presentation has not been observed on the user's terminal-progression save state;
- the physical 3 Silver / 3 Gold Story drop from a real successful premium Imagination sermon has not yet been visually observed, although it uses the verified native sermon-drop path.

Future changes should start from a concrete gameplay/UX finding and create a new explicit proposal rather than reviving superseded historical values.


## 2026-09-20 specialist-purity audit

Status: **design hypothesis; no production change yet**.

### Direct current-state fact

Historical 0.2.10 state before the accepted 0.2.13 specialist-purity cleanup:

| Family | Stock/inherited Faith rider | Stock/inherited donation rider | Current specialist role |
| --- | ---: | ---: | --- |
| Repentance `b_sins` | +25 / +50 / +75% | +10% | confession probability 50 / 75 / 100%, 30 / 42 / 54 min |
| Shoots & Roots `b_plant` | +25% | +25% | growth time -20 / -30 / -40%, 36 / 72 / 108 min |
| Repose `b_skull` | +10% | +25 / +50 / +75% | corpse-tier reliability, 30 / 42 / 54 min |
| Combat `b_sword` | +25% | +25% | damage + armor + regeneration, 36 / 72 / 108 min |
| Imagination `b_pen` | +25 / +50 / +75% | +10% | +0.7 writing quality; premium tiers add 3 Stories |
| Excellence `b_star` | +25 / +50 / +75% | +10% | linked-craft quality +0.2 / +0.5 / +1.0 |
| Prosperity `b_village` | +25% | +25% | 1 / 2 / 3 Commercial Blessings |
| Soul Contentment `b_grat_points_incr` | +25% | +25% | +20% Soul Gratitude, 36 / 72 / 108 min |
| Thorough Cleansing `b_sin_shard` | +25% | +25% | x2 Sin Shards, 36 / 72 / 108 min |

This mapping is direct project evidence from the verified stock mechanics catalogue. Rebalanced static projection changes `k_faith` / `k_money` only when the corresponding rule provides an override; the nine families above currently do not.

Stock sermon data also carries small fixed success outputs, most visibly `Faith x1/x2/x3` on utility prayers; Prosperity additionally carries 1/2/3 silver. Before production removal, exact fixed-output rows should be re-verified family-by-family from current 1.407 data, but they are the same semantic cleanup target rather than a separate role.

### Proposed classification

| Family | Generic rider action | New compensation? | Reason |
| --- | --- | --- | --- |
| Repose | **remove** off-role Faith/donation riders and fixed Faith | **none** | accepted reliability rework + q60 Gold + 30/42/54 min already define the value proposition |
| Repentance | **remove** | **none** | the specialist effect itself creates future confession Faith/Stories; generic sermon Faith is redundant |
| Shoots & Roots | **remove** | **none** | accepted -20/-30/-40% scaling and 36/72/108 min are already the quality proposition |
| Combat | **remove** | **none** | accepted merged damage/armor/regen package is deliberately strong in its niche |
| Imagination | **remove** | **none** | +0.7 quality plus the accepted Silver/Gold 3-Story premium already replaced generic power with specialist power |
| Excellence | **remove** | **none** | accepted +0.2/+0.5/+1.0 magnitude ladder is the premium |
| Prosperity | **remove** | **none** | Blessings permanently advance vendor tiers; natural obsolescence after that job is complete is accepted |
| Soul Contentment | **remove** | **none initially** | Rebalanced already doubled the stock Gratitude magnitude to +20%; duration is the quality axis |
| Thorough Cleansing | **remove** | **none** | x2 Sin Shards is already a strong narrow effect |

### Explicit exceptions

Do **not** apply specialist-purity cleanup to:
- Faith — resource output is the role;
- Donations — resource output is the role;
- Combo — resource output is the role;
- BSS Soul's Repose — its +50/+100/+150% Faith is the role;
- Casual Prayer — stock starter baseline.

### Design consequence

If accepted, successful utility sermons still provide the universal Base result. Success remains important because it gates the named buff/reward. What disappears is only the unrelated prayer-owned resource garnish.

This is expected to:
- make “Bonuses on success” semantically immediate;
- reduce tooltip lines;
- strengthen prayer identity;
- reduce incidental power creep introduced by keeping stock generic riders after strengthening the specialist effects;
- make Faith/Donations/Combo visibly own the resource-generation roles.

No production implementation should begin until this rule and the fixed-output cleanup scope are accepted.


### 2026-09-20 accepted follow-up — specialist purity

User decision:
- accept the specialist-purity direction for Rebalanced;
- utility/specialist prayers should lose unrelated prayer-owned Faith/donation percentage riders;
- unrelated fixed Faith/money success outputs should also be removed where present;
- do this **without compensating buffs** unless a prayer later proves weak in its own specialization;
- resource specialists remain exceptions because resource generation is their actual job.

Accepted Donations ladder for the next Rebalanced candidate:
- Bronze: **+5 silver**;
- Silver: **+15 silver**;
- Gold: **+30 silver**.

Accepted Combo follow-up for the next Rebalanced candidate:
- Faith bonus remains **+100 / +150 / +200%**;
- donation bonus becomes **+100 / +200 / +300%**;
- Bronze therefore stays unchanged, Silver donation scaling rises from +150% to +200%, and Gold rises from +200% to +300%;
- successful Gold Combo pays **4x base donations** in total. Against Gold Donations (`base + 30s`), the pure-money crossover is base donations = 10s: about Graveyard Quality 250 with Cardinal or 333 without Cardinal.

This keeps Gold Combo a late scaling generalist without making Gold Donations obsolete as soon as q80 becomes available.


## Rebalanced 0.2.11 candidate specification

Status: **implemented on development branch; runtime acceptance still required**.

Candidate resource family:
- Молитва веры: unchanged, +5 / +10 / +20 Faith;
- Молитва о пожертвованиях: **+5 / +15 / +30 silver**;
- Комбо-молитва Faith: **+100 / +150 / +200%**;
- Комбо-молитва donations: **+100 / +200 / +300%**;
- Молитва за упокой душ: unchanged +50 / +100 / +150% Faith on the Souls baseline.

Candidate specialist-purity rule:
- Покаяние, Корни и побеги, Упокоение, Combat, Воображение, Совершенство, Процветание, Довольство душ и Тщательное очищение explicitly set prayer-owned Faith/donation percentage bonuses to zero;
- those families also remove prayer-owned fixed Faith/money outputs if present;
- physical specialist rewards remain untouched (for example Commercial Blessings and Imagination's Story rewards);
- the retired Protection alias is cleaned the same way as canonical Combat so old/legacy prayer items cannot retain the removed generic resource garnish.

Implementation uses the already accepted load-time CraftDefinition projection:
- `k_faith` / `k_money` are set directly on the prayer craft data;
- only output rows whose item IDs are exactly `faith` or `money` are removed;
- specialist item drops and buff/effect data are not rewritten.

Runtime acceptance should verify presentation/state rather than repeat the whole sermon-mechanics audit:
1. one ordinary specialist (preferably Упокоение) shows its named effect immediately under “Бонусы при успехе”, with no generic Faith/donation row;
2. Процветание still shows 1/2/3 Commercial Blessings but no unrelated Faith/money reward;
3. Пожертвования shows +5/+15/+30 silver;
4. Комбо shows Faith +100/+150/+200% and donations +100/+200/+300%;
5. if convenient, one BSS utility prayer shows only its named specialist effect.

A real sermon payout retest is required only if those projected values disagree with the UI or a runtime error appears, because the accepted stock payout path consumes these same CraftDefinition fields/output rows directly.


## 2026-09-20 0.2.11 runtime result — Technology regression

Status: **0.2.11 mechanics/balance changes passed the requested visual-value checks, but the candidate is not accepted because its Technology prayer presentation regressed.**

User runtime evidence confirmed:
- Donations shows the accepted +5 / +15 / +30 silver ladder;
- Combo shows Faith +100 / +150 / +200% and donations +100 / +200 / +300%;
- generic Faith/donation garnish disappeared from specialist prayers as intended;
- prayer-item tooltips, pulpit/HUD and Character -> Temporary Effects remained correct.

New Technology-only regression:
- after specialist cleanup, stock Technology no longer emits the vanilla `preach_params_2` success-bonus header/body for prayers whose `k_faith`, `k_money` and fixed Faith/money outputs are all zero/absent;
- the existing Clarity replacement seam used that header as its anchor, so it fell back to appending Clarity sections at the end;
- the stock `preach_params` requirement sentence therefore reappeared before prayer lore, while the stock crafting-location footer remained before the appended Clarity sections;
- this is why the UI showed e.g. the old “20–60 required to guarantee success” sentence and moved “Crafted at” above Base result.

The stock `TechUnlock.GetTooltip` construction order independently matches the runtime symptom: the requirement+lore row is always built for a multi-quality prayer, while the `preach_params_2` block is conditional on there being a non-empty generic Faith/money contribution.

Soul Contentment exposed a second stale-stock-text boundary: its stock description embeds **+10%** directly, while Rebalanced's accepted effective value is **+20%**. Rebalanced Technology must not retain that stock numeric sentence alongside the current effect.

### 0.2.12 fix rule

Use the always-present stock prayer requirement/lore row as the fallback Technology anchor when `preach_params_2` is absent:
- strip the stock requirement sentence;
- insert PrayerClarity Base result / Bonuses on success immediately after the lore row, which naturally keeps the stock crafting-location footer last;
- for Rebalanced Soul Contentment only, suppress the obsolete stock +10% lore/mechanics row and let the effective +20% Rebalanced effect be the single mechanics statement.

This is presentation-only. It must not change 0.2.11 prayer rules, payout fields, specialist effects, timers, stacking or other accepted surfaces.


## 2026-09-20 0.2.12 runtime result — presentation repaired

The focused Technology retest passed. The 0.2.11 layout regression is closed: cleaned specialist prayers again show the intended Clarity hierarchy, the stock requirement sentence stays removed, and the crafting-location footer stays last.

One small wording defect remains in Russian Soul Contentment:
- effective mechanic is correct at +20% Soul Gratitude;
- current Rebalanced Russian text says only `+20% благодарности`;
- the project already uses the game's `(gratitude_points)` icon token on nearby Soul Gratitude surfaces;
- preferred next wording is the compact icon form, e.g. `За исцеление души: +20% (gratitude_points)`.

BSS Soul's Repose also produced a useful clarity observation. Its tier bonus (+50/+100/+150% Faith) applies to the Souls-specific Faith base:
`(Church Quality + current Soul Gratitude) * 0.1 * EloquenceFactor`.
Therefore its smaller-looking percentage can beat Combo on Faith once Soul Gratitude is material. The Base result already exposes the enlarged base; if further clarification is desired, prefer replacing the currently redundant shared effect sentence with a one-line explanation that the tier Faith bonus is calculated from that base. Do not repeat the full formula or add another tooltip block.


## 2026-09-20 Soul wording refinement — 0.2.13

Accepted wording direction after the 0.2.12 visual pass:

- **Soul Contentment**: keep the correct +20% mechanic, but render the affected resource with the native `(gratitude_points)` icon instead of the ambiguous generic noun “gratitude”.
- **BSS Soul's Repose**: do not use the generic wording “the Faith bonus is calculated from this base”. That statement is formally true but does not explain what is special about this prayer.

Preferred concise explanation:

`Чем больше (gratitude_points), тем больше бонус веры.`

This directly explains the player-relevant consequence of the verified formula: Soul Gratitude enlarges the Faith baseline, so the absolute value of the +50/+100/+150% tier bonus rises with current Soul Gratitude. The full formula remains documented in mechanics docs and is not repeated in the tooltip.
