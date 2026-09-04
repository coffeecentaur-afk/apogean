# Wastes tree: A — Snapped approved direction

Decision: 2026-09-04. User: “A is good.”

Reference: [deadwood study](2026-09-04-Wastes-Deadwood-Study.png). This board also contains B and C for comparison; **only A is approved**.

## Accepted visual direction

A narrow ordinary dead tree, with a jagged diagonal break, small subordinate snapped stubs, compact roots, and subdued brown bark. No leaves, branching antler crown, oversized root flare, or bright smooth crown pasted over a differently shaded trunk. Match material, grain scale, highlight direction and contrast across trunk, cap, branch and root.

This approves the direction, not the current installed terminal forks, and not every pixel of the generated board. B and C require separate approval. Maw trees are the next distinct biome family, not a recolor automatically approved by this decision.

## Implementation contract to carry forward

- Retain ModTree / TileID.Trees segmentation, independent chop points, native variable heights, acorn growth and ordinary Wood drops.
- Current atlas layout is trunk 176×264, branches 84×126 with 40×40 frames, and tops 246×82 with three 80×80 frames. Preserve the verified layout unless the exact installed renderer requires a documented change.
- The A cap must continue the trunk's silhouette and bark through a centered wind-safe attachment. Use wood-only pixels; suppress leaf effects.
- Keep native root cells compact and touching terrain. No whole-tree scale transform, separate root overlay, or repeated complete-tree stamp.
- Produce variations within A's accepted broken-wood language. The world-generation frequency of physically shortened trunks is unchanged in this documentation turn; A's broken-top appearance and an actually shortened tree are different concepts.
- The board is a 1536×1024 opaque review image, not a transparent atlas or measurable native-scale fixture. Do not crop it directly into production textures and declare success.

## Required next evidence

Show the authored component sheet and a native-scale assembled comparison against A before loading it for an in-game fixture. Then verify isolated/adjacent short and tall trees, wind, slopes, paint, growth, chopping at several heights, reload and multiplayer observation. Preserve previous mechanical evidence, but do not transfer its pass to new art without rechecking.

No sprite or game-code changes were made when recording this approval.
