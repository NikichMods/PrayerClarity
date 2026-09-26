# PrayerClarity — Technology Tooltip UX Rework Hypothesis

Status: **design hypothesis / research only**, 2026-09-17. No production code, mechanics, balance, localization resources, accepted refs, or release state are changed by this document.

Accepted runtime baseline remains PrayerClarity 1.0.9 at `4b86b972baeef19aa656a9e414891405006cf67f`. The 1.0.9 Technology tooltip is technically accepted and viewport-safe. This document reopens only the **information architecture/readability** question after a direct usability review.

## Why the accepted property-first layout is being reconsidered

The 1.0.9 renderer is property-first: Requirement, Faith, Donations, Effect, Quantity and Duration are compared as separate properties, with Bronze/Silver/Gold values shown only where the property changes. This successfully removes repeated information and is mechanically accurate.

A 2026-09-17 review with one experienced Graveyard Keeper player exposed a different failure mode. This is **one-participant usability evidence, not community consensus**.

Observed signals:

- the tooltip felt visually dense because most rows/sections had similar spacing and therefore weak semantic grouping;
- even while looking at the `Faith` heading, the reader did not understand a value such as `+25% +1` and asked what the `+1` meant;
- after explanation, the reader reconstructed the mechanic as `1.25× base + 1`, showing that the values were present but the presentation did not communicate their relationship;
- `Требуется: 10` was still ambiguous enough to prompt the question whether 10 meant Church Quality;
- the vanilla tooltip was judged more compact and in some respects easier to parse despite containing less information;
- the reader independently proposed grouping the changing values by prayer tier rather than by property.

### UX finding

The property-first representation minimizes textual duplication but can increase **reconstruction cost**: to answer “what does Bronze/Silver/Gold actually give me?”, the player must gather one tier's requirement, percentage modifiers, fixed outputs, reward count and duration from several independent property rows.

A second concrete problem is arithmetic ambiguity: a compact combined value such as `+25% +1` is mathematically correct but does not visually distinguish a proportional modifier from a flat success-only addition.

Runtime acceptance of 1.0.9 established correctness, geometry and compatibility. It did not establish that this information architecture is the best human-readable form.

## New design hypothesis: shared invariants -> compact tier snapshots

Technology remains the comparison/planning surface, but the comparison grammar changes from **property-first** to a hybrid:

1. preserve the vanilla title, description/lore and crafting source;
2. show genuinely shared/invariant prayer behavior once;
3. group all materially tier-dependent information under the corresponding native Bronze/Silver/Gold quality glyph;
4. use the quality glyph itself as the tier heading — do not also print `Bronze`, `Silver`, `Gold` text unless localization/runtime evidence later proves it necessary;
5. keep percentage modifiers and flat additions semantically separate; never render them as an unexplained expression such as `+25% +1`;
6. give different conceptual blocks visibly different vertical spacing, while keeping lines inside one tier compact.

The intended reading order is:

`description -> shared success mechanic/effect -> Bronze snapshot -> Silver snapshot -> Gold snapshot -> crafting source`

This preserves the anti-duplication goal of the accepted renderer while optimizing the question the player is most likely to ask in Technology: **“What does each prayer quality buy me?”**

## Success-requirement wording

The tier glyph and success requirement should share one line.

Preferred Russian grammar:

`(s1) Для 100% успеха требуется 10 (cross)`

and equivalently for Silver/Gold.

Do **not** shorten this to only `Требуется: 10` or `Для 100% успеха: 10` merely to save width. The verb `требуется` closes the semantic relationship: the number is the Church Quality threshold for guaranteed success, not a chance, price or prayer stat.

Avoid the slightly more mechanical punctuation `Для 100% успеха требуется: 10`; the natural sentence without the second colon is preferred where the locale permits it.

Localization must express the same semantic relationship naturally rather than translate the Russian word order literally.

## Width policy

The accepted local `TechnologyTooltipMaxWidth = 360` is now considered a **candidate implementation constraint, not a product requirement**.

For a future prototype, Technology tooltip width should be content-driven:

