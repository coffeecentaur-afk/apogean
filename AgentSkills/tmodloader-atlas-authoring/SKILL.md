---
name: tmodloader-atlas-authoring
description: Author, repair, or validate tModLoader tile and wall atlases, especially connected blocks, slopes, grass, merge seams, transparency masks, furniture framing, and Terraria-style pixel materials. Use when a mod tile renders as grids, rectangles, white seams, disconnected corners, wrong frames, or non-Terraria-looking blocks.
---

# tModLoader Atlas Authoring

Treat a tile sheet as renderer input, not a standalone illustration. Do not call an atlas complete because its PNG looks plausible outside Terraria.

## Required workflow

1. Read the project context, existing tile class, generator, and validation fixture.
2. Identify the exact engine contract: connected terrain, grass, wall, platform, or `TileObjectData` furniture. Do not reuse one contract for another.
3. Obtain a topology reference from the installed tModLoader/ExampleMod or a runtime export. Record its source and dimensions.
4. Add a fast failing contract before changing the asset. Assert dimensions, frame divisibility, alpha topology, forbidden opaque key colors, and palette budget.
5. Generate a diagnostic candidate. Never overwrite production assets during aesthetic iteration.
6. Build the mod and render a deterministic in-game fixture containing isolated, horizontal, vertical, corner, slope, half-block, merge, painted, and actuated cases as applicable.
7. Compare the screenshot at 1x gameplay scale. Reject visible 16-pixel graph paper, repeated-frame motifs, bright seams, floating edges, and details that only read when zoomed.
8. Promote the candidate only after static checks and the live fixture pass. Re-run the full visual suite.

## Authoring rules

- Preserve the reference atlas's padding and alpha topology unless the tile class deliberately implements custom drawing.
- Never preserve opaque white or magenta exporter keys merely because vanilla's specialized renderer understands them. Verify whether a `ModTile` consumes them; otherwise map or remove them.
- Connected materials should read as broad surfaces across multiple tiles. Avoid outlining every 16x16 frame.
- Put seams, bolts, cracks, and motifs in a minority of frames. Judge density in a room-sized fixture.
- Grass must be tested against its actual substrate and on all four slope directions. Copying `TileID.Sets.Grass` does not prove the visual merge.
- Furniture sheets must match `TileObjectData` width, height, coordinate heights, padding, style layout, and animation rows exactly.
- Use hard alpha for pixel art unless a renderer contract specifically requires translucency.

## Tools

Use `scripts/New-PaletteAtlas.ps1` to compile a native-topology palette candidate and `scripts/Test-Atlas.ps1` to reject structural mistakes. A project may keep wrappers with stricter content-specific thresholds.

Read [atlas-contracts.md](references/atlas-contracts.md) before introducing a new atlas family.
