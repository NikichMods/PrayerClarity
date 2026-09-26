# PrayerClarity — Design Notes

Status: product/architecture source of truth, reconciled 2026-09-27 with the accepted stable releases PrayerClarity: Vanilla 1.0.33 and PrayerClarity: Rebalanced 0.2.47.

Detailed evidence and history live in specialized documents rather than being duplicated here:

- `PRAYER_MECHANICS.md` — stock Graveyard Keeper 1.407 mechanics;
- `PLAYER_UX_RESEARCH.md` — player/presentation evidence;
- `PRAYER_DESIGN_AUDIT.md` — current prayer-by-prayer role/design verdicts;
- `PRAYER_POWER_BUDGET.md` — quantitative stock unlock/craft/quality/opportunity-cost analysis;
- `PRAYER_REBALANCE_OPTIONS.md` — locked Rebalanced design specification;
- `PULPIT_REVEAL_UX.md` — accepted pre-sermon reward-reveal boundary;
- `TECHNOLOGY_TOOLTIP_UX.md` and current candidate/runtime docs — detailed Technology UX evolution;
- `CLARITY_UI_STATUS.md` and `RUNTIME_1.0.20_RESULT.md` — accepted Clarity/runtime evidence.

Do not use historical candidate scope files or older balance examples as the current Rebalanced roster when they conflict with `PRAYER_REBALANCE_OPTIONS.md`.

## Product family

Accepted public naming:

1. **PrayerClarity: Vanilla** — complete Clarity presentation over stock Graveyard Keeper 1.407 prayer/sermon mechanics and balance.
2. **PrayerClarity: Rebalanced** — sibling edition containing the same Clarity experience plus the intentional locked Rebalanced ruleset.

They are peer alternatives in one PrayerClarity family, not base mod + upgrade/add-on. A player should install one edition or the other.

`PrayerClarity` remains the repository/codebase family name. Public edition subtitles do not force repository, namespace, plugin GUID or DLL naming decisions.

Internal evidence/design layers remain distinct:

- **Clarity** — information/presentation only;
- **Vanilla Fixes** — evidence-backed repair where intended stock mechanic/magnitude is recoverable;
- **Balance / Rework** — deliberate new design/tuning.

**Vanilla Fixes is not an accepted third public edition.** PrayerClarity: Vanilla explicitly promises stock gameplay behavior, so fixes must not be silently folded into it. PrayerClarity: Rebalanced may incorporate a proven Vanilla Fix where the locked Rebalanced design requires the repaired mechanic, while documentation must still distinguish repair from invented tuning.

## Accepted baseline

Current accepted stable runtime identities are edition-specific:

- **PrayerClarity: Vanilla 1.0.33** — accepted ref `accepted/vanilla-1.0.33`, exact source `93b66e747ffe1685003afb894b24f14416edb8c0`, release `v1.0.33`, DLL SHA-256 `30b23f9ed62148f3fd08e0c34ae54f165da0e639a041d1e9d7c4abe74268da8a`.
- **PrayerClarity: Rebalanced 0.2.47** — accepted ref `accepted/rebalanced-0.2.47`, exact runtime source `6b3aa5399c8913d368f2b09bab963326db17e7f3`, release `rebalanced-v0.2.47`, DLL SHA-256 `2482a159c3108a7868123d7c2cac7a8a620fe537952fee0cda6024c6ea5c7c96`.

Both stable releases were published from their exact accepted CI artifacts without rebuilding. Later `main` documentation/repository-hygiene commits do not redefine those frozen runtime identities.

## Permanent gameplay/product policies

- Preserve full **base donations on failed sermons**. This is an explicit PrayerClarity product decision. Prayer-specific bonuses/special success outputs may still be lost according to verified mechanics.
- Keep stock 1.407 behavior documented independently of Rebalanced behavior.
- Do not present intentional Rebalanced values as restored developer intent.
- Use temptation parity: unlock/craft/quality/success/weekly opportunity cost all matter.
- Bronze must already be credible. Silver/Gold should buy meaningful magnitude, reliability, duration, output or certainty.
- Do not rebalance for visual/numerical symmetry.
- Prefer strengthening alternatives over nerfing familiar healthy rewards unless evidence justifies a nerf.
- Natural obsolescence is acceptable for progression prayers that have completed their role.

## Shared semantic architecture

