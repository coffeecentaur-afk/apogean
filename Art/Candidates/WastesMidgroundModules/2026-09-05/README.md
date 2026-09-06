# Wastes modular Mid — upper silhouettes only

Status: design discussion, not approved art, not an engine asset. Nothing here is loaded by the mod. Current camera validation remains in `Art/Validation/WastesLandscapeV1/2026-09-05-Flight/`.

`UpperSilhouettes-v2-CONCEPT-ONLY.png` depicts three original subjects: a broken highway on a hill, a quiet pipe mound, and an abandoned fuel station on a smaller rise. It illustrates modular subject choice, not final spacing, material fidelity, depth, resolution or in-game scale. Leave two or three quieter/open sections between major landmarks in the assembled landscape, not just this sheet's narrow spaces.

## Export audit

- Actual PNG dimensions: 1774 x 887; requested conceptual 512 x 256 / 4x presentation was not an exact export contract.
- Alpha audit: 1,573,538 opaque pixels, zero transparent pixels, zero partial-alpha pixels. The checkerboard is baked into the image. This fails the requested transparency requirement and must never be loaded as-is.
- Flat/shallow bottoms are not accepted. User explicitly pointed out that flight can expose them.
- Pixel quantization, palette limits, native-detail fidelity and module joins are unverified. Do not call this an HD runtime export or a native-asset preview.

## Next assembly contract

1. Use a stable world/region height reference, not a player-following height. Calibrate the Mid projection with the approved first-third-ascent dominance requirement before locking final asset dimensions.
2. Each occupied section includes its hill/ruin top, a deeper authored cliff face and compatible lower continuation sections where needed. End caps slope down or terminate as deliberate eroded cliffs, not straight cropped rectangles. Exposed pipes and restrained rock detail can break the silhouette; heavy sediment stays mainly in Close.
3. Joined groups share authored edge elevations/materials and continuation sockets. Stable layout selection persists through travel, reload and greenification. Restoration changes materials/vegetation on the same arrangement.
4. Open valleys remain genuinely transparent beside and between the deeper pieces. Far owns coverage through them. No full-width Mid fill, stretched last rows, mirrored lower-strata repetition or fade used to conceal an unfinished edge.
5. Show actual upper/cliff/continuation assemblies from ground, first-third ascent and high flight before installing. Validate both directions at joins and shallow/deeper descent too. A fixed anchor alone cannot prove that a bottom is hidden.
6. Runtime extraction must pass real alpha, source/screen scale, memory, shape and socket tests. Update geometry validators so intended Mid gaps are allowed while Far coverage remains required. A concept approval does not waive live fixture/routing gates.

## Provenance and generation prompt

Generated with the built-in image-generation tool, September 5, 2026. No external paid API fallback. The first draft was rejected for painterly shading and floating-island bases; this second draft remains a composition reference only. Original generated file: `exec-a77c4f08-afba-4cf4-b0b7-20994cab287a.png`, copied without alteration. Neither draft was installed.

Final submitted edit prompt (instructions, not claims that the output satisfied them):

> Edit this concept sheet. Keep the three subjects: broken highway on an eroded hill, quiet low pipe mound, abandoned gas station on a smaller hill. Preserve the genuinely transparent background and separated module idea. Correct the ART STYLE decisively: draw it as genuine, clearly quantized 16-bit side-scrolling pixel art, as if hand-pixeled at approximately 512 x 256 then displayed at 4x nearest neighbor. Deliberate chunky square pixel clusters, 24-color muted brown/slate/rust palette, 3 shading values per material, clean stepped silhouettes, readable masonry shapes, simplified coherent rock masses, NO noisy speckling, NO airbrush shading, NO realistic painting, NO smooth 3D render, NO isometric view. Strict straight-on side elevation; roofs/highway read as horizontal platform silhouettes, hills rise above the shared ground baseline. Shorten the downward soil fringes into shallow earth rims rather than pointy floating-island bases. Increase horizontal TRANSPARENT spacing substantially between modules; total transparent gaps about one third of the whole width. Nothing connects the modules: no full-width soil ribbon. Keep transparency below and above and between them. Original post-apocalyptic pixel-game terrain concept, no text, labels, watermark, interface, sky or checkerboard image.

The user's later underside correction supersedes the prompt's shallow-earth-rim request. Follow the assembly contract above for the next candidate.
