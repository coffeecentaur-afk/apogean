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
