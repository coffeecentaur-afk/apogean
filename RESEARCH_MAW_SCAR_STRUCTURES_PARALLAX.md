# Maw Scar, Authored Structures, and Parallax Research

Status: primary-source technical report; no implementation changes

Target repository: Apogean for tModLoader

## Source baseline and evidence labels

This report uses the locally installed/current tModLoader `stable` source at commit [`666f69962d3bdffde54fc14025f02634965b4e7c`](https://github.com/tModLoader/tModLoader/tree/666f69962d3bdffde54fc14025f02634965b4e7c), tagged locally as `v2026.07.2.6`. First-party implementation references are pinned to Remnants [`9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389`](https://github.com/lazy-wombat/Remnants/tree/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389), Calamity [`1a8cebd27ec5615316b78f71973446b5528d2b78`](https://github.com/CalamityTeam/CalamityModPublic/tree/1a8cebd27ec5615316b78f71973446b5528d2b78), and Terraria Overhaul [`e202f4719bdff845035e352ba70169b7022cbe09`](https://github.com/Mirsario/TerrariaOverhaul/tree/e202f4719bdff845035e352ba70169b7022cbe09).

Labels used throughout:

- **Proven:** directly visible in the owning tModLoader API/source, official tModLoader documentation/ExampleMod, Terraria source patches, or the cited first-party mod source.
- **Inference:** follows from proven control flow or data layout but is not guaranteed as a public API contract.
- **Recommendation:** an Apogean design or implementation decision proposed from the evidence.

## Executive findings

1. **Surface backgrounds cross-fade whole style slots; they do not spatially stitch biomes.** tModLoader draws each active modded far and middle style with its slot's far alpha, and the close style with its front alpha. A style can fully replace its close-layer draw through `PreDrawCloseBackground`, but there is no matching custom-draw hook for only the far or middle layer. A true world-positioned seam is therefore feasible for the close/foreground layer, while far/middle should remain full-screen cross-fades unless Apogean assumes responsibility for a larger custom renderer. [tModLoader background loaders](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L195-L340) [surface background API](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L31-L84)
2. **Underground backgrounds are atomic four-texture styles with a very short engine transition.** Slots 0 and 2 are vertical depth borders, not horizontal biome-border masks. The safe solution is stable target selection, a spatial transition band, and hysteresis; a long alpha blend requires custom rendering or a fragile engine hook. [underground background API](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L13-L29) [Terraria background selection patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L6333-L6396)
3. **The Maw scar must be planned before it is painted.** It should be one deterministic world feature represented by a saved centerline/envelope, with hard no-write masks for spawn, Dungeon, Temple, containers, tile entities, and all Apogean future reservations. `StructureMap` is cooperative world-generation protection, not a permanent runtime registry and not proof that every vanilla feature is protected. [official `StructureMap` source documentation](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/WorldBuilding/StructureMap.cs.patch#L7-L48) [official world-generation guide](https://github.com/tModLoader/tModLoader/wiki/World-Generation#structuremap)
4. **Large buildings should be procedural layouts assembled from authored tile modules.** Reserve the whole envelope first, then place terrain/shell, room modules, connectors, furniture, containers, tile entities, wires, and liquids in controlled phases. Remnants demonstrates procedural room graphs populated by authored modules; Calamity demonstrates a self-owned schematic format with explicit frame, wall, liquid, wire, container, and tile-entity handling. [Remnants mineshaft generation](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L82-L180) [Calamity schematic metadata](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/CalamitySchematicIO.cs#L17-L76) [Calamity schematic placement](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L187-L291)
5. **Pixel-art assets must be authored to the engine's sheet grammar, not scaled from concept art.** Standard terrain and wall templates, `TileObjectData` cell geometry, and the strict underground four-file sizes are engine-facing contracts. Surface panorama sizes are not fixed, but every variant in a cross-fading family should share dimensions, baseline, repeat seam, scale, and parallax. [official Basic Tile guide](https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#padding) [ExampleMod surface style](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs) [ExampleMod underground style](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs)

## 1. Surface far, middle, close, fading, and biome seams

### 1.1 Selection and priority

**Proven:** A `ModSurfaceBackgroundStyle` is normally supplied by a `ModBiome` scene effect. tModLoader reads the local player's selected scene effect and its `SceneEffectPriority`; the Terraria selector gives modded styles insertion points around vanilla Ocean, Mushroom, Desert, evil, Hallow, Jungle, and Snow checks. Every `GlobalBackgroundStyle.ChooseSurfaceBackgroundStyle` hook then receives the final integer by reference and can replace it without priority arbitration. [surface loader selection](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L143-L180) [Terraria surface selector patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L7167-L7211) [global background hooks](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L86-L125)

**Recommendation:** Concentrated Engraft should be an explicit `ModBiome` style at a deliberate scene priority. The ruined-world baseline may remain a global fallback, but that hook must preserve another mod's selected style outside an active Apogean biome. A global hook that always overwrites the result can erase third-party backgrounds regardless of their scene priority.

### 1.2 What actually fades

**Proven:** tModLoader resizes both `Main.bgAlphaFrontLayer` and `Main.bgAlphaFarBackLayer` for modded style slots. The far and middle renderers iterate every registered style, skip zero alpha, and draw using `bgAlphaFarBackLayer[style.Slot]`. The close renderer uses `bgAlphaFrontLayer[style]`, initializes scale/parallax, and repeats the selected texture across the screen. [background alpha arrays and draw loops](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L152-L157) [far/middle draw](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L195-L279) [close draw](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L281-L340)

**Proven:** The current Terraria patch updates vanilla back/front visibility, then calls the modded method named `ModifyFarFades` with `bgAlphaFrontLayer`. This does not match the method's documentation, which describes far-background transparency. Copying ExampleMod's fade loop therefore advances the front alpha again on this exact source revision while far/middle continue reading the other array. [current call site](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L7145-L7154) [documented hook](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L43-L46) [ExampleMod fade loop](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs#L5-L23)

**Recommendation:** For the pinned tModLoader target, Apogean should version-test this behavior and make `ModifyFarFades` a no-op unless a future tModLoader version passes the documented far array. The engine's own style-slot transition should own the fade. Do not silently depend on this discrepancy: guard it with a regression test when changing tModLoader versions.

### 1.3 Why variants still snap

**Proven:** `ChooseFarTexture`, `ChooseMiddleTexture`, and `ChooseCloseTexture` are queried during drawing. Alpha belongs to the style slot, not to the texture returned by that slot. [surface background API](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L48-L83) [draw-time texture queries](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L207-L278)

**Inference:** If Apogean changes a biome variant, day/night file, or landmark set behind an already opaque style slot, the new texture appears at the existing alpha. No old texture slot remains to fade out. This is a same-slot snap, even though changing between two different style slots cross-fades correctly.

**Recommendation:** Give independently transitioning landmark sets separate style slots. Keep day/night, weather, ash, and emissions as color/overlay states when possible so the geometry and repeat phase do not change.

### 1.4 Is a stitched custom foreground feasible?

| Technique | Proven capability | Limitation | Apogean use |
|---|---|---|---|
| Whole-style cross-fade | Far, middle, and close layers can overlap as style-slot alphas change. | It is a full-screen dissolve, not a seam at a world X coordinate. | Default transition between complete biome panoramas. |
| Dedicated transition style | Another normal style can provide `Far/Mid/Close` art for a boundary band. | Still full-screen; it creates a designed bridge, not literal left/right stitching. | Low-risk option for broad ruined-biome boundaries. |
| `PreDrawCloseBackground` compositor | A style may draw the close background itself and return `false` to suppress tModLoader's close draw. [API](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L63-L69) | Only the close layer has this supported escape hatch. Apogean must reproduce tiling, parallax, alpha, clipping, resolution handling, and draw ordering. | Feasible for a world-anchored foreground seam, roots, ruined walls, or silhouette occluders. |
| `CustomSky`/full custom renderer | Can draw multiple depth ranges and maintain its own intensity. | Larger ownership surface; must coexist with clouds, event skies, other mods, and the normal surface renderer. | Reserve for Underworld composition or truly dynamic special scenes. |
| Foreground world tiles/walls | Spatially exact and naturally tied to the biome border. | Not a panorama layer; can affect collision/building if implemented as solid tiles. | Preferred for literal tendrils, dead roots, rubble, fences, and seam landmarks. |

**Inference:** A close-layer stitch is technically feasible by calculating a world-space scar boundary, converting it through the close layer's parallax transform, and drawing left/right repeatable strips with clipping or an authored mask. It is not available as a built-in “stitch two textures” call.

**Recommendation:** Use normal style-slot cross-fades for far and middle. Build a single reusable close-layer compositor only if foreground seam art materially improves the transition. Keep the seam's world-space landmark in tiles/walls so the player can see and trust where the biome actually begins.

## 2. Underground four-texture styles and abrupt switching

### 2.1 The four slots are vertical depth layers

**Proven:** `ModUndergroundBackgroundStyle.FillTextureArray` supplies exactly four documented slots: sky/ground border, ground field, ground/rock border, and rock field. The border textures are 160×16; the field textures are 160×96; the documentation notes that the rightmost 32 pixels of the field are a duplicate of the left edge for wrapping. [underground API contract](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L13-L29) [official ExampleMod implementation](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs)

**Proven:** Terraria selects one integer `undergroundBackground`, remembers the previous integer in `oldUndergroundBackground`, fills texture arrays for the current and old styles, and uses its private `ugBackTransition` path when the integer changes. tModLoader exposes no underground equivalent of `ModifyFarFades` and no custom underground-layer draw hook. [underground selection/fill patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L6333-L6396) [underground loader](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L98-L140)

**Inference:** The two 160×16 border files cannot create a horizontal biome seam; they are selected by camera depth inside the current style. Replacing them with left/right blend strips would break the dirt-to-cavern transition without solving the biome border.

### 2.2 Safe transition options

1. **Recommendation — stable target resolver:** Compute the intended underground style once per client update from the local player's/camera's location. Do not let `FillTextureArray` or a draw-time texture query mutate transition state.
2. **Recommendation — hysteresis:** Enter a biome only after crossing an inner boundary and leave only after crossing a wider outer boundary. Start with an 8–16 tile dead band and 0.15–0.30 seconds of dwell, then tune by movement-speed playtests. These are Apogean values, not engine constants.
3. **Recommendation — spatial transition band:** Author a neutral cave seam style for a 16–32 tile border band. Its ground and rock horizons must match both neighboring sets. Crossing becomes A → seam → B instead of A → B, reducing landmark shock even if each engine transition is brief.
4. **Recommendation — matching geometry:** All styles in a transition family should use the same 160×96 repeat phase, 32-pixel wrap strip, horizon height, border silhouette, and palette-value range. Change landmarks and materials, not the geometry expected to line up.
5. **Recommendation — preserve unsupported micro-biomes:** Until Apogean has authored Mushroom, Dungeon, Temple, Granite, Marble, Spider, and other intentional sets, leave the selected vanilla/modded underground style unchanged rather than falling back to ruined Forest.
6. **High-risk option:** A longer true alpha fade requires a custom compositor or an IL/detour of Terraria's underground renderer. There is no supported per-layer alpha hook in the reviewed API, so this should not be part of the first production slice.

**Proven implementation reference:** Remnants promotes Marble, Granite, and Beehive into `BiomeHigh` mod biomes with dedicated four-slot underground styles. This proves that an intentionally authored micro-biome can own a full underground panorama; it does not add a new transition mechanism. [Remnants biome declarations](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/Biomes/Biomes.cs#L13-L84) [Remnants Marble style](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/Biomes/Backgrounds/marblecave.cs)

## 3. A continuous authored Engraft/Maw scar

### 3.1 Current Apogean hazard

**Proven—current repo:** `EngraftSystem` inserts immediately after the pass named `Jungle`, reads `Main.spawnTileX`, chooses one surface X, and paints an ellipse only around the surface. It does not consult `GenVars.structures`, reserve a corridor, or persist a scar envelope. [`Content/World/EngraftSystem.cs`](Content/World/EngraftSystem.cs#L46-L113)

**Proven:** The official world-generation guide lists assignment of `Main.spawnTileX/Y` at the later “Spawn Point” event. The public `ModifyWorldGenTasks` documentation also emphasizes that pass placement determines whether later terrain work cuts structures. [official world-generation timeline](https://github.com/tModLoader/tModLoader/wiki/World-Generation#vanilla-world-generation-timeline) [`ModSystem.ModifyWorldGenTasks`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L397-L424)

**Inference:** Reading the final spawn immediately after `Jungle` is not a reliable spawn-safety contract. The current pass can be far from the intended location by accident, and the surface ellipse cannot express the planned vertical scar or protect later content.

### 3.2 Required world model

**Recommendation:** Generate one deterministic `ApogeanWorldPlan` before any Apogean destructive pass. Save the resolved results, not just the random seed:

```text
ApogeanWorldPlan
├── SpawnSanctuary rectangle
├── MawScar
│   ├── centerline waypoints from surface to Underworld
│   ├── core/shell/fringe radii by depth
│   ├── surface mouth rectangle
│   └── Underworld terminus rectangle
├── Corporation territory envelopes
├── Major authored structure envelopes
└── Reserved expansion sockets / future arenas
```

`StructureMap` exists only as cooperative generation-time state. The resolved rectangles and scar metadata should also be copied into a compact, versioned `ProtectedRegionRegistry` saved through `ModSystem.SaveWorldData` and synchronized through `NetSend/NetReceive`, so runtime Engraft spread, post-boss ore, and later structure retrofits can obey the same exclusions. tModLoader documents world save/load and world-data synchronization as paired hooks. [`ModSystem` world persistence hooks](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L323-L377)

### 3.3 Protection mask: what “does not damage” must mean

**Proven:** `StructureMap.CanPlace` checks protected rectangles and tiles rejected by the supplied valid-tile set; `AddProtectedStructure` reserves a placed region. The API explicitly says protection is cooperative and modders must both check and register. [StructureMap source documentation](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/WorldBuilding/StructureMap.cs.patch#L7-L48) [official guide](https://github.com/tModLoader/tModLoader/wiki/World-Generation#structuremap)

**Proven:** The official guide says only some vanilla structures are represented and warns that late destructive operations can corrupt multitiles. It specifically names Hives, Enchanted Sword Altars, and Cabins as examples, not an exhaustive guarantee for Dungeon, Temple, or all chests. [official StructureMap guidance](https://github.com/tModLoader/tModLoader/wiki/World-Generation#structuremap) [pass-order hazard](https://github.com/tModLoader/tModLoader/wiki/World-Generation#determining-a-suitable-index)

**Recommendation:** Build a hard no-write mask from all of the following, dilated by safety padding:

- a predetermined center-world spawn sanctuary, then the final `Main.spawnTileX/Y` sanctuary once known;
- every Apogean planned structure/future rectangle, registered in both `GenVars.structures` and the persistent registry;
- every `StructureMap` collision queried during path planning;
- connected regions containing Dungeon brick/walls and Lihzahrd brick/Temple walls;
- every active chest rectangle from `Main.chest`, not only chest tiles discovered incidentally;
- every tile entity position expanded to its owning `TileObjectData` footprint;
- frame-important tiles, signs, containers, doors, altars, pylons, and tiles with `TileID.Sets.GeneralPlacementTiles[type] == false`;
- world-edge padding and any biome feature explicitly reserved by configuration or cross-mod integration.

The scar painter must use a natural-terrain allowlist. It may convert dirt, stone, mud, clay, sand families, and explicitly approved natural walls; it must not call `KillTile` or overwrite arbitrary frame-important content just because a point lies inside the desired shape.

### 3.4 Path algorithm

**Recommendation:** Plan the scar on a coarse grid, then rasterize it at tile resolution:

1. Select several deterministic surface-mouth candidates outside the spawn sanctuary and ocean margins.
2. Construct a coarse cost grid, for example 8×8 tile cells. Protected cells are impassable; biome/structure buffers are expensive; natural terrain is cheap.
3. Run a bounded path search from surface to an allowed Underworld terminus. Prefer increasing Y, limit horizontal backtracking, and penalize sharp turns so the scar reads as one wound rather than a maze.
4. If no route exists, try the next candidate. Every loop needs an attempt limit; the official guide explicitly warns that unbounded placement attempts can hang world generation. [official bounded-attempt guidance](https://github.com/tModLoader/tModLoader/wiki/World-Generation#try-until-success)
5. Convert the winning centerline into three continuous masks: cavity/core, Engraft shell, and sparse fringe. Vary radius smoothly by depth; do not randomize every tile independently.
6. Revalidate each mask cell immediately before writing. If another mod inserted a protected feature after planning, clip the fringe/shell and reroute the core locally rather than overwrite it.
7. Assert that the final core is connected from surface to the Underworld terminus. Treat a disconnected mandatory scar as a generation failure with a clear pass name/log, not a silently half-generated world.

**Inference:** A perfectly vertical line is less robust than an authored wandering centerline because Dungeon, Temple, and large modded structures can form obstacles. “Continuous” should mean one traversable/traceable connected feature, not one immutable X coordinate.

### 3.5 Pass ordering

No single insertion point simultaneously knows every final feature and guarantees that no later pass can overwrite the scar. The safe design is phased.

| Phase | Timing goal | Work allowed |
|---|---|---|
| `Apogean World Plan` | After enough terrain/biome-side information exists, before Apogean structures | Choose sanctuary, territories, candidate scar corridor, and future envelopes; reserve known Apogean regions. No destructive tile work. |
| `Maw Topology` | Before broad frame-important decoration/chest placement, but after the last vanilla unique structure that must be routed around where practical | Build the hard mask, resolve the connected path, carve/convert only allowlisted natural terrain. |
| `Apogean Major Structures` | After scar topology, before decorations | Place company shells, arenas, major ruins, and scar organs into reserved envelopes. |
| `Apogean Objects` | After all destructive terrain shaping | Place doors, furniture, containers, signs, wires, tile entities, and authored loot. |
| `Apogean Validation` | Before final cleanup/framing/liquid completion | Verify connectivity, mandatory structure count, chest/entity ownership, world bounds, and no overlap with forbidden regions. |

**Recommendation:** Resolve pass names defensively with `FindIndex`, use the later of the required vanilla anchors, and log/disable the affected feature if anchors are missing. Do not fall back to `tasks.Count - 1` for destructive work: the official guide warns that late terrain methods corrupt chests, doors, and other multitiles. [official pass-order guidance](https://github.com/tModLoader/tModLoader/wiki/World-Generation#determining-a-suitable-index) [`ModifyWorldGenTasks` documentation](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L397-L411)

If the current vanilla order offers no slot after both Dungeon/Temple and before broad containers, split topology again: reserve and carve the guaranteed core earlier, then perform only non-destructive conversion/decoration around protected structures later. Never solve the conflict by blindly carving after `Final Cleanup`.

## 4. Large procedural and authored structures

### 4.1 Proven implementation patterns

**Proven—Remnants:** Remnants replaces/reorders many world passes, reserves full rectangles, builds a procedural room grid, and fills room sockets with authored StructureHelper modules. Its Desert Ruins similarly assemble entrance/shaft/exit and room variants, then reserve added connector rectangles. This is evidence for the hybrid design—procedural topology plus authored rooms—but StructureHelper is a dependency used by Remnants, not a native tModLoader schematic API. [Remnants pass orchestration](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L35-L79) [Remnants mineshaft reservation/modules](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L82-L180) [Remnants Desert Ruins assembly](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L1668-L1866)

**Proven—Calamity:** Calamity loads its own `.csch` files into `SchematicMetaTile[,]`, validates bounds, records original tiles, clears the target carefully, applies stored metadata, repairs flipped frames with `TileObjectData`, creates chest records and loot, and manually creates supported tile entities at their top-left frames. [schematic registry/load](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L14-L183) [placement and frame handling](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L187-L327) [containers/entities](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L497-L605)

**Proven—Calamity placement:** Draedon laboratories use bounded candidate loops, biome/material validation, distance checks, `StructureMap.CanPlace`, schematic placement, and padded protected rectangles. [Draedon structure placement](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs#L101-L140) [Hell lab placement](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs#L254-L305)

**Source-audit result—Terraria Overhaul:** The pinned current revision contains no useful implementation of `ModifyWorldGenTasks`, `ModSurfaceBackgroundStyle`, `ModUndergroundBackgroundStyle`, or a structure/schematic generator. It is not a primary-source precedent for this subsystem; compatibility testing should focus on its rendering, ambience, camera, and lighting changes rather than copying a nonexistent world-generation architecture. [Terraria Overhaul pinned tree](https://github.com/Mirsario/TerrariaOverhaul/tree/e202f4719bdff845035e352ba70169b7022cbe09)

### 4.2 Recommended Apogean module format

Each authored module should have a data header separate from its tile payload:

```text
ModuleHeader
├── stable ID + schema version
├── width/height + anchor
├── allowed rotations/flips
├── required biome/depth/material percentages
├── connection sockets (door, corridor, elevator, cable, rail)
├── mandatory/optional tags (HQ, barracks, lab, arena, loot)
├── keep-air / keep-tile masks
├── chest marker IDs and loot-table IDs
├── tile-entity marker IDs
└── protection padding
```

The payload must represent tile type, wall type, frame/state where required, paint, slope/half-block, wires/actuator, liquid amount/type, and semantic markers. Calamity's `SchematicMetaTile` proves these fields are necessary for faithful structures and exposes how subtle liquid/frame restoration bugs can occur. [Calamity schematic fields and apply logic](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/CalamitySchematicIO.cs#L17-L213)

### 4.3 Placement pipeline

1. Generate a deterministic room/sector graph with required content tags.
2. Assign compatible modules to graph nodes; validate sockets and total envelope before touching tiles.
3. Check and reserve the entire envelope plus padding with `StructureMap`.
4. Terraform only the envelope's approved merge margin.
5. Stamp structural terrain and walls.
6. Stamp frame-important modules or use `WorldGen.PlaceObject` when normal placement behavior is desired. The official guide notes that manual frame-important placement is harder and that `PlaceObject` is more appropriate when `PlaceTile` cannot express style/orientation. [official placement guidance](https://github.com/tModLoader/tModLoader/wiki/World-Generation#terrariaworldgen-public-static-bool-placetile)
7. Create chest records, then populate loot; `WorldGen.PlaceChest` returns a chest index or `-1`, and existing chest arrays must not be overwritten. [official chest guidance](https://github.com/tModLoader/tModLoader/wiki/World-Generation#terrariaworldgen-public-static-int-placechestint-x-int-y-ushort-type--21-bool-notnearotherchests--false-int-style--0)
8. Place tile entities exactly once at each object's true top-left coordinate derived from `TileObjectData`; validate `IsTileValidForEntity` after placement. ExampleMod documents the entity/tile ownership contract, while Calamity demonstrates manual structure-time creation. [ExampleMod basic tile entity](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/TileEntities/BasicTileEntity.cs#L86-L128) [Calamity entity placement](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L521-L605)
9. Apply wires, actuators, liquids, and decorations after the structural shell is stable.
10. Register the final envelope in both generation-time and persistent protection registries.

### 4.4 Framing and liquids

**Proven:** During normal world generation, Terraria frames tiles when loading the world; runtime edits require explicit nearby framing and network synchronization. The official guide also warns that `TileRunner` with override can corrupt multitiles and says `OreRunner` is safer after frame-important content because it restricts replacement and frames/syncs its changes. [official framing guidance](https://github.com/tModLoader/tModLoader/wiki/World-Generation#framing) [TileRunner/OreRunner safety](https://github.com/tModLoader/tModLoader/wiki/World-Generation#terrariaworldgen-public-static-void-tilerunnerint-i-int-j-double-strength-int-steps-int-type-bool-addtile--false-float-speedx--0f-float-speedy--0f-bool-noychange--false-bool-override--true)

**Recommendation:** During world creation, prefer one late global framing/validation boundary rather than calling `SquareTileFrame` for every terrain tile. Preserve explicit frame data for frame-important modules. Any future in-game structure or ore operation must run server-side, frame the changed boundary, synchronize tile squares in bounded chunks, and avoid a single long synchronous edit.

**Proven:** Liquid amount/type live on each tile. [official liquid guidance](https://github.com/tModLoader/tModLoader/wiki/World-Generation#liquids) **Recommendation:** Place liquid-bearing modules before the last liquid-settle phase when possible. If a structure is added after settling, clear stale liquid in solids, enqueue/settle affected liquids deliberately, and test lava/water/honey/shimmer preservation. Calamity's schematic source explicitly records liquid type/amount and even documents a prior lava-to-water restoration bug, showing why liquid state cannot be treated as decoration. [Calamity liquid apply paths](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/CalamitySchematicIO.cs#L76-L213)

## 5. Native Terraria pixel-art sheet contracts

### 5.1 Contract table

| Asset kind | Proven engine/example contract | Apogean art rule |
|---|---|---|
| Standard framed terrain | Most tile pieces are 16×16 with 2 pixels of right/bottom padding (18×18 stride). Terraria chooses frames from neighbor context and random variation. The official `ExampleBlock.png` is a 288×270 standard terrain template. [Basic Tile padding/framing](https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#padding) [official ExampleBlock asset](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Tiles/ExampleBlock.png) | Paint every required edge, corner, isolated, merge, and variation cell. Never stretch a 16×16 concept over the sheet. Use reduced colors and repeated texture motifs across all frames. |
| Standard wall | Official ExampleMod's normal wall sheet is 468×180; its animated advanced wall stacks another full wall sheet vertically at 468×360. [ExampleWall asset](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Walls/ExampleWall.png) [advanced animated wall](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Walls/ExampleWallAdvanced.png) [animation code](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Walls/ExampleWallAdvanced.cs#L32-L55) | Use the official sheet as the template. Dried-root wall growth should be authored into all frames so no vanilla green/leaf cell leaks through. Animation duplicates the complete sheet; do not add arbitrary rows. |
| Frame-important/multitile | `TileObjectData` defines width, height, origin, anchors, `CoordinateWidth`, per-row `CoordinateHeights`, 2-pixel padding, style direction, wrap, multiplier, alternates, states, and animation. `CopyFrom` must precede edits and `addTile` must be last. [Basic Tile TileObjectData contract](https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#basic-tileobjectdatanewtile-structure) [official showcase](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Tiles/TileObjectDataShowcase.cs#L21-L92) | Design the code contract first, calculate the sheet from it, then draw. One structure module may contain many multitiles; the entire building is not one giant multitile sprite. |
| Underground panorama | Exactly four files: 160×16, 160×96, 160×16, 160×96. The rightmost 32 pixels of each field duplicate the leftmost 32 for wrapping. [API](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L25-L28) [ExampleMod assets](https://github.com/tModLoader/tModLoader/tree/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Assets/Textures/Backgrounds) | Treat each four-file set as one atomic palette/horizon family. Verify wrap strips byte/pixel-for-pixel before playtest. |
| Surface far/middle/close | No fixed public dimensions. Loader uses each texture's actual width/height and repeats it. Official ExampleMod happens to use 1024×408 far, 1024×600 middle variants, and 952×480 close. [loader dimension use](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L64-L77) [ExampleMod background assets](https://github.com/tModLoader/tModLoader/tree/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Assets/Textures/Backgrounds) | Pick one canvas contract per layer family. Every variant must share width, seam phase, skyline/baseline, and transparent/opaque coverage so cross-fades do not slide or expose gaps. |

### 5.2 Art-production rules for Apogean

**Recommendation:** Every final asset starts from a sheet template, not from a generated illustration. Concept art may define silhouette, palette, and material language, but it must be manually reduced/redrawn at native resolution.

- Use hard nearest-neighbor pixels, no antialiasing or semitransparent edge fuzz on terrain, walls, or tile objects.
- Keep terrain texture low-frequency enough that repeating frames do not create noise. Reserve bright amber for active cysts, powered organs, and explicit light-bearing growth.
- Dried wall foliage should read as roots/tumbleweed fibers: ochre/brown strand clusters, dark gaps, sparse pale tips, and no rounded green canopy shapes.
- Ashen/fire-damaged trees need separate trunk/branch/top/sapling sheets matching the `ModTree` contract; fire glow should be a controlled overlay or explicit emissive variant, not baked into every dead tree.
- A ruin is a composition of terrain tiles, walls, platforms, furniture, rubble objects, decals, and optional large multitiles. Background ruins should echo the same palette and silhouettes but not reproduce foreground tile detail at miniature scale.
- Parallax layers need horizontally tileable left/right seams and enough vertical bleed to cover camera/resolution extremes. A visible seam is an asset failure even when the code repeats correctly.
- Keep the near layer highest contrast, middle layer lower contrast, and far layer closest to sky/fog values. This depth separation is an art requirement layered on top of the engine's parallax.

## 6. Implementation hazards

| Hazard | Evidence/status | Mitigation |
|---|---|---|
| Same-slot background variant snap | **Inference from proven draw path** | Separate style slots for changing landmark sets; overlays for tint/weather. |
| Close layer fades faster than far/middle | **Proven on pinned tModLoader source** because `ModifyFarFades` receives the front array | Version-gated no-op and a regression test. |
| “Foreground stitch” drawn twice or at wrong alpha | **Inference** when multiple styles overlap | Centralize custom close drawing, read the correct slot alpha, and test both directions at partial opacity. |
| Underground boundary flaps every few frames | **Inference** from one chosen style plus brief old/new transition | Spatial dead band, dwell timer, stable update-time target. |
| Misusing underground slots 0/2 for horizontal seams | **Proven contract mismatch** | Keep 0/2 as vertical depth borders; use a full transition style. |
| Current Engraft pass trusts spawn too early | **Current repo + official timeline** | Replace ellipse pass with saved world plan and sanctuary reservation. |
| StructureMap assumed to protect everything | **Proven false assumption**; API is cooperative and guide lists only examples | Add explicit Dungeon/Temple/chest/entity/future masks and allowlisted painting. |
| Late `TileRunner` corrupts doors/chests | **Proven official warning** | Finish destructive topology before object placement; use safe conversion algorithms later. |
| Infinite candidate loops | **Proven official warning** | Bounded attempts, deterministic fallback, clear generation failure. |
| A structure's tile exists but its chest/entity record does not | **Proven implementation concern** in ExampleMod/Calamity | Dedicated post-stamp container/entity phase and validation. |
| Mirrored modules have broken directional frames | **Proven Calamity complexity** | Initially author both orientations or implement and test TileObjectData-aware flipping; do not naïvely mirror frame X. |
| Liquid transmutation/stale liquid in solids | **Proven Calamity bug history** | Store amount and type, clear liquids in solids, settle/validate after placement. |
| Runtime Engraft spread damages future structures | **Inference from StructureMap lifetime** | Persist a separate protection registry and query it for every runtime conversion. |
| Cross-mod pass names missing or reordered | **Proven modded task-list reality** | Defensive anchors, feature-specific disable/log path, compatibility mode. |

## 7. Staged implementation and test plan

### Stage 0 — Instrumentation before content

- Add a worldgen debug mode that records pass names/order, plan seed, candidate rejection reasons, final reservations, scar waypoints, and elapsed milliseconds.
- Add map overlays for spawn sanctuary, `StructureMap` query failures, Apogean reservations, core/shell/fringe masks, and module envelopes.
- Add deterministic plan serialization and a command that prints the saved schema/version.
- Acceptance: the same world seed/configuration yields the same Apogean plan on repeated generations.

### Stage 1 — Scar topology with placeholder tiles

- Implement only world plan, protected masks, coarse path, connected raster masks, and simple placeholder terrain conversion.
- Generate at least 25 seeds for each world size and all vanilla evil choices; include secret seeds supported by the mod configuration.
- Automated assertions: one surface mouth, one Underworld terminus, connected core, zero writes in sanctuary/Dungeon/Temple/container/entity/future masks, no out-of-world access, bounded attempt count.
- Manual acceptance: the scar looks continuous on the full map, bends around landmarks without absurd horizontal detours, and preserves a safe spawn/build zone.

### Stage 2 — Surface/underground visual routing

- Give complete biome variants separate style slots; version-gate `ModifyFarFades` behavior.
- Add underground hysteresis and one neutral transition-band style.
- Test at 800×720, 1920×1080, ultrawide, maximum zoom out, minimum zoom, both travel directions, grappling speed, minecart speed, teleport, world join, and respawn.
- Acceptance: no gaps to skybox, no same-slot landmark snap, no boundary flicker, matching far/mid/close transition timing, and no replacement of un-authored Mushroom/Dungeon/Temple backgrounds.

### Stage 3 — Authored module pipeline

- Start with one small surface ruin, one underground room, and one Maw organ chamber.
- Validate module schema, anchors, keep masks, chest markers, tile-entity markers, directional frames, wire networks, and liquid cells.
- Acceptance: every generated object has exactly one owning record; breaking/reloading containers and tile entities behaves normally; no invalid multitiles appear at module edges.

### Stage 4 — Major structures and repairable arenas

- Assemble one medium procedural structure from authored modules, then the first company compound/arena.
- Reserve the full envelope before placement and run a post-generation integrity report: required rooms, reachable entrances, valid doors/platforms, chest count, entity count, and protected padding.
- Test all world sizes and both sides of the world. Force candidate scarcity to exercise fallback logic.
- Acceptance: no overlap with scar/sanctuary/vanilla unique structures and no mandatory room omitted silently.

### Stage 5 — Final art and compatibility matrix

- Replace placeholder terrain/walls/trees/backgrounds module by module using native templates.
- Pixel QA: exact dimensions, alpha, no filtered edges, complete terrain/wall frame coverage, 32-pixel underground wrap duplication, horizontal panorama seam, palette/value hierarchy.
- Multiplayer: dedicated server plus two clients standing in different background states; joining mid-session; chest/entity interaction; server-authoritative runtime spread.
- Compatibility: vanilla-only, Remnants, Calamity, Terraria Overhaul, and representative background/worldgen mods. Apogean overhaul mode may declare generation ownership, while compatibility mode must preserve unsupported third-party styles and structures.
- Performance acceptance: record generation duration and peak memory by stage; reject unbounded full-world rescans repeated by multiple passes.

## Final recommendation

Build the next vertical slice in this order: `ApogeanWorldPlan` and persistent reservations → connected protected Maw scar with placeholder art → stable surface/underground routing and transition band → a small authored-module pipeline → one repairable ruin/arena → final pixel art. This order separates world correctness from art polish while ensuring every finished sprite is drawn against a stable engine contract.

The literal foreground “stitch” should remain optional until the normal slot cross-fades, matching parallax families, and world-tile seam landmarks are proven insufficient. The scar and structure planner are not optional: they are the foundation that prevents later buildings, bosses, ores, and post-Moon-Lord expansion content from competing destructively for the same world space.