The core architectural requirement for both editions is **one effective prayer semantic model**.

The model must be side-effect free for presentation and must describe, where relevant:

- prayer/craft/event identity;
- quality tier;
- success requirement and probability inputs;
- Faith and donation percentage modifiers;
- fixed prayer-owned success outputs;
- special effect magnitude/probability/reliability;
- duration;
- discrete item rewards;
- relevant dependencies such as Church Quality, Graveyard Quality or Soul Gratitude.

The four accepted player-facing surfaces consume context-appropriate projections of that same model:

1. Pulpit;
2. Technology tooltip;
3. prayer item tooltip;
4. Character -> Temporary Effects.

PrayerClarity: Vanilla resolves effective semantics to stock 1.407 values. PrayerClarity: Rebalanced resolves them to the locked ruleset. Do not maintain a second UI-only table of Rebalanced numbers beside separate gameplay patches.

The mechanics layer may require narrow hooks for behavior that is not representable merely by changing stock data. Those hooks must still consult the same effective definitions/quality semantics so gameplay and UI cannot drift.

## Shared prayer information-design contract

This presentation contract applies to **both PrayerClarity: Vanilla and PrayerClarity: Rebalanced**. Edition-specific mechanics and values differ; the information grammar should not fork unless evidence requires it.

Primary rule:

> Describe the in-world effect once in natural player-facing language; show only the quality-dependent delta inside each Bronze/Silver/Gold block.

Practical rules:

- write from the player's/game-world perspective first, then expose the exact number that controls the mechanic;
- prefer concrete game actions and objects (`the donkey brings a corpse`, `a confession can occur`, `plants take less time to grow`) over implementation language (`roll`, `scheduler`, `range narrowing`) unless the technical term is itself player-facing;
- shared/invariant behavior appears once above the quality tiers;
- each tier contains only materially changing information plus its 100%-success Church Quality requirement;
- short label/value mechanics are atomic clauses and should not wrap between the label and its value (`Craft quality: +0.5`, `Confession chance: 75%`, `Plant growth time: -30%`);
- preserve semantic hierarchy with spacing before adding more prose: shared effect -> tier snapshots -> crafting source;
- use native icons where they reduce explanation cost, but do not let an icon become the sole carrier of essential information;
- use color as a secondary cue, never as the only source of meaning;
- special named entities keep the same accent everywhere they recur in one tooltip (for example `Commercial Blessing` in the shared explanation and tier rewards);
- Bronze/Silver/Gold color may emphasize a **key tier value** when that value itself is a direct quality ladder; color only the value, not the entire sentence;
- do not color durations merely because they vary; duration is normally secondary and should remain a compact readable line;
- avoid decorative color proliferation: structural/entity accents and quality-linked value accents must have a defined semantic role;
- prefer exact but natural wording over technically exhaustive prose. Do not add fallback/else-case explanations when they do not materially improve the player's decision;
- when a mechanic cannot be truthfully expressed as one fixed probability, do not invent a percentage merely to make the tier ladder visually symmetric.

The intended scan path is:

`what happens -> what this quality changes -> how long / what extra reward -> where it is crafted`

This contract governs presentation structure, not mechanics. PrayerClarity: Vanilla must continue to describe stock 1.407 behavior honestly; PrayerClarity: Rebalanced may describe repaired/reworked behavior from its effective ruleset.

## Pulpit presentation contract

The accepted pulpit model is:

`base/guaranteed dependency -> success-only prayer contribution -> special effect`

Before the sermon, explain what drives the reward and what the selected prayer changes. Preserve the sermon animation as the reveal moment for the exact final Faith/donation totals.

Default pulpit presentation therefore keeps:

- current relevant context such as Church Quality and Graveyard Quality;
- exact sermon success probability;
- dependency relationships for base Faith/donations and verified special inputs such as Soul Gratitude;
- exact prayer-owned percentage/fixed modifiers;
- exact intrinsic mechanics such as duration, growth reduction, combat stats, regeneration, confession chance, resource multiplier or reward quantity.

It does **not** expose the fully resolved current final Faith/donation payout merely because the semantic model can calculate it.

See `PULPIT_REVEAL_UX.md` for the detailed rationale.

### Accepted pulpit layout/language behavior

For the shared pulpit presentation layer, the accepted runtime behavior is:

