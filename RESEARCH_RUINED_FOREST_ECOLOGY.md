# Ruined Surface Forest Ecology — Implementation Report

## Scope and source baseline

This report targets the installed stable build, tModLoader `1.4.4.9+2026.07.3.0`, whose embedded source revision is [`666f69962d3bdffde54fc14025f02634965b4e7c`](https://github.com/tModLoader/tModLoader/tree/666f69962d3bdffde54fc14025f02634965b4e7c). Facts about vanilla method bodies and pass indexes were verified against the installed `tModLoader.dll`; tModLoader's public repository contains patches rather than every complete Terraria method body. API guidance comes from the official [`ModTree`](https://docs.tmodloader.net/docs/stable/class_mod_tree.html) and [`ModSystem`](https://docs.tmodloader.net/docs/stable/class_mod_system.html) documentation, official [ExampleTree](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Tiles/Plants/ExampleTree.cs), [ExampleSapling](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Tiles/Plants/ExampleSapling.cs), and [ModPlants source](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModPlants.cs).

## Recommended architecture

Apogean should not replace or redefine vanilla `TileID.Grass`. It should retain vanilla Grass as a valid, restorable player resource while using a separate `DeadGrass` ModTile for the world's initially ruined surface. A terminal world-generation pass should convert only the final, naturally generated purity-forest ecology. This gives Apogean the dead-world appearance without breaking vanilla building materials, Dryad restoration, tree resource loops, or compatibility with systems that recognize vanilla Grass.

The replacement pass should run only during new-world generation. It should preserve ordinary `TileID.Trees` trunks and change their rendering and lifecycle through a `ModTree` registered for `DeadGrass`. Living Trees should remain traversable structures with their wood, rooms, walls, furniture, chests, and loot untouched; only their generated green crowns should be removed. Safe, player-placeable walls must never be globally converted.

## DeadGrass, spreading, conversion, and restoration

Installed vanilla `WorldGen.SpreadGrass` operates on explicit source and destination tile types; making a ModTile resemble grass does not automatically give it all vanilla grass behavior. The official tModLoader issue tracking full custom-grass support documents additional hardcoded grass interactions, including placement, liquid, mining, mowing, flowers, and seed behavior ([tModLoader issue #4507](https://github.com/tModLoader/tModLoader/issues/4507)). Apogean should therefore keep `DeadGrass` deliberately narrow: framed as custom terrain, mineable to Dirt, valid as dead-tree soil, and non-spreading unless a later design explicitly adds controlled propagation.

Vanilla Grass Seeds (`ItemID.GrassSeeds`, 62) are still sold by the Dryad through tModLoader's official [NPC shop database](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/NPCShopDatabase.cs). Installed placement logic accepts those seeds on vanilla Dirt (`TileID.Dirt`, 0), not on arbitrary custom grass. The reliable restoration loop is therefore:

1. Mine DeadGrass and receive Dirt.
2. Place or expose Dirt.
3. Use Dryad Grass Seeds to create vanilla Grass.
4. Allow vanilla Grass to spread through adjacent Dirt normally.

Directly using a vanilla Grass Seed on DeadGrass would require a custom, server-authoritative interaction and must be treated as an optional convenience feature, not assumed vanilla behavior. A custom reclamation seed is a safer alternative if one-step conversion is later desired.

`TileID.Sets.Conversion.Grass[DeadGrass]` is a design switch, not a prerequisite for `ModTree`. Enabling it lets conversion systems treat DeadGrass as grass, but may also make evil, Hallow, or purification conversions overwrite it. Apogean should decide this policy explicitly and test it against the planned Ingraft-contamination rules.

## Normal trees and the renewable resource lifecycle

`ModTree` intentionally shares the vanilla tree tile, `TileID.Trees` (5). Its `GrowsOnTileId` soil registration lets tModLoader select a tree's textures and behavior from the supporting tile; this also permits an existing ordinary tree to change appearance when its soil changes. Apogean's forest resource tree should therefore remain `TileID.Trees`, rooted on `DeadGrass`, rather than being replaced by a fixed decorative multi-tile.

The dead forest `ModTree` should implement this lifecycle:

- `GrowsOnTileId` contains `DeadGrass`.
- `CountsAsTreeType` returns the forest category so vanilla forest shake loot remains appropriate.
- `DropWood()` returns ordinary Wood (`ItemID.Wood`, 9), preserving base-building progression.
- `CanDropAcorn()` returns true.
- `SaplingGrowthType` returns Apogean's dead-forest sapling.
- The sapling follows official ExampleMod tile-object data and calls `WorldGen.GrowTree` during random updates.
- `TreeLeaf()` returns `-1`, and `Shake(..., ref createLeaves)` sets `createLeaves = false`.

tModLoader's plant loader intercepts vanilla sapling placement and asks the supporting soil which sapling should be created. Consequently, an Acorn (`ItemID.Acorn`, 27) used on DeadGrass can become the custom dead sapling, which later grows a renewable `TileID.Trees` tree and drops ordinary Wood and Acorns. A separate custom `DeadTree` tile may be used for decorative snags, but it should not replace the resource-tree system.

If trees appear absent in a playtest world, test a newly generated world first. `ModifyWorldGenTasks` does not retroactively populate an older save, and registering a `ModTree` cannot recreate trunks removed by an earlier build.

## Leaf-particle suppression

For normal trees, `ModTree.TreeLeaf()` supplies the gore used by growth, shaking, and wind-driven leaves. Returning `-1`, combined with clearing `createLeaves` during shaking, is the supported no-leaf implementation.

Living Tree crowns are different. Installed tile drawing contains a hardcoded particle branch for `TileID.LeafBlock` (192), emitting normal tree-leaf gore. `GlobalTile.EmitParticles` cannot cancel the later vanilla branch. The clean solution is to remove the generated `LeafBlock` crown itself. This simultaneously removes the green canopy and its falling leaves without a fragile rendering detour.

## Living Trees and natural walls

The terminal pass should identify generated Living Tree components and remove only `TileID.LeafBlock` tiles connected to their `TileID.LivingWood` (191) structure, plus vines anchored to those leaves. It must preserve Living Wood, `WallID.LivingWoodUnsafe` (244), doors, platforms, rooms, furniture, Living Looms, containers, chest contents, liquids, wires, and open routes. It should not remove or fill air inside the tree.

`WallID.LivingLeaf` (60) is a safe wall. The installed vanilla Living Tree generator does not use it for normal tree routes, although players and other mods can place it. Never globally replace or erase it. This distinction is especially important for existing worlds.

The vanilla `Grass Wall` generation pass creates natural unsafe `WallID.GrassUnsafe` (63) and sometimes `WallID.FlowerUnsafe` (65). These are the correct walls to replace with Apogean's dead unsafe wall variants inside the ruined surface domain. Preserve safe `WallID.Grass` (66), safe `WallID.Flower` (68), safe Living Leaf wall (60), and all unrelated biome walls. Plants (`TileID.Plants`, 3), Plants2 (73), sunflowers (27), and vines (52) should be removed or replaced only when rooted in the converted target soil; broad depth-wide deletion risks damaging other biomes and structures.

## World-generation order and safe placement

The installed build contains 107 vanilla passes. The ecology-relevant order is: `43 Living Trees`, `44 Wood Tree Walls`, `74 Spreading Grass`, `76 Place Fallen Log`, `80 Grass Wall`, `82 Sunflowers`, `83 Planting Trees`, `84 Herbs`, `85 Dye Plants`, `87 Weeds`, `90 Vines`, `91 Flowers`, `99 Cactus, Palm Trees, & Coral`, `100 Tile Cleanup`, `102 Micro Biomes`, and `106 Final Cleanup`.

Apogean should locate `Final Cleanup` by name in `ModifyWorldGenTasks` and insert one terminal ruined-ecology pass immediately after it, with a guarded fallback if the name is unavailable. Pass indexes and names are implementation details and may change across tModLoader versions; name lookup is safer than a fixed index. Running after final cleanup ensures vanilla has already generated trees, special trees, vegetation, walls, and micro-biomes before Apogean examines the completed result.

The pass should first record its target mask, tree roots, and Living Tree components, then mutate tiles. It should use `WorldGen.genRand`, convert only purity Grass in the configured ruined-surface band, preserve evil/jungle/desert/snow-specific tiles, normalize Sakura and Willow trees only when rooted in the target mask, replace anchored vegetation and unsafe walls, remove Living Tree crowns, and finally frame changed boundaries plus a one-tile halo. Let vanilla generate resource geometry first; do not replace `Planting Trees` wholesale.

[Remnants' Terrain pass](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Terrain.cs) demonstrates explicit pass removal and replacement, while its [Structures source](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs) authors giant trees and reserves structure space. Those are source facts, but using Remnants' full-world replacement model for Apogean would be an inference with a much larger compatibility cost. Terraria Overhaul's current [TreeFallingSystem](https://github.com/Mirsario/TerrariaOverhaul/blob/e202f4719bdff845035e352ba70169b7022cbe09/Common/TreeFalling/TreeFallingSystem.cs) recognizes vanilla tree trunks and bypasses falling animation during world generation; retaining `TileID.Trees` is therefore the more compatible design. Its current public source does not substantiate a custom dead-grass worldgen system.

## Multiplayer and existing-world policy

New-world generation is server/single-player authoritative and is saved before clients use the world, so the terminal pass should mutate tiles directly without sending a network packet per tile. Persist an ecology schema version through `ModSystem.SaveWorldData`; synchronize only compact world flags through `NetSend` and `NetReceive`.

Existing saves have no reliable provenance bit distinguishing world-generated grass, leaves, walls, or trees from player-placed copies. Automatic global retrofitting could destroy builds and other mods' structures. The supported default should be “new Apogean world required.” Any future migration must be explicit, backup-gated, server-only, bounded over time, versioned, resumable, and conservative; it must never globally replace safe walls or Living Leaf blocks. Runtime restoration interactions must validate target, range, held item, and consumption on the server, then synchronize the changed tile square.

## Playtest acceptance checklist

- Generate small, medium, and large new worlds without worldgen exceptions; confirm deterministic results for a fixed seed.
- Confirm the purity surface is ruined while jungle, evil, desert, snow, and protected structures retain their own tile families.
- Verify a practical number of dead trees exists near spawn and across the world.
- Chop a dead tree: it drops ordinary Wood and can drop Acorns.
- Plant an Acorn on DeadGrass: the custom sapling appears, grows, and remains renewable after reload.
- Shake and observe dead trees in wind: no leaf gore appears.
- Mine DeadGrass to Dirt, use Dryad Grass Seeds, and confirm vanilla Grass returns and spreads through Dirt.
- Confirm Living Trees have bare crowns but intact trunks, tunnels, rooms, walls, doors, furniture, Living Looms, chests, loot, wires, and liquids.
- Confirm unsafe natural grass/flower walls become dead variants while safe player walls remain unchanged.
- Confirm Sakura/Willow normalization affects only target ruined-forest trees.
- Reload and join a dedicated multiplayer world; compare tiles, trees, walls, chests, and ecology-version flags between server and clients.
- Test with Terraria Overhaul enabled: dead resource trees still chop correctly and world generation does not trigger falling-tree animation.
- Open a pre-Apogean world and verify no automatic destructive conversion occurs.

These decisions preserve Terraria's core resource and restoration loops while making the naturally generated surface visibly dead. The main version-sensitive points are pass names/order, hardcoded Grass interactions, Living Leaf particle logic, and Sakura/Willow frame compatibility; re-verify them whenever the installed tModLoader branch changes.
