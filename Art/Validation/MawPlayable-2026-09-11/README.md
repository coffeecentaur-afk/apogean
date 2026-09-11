# Maw playable-material preview — September 11

## Scope

The user accepted the harsh twelve-material artwork, then authorized a two-hour
autonomous verification window. This is the next integration probe, not another
art redesign or a claim that Maw world generation is finished.

Only gg / Apogee Native Visual V3 / single-player was used. Created ONCE at
X7156 Y360, 128x104 tiles in empty space. Original saved layout digest:
`9B29BC48D04974A2397997F8522F26AB55BE600BD90756BB8B5E83FB18BAFEB2`.
Never rebuild it. The separate inert material fixture at6276,420 also remains.

The opt-in `Build-CurrentQAPackage.ps1 -PackedMawPreview` build binds nine actual
Maw tile types and seven actual wall types to the existing approved texture
bank. Fibers, membrane and amber remain inert studies. 4,111 inspected actual
tile/wall cells share the SAME texture objects with that bank; no duplicated
246MiB texture set was added. Default builds do not enable the preview. This is
not full production memory/performance acceptance.

## Repairs found by the native tests

- Ordinary terrain/wall dust used AmberBolt: thirteen policy failures changed
  to matching native soil/mineral/sand/ice/snow/mud dust. Emissive organs stay
  separate. This tests the configured policy, not every dust shader.
- Legacy deep Gullet wall lacked two-stage purification. It now becomes Wastes
  dirt wall, then vanilla dirt wall; coatings remain intact.
- Legacy EngraftTurf lacked the grass/substrate framing declarations used by
  MawGrass. Those declarations now match.
- Maw soil/mud lacked engine material classifications. All atlas masks could
  pass while actual mixed neighbors chose exposed-edge frames. Baseline had
  eight excess-gap joins; soil-only repair left two mud joins. Dirt plus Mud
  classifications pass12/12 native base-mask comparisons. Removing both
  reproduces eight failures; removing Mud alone reproduces two. No blanket
  merge-to-everything rule and no texture repaint.

Native-verified repair/capture package:
`1751EED98544C573E09295E7A4C59CD9DAF22722E470149463656FD8CEB27CAD`.
Clean full build, zero warnings/errors; all466 pinned PNG/map bytes unchanged.

## Native evidence

At19:33:58 UTC the saved-world seam regression passes, including its two real
classification mutations and exact fixture restoration. At19:34:10 the loaded
matrix passes3,709 draws /949,504 tile alpha/key pixels /850 wall frames and10
paired full-grass/slope/soil cases. Authored cells match the original layout;
265 control-bank cells have allowed ordinary vanilla biological growth.
The original hash is retained, not reset to today's contents.

Earlier native property run after legacy repairs:449/449. It exercises actual
PlaceTile/PickTile, exactly one actual drop, solid and actuated collision,
58-reject/59-accept Mawstone mining, starter mining for the other tested ground,
purity and powder over six shapes, preserved wires/paint, eight wall families,
and protected constructed materials. Structural OssuaryBone has its separate
older proof; it is NOT counted in this449. These are native API tests, not
manual mouse, multiplayer or bomb simulations.

Native sand probe at19:13:34: removing its support creates the real falling
projectile, it moves down, then deposits exactly one MawSand tile after42 game
ticks. Its owned cells/projectile are cleaned up and the full starting fixture
digest matches. This is actual falling physics, not a registration flag check.
Final rerun records follow in the scoped JSON evidence.

At19:34:32 properties repeat449/449 on1751EED9. Sand repeats at19:35:29 with42
ticks and exact restoration. Eight real cell mutations pass at19:35:40.
`final-native-suite.json` records a subsequent whole six-request CLI pass:
matrix, properties, physics, seams, cell mutations, matrix again. Six isolated
sender-stub CLI controls prove stale-only logs, native rejection, missing sand
physics, blocked sender and existing evidence cannot return success.

Final orchestration hardening additionally requires `MAW LAB COMPLETE` after
synchronous restoration guards (sand also requires its later physics/restore).
Seven isolated CLI controls now include missing completion. Package
`4BB0544AC5F2AEA2CB7829038FADB5229FFC9D81626F9916B0F81E1B1ECAF7FD`
adds that marker only; no art, layout or material behavior changed. Its native
rerun is recorded separately from the earlier `final-native-suite.json`.