- expand far enough to keep normal tier lines intact when the viewport has room;
- prefer buying horizontal space to avoid avoidable line wrapping and excess height;
- retain a viewport-safe upper bound derived from the available screen/safe-area geometry rather than allowing unbounded width;
- preserve the existing viewport clamp as the final safety layer;
- do not force every locale to one physical width;
- if an unusually long localization still cannot fit, wrap only at semantic boundaries rather than splitting a quality glyph from its requirement/value or breaking another atomic clause.

The goal is not “never wrap under any circumstance”. The goal is **do not wrap a coherent tier row merely because of an arbitrary 360 px cap when usable horizontal space exists**.

## Vertical rhythm

The current visual density suggests that spacing itself should encode hierarchy.

Candidate rule:

- small/no extra gap between lines that belong to one tier snapshot;
- clear gap before the first tier and between tier snapshots;
- clear gap between shared mechanics and tier snapshots;
- clear gap before the final vanilla crafting-source row;
- avoid giving every heading/property the same vertical weight.

This is a layout hypothesis to verify in runtime, not a request to inflate the tooltip uniformly.

## Reference case — Prayer for Prosperity

Prosperity is the best control case because stock 1.407 has both shared percentage modifiers and clearly tier-dependent fixed rewards:

- requirement: 10 / 20 / 30 Church Quality;
- Faith modifier: +25% at every tier;
- Donations modifier: +25% at every tier;
- fixed Faith: +1 / +2 / +3 on success;
- fixed money: +1 / +2 / +3 silver on success;
- Commercial Blessing: ×1 / ×2 / ×3 on success.

Conceptual Russian mockup; native icons/glyphs should be used by the renderer:

```text
<vanilla description>

Бонусы при успехе
(faith) Вера +25% · Пожертвования +25%

(s1) Для 100% успеха требуется 10 (cross)
+1 (faith) · +1 (slv) · Благословение коммерции ×1

(s2) Для 100% успеха требуется 20 (cross)
+2 (faith) · +2 (slv) · Благословение коммерции ×2

(s3) Для 100% успеха требуется 30 (cross)
+3 (faith) · +3 (slv) · Благословение коммерции ×3

<vanilla crafting source>
```

The exact final typography, separators and whether the shared heading says `Бонусы при успехе` or another localized equivalent remain prototype details. The important semantic separation is:

- shared +25%/+25% appears once;
- flat +1/+2/+3 outputs are owned by the individual tier;
- the percentage and flat additions are no longer concatenated into one ambiguous arithmetic string.

### Commercial Blessing description

The sentence explaining that a Commercial Blessing can be sold to a merchant to raise merchant level should be removed from the **Technology comparison body** in this design hypothesis. It explains the rewarded item rather than the prayer-quality comparison and consumes substantial vertical space.

The reward identity and count remain visible. The blessing's own item/other appropriate surface can carry the full item-purpose explanation.

## Grammar checks against other prayer shapes

These checks test whether the information architecture generalizes. They are not new mechanics claims and do not invent values absent from accepted evidence.

### Faith prayer

Verified stock shape:

- q: 10 / 20 / 50;
- Faith proportional bonus: +50% / +100% / +150%;
- Donations proportional bonus: +20% invariant;
- prayer also has fixed success-only Faith/money outputs.

Proposed grouping:

- shared block: Donations +20%;
- each tier: quality glyph + full 100%-success requirement;
- tier body: the tier's Faith percentage, explicitly labeled as a proportional/base modifier;
- flat fixed outputs on a separate clause/line from the percentage.

Important rule: do not collapse a tier back into `Faith: +50% +N`. Prefer a grammar equivalent to `Faith: +50% to base` plus `Additionally: +N Faith ...`, using icons/compact punctuation where that remains unambiguous.

**Result:** the hybrid grammar fits; this is a key stress case because the proportional modifier itself varies by tier.

### Donations prayer

Verified stock shape mirrors Faith:

