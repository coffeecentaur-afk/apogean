# Explicit-mask city candidate — not installed

User authorized building the deterministic mask exporter after recurring generated checkerboard failures. This derivative uses the EXISTING generated city; no new generation, software installation or paid API.

## Outputs and provenance

- City-Transparent.png:1586×992, SHA256 71C14F299210D278E1EB61B31EEBC9E341F2EF986E0C4D4159D0B3C2A41B785B.
- City-Mask.png: opaque black/remove, white/keep; SHA256 9220E7BB1AC8B0B4E4E4E3EB2C3F46555122F2312C81F56ECCF2EF596E6493D0.
- City-EdgeSource.png: color-prepared master, SHA256 53F0767B845A937B0AF3A7BCCE7C110CB4EB32C027B21769F89209E34B3E8198.
- Original ../City-Concept.png is unchanged; original generation prompt remains ../PROMPT.md.
-750,305 genuinely transparent pixels;823,007 retained opaque pixels; zero partial alpha and zero hidden RGB under transparent pixels.

The exporter preserves every kept pixel of the PREPARED master. It does not claim that master is unchanged from the original:6,926 pale edge-color pixels were replaced separately, recorded in edge-color-changes.json. The alpha mask was unchanged by this color step.

## Mask authoring and preparation

Tools/New-WastesCityMask.ps1 creates proposals from connected near-white pixels in the pinned source's upper570-row band. The top-connected sky and131 enclosed/disconnected components were inspected in the full dark preview and representative native facade crops, then selected in ../Mask-Decision.json. That selection is an editable agent-reviewed candidate, not user acceptance or pixel-perfect semantic certification.

This proposal depends on the city source and is not reusable as a global gray-removal rule. The reusable Export-MaskedBackground.ps1 accepts the explicit mask, not those thresholds. Future masks may be authored manually or with a different proposal method.

The initial mask-only dark preview exposed pale skyline/window rims. Color preparation is limited to kept neutral pale pixels within two pixels of the mask boundary, above row570, with donor colors from original kept art within radius6. No averaging, alpha erosion, global palette conversion or lower-earth changes. Donors and every modified coordinate are retained. Thin antennas remain in the mask; their edge colors changed, not their geometry.

Both final solid-backing previews were inspected. The continuous pale rim is substantially reduced and facade openings reveal the backing. Individual material choices and mask semantics still need art review; this is not a guarantee that every highlight is ideal.

## Reproduce

Use a NEW output directory:
```powershell
pwsh -NoProfile -File Tools/New-WastesCityMask.ps1 -OutputDirectory <new-directory> -DecisionPath Art/Candidates/WastesFarCity-v1/Mask-Decision.json
pwsh -NoProfile -File Tools/Export-MaskedBackground.ps1 -SourcePath <new-directory>/City-EdgeSource.png -MaskPath <new-directory>/City-Mask.png -OutputPath <new-directory>/City-Transparent.png -SourceSHA256 53F0767B845A937B0AF3A7BCCE7C110CB4EB32C027B21769F89209E34B3E8198 -MaskSHA256 9220E7BB1AC8B0B4E4E4E3EB2C3F46555122F2312C81F56ECCF2EF596E6493D0
pwsh -NoProfile -File Tools/Test-MaskedBackgroundExport.ps1
pwsh -NoProfile -File Tools/Test-WastesCityMask.ps1 -TestDefects
```

The independent city check verifies all6,926 donor records, permitted edge region, no interior/lower-earth/alpha changes during preparation, and every final PNG pixel. It rejects in-memory interior, alpha and sky mutations. Generic exporter evidence is in Tools/BackgroundMaskWorkflow.md.

A second complete recipe/export run in a separate temporary directory reproduced
the exact mask, prepared source and final PNG hashes. Verification copies remain
at C:/Users/max_h/AppData/Local/Temp/ApogeanCityMaskRepro-ce0f9207a0f548a3a10dbe36ea6017c1;
the canonical candidate is this directory. No user files were removed.

All21 authoring records and8 installed/Git skill mirrors pass their existing
checks. The background skill received a focused mask-workflow reference update.
Its generic metadata validator could not run because bundled Python lacks PyYAML;
frontmatter/invocation policy were unchanged, and that check is not claimed passed.

## Boundaries

Preview-Dark/Light and Edge-* images are OFFLINE source-pixel composites, not game screenshots. Their solid backing is only a transparency inspection aid; City-Transparent.png is the actual alpha asset.

The requested runtime canvas is still2048×1280. This1586×992 derivative was NOT enlarged to fake compliance. Composition, side-view suitability, full runtime extent, repeat joins, native live rendering and user approval remain pending. The original rejected generated export remains historical evidence. Accepted Station, foreground v2, renderer, Content assets and all worlds are unchanged.
