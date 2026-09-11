# Atlas contracts

## Connected terrain and walls

- Start from the topology expected by the installed tModLoader version, not a remembered sheet size.
- Keep frame padding transparent and preserve the source alpha mask.
- Validate all neighbor combinations in-engine. Static alpha equality catches damaged frames but cannot prove `Main.tileMerge`, grass framing, or slope rendering.
- A repeated-cell score is advisory: connected materials need variation, but legitimate topology repeats some silhouettes.

## Grass

- Grass uses a larger and more specialized atlas than ordinary 288x270 terrain in current Terraria builds.
- Exporter-only colors can be meaningful to vanilla drawing code and meaningless to a `ModTile`. Any visible pure-white or magenta pixel is a failed fixture unless intentionally authored.
- Validate grass-to-substrate merges, exposed edges, corners, half blocks, and slopes separately.

## Native integration regressions

- Test full grass beside a sloped neighbor over the real substrate, in both orders and all four slopes. Isolated slopes do not cover this junction.
- Check the actual engine-selected frames at mixed-material joins. Equality with the alpha mask of the selected frame cannot establish that the engine selected the correct merge frame. Compare with native controls; distinguish base-mask metrics from complete draw/overlay evidence.
- Keep art-only specimens separate from playable tile proofs. Exercise native mining/drop, conversion and collision paths on the actual registered types. Falling-block claims require real update ticks from support loss through projectile identity to the returned tile, not just a registered flag or manually invoked AI.
- Saved biological control banks need narrowly scoped growth allowances. Replay the original layout and keep its creation digest; mutation-test allowed growth versus forbidden tile, wall, coating and geometry changes. Never refresh a failed baseline to the current world.
- Pin every accepted texture AND associated frame map before a QA package is installed. Validate missing/altered assets through the real build entrypoint. Large shared atlases should prove runtime texture reuse before production adoption.
- Preserve unedited native captures, including failed captures. An incomplete lighting/camera capture is not evidence that off-camera materials work; center the fixture or capture smaller panels and inspect again.

## Furniture

- Compute sheet width from object width, coordinate width, padding, style count, and horizontal/vertical style layout.
- Compute frame height from every `CoordinateHeights` entry plus padding. Do not assume all cells are 16 pixels tall.
- Animation frames stack using the registered frame height. Assert divisibility before loading.

## Evidence gate

An accepted asset has all four:

1. an authoritative topology reference;
2. a deterministic generator or checked-in source;
3. a passing static contract;
4. a dated in-game screenshot at useful zoom.