- q: 10 / 20 / 50;
- Faith proportional bonus: +20% invariant;
- Donations proportional bonus: +50% / +100% / +150%;
- fixed success-only outputs also exist.

Proposed grouping mirrors Faith with Faith +20% in the shared block and the varying Donations rate inside each tier snapshot. Flat outputs remain separate from the percentage.

**Result:** fits the same grammar without a special renderer concept.

### Combo prayer

Verified stock shape:

- q: 15 / 30 / 60;
- Faith +50% / +100% / +150%;
- Donations +50% / +100% / +150%;
- fixed success-only outputs also exist.

There is no useful invariant proportional resource block to pull above the tiers. Each tier can instead contain one compact proportional line such as `Faith +50% · Donations +50%`, followed separately by any flat additions.

**Result:** fits. Tier-first grouping is arguably more natural here than property-first because both key reward dimensions advance together.

### Shoots & Roots

Verified Clarity-only stock shape:

- q: 10 / 20 / 30;
- Faith +25% invariant;
- Donations +25% invariant;
- timed `buff_plant` duration changes by tier;
- stock 1.407 has the known parameter-owner wiring mismatch, so Clarity must continue to report that the prayer does not actually accelerate plant growth in the inspected stock path.

Proposed grouping:

- shared block: Faith +25%, Donations +25%, plus the known stock-effect warning once;
- each tier: quality glyph + full 100%-success requirement + that tier's duration.

**Result:** fits. This case also shows why the system should not blindly force every property into every tier: the disconnected-effect statement is invariant and belongs once above the tier snapshots, while duration is tier-dependent.

## Scope boundaries

This hypothesis changes only Technology presentation.

It does **not** reopen:

- the accepted pulpit reveal boundary;
- concrete prayer-item tooltip semantics (current tier only);
- Temporary Effects behavior;
- Technology viewport ownership/clamp architecture;
- stock prayer mechanics;
- Vanilla Fixes or Balance/Rework values.

A future renderer prototype should continue to consume the same side-effect-free semantic model and should not create a second source of prayer mechanics truth.

## Next gate

Do not modify production renderer code yet solely from this document.

Before implementation, use this hypothesis to define a narrow Technology-only candidate scope. The first runtime candidate should prove:

1. Prosperity reads as a compact shared block plus three tier snapshots;
2. Faith/Donations/Combo remain understandable without reintroducing `% + flat` ambiguity;
3. one timed prayer such as Shoots & Roots remains compact and clearly separates invariant effect from tier-dependent duration;
4. content-driven width prevents avoidable wraps at 1920x1080 while remaining viewport-safe;
5. Russian plus at least one long Latin locale and one CJK locale remain structurally sound;
6. ordinary non-prayer Technology tooltips remain untouched.

Only after that visual/runtime check should this hypothesis replace the accepted 1.0.9 Technology renderer design.

## 2026-09-27 — prayer Technology alignment unification candidate

Status: **0.2.49 superseded after runtime failure; 0.2.50 correction pending**.

### UX rule

Prayer Technology should use the same hierarchy already accepted on prayer-item surfaces:

- prayer title/header: centered;
- `Base Result` header: centered;
- `On Success` header: centered;
- mechanics/body content: left-aligned.

Lore/description and crafting-source rows remain stock-owned rather than being normalized merely for symmetry.

### Owner / input-path proof

Stock 1.407 `TechUnlock.GetTooltip(Tooltip)` creates the Technology title and prayer rows as `BubbleWidgetTextData`. `BubbleWidgetText.Draw` later applies that row's `alignment` directly to its `UILabel`.

The same data producer is used for both input modes:

- mouse prayer-unlock tooltip -> `TechUnlock.GetTooltip`;
- stock gamepad Technology assembly -> `TechTreeGUIItem.InitGamepadTooltip` -> `TechUnlock.GetTooltip`;
- PrayerClarity selected-unlock carousel -> clears/rebuilds the selected tooltip by invoking the same `TechUnlock.GetTooltip`.

Therefore the least-complex adequate implementation is to normalize only the three prayer heading rows in the shared tooltip data, rather than add mouse/controller-specific or late-layout alignment patches.

