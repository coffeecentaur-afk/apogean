# Authored Structure and Tileset Production for Apogee

**Research date:** 2026-09-01
**Scope:** tModLoader 1.4.4-era world generation; fixed faction buildings; reusable ruins; complete custom tilesets.
**Source policy:** only official tModLoader documentation/source, the public Remnants source, and the public Calamity Mod source were used. Recommendations below are original inferences from those sources; no third-party code or art should be copied into Apogee.

## Executive decision

Apogee should stop drawing its permanent faction buildings with large C# rectangle/tile loops. Kessler, Helix, and Sentrix should each be authored as a complete building in an editor world and exported as a versioned blueprint. Procedural code should remain responsible for finding a safe location, adapting the foundation to terrain, blending the perimeter into the Waste, selecting limited cosmetic variants, assigning loot, and validating the result.

The best long-term format is an **Apogee-owned compiled blueprint** (for example, `.apstruct`) plus a small, reviewable JSON metadata file. A `.tpl` file has no built-in meaning to tModLoader, and tModLoader itself does not provide a serialized structure format: its `GenStructure` abstraction only exposes a procedural `Place(Point, StructureMap)` method. Remnants uses StructureHelper's `.shstruct`/`.shmstruct` files, while Calamity implements its own compressed `.csch` format. [Official `GenStructure` documentation](https://docs.tmodloader.net/docs/stable/class_gen_structure.html), [Remnants structure assets](https://github.com/lazy-wombat/Remnants/tree/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures), [Calamity schematic assets](https://github.com/CalamityTeam/CalamityModPublic/tree/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics)

The recommended split is:

| Content | Production method |
|---|---|
| Kessler compound, Helix tower, Sentrix spire | One authored whole-building blueprint each, with optional authored variants |
| Major rooms, gatehouses, landing pads | Authored sub-blueprints referenced by the parent blueprint when a controlled variant is desired |
| Abandoned laboratories, mines, large procedural dungeons | Remnants-style room graph assembled from authored room modules |
| Foundations, terrain skirts, roads, crash scars, snow/sand burial, debris | Bounded procedural post-processing |
| Chests, terminals, doors, animated fixtures, tile entities, NPC anchors | Semantic markers resolved after structural cells are stamped |

This gives the faction buildings a deliberate, polished silhouette without giving up compatibility or terrain integration.

## 1. What tModLoader actually provides

### No official `.tpl` or JSON structure standard

tModLoader provides generation passes, tile APIs, `GenStructure`, and `StructureMap`; it does not define an official file format for authored buildings. Consequently, `.tpl`, `.json`, `.shstruct`, and `.csch` are only meaningful when a specific loader defines their schema. `GenStructure` can be a useful interface around Apogee's placer, but it is not a serializer. [Official `GenStructure` documentation](https://docs.tmodloader.net/docs/stable/class_gen_structure.html)

A full-building JSON tile matrix would be human-readable but unnecessarily large and easy to corrupt. It would also need to model every important `Tile` field: tile and wall identities, frames, slopes, half blocks, paint, wires, actuators, liquids, coatings, and visibility/fullbright state. Calamity's `SchematicMetaTile` demonstrates that this is substantially more than a foreground tile ID. [Calamity `SchematicMetaTile`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/CalamitySchematicIO.cs#L16-L75)

Therefore:

- Use JSON for metadata, anchors, tags, clearance policy, marker objects, and version-control review.
- Use a compressed binary blueprint for the dense tile/wall state.
- Give the binary a magic header, schema version, dimensions, checksum, and symbolic content table.
- Do not persist runtime numeric IDs for modded tiles or walls. Those IDs can change with the loaded mod set.

