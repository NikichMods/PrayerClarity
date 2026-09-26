# Test / Research Build Log

## 2026-09-27 — Rebalanced 0.2.49 Technology alignment partial failure

- Exact tested candidate: Rebalanced **0.2.49**, source `8e95d45a4b501c2852428eb8d4d37883be93b7c4`.
- Runtime result: **superseded before acceptance**.
- User evidence:
  - prayer Technology headers remained visibly left-aligned with mouse;
  - prayer Technology headers were centered with gamepad;
  - prayer-item headers were centered as expected in both input modes.
- The 0.2.49 data-level change itself did set the prayer title, Base Result and On Success rows to `Center`. The failed assumption was that row text alignment alone owned the visible horizontal position.
- Host/final-writer proof:
  - `Tooltip.Show` copies the concrete Tooltip component's alignment into the `BubbleWidgetDataContainer`;
  - `WidgetsBubbleGUI.Redraw` maps container `Left` to `SimpleUITable.TopLeft` / `UITable.TopLeft`;
  - `WidgetsBubbleGUI.Reposition` additionally gives labels a Left pivot when the container is Left;
  - therefore a short child UILabel can have centered text but still sit at the left edge as a narrow child widget.
- This exactly explains the mouse/gamepad split: the mouse child Technology tooltip and gamepad parent tooltip use different container placement contexts.
- 0.2.49 must not be promoted. The next correction keeps the accepted Center row alignment but changes only the centered header child span at the stock `UpdateSizeAndWidgetsPositions` commit point, up to the already-existing native maximum width.

## 2026-09-27 — Rebalanced 0.2.49 / Vanilla 1.0.55 Technology heading-alignment candidate

- Baseline: accepted Rebalanced 0.2.48 / `accepted/rebalanced-0.2.48`, exact source `42b284986f768b6dd1e1760f7d80c8e5763765b2`.
- Candidate branch: `candidate/rebalanced-0.2.49`.
- Gate-only commit: `404d5ff78dd8330d847406ea220ff506596a3284`.
- Exact candidate source SHA: `8e95d45a4b501c2852428eb8d4d37883be93b7c4`.
- CI run: `36272619150`; result: **success**.
- Artifact ID: `10915983019`.
- Artifact: `PrayerClarity-rebalanced-0.2.49-ci-8e95d45a4b501c2852428eb8d4d37883be93b7c4`.
- Artifact ZIP digest: `sha256:82a8d69fd8d1dd5c219d6291af41d50cc213b359d97b17a598414c7d46bcb6e4`.
- Rebalanced 0.2.49 DLL SHA-256: `f3d42a45e539a5af1de5f128158588daeaf74c82b700f728dfac479033cabbce`.
- Shared sibling Vanilla 1.0.55 DLL SHA-256: `c6c3437538598e112fc941477a2c1a6bbbf823637313d2ef30d70ecf9d6f41b9`.
- Alignment policy:
  - top prayer Technology title (`Create: Prayer ...`) = **Center**;
  - `Base Result` header = **Center**;
  - `On Success` header = **Center**;
  - Base Result / On Success mechanics content remains **Left**;
  - lore/description and crafting-source alignment remain stock-owned.
- Shared-path proof:
  - mouse prayer unlocks use stock `TechUnlock.GetTooltip`;
  - stock gamepad Technology assembly uses the same `TechUnlock.GetTooltip`;
  - PrayerClarity's gamepad prayer carousel rebuilds the selected child by invoking that same method;
  - therefore the data-level alignment normalization applies identically to mouse/gamepad and Vanilla/Rebalanced.
- No width, viewport clamp, carousel navigation, prayer-item, pulpit, Temporary Effects, localization or mechanics behavior changed.
- Focused runtime acceptance: inspect one prayer Technology with mouse and one with gamepad. Confirm the three headers are centered, content remains left-aligned, and the tooltip size/position is unchanged. A quick Vanilla sibling check is useful to confirm the shared path, but no separate mechanics test is required.

## 2026-09-27 — Rebalanced 0.2.48 runtime accepted

- User runtime acceptance: **2026-09-27** — Combo Prayer showed the intended values consistently in Technology, a concrete prayer item and the pulpit; no problems were observed.
- Frozen accepted ref: `accepted/rebalanced-0.2.48`.
- Exact accepted runtime/source SHA: `42b284986f768b6dd1e1760f7d80c8e5763765b2`.
- Accepted Rebalanced DLL SHA-256: `38d51a9647d67b1b14f70a9045b8bbf493dae4b434251284bbab22c2bf1f10e3`.
- Accepted Combo values:
  - Faith: **+100 / +150 / +200%**;
  - donations: **+100 / +250 / +500%**;
  - q40 / q60 / q80 and the Hard Book +7 Faith craft gate remain unchanged.
- The native `k_money` calculation path was not changed and did not require sermon replay.
- This accepted ref becomes the Rebalanced development/runtime baseline for subsequent candidates.
- Public stable remains Rebalanced **0.2.47** until a separate promotion decision.

## 2026-09-27 — Rebalanced 0.2.48 Combo donation-scaling candidate

- Stable runtime remains Rebalanced 0.2.47 / `accepted/rebalanced-0.2.47`. Public Vanilla remains 1.0.33.
- Baseline repository SHA: `cd0d751b80b58c4a4ea93548822a6f37f4f5935a`.
- Candidate branch: `candidate/rebalanced-0.2.48`.
- Gate-only commit: `02cc5619811db949dd4f8c6fbfb81ed787c5b526`.
- Exact candidate source SHA: `42b284986f768b6dd1e1760f7d80c8e5763765b2`.
- CI run: `36271945212`; result: **success**.
- Artifact ID: `10916191327`.
- Artifact: `PrayerClarity-rebalanced-0.2.48-ci-42b284986f768b6dd1e1760f7d80c8e5763765b2`.
- Artifact ZIP digest: `sha256:31062e3173af49609813c1f2926f0be586f4cabedaa0e2da2cb3d563a876fbce`.
- Rebalanced 0.2.48 DLL SHA-256: `38d51a9647d67b1b14f70a9045b8bbf493dae4b434251284bbab22c2bf1f10e3`.
- Shared sibling Vanilla 1.0.54 DLL SHA-256: `f6266985ad5f21595b47d3f7e6568be0ee46399ac271e5ba64df83b7adb7ad2e`.
- Production delta is intentionally one balance rule:
  - Combo Prayer Faith remains **+100 / +150 / +200%**;
  - Combo Prayer donations change **+100 / +200 / +300% -> +100 / +250 / +500%**;
  - q40 / q60 / q80, Hard Book +7 Faith gate, fixed-output removal and all other prayers remain unchanged.
- Ownership evidence: `RebalancedRuleSet` owns the three rates; `RebalancedStaticProjection.ApplyStockOwnedFields` writes them directly to stock `CraftDefinition.k_money`; stock `PrayLogics.CalculatePray` remains the final money calculation consumer. PrayerForecast/UI reads the same effective craft field.
- Balance intent: against the accepted Donations specialist (+20 / +50 / +100 silver), all three same-quality pure-money crossovers become approximately **GQ 500 with Cardinal** or **GQ 667 without Cardinal**.
- Focused acceptance: inspect Combo Prayer in Technology, one concrete prayer-item tooltip and the pulpit. Expected donation values are **100% / 250% / 500%** while Faith remains **100% / 150% / 200%**. No sermon execution is required because the native `k_money` calculation path is unchanged and already accepted.

## 2026-09-26 — Rebalanced 0.2.47 accepted stable

- User runtime acceptance: **2026-09-26** — all requested 0.2.47 visual changes worked as expected and no problems were observed.
- Frozen accepted ref: `accepted/rebalanced-0.2.47`.
- Exact accepted runtime/source SHA: `6b3aa5399c8913d368f2b09bab963326db17e7f3`.
- CI run: `36266833620`; result: **success**.
- Artifact ID: `10913543057`.
- Artifact: `PrayerClarity-rebalanced-0.2.47-ci-6b3aa5399c8913d368f2b09bab963326db17e7f3`.
- Artifact ZIP digest: `sha256:5c07d5b4922467cf239a20f87e64c5ea8036e036dd8e84e6b72386d9645bedb1`.
- Accepted Rebalanced DLL SHA-256: `2482a159c3108a7868123d7c2cac7a8a620fe537952fee0cda6024c6ea5c7c96`.
- Accepted presentation closure:
  1. prayer-item parchment uses the game's standard native width;
  2. the 100%-success requirement content is centered and remains coherent when it wraps;
  3. Base Result and On Success content share a visible left edge while their section headers remain centered;
  4. Repose wording/presentation is consistent across Technology, item, ordinary pulpit, terminal pulpit and Temporary Effects;
  5. terminal Repose states no longer imply that another ordinary body-quality tier can still be unlocked when the natural ceiling has been reached.
