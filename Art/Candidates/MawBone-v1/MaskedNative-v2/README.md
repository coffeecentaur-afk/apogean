# Quieter bone — native-mask candidate v2

Status: **user approves this material; bounded native bone fixture passes**.
September9: installed through a pinned isolated QA override; production atlas,
world generation, mining rule and tooth behavior remain unchanged.13 native
API checks pass before/after save/reload, with actual day/night captures in
`Art/Validation/MawBone-2026-09-09/README.md`. Native user appearance review and
finished rib morphology are not implied. Supporting bone stays separate from
brittle tooth hazards. The following source/static observations remain valid.

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
in context. Native edges, four slopes, a half-block, paint, actuation and the
bone/Mawstone join now have a disposable fixture. This does not exhaust all
grass/stone merge permutations, settings, final ribs or native user review.

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

Next gate: separate brittle-tooth atlas and hazard fixture, then approved mouth
integration. Do not re-ask this material's offline approval or mistake the
flat material fixture for final morphology. No new full-biome/mob branch.
