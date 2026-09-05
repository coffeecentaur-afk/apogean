# A — Snapped v3 / slightly thicker branches

User approval on 2026-09-04: “can you make the branches a little thicker then it should be good to go”. This is the bounded revision of v2: exactly **one native pixel above and below** every branch column. Socket thickness increases from 7 to 9 pixels. Centerlines, length, fractured outer tips, smooth intact contours, paired native pivots and the five-color palette remain unchanged.

Trunk and top PNGs are byte-identical to v2, including quiet bark, knot and hollow details. The existing editable pixel-authoring source was adjusted; no new image generation or redesigned concept was needed. Source-image provenance remains in `../WastesSnappedA-v2/PROMPT.md`.

`Tools/New-WastesSnappedACandidate.ps1 -Revision 3` owns these candidate exports. Its trunk input is now frozen under `Art/Source/Trees/WastesSnappedA-native-trunk-input.png`, with the original SHA-256 pinned, so installing the new production texture cannot corrupt or invalidate the exporter input. Native sizes remain trunk 176×264, tops 246×82, paired branches 84×126.

`Native-comparison.png` and `Native-assembly.png` are offline actual-pixel assemblies. Live evidence is in `../../Validation/WastesSnappedA-v3/README.md`. Exact tested textures were installed in `Content/Tiles` after the bounded live grove check; broad production validation is not complete.

## Checksums

| Sheet | SHA-256 |
| --- | --- |
| Trunk | `B925D5D1BD7FFE4B1315E1D441B393D22CC1CAABBFF605F6DCD53EEBFE396433` |
| Tops | `75BED027B9541217BD04C44525B568B4C16194667A61CAA364929D33CA55F394` |
| Branches | `F7970EB6682CA9052FD0432D8B1C2C4B2D21DFED25B23C5E8FBB61F820360F19` |

## Reproduce static proof

```powershell
pwsh -File Tools/New-WastesSnappedACandidate.ps1 -Revision 3
pwsh -File Tools/Test-SnappedTreeRevision.ps1 -CandidateDirectory Art/Candidates/WastesSnappedA-v3 -ThicknessReferenceDirectory Art/Candidates/WastesSnappedA-v2
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Tree
```

The thickness regression also rejects an unchanged v2 supplied as both candidate and reference. Build candidate overrides can be staged in an isolated mirror using `Tools/Build-ApogeanIsolated.ps1 -TreeCandidateDirectory Art/Candidates/WastesSnappedA-v3`; this packages the mod for testing without rewriting repository Content assets. Loading that test package is reserved for the disposable world until reviewed.
