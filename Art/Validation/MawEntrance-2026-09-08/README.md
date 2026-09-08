# Maw entrance audit — 2026-09-08

Status: structural-wall grid repaired and checked in native A3 and B2
surface/shallow views; bounded fixture pass, user review pending. Not an
accepted Maw scene. Wayfinder #26 tracks remaining work.

The user accepted the arrival divot (`it looks great continue`). The next bounded
slice inspects the existing major Maw entrance in disposable Arrival QA worlds.
No generation changes or replacement of approved pod/Wastes artwork in this audit.

## First red gate

`pwsh -NoProfile -File Tools/Test-TModLoaderAtlas.ps1 -Atlas Content/Walls/MawWallUnsafe.png -ReferenceAtlas Tools/Templates/TerrariaWallFrameMask.png -MaximumOpaqueColors 256`

Initial ExampleWall comparison: FAIL — alpha topology differs at 14,756 pixels.
That generic reference alone is insufficient: native wall styles legitimately
have different edge silhouettes. The decisive material-specific comparison uses
the installed `Vanilla-DirtUnsafe-Wall.png` export. Legacy structural atlas:
14,988 alpha mismatches; existing Maw dirt-wall atlas: zero mismatches, 468x180,
28,864 opaque pixels and five colors. No soft alpha or opaque white.
Native surface view in A3 reproduces the white/sky-colored grid across the membrane.
This is separate from the still-placeholder Maw scenery, bone appearance and
entrance geometry; those are not claimed fixed by an atlas check.

## Capture conditions

Player `gg`, single-player, `Apogee Arrival QA A3`. Diagnostic selects an existing
dry standing point, turns off the previous forced Forest bank and sets noon.
Normal game camera, lighting and biome routing; no terrain construction, framing,
flight hold or injected illumination. Exports survey the saved cells read-only.

Further results and artifact provenance will be recorded below.

## Focused repair and regression

`Tools/Test-MawStructuralWall.ps1` follows the actual class's literal texture
override (or conventional path if absent). It failed against the old runtime
binding with 14,988 alpha mismatches before the fix. The sole material change is
`MawWallUnsafe.Texture` -> the existing `MawDirtWallUnsafe` texture. Saved wall
identity, unsafe housing behavior, generation and collision stay unchanged.
The old PNG remains unused, retained as an explicit negative control. This
reuses a known native-layout Maw material; it does not author or approve final
fibrous membrane art, and no PNG was edited.

The runtime-path contract now passes. Supplying the legacy atlas must still
fail. Actual same-world screenshots after reload are required in addition.

Completed A3 replay: `A3-surface-before.png` / `A3-surface-after.png` and
`A3-shallow-before.png` / `A3-shallow-after.png` are unedited native captures.
Both after views remove the sky-colored grid and black square outlines. The
standing points match the original requests. `A3-surface-survey.json` and
`A3-after-survey.json` preserve identical collision and bone masks, plan hash,
3,349 structural-wall cells and full-route results. Ordinary liquid simulation
and enemy activity were not frozen; this is not a full-world byte comparison.

Installed package SHA256:
`4718C2EB7712198ED77484A14B80394DE860FF0D97022741A01FA3120A7B5AB4`.
Zero-warning/error isolated build. All410 Content PNGs match the accepted
previous3aec mirror; no generation files changed. The updated Texture binding
is code reuse, not an unreviewed new PNG or a world migration.

Static checks rerun: runtime-path structural-wall test PASS; explicit old-atlas
negative control FAIL as required; all14 Wastes terrain-atlas contracts PASS;
Status gate PASS (40 families, eight skill mirrors, generator ownership).
No claim that the broader legacy production gates passed.

## A3 baseline survey

Plan `04895B94`, mouth (3211,443), surface standing point (3210,443), shallow
standing point (3223,500). Survey: 3,349 structural-wall cells, 441 OssuaryBone
tiles and 1,065 liquid cells in the 241x181 rectangle (including surroundings).
The full-route existing validator passes, reaches all spine waypoints, reports
max fall111/120, Stomach clearance41, outlet plug85 and zero legacy acid tiles.
This is flood/path validation, not a fresh-character traversal playtest.

Native evidence exposes remaining problems: legacy bone reads as speckled
rectangles, the surface is a large arch/cavity, and the Maw still selects old
placeholder scenery. These are separate next gates, not hidden by the wall fix.

## B2 cross-check

`B2-surface-after.png` and `B2-shallow-after.png` are unedited native captures
in the second existing disposable world. Continuous structural wall without
the old grid at both views. `B2-survey.json`: plan A4B3108E, mouth (5438,485),
4,958 structural-wall cells, 727 liquid cells including surroundings. Full-route
validator passes all waypoints with maxfall97/120, Stomach clearance41, plug85
and zero legacy acid. No pre-fix B2 capture or B2 byte-equality claim.

Actual test points: surface feet(5441,476), shallow feet(5436,554).
After screenshots, both QA worlds were saved and exited; no regular worlds
were opened, altered or regenerated. Original world and player files remain
local. Only scoped survey data and allowlisted log lines are committed.

## Reproduce / next step

Build the same accepted candidate configuration documented in the handoff;
use gg in A3 or B2, then `Tools/Request-MawEntranceValidation.ps1 -Case surface`
or `-Case shallow`. Do not run the destructive older fixture/generator commands
to obtain this view. `-Case survey` exports without teleport/time changes.

Next: baseline the OssuaryBone atlas and its edge/merge contract; separate that
technical repair from an approved preview of true inward ribs and the mouth
silhouette. The current oversized arch, no native amber lighting, and old Maw
background are visible backlog, not accepted artwork. Broader wall conversion,
styles/zoom/lighting, performance and multiplayer remain open.