Calamity solves the last problem by exporting fully qualified `ModName/ContentName` strings and remapping them to runtime IDs while loading. It also version-marks its format and GZip-compresses the payload. Apogee should adopt the same concepts in original code. [Calamity symbolic ID export](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/CalamitySchematicIO.cs#L455-L515), [Calamity import and format checks](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/CalamitySchematicIO.cs#L729-L873)

### Generation pass placement

`ModSystem.ModifyWorldGenTasks` is the supported hook for inserting an explicit generation pass. Official documentation recommends finding passes by name, coding defensively when a pass is missing, and avoiding cached indices because every insertion changes subsequent indices. It also warns that early passes may ignore multitiles and that late terrain edits can corrupt furniture, doors, and chests. Deterministic choices must use `WorldGen.genRand`, not `Main.rand` or a time-seeded RNG. [Official world-generation guide: pass selection and deterministic RNG](https://github.com/tModLoader/tModLoader/wiki/World-Generation#determining-a-suitable-index), [official `ModSystem` source](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L395-L411)

For Apogee's current additive architecture, an explicit **`Apogee: Authored Structures` pass immediately after `Final Cleanup`** is defensible because it sees the completed vanilla/modded terrain and all structures generated before that point. It must, however, behave like an atomic structure placer rather than a late terrain carver. `PostWorldGen` is also officially available for placing tiles after generation, but an explicit pass is preferable here because it has named progress, internal ordering, timing, diagnostics, and a visible place in the pass list. `PostWorldGen` should be reserved for a final audit or non-destructive registration. [Official `ModSystem` documentation](https://docs.tmodloader.net/docs/stable/class_mod_system.html)

### `StructureMap` is cooperative, not enforcement

`GenVars.structures.CanPlace` checks protected rectangles and, by default, rejects tiles outside `TileID.Sets.GeneralPlacementTiles`. An overload accepts a custom valid-tile set. `AddProtectedStructure` records the placed footprint for later cooperating generation. The official source explicitly states that modders must both check and register structures themselves. A later mod that ignores `StructureMap` can still overwrite Apogee. [Official `StructureMap` source](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/WorldBuilding/StructureMap.cs.patch#L7-L48), [official world-generation guide: `StructureMap`](https://github.com/tModLoader/tModLoader/wiki/World-Generation#structuremap)

For every mandatory building, Apogee should perform all of the following before mutating one tile:

1. Check world bounds for the footprint, safety envelope, foundation, entrance, and approach path.
2. Check `StructureMap.CanPlace` with an explicit Apogee overwrite policy.
3. Reject Dungeon, Temple, oceans, Shimmer, hives, other mod tiles/walls, chests, dressers, tile entities, spawn sanctuary, and every reserved Apogee landmark.
4. Check biome/terrain composition and available air/solid ratios.
5. Verify that the final foundation and every object marker will have valid anchors.
6. Only after the complete candidate passes, stamp the blueprint and register its padded footprint.

Use a deterministic, pre-ranked candidate list and a hard attempt cap. The same seed is reproducible only for the same mod list, configuration, pass order, and blueprint version; changing any of those inputs legitimately changes the completed terrain against which placement is planned.

## 2. What Remnants does

Remnants is not evidence that polished structures should be constructed entirely from procedural tile loops. Its source shows a hybrid system:

- A procedural `Dungeon` grid stores occupied room cells and connection markers.
- Worldgen code chooses a topology and checks room connectivity.
- Authored `.shstruct` and `.shmstruct` modules are then pasted into selected cells through StructureHelper.
- The completed footprint is registered with `StructureMap`.

[Remnants room-grid implementation](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/General.cs#L757-L875), [Remnants mineshaft composition](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L82-L185), [Remnants Magical Lab composition](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Dungeons.cs#L2580-L2655)

At the inspected commit, Remnants contains 149 authored structure assets: 95 single structures and 54 multi-structure collections. Its mineshafts, dungeon, Magical Lab, observatory, cabins, towers, and ruins use those authored modules for the room art. Procedural code supplies the macro-layout, shells, terrain transformation, and object/loot post-processing. [Remnants authored structure tree](https://github.com/lazy-wombat/Remnants/tree/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures)

This is the right reference for a future Apogee procedural dungeon, ruined laboratory network, or modular corporate interior. It is unnecessary for a permanent faction headquarters whose silhouette, floors, narrative spaces, and progression doors should remain recognizable in every world.

Remnants also aggressively replaces or removes vanilla and other-mod generation passes rather than behaving as a small additive structure pack. Its systems replace biome, terrain, dungeon, Temple, chest, micro-biome, and cleanup passes and explicitly remove several third-party pass names. This is why its techniques cannot be copied wholesale into an additive compatibility-first mod. [Remnants terrain pass replacement](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Terrain.cs#L28-L84), [Remnants biome pass replacement](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Biomes.cs#L30-L82), [Remnants pass cleanup](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/General.cs#L60-L163)

## 3. What Calamity does for laboratories

Calamity is the closer reference for Apogee's faction headquarters. It ships six complete Draedon laboratory `.csch` files, loads them into named tile maps during mod loading, finds biome-appropriate candidate rectangles, pastes the entire schematic, fills chests through callbacks, stores the lab center, and protects the rectangle with padding. [Calamity schematic registry](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L17-L166), [Calamity lab placement](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs#L254-L305)

The biome laboratories do more than call `StructureMap`. Ice, Plague, and Cavern placement scan an expanded rectangle for forbidden locations, require a minimum ratio of appropriate terrain, enforce separation from other facilities, impose a bounded retry count, and only then paste/protect the schematic. [Calamity Ice Lab validation](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs#L431-L485), [Calamity Plague and Cavern validation](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs#L519-L576), [Calamity forbidden-location policy](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs#L23-L62)

Calamity's labs are inserted **before** vanilla `Final Cleanup`, not after it. Its `currentFinalIndex` begins immediately before `Final Cleanup`, and the lab pass is inserted into that pre-final sequence. Therefore, Calamity proves the authored-schematic model and validation pattern, but not that its exact paste implementation is automatically safe after cleanup. Apogee's post-final placer must explicitly do all framing, liquid, chest, and tile-entity finalization that it needs. [Calamity worldgen task order](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Systems/World/WorldgenManagementSystem.cs#L221-L359)

Calamity's implementation also reveals two expensive edge cases:

- Horizontal schematic flipping requires corrections for slopes, `TileObjectData` widths/directions, platforms, tracks, and exceptional vanilla tiles.
- Raw tile-state application does not automatically create logical `Chest` entries or `ModTileEntity` instances, so Calamity explicitly creates chests and maintains a tile-entity placement list.

[Calamity schematic flipping](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L286-L496), [Calamity chest and tile-entity finalization](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L497-L607)

Apogee should initially disallow runtime blueprint mirroring and use authored left/right variants only where truly necessary. It should encode interactive furniture as semantic markers and place those objects through `WorldGen.PlaceObject`, `WorldGen.PlaceChest`, or the relevant tile-entity API after the structural layer is complete. This is simpler and safer than duplicating Calamity's increasingly specialized frame-correction code.

## 4. Recommended Apogee blueprint pipeline

### Authoring artifacts

Each structure should have:

- `StructureName.apstruct`: compressed structural cell data.
- `StructureName.json`: schema version, dimensions, origin, entrance, foundation line, safety padding, allowed world-size/biome bands, terrain overwrite policy, named zones, and object markers.
- `StructureName.preview.png`: generated map-scale preview used for review, not runtime placement.

The binary cell format should preserve inert structural state: tile/wall identity, frame data where required, slope/half-block, tile/wall paint, wires, actuator state, liquid, coating/invisibility, and fullbright state. Modded identities should use fully qualified symbolic names in a lookup table. The JSON marker layer should contain chests, doors, crafting stations, terminals, NPC communication positions, power-armor racks, symbiote tanks, hologram projectors, spawn points, loot tables, and progression locks.

Mandatory Apogee-owned content that fails to resolve should fail blueprint loading with a precise error. Silently substituting an unloaded placeholder would turn a core headquarters into a corrupt structure. Optional cross-mod decoration may be skipped through an explicit optional marker, but faction blueprints should never embed Calamity or Remnants tiles.

### Runtime sequence

1. **Load:** parse and validate all blueprints once during mod loading; resolve symbolic Apogee tile/wall names.
2. **Plan:** after completed terrain exists, rank legal candidate anchors using only `WorldGen.genRand` and saved world-plan constraints.
3. **Preflight:** perform every bounds, `StructureMap`, forbidden-content, terrain-ratio, entrance, and object-anchor check without mutation.
4. **Foundation:** apply a bounded cut/fill mask and terrain skirt. Do not excavate outside the declared envelope.
5. **Structural paste:** place walls and inert structural cells. Clear only cells the blueprint explicitly owns.
6. **Objects:** resolve markers and place multitile furniture through placement APIs; verify every result.
7. **State:** create chests and tile entities, attach loot/progression metadata, and store named structure zones.
8. **Finalize:** frame the footprint and one-tile perimeter; settle only blueprint-owned liquids; update lighting/framing where needed.
9. **Protect:** call `AddProtectedStructure` with declared padding and save the final footprint in Apogee world data.
10. **Audit:** assert structure hash/version, entrance reachability, multitile integrity, required marker count, chest/tile-entity existence, and zero forbidden overlap.

No placement failure should leave half a headquarters. Preflight must make the paste effectively atomic. If a mandatory structure has no legal full-size location, generation should report a diagnostic containing the seed, candidate rejection counts, and conflicting rectangles. It should not silently shrink, distort, or overwrite a protected feature.

## 5. Complete tileset contract

`TileObjectData` governs the width, height, origin, anchors, directions, styles, and spritesheet coordinates of multitile furniture. Official documentation recommends copying a known template or existing tile, making changes, then calling `TileObjectData.addTile(Type)` last. `CoordinateHeights` must match `Height`, and Terraria conventionally expects two pixels of padding between tile cells. [Official `TileObjectData` source](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/ObjectData/TileObjectData.cs.patch#L470-L648), [official Basic Tile guide](https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#basic-tileobjectdatanewtile-structure)

For a normal framed object with 16-pixel cells and two-pixel padding, a `W x H` frame occupies `18W x 18H` pixels unless a row deliberately uses an 18-pixel coordinate height or a padding fix. Art dimensions must be derived from the registered `TileObjectData`, not guessed independently.

Each faction needs a coherent, complete family rather than one generic block and one oversized fixture:

| Family | Minimum production set per faction |
|---|---|
| Structural | primary block, secondary panel, beam/support, trim/hazard block, damaged/rusted variant, window/glass treatment |
| Walls | safe interior wall, unsafe worldgen wall, beam/pillar wall, window/technical wall, damaged variant |
| Traversal | platform, closed/open door pair, gate or security shutter, ladder/elevator visual treatment |
| Housing/function | chair, table, workbench/crafting station, storage, light source; add bed/dresser only where player housing is intentionally supported |
| Lighting | torch/sconce, lamp, hanging light, chandelier/large ceiling light, emergency/off state |
| Common dressing | shelf, crate, pipe/conduit, terminal, display, signage, floor clutter, rubble/debris |
| Signature dressing | Kessler armor/missile racks; Helix animated culture/symbiote tanks; Sentrix hologram/AI/CCTV arrays |
| Progression | locked door, communication terminal, faction crafting station, raid/boss trigger, post-defeat salvage state |

### Required behavior

- **Map:** every structural tile and important fixture needs an intentional `AddMapEntry` color/name. `AddMapEntry` controls minimap color and optional hover text. [Official `ModTile` documentation](https://docs.tmodloader.net/docs/stable/class_mod_tile.html), [official Basic Tile guide](https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#addmapentry)
- **Walls:** player-safe walls require `Main.wallHouse[Type] = true`; world-generated unsafe walls should omit it. Safe and unsafe variants can be configured to blend. [Official safe/unsafe wall examples](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Walls/ExampleWall.cs), [unsafe wall example](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Walls/ExampleWallUnsafe.cs)
- **Housing:** doors, chairs, tables/workbenches, platforms, and lights must register with the corresponding `TileID.Sets.RoomNeeds` arrays when intended to satisfy housing. [Official chair example](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExampleChair.cs), [official workbench example](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExampleWorkbench.cs), [official platform example](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExamplePlatform.cs)
- **Doors:** closed and open tiles must reference one another through `OpenDoorID`/`CloseDoorID`, register room-door behavior, and correctly describe all anchors/origins. [Official closed/open door examples](https://github.com/tModLoader/tModLoader/tree/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture), [Calamity four-tile laboratory doors](https://github.com/CalamityTeam/CalamityModPublic/tree/1a8cebd27ec5615316b78f71973446b5528d2b78/Tiles/DraedonStructures)
- **Crafting:** use `AdjTiles` for vanilla-equivalent behavior and register the custom tile directly in recipes for faction-specific stations. The tile's physical table/housing behavior is separate from recipe adjacency.
- **Lighting:** set `Main.tileLighted`, return color through `ModifyLight`, and use glow/flame overlays only for pixels that should appear emissive. Wire-controlled fixtures must change the complete multitile and synchronize state. [Official animated tile example](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/ExampleAnimatedTile.cs), [Calamity caged laboratory light](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Tiles/DraedonStructures/CagedLights/CagedLablight.cs)
- **Animation:** use `AnimationFrameHeight` with `AnimateTile` for a shared loop and `AnimateIndividualTile` or a tile entity only when instances need unique state. The official drawing path already applies `AnimationFrameHeight * Main.tileFrame[type]` before individual offsets. [Official `ModTile` animation documentation](https://docs.tmodloader.net/docs/stable/class_mod_tile.html), [official glowmask animation example](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/ExampleAnimatedGlowmaskTile.cs)
- **Tile entities:** use them only for persistent per-object data or interaction, not for ordinary looping decoration. Define a placement hook, remove the entity when the multitile is destroyed, save/net-sync its state, and explicitly instantiate it when a blueprint bypasses normal player placement. Calamity's hologram projector is the relevant reference. [Calamity hologram projector](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Tiles/DraedonStructures/LabHologramProjector.cs)

Calamity's laboratory family is useful as a completeness reference: it includes plating, panel and pipe blocks; multiple wall types; doors; containment boxes; consoles; terminals; screens; servers; shelves; crates; security chests; caged lights; turrets; factories; and hologram projectors. Apogee should match that breadth while keeping its own faction-specific silhouettes and palette. [Calamity Draedon structure tiles](https://github.com/CalamityTeam/CalamityModPublic/tree/1a8cebd27ec5615316b78f71973446b5528d2b78/Tiles/DraedonStructures), [Calamity Draedon walls](https://github.com/CalamityTeam/CalamityModPublic/tree/1a8cebd27ec5615316b78f71973446b5528d2b78/Walls/DraedonStructures)

## 6. Compatibility and validation risks

| Risk | Required mitigation |
|---|---|
| Another mod changes pass order or removes `Final Cleanup` | Resolve the named pass defensively at insertion time; log and use a documented fallback rather than a hard-coded index |
| Another post-final pass ignores `StructureMap` | Save structure bounds, run a final integrity audit, and maintain a compatibility matrix; `StructureMap` cannot enforce cooperation |
| Calamity overlap | Apogee's post-final preflight sees Calamity labs and their protected rectangles because Calamity places them before final cleanup; still scan actual tiles and tile entities |
| Remnants overlap | Treat full Remnants worldgen as a separate compatibility target. It replaces/removes major passes and terrain producers; do not claim compatibility until a dedicated adapter and seed matrix pass |
| Numeric mod IDs change | Store fully qualified names and resolve them at load time |
| Blueprint format becomes stale | Magic header, schema version, checksum, migration policy, and graceful diagnostic failure |
| Partial multitiles or wrong frames | Export-time `TileObjectData` validation plus runtime object-marker placement and post-paste integrity scan |
| Missing chests/tile entities | Use semantic markers and assert logical entity creation, not just visible frames |
| Invalid housing | Automated room test for safe walls plus door/chair/table/light registration; separately test intentionally unsafe corporate rooms |
| Liquid leakage after final cleanup | Keep structure liquids rare; validate containment; explicitly settle only the affected rectangle |
| Horizontal flipping corrupts objects | No runtime mirroring in the first production pipeline; author or compile validated directional variants |
| Unbounded placement loops | Precompute candidate bands, rank deterministic candidates, cap attempts, and emit rejection diagnostics |
| Art and `TileObjectData` disagree | Generate a machine-readable art contract from width, height, styles, alternates, and animation frames; fail the build on dimension mismatch |
| Source reuse | Calamity is proprietary and permits reference use subject to its license; Remnants has no license file at the inspected root. Write original Apogee code and art rather than lifting either implementation. [Calamity license](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/LICENSE.md), [Remnants repository root](https://github.com/lazy-wombat/Remnants/tree/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389) |

### Minimum validation matrix before replacing placeholders

1. Generate at least 20 Large worlds for Corruption and 20 for Crimson using fixed recorded seeds.
2. Repeat the matrix with Calamity enabled; separately test Remnants only after defining an explicit compatibility mode.
3. Include normal, Drunk, Remix/Don't Dig Up, and Get Fixed Boi seeds if Apogee intends to support them.
4. Record blueprint version, selected anchor, candidate rejection totals, footprint hash, generation time, and validation result.
5. Assert all mandatory structures exist exactly once, are inside world bounds, avoid protected landmarks, and preserve required approaches.
6. Assert every required door, chest, crafting station, terminal, light, and tile entity exists and has a valid top-left/object origin.
7. Test door toggles, wires, animations, glow masks, map entries, mining protection, post-CEO destructibility, housing, paint/coatings, and all Terraria lighting modes.
8. Load the generated worlds in dedicated-server multiplayer and verify tile entities, chests, and progression locks synchronize.

## 7. Proposed production gate for the next implementation

Do not author all three buildings simultaneously. Prove the pipeline with **Kessler**, because its compound has the hardest terrain interface, gate, wall, armory dressing, and mining lock.

The Kessler gate is complete only when:

1. The full Kessler tileset contract is implemented and its spritesheet dimensions are build-validated.
2. A polished compound is built in an authoring world and exported without hand-written room loops.
3. The same blueprint places deterministically on the recorded seed matrix.
4. The exterior foundation blends into snow, Waste soil, and ordinary stone without cutting protected content.
5. Doors, platforms, lights, crafting objects, armory racks, chests, map colors, and progression locks all function.
6. The building survives save/reload and dedicated-server testing.

Once that works, Helix and Sentrix reuse the pipeline and differ primarily in their tilesets, blueprints, markers, and terrain adapters. Procedural ruins can then reuse the same blueprint format as smaller authored modules.

## Sources inspected

### tModLoader — branch `1.4.4`, commit [`b596b760`](https://github.com/tModLoader/tModLoader/commit/b596b760ee90dc27d11dad756d955fb3f7da795e)

- [`ModSystem.cs`](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/ModLoader/ModSystem.cs)
- [`StructureMap.cs.patch`](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/WorldBuilding/StructureMap.cs.patch)
- [`TileObjectData.cs.patch`](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/ObjectData/TileObjectData.cs.patch) and [`TileObjectData.TML.cs`](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/ObjectData/TileObjectData.TML.cs)
- Official [World Generation guide](https://github.com/tModLoader/tModLoader/wiki/World-Generation), [Basic Tile guide](https://github.com/tModLoader/tModLoader/wiki/Basic-Tile), [`ModSystem`](https://docs.tmodloader.net/docs/stable/class_mod_system.html), [`ModTile`](https://docs.tmodloader.net/docs/stable/class_mod_tile.html), and [`GenStructure`](https://docs.tmodloader.net/docs/stable/class_gen_structure.html) documentation
- ExampleMod furniture, wall, door, platform, and animation sources under [`ExampleMod/Content/Tiles`](https://github.com/tModLoader/tModLoader/tree/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles) and [`ExampleMod/Content/Walls`](https://github.com/tModLoader/tModLoader/tree/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Walls)

### Remnants — branch `main`, commit [`9c2cbf9c`](https://github.com/lazy-wombat/Remnants/commit/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389)

- [`Content/World/General.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/General.cs)
- [`Content/World/Structures.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs)
- [`Content/World/Dungeons.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Dungeons.cs)
- [`Content/World/Terrain.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Terrain.cs)
- [`Content/World/Biomes.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Biomes.cs)
- [`Content/World/Structures`](https://github.com/lazy-wombat/Remnants/tree/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures) template tree and [`build.txt`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/build.txt)

### Calamity Mod Public — branch `1.4.4`, commit [`1a8cebd2`](https://github.com/CalamityTeam/CalamityModPublic/commit/1a8cebd27ec5615316b78f71973446b5528d2b78)

- [`Schematics/SchematicManager.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs)
- [`Schematics/CalamitySchematicIO.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/CalamitySchematicIO.cs)
- [`Schematics/SchematicDataTypes.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicDataTypes.cs)
- [`Systems/World/WorldgenManagementSystem.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Systems/World/WorldgenManagementSystem.cs)
- [`World/DraedonStructures.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs)
- [`Utilities/WorldgenUtils.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Utilities/WorldgenUtils.cs)
- Draedon structure [`Tiles`](https://github.com/CalamityTeam/CalamityModPublic/tree/1a8cebd27ec5615316b78f71973446b5528d2b78/Tiles/DraedonStructures), [`Walls`](https://github.com/CalamityTeam/CalamityModPublic/tree/1a8cebd27ec5615316b78f71973446b5528d2b78/Walls/DraedonStructures), and `.csch` [`Schematics`](https://github.com/CalamityTeam/CalamityModPublic/tree/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics)
- [`LICENSE.md`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/LICENSE.md)
