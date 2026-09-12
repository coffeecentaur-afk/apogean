# Cortical rib surface v2 — separate candidate

September 11, 2026. Generated with the built-in image tool, not the API/CLI.
Source: `source.png`; reproducible native-mask bank: `Native-v1/recipe.json`.
No accepted terrain texture or v1 evidence is overwritten.

The v1 native comparison reads too much like horizontal sediment/wood grain.
This alternative asks for broader chalky cortex with sparse shallow pores.
The same compiler fits a 128×128 sixteen-color field and applies the exact
native Stone masks. The fiber-cap bank is unchanged from v1. The field itself
is NOT a Terraria atlas; only the compiled bank is suitable for the diagnostic.
Native testing and the user's art judgment are separate from mask correctness.

Reproduce without overwriting an existing output:

```powershell
pwsh -NoProfile -File Tools/New-MawAnatomyCandidate.ps1 -SourcePath Art/Candidates/MawRibSurface-v2/source.png -OutputDirectory <new absolute folder under Art/Candidates>
pwsh -NoProfile -File Tools/Test-MawAnatomyCandidate.ps1 -CandidateDirectory <that folder>
pwsh -NoProfile -File Tools/Test-MawAnatomyMutations.ps1 -CandidateDirectory <that folder>
```

The optional `MawAnatomyCandidateDirectory` QA-build argument selects a bank
without replacing the original candidate or rebuilding its saved geometry.
Ordinary builds do not register these materials. Neither version is final art.

## Generation prompt

Use case: stylized-concept. Asset type: original tileable pixel-art material FIELD for a Terraria-scale giant rib shaft in Apogean. A flat square close-up of a SINGLE continuous weathered cortical bone surface, filling every edge. This is texture material, NOT a pile of bones, NOT separate tiles, NOT a landscape, NOT a whole bone silhouette. Chalky muted ivory, warm grey, dark umber shallow chips; muted slate-brown shadows. Large readable pixel clusters with only 8–12 flat shades, drawn as if the source were 128x128 pixels enlarged with nearest neighbor. Most of the surface is broad unbroken smooth bone with sparse tiny shallow pores and short irregular chips. Subtle uneven mottling with no dominant horizontal or vertical grain. Very modest contrast inside the material; smooth cortex should remain one solid mass. Hard square pixels, no antialias, no photorealistic texture, no gradients, no blur, no bloom, no wood grain, no sediment layers, no cracks separating plates, no brickwork, no rows, no alternating light/dark stripes. No isolated bones or skulls. No transparent area, no border, no checkerboard, no labels or text. Even flat lighting across the full field, no center highlight or shaded edges. Seamless edges preferred. This should be a calm solid bone cortex usable on a continuous irregular hammered rib in a 2D game.
