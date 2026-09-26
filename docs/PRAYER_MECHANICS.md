# Prayer Mechanics — Graveyard Keeper 1.407

Status: **canonical stock Graveyard Keeper 1.407 mechanics evidence map**, originally closed from read-only probes 0.1.0–0.1.5 and retained after the stable PrayerClarity releases.

This document records verified **stock** mechanics independently of PrayerClarity's modded behavior. `prayer item`, `PrayCraft`, `PrayEventDefinition`, localized prayer name, buff, sermon FlowCanvas, and downstream consumers remain distinct layers unless direct evidence maps them.

## Evidence basis

Primary evidence comes from the user's installed Graveyard Keeper 1.407 runtime:

- `Assembly-CSharp, Version=11.0.0.0`;
- module MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`;
- Assembly-CSharp IL;
- runtime `GameBalance` definitions;
- current Russian localization;
- serialized/loaded FlowCanvas graphs;
- narrow read-only probes 0.1.0–0.1.5.

The probes contain no Harmony patches, do not execute FlowCanvas graphs, and do not intentionally mutate game/save state. Proprietary assemblies, full decompilations, and raw game assets are not committed.

Community pages and wikis are cross-check / UX evidence, not authority for mechanics when direct 1.407 evidence exists.

## Internal sermon model

A sermon spans:

1. prayer item, e.g. `b_faith:1`;
2. `CraftDefinition` with `craft_type = PrayCraft`;
3. `linked_sub_id` -> `PrayEventDefinition`;
4. event expressions for base `people`, `faith`, `money`;
5. prayer fields such as `needs_quality`, `k_faith`, `k_money`, fixed outputs, optional `buff`, `dur_parameter`;
6. `FlowCanvas/pray` distribution/animation graph;
7. downstream visitor, item-drop, and buff consumers.

## Core prayer calculation

### Base values

`PrayLogics.CalculatePray` evaluates:

- `people = max(0, RoundToInt(event.people))`
- `faith = max(0, RoundToInt(event.faith))`
- `money = max(0, event.money)`

Base people/Faith are integer-rounded here. Base money remains floating-point.

### Success probability

For `needs_quality != 0`:

`success_percent = RoundToInt(current_church_quality / needs_quality * 100)`

Then clamp to `0..100`.

- 100% succeeds automatically;
- otherwise success is `Random.Range(0,100) < success_percent`.

For `needs_quality == 0`, success is 100%.

### Success-only prayer bonuses

On success:

- fixed Faith output -> `faith_bonus += value`;
- fixed money output -> `money_bonus += value / 100`;
- `k_faith` -> `faith_bonus += RoundToInt(base_faith * k_faith)`;
- `k_money` -> `money_bonus += Round(base_money * k_money * 100) / 100`;
- non-Faith/non-money outputs remain in `_sermon_drops`.

On failure:

- `faith_bonus = 0`;
- `money_bonus = 0`.

Base event values remain intact.

`Flow_CalculatePrayEvent` exposes downstream `people`, base `faith`, separate `faith_bonus`, combined `money = base_money + money_bonus`, and `success`.

## Final sermon payout wiring

Probe 0.1.2 captured the stock `FlowCanvas/pray` graph.

### Faith

Base Faith is sent to `Flow_SpreadFaithIncome` before the success/failure reaction stage. A second `Flow_SpreadFaithIncome` receives `faith_bonus` later.

Therefore:

- success: `Faith delivered = base_faith + faith_bonus`;
- failure: `Faith delivered = base_faith`.

Failure does not remove base Faith.

### Money

The graph passes combined money to `Flow_SpreadMoneyIncome`. Its `chance` input is `1.0` on success and `0.5` on failure.

However, `SpreadMoneyIncome` tests visitors with the integer overload `UnityEngine.Random.Range(0,1)`. That overload always returns `0`, so both `0 < 1.0` and `0 < 0.5` are true. In stock 1.407 all visitors are therefore selected in both branches.

Actual result:

- success: `money delivered = base_money + money_bonus`;
- failure: `money delivered = base_money`.

This is an implementation mismatch: the graph looks structured to reduce donation participation on failure, but the current helper does not do so.

### Distribution details

`SpreadFaithIncome(prayers, faith)` assigns each Faith unit independently to a random valid visitor.

`SpreadMoneyIncome(prayers, money, success_percent)`:

1. chooses selected visitors using the test above;
2. gives each selected visitor `floor(money*100/selected_count)/100`;
3. distributes leftover cents randomly until the requested total is reached.

Visible per-person coin drops are distribution of a total pool, not the formula creating the pool.

## Verified PrayEventDefinition catalogue

Notation:

- `CQ = GetZoneQ("church")`
- `GQ = GetZoneQ("graveyard")`
- `E = Ppar("p_eloquence")`
- `C = Ppar("p_cardinal")`
- `GP = Ppar("gratitude_points")`

| Event ID | Faith | Money | People |
| --- | --- | --- | --- |
| `default` | `CQ*0.4*(1+0.3E)` | `GQ*0.06` | `CQ*0.5` |
| `default_0` | `CQ*0.2*(1+0.3E)` | `GQ*(0.03+0.01C)` | `CQ*1` |
| `default_1` | same | same | `CQ*0.5` |
| `default_2` | same | same | `CQ*0.34` |
| `default_3` | same | same | `CQ*0.34` |
| `pray_for_souls_1` | `(CQ+GP)*0.1*(1+0.3E)` | `GQ*(0.03+0.01C)` | `CQ*0.5` |
| `pray_for_souls_2` | same | same | `CQ*0.34` |
| `pray_for_souls_3` | same | same | `CQ*0.34` |

Consequences:

- ordinary base Faith depends on church quality;
- base donations depend on graveyard quality;
- Eloquence changes base Faith;
- Cardinal changes the ordinary donation coefficient;
- Souls prayer adds current Soul Gratitude to the Faith baseline.

Without Eloquence, Souls base Faith is `(CQ + GP)/10` before integer rounding.

## Verified player prayer families

`q` = `needs_quality`; `kF` = `k_faith`; `kM` = `k_money`.

| Internal family | Current RU name | Event(s) | q tiers | kF tiers | kM tiers | Special data |
| --- | --- | --- | --- | --- | --- | --- |
| `b_empty` | Обычная молитва | `default_0` | 10 | 0 | 0 | fixed Faith on base craft; suffixed/placeholder reachability still open |
| `b_faith` | Молитва веры | `default_1/2/3` | 10/20/50 | .5/1/1.5 | .2/.2/.2 | fixed Faith + money |
| `b_money` | Молитва о пожертвованиях | `default_1/2/3` | 10/20/50 | .2/.2/.2 | .5/1/1.5 | fixed Faith + money |
| `b_faith_money` | Комбо-молитва | `default_1/2/3` | 15/30/60 | .5/1/1.5 | .5/1/1.5 | fixed Faith + money |
| `b_plant` | Молитва о корнях и побегах | `default_1/2/3` | 10/20/30 | .25/.25/.25 | .25/.25/.25 | `buff_plant`; 36/72/108 min; growth-speed wiring mismatch in stock 1.407 |
| `b_sins` | Молитва о покаянии | `default_1/2/3` | 10/20/40 | .25/.5/.75 | .1/.1/.1 | `buff_sins`; 18/36/54 min; no gameplay consumer found |
| `b_skull` | Молитва об упокоении | `default_1/2/3` | 20/40/50 | .1/.1/.1 | .25/.5/.75 | `buff_skull`; +1 Donkey maximum corpse tier; 18/36/54 min |
| `b_sword` | Молитва о возмездии | `default_1/2/3` | 10/20/40 | .25/.25/.25 | .25/.25/.25 | `buff_sword`; +5 damage; 36/72/108 min |
| `b_shield` | Защитная молитва | `default_1/2/3` | 10/20/40 | .25/.25/.25 | .25/.25/.25 | `buff_shield`; +4 armor; 36/72/108 min |
| `b_pen` | Молитва воображения | `default_1/2/3` | 10/40/60 | .25/.5/.75 | .1/.1/.1 | `buff_pen`; `craft_q=0.7`; 18/36/54 min |
| `b_star` | Молитва о совершенстве | `default_1/2/3` | 10/40/60 | .25/.5/.75 | .1/.1/.1 | `buff_star`; `craft_q=0.2`; 18/36/54 min |
| `b_village` | Молитва о процветании | `default_1/2/3` | 10/20/30 | .25/.25/.25 | .25/.25/.25 | Faith 1/2/3; money 1/2/3 silver; `blessing_commerce` x1/x2/x3 |
| `b_souls` | Молитва за упокой душ | `pray_for_souls_1/2/3` | 15/30/60 | .5/1/1.5 | .25/.25/.25 | Souls-specific baseline |
| `b_grat_points_incr` | Молитва о довольстве душ | `default_1/2/3` | 10/20/30 | .25/.25/.25 | .25/.25/.25 | `increase_gp_gain=1`; +10% before rounding; 36/72/108 min |
| `b_sin_shard` | Молитва за тщательное очищение душ | `default_1/2/3` | 10/20/30 | .25/.25/.25 | .25/.25/.25 | doubles Sin Shards; 36/72/108 min |

Better Save Soul technology `soul_church_additions` directly unlocks `b_souls`, `b_grat_points_incr`, and `b_sin_shard`.

### Prayer for Prosperity physical output

`b_village` outputs:

- tier 1: Faith x1, 1 silver, `blessing_commerce` x1;
- tier 2: Faith x2, 2 silver, `blessing_commerce` x2;
- tier 3: Faith x3, 3 silver, `blessing_commerce` x3.

Faith/money become bonus fields on success. `blessing_commerce` remains in `_sermon_drops` and is handled as a physical sermon drop.

Current RU item text identifies it as `Благословение коммерции`, sold to a merchant to raise that merchant's level. Exact merchant-side implementation is outside the present prayer-selection UX scope.

### Unclassified PrayCraft rows

Runtime data also contains `b_ghost`, `b_energy`, `b_random`, `b_techpoint_blue`, `b_techpoint_green`, `b_techpoint_red`, `b_circle`, `b_cross`.

These remain internal/unclassified until unlock/item/UI reachability proves player use.

## Buff mechanics

| Buff ID | Direct data | Verified meaning/status |
| --- | --- | --- |
| `buff_plant` | `buff_plant=1` | intended-looking `-0.2*WGOpar("buff_plant")` growth term exists, but prayer writes the param to the player while the craft expression reads the growing/workbench WGO; no propagation path found |
| `buff_sins` | `buff_sins=1` | timed buff exists; no gameplay consumer found in final reference audit |
| `buff_skull` | `body_max=1` | raises live Donkey `Tier max` by exactly 1 while active |
| `buff_sword` | `add_damage=5` | +5 damage |
| `buff_shield` | `add_armor=4` | +4 armor |
| `buff_pen` | `craft_q=0.7` | +0.7 input to linked-buff multi-quality calculation |
| `buff_star` | `craft_q=0.2` | +0.2 input to linked-buff multi-quality calculation |
| `buff_sin_shard` | `increase_sin_shard_drop=1` | doubles Sin Shards from soul healing |
| `buff_gp_increase` | `increase_gp_gain=1` | `GP_awarded = RoundToInt(GP_base * 1.1)` |

## Prayer of Repentance (`buff_sins`)

Probe 0.1.3 searched three independent surfaces in the running 1.407 installation:

1. string-literal users across loaded assemblies;
2. serialized loaded FlowCanvas graphs;
3. runtime GameBalance fields/SmartExpressions/nested data.

Results:

- no game-code method contains a `buff_sins` literal consumer;
- 180 loaded FlowCanvas graphs contained zero `buff_sins` hits;
- GameBalance hits were only the buff definition and the three `b_sins` prayer crafts.

**Fact:** the prayer produces/attaches a timed `buff_sins`, but no consumer was found on the inspected runtime surfaces.

**Strong hypothesis:** the special gameplay effect is inert/unimplemented in stock 1.407. External wiki/community reports independently point in the same direction, but the direct runtime audit is the primary evidence.

## Prayer for Shoots and Roots (`buff_plant`) — confirmed wiring mismatch

The current balance contains many growth/planting craft-time expressions such as:

`1440 * (1 - 0.2*WGOpar("grow_time") - 0.2*WGOpar("buff_plant"))`

and simpler forms such as:

`1080 * (1 - 0.2*WGOpar("buff_plant"))`.

Affected definitions include ordinary garden crops, vineyard crops, refugee-garden planting, berry bushes, apple/tree growth, flowers, and other plant respawn/growth crafts.

Probe 0.1.5 resolved `WGOpar` exactly:

`WGOpar(name) -> SmartExpression._wgo.GetParam(name, 0)`.

CraftComponent evaluates `current_craft.craft_time` with the workbench/growing `WorldGameObject` as the first (`wgo`) argument and the player as the character argument. Therefore the `WGOpar("buff_plant")` term reads the growing/workbench object's parameter.

But prayer buff application adds `BuffDefinition.res` to `MainGame.me.player.data`; for `buff_plant`, that puts `buff_plant=1` on the player. The 0.1.4 scan found no loaded FlowCanvas reference and no separate stock propagation path copying that player parameter onto growing WGOs.

**Facts:**

- the `-20%` craft-time term exists;
- the prayer buff writes `buff_plant=1` to the player;
- the term reads `buff_plant` from the growing/workbench WGO;
- no propagation path was found between those scopes.

**Conclusion for stock 1.407:** the normal prayer path does not feed its `buff_plant` value into the growth-time formula. The advertised/intended-looking special growth-speed effect is therefore effectively inert under the inspected stock wiring. This is a concrete parameter-scope mismatch, not merely a wiki claim.

Do not silently “fix” this in PrayerClarity; changing behavior would be a separate balance/bug-fix product decision.

## Prayer for Repose (`buff_skull`) — live Donkey consumer closed

Prayer buff application adds `body_max=1` to the player's parameters while active.

Probe 0.1.5 captured the complete loaded `npc_donkey` graph. Its live corpse-generation path wires:

- `body_min` + `add_body_min` -> `Flow_DropBody.Tier min`;
- `body_max` + `add_body_max` -> `Flow_DropBody.Tier max`.

`add_body_min`/`add_body_max` are separate progression/event modifiers. The prayer changes `body_max` itself.

Therefore, while Prayer for Repose is active:

`Donkey Tier max = normal body_max + 1 + add_body_max`

while the minimum tier is unchanged by the prayer.

**Fact:** Prayer for Repose raises the maximum corpse tier the Donkey can generate by exactly one tier for 18/36/54 minutes depending on prayer quality. Prayer quality changes duration, not the +1 magnitude.

## Multi-quality buffs

`CraftDefinition.GetBuffValue(buff_id)` returns 0 when a buff is inactive and otherwise returns `BuffDefinition.craft_q`.

`CraftDefinition.GetMultiqualityResult(...)` consumes linked buff IDs through this accessor. Therefore `buff_pen=0.7` and `buff_star=0.2` are real additive quality inputs for crafts whose definitions explicitly list those buffs.

Do not generalize their affected craft set until `linked_buffs` are enumerated if exact wording requires that scope.

## Sin Shards

Soul-healing output starts from `sins_count` and adds:

`increase_sin_shard_drop * sins_count`.

With `buff_sin_shard=1`:

`Sin Shards = 2 * sins_count`.

## Soul Gratitude

`SoulsHelper.CalculatePointsAfterSoulRelease(healed_soul)` computes:

- `d = healed_soul.durability`;
- `s = healed_soul.GetParam("sins_count",0)`;
- if `d > 0.9`, replace `d` with `1`;
- return `5*d + 5*s`.

Thus:

`GP_base = 5 * effective_durability + 5 * sins_count`.

Probe 0.1.4 captured the full loaded `soul_portal` graph and its connections. The prayer modifier is applied before integer rounding:

`GP_awarded = RoundToInt(GP_base * (1 + 0.1 * increase_gp_gain))`.

For `buff_gp_increase`, `increase_gp_gain=1`, so:

`GP_awarded = RoundToInt(GP_base * 1.1)`.

The rounded award is then added to current `gratitude_points`; the graph also enforces the Souls-zone-quality limit on the resulting total.

The previously open `+10%` rounding-order question is closed.

## Prayer buff application and duration

The stock path is closed:

1. `PlayerComponent.StartPrayAnimation(pray_craft, success)` stores the craft, resolved buff, and success state.
2. `PlayerComponent.CreatePrayBuffFlyingObject(pos)` passes non-zero `dur_parameter` as duration override.
3. `FlyingObject.CreateBuffFlyingObject(...)` stores the override.
4. `FlyingObject.ReallyGiveBuff(...)` calls `BuffsLogics.AddBuff(buff.id, override)`.
5. `AddBuff` uses the override instead of the BuffDefinition default when supplied and adds `BuffDefinition.res` to player data.
6. `PlayerBuff.GetTimerText()` uses the same 450-second game-time basis.

Prayer durations therefore map directly to displayed game-timer minutes:

- `18/36/54` -> 18/36/54 minutes;
- `36/72/108` -> 36 min / 1 h 12 min / 1 h 48 min.

Higher prayer quality can therefore increase duration while buff magnitude remains unchanged.

### Success animation / physical-drop path

Probe 0.1.3 established:

- `BaseCharacterComponent.StartPrayAnimation(success)` writes Animator bool `success`;
- `ChurchPulpit.DoBuffSuccessAnimation()` calls both `PlayerComponent.CreatePrayBuffFlyingObject(...)` and `PrayLogics.DropPrayItems()`.

No C# caller exists for `DoBuffSuccessAnimation`, consistent with a Unity AnimationEvent callback. The exact animation-asset edge was not inspected.

**Fact:** buff creation and physical sermon drops are grouped in the method explicitly named `DoBuffSuccessAnimation`, while the Animator receives the sermon success flag.

**Strongly supported, not asset-level proven:** buffs and `_sermon_drops` are success-animation-only outputs.

## Current Russian presentation

Selection UI shows:

- `Качество церкви: %1`
- `Проповедь требует: %1`
- `Шанс успеха: %1`
- below threshold: `Попытка молитвы`
- at/above threshold: `Молиться`

Prayer item descriptions also append `%1 необходимо, чтобы гарантировать успех проповеди.`

`PrayCraftGUI.RedrawTextValues` shows church quality, requirement, and success chance only. It does not forecast current Faith/donation output or quantify passive effects/duration.

Post-sermon report shows base Faith/money plus separate success-only bonus rows; it does not show one combined Faith total or donation total.

Several passive prayer descriptions are qualitative only, including `b_plant`, `b_sins`, `b_skull`, `b_sword`, `b_shield`, `b_pen`, and `b_star`.

Better Save Soul descriptions are clearer:

- `b_souls`: says Soul Gratitude increases Faith;
- `b_grat_points_incr`: states `+10%` Gratitude gain;
- `b_sin_shard`: states `x2` Sin Shards.

## Remaining non-blocking stock-research questions

The mechanics questions required for the stable PrayerClarity siblings are closed. Remaining questions are peripheral and should be reopened only when a concrete feature needs them:

1. exhaustive linked-craft scope for `buff_pen` / `buff_star`;
2. merchant-side implementation of `blessing_commerce`;
3. reachability classification for extra internal PrayCraft rows;
4. exact Animator asset edge if a future implementation needs that lifecycle target.

No additional static probe or user in-game mechanics test is currently justified.

## Stable-state conclusion

The core stock sermon calculation, success/failure payout behavior, requirements, prayer catalogue, principal buff durations/magnitudes, Souls mechanics, live Donkey Repose consumer, Roots/Shoots wiring mismatch, Sin Shard effect, and selection/report presentation are sufficiently verified for the current product family.

Two stock-1.407 anomalies remain important boundaries:

- Prayer of Repentance: timed buff exists, no gameplay consumer was found;
- Prayer for Shoots and Roots: the `-20%` growth formula exists, but the stock prayer buff and formula read/write different parameter owners.

**PrayerClarity: Vanilla 1.0.57** preserves these stock mechanics and presents them truthfully. **PrayerClarity: Rebalanced 0.2.51** intentionally repairs/reworks the affected behavior according to the separate accepted ruleset in `PRAYER_REBALANCE_OPTIONS.md`. Do not rewrite this stock evidence to match Rebalanced behavior.

## Repose terminal corpse progression — direct closure

Status: **direct stock 1.407 evidence; no new probe required**.

The previously archived read-only `PrayerClarity-audit-0.1.4` plus existing GameBalance/IL dumps close the late-game Repose endpoint.

### Permanent Donkey progression

The loaded `body_min` / `body_max` graph audit found only three owner graphs: `npc_bishop`, `npc_donkey`, and `npc_inquisitor`.

Permanent progression writes are:

- Bishop church-opening progression establishes `body_min = 1`, `body_max = 1`.
- Inquisitor `@inquisitor_tent_ready` raises `body_min` by +1, with compatibility/set path `body_min = 2`.
- Inquisitor `second_witch_burning` raises `body_max` by +1, with compatibility/set path `body_max = 2`.
- Later Inquisitor Dark Body progression raises `body_max` by +1 again, with compatibility/set path `body_max = 3`.
- The loaded Inquisitor graph contains exactly two `body_min` occurrences (the +1 write and set-to-2 compatibility path); no permanent `body_min = 3` step exists on the inspected loaded graph.
- The Donkey graph's `add_body_min` / `add_body_max` values are separate temporary/event modifiers and are reset by that flow; they are not another permanent corpse-quality tier.

Therefore the normal terminal permanent range is:

`body_min = 2`
`body_max = 3`

### Available ordinary body tiers

The direct 1.407 `GameBalance.bodies_data` dump contains ordinary `linked_item_id="body"` definitions at tiers 0, 1, 2 and 3.

After the tier-3 ordinary definitions, the remaining body definitions jump to special/story tiers 100, 101, 228 and 666. There is **no ordinary tier-4 BodyDefinition**.

### GenerateBody behavior

`GameSave.GenerateBody(tier_min, tier_max, ...)`:

1. enumerates `GameBalance.bodies_data`;
2. keeps definitions whose `BodyDefinition.tier` is between `tier_min` and `tier_max` inclusive;
3. returns null if no definition survives;
4. otherwise chooses a random surviving BodyDefinition.

There is no clamp from an empty requested tier to the nearest existing tier.

### Stock Repose endpoint

At terminal normal progression the ordinary Donkey range is `2..3`.

Stock `buff_skull` adds +1 only to player `body_max`, so the evaluated range becomes `2..4`.

Because no ordinary tier-4 body exists, the candidate BodyDefinition set for `2..4` is identical to the candidate set for `2..3`: tiers 2 and 3 only.

**Fact:** stock Prayer for Repose has no corpse-quality/distribution effect at terminal normal progression. It can still have ordinary sermon Faith/donation value.

This directly supports a contextual pulpit endpoint message in PrayerClarity: Vanilla.

### Rebalanced consequence

The Rebalanced concept "best eligible tier" must mean the **highest actually existing ordinary BodyDefinition tier inside the evaluated range**, not the raw numeric `tier_max`.

At terminal progression with Repose active:

- raw evaluated range: `2..4`;
- highest existing ordinary tier in that range: `3`.

Therefore the safe intended behavior is:

- Bronze: leave stock `2..4` unchanged (same effective stock pool 2/3);
- Silver: on the reliability branch, narrow to existing tier 3;
- Gold: narrow to existing tier 3, then keep only the tier-3 BodyDefinitions with the maximum game-derived total skull count.

At terminal 1.407 progression this resolves to the nine known 10-total-skull tier-3 definitions. The rule itself is dynamic and does not hard-code tier 3, those body IDs, or the value 10.

Historical Rebalanced 0.1.5 set `tier_min = tier_max` directly. At the terminal raw `2..4` state that could request `4..4`, for which stock `GenerateBody` has no definition and returns null. This was a **latent late-game edge-case in 0.1.5**, not an accepted behavior target.

The current stable Rebalanced 0.2.51 resolves the highest actually existing eligible ordinary BodyDefinition before narrowing and, for Gold, restricts that tier to the maximum live total skull score. Runtime evidence observed all 9 maximum-score terminal candidates and ten consecutive real native generations producing tier-3, 10-total-skull bodies with exact catalog restoration after each call. The old `4..4` failure mode is no longer the production design. The formerly deferred terminal endpoint wording/presentation was later runtime-observed and accepted across the relevant Repose surfaces.


### Directly inspected fixed success outputs relevant to specialist cleanup

For the next Rebalanced specialist-cleanup pass, fixed sermon outputs were cross-checked against an extracted Graveyard Keeper craft dataset and the project's existing runtime semantics.

Directly observed base-game prayer craft rows:
- Repentance: Faith x1 / x2 / x3;
- Shoots & Roots: Faith x1 / x2 / x3;
- Repose: Faith x1 / x2 / x3;
- Retribution/Combat source family: Faith x1 / x2 / x3;
- Imagination: Faith x1 / x2 / x3;
- Prosperity: Faith x1 / x2 / x3 plus 1 / 2 / 3 silver, in addition to Commercial Blessings;
- Faith / Donations / Combo stock rows also contain Faith x1/x2/x3 and 1/2/3 silver, but Rebalanced already replaces/removes those as part of the accepted resource-specialist rules.

The current PrayerClarity forecast reads fixed prayer outputs directly from each craft's output list, and Rebalanced already has a narrow static mechanism to remove prayer-owned Faith/money outputs without touching physical specialist rewards.

Excellence and Better Save Soul fixed-output rows are not declared present merely by symmetry. The 0.2.11 cleanup therefore does not depend on assuming that they exist: it applies a narrow postcondition to those specialist craft rows, removing only outputs whose IDs are exactly `faith` or `money` if such rows are present, while leaving all other outputs untouched. Runtime acceptance should confirm the resulting specialist-only presentation on at least one BSS utility surface when convenient.
