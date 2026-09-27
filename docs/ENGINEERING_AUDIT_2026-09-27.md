# Engineering Audit — 2026-09-27

## Scope

Full engineering/structural audit of the stable PrayerClarity siblings:

- Vanilla 1.0.57;
- Rebalanced 0.2.51;
- exact accepted runtime source `13c85c824bd2932b1175200368d212e03bbae79f`.

This audit is broader than the earlier mechanic/save-lifecycle post-audit. It reviews host integration, failure containment, compatibility policy, runtime state, logging/supportability, version identity, CI/release structure, repository consistency and unnecessary complexity.

## Established strengths

The current architecture remains fundamentally sound:

- host-owned calculations/lifecycle are preserved wherever a verified native seam can express the required behavior;
- no background threads, broad recurring Unity scans or production diagnostic polling were found;
- runtime-sensitive modifiers are scoped to narrow native consumers and restored through finalizers where required;
- persistent tier state is native player-param state and remains inert without the corresponding live prayer buff;
- Repose Gold's scoped body-catalog projection retains the already accepted owner/restoration/runtime evidence;
- the exact Graveyard Keeper 1.407 `Assembly-CSharp` identity is known and remains the verified target;
- candidate/accepted artifact identity, localization validation and immutable handoff/release discipline are strong.

The audit does **not** justify broad mechanic rewrites or architecture churn.

## Findings requiring hardening

### 1. Startup patch activation was not atomic

Both plugins installed Harmony patches incrementally. A later initialization failure could leave earlier PrayerClarity-owned patches installed for the rest of the session.

Selected solution: owner-scoped patch transaction/rollback. Only PrayerClarity owner IDs may be removed; global Harmony unpatch is forbidden. Optional presentation modules use savepoints so one optional failure can degrade independently when its rollback succeeds.

### 2. Rebalanced static projection was not atomic

`RebalancedStaticProjection.ApplyOnce()` mutated shared definitions sequentially. An exception after earlier mutations set `_projectionFailed` but did not restore those earlier changes.

Selected solution: capture every affected scalar member/list before the first projection mutation, restore exact pre-projection state on failure, and keep projection-dependent Rebalanced consumers disabled unless the transaction reaches `ready`.

### 3. Build identity was duplicated

Current version numbers were repeated in project metadata, BepInPlugin constants and workflows. This already produced a real stale-workflow correction after the 1.0.57 release.

Selected solution: each edition's project `<Version>` is the single authored build identity. MSBuild generates the compile-time BepInPlugin constant and CI queries the same project property for artifact/BUILD_INFO naming.

### 4. Compatibility admission was too strict

The stable plugins treated one known `Assembly-CSharp` MVID as an execution permission gate. This is safe but unnecessarily excludes semantically compatible storefront/rebuilt assemblies.

Selected policy:

- known 1.407 MVID -> `verified`;
- other MVID -> `unverified`, one support warning, best-effort activation through exact existing contracts;
- missing/changed required contracts -> contained failure/rollback, never guessed alternate targets.

No transient in-game compatibility overlay is added: it would itself introduce an extra unverified UI/lifecycle dependency. The BepInEx log is the canonical support surface.

### 5. Production logging lacked a common support contract

PrayerClarity was already low-noise: recurring runtime failures were mostly log-once and no per-frame production logging was found. The problem was inconsistency and missing environment identity.

Selected logging contract:

- sparse `PC_START` / `PC_READY` lifecycle identity;
- `PC_COMPAT_UNVERIFIED` once for unknown host identity;
- stable grep-friendly `PC_RUNTIME_FALLBACK`, rollback and projection event identifiers;
- successful internal operations at `Debug` or unlogged;
- unexpected exceptions retain their stack trace.

The same principles were promoted globally to `NikichMods/DevRules` in PR #4.

## Repository findings

- README incorrectly claimed the current project license was MIT while `LICENSE`, `src/LICENSE` and `LICENSING.md` establish MPL-2.0 for current source.
- `PRAYER_REBALANCE_OPTIONS.md` still contained an obsolete immutable-bytes sentence naming 0.2.38.
- an old BSS source comment still named Vanilla 1.0.33 as the accepted sibling byte identity.

These are repository consistency issues, not runtime defects.

## Deferred hypothesis: Technology carousel state lifetime

`TechnologyPrayerCarousel` stores parent `TechTreeGUIItem` state in a static reference-keyed dictionary. Accepted shared research proves Technology focus/input ownership but does not establish whether fresh parent items are created repeatedly across tree reopen cycles.

Therefore a leak is **not established**. Do not replace the dictionary merely because a `ConditionalWeakTable` appears aesthetically safer. Re-open only if direct lifecycle evidence or memory behavior shows repeated object creation/retention.

## Candidate hardening

The above runtime hardening is being implemented under the per-change READY gates in `docs/CANDIDATE_GATE.json` for:

- Rebalanced 0.2.52;
- Vanilla 1.0.58.

Until runtime/build acceptance, stable releases remain Vanilla 1.0.57 and Rebalanced 0.2.51.

## Runtime smoke — Rebalanced 0.2.52

Accepted on exact runtime artifact source `9a236d217ca335ac944087dcbb23504bb391fe26`.

The returned Graveyard Keeper 1.407 BepInEx log shows Rebalanced 0.2.52 starting with the verified `Assembly-CSharp` MVID and reaching `PC_READY`. The save then completes the normal craft-list loading path and enters gameplay. No PrayerClarity compatibility warning, initialization failure, rollback failure, static-projection failure or runtime-fallback event appears in the complete log.

Result: the engineering-hardening runtime smoke is accepted for Rebalanced 0.2.52 on the verified 1.407 host. Stable promotion remains separate.

