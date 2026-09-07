# One bounded Station style-bridge edit

Built-in image generation; candidate only. User likes garage design/contour but
prefers Station's coarser pixel shapes. Existing Station is now revisable, not an
immutable aesthetic target. Preserve both originals and all game assets.

## Exact prompt

Use case: style-transfer / precise-object-edit.
Asset type: ORIGINAL pixel-art Wastes midground gas station for a Terraria-scale 2D game, a bounded style-bridge candidate.

Image 1 is the EDIT TARGET: the existing gas station. Image 2 is a SUPPORTING REFERENCE ONLY: the garage with the preferred clean, cohesive, deliberately drawn outlines and convincing materials. Do not include a garage in the output.
Redraw the gas station from image 1. Preserve its basic identity, near-frontal view, lighting from upper left, red rusted canopy with two pumps underneath, small abandoned shop to the right with one door and a dark window, tall rusty roadside sign at right, connected sparse dead branches and amber grass, and the shallow exposed dirt/rock base. Keep composition, subject size relative to canvas, and soil datum close to the source. No new vehicles or people, logos, lettering, cables or unrelated props.

The key correction is a MIDDLE GROUND in pixel grain: larger, deliberately grouped pixel shapes like the old station, with the garage's crisp cohesive boundary and controlled material contours. NOT the garage's high density of fine one-pixel cracks. Give concrete 3-4 large coherent shadow/color planes. Draw a small number of chunky rust/chipped-paint patches and grounded rubble groups; do not fill every surface with tiny flecks. It is still dirty, damaged and abandoned, NOT pristine. Readable large wear shapes, not photorealistic detail. Avoid continuous bright outlines, pale fringes, detached single pixels, hairline grass, disconnected limbs, and the look of a softly rendered painting cut out with a rough mask. All limbs and canopy posts physically connect. Natural dark material-colored contours, not a sticker border.

Requested canvas 1024x920. Compose using a coarse logical 256x230 pixel grid, expanded 4x nearest-neighbor for delivery: final intended game study is 512x460 with roughly 2x2-pixel clusters. Soil lip around logical row170 / output y680; leave room below for the shallow ground crop, and generous transparent space above, as in the reference. Do not create extra fine detail merely because the delivery canvas is larger.
Pixel medium: hard square pixels, coherent warm umber/ochre/rust-red palette with restrained dusty beige highlights, no smooth gradients, no anti-aliasing, no blur, no bloom, no photographic materials, no dense dithering. Preserve the old station's recognizable arrangement; ONLY its drawing/pixel language and silhouette quality are revised.

TRANSPARENCY: deliver real alpha transparency in all empty space, including under the canopy, around branches, above and outside terrain. Do NOT draw a checkerboard, white/gray matte, colored backdrop, ground shadow outside the object, comparison board, labels or framing. Keep dark shop/window recesses opaque; the outdoors between structural pieces is transparent. Output exactly one standalone gas station cutout.
