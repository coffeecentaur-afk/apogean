# Native mixed-material join repair — September 11

This is a scoped repair, not blanket approval of all terrain interfaces.
The approved material pixels and both original saved galleries are unchanged.

## Result

Two reciprocal relations were missing: **OssuaryBone / Study_fibers** and
**OssuaryBone / Study_membrane**. `MawTerrainStudies.PostSetupContent` enables
only these relations, and only with the opt-in packed Maw QA preview.
No new atlas, opaque backing wall or all-to-all merge was introduced.

`MawMaterialJoinChecks` reproduces seven material pairs on an empty, restored
5x5 pad: two straight boundaries, four internal corners, both neighbor orders.
It reads native selected texture frames, not an atlas compared with itself.
Original / ChecksForMerge-only / pair-only / both / restored trials distinguish
the actual cause. Before the fix, each faulty relationship failed 12/12 cases;
ChecksForMerge alone did not fix it. Pair-only did. Removing either repaired
relation still reproduces 12/12 defects as an independent negative control.

Final package `52870164` passes all 420 trial comparisons, both negative controls,
and the existing six-request native suite. After normal rendering, 0/1599
fully enclosed tissue-family cells in the unchanged saved scene have transparent
base pixels. This does not audit every grass overlay or exposed silhouette.

## Exposed soil/stone contour is a different case

The exact saved neighborhood centered at local52,20 (world7208,380) includes
air, not a fully enclosed join. Copying its actual five-by-five layout into
the pad gives **76 transparent base pixels for both native Dirt/Stone and
Maw soil/stone**. The direct soil-pair trial changes Maw to64 while native stays76;
that is not evidence that the extra relation is correct, so it was not installed.
All12 simple soil/stone internal-corner comparisons also match the native control.

Earlier `join-exact-neighborhood.json` recorded156 instead of76 because its
diagnostic counted TileID0's base texture even for air. The final probe skips
air entirely. Keep the older record; do not cite its absolute count as ground
truth. No texture or saved terrain was edited to make these numbers agree.

## Evidence and safety

- `join-diagnosis-before.json`, `join-before.png`: retained pre-fix/red work.
- `join-fixed-suite.json`, `join-fixed.png`: narrow repair and raw native image.
- `join-reopened-suite.json`: repaired package after actual save/reopen.
- `join-soil-simple-corners.json`, `join-exact-neighborhood.json`: intermediate
  diagnosis, not additional fixes or visual acceptance.
- `join-final-package-suite.json`: current package, corrected air counting,
  actual saved-frame scan, 420 trials, missing-rule controls and six-request suite.
- `join-final-reopened.png`: unchanged whole-scene native capture after the
  fiber study was saved/reopened and both regression checks were rerun.

The raw gallery capture's old48px left-border artifact is retained rather than
cropped out; ordinary gameplay screenshots do not show that border defect.
The original playable checkpoint stays
`9B29BC48D04974A2397997F8522F26AB55BE600BD90756BB8B5E83FB18BAFEB2`.
Warm the scene with `Request-MawNaturalValidation.ps1 -Playable -Case natural`
before `-Case joins`: cold saved frames can still be zero before normal drawing.
Run the native suite afterward to verify cleanup. Never rebuild/rebaseline.

## Next independent failure

The new `../MawFiberRib-2026-09-11/` scene exposes small sky-colored openings
around **bone / actual soil and grass attachment edges**. These are NOT the
inert fiber/membrane pairs fixed above. Capture and compare those exact root
neighborhoods against native exposed contours before adding any relationship.
The three rough arcs also need anatomical silhouette work; no traversal or
world-generation approval follows from this repair.
