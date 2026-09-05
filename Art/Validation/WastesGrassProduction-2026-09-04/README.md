# Production Wastes grass/terrain checks — 2026-09-04

## Corrected test coverage

The old `GrassLabGallery` compared vanilla against `WastesGrassCandidate`/`WastesSoilCandidate`, which are different ModTile classes and assets from the installed terrain. In particular, the production grass registers `NeedsGrassFraming` and its custom dirt substrate. Results from the old candidate were not reliable evidence for the user's current corner complaint.

The gallery now uses actual `WastesGrass`, `WastesSoil` and `WastesGrassWallUnsafe` on the right, with identical vanilla Grass/Dirt/GrassUnsafe controls on the left. It adds soil beneath the half-block/four-slope strip and four separate full-grass + sloped-grass + underlying-soil junctions. Existing dense patches, flat caps and stair-step mounds remain. No terrain PNG or production terrain logic was changed during this pass.

## Evidence

- `Live-production-corners.png`: unedited Windows viewport, tModLoader 2026.7.3.0 / Terraria 1.4.4.9, 2560×1369 plus chrome. No white gaps or detached soil seams were observed in these sampled junctions. This is a bounded shape matrix, not proof of every possible terrain neighborhood. HUD overlaps part of the far-right fixture.
- `Native-corners-capture.png`: separate engine capture-camera output with the entire comparison visible at native tile scale. This also shows all four corner junctions without the HUD. Its simplified backdrop differs from ordinary gameplay; do not present it as a Windows viewport capture.
- `Live-production-properties.png` and `Native-properties-capture.png`: existing seven-material fixture (Soil, Grass, Stone, Sand, Ice, Snow, Mud), showing irregular tile fields, walls, supported paint/coating cells, half-block/slope probes, and standing water. Runtime registration assertions passed for solidity, drop mappings, neutral spread flags, sand/falling/ammo identity, and ice/snow identity. Those assertions do **not** simulate mining every block or firing a Sandgun. Paint/coating coverage remains a visual probe rather than an exhaustive color matrix.
- Actual Terrain gate passed all **14** tile/wall atlases, including exact native grass white-mask coordinates, plus reported-visual and surface regression scripts. White mask texels were not indiscriminately erased.

Live log: `grass` consumed at 22:04:48; capture at 22:04:51, water style 0. `wastes-properties` consumed at 22:06:23 after runtime assertions; capture at 22:06:26, water style 0. Both were run only in the backed-up disposable **Apogee Native Visual V3** world. See the tree Extended README for backup and successful save/quit details.

## Remaining boundary

Keep the wider Wastes terrain family **contracted**, with bounded live evidence, pending final corner/ground-cover visual review and the remaining actual interaction/paint/actuation/merge combinations. Do not promote the background family on the strength of the scenery behind these tests. No new world generation or user playthrough-world changes occurred.
