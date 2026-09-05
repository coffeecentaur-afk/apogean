# A — Snapped / native candidate v1

Review only. **Not installed or loaded in tModLoader.** Start with `Native-comparison.png`: the middle panel uses actual candidate pixels at 1×, the right panel enlarges the same cap/joint pixels by 4×, and the left panel labels the resized approved concept. `Native-assembly.png` is the standalone 520×350 offline assembly. Neither is a live screenshot.

## Asset contract

| Role | PNG dimensions | Authored change |
| --- | --- | --- |
| Trunk/root/cut atlas | 176×264 | Byte-identical copy of the current segmented native-topology atlas. |
| Tops | 246×82 | Three 80×80 cells, 2px gutters; 16px-wide shafts with distinct jagged breaks and continuous trunk-material sockets. |
| Branches | 84×126 | Three paired 40×40 rows, 2px gutters; short woody stubs, with the right pivot six pixels below the left. |

The source sheet was generated using the approved A study and current trunk as references (`../../Source/Trees/WastesSnappedA-components-source-v1.png`). It returned RGB with a simulated pale checkerboard. The deterministic exporter strips that inspected backdrop, samples without smoothing, quantizes wood, and replaces over-detailed shaft material with the actual trunk pixels. It does not install the source board as an atlas. Exposed end grain and fracture silhouettes derive from the source; the shafts deliberately match the coarser native trunk. Sparse stubs replace broad terminal forks. B/C were not used.

`Tools/New-WastesSnappedACandidate.ps1` is the sole exporter for this candidate directory and has no promotion switch. It rebuilds the sheets, labeled atlas inspection, and comparison. Root/branch/cut frame roles and draw offsets were checked in the locally installed tModLoader assembly's `WorldGen.GrowTree` and `TileDrawing.DrawTrees`. The native samples use 6, 11 and 16 trunk cells from the existing growth range, plus a four-cell cut remainder. Current roughly-one-in-seven world-generation shortening is untouched. New source prompt/provenance: `PROMPT.md`.

## Validation and stop boundary

SHA-256 checkpoint: trunk `F472C977973DE49B95CA4B6B4ED923113061F2C2597DD4672798F05B4EFA60CF` (identical to untouched production trunk); tops `0124206D575497C4D519F5EB7462377141662A5934CE6A4DB031E14FE2553DCA`; branches `A71932C666339C0E1155983EC985A9E9AAB0FCC5BA21468CF55162BC9FB31028`.

Run the versioned `AgentSkills/tmodloader-tree-authoring/scripts/Test-TreeSet.ps1` against these three candidate PNGs, with the live-exported `Vanilla-ForestTree-Trunk.png` as `-TrunkReference`. Dimensions, hard alpha, palette, sparse mass, native trunk alpha and connected top sockets pass. The trunk is copied, not silently redrawn. The stronger validator's deliberate-defect regressions are recorded in `../../Validation/ValidatorRecovery-2026-09-04.md`.

User must review this actual assembly before installation. Then run a disposable live grove with wind, terrain anchors/slopes, paint, native multi-height chopping, acorn growth, reload and multiplayer observation. Offline geometry is not a substitute for these engine tests, and no previous candidate's visual pass transfers. The broader Wastes grass and background passes resume only after this bounded tree gate.
