---
name: tmodloader-tree-authoring
description: Author, repair, or validate tModLoader ModTree assets, growth, bases, branches, tops, saplings, chopping, spacing, and Terraria-scale tree variation. Use when custom trees float, overlap, repeat, keep unwanted foliage, shrink when mined, or fail to behave like vanilla trees.
---

# tModLoader Tree Authoring

A tree is an engine-composed organism: trunk atlas, branch atlas, top atlas, sapling, growth rules, terrain anchor, and chop behavior. Judge the assembled tree, never one PNG.

## Workflow

1. Read the `ModTree`, sapling/global tile hooks, world-generation placement, source generator, and latest live screenshot.
2. Export or locate the exact vanilla tree assets from the installed Terraria build. Record the tModLoader target, dimensions, alpha topology, and drawing role before drawing. Stable `ModTree` expects 40×40 branch textures and defaults to 80×80 tops; verify when the installed target changes.
3. Write a tree contract: substrate, trunk width, height range, branch frequency, crown role, base footprint, palette, drop, growth, and nearest vanilla silhouette.
4. Version a candidate asset set. Preserve the vanilla atlas topology unless custom drawing is a deliberate requirement.
5. Run `scripts/Test-TreeSet.ps1`; a passing PNG is only permission to render.
6. Build a deterministic grove containing short, medium, tall, mirrored, sloped, painted, tightly spaced, and isolated trees. Include a naturally grown sapling and world-generated trees.
7. Test axe hits at the base and middle, plus shake, paint, growth, reload, and a multiplayer client observing the same result. A normal tree remains inside Terraria's tree logic; it does not swap between hand-authored whole-tree sprites.
8. Inspect at ordinary zoom in day and night. Promote only when bases touch terrain, neighboring silhouettes stay legible, variation is structural, and every chop/growth contract passes.

## Visual contracts

- Default to Terraria's segmented trunk behavior. Whole-tree multitiles require an explicit gameplay reason.
- A leafless tree uses wood-only branch and top frames. Recolored leaf clusters are still foliage.
- Every top frame needs a deep, centered, trunk-width socket at its bottom-center wind anchor. Validate it both statically and at multiple nonzero wind frames.
- Keep the physical base near vanilla trunk width. Large roots belong to rare set pieces, not every tree.
- The base's lowest opaque pixels meet the tile surface. Use one draw-offset source; do not compensate in art, `SetDrawPositions`, and a global overlay.
- Variation comes from height, branch placement, branch choice, and sparse top choices—not repeated mirroring of one complete silhouette.
- Apply minimum spacing in world generation, then validate natural regrowth too.
- Keep optional root overlays disabled until their anchor and mining lifecycle have a dedicated fixture.
- Keep `GrowsOnTileId`, sapling style, wood/acorn drops, and foliage framing in one reviewed contract. A soil/sapling disagreement fails even if world generation can force-place the tree.

Read [tree-contracts.md](references/tree-contracts.md) before adding a new tree family.