- long localized `Effect:` text may increase the real pulpit root-window height only when the final wrapped content would otherwise violate the action-button clearance;
- short prayers must remain compact rather than inheriting worst-case fixed height;
- the native Pray GUI action-button anchors remain authoritative; PrayerClarity must change the verified upstream window geometry rather than repeatedly forcing the button transform;
- PrayerClarity-owned raw `UILabel` forecast widgets must use the game's current-language font owner on redraw, so live language switching cannot leave stale CJK/Latin/Cyrillic font bindings or metrics;
- these layout/font rules are presentation-only and must not change prayer mechanics, values, wording, localization content, or sermon outcome semantics.

Runtime acceptance: Rebalanced 0.2.37 on Russian, Japanese, English and Korean pulpit views, including long Soul's Repose and short Faith control.


## Technology tooltip contract

Technology is the comparison/planning surface.

Accepted general grammar is:

`shared prayer information -> compact Bronze/Silver/Gold tier snapshots`

- shared/invariant behavior appears once;
- each tier shows the native quality marker, Church Quality required for 100% success, and only values that materially vary by prayer quality;
- percentage and flat contributions remain semantically distinguishable;
- long special effects use content-driven wrapping instead of forcing every tooltip to the width of the longest string;
- Effect duration is semantically separated from Faith/Donations bonus information;
- the `Crafted at` footer remains visually separated from the last tier;
- vanilla lore comes through the verified base `b_*_d` localization seam rather than punctuation-sensitive parsing.
- preserve prayer lore when it exists; if Rebalanced makes one mechanic clause or numeric value stale, replace or neutralize only that conflicting mechanic text and keep the non-conflicting lore rather than suppressing the whole paragraph.

Rebalanced effects should fit this data-driven structure. Do not add bespoke layout code per prayer unless runtime evidence proves the generic semantic structure insufficient.

### Controller navigation inside prayer-bearing Technologies

For gamepad/controller presentation, a Technology node that exposes multiple visible unlocks and contains at least one prayer should behave as a short horizontal sequence of its **existing visible child unlocks**, rather than concatenating every child tooltip into one long parent tooltip.

Accepted interaction model from the Better Save Soul prototype/runtime pass:

- the real Technology node remains the only navigation/progression object;
- one child unlock is visually selected at a time; non-selected child icons are dimmed;
- the parent tooltip is rebuilt from the selected existing `TechUnlock.GetTooltip`;
- Left/Right walks child unlocks in their native visible order;
- at the first/last child, the next Left/Right input falls through to ordinary Technology-tree navigation;
- horizontal entry chooses the corresponding boundary child, so movement remains spatially reversible;
- Up/Down remains ordinary tree navigation;
- no child focus object, duplicate Technology, save state, or custom unlock state is created.

Scope policy: apply this only to **prayer-bearing** Technologies, not to the entire Technology tree. If such a node also contains non-prayer perks/buildings/recipes, those visible children participate in the same sequence so each icon has exactly one matching tooltip while the node is active.

The activation rule should be data-driven from the visible `TechUnlock` list and the existing prayer-craft resolver, not a hard-coded Technology-name list. Mouse presentation stays native because stock 1.407 already gives child unlocks independent tooltips there.

This is shared presentation behavior for PrayerClarity: Vanilla and PrayerClarity: Rebalanced; it changes no mechanics or balance.

## Prayer item tooltip contract

A prayer item tooltip describes the **concrete quality currently held**, not the whole Bronze/Silver/Gold comparison.

It should continue to use the same effective semantics as Technology and Pulpit so a Rebalanced item never reports stock mechanics.

## Temporary Effects contract

Active prayer effects show their concrete quantitative meaning and remaining duration. Long remaining durations are expressed in in-game days using the game's effective day length; the precise stock timer returns inside the final day.

Rebalanced timed buffs must feed their actual effective magnitude through this same surface. Do not infer active values from localized strings.

## Localization

All PrayerClarity-owned player-facing strings must support the same 11 interface languages as the accepted Vanilla edition:

`en`, `fr`, `de`, `zh_cn`, `es`, `pt_br`, `ko`, `ja`, `ru`, `it`, `pl`.

Rules:

- follow the game's active language;
- reuse vanilla terminology/localization where practical;
- keep dynamic values separate from translatable prose;
- English is the safe fallback;
- no polling for language changes;
- new Rebalanced player-facing text is not localization-complete if it exists only in English/Russian.