- Prayer mechanics, balance, requirements, durations, payouts, corpse-generation behavior and RNG remain unchanged from the accepted Rebalanced ruleset.
- Public sibling Vanilla remains **1.0.33**. The 1.0.53 sibling binary produced by the shared candidate workflow is not promoted by this Rebalanced release.
- Stable promotion merge: `29e300b2d11bc6936e19ed8d9522caceecccbca2` (PR #40).
- Publication workflow run: `36271343039`; result: **success**.
- Stable release/tag: `rebalanced-v0.2.47`; canonical asset: `PrayerClarity.Rebalanced.dll`, published from the exact accepted candidate bytes without rebuilding.
- Publication verification re-downloaded the release asset and confirmed SHA-256 `2482a159c3108a7868123d7c2cac7a8a620fe537952fee0cda6024c6ea5c7c96`; the tag resolves to exact accepted source `6b3aa5399c8913d368f2b09bab963326db17e7f3`.
- No further in-game test is required unless the accepted implementation changes.

## 2026-09-26 — Rebalanced 0.2.47 / Vanilla 1.0.53 native-span left content candidate

- Stable runtimes remain Rebalanced 0.2.38 / `accepted/rebalanced-0.2.38` and Vanilla 1.0.33 / `accepted/vanilla-1.0.33`.
- Baseline: Rebalanced 0.2.46 exact source `ef027959871b9f520cee73be88ce381306b30c10`.
- Runtime result of 0.2.46: **requirement centering accepted; one pre-existing visual alignment defect remains and supersedes the candidate before full acceptance**.
  - the 100%-success requirement content is now centered correctly under standard vanilla width;
  - the screenshot also proves that a short one-line On Success row such as the Donations `+20` still appears globally centered even though its `BubbleWidgetTextData.alignment` is already `Left`;
  - Base Result and On Success were therefore not accidentally switched to Center at the data level.
- Direct host inspection explains the mismatch:
  - `BubbleWidgetText.Draw` applies `data.alignment` only inside that row's UILabel;
  - `WidgetsBubbleGUI` with Center container alignment centers each child widget as a whole;
  - `UpdateSize` chooses the bubble width from the maximum child widget width, then `Reposition` lays out the rows.
  A short one-line Left row can therefore still look centered because its UILabel itself is only as wide as its text.
- Solution-space checkpoint:
  - setting `Left` again is ineffective because it is already Left;
  - changing the whole container to Left would also move centered headings;
  - returning to fixed 200/280/420 widths is rejected by prior runtime evidence;
  - selected: at `WidgetsBubbleGUI.UpdateSizeAndWidgetsPositions`, measure the already-drawn native child widths and expand only PrayerClarity-marked Left content rows to that same existing maximum. This preserves the native outer parchment width while giving Base Result and On Success a shared left content edge.
- Candidate branch: `candidate/rebalanced-0.2.47`.
- Gate-only commit: `0574036e678f5aa47a40a3d6e74554a0f4397f3d`.
- Exact candidate source SHA: `6b3aa5399c8913d368f2b09bab963326db17e7f3`.
- CI run: `36266833620`; result: **success**. Production gate, localization validation, both sibling builds, staging and upload all passed.
- Artifact ID: `10913543057`.
- Artifact: `PrayerClarity-rebalanced-0.2.47-ci-6b3aa5399c8913d368f2b09bab963326db17e7f3`.
- Artifact ZIP digest: `sha256:5c07d5b4922467cf239a20f87e64c5ea8036e036dd8e84e6b72386d9645bedb1`.
- Rebalanced 0.2.47 DLL SHA-256: `2482a159c3108a7868123d7c2cac7a8a620fe537952fee0cda6024c6ea5c7c96`.
- Shared sibling Vanilla 1.0.53 DLL SHA-256: `eecbcddb5b06856802cde14812acab7a593846c3c7fb950971f25311a15294f5`.
- Scope:
  - Base Result and On Success content rows remain semantically `Left`;
  - only those marked Left rows are widened to the already-existing native maximum child width before stock `UpdateSize/Reposition`;
  - centered section headers and centered 100%-success requirement are untouched;
  - the amount+inline-resource repair is re-evaluated at the final content-row width;
  - outer tooltip width, wording, values and all prayer mechanics remain unchanged.
- Focused runtime acceptance:
  1. Prayer for Donations (or another one-line On Success prayer): `+money/+faith` must begin on the same left content column as Base Result while the `On Success` header stays centered;
  2. one multi-line prayer: Base Result and On Success lines should share a clean left edge;
  3. confirm the 100%-success requirement remains centered and the parchment width is unchanged from 0.2.46.
- No Repose, sermon, Temporary Effects, corpse-generation or balance retest is required.

## 2026-09-26 — Rebalanced 0.2.46 / Vanilla 1.0.52 centered prayer-item success requirement candidate

- Stable runtimes remain Rebalanced 0.2.38 / `accepted/rebalanced-0.2.38` and Vanilla 1.0.33 / `accepted/vanilla-1.0.33`.
- Baseline: Rebalanced 0.2.45 exact source `42feed5758e22118c5c1ef1ab573ad2e7bd10039`.
- Runtime result of 0.2.45: **Repose and vanilla-width direction accepted; one shared prayer-item alignment polish supersedes the candidate before full acceptance**.
  - Repose item text, ordinary pulpit, terminal pulpit and Character -> Temporary Effects were all reported correct.
  - Terminal Bronze quality stars and the one-line-per-tier comparison were accepted.
  - Returning prayer-item geometry to the game's standard vanilla width was accepted as the correct width policy.
  - Under that standard width, Russian prayer items consistently wrap the final Church Quality number + cross of the 100%-success requirement onto a second line. Functionally correct, but left alignment makes the wrapped block look accidental.
- Candidate branch: `candidate/rebalanced-0.2.46`.
- Gate-only commit: `87226b8c7e6ca631b56e4ba40e301272f5bb638e`.
- Exact candidate source SHA: `ef027959871b9f520cee73be88ce381306b30c10`.
- CI run: `36265634079`; result: **success**. Production gate, 11-locale localization validation, both sibling builds, staging and upload all passed.
- Artifact ID: `10914431514`.
- Artifact: `PrayerClarity-rebalanced-0.2.46-ci-ef027959871b9f520cee73be88ce381306b30c10`.
- Artifact ZIP digest: `sha256:904cf1752329eac4483da96f17db47c5368587c6680e984e3efb95ad844912a7`.
- Rebalanced 0.2.46 DLL SHA-256: `789504068d4ab33f988ee55ad2a18fbaed787163f970ff442d81ec6cd45bfba3`.
- Shared sibling Vanilla 1.0.52 DLL SHA-256: `543e6076ca24340415340475290099bbb95f0e14fddb9702767608cfce7f7264`.
- Production change is intentionally one line: the PrayerClarity-owned prayer-item requirement content row changes alignment from `Left` to `Center`.
- Preserved:
  - game-owned standard prayer-item width/height/overflow;
  - requirement text/value and normal NGUI wrapping;
  - centered section header;
  - left alignment for Base Result and On Success content;
  - accepted amount + inline-resource wrap repair;
  - all Technology, pulpit, Temporary Effects, prayer mechanics and balance.
- Focused runtime acceptance: inspect one Russian prayer whose requirement wraps (for example Silver Repose / Faith) and one whose requirement does not. Both lines of a wrapped requirement must read as one centered block; parchment geometry and all other section alignments must remain unchanged.
- No Repose state-switching, sermon execution, corpse generation, Temporary Effects replay or other prayer test is required.

## 2026-09-26 — Rebalanced 0.2.45 / Vanilla 1.0.51 vanilla-width + final Repose wording candidate

- Stable runtimes remain Rebalanced 0.2.38 / `accepted/rebalanced-0.2.38` and Vanilla 1.0.33 / `accepted/vanilla-1.0.33`.
- Baseline: Rebalanced 0.2.44 exact source `7886a5cb860842aa1df139a84e81d69622dc7be1`.
- Runtime result of 0.2.44: **partial pass / superseded by 0.2.45 before acceptance**.
  - Repose item text structure was reported correct/readable.
  - Terminal Bronze quality stars rendered correctly and the one-line-per-tier scan pattern was accepted.
  - The fixed 280-unit prayer-item mechanics column still produced visibly non-native/unbalanced parchment geometry, so the third custom-width attempt was rejected.
  - Product decision: stop owning prayer-item width entirely and return marked mechanics rows to the game's standard item-tooltip geometry.
  - Repose wording received one final clarity refinement: Bronze explicitly says the best body is not guaranteed; terminal Silver/Gold explicitly refer to the bodies the Donkey can already bring.
- Candidate branch: `candidate/rebalanced-0.2.45`.
- Gate-only commit: `afeb8a9740a92c76bc041cfb943fe753683d6cbc`.
- Exact candidate source SHA: `42feed5758e22118c5c1ef1ab573ad2e7bd10039`.
- CI run: `36263711336`; result: **success**. Gate validation, 11-locale contract validation, both sibling builds, staging and upload all passed.
- Artifact ID: `10913227944`.
- Artifact: `PrayerClarity-rebalanced-0.2.45-ci-42feed5758e22118c5c1ef1ab573ad2e7bd10039`.
- Artifact ZIP digest: `sha256:92407344af9bcd3526b19f2fb084f5da412786950a5687d08bf021d6258b4bdc`.
- Rebalanced 0.2.45 DLL SHA-256: `43e98d1819e36a334ee1412ee7fccd2f53b10e91b914e49507689fe6e447e46f`.
- Shared sibling Vanilla 1.0.51 DLL SHA-256: `a9ad41486eb9008aec383ad0541fdca8df0cf826a5cce69eb6f4898db681af78`.
- Prayer-item geometry:
  - PrayerClarity still marks its own inserted mechanics rows so the accepted amount+inline-resource final-wrap repair can remain narrowly scoped.
  - For those rows, PrayerClarity no longer writes `UILabel.width`, `height`, `overflowMethod` or `overflowWidth`; standard game item-tooltip geometry is authoritative again.
  - Technology-tooltip width behavior is unchanged.
- Rebalanced Repose wording:
  - concrete Bronze forecast: the Donkey can bring higher-quality bodies, **but getting the best body is not guaranteed**;
  - concrete Silver/Gold forecast: shared higher-quality-body effect plus the quality-specific reliability result;
  - Technology remains compact: shared effect once, then Bronze “no guarantee of the best”, Silver improved chance, Gold guarantee;
  - terminal Bronze retains the accepted parenthetical three-line `(s1)/(s2)/(s3)` presentation;
  - terminal Silver/Gold explicitly describe the best among the bodies the Donkey can **already** bring, and still omit the false higher-tier sentence.
- Vanilla does not receive Rebalanced reliability semantics. Its only 1.0.51 runtime change is the shared return to standard item-tooltip width ownership.
- Focused runtime acceptance:
  1. inspect Repose, Imagination/another ordinary prayer, and Prosperity/another long-line prayer as items; parchment width should now look like ordinary vanilla item tooltips, while amount+resource pairs remain intact;
  2. inspect Repose Technology and Bronze/Silver/Gold item text for the final reliability wording;
  3. inspect Early Silver/Gold Repose at the pulpit: shared higher-quality-body sentence + reliability sentence;
  4. inspect terminal Bronze/Silver/Gold: accepted star-per-line Bronze note; Silver/Gold “among bodies the Donkey can already bring” wording.
- No sermon execution, corpse generation, Temporary Effects replay, Imagination payout replay, or mechanics test is required.

## 2026-09-26 — Rebalanced 0.2.44 / Vanilla 1.0.50 Repose wording + stable prayer-item width candidate

- Stable runtimes remain Rebalanced 0.2.38 / `accepted/rebalanced-0.2.38` and Vanilla 1.0.33 / `accepted/vanilla-1.0.33`.
- Baseline: Rebalanced 0.2.43 exact source `d19902ae7f699384334f7b8b53e9c88b5d9e86c9`.
- Runtime result of 0.2.43: **partial pass / superseded by 0.2.44 before acceptance**.
  - accepted: terminal Bronze no longer shows the obsolete corpse-quality cue after the natural-limit sentence;
  - accepted: Character -> Temporary Effects now reflects terminal Repose state correctly;
  - accepted: Neutral Test Console 0.1.18 synthetic buff setup/removal works through the native buff API;
  - rejected: returning prayer-item mechanics rows to `ResizeFreely + overflowWidth=420` lets one long line determine the width of the whole parchment;
  - wording follow-up: Repose reliability language should read as the best body the Donkey can currently/possibly bring, and terminal Bronze should show one glyph-led line per quality tier.
- Candidate branch: `candidate/rebalanced-0.2.44`.
- Gate-only commit: `45537221bfb2816dc88867c3483060af20c6ddff`.
- Exact candidate source SHA: `7886a5cb860842aa1df139a84e81d69622dc7be1`.
- CI run: `36261054849`; result: **success**. Production gate, localization validation, both sibling builds, staging and artifact upload all passed.
- Artifact ID: `10912691303`.
- Artifact: `PrayerClarity-rebalanced-0.2.44-ci-7886a5cb860842aa1df139a84e81d69622dc7be1`.
- Artifact ZIP digest: `sha256:690dc87946206b59068b7d217db895392b35e146dc1a78afb2cb2f80c3c68899`.
- Rebalanced 0.2.44 DLL SHA-256: `d31645878437ee591ba83ec096492d3764078e521494d94d5bdd4909f759c8c4`.
- Shared sibling Vanilla 1.0.50 DLL SHA-256: `248f00aeedf800c5b44574922e1d6c144a8478ce6604fdaf28a94e625bd21d92`.
- Repose wording:
  - Russian Silver/Gold now say `лучшее из возможных тел`; other locales use the equivalent meaning “the best body the Donkey can bring” rather than a literal translation that could imply absolute endgame maximum;
  - terminal Bronze remains one parenthetical note but is split into three lines beginning with native `(s1)/(s2)/(s3)` quality glyphs;
  - terminal Silver/Gold intentionally continue to omit the shared “higher-quality bodies” sentence, because that statement would be false once no higher ordinary tier exists.
- Prayer-item width solution-space checkpoint was re-opened after both previous extremes failed:
  - fixed 200-unit `ResizeHeight` column: rejected by runtime evidence as visibly too narrow / leaving unused parchment;
  - native `ResizeFreely + overflowWidth=420`: rejected by 0.2.43 runtime evidence because one longest line expands the whole parchment;
  - selected: fixed **280-unit** `ResizeHeight` mechanics column, keeping the accepted final-wrap amount+inline-icon repair. No new placement/lifecycle hook is added.
- Focused runtime acceptance for 0.2.44:
  1. Repose Technology/item wording: confirm the new “possible/currently possible” reliability language reads naturally;
  2. terminal Bronze pulpit: confirm three separate quality-glyph lines inside the note, with clean wrapping/button clearance;
  3. Early-state Silver/Gold pulpit via Repose State Switcher: confirm the ordinary pulpit already shows the shared higher-quality-body line plus the quality-specific reliability line;
  4. terminal Silver/Gold pulpit: confirm only the still-true reliability line remains;
  5. prayer-item geometry: inspect Repose, Prosperity (or another long-line prayer) and one short prayer; width should remain stable and bounded, with no clipping or orphaned amount/resource icon.
- No sermon execution, corpse generation, active-buff replay or Imagination replay is required.

## 2026-09-26 — Imagination Silver/Gold physical payout accepted

- Real successful Silver Imagination sermon was observed in the user's runtime and the game collected `story:2 ×3`.
- Real successful Gold Imagination sermon was observed in the user's runtime and the game collected `story:3 ×3`.
- This closes the previously documented non-blocking evidence gap for the physical three-Story payout path. No further Imagination payout replay is required unless the implementation changes.

## 2026-09-26 — Rebalanced 0.2.43 / Vanilla 1.0.49 Repose terminal-sync + prayer-item width candidate

- Stable runtimes remain Rebalanced 0.2.38 / `accepted/rebalanced-0.2.38` and Vanilla 1.0.33 / `accepted/vanilla-1.0.33`.
- Baseline: Rebalanced 0.2.42 exact source `dd683e5c369a9056151ab43ae5eb18da2522b33d`.
- Runtime review of 0.2.42 found three presentation defects:
  1. terminal Bronze Repose still showed the corpse-quality glyph cluster after the natural-limit sentence, although no higher ordinary quality tier remains;
  2. the Bronze terminal comparison needed to read as a parenthetical note with explicit Bronze/Silver/Gold **prayer** names;
  3. Character -> Temporary Effects still said the Donkey can bring higher-quality bodies at the terminal state, which is false once the ordinary body-quality ceiling is reached.
- The same runtime pass also exposed a separate shared prayer-item layout regression: the fixed 200-unit mechanics column introduced in 0.2.29 leaves a conspicuous one-sided empty strip on ordinary prayer-item parchment.
- Candidate branch: `candidate/rebalanced-0.2.43`.
- Gate-only commit: `515e6cd3fb06c2cd7aee1406c1277524e6452820`.
- Exact candidate source SHA: `d19902ae7f699384334f7b8b53e9c88b5d9e86c9`.
- CI run: `36259045391`; result: **success**. Gate/localization/build/staging all passed.
- Artifact ID: `10911891530`.
- Artifact: `PrayerClarity-rebalanced-0.2.43-ci-d19902ae7f699384334f7b8b53e9c88b5d9e86c9`.
- Artifact ZIP digest: `sha256:0964ddcc9a809f4297ff37ef4cc7cd79cfa28228a8931d749dbdc9d7f857abc1`.
- Rebalanced 0.2.43 DLL SHA-256: `1bfe78dfeaeea3cc9b89c1dec7ab9df3b4e7ec9d696c5488d43b5250cb4a71a5`.
- Shared sibling Vanilla 1.0.49 DLL SHA-256: `0415f86977a275b02a211001c96c46cea1c0821a28cdaf65060001d74871ca32`.
- Terminal Repose:
  - shared natural-limit text no longer carries `(up)/(skull)/(rskull)`;
  - Rebalanced Bronze comparison is parenthetical and explicitly names Bronze/Silver/Gold prayers;
  - terminal Silver/Gold working-only reliability rows keep the corpse-quality cue.
- Active Repose Temporary Effects now consult the same accepted corpse-tier semantics as the pulpit. At the terminal state Bronze shows the natural-limit message; Silver/Gold show only their still-working reliability benefit while distribution narrowing remains meaningful.
- Prayer-item mechanics rows return from the fixed `ResizeHeight width=200` column to the narrower/native `ResizeFreely + overflowWidth=420` path. The accepted conditional final-wrap repair that keeps `amount + inline resource icon` together remains in the same final `BubbleWidgetText.Draw` postfix.
- No prayer mechanics, Repose endpoint predicates, corpse generation/distribution, requirements, durations, RNG, Technology layout, non-prayer item tooltips, or unrelated prayer behavior changed.
- Focused runtime acceptance:
  1. terminal Bronze Repose at the pulpit via Repose State Switcher: no corpse-quality glyphs on the natural-limit line; parenthetical note uses explicit prayer names; no clipping/button overlap;
  2. activate Repose synthetically through Neutral Test Console 0.1.18 and inspect Character -> Temporary Effects in Early vs Terminal state;
  3. inspect Repose item, a long Soul's Repose item and one short prayer item: parchment use should be visually balanced, with no excessive width, clipping or orphaned amount/resource icons.
- No sermon execution or corpse-generation sampling is required.

## 2026-09-26 — Neutral Test Console 0.1.18 restores native synthetic prayer-buff setup

- Branch: `candidate/rebalanced-test-console-0.1.18`.
- Exact source SHA: `1e604fc66a1f32ce05ddcf037cb7efe8b0830dc5`.
- CI run: `36259211806`; result: **success**.
- Artifact ID: `10910759435`.
- Artifact: `PrayerClarity-RebalancedTestConsole-0.1.18-ci-1e604fc66a1f32ce05ddcf037cb7efe8b0830dc5`.
- Artifact ZIP digest: `sha256:8e509ce31e10a4a45292688ba9d5ee2341fed218f05c9d2717b68865e9faddf9`.
- DLL SHA-256: `03c4e06b8dfb51deacb3f71bdb83bf1a004bd4501e55ded62f9bbc69206c1eeb`.
- The long-lived Neutral Test Console remains hook-free: no Harmony patches, presentation rewrites or diagnostic probes were added.
- It now exposes Bronze/Silver/Gold synthetic activation/removal for the verified timed prayer-buff families through native `BuffsLogics.AddBuff/RemoveBuff`, specifically to avoid replaying a full sermon when the test property is the already-verified active-buff presentation.
- Rebalanced tier tokens needed by tier-dependent active effects are set only as test-state setup; their prior values are captured and restored by console cleanup. Activation is blocked if a non-console copy of the same buff is already live, so a real sermon buff is not silently replaced.
- Prayer craft `dur_parameter` is read from the live projected linked craft instead of duplicating duration tables in the console.
- Synthetic buffs/tier tokens can become save-visible state. The console explicitly warns to run cleanup before saving a permanent playthrough state.

## 2026-09-26 — Rebalanced 0.2.42 / Vanilla 1.0.48 Repose surface-language candidate

- Runtime result: **superseded by 0.2.43 before acceptance**. The surface-language direction was broadly readable, but terminal Bronze retained unnecessary corpse-quality glyphs, its comparison note needed explicit prayer names/parentheses, active Temporary Effects remained contextually false at the terminal body-quality state, and the existing shared 200-unit prayer-item mechanics column was reported as visibly unbalanced.

- Stable runtimes remain Rebalanced 0.2.38 / `accepted/rebalanced-0.2.38` and Vanilla 1.0.33 / `accepted/vanilla-1.0.33`.
- Baseline: Rebalanced 0.2.41 exact source `f571eebd6942f14eaeddcf979c9689feec9b358c`. 0.2.41 is superseded before acceptance because its shared sentence could read as a fourth effect and its tier lines did not make inheritance of the base effect obvious.
- Candidate branch: `candidate/rebalanced-0.2.42`.
- Gate-only commit: `d295ba41e566313a52938e6880601a0869247af5`.
- Exact candidate source SHA: `dd683e5c369a9056151ab43ae5eb18da2522b33d`.
- CI run: `36257362849`; result: **success**.
- Artifact ID: `10910459521`.
- Artifact: `PrayerClarity-rebalanced-0.2.42-ci-dd683e5c369a9056151ab43ae5eb18da2522b33d`.
- Artifact ZIP digest: `sha256:868d5f86f699a701ba22bc4dbe6aac569cfccd75572fd83230dbe88ed91f39d9`.
- Rebalanced 0.2.42 DLL SHA-256: `73ddc586bfd340b31c4c98271015499dfefcd956946579303440e5b97134778f`.
- Shared sibling Vanilla 1.0.48 DLL SHA-256: `455e7acfcf147559ed1745b132cb9ff5e79636203778b1bb2e0c91fcd512ecb0`.
- Player-facing model:
  - forecast/planning surfaces use future tense: the Donkey **will be able** to bring higher-quality bodies;
  - Character -> Temporary Effects uses present tense: the Donkey **can** bring higher-quality bodies;
  - Rebalanced Technology states that shared effect once and shows only the quality delta: Bronze = main effect only, Silver = the chance of the best available body will increase, Gold = the best available body will be guaranteed;
  - a concrete Rebalanced item or ordinary pulpit selection is self-contained: Bronze shows the base effect; Silver/Gold show the base effect plus their reliability delta.
- Terminal pulpit model:
  - shared Vanilla/Rebalanced endpoint wording explains a natural ceiling: no still-higher-quality body can be obtained, even with prayer;
  - terminal Rebalanced Bronze adds a worded Bronze/Silver/Gold explanation rather than star-glyph labels;
  - terminal Rebalanced Silver/Gold, while reliability still changes the distribution, show only their still-working reliability sentence plus the corpse-quality cue; the no-longer-true higher-tier sentence is removed.
- All player-facing changes are synchronized across the 11 supported locales. No Repose mechanics, endpoint predicates, corpse-tier selection/distribution, requirements, duration, RNG, layout/font lifecycle, or unrelated prayer behavior changed.
- Focused runtime acceptance: in Russian Rebalanced 0.2.42, inspect Repose Technology, one Silver/Gold prayer item, an ordinary Silver/Gold pulpit state, the active Repose Temporary Effect, then terminal Bronze/Silver/Gold using Repose State Switcher 0.1.0. Verify meaning, glyphs, wrapping and button clearance. No sermon execution or corpse-generation sampling is required. Vanilla 1.0.48 uses the same accepted renderers and shared base/endpoint strings; its one-line forecast-key substitution is statically covered by source/CI and does not require a separate mechanics replay.

## 2026-09-26 — Rebalanced 0.2.41 Repose terminology-unification candidate

- Stable runtime remains Rebalanced 0.2.38 / `accepted/rebalanced-0.2.38`.
- Baseline for this candidate: Rebalanced 0.2.40 exact source `229d1837123c980b09192d4e7a9b7a143ae573c4`; 0.2.40 is superseded for wording, not mechanics.
- Candidate branch: `candidate/rebalanced-0.2.41`.
- Gate-only commit: `af975ae76876daf10269e62872c9deaa66b04ab4`.
- Exact candidate source SHA: `f571eebd6942f14eaeddcf979c9689feec9b358c`.
- CI run: `36253408463`; result: **success**.
- Artifact ID: `10910140511`.
- Artifact: `PrayerClarity-rebalanced-0.2.41-ci-f571eebd6942f14eaeddcf979c9689feec9b358c`.
- Artifact ZIP digest: `sha256:4f71a93f06b3471962d5f21f73cfb3f8428cb2a1251d3e3d95c99252aa6f9500`.
- Rebalanced 0.2.41 DLL SHA-256: `76c1179e40406cb43016030fb86d70e22f2a3c475dfdfbdd94a779416caa5c2f`.
- UX change: Repose now uses one player-facing vocabulary across its explanatory surfaces. The shared base effect talks about bodies from a higher **quality tier**; Rebalanced Bronze opens the next tier, Silver increases the chance of the **best among already available bodies**, and Gold guarantees the best available body.
- The Bronze terminal message uses the same distinction plus native corpse-quality and prayer-quality glyphs: no new body-quality tiers can be opened; Bronze no longer opens new tiers; Silver raises the chance of the best available body; Gold guarantees the best.
- The shared `active.skull` phrase was synchronized in all 11 base locales, so the clearer base terminology applies to both Vanilla and Rebalanced. The Rebalanced tier/terminal strings were synchronized in all 11 overlays. Vanilla mechanics and its true-terminal `repose.endpoint` wording remain unchanged.
- Repose mechanics, endpoint predicates, corpse generation/distribution, q20/q40/q95, durations, layout/font lifecycle and all other prayers are unchanged.
- Focused runtime acceptance required: with Rebalanced 0.2.41 + Repose State Switcher 0.1.0 in Russian, inspect Bronze once in **Can still unlock** and once in **Terminal**. Confirm the new terminology is clear, `(up)/(skull)/(rskull)` and `(s1)/(s2)/(s3)` render correctly, and the terminal text remains wrap/button-safe. No sermon, corpse generation, Silver/Gold mechanics replay, or additional progression test is required.

## 2026-09-26 — Repose State Switcher 0.1.0 research helper

- Purpose: provide deterministic visual setup for Rebalanced 0.2.40 Bronze Repose endpoint acceptance without changing the user's save/progression or patching the production presentation writer.
- Research branch: `research/repose-state-switcher-0.1.0`.
- Exact helper source SHA: `7d4d368935a2ae3617ef79ef9182c71e87b17a01`.
- Production baseline under test: Rebalanced 0.2.40 candidate source `229d1837123c980b09192d4e7a9b7a143ae573c4`.
- CI run: `36250867047`; build/stage/upload result: **success**.
- Artifact ID: `10908584736`.
- Artifact ZIP digest: `sha256:7687337099efef7641f45a8c9df0b7c16beab9773345621be5f009690d9678f5`.
- Helper DLL SHA-256: `31b7e4ba729873e8c1c27e5c63e86b52199462a44d081299240f4c89f4bc5ed7`.
- Mechanism: Harmony-prefix only `PrayerClarity.R.PlayerParam(string,float)` inside the Rebalanced assembly while an override is enabled. It supplies synthetic read values for `body_min`, `body_max`, `add_body_min`, and `add_body_max`; it does not write game/player/save parameters and does not patch `PulpitLayoutV5`, `CorpseTierSemantics`, or host corpse-generation code.
- **Early / can still unlock:** underlying progression fixture `body_min=1`, `body_max=1`, add modifiers 0. Bronze Repose should remain on the ordinary higher-tier presentation.
- **Terminal:** underlying progression fixture `body_min=2`, `body_max=3`, add modifiers 0. Because ordinary body definitions stop at tier 3, Bronze Repose should use the terminal 0.2.40 wording.
- A live `buff_skull` is accounted for by exposing its stock +1 in the synthetic `body_max` read; the production endpoint logic still performs its normal active-buff normalization.
- F3 opens the helper. `Restore LIVE save state` disables all synthetic reads immediately. Unloading the helper also restores live reads.
- This helper is temporary research tooling and is not part of PrayerClarity production or the Neutral Test Console.


## 2026-09-26 — Rebalanced 0.2.40 complete Repose endpoint wording candidate

- Baseline: current stable/docs main `4563481a5e37c07516525c0e3ad596306e07a7c3`, with runtime behavior still based on stable Rebalanced 0.2.38 / `accepted/rebalanced-0.2.38`.
- Candidate branch: `candidate/rebalanced-0.2.40`.
- Gate-only commit: `663bf85ae9ab27307c1195f76f6b2091fd47bec4`.
- Exact candidate source SHA: `229d1837123c980b09192d4e7a9b7a143ae573c4`.
- CI run: `36250317302`; result: **success**.
- Artifact ID: `10908708470`.
- Artifact: `PrayerClarity-rebalanced-0.2.40-ci-229d1837123c980b09192d4e7a9b7a143ae573c4`.
- Artifact ZIP digest: `sha256:e2c8c848b3ebae9527e6cba8cf87f16a69993ae03a72d9d54926b76792689636`.
- Rebalanced 0.2.40 DLL SHA-256: `7c959c328b202af82e551ed501feed16c652b7f6c0d0833c04ae52c5465e1773`.
- Change scope: wording/presentation only. In the Bronze terminal Repose state where higher ordinary quality can no longer be unlocked but Silver/Gold can still improve the delivered-body distribution, the first line now says the highest available quality tier is already unlocked and Bronze can no longer raise it; the parenthetical explains that the donkey can still bring a worse available body, Silver raises the chance of the best available body, and Gold guarantees it.
- All 11 Rebalanced locale overlays contain the same semantics. The shared `repose.endpoint` key remains unchanged for true terminal Silver/Gold states and for Vanilla.
- Repose mechanics, corpse generation/distribution logic, q20/q40/q95, duration, pulpit layout/font lifecycle and all other prayers remain unchanged.
- Runtime result: the Repose State Switcher successfully changed the displayed Bronze Repose behavior between a state where a higher body-quality tier was still available and the terminal state, confirming the existing endpoint selection responds correctly to the underlying progression inputs. The user rejected the 0.2.40 terminal wording as too bulky / insufficiently immediately clear. **0.2.40 is superseded by 0.2.41 for wording and must not be promoted.** No mechanics defect was found.


## 2026-09-26 — Rebalanced 0.2.39 Repose endpoint wording candidate

- Baseline: stable Rebalanced 0.2.38 / `accepted/rebalanced-0.2.38`.
- Candidate branch: `candidate/rebalanced-0.2.39`.
- Gate-only commit: `b678ca69764b7248de829850113a5b332cb7b0ae`.
- Exact candidate source SHA: `9434871a27c212bfefc16bec1cd77df2003ce815`.
- CI run: `36249314660`; result: **success**.
- Artifact ID: `10907854112`.
- Artifact ZIP digest: `sha256:deebe1c79849b88e7f3ffe8ef376f644e5d5b6c4f7f6d88d358bcea733f2a041`.
- Rebalanced 0.2.39 DLL SHA-256: `a57bdf4b423c1d2ab79cb2249bc0d35bb103d5a0388444ea8f15dc2f976c70ec`.
- Change scope: wording only. The Bronze terminal Repose parenthetical now explains that the donkey can still bring a body below the best currently available; Silver raises the chance of receiving the best one and Gold guarantees it.
- All 11 Rebalanced locale overlays were updated. Repose mechanics, endpoint condition, layout/font ownership and Vanilla remain unchanged.
- **Superseded before runtime acceptance:** 0.2.39 changed only the parenthetical hint and therefore did not implement the full user-approved two-line Bronze endpoint wording. Do not use 0.2.39 for acceptance; 0.2.40 replaces it.


## 2026-09-26 — Rebalanced 0.2.38 accepted stable

- User runtime acceptance: **2026-09-26**.
- Frozen accepted ref: `accepted/rebalanced-0.2.38`.
- Exact accepted runtime/source SHA: `6f5ef168810945135cee57082cbf90d31776e46b`.
- CI run: `36245784593`; result: **success**.
- Artifact ID: `10907083834`.
- Artifact: `PrayerClarity-rebalanced-0.2.38-ci-6f5ef168810945135cee57082cbf90d31776e46b`.
- Artifact ZIP digest: `sha256:875905412950ab7c27ded1985997cddb39b5d39a89084ad5f568cfc71acc5ecd`.
- Accepted Rebalanced DLL SHA-256: `e547f7bb76e0ef512dc669ea30543dada5a2aeb8044b1c7ffcbe7fecf17303e5`.
- Runtime acceptance explicitly confirmed all three 0.2.38 presentation properties:
  1. Shoots & Roots Bronze/Silver omit the 95% combined-cap note, while Gold shows the all-sources 95% cap wording;
  2. Gold Prayer for Donations shows its +100-silver-equivalent fixed reward as **+1 gold** at the pulpit;
  3. Soul's Repose endpoint note uses the parenthetical presentation and the prayer effects remain correct across all three qualities.
- Legacy Protective Prayer / `b_shield` historical item lore is intentionally preserved by product decision.
- New follow-up wording question: the Bronze Repose endpoint note is mechanically correct but may not explain “reliability” clearly enough to a player who does not know the corpse-quality distribution system. This is a separate future wording/design task and does not invalidate 0.2.38 acceptance.
- Stable promotion PR: **#39**, squash-merged to `main` as `d40872af6a6088052922f9a9fa4c0a20ac03811e`.
- Publication workflow run: `36248438930`; result: **success**.
- GitHub release: `rebalanced-v0.2.38` -> exact accepted source `6f5ef168810945135cee57082cbf90d31776e46b`.
- Release ID: `397263631`; canonical asset `PrayerClarity.Rebalanced.dll`, asset ID `590809031`, digest `sha256:e547f7bb76e0ef512dc669ea30543dada5a2aeb8044b1c7ffcbe7fecf17303e5`.
- Publication downloaded the accepted CI artifact, verified the recorded SHA-256, and uploaded the exact DLL bytes; **no rebuild occurred**.
- Numbered accepted bytes are immutable.



## 2026-09-24 — Rebalanced 0.2.22 prayer-carousel result accepted

- User runtime acceptance: the production Rebalanced 0.2.22 DLL reproduced the accepted research prototype exactly; the user reported that navigation and presentation work correctly as designed.
- Frozen accepted ref: `accepted/rebalanced-0.2.22`.
- Exact tested source SHA: `302104a00a6ffef03b1c3746faa7781b84da2c71`.
- GitHub Actions run: `35989524570`; result: **success**, Rebalanced and Vanilla sibling builds both 0 warnings / 0 errors.
- Artifact ID: `10803935926`.
- Artifact: `PrayerClarity-rebalanced-0.2.22-ci-302104a00a6ffef03b1c3746faa7781b84da2c71`.
- Artifact ZIP digest: `sha256:e031e3f5fa5e3ce44d4550b2cbb3c8d01172e6b9f28e734ae167d338c91b3d7b`.
- Accepted Rebalanced DLL SHA-256: `19819097d7c90ee3ddfd4385cf8d2546a83e9c2bb78a1d1427e6a60bc0bcbaf6`.
- Vanilla 1.0.32 sibling DLL from the same build: `742c06785b92deeb24a9251906e30f3c97596620921989530e87263ee2aaa21c`.
- Accepted behavior for the Better Save Soul three-prayer Technology node:
  - gamepad focus remains on the real parent Technology;
  - exactly one existing child `TechUnlock.GetTooltip` is shown at a time;
  - the selected child icon remains fully visible while the others are dimmed;
  - Right traverses prayer 1 -> 2 -> 3 and then falls through to stock tree navigation;
  - Left traverses prayer 3 -> 2 -> 1 and then falls through to stock tree navigation;
  - entering horizontally selects the boundary child corresponding to the direction of entry;
  - Up/Down and Technology purchase/progression state remain stock;
  - no save, cost, unlock, prayer-mechanics, or DLC progression state is written.
- This closes the BSS-specific UX hypothesis. The result is retained as exact accepted runtime evidence while the immediately following candidate tests the user's requested generalization to all prayer-bearing Technology nodes and to PrayerClarity: Vanilla.


## 2026-09-24 — stable publication complete for Vanilla 1.0.33 / Rebalanced 0.2.23

- Stable promotion PR: **#32**, squash-merged to `main` as `1cfef778e4f6744dd226c5ffab03a1de8cad9e27`.
- Publication workflow run: `35994536699`; result: **success**.
- The workflow downloaded accepted artifact `10803884921`, verified both recorded SHA-256 values, and published the exact accepted DLL bytes; **no rebuild occurred**.
- Vanilla release: `v1.0.33` -> target `93b66e747ffe1685003afb894b24f14416edb8c0`.
  - release ID: `395614611`;
  - asset ID: `585838555`;
  - asset: `PrayerClarity.dll`;
  - asset digest: `sha256:30b23f9ed62148f3fd08e0c34ae54f165da0e639a041d1e9d7c4abe74268da8a`.
- Rebalanced release: `rebalanced-v0.2.23` -> target `93b66e747ffe1685003afb894b24f14416edb8c0`.
  - release ID: `395614626`;
  - asset ID: `585838632`;
  - asset: `PrayerClarity.Rebalanced.dll`;
  - asset digest: `sha256:22ba558786eb139f01aabeaa99a31b8a55bc263c9a320ed1790ca00494f80796`.
- Final status: **accepted, frozen, merged to main, and published as both current stable sibling editions**.
- Numbered accepted bytes are immutable.


## 2026-09-24 — Rebalanced 0.2.23 / Vanilla 1.0.33 accepted

- User runtime acceptance: explicit confirmation that the generalized prayer-Technology navigation works correctly in both Rebalanced and Vanilla.
- Frozen refs: `accepted/rebalanced-0.2.23` and `accepted/vanilla-1.0.33`.
- Exact tested source SHA for both sibling DLLs: `93b66e747ffe1685003afb894b24f14416edb8c0`.
- GitHub Actions run: `35991353623`; build result: **success**, both sibling builds 0 warnings / 0 errors.
- Artifact ID: `10803884921`.
- Artifact ZIP digest: `sha256:b4972906bd7c22d9f8b210f9830f46a745a81216b805899bc0fedb093361cce5`.
- Rebalanced 0.2.23 DLL SHA-256: `22ba558786eb139f01aabeaa99a31b8a55bc263c9a320ed1790ca00494f80796`.
- Vanilla 1.0.33 DLL SHA-256: `30b23f9ed62148f3fd08e0c34ae54f165da0e639a041d1e9d7c4abe74268da8a`.
- Accepted scope: on gamepad, any Technology with at least two visible unlocks and at least one prayer becomes a horizontal one-unlock-at-a-time sequence across all of that node's visible unlocks. Boundary Left/Right falls through to stock tree navigation; Up/Down remains stock; mouse behavior remains stock.
- The selected unlock's existing `TechUnlock.GetTooltip` remains authoritative. PrayerClarity only changes the controller presentation/navigation layer; Technology costs, unlock state, prayer mechanics, save state and non-prayer Technologies are unchanged.
- No additional runtime replay is required for stable promotion.
- Stable publication must reuse the exact accepted DLL bytes above; do not rebuild these numbered versions.


## PrayerClarity: Rebalanced 0.2.23 / Vanilla 1.0.33 — shared prayer-Technology navigation candidate

- Product direction from the accepted 0.2.22 test: the one-child-at-a-time controller interaction is substantially more readable and feels native enough to generalize, but applying it to every Technology would be excessive.
- Scope rule: on gamepad only, activate the carousel for a Technology when it has at least two visible unlocks and at least one visible unlock resolves through PrayerClarity's existing prayer-craft path.
- Once activated, **all visible unlocks in that prayer-bearing Technology** participate in the horizontal sequence, including non-prayer perks/buildings/recipes. This keeps icon highlight and tooltip ownership one-to-one instead of leaving a mixed node partly combined.
- Activation is data-driven; no Technology ID/name allowlist is introduced.
- Each selected child is rendered through its existing `TechUnlock.GetTooltip` path. Prayer unlocks therefore continue through PrayerClarity's semantic Technology formatter; non-prayer unlocks remain stock.
- Left/Right moves between child unlocks until an outer edge, then the original `BaseGUI.OnPressedLeft/Right -> GamepadNavigationController.Navigate` path runs unchanged.
- Technologies with no prayer remain entirely stock. A prayer-bearing Technology with only one visible unlock also remains stock because there is nothing to disambiguate.
- Mouse behavior remains stock: child unlocks already own independent tooltips in 1.407.
- The same shared source is compiled into Rebalanced 0.2.23 and Vanilla 1.0.33. No prayer mechanics, balance values, localization strings, Technology costs, unlock state, or save state change.
- Acceptance gate: one mixed prayer Technology with 2-3 visible child icons must prove icon/tooltip traversal and boundary exit; then spot-check the same behavior in the Vanilla sibling. BSS itself does not need another mechanics or architecture replay.
- Status: **candidate implementation prepared; CI/runtime acceptance pending**.

## PrayerClarity: Rebalanced 0.2.20 — visual acceptance candidate

- Exact production source SHA: `20781bfab72953694ce32c69ec2b07bcc85496ca`.
- Candidate ref: `candidate/rebalanced-0.2.20`.
- GitHub Actions run: `35907572726`; job: `107338980659`.
- Build result: **success**; Rebalanced 0 warnings / 0 errors, Vanilla 0 warnings / 0 errors.
- Artifact ID: `10772181010`.
- Artifact name: `PrayerClarity-rebalanced-0.2.20-ci-20781bfab72953694ce32c69ec2b07bcc85496ca`.
- Artifact ZIP digest: `sha256:89a1ff45a6044660e773fc0e05ae320f4f360257ecd41111b0c839ba1dc52207`.
- Rebalanced DLL SHA-256: `c77322aec1020eb920fd6ea6cb6abea735d8ac0048089cb03ba21053414a69a0`.
- Vanilla sibling DLL SHA-256 from the same shared-source build: `1d547ac036280f9a9e9ebed9fdb2425cba06e7054f4637c2d4897bb7357fc2bc`.
- 0.2.20 deliberately removes the unaccepted 0.2.19 mixed-family grouping/lore-suppression experiment and restores the 0.2.18 Technology structure before applying the narrower repairs below.
- Pulpit repair: the verified final writer `PulpitPolish -> PresentationText.BuildPulpitResultRows` now renders the current Soul's Repose success transaction from `SoulGratitudeConversion`; when that conversion is the prayer's success effect, the otherwise empty separate `Effect: —` row is hidden.
- Lore repair: BSS lore remains visible. Soul Contentment replaces only the stock percentage token with the percentage produced by the current Rebalanced Technology effect; Thorough Cleansing removes only the stock fixed `(x2)` suffix, leaving the sentence itself intact; Soul's Repose lore is preserved unchanged.
- Width repair: historical 1.0.12/1.0.13 evidence is treated as canonical. The combined BSS tooltip marks its retained real lore rows plus the PrayerClarity mechanics row for the proven `UILabel.overflowWidth -> ResizeFreely -> WidgetsBubbleGUI.UpdateSize()` lifecycle. Those rows use a finite 520-unit expansion ceiling; ordinary single-prayer Technology tooltips keep the accepted atomic/content-driven width policy.
- No prayer mechanics, requirements, conversion arithmetic, Contentment decay behavior, Sin Shard scaling, or duration values changed from the already-tested 0.2.18 mechanics.
- Required user acceptance is presentation-only:
  1. select Gold Soul's Repose at the pulpit with nonzero SG and confirm the success row shows the live SG -> Faith transaction (115 SG should currently show 90 -> 90) and no `Effect: —` row remains;
  2. with gamepad placement, open the combined BSS prayer Technology tooltip and confirm the parchment is visibly wider, the complete tooltip fits the viewport, and all three prayers retain readable lore;
  3. with keyboard/mouse, spot-check the three individual BSS prayer Technology tooltips: Soul's Repose lore remains; Soul Contentment lore remains with the current Rebalanced percentage rather than stock 10%; Thorough Cleansing lore remains but no longer contains the stale parenthetical `(x2)`, while the red tier values remain x2/x3/x4.
- No sermon, soul healing, decay wait, or Test Console run is required for this candidate.
- Status: **compiled immutable candidate; visual acceptance pending; not merged or published**.
- Runtime/visual result, 2026-09-24: **partial pass / candidate not accepted**.
  - Pulpit final-writer repair passed visually: Gold Soul's Repose with 115 SG now shows the live 90 SG -> 90 Faith success transaction and the stray `Effect: —` row is gone.
  - The compact arrow-only pulpit wording is mechanically correct but reads too much like a rebus. Product direction: keep the dynamic amount, but make the transaction explicit in words because this surface has sufficient space.
  - Combined BSS Technology tooltip still exceeds the viewport under the user's current mod set; the expected widening is not visible. The user has a separate Companion mod that repositions tooltips to the lower-left. Because it touches the same final tooltip surface, one controlled A/B without Companion is required before attributing the remaining geometry failure to PrayerClarity.
  - Individual Soul's Repose Technology wording is still too ambiguous: “each spent Soul Gratitude” does not clearly tell a player who has not yet used the sermon that the sermon consumes their currently stored Soul Gratitude. Reword around “stored/current Soul Gratitude is converted into Faith 1:1 on a successful sermon” plus the existing tier cap.
  - Soul Contentment should keep a short thematic lore sentence about preserving soul condition; do not duplicate the +50% magnitude in lore because the exact effect block already owns it.
  - Thorough Cleansing still shows the stock fixed `(x2)` inside lore, so the 0.2.20 regex normalization did not satisfy the observable property. Do not add another punctuation/Unicode-sensitive regex guess; use an edition-owned BSS lore string or another exact-key replacement with verified localized output.
  - Contentment duration is now explicitly reopened as a visual emphasis question because +50% is invariant across qualities and duration is the only tier ladder (4/8/12 days in the user's Longer Days presentation). If emphasized, color only the duration value, not the label.
- Width next step: test the same combined BSS Technology tooltip once with Companion disabled. If it expands/fits, investigate compatibility/patch ordering with Companion. If it still does not, create a narrow read-only Technology geometry/patch-owner probe; do not ship another width-policy production guess.
- Companion-off A/B result, 2026-09-24: **failure persists and Companion is ruled out for this case**.
  - The supplied runtime session loads Rebalanced 0.2.20 on Graveyard Keeper 1.407 at 2560x1440 and does not load Companion.
  - The combined Better Save Soul prayer Technology tooltip still runs below the bottom of the viewport.
  - User control observation: other prayer Technology tooltips shown through gamepad placement move upward when needed; this combined three-prayer tooltip is the only observed prayer case that does not.
  - Static source inspection now explains the vertical-placement difference: `TechnologyTooltipViewportClamp.ClampAxis` returns the original coordinate whenever the bubble itself is taller than the available safe axis (`min > max`). Therefore ordinary tooltips can be clamped upward because they fit; an over-tall combined BSS tooltip cannot be made fully visible by translation alone and is deliberately left at its original Y.
  - This closes the Companion/vertical-clamp hypothesis. It does **not** close the remaining width question: why the combined BSS rows marked for a 520-unit `UILabel.overflowWidth` expansion ceiling are not producing a sufficiently wide/short final bubble.
  - Next evidence gate: a separate research-only, read-only Technology tooltip probe must record the post-draw live label geometry, final bubble geometry, and Harmony patch owners/order for `BubbleWidgetText.Draw`, `Tooltip.Show`, and `WidgetsBubbleGUI.Update`. Production 0.2.20 remains unchanged while this is investigated.
- Research helper prepared for that gate: **Technology Tooltip Probe 0.1.0**.
  - research ref: `research/technology-tooltip-probe-0.1.0`;
  - exact source SHA: `3591b8b0b1287f88528f5112af72792bdee354bc`;
  - GitHub Actions run: `35937587610`; result: **success**;
  - artifact ID: `10783462850`;
  - artifact ZIP digest: `sha256:35e4819df17c1df64a4abccb80e24999decd96472b9df1ddbf8b956a64ab0979`;
  - handoff DLL: `PrayerClarity.TechnologyTooltipProbe-0.1.0-ci.dll`;
  - DLL SHA-256: `ff96a2fb380b2596bf297202018e63d4495531fef50faa7b95000d548a97a8a8`.
  - The probe is read-only with respect to game/UI/save state: it observes PrayerClarity-owned `max_width=900` rows after live `BubbleWidgetText.Draw`, records label `overflowWidth`/processed geometry, records final `WidgetsBubbleGUI` geometry for the displayed tooltip, and dumps Harmony patch ownership/order. It does not write label geometry or save data.
  - Required runtime action: keep Rebalanced 0.2.20 unchanged, install only the probe DLL alongside it, open Technology -> Spiritualism, highlight the combined Better Save Soul prayer node once with the gamepad, then return the resulting `LogOutput.log`. No sermon/mechanics action is required.
- Technology Tooltip Probe 0.1.0 runtime result, 2026-09-24: **root cause isolated**.
  - Rebalanced 0.2.20 and the read-only probe loaded on GK 1.407 at 2560x1440; Companion / Gamepad Tooltip Position Fix was not loaded in the session.
  - Harmony ownership on the inspected path contained only PrayerClarity's content-width/viewport hooks plus the probe. No third-party final writer was present on `BubbleWidgetText.Draw`, `Tooltip.Show`, or `WidgetsBubbleGUI.Update`.
  - The final combined BSS tooltip contained 29 rows and six PrayerClarity-owned mechanics rows with `max_width=900` (base + success body for each of the three prayer families).
  - Despite that marker, the live post-draw labels remained narrow: measured `overflowWidth` was 168-180 and live widths 166-178. The final bubble was 178 x 910 UI units while the safe viewport was 1256 x 696, so the bubble was vertically impossible to fit by translation alone.
  - The retained BSS lore rows still had `max_width=-1`, while per-family semantic normalization had already run (for example Soul Contentment showed the Rebalanced +50% wording). This proves the intended `preferWideLayout` classification was false during each per-family Technology pass; the problem is not a later NGUI overwrite.
  - Source correlation: `ShouldPreferWideBssLayout(crafts)` requires multiple BSS families inside one `ResolvePrayerCrafts` result, but the shared tooltip is accumulated from separate family passes. Each pass therefore sees one family and never marks its rows for the 520-unit `UILabel.overflowWidth` ceiling, even though all three families are present in the final Tooltip data.
  - Next production change is narrow: after a BSS family pass, inspect the already-accumulated Tooltip data; once at least two BSS prayer blocks are present, mark the existing PrayerClarity mechanics rows and their immediately preceding lore rows for the already-verified wide `overflowWidth` path. Do not change viewport math, manually force final label width/height, or add a new placement algorithm.
- No mechanics retest is required.
- Status after this visual pass: **0.2.20 remains rejected for promotion; pulpit mechanics presentation path is proved, combined Technology layout and BSS wording/lore still require follow-up.**


## 2026-09-23 — Rebalanced 0.2.19 runtime visual result: rejected candidate

Tested immutable production candidate:
- source: `b593a9391f0b8a1c1bc1a2f362adb63db2287ce9`;
- ref: `candidate/rebalanced-0.2.19`;
- Rebalanced DLL SHA-256: `7b163d31e1f9f61d3db1460ba0794c06d8fba649b653cc870265186c7546203c`.

Runtime/visual evidence:
- PrayerClarity: Rebalanced 0.2.19 loaded normally on Graveyard Keeper 1.407; no PrayerClarity-specific runtime exception was reported in the supplied session.
- Gold Soul's Repose at the pulpit still failed the presentation gate: with Church Quality 94 and Soul Gratitude 115, the success row did not show the expected live 90 SG -> 90 Faith transaction and the separate Effect row rendered only a dash.
- The combined Better Save Soul prayer Technology tooltip still exceeded the viewport under gamepad placement. The attempted 360-wide content preference did not visibly widen the parchment/frame.
- The 0.2.19 broad BSS lore suppression removed the base lore from the individual keyboard/mouse Technology tooltips for Soul's Repose, Soul Contentment, and Thorough Cleansing. This is an unacceptable regression.
- User product decision: preserve prayer lore where it exists. Resolve stale Rebalanced mechanic clauses/values narrowly; do not delete the whole lore paragraph as a space-saving or conflict-avoidance shortcut. A genuinely wider final Technology tooltip is preferred over sacrificing lore.

Root-cause assessment:
- Soul's Repose mechanics and live craft identity remain closed by prior accepted evidence. The 0.2.19 pulpit fix targeted an intermediate writer only. `PulpitPolish.Apply` is the final presentation pass; its `PolishResultRows` rewrites the result text through `PresentationText.BuildPulpitResultRows`, which does not render `SoulGratitudeFaithCap` / `SoulGratitudeConversion`. `PolishEffectRow` then turns the intentionally empty separate effect into a visible dash.
- Lore loss is deterministic production behavior introduced in 0.2.19: `ShouldSuppressVanillaPrayerLore` was broadened from the previously narrow stale-Contentment case to all three Rebalanced BSS prayer families and therefore affected both the combined unlock and individual prayer Technology surfaces.
- Tooltip-width ownership is **not** an open evidence gap. Historical 1.0.12/1.0.13 research already proved the live Technology mechanics `UILabel` uses `ResizeFreely`, that its native `overflowWidth` is the effective wrapping/expansion ceiling, and that `WidgetsBubbleGUI.UpdateSize()` subsequently sizes the outer bubble/parchment from the child widgets. PrayerClarity 1.0.13 was then runtime-verified to make Technology prayer tooltips expand with content through this seam. The 0.2.19 failure is therefore a sizing-policy error, not an unknown owner: `WideMinimumAnchorWidth=360` was fed back as an `overflowWidth` ceiling, but `ResizeFreely` still chooses the natural content width and is not forced to 360 merely because the ceiling is at least 360.

Assessment:
- **0.2.19 is rejected and must not be rebuilt, merged, or published.**
- Reuse the already-passed Soul's Repose conversion arithmetic, Soul Contentment preservation, Thorough Cleansing scaling, and natural pulpit craft-identity evidence.
- Do not start production 0.2.20 from the unverified 0.2.19 layout assumptions.
- Next step does **not** require another width-owner probe. Reuse the verified `UILabel.overflowWidth -> natural label size -> WidgetsBubbleGUI.UpdateSize() -> outer parchment` lifecycle, preserve BSS lore, and correct only the combined-tooltip sizing policy plus the final pulpit renderer. A new runtime probe is justified only if the corrected width policy needs a host behavior not already covered by that accepted evidence.

## PrayerClarity: Rebalanced 0.2.19 — visual acceptance candidate

- Exact production source SHA: `b593a9391f0b8a1c1bc1a2f362adb63db2287ce9`.
- Candidate ref: `candidate/rebalanced-0.2.19`.
- GitHub Actions run: `35900091899`; job: `107313733159`.
- Build result: **success**; Rebalanced 0 warnings / 0 errors, Vanilla 0 warnings / 0 errors.
- Artifact ID: `10767684729`.
- Artifact name: `PrayerClarity-rebalanced-0.2.19-ci-b593a9391f0b8a1c1bc1a2f362adb63db2287ce9`.
- Artifact ZIP digest: `sha256:001b7c42bdee37c57172ff0f516fdf00aa28c458f3e8acc783f4f587b9dd565c`.
- Rebalanced DLL SHA-256: `7b163d31e1f9f61d3db1460ba0794c06d8fba649b653cc870265186c7546203c`.
- Scope:
  - preserve the correct Soul's Repose success transaction through the final pulpit layout pass and suppress the obsolete stock effect sentence there;
  - keep Soul Gratitude separate from ordinary base-Faith dependency semantics while still showing the current SG context for the conversion;
  - group mixed Rebalanced Technology prayer tiers by localized prayer family;
  - compact each mixed-family Bronze/Silver/Gold snapshot to one semantic row;
  - request a wider content-driven layout only for the mixed success-details body;
  - suppress conflicting stock BSS lore when Rebalanced owns replacement semantics, including the stale Thorough Cleansing `x2`.
- Mechanics unchanged from the already-tested 0.2.18 paths. Reuse the accepted Soul's Repose conversion arithmetic, Contentment preservation, and Thorough Cleansing output evidence; **no sermon, soul-healing, or decay replay is required**.
- Required acceptance is visual/perceptual only:
  1. at the pulpit, select Gold Soul's Repose with the current SG state and confirm the success row shows the live SG -> Faith transaction while the old stock effect sentence is absent;
  2. open the combined Better Save Soul prayer Technology tooltip with gamepad placement and confirm it fits the viewport, remains readable, is grouped by prayer, and contains no stale fixed `x2` lore.
- Test Console 0.1.7 is not required for this pass.
- Status: **compiled immutable candidate; visual acceptance pending; not merged or published**.

## PrayerClarity: Rebalanced 0.2.18 — runtime candidate

- Candidate source SHA: `449748ceb9f50c873e15dede485315912ca9935c`.
- Candidate ref: `candidate/rebalanced-0.2.18`.
- GitHub Actions run: `35871569228`; job: `107216690190`.
- Build result: **success**; Rebalanced 0 warnings / 0 errors, Vanilla 0 warnings / 0 errors.
- Artifact ID: `10754947841`.
- Artifact name: `PrayerClarity-rebalanced-0.2.18-ci-449748ceb9f50c873e15dede485315912ca9935c`.
- Artifact ZIP digest: `sha256:acc7044b3deece710607c210ee511d23d1f93a499bf6c9ab9f835e5887d7e9ed`.
- Rebalanced DLL SHA-256: `f9e7d455bc027529547252628f9b10626b0c4dbdac3bc686103cfea4bf8369a4`.
- Scope relative to 0.2.17: live Soul's Repose event-argument projection + matching forecast event semantics; compact/wrap-safe BSS Technology tier values; red Thorough Cleansing x2/x3/x4 values; tier-neutral Thorough Cleansing description. Balance values and already-passed Thorough Cleansing runtime seam are unchanged.
- Status: **runtime-tested and rejected; not merged or published**. Mechanics probes passed; the remaining failures were localized to pulpit presentation overwrite and the combined BSS Technology layout.

Research helper for the remaining gate:
- Rebalanced Test Console **0.1.6**.
- Candidate source: `e61481a754c38f2b362b7486d15148bec96fe8df`.
- Candidate ref: `candidate/rebalanced-test-console-0.1.6`.
- GitHub Actions run: `35871876168`; job: `107217747611`.
- Build result: **success**, 0 warnings / 0 errors.
- Artifact ID: `10755627030`.
- Artifact name: `PrayerClarity-RebalancedTestConsole-0.1.6-ci-e61481a754c38f2b362b7486d15148bec96fe8df`.
- Artifact ZIP digest: `sha256:a5e8c5311682cf62d2dddc426f6017880b4bffef8d37ac28f7fc0f78e4acb99b`.
- DLL SHA-256: `3e30ba6cc68256e14aa600cac36f528cacd2baafbfdee94226e1b798b60e8fd6`.
- 0.1.6 fixes the Contentment probe's game-type/method lookup and strengthens the Soul's Repose probe by deliberately feeding the old `pray_for_souls_3` event into the real patched call, requiring 0.2.18 to normalize it while base Faith remains independent of SG.

## 2026-09-23 — Rebalanced 0.2.18 runtime result: rejected candidate

Tested immutable production candidate:
- source: `449748ceb9f50c873e15dede485315912ca9935c`;
- ref: `candidate/rebalanced-0.2.18`;
- DLL SHA-256: `f9e7d455bc027529547252628f9b10626b0c4dbdac3bc686103cfea4bf8369a4`;
- research helper used for this pass: Rebalanced Test Console 0.1.6, source `e61481a754c38f2b362b7486d15148bec96fe8df`.

Accepted/reusable runtime evidence:
- The strengthened Soul's Repose probe deliberately passed stock `pray_for_souls_3` into the real patched `PrayLogics.CalculatePray` path. All four controlled cases passed with a single SG-independent base Faith value of 19: 73 SG -> +73 Faith / 0 SG; 136 SG -> +90 Faith / 46 SG; 0 SG -> +0; deterministic failure -> +0 and no SG spend.
- The corrected Soul Contentment decay probe passed: ordinary extracted Soul and in-corpse SoulBodyPart decayed without Contentment, both remained protected with `buff_gp_increase`, and ordinary Body decay continued.
- Thorough Cleansing's real Gold x4 healing evidence from 0.2.17 remains valid because the production seam did not change in 0.2.18; no repeat heal is required.
- Keyboard/mouse per-prayer BSS Technology tooltips are structurally correct: Soul's Repose shows q30/q60/q90 and 30/60/90 SG caps; Contentment shows +50% SG and the projected durations; Thorough Cleansing shows q30/q60/q120 and red x2/x3/x4 values.

Rejected/unclosed runtime evidence:
- The natural Gold Soul's Repose pulpit with 115 SG still did **not** show the required live `SG -90 -> Faith +90` transaction. It showed the old Soul-Gratitude-dependent effect wording instead.
- Follow-up Test Console 0.1.7 closed the identity uncertainty: a naturally selected Gold Soul's Repose at the pulpit is the exact canonical `pray:b_souls:3` `CraftDefinition` reference, with `linked_sub_id=default_3`, q90, and 115 live SG. The production tier model resolves the same craft as tier 3 with a 90-SG conversion cap. Therefore the passed real `CalculatePray` conversion probe is representative of the naturally selected craft identity; no weekly sermon replay is required to re-prove that mechanic.
- The combined BSS Technology tooltip under gamepad placement is physically taller than the viewport by roughly an additional prayer block. Position clamping cannot solve an object that is itself taller than the available screen height.
- Thorough Cleansing still exposes stale vanilla lore ending in `(x2)`; Rebalanced tier mechanics are x2/x3/x4, so the shared lore must become tier-neutral on every supported locale.
- The Soul's Repose item tooltip appears semantically correct in the supplied screenshot; no separate item-tooltip blocker was found.

Assessment:
- **0.2.18 is rejected and must not be rebuilt under the same version.**
- Reuse the passed conversion arithmetic, Contentment decay, and Thorough Cleansing runtime evidence.
- The natural pulpit craft identity is now resolved. The 0.2.18 visible failure is a presentation-layer overwrite: `PulpitPresentation` first builds the correct conversion row, then `PulpitLayoutV4` replaces it with the generic resource row and restores the old Soul's Repose effect sentence. Production 0.2.19 should fix that downstream rendering only; the accepted conversion arithmetic/seam remains unchanged.
- Technology overflow design direction accepted by the user: normal tooltip unchanged when it fits; otherwise widen/reflow first, remove group-level duplicate information only if still needed, and consider columns only if the viewport still cannot contain the content. Do not shrink text into an unreadable fallback.

Research helper prepared for the identity gap:
- Rebalanced Test Console **0.1.7**.
- Exact source: `fefe877347b68c51e108d9b4824b43549f182921`.
- Candidate ref: `candidate/rebalanced-test-console-0.1.7`.
- GitHub Actions run: `35898012483`; job: `107306705139`; build **success**.
- Artifact ID: `10767965435`.
- Artifact name: `PrayerClarity-RebalancedTestConsole-0.1.7-ci-fefe877347b68c51e108d9b4824b43549f182921`.
- Artifact ZIP digest: `sha256:25dd564f33b5d46612b3553693ee6a2a3b46805926754f34160a5524c475b578`.
- DLL SHA-256: `abbb8df8c3b842ceb9215431e9ded1134c22a3114d1070eec24aaea9ba717e82`.
- New action: **Capture live pulpit Soul's Repose identity (no sermon)**. With the pulpit open and Gold Soul's Repose selected naturally, it logs the exact live craft type/ID/event/requirement/modifiers/output, relevant GUI/craft identity members, reference equality against canonical `pray:b_souls:3`, and the production `PrayerForecast.BuildTierDetails` interpretation. It is read-only and spends no sermon/resource.
- Runtime result, 2026-09-23: **PASS**. Natural pulpit Gold Soul's Repose was `CraftDefinition id=pray:b_souls:3`, `linked_sub_id=default_3`, `needs_quality=90`, `k_faith=0`, `k_money=0`, and `same_ref_as_canonical_gold=True` with live SG 115. The production tier model reported `CraftId=pray:b_souls:3`, `EventId=default_3`, tier 3, and `SoulGratitudeFaithCap=90`.
- Consequence: the suspected live-craft identity mismatch is disproved. The remaining Soul's Repose defect is downstream presentation state, not prayer selection identity or the already-passed conversion arithmetic.
- Status: identity question closed; Test Console 0.1.7 is no longer required for the next production visual acceptance pass.

## 2026-09-23 — Rebalanced 0.2.17 runtime result: rejected candidate

Tested immutable production candidate:
- source: `fbffbb4957adc186ef42d4874241ca80047e597e`;
- ref: `candidate/rebalanced-0.2.17`;
- DLL SHA-256: `68d19f495d44413e4f29a4b24a062b684be62bb1d1834020bbc0c2d57fcf42f2`;
- research helper: Rebalanced Test Console 0.1.5, source `233ea9d6680b27ebf8dd536779442872dec05b1e`.

Accepted evidence retained from this candidate:
- Soul's Repose direct native-calculation probe passed all four controlled cases: below cap 73 SG -> +73 Faith / 0 SG, Gold above cap 136 -> +90 Faith / 46 SG, zero SG -> +0, deterministic failure -> +0 and no SG spend.
- Thorough Cleansing real Gold healing path passed: base 4 Sin Shards, expected x4, actual 16. A no-buff baseline also matched base output.
- No additional Bronze/Silver Thorough Cleansing runtime repetition is required unless that seam changes.

Unclosed / rejected evidence:
- Soul Contentment decay probe did not reach the assertion. Test Console 0.1.5 failed its reflection lookup for `Item(string,int) / Item.UpdateDurability(float,float)`; this is a research-harness failure and is not evidence that production preservation failed.
- The real pulpit with Gold Soul's Repose, Church Quality 94 and Soul Gratitude 115 did not show the required `SG -90 -> Faith +90` success transaction. It instead exposed the old Soul-Gratitude-dependent effect semantics.
- The BSS Technology tooltip overflowed the viewport under gamepad placement. Soul's Repose tier rows also split the numeric cap from the Soul Gratitude icon.
- Thorough Cleansing wording was too generic for the tier ladder and its x2/x3/x4 values lacked the established Technology accent.

Assessment:
- **0.2.17 is not accepted and must not be rebuilt.**
- The direct conversion and Thorough Cleansing evidence remain reusable.
- 0.2.18 must close the live Soul's Repose event/pulpit path, compact the BSS Technology presentation, and use a corrected research helper for the Contentment decay assertion.

## PrayerClarity: Rebalanced 0.2.17 — candidate

- Candidate scope accepted for implementation on **2026-09-23**; runtime acceptance is still pending.
- Donations: q20 / q40 / q60; success-only flat payout **+20 / +50 / +100 silver**.
- Soul's Repose: q30 / q60 / q90; ordinary church-derived base Faith plus success-only **1 Soul Gratitude -> 1 Faith**, capped at **30 / 60 / 90** and spending only the amount converted.
- Soul's Repose pulpit shows the live pending transaction under the success row.
- Soul Contentment: q20 / q40 / q60; **+50% Soul Gratitude**; duration **45 / 90 / 135 minutes** = 1 / 2 / 3 vanilla six-day weeks; passive soul-condition decay is suspended both in-corpse and after extraction, while direct Soul Extractor damage remains vanilla.
- Thorough Cleansing: q30 / q60 / q120; native Sin Shard output scales **x2 / x3 / x4**.
- Implementation architecture:
  - Donations stays on the accepted fixed-money prayer-output path.
  - Soul's Repose temporarily projects the conversion as a native fixed Faith output around `PrayLogics.CalculatePray`, then spends SG only if the native result succeeded.
  - Contentment blocks only `Item.UpdateDurability` for `SoulBodyPart` / `Soul` while `buff_gp_increase` is active; no inventory/world scans.
  - Thorough Cleansing temporarily projects the Silver/Gold delta into the native `increase_sin_shard_drop` input around `SoulHealingWidget.OnStartHealButtonPressed`.
- Localization: all 11 supported Rebalanced locale overlays updated.
- Required gate: clean CI build/package validation, then focused runtime evidence for Soul's Repose conversion/spend, Contentment preservation/resume, and Thorough Cleansing x2/x3/x4.
- Status: **candidate build requested; not accepted, not merged, not published**.

## PrayerClarity: Rebalanced 0.2.16 — accepted for stable promotion

- User acceptance/design approval: **2026-09-23**.
- Frozen accepted ref: `accepted/rebalanced-0.2.16`.
- Exact executable source SHA: `d44bf75227f6efc1c4f09fb5cd3c4eaf3b9ee010`.
- GitHub Actions run: `35801691040`; Rebalanced build: **success, 0 warnings, 0 errors**.
- Workflow artifact ID: `10726206110`.
- Workflow artifact ZIP digest: `sha256:7a1502e61d54ecd96176e5cd7d5edb42a631b96366db9812eb12e11979704adb`.
- Canonical Rebalanced DLL SHA-256: `2ac89af8f517905fb859dbf496e9f16c098f9e75425b8ed03617927b27f75591`.
- Executable delta from 0.2.15:
  - Repose Gold requirement: **90 -> 95 Church Quality**;
  - Excellence Gold requirement: **90 -> 95 Church Quality**;
  - no other prayer requirement or effect changed;
  - plugin/assembly version advanced to 0.2.16;
  - shared candidate workflow packaging labels advanced to 0.2.16.
- Static ownership evidence: `RebalancedStaticProjection` writes `rule.Requirements[tier]` directly into the existing stock `CraftDefinition.needs_quality` field.
- **No fresh in-game runtime test required.** The changed property is only two scalar inputs to an already accepted projection/native success path; a runtime replay would not prove a new lifecycle, formula, hook, RNG, save, or effect behavior.
- Acceptance gate for this micro-release is exact source diff + clean compile/package/localization validation + immutable artifact identity.
- Stable promotion PR: **#30**, squash-merged to `main` as `7c90691f84bfba8be500ed3d60a53432bbe65e40`.
- Stable publication workflow: `35801981914`; result: **success**.
- Published release: `rebalanced-v0.2.16` -> target `d44bf75227f6efc1c4f09fb5cd3c4eaf3b9ee010`.
- Release asset ID: `582562218`, canonical filename `PrayerClarity.Rebalanced.dll`, asset digest `sha256:2ac89af8f517905fb859dbf496e9f16c098f9e75425b8ed03617927b27f75591`.
- Publication reused and hash-verified the exact accepted CI DLL; **no rebuild occurred**.
- Final status: **accepted, merged to main, frozen by accepted ref, and published as stable Rebalanced release**.
- Numbered accepted bytes are immutable.

## PrayerClarity: Rebalanced 0.2.15 — accepted stable

- User runtime acceptance: **2026-09-23**.
- Frozen accepted ref: `accepted/rebalanced-0.2.15`.
- Exact build/source SHA: `ac953fe25ef17dbaf63340a7b9309dadec7e207e`.
- GitHub Actions run: `35791783893`; Rebalanced build: **success, 0 warnings, 0 errors**.
- Workflow artifact ID: `10722636745`.
- Workflow artifact ZIP digest: `sha256:fc99440a10056010e741e7f6459c09895c95ef1dcd5a90032d7609569d67af7a`.
- Canonical Rebalanced DLL SHA-256: `a980ecfeec4553c208280ca6ca2ca49196d4fb2bab6b6e121c908d0550ce0c4f`.
- Handoff DLL: `PrayerClarity.Rebalanced-0.2.15-test.dll`; it is the exact canonical Rebalanced DLL bytes from the run, renamed without rebuilding.
- The accepted CI artifact was produced while the shared workflow still stamped its package filename/BUILD_INFO as 0.2.14. Those stale packaging labels are **not** authoritative; the compiled assembly itself contains `0.2.15.0`, informational version `0.2.15+ac953fe25ef17dbaf63340a7b9309dadec7e207e`, and plugin version `0.2.15`. The workflow is corrected as part of stable promotion; the accepted DLL itself is not rebuilt.
- Candidate changes:
  - Repose requirements: **20 / 40 / 90**;
  - Bronze unchanged;
  - Silver unchanged from accepted 0.2.14;
  - Gold keeps best-available-tier narrowing and additionally limits that tier to the game-derived maximum total skull score;
  - no hard-coded terminal tier/body IDs/10-skull value;
  - vanilla `GameSave.GenerateBody`, RNG and body construction remain authoritative;
  - temporary body-catalog projection is restored in a finalizer.
- Accepted runtime evidence:
  - the live 1.407 terminal fixture contained 33 ordinary candidates across tiers 2..3, with best tier 3, maximum total skull score 10, and 9 tied maximum-score candidates;
  - ten consecutive real `GameSave.GenerateBody(2,4,-1,-1)` calls exercised the production Gold seam;
  - every completed assertion selected tier 3 with total skull score 10, saw the same 9-candidate maximum-score scoped pool, and confirmed exact catalog restoration after the call;
  - an 11th native tier-3 body generation began before the research harness itself stalled from batching too many heavyweight calls into one UI callback; the harness issue is separate from production Gold behavior;
  - the changed Gold-generation/restoration property is accepted without further user repetition.
- Silver remains unchanged from accepted 0.2.14 and was not reopened.
- Earlier-progression testing is not required because the implementation derives the best available tier and maximum score dynamically from the live Repose-expanded range.
- Stable promotion PR: **#29**, squash-merged to `main` as `c9dede71131dbab4322e37409ca0bdf41994fcb9`.
- Stable publication workflow: `35798813653`; result: **success**.
- Published release: `rebalanced-v0.2.15` -> target `ac953fe25ef17dbaf63340a7b9309dadec7e207e`.
- Release asset ID: `582493849`, canonical filename `PrayerClarity.Rebalanced.dll`, asset digest `sha256:a980ecfeec4553c208280ca6ca2ca49196d4fb2bab6b6e121c908d0550ce0c4f`.
- Publication reused and hash-verified the exact accepted CI DLL; **no rebuild occurred**.
- Final status: **accepted, merged to main, frozen by accepted ref, and published as stable Rebalanced release**.
- Numbered accepted bytes are immutable.

This file records handed executable artifacts once PrayerClarity research reaches a point where the user's installed Graveyard Keeper 1.407 runtime must provide evidence.


### Repose Gold one-button runtime companion

- Research-only companion: `PrayerClarity.ReposeGoldSelfTest 0.1.0`.
- Exact self-test source SHA: `2f878bbee45bc7621e0f67bb9af864d7a2f94d26`.
- CI run: `35792990408`; result: **success, 0 warnings, 0 errors**.
- Artifact ID: `10722798343`.
- Artifact ZIP digest: `sha256:beeabab41441aeec06e9fa5a685d65ba66f881858140c0bb6cab4d56f0baa092`.
- Self-test DLL SHA-256: `382061c506177d65e0116dd677e94f10f505e534b143907be635fc23f837865d`.
- Target production bytes remain the exact 0.2.15 candidate DLL SHA-256 `a980ecfeec4553c208280ca6ca2ca49196d4fb2bab6b6e121c908d0550ce0c4f` from source `ac953fe25ef17dbaf63340a7b9309dadec7e207e`.
- The companion does not replace production mechanics. It opens an F1 research window with one button.
- The button refuses to run if a real `buff_skull` is already active; otherwise it activates `buff_skull` through native `BuffsLogics.AddBuff`, injects the already-accepted post-Donkey Gold pending state, and performs 16 real `GameSave.GenerateBody(2,4,-1,-1)` calls.
- It independently inspects the live 1.407 body catalogue, expects the canonical fixture of 33 ordinary definitions across tiers 2..3, best tier 3, maximum total skull score 10, and 9 tied maximum-score definitions.
- During every generation it verifies that production installed a narrowed scoped body catalogue containing only maximum-score candidates, that the actually selected `BodyDefinition` has the expected best tier/score, and that the exact original `GameBalance.bodies_data` reference is restored after the call.
- Cleanup removes the synthetic buff. If production catalogue restoration fails, the harness records FAIL first and then performs an emergency restore of the exact pre-test catalogue reference.
- What this proves: the new 0.2.15 Gold filtering/generation/restoration path under the real game runtime.
- What this intentionally does not re-prove: the already accepted ordinary-Donkey FlowCanvas caller predicate or sermon calendar/pulpit lifecycle.


#### Self-test 0.1.0 runtime result — rejected harness, accepted Gold-path evidence

- User runtime on Graveyard Keeper 1.407 loaded Rebalanced 0.2.15 and Self-Test 0.1.0 correctly.
- 0.1.0 executed a synchronous loop of 16 real `GameSave.GenerateBody` calls directly from the UI action.
- The game became unresponsive during that batched run; therefore Self-Test 0.1.0 is **rejected as a reusable test harness** and must not be run again.
- Before the hang, samples 1–10 completed their full independent assertions. Every one selected tier 3, total skull score 10, saw exactly 9 scoped maximum-score candidates, and observed `catalog_restored=true`.
- The native game log then recorded an 11th body creation/generation (`body_3_2`, tier 3) before the harness emitted its own sample-11 assertion. This localizes the failure to the batched harness execution/verification envelope rather than demonstrating a Gold-generation failure.
- For the **production Gold-generation/restoration property**, this evidence is sufficient: the harness directly observed the complete scoped candidate set (9 tied max-score definitions) and verified the selected body plus exact catalog restoration on ten consecutive real native calls. Further user repetition would add cost without materially increasing confidence.
- The missing final cleanup log belongs to the rejected harness session lifecycle, not to the per-call production finalizer; per-call production restoration was already observed ten times.
- Repose Gold generation/restoration is therefore **accepted runtime evidence for the 0.2.15 candidate**. No rerun with 0.1.1 is required for this change.

#### Self-test 0.1.1 replacement

- Status: **reserve regression diagnostic; not required for current 0.2.15 acceptance**.
- Exact source SHA: `d52e3fce96e3c44316e824cb2be6955fe2f70ddf`.
- CI run: `35795884919`; result: **success, 0 warnings, 0 errors**.
- Artifact ID: `10723344922`.
- Artifact ZIP digest: `sha256:206c2739b1a50f7af9cc7ed0a62c5012c2138e7b2569714e1bae692fb2b17519`.
- Self-test DLL SHA-256: `e7b2685f95c5b0ca4e0c82cb0641252ba8e3bec8697adac18fbe31017eff2274`.
- Production candidate remains unchanged: Rebalanced 0.2.15 source `ac953fe25ef17dbaf63340a7b9309dadec7e207e`, DLL SHA-256 `a980ecfeec4553c208280ca6ca2ca49196d4fb2bab6b6e121c908d0550ce0c4f`.
- 0.1.1 removes the synchronous 16-call UI loop. It performs **4 real GenerateBody calls**, one paced step at a time with 0.25 s between steps, while keeping the same production-path, scoped-catalog, selected-body and restoration assertions.
- Four samples are sufficient because the core Gold assertion is deterministic: the harness independently verifies that the live scoped candidate set contains all 9 tied maximum-score definitions; repeated generation is retained only as a small runtime/restoration sanity sample, not as statistical proof.

## PrayerClarity: Rebalanced 0.2.14 — accepted stable

- User runtime acceptance: **2026-09-20**.
- Frozen accepted ref: `accepted/rebalanced-0.2.14`.
- Exact tested/build source SHA: `11fa4648fe57995938a2a17093ac4ed4f5e314cd`.
- Candidate CI run: `35503599956`; result: **success**.
- Artifact ID: `10603401146`.
- Artifact: `PrayerClarity-rebalanced-0.2.14-ci-11fa4648fe57995938a2a17093ac4ed4f5e314cd`.
- Artifact ZIP digest: `sha256:5e170c82b46dc42d8e4e3799bf91fddf2e651c7aafc409b7dd65328c18d799cc`.
- Accepted/release DLL SHA-256: `1f26c487777c4c4744f6ea4318796d369aead2b2fbd6ed299ad3edd8e79b4dc8`.
- Stable promotion PR: **#26**, squash-merged to `main` as `8a2e9634538dd0a789fc0636a5a427fa484bdcaf`.
- Accepted behavior:
  - Rebalanced Technology uses **При успехе:** / **On success:** instead of **Бонусы при успехе** / **Bonuses on success**;
  - prayer-item tooltips remain unchanged and keep the existing **Бонусы при успехе** hierarchy;
  - all accepted 0.2.13 mechanics, balance, requirements, payouts, durations, Soul wording, HUD and Temporary Effects behavior remain unchanged.
- Runtime evidence explicitly confirmed both requested conditions: Technology shows **При успехе:** and prayer-item tooltips still show **Бонусы при успехе**.
- Status: **accepted and promoted to main; publication must reuse the exact handed DLL bytes without rebuilding**.
- Stable publication workflow: `35504153407`; result: **success**. It downloaded artifact `10603401146`, re-verified DLL SHA-256 `1f26c487777c4c4744f6ea4318796d369aead2b2fbd6ed299ad3edd8e79b4dc8`, and published the exact accepted bytes without rebuilding.
- Published release: `rebalanced-v0.2.14` -> target `11fa4648fe57995938a2a17093ac4ed4f5e314cd`, asset `PrayerClarity.Rebalanced.dll`.
- Final status: **accepted, merged to main, frozen by accepted ref, and published as stable Rebalanced release**.

## PrayerClarity: Rebalanced 0.2.13 — accepted stable

- User runtime acceptance: **2026-09-20**.
- Frozen accepted ref: `accepted/rebalanced-0.2.13`.
- Exact tested/build source SHA: `4726160ced2dfd15f08b10cc0499eb8c5490e3eb`.
- Candidate CI run: `35482448986`; result: **success**.
- Artifact ID: `10596500719`.
- Artifact: `PrayerClarity-rebalanced-0.2.13-ci-4726160ced2dfd15f08b10cc0499eb8c5490e3eb`.
- Artifact ZIP digest: `sha256:bc72d63459537e4e52c1a6e71b515d2ca8a9ca19a6d6688be2b8cc26057d7911`.
- Accepted/released DLL SHA-256: `da8633b622ad755ca9bac76bbf737e37d6a0f5fb24dab8862efa297c2b1874bd`.
- Stable promotion PR: **#20**, squash-merged to `main` as `b8888d32648a6f7c16f473792636872bc8212344`.
- Accepted behavior:
  - specialist prayers no longer carry unrelated prayer-owned Faith/donation percentage bonuses or fixed Faith/money outputs;
  - Prayer for Donations pays **+5 / +15 / +30 silver**;
  - Combo Prayer Faith remains **+100 / +150 / +200%** and donations are **+100 / +200 / +300%**;
  - 0.2.12 Technology layout repair is accepted: cleaned specialists no longer revive the stock requirement sentence, and crafting-location text stays at the bottom;
  - Soul Contentment shows the effective **+20% Soul Gratitude** with the native Soul Gratitude icon;
  - BSS Soul's Repose uses the approved concise scaling explanation that more Soul Gratitude means a larger Faith bonus.
- Runtime screenshots/user feedback explicitly accepted the final Soul Contentment and Soul's Repose wording; earlier 0.2.11/0.2.12 checks had already confirmed the resource values, specialist cleanup, Technology ordering, prayer-item tooltip, HUD and Temporary Effects behavior.
- Stable publication workflow: `35482867100`; result: **success**. It downloaded artifact `10596500719`, re-verified the accepted DLL hash, and published the exact accepted bytes without rebuilding.
- Published release: `rebalanced-v0.2.13` -> target `4726160ced2dfd15f08b10cc0499eb8c5490e3eb`, asset `PrayerClarity.Rebalanced.dll`.
- Final status: **accepted, merged to main, frozen by accepted ref, and published as stable Rebalanced release**.

## PrayerClarity: Rebalanced 0.2.12 candidate

- Status: **handed for focused Technology runtime/visual acceptance; not stable; do not merge to main yet**.
- Candidate branch: `candidate/rebalanced-0.2.12`.
- Exact build/source SHA: `2ac84b31dce30ccb35648a3706e58f40dda6429d`.
- GitHub Actions run: `35481230526`; result: **success**.
- Artifact ID: `10595647907`.
- Artifact: `PrayerClarity-rebalanced-0.2.12-ci-2ac84b31dce30ccb35648a3706e58f40dda6429d`.
- Artifact ZIP digest: `sha256:9a96958fe6c7f37c92715783487880e4deedc8f1c8b338a0c8c331de070abb2c`.
- Handoff DLL: `PrayerClarity.Rebalanced-0.2.12-ci.dll`; SHA-256: `2b8b9246781749be4a33988d99c0fbccffd85979803032a862fdc8a027f79a28`.
- 0.2.12 preserves the accepted-in-testing 0.2.11 balance/specialist-cleanup values unchanged.
- Technology presentation repair:
  - when specialist cleanup leaves no generic Faith/money success contribution, PrayerClarity no longer depends on vanilla emitting the optional `preach_params_2` block;
  - it uses the always-present prayer requirement/lore row as the Rebalanced fallback anchor, removes the stale stock requirement sentence, and inserts Base result / Bonuses on success before the stock crafting-location footer;
  - Rebalanced Soul Contentment suppresses its obsolete stock +10% Technology sentence so the effective +20% effect is the only numeric mechanic shown.
- Vanilla 1.0.31 is still the accepted stable sibling. The workflow rebuilt Vanilla only as a compile/regression check; no new Vanilla artifact is handed out or accepted here.
- Requested focused runtime acceptance:
  1. Technology -> **Молитва об упокоении** (or another cleaned specialist): no stock “20–60 required...” sentence; lore -> Base result -> Bonuses on success -> crafting location at the bottom; q20/q40/q60 tier data unchanged.
  2. Technology -> **Молитва о процветании**: Commercial Blessing x1/x2/x3 remains; no generic Faith/money garnish; crafting location is again at the bottom.
  3. Technology -> **Молитва о довольствии душ**: obsolete vanilla +10% sentence is absent; effective Rebalanced +20% is shown.
  4. Spot-check Donations +5/+15/+30 silver and Combo Faith +100/+150/+200%, donations +100/+200/+300% remain intact.
- No repeat HUD, Temporary Effects, prayer-item-tooltip or full-sermon test is requested unless this candidate exposes a new discrepancy; those surfaces passed in 0.2.11 and 0.2.12 changes only Technology presentation.
- Numbered binaries are immutable after this handoff.
- User runtime result, 2026-09-20: **0.2.12 Technology repair passed**.
  - cleaned specialist Technology tooltips no longer show the stale stock requirement sentence;
  - Base result / Bonuses on success / tier effect rows are back in the intended order;
  - crafting location is back at the bottom;
  - Prosperity keeps Commercial Blessing x1/x2/x3;
  - Donations +5/+15/+30 silver and Combo Faith +100/+150/+200%, donations +100/+200/+300% remain correct;
  - prayer-item tooltip, HUD and Temporary Effects remained correct.
- One wording issue remains before stable acceptance: Russian Soul Contentment currently says `За исцеление души: +20% благодарности`; the mechanic is correct, but the named resource **Soul Gratitude** is not explicit. Prefer the existing `(gratitude_points)` game icon/token rather than another long noun phrase.
- New non-blocking UX observation: BSS Soul's Repose is mathematically stronger than its +50/+100/+150% tier numbers look because those percentages apply to the enlarged Souls Faith base `Church Quality + current Soul Gratitude`. The current Base result already shows that dependency, but a shared effect line could explain that the tier bonus is calculated from that base without adding more rows.


## PrayerClarity: Rebalanced 0.2.11 candidate

- Status: **handed for runtime/visual acceptance; not stable; do not merge to main yet**.
- Candidate branch: `candidate/rebalanced-0.2.11`.
- Exact build/source SHA: `71bf43a3e622bfc44cd58b991df390389a5debb5`.
- GitHub Actions run: `35480090700`; result: **success**.
- Artifact ID: `10595801273`.
- Artifact: `PrayerClarity-rebalanced-0.2.11-ci-71bf43a3e622bfc44cd58b991df390389a5debb5`.
- Artifact ZIP digest: `sha256:753fba0c8aebfda7d29d132038dddff7d21a73a8ff7d14c5a0362b05e90816e5`.
- Handoff DLL: `PrayerClarity.Rebalanced-0.2.11-ci.dll`; SHA-256: `2768796539631904962f9b89f36a29ee9649c4012eae000ee73933c857c9ad6b`.
- Vanilla sibling was rebuilt only as a compile/regression check at unchanged version 1.0.31; this candidate changes Rebalanced behavior only.
- Candidate behavior:
  - Молитва о пожертвованиях: success-only flat reward **+5 / +15 / +30 silver**;
  - Комбо-молитва: Faith **+100 / +150 / +200%**, donations **+100 / +200 / +300%**;
  - utility/specialist prayers explicitly remove unrelated prayer-owned Faith/donation percentages and fixed Faith/money outputs;
  - named specialist effects, durations, Repose logic, Roots cap, Commercial Blessings and Imagination Story rewards remain unchanged;
  - Молитва за упокой душ remains the deliberate Faith-scaling specialist and keeps +50 / +100 / +150% Faith.
- Requested runtime/visual acceptance:
  1. inspect Молитва об упокоении (or another ordinary specialist): under **Бонусы при успехе** the named effect should appear without generic Faith/donation rows;
  2. inspect Молитва о процветании: Commercial Blessing x1/x2/x3 must remain, while generic Faith/money rows are gone;
  3. inspect Молитва о пожертвованиях: +5/+15/+30 silver;
  4. inspect Комбо-молитва: Faith +100/+150/+200%, donations +100/+200/+300%;
  5. if convenient, inspect one Better Save Soul utility prayer and confirm only its named specialist effect remains.
- A full sermon payout retest is not required unless the projected tooltip values disagree with runtime behavior or a PrayerClarity error appears, because the accepted stock payout path consumes these same projected craft fields/output rows.
- Numbered binaries are immutable after this handoff.
- User runtime result, 2026-09-20: **balance/specialist cleanup passed, Technology presentation failed; candidate not accepted**.
  - Donations +5/+15/+30 silver confirmed.
  - Combo Faith +100/+150/+200% and donations +100/+200/+300% confirmed.
  - specialist generic Faith/donation garnish removal confirmed.
  - prayer-item tooltips, HUD, Temporary Effects and other checked surfaces remained correct.
  - Technology tooltips regressed for cleaned specialists because vanilla omitted the `preach_params_2` anchor once generic resource contributions became empty; the stock requirement sentence reappeared and the crafting-location footer came before appended Clarity sections.
  - Rebalanced Soul Contentment also retained the stale stock +10% description despite the effective +20% mechanic.
- **0.2.11 is superseded by 0.2.12 for presentation repair; its handed bytes/source remain immutable evidence.**

## PrayerClarity: Vanilla 1.0.32 — accepted stable

- User runtime acceptance: **2026-09-20**.
- Frozen accepted ref: `accepted/vanilla-1.0.32`.
- Exact tested/build source SHA: `aaabd3cf154faa019e36b2112439ec9990fdc1bd`.
- Candidate CI run: `35502879595`; result: **success**.
- Artifact ID: `10602579329`.
- Artifact: `PrayerClarity-1.0.32-ci-aaabd3cf154faa019e36b2112439ec9990fdc1bd`.
- Artifact ZIP digest: `sha256:774f041e451bcac296cb20aca0a1c9c2a6a1cdca2e8b72e4e557e0524b049e85`.
- Accepted/release DLL SHA-256: `c12742a23d78214c9d4514a758f9c2417a578df683bebb2cda32a69a8d7674bb`.
- Stable promotion PR: **#23**, squash-merged to `main` as `71457fe9bcf0f068687ea23f7ebf548582f13efa`.
- Accepted behavior:
  - Vanilla Technology uses **При успехе:** / **On success:** as the condition-style success header;
  - a shared stock Faith/donation rider and a shared named effect are rendered as one continuous success-only block;
  - spacing before Bronze/Silver/Gold tier blocks remains;
  - prayer-item tooltips remain unchanged and keep the existing **Бонусы при успехе** hierarchy;
  - stock Graveyard Keeper 1.407 prayer mechanics, requirements, rewards and balance remain unchanged.
- Runtime evidence explicitly confirmed that the new Technology wording reads naturally across prayers and that prayer-item tooltips did not change.
- Status: **accepted and promoted to main; publication must reuse the exact handed DLL bytes without rebuilding**.
- Stable publication workflow: `35503356665`; result: **success**. It downloaded artifact `10602579329`, re-verified DLL SHA-256 `c12742a23d78214c9d4514a758f9c2417a578df683bebb2cda32a69a8d7674bb`, and published the exact accepted bytes without rebuilding.
- Published release: `v1.0.32` -> target `aaabd3cf154faa019e36b2112439ec9990fdc1bd`, asset `PrayerClarity.dll`.
- Final status: **accepted, merged to main, frozen by accepted ref, and published as stable Vanilla release**.

## Current stable baselines — 2026-09-20

- **PrayerClarity: Vanilla 1.0.32** — `accepted/vanilla-1.0.32`, source `aaabd3cf154faa019e36b2112439ec9990fdc1bd`, release `v1.0.32`, DLL SHA-256 `c12742a23d78214c9d4514a758f9c2417a578df683bebb2cda32a69a8d7674bb`.
- **PrayerClarity: Rebalanced 0.2.15** — `accepted/rebalanced-0.2.15`, source `ac953fe25ef17dbaf63340a7b9309dadec7e207e`, release `rebalanced-v0.2.15`, DLL SHA-256 `a980ecfeec4553c208280ca6ca2ca49196d4fb2bab6b6e121c908d0550ce0c4f`.
- Entries below are immutable historical build/test evidence. A section naming an older release records what was stable **at that point in the history**; it does not override this current-baseline header.


### Vanilla direct-runtime coverage note — 2026-09-20

- The user has now explicitly loaded **PrayerClarity: Vanilla 1.0.31** and supplied direct in-game Technology screenshot evidence.
- The screenshot confirms the accumulated shared Clarity presentation is active on Vanilla itself, but also exposes one remaining Vanilla-specific UX gap: `Bonuses on success -> Faith +10% -> blank gap -> Effect` reads as if the named effect were outside the success condition.
- This is a presentation/hierarchy issue only; no stock prayer mechanic discrepancy is indicated.
- A focused Vanilla 1.0.32 candidate should test only the new success-scope hierarchy plus a small regression spot-check of the already accepted shared surfaces.
- Focused direct Vanilla checklist:
  1. Technology: ordinary prayer Base result uses compact parenthetical dependencies; stock success values remain stock.
  2. Technology: Prosperity keeps Commercial Blessing x1/x2/x3; Prayer for Excellence shows its stock lore once at Writing Desk II.
  3. Prayer item: visible spacing between lore -> Base result -> Bonuses on success; 100%-success threshold stays on one line.
  4. Reward rows: item rewards render as one `name ×N` line and percentage rows omit the old “of base value” suffix.
  5. Soul's Repose: Base result still names both Church Quality and Soul Gratitude inputs, while all prayer mechanics/requirements remain vanilla.
  6. Character -> Temporary Effects / HUD: only a visual spot-check is needed; no Rebalanced-only semantics should appear.

## Shared candidate — Vanilla 1.0.31 / Rebalanced 0.2.10

- Status: **accepted and promoted to main; stable publication uses these exact handed bytes**.
- Candidate branch: `candidate/rebalanced-0.2.10`.
- Exact build/source SHA: `30b036f16dc6a7964f7ef72e2e3ececa5951c812`.
- GitHub Actions run: `35475014644`; result: **success**.
- Artifact ID: `10594265029`.
- Artifact: `PrayerClarity-shared-ui-1.0.31-rebalanced-0.2.10-ci-30b036f16dc6a7964f7ef72e2e3ececa5951c812`.
- Artifact ZIP digest: `sha256:a01744655280ee598451c79d64bee3c845e1e5d521c253cc64e082342adf3c0a`.
- **PrayerClarity: Rebalanced 0.2.10** handoff DLL SHA-256: `1f7131bda66554bb18396bed28a5997a531c2a0b9a44df1a61071d2c7e9b78e1`.
- **PrayerClarity: Vanilla 1.0.31** handoff DLL SHA-256: `140a2b3bc21eaa0b9e95f344a8a9ab5f47d5b9aa37572a0b159d905c79351ba3`.
- Candidate change:
  - replace the ineffective `BubbleWidgetBlankSeparatorData` item-tooltip spacing attempt with one controlled leading newline on PrayerClarity-owned **Base result** and **Bonuses on success** title rows;
  - apply this only to prayer-item tooltips, including the fallback append path;
  - remove the now-unused blank-separator helper from `ItemTooltipPresentation`;
  - Technology comparison tooltips remain unchanged.
- All accepted 0.2.9 behavior is preserved: Repose q20/q40/q60, stock Excellence lore fallback, compact Base-result dependency grammar, Roots cap presentation and existing mechanics.
- Runtime acceptance requested:
  - inspect a prayer item with lore and confirm visible `lore -> gap -> Base result -> gap -> Bonuses on success` rhythm;
  - spot-check that no excessive double-gap appears when the stock prayer description itself wraps;
  - no mechanics, Repose, Excellence-lore or Technology retest is required unless the presentation exposes a discrepancy.
- Numbered binaries are immutable after this handoff.
- User runtime/visual result, 2026-09-20: **passed**. The prayer-item tooltip now shows a clearly visible lore -> gap -> Base result -> gap -> Bonuses on success rhythm. No additional spacing change is requested.
- The supplied acceptance session loaded **PrayerClarity: Rebalanced 0.2.10** successfully; no PrayerClarity-specific error was reported. Repose q20/q40/q60 and the restored stock Excellence lore were already accepted in the preceding 0.2.9 pass and are carried forward unchanged.
- Frozen accepted refs: `accepted/vanilla-1.0.31` and `accepted/rebalanced-0.2.10`, both pointing to exact tested source `30b036f16dc6a7964f7ef72e2e3ececa5951c812`.
- Stable promotion PR: **#14**, squash-merged to `main` as `1d4e196f51f067b8d0127aafa8505cfc4ee88d40`.
- Stable publication workflow: `35476301759`; result: **success**. It downloaded artifact `10594265029` from the accepted CI run, re-verified both recorded DLL hashes, and published the exact accepted bytes without rebuilding.
- Published releases:
  - `v1.0.31` -> target `30b036f16dc6a7964f7ef72e2e3ececa5951c812`, asset `PrayerClarity.dll`;
  - `rebalanced-v0.2.10` -> target `30b036f16dc6a7964f7ef72e2e3ececa5951c812`, asset `PrayerClarity.Rebalanced.dll`.
- Final status: **accepted, merged to main, frozen by accepted refs, and published as stable sibling releases**.


## Shared candidate — Vanilla 1.0.30 / Rebalanced 0.2.9

- Status: **handed for runtime/visual acceptance; not stable; do not merge to main yet**.
- Candidate branch: `candidate/rebalanced-0.2.9`.
- Exact build/source SHA: `305c18c7082ccf93f5b31eab264eb73f539902ef`.
- GitHub Actions run: `35474112411`; result: **success**.
- Artifact ID: `10594182101`.
- Artifact: `PrayerClarity-shared-ui-1.0.30-rebalanced-0.2.9-ci-305c18c7082ccf93f5b31eab264eb73f539902ef`.
- Artifact ZIP digest: `sha256:85b168fb1b99b0a7d5f2e6c7a59c96eb1534a284d49ee4e714a7d6fe2cc8e977`.
- **PrayerClarity: Rebalanced 0.2.9** handoff DLL SHA-256: `928de524fc5aacce347d2a425926db7343310c1d8aa9acf7d081cd54abb6bc99`.
- **PrayerClarity: Vanilla 1.0.30** handoff DLL SHA-256: `b7819e6048edab2ce2c0852aa454715b58c472da66d9887917ca570a7c432747`.
- Candidate changes:
  - Rebalanced ordinary Repose Gold 100%-success requirement changes from q50 to **q60**; Bronze/Silver remain q20/q40, corpse-quality behavior and 30/42/54-minute durations are unchanged;
  - single-prayer item tooltips add one native blank separator before **Base result**, matching the existing native gap before **Bonuses on success**; Technology tooltips are unchanged;
  - crafting descriptions for Prayer for Excellence receive a narrow fallback to the game's own localized `b_star_d` lore only when the Excellence output path omitted it.
- Lore evidence:
  - direct decompiled `ItemDefinition.GetItemDescription` falls back from colon-quality IDs to the base `*_d` localization key;
  - direct decompiled `CraftDefinition.GetDescription` routes multi-quality outputs through `Item.GetMultiqualityItemDescription`, which does not use that same colon fallback;
  - public extracted localization data contains `b_star_d` but no corresponding quality-specific Excellence description key observed in the user's Desk II path;
  - the production fallback therefore reuses existing stock localized copy instead of inventing new text or globally intercepting localization.
- Runtime acceptance requested:
  - verify Rebalanced Repose now reads q20 / q40 / q60 in Technology and that no other Repose behavior changed;
  - inspect one prayer item tooltip and confirm the visual rhythm is lore -> gap -> Base result -> gap -> Bonuses on success;
  - inspect Prayer for Excellence at Writing Desk II and confirm its short stock lore line appears naturally and only once;
  - spot-check that the accepted 1.0.29 / 0.2.8 Base-result parentheses and Roots active-cap presentation remain unchanged.
- Numbered binaries are immutable after this handoff.
- User runtime/visual result, 2026-09-20: **0.2.9 partial acceptance**. Rebalanced Repose q20/q40/q60 is confirmed in-game. With Longer Days configured to 675 seconds/day, the unchanged 30/42/54-minute Repose durations correctly present as approximately 2.7/3.7/4.8 game days. Prayer for Excellence now shows the restored stock lore at Writing Desk II.
- The intended item-tooltip vertical spacing did **not** become visibly larger. Screenshot evidence shows lore -> Base result and Base result -> Bonuses on success remain visually tight. This is not a PrayerClarity runtime error; the session loaded Rebalanced 0.2.9 successfully and no PrayerClarity-specific error was logged.
- Source follow-up: `BubbleWidgetBlankSeparatorData` has no draw behavior and its visible height comes only from the serialized prefab/widget size. On this item-tooltip surface that prefab spacing is effectively negligible, so inserting another BlankSeparatorData row did not satisfy the UX goal. Do not repeat that mechanism in the next candidate.


## Shared UI candidate — Vanilla 1.0.29 / Rebalanced 0.2.8

- Status: **handed for runtime/visual acceptance; not stable; do not merge to main yet**.
- Candidate branch: `candidate/rebalanced-0.2.8`.
- Exact build/source SHA: `436cda740d826cbbd8964dec3dd6002892efbac4`.
- GitHub Actions run: `35473008241`; result: **success**.
- Artifact ID: `10593861146`.
- Artifact: `PrayerClarity-shared-ui-1.0.29-rebalanced-0.2.8-ci-436cda740d826cbbd8964dec3dd6002892efbac4`.
- Artifact ZIP digest: `sha256:b63f1f0c03c964ce355eee660af6f686c017c24364bc4871c9d6d0e1eaa4ff28`.
- **PrayerClarity: Rebalanced 0.2.8** handoff DLL SHA-256: `70a3b7e44bd96a72f15f3b7b4a654607c68d05e0109c92561a7c352fa547f501`.
- **PrayerClarity: Vanilla 1.0.29** handoff DLL SHA-256: `fb08bf6f7bd3727683b057b5e493375b79ef23bb1eb08dc905c2c40635c446d9`.
- Candidate changes:
  - compact Base result dependencies to parenthetical source labels: `Faith (Church Quality)` / `Donations (Graveyard Quality)`; Soul's Repose keeps Soul Gratitude in the Faith source;
  - Rebalanced Shoots & Roots prayer-selection surfaces show only the direct tier reduction; the accepted 95% combined safety cap remains mechanically unchanged and remains visible in Character -> Temporary Effects;
  - preserve all accepted 1.0.28 / 0.2.7 compact percentage, item-reward and atomic-success-row behavior;
  - keep the accepted top-HUD decimal-dot residual behavior; no heavier punctuation-only workaround was added.
- Runtime acceptance requested:
  - inspect ordinary prayer Base result in Russian and confirm the parenthetical dependency grammar is immediately understandable and materially narrower;
  - inspect BSS Soul's Repose if convenient and confirm Faith still clearly shows both Church Quality and Soul Gratitude as inputs;
  - inspect Rebalanced Shoots & Roots in Technology/prayer-item tooltip/pulpit and confirm the 95% cap text is gone there while Character -> Temporary Effects still shows the cap;
  - no sermon mechanics, stacking, duration, payout or balance retest is required unless the UI exposes a discrepancy.
- Numbered binaries are immutable after this handoff.
- User runtime/visual result, 2026-09-20: **1.0.29 / 0.2.8 presentation scope passed**. Parenthetical Base result dependencies render correctly in Russian, including Soul's Repose with Soul Gratitude icon; Roots no longer shows the 95% cap on Technology/selection surfaces while Character -> Temporary Effects still shows the cap.
- New follow-up observations are not regressions in this candidate: Rebalanced ordinary Repose still uses the accepted q20/40/50 ladder; prayer-item spacing may benefit from one additional native blank separator before Base result; Prayer for Excellence appears to lack its lore line at Desk II and requires source-localization investigation before any fix.


## Shared UI candidate — Vanilla 1.0.28 / Rebalanced 0.2.7

- Status: **handed for runtime/visual acceptance; not stable; do not merge to main yet**.
- Candidate branch: `candidate/rebalanced-0.2.7`.
- Exact build/source SHA: `94a8ecf4d5b17e5eed2115ec49c97b3a6aec2fa4`.
- GitHub Actions run: `35471739528`; result: **success**.
- Artifact ID: `10593055414`.
- Artifact: `PrayerClarity-shared-ui-1.0.28-rebalanced-0.2.7-ci-94a8ecf4d5b17e5eed2115ec49c97b3a6aec2fa4`.
- Artifact ZIP digest: `sha256:d0a3145aa2c201d983a66d7a3f579cc4481a5d44b22a07a81b6a8023b63ad7f6`.
- **PrayerClarity: Rebalanced 0.2.7** handoff DLL SHA-256: `4e7dedfea65aaee7b4e5cc52ce782053fe1b208327976c0c689d28148e99a0dd`.
- **PrayerClarity: Vanilla 1.0.28** handoff DLL SHA-256: `69a927a235ea3547a6b6392dab7db2d208f672fa57d4b2ad1736733fddfecbb6`.
- Candidate changes:
  - remove the redundant “of base value” phrase from Faith/donation percentage rows;
  - render single-item prayer rewards as one atomic `localized name ×N` row;
  - let PrayerClarity-owned prayer-item success rows use the existing content-width seam so the 100% success threshold does not split;
  - refresh the active game locale once on the first long-prayer HUD timer render, covering plugin initialization before `LoadGameSettings` without per-frame language polling.
- Runtime acceptance requested:
  - verify percentage rows, Commercial Blessing / Story reward rows, and the concrete prayer-item success threshold in Russian;
  - verify the long-prayer HUD timer uses a comma decimal separator on Russian UI; if it still renders a dot, do not add a heavier punctuation-only workaround without a new design decision;
  - spot-check that the accepted Base result / Bonuses on success hierarchy, Soul's Repose dependency context, Technology durations and Character -> Temporary Effects remain unchanged.
- The earlier run `35471702289` failed at compile time because `ItemTooltipPresentation.CreateTextData` had not yet exposed its existing native `max_width` constructor argument. No artifact was uploaded from that failed source. The corrected source above builds both sibling DLLs with 0 errors.
- Numbered binaries are immutable after this handoff.
- User runtime/visual result, 2026-09-20: **the intended 1.0.28 / 0.2.7 presentation changes passed**. Percentage rows no longer contain the redundant “of base value” wording; single-item rewards render on one `name ×N` row; the concrete prayer-item 100%-success threshold remains intact; Technology and Character -> Temporary Effects remained readable, including the controller/gamepad presentation.
- Russian long-prayer HUD punctuation did **not** change: the compact HUD timer still uses a dot rather than a comma. The user explicitly accepted leaving this alone rather than adding a heavier punctuation-only workaround. This is not a blocker for the candidate.
- The supplied session loaded **PrayerClarity: Rebalanced 0.2.7** and exercised synthetic Gold Repose and Gold Shoots & Roots through the existing research console; no PrayerClarity-specific runtime error was reported in the supplied log.
- Follow-up UX/design discussion remains open and therefore belongs to a later numbered candidate: whether to shorten/split the base-donation dependency line, and whether to move the Roots 95% aggregate-cap explanation out of prayer-selection tooltips while retaining it on the active-effect surface.

## Stable sibling releases — Vanilla 1.0.25 / Rebalanced 0.2.0

- User acceptance: **2026-09-18**. After separate runtime passes, the user explicitly approved both candidates for promotion to `main` and stable GitHub publication.
- Frozen accepted refs:
  - `accepted/vanilla-1.0.25` -> source SHA `ebe069b4ad202ae786af9c63ded0ffb00502cff7`
  - `accepted/rebalanced-0.2.0` -> source SHA `26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`
- **PrayerClarity: Vanilla 1.0.25**
  - accepted CI run: `35347730945`
  - artifact ID: `10547383753`
  - artifact: `PrayerClarity-1.0.25-ci-ebe069b4ad202ae786af9c63ded0ffb00502cff7`
  - artifact ZIP digest: `sha256:d7fb3fe9bd789a6568a0204d033c044c2f17c028b8b6125b15858317cb6a0f92`
  - release asset: `PrayerClarity.dll`
  - DLL SHA-256: `72b2237607734d8b50d666cef56e18d458b88c6a456211d496e2410ac646e3a9`
- **PrayerClarity: Rebalanced 0.2.0**
  - accepted CI run: `35346055221`
  - artifact ID: `10547166000`
  - artifact: `PrayerClarity-shared-ui-1.0.24-rebalanced-0.2.0-ci-26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`
  - artifact ZIP digest: `sha256:54a49d9a460d255a594b788089cde9b26433f38eb0f1214d6a3a5efa94c5051f`
  - release asset: `PrayerClarity.Rebalanced.dll`
  - DLL SHA-256: `84cf07be553e137d4663d24267ab18257facfbe44c0a833d87579876742b6de1`
- Stable publication policy: publish these **exact accepted bytes without rebuilding**.
- Vanilla runtime acceptance: current Soul Gratitude row rendered correctly for Soul's Repose; stock Gold Soul's Repose remained q60; non-terminal Repose retained normal stock wording; no PrayerClarity-specific runtime error was observed.
- Rebalanced runtime acceptance: revised Faith/Donations/Combo and requirements rendered as designed; Soul's Repose q30/60/120 and Soul Gratitude context rendered correctly; representative timed prayers matched across pulpit/Technology/Temporary Effects; Test Harness + Rebalanced Compatibility operated correctly; no PrayerClarity-specific runtime error was observed.
- Deferred, non-blocking runtime verification:
  - terminal Repose endpoint wording/behavior awaits a save with terminal Donkey corpse progression;
  - the real successful Silver/Gold Imagination sermon has not yet been visually observed dropping its 3 premium Stories, although the implementation uses the verified native sermon-drop path.
- Any future change required by deferred verification must use a new version; the accepted 1.0.25 / 0.2.0 binaries are immutable.
- Stable promotion PR: **#3**, squash-merged to `main` as `d62d44b5b58b5f799432d7ec8475b1e3fe50dbf6`.
- Publication workflow: `35353739384`; result: **success**.
- Publication downloaded the two exact accepted CI artifacts, re-verified both DLL SHA-256 values, renamed only to canonical install filenames, and created:
  - tag/release `v1.0.25` -> target `ebe069b4ad202ae786af9c63ded0ffb00502cff7`, asset `PrayerClarity.dll`;
  - tag/release `rebalanced-v0.2.0` -> target `26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`, asset `PrayerClarity.Rebalanced.dll`.
- Initial publication run `35353663348` stopped after artifact/hash verification because of a shell-control syntax error in the temporary release workflow; no release asset was created or modified by that failed attempt. The workflow-only fix did not change either accepted binary.
- Status: **accepted, merged to main, frozen by accepted refs, and published as two stable sibling releases**.

## Stable sibling releases — Vanilla 1.0.24 / Rebalanced 0.1.5

- User acceptance: 2026-09-18. The tested shared candidate was explicitly approved for promotion to the stable line for both sibling editions.
- Exact accepted runtime/source SHA: `3b7cea7986138f57d7ace6998b9cc6bca952af1e`.
- Frozen accepted refs: `accepted/vanilla-1.0.24` and `accepted/rebalanced-0.1.5`.
- Stable promotion PR: #1, merged to `main` as `a1510a89a31f687b0fa77d8ab8e5221e3fe34e4b`.
- Publication workflow run: `35290795881`; result: **success**.
- Publication reused the exact accepted CI artifact from run `35286685613` and verified both hashes before release; neither DLL was rebuilt.
- **PrayerClarity: Vanilla 1.0.24** — tag `v1.0.24`, asset `PrayerClarity.dll`, SHA-256 `ab53816f120ee9318944459a976a74bfe48125fabf872b6f9666920c779b4879`.
- **PrayerClarity: Rebalanced 0.1.5** — tag `rebalanced-v0.1.5`, asset `PrayerClarity.Rebalanced.dll`, SHA-256 `aa34c5fc62aa7ad02d32099264389554e2df05c212374ec5be7701fa95c428cf`.
- Runtime acceptance included the shared Technology/presentation polish and the final Temporary Effects gate. Rebalanced Silver semantics matched the accepted rule set; the Vanilla sibling retained stock mechanics/presentation policy where intended.
- Status: **accepted, merged to main, and published as two stable sibling releases**.

## PrayerClarity Test Harness Rebalanced Compatibility 0.1.2

- Type: research/test-only compatibility adapter; not production PrayerClarity behavior.
- Purpose: replace the failed 0.1.1 probe with a hook on the already runtime-verified Harness seam `BuffsLogics.AddBuff(string, Nullable<float>)`. Before the synthetic PlayerBuff is added, the adapter infers Bronze/Silver/Gold from the Harness duration override and writes the same Rebalanced tier/effect tokens that the normal successful-sermon path would capture.
- Source branch: `research/test-harness-rebalanced-compat`.
- Candidate ref: `candidate/test-harness-rebalanced-compat-0.1.2`.
- Exact source SHA: `437e8020208a91818d6ca8aef4dc9f44c2a3fd22`.
- GitHub Actions run: `35289408160`.
- Workflow result: success on `ubuntu-latest`; `net472` Release build and artifact upload passed.
- Workflow artifact ID: `10525896671` (`PrayerClarity-TestHarness-RebalancedCompat-0.1.2-ci-437e8020208a91818d6ca8aef4dc9f44c2a3fd22`).
- Artifact ZIP digest: `sha256:1de2867e41cfd2008be8c06920cb2189b9a4a59d047601159ed418045ac642fd`.
- Handoff filename: `PrayerClarity.TestHarness.RebalancedCompat-0.1.2-ci.dll`; SHA-256: `4b1e13c70e23ae0b02c8a3ef080518f7f054abf6ee7d2112b2deb6c90694788b`.
- Test protocol: replace compatibility 0.1.1 with 0.1.2; keep Rebalanced 0.1.5 and the existing Harness/bridges; use Silver; activate each canonical timed prayer once and skip retired Protection/b_shield. The log should contain `Synthetic Rebalanced tier projected at BuffsLogics.AddBuff` for Roots, Repentance, Repose, Combat and Excellence before judging Temporary Effects text.
- Expected Silver Temporary Effects: Repentance 75%; Roots -30% growth time; Repose Silver reliability wording; Combat +10 damage / +4 armor / 2 HP/s; Imagination +0.7; Excellence +0.5; Soul Contentment +20%; Thorough Cleansing x2.
- Runtime result, 2026-09-18: **passed**. Rebalanced 0.1.5 loaded with compatibility 0.1.2 and the legacy Harness/bridges. The log records successful tier projection immediately before native AddBuff for Silver Roots (`buff_plant`, 72), Repentance (`buff_sins`, 36), Repose (`buff_skull`, 36), Combat (`buff_sword`, 72) and Excellence (`buff_star`, 36). The resulting Temporary Effects UI shows the expected Silver semantics: Roots -30%, Repentance 75%, Repose's half-guaranteed-best wording, Combat +10/+4/2 HP/s, Imagination +0.7, Excellence +0.5, Soul Contentment +20% and Thorough Cleansing x2.
- The current Rebalanced log contains no `InvalidCastException` or `SmartExpression` failure. The earlier Roots exception is therefore **not reproduced** under the correctly tier-projected test path and is no longer treated as a confirmed current blocker. Reopen only if it appears in a normal-sermon/runtime path.
- Status: **test adapter verified; Temporary Effects Silver presentation gate passed; research-only artifact remains non-production**.

## PrayerClarity Test Harness Rebalanced Compatibility 0.1.1

- Type: research/test-only compatibility adapter; not production PrayerClarity behavior.
- Purpose: retain the 0.1.0 Vanilla-GUID compatibility alias and additionally mirror Rebalanced's successful-prayer tier/effect-token capture immediately before the legacy Harness synthetically activates a timed buff. This makes Temporary Effects tests representative of Silver/Gold Rebalanced semantics without running the real sermon/reward path.
- Source branch: `research/test-harness-rebalanced-compat`.
- Candidate ref: `candidate/test-harness-rebalanced-compat-0.1.1`.
- Exact source SHA: `0652b8f9feb18eb6dd72c77756bda6f23b32e3d6`.
- GitHub Actions run: `35288758209`.
- Workflow result: success on `ubuntu-latest`; `net472` Release build and artifact upload passed.
- Workflow artifact ID: `10525277772` (`PrayerClarity-TestHarness-RebalancedCompat-0.1.1-ci-0652b8f9feb18eb6dd72c77756bda6f23b32e3d6`).
- Artifact ZIP digest: `sha256:c48907ef8e721a4c450fdef8c148bf481efdcb499e46c6efea68b3545c5a5d08`.
- Handoff filename: `PrayerClarity.TestHarness.RebalancedCompat-0.1.1-ci.dll`; SHA-256: `f5ee41d053a49411754cbc4dabb645fdb9d59c480711d8e7fed24dc4501642b6`.
- Test protocol: replace compatibility 0.1.0 with 0.1.1, keep Rebalanced 0.1.5 and the existing Harness/bridges, use Silver, activate each timed prayer once, and **skip retired Protection/b_shield** when validating canonical Combat because both Combat and Protection resolve to the same `buff_sword` and double activation extends its timer. Do not save the game with synthetic buffs active.
- Expected Silver Temporary Effects: Repentance 75%; Roots -30% growth time; Repose Silver reliability wording; Combat +10 damage / +4 armor / 2 HP/s; Imagination +0.7; Excellence +0.5; Soul Contentment +20%; Thorough Cleansing x2. Durations should continue to reflect the active Longer Days day length.
- Runtime result, 2026-09-18: **failed before installing the adapter patch**. Startup throws `MissingMethodException: PrayCraftGUI.DoPrayForBuff()` because 0.1.1 incorrectly assumed a zero-argument overload. Test Harness 0.1.2 and its bridges still load afterward, so the screenshot necessarily remains stock/fallback for tier-dependent Rebalanced effects.
- Status: **superseded by compatibility 0.1.2; do not use 0.1.1**.

## PrayerClarity Test Harness Rebalanced Compatibility 0.1.0

- Type: research/test-only compatibility shim; no prayer mechanics, save state, UI or Harmony patches.
- Purpose: allow the existing legacy PrayerClarity Test Harness to load while testing PrayerClarity: Rebalanced. The legacy Harness hard-depends on the Vanilla plugin GUID `nikich.graveyardkeeper.prayerclarity`; the shim exposes only that dependency identity and itself hard-depends on `nikich.graveyardkeeper.prayerclarity.rebalanced`, forcing Rebalanced to load first.
- Source branch: `research/test-harness-rebalanced-compat`.
- Candidate ref: `candidate/test-harness-rebalanced-compat-0.1.0-build2`.
- Exact source SHA: `c251c45687c5e2f45bff9b703719ad7478c5f888`.
- GitHub Actions run: `35287963473`.
- Workflow result: success on `ubuntu-latest`; `net472` Release build and artifact upload passed.
- Workflow artifact ID: `10524862386` (`PrayerClarity-TestHarness-RebalancedCompat-0.1.0-ci-c251c45687c5e2f45bff9b703719ad7478c5f888`).
- Artifact ZIP digest: `sha256:93e6d50dcc4d7519fd4060d18c24dd6e0eccf459cf425df06ad141a34cb8cde4`.
- Handoff filename: `PrayerClarity.TestHarness.RebalancedCompat-0.1.0-ci.dll`; SHA-256: `721b63377b365f7d483a1ac93c4b90fc6e7188154180910acfc88ada3190f23a`.
- Runtime gate: install only alongside PrayerClarity: Rebalanced and the existing Test Harness/bridge DLLs. Confirm the Harness and bridges now load and appear in Configuration Manager. If the Harness has a compile-time assembly reference to `PrayerClarity.dll` rather than only the BepInEx GUID dependency, this shim will not be sufficient; the next runtime log will prove that distinction.
- Runtime result, 2026-09-18: **dependency compatibility proved**. Rebalanced 0.1.5, compatibility 0.1.0, Test Harness 0.1.2 and all three Harness bridge plugins loaded together. The next test exposed that the legacy Buff Bridge bypasses Rebalanced tier capture, so 0.1.0 is superseded by 0.1.1 for tier-dependent Temporary Effects verification.
- Status: **dependency question answered; superseded by compatibility 0.1.1 for active-effect testing**.

## Shared polish candidate — PrayerClarity: Vanilla 1.0.24 / Rebalanced 0.1.5

- Type: narrow shared presentation correction plus one Rebalanced Temporary Effects consistency repair; no prayer balance values, success formulas or gameplay mechanics changed in this candidate.
- Development branch: `dev/shared-tooltip-polish-1.0.24-0.1.5`.
- Frozen candidate ref: `candidate/rebalanced-0.1.5-runtime`; one exact source state intentionally produces both sibling editions.
- Exact build source SHA: `3b7cea7986138f57d7ace6998b9cc6bca952af1e`.
- GitHub Actions run: `35286685613`.
- Workflow result: success on `ubuntu-latest`; all 11 base and Rebalanced localization JSON files parsed, Rebalanced `net472` Release build passed, Vanilla sibling `net472` Release build passed, required embedded locale markers were present, and both candidate DLL pairs were staged and uploaded.
- Workflow artifact ID: `10524397397` (`PrayerClarity-shared-ui-1.0.24-rebalanced-0.1.5-ci-3b7cea7986138f57d7ace6998b9cc6bca952af1e`).
- Artifact ZIP digest: `sha256:b09478ceb3009ff5c7ebb603b9e5de7dcb0404270eeb71c859263230ca72c5d4`.
- Vanilla handoff filename: `PrayerClarity-1.0.24-ci.dll`; canonical filename: `PrayerClarity.dll`; SHA-256: `ab53816f120ee9318944459a976a74bfe48125fabf872b6f9666920c779b4879`.
- Rebalanced handoff filename: `PrayerClarity.Rebalanced-0.1.5-ci.dll`; canonical filename: `PrayerClarity.Rebalanced.dll`; SHA-256: `aa34c5fc62aa7ad02d32099264389554e2df05c212374ec5be7701fa95c428cf`.
- Technology fix: a reward is hoisted into the shared Effect block only when both reward identity **and quantity** are invariant across tiers. Prosperity therefore no longer shows the orphan shared `Effect: Commercial Blessing`; its `x1/x2/x3` Commercial Blessing outputs remain tier-local.
- Temporary Effects audit/fix: the vanilla game still owns the buff title and icon. PrayerClarity replaces only the active-buff description and strategic timer presentation. Rebalanced Roots/Repentance now promote their meaningful remaining duration into that description when their repaired edition-specific active semantics are available; Vanilla keeps the stock broken/unverified presentation policy.
- Requested user test: first inspect Prosperity in Technology in either edition and confirm the shared `Effect: Commercial Blessing` line is gone while Bronze/Silver/Gold still show `x1/x2/x3`. In Rebalanced, activate Shoots & Roots or Repentance and open Character -> Temporary Effects; confirm the concrete effect text is present together with the remaining-duration text when at least one in-game day remains, while the stock title/icon still look normal. No full sermon/mechanics regression pass is required.
- Runtime result, 2026-09-18: the user confirmed Prayer for Prosperity now renders correctly in Technology: the meaningless shared `Effect: Commercial Blessing` line is gone and the Bronze/Silver/Gold `x1/x2/x3` Commercial Blessing quantities remain intact. This Technology fix is accepted.
- Temporary Effects verification is currently blocked by the installed legacy Test Harness. Runtime log shows `PrayerClarity Test Harness 0.1.2` is rejected because its hard dependency is `nikich.graveyardkeeper.prayerclarity` (Vanilla GUID), while the active sibling is `nikich.graveyardkeeper.prayerclarity.rebalanced`; all Harness bridge plugins then skip because the Harness did not load.
- The same runtime log exposed an independent Rebalanced Roots issue before the requested Temporary Effects test: projected plant `SmartExpression` evaluation repeatedly throws `InvalidCastException` on `Ppar("buff_plant")*Ppar("prayerclarity_rebalanced_plant_reduction")`. Treat this as a new mechanics/runtime defect requiring diagnosis before Roots is accepted; do not attribute it to the Test Harness because the Harness never loaded.
- Follow-up runtime evidence, 2026-09-18: compatibility shim 0.1.0 worked. BepInEx loaded Rebalanced 0.1.5, the compatibility alias, Test Harness 0.1.2, Buff Bridge 0.1.4, Button Bridge 0.1.3 and Item Cell Bridge 0.1.5 in one session. The Harness was configured to Silver and synthetic Silver timed buffs were activated across the roster.
- Temporary Effects screenshot/result: tier-invariant effects render correctly (Imagination +0.7, Soul Contentment +20%, Thorough Cleansing x2). Tier-dependent effects fall back to stock/Vanilla presentation: Repentance and Roots show the known-issue text; Repose shows stock +1-tier wording; Combat shows only stock +5 damage; Excellence shows stock +0.2 instead of Silver +0.5. This does **not** yet prove a production Rebalanced presentation defect.
- Root cause of that mismatch is the test path: the Harness Buff Bridge directly calls native `BuffsLogics.AddBuff` and explicitly bypasses the normal sermon path. Rebalanced quality capture is owned by `PlayerComponent.StartPrayAnimation(CraftDefinition,bool)`, so the synthetic path never writes the persisted tier/effect tokens consumed by Rebalanced Temporary Effects. The test harness therefore needs a Rebalanced tier-capture adapter before this surface can be judged.
- Combat timer note: the session activated both canonical Combat `b_sword:2` and retired Protection alias `b_shield:2`; Rebalanced maps both to `buff_sword`, so the same buff was added twice and the screenshot's 12.8-day duration is a Harness artifact rather than the canonical Silver Combat duration.
- Final Temporary Effects runtime result, 2026-09-18: compatibility 0.1.2 correctly projected Silver tier state before synthetic AddBuff. The Rebalanced screenshot matches the expected semantics across every canonical timed prayer: Roots -30%, Repentance 75%, Repose Silver reliability text, Combat +10 damage / +4 armor / 2 HP/s, Imagination +0.7, Excellence +0.5, Soul Contentment +20%, Thorough Cleansing x2. The user explicitly reported the surface as working correctly.
- Vanilla sibling spot-check, 2026-09-18: PrayerClarity 1.0.24 loaded instead of the compatibility alias (BepInEx correctly skipped the lower-version duplicate GUID shim). Vanilla Temporary Effects retained stock/known-issue semantics as intended; the reported long Rage timer came from accidental repeated synthetic activation and is not a PrayerClarity defect.
- The earlier Rebalanced Roots `InvalidCastException`/SmartExpression concern was not reproduced in the correctly tier-projected session. It is downgraded from a confirmed blocker to a non-reproduced prior anomaly; reopen only on normal-sermon evidence.
- Status: **Prosperity Technology correction accepted; Temporary Effects presentation accepted for both sibling editions within the tested scope. No remaining blocker from this polish pass.**

## Shared UI candidate — PrayerClarity: Vanilla 1.0.21 / Rebalanced 0.1.2

- Type: shared Technology information-design/presentation candidate for both editions, plus the accepted Rebalanced specialist-bonus adjustment from **+250/+350/+450%** to **+200/+300/+400%** for Faith, Donations and BSS Soul's Repose. No other Rebalanced mechanic/hook architecture changes.
- Development branch: `dev/shared-tooltip-polish-1.0.21-0.1.2`.
- Frozen candidate ref: `candidate/rebalanced-0.1.2`; one frozen source state intentionally produces both sibling editions.
- Exact build source SHA: `a0bdbc8a66e28a4175376765419ff004d7bae6a7`.
- GitHub Actions run: `35277513366`.
- Workflow result: success on `ubuntu-latest`; all 11 base and Rebalanced localization JSON files parsed, Rebalanced `net472` Release build passed, Vanilla sibling `net472` Release build passed, all required embedded locale markers were present, and both candidate DLL pairs were staged in one run.
- Workflow artifact ID: `10521206721` (`PrayerClarity-shared-ui-1.0.21-rebalanced-0.1.2-ci-a0bdbc8a66e28a4175376765419ff004d7bae6a7`).
- Artifact ZIP digest: `sha256:9a9ac2480d23dd05732da29f539135bfdd7a7b2fa9eccbaf126a25a7ef9d66cc`.
- Vanilla handoff filename: `PrayerClarity-1.0.21-ci.dll`; canonical filename: `PrayerClarity.dll`; SHA-256: `847953001c7fb2a5be008d7d705b2da5971faa38b2b5d33244140a8c9b8be8f6`.
- Rebalanced handoff filename: `PrayerClarity.Rebalanced-0.1.2-ci.dll`; canonical filename: `PrayerClarity.Rebalanced.dll`; SHA-256: `7b08540adb952466e84cd92246e853c07b9a5e26a1e4f22f949439c65c069ddf`.
- Shared presentation scope: Technology follows `shared in-world effect -> terse quality delta`; short mechanics rows are atomic; Effect/special-entity labels gain restrained semantic accents; Commercial Blessing keeps one consistent entity accent; Excellence can color only its key tier value Bronze/Silver/Gold; durations remain plain; new strings ship in all 11 locales. Vanilla and Rebalanced compile the same shared renderer/style/localization infrastructure while edition-specific semantics remain separate.
- Rebalanced wording scope: Roots uses a shared plant-growth explanation plus `growth time -20/-30/-40%`; Repentance uses a shared daily-confessional explanation plus `confession chance 50/75/100%`; Repose uses a shared better-corpse cue with upward/white-skull/red-skull symbols and the accepted natural Bronze/Silver/Gold reliability wording; Imagination lifts its invariant +0.7 writing-quality effect above the tiers and preserves Silver/Gold Story rewards; Excellence presents its +0.2/+0.5/+1.0 values as the key quality ladder; BSS Soul's Repose explicitly explains that base Faith depends on Church Quality and Soul Gratitude before the tier percentage is applied.
- Requested user test: install **one edition at a time**. Primary gate is Rebalanced 0.1.2 in Russian: inspect Repose, Repentance, Shoots & Roots, Imagination, Excellence, Prosperity, Faith, Donations and BSS Soul's Repose where available. Confirm the shared/tier hierarchy reads naturally; skull/up icons render; semantic colors render rather than exposing raw NGUI tags; atomic label/value rows do not split awkwardly; Commercial Blessing and Story accents are coherent; and the tooltip remains viewport-safe. Then spot-check Vanilla 1.0.21 (especially Repose, Imagination/Excellence and Prosperity) to confirm the same shared grammar improves presentation without changing stock mechanics. Existing 0.1.0/0.1.1 runtime evidence already covers the underlying Rebalanced mechanic seams; this pass does not require repeating the full sermon/mechanics smoke unless the UI or values expose a discrepancy.
- Status: **ready for runtime visual/UX verification; not accepted**.

## PrayerClarity: Rebalanced 0.1.1 — presentation/localization polish candidate

- Type: Rebalanced presentation/localization follow-up; **no prayer-mechanics or balance changes** and no Technology-tooltip layout redesign.
- Development branch: `dev/rebalanced-0.1.1`.
- Frozen candidate ref: `candidate/rebalanced-0.1.1-runtime`.
- Exact build source SHA: `40c5f60062267e887756864e096a0f731efe54f0`.
- GitHub Actions run: `35255682271`.
- Workflow result: success on `ubuntu-latest`; Rebalanced `net472` Release build, sibling Vanilla shared-source build, all 11 base/Rebalanced locale-resource checks, artifact staging and upload passed.
- Workflow artifact ID: `10512445366` (`PrayerClarity-Rebalanced-0.1.1-ci-40c5f60062267e887756864e096a0f731efe54f0`).
- Artifact ZIP digest: `sha256:0c97f4fe018d8fb03ef933f0cf8c327283804e6ee67ef8c3ebe483582dc6aedc`.
- Handoff filename: `PrayerClarity.Rebalanced-0.1.1-ci.dll`.
- Canonical install filename inside the artifact: `PrayerClarity.Rebalanced.dll`.
- Handoff/canonical DLL SHA-256: `93313be354b5fc965c6cbef3a456b37c7ef10371935696f0ba1229452f01d721`.
- Scope: Bronze Repose no longer calls the parameterized stock `active.skull` localization key without its required argument; it uses a dedicated Rebalanced semantic string describing the stock-style expanded body range. Roots, Repentance, Repose, Combat and Soul Contentment presentation remains driven by the same effective-rule semantic layer; new Rebalanced wording was shortened and made more player-facing in all 11 `lang_rebalanced` overlays. Common `lang/*.json`, mechanics rules/hooks and Technology layout were not changed. Version metadata/build packaging was advanced to 0.1.1.
- Requested user test: in Russian, inspect the Technology tooltips for Repose (all three qualities), Shoots & Roots, Repentance and Soul Contentment; optionally spot-check Combat. Confirm Bronze Repose is Russian rather than falling back to English, the new effect lines are shorter/easier to parse, and no new clipping/wrapping or excessive tooltip height appears. No sermon execution or mechanics test is required for this candidate.
- Status: **ready for short runtime visual retest; not accepted**.

## PrayerClarity 1.0.10 — Technology tier-first UX candidate

- Type: Clarity-only Technology presentation candidate; no prayer-mechanics or balance changes.
- Development branch: `dev/technology-tooltip-ux-1.0.10`.
- Frozen candidate ref: `candidate/1.0.10`.
- Exact build source SHA: `f21674f0ddcc7e06a7d0d1b587faa57e2e5c0b1d`.
- GitHub Actions run: `35160674466`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` Release build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10473296768` (`PrayerClarity-1.0.10-ci-f21674f0ddcc7e06a7d0d1b587faa57e2e5c0b1d`).
- Artifact ZIP digest: `sha256:31dc000d9e1d70ba00d6bc93c7d56115eb27a11c97d725b0e693e07c5a7bbbab`.
- Handoff filename: `PrayerClarity-1.0.10-ci.dll`.
- Handoff DLL SHA-256: `10c04409124914529c962017a7829eecd4fc1e1c2ae376f0230aaa06446c2ff7`.
- Scope: Technology comparison changes from property-first to `shared invariants -> per-tier snapshots`; uses native quality glyphs as tier headings; uses explicit localized `100% success requires` wording; separates proportional and flat success additions; omits the Commercial Blessing purpose paragraph from the comparison body; gives the Technology body an adaptive maximum width derived from 72% of logical safe-area width with the prior 360 value retained as fallback. The accepted 1.0.9 PrayerClarity-owned viewport clamp remains unchanged.
- Requested user test: inspect Prayer for Prosperity first, then Faith or Donations, Combo and Shoots & Roots in Technology. Confirm the tier grouping is immediately readable, the `% + flat` ambiguity is gone, the threshold wording is clear, normal tier rows no longer wrap unnecessarily, and the bubble remains inside the viewport. Russian at the normal resolution is the primary gate; German and Japanese are useful spot checks if convenient. Ordinary non-prayer Technology tooltips should remain vanilla. No sermon execution is required.
- Status: **ready for runtime UX/geometry verification; not accepted**.

## PrayerClarity 0.1.9 — real-window resize / pulpit layout candidate

- Type: Clarity-only presentation/calibration candidate; no intended prayer-mechanics or balance changes.
- Purpose: replace the disproved child-art resizing attempts with live resizing of the verified real Pray GUI `window`/`container`, retain the game's native sliced frame behavior, separate the `Result` heading from result rows, and repair the remaining special-prayer presentation defects found in the 0.1.8 test.
- Development branch: `dev/pulpit-window-resize-0.1.9`.
- Frozen candidate ref: `candidate/0.1.9`.
- Exact build source SHA: `7bc60963e3eb589ed14b7fe011d9c9338aa38dd9`.
- GitHub Actions run: `34908149431`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10373317657` (`PrayerClarity-0.1.9-ci-7bc60963e3eb589ed14b7fe011d9c9338aa38dd9`).
- Handoff filename: `PrayerClarity-0.1.9-ci.dll`.
- Handoff DLL SHA-256: `3bd34f80d7082ed728732df1e8b9f92f01c567b4f17a427f0383b5735bd2f378`.
- Real-window controls: Configuration Manager section `Prototype pulpit layout tuning v4` exposes `Window extra width` and `Window extra height`. These modify the actual `UI Root/Pray GUI/window` and anchored `container`, not merely decorative child sprites.
- Frame architecture: the verified stock `back` (`Sliced`, border 30), `decore_back` (`Sliced`, border 15) and `header` (`Sliced`, border 65/5/65/5) are resized from captured vanilla dimensions. The `pulpit_bench_back` `Simple` decoration is deliberately not stretched. Header/close controls and controller tips move relative to the enlarged frame.
- Layout calibration: latest 0.1.8 user values are the v4 starting point: context `8/72/14`, Result heading `8/20/14`, result rows `16/4/14`, effect `-122/-30/12` with icon `10`, note `-6/-87/9`, selector `70/45`, prayer button `0/-120`. All remain temporary live tuning controls.
- Result grammar: heading is now an independently positioned/font-sized label; resource rows contain only `Guaranteed` and the shorter `On success (N%)` wording. Specialist arrows remain attached only to the resource actually improved by Faith or Donations.
- Repose wording: stock Clarity uses the player-facing Donkey/body formulation with native `(up)` and `(skull)` symbols; it still does not claim the future Rebalanced bronze/silver/gold reliability ladder.
- Vanilla localization fix: direct audit evidence places `GJL` in `Assembly-CSharp-firstpass`; `R.VanillaLocalize` now resolves `GJL.L(string)` across loaded assemblies once and caches the method. This fixes raw localization IDs such as `blessing_commerce` and the blank Soul's Repose description caused by the old Assembly-CSharp-only resolver.
- Prosperity: uses the vanilla localized `Blessing of commerce` name/description and the stock output count, without the redundant `Reward:` prefix inside an `Effect:` row.
- Soul's Repose: explicitly reuses vanilla `b_souls_d` after the localization resolver fix.
- Thorough Cleansing: still requests verified native sprite `i_sin_shard`; the v4 effect-icon path lazily resolves it and caches only successful sprite lookups, allowing a later UI redraw to recover if the atlas was not ready on the first attempt.
- Localization: all changed PrayerClarity-owned wording is present in the same 11 interface locales as the base game.
- Requested user test: replace 0.1.8 with 0.1.9 and remove the completed frame-slice probe. Keep Test Harness 0.1.0. First adjust only `Window extra width/height` and confirm the *actual frame* grows without stretching the pulpit bench artwork or accumulating drift. Then tune Result heading/rows, selector, prayer button, effect and note as desired. Finally switch Faith -> Donations -> Combo -> Repose -> Prosperity -> Soul's Repose -> Soul Contentment -> Thorough Cleansing and report screenshots plus final F1 values. No sermon execution is required.
- Status: **ready for runtime UX/geometry verification; not accepted**.

## PrayerClarity Pulpit Frame Slice Probe 0.1.0 — completed runtime evidence

- Type: research-only, read-only UI geometry probe; no intended save/player/world mutation.
- Exact source SHA: `74f617ae4ea051e60324b2504611e0b3764e60dc`.
- GitHub Actions run: `34906052752`.
- Workflow artifact ID: `10372548949`.
- Probe DLL SHA-256: `b8c3cc5e82b2e09371f1050100d6834ef41ef834152bea49c0ee4caea64872cd`.
- Runtime evidence, 2026-09-15: the real `UI Root/Pray GUI/window` is a `UIWidget` 274x241; its anchored `container` is 274x199. The visible frame is not one simple bitmap: `back` is an NGUI `UI2DSprite` already configured `Sliced` with 30 px borders; `decore_back` is `Sliced` with 15 px borders; `header` is `Sliced` with 65/5/65/5 borders. `decore` / `pulpit_bench_back` is `Simple` and therefore must not be stretched as the frame grows.
- Consequence: the earlier 0.1.6/0.1.7 failures targeted child artwork rather than the real window boundary. The next resize implementation may legitimately change the root window/container dimensions and use the stock sliced frame contract instead of inventing a custom texture/frame system.
- Status: **question answered; remove probe after capture**.

## PrayerClarity 0.1.8 — runtime result: information improved, fixed-window layout not accepted

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Frozen candidate ref: `candidate/0.1.8`.
- Exact build source SHA: `46f5d9954d710a160308e393f2cb1bf91e28e82a`.
- GitHub Actions run: `34899841772`.
- Workflow artifact ID: `10369988505` (`PrayerClarity-0.1.8-ci-46f5d9954d710a160308e393f2cb1bf91e28e82a`).
- Handoff filename: `PrayerClarity-0.1.8-ci.dll`.
- Handoff DLL SHA-256: `8b78d5bf6b3bd395b0b8399e016f6e49b894e08ad6e509f50c4f4da2d5eb9656`.
- Runtime result, 2026-09-15: the fixed-window composition is conceptually useful but cannot be finalized inside the stock frame. The user explicitly requested true window resizing before accepting final text placement.
- Latest calibration from the supplied 0.1.8 screenshot: context `8/72/14`, result `8/20/14`, effect `-122/-30/12`, effect icon `10`, note `-6/-87/9`, selector `70/45`, prayer button `0/-120`.
- UX findings: `Result` must be movable/font-sized independently from Guaranteed/Success rows; `Additional on success (100%)` is too long and should be shortened; long special effects need localization-safe wrapping; Repose should use a Donkey + higher-quality-body formulation with native upward/skull cues.
- Remaining presentation defects observed in 0.1.8: Prosperity still exposed raw `blessing_commerce`/redundant `Effect: Reward:` wording; BSS Soul's Repose remained blank; Thorough Cleansing still lacked the expected Sin Shard icon. Soul Contentment ordering was reported as corrected.
- Status: **superseded by 0.1.9; information model retained, fixed-window layout rejected**.

## PrayerClarity 0.1.7 — runtime result: fixed-window layout useful, frame resize rejected

- Type: Clarity-only presentation/calibration candidate; no intended prayer-mechanics or balance changes.
- Frozen candidate ref: `candidate/0.1.7`.
- Exact build source SHA: `4df3d204866afc39ee0b848c14bb724101a29761`.
- GitHub Actions run: `34895350258`.
- Workflow artifact ID: `10368661089` (`PrayerClarity-0.1.7-ci-4df3d204866afc39ee0b848c14bb724101a29761`).
- Handoff filename: `PrayerClarity-0.1.7-ci.dll`.
- Handoff DLL SHA-256: `3742468ee5fef4190ad631933e4c0da220657e14aab46b3246fd9799ea99d4f2`.
- Runtime evidence, 2026-09-15: the independent Window extra width/height controls still do **not** enlarge the actual pulpit window. They deterministically stretch/move the child background/decor artwork, proving that the targeted child widgets are not the real usable window boundary. The frame-resize approach is rejected rather than iterated again.
- Accepted UX direction: moving the prayer selector into the upper-right creates enough usable space inside the unchanged stock pulpit to fit the forecast. The second supplied screenshot produced a workable calibration baseline: context `8/72/12`, result `8/20/13`, effect `-122/-35/10`, effect icon `10`, note `-6/-87/9`, selector `90/55`, prayer button `0/-120`.
- Accepted arrow semantics: Faith highlights Faith; Donations highlights money; Combo and non-resource specialists show no green up arrow. The user reported Faith/Donations/Combo as reading correctly.
- New presentation failure: Shoots & Roots effect text can run beyond the available horizontal region. This is a localization problem, not a Russian-only string problem, so the next candidate must wrap the effect label automatically rather than insert language-specific hard line breaks.
- Repose finding: the stock text `corpses can be one tier better` is too abstract. A Donkey/body-quality formulation is clearer. However, stock 1.407 exposes the same `body_max +1` special magnitude at every prayer quality, so bronze/silver/gold reliability language must wait for the future Rebalanced implementation.
- BSS Soul's Repose remained blank in the Effect row. Root cause in 0.1.7 code: the special-text path matched a guessed prayer-craft prefix instead of the already-verified `pray_for_souls_*` event family.
- Soul Contentment still read ambiguously because a leading effect icon plus an inline Soul Gratitude icon visually duplicated the noun. The desired grammar is `+10% [Soul Gratitude] from soul healing`.
- Thorough Cleansing still did not show the expected Sin Shard art. Direct balance evidence resolves the icon path: stock Sin Shard body-part crafting rows use `i_sin_shard`, while the `sin_shard` ItemDefinition has blank icon fields.
- Status: **superseded by 0.1.8; fixed-window composition retained, frame resizing rejected**.

## PrayerClarity 0.1.6 — runtime result: live tuning useful, frame/arrow design rejected

- Type: Clarity-only presentation/calibration candidate; no intended prayer-mechanics or balance changes.
- Frozen candidate ref: `candidate/0.1.6`.
- Exact source SHA: `adf752e1a04c93172bdedc54b913d4b926413c0d`.
- GitHub Actions run: `34891988740`.
- Workflow artifact ID: `10367390730` (`PrayerClarity-0.1.6-ci-adf752e1a04c93172bdedc54b913d4b926413c0d`).
- Handoff filename: `PrayerClarity-0.1.6-ci.dll`.
- Handoff DLL SHA-256: `cd741f905510db6dc968cdf87a0c2ca6f74e507bc9deb29b3bd801b2685e0a8d`.
- Runtime evidence, 2026-09-14: PrayerClarity 0.1.6 loaded successfully in Graveyard Keeper 1.407 at 2560x1440 alongside Test Harness 0.1.0 and the user's normal mod set. The supplied log contains no reported PrayerClarity forecast exception while the harness switched repeatedly across prayer families/qualities.
- Accepted evidence: live Configuration Manager changes apply while the pulpit is open, so runtime calibration is a better iteration tool than rebuild-per-coordinate guessing. The earlier cumulative prayer-switch drift did not return.
- Rejected frame behavior: `Window extra height` was not coherent. Increasing it could make the pulpit explode horizontally, and decorative/frame parts appeared to jump or move independently. This invalidates the 0.1.6 frame-resize implementation, not the general idea of live calibration.
- Rejected arrow grammar: one `(up)` before the whole success-bonus group reads as if it modifies the first resource. This is misleading on Donations, where Faith is printed first but money is the specialization. Ordinary and non-resource-specialist prayers also do not benefit from an up-arrow cue.
- Additional UX findings: Effect X needed a much wider negative range; the prayer selector and prayer button also need live X/Y controls; Soul's Repose had no special-effect explanation; Soul Contentment's `+10%` lacked an obvious noun; Thorough Cleansing did not present the intended Sin Shard icon.
- Context/dependency direction remains useful: removing `Контекст проповеди` reduced clutter, and icon-first source relationships remain preferable.
- Status: **superseded by 0.1.7; not accepted for stable**.

## PrayerClarity 0.1.5 — runtime result: stable geometry, layout rejected

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Purpose: replace the rejected 0.1.4 single expanding label with a layout derived from measured stock pulpit geometry, while retaining the accepted `guaranteed + success bonus + special effect` information model.
- Frozen candidate ref: `candidate/0.1.5`.
- Exact build source SHA: `91cc03b7e1902814d651565ffd591e81338ae4f9`.
- GitHub Actions run: `34888807911`.
- Workflow result: success on `ubuntu-latest`; `net472`, all 11 embedded-locale markers, artifact staging and upload passed.
- GitHub Actions artifact ID: `10366245690` (`PrayerClarity-0.1.5-ci-91cc03b7e1902814d651565ffd591e81338ae4f9`).
- Handoff filename: `PrayerClarity-0.1.5-ci.dll`.
- Handoff DLL SHA-256: `5f5684dac333683bbf6a547ee4f4d97b5df81b0d25faf4dad7555d3b668b58fb`.
- Geometry evidence: the stock `l_total_values` label is 242x68 at local `(-3,49)`, center pivot, 16 px font, `spacingY=-3`, `ShrinkContent`; the selected prayer item cell occupies the middle of the container and the craft button is at window-local `y=-85`.
- Layout change: context remained in the measured upper text region; a native-font cloned result label occupied the measured region below the prayer item cell; the craft button moved to `y=-100`; a smaller dependency note occupied the gap below that button and above the stock controller tips. No `ResizeHeight` was used for the main stock label.
- Runtime evidence, 2026-09-14: repeated Test Harness switching across Faith, Combo, Souls, Retribution, Donations, Repose, Thorough Cleansing, Gratitude, Roots, Imagination and other synthetic selections no longer produced the cumulative downward drift seen in 0.1.4. The supplied runtime log shows PrayerClarity 0.1.5 loaded and the switches completed without a reported PrayerClarity forecast failure.
- UX result: **the drift fix is accepted as evidence, but the layout is rejected**. The fixed blocks still overlap/crowd the stock prayer slot/button area, the frame remains too short for the desired information hierarchy, and `Контекст проповеди` adds little value.
- Additional UX findings from the runtime test: the dependency note should replace church/graveyard source nouns with their native icons; Thorough Cleansing needs a Sin Shard visual cue; and repeated rebuilds are an inefficient way to calibrate several local NGUI positions.
- Status: **superseded by 0.1.6 calibration candidate; not accepted for stable**.

## PrayerClarity 0.1.4 — runtime result: single expanding label rejected

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Purpose: replace the rejected 0.1.3 fixed-column/multi-widget layout with a single native-label vertical information block.
- Frozen candidate ref: `candidate/0.1.4`.
- Exact build source SHA: `cacdb1a544294c3e3601d2ee9022a137573963c7`.
- GitHub Actions run: `34883623571`.
- Workflow result: success on `ubuntu-latest`; `net472`, all 11 embedded-locale markers, artifact staging and upload passed.
- GitHub Actions artifact ID: `10363439062` (`PrayerClarity-0.1.4-ci-cacdb1a544294c3e3601d2ee9022a137573963c7`).
- Handoff filename: `PrayerClarity-0.1.4-ci.dll`.
- Handoff DLL SHA-256: `bb0d6dfe80de07303c24756d0e3b21c0d9b522e517de8f7750a48556cf94eb07`.
- Runtime UX evidence, 2026-09-14: the semantic content was useful, but the block overlapped the selected prayer slot/button and did not provide the intended paragraph/indent hierarchy.
- Runtime geometry probe 0.1.0 established the exact stock geometry and symbol table. During repeated Test Harness selection changes, the 0.1.4 label changed from the stock 68 px center-pivot label to 170/208 px `ResizeHeight`; its local Y then progressed `49 -> 35 -> 21 -> 7 -> -7 -> -21 -> -35` across redraws. This confirms the reported cumulative downward drift is produced by the 0.1.4 presentation path rather than by prayer mechanics.
- The same probe confirmed native inline symbols `(wskull)` (`icon_skull_wreath_green`), `(faith)` (`icon_faith_ol`), `(gld)/(slv)/(brz)` and `(gratitude_points)` (`techpoint_drop_smile`).
- UX result: **rejected**. The information model remains accepted; the single `ResizeHeight` label geometry does not.
- Status: **superseded by 0.1.5**.

## PrayerClarity 0.1.3 — runtime result: fixed-column layout rejected

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Purpose: validate the 0.1.2 information model with a less dense, fixed-column pulpit layout and stable font sizing.
- Frozen candidate ref: `candidate/0.1.3`.
- Exact source SHA: `dbbb6d26b2b87ee46819984e6ae50c60b44328e0`.
- GitHub Actions run: `34877867872`.
- Workflow artifact ID: `10360339790`.
- Handoff DLL SHA-256: `e24b93a6e70c69f6c440da08d7f2bdf91c9d6bafc2a979cd6307d0ae7092066c`.
- Runtime evidence, 2026-09-14: PrayerClarity 0.1.3 and Test Harness 0.1.0 loaded successfully; synthetic prayer selection continued to work and no PrayerClarity forecast exception was observed. The rendered multi-label layout visibly drifted outside the pulpit window.
- UX result: the fixed-column/multi-widget approach is **rejected**. It introduced brittle geometry without improving clarity enough to justify the extra UI hierarchy. The underlying `guaranteed + success bonus + special effect` information model remains accepted.
- Next narrow candidate: reuse the existing vanilla pulpit UILabel instead of creating a parallel UI hierarchy; present a spacious vertical `context -> result -> effect -> dependency note` block, add live graveyard quality, and use `ResizeHeight` rather than shrinking text.
- Status: **superseded for presentation; information model retained**.

## PrayerClarity 0.1.2 — runtime result: information model retained, single-label layout rejected

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Purpose: test the `guaranteed + success bonus` information model in the live pulpit after the 0.1.1 forecast seam was proved.
- Frozen candidate ref: `candidate/0.1.2`.
- Exact source SHA: `12da4d081aaf0dac11c8b2e528daf441af3e12ac`.
- GitHub Actions run: `34867338643`.
- Artifact ID: `10356689583`.
- Handoff DLL SHA-256: `bf02d1035664c798967ebcf06a376af7f8e877e3f11d1673fc92e0de5c12c167`.
- Runtime evidence, 2026-09-14: the user exercised ordinary, Combo, Donations, Retribution and Thorough Cleansing examples through the real pulpit/Test Harness. Forecast values rendered and updated correctly.
- UX result: the semantic split into guaranteed base output and success-only additions is useful and should be retained. The single-label layout is **not accepted**: resource positions move between rows, the context and forecast read as one dense block, and long special-effect text causes NGUI `ShrinkContent` to reduce the font size of the entire label.
- Next narrow candidate: separate context from results, align repeated resources into fixed columns, render the special effect in its own row, preserve normal font size, and test native cached icons where verified.
- Status: **superseded for presentation; information model retained**.

## PrayerClarity 0.1.1 — runtime result: forecast path verified, presentation not accepted

- Type: narrow follow-up production-architecture candidate; **Clarity only**, with no intended prayer-mechanics or balance changes.
- Purpose: prove that the 0.1.0 pulpit forecast architecture works once the reflection resolver correctly closes the game's generic `GameBalance.GetData*<T>(string)` seam.
- Source branch: `dev/clarity-pulpit-v0.1`.
- Frozen candidate ref: `candidate/0.1.1`.
- Exact source SHA: `bfa437a729cedebdad8c787fd663edfa5ed51630`.
- GitHub Actions run: `34864837824`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10355544046` (`PrayerClarity-0.1.1-ci-bfa437a729cedebdad8c787fd663edfa5ed51630`).
- Handoff filename: `PrayerClarity-0.1.1-ci.dll`.
- Handoff DLL SHA-256: `f86dc5710b79c4f7db081f97a634f59ab25831ba964d856e8999d67006228987`.
- Change from 0.1.0: `R.BalanceData` resolves a one-string-parameter balance getter against the expected result type; it uses a compatible closed overload when present, otherwise binds the verified one-generic-argument method definition with `MakeGenericMethod(expectedType)` before invocation. Callers explicitly request `PrayEventDefinition` or `BuffDefinition`.
- Runtime evidence, 2026-09-14: forecast output now renders successfully in the live 1.407 pulpit UI and updates while the Test Harness switches prayer families and qualities. The supplied runtime log contains no recurring `PrayerClarity forecast failed` error for 0.1.1 and confirms synthetic selection across ordinary, Faith, Donations, Repose, Excellence and Roots examples without save/inventory mutation.
- UX result: current append-only presentation is **not accepted**. Vanilla `Church quality / Sermon requires / Success chance` remains useful, but `Success | Failure` on one line is visually poor and separates probability from its corresponding outcome.
- Status: **forecast/runtime seam verified; superseded for UX iteration, not accepted for stable**.

## PrayerClarity 0.1.0 — runtime result: superseded

- Type: first production-architecture prototype; **Clarity only**, with no intended prayer-mechanics or balance changes.
- Purpose: validate the minimal pulpit decision-time surface before implementing broader tooltip/HUD layers or any Vanilla Fix / Balance-Rework behavior.
- Source branch: `dev/clarity-pulpit-v0.1`.
- Frozen candidate ref: `candidate/0.1.0`.
- Exact source SHA: `6321da4ca868998c166a115d77d2d634e372ffde`.
- GitHub Actions run: `34860993438`.
- Workflow result: success on `ubuntu-latest`; `net472`, all 11 embedded-locale markers, artifact staging and upload passed.
- GitHub Actions artifact ID: `10353769580` (`PrayerClarity-0.1.0-ci-6321da4ca868998c166a115d77d2d634e372ffde`).
- Handoff filename: `PrayerClarity-0.1.0-ci.dll`.
- Handoff DLL SHA-256: `dc47501e5a9d444309954f4b2a58c230f2bc1c8a725c1163eb43944576b6c949`.
- Supported target identity: Graveyard Keeper 1.407 Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`; other MVIDs fail closed before patching.
- Localization contract: English, French, German, Simplified Chinese, Spanish (Spain), Portuguese (Brazil), Korean, Japanese, Russian, Italian and Polish are embedded in the DLL; current game language is resolved on the relevant UI redraw with English fallback.
- Runtime architecture: one postfix on `PrayCraftGUI.RedrawTextValues(float,float)`; no polling, no Unity-wide scans and no call to `PrayLogics.CalculatePray` for preview. Base Faith/money are intended to be read through the game's own `SmartExpression.EvaluateFloat`; known special effects are read from verified game definitions/semantics.
- Fail-safe behavior: a forecast error is logged once and vanilla pulpit UI remains usable.
- Runtime evidence, 2026-09-14: the test harness successfully switched synthetic prayer families/qualities in the live pulpit UI without granting them to the save. The visible `Church quality / Sermon requires / Success chance` block was confirmed to be vanilla UI. PrayerClarity's own forecast did **not** render because `R.BalanceData` selected an open generic `GameBalance.GetData*` overload and late-bound invocation failed with `ContainsGenericParameters=true`. The plugin failed safe and left the vanilla pulpit usable; no prayer-mechanics change was observed.
- Status: **superseded / not accepted**.

## PrayerClarity Audit Probe 0.1.0 — pending runtime evidence

- Type: research-only, read-only diagnostic probe; **not** production mod code.
- Purpose: close static-evidence gaps around the final `PrayResult` consumer, prayer buff application/duration, passive buff consumers, and exact prayer-selection/report presentation.
- Source branch: `research/initial-audit`
- Exact source SHA: `d0d42c5011e4f909d9dddeda876c0bed7e9cfab2`
- GitHub Actions run: `34824714263`
- Build result: success on `ubuntu-latest`; `net472`, AnyCPU.
- Handoff filename: `PrayerClarity.AuditProbe.0.1.0.dll`
- Handoff DLL SHA-256: `3f4a5758541b22b9ccfa695142e27eadf58fdb2489491eb6faca1dbeb1d0bc49`
- Runtime contract: reflection / IL inspection only; no Harmony patches and no intended save/player/world mutation.
- Expected evidence file: `BepInEx/PrayerClarity-audit-0.1.0.txt`.
- Required user action: install the probe DLL in `BepInEx/plugins`, launch Graveyard Keeper 1.407 far enough for BepInEx/game assemblies to load, then return the generated text file. No sermon execution or save mutation is required.
- Status: **pending user runtime capture**.

Do not treat this probe as a release candidate or production architecture. Once its narrow questions are answered, durable derived facts belong in `docs/PRAYER_MECHANICS.md` and the probe should not become a permanent runtime dependency.

## PrayerClarity: Rebalanced 0.2.0 — first executable candidate

- Type: executable candidate for the accepted 2026-09-18 fairness/rework specification.
- Frozen candidate ref: `candidate/rebalanced-0.2.0`.
- Exact build source SHA: `26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`.
- GitHub Actions run: `35346055221`.
- Workflow result: **success** on `ubuntu-latest`; localization JSON validation, restore, Rebalanced build, Vanilla sibling build, locale-marker verification, staging and artifact upload all passed.
- Workflow artifact ID: `10547166000`.
- Artifact: `PrayerClarity-shared-ui-1.0.24-rebalanced-0.2.0-ci-26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`.
- Artifact ZIP digest: `sha256:54a49d9a460d255a594b788089cde9b26433f38eb0f1214d6a3a5efa94c5051f`.
- Handoff filename: `PrayerClarity.Rebalanced-0.2.0-ci.dll`.
- Handoff DLL SHA-256: `84cf07be553e137d4663d24267ab18257facfbe44c0a833d87579876742b6de1`.
- Supported target: Graveyard Keeper 1.407, Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.
- Mechanics scope: accepted 0.2.0 resource-family balance, revised CQ ladders, BSS Soul's Repose q30/60/120, fixed specialist outputs through stock success-only prayer output items, and best-existing-tier Repose narrowing.
- Shared presentation scope: current Soul Gratitude pulpit metric for Soul's Repose; contextual Repose endpoint replacement derived from current player/body data; all new strings present in 11 official locales.
- Performance shape: no per-frame work; corpse tier catalogue is inspected only on relevant Donkey delivery or pulpit redraw; stock `GameSave.GenerateBody` remains authoritative.
- Runtime status: **not yet accepted**. Focused in-game acceptance is required; do not promote to `main` yet.


## PrayerClarity: Vanilla 1.0.25 — shared clarity candidate

- Type: shared Clarity/pulpit follow-up for the Vanilla sibling; **no prayer mechanics or balance changes**.
- Frozen candidate ref: `candidate/vanilla-1.0.25`.
- Exact source SHA: `ebe069b4ad202ae786af9c63ded0ffb00502cff7`.
- GitHub Actions run: `35347730945`.
- Workflow result: **success** on `ubuntu-latest`; 11 localization JSON files validated, restore/build passed, embedded-locale markers passed, candidate/Nexus staging passed.
- Workflow artifact ID: `10547383753`.
- Artifact: `PrayerClarity-1.0.25-ci-ebe069b4ad202ae786af9c63ded0ffb00502cff7`.
- Artifact ZIP digest: `sha256:d7fb3fe9bd789a6568a0204d033c044c2f17c028b8b6125b15858317cb6a0f92`.
- Handoff filename: `PrayerClarity-1.0.25-ci.dll`.
- Handoff DLL SHA-256: `72b2237607734d8b50d666cef56e18d458b88c6a456211d496e2410ac646e3a9`.
- Supported target: Graveyard Keeper 1.407, Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.
- Scope:
  - Soul's Repose pulpit context displays current Soul Gratitude as an additional input metric.
  - Repose pulpit can replace the stock effect line with the localized endpoint message when the stock +1 maximum adds no higher ordinary corpse tier.
  - shared corpse-tier catalogue/state lookup is presentation-only in Vanilla.
  - all stock prayer requirements, outputs, formulas, buffs and corpse-generation mechanics remain unchanged.
- Test Harness protocol: use the normal Vanilla Test Harness only. **Do not load PrayerClarity.TestHarness.RebalancedCompat** with Vanilla; that adapter exists only for the Rebalanced sibling and uses the legacy Vanilla GUID alias.
- Runtime acceptance:
  1. select Soul's Repose and confirm the additional Soul Gratitude row fits naturally;
  2. select Repose and confirm the ordinary effect wording remains while a higher ordinary tier is available;
  3. on a save at terminal Donkey progression, confirm the effect line becomes `Ещё более качественные тела недоступны.`;
  4. confirm no stock mechanics/balance values changed.
- Status: **ready for focused runtime verification; not accepted**.


### Runtime check 2026-09-18 — Rebalanced 0.2.0 first pass

User-tested candidate:
- Rebalanced source: `26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`
- DLL SHA-256: `84cf07be553e137d4663d24267ab18257facfbe44c0a833d87579876742b6de1`
- Test Harness 0.1.2 + Rebalanced Compatibility 0.1.2 + existing bridge set.

Accepted observations from this pass:
- Rebalanced 0.2.0 and Test Harness compatibility loaded successfully together.
- Faith / Donations / Combo preview semantics and revised requirements appeared as designed.
- Soul's Repose pulpit displayed the additional current Soul Gratitude metric without requiring manual layout tuning.
- Soul's Repose Bronze and Gold screenshots matched the candidate ladder: q30 Bronze / q120 Gold and +50% / +150% success contribution.
- RU/EN/DE/JA screenshots rendered the added context; JA is visually denser but no blocking clipping/overflow was reported.
- User exercised multiple timed prayer previews/activations and reported matching descriptions across pulpit, Technology and active-effect/character surfaces.
- Harness compatibility successfully projected Rebalanced tier state for real synthetic native buffs (observed Gold Excellence 54 min, Gold Roots 108 min, Gold Combat 108 min).
- No PrayerClarity-specific exception/error was observed in the supplied runtime log.

Not verified in this pass:
- terminal Repose endpoint presentation/behavior: current save has not yet reached terminal corpse progression;
- actual normal-sermon reward payout for the new fixed-only Faith/Donations and percentage-only Combo family: Test Harness synthetic preview bypasses normal sermon rewards by design;
- exhaustive visual acceptance of every locale; multi-language switching was exercised, with screenshots supplied for representative locales;
- exact timed-effect duration audit was not manually repeated because durations are unchanged by the candidate.

Status:
- **presentation/semantic/timed-buff first pass accepted**;
- candidate remains pending the narrow normal-sermon reward check and terminal Repose check (the latter can wait until a suitable save/progression state exists).


### Runtime check 2026-09-18 — Vanilla 1.0.25 shared-Clarity pass

User-tested candidate:
- Vanilla source: `ebe069b4ad202ae786af9c63ded0ffb00502cff7`
- DLL SHA-256: `72b2237607734d8b50d666cef56e18d458b88c6a456211d496e2410ac646e3a9`
- Test Harness 0.1.2 + existing bridge set; Rebalanced Compatibility correctly absent.

Accepted observations:
- PrayerClarity 1.0.25 loaded successfully as the clarity-only sibling.
- Soul's Repose displayed the added current Soul Gratitude row at the pulpit.
- Vanilla Soul's Repose retained stock q60 Gold and stock sermon contributions; the shared UI addition did not alter mechanics.
- Vanilla Repose Gold retained its stock q50/effect presentation on the tested non-terminal save, so the endpoint replacement did not trigger prematurely.
- No PrayerClarity-specific exception/error was observed in the supplied runtime log.

Not verified:
- terminal Repose endpoint wording, because no available save has terminal Donkey corpse progression.

Status:
- **Vanilla 1.0.25 shared-Clarity runtime pass accepted except terminal Repose endpoint**, which can wait for a suitable save.


## PrayerClarity: Rebalanced 0.2.1 — Roots runtime fix candidate

- Type: focused bug-fix candidate over accepted Rebalanced 0.2.0.
- Candidate ref: `candidate/rebalanced-0.2.1`.
- Triggering bug: Rebalanced 0.2.0 rewrote plant `craft_time` with `Ppar("buff_plant")*Ppar("prayerclarity_rebalanced_plant_reduction")`; live Graveyard Keeper 1.407 evaluation throws `InvalidCastException` inside Expressive and `SmartExpression.EvaluateFloat` falls back to `1f`, causing auto-growth to finish almost immediately.
- Vanilla control: PrayerClarity: Vanilla 1.0.25 leaves plant mechanics untouched and the same carrot/cabbage scenario shows no SmartExpression/Expressive exception.
- Fix architecture: keep the verified stock `craft_time` expression unchanged. On affected plant `CraftComponent.DoAction` calls only, temporarily project the active Rebalanced Roots reduction into the stock WGO-owned **NonSerialized `totem_effect`** `buff_plant` entry as `reduction / 0.20` (1 / 1.5 / 2 for Bronze/Silver/Gold), then restore the exact original runtime-effect value in a Harmony finalizer.
- Semantics: preserves the game's additive `grow_time + buff_plant` formula rather than multiplying elapsed time externally.
- Save safety: the injected value lives only in the native NonSerialized `totem_effect` aggregate for the duration of `DoAction`, and is restored even when the original call throws; serialized plant Item data is never modified.
- Performance shape: no per-frame polling or scans. One narrow `CraftComponent.DoAction` hook exits immediately for non-Roots plant crafts; affected auto-growth already executes through this native path at the game's own cadence.
- Exact candidate source SHA: `d13655e01a1b8e797ed019b636f040b3d2f2a55f`.
- GitHub Actions run: `35377381232` — success.
- Workflow artifact ID: `10560138983` (`PrayerClarity-shared-ui-1.0.25-rebalanced-0.2.1-ci-d13655e01a1b8e797ed019b636f040b3d2f2a55f`).
- Handoff filename: `PrayerClarity.Rebalanced-0.2.1-ci.dll`.
- Handoff DLL SHA-256: `adb0bfc90c1245cb652662a826a168f6f4410a696a352e9360abfe8507ab20c7`.
- Build result: success on `ubuntu-latest`; Rebalanced and Vanilla compiled, all 11 locale sets validated/staged, and the shared artifact uploaded.
- Required runtime gate after the clean build:
  1. ordinary freshly planted carrot/cabbage no longer completes in seconds and produces no SmartExpression/Expressive error;
  2. with active Roots, Bronze/Silver/Gold retains the intended -20/-30/-40 percentage-point term in the native additive growth formula;
  3. repeat one case with `grow_time` fertilizer to verify the additive interaction remains intact;
  4. return the runtime log so the absence of the 0.2.0 exception can be confirmed.
- Broader Rebalanced behavior-risk audit is intentionally deferred until this blocker is closed; it remains a required follow-up requested by the user.


### Runtime check 2026-09-18 — Rebalanced 0.2.1 Roots fix, no-buff half of gate

User-tested candidate:
- Rebalanced source: `d13655e01a1b8e797ed019b636f040b3d2f2a55f`
- Rebalanced 0.2.1 loaded successfully and static projection applied.
- Fresh carrot/cabbage growth crafts started normally after planting; the newly planted crops did not transition to `*_ready` during the remainder of the supplied log.
- The supplied runtime log contains zero `ExpressiveException`, zero `InvalidCastException`, zero `SmartExpression` error, and zero `Error in expression` entries.
- User visual observation agrees: freshly planted carrots no longer become ready almost immediately.
- This closes the original 0.2.0 failure mode for ordinary/no-Roots growth.
- Remaining acceptance gate: prove active Rebalanced Roots actually shortens native crop `craft_time` by the intended Bronze/Silver/Gold amount, including one fertilizer-adjusted case.

A research Test Console 0.1.1 was prepared to remove stopwatch/manual timing from that gate:
- candidate ref: `candidate/rebalanced-test-console-0.1.1`
- source SHA: `187579dcc329c05c845aec80849772cb7fbd67bb`
- GitHub Actions run: `35379100576` — success
- artifact ID: `10561017600`
- handoff DLL SHA-256: `4de6e4cee802c88757439ba9cb6ccdb83617601158588cfcd90e024047cad707`
- Roots diagnostic logs, once per affected craft after activation, both `craft_time_without_roots` and `craft_time_with_roots`, plus tier, configured reduction, effective WGO `buff_plant`, fertilizer `grow_time`, and raw native expression.
- Diagnostic comparison is performed after the native `DoAction` call while RebalancedRoots' temporary nonserialized WGO projection is still in scope; it temporarily subtracts only the projected Roots runtime contribution for the read-only comparison evaluation, restores it immediately, and leaves the production finalizer to restore the original WGO state.


### Runtime check 2026-09-18 — active Roots + fertilizer + removal

User-tested Rebalanced 0.2.1 with Rebalanced Test Console 0.1.1.

Observed runtime evidence:
- Bronze synthetic Roots activation used native `BuffsLogics.AddBuff`; diagnostic on `tree_growing`: baseline `1800`, with Roots `1440`, saved `360` = exactly 20%.
- Silver activation used the same native buff path; `tree_growing`: `1800 -> 1260` = exactly 30%.
- Silver on ordinary carrot/cabbage: `1440 -> 1008` = exactly 30% of base growth time.
- Silver plus one time-fertilizer unit on wheat: native formula `1440*(1-0.2*grow_time-0.2*buff_plant)`; `grow_time=1`; no-Roots fertilizer baseline `1152`; Roots result `720`. This proves the intended additive stacking: fertilizer contributes -20 percentage points of base time and Silver Roots contributes another -30 points, for total -50% of base time.
- No `ExpressiveException`, `InvalidCastException`, `SmartExpression` error, or `Error in expression` occurred.
- User removed Roots through the console; runtime logged native `BuffsLogics.RemoveBuff("buff_plant")`.
- The console removal is a valid simulation of natural expiry because stock `BuffsLogics.RecalculateBuffs` removes expired buffs through that same `RemoveBuff` path.
- Production semantics after removal: already accumulated crop progress is retained; subsequent `CraftComponent.DoAction` calls no longer receive the temporary Roots WGO projection because the prefix requires active player `buff_plant`. Therefore an in-progress plant continues from its current progress at its ordinary/fertilizer-adjusted rate rather than rewinding or finishing instantly.

Assessment:
- The 0.2.0 near-instant-growth regression is fixed.
- The 0.2.1 native scope bridge is runtime-confirmed for active Roots.
- Additive interaction with fertilizer is runtime-confirmed.
- Manual removal correctly exercises the same stock removal path as timed expiry.
- Gold is not separately runtime-sampled in this log, but it has no distinct control-flow branch: the same verified bridge uses the persisted tier reduction scalar (.20/.30/.40). Bronze and Silver runtime samples plus definition validation cover the mechanism; no additional Gold-specific runtime test is required unless behavior changes.


### 2026-09-18 — Rebalanced 0.2.2 Roots edge accepted

- Accepted runtime source: `924900365d44cd1ec9e530c9dd9b7e2f6a796bed`.
- Accepted ref: `accepted/rebalanced-0.2.2`.
- Production promotion: PR #5 merged to `main` as `1c4c395d375209e1505a312c922f4f176d78b0da`.
- CI run: `35382737488` — success, 0 warnings / 0 errors.
- Rebalanced DLL SHA-256: `4655fea2a57125aa78965a807fde76f9a056dbbd7f361246cf12351ff45074d6`.
- Runtime helper: Rebalanced Test Console 0.1.2, source `f50fff1d7dc314f7f27ac125d1e760346a6ce6fe` (research-only; not promoted to production).

Accepted edge-case runtime evidence:
- Test Console enabled its nonpersistent Boost-II simulation, which supplies `grow_time=3` only to verified plant expressions that consume both `grow_time` and `buff_plant`.
- Gold Shoots & Roots was activated through native `BuffsLogics.AddBuff`.
- `tree_growing`, whose stock expression has no `grow_time` term, remained ordinary Gold Roots: `1800 -> 1080` (-40%); the Boost-II simulation did not leak into that consumer.
- `garden_wheat_growing` reported `grow_time=3`, effective WGO `buff_plant=1.75`, fertilizer-only baseline `576`, and capped Gold result `72`.
- For the 1440-second stock crop base, 72 seconds is exactly 5% remaining time = 95% combined reduction.
- The effective prayer contribution at the cap is 35 percentage points (1.75 stock prayer units), so 60% fertilizer + 35% applied Gold = 95%; Silver + the same fertilizer remains 90%, preserving a real Silver -> Gold upgrade.
- No `ExpressiveException`, `InvalidCastException`, `SmartExpression`, or `Error in expression` failure occurred in the supplied test log.

Result: the original 0.2.0 instant-growth regression and the later 100%-stack edge are both closed. Rebalanced 0.2.2 is the accepted stable baseline for Roots.


### 2026-09-18 — Rebalanced 0.2.2 publication correction

A GitHub Release/tag for `rebalanced-v0.2.2` was created prematurely during internal stabilization and was later deleted before the public 0.2.3 release. The accepted 0.2.2 source/ref remains valid historical runtime evidence for the Roots repair, but `rebalanced-v0.2.2` is **not** a current published release.


### 2026-09-18 — Rebalanced 0.2.3 accepted and published

- User acceptance: explicit `фиксируем 023` after the focused native-seam runtime pass.
- Accepted ref: `accepted/rebalanced-0.2.3`.
- Exact accepted runtime source: `ab1eb67cbf2465a912c392120395011503b720c3`.
- Candidate CI run: `35388483840` — success.
- Artifact ID: `10564359079`.
- Artifact: `PrayerClarity-shared-ui-1.0.25-rebalanced-0.2.3-ci-ab1eb67cbf2465a912c392120395011503b720c3`.
- Artifact ZIP digest: `sha256:097febe34877eb16cb1ac79df56301f24e96deb908c60eb66830d8443bdf4a06`.
- Accepted/released Rebalanced DLL SHA-256: `03a4a8b43c8a5ffef8ec62eac58370d3f6bced347bc2fb4f49a2ef81a23cb1ca`.
- Stable promotion: PR #7 merged to `main` as `f953e34901de139a5f7af7162af6fc62bb02b4d9`.
- Stable tag/release: `rebalanced-v0.2.3`, targeting the exact accepted runtime source.
- Publication workflow run: `35391896166` — success.
- Publication reused and hash-verified the exact accepted CI artifact; **no rebuild occurred**.
- Published asset: `PrayerClarity.Rebalanced.dll`.

Accepted runtime evidence with Test Console 0.1.3:
- Repentance: Bronze/Silver/Gold effective confession probability = 0.50 / 0.75 / 1.00 while the controlled stored stock value remained 0.15 before each accessor read.
- Combat damage: Bronze/Silver/Gold tier delta relative to the native Bronze result = 0 / +5 / +10; persisted stock `add_damage` remained 5.
- Combat armor: controlled 20 damage caused 20 stock HP loss vs 16 with Combat, exactly 4 damage prevented.
- Combat regeneration: Bronze/Silver/Gold native buff ticks were observed as +1 / +2 / +4 HP, with stock max-HP clamping.
- No PrayerClarity runtime failure, `Exception`, `ExpressiveException`, `InvalidCastException`, `SmartExpression`, or `Error in expression` occurred in the supplied log.
- Synthetic Combat buff was removed at the end of the test.

Architecture accepted in 0.2.3:
- Repentance leaves stock `SetPpar("confession_probability", 0.15)` authoritative and projects only the effective FlowCanvas player-param read while `buff_sins` is live.
- Combat damage projects only the Silver/Gold delta through nonserialized `totem_effect["add_damage"]` around native `GetDamage`.
- Combat armor projects +4 through nonserialized `totem_effect["add_armor"]` around native `DecHP`, eliminating the prior ThreadStatic + generic `GetParam` interception.
- Combat regeneration remains on the native `BuffDefinition.se_tick` extension point.
- Roots retains the accepted 95% aggregate growth-time-reduction safety cap from 0.2.2.

Research helper identity:
- Test Console 0.1.3 source: `5de0f27e97cb38a72fb2b53b93e02131225012e7`.
- Test Console DLL SHA-256: `f7624c8696f5dbbf1bf64a959ea370c545c198332bbfe0c1b8eeec4e0775d6fc`.
- The helper remains research-only and is not part of the public release.

### 2026-09-19 — Rebalanced 0.2.4 accepted and published

- User acceptance: after testing the 0.2.4 candidate, the user reported Repentance and Repose duration presentation as **2.7 / 3.7 / 4.8 in-game days** under Longer Days +50% and explicitly approved fixing, publishing, and documentation consolidation.
- Accepted behavior: Repentance and Repose duration **30 / 42 / 54 min**. Repentance probability remains 50 / 75 / 100%; Repose Bronze/Silver/Gold corpse-quality logic is unchanged.
- Implementation: tiered duration is projected to stock `CraftDefinition.dur_parameter` through the existing once-per-load static projection. No new Harmony hook, polling, persistent mod timer state, or runtime algorithm was introduced.
- Accepted ref: `accepted/rebalanced-0.2.4`.
- Exact accepted runtime source: `4d5d3021c0b9eef16d09402c5f25851ff7a66981`.
- Candidate CI run: `35441674008` — success, 0 warnings / 0 errors.
- Artifact ID: `10583657918`.
- Artifact: `PrayerClarity-shared-ui-1.0.25-rebalanced-0.2.4-ci-4d5d3021c0b9eef16d09402c5f25851ff7a66981`.
- Artifact ZIP digest: `sha256:1e8759dee5652d5e9d8f0180462a8875e569174d375ec70dc7b85a6591f865a5`.
- Accepted/released Rebalanced DLL SHA-256: `ecba7297a80bde91a044d4d7c7e4348fd32805e960e3fa467be8d1465cfa52e2`.
- Stable promotion: PR #8 merged to `main` as `d176fec0eb42852019956a55347dd3af29b3fe1f`.
- Stable tag/release: `rebalanced-v0.2.4`, targeting the exact accepted runtime source.
- Release asset ID: `574716424`, canonical filename `PrayerClarity.Rebalanced.dll`, asset digest `sha256:ecba7297a80bde91a044d4d7c7e4348fd32805e960e3fa467be8d1465cfa52e2`.
- Publication workflow run: `35442370524` — success.
- Publication downloaded artifact `10583657918`, verified the accepted source/version/hash, and uploaded the exact accepted DLL; **no rebuild occurred**.
### 2026-09-26 — Rebalanced 0.2.31 rejected; process gate hardened

- Runtime under Graveyard Keeper 1.407 / Russian locale confirmed the intended 0.2.31 build was loaded; subsequent isolation used Rebalanced Test Console 0.1.11. This was not a stale-DLL/install mix-up.
- Candidate 0.2.31 is **rejected** and must not be promoted or used blindly as the next production baseline.
- Confirmed production presentation regressions: Soul's Repose item wording exposes a literal `\\n` and the experimental `1 icon = 1 icon` rewrite is rejected in favor of restoring the previously understandable wording; Shoots & Roots still shows the 95% all-sources cap on Bronze/Silver because the wrong presentation writer was edited; Bronze Repose still overlaps the pulpit action button because the current layout has a fixed extra-height budget rather than content-driven root-window growth. The earlier Combo Prayer double-glyph observation was not a production regression.
- Root process finding: the 0.2.30/0.2.31 follow-up treated several apparently small presentation issues as obvious and bypassed/reduced the normal per-change owner/final-writer evidence gate. This violated existing DevRules rather than exposing a missing prayer-specific mechanics rule.
- Global DevRules and PrayerClarity local policy were hardened: every materially independent production change now requires a reviewable READY/BLOCKED gate before the first production-source mutation, with no small/obvious/presentation-only/follow-up exception; candidate CI requires a gate-only record before production edits relative to the declared baseline.
- PrayerClarity candidate localization CI now validates the complete 11-locale key set, placeholder parity, and rejects literal escaped control sequences such as the observed `\\n` failure.
- Combo Prayer double-glyph investigation is **closed**. Test Console 0.1.10 started its experimental quality-title mutation probe enabled and added a quality glyph on the same tooltip path as production. Test Console 0.1.11 starts that probe disabled; with 0.1.11 the user observed exactly one glyph. The read-only trace also recorded the native pre-production title as plain `Комбо-молитва` for Bronze/Silver/Gold. Therefore production is the intended single quality-glyph writer; the second glyph belonged to the research helper. No production fix is required for the observed duplicate-glyph symptom. The speculative 0.2.31 defensive title check is unnecessary and should not be carried forward merely because of this research-artifact symptom.
- No prayer mechanics retest is required. Next production work must choose its baseline deliberately and carry forward only accepted/proved presentation changes plus separately READY fixes.
### 2026-09-26 — Test Console 0.1.12 invalidated; research-method rule hardened

- Test Console 0.1.12 attempted to answer the Soul's Repose item-tooltip wrapping question while keeping rejected Rebalanced 0.2.31 installed.
- This was a bad evidence-method choice: 0.2.31 had already changed the wording and embedded the literal `\\n`, so the probe first had to reconstruct the older 0.2.30 presentation before it could test the intended sentence-boundary newline. The user's screenshot showed that 0.1.12 did not restore that baseline and therefore did **not** test the intended property.
- 0.1.12 evidence is **invalid for acceptance** and must not be used to close the Soul's Repose wrapping gate.
- Simpler available path identified after review: use the existing exact 0.2.30 presentation baseline and add only the one new layout experiment when possible, rather than adding research machinery solely to avoid a DLL swap/restart.
- DevRules now requires a research-method checkpoint before new probe/harness/shim code: exact question, existing evidence/artifact/direct-action path, and justification for new research code. The optimization target is lowest combined evidence complexity/error risk, not fewest user actions.
- Test Console 0.1.13 corrects the immediate probe by restoring the accepted Russian 0.2.30 wording and changing only the sentence separator, but it remains research-only and unaccepted until runtime observation.
### 2026-09-26 — Test Console 0.1.13: sentence break insufficient; universal resource-cluster gate opened

- Runtime loaded PrayerClarity: Rebalanced 0.2.31 with Test Console 0.1.13 and the probe restored the accepted Russian Soul's Repose wording with a real newline between the two sentences.
- The sentence break kept the final `90 (gratitude_points)` cluster together, but the same screenshot showed `1` at the end of one line and the `(faith)` icon alone on the next line.
- Therefore the 0.1.13 sentence-break variant is **not accepted as the production fix**. The open property is broader: PrayerClarity-owned prayer-item mechanics rows must not orphan inline resource icons from their immediately associated numeric amounts.
- U+00A0 and prayer-specific hard line breaks are not sufficient general solutions for this runtime symbol/wrap behavior.
- Production gate state for the universal amount+inline-resource wrapping behavior is **BLOCKED** pending final NGUI wrap ownership evidence.
- Research-method checkpoint: use exact Rebalanced 0.2.30 as the known baseline and a read-only final-wrap trace; do not reconstruct 0.2.30 wording inside the Test Console merely to avoid a DLL swap.
### 2026-09-26 — Test Console 0.1.14 closes final prayer-item wrap owner trace

- Runtime used exact PrayerClarity: Rebalanced 0.2.30 with Test Console 0.1.14 under Graveyard Keeper 1.407 / Russian locale.
- The read-only trace observed the PrayerClarity-owned 200-unit Soul's Repose mechanics row after the normal content-width pass.
- Raw row retained intact semantic clusters: `... даёт 1 (faith). ... до 90 (gratitude_points).`
- Final NGUI `processedText` rewrote those points as `... даёт 1\n(faith). ... до 90\n(gratitude_points).`
- Therefore the orphaned inline icons are produced by final NGUI wrapping after PrayerClarity supplies the row text and fixed item-mechanics width; localization and the semantic renderer are not the final split owner.
- The common production-fix gate remains **BLOCKED only on the correction mechanism**, not on owner/final-writer uncertainty.
- Research-only Test Console 0.1.15 is the next narrow hypothesis: after observing final `processedText`, move an automatically inserted break from `amount | icon` to immediately before the amount, preserving wording and width. Runtime visual acceptance is required before production.
### 2026-09-26 — Test Console 0.1.15 validates common amount+inline-icon repair

- Runtime used exact PrayerClarity: Rebalanced 0.2.30 with Test Console 0.1.15.
- Russian Soul's Repose: the probe observed final NGUI splits at both `1 | (faith)` and `90 | (gratitude_points)`, then moved each wrap before the numeric amount. User visual inspection confirmed both amount/icon clusters stayed together and the tooltip remained readable.
- Cross-locale runtime evidence supports the same conditional rule rather than a Russian-specific wording fix: Italian and German exposed the `1 | (faith)` split and were repaired; Polish exposed the `90 | (gratitude_points)` split and was repaired; French, Japanese and Simplified Chinese did not need that repair at the observed lines and remained on their normal NGUI wrapping.
- The accepted research rule is therefore conditional: only PrayerClarity-owned prayer-item mechanics rows whose final NGUI `processedText` actually splits an integer amount from the immediately following inline symbol should be normalized, by moving that line boundary before the amount.
- Wording, localization, tooltip width, mechanics and other surfaces are preserved.
- Production gate for the common prayer-item amount+inline-icon wrapping behavior is **READY**.
- Rebalanced 0.2.32 / Vanilla 1.0.41 were created from the exact 0.2.30 / 1.0.39 candidate baseline, intentionally excluding rejected 0.2.31 wording/title changes. Candidate source: `768bb9929823a3b3fd2496fdfd71de0587ddeb27`; CI run `36199112978` succeeded. Runtime acceptance is still pending.


### 2026-09-26 — Rebalanced 0.2.32 production amount+icon wrapping accepted

- Runtime used exact PrayerClarity: Rebalanced 0.2.32 source `768bb9929823a3b3fd2496fdfd71de0587ddeb27` under Graveyard Keeper 1.407 with the research Test Console removed, so no probe could supply or mask the production wrapping behavior.
- Russian Gold Soul's Repose visually confirmed both previously failing semantic clusters now remain intact: `1 + Faith icon` and `90 + Soul Gratitude icon`. The readable 0.2.30 wording is preserved.
- Japanese spot-check showed no visible regression; the conditional repair stayed out of the way where the locale's normal final wrapping was already readable.
- The production acceptance criterion for the common prayer-item amount+inline-resource wrapping change is therefore satisfied.
- This accepts the 0.2.32 behavior change as tested evidence. It does **not** by itself promote either edition to the stable line; stable promotion remains a separate user decision.
- Test-process observation from this run: removing the research Test Console to obtain a clean production path also removes useful neutral setup conveniences such as spawning prayer items. Treat possible separation of neutral test utilities from behavior-mutating probes as a separate research-tooling design question, not as part of the accepted production change.


### 2026-09-26 — Neutral Test Console 0.1.16 and Rebalanced 0.2.33 pulpit-height candidate

- Permanent tooling rule was added to `AGENTS.md`: the long-lived Rebalanced Test Console is a neutral setup/access utility only. Behavior-mutating or path-intercepting probes must be separate temporary DLLs/branches.
- Neutral Test Console **0.1.16**:
  - branch: `candidate/rebalanced-test-console-0.1.16`;
  - exact source: `41d09d81e571fb1f2886b6c48ec49c37906b8136`;
  - CI run: `36200428185` — success;
  - artifact ID: `10891079949`;
  - artifact ZIP digest: `sha256:894fd42f5c1411ce705b23c43606b0af51781f5366c7029ec3ffc685d636e2fd`;
  - DLL SHA-256: `eea36adf4c4737c35847bc0756ed3fcd97a972dc9358bf7e2349ca635a7ce1c7`;
  - source contains no Harmony patch/prefix/postfix installation. Retained utilities are prayer-item gallery/cleanup, temporary inventory expansion/restoration and native pulpit opening.
- Rebalanced **0.2.33** / Vanilla **1.0.42** were created from exact accepted 0.2.32 source `768bb9929823a3b3fd2496fdfd71de0587ddeb27`.
- Gate-only commit: `6d26bb8c78824cf15ac3cf9f73a71ef48ec9c625`; gate `pulpit-content-driven-bottom-clearance` = **READY**.
- Candidate exact source: `82f6a8501658feed989da3a49861c72670f44085`.
- CI run: `36200797591` — success; production candidate gate validation passed.
- Artifact ID: `10891778094`; artifact ZIP digest: `sha256:0caec4f241fa83c7db2884395b392b6df3bc4d4fb46355f2eb543f00b76d6262`.
- Rebalanced 0.2.33 DLL SHA-256: `2156dc6bd6b4a9e4305f2d613c7a901b0e2a4be0284ddb67fdf74e15aa25adf2`.
- Vanilla 1.0.42 DLL SHA-256: `9eaaf170b801a6c888b0cec45931eab31bdc7d9dd3eb1b9ec2111547cddb8ffa`.
- Change scope: preserve the accepted +100 pulpit extra-height baseline; if final wrapped Effect text forces the prayer button below the available parchment bottom margin, grow the already-verified real root window by the measured missing clearance instead of pushing the button back upward into the Effect text. The dynamic amount resets on every redraw, preventing cumulative growth.
- Runtime acceptance pending. Required focused test: Russian Repose that previously overlapped the action button, then short Faith as the no-unnecessary-growth control. No mechanics retest is required.


### 2026-09-26 — Rebalanced 0.2.33 rejected; post-render pulpit geometry probe

- Runtime used exact **PrayerClarity: Rebalanced 0.2.33** with **Neutral Test Console 0.1.16** under Graveyard Keeper 1.407 / Russian locale. The returned log confirms both exact versions loaded and the neutral console reported no presentation probes.
- Visual result: **0.2.33 is rejected**. Russian Repose still lets the long Effect block reach/overlap the action button; the user also reports Japanese fails while English fits. Short Faith and the ordinary pulpit frame showed no noticeable size change.
- The returned log contains no PrayerClarity forecast exception/failure, so this is not a fallback-to-vanilla/error-path symptom.
- New exact gate state: **BLOCKED**. The real root-window owner and PrayerClarity final button writer remain known, but 0.2.33 falsified the assumption that same-redraw geometry used by `PulpitPolish.MovePrayerButton` is sufficient to detect the final localized overlap/required bottom deficit.
- Research-method checkpoint: screenshot/direct observation proves failure but cannot distinguish stale Effect `worldCorners`, selection of a non-representative craft-button UIWidget, or another post-layout geometry difference. Source inspection alone cannot resolve which live value differs.
- Separate research-only **Pulpit Geometry Probe 0.1.0** was created; the neutral Test Console remains unchanged.
  - branch: `research/pulpit-geometry-probe-0.1.0`;
  - exact source: `8ed0995e959976531e9962e3e8eb37a56174c14e`;
  - CI run: `36201536038` — success;
  - artifact ID: `10891434373`;
  - artifact ZIP digest: `sha256:d3c90c05160bd2f616bee50f3730df4abb162435ebf1b5d051f6a18900a183e8`;
  - DLL SHA-256: `9a5b7766157c0edb98d46566d2919fb3590950eab2d9981f65e6ccd02ea02117`.
- Probe behavior: no Harmony patches and no UI/save/game mutation. With the failing pulpit screen fully rendered, **F8** dumps root/container geometry, Effect raw/processed text and settled label bounds, every craft-button UIWidget, the exact widget 0.2.33 would select, and a replay of the 0.2.33 overlap/deficit calculation.
- Minimum next runtime action: keep Rebalanced 0.2.33 + Neutral Test Console 0.1.16, add the probe, open failing Russian Repose, wait until rendered, press F8 once, return the log. No sermon execution or multi-locale sweep is required for this research step.


### 2026-09-26 — Pulpit geometry cause closed; Rebalanced 0.2.34 ready

- Neutral Test Console shortcut conflict was confirmed in the returned runtime log: BepInEx Configuration Manager uses F1. Neutral Test Console **0.1.17** therefore changes only its toggle to **F2**; the neutral tooling boundary and utilities are unchanged.
  - branch: `candidate/rebalanced-test-console-0.1.17`;
  - exact source: `922924f228777830d8551c40481a6083c359cbaa`;
  - CI run: `36202280309` — success;
  - artifact ID: `10892053632`;
  - artifact ZIP digest: `sha256:6230e2b16e4648e759f8797eb051f23711d933ce53fff00cc4bf8e98f52e5fdd`;
  - DLL SHA-256: `9d4743e443d99bf9b77b0aec99d795b9b348ca9e206c40a58c39b8a74577359c`.
- The exact Pulpit Geometry Probe 0.1.0 runtime closed the 0.2.33 failure cause:
  - settled root window: 284x341;
  - settled Russian Repose Effect label: 240x56, bounds Y `[-131.939, -76]`;
  - craft-button root after the 0.2.33 immediate pass: local Y `-134`;
  - 0.2.33 selected the direct button UILabel only, bounds `[-142, -126]`;
  - the active visible red button background spans `[-147, -123]`, proving the UILabel does not represent the full visible button;
  - replaying the same 0.2.33 calculation after NGUI had settled still required another `13.938` UI units downward. Therefore the same-redraw measurement was not final settled geometry.
- Exact Graveyard Keeper 1.407 decompilation at `Kupie/GYK_DECOMP@6abf79199d92482af1c7573870dd9a20ec2270b9` confirms `PrayCraftGUI : MonoBehaviour`, so a bounded one-shot next-frame coroutine can observe final NGUI geometry on the existing host GUI instance without adding a permanent Update poll.
- New production gate `pulpit-settled-visible-button-clearance` = **READY** in gate-only commit `07ecf281aeecdf3d84789359f903e96f8fcebede`.
- Rebalanced **0.2.34** / Vanilla sibling **1.0.43** were deliberately created from accepted 0.2.32 source `768bb9929823a3b3fd2496fdfd71de0587ddeb27`, not from rejected 0.2.33.
- Candidate exact source: `0cbd08055d40d9bb2f25493ad46878cfec76de42`.
- CI run: `36202519978` — success; candidate gate validation, localization validation and both sibling builds passed.
- Artifact ID: `10892562384`; artifact ZIP digest: `sha256:96aa6336d7c1e16fdeaac09cf38676d31b605c589c0b2e3f2db28c724c23a720`.
- Rebalanced 0.2.34 DLL SHA-256: `f536b9a6f776dc060c966a4e74e29793687d4444b4e01b9d8eeeb1c6b4d6706c`.
- Vanilla 1.0.43 DLL SHA-256: `2bc7a3eb4aafec3d46a92ab63c57bd9a36169feb24bd70b9e8019c11078788b0`.
- 0.2.34 implementation:
  - renders/resets immediately on the normal `RedrawTextValues` path;
  - schedules only a bounded settled-layout pass on the existing `PrayCraftGUI` MonoBehaviour;
  - measures the union of **all active UIWidget children** under the craft button, rather than only its UILabel;
  - moves the visible button below the settled Effect by 8 UI units;
  - grows the verified real root window only when the predicted settled button bottom would violate the 10-unit parchment margin;
  - dynamic growth resets on every host redraw; there is no permanent per-frame polling.
- Runtime acceptance pending. Minimum focused acceptance: Russian Repose, Japanese Repose, then short Faith as the no-unnecessary-growth control. The geometry probe should be removed for this production acceptance; Neutral Test Console 0.1.17 may remain installed.


### 2026-09-26 — Rebalanced 0.2.34 rejected; final settled placement still wrong

- Runtime used exact **PrayerClarity: Rebalanced 0.2.34** with **Neutral Test Console 0.1.17** and with the pulpit geometry probe removed. The returned log confirms both versions and confirms the neutral console uses F2.
- Visual result: **0.2.34 is rejected**.
  - Russian Soul's Repose: the pulpit visibly redraws/grows compared with the fixed baseline, but the last Effect line is still too close to / visually intrudes into the action-button area.
  - Japanese Soul's Repose: the failure is stronger; the final Effect block visibly overlaps the action button.
- The screenshots also show unused parchment below the button, especially in Japanese. Therefore the remaining defect is not simply "the root window cannot become long enough"; the final relationship between settled Effect bounds and the final visible button position is still wrong.
- The returned log contains no PrayerClarity forecast exception/failure. It confirms Japanese was selected through the normal language path and the native pulpit was reopened under that locale.
- Fresh regression gate for the exact final-placement property is **BLOCKED**. 0.2.34 proved that the selected next-frame/full-button-union mechanism is still insufficient; do not produce 0.2.35 by changing constants or adding more growth passes without measuring the final 0.2.34 state.
- Solution-space checkpoint reopened:
  - keep fixed/worst-case geometry as an available simpler fallback if dynamic geometry continues to require fragile lifecycle assumptions;
  - before choosing, measure the already-rendered 0.2.34 result rather than adding another production mechanism.
- Research-method checkpoint:
  - exact question: after 0.2.34 has finished all bounded settled-layout passes, what are the final root-window bounds, Effect UILabel bounds/printed size, and full visible button bounds in a failing locale, and how much clearance actually remains?
  - existing path: the already-built read-only **Pulpit Geometry Probe 0.1.0** can answer this without modifying production or the neutral Test Console; no new probe build is needed.
  - minimum runtime action: install the existing probe alongside exact 0.2.34, open failing Russian Repose and press F8 once, then switch to Japanese Repose and press F8 once, and return the log. No sermon execution or Faith retest is needed at this research step.


### 2026-09-26 — Rebalanced 0.2.34 final geometry closes solution choice; 0.2.35 ready

- Exact runtime used **PrayerClarity: Rebalanced 0.2.34**, **Neutral Test Console 0.1.17**, and the existing read-only **Pulpit Geometry Probe 0.1.0**. The log confirms all three exact versions loaded under Graveyard Keeper 1.407.
- Russian post-render geometry:
  - root window height `350`, bounds `[-175, 175]`;
  - Effect bounds `[-131.939, -76]`;
  - active red button background bounds `[-152, -128]`;
  - actual Effect-to-button gap = `-3.939` UI units (3.939 overlap);
  - required movement for the accepted 8-unit clearance = `11.939` UI units downward;
  - after that movement the button bottom would be `-163.939`, leaving `11.061` UI units above the window bottom. **No additional window growth is required.**
- Japanese post-render geometry:
  - root window height `346`, bounds `[-173, 173]`;
  - Effect bounds `[-129.939, -74]`;
  - active red button background bounds `[-150, -126]`;
  - the same actual gap `-3.939`, the same required final movement `11.939`, and the same resulting bottom margin `11.061`. **No additional window growth is required.**
- This closes the 0.2.34 uncertainty: the content-driven root-window sizing is already sufficient in both independently failing locales. The remaining defect is one final settled button correction.
- Solution-space checkpoint was re-opened after the failed candidate:
  - **content-driven dynamic layout** — retained;
  - **bounded fixed/worst-case layout** — rejected for this iteration because the measured dynamic window is already adequate and fixed geometry would unnecessarily enlarge short prayers while requiring a new fixed-height choice;
  - **wording/content reduction** — not used as a technical shortcut because it changes a user-owned presentation/design requirement.
  - Current dynamic path remains the least-complex adequate family because the remaining correction is local, measured and requires no new host hook/lifecycle assumption.
- Rebalanced **0.2.35** / Vanilla sibling **1.0.44** were created from exact accepted 0.2.32 source `768bb9929823a3b3fd2496fdfd71de0587ddeb27`, not from rejected 0.2.34.
- Gate-only commit: `9acd438ae63b421168722ca5553cc67604907508`; gate `pulpit-final-settled-button-correction` = **READY**.
- Candidate exact source: `e02ec8deaf41ba55c223ef2a64582aa59b0f3bc2`.
- Implementation reproduces the evidence-backed 0.2.34 settled/full-visible-button mechanism but allows one **fourth bounded settled pass**. Runtime evidence shows that at the measured 0.2.34 final state this pass requires only the final 11.939-unit button correction in RU and JA; no further root-window growth is predicted.
- CI run: `36203734225` — success. Candidate-gate validation, localization validation, Rebalanced build, Vanilla sibling build and artifact staging all passed.
- Artifact ID: `10892742207`; artifact ZIP digest: `sha256:80c44e84cd82bdb35939ef5ad03baa2802bf260a5eab929e1e13405dd7a40df9`.
- Rebalanced 0.2.35 DLL SHA-256: `020e1b8a97286d53be76cbca252013ddd32e272f4d0c8e9383f36d412bcfea8d`.
- Vanilla 1.0.44 DLL SHA-256: `2f6981f23edd474241bc5ae3f6e829e387f3ac1be0d698878f74dbc4c31234d2`.
- Runtime acceptance pending. Remove the geometry probe for production acceptance. Keep Neutral Test Console 0.1.17 if useful. Check Russian Soul's Repose, Japanese Soul's Repose, then short Faith as the no-unnecessary-growth control.


### 2026-09-26 — Rebalanced 0.2.35 rejected; solution space re-opened again

- Runtime used exact **PrayerClarity: Rebalanced 0.2.35**, **Neutral Test Console 0.1.17**, and the read-only **Pulpit Geometry Probe 0.1.0** remained installed.
- Visual result: **0.2.35 is rejected**.
  - Russian Soul's Repose is improved relative to the prior candidate but the final Effect block still sits too close to / intrudes into the action-button area.
  - Japanese Soul's Repose still visibly overlaps the action button.
  - Korean Soul's Repose independently reproduces the long-locale collision.
  - Chinese was visually observed to fit, so the defect is not simply "all CJK locales"; actual glyph metrics/wrapping matter.
- Source invariant: PrayerClarity still requests the same Effect font size for every locale. The more compact Chinese result therefore must not be explained as a PrayerClarity per-locale font-size override without further evidence.
- The returned log confirms exact 0.2.35 + Neutral Test Console 0.1.17 + Pulpit Geometry Probe 0.1.0 loaded, and confirms normal language transitions through Russian, Japanese, Chinese and Korean. No PrayerClarity forecast exception/failure is present.
- Important limitation: this returned log contains **no F8 PULPIT_GEOMETRY dump**. The probe was installed but was not triggered, so there is no post-render 0.2.35 geometry sample yet.
- Fresh gate for the exact final long-locale button-clearance property is **BLOCKED**. Do not create 0.2.36 by merely increasing the bounded-pass count again.
- Solution-space checkpoint re-opened after the second failure of the settled-pass mechanism:
  - continuing the current iterative settled-layout family is still plausible, but only if the existing 0.2.35 post-render geometry shows a narrow, deterministic remaining error;
  - a deterministic direct-layout family (derive final button/window geometry from settled content once, rather than iterating toward it) is now a credible simpler alternative and must be compared if the 0.2.35 dump shows continued convergence/lifecycle drift;
  - fixed/worst-case geometry remains available but would enlarge short layouts and is not preferred unless dynamic/direct placement proves lifecycle-fragile;
  - wording/content reduction remains a separate user-owned design option and is not a technical shortcut.
- Research-method checkpoint:
  - exact question: after exact 0.2.35 has finished its four bounded settled passes, what are the final Effect bounds, full visible button bounds, button-root position and root-window bounds in one still-failing locale?
  - existing exact artifact: Pulpit Geometry Probe 0.1.0 already answers this; no new probe or Test Console change is needed.
  - minimum runtime action: before removing the probe, open one clearly failing locale (Japanese is sufficient), wait for the pulpit to settle, press **F8 once**, and return the log. Korean/Russian duplicate F8 dumps are not required unless the Japanese geometry differs unexpectedly.
- The previously discussed wording change that puts the Silver/Gold reliability sentence in parentheses remains deferred. It is a separate presentation change and can alter wrapping by a small amount, so it should be applied only after the layout mechanism is accepted, then included in the final long-locale stress check.


### 2026-09-26 — 0.2.35 Japanese F8 proves extra pass did not become final geometry

- Exact runtime log confirms **PrayerClarity: Rebalanced 0.2.35**, **Neutral Test Console 0.1.17** and **Pulpit Geometry Probe 0.1.0** loaded under Graveyard Keeper 1.407.
- Japanese Soul's Repose F8 after full render:
  - root window: 284x346, bounds `[-173,173]`;
  - Effect: 240x56, bounds `[-129.939,-74]`;
  - craft-button root local Y: `-137`;
  - active visible red background: bounds `[-150,-126]`.
- Those final Japanese values are effectively the same as the previously recorded 0.2.34 Japanese post-render geometry. Therefore increasing the bounded settled loop from three to four passes did **not** produce a durable different final button geometry.
- Using the actual active visible-button union, the 0.2.35 final state still needs `11.939` UI units downward for the accepted 8-unit Effect/button clearance. The existing window is already tall enough: moving the visible button union from `[-150,-126]` to `[-161.939,-137.939]` leaves `11.061` UI units above the root bottom `-173`, so further window growth is not the missing behavior.
- Source inspection confirms 0.2.35 writes `craft button.transform.localPosition` inside the bounded coroutine. Because the extra pass does not survive into the observed final geometry, the current open hypothesis is that NGUI anchor/layout ownership rewrites that transform after PrayerClarity's mutation. This is **not yet accepted fact** because probe 0.1.0 did not record anchor state.
- Production gate remains **BLOCKED**; do not make 0.2.36 by increasing pass count.
- Solution-space implication: iterative convergence by adding more settled passes is no longer a credible next production step. If anchor ownership is confirmed, compare an anchor-aware/direct deterministic placement with fixed geometry; prefer the least-complex path that owns the actual final geometry.
- Existing probe was narrowly extended to **Pulpit Geometry Probe 0.1.1** instead of creating a new diagnostic family.
  - branch: `research/pulpit-geometry-probe-0.1.1`;
  - exact source: `d33c141f132b430d79140b96c683886851b9cc0a`;
  - CI run: `36205348199` — success;
  - artifact ID: `10893244522`;
  - artifact ZIP digest: `sha256:ebec18c730987a1a3c79bf16d964b2ce0b68fcc85556133f4580f75476658fc8`;
  - DLL SHA-256: `eb0dec1e9adce244aa54080e1e56bc80db17099e28a7e4893a97aadf50959d14`.
- Probe 0.1.1 remains read-only and adds: full active visible-button union calculation, every button UIWidget's anchor state/targets/offsets, and all components on the craft-button root.
- Minimum next runtime action: replace probe 0.1.0 with 0.1.1, keep exact Rebalanced 0.2.35, open Japanese Soul's Repose, wait for final render, press **F8 once**, return the log. No RU/KO/Faith retest is needed.


### 2026-09-26 — Craft-button anchor owner proved; Rebalanced 0.2.36 ready

- Exact runtime used **PrayerClarity: Rebalanced 0.2.35**, **Neutral Test Console 0.1.17**, and **Pulpit Geometry Probe 0.1.1** under Graveyard Keeper 1.407 / Japanese.
- Probe 0.1.1 proved the actual final button owner:
  - root `UI Root/Pray GUI/window/craft button` carries the root `UILabel`;
  - that UILabel is `isAnchored=True`, `isAnchoredVertical=True`, `updateAnchors=OnUpdate`;
  - its vertical anchors both target `UI Root/Pray GUI/window` at relative `0`, with bottom absolute `28` and top absolute `44`;
  - the active red `craft button back` UI2DSprite is itself anchored to the root craft-button widget with bottom/top absolutes `-5/+3`.
- This proves NGUI anchor resolution, not PrayerClarity's `craft button.transform.localPosition`, is the final writer of action-button placement. The failed 0.2.34/0.2.35 transform mutations were therefore being legitimately overwritten by native NGUI ownership.
- Same Japanese settled sample:
  - root window bounds `[-173,173]`, height `346`;
  - Effect bounds `[-129.939,-74]`;
  - full active visible button union `[-150,-126]`;
  - current algorithm requires `11.938` UI units downward for 8-unit clearance.
- Native-anchor arithmetic:
  - the button root is bottom-anchored to the root window, so increasing total root height by `2*d` moves the button down by `d`;
  - the stock content container is already accepted as anchored on all four sides with fixed bottom/top margins `+7/-35`, so symmetric root growth increases container height while preserving its centre (`(-35 + 7)/2 = -14`), leaving PrayerClarity forecast positions stable;
  - therefore the measured Japanese collision can be resolved by root-window growth alone, without mutating button anchors or transforms.
- Solution-space checkpoint after anchor proof:
  - iterative transform correction — rejected: it writes the wrong owner;
  - direct anchor-offset mutation — viable but more invasive than necessary;
  - fixed worst-case window — viable but unnecessarily enlarges short prayers;
  - **native anchor-driven root growth** — selected as the least-powerful adequate mechanism because it writes the already-verified root owner and lets NGUI perform its own final button placement.
- Rebalanced **0.2.36** / Vanilla sibling **1.0.45** were created from accepted 0.2.32 source `768bb9929823a3b3fd2496fdfd71de0587ddeb27`.
- Gate-only commit: `135a8d724ca56c21f8446dfbc95f265432e84568`; gate `pulpit-native-anchor-driven-clearance` = **READY**.
- Candidate exact source: `6c4d1ffddc53a8c32f0bdac39c7dcbfe127eefee`.
- 0.2.36 implementation:
  - waits one frame for localized ResizeHeight Effect geometry to settle;
  - measures the full active visible button-widget union;
  - computes required Effect-to-button clearance once;
  - changes **only** the unanchored real root-window height by approximately `2 * requiredDownward` with a small integer rounding guard;
  - performs no craft-button transform write, no anchor-offset mutation, no iterative convergence loop and no permanent polling.
- CI run: `36232422519` — success; candidate gate, localization validation and both sibling builds passed.
- Artifact ID: `10903415227`; artifact ZIP digest: `sha256:329665f1ce2a730805c0fd2d8f080e20bda59301d1751acac1d30456941a4b4c`.
- Rebalanced 0.2.36 DLL SHA-256: `228539ed3579fb9a94257e6747489e32657e615ba69da8990e96195ac6e9ca15`.
- Vanilla 1.0.45 DLL SHA-256: `1cf45a439d9b75725cfd417b802502718da95a3a01e7c9f45002838642fb9bdc`.
- Runtime acceptance pending. Remove Pulpit Geometry Probe before acceptance; Neutral Test Console 0.1.17 may remain. Check Russian, Japanese and Korean Soul's Repose, then short Faith as the no-unnecessary-growth control.
- If accepted, promote the reusable host fact about the pulpit craft-button anchor chain/final-writer ownership into `NikichMods/GraveyardKeeperResearch`.


### 2026-09-26 — Rebalanced 0.2.36 layout improved; live language-switch font regression found

- Runtime used exact **PrayerClarity: Rebalanced 0.2.36** and **Neutral Test Console 0.1.17** with the pulpit geometry probe removed.
- Visual result:
  - Japanese Soul's Repose now shows a clear gap between the settled Effect block and the red action button; the native-anchor-driven window-growth mechanism is visibly behaving in the intended direction.
  - English Soul's Repose also clears the action button.
  - Full acceptance of 0.2.36 is **blocked** by a separate live-language-switch regression before RU/KO control coverage could be completed.
- Regression: after cycling through CJK and other languages in the same running session, PrayerClarity pulpit forecast text can inherit the wrong font/metrics. In the reported Russian screen most Cyrillic forecast glyphs disappear while native/localized window chrome (title/action button) remains readable; other post-CJK languages can also look oversized.
- Exact log sequence immediately before the broken Russian reproduction includes `ko -> zh_cn -> ja -> pl -> it -> ru -> es -> ru`, followed by reopening the church pulpit. No PrayerClarity exception is logged.
- Host/static ownership proof:
  - `GameSettings.ApplyLanguageChange()` calls `GJL.LoadLanguageResource(language)`, then `GUIElements.UpdateLanguageChangeForAllBaseGUI()`.
  - `GUIElements.UpdateLanguageChangeForAllBaseGUI()` and `BaseGUI.UpdateLocalizedLabels()` delegate font ownership to `GJL.EnsureChildLabelsHasCorrectFont(..., true)`.
  - native `LocalizedLabel.Localize()` explicitly calls `GJL.EnsureLabelHasCorrectFont(label, true)` after assigning localized text.
- PrayerClarity source finding:
  - PrayerClarity creates raw cloned `UILabel` forecast widgets and copies `bitmapFont` / `trueTypeFont` from the pulpit template only when those labels are created;
  - subsequent renders reuse the same custom widgets and do not explicitly re-run the game's language-aware font owner on those labels.
- Root-cause classification: **PrayerClarity regression**, not accepted as a vanilla game bug. The mod owns the custom raw labels and must rejoin the game's native language-font lifecycle.
- Solution-space checkpoint:
  - destroy/recreate forecast labels on every language change — works in principle but adds lifecycle churn and a new language-change state concern;
  - add a dedicated `GJL.LoadLanguageResource` hook just for pulpit labels — unnecessary because pulpit redraw is already an exact consumer boundary;
  - manually map fonts by language — rejected because GJL already owns the native mapping;
  - **selected:** before each PrayerClarity pulpit render, call the native `GJL.EnsureLabelHasCorrectFont(label, true)` on the template/custom forecast labels, and likewise on any layout-created label. This is the least-complex path and delegates to the verified host owner.
- Fresh production gate for this exact property is **READY**; no diagnostic probe is needed because runtime reproduction + direct host/source inspection establish owner and correction seam.


### 2026-09-26 — Rebalanced 0.2.37 current-language pulpit font refresh candidate

- Baseline: exact Rebalanced 0.2.36 source `6c4d1ffddc53a8c32f0bdac39c7dcbfe127eefee`; the 0.2.36 native-anchor-driven clearance mechanism is preserved unchanged.
- Gate-only commit: `002369e72512d107578817a49f581740b7ede07a`; gate `pulpit-language-font-refresh` = **READY**.
- Selected fix follows the verified host owner instead of maintaining a PrayerClarity font map:
  - added a narrow bridge to native `GJL.EnsureLabelHasCorrectFont(label, true)`;
  - every PrayerClarity pulpit redraw now refreshes the current-language font on the pulpit template plus persistent Result / Effect / DependencyNote labels before assigning/configuring text;
  - the layout-created ResultHeader receives the same native font refresh immediately after capture/creation.
- No language-change hook, polling, label recreation, custom font table, font asset or new diagnostic probe was added.
- Rebalanced candidate: **0.2.37**; Vanilla sibling: **1.0.46**.
- Exact candidate source: `d95eb760105fa0fdcb4922a1d0f270eb5dbc2c05`.
- CI run: `36233062229` — success; production gate, localization validation, Rebalanced build, Vanilla sibling build and artifact staging all passed.
- Artifact ID: `10903635730`; artifact ZIP digest: `sha256:60b9e9dd8e784cc78db8d400621c5ce03b84b61e1d71f3ae516800c6bcb19b38`.
- Rebalanced 0.2.37 DLL SHA-256: `6498e120809832a7efdb9ec17fd1a36e91e1faee9e102de083f35d6804f612dc`.
- Vanilla 1.0.46 DLL SHA-256: `eed4f35a929e42442ec11defad09112253202c1c6cb86641046eefb949f5b7fc`.
- Runtime acceptance pending. Minimum combined test: in one session check Japanese Soul's Repose, switch to English and reopen, switch to Russian and reopen; then Korean Soul's Repose once and short Faith once. This simultaneously verifies the new font-lifecycle regression fix and completes the still-pending long-locale/no-unnecessary-growth acceptance for the 0.2.36 layout mechanism.


### 2026-09-26 — Rebalanced 0.2.37 runtime accepted

- User runtime acceptance was performed on exact **PrayerClarity: Rebalanced 0.2.37** under Graveyard Keeper 1.407 with **Neutral Test Console 0.1.17** and no pulpit geometry probe.
- Exact candidate source remains `d95eb760105fa0fdcb4922a1d0f270eb5dbc2c05`; accepted Rebalanced DLL SHA-256 remains `6498e120809832a7efdb9ec17fd1a36e91e1faee9e102de083f35d6804f612dc`.
- Visual acceptance evidence supplied by the user:
  - Russian long Soul's Repose renders all Cyrillic forecast text correctly and the full Effect block clears the action button.
  - Russian short Faith remains compact and does not acquire unnecessary long-prayer window growth.
  - Japanese Soul's Repose renders with the correct CJK font/metrics and its Effect block clears the action button.
  - English Soul's Repose renders normally after live language switching.
  - Korean Soul's Repose renders with the correct font/metrics and its long Effect block clears the action button.
- Returned runtime log confirms 0.2.37 loaded and records repeated live language-resource changes through Latin, Cyrillic and CJK locales plus repeated pulpit opens; no PrayerClarity exception/fallback is present during the accepted run.
- **Accepted result:** both previously open pulpit defects are closed:
  1. long localized Effect text now obtains sufficient bottom clearance through native-anchor-driven root-window growth without permanent button-transform fighting;
  2. PrayerClarity-owned pulpit UILabels now rejoin the game's native GJL current-language font lifecycle on redraw, preventing stale CJK/Latin/Cyrillic font inheritance across live language changes.
- This is an accepted runtime behavior result for 0.2.37. It does **not** by itself promote 0.2.37 / Vanilla 1.0.46 to the public stable line; stable promotion remains a separate user decision.


### 2026-09-26 — Rebalanced 0.2.37 frozen as accepted runtime baseline

- Exact accepted runtime/source SHA: `d95eb760105fa0fdcb4922a1d0f270eb5dbc2c05`.
- Frozen accepted ref: `accepted/rebalanced-0.2.37`.
- Accepted Rebalanced DLL SHA-256: `6498e120809832a7efdb9ec17fd1a36e91e1faee9e102de083f35d6804f612dc`.
- User runtime acceptance already closed both pending pulpit properties on exact 0.2.37:
  1. long localized Effect text clears the full visible action button through native-anchor-driven root-window growth while short Faith remains compact;
  2. PrayerClarity-owned pulpit UILabels follow the current-language GJL font lifecycle across live CJK / Latin / Cyrillic switching.
- This ref is the current **accepted Rebalanced development/runtime baseline** for future candidates.
- This bookkeeping step does **not** publish 0.2.37 as the current public stable GitHub/Nexus release and does not imply acceptance of the untested Vanilla sibling 1.0.46.
- No new runtime test or hosted CI is required for this ref/docs-only closure.


### 2026-09-26 — post-0.2.37 backlog reconciliation corrected

A deeper review of recent project-chat decisions against exact accepted source `d95eb760105fa0fdcb4922a1d0f270eb5dbc2c05` corrected the earlier statement that only the Soul's Repose parenthetical remained.

Still open:
- Shoots & Roots: user explicitly rejected the 95% combined-cap note on Bronze/Silver and accepted it for Gold only. Exact 0.2.37 active-effect source still calls `rebalanced.active.plant` with the combined 95% cap for every active tier.
- Prayer for Donations: user accepted `1 gold` as the Gold pulpit denomination, matching the item tooltip. Exact 0.2.37 final pulpit writer still formats fixed money through `PresentationText.FormatPrayerContribution` as `(slv) +100`.
- Soul's Repose: Silver/Gold reliability sentence remains queued for parenthetical-note presentation.

Decision pending:
- legacy Protective Prayer / `b_shield`: item lore still resolves stock `b_shield_d` while Rebalanced maps surviving items/mechanics to Combat / `buff_sword` and removes Protection crafting/Technology. User noticed the wording mismatch but explicitly asked not to change it yet; no production gate should open until a product decision is made.

Closed by later evidence/source:
- Combo duplicate quality star = Test Console artifact, not production;
- item-tooltip requirement structure = accepted separate Sermon success section;
- item inline icon wrapping = accepted 0.2.32;
- pulpit localized-text clearance + live-language font lifecycle = accepted 0.2.37.

Non-blocking evidence gaps remain unchanged: terminal Repose endpoint and real successful Silver/Gold Imagination three-Story payout.


### 2026-09-26 — legacy Protective Prayer lore decision closed

- User product decision: preserve the historical stock lore on legacy `b_shield` / Protective Prayer items.
- Rebalanced continues to retire Protection crafting/Technology and map existing legacy `b_shield` items/mechanics to the Combat `buff_sword` path.
- Do **not** normalize legacy item lore to Combat Prayer lore. The current stock `b_shield_d` item text is intentionally preserved.
- No production change or runtime test is required for this decision.


### 2026-09-26 — Rebalanced 0.2.38 presentation cleanup candidate

- Baseline: accepted Rebalanced 0.2.37 source `d95eb760105fa0fdcb4922a1d0f270eb5dbc2c05` / `accepted/rebalanced-0.2.37`.
- Candidate branch: `candidate/rebalanced-0.2.38`.
- Gate-only commit: `e2ec253a538178932ad009196be5c3c6456c3e40`.
- Included READY changes:
  1. Shoots & Roots active-effect 95% all-sources cap note is Gold-only; Bronze/Silver show only their own prayer reduction.
  2. Fixed-only pulpit money bonuses use the native money formatter, so Gold Donations presents 100 silver-equivalent as 1 gold rather than 100 silver.
  3. Soul's Repose endpoint reliability clarification keeps its existing meaning and becomes a parenthetical note.
- Legacy Protective Prayer / `b_shield` historical item lore is intentionally preserved and is not part of this candidate.
- Rebalanced candidate: **0.2.38**. Shared sibling build metadata advances Vanilla to **1.0.47**.
- Exact runtime artifact source: `6f5ef168810945135cee57082cbf90d31776e46b`.
- CI run: `36245784593` — success; production gate validation, localization validation, Rebalanced build, Vanilla sibling build and artifact staging passed.
- Artifact ID: `10907083834`; artifact ZIP digest: `sha256:875905412950ab7c27ded1985997cddb39b5d39a89084ad5f568cfc71acc5ecd`.
- Rebalanced 0.2.38 DLL SHA-256: `e547f7bb76e0ef512dc669ea30543dada5a2aeb8044b1c7ffcbe7fecf17303e5`.
- Vanilla 1.0.47 sibling DLL SHA-256: `48af2068935f1e34152dbbfeda7e13bd312ee91fd4c73a4c88712acf8b8ac94e`.
- Runtime acceptance pending. Minimal combined visual pass:
  - Bronze/Silver/Gold Shoots & Roots Temporary Effects: no 95% cap note on Bronze/Silver, explicit all-sources 95% cap on Gold;
  - Gold Prayer for Donations at the pulpit: fixed bonus shown as 1 gold;
  - Bronze Soul's Repose endpoint case: reliability clarification shown as a parenthetical note and wraps cleanly.
- No prayer-mechanics replay is required because all three changes are presentation-only over already accepted mechanics.
