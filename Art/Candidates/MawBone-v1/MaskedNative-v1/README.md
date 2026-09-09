# First original-color / native-mask bone candidate

**Static export passes; artwork is NOT approved and native rendering is pending.**
The repeat preview is too speckled and needs quieter grouped material before
promotion. Nothing here is installed or a substitute for the tooth fixture.

This is the background mask method with a different mask authority:

1. Original opaque material art, pinned in `../MaskedSource-v1`.
2. Explicit lossy preparation: nearest-center 64x64 sampling, nine-color fit,
   2x2 native color clusters, 64 patches and shared edge-color strips.
3. The locally exported native Stone alpha owns every frame's visibility,
   including padding and unused regions. None of its RGB is imported.
4. The EXISTING `Tools/Export-MaskedBackground.ps1` combines color and mask,
   unchanged. It preserves kept color pixels and zeros every excluded RGBA pixel.
5. Exact alpha, palette, frame diversity, source preservation and shared color
   endpoints are checked. `preview.png` assembles actual exported pixels, not
   an AI mockup. Its frame selections are diagnostic, not a world arrangement.

Output is 288x270: 16x16 frame bodies at18px pitch. There are44,104 opaque and
33,656 transparent pixels; zero partial-alpha pixels or native-mask mismatches.
Atlas SHA256:
`4E6E9E7EFA4C3F3C8B27E6614B6BADD34BB3AB6A66C4D4798C02ED6F959A7F9D`.
`recipe.json` records fitting and reference/compiler hashes;
`OssuaryBone-candidate.png.report.json` records the exact mask export.

Reproduce with PowerShell7 and the locally owned native atlas export:

```powershell
pwsh -NoProfile -File Tools/New-MaskedTerrainCandidate.ps1 -SourcePath Art/Candidates/MawBone-v1/MaskedSource-v1/material-source.png -ReferenceAtlas 'ABSOLUTE-PATH/Vanilla-Stone-Tile.png' -OutputDirectory 'ABSOLUTE-REPO/Art/Candidates/MawBone-v1/NEW-VERSION' -SourceSHA256 09DABAA17B0D4ABBFED3338B7BAB22361D9C6C36A330BDB73D69E0598B748221 -ReferenceSHA256 48907D0C61D9B68997C33FD25B0BAADB0A2D8276B6E759761C14E6B153C917EC
pwsh -NoProfile -File Tools/Test-MaskedTerrainCandidate.ps1 -ReferenceAtlas 'ABSOLUTE-PATH/Vanilla-Stone-Tile.png'
```

Eleven checks pass in `checks.json`: two end-to-end runs produce identical five
PNGs; evidence stays explicitly unapproved; CLI rejects input hash changes,
overwrite and production output; verifier rejects changed masks, kept colors,
color sockets and canvas; inputs and production atlas remain unchanged.
The separate existing mask-export suite also passes its two positive and11
negative controls. Its transparency implementation was not forked.

The new wrapper is deliberately a **bone-specific experiment**, not a universal
tile/wall/grass compiler. Different families need their own native framing,
merge and slope contracts. It creates only new candidate or bounded temporary
directories; an unsuccessful post-preflight run can leave partial diagnostic
files, never a claimed successful recipe. It does not build or modify Content.

Shared endpoints can still make a visible repeating motif. Alpha equality does
not prove good edge shading, correct engine frame selection, slopes, lighting,
merges or behavior. Next: simplify the fitted interior into calmer bone planes,
review actual native-sized art, then the small native bone/tooth fixture. Keep
production RED until that succeeds. Do not expand to every terrain family yet.

Primary framing reference:
[tModLoader Basic Tile](https://github.com/tModLoader/tModLoader/wiki/Basic-Tile).
