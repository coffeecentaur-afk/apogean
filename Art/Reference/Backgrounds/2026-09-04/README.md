# Landscape review — 2026-09-04

Status: **composition approved; installation authorized, export pending**. The user approved these new backgrounds on 2026-09-04 and requested quality comparable to high-quality background mods. No image in this directory is yet used by the game. Built-in image generation produced these originals; full prompts are in [PROMPTS.md](PROMPTS.md).

The user temporarily put landscapes/backgrounds ahead of implementing the approved A — Snapped tree. Tree A remains approved but unimplemented; these landscape trees do not replace its native atlas.

## Concepts

| File | Actual pixels | Intent |
| --- | --- | --- |
| [Wastes surface](Wastes-Surface-Concept.png) | 1672 × 941 | Broken viaduct, abandoned utility stop, narrow snapped deadwood |
| [Restored forest](Restored-Forest-Concept.png) | 1672 × 941 | Same ruined geography reclaimed by grass, moss and trees |
| [Maw surface](Maw-Surface-Concept.png) | 1536 × 1024 | Feeding wound, fossil teeth, ochre fibers pulling at industrial ruins |
| [Maw cavern awake](Maw-Cavern-Awake-Concept.png) | 1672 × 941 | Ossuary ribs, swallowed mine hut, embedded amber lights |
| [Maw cavern dormant](Maw-Cavern-Dormant-Concept.png) | 1672 × 941 | Biological light recedes; cave and industrial remains stay |

All five are opaque RGB images, **not transparent parallax layers**. Total PNG storage: 10,622,947 bytes. No runtime texture-memory increase is attributable to these references; Art/* is excluded from the packaged mod.

## What works and what still fails

The Wastes/restored pair preserves recognizable road, station and mountain landmarks. Restoration does not rebuild the road. The cavern pair makes dormancy legible without deleting the environment. These are useful art-direction comparisons, not proven pixel-identical transition pairs.

The first Wastes output was too painterly; the retained refinement simplifies it, but fine noise and uneven effective pixel scale remain. Maw surface is too densely textured for direct gameplay use. The restored trees read more coniferous than the intended ordinary forest; revise before final art. The dormant cave has almost extinguished its amber, and should retain a small residual biological pulse in a later asset pass. No subjective visual approval is inferred from generation success.

## Before any installation

1. Composition feedback is now affirmative. Author real Far/Mid/Close layers with shared coordinates and transparent sky. Do not install the flattened master as one backdrop.
2. Remove baked sunset/sun/clouds from surface layers. Terraria owns sky, weather and time. Keep lighting out of geography.
3. The restoration and dormancy pairs need shared base geometry plus a separate vegetation/glow mask. AI-edited full-frame outputs must not be assumed pixel-aligned.
4. Author wider and vertically deeper coverage at the actual target resolution. These masters are smaller than a 2560×1440 viewport and cannot pass the native-detail target merely by upscaling.
5. Compose nonrepeating landmarks and authored joins; alternating mirrored roads and stretching a last row are not final solutions.
6. Cavern scenery uses a separate renderer contract; this large cave concept cannot simply be packed into Terraria's small repeating underground slots. Evaluate sparse tile/furniture landmarks versus a bounded backdrop layer.
7. Run the ground/flight/pan/noon/night/rain/eclipse/adjacent-biome matrix, check combat readability and measure residency before promotion.

## Current implementation boundary

The forum's first implemented reactive-scene behavior is **local forest restoration**, not per-block deforestation. The source uses cached nearby grass counts and falls back to Terraria's native forest after enough local restoration. See [validation record](../../../../FOREST_RESTORATION_VALIDATION.md).

Continuous percent-based art blending and a runtime Maw glow-mask transition remain pending. The new art is not installed; existing background defects, including row stretching and missing alternate compositions, are not claimed fixed by this pass.

## Export attempt after approval

[Measured resolution research](../../../../RESEARCH_BACKGROUND_RESOLUTION_TARGET.md) distinguishes actual pack dimensions from renderer scale. There is no single community “HD” size. Maintain the current memory budgets until a measured residency change is deliberately adopted.

The first far-layer export failed: requested 3072×1536, returned 1774×887 opaque RGB with a painted checkerboard. It is retained only under [Rejected](Rejected/README.md), excluded from the mod package. No bad export was installed and no lower-resolution enlargement was described as new detail. The read-only pre-import gate is `Tools/Test-SurfaceLayerExport.ps1`.
