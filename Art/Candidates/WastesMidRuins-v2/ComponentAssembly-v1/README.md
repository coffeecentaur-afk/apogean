# Depot component assembly — v1

Latest review: garage design/cutout liked, but the Station/garage pair is not
accepted for pixel-style cohesion. Station's broader grain is preferred while
its rougher cutout is criticized. One separate Station bridge candidate is in
`../../WastesStationBridge-v1/`; these source pixels remain unchanged. Technical
checks below are historical evidence, not a later art-acceptance verdict.

User direction: retain the earlier, cleaner depot BUILDING; use the rougher
redraw's TERRAIN. This selection does not certify a Station pixel-style match.
No new image generation, repainting, global grit filter or source edits.

## Exact assembly

- Building: `../PixelStyle-v1/Selected/MotorDepot-Upper.png`.
- Ground: `../PixelStyle-v2/GridReview/MotorDepot-Upper.png` (the latest shown
  fixed-2px resampling study, not freshly authored pixel detail).
- Both already registered at512x432 with approximate soil row340. Copy exact
  same-coordinate pixels. A stepped join at rows342–346 stays below the building
  footing and its grass fringe; no blending or rescaling.
- `Source-Selection.png`: red=earlier upper art, blue=rough ground, black=padding.
  This is a provenance diagnostic, not a runtime texture.
- `MotorDepot-Upper.png`:512x460 for the existing ground gallery. Rows432–459
  are **empty QA padding**. They are not authored foundation, and this asset is
  explicitly ineligible for ordinary Mid routing or aerial coverage.
- Candidate SHA256: `AD30DA51EDF8CAE32822AFF52D67B28733D1AC965AF185BC7D01A4E1DEAEF890`.
- The accepted Station, Close and Far remain hash-identical. Station's gallery
  crop is an exact1:1 reference, not resized to match the depot.

`Comparison-Light.png` and `Comparison-Dark.png` are OFFLINE1:1 boards, not game
captures. Their short lower crop is shown honestly rather than faking depth.
The independent audit proves source pixels and covered footing, not appearance.

## Reproduction and checks

```powershell
pwsh -NoProfile -File Tools/New-WastesDepotAssembly.ps1 -OutputDirectory <new-directory>
pwsh -NoProfile -File Tools/Test-WastesDepotAssembly.ps1 -CandidateDirectory Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1
```

All235520 output pixels and the complete selection mask are checked independently.
Actual CLI faults `BuildingChanged`, `SeamGap`, `PaddingLeak` fail respectively as
`BUILDING_PROVENANCE`, `TERRAIN_PROVENANCE`, `PADDING_LEAK`; mutations exist only in
decoded test copies. Reproduction first caught unordered JSON properties; the
generator now uses an ordered record. A fresh-directory repeat matches every
PNG and JSON byte. No failed check was silently counted as passing.

Isolated build passed with0 warnings/errors. Explicit build option
`-WastesDepotAssemblyDirectory` requires the unchanged scale-study audit first,
then this assembly's own pixel audit. It replaces only the depot in the temporary
QA gallery pack. Existing validation thresholds and repository Content stay
unchanged. Native result is recorded separately in the validation folder.

Remaining gates: combined art review, deep foundations, quiet seeded placement,
flight/lighting/biome/restoration coverage and production integration. Do not
regenerate the building just because those gates remain.
