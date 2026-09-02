# Terraria-native tilesets, Maw conversion, and authored-structure placement

Research target: tModLoader 1.4.4. Sources are limited to the official tModLoader documentation and ExampleMod, plus the first-party Remnants and Calamity public repositories. Links are pinned to the source revisions inspected.

Implementation status (2026-09-02): native terrain/wall masks, Wastes and Maw material families, the shared conversion registry, two-step purification, surface-anchored Campus mutation masks, layered backgrounds, and the in-game material gallery are implemented and validated in `Apogee Native Visual V3`. Paired doors, authored chests/tile entities, glowmasks, and full interactive Campus progression remain later Campus-content work rather than hidden inside this environmental repair.

## Executive conclusion

Apogean does **not** need a resource pack for its custom terrain, trees, walls, or furniture. A resource pack replaces existing Terraria assets; `ModTile`, `ModWall`, and `ModTree` load their own textures from the mod. ExampleMod demonstrates this directly by requesting the trunk, branch, and top textures from mod content in `ModTree.SetStaticDefaults` ([ExampleTree.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Plants/ExampleTree.cs#L27-L53)).

The current visual problem is an asset-contract problem: a concept illustration or one repeated 16×16 square is not a Terraria terrain tileset. Each tile class must have a texture sheet whose frames exactly match its framing code and `TileObjectData`. Structures must then stamp those real tiles and objects without clearing a rectangular moat around the authored silhouette.

The recommended repair is:

1. Rebuild every material as a verified Terraria-format sheet before using it in a campus or biome.
2. Implement the Maw as a `ModBiomeConversion` backed by one explicit tile/wall conversion registry.
3. Replace bounding-box clearing with authored `keep`, `clear`, `tile`, `wall`, `liquid`, and `object` cells.
4. Surface-anchor Helix by its dome/foundation datum, fill foundations down into terrain, and reserve padding without erasing it.
5. Validate assets in an in-game fixture room and conversions in a matrix world; a successful C# build alone cannot prove that a texture is framed correctly.

## 1. Native tile texture contracts

### Universal coordinate rule

Terraria world tiles occupy 16×16 pixels. Frame-important objects normally use 2 transparent padding pixels between source cells. `TileObjectData.CoordinateWidth` should almost always be 16, `CoordinatePadding` should normally be 2, and `CoordinateHeights` must contain one entry for every tile row. The official API warns that engine code assumes two-pixel padding in many places ([TileObjectData documentation](https://docs.tmodloader.net/docs/stable/class_tile_object_data.html)).

This makes a normal source-cell stride 18 pixels, but the last row may use a height of 18 so the object sheet contains its bottom padding. Sheet size must be derived from the registered object data; it must not be guessed from the concept art.

### Contract matrix

| Content | Required source contract | Official reference |
|---|---|---|
| Auto-framed solid terrain | `Main.tileSolid = true`; normally not frame-important; texture must contain the complete terrain framing atlas expected by vanilla framing. ExampleMod's canonical sheet is **288×270**, not one repeated square. | [ExampleBlock.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/ExampleBlock.cs#L10-L24), [ExampleBlock.png](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/ExampleBlock.png) |
| Background wall | Standard auto-framed wall atlas. ExampleMod's wall sheet is **468×180**; a second animation bank doubles its height to 360. `Main.wallHouse` distinguishes safe housing walls from unsafe natural walls. | [ExampleWall.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Walls/ExampleWall.cs), [ExampleWall.png](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Walls/ExampleWall.png), [ExampleWallAdvanced.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Walls/ExampleWallAdvanced.cs) |
| Platform | 27 horizontal 18-pixel frames: **486×18**. Register solid-top/platform/table/door behavior and `StyleMultiplier = StyleWrapLimit = 27`. | [ExamplePlatform.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExamplePlatform.cs#L12-L40), [ExamplePlatform.png](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExamplePlatform.png) |
| Chair | 1×2 object with left/right alternates. ExampleMod uses `[16,18]`, horizontal styles, multiplier 2, and a **36×40** sheet. Sitting direction and anchor must be calculated from frame coordinates. | [ExampleChair.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExampleChair.cs#L19-L89), [ExampleChair.png](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExampleChair.png) |
| Table | `Style3x2`, heights `[16,18]`, **54×36** per style; register table/room behavior. | [ExampleTable.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExampleTable.cs#L12-L34), [ExampleTable.png](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExampleTable.png) |
| Workbench | `Style2x1`, height `[18]`, **36×20** in ExampleMod; register workbench adjacency and table/room behavior. | [ExampleWorkbench.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExampleWorkbench.cs#L12-L34), [ExampleWorkbench.png](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExampleWorkbench.png) |
| Door | Closed and open are paired tile types. The closed example copies vanilla closed-door data and includes three subtle per-segment random variants; open has separate width/orientation/anchor data. | [ExampleDoorClosed.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExampleDoorClosed.cs#L13-L64), [ExampleDoorOpen.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Furniture/ExampleDoorOpen.cs#L17-L82) |
| Tree | A `ModTree` still uses vanilla tree tile ID 5 and selects its style from the soil. It needs trunk, branch, and top atlases plus `GrowsOnTileId`, sapling type/style, wood drop, and foliage behavior. ExampleMod's templates are **176×264**, **84×126**, and **246×82** respectively. | [ModTree API](https://docs.tmodloader.net/docs/stable/class_mod_tree.html), [ExampleTree.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Plants/ExampleTree.cs#L27-L62), [trunk](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Plants/ExampleTree.png), [branches](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Plants/ExampleTree_Branches.png), [tops](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/Plants/ExampleTree_Tops.png) |

The exact terrain sheet may differ when custom framing code is deliberately used. Calamity's laboratory plating calls a custom gemspark framer and uses a 324×90 texture, while its pipe plating uses a 288×270 atlas and a different custom merge function ([LaboratoryPlating.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Tiles/DraedonStructures/LaboratoryPlating.cs), [LaboratoryPipePlating.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Tiles/DraedonStructures/LaboratoryPipePlating.cs)). Therefore every Apogean sheet needs a declared pairing: **texture template + framing implementation + merge rules**. Mixing one contract with another produces repeated bamboo-like dots, bad seams, or wrong source rectangles.

### Terraria-style art acceptance rules

These are art-direction constraints inferred from the native/first-party sheets above rather than tModLoader API requirements:

- Draw at 1× native resolution with nearest-neighbor tools and no antialiasing.
- Keep the 2-pixel gutters transparent; never paint across source-cell padding.
- Give terrain distinct outer-edge, corner, isolated, horizontal, vertical, and interior frames. Interior frames need several low-frequency variations, not one noisy repeated motif.
- Use a small material palette with readable light-facing edges and darker mass/interior pixels. Black should be reserved for crevices and outlines; a mostly flat black fill cannot communicate soil depth.
- Separate structural function visually: load-bearing frame, wall panel, trim, window, hazard marking, and emissive detail should not all be baked into one generic block.
- Emissive pixels belong in a matching glowmask drawn with the same frame rectangle, as Calamity does for its laboratory console and server ([LaboratoryConsole.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Tiles/DraedonStructures/LaboratoryConsole.cs), [LaboratoryServer.cs](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Tiles/DraedonStructures/LaboratoryServer.cs)).

## 2. A complete, safe Maw infection registry

tModLoader supplies the correct extension point. `ModBiomeConversion` creates a custom conversion ID and exposes `PostSetupContent`, specifically so conversions depending on populated ID sets can be registered there ([ModBiomeConversion.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/ModLoader/ModBiomeConversion.cs#L8-L36)). `TileLoader` and `WallLoader` provide `RegisterConversion`, `RegisterSimpleConversion`, and conversion fallbacks ([TileLoader.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/ModLoader/TileLoader.cs#L712-L838), [WallLoader.cs](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/ModLoader/WallLoader.cs#L279-L457)).

ExampleMod proves the full loop: register source→infected conversion, mark a new source infectable when necessary, register reverse conversions, and call `WorldGen.SpreadInfectionToNearbyTile` from the infected tile's random update ([tile example](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Tiles/ExampleVanillaConversionTiles.cs#L14-L105), [wall example](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/ExampleMod/Content/Walls/ExampleVanillaConversionWalls.cs#L14-L51)). The spread helper also honors hardmode, Journey's spread toggle, Plantera slowdown, chlorophyte, and sunflower protection ([WorldGen API](https://docs.tmodloader.net/docs/stable/class_world_gen.html#details)).

### Registry design

Use one immutable registry as the source of truth for initial Maw world generation, runtime spread, Clentaminator-style conversion, restoration, and compatibility tests:

```csharp
public sealed record TileConversion(int Source, int Maw, int Waste, MawMaterial Family);
public sealed record WallConversion(int Source, int MawUnsafe, int WasteUnsafe, MawMaterial Family);

public sealed class MawBiomeConversion : ModBiomeConversion
{
    public override void PostSetupContent()
    {
        MawConversionRegistry.BuildAndValidate();

        foreach (TileConversion pair in MawConversionRegistry.Tiles)
            TileLoader.RegisterSimpleConversion(pair.Source, Type, pair.Maw, purification: false);

        foreach (WallConversion pair in MawConversionRegistry.Walls)
            WallLoader.RegisterSimpleConversion(pair.Source, Type, pair.MawUnsafe, purification: false);

        MawConversionRegistry.RegisterMawToWastePurification();
    }
}
```

`purification: false` is important for Apogean's established two-step ecology. The helper's default reverse registration would restore the original source directly. Instead, register Maw→Waste for purity/purification powder, then Waste→vanilla green through the later restoration conversion.

### Required material families

Build category mappings after ID sets are populated. The official conversion sets include dirt, grass, golf grass, jungle grass, mushroom grass, stone, moss, moss brick, sand, hardened sand, sandstone, ice, snow, thorns, and related families ([TileID conversion-set members](https://docs.tmodloader.net/docs/preview/class_tile_i_d_1_1_sets_1_1_conversion-members.html)). Wall conversion must independently cover grass/flower, dirt, stone and cave-rock families, snow, ice, sandstone, and hardened sand; tModLoader's own wall fallback table shows that these wall families are separate from tile conversion ([WallLoader fallback registrations](https://github.com/tModLoader/tModLoader/blob/b596b760ee90dc27d11dad756d955fb3f7da795e/patches/tModLoader/Terraria/ModLoader/WallLoader.cs#L340-L425)).

Apogean should add explicit allowlisted entries for natural materials outside those vanilla infection sets: mud, clay, silt, slush, ash/ash grass, desert fossil, living wood/leaf families, hive/honey families, granite, and marble. Each needs an authored Maw and Waste target or an intentional documented exemption. Do not infer naturalness from numeric tile ranges.

Safety rules:

- Never blanket-replace `TileLoader.TileCount` or every solid tile. Exclude dungeon/temple blocks and walls, chests, furniture, tracks, wires' host structures, altars, pylons, corporate materials, and protected authored structures.
- Ores require an explicit policy. Replacing an unknown modded ore destroys that mod's progression. Preserve it by default; known ores may opt into a Maw-coated variant that retains the original drop, or another mod may register a compatible pair through a small public API.
- Terraria does not normally record whether a stone block was naturally generated or player-placed. Like vanilla infection, a type-based registry affects both. “Natural only” would require persistent per-coordinate provenance data; do not pretend a tile-type check can distinguish them.
- Natural walls should convert to **unsafe** Maw walls so they do not become valid housing. Player-crafted safe walls should remain outside infection unless explicitly registered.
- Detect duplicate source registrations and missing reverse mappings at load time and fail with a useful message. Category priority must be deterministic where ID sets overlap.
- Use `WorldGen.ConvertTile` and `WorldGen.ConvertWall` for live conversion because they frame and network-sync the result; use `WorldGen.Convert(..., size: 0)` when invoking the registered conversion ID ([WorldGen conversion API](https://docs.tmodloader.net/docs/stable/class_world_gen.html)). A world-generation fast path may assign through the same registry in batches, but it must frame the changed region afterward.
- Trees should be converted through their supporting soil. `ModTree` shares vanilla tree tile ID 5 specifically so tree style can change when soil changes ([ModTree API](https://docs.tmodloader.net/docs/stable/class_mod_tree.html)). Maw soil must be in the dead/Maw tree's `GrowsOnTileId`; otherwise use `tryBreakTrees: true` when converting the support tile.

## 3. Fixed structures without floating clearance gaps

`GenVars.structures.CanPlace` and `AddProtectedStructure` are reservation/overlap tools. Padding protects an area from later world-generation features; it is not an instruction to erase that padding ([StructureMap documentation](https://docs.tmodloader.net/docs/stable/class_structure_map.html)). The campus placer must keep three concepts separate:

1. **Authored bounds:** exact dimensions of the schematic.
2. **Mutation mask:** only cells explicitly authored as tile, wall, liquid, clear-air, or object.
3. **Reservation bounds:** authored bounds plus compatibility padding passed to `AddProtectedStructure`.

The blueprint format needs at least these cell states: `KeepExisting`, `ClearTile`, `ClearWall`, `SetTile(full frame/state)`, `SetWall`, `SetLiquid`, and `ObjectMarker`. Empty/transparent schematic cells must mean keep-existing unless the author deliberately paints clear-air. Chests and tile entities need a post-stamp creation pass. Calamity's schematic system retains original tiles, supports explicit anchors, applies complete schematic cells, corrects flipped multi-tile frames through `TileObjectData`, creates chests, and recreates tile entities ([anchor/preflight](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L188-L257), [placement/framing](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L285-L330), [chests/tile entities](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs#L500-L565)).

### Surface-campus placement algorithm

For Helix, define a bottom-center foundation anchor and a separate public entrance anchor in the authored data.

1. Sample the first stable surface tile in every footprint column.
2. Reject cliffs or pits whose surface range exceeds the campus's authored tolerance.
3. Choose a robust foundation datum (normally the median/entrance surface), then position the dome so its visible shell is above that datum and lower laboratories extend downward.
4. Clear only authored interior/door/approach cells. Do not clear the whole rectangle or side padding.
5. For every authored foundation column, fill downward through air until stable natural terrain, with a maximum depth. If any required support exceeds that depth, reject and retry the site.
6. Add a short faction-material footing and a 2–6 tile natural terrain skirt, preserving the host biome outside it. The entrance ramp is authored and connected to sampled ground.
7. Frame tiles/walls around the mutation hull, place frame-important objects with valid origins/anchors, create chests/tile entities, then reserve the final occupied rectangle.
8. Assert after placement that there is no open-air run beneath a foundation cell and that the entrance has a walkable route to outside terrain.

Underground campuses use the same mutation-mask rule but can have an authored excavation shell. A two-tile maintenance cavity is acceptable only where the blueprint visibly designs one; an invisible rectangular gap is a placement failure.

## 4. What Remnants and Calamity actually do

### Remnants

Remnants depends on the external `StructureHelper` library ([build.txt](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/build.txt)). It inserts and replaces named world-generation passes deliberately ([StructurePasses](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L35-L65)). Its mineshaft first reserves a `StructureMap` area, prepares surrounding terrain, searches for the real surface, terraforms only the entrance route, and then stamps authored entrance/start modules ([Mineshafts](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L91-L155)).

For surface/embedded structures it also validates support and air before placement. The frozen watchtower requires a solid band below, clear spaces at its openings, correct biome membership, and a free protected area before stamping authored base/head modules ([Frozen Watchtower](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L5748-L5801)). Its magical lab demonstrates the other side of the design: generate an enclosing biome/material envelope, protect the area, then assemble authored room schematics on a procedural topology ([MagicalLab](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Dungeons.cs#L2580-L2655)).

**Apogean use:** copy the pattern, not the dependency blindly. Use fixed whole-campus schematics for signature headquarters; use Remnants-style authored modules plus procedural topology later for repeatable ruins, labs, and dungeons.

### Calamity

Calamity's Draedon structures are fixed schematics with biome-specific preflight. The common avoidance test rejects lava, dungeon, temple, evil, and Calamity biome materials ([DraedonStructures.ShouldAvoidLocation](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs#L20-L62)). Workshop placement measures the exact schematic, scans its area, rejects nearby labs and occupied structure-map regions, stamps the schematic, then protects its bounds ([workshop placement](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs#L101-L139)). Ice and plague labs additionally require a threshold of host-biome tiles before placement ([ice lab](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs#L431-L481), [plague lab](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs#L519-L574)). Their dedicated pass places biome labs first and then scales workshop/facility counts by world width ([WorldgenManagementSystem](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Systems/World/WorldgenManagementSystem.cs#L313-L359)).

**Apogean use:** Kessler, Helix, and Sentrix should follow the fixed-schematic model with faction-specific site preflight and full authored furniture. World-size scaling should control optional outposts, not distort the signature headquarters.

## 5. Repair order and proof gates

1. **Contract harness first:** add a development gallery that places every terrain material beside dirt, stone, sand, slopes, half-blocks, paint/coating, and its own material. Place every furniture orientation/style and all platform frames.
2. **Rebuild Wastes family:** Waste dirt/turf, stone, sand families, walls, sapling, and complete tree atlases. Do not continue until tree growth, chopping, shaking, soil conversion, and save/reload pass.
3. **Rebuild Maw family:** one complete visual family per conversion category, including unsafe natural walls. Validate every target sheet before registering spread.
4. **Implement registry:** load-time duplicate/reverse-map checks, conversion matrix command, and protected-tile exclusions. Test runtime spread, solution conversion, multiplayer sync, and two-step Maw→Waste→green restoration.
5. **Rebuild faction construction kits:** structural terrain, trim, wall, window, platform, door pair, light, chair, table, workbench, console, storage, signature animated object, and glowmask where needed. A shell made from one dotted block is not a completed kit.
6. **Upgrade schematic data:** preserve full tile state and explicit keep/clear masks; place objects, chests, and tile entities after the terrain stamp.
7. **Terrain-integrate campuses:** Helix surface datum/foundations first, then Kessler ground compound, then Sentrix sky structure. Add automated no-floating-foundation and reachable-entrance assertions.
8. **World matrix:** generate at least three seeds for every supported world size and evil, with and without major compatibility mods. Record structure bounds, host biome, support depth, protected overlaps, conversion counts by family, and generation exceptions.

Definition of done is an in-game captured validation world where the material gallery, dead trees, Maw conversion matrix, and all three furnished campuses render correctly after save/reload. Build success and blueprint preview PNGs are necessary checks, but they are not visual acceptance.
