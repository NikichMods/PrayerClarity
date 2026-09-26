# PrayerClarity Project Contract

PrayerClarity inherits the global development contract from `NikichMods/DevRules`.

Before substantive work, read:

- `DevRules/ENGINEERING_RULES.md`
- `DevRules/CI_POLICY.md`
- `DevRules/GIT_WORKFLOW.md`
- `DevRules/PROJECT_BOOTSTRAP.md`
- this file
- the current project docs and relevant branch/commit/PR evidence

## Project identity

Target: Graveyard Keeper 1.407.

Purpose: establish how the prayer/sermon system actually works, make its effects understandable at the point of choice, repair narrowly proven broken/disconnected behavior where vanilla intent is recoverable, and explicitly redesign/tune prayers where the verified system still lacks a healthy choice structure.

### Public product family naming

The accepted public naming architecture is:

1. **PrayerClarity: Vanilla** — the current Clarity-only edition. `Vanilla` refers to prayer gameplay mechanics and balance: this edition may improve presentation and explanation, but it must preserve stock Graveyard Keeper 1.407 prayer/sermon behavior.
2. **PrayerClarity: Rebalanced** — a separate sibling edition for intentional prayer rebalance/rework once that behavior is implemented, tested and accepted.

Treat these as peer alternatives in the PrayerClarity family, not as a base mod plus upgrade/add-on. A player who encounters either edition first should be able to infer its gameplay philosophy from the edition name.

`PrayerClarity` remains the family/repository/codebase name. Existing technical identifiers such as the repository name, plugin identity and canonical DLL filename do not need to change merely because the public edition is named **PrayerClarity: Vanilla**.

### Current accepted stable baselines

- **PrayerClarity: Vanilla 1.0.57** — tag/release `v1.0.57`, accepted ref `accepted/vanilla-1.0.57`, canonical DLL `PrayerClarity.dll`, exact accepted source SHA `13c85c824bd2932b1175200368d212e03bbae79f`, accepted DLL SHA-256 `3ff5f830a3ce20d1bf789e5f9ff0d3ac11cf9511d4457660b56c1f286dc31195`.
- **PrayerClarity: Rebalanced 0.2.51** — tag/release `rebalanced-v0.2.51`, accepted ref `accepted/rebalanced-0.2.51`, canonical DLL `PrayerClarity.Rebalanced.dll`, exact accepted runtime source SHA `13c85c824bd2932b1175200368d212e03bbae79f`, accepted DLL SHA-256 `083168ec6cd76fd55366ca10426e212a30a3bc2c7ca61a9e9de0dc6f656c8ccd`.
- Stable publication must reuse the exact accepted CI binaries without rebuilding or changing bytes under the same version.
- `main` may contain later documentation/repository-hygiene commits; numbered stable runtime identity remains tied to the frozen accepted refs and release hashes recorded in `docs/TEST_BUILD_LOG.md`.

The internal evidence/design categories remain distinct from public edition naming:

1. **Clarity** — information only; no mechanics changes.
2. **Vanilla Fixes** — evidence-backed repairs where the intended mechanic/magnitude is sufficiently recoverable.
3. **Balance / Rework** — intentional new design or tuning. Never present these changes as recovered vanilla mechanics.

**Vanilla Fixes is not an accepted third public edition name.** Because **PrayerClarity: Vanilla** explicitly promises stock mechanics, do not silently ship gameplay repairs in that edition. The eventual packaging of evidence-backed vanilla repairs requires an explicit product decision; until then they remain a separate semantic/research category.

Stock Graveyard Keeper 1.407 behavior must remain documented independently of modded behavior.

Do not assume `prayer`, `sermon`, localized names, item IDs, `PrayCraft`, or `PrayEventDefinition` are interchangeable. Record mappings only when evidence supports them.

## Production-change gate enforcement

PrayerClarity uses the global DevRules evidence gate as a hard stop, not as an advisory checklist.

For every materially independent production behavior change:

1. before the first production-source mutation, make the gate reviewable as **READY** or **BLOCKED** with the observable property, canonical owner, final writer/consumer/commit point where applicable, blast radius, preserved invariants, and acceptance evidence;
2. there is no exception for a change that appears small, obvious, presentation-only, follow-up, or convenient to bundle;
3. **BLOCKED means research/probe only**; do not change production behavior under that gate;
4. a fresh runtime/user-visible regression opens a new gate for that exact property; prior PrayerClarity/shared research may be reused only when it proves the relevant owner/final-writer path;
5. treat the reported defect/request as the default scope. Wording, mechanics, layout, data semantics, lifecycle, and other adjacent behavior are preserved unless the proved path requires changing them or the user separately accepts that additional change;
6. do not optimize for fewer game restarts, candidate versions, or user test cycles by bypassing or combining unresolved gates.