### Candidate identity

- exact source: `8e95d45a4b501c2852428eb8d4d37883be93b7c4`;
- CI: `36272619150` — success;
- Rebalanced DLL SHA-256: `f3d42a45e539a5af1de5f128158588daeaf74c82b700f728dfac479033cabbce`;
- Vanilla sibling DLL SHA-256: `c6c3437538598e112fc941477a2c1a6bbbf823637313d2ef30d70ecf9d6f41b9`.

No Technology width/viewport policy, navigation behavior, mechanics or localization strings are changed by this candidate.


### 0.2.49 runtime correction to the ownership model

Runtime disproved one part of the original candidate model: sharing `TechUnlock.GetTooltip` does **not** imply that row-level `BubbleWidgetTextData.alignment` alone determines the visible horizontal position.

On the stock bubble path, `Tooltip.Show` supplies a container alignment to `WidgetsBubbleGUI`. A Left container uses a TopLeft table placement and later gives label widgets a Left pivot. Consequently, a short child row with centered UILabel text remains visually at the left edge if the child widget itself is only text-wide.

Observed 0.2.49 result matches this exactly:

- mouse Technology prayer headers: still visibly left;
- gamepad Technology prayer headers: centered.

The corrected least-powerful mechanism is therefore:

1. retain row alignment = Center for the three prayer Technology headings;
2. at `WidgetsBubbleGUI.UpdateSizeAndWidgetsPositions`, first measure the already-existing native maximum child width;
3. expand only the marked centered heading children to that same span;
4. let stock UpdateSize/Reposition finish.

Because the correction never exceeds the width the native bubble already had, it does not introduce another custom Technology width policy. Body/lore/crafting rows remain untouched.


### 0.2.50 candidate identity

- branch: `candidate/rebalanced-0.2.50`;
- exact source: `da770d4e3d2131ece502f6379bca520cb2819d2a`;
- CI run: `36273185335` — success;
- Rebalanced DLL SHA-256: `59fcc8b922dd0841745189d86edc60450f6eea064cca139d4ab5ad1a5c1d0afe`;
- Vanilla sibling DLL SHA-256: `2aa6f9361fcd2d2c0218bd3d195192ee347239d00d6634ca9af931e9fc6ca7e4`.

Runtime acceptance is pending only for the visible mouse/gamepad header-centering result and a quick control that the previously accepted prayer-item alignment remains unchanged after the shared native-span helper refactor.


### 0.2.50 runtime result and 0.2.51 composition correction

0.2.50 proved that the native-span final layout correction is sufficient for mouse Technology section headers: `Base Result` and `On Success` centered correctly without changing the outer parchment width. The top `Create: Prayer...` row alone remained left-aligned.

That isolates the remaining defect to **semantic row identification**, not to the final layout writer.

The stock 1.407 `TechUnlock.GetTooltip` contract is structural: each call first appends the current unlock title row, then a blank separator. PrayerClarity therefore no longer needs to infer that row by comparing localized text to `TechUnlock.GetData().name`.

0.2.51 uses a composition-role model:

- current stock title -> structurally identified `Title`;
- `Base Result` -> assigned `SectionHeader` when reused/created;
- `On Success` -> assigned `SectionHeader` when created;
- mechanics rows -> remain body/Left.

A prefix on the same `TechUnlock.GetTooltip` call records the pre-call row count, so the exact current title index remains unambiguous even when gamepad Technology combines multiple unlocks into one tooltip. The existing native-span final writer then treats only those marked centered headings.

Candidate identity:
- source `13c85c824bd2932b1175200368d212e03bbae79f`;
- CI `36275212482` — success;
- Rebalanced DLL SHA-256 `083168ec6cd76fd55366ca10426e212a30a3bc2c7ca61a9e9de0dc6f656c8ccd`;
- Vanilla sibling DLL SHA-256 `3ff5f830a3ce20d1bf789e5f9ff0d3ac11cf9511d4457660b56c1f286dc31195`.
