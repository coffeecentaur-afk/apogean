# Landscape background resolution target

Research snapshot: 2026-09-04. Scope: resolution, rendering scale, and memory only. Recommendations below are engineering proposals, not third-party specifications or live visual approval.

## Finding

The measured references do **not** establish a standard of full-screen 1920×1080 or 2560×1440 source layers. Backgrounds o' Plenty predominantly uses 1024-wide landscape textures; HD Scenery includes one 2048-wide layer; Calamity's Astral renderer enlarges 1024-wide layers by approximately 2.5–2.7 times. Matching their texture dimensions and delivering one source pixel per display pixel are different targets.

For Apogean's requested native-pixel presentation, recommend original **3072×2048 authoring canvases per depth** for the shared 1440p/1080p set, with cropped runtime layers and explicit placement offsets where possible. A separate 1080p-only set can start at 2048×1536. These dimensions provide composition and camera margins; they are not dimensions claimed by the reference authors.

## Three primary references: measured dimensions and scale

1. **[HD Scenery — Just JK's Workshop page](https://steamcommunity.com/workshop/filedetails/?id=2892508247).** The author describes high-definition sky/celestial/liquid replacements and ocean-background changes. Verified installed metadata identifies Just JK and version 1.0. Reading all ten `Background*.png` headers found:

   | Installed image(s) | Actual pixels |
   |---|---:|
   | `Background_110`, `_111`, `_209`, `_210`, `_28` | 1024×600 each |
   | `Background_112` | 2048×434 |
   | `Background_113` | 1024×550 |
   | `Background_283` | 1024×1140 |
   | `Background_0`; `Background` | 48×1400; 8×2048 |

   The narrow strips must not be counted as panoramic detail. **Rendering scale: unverified for these individual slots in the user's runtime.** The author page supplies no numeric draw scale; PNG dimensions alone cannot establish it.

2. **[Backgrounds o' Plenty — Shashwambam's Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=2971754944).** The author describes a modernization of older Terraria backgrounds. Verified installed version 1.25 contains 173 `Background*.png` files: **98 are 1024 pixels wide**, 73 are 160 pixels wide, and two are 542×353 / 606×465. The 1024-wide group spans heights 227–1140; common sizes include 1024×533 (14 files), 1024×699 (11), and 1024×600 (6). Examples: `Background_61` is 1024×533, `_254` is 1024×699, and `_283` is 1024×1140. The author identifies `_283` as the beach replacement. **Rendering scale: not independently measured.** This is a resource-pack texture benchmark, not evidence of a custom native-pixel renderer.

3. **Calamity Astral — [official renderer](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Backgrounds/AstralSurfaceBGStyle.cs#L68), [official asset directory](https://github.com/CalamityTeam/CalamityModPublic/tree/1a8cebd27ec5615316b78f71973446b5528d2b78/Backgrounds).** Verified against commit `1a8cebd27ec5615316b78f71973446b5528d2b78`, including PNG headers and the asset requests in [SkyTextureRefs](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Skies/SkyTextureRefs.cs).

   | Astral layer | Actual pixels | Explicit draw scale | Horizontal parallax |
   |---|---:|---:|---:|
   | Horizon | 1024×435 | Delegated to engine; not verified here | — |
   | Far | 1024×492 | Delegated to engine; not verified here | — |
   | Middle | 1024×600 | 1.25 × 2 = **2.50** | 0.40 |
   | Close | 1024×700 | 1.31 × 2 = **2.62** | 0.43 |
   | Front | 1024×600 | 1.34 × 2 = **2.68** | 0.49 |

   Middle/Close/Front glow masks match their base dimensions. The renderer repeats textures using `screenWidth / scaledWidth + 2`. Calculated middle-layer coverage is 2560×1500 draw-space pixels, despite only 1024×600 source pixels. These scales precede any SpriteBatch or final-output transform; they are not measured physical-display ratios. This is a verified dynamic-parallax example, not a universal Terraria scale rule.

The informal names “HD Backgrounds” and “Beautiful Backgrounds” were not positively identified as exact Terraria product titles in this bounded search. HD Scenery is a plausible match for the former, not a confirmed alias. Promotional screenshots and desktop wallpapers were not used to infer asset dimensions.

## Native-pixel contract and practical sizes

**Recommendation:** keep the final source-to-output scale at 1.0 for both 1920×1080 and 2560×1440. Use the same shared assets at 1080p with a smaller visible region; fitting the entire 1440p composition into 1080p introduces resampling. A `SpriteBatch.Draw` scale of 1 alone is insufficient if the batch matrix, intermediate target, game zoom, or final presentation enlarges it. Verify the complete path at both resolutions.

| Native viewport | Suggested full layer canvas | Raw RGBA8 per layer | Three full layers | Two sets during fade |
|---|---:|---:|---:|---:|
| 1920×1080 only | 2048×1536 | 12 MiB | 36 MiB | 72 MiB |
| 2560×1440; shared with 1080p | 3072×2048 | 24 MiB | 72 MiB | 144 MiB |
| Wider repeat period, if needed | 4096×2048 | 32 MiB | 96 MiB | 192 MiB |

These are generous full canvases, not mandatory full-screen rectangles for every depth. A transparent distant silhouette can occupy a shorter image. Preserve its original coordinates when cropping. A width below the viewport can still cover it through repetition; choosing 3072 gives more room between repeated landmarks. Author seamless edges or compatible variants, and evaluate recognizable symmetry before using mirrored repeats.

Height depends on camera travel. For a fixed output band of height `B` and vertical layer displacement spanning `D`, plan at least `B + D` authored pixels, plus seam/filter margins. In a linear camera model, `D = abs(verticalParallax) × cameraTravel`. Thus 2048−1440 = 608 pixels is only a conservative full-screen displacement allowance, not proof of flight coverage. Horizon/sky ownership and layer placement can reduce the required band. A stretched last row supplies coverage but no new landscape detail.

## Memory and current Apogean constraints

Calculated uncompressed RGBA8 storage is `width × height × 4 / 1,048,576` MiB. Transparent pixels cost the same allocation as opaque pixels. PNG file size does not predict GPU storage. Full mip chains add approximately one third; glow masks, render targets, decoded CPU copies, and driver overhead are additional. One full-resolution RGBA8 render target costs 7.91 MiB at 1080p or 14.06 MiB at 1440p. Repeated draws reuse a texture allocation.

Read-only inspection found that [the current validator](Tools/Test-BackgroundHdContracts.ps1) caps each axis at 4096, each biome at **32 MiB**, and the complete library at **256 MiB**. All 27 current diagnostic layers total approximately **162.01 MiB**, with widths 1672–2161 and heights 728–941. These are snapshots of parallel work, not a claim that the assets are final.

The full-canvas options above exceed the existing per-biome cap. Two concrete paths for the authoring owner:

- **Retain current budgets:** export only authored depth bands and preserve offsets. For example, three 2048-wide layers of heights 768/1024/1280 total **24 MiB**; three 2560-wide layers of those heights total **30 MiB**. Coverage must justify these crops. At 3072-wide, the 32 MiB cap allows at most 2730 total rows across three RGBA8 layers. Nine 30 MiB sets would still exceed the 256 MiB library cap; 28 MiB per set leaves only 4 MiB for shared extras, so target closer to 24 MiB if all nine remain resident.
- **Retain full 3072×2048 layers:** budget **72 MiB** for three layers per biome and **144 MiB** for a two-biome fade, before extras. A provisional 96 MiB active-set / 192 MiB two-set allowance permits one additional full-size RGBA mask per set. This requires an intentional budget and residency-policy change: nine base sets alone would be **648 MiB**. Asset-cache ownership must actually release GPU resources; clearing a local lookup is not proof of unloading.

The [current custom renderer](Content/Backgrounds/HighDefinitionSurfaceBackgroundRenderer.cs) passes scale 1, repeats horizontally, and uses last-row continuation below the image. That establishes its local draw intent, not native output or adequate authored flight coverage. Following the background-authoring skill, acceptance should include 1× screenshots at both viewports, maximum flight altitude, at least 2.5 texture-width pans in both directions, and a two-biome fade with measured resident memory. No visual acceptance or production changes were performed for this research.

## Measurement provenance

Installed first-party pack files were read in place under `E:/SteamLibrary/steamapps/workshop/content/105600/{2892508247,2971754944}/`. Dimensions came from PNG IHDR bytes 16–23, not thumbnail sizes. Local metadata SHA-256: HD Scenery `BE238DFADA156789B8CCD49421874B44559D5C9C135AB08E3DECD7AFE0E8796A`; Backgrounds o' Plenty `875DB475A40AECA9A40B5AECCB966C3516649DA0A94CF9EACB51602D46B3B0D5`. Installed versions need not match the latest Workshop release.

Calamity image streams were read only far enough to obtain their 24-byte headers; no third-party artwork was saved, installed, copied into Apogean, or modified. Only this research file was written.
