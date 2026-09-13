# Ordinary rope: actual item-use comparison

September13 bounded QA, not a new rope mechanic or production generator.
Previous U direct tile placement does not exercise player adjacency/reach;
V proves ascent only on preplaced rope. This comparison closes a separate gate.

Use idle gg in Apogee Native Visual V3, single-player, with an EXISTING ordinary
Rope stack in a hotbar slot. Do not grant items, alter measured timers, bypass normal
consumption, move/accelerate the player during measurement, or call private
placement methods. Selecting the existing stack and setup/teardown relocation
are explicit test setup. Successful tests consume ordinary QA rope; no refund.
Other inventory/loadout changes are failures, not silently overwritten.

Preflight an empty dry32x32 patch and12-tile halo using existing exclusions.
Build one small gray-brick floor, one support or wall when requested, but NEVER
preplace the target rope. At local(11,23), compare left gray brick, candidate
rib, candidate fiber cap, wall-only, diagonal-only, and fully unsupported targets.
A far target(28,23) has support but must remain beyond the recorded native reach.
All targets are initially empty, avoiding rope auto-extension's longer scan.

Run60 consecutive updates: initial neutral input, then ONE ordinary controlUseItem
pulse at tick1 while the target is empty. Supply the owned tile target only around normal
ItemCheck through public Pre/Post hooks, restoring the pre-hook static target
immediately. After the pulse, park the still-running normal item animation on
an independently empty/unsupported cell(2,2) inside the guard. Stop further use
as soon as native placement is observed; never aim at existing rope. Terraria
supplies animation, use timers, adjacency, reach,
placement and consumption. Native research must verify this hook order before
deployment. A scripted target is not manual mouse-play evidence.

Keep per-update hook/input/target/tile/stack/animation/timer/position evidence.
Positive cases require exactly one new rope cell and one consumed rope; negative
cases require neither. No collateral terrain changes; permit only nearby native
frame recalculation and verify logical tile/wall/shape/state. Restore exact
original native56x56 storage after success or failure. Preserve old galleries.
Record all other inventory and equipment fingerprints without restoring them.

Cancel before further input on context/pause/focus/death/capture/loadout/selected
item change, nearby foreign actors, damage, envelope departure or6seconds. Release
only this probe's owned controls and target override. Teardown restores the
captured selected slot and idle item timers after recording their measured final
values, so an aborted rope animation cannot run against the returned player's
original weapon. This reset is outside evidence, never used to accelerate use.
Preserve partial failures,
actual item deltas and cleanup result. Save only after cleanup is finished.

Test the hook seam with supported vanilla and unsupported controls first. If the
seam fails, retain the failure and diagnose the harness before blaming materials.
No untouched starter-baseline allowlist, art, difficulty or worldgen changes.