Gate granularity and candidate granularity are deliberately separate:

- every materially independent behavior change still has its own gate and must be **READY** before its first production-source mutation;
- several independent READY changes may share one PrayerClarity candidate when they use a coherent baseline, their interactions are understood, and one combined acceptance run can still verify each property without making a failure ambiguous;
- a non-urgent READY micro-change may remain queued until the next natural candidate boundary rather than consuming its own version/DLL;
- never include a **BLOCKED** change or an independent unverified mechanism merely because another READY change already justifies a build;
- split candidates when combined testing would materially reduce fault attribution, rollback clarity, or confidence.

For a numbered production candidate branch handled by the standard PrayerClarity candidate workflows:

- create `docs/CANDIDATE_GATE.json` from `docs/CANDIDATE_GATE_TEMPLATE.json` in a **gate-only commit** before production-source changes relative to the declared `baseline_sha`;
- `baseline_sha` is the exact pre-change source state for that candidate's change set;
- list each materially independent included change separately in the `changes` array;
- every listed change must be `READY`; research-only probes/Test Console branches do not use this production gate file;
- candidate CI validates the gate record and its ordering before compiling or uploading a handoff artifact;
- if a tested candidate is rejected, do not blindly stack the next candidate on the full rejected source. Choose and record the next baseline deliberately, retaining only already accepted/proved changes plus newly READY fixes.

The gate file is mutable candidate evidence, not stable product documentation. Accepted durable conclusions still belong in the canonical project/shared docs.

## Research / design order

Use this sequence unless new evidence justifies a narrower detour:

`catalogue -> verify mechanics -> audit presentation -> research player experience -> identify UX gap -> define acceptance envelope -> compare viable solution families -> choose least-complex adequate path -> design/role audit -> quantitative power-budget audit -> candidate roster/spec -> READY/BLOCKED evidence gate -> narrow prototype -> runtime test -> accept`

For PrayerClarity presentation/UX defects, do not treat the first technical mechanism as the requirement. When the same player-facing goal could plausibly be reached through different families such as wording/content changes, layout/spacing changes, bounded fixed geometry, or content-driven/dynamic geometry, apply the global DevRules solution-space checkpoint before substantial implementation or fresh host/UI research. If the selected path fails runtime acceptance or would require materially deeper UI/runtime probing, re-open those alternatives before continuing down the same branch. Product-owned wording/semantics changes remain separate user decisions, not silent implementation shortcuts.

Do not write broad production behavior before the relevant mechanic, UI surface, design rule, candidate specification and acceptance condition are established.

## Evidence labels

Use these statuses explicitly where useful:

- **fact** — supported by accepted project data, direct assembly/resource/localization inspection, runtime measurement, or another direct source;
- **hypothesis** — plausible explanation not yet proved;
- **community signal** — a player report/question useful for UX/design research but not proof of mechanics;
- **UX finding** — supported mismatch between information the player needs and information the game supplies;
- **design hypothesis** — possible presentation/fix/rebalance rule not yet accepted;
- **accepted result** — runtime-sensitive behavior or design accepted after the required user test.

Evidence priority for mechanics:

1. accepted project data;
2. direct assemblies/resources/localization;
3. targeted calling-code inspection;
4. narrow runtime probe;
5. user in-game test when only the installed game can resolve uncertainty.

Wiki/guides/community sources are useful for terminology, discovery, cross-checking and player-experience/design research. They are not a substitute for direct mechanics evidence when game data/code can answer the question.

## Prayer mechanics research

For each player-relevant prayer establish where possible:

- exact player-facing name and internal item/craft/event IDs;
- use conditions and success/failure rules;
- input parameters/formulas;
- fixed vs random components;
- prayer-quality and church/graveyard/other dependencies;
- rounding, thresholds, caps and probabilities;
- Faith, money, item drops and temporary effects;
- relevant getters/methods/data definitions;
- text shown before and after use;
- discrepancies between presentation and actual behavior.

Do not guess IDs, localization keys, formulas, Harmony targets, lifecycle or supposed developer intent.

## Stock-vs-mod boundary

For every gameplay change record separately:

- what stock 1.407 actually does;
- what evidence indicates broken/disconnected behavior vs weak/niche behavior;
- what the mod changes;
- whether the change is Vanilla Fix or Balance/Rework;
- what runtime/user evidence is required before acceptance.

A broken mechanic may be a Vanilla Fix only when intended behavior is sufficiently recoverable. If role is known but magnitude/algorithm must be invented, the change is Balance/Rework.

### Permanent project policy: failed-sermon donations

Stock 1.407 gives the full base donation pool even on sermon failure because the apparent 50% visitor-participation path is defeated by integer `Random.Range(0,1)` behavior.

**Do not change this.** Preserve full base donations on failed sermons in all profiles. Prayer-specific bonuses/special success outputs may still be lost according to verified mechanics. This is an explicit product decision, not a claim about original developer intent.

## UX research

Keep separate:

1. What does the game actually do?
2. What do players think it does?
3. What can a player infer from the UI without external sources?

Do not call a handful of comments a consensus. Preserve date/version context. State UX gaps concretely. Do not assume the answer is a long tooltip.

## Balance / role research

### Core principle: temptation parity

A prayer is a strategic investment, not just a numeric buff. Its real cost includes:

- technology unlock/prerequisite investment;
- crafting materials and Faith;
- effort to obtain prayer quality;
- church-quality/success requirement;
- the opportunity cost of spending the weekly sermon on it instead of another prayer.

**Design target:** every prayer should be a tempting purchase and a tempting weekly choice at the stage/niche where it belongs.

This is not numerical parity. A narrow prayer may be deliberately very strong in its niche. A progression prayer may legitimately become obsolete after completing its role. What is undesirable is paying substantial unlock/craft/week cost for a prayer that never creates a convincing moment where the player wants it.

Bronze should already be credible. Silver/gold should create meaningful extra value through magnitude, duration, outputs, success gates, or crossing future sermon weeks.

Do not rebalance for symmetry. Before changing a functioning prayer classify it as:

- broken/disconnected;
- misleading/opaque;
- healthy niche;
- progression tool;
- dominated/redundant;
- underpowered relative to its full investment/opportunity cost.

Compare unlock timing/cost, chapter vs book recipe class, quality difficulty, alternative systems/consumables/perks, church requirements, duration, stage relevance and weekly opportunity cost.

Prefer making alternatives attractive over reducing familiar player rewards. Nerfs require stronger justification than existence of a meta choice.

`docs/PRAYER_DESIGN_AUDIT.md` is the current source of truth for prayer-by-prayer design judgements.
`docs/PRAYER_POWER_BUDGET.md` is the current source of truth for quantitative unlock/craft/opportunity-cost comparisons.
`docs/PRAYER_REBALANCE_OPTIONS.md` is the canonical accepted Rebalanced ruleset unless a later accepted runtime-safety constraint is recorded in `docs/TEST_BUILD_LOG.md`. Rebalanced 0.2.51 retains the accepted native runtime seams and 95% combined Roots safety cap, uses 30/42/54-minute Repentance/Repose durations, Repose q20/q40/q95, Excellence q20/q60/q95, Donations +20/+50/+100 silver-equivalent, Combo Faith +100/+150/+200% with donations +100/+250/+500%, BSS Soul's Repose q30/q60/q90 with success-only 1:1 Soul Gratitude -> Faith conversion capped at 30/60/90, Soul Contentment +50% with 45/90/135-minute duration, and Thorough Cleansing x2/x3/x4 at q30/q60/q120. Historical candidate values elsewhere are superseded unless explicitly retained as analysis.

## Player-facing clarity target

The intended end state is a **white-box player experience**, not a developer-facing formula viewer and not a pre-sermon final-payout calculator.

The UI should let the player answer:

- what the prayer does;
- what current values/conditions affect it;
- what changing prayer quality or inputs changes;
- how it differs from alternatives;
- when a selected profile repairs or intentionally changes vanilla behavior.

For the pulpit specifically, preserve the sermon as the reveal moment for the exact resolved Faith/donation payout. Default pre-sermon presentation should explain the dependency chain and the prayer's own contribution instead of displaying the fully computed current totals:

- keep `guaranteed/base -> success-only prayer contribution -> special effect` as the semantic decomposition;
- Guaranteed explains that base Faith comes from Church Quality and base donations from Graveyard Quality; BSS-specific dependencies such as Soul Gratitude remain explicit where verified;
- success presentation keeps the exact success probability and shows exact prayer-owned modifiers such as Faith `+50%` or Donations `+25%`, plus fixed prayer-owned outputs where relevant;
- exact intrinsic prayer mechanics remain exact: duration, growth reduction, damage, armor, regeneration, confession probability, Soul Gratitude/Sin Shard multipliers, Blessing counts and comparable prayer-owned properties;
- do not replace real progression with qualitative `low / medium / high` buckets;
- the side-effect-free semantic model may retain exact payout calculations for correctness/testing/balance, but default player-facing pulpit rendering must not expose the final current payout merely because it can be calculated.

Prefer concrete intrinsic values, short dependency explanations and visual hierarchy over raw formulas. `docs/PULPIT_REVEAL_UX.md` is the canonical detailed rationale for this reward-reveal boundary.

### Post-audit architecture closure

`docs/POST_AUDIT_VERDICT.md` is the mandatory starting point for future PrayerClarity architecture/lifecycle audits.

The 2026-09-19 post-audit closed the remaining Rebalanced tier-state save/load question and set both current lines to **A — no architecture action**. In particular, do not reopen the accepted Repentance seam, Combat scoped modifiers, Roots stock-formula/input-bridge design, Excellence `GetBuffValue("buff_star")` seam, Soul Contentment live-graph projection, or persistent tier-token strategy without new direct evidence, changed source/game binary, or a concrete runtime conflict.

The remaining terminal Repose endpoint check is presentation-only/non-blocking. Do not require user progression solely to close it.

Use one mod-owned semantic model for all prayer presentation surfaces so technology text, item tooltip, pulpit forecast, active-buff presentation and effective configured mechanics cannot contradict each other.

## Localization / language support

PrayerClarity is intended to be universal across the languages officially supported by Graveyard Keeper, not an English/Russian-only mod.

Every PrayerClarity-owned player-facing string must ship in the same 11 interface languages as the base game:

- English (`en`);
- French (`fr`);
- German (`de`);
- Simplified Chinese (`zh-cn` / normalized `zh_cn`);
- Spanish — Spain (`es`);
- Portuguese — Brazil (`pt-br` / normalized `pt_br`);
- Korean (`ko`);
- Japanese (`ja`);
- Russian (`ru`);
- Italian (`it`);
- Polish (`pl`).

Localization requirements:

- follow the current in-game language automatically;
- use the game's current language state rather than OS locale or Steam language when the game exposes the active language;
- normalize equivalent language-code separators/casing rather than maintaining duplicate translation logic;
- keep English as the safe fallback for a missing locale or key;
- reuse vanilla localized terminology/names where practical instead of retranslating game-owned terms;
- keep dynamic numbers/formulas separate from translatable sentence templates;
- do not hard-code player-facing prose in Harmony patches or mechanics code;
- a candidate is not localization-complete if any new player-facing string exists only in English/Russian.

Language switching must not introduce per-frame polling. Resolve/reload localization at an existing language/UI lifecycle boundary or lazily when rendering relevant UI.

## Native-mechanics implementation gate

PrayerClarity follows the global DevRules host-native-first gate, with these Graveyard Keeper-specific defaults:

- when a verified stock formula already consumes a parameter such as `buff_plant`, `add_damage`, `add_armor`, `craft_q`, or another game-owned modifier, prefer supplying the intended value through that parameter/state seam and leave the stock calculation authoritative;
- prefer changing verified `CraftDefinition`, `BuffDefinition`, `GameRes`, player/WGO parameters, or other stock-owned data that the existing execution path already consumes;
- a game-owned `SmartExpression` parser or FlowCanvas graph engine does **not** make a newly authored replacement expression/graph "native": ownership of the formula/graph still moved into the mod;
- do not replace an existing SmartExpression, FlowCanvas computation, or host algorithm merely to change one operand/constant until the relevant input/parameter/extension seams have been directly inspected and documented insufficient;
- an empty stock extension point (for example a BuffDefinition tick expression deliberately executed by the game's own buff lifecycle) may be populated when it adds new behavior without replacing an existing host-owned calculation;
- when a full formula/graph replacement is genuinely necessary, document why narrower seams cannot express the accepted semantics, then runtime-test the exact live execution path and at least one material stacking/edge interaction before acceptance.

The preferred architecture is: **Graveyard Keeper owns calculation, lifecycle, stacking, rounding and side effects; PrayerClarity supplies the smallest changed input.**

## Runtime and performance constraints

Production should be event-driven and cheap:

- compute only when relevant UI opens/refreshes;
- prefer existing game getters/state;
- avoid per-frame polling, broad Unity scans, repeated heavy reflection/enumeration, duplicate subscriptions and persistent verbose logging;
- fixes/rebalance should patch the narrow verified behavior rather than repeatedly scanning all objects.

A diagnostic probe must answer one narrow question and be removable.

When several closely related runtime questions require the user's installed game, prefer a research-only Test Console/harness that turns them into explicit buttons/actions and one returned log. Use native game APIs for the behavior under test, keep synthetic setup narrow/reversible, log the effective inputs/results, warn about any save-persistent test state, and never ship the harness in production. Follow the global DevRules user-operated runtime harness contract.

Before creating or extending a PrayerClarity Test Console/probe, make the DevRules research-method checkpoint explicit: state the exact open question, whether accepted evidence/source inspection/an existing exact candidate or accepted artifact/a short direct in-game action can answer it, and why new research code is still simpler or more reliable if those paths are insufficient.

Do not add Test Console machinery merely to avoid one cheap DLL swap, restart, tooltip hover, or other deterministic user action. In particular, if an existing immutable candidate already provides the required baseline behavior, prefer testing against that exact candidate rather than reproducing the baseline inside the console and thereby adding another writer/assumption. Optimize for fewer assumptions and moving parts, not fewer user clicks.

### Neutral Test Console boundary

PrayerClarity's long-lived Rebalanced Test Console is a **neutral setup/access utility**, not a probe host.

It may:
- spawn/remove verified prayer items for inspection;
- temporarily expand/restore inventory capacity;
- activate/remove already-verified prayer buffs through the game's native buff API when this is only test-state setup;
- open an already-verified game UI seam such as the pulpit without rewriting that UI's behavior.

It must **not**:
- install Harmony patches or other hooks into the production path under test;
- rewrite UI text/layout, mechanics values, formulas, final writers, consumers, or lifecycle behavior;
- simulate an input/value specifically to prove a hypothesis;
- contain read/write diagnostic probes whose presence adds another interception/order dependency to the path being accepted.

Any behavior-mutating or path-intercepting research probe must be a separate temporary DLL and separate research/candidate branch, with its own exact question and research-method checkpoint. Removing that probe DLL must leave the neutral Test Console available, so production can be tested without probe influence while retaining setup conveniences.

Do not add future probes back into the neutral Test Console merely because the UI already exists there.

## Repository policy

Long-lived findings belong primarily in:

- `docs/PRAYER_MECHANICS.md` — stock 1.407 mechanics;
- `docs/PLAYER_UX_RESEARCH.md` — presentation/player evidence;
- `docs/PRAYER_DESIGN_AUDIT.md` — role/balance/fix judgements;
- `docs/PRAYER_POWER_BUDGET.md` — quantitative full-cost/progression analysis;
- `docs/PRAYER_REBALANCE_OPTIONS.md` — canonical accepted Rebalanced roster/ruleset for the current stable edition;
- `docs/DESIGN_NOTES.md` — product/UI architecture and accepted design direction;
- `docs/PULPIT_REVEAL_UX.md` — accepted pre-sermon reward-reveal boundary;
- `docs/TEST_BUILD_LOG.md` — only when distributable/testable production candidates exist.
- `docs/CHATGPT_PROJECT_INSTRUCTIONS.md` — canonical thin bootstrap for the ChatGPT Project settings field; keep mutable development state out of it.

Create additional design-analysis files only when they become durable sources of truth rather than temporary scratch work.

Do not commit game DLLs, full decompiled source, proprietary assets or bulk localization/resources. Store only minimal derived facts, IDs, signatures, formulas, hashes and conclusions.

`main` is stable. Use `research/*` for evidence/design gathering and `dev/*` for runtime implementation. Unaccepted runtime behavior stays off `main`.

Do not create hosted CI for routine research/docs/bookkeeping. Use hosted CI only when a concrete executable property requires it, and preserve the clean build/handoff gate for any binary given to the user.

## Shared Graveyard Keeper research

Cross-project Graveyard Keeper 1.407 host/runtime research is centralized in `NikichMods/GraveyardKeeperResearch`.

Before starting a fresh investigation into vanilla/game-engine/UI/NGUI/data/lifecycle behavior:

1. read this repository's own canonical verified-data / architecture docs first;
2. consult `NikichMods/GraveyardKeeperResearch/docs/RESEARCH_INDEX.md` and the linked shared knowledge documents;
3. search accepted local/shared test evidence and relevant history if the result has not yet been promoted;
4. perform new static/runtime research or a probe only if the question remains open.

Project-specific mechanics, product/UX decisions, release state, and build acceptance remain canonical in this repository. Reusable host/runtime facts that can serve multiple Graveyard Keeper mods should be promoted back into the shared research repository after acceptance rather than left only in chat, commit history, or a test log.