At19:46:07–19:46:11 UTC, the final build passes all six requests after an actual
save/client-restart/world-reopen. `final-reopened-completion-suite.json` contains
103 scoped records including completion markers and deliberate mutations.
449/449 properties,42-tick sand/exact cleanup,12 join comparisons,8 cell controls,
and both full matrix runs pass.333 native control growth cells are allowed;
the original authored layout/digest is unchanged. Final offline MawMaterials
gate passes24 atlases/148945920 pixels,1326272 actual runtime-map cases,
8 compiler controls,7 asset CLI controls and7 suite CLI controls.

All scoped logs, including the failed dust/legacy/seam trials, are in the sibling
`../MawNatural-2026-09-11/` folder. Evidence export refuses overwriting.

## Unaltered native captures

| File | Original game capture | SHA256 |
| --- | --- | --- |
| capture-camera-failure.png | Apogean MawPlayableLab 0 20260911-185249.png | 2EE4833877161E91C6FAA85ED556F04BD8F5875086937F9C138658B5E725B31C |
| playable-day.png | Apogean MawPlayableLab 0 20260911-190116.png | B5734A904275F4C3D8D7AC9806039DA7BF423DA755F19020DDF45AF7631E46AF |
| playable-night.png | Apogean MawPlayableLab 0 20260911-190221.png | 628DFD25B62B1E12782E6E6988D568D662AFD045BDEFB30940AFDE4BB913EA67 |
| playable-corners.png | Apogean MawPlayableLab 1 20260911-190419.png | 6EE7CE802F8249FB9181A7740A8E900AC21A5A89DEC4C411F6370F1EA8537187 |
| playable-day-classifications.png | Apogean MawPlayableLab 0 20260911-193638.png | 1663CBBA1AD28B5E5E1FA1BD88650AE30EB8DBCA57ECDD4D1D163CE53D4A4F25 |
| playable-corners-classifications.png | Apogean MawPlayableLab 1 20260911-193717.png | 8CE1E9B03C947B46DF3599D9E4706C4432E90BB805C01C2407F441362D07111F |

First capture rejected: player fell away during capture warmup, leaving an
incomplete lighting/camera cache. The view now holds the player at the panel,
then restores position/time on release. This renderer view is NOT traversal.
These four files predate the final dirt/mud classification fix; retain them as
comparisons. Native forest/resource-pack scenery here is NOT Maw background
proof. Grey test supports are NOT proposed Gullet safe ledges.

The two `-classifications` captures are the final repair, inspected unchanged.
Grass/soil support is continuous in the paired corner matrix. Whole-scene
inspection still sees small sky-colored openings at some mixed fiber/bone study
interfaces and diagonal joins; these are NOT cleared by the12 base-mask cases.
Carry this forward as an explicit remaining coverage test. No wholesale visual
approval. The capture image has a48px leftmost black strip over its upper464px;
the live window view does not show that border. Keep the raw capture and treat
the capture-boundary artifact separately, not as repaired terrain evidence.

## Still open

Resolve the observed mixed-study/diagonal openings; paint/echo/settings combinations,
manual placement/mining, actual bombs, multiplayer, runtime memory, natural
generation and full conversion/spread integration. No normal worlds modified.
No acid, mobs, bosses, new art/lore or broad worldgen added.

The historical grove cross-reload mismatch remains RED, unchanged at
`9D21A431C3E3FD8742B303DD28E3C994340E90D68F72969AA14F04BC20F2909F`.
Within-request guards pass; that does not resolve its older saved-hash failure.
Never rebuild or rebaseline it to hide the failure.

## Review and next run

Start with `playable-corners-classifications.png` for the previously reported
grass/soil corner issue, then `playable-day-classifications.png` for material
scale and the explicitly unfinished interfaces. The first four captures are
historical comparisons, not the final repair. These are actual game images.

For live inspection, load gg and Apogee Native Visual V3 in SINGLEPLAYER; run
`Tools/Request-MawNaturalValidation.ps1 -Playable -Case natural` or `corners`.
`release` restores camera/player/time. Do not use `build` on either saved fixture.
Only review in the disposable world while the packed QA preview is installed.
The next implementation should extend native coverage to inner corners and
fiber/bone/stone intersections, isolate their exact selected frame failures,
then repair only the responsible material rules. Do not reauthor the accepted
pixels or silently enable broad production worldgen while chasing a seam.
