# Motor depot pixel-language correction

Built-in image generation, one style-transfer edit. Target: `../ScaleStudy-v3/MotorDepot-Upper.png`.
Style reference only: `../ScaleStudy-v3/Station-Upper.png`. Original output is retained
as `MotorDepot-original.png`; no accepted asset is overwritten.

## Exact prompt

Use case: style-transfer.
Image 1 is the EDIT TARGET: the motor depot upper-module study.
Image 2 is a STYLE REFERENCE ONLY: the accepted gas station. Do not redraw, include, or modify the gas station.
Redraw the motor depot as hand-authored, deliberately clustered Terraria-compatible pixel art matching the gas station's material and pixel grain. This is an artistic redraw, NOT a photo/pixelation filter. Replace its realistically gritty tiny texture with distinct broad color shapes, readable chipped concrete blocks, grouped rust patches and decisive stepped shading. Simple 3-4 tone ramps per material, roughly 24-32 coherent colors overall; no micro-speckle, noise, tiny random cracks, blur, dithering or smooth gradients. Match the gas station's 2-4 screen-pixel clusters and slightly exaggerated readable details, warm muted sandstone, charcoal, rusty red, amber dry grass. Keep upper-left lighting.
Output a single isolated depot on genuine transparent alpha. No black/white/checkerboard background; no text, title, labels, border, shadow on a backdrop, or other buildings.
Canvas: 1024 by 928 pixels, authored as an exact 2x nearest-neighbor presentation of a 512 by 464 logical-pixel sprite. Each logical pixel occupies a solid 2x2 output block. This doubled canvas is only transport; final game size will remain the SAME 512x460 crop as target, not a larger building.
Preserve the target layout at exactly twice its coordinates: roofline around output y450, building ground/soil datum y680; leave its large empty upper area empty. Maintain the same building width, height, viewpoint, short cylindrical roof vent, collapsed roof at right, dark two-bay garage, bent shutter at left, right doorway, small hazard plates, and cliff support with drainpipe. Preserve object placement and silhouette proportions. Do not zoom or fit-to-fill. The garage doors must not shrink or grow. Reinterpret texture on both building AND lower cliff in the reference's clear chunky pixel style. Sparse intentional highlights, no white rim. Keep the broken roof visibly broken but simplify rubble into larger readable groups. No leaves, no new props, no added floors, no photorealistic or 3D render look.
