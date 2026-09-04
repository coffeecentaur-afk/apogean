# tModLoader 1.4.4 Visual Content Authoring Pipeline

Research date: 2026-09-04
Target: Terraria 1.4.4 / tModLoader `1.4.4` branch
Scope: connected terrain and grass, vanilla-like leafless trees, surface/underground backgrounds, and `ModNPC` sprites.

## Executive conclusion

The current Apogean visual problems cross four separate engine contracts:

1. Connected terrain uses frame-selected 16×16 cells with 2-pixel sheet padding. The sheet topology must match the framing algorithm.
2. Grass is a specialized terrain family. It must identify itself as grass and identify the dirt beneath it.
3. `ModTree` separates trunk, branches, and top art while reusing vanilla segmented-tree behavior.
4. Background and NPC APIs select and crop art; they do not guarantee complete screen coverage or readable art scale.

Therefore a PNG is not production-ready merely because its dimensions compile. Every asset family needs a canonical template, a static validator, a deterministic in-game fixture, and live screenshot/animation review.

## Sources and confidence

This report uses only:

- official [tModLoader API documentation](https://docs.tmodloader.net/docs/stable/);
- official [`1.4.4` tModLoader/ExampleMod source](https://github.com/tModLoader/tModLoader/tree/1.4.4/ExampleMod);
- the official public [Calamity Mod source mirror](https://github.com/CalamityTeam/CalamityModPublic/tree/1.4.4) for released custom-grass and custom-background examples.

Recommendations not guaranteed by the API are labeled as engineering recommendations.

---

## 1. Connected `ModTile` atlases

### 1.1 Native terrain contract

tModLoader distinguishes automatically framed 1×1 terrain tiles from frame-important furniture/multitiles. Terrain frames are recalculated when neighbors change. Frame-important tile coordinates are preserved. The official Basic Tile guide states that a normal drawable cell is 16×16 pixels with 2 pixels of padding to the right and bottom, producing an 18×18 pitch. It provides different templates for ordinary terrain, dirt-merging terrain, and eight-neighbor gemspark framing. See [Basic Tile: padding and terrain tiles](https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#terrain-or-framed-tiles).

The official ExampleMod dirt-merging tile is a concrete template:

- code: [ExampleBlock.cs](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Tiles/ExampleBlock.cs)
- atlas: [ExampleBlock.png](https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/ExampleMod/Content/Tiles/ExampleBlock.png)
- measured dimensions: 288×270, or 16 columns × 15 rows at an 18-pixel pitch

That size is canonical for this framing family, not universal. Eight-way outlined tiles use a different sheet. ExampleMod's gemspark tile calls `Framing.SelfFrame8Way`, returns `false` to suppress default framing, and registers the gemspark framing sets. See [ExampleGemsparkBlock.cs](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Tiles/ExampleGemsparkBlock.cs).

Custom merges require both participants to agree. ExampleMod sets `TileID.Sets.ChecksForMerge[Type]`, registers the neighboring tile's merge back to the custom tile, then uses `ModifyFrameMerge` and `WorldGen.TileMergeAttempt` to select custom merge frames. See [ExampleCustomFramingTile.cs](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Tiles/ExampleCustomFramingTile.cs) and [`ModTile.ModifyFrameMerge`](https://docs.tmodloader.net/docs/stable/class_mod_tile.html).

### 1.2 Slopes and half-blocks

`Tile.Slope` and `Tile.IsHalfBlock` are tile state. `TileFrameX` and `TileFrameY` identify the selected source cell. Drawing derives a cropped rectangle and offset from this data. See the official [`Tile` documentation](https://docs.tmodloader.net/docs/stable/struct_tile.html) and [`TileDrawInfo`](https://docs.tmodloader.net/docs/stable/class_tile_draw_info.html).

Practical consequences:

- A terrain frame that may be sloped must contain intentional pixels across its full 16×16 drawable cell. Transparent holes or a white matte become visible when cropped.
- The 2-pixel separators are gutters, not drawable art. Bleeding into them can sample neighboring cells and create seams.
- Native slope drawing should be the default. Custom drawing should not compensate for an atlas using the wrong topology.
- For gemspark-style smooth-border framing, ExampleMod enables `TileID.Sets.AllBlocksWithSmoothBordersToResolveHalfBlockIssue`, specifically to avoid half-block visual problems. This does not repair malformed source art.
- Validation must include every slope direction, half blocks, actuated blocks, isolated tiles, straight runs, inner/outer corners, and every merge neighbor.

### 1.3 Grass is not a recolored dirt atlas

The official API exposes `TileID.Sets.Grass`, `TileID.Sets.NeedsGrassFraming`, `TileID.Sets.NeedsGrassFramingDirt`, and `TileID.Sets.Conversion.Grass`. See [`TileID.Sets`](https://docs.tmodloader.net/docs/stable/class_tile_i_d_1_1_sets.html) and [`TileID.Sets.Conversion`](https://docs.tmodloader.net/docs/stable/class_tile_i_d_1_1_sets_1_1_conversion.html).

Calamity's released Astral Grass provides a production example. It:

- registers as solid grass and convertible grass;
- assigns Astral Dirt to `NeedsGrassFramingDirt`;
- registers merges with its dirt and vanilla grass families;
- changes back to Astral Dirt on a failed mining hit;
- uses a dedicated grass sheet and page-based visual variation.

See [AstralGrass.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Tiles/Astral/AstralGrass.cs) and [AstralGrass.png](https://raw.githubusercontent.com/CalamityTeam/CalamityModPublic/1.4.4/Tiles/Astral/AstralGrass.png). The measured grass sheet is 576×396; its code treats each variation page as 288 pixels wide. This confirms that its grass topology is distinct from the 288×270 dirt-merging template.

Baseline registration for an Apogean grass/dirt pair should follow this shape:

```csharp
Main.tileSolid[Type] = true;
Main.tileBlockLight[Type] = true;
TileID.Sets.Grass[Type] = true;
TileID.Sets.Conversion.Grass[Type] = true;
TileID.Sets.NeedsGrassFraming[Type] = true;
TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<WasteDirt>();
RegisterItemDrop(ModContent.ItemType<WasteDirtItem>());
```

The merge relationship and conversion behavior must also be registered. If mining peels grass into dirt, that transition must be implemented deliberately.

### 1.4 Required atlas pipeline

Each atlas should begin with a manifest:

```text
kind: terrain | dirt-merge | eight-way | grass | furniture
cell: 16x16
padding: 2
columns/rows: canonical-template values
variation-pages: N
merge-targets: [...]
supports-slopes: true/false
supports-half-blocks: true/false
```

Authoring and test sequence:

1. Copy the topology matching the code path, not a similarly sized image.
2. Paint inside each 16×16 cell with nearest-neighbor tools and preserve all gutters.
3. Keep terrain alpha binary unless transparency is intentional.
4. Validate dimensions, pitch, gutter contamination, forbidden matte colors, required frame coverage, duplicate pages, and palette limits.
5. Generate an in-game gallery containing all adjacency, merge, slope, and half-block states.
6. Capture at 100% game/UI zoom in daylight, darkness, and representative paint/coating states.
7. Promote only after the live gallery contains no seams, white corners, or obvious repeated frame cells.

This would have caught the reported Waste Grass corners and repeating Helix cells before world generation.

---

## 2. Vanilla-like leafless `ModTree`

### 2.1 Engine behavior

A `ModTree` shares vanilla tree tile ID 5. Soil selects the registered tree, so converting soil can convert tree style. The API separates trunk, branch, and top textures and exposes soil, sapling, wood drop, dust, acorn, shake, and leaf-gore behavior. See [`ModTree`](https://docs.tmodloader.net/docs/stable/class_mod_tree.html) and [ExampleTree.cs](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Tiles/Plants/ExampleTree.cs).

Using `ModTree` rather than a large furniture tile preserves segmented trunks, natural height variation, axe chopping, and falling-tree behavior. `DropWood()` controls the bulk wood drop.

### 2.2 Canonical asset shapes

Measured from official ExampleMod 1.4.4 assets:

- trunk: 176×264 — [ExampleTree.png](https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/ExampleMod/Content/Tiles/Plants/ExampleTree.png)
- branches: 84×126 — [ExampleTree_Branches.png](https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/ExampleMod/Content/Tiles/Plants/ExampleTree_Branches.png)
- tops: 246×82 — [ExampleTree_Tops.png](https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/ExampleMod/Content/Tiles/Plants/ExampleTree_Tops.png)

The API documents top frames as 80×80 by default and branch frames as fixed 40×40. The sheets therefore use a 2-pixel separator: three 82-pixel top columns and 42-pixel branch cells. See [`ModTree.SetTreeFoliageSettings`](https://docs.tmodloader.net/docs/stable/class_mod_tree.html).

### 2.3 Correct leafless implementation

For “a normal Terraria tree, but dead and leafless”:

- Keep the official trunk atlas topology and draw a conventional narrow segmented trunk.
- Draw bark-only left/right limbs in the branch atlas.
- Draw bare fork/crown silhouettes in the top atlas. Do not include leaf clusters. Do not leave the whole top empty unless a blunt one-tile trunk ending is desired.
- Leave `TreeLeaf()` at its default `-1` (or explicitly return `-1`) to prevent leaf gore.
- Return `false` from `CanDropAcorn()` if dead trees should not drop acorns.
- Return `TreeTypes.None` from `CountsAsTreeType` if they should not be shakeable; the API documents that value as preventing tree shaking.
- Set `GrowsOnTileId = [ModContent.TileType<WasteGrass>()];` using the actual Apogean soil type, following ExampleMod 1.4.4.

> tModLoader ships monthly API revisions while still targeting Terraria 1.4.4. If a future build changes an API member, use the installed assembly/XML documentation and matching ExampleMod snapshot rather than an older copied snippet.

### 2.4 Tree acceptance fixture

A deterministic grove must demonstrate:

- at least six natural height/branch combinations;
- no leaf pixels, leaf gore, or acorn drops;
- roots visually contacting flat, sloped, and uneven soil;
- chopping the base destroys the whole tree and drops intended wood;
- chopping a middle segment behaves like a vanilla tree, not a static prop becoming shorter;
- branches and tops remain attached during wind motion;
- no oversized identical crown repeated across the grove.

---

## 3. Surface and underground backgrounds

### 3.1 Style selection

`ModBiome.IsBiomeActive(Player)` determines whether a biome scene effect participates. Priority and, among equal-priority effects, weight determine which effect wins. A biome exposes `SurfaceBackgroundStyle` and `UndergroundBackgroundStyle`. See [`ModBiome`](https://docs.tmodloader.net/docs/stable/class_mod_biome.html) and [`ModSceneEffect`](https://docs.tmodloader.net/docs/stable/class_mod_scene_effect.html).

For a world-wide Waste plus vanilla sub-biomes, both a generic Waste effect and a Waste-Jungle effect may be active. If their activation, priority, or weight is ambiguous, the Forest background can win in Jungle. That is a selection bug rather than an art bug.

Recommended routing:

- Make Waste Forest, Jungle, Desert, Snow, etc. mutually testable.
- Include depth checks so surface and underground styles cannot claim the same camera position.
- Give specific sub-biomes deterministic precedence over generic Waste.
- Alternatively centralize replacement with `GlobalBackgroundStyle.ChooseSurfaceBackgroundStyle` and `ChooseUndergroundBackgroundStyle`. See [`GlobalBackgroundStyle`](https://docs.tmodloader.net/docs/stable/class_global_background_style.html).
- Add diagnostic telemetry for active biome, winning slot, priority, weight, texture paths, camera tile position, and vanilla zone flags.

### 3.2 Native surface layers

`ModSurfaceBackgroundStyle` supplies far, middle, and close texture selection. `ModifyFarFades` fades the active style toward 1 and other styles toward 0. `ChooseCloseTexture` can modify close-layer scale and parallax. `PreDrawCloseBackground` can replace native close drawing by returning `false`. See [`ModSurfaceBackgroundStyle`](https://docs.tmodloader.net/docs/stable/class_mod_surface_background_style.html) and [ExampleSurfaceBackgroundStyle.cs](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs).

Prefer native far/middle/close slots where possible. They retain Terraria's normal transitions and parallax. The art still needs a complete layer composition: the far layer supplies horizon/sky, while middle/close layers need transparent silhouettes and intentional lower-edge continuation.

### 3.3 Custom high-resolution coverage

Calamity's Astral Desert is an official example of custom close-background drawing. Its `PreDrawCloseBackground`:

- derives vertical position from camera Y, `Main.worldSurface`, screen offsets, and scale;
- uses different parallax and scale values for layers;
- calculates repeating X from camera position;
- draws enough horizontal copies to cover the screen;
- gates the draw by surface altitude;
- returns `false`, taking responsibility for native close drawing.

See [AstralDesertSurfaceBGStyle.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Backgrounds/AstralDesertSurfaceBGStyle.cs).

The API does not extend a custom panorama beyond its source rectangle. Once custom drawing takes over, vertical gaps must be prevented explicitly.

Custom renderer rules (**engineering recommendation**):

1. Draw an opaque sky/far fill across the full viewport before transparent panorama layers.
2. Derive layer Y from a world-surface anchor and camera Y, not a fixed screen percentage.
3. Repeat each layer from before the left edge through after the right edge.
4. Give each layer an opaque/tileable lower continuation. If needed, draw a matching continuation strip from the layer's baseline to screen bottom.
5. Do not vertically stretch pixel art; use a repeatable strip or separate fill layer.
6. Define an upper altitude transition to deliberate sky/space art instead of exposing a panorama edge.
7. Test every supported resolution and zoom.

Required captures: ground level, 30 tiles above, 80 tiles above, maximum normal flight, the upper style cutoff, daytime, midnight, rain, and eclipse. Any exposed source-image edge fails.

### 3.4 Underground layers

`ModUndergroundBackgroundStyle.FillTextureArray` fills four slots:

0. ground/sky border;
1. ground layer;
2. ground/rock border;
3. rock layer.

The API documents border images as 160×16 and layer images as 160×96, with the rightmost 32 pixels apparently duplicating the leftmost 32 for seamless tiling. See [`ModUndergroundBackgroundStyle`](https://docs.tmodloader.net/docs/stable/class_mod_underground_background_style.html) and [ExampleUndergroundBackgroundStyle.cs](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs).

Each intended underground biome needs a complete four-texture set or an explicit inheritance decision. Test all depth transitions separately for Waste Forest, Jungle, Snow, Desert, Mushroom, evil biomes, Hallow, Maw, and Underworld.

---

## 4. `ModNPC` scale, frames, hitboxes, and readability

### 4.1 Engine contract

`NPC.width` and `NPC.height` are the gameplay hitbox in pixels; the texture need not have identical dimensions. `NPC.frame` is the source rectangle drawn, `NPC.scale` changes visual scale, and `spriteDirection` controls orientation. See [`NPC`](https://docs.tmodloader.net/docs/stable/class_n_p_c.html).

For custom animation:

- set `Main.npcFrameCount[Type]` in `SetStaticDefaults`;
- implement `FindFrame(int frameHeight)`;
- set `NPC.frame.Y` to frame index × `frameHeight`;
- update `NPC.frameCounter` for timing;
- set `NPC.spriteDirection` consistently with movement;
- use `NPCID.Sets.NPCBestiaryDrawModifiers` to correct bestiary presentation without changing world rendering.

See [`ModNPC.FindFrame`](https://docs.tmodloader.net/docs/stable/class_mod_n_p_c.html), [ExampleCustomAISlimeNPC.cs](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/NPCs/ExampleCustomAISlimeNPC.cs), and [`NPCBestiaryDrawModifiers`](https://docs.tmodloader.net/docs/stable/struct_n_p_c_i_d_1_1_sets_1_1_n_p_c_bestiary_draw_modifiers.html).

ExampleMod's custom slime uses a 36×228 sheet, six frames, 36×36 visible art, and 2 pixels of vertical padding per frame, yielding a 38-pixel frame pitch. Its hitbox is 36×36. This demonstrates deliberate coordination among sheet size, frame count, visible art, and hitbox.

### 4.2 Readability is a project gate

tModLoader specifies no minimum hostile-enemy sprite size. “Too small” is therefore not an API error; the project must reject it.

Recommended Apogean starting classes (**engineering recommendation**):

- Tiny ambient critter: below 24 pixels only when intentionally nonthreatening.
- Small hostile flier/Mothling: 36–48 pixels on the dominant axis; fit the hitbox to the body, not wing tips.
- Hound-sized ground enemy: 56–80 pixels long and 32–48 pixels tall, with a stable foot baseline.
- Elite: visibly larger than the player's gameplay silhouette in at least one dimension.

These are starting points, not engine rules. Acceptance requires live comparison with the player and representative vanilla enemies at normal zoom.

Do not use `NPC.scale` as the production fix for undersized source art. It enlarges the same limited pixel information. Redraw at the intended native size and then fit the hitbox.

### 4.3 NPC validator and fixture

For every NPC sheet, check:

- `imageHeight % frameCount == 0`;
- constant frame pitch and intended padding;
- no opaque pixels crossing frame boundaries;
- stable body/head/eye anchors between frames;
- stable foot baseline for grounded states;
- disconnected alpha islands are intentional parts, not broken outlines;
- art-facing direction agrees with `spriteDirection`;
- visible bounds reasonably contain the hitbox without large invisible damage zones;
- smallest frame remains readable against bright sky, dark caves, snow, soil, and interiors;
- bestiary position and scale are deliberate.

The live fixture must show idle, movement, attack, hit-stun, death, both directions, partial tile occlusion, and side-by-side player/vanilla references. Use animation capture, not only still images.

---

## 5. Apogean quality gates

### Static gate

Fail when:

- atlas dimensions violate the declared pitch/page layout;
- a gutter contains unexpected opaque pixels;
- terrain contains forbidden white/magenta/checkerboard matte pixels;
- grass lacks its paired dirt or grass/conversion registration;
- tree assets violate trunk/branch/top topology;
- NPC sheet height does not divide by frame count;
- an NPC/item violates its project size class without an exemption.

### Deterministic gallery gate

Maintain generated fixtures for:

- terrain adjacency, merges, slopes, half blocks, paint, and actuation;
- every grass/dirt boundary shape;
- tree growth, variation, wind, and chopping;
- each surface biome at multiple altitudes/times;
- all four underground slots for every biome/depth pair;
- NPC scale and animation beside the player.

### Runtime telemetry gate

For background failures, record winning biome/effect, priority, weight, style slot, texture paths, camera position, depth, and zone flags. This distinguishes “wrong image selected” from “right image drawn incorrectly.”

### Live review gate

Capture from the running tModLoader client at 100% scale. Standalone mockups and enlarged screenshots cannot prove frame topology, style selection, camera coverage, chopping, collision, or gameplay readability.

## 6. Recommended implementation order

1. Build the atlas manifest and static validator.
2. Prove one plain Waste dirt/stone family across all adjacency/slope states.
3. Build Waste Grass as true grass paired with Waste Dirt and prove every boundary.
4. Replace current trees with one native leafless `ModTree` and prove chopping/variation.
5. Instrument style selection; fix Jungle routing and surface vertical coverage.
6. Complete underground four-slot sets.
7. Establish NPC size classes; repair Mothling and Hound before adding more enemies.
8. Apply proven terrain/furniture contracts to faction structures instead of using one generic repeating tile for walls, trim, glass, floors, and machines.

This order proves each renderer independently before world generation combines them.

## 7. Primary source index

- [tModLoader Basic Tile guide](https://github.com/tModLoader/tModLoader/wiki/Basic-Tile)
- [`ModTile`](https://docs.tmodloader.net/docs/stable/class_mod_tile.html)
- [`Tile`](https://docs.tmodloader.net/docs/stable/struct_tile.html)
- [`TileDrawInfo`](https://docs.tmodloader.net/docs/stable/class_tile_draw_info.html)
- [`TileID.Sets`](https://docs.tmodloader.net/docs/stable/class_tile_i_d_1_1_sets.html)
- [`TileID.Sets.Conversion`](https://docs.tmodloader.net/docs/stable/class_tile_i_d_1_1_sets_1_1_conversion.html)
- [ExampleBlock](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Tiles/ExampleBlock.cs)
- [ExampleGemsparkBlock](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Tiles/ExampleGemsparkBlock.cs)
- [ExampleCustomFramingTile](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Tiles/ExampleCustomFramingTile.cs)
- [Calamity AstralGrass](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Tiles/Astral/AstralGrass.cs)
- [`ModTree`](https://docs.tmodloader.net/docs/stable/class_mod_tree.html)
- [ExampleTree](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Tiles/Plants/ExampleTree.cs)
- [`ModBiome`](https://docs.tmodloader.net/docs/stable/class_mod_biome.html)
- [`ModSceneEffect`](https://docs.tmodloader.net/docs/stable/class_mod_scene_effect.html)
- [`GlobalBackgroundStyle`](https://docs.tmodloader.net/docs/stable/class_global_background_style.html)
- [`ModSurfaceBackgroundStyle`](https://docs.tmodloader.net/docs/stable/class_mod_surface_background_style.html)
- [`ModUndergroundBackgroundStyle`](https://docs.tmodloader.net/docs/stable/class_mod_underground_background_style.html)
- [ExampleMod surface background](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs)
- [ExampleMod underground background](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs)
- [Calamity Astral Desert background](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Backgrounds/AstralDesertSurfaceBGStyle.cs)
- [`ModNPC`](https://docs.tmodloader.net/docs/stable/class_mod_n_p_c.html)
- [`NPC`](https://docs.tmodloader.net/docs/stable/class_n_p_c.html)
- [ExampleMod custom AI slime](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/NPCs/ExampleCustomAISlimeNPC.cs)
- [`NPCBestiaryDrawModifiers`](https://docs.tmodloader.net/docs/stable/struct_n_p_c_i_d_1_1_sets_1_1_n_p_c_bestiary_draw_modifiers.html)
