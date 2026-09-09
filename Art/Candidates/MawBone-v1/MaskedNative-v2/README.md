# Quieter bone — native-mask candidate v2

Status: **static checks pass; actual-art review and native fixture pending**.
No production atlas, installed package, world generation, mining rule or tooth
behavior changed. Supporting bone remains separate from brittle tooth hazards.

Only the source artwork changed. The original-color compiler and explicit-mask
exporter were NOT changed to force this result. All288x270 native alpha remains
identical to v1 and the pinned local Stone reference:44,104 opaque pixels,
33,656 transparent pixels,111 contoured frame bodies,74 distinct populated
masks, no soft-alpha or visible white/exporter-key pixels. The fitted atlas
uses **five** colors from the existing nine-color allowance.

Atlas SHA256:
`5B4721D3A914EE5AEFD9D56AF95C0F883052BFB633005BC842E5CD101A5CF19E`.
Source SHA256:
`FE73348B438CFCE260AF7CEFE1771E1445E2EA06FE8C243269C3EEBC57C080FF`.
Exact preparation and cutout provenance: `recipe.json` and
`OssuaryBone-candidate.png.report.json`. Source and built-in edit prompt are in
`../MaskedSource-v2/`. Do not call fitting lossless or the large image an atlas.

`comparison.png` uses the same12x6 interior-frame selection on both versions,
at1x native texture resolution and2x nearest-neighbor enlargement. Both sides
are actual exported atlas pixels, not generated previews. It is explicitly
offline: no Terraria lighting, camera zoom, collision, contour framing or merge
simulation. `comparison.png.json` pins the inputs and selection rule.

Inspection: v2 has broader quiet material and less mottling than v1. It remains
a flat material repeat, not proof that finished structural ribs look convincing
in context. Native exterior shading, grass/stone merges, painted/actuated tiles,
all slopes and half-blocks still need their own disposable game fixture.
Do not treat a quieter picture or unchanged alpha as approval of those cases.

`Tools/Test-MaskedTerrainCandidate.ps1` now accepts a separately pinned source
without dropping its v1 default regression. Both source versions pass all11
checks, including two deterministic CLI exports and deliberately damaged masks,
pixels, sockets, dimensions, wrong input hashes, overwrite and unsafe output.
Current source-pinned evidence is in `checks.json`; v1 replay is retained as
`v1-regression-checks.json`. Inputs and production atlas are byte-identical.

Reproduce (PowerShell7, native export remains local):

```powershell
pwsh -NoProfile -File Tools/New-MaskedTerrainCandidate.ps1 -SourcePath Art/Candidates/MawBone-v1/MaskedSource-v2/material-source.png -ReferenceAtlas 'ABSOLUTE-PATH/Vanilla-Stone-Tile.png' -OutputDirectory 'ABSOLUTE-REPO/Art/Candidates/MawBone-v1/NEW-VERSION' -SourceSHA256 FE73348B438CFCE260AF7CEFE1771E1445E2EA06FE8C243269C3EEBC57C080FF -ReferenceSHA256 48907D0C61D9B68997C33FD25B0BAADB0A2D8276B6E759761C14E6B153C917EC
pwsh -NoProfile -File Tools/Test-MaskedTerrainCandidate.ps1 -ReferenceAtlas 'ABSOLUTE-PATH/Vanilla-Stone-Tile.png' -SourcePath Art/Candidates/MawBone-v1/MaskedSource-v2/material-source.png -SourceSHA256 FE73348B438CFCE260AF7CEFE1771E1445E2EA06FE8C243269C3EEBC57C080FF
pwsh -NoProfile -File Tools/Compare-MaskedBoneCandidates.ps1 -BeforeDirectory Art/Candidates/MawBone-v1/MaskedNative-v1 -AfterDirectory Art/Candidates/MawBone-v1/MaskedNative-v2 -OutputPath 'ABSOLUTE-REPO/Art/Candidates/MawBone-v1/NEW-COMPARISON.png'
```

Next gate: show this actual texture comparison; on acceptance move to one
small bone/tooth native fixture. No new concept batch, biome family or mob work.
