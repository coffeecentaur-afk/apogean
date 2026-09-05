# Rejected export — not installed

The landscape composition is user-approved. This **export** is not engine-ready.

Built-in image generation produced `Wastes-Far-Export-v1.png`; copied unchanged from
`C:/Users/max_h/.codex/generated_images/01a05af6-a783-7c71-95d1-747436f4fdbc/exec-2ba5d48b-f70b-4c03-ae67-b8f10d447ef6.png`.
SHA-256: `0065CBDA0704B7823EE3350389CF32034E08B7F71A13B13D4154EF4849A3EA4A`.

| Check | Result |
| --- | --- |
| Requested dimensions | 3072 × 1536 |
| Actual dimensions | 1774 × 887 |
| Pixel format | RGB, no alpha |
| Transparent pixels | 0 of 1,573,538 |
| Top two rows | All 3,548 pixels opaque |
| Differing horizontal edge rows | 836 |
| Image interpretation | The visible white/grey checkerboard is painted into the image |
| Decision | Reject import; no production texture or renderer changed |

`Tools/Test-SurfaceLayerExport.ps1` now checks every pixel's alpha and reports dimensions, hash, memory, sky/ground bands and outer-column differences. This candidate fails the default 2048×720 minimum and transparency requirements. The existing `ForestConceptV0_Far.png` passes the export checks (2161×728, real hard alpha, matching edge columns); that positive control is **not** a new art or live-camera approval.

No upscaling, color-key removal, image manipulation or API fallback has been performed. A separate permission question asks whether deterministic mask cleanup/export is acceptable. Even if approved, processing cannot invent resolution: native-detail and live camera checks remain required.

## Exact generation prompt

```text
Use case: background-extraction.
Asset type: Terraria-like pixel art SIDE VIEW 2D parallax background, FAR layer, real transparent PNG. Requested output canvas 3072 x 1536 pixels, native high detail, wide 2:1 ratio.
Input image 1 is the approved Wastes landscape composition and style reference, NOT a request to keep the foreground.
Generate ONLY the far depth: subdued ash-blue/grey distant mountains and modest ruined city silhouettes, one narrow damaged broadcast tower. Fill in terrain concealed by foreground in the reference. NO highway, NO station, NO trees, NO poles, NO people, NO lights or smoke. Remove the entire sky including sun and clouds: every sky pixel above the mountain contour must have actual alpha transparency, never a painted checkerboard or flat colored matte.
Terrain surface/ground datum lies at y=512, one third down the canvas. Mountain/ruin peaks lie y=100 through y=480. The entire region below y=512 down to y=1535 is opaque muted slate foothill/eroded rock with quiet but AUTHORED changing strata throughout, not a stretched row. Keep distant contrast modest. Left and right edges should be compatible continuous foothills, no landmark at edges, no duplicated or mirrored skyscrapers.
Style: high-quality hand-clustered pixel art, crisp edges, visually consistent 2-pixel clusters, 20-32 subdued terrain colors, no photorealism, no blur, no excessive noisy grit. Neutral daylight illumination for runtime tint. Keep the approved landscape's mountain language and post-war abandoned world. No text, labels, UI, borders, sun or clouds. Actual alpha sky is essential.
```
