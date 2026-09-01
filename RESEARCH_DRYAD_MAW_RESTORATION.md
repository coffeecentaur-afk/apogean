# Dryad Reporting and Maw Restoration Research

Status: primary-source technical report for Wayfinder research issue 18; no implementation changes

Target: Apogean on locally installed tModLoader stable `2026.07.3.0`

## Evidence baseline and labels

The installed `E:\SteamLibrary\steamapps\common\tModLoader\tModLoader.dll` reports product version `1.4.4.9+2026.07.3.0|2026.07|stable|Stable|666f69962d3bdffde54fc14025f02634965b4e7c|...`. Its matching local tModLoader source checkout is commit [`666f69962d3bdffde54fc14025f02634965b4e7c`](https://github.com/tModLoader/tModLoader/tree/666f69962d3bdffde54fc14025f02634965b4e7c). Assembly IL was inspected when unchanged Terraria method bodies are not present in patch form.

Labels used throughout:

- **Proven:** directly visible in the exact installed assembly, matching tModLoader source, official tModLoader documentation/ExampleMod, or Terraria/tModLoader source patches.
- **Inference:** follows from proven control flow or data layout but is not guaranteed as a public API contract.
- **Recommendation:** the proposed Apogean implementation or content rule.

Only first-party tModLoader/Terraria evidence is used. No third-party mod behavior is treated as authoritative.

## Executive conclusion

1. **Proven:** tModLoader has no dedicated hook that appends another percentage inside `Lang.GetDryadWorldStatusDialog`. The least fragile supported route is `GlobalNPC.PreChatButtonClicked` on the Dryad's second button: call the vanilla status method, append localized Maw status, assign `Main.npcChatText`, and return `false`. A detour of `Lang.GetDryadWorldStatusDialog` preserves all downstream vanilla button behavior but is a higher compatibility risk. [`GlobalNPC` chat hooks](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/GlobalNPC.cs#L671-L710) [Dryad button insertion point](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L4778-L4791)
2. **Proven:** `ModSystem.TileCountsAvailable` reports the scene scan around a player and is client-only. It is suitable for entering a local Maw biome, not a whole-world Dryad percentage. [`ModSystem.TileCountsAvailable`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L441-L446) [scene-count integration](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/SceneMetrics.cs.patch#L159-L212)
3. **Proven:** the installed Terraria assembly computes vanilla Dryad percentages through an amortized whole-world column scan in `WorldGen.CountTiles(int)`. It scans one column every 30 world updates, weights surface tiles by five and deeper tiles by one, rounds each alignment count against `totalSolid`, forces nonzero contamination to display at least 1%, sends vanilla world-alignment data from the server, then clears its private working counts. There is no public callback before that clear. **Recommendation:** do not IL-hook this scanner; maintain an Apogean server/SP scanner with per-column Maw counts and publish an atomic cached result.
4. **Proven:** custom solution ammo, spray projectiles, custom conversion IDs, tile/wall conversion delegates, fallbacks, framing, and network synchronization are all directly supported. Official ExampleMod is a complete reference. [`ExampleSolution`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Items/Ammo/ExampleSolution.cs) [`ModBiomeConversion`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBiomeConversion.cs) [conversion registry](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/TileLoader.cs#L730-L878)
5. **Recommendation:** enforce the agreed two-stage ecology with two conversion types: Apogean **Sterilant** converts Maw families to matching Wastes families; vanilla Green Solution (`BiomeConversionID.Purity`) converts Wastes families to their matching vanilla families. Green Solution should not directly convert Maw. This makes two passes mechanically real rather than merely descriptive.
6. **Proven:** Terraria tiles do not carry a persistent “player placed” provenance flag. `PlaceInWorld` is called on the local client and single-player, not as an authoritative server placement record. **Recommendation:** protect explicit saved regions and safe housing walls, and avoid converting crafted wood/furniture; do not promise exact provenance protection without a separate synchronized placement ledger. [`GlobalBlockType.PlaceInWorld`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/GlobalBlockType.cs#L104-L119)

## 1. Dryad dialogue: supported hooks and detour boundary

### 1.1 Vanilla data and call path

**Proven:** `Lang.GetDryadWorldStatusDialog(out bool worldIsEntirelyPure)` is public. The installed method reads `WorldGen.tGood`, `WorldGen.tEvil`, and `WorldGen.tBlood`, selects localized combinations for Hallow/Corruption/Crimson, sets the out parameter only for vanilla purity, and appends a qualitative world description. The source patch exposes the public method but does not add an extension hook inside it. [`Lang` patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Lang.cs.patch#L91-L99)

**Proven:** when the second chat button is clicked, tModLoader calls `NPCLoader.PreChatButtonClicked(false)`, then `NPCLoader.OnChatButtonClicked(false)`, and only afterward vanilla calls `Lang.GetDryadWorldStatusDialog` for `NPCID.Dryad`. Returning false from the pre-hook exits before vanilla's branch. [`Main.GUIChatDrawInner` patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L4778-L4791) [`NPCLoader` dispatch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/NPCLoader.cs#L1200-L1264)

**Proven:** `GlobalNPC.GetChat` modifies the ordinary opening chat string. It does not run after the Dryad's status button replaces `Main.npcChatText`. `GlobalNPC.OnChatButtonClicked` runs before the vanilla replacement, so text assigned there is overwritten. [`GlobalNPC.GetChat`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/GlobalNPC.cs#L681-L710)

### 1.2 Supported implementation

**Recommendation:** implement a `GlobalNPC.PreChatButtonClicked` handler with this narrow condition:

```text
npc.type == NPCID.Dryad && firstButton == false
```

It should:

1. call `Lang.GetDryadWorldStatusDialog(out vanillaPure)`;
2. read the synchronized cached `MawTileCount`, `MawPercent`, and scan-ready flag;
3. append a localized Maw sentence;
4. derive Apogean purity as `vanillaPure && MawTileCount == 0`;
5. assign `Main.npcChatText` and return `false`.

Suggested semantic states are finite and localization-friendly:

| State | Dryad addition |
|---|---|
| Scan not yet complete | “The Maw's reach is still being surveyed.” |
| Maw count is nonzero but rounds below 1% | “The Maw infects less than 1% of the world.” |
| Maw percentage 1–99 | “The Maw infects {Percent}% of the world.” |
| Maw zero, Wastes remain | “The Maw is absent. The Wastes remain dormant, not restored.” |
| Maw zero and vanilla world pure | A distinct full-purity line may acknowledge that active contamination is gone; Wastes remain neutral and must not make vanilla purity false. |

**Hazard:** intercepting the button bypasses the remainder of vanilla's Dryad branch. In the exact installed assembly that branch also checks the Joja Cola/Stardew animation path after obtaining the status. This is a rare behavior loss, but it is real.

### 1.3 Optional detour

**Proven:** `MonoModHooks.Add(MethodBase, Delegate)` is an officially exposed runtime-detour mechanism, and tModLoader tracks and unloads mod-owned hooks. [`MonoModHooks.Add`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/MonoModHooks.cs#L149-L171)

**Recommendation:** use a detour on `Lang.GetDryadWorldStatusDialog` only if preserving the complete vanilla button branch is a hard requirement. The hook should call `orig(out vanillaPure)`, append Maw text, and update the returned out value to `vanillaPure && MawCount == 0`. It must never replace vanilla localization selection.

**Hazards:** method signature changes, another mod detouring the same method incorrectly, hook ordering, and future Terraria changes all raise maintenance cost. A full IL edit of `Main.GUIChatDrawInner` is worse because it depends on local layout. Prefer the supported pre-button interception for the first implementation and record the accepted Joja behavior tradeoff.

## 2. Whole-world Maw percentage

### 2.1 Why scene counts are not enough

**Proven:** `SceneMetrics` counts a bounded area used for the local player's active scene and passes the resulting `ReadOnlySpan<int>` to `ModSystem.TileCountsAvailable`. The hook is called on clients. It is not the world-alignment scanner and cannot support a stable Dryad world percentage. [`SceneMetrics` scan and hook](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/SceneMetrics.cs.patch#L159-L212) [`ModSystem` documentation](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L441-L446)

**Recommendation:** continue using `TileCountsAvailable` for local Maw biome activation only. Keep its threshold independent from the Dryad percentage denominator.

### 2.2 Installed vanilla scanner facts

**Proven from installed assembly IL:** `WorldGen.UpdateWorld_Inner` calls `WorldGen.CountTiles(totalX)` after `totalD` reaches 30, then increments `totalX`. `CountTiles` scans a single world column, using weight 5 from the top through approximately `Main.worldSurface + 1` and weight 1 below. At the end of all columns, `AddUpAlignmentCounts(false)` sums registered evil/Hallow families and a hardcoded `totalSolid` family, then clears `WorldGen.tileCounts`. On the next cycle start, the previous totals become published `tGood/tEvil/tBlood` bytes and multiplayer server message 57 is sent.

**Inference:** a large world may take a long time to refresh vanilla alignment because work is intentionally amortized. A Maw percentage need not update every shot; it must avoid a full-world hitch and eventually converge.

**Proven:** tModLoader resizes `WorldGen.tileCounts` for modded tile IDs, but there is no `ModSystem` hook around `WorldGen.AddUpAlignmentCounts` before its array clear. [`TileLoader` resize](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/TileLoader.cs#L185-L200) [world tile-count declaration patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/WorldGen.cs.patch#L30-L38)

**Recommendation:** do not read `WorldGen.tileCounts` opportunistically and do not IL-hook its clear. Both approaches depend on undocumented timing.

### 2.3 Apogean scanner architecture

**Proven:** `ModSystem.PostUpdateWorld` is called only in single-player or on the server, making it an appropriate authority for bounded world-state work. [`PostUpdateWorld`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L226-L236)

**Recommendation:** maintain two integer arrays indexed by world X: `mawWeightByColumn` and `eligibleWeightByColumn`, plus global sums. Scan on the main world thread with a strict time or tile budget:

1. On world load, mark every column dirty and keep the last saved percentage marked stale.
2. In `PostUpdateWorld`, rescan columns until a small fixed budget is exhausted.
3. For each active tile, add weight 5 at/above the vanilla surface boundary and 1 below, matching vanilla reporting emphasis.
4. Classify the numerator by an Apogean ID set containing only active Maw terrain. Do not count decorative plants, furniture, nodes, walls, or enemies as contamination percentage.
5. Classify the denominator by explicit convertible terrain families: vanilla purity terrain, Wastes terrain, and Maw terrain. Do not divide by every solid block, buildings, ores, furniture, or empty world area.
6. Replace a column's old contribution atomically when its scan completes.
7. Publish a new snapshot after a full initial pass and thereafter when a full rolling pass finishes or dirty columns settle.

**Recommendation:** compute display percentage with the vanilla convention: round to the nearest whole percent and display 1% when the Maw numerator is nonzero but rounding yields zero. Store raw numerator and denominator as 64-bit integers to avoid overflow and to support diagnostics.

**Inference:** per-column storage turns rescanning after a large spray into replacement rather than error-prone deltas. A continuously rolling low-budget scan also repairs changes made by other mods through direct tile writes that bypass conversion hooks.

**Hazards:** never scan the mutable `Main.tile` map from a background thread; no reviewed API promises thread safety. Never scan the whole world on Dryad click. Never publish a partially reset numerator as a new percentage.

## 3. Custom solution and conversion pipeline

### 3.1 Supported pieces

**Proven:** official ExampleMod defines a solution item with `Item.DefaultToSolution(projectileType)`, a spray projectile with `Projectile.DefaultToSpray()`, a custom `ModBiomeConversion`, and calls `WorldGen.Convert(centerX, centerY, conversionType, size)` from projectile AI. It handles the Terraformer's larger area through `Projectile.ai[1]`. [`ExampleSolution` item/projectile](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Items/Ammo/ExampleSolution.cs#L11-L95)

**Proven:** `ModBiomeConversion` supplies a modded conversion ID and a `PostSetupContent` stage intended for `TileLoader.RegisterConversion` and `WallLoader.RegisterConversion` after ID sets are populated. It does not itself perform conversion. [`ModBiomeConversion`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBiomeConversion.cs)

**Proven:** `TileLoader.RegisterConversion` and `WallLoader.RegisterConversion` accept either a destination type or a delegate. Returning `false` from a delegate tells conversion dispatch that custom behavior handled or blocked the conversion. `RegisterSimpleConversion` also creates a fallback and optional purity/powder reverse conversions. [`TileLoader` conversion API](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/TileLoader.cs#L730-L878) [`WallLoader` conversion API](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/WallLoader.cs#L281-L337)

**Proven:** `WorldGen.Convert` invokes modded wall and tile conversion first, skips vanilla handling when a delegate returns false, and emits `OnTileConverted`/`OnWallConverted` once at the outermost conversion recursion when a type changed. [`WorldGen.Convert` patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/WorldGen.cs.patch#L1952-L1977) [dispatch and notifications](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/WorldGen.cs.patch#L1979-L2004) [outer notification](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/WorldGen.cs.patch#L2253-L2264)

### 3.2 Two-stage conversion contract

**Recommendation:** define exactly one custom conversion ID for the first stage:

```text
Maw family --Apogean Sterilant--> Wastes family
Wastes family --BiomeConversionID.Purity/Green Solution--> vanilla family
```

Green Solution should have no registered conversion for Maw terrain. Purification Powder may clean only explicitly designated fringe Maw tiles to Wastes. Core Maw terrain and Nodes return false without changing.

**Recommendation:** do not use `RegisterSimpleConversion` blindly for this pipeline. Its automatic reverse registrations include both `BiomeConversionID.Purity` and `BiomeConversionID.PurificationPowder`, which can accidentally collapse progression. Register each direction explicitly so Sterilant, Green Solution, and Powder have distinct permissions.

**Hazard:** if both Maw→Wastes and Wastes→vanilla use `BiomeConversionID.Purity`, a single lingering Green Solution spray can hit the same coordinate on successive AI updates and perform both stages. Separate conversion IDs avoid that failure.

### 3.3 Family-preserving mapping

**Recommendation:** Maw and Wastes need parallel tile identities for every terrain family that must restore accurately. One generic `MawBlock` cannot remember whether it originated as grass, stone, sand, ice, or jungle terrain.

| Origin family | Active Maw | Sterilized Wastes | Green restoration | Notes |
|---|---|---|---|---|
| Dirt | `MawDirt` | `WastesDirt` | `TileID.Dirt` | Dirt does not need grass behavior. |
| Forest grass | `MawGrass` | `DeadGrass`/`WastesGrass` | `TileID.Grass` | Soil conversion controls generic surface-tree style. |
| Stone/moss | `MawStone` | `WastesStone` | `TileID.Stone` | Decide whether moss is discarded; base purity is safest. |
| Sand | `MawSand` | `WastesSand` | `TileID.Sand` | Set correct sand/falling/conversion ID sets on both custom tiles. |
| Hardened sand | `MawHardenedSand` | `WastesHardenedSand` | `TileID.HardenedSand` | Separate from loose sand. |
| Sandstone | `MawSandstone` | `WastesSandstone` | `TileID.Sandstone` | Separate wall mapping too. |
| Ice | `MawIce` | `WastesIce` | `TileID.IceBlock` | Preserve ice collision/conversion behavior. |
| Snow | `MawSnow` | `WastesSnow` | `TileID.SnowBlock` | Snow and ice are distinct families. |
| Mud | `MawMud` | `WastesMud` | `TileID.Mud` | Do not restore as dirt. |
| Jungle grass | `MawJungleGrass` | `WastesJungleGrass` | `TileID.JungleGrass` | Preserves the Jungle rather than erasing it. |
| Mushroom grass, if infectable | `MawMushroomGrass` | `WastesMushroomGrass` | `TileID.MushroomGrass` | Include only if design permits Maw conversion there. |
| Unsafe natural walls | parallel family | parallel Wastes unsafe wall | matching vanilla unsafe wall | Never map an unsafe natural wall to a housing-safe wall. |

**Proven:** conversion sets such as `TileID.Sets.Conversion.Sand` let vanilla and modded conversions recognize family behavior; ExampleMod applies this to custom sand. [`ExampleSand`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Tiles/ExampleSand.cs#L12-L31)

**Recommendation:** every family should have a single registration table used by Sterilant, Green restoration, scanner classification, map colors, spread eligibility, and tests. Do not duplicate switch statements across projectiles, tiles, and the Dryad system.

## 4. Grass, trees, plants, wood, and walls

### 4.1 Trees and grass

**Proven:** `WorldGen.ConvertTile` attempts to kill trees that would become invalid, changes the tile type, frames the area, and synchronizes it. [`WorldGen.ConvertTile`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/WorldGen.TML.cs#L40-L63)

**Proven:** official ExampleMod explains that `ModTree` shares vanilla generic tree tile IDs and automatically changes visual tree style when the supporting soil changes. Gem trees, vanity Sakura/Willow, ash trees, and plants that do not share that behavior require explicit conversion handling. [`ExampleSolution` tree conversion](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Items/Ammo/ExampleSolution.cs#L116-L241) [`ExampleHellSolution` tree/plant handling](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Items/Ammo/ExampleHellSolution.cs#L93-L222)

**Recommendation:** retain generic `TileID.Trees` for the ordinary Wastes forest and make its appearance soil-driven through the Wastes `ModTree`. Then `DeadGrass -> TileID.Grass` naturally changes those trunks to vanilla forest trees without replacing every trunk tile. Handle vanity trees and custom Maw tree objects explicitly or exclude them from conversion until a tested mapper exists.

**Recommendation:** convert or reframe plants/vines only after or as part of their floor change. ExampleMod documents that otherwise vines can break and shows a `TileFrame` self-correction pattern. [`ExampleVine`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Tiles/Plants/ExampleVine.cs#L119-L148)

### 4.2 Wood and constructed objects

**Proven:** vanilla biome conversion targets terrain families; ExampleMod explicitly registers chairs/workbenches because those objects are not normally solution-converted. [`ExampleSolution` object registrations](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Items/Ammo/ExampleSolution.cs#L109-L115)

**Recommendation:** do not convert Wood, Living Wood, crafted Wastes Wood, platforms, doors, furniture, containers, wires, or frame-important structures. Trees should restore through soil/tree logic; harvested construction materials should remain what the player built. If a visual “natural dead log” must restore, give it a distinct natural-rubble tile rather than sharing an item-placeable wood tile.

### 4.3 Walls

**Proven:** `Main.wallHouse[type]` distinguishes housing-safe wall types. ExampleMod uses it to preserve safe/unsafe status during custom wall conversion. `WorldGen.ConvertWall` changes the wall, frames it, and synchronizes the tile square. [`ExampleSolution` wall conversion](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Items/Ammo/ExampleSolution.cs#L262-L272) [`WorldGen.ConvertWall`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/WorldGen.TML.cs#L65-L84)

**Recommendation:** register conversions only for Apogean natural unsafe walls. Keep crafted safe wall variants outside all spread and solution tables. If a generic callback accepts both, map safe→safe and unsafe→unsafe; never use wall texture resemblance as the distinction.

**Proven limitation:** the exact vanilla Purification Powder AI calls `TileLoader.Convert(..., BiomeConversionID.PurificationPowder)` before its hardcoded tile conversion, but it does not call `WallLoader.Convert`. It only reports a wall conversion notification if vanilla hardcoded behavior changed a wall. [`Projectile` powder patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Projectile.cs.patch#L1565-L1590)

**Recommendation:** pre-Hardmode powder may intentionally sterilize fringe Maw tiles but leave walls, or Apogean may add a small custom powder projectile/`GlobalProjectile` extension that calls `WorldGen.Convert` for custom walls. Do not claim that `WallLoader.RegisterConversion` alone makes vanilla Powder cleanse custom walls.

## 5. Player builds and housing protection

### 5.1 What the engine does not store

**Proven:** the reviewed `Tile` representation and tile hooks expose material/frame/liquid/wire/coating state, not persistent origin provenance. `GlobalTile/GlobalWall.PlaceInWorld` is documented as local-client and single-player. [`GlobalBlockType.PlaceInWorld`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/GlobalBlockType.cs#L104-L119)

**Inference:** after save/load, natural `WastesStone` and player-placed `WastesStone` are indistinguishable unless Apogean records separate metadata. A conversion delegate cannot infer provenance from tile type alone.

### 5.2 Finite protection policy

**Recommendation:** use layered, explicit protection:

1. Never convert furniture, containers, wood blocks, platforms, doors, wires, or other constructed-object families.
2. Never spread through or solution-convert housing-safe wall types unless an explicit restoration action requests it.
3. Query the shared Apogean protected-region registry for spawn sanctuary, corporations, arenas, authored structures, and player-registered settlements before any Maw spread or custom solution conversion.
4. Give players a later-game **stabilizer/settlement beacon** that registers a bounded protected rectangle. Save it through `ModSystem.SaveWorldData/LoadWorldData` and synchronize it.
5. Treat occupied NPC home coordinates as an additional coarse guard if desired, but do not run Terraria's full room validation for every spray tile.

**Proven:** `ModSystem` supports paired world persistence and world-data synchronization hooks. [`SaveWorldData/LoadWorldData`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L321-L336) [`NetSend/NetReceive`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L363-L377)

**Recommendation:** do not build a per-tile provenance ledger in the first release. If exact provenance is later mandatory, use a compressed coordinate/chunk bitset, validate client placement claims on the server, remove records on destruction, version the save format, and cap memory. This is a separate feature, not a hidden addition to the solution system.

**Hazard:** `Main.wallHouse[type]` says a wall type can form housing; it does not prove a valid room exists at that coordinate. It is a cheap safety heuristic, not a housing-query API.

## 6. Multiplayer, packets, framing, and performance

### 6.1 Tile authority and synchronization

**Proven:** official ExampleMod runs spray conversion when `Projectile.owner == Main.myPlayer`, matching Terraria's owner-side projectile pattern, and calls `WorldGen.Convert`. [`ExampleSolutionProjectile.AI`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Items/Ammo/ExampleSolution.cs#L49-L95)

**Proven:** `WorldGen.ConvertTile` and `ConvertWall` perform framing and call `NetMessage.SendTileSquare` when not in single-player. [`WorldGen.TML`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/WorldGen.TML.cs#L40-L84)

**Recommendation:** follow the official projectile pattern for the initial solution implementation. Keep protection-region data deterministic and synchronized to clients because the owner performs the conversion decision. The server's world tile map remains authoritative for the Dryad scanner.

**Hazard:** direct `tile.TileType = ...` writes bypass automatic framing/sync and may bypass conversion notifications. Use `WorldGen.ConvertTile/ConvertWall`, or if batching direct writes after profiling, explicitly frame, notify, and send bounded tile rectangles.

**Proven:** `OnTileConverted`/`OnWallConverted` hooks exist for observing conversions, but they fire through the conversion pipeline, not for every arbitrary tile write, placement, or mining action. [`GlobalTile.OnTileConverted`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/GlobalTile.cs#L400-L403) [`GlobalWall.OnWallConverted`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/GlobalWall.cs#L55-L58)

### 6.2 Percentage synchronization

**Recommendation:** synchronize only the published Dryad snapshot, not per-tile count changes:

- Include numerator, denominator, whole percentage, and ready/stale state in `ModSystem.NetSend/NetReceive` so joining clients receive it with world data.
- Send a small custom `ModPacket` after the server finishes a scan and the displayed percentage changes, rate-limited to at most once per second.
- Validate packet direction: only the server publishes Dryad count snapshots; clients never submit a percentage.

**Proven:** ExampleMod demonstrates discriminated `ModPacket` handling and server rebroadcast, while `ModSystem.NetSend` is specifically called with `MessageID.WorldData`, including player join. [`ExampleMod.Networking`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/ExampleMod.Networking.cs#L10-L70) [`ModSystem` network contract](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L363-L377)

### 6.3 Performance boundaries

**Recommendation:** start with a configurable scanner budget equivalent to roughly one to four columns per world update, then profile small/medium/large worlds and dedicated server tick time. Publish only complete snapshots. Dirty columns created by Apogean conversions may be prioritized, but a continuous rolling pass remains the compatibility repair mechanism.

**Recommendation:** keep spray radius at the official Clentaminator/Terraformer pattern (`size` 2/3) and rely on early return when the destination already matches. Do not send one custom packet per converted tile. Do not frame an entire screen for a single-tile change.

**Hazard:** conversion delegates are called inside nested tile loops. They must use O(1) family tables and cheap region lookups; never perform housing searches, flood fills, LINQ allocations, or world scans per tile.

## 7. Early-game purification pattern

**Proven:** Purification Powder has its own conversion ID, distinct from Green Solution purity, because Powder does not purify Hallow. tModLoader's conversion API explicitly documents handling both IDs where appropriate. [`ModBlockType.Convert`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBlockType.cs#L188-L202) [`TileLoader.RegisterSimpleConversion`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/TileLoader.cs#L748-L774)

**Proven:** official legacy ExampleMod uses `GlobalProjectile.PostAI` with a projectile-type check and excludes multiplayer clients when adding authoritative behavior to vanilla Purification Powder. [`FallenSoul PurificationPowder extension`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Old/NPCs/FallenSoul.cs#L130-L145)

**Recommendation:** define three Maw durability bands:

| Band | Powder | Sterilant Solution | Green Solution |
|---|---|---|---|
| Fringe growth | Maw→Wastes in a tiny radius | Maw→Wastes | no direct effect |
| Mature terrain | no effect before its boss/tool unlock | Maw→Wastes | no direct effect |
| Hardened core/Node | no terrain conversion; objective interaction required | no effect until explicit upgrade, if ever | no effect |
| Wastes terrain | no effect | no effect | Wastes→vanilla |

**Recommendation:** use explicit conversion delegates that return false on disallowed bands. A custom early purification item can reuse Powder visuals/AI principles while calling the same centralized conversion table. Keep its radius small and cost meaningful; it is containment, not full biome deletion.

**Hazard:** extending vanilla Powder with `GlobalProjectile.PostAI` can repeat conversion every AI tick and on multiple machines if the authority gate is wrong. Test single-player, host-and-play owner/non-owner, and dedicated server separately.

## 8. Mod compatibility policy

**Recommendation:** the default conversion registry should own only Apogean tiles/walls. Third-party tiles are untouched unless their mod opts in through a documented `Mod.Call` contract such as `RegisterMawTerrainFamily(tileType, family, wastesType, vanillaType)` and an optional protection callback. tModLoader defines `Mod.Call` specifically as weak inter-mod communication. [`Mod.Call`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/Mod.cs#L331-L338)

**Proven:** conversion fallbacks allow one tile to be temporarily treated as another family when a conversion lacks a direct handler. They are powerful and global. [`TileLoader.RegisterConversionFallback`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/TileLoader.cs#L825-L859) [`WallLoader.RegisterConversionFallback`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/WallLoader.cs#L443-L477)

**Recommendation:** use fallbacks only within a known family and document every exception. Do not set broad fallbacks that silently convert every third-party stone/sand/grass tile. Conversion callbacks should respect the shared protected-region service before mutating.

**Recommendation:** expose read-only integration points:

- current Maw conversion ID;
- predicates/ID sets for Maw and Wastes terrain;
- family registration during load only;
- protected-region query/registration;
- latest world contamination snapshot.

**Recommendation:** avoid detours for conversion, scanning, framing, packets, and Powder terrain handling; supported APIs cover them. If the Dryad detour is selected, isolate it in one compatibility class, validate the method signature at load, log failure, and fall back to the pre-button path.

## 9. Primary hazards register

| ID | Hazard | Consequence | Required control |
|---|---|---|---|
| H1 | Using `TileCountsAvailable` as world purity | Percentage changes as the player walks | Separate server/SP world scanner. |
| H2 | Full scan on Dryad click/load tick | Multi-second freeze on large worlds | Budgeted per-column scan and cached snapshot. |
| H3 | Partial scan published | Percentage oscillates or temporarily reads zero | Double-buffer/atomic publication. |
| H4 | Green Solution registered for both stages | One spray collapses Maw directly to green | Custom Sterilant ID for Maw→Wastes only. |
| H5 | `RegisterSimpleConversion` automatic reverse behavior | Powder or Chlorophyte bypasses progression | Explicit conversion registrations. |
| H6 | One Maw tile for all substrates | Wrong restoration material/biome | Parallel family IDs. |
| H7 | Direct tile writes | Broken frames, desync, missing notifications | `ConvertTile/ConvertWall` or explicit batch protocol. |
| H8 | Assuming Powder calls `WallLoader.Convert` | Custom Maw walls remain silently unchanged | Custom projectile/extension or intentional limitation. |
| H9 | Treating `wallHouse` as a valid-room query | False safety assumptions | Region protection plus safe-wall heuristic. |
| H10 | Claiming built-in player-placement provenance | Player builds converted unexpectedly | Explicit protected regions; optional future ledger. |
| H11 | Per-tile network/count packets | Bandwidth and server load | Tile-square sync plus rate-limited snapshot packet. |
| H12 | Background-thread tile scan | Race/corrupt read risk | Main-thread bounded scan only. |
| H13 | Broad conversion fallbacks | Other mods' terrain converted unexpectedly | Opt-in family registration. |
| H14 | Dryad pre-button interception | Joja/Stardew special branch skipped | Accept/document or use guarded detour. |
| H15 | Unlocalized concatenated status | Broken translations/grammar | Localized whole sentences for finite states. |

## 10. Finite test matrix

The feature is complete only when every row passes on the pinned tModLoader build. No test expands into an unbounded visual or seed loop.

| ID | Mode/world | Action | Pass condition |
|---|---|---|---|
| T01 | Single-player small | Load new world; wait for initial scan | No hitch over budget; ready flag becomes true; saved raw counts are nonnegative. |
| T02 | Single-player large | Profile initial and rolling scans for 120 seconds | No frame spike attributable to a full scan; per-tick budget is respected. |
| T03 | Any | Compare scanner result to an offline one-time diagnostic full count | Numerator and denominator match after a quiet full pass. |
| T04 | Any | Place one nonzero patch smaller than 0.5% | Dryad reports “less than 1%” or 1%, never 0% while numerator is nonzero. |
| T05 | Any | Talk to Dryad before scanner is ready | Localized surveying status appears; no synchronous scan occurs. |
| T06 | Any | Vanilla pure world, Maw present | Vanilla status remains intact and Maw line reports contamination; Apogean full purity is false. |
| T07 | Any | Maw zero, Wastes remain | Wastes are described as neutral/dormant and do not count as Maw evil. |
| T08 | Host-and-play | Host sprays Sterilant; client observes | Same tile/wall results on both peers; Maw percentage later converges. |
| T09 | Host-and-play | Non-host client sprays Sterilant | Server and all clients converge; no duplicate item use or conversion. |
| T10 | Dedicated server, two clients | Join after a completed scan | Joining client immediately receives the cached snapshot through world data. |
| T11 | Dedicated server | Change enough Maw to cross a whole percent | Server sends one rate-limited snapshot update; clients show the same value. |
| T12 | Terrain family fixture | Sterilize each Maw family | Every Maw tile becomes its matching Wastes family; no family collapses to generic dirt/stone. |
| T13 | Same fixture | Apply Green Solution to Wastes | Each Wastes family becomes the specified vanilla family with correct frames. |
| T14 | Same fixture | Apply Green Solution directly to Maw | No Maw tile becomes Wastes or vanilla. |
| T15 | Fringe/mature/core fixture | Apply early Powder | Only fringe tiles convert; mature/core/Nodes remain; documented wall behavior matches implementation. |
| T16 | Tree fixture | Restore Wastes grass under generic dead trees | Trees remain structurally valid and display the correct vanilla style; no mass break/drop. |
| T17 | Special vegetation fixture | Restore Sakura/Willow/custom trees, plants, vines, cactus | Every explicitly supported type maps/reframes; unsupported types are preserved, not corrupted. |
| T18 | Housing fixture | Spray across safe-wall room, unsafe natural wall, furniture, chest, wire | Natural registered wall converts as designed; safe room and constructed objects remain intact. |
| T19 | Protected-region fixture | Spray and run spread across spawn/company/settlement rectangle edge | No protected coordinate changes; adjacent unprotected terrain converts normally. |
| T20 | Save/reload | Save during partial scan, reload, rejoin | Last complete snapshot loads stale, scanning resumes safely, then atomically replaces it. |
| T21 | Compatibility fixture | Load an opt-out mod with custom stone/sand tiles | Third-party blocks do not convert. |
| T22 | Compatibility fixture | Register one third-party family through Apogean's opt-in API | Only registered family converts and restores through both stages. |
| T23 | Stress fixture | Hold Terraformer spray across mixed terrain for 60 seconds | No unbounded packet growth, crash, tile-frame corruption, or sustained tick degradation. |
| T24 | Dryad regression | Test both standard status click and Joja/Stardew condition | Selected hook strategy's documented behavior is exact; fallback path logs cleanly if a detour cannot attach. |

## Final recommendation

Implement issue 18 as four deliberately separate modules:

1. `MawTerrainFamilyRegistry`: one source of truth for Maw→Wastes→vanilla mappings and protection checks.
2. `MawWorldSurveySystem`: server/SP per-column scan, saved cached snapshot, and rate-limited network publication.
3. `MawRestorationContent`: Sterilant solution/projectile plus explicit Purity and Powder registrations.
4. `DryadMawStatusGlobalNPC`: localized status integration using the supported pre-button hook, with an isolated optional detour only if preserving the Joja branch is required.

This design uses public tModLoader conversion, framing, chat, persistence, and networking APIs; keeps the Wastes neutral; makes Maw resistance genuinely two-stage; and avoids claiming that Terraria tracks player-placed terrain when it does not.