## Runtime/performance architecture

- Keep UI work at existing UI lifecycle/redraw seams.
- Prefer native getters/state over mirrored runtime state.
- No broad Unity scans or background polling in production.
- Cache unavoidable reflection/compatibility bindings.
- Mechanics changes should hook the narrow semantic event that owns the behavior rather than broad `Update()` loops.
- Diagnostic probes remain narrow, removable research artifacts.

## Rebalanced roster status

The design roster is **locked**. `PRAYER_REBALANCE_OPTIONS.md` is the only canonical concrete table of current Rebalanced values.

The old intermediate candidates formerly recorded in this file — including earlier Faith/Donations coefficients, earlier Repentance probabilities, earlier Combat regeneration/damage, earlier Imagination curves and speculative BSS magnitude ladders — are superseded and must not be implemented.

No new balance round is required unless implementation evidence contradicts an assumption that materially affects the locked behavior.

## Current engineering state

The current **public stable Rebalanced release** is **0.2.47**. The current **accepted Rebalanced development/runtime baseline** is **0.2.48**, frozen at `accepted/rebalanced-0.2.48` / `42b284986f768b6dd1e1760f7d80c8e5763765b2`, with DLL SHA-256 `38d51a9647d67b1b14f70a9045b8bbf493dae4b434251284bbab22c2bf1f10e3`.

0.2.47 carries forward the closed gameplay/save-lifecycle architecture from `POST_AUDIT_VERDICT.md` (**A — no architecture action**) and preserves the accepted Rebalanced roster/balance from 0.2.38. The 0.2.39–0.2.47 line is presentation/clarity work only.

### Current actionable UX / presentation backlog

The Repose and prayer-item presentation work through 0.2.47 is **closed and runtime-accepted**:

- Repose now uses one consistent player-facing model across Technology, prayer items, ordinary/terminal pulpit states and Character -> Temporary Effects.
- The prayer-item parchment uses the game's standard/native width; the rejected fixed-200, ResizeFreely/420 and fixed-280 experiments are historical only.
- The 100%-success requirement content is centered so wrapped Church Quality + cross remains coherent.
- Base Result and On Success content are visibly left-aligned within the existing native tooltip span, while section headers and the requirement remain centered.
- The accepted amount + inline-resource wrap repair remains scoped to PrayerClarity-owned prayer-item mechanics rows.

No further in-game retest is required for these accepted 0.2.47 properties unless their implementation changes.

### Current Technology presentation candidate

- **Prayer Technology alignment unification — Rebalanced 0.2.49 / Vanilla sibling 1.0.55 pending focused visual acceptance.** The shared prayer-Technology path now centers the top `Create: Prayer...` title plus `Base Result` and `On Success` headers while leaving mechanics content left-aligned. The change is data-level at `TechUnlock.GetTooltip`, so the same hierarchy applies to mouse, stock gamepad Technology assembly and PrayerClarity's gamepad prayer carousel without a second layout mechanism. Lore/crafting rows, tooltip width, viewport clamp and non-prayer Technology tooltips are preserved.

### Current accepted development balance

- **Combo Prayer donation-side scaling — runtime-accepted in Rebalanced 0.2.48.** Faith remains **+100 / +150 / +200%** while donations are **+100 / +250 / +500%**. With the accepted Donations specialist (+20 / +50 / +100 silver), the same-quality pure-money crossover is roughly **GQ 500 with Cardinal** or **GQ 667 without Cardinal** for all three qualities. No other prayer balance value changed. Public stable remains 0.2.47 until separate promotion.

**Closed product decision:**

- **Legacy Protective Prayer (`b_shield`) lore stays historical.** Rebalanced retires Protection crafting/Technology while retaining already-existing `b_shield` items as Combat aliases. Their item tooltip deliberately keeps stock `b_shield_d` lore rather than being normalized to Combat Prayer lore.

Known evidence gaps that are **not** active UX backlog:
- none remain for the accepted Repose presentation/mechanics path.

Accepted runtime closure, 2026-09-26:
- real successful Silver and Gold Imagination sermons physically delivered **3 Silver Stories** and **3 Gold Stories** respectively, closing the former native-drop evidence gap.

No further in-game retest is required for the accepted 0.2.47 properties unless their implementation is changed.
