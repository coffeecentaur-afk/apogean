# V: native preplaced-rope ascent

September13 01:25–01:37UTC. Separate native-created Classic **Maw QA Rope**,
**Apogee Native Visual V3**, single-player. No ordinary worlds or Plain baseline
changed. Installed candidate SHA256:
`52EF685882EA42BF289170B157874ABCAE110DCADD4DC191F75EF5E8B6EB96B8`.
Isolated mirror `3e39bf1a3cf24c2481a5c15c9185a170`, zero warnings/errors,
466 unchanged base pins plus seven anatomy additions and one cave-study texture.

## Actual results

| Raw report | Updates | Rise, px | Pulley / upward updates | HP | Outcome |
| --- | ---: | ---: | ---: | --- | --- |
| V-vanilla-1.json | 210 | 266.2201 | 210 / 81 | 80→80 | matched ascent pass |
| V-rib-1.json | 210 | 266.2201 | 210 / 81 | 58→58 | matched ascent pass |
| V-none-1.json | 210 | 0 | 0 / 0 | 56→56 | expected grounded control |
| V-focus-attempt-rib.json | 210 | 266.2201 | 210 / 81 | see raw | completed; NOT interruption proof |
| V-stop-rib.json | 31 | 74.72009 | 31 / 31 | 100→100 | explicit stop; incomplete retained |

The first vanilla/rib movement and input traces are identical excluding Life.
210-update samples take about3.5seconds. Setup creates20 ordinary rope cells,
one top support and a gray-brick landing floor in32×32 dry scratch at1396,60.
Native controls supply Up for180updates and neutral for30. Relocation/zero initial
velocity occurs only before sampling; native Terraria supplies all measured
motion and pulley acquisition. The no-rope control uses the same floor/rib.

All five runs restore exact native storage in the56×56 guard before serialization,
with unchanged nine known historical rectangles. Nine missing rectangles remain
unverified. No new items, equipment, health or movement powers were granted.
Ambient nighttime/graveyard spawn attacks BETWEEN trials explain varying start
health; they are not hidden damage inside the samples. Native screenshots were
too dark for an art verdict. No screenshot here is represented as visual proof.

A Steam focus-switch attempt did not interrupt the comparison. This is not proof
the focus guard works or fails: focus state was not independently recorded.
One explicit stop request does prove live cancellation and scratch cleanup after
31updates. Focus/pause/death/world-unload guards still need separate native trials.

## Persistence and independent replay

Three real normal saves each retain all16 existing canonical system digests
against the same pre-run copy. `V-save-tags.json`, `V-save2-tags.json` and
`V-save3-tags.json` preserve that comparison. Saved contour `6F7998FF…` and shallow
interaction `24163E65…` remain unchanged, with original creation identity retained.
`V-native-log*.json` are cumulative snapshots, not independent extra trials;
the last contains16 scoped records and zero matching caught render failures.

The five movement reports are unedited copies from native Captures files.
`Tools/Test-MawRopeClimbEvidence.ps1 -Path <reports>` independently recomputes
rise, pulley counts and verdicts, checks bounded inputs/positions/health/hashes,
rejects24 corrupt synthetic reports, and retains incomplete evidence. Synthetic
checker controls are not native movement. `Test-MawRopeClimbScope.ps1` passes525
policy/input checks; persistence's actual-hook matrix is240cases, dispatch's
actual-prefix test66checks/four omitted-release controls.

## Remaining gates

This proves attachment/ascent on **preplaced** rope, not actual player item-use
placement, reach/swing timing, top exit, whole-route returnability, hand-played
feel, hazard balance, multiplayer, final artwork or production generation.
The earlier U API/mining evidence remains in `../MawRope-2026-09-13/README.md`;
candidate rib/cap item bindings remain absent. No production safety rope added.
