# PrayerClarity — Clarity UI status

Status: **historical accepted Clarity milestone** for Graveyard Keeper 1.407. This file preserves the 1.0.9 acceptance evidence; it is **not** the current release status.

Accepted build: **PrayerClarity 1.0.9**  
Accepted runtime/source SHA: `4b86b972baeef19aa656a9e414891405006cf67f`  
Frozen candidate ref: `candidate/1.0.9`  
Frozen accepted ref: `accepted/clarity-1.0.9`  
Build run: `35155851294`  
Artifact ID: `10470818616` (`PrayerClarity-1.0.9-ci-4b86b972baeef19aa656a9e414891405006cf67f`)  
Artifact ZIP digest: `sha256:dea2af19f544f5078c7b1af091c71346d5c6664056aff45d0290dd4a5b5d363d`  
DLL SHA-256: `0c874740643d8522857ee0a48a3f12fb2bdcc0612a61685e0db4fe7ad5d20788`

The user runtime-tested the 1.0.9 candidate on 2026-09-17 and explicitly accepted the result for promotion to `main`. The accepted visual coverage includes long Technology prayer tooltips at 1920x1080 with mouse, long combined controller tooltips without the companion positioning mod, higher-resolution/default use, and coexistence with Gamepad Tooltip Position Fix. Prayer item tooltips remained correct and unaffected by the viewport work.

This accepted baseline is information-only: it contains **no Vanilla Fixes and no Balance/Rework mechanics**.

Historical note: at the time of this milestone, the latest public release was v1.0.1. The current stable Vanilla release is **PrayerClarity: Vanilla 1.0.33**; the current stable Rebalanced release is **PrayerClarity: Rebalanced 0.2.47**. Current release identity is recorded in `README.md`, `CHANGELOG.md`, `DESIGN_NOTES.md`, and `TEST_BUILD_LOG.md`.

## Accepted pulpit presentation

The pulpit keeps the stock sermon reveal moment for the exact final payout. PrayerClarity explains the dependency chain and prayer-owned contribution instead of showing the fully resolved current Faith/donation totals in advance.

Accepted lower-block hierarchy:

1. `Guaranteed` / localized equivalent;
2. resource-source lines (`Faith — from Church Quality`, `Donations — from Graveyard Quality`);
3. `On success (N%)` / localized equivalent;
4. prayer-owned success modifiers, grouped by resource;
5. separate `Effect` row for intrinsic/special prayer behavior.

The redundant general `Result` heading is intentionally absent. The upper context block remains the stock-derived current context: Church Quality, sermon requirement, and Graveyard Quality.

Temporary prayer durations shown before use are expressed in **in-game days**, using the effective game day length rather than a hard-coded vanilla constant. This follows compatible day-length changes such as Longer Days without a direct mod dependency.

The accepted pulpit contains no prototype/developer-facing layout controls or persistent diagnostics.

## Accepted Technology presentation

Prayer-related technology tooltips are enriched at the native `TechUnlock.GetTooltip(Tooltip)` seam.

Technology is treated as a **comparison/planning surface**. The accepted renderer is property-first rather than three repeated Bronze/Silver/Gold mini-cards:

- preserve vanilla title, lore and crafting context;
- keep a dedicated `Prayer details` / localized section boundary;
- show the complete Bronze/Silver/Gold progression only where a property actually differs;
- collapse invariant properties so the same effect is not repeated three times;
- compare compound properties by component, so shared percentages and varying flat values can be presented without redundant duplication;
- use native quality stars and Church Quality symbols;
- show success-only Faith/donation contribution, intrinsic effect, duration and quantity where applicable;
- keep semantic sections visually separated while keeping tier progressions compact.

The accepted progression grammar uses atomic tier segments: a quality marker and its value are not allowed to wrap apart. This prevents cases where a Gold star is left at the end of one line while its value moves to the next.

Short scalar progressions such as requirements, duration and quantity are allowed to remain horizontal. Longer progressions wrap only between whole tier segments.

### Technology viewport safety

Information compaction alone did not eliminate all 1920x1080 overflow cases. The accepted baseline therefore adds a narrow viewport clamp for **PrayerClarity-owned prayer Technology bubbles only**.

Architecture and behavior:

- only a `Tooltip` explicitly enriched by PrayerClarity is marked;
- after `Tooltip.Show(bool)` the concrete linked bubble is associated with that marked tooltip;
- final correction runs at `WidgetsBubbleGUI.Update()`, the verified late-layout seam where authoritative bubble dimensions are available;
- ordinary non-prayer Technology tooltips are unmarked and untouched;
- viewport geometry uses current `Screen.safeArea`, screen dimensions and the active `UIRoot.manualHeight` scale instead of resolution-specific hard-coded coordinates;
- the clamp uses a 24-screen-pixel logical safety margin;
- if the bubble already fits, no position write occurs;
- Harmony ordering is explicitly after `nikich.gyk.movegamepadtooltips`, so the companion mod may choose its preferred controller position first and PrayerClarity only corrects an actual viewport overflow afterward.

Runtime testing confirmed the same logic works for mouse and controller presentation and does not require Gamepad Tooltip Position Fix as a hard dependency. The companion remains an optional positioning/QoL mod.

## Accepted prayer-item tooltip presentation

The prayer-item tooltip is an **inspection surface for the concrete hovered item**, not a second comparison table.

Accepted behavior:

- preserve the native prayer item title, flavor text and crafting-location text;
- show only the hovered item's linked prayer craft/current quality tier;
- do not resolve Bronze/Silver/Gold sibling items merely to reproduce the Technology comparison;
- show the current item's requirement, success-only contribution and special effect/duration as applicable;
- ordinary prayer remains a single non-quality case.

This avoids the 1.0.1 problem where hovering a Silver prayer could redundantly describe Bronze, Silver and Gold at once.

## Accepted Character -> Temporary Effects presentation

Prayer buffs shown in Character -> Temporary Effects receive a concrete description of their current stock effect.

For long prayer buffs, remaining time is shown in **in-game days** in the normal description text. The small HUD/row timer is hidden while at least one full in-game day remains; inside the final day the stock precise timer returns.

The implementation derives remaining days from the native `PlayerBuff` state (`end_time - MainGame.game_time`) rather than parsing the formatted timer string. It does not add a second timer state.

## Localization

PrayerClarity-owned player-facing strings ship in all 11 supported interface languages:

`en`, `fr`, `de`, `zh_cn`, `es`, `pt_br`, `ko`, `ja`, `ru`, `it`, `pl`.

The 1.0.9 candidate CI verified all embedded locale markers. The accepted runtime polish was visually checked primarily in Russian and English while the renderer architecture remains locale-independent.

## Architecture / performance boundary

The accepted Clarity layer remains narrow and UI-lifecycle driven:

- pulpit calculation/rendering occurs at the verified pulpit redraw seam;
- Technology content is produced only when the native tooltip is built;
- prayer-item details are produced only when the native item tooltip is built;
- active-effect text is bound through the native active-buff row path;
- Technology viewport correction uses the verified late `WidgetsBubbleGUI.Update()` seam, but only PrayerClarity-owned bubbles proceed past the weak-marker guard;
- reflection bindings for viewport geometry are compiled/cached during install rather than rediscovered every frame;
- no broad Unity scans, background polling, mirrored prayer runtime state or mechanics mutation is introduced.

## Historical promotion state

`candidate/1.0.9` and `accepted/clarity-1.0.9` point to the exact accepted runtime/source SHA `4b86b972baeef19aa656a9e414891405006cf67f`.

That state was accepted at the time and later superseded by subsequent Clarity releases. Current stable runtime identity is Vanilla 1.0.31 / Rebalanced 0.2.10 from shared accepted source `30b036f16dc6a7964f7ef72e2e3ececa5951c812`.

The last public release remains `v1.0.1`; publishing 1.0.9 is a separate release step and is not implied by mainline acceptance.

## Scope boundary

This accepted baseline describes **stock 1.407 behavior clearly**. It is not evidence that every stock prayer mechanic is healthy.

Future work remains separated into:

- **Vanilla Fixes** — only evidence-backed repairs where intended vanilla behavior/magnitude is recoverable;
- **Balance / Rework** — explicit intentional tuning or redesigned behavior.

Both later layers must feed changed values through the accepted shared UI model rather than creating separate presentation logic.