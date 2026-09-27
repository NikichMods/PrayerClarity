# PrayerClarity 0.2.52 fault-injection research

Base runtime source: `9a236d217ca335ac944087dcbb23504bb391fe26`.

Purpose: prove the failure-containment paths added by the 0.2.52 engineering hardening without adding test switches to the production candidate. These builds are research-only and must never be promoted or released.

## Scenarios

### unverified

Forces the otherwise verified 1.407 MVID to be classified as unverified.

Expected:
- `PC_COMPAT_UNVERIFIED`;
- normal `PC_READY`;
- `PC_TEST_ASSERT scenario=unverified ... continued_to_ready=PASS`;
- after loading a save, `projection_ready=PASS`.

### optional-rollback

Allows the Technology Prayer Carousel to install, then throws immediately afterward inside the optional-feature savepoint.

Expected:
- `PC_FEATURE_DISABLED ... feature=technology-carousel rollback=success`;
- `PC_TEST_ASSERT ... rollback=PASS owner_cleanup=PASS`;
- normal `PC_READY`;
- after loading a save, `projection_after_optional_failure=PASS`.

### core-rollback

Throws after Rebalanced Combat has installed, so multiple PrayerClarity owners already exist.

Expected:
- no `PC_READY`;
- `PC_INIT_FAILED ... rollback=success`;
- `PC_TEST_ASSERT ... owner_cleanup=PASS semantics_reset=PASS`.

### projection-rollback

Allows the static projection to mutate Combat expression data and legacy Combat aliases, then throws before the remaining projection continues.

Expected after loading a save:
- `PC_STATIC_PROJECTION_FAILED ... rollback=success runtime=disabled`;
- `PC_TEST_ASSERT ... rollback=PASS snapshot_restore=PASS runtime_disabled=PASS`.

The assertion compares every member/list captured by the production projection snapshot against live post-rollback state.

## User operation

Install **one** fault DLL at a time in place of the normal Rebalanced DLL. Do not install several together because all four intentionally use the same BepInEx plugin GUID.

For a uniform procedure, launch the game, load the same save to active gameplay, quit, and keep the resulting full `LogOutput.log`. The core-rollback build will intentionally run without Rebalanced after startup failure; loading the save is still safe and keeps the procedure consistent.

These DLLs are not gameplay candidates.
