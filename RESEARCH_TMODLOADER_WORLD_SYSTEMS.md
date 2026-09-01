# Apogean: tModLoader World, Tileset, Story UI, and Space Systems Research

Status: implementation-oriented research and architectural recommendation. It records what the engine supports, what the current prototype gets wrong, what must be measured, and the safest sequence for future development. It is not a promise that every system ships in Act 1.

Scope: drastic world generation, ruined-biome replacement, custom tilesets and backgrounds, authored/procedural structures, progression ores, dialogue trees, quest state, multiplayer authority, persistence, hardware/software limits, and later space/subworld architecture.

Project authority: this report supplements `DESIGN_BIBLE.md`. Where creative direction conflicts, the Design Bible wins; where an implementation experiment conflicts, this report explains the replacement architecture.

## Source baseline and confidence

This report uses only primary sources. Repository observations are pinned to the exact revisions reviewed so that later engine or mod changes do not silently invalidate the conclusions.

- tModLoader branch `1.4.5`, commit [`2534f5682a46661c9aec633bea0852020e4fa796`](https://github.com/tModLoader/tModLoader/tree/2534f5682a46661c9aec633bea0852020e4fa796).
- CalamityModPublic branch `1.4.4`, commit [`1a8cebd27ec5615316b78f71973446b5528d2b78`](https://github.com/CalamityTeam/CalamityModPublic/tree/1a8cebd27ec5615316b78f71973446b5528d2b78). Calamity's repository states that active development is private, so this case study can describe the public release source but cannot establish how unreleased builds are organized. [CalamityModPublic repository](https://github.com/CalamityTeam/CalamityModPublic)
- Remnants branch `main`, commit [`9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389`](https://github.com/lazy-wombat/Remnants/tree/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389).
- Subworld Library branch `master`, commit [`b5a2a395c1b9f42de0adb308f5c33cc34edaf571`](https://github.com/jjohnsnaill/SubworldLibrary/tree/b5a2a395c1b9f42de0adb308f5c33cc34edaf571).

Terminology in this report:

- **Hard/current implementation limit** means a type, allocation, or protocol constraint visible in the reviewed source. It is not necessarily a supported configuration.
- **Supported baseline** means a configuration exposed and exercised by Terraria/tModLoader as a normal world preset.
- **Practical recommendation** is an Apogean engineering policy inferred from the sources. It is not an official tModLoader limit.
- **Unavailable evidence** marks a question that the public primary sources do not answer reliably.

### Runtime target used for Apogean

The installed development runtime inspected on this machine is tModLoader `1.4.4.9+2026.07.3.0 | stable`. Apogean's current `build.txt` does not pin another loader version, so code must compile and be playtested against that installed stable build. The upstream source case studies below are revision-pinned snapshots, including a newer public `1.4.5` branch where noted; they are evidence for architecture, not permission to call an API absent from the installed runtime. Before implementing each subsystem, confirm the final signatures against the locally installed references.

The development machine is unusually capable: AMD Ryzen 9 7950X (16 cores/32 threads), about 32 GiB system RAM, and an NVIDIA RTX 3090 with 24 GiB VRAM. That is ample for generation profiling, but it is not the target budget. tModLoader's Steam page recommends 16 GB RAM and notes that requirements vary with enabled mods. Apogean therefore needs 16 GB-class testing and dedicated-server testing; a fast workstation can hide allocation and stall problems that players will feel. [tModLoader system requirements](https://store.steampowered.com/app/1281930/tModLoader/)

## Executive conclusion

Apogean's drastic Earth overhaul is technically feasible if the main world remains within Terraria's standard dimensions and its generation is organized as deterministic, bounded passes. Large corporate compounds, ruined infrastructure, altered biomes, and progression-triggered world changes fit the ordinary tModLoader model.

The space expansion should not be one enormous tilemap. The safer architecture is:

1. Keep Earth at a vanilla-supported size, preferably large only when the player selects large.
2. Generate large Earth structures during world generation, reserve their rectangles through `StructureMap`, and persist only compact metadata about them.
3. Put each handcrafted planet in a separate, bounded, persistent Subworld Library subworld.
4. Put procedural star-chart missions in deterministic, normally non-saving subworlds, while saving only the mission seed, node state, and rewards in the main world.
5. Limit concurrent occupied subworlds in multiplayer because Subworld Library starts a separate server process for each occupied subworld.

This isolates memory and save costs, keeps ordinary Terraria compatibility possible on Earth, and lets later planets evolve without forcing every world's full tile data into memory or one `.wld` lifecycle.

## 0. Current Apogean prototype audit

The current code is useful as a visual/gameplay prototype, but it is not a foundation to scale by adding more passes and textures.

| Current area | What it does now | Why it cannot be the final system |
|---|---|---|
| `Content/World/RuinedSurfaceSystem.cs` | Runs after vanilla tree planting, converts exposed vanilla grass to `DeadGrass`, removes plants/vines/tree trunks, and places a few dead objects. | This is a destructive recolor/post-pass. It does not define a coherent dead-soil depth, vegetation lifecycle, special-tree mapping, roads, ruins, background variants, or protected player/world regions. |
| `Content/World/EngraftSystem.cs` | Inserts a pass near Jungle generation and directly paints an ellipse of Engraft tiles; runtime growth edits nearby tiles and sends tile squares. | It has no shared placement plan, `StructureMap` reservation, persistent protected-region registry, migration version, resumable transformation job, or full-volume biome grammar. A shallow ellipse is why the biome currently reads as a top-layer stain. |
| `Content/Structures/CompoundGen.cs` | Places small randomized 9×9 boxes after the Dungeon pass. | It does not validate terrain, reserve footprints, blend foundations, guarantee access, author rooms, protect structures, or safely place chests/tile entities. It is a placeholder, not a corporate compound generator. |
| `Content/UI/Dialogue/*` | Stores dialogue as C# nodes with `Func<Player, bool>` conditions and `Action<Player>` effects, then invokes the selected effect from the local UI. | Branching story choices and rewards must be server-authoritative. Delegates are not data, cannot be validated or migrated, and a client click must never directly grant rewards or mutate shared story state. |
| `DeadGrass.png` and `EngraftTurf.png` | Each is a 16×16 image. | A terrain tile auto-frames against its neighbors. A single 16×16 image is not the normal framed terrain sheet and cannot provide Terraria's edge/corner/variation vocabulary. |
| Biome detection | Some checks scan nearby tiles manually. | tModLoader already exposes `ModSystem.TileCountsAvailable`; one central count cache is cheaper and consistent with ExampleMod's biome pattern. |

**Decision:** preserve the prototype as a playtest reference, but replace these seams one subsystem at a time. Do not pile the final overhaul on top of them.

## 1. World dimensions and tile-count ceilings

### Supported baseline

tModLoader exposes Terraria's standard world dimensions as constants: small is 4,200 × 1,200, medium is 6,400 × 1,800, and large is 8,400 × 2,400. [Official `WorldGen` documentation](https://docs.tmodloader.net/docs/stable/class_world_gen.html)

| Preset | Dimensions | Tile positions |
|---|---:|---:|
| Small | 4,200 × 1,200 | 5,040,000 |
| Medium | 6,400 × 1,800 | 11,520,000 |
| Large | 8,400 × 2,400 | 20,160,000 |

**Practical recommendation:** Apogean should support all three vanilla presets and treat 8,400 × 2,400 as the maximum main-world target. A custom oversized Earth would make every full-world pass, tile data plane, map allocation, save scan, section table, and compatibility assumption more expensive.

### Representational limits are not supported-size promises

The current `Tilemap` stores `Width` and `Height` as `ushort`, constructs its data planes from `ushort` dimensions, and computes the plane length as width × height. That makes 65,535 the representable per-axis ceiling in this implementation. [Current `Tilemap.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Tilemap.cs)

Subworld Library's resizing hooks clamp enlarged dimensions to at most 65,534 before rebuilding tile and map storage. This is direct evidence that its implementation can request dimensions beyond vanilla presets, but the clamp is a representational guard, not a performance or compatibility guarantee. [Current `SubworldLibrary.cs`](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/SubworldLibrary.cs)

.NET arrays add a separate ceiling. Microsoft documents a maximum index of `0X7FEFFFFF` for non-byte arrays and approximately four billion total elements for arrays in general. Because tModLoader's tile planes are one-dimensional arrays with width × height elements, an individual typed plane can hit the .NET element ceiling before both axes reach their `ushort` ceiling. [Microsoft `Array` documentation](https://learn.microsoft.com/en-us/dotnet/api/system.array)

**Inference:** A square non-byte plane reaches the documented non-byte element-index limit at roughly 46,329 × 46,329. Such a map is far beyond practical Terraria memory, generation, save, and networking budgets and has not been established as supported by any reviewed primary source.

**Unavailable evidence:** No current official tModLoader source or documentation reviewed here publishes a supported maximum custom world dimension beyond the three standard presets. Therefore neither 65,534 nor approximately 46,329 per side should appear in an Apogean user-facing size option.

## 2. Memory and hardware constraints

### Tile storage has a large fixed cost

Current tModLoader stores tile state in parallel pinned managed arrays rather than one object per tile. The default data structures contain two `ushort` fields for tile and wall types, two liquid bytes, one `BitsByte` for brightness/invisibility state, and a tile/wall/wire state structure containing two `short` fields and one `int`. [Current `TileData.Default.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/TileData.Default.cs) Each registered tile-data plane is allocated to the full tile count. [Current `TileData.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/TileData.cs)

**Inference from the field payloads:** The default tile planes contain at least 15 bytes of field data per tile before array headers, alignment, map data, lighting, entities, world metadata, mod allocations, textures, and garbage-collector overhead. A vanilla large world therefore represents at least 302,400,000 bytes, about 288.4 MiB, in those tile fields alone.

This lower bound is why oversized worlds become expensive rapidly:

| Dimensions | Tiles | 15-byte tile-field lower bound |
|---|---:|---:|
| 8,400 × 2,400 | 20.16 million | 288.4 MiB |
| 12,000 × 3,600 | 43.20 million | 618.0 MiB |
| 16,800 × 4,800 | 80.64 million | 1.13 GiB |

These figures are not total process memory. The real process must also hold the map, generation scratch structures, loaded mods, assets, networking state, players/NPCs/projectiles, and runtime/GC data.

.NET can permit arrays larger than 2 GB in modern runtimes, but an allocation can still fail because of address-space fragmentation, contiguous-allocation requirements, commit limits, or insufficient physical memory. [Microsoft `OutOfMemoryException` documentation](https://learn.microsoft.com/en-us/dotnet/api/system.outofmemoryexception) Microsoft also documents that the .NET garbage collector becomes more aggressive around its high-memory-load threshold, which defaults relative to physical memory and is intended to avoid paging. [Microsoft GC runtime configuration documentation](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/garbage-collector)

### World generation scratch memory can rival tile storage

Remnants provides a concrete warning. Its biome system uses a coarse biome map with `CellSize = 50`, but also allocates three full-resolution `float[Main.maxTilesX, Main.maxTilesY]` arrays named `BlendX`, `BlendY`, and `Materials`. [Remnants `Biomes.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Biomes.cs)

**Inference:** On a vanilla large world, each float grid contains 20.16 million elements and approximately 76.9 MiB of float payload; all three total about 230.7 MiB before array overhead. This can be a reasonable tradeoff for a total-overhaul generator, but Apogean should not casually repeat this pattern for contamination, climate, faction influence, ruins, height, moisture, and decoration as separate full-resolution fields.

**Practical recommendations:**

- Represent broad fields at coarse resolution and interpolate when needed.
- Prefer `byte`, packed bits, or `ushort` to `float` when the required precision allows it.
- Reuse or release scratch buffers between passes instead of retaining every analysis grid.
- Combine related whole-world scans. Ten independent O(width × height) passes cost ten full scans even when each scan looks simple.
- Use bounded local searches around planned feature regions instead of repeatedly searching the entire world.
- Benchmark generation peak working set, not only settled in-game memory.

### Hardware policy for Apogean

**Unavailable evidence:** The reviewed official sources do not define a RAM or CPU minimum specifically for heavily modded, drastic tModLoader world generation. Hardware behavior also varies with enabled mods and world size.

**Practical recommendation, not an engine requirement:** Develop and profile against at least three memory classes—8 GB, 16 GB, and 32 GB system RAM—with 16 GB as Apogean's normal development/recommended target. An 8 GB test machine is valuable as a failure-budget check, while 32 GB testing catches scaling errors without mistaking abundant memory for efficiency. Publish a final user recommendation only after measuring Apogean together with representative modpacks.

The most relevant hardware resources are CPU time and system RAM, not maximum GPU texture size. The world is stored as tile data planes and rendered from visible regions; it is not uploaded as one world-sized texture. This is an architectural inference from tModLoader's `Tilemap` storage, not a formal hardware guarantee. [Current `Tilemap.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Tilemap.cs)

## 3. Save, load, and multiplayer constraints

### Save/load scales with tile count

tModLoader's current mod-tile I/O traverses the tilemap to serialize and restore modded tile and wall information. The basic writer emits entries through full tile loops, and the reader reconstructs those records across the world. [Current `TileIO_Basic.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/IO/TileIO_Basic.cs)

**Practical consequence:** Sparse mod tiles do not make every part of mod-world save/load work constant-time. Larger maps still make full-map scans longer, and extensive modded terrain increases compressed data and processing. Apogean should save compact world-system state—such as structure bounds, IDs, generation version, faction states, and mission seeds—rather than a second per-tile mirror of facts already represented by tiles.

`ModSystem.SaveWorldData` stores system data in a `TagCompound`; the corresponding networking hooks send world-system state when world data is synchronized. [Current `ModSystem.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/ModSystem.cs) The central `WorldIO` implementation collects each system's saved and networked state. [Current `WorldIO.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/IO/WorldIO.cs)

**Practical consequence:** `NetSend` should contain compact authoritative state, not tile grids, structure templates, or large quest histories. Tiles have their own section synchronization path.

### Terraria streams world sections

The current networking code divides the world into sections 200 tiles wide by 150 tiles high and tracks which sections each remote client has received. [Current `RemoteClient.cs` patch](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/RemoteClient.cs.patch)

For bounded runtime edits, `NetMessage.SendTileSquare` synchronizes a rectangular tile area. [Official `NetMessage` documentation](https://docs.tmodloader.net/docs/stable/class_net_message.html) For ore-like in-game generation, `WorldGen.OreRunner` is documented as framing and syncing its changes and as suitable for use during gameplay/multiplayer. [Official `WorldGen` documentation](https://docs.tmodloader.net/docs/stable/class_world_gen.html)

The server can also reset a client's section-loaded state, causing sections to be sent again as needed. [Current `Netplay.cs` patch](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Netplay.cs.patch)

**Practical recommendations for progression-triggered terrain changes:**

- Perform authoritative world mutation on the server or single-player host, never independently on every client.
- For small edits, sync bounded rectangles.
- For very large edits, process work in chunks and consider resetting relevant section delivery rather than emitting thousands of tiny tile-square messages.
- Avoid a synchronous, world-wide conversion on the gameplay thread at the instant a boss dies. Queue or phase the conversion and communicate progress to players.
- Store a conversion version and completion state so interrupted saves can be detected and resumed safely.

**Unavailable evidence:** The reviewed primary sources do not publish one universal maximum `.wld`/`.twld` file size, maximum safe tile-change packet volume, or acceptable join time. Those are deployment measurements Apogean must obtain with dedicated-server tests.

## 4. Safe world-generation organization

### Passes and deterministic randomness

Current tModLoader exposes `ModSystem.ModifyWorldGenTasks(List<GenPass> tasks)` and `ModifyHardmodeTasks(List<GenPass> tasks)` for ordered generation changes. A mod can insert passes, inspect pass names, and disable a pass through the generation-pass API. [Current `ModSystem.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/ModSystem.cs)

World generation should use `WorldGen.genRand` so generation remains tied to the world seed. [Official `WorldGen` documentation](https://docs.tmodloader.net/docs/stable/class_world_gen.html)

**Practical Apogean pass families:**

1. Analyze vanilla terrain and reserve immutable/sensitive regions.
2. Establish global dead-world morphology and InGraft regions.
3. Place major corporate/ruin footprints from largest to smallest.
4. Carve roads, utility corridors, and biome transitions around reserved structures.
5. Place medium structures and biome features.
6. Populate tiles, walls, liquids, chests, and entities.
7. Repair frames, validate access paths, record metadata, and run generation invariants.

The exact insertion points should be located defensively by pass name and checked for “not found.” ExampleMod demonstrates finding a named pass before inserting a mod pass after it. [Current ExampleMod `ExampleOre.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/ExampleMod/Content/Tiles/ExampleOre.cs)

### Bounds, reservations, and bounded attempts

`WorldGen.InWorld` exists to check that coordinates—including a requested safety margin—are inside the map; the official documentation warns that invalid tile access can crash world generation. [Official `WorldGen` documentation](https://docs.tmodloader.net/docs/stable/class_world_gen.html)

`StructureMap.CanPlace` checks whether a proposed rectangle conflicts with protected structures and whether its tile types are allowed. After placement, modders are expected to call `AddProtectedStructure` to reserve the placed area. [Official `StructureMap` documentation](https://docs.tmodloader.net/docs/stable/class_structure_map.html)

ExampleMod's rubble generator also demonstrates an important failure policy: random placement retries stop after 1,000 attempts rather than looping until success forever. [Current ExampleMod `RubbleWorldGen.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/ExampleMod/Common/Systems/RubbleWorldGen.cs)

**Practical Apogean policy:** Every search must have a maximum attempt count, a fallback region or skipped-feature path, and a diagnostic reason. Mandatory story structures need deterministic fallback placement and terrain adaptation; optional ruins may be skipped. A generator must never hang because another mod consumed the ideal terrain.

### Generation-version tracking

tModLoader records which mods generated a world and exposes a method to retrieve the mod version used during generation. [Official `WorldFileData` documentation](https://docs.tmodloader.net/docs/stable/class_world_file_data.html)

**Practical recommendation:** Store an independent Apogean world-schema version as well. A mod release version and a world-layout schema answer different questions. Use the schema to decide whether to migrate compact metadata, add newly introduced optional structures, or refuse a destructive regeneration.

## 5. Primary-source case study: CalamityModPublic

### Pass organization

Calamity centralizes world-pass changes in `WorldgenManagementSystem`. It locates vanilla passes by name, conditionally inserts `PassLegacy` operations before or after them, and only replaces a pass when the named index exists. It also orders dependencies among its own features—for example, ensuring later features follow prerequisite terrain. [Calamity `WorldgenManagementSystem.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Systems/World/WorldgenManagementSystem.cs)

It scales some feature counts with world width, such as workshop and laboratory counts, rather than using one fixed count for all presets. [Calamity `WorldgenManagementSystem.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Systems/World/WorldgenManagementSystem.cs)

**Lesson for Apogean:** Keep one visible orchestration layer that owns pass order and dependencies, while moving each feature's implementation into a focused generator. Scale density deliberately by world area or width, but do not assume every structure should multiply linearly; unique story compounds should remain unique.

### Large structures and templates

Calamity's Draedon structure generator uses bounded candidate loops, scans candidate terrain for biome/material suitability, checks distances from other structures, calls `StructureMap.CanPlace`, places a schematic, and then protects the resulting rectangle with padding. [Calamity `DraedonStructures.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/DraedonStructures.cs)

Calamity's schematic system is a custom implementation. It loads its own `.csch` templates, validates placement corners with `WorldGen.InWorld`, handles anchors/flipping/chests, and can preserve existing tile state where the schematic requests it. [Calamity `SchematicManager.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Schematics/SchematicManager.cs)

**Lesson for Apogean:** A template layer is valuable for authored compounds and ruins, but it needs explicit policies for anchors, protected rectangles, liquids, multi-tiles, chests, terrain blending, and preservation. Calamity's `.csch` pipeline is Calamity code, not a built-in tModLoader schematic format to copy casually.

### Space content without dimensions

Calamity's planetoids are generated as sky micro-biomes inside the main Terraria world. Their counts scale with world width; placement uses explicit attempt limits, biome placement helpers, solid-tile scans, and the shared `StructureMap`. [Calamity `Planetoid.cs`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/Planets/Planetoid.cs)

The reviewed Calamity release manifest does not declare Subworld Library, and no active custom-dimension implementation was found in this public revision. [Calamity `build.txt`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/build.txt)

**Evidence boundary:** This establishes only what exists in the reviewed public release source. It cannot establish whether Calamity's private active-development source has experiments or plans for dimensions.

## 6. Primary-source case study: Remnants

### Pass replacement and compatibility posture

Remnants provides centralized helpers to find, insert, remove, or disable named passes. It tags its own passes and conditionally detects other mods to suppress known conflicting generation. [Remnants `General.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/General.cs)

Its biome system disables or replaces major vanilla biome passes and inserts a unified biome-generation operation. [Remnants `Biomes.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Biomes.cs)

**Lesson for Apogean:** Total world replacement is feasible, but compatibility becomes an explicit product choice. Replacing many named vanilla passes and disabling other mods' passes is inherently more version- and modpack-sensitive than additive placement. Apogean should expose a clearly named overhaul mode, detect known incompatibilities, and never advertise universal worldgen compatibility without a tested compatibility matrix.

### Procedural macro-structures plus authored modules

Remnants replaces major structures such as the Dungeon and Jungle Temple with large procedural layouts assembled from room grids and StructureHelper templates, then reserves their areas in the structure map. [Remnants `Dungeons.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Dungeons.cs)

Its broader structure generator composes many `.shstruct`/`.shmstruct` template modules and repeatedly uses `CanPlace`/`AddProtectedStructure` around authored placements. [Remnants `Structures.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs)

**Lesson for Apogean:** The strongest approach for very large corporate facilities is hybrid generation: procedural macro-layout for floors, shafts, access routes, and damage states; authored modules for recognizable rooms, set pieces, and loot spaces. One giant rigid schematic is harder to adapt to world size and neighboring terrain.

### Hardmode transformations

Remnants replaces the hardmode Good/Evil tasks and performs broad post-Wall-of-Flesh conversions through `ModifyHardmodeTasks`. [Remnants `PostGen.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/PostGen.cs)

**Lesson for Apogean:** Progression-triggered world changes belong in a dedicated transformation subsystem with its own pass order and metadata. The source demonstrates feasibility, but Apogean's multiplayer version must additionally batch and synchronize runtime changes as described earlier.

### Dimension evidence

The reviewed Remnants build manifest packages StructureHelper and declares WombatQOL integration, but it does not declare Subworld Library as an active dependency. [Remnants `build.txt`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/build.txt)

**Evidence boundary:** No active custom-subworld architecture was found in the reviewed revision. A project reference or commented namespace alone would not prove an active runtime feature, so this report does not treat Remnants as a subworld case study.

## 7. Subworld Library and space architecture

### What the library provides

A Subworld Library subworld defines custom width and height, an ordered list of `GenPass` tasks, lifecycle hooks, and options controlling saving, player saving, normal updates, lighting, and gravity. `ShouldSave` is false by default. [Current `Subworld.cs`](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/Subworld.cs)

During a transition, the library changes the active dimensions, clears/rebuilds world state, and runs the destination's generation tasks in order. Saved subworlds live beneath a directory associated with the main world's identity, and data can be copied between the main world and subworld through `TagCompound` lifecycle interfaces. [Current `SubworldSystem.cs`](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/SubworldSystem.cs)

Its README states that multiplayer is supported and that a server is opened for each subworld currently occupied by players. [Subworld Library README](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/README.md) The implementation starts and coordinates separate subserver processes for occupied subworlds. [Current `SubworldSystem.cs`](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/SubworldSystem.cs)

The library relies on extensive hooks and IL edits to make an engine not originally designed for dimensions resize and transition correctly. Its own README explicitly describes this injection-heavy approach. [Subworld Library README](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/README.md) The resizing hooks are visible in the implementation. [Current `SubworldLibrary.cs`](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/SubworldLibrary.cs)

### Proposed Apogean topology

```text
Main-world save: Earth
├── Vanilla-sized overhauled surface/underground
├── Faction and story state
├── Structure registry and generation schema
├── Ship upgrade state
└── Star-chart node records
    ├── Handcrafted planet A -> persistent bounded subworld save
    ├── Handcrafted planet B -> persistent bounded subworld save
    ├── Ship interior       -> small persistent subworld, if a tile-built interior is needed
    └── Procedural mission  -> deterministic temporary subworld; save seed/results, not tiles
```

**Practical recommendations:**

- Use Subworld Library rather than building a new IL-hooked dimension framework. The reviewed implementation already owns resizing, transitions, save paths, multiplayer relocation, and subservers.
- Pin and test a supported Subworld Library version. Its necessary IL hooks create engine-version coupling, so tModLoader preview updates should not be treated as automatically safe.
- Keep early handcrafted planets no larger than vanilla small or medium worlds unless profiling demonstrates a real need. Planet size should be chosen from traversable content density, not from the representational maximum.
- Default procedural mission subworlds to `ShouldSave = false`; derive generation from a stored deterministic node/mission seed and persist only durable outcomes.
- If players may split across planets, budget approximately one additional server process per occupied subworld. This implies multiplied runtime, loaded world state, and operating-system overhead. The precise RAM multiplier needs measurement because the public sources do not publish it.
- For a cooperative story, prefer one active travel destination per party/session unless independent expeditions are an explicit feature worth the server cost.
- Define what crosses dimensions. Character inventory may travel, while Earth NPCs, active bosses, loose projectiles, and transient faction invasions should not be assumed to transfer.

The Subworld Library manifest identifies the reviewed release as 2.3.0.1. [Current `build.txt`](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/build.txt)

**Licensing caution:** The library README places conditions on derivative dimension APIs. Using the library as a dependency is not the same engineering action as copying or forking its internals, but Apogean should review the current license and README before any derivative implementation. [Subworld Library README](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/README.md)

## 8. Hard limits versus Apogean policies

| Topic | Hard/current fact | Apogean policy |
|---|---|---|
| Normal dimensions | Presets end at 8,400 × 2,400. [`WorldGen`](https://docs.tmodloader.net/docs/stable/class_world_gen.html) | Do not enlarge Earth beyond the chosen vanilla preset. |
| Axis representation | Current `Tilemap` dimensions are `ushort`. [`Tilemap.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Tilemap.cs) | Never market the representational ceiling as supported. |
| Array size | .NET imposes per-array element/index limits. [Microsoft `Array`](https://learn.microsoft.com/en-us/dotnet/api/system.array) | Keep every allocation far below the ceiling; use coarse/packed scratch maps. |
| Tile memory | Full-size parallel planes are allocated for the tile count. [`TileData.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/TileData.cs) | Treat tile count as the primary memory multiplier. |
| Network regions | Client world delivery is sectioned into 200 × 150 tiles. [`RemoteClient.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/RemoteClient.cs.patch) | Batch runtime mutations and let the server own them. |
| Structure collision | `StructureMap` offers reservation checks and protected rectangles. [`StructureMap`](https://docs.tmodloader.net/docs/stable/class_structure_map.html) | Every medium/large structure reserves its footprint and padding. |
| Subworld size | Subworld Library supports custom dimensions and clamps enlarged axes. [`SubworldLibrary.cs`](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/SubworldLibrary.cs) | Keep planets bounded to content needs, initially at or below vanilla medium. |
| Multiplayer subworlds | Occupied subworlds can run separate server processes. [`SubworldSystem.cs`](https://github.com/jjohnsnaill/SubworldLibrary/blob/b5a2a395c1b9f42de0adb308f5c33cc34edaf571/SubworldSystem.cs) | Prefer a shared party destination and profile any split-party mode. |

## 9. Recommended validation gates before implementation expands

These are project recommendations inferred from the constraints above:

1. **Generation determinism:** Generate each size twice from the same seed and compare Apogean structure records and validation hashes.
2. **Attempt exhaustion:** Force hostile terrain and mod conflicts; prove every structure search exits with a placed, fallback, or skipped result.
3. **Access invariants:** Validate spawn safety, required route connectivity, mandatory facility entrances, and progression locks after all passes finish.
4. **Peak-memory benchmark:** Record process peak working set for small/medium/large generation on 8/16/32 GB systems.
5. **Save-cycle benchmark:** Measure initial save, repeated save, load, and `.wld`/`.twld` size for each preset.
6. **Dedicated-server join:** Measure first join, section delivery while teleporting, and reconnect after a large terrain transformation.
7. **Interrupted transformation:** Terminate during a staged hardmode/boss conversion and verify recovery from the recorded conversion version.
8. **Subworld concurrency:** Test one, two, and several occupied planets and record total process count, RAM, CPU, transition reliability, and reconnect behavior.
9. **Compatibility matrix:** Test vanilla only, Subworld Library, Calamity, Remnants, and selected combinations. Where both mods replace the same passes, report incompatibility rather than silently corrupting generation.

## 10. Open evidence gaps

The following questions require prototypes and measurements; the primary sources do not supply trustworthy universal answers:

- Maximum acceptable Apogean generation time per world size.
- Actual peak memory after Apogean's final tiles, systems, assets, and other common mods are loaded.
- Maximum safe size for a persistent planet on low-memory clients and dedicated servers.
- Real per-subworld server-process memory and CPU overhead under split-party multiplayer.
- Maximum acceptable runtime conversion area per frame and corresponding section-resend strategy.
- Final `.wld`/`.twld` growth from Apogean's tile distribution and metadata.
- Compatibility with private/unreleased Calamity world-generation changes.

Until those measurements exist, vanilla-sized Earth plus bounded subworld planets is the best-supported architecture, while custom giant main worlds should be treated as an unsupported experiment.

## 11. How Terraria tiles and tilesets actually work

### 11.1 Terrain tiles are framed systems, not isolated sprites

Terraria tiles occupy 16×16 world pixels. Most sprite-sheet frame cells include two pixels of spacing on the right and bottom, so the common source cell is 18×18. A terrain tile is not `frameImportant`: Terraria chooses a frame from its neighboring tiles, creating edges, corners, interior fills, slopes, and visual variants. ExampleMod's ordinary terrain sheet is 288×270 pixels, or 16×15 padded frame cells. The engine normally exposes three random frame-number variations so broad surfaces do not repeat one stamp. [Official Basic Tile guide](https://github.com/tModLoader/tModLoader/wiki/Basic-Tile) [ExampleBlock implementation](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/ExampleBlock.cs)

That explains the current failure mode: a 16×16 `DeadGrass.png` or `EngraftTurf.png` can show one attractive sample, but it cannot describe the complete adjacency vocabulary Terraria expects. Scaling or packing a highly detailed concept image into that slot makes the result noisy rather than more detailed.

There are two major families:

- **Terrain tiles:** dirt, stone, grass, ore, sand-like masses, and similar blocks. These usually use automatic framing and a full terrain sheet.
- **Frame-important objects:** furniture, rubble, plants, organs, doors, machinery, lamps, terminals, and multi-tile structures. These use `TileObjectData` to declare width, height, origin, anchors, style variants, alternates, coordinate heights, placement rules, and optional animation. `CopyFrom` should happen before modifications and `addTile` after the full definition. [Official `TileObjectData` documentation](https://docs.tmodloader.net/docs/stable/class_tile_object_data.html) [ExampleMod TileObjectData showcase](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Furniture/Showcases/TileObjectDataShowcase.cs)

### 11.2 Required tileset contract

Every Apogean tile family needs a written behavior contract before art is drawn:

| Contract field | Examples of what must be decided |
|---|---|
| Physical behavior | Solid, platform, cuttable, falling, slope/half-block support, explosion resistance, liquid interaction. |
| Framing | Terrain auto-frame, 8-way custom frame, TileObjectData dimensions, styles, alternates, horizontal flip. |
| Merging | Self only, dirt, stone, mud, snow, faction blocks, Engraft neighbors, or deliberately hard seams. |
| Progression | `MinPick`, `MineResist`, explosion immunity, required quest tool, protected/unbreakable state. |
| Presentation | Map color/name, dust, hit sound, paint/coating behavior, light emission, glow mask, ambient particles. |
| Ecology | Can grass spread onto it, can trees/plants/vines grow, can Engraft convert it, can Hallow resist it. |
| Worldgen | Valid host materials, depth bands, structure protection, frequency, replacement rules. |
| Compatibility fallback | Vanilla tile used if the mod is removed, and whether conversion is safe in existing worlds. |

Only explicit energy-bearing pieces—amber cysts, Maw Nodes, active organs, powered corporate fixtures—should emit light. Ordinary dead soil and Engraft turf should not. This makes light meaningful and fixes the current effect where mining a dark block reveals the underground with an unexplained glow.

### 11.3 Recommended Earth tile families

The ruined world needs a small coherent kit before it needs hundreds of props:

1. **Dead baseline:** dead soil, dead grass, exposed dry subsoil, cracked stone accents, dead tree/sapling, scrub plants, small/medium rubble, ruined road/asphalt, broken concrete wall, and hanging cable variants.
2. **Engraft:** Engraft turf, fibrous subsoil, hardened graftstone, cord/root tiles, hanging growth, amber cyst, Maw Node components, larval nest objects, and Engraft walls. Dominant colors remain charcoal, sickly ochre/amber, dry stringy bone, and restrained wet accents—not Corruption purple or Crimson red fields.
3. **Kessler:** gunmetal structural block, armored wall, hazard trim, industrial platform, blast door, turret socket, cable conduit, crate and terminal sets.
4. **Helix:** sterile panel, containment glass, surgical wall, drain/grate, growth vat, lab furniture, clinical indicators.
5. **Sentrix:** black composite, cold conductor, scan-line wall, precise vertical panels, sensor/turret sockets.
6. **Shared ruins:** damaged variants of all three factions, generic settlement material, highway pieces, pylons, pipes, freight debris, frozen and overgrown variants.

This is enough to construct distinct places. Later props should extend these grammars instead of introducing an unrelated palette for every room.

### 11.4 Native-scale art workflow

The production workflow should be deliberately strict:

1. Implement a checkerboard/debug terrain sheet from the official 288×270 template.
2. Verify framing, slopes, half-blocks, paints, coatings, actuators, liquids, minimap entry, dust, and merge behavior in a small test world.
3. Draw at final resolution with hard opaque pixels. Do not generate at 1024 pixels and downscale.
4. Start with silhouette and material clusters; use fewer colors and larger shapes than the current weapon concepts.
5. Fill all three terrain variation regions, then author merge/custom-frame sheets only where the material needs them.
6. Test a generated adjacency board containing isolated blocks, every edge/corner, 1-tile columns, cavities, slopes, mixed neighbors, walls, liquids, paint, Echo coating, and actuators.
7. Inspect at 1× and normal gameplay zoom. If detail vanishes or flickers in motion, simplify it.

Large texture atlases are not useful here. A conservative compatibility policy is to split unrelated assets and keep any individual sheet well below 4096 pixels per axis, but 4096 is an asset budget—not a claimed modern hard limit. Terrain sheets are normally far smaller.

### 11.5 Trees, plants, vines, and walls are separate systems

Changing grass does not automatically create a dead ecosystem. A custom tree uses `ModTree`, declares which ground tile it grows on, and supplies separate trunk, branch, and top textures. ExampleMod's reference assets are 176×264 for its trunk sheet, 84×126 for branches, and 246×82 for tops; those measurements are examples of Terraria's layout, not required Apogean art dimensions. Saplings are frame-important objects with placement styles and random variants. [ExampleMod tree](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Plants/ExampleTree.cs) [ExampleMod sapling](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Plants/ExampleSapling.cs)

Vines need explicit growth, framing, conversion, paint-copy, and multiplayer synchronization behavior. Walls have their own framing and blend system; an advanced wall can animate, emit light, restrict teleporting, and customize blending. [ExampleMod vine](https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/Tiles/Plants) [ExampleWallAdvanced](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Walls/ExampleWallAdvanced.cs)

Apogean therefore needs an explicit vegetation replacement map rather than “delete every trunk”:

- world-generated forest trees become dead-tree species on dead grass;
- ordinary plants/vines become dry scrub, wireweed, or nothing according to seeded density;
- special/player-planted trees such as willow, sakura, palm, gem, and faction decoration are not accidentally transformed by a broad runtime sweep;
- each biome keeps appropriate recognizable tree mechanics where the Design Bible requires it;
- regrowth and sapling placement remain deterministic and multiplayer-safe.

### 11.6 Biome counting

Use one `ModSystem.TileCountsAvailable(ReadOnlySpan<int> tileCounts)` implementation to cache relevant dead/Engraft/faction tile totals, then let each `ModBiome.IsBiomeActive` combine those counts with position/depth rules. This is the official ExampleMod pattern and avoids repeated rectangle scans from every player or UI system. [Example biome tile count](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/Systems/ExampleBiomeTileCount.cs) [Example surface biome](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Biomes/ExampleSurfaceBiome.cs)

## 12. Backgrounds: why the first attempt separated and how to rebuild them

Terraria surface backgrounds are a parallax system, not one panoramic canvas. `ModSurfaceBackgroundStyle` independently selects far, middle, and close textures, fades styles in/out, and allows close-layer scale and parallax control. The official ExampleMod textures happen to be 1024×408 (far), 1024×600 (middle), and 952×480 (close); those are examples, not universal size contracts. The layer art must be horizontally tileable and must overlap/bleed safely at its repeat seams. [Official `ModSurfaceBackgroundStyle` source](https://github.com/tModLoader/tModLoader/blob/stable/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs) [Example surface background](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs)

Underground styles use exactly four slots: sky/ground transition, dirt field, dirt/rock transition, and rock field. The documented reference sizes are 160×16 for the two border strips and 160×96 for the two fields, with the right 32 pixels duplicating the left edge for seamless repetition. [Example underground background](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs)

The Apogean background contract is:

- two or more authored compositions per surface biome;
- each composition is a matched, transparent far/middle/close set;
- far carries skyline and distant civilization; middle carries highways, research stations, towers, pipelines, ports, or wrecks; close carries local debris/vegetation silhouettes;
- all layers tile seamlessly past both screen edges at common resolutions and UI scales;
- a world-seed-derived variant is saved per biome and remains stable;
- Terraria's sky, ambient color, night, rain, blood moon, and solar eclipse tint the same landmark composition, so buildings do not teleport when time changes;
- optional powered lights are a small separately drawn/emissive overlay, not a baked daytime sky;
- a later projector/background-selector item changes the saved variant deliberately.

The ordinary forest is not a custom biome in vanilla, so replacing its visual baseline requires a global background policy (`GlobalBackgroundStyle`/scene-effect routing) based on the local player's vanilla zone and the saved variant. Engraft can use its own `ModBiome` scene effect. Priority rules must be explicit so Ocean, Desert, Snow, Jungle, Corruption, Crimson, Hallow, Graveyard, events, and Engraft do not fight over the active style.

## 13. Rebuilding the green overworld without deleting Terraria

The Design Bible calls for two visually and mechanically separate layers:

1. **Ruined Earth baseline:** the ordinary safe forest is brown, exhausted, and full of environmental history. It still serves Terraria's spawn, housing, building, exploration, and early progression roles.
2. **Concentrated Engraft:** discrete, dangerous, living contamination regions with full-volume terrain, active roots/cords, Maw Nodes, bespoke enemies, and stronger spread pressure.

If all dead terrain is counted as Engraft, spawn becomes hostile and the special biome loses its silhouette. If only the first grass row is recolored, the world looks cosmetically gray with intact green systems beneath it. The generator needs a morphology pass, not a palette pass.

### 13.1 Recommended world-generation pipeline

```text
Vanilla dimensions and terrain foundation
        ↓
Analyze oceans, spawn, Dungeon, Temple, biome bands, surface height, caves
        ↓
Create deterministic WorldPlan (regions, routes, mandatory sites, variants)
        ↓
Reserve spawn sanctuary and three corporation territories
        ↓
Apply ruined-Earth morphology and shallow material bands
        ↓
Carve Engraft ruptures/outgrowths as 2D volumes, not surface stripes
        ↓
Place major structures and transit corridors from largest to smallest
        ↓
Blend foundations, roads, walls, liquids, and damage skirts into terrain
        ↓
Place chests/tile entities, then vegetation/debris and framed objects
        ↓
Frame/settle, validate invariants, persist compact WorldPlan metadata
```

Use vanilla terrain as the foundation instead of replacing every generation pass in the first release. This preserves Terraria's recognizable topology and greatly reduces conflict risk. Apogean can still transform the result aggressively through coordinated, deterministic passes.

### 13.2 Spawn sanctuary

Spawn should look dead without functioning as Engraft. Reserve a deterministic sanctuary extending far enough for starter housing, trees/scrub, and early crafting. The sanctuary:

- uses dead soil/grass, custom background, dead vegetation, and small harmless ruins;
- blocks Engraft conversion and active Maw Nodes;
- suppresses the mod's dangerous spawn pool until the player leaves its bounds or meets a progression trigger;
- remains a protected region for later spread and progression ore jobs;
- does not prevent normal player building or vanilla NPC housing.

Safety should come from region/spawn rules, not bright green healthy grass. That lets the opening communicate apocalypse without creating unavoidable first-minute deaths.

### 13.3 Biome treatment matrix

| Biome | Keep | Apogean transformation |
|---|---|---|
| Forest | Spawn/housing/building role, basic terrain shape | Dead soil and grass, dead trees/scrub, broken settlement infrastructure, stable ruined parallax sets. |
| Desert | Sand mechanics and underground-desert threat | Collapsed highways, buried freight/logistics works, wreck markers, localized contaminated pockets. |
| Snow | Snow/ice mechanics | Frozen relays, pipelines, abandoned camps, snow-buried debris; dead cold palette rather than simply gray snow. |
| Jungle | Mud/chlorophyte/Temple progression and lush density | Failed Helix research/containment sites and war damage; it can remain biologically dense because its horror is uncontrolled life, not universal brown recoloring. |
| Ocean | Ocean boundaries/fishing | Drowned ports, broken sea walls, evacuation wrecks and offshore silhouettes. |
| Corruption/Crimson | Vanilla mechanics, enemies, progression, immediate recognizability | Their own force consumes the same ruined civilization. Do not recolor them into Engraft. Their border relationship with Engraft is handled explicitly. |
| Hallow | Hardmode mechanics and recognizability | Hallow transforms ruined infrastructure and mechanically resists/slows Engraft. |
| Engraft | N/A; it is Apogean's separate hostile ecology | Deep charcoal/ochre volumes, ruptures, cords, nodes, larvae, distorted terrain and bespoke danger. |

### 13.4 Spread and conversion

Pre-Hardmode Engraft growth should be slow and local to Maw Nodes. Hardmode enables stronger bounded fronts, including optional seeding tied to altar/world progression. Conversion must use an allowlist of natural terrain, not “anything nearby.” It must reject:

- spawn sanctuary and all persistent protected regions;
- tiles near chests, signs, tile entities, housing walls, doors, wires/actuators, and authored arenas;
- placed faction blocks and known structure materials;
- recent player construction where ownership cannot be inferred safely;
- out-of-bounds or unloaded transition conditions.

The Design Bible already forbids bosses from permanently repainting player terrain during a fight. Boss projectiles may create temporary hazards; persistent ecology belongs to the world system.

### 13.5 New worlds versus existing worlds

The full experience should require a newly generated **Apogean Overhaul world**. Existing worlds can support the mod's items, bosses, UI, limited structures, and a guarded retrofit command, but they cannot safely receive the same continent-scale ruined morphology and guaranteed compounds without destroying player work. A **Compatibility/Classic worldgen mode** should keep transformations smaller and disable pass replacement for modpacks. The chosen mode and schema version are frozen into world data at creation.

## 14. Large structures and corporation territories

### 14.1 Generate the shell on day one; change state later

Stamping a full headquarters into an active multiplayer world when a corporation “arrives” is the riskiest option: it can overwrite player builds, stall the server, require huge network synchronization, and produce different terrain on clients. The Jungle Temple model is safer. Generate each major footprint, sealed shell, arena, and underground service volume during world creation. Progression later performs small controlled state changes:

- open or seal bulkhead doors;
- activate NPCs, turrets, lights, terminals, and shops;
- swap a limited set of damaged/intact tiles;
- place a landing craft in a pre-reserved pad;
- enable room access and dialogue routes;
- update banners, alarms, faction ownership, and background effects.

The world can therefore show all three corporations from day one without making all content accessible. Kessler's post-Wall-of-Flesh impact can still be dramatic because the landing pad, dormant outpost, sky silhouette, and locked proving ground already exist and become active.

### 14.2 Hybrid generation is the right level of proceduralism

Use a procedural macro-layout plus authored room modules:

- the macro generator chooses floors, shafts, entry route, arena, service tunnels, damage zones, and terrain fit;
- authored modules provide recognizable rooms—quartermaster bay, security checkpoint, armory, lab, containment wing, reactor, executive chamber, wrecked habitation, and loot scenes;
- connector metadata guarantees compatible doors/corridors;
- a damage pass removes selected walls/furniture and adds rubble without breaking the mandatory route;
- left/right variants are authored or mirrored only where every contained tile supports mirroring.

This follows the useful split visible in Remnants: a large procedural plan assembled from authored modules. Calamity demonstrates the complementary pattern for bounded labs: candidate terrain validation, distance rules, `StructureMap.CanPlace`, schematic placement, and protected padding.

### 14.3 Proposed structure data model

Each module should have stable metadata rather than raw runtime tile IDs:

```text
StructureModule
  id / schema version
  width / height / anchor
  connector list (side, offset, type, clearance)
  allowed biome/depth/terrain tags
  tile and wall names (mod:name), not transient numeric IDs
  liquid, wire, actuator, slope, paint and coating layers
  object/chest/sign/tile-entity markers
  protection and terrain-blend padding
  optional intact/damaged variants
```

Whether Apogean uses StructureHelper or its own compact format should be decided after a proof-of-concept and license/version review. The important requirement is an explicit loader/validator; Terraria/tModLoader does not provide a complete built-in large-schematic authoring pipeline.

### 14.4 Shared `WorldPlan` and persistent protected regions

All generators should consume one deterministic `WorldPlan`, created before destructive placement. It owns spawn sanctuary, corporate territories, major roads, Engraft regions, unique structures, biome variants, and fallback coordinates. This stops independent passes from competing blindly for the same land.

`GenVars.structures`/`StructureMap` is essential during world generation, but runtime systems also need protection after generation finishes. Persist a compact `ProtectedRegionRegistry` containing rectangle, stable owner ID, protection tags, schema version, and optional expansion padding. Engraft spread, progression ore, event terrain, teleporters, and retrofit commands all query this registry.

Every medium or large placement follows this transaction:

1. candidate is inside world bounds with safety margin;
2. biome, depth, ocean, Dungeon, Temple, spawn, and route rules pass;
3. `StructureMap.CanPlace` passes with padding;
4. terrain-fit or foundation plan succeeds within a bounded attempt count;
5. module placement succeeds completely;
6. chests, tile entities, signs, wires, liquids, and frames validate;
7. generation `StructureMap` and persistent registry receive the final rectangle;
8. failure records a diagnostic and uses a deterministic fallback or safely skips optional content.

### 14.5 Do not spend fixed entity pools on scenery

The inspected stable source allocates arrays for 201 NPC entries, 1001 projectile entries, 401 world-item entries, 8000 chests, and 1000 signs; the final slot in several gameplay arrays is a dummy/reserved entry, leaving the familiar 200 active NPC, 1000 projectile, and 400 loose-item budgets. Maximum players is 255. [Current tModLoader `Main.cs` patch](https://github.com/tModLoader/tModLoader/blob/stable/patches/tModLoader/Terraria/Main.cs.patch)

These are shared engine pools, not per-mod allowances. Apogean's bullet-hell bosses, invasions, ambient wildlife, other mods, and summons all compete for them. Corporate cities should therefore use visual tile objects for most screens, crates, corpses, terminals, and machinery; reserve NPCs, real chests, signs, and updating tile entities for interactions that need state.

### 14.6 Worldgen validation harness

World generation needs automated multi-seed testing before more structures are authored. For every world size and supported evil, record:

- generation completed within a time/working-set budget;
- same seed/config produces the same Apogean plan hash;
- spawn is safe and has enough buildable area;
- all required HQs, arenas, Maw Ruptures, and routes exist exactly once;
- no protected rectangles overlap;
- required entrances are reachable without bypassing progression locks;
- chests and tile entities occupy valid origins and contain valid items;
- no mandatory structure intersects oceans, Dungeon, Temple, or forbidden biome bands;
- required ore minimums are met after each simulated unlock;
- all bounded searches report success/fallback/skip and never loop forever.

Save the seed and diagnostics for any failure. A reproducible bad seed is a test case, not a one-off mystery.

## 15. Progression resources and faction ores

### 15.1 Feasibility

This is directly supported. A custom ore is an ordinary terrain `ModTile` with ore flags, map entry, dust/hit sound, spelunker and metal-detector behavior, `MinPick`, and `MineResist`. World-creation ores normally use a `GenPass`; post-boss ore can be generated on the server/single-player host with `WorldGen.OreRunner` or a custom vein algorithm. ExampleMod demonstrates both patterns. [ExampleOre](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/ExampleOre.cs)

tModLoader also exposes `ModifyHardmodeTasks`, which is appropriate for material that appears as hardmode begins. Later boss/quest unlocks happen during gameplay and must be synchronized and interruption-safe. [Official `ModSystem` documentation](https://docs.tmodloader.net/docs/stable/class_mod_system.html)

### 15.2 One world unlock, not per-player geology

Ore existence is shared world state. The first eligible completion sets one versioned unlock flag and schedules one generation job. Personal reputation/clearance may control recipes, buying, corporate refining services, or quest rewards, but two players cannot safely inhabit different geological versions of the same tilemap.

Each resource needs two equivalent unlock routes:

- **allied/contract route:** complete the corporation's extraction, survey, or boss contract;
- **hostile/independence route:** raid the same corporation or defeat its gatekeeper to seize the catalyst/scanner.

That preserves the Design Bible rule that politics changes acquisition rather than deleting class content.

### 15.3 Resource identity

Do not ship three recolored metal ores. The provisional identities should be mechanically different even before final names are chosen:

| Stage | Fiction and visual read | Extraction/gameplay distinction | Draft gate, not final balance |
|---|---|---|---|
| Kessler, immediately after Wall of Flesh / first contract | Dense military alloy or impact-reactive ferrite exposed by Kessler's landing/ordnance scan; gunmetal with signal-orange inclusions. | Tough conventional veins in deep stone and wreck fields; noisy mining or sparks can attract Kessler salvage units. | Cobalt/Palladium-class pick access; high `MineResist` keeps it substantial without an impossible lock. |
| Helix, after all mechanical bosses | Cultured bio-mineral that grows through stone after an enzyme/catalyst release; pale substrate with restrained clinical-green indicators. | Veins include vulnerable culture nodes or require harvesting intact samples; careless mining yields less refined material. | Mythril/Orichalcum through Adamantite/Titanium-class access, finalized by playtest. |
| Sentrix, after Plantera | Signal-bearing conductor crystal revealed/phase-locked by a Sentrix scan; black/cold-cyan precision. | Deposits may be shielded until nearby relays are disabled or align into exposed windows rather than simply taking longer to mine. | Pickaxe Axe/Drax-class access unless the story intentionally gates it after Golem. |

Approximate vanilla pick-power values often used for these bands are 110, 150/180, and 200; they are balance anchors, not decisions. `MineResist` can make a resource feel tougher without setting `MinPick` above the player's possible tools.

### 15.4 Runtime generation job

The official example queues gameplay-time ore work to a thread pool to avoid a visible stall. Apogean's operation is more stateful and needs crash recovery, structure avoidance, minimum-count validation, and potentially custom shapes. The recommended implementation is a server-authoritative, deterministic, resumable job:

```text
ResourceUnlockJob
  resource id + algorithm version
  deterministic seed
  planned candidate regions/veins
  current candidate index
  placed tile/vein counts
  started/completed/failed state
```

Process a bounded amount of work per world update on the main world thread, then synchronize coarse changed rectangles/sections. This trades a single freeze for a short world event that can show a message or scanner pulse. If profiling later proves a background worker safe for an isolated operation, adopt it deliberately; do not let multiple jobs mutate tiles concurrently.

Candidate selection must reject spawn sanctuary, housing, chests, signs, tile entities, wires/actuators, liquids where unsafe, all protected regions, and non-natural host materials. Generate a deterministic candidate list from world seed + resource ID + algorithm version so save/reload resumes the same job. Persist completion and never re-bless an already completed world.

### 15.5 Scarcity and multiplayer

Each initial unlock should produce enough material for a multiplayer group at the world size's expected density, and each faction should later offer a renewable but slower source—contracts, recycling, orbital drops, cultured growth, or procedural missions. Otherwise a finite world can strand late joiners or encourage resource theft. The exact reserve/renewal policy is a design decision to settle before recipes are balanced.

## 16. Dialogue trees and quest lines

### 16.1 There is no complete built-in quest framework

tModLoader provides UI composition, localization, save hooks, player/world state, achievements, and packets; it does not provide a finished branching-dialogue/quest engine. Apogean needs a custom story subsystem built on those primitives.

`UIState` and `UserInterface` are client presentation systems. A `ModSystem` updates them through `UpdateUI` and draws them by inserting an interface layer in `ModifyInterfaceLayers`. Dynamic elements need recalculation when their content changes; the root panel should set `Player.mouseInterface`, and scrollable UI may lock vanilla mouse-wheel hotbar input. [Basic UI guide](https://github.com/tModLoader/tModLoader/wiki/Basic-UI-Element) [Advanced custom UI guide](https://github.com/tModLoader/tModLoader/wiki/Advanced-guide-to-custom-UI)

World story data belongs in `ModSystem.SaveWorldData`/`LoadWorldData` plus compact `NetSend`/`NetReceive`. Personal story data belongs in `ModPlayer.SaveData`/`LoadData` and explicit player sync. Custom requests and deltas use `Mod.GetPacket`/`HandlePacket`. [Official `ModSystem` documentation](https://docs.tmodloader.net/docs/stable/class_mod_system.html) [Official `ModPlayer` documentation](https://docs.tmodloader.net/docs/stable/class_mod_player.html) [Official `Mod` networking documentation](https://docs.tmodloader.net/docs/stable/class_mod.html)

`ModAchievement` is useful for the later achievement book, but achievements are user-profile completion records and are not loaded on a dedicated server. They must not be the authority for world quests, branching choices, or item rewards. [Official `ModAchievement` documentation](https://docs.tmodloader.net/docs/stable/class_mod_achievement.html)

### 16.2 Calamity's useful pattern—and its limit

The reviewed public Calamity release loads dialogue data from JSON into a registry keyed by stable names, separates loading from rendering, supports localization revisions, and sends a packet that tells a client to start a dialogue display. [Calamity DialogueLoader](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Dialogues/DialogueLoader.cs) [Calamity dialogue UI](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/UI/DialogueDisplay/DialogueDisplayUI.cs) [Calamity start-dialogue packet](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Packets/StartDialogueDisplayPacket.cs)

That is a strong precedent for stable IDs and data-driven presentation. It is primarily a sequential display system, however; it does not remove Apogean's need for branching choice validation, quest objectives, world votes, reputation, hostile states, and reward authority.

### 16.3 Recommended story-domain model

Separate immutable definitions from mutable runtime state.

```text
Definitions (content, validated at load)
├── DialogueGraphDefinition
│   ├── stable graph id + revision + root node id
│   └── nodes: speaker key, text key, portrait/style, option list
├── DialogueOptionDefinition
│   └── stable option id, text key, next node, condition ids, effect ids
└── QuestDefinition
    └── stable id/revision/scope, prerequisites, objectives, rewards,
        failure policy, follow-up quests

Runtime state (saved and synchronized)
├── StoryWorldState
│   └── arrivals, alliance/vote, boss flags, ore unlocks, structure states
├── StoryPlayerState
│   └── reputation, clearance, hostile timer, personal quests, dialogue memory
└── EncounterPartyState
    └── temporary boss/revival/spectator/vote participation state
```

Data files should reference condition/effect/action IDs from whitelisted C# registries. They should not serialize arbitrary delegates. Dialogue graph structure can live in JSON, while player-facing prose lives in tModLoader HJSON localization keys. This permits localization, validation, migration, and safer content editing.

### 16.4 Server-authoritative dialogue protocol

```text
Player interacts with NPC/terminal
        ↓ request graph
Server validates NPC identity, distance, activity, world/player state
        ↓
Server resolves visible options and sends snapshot + short-lived token/revision
        ↓
Client UI renders speaker, body, options, requirements
        ↓ choose option id
Server revalidates graph/node/option/token and executes registered effects
        ↓
Server saves/syncs story and quest deltas, returns next snapshot or closes
```

The client may animate text, portraits, hover states, sound, and selection. It may not grant an item, start a quest, change reputation, make a faction hostile, open a shared door, resolve a vote, or unlock an ore. This directly replaces the prototype's local `Action<Player>` call.

Minimum packets:

- request/open interaction;
- dialogue snapshot;
- choose-option request;
- choice result/next snapshot;
- quest delta and full resync;
- story-world delta/full state;
- world-vote start, ballot, tally/result;
- optional interaction cancellation/reason.

All packet readers validate bounds, IDs, sender ownership, expected state, distance, and revision before mutation. Joining players receive a compact full state; normal play receives deltas.

### 16.5 Quest engine

Quest objectives should be event-driven rather than scanning every quest every tick. Objective handlers subscribe to normalized story events:

- NPC/boss killed;
- event wave or invasion completed;
- item obtained, crafted, delivered, or spent;
- biome/structure entered;
- dialogue option chosen;
- terminal/repair object activated;
- ore/sample mined;
- arena component repaired;
- faction standing or world flag changed.

An index maps event type to currently relevant objectives. The server updates progress, emits a delta, and grants rewards once. If inventory is full, a claim queue/mailbox or explicit turn-in prevents dropped/duplicated rewards.

Quest scopes must be first-class:

| Scope | Examples |
|---|---|
| Player | Personal reputation, clearance, purchase unlocks, individual dialogue memory. |
| World | Corporation arrival, first boss defeat, ore blessing, alliance, HQ state. |
| Party/encounter | Shared boss objective, revive/spectator state, temporary raid result. |
| Vote | Major story decision requiring currently eligible players and host override policy. |

Story quests should not permanently fail and erase progression. Failure can alter respect, prices, dialogue, or the next encounter, then expose a retry/recovery route. The independence path and hostile raids must reach equivalent progression gates.

### 16.6 Dialogue and journal UI

Use two surfaces:

1. **Dialogue overlay:** bottom-center panel with speaker portrait/name, readable text, 2–5 visible choices, requirement/reputation hints, scroll support, keyboard/gamepad focus, and explicit close. It does not pause multiplayer; it should close or enter a warning state when the player leaves range, becomes hostile, dies, or the NPC disappears.
2. **Quest journal/star-chart shell:** a larger `IngameFancyUI`-style screen for active/completed quests, faction standings, world history, vote records, and later star destinations. This is presentation over server-synchronized state, not the state owner.

Accessibility requirements: UI-scale testing, 1280×720 through ultrawide, color-independent choice states, controller navigation, text-speed/instant-text toggle, reduce-motion mode, and a dialogue-history pane. Rebuild/recalculate only when data changes, not every draw frame.

### 16.7 Load-time validators and migrations

Fail development builds loudly for:

- duplicate graph/node/option/quest IDs;
- missing roots or target nodes;
- unreachable nodes and accidental infinite cycles;
- gated nodes with no fallback/exit;
- missing localization, condition, effect, objective, or reward IDs;
- duplicate reward paths or one-time effects reachable repeatedly;
- invalid quest prerequisites/follow-ups;
- scope mismatch, such as a player quest directly setting a world alliance.

Persist a story schema version. Migrations map renamed IDs and old state shapes. If a definition is removed, quarantine the old state and allow safe abandonment/recovery instead of crashing the world.

## 17. Multiplayer and persistence rules across all systems

The same authority rule should govern worldgen transitions, structures, quests, votes, and space travel:

- clients request and render;
- the server validates and mutates;
- world-level facts are written once to `StoryWorldState`/world systems;
- personal facts are written to the initiating `ModPlayer`;
- temporary encounter facts are discarded or summarized after the encounter;
- late joiners receive full compact state, then deltas;
- tile changes use tile/section synchronization rather than duplicating tile grids inside mod packets.

No saved structure template, dialogue graph, quest definition, or full tile map should be serialized into every player/world record. Save stable IDs, versions, seeds, bounds, counters, and state. Content definitions come from the installed mod.

Dedicated-server tests are mandatory for:

- simultaneous dialogue choices at one NPC;
- duplicate interaction/reward packets;
- disconnect during a vote or quest turn-in;
- player join during a resource generation job;
- world save/exit during a staged conversion;
- two players entering/leaving a boss arena while revive state is active;
- late join after a faction HQ opens;
- subworld transition, death, recall, reconnect, and host shutdown.

## 18. Space expansion feasibility and boundaries

Space is feasible if it is a collection of destinations, not a single physically contiguous universe. Subworld Library provides custom dimensions, generation tasks, lifecycle hooks, save policies, transition logic, and multiplayer subservers; tModLoader itself still does not expose native first-class arbitrary dimensions. Its hook-heavy implementation is a real dependency risk, but writing an equivalent system inside Apogean would be much riskier. [Subworld Library](https://github.com/jjohnsnaill/SubworldLibrary) [Open tModLoader multiple-dimensions issue](https://github.com/tModLoader/tModLoader/issues/1290)

Recommended future topology:

- Earth remains the authoritative campaign world;
- the ship's upgrades and star chart are compact Earth/world story state;
- a small persistent ship interior may become a subworld if tile-by-tile building is required;
- handcrafted story planets are persistent bounded subworlds, initially no larger than vanilla small/medium;
- procedural completion missions are deterministic temporary subworlds (`ShouldSave = false`) and persist only seed, node status, outcomes, and unique discoveries;
- only destinations currently occupied by players exist as active processes.

Create an Apogean-owned interface such as `IWorldDestinationService` before taking the dependency. Earth/Act 1 can use a null implementation; a later proof can use Subworld Library. This keeps story and UI code from calling dependency APIs everywhere.

The first proof must answer, with measurements:

1. Can a party enter one handcrafted small planet, save, leave, reload, and return deterministically?
2. What happens to inventory, buffs, death, recall, spawn, quests, invasions, and boss state?
3. What is total RAM/CPU with one and then two occupied subworlds?
4. Can a dedicated server recover cleanly after a subserver crash/disconnect?
5. How are subworld saves backed up/deleted with the parent world?
6. Does the selected Subworld Library release work with the exact pinned tModLoader build and common mods?

Until that proof passes, the star map remains a roadmap interface, not a dependency in Act 1.

## 19. Compatibility posture

Calamity is mostly an additive-worldgen reference in the reviewed public release: it inserts named passes, scales feature counts, validates placements, and uses protected structures. Remnants is a total-overhaul reference: it disables/replaces major vanilla passes and known conflicting generation. Apogean cannot simultaneously promise a drastic replacement and universal compatibility.

Ship two explicit generation profiles:

- **Apogean Overhaul (intended/default for the full campaign):** dead-Earth baseline, guaranteed corporation territories, Engraft morphology, major infrastructure and progression systems. It owns substantial generation order.
- **Compatibility/Classic:** additive Engraft regions, smaller ruins/outposts, story systems, bosses, items, and limited backgrounds without replacing broad vanilla/modded terrain.

At world creation, detect known high-conflict mods and show an honest warning or select Compatibility mode only with user consent. Never silently disable another mod's generation. Maintain a versioned test matrix containing vanilla, Apogean alone, Calamity, Remnants, Subworld Library, and explicitly supported combinations.

Special seeds (`drunkWorldGen`, `remixWorldGen`, `everythingWorldGen`, and others) violate ordinary assumptions about layers and pass behavior. First release policy should be either tested explicit support or a guarded compatibility fallback with a generation warning—not accidental partial support.

## 20. Recommended code architecture

```text
Common/
├── WorldGeneration/
│   ├── ApogeanWorldGenSystem          # pass orchestration only
│   ├── WorldPlan / WorldPlanBuilder   # deterministic placement plan
│   ├── ProtectedRegionRegistry        # saved runtime protection
│   ├── Validation/                    # invariants and diagnostics
│   ├── Terrain/                       # dead Earth + Engraft morphology
│   ├── Structures/                    # module loader, planners, placers
│   └── Resources/                     # resumable unlock jobs
├── Biomes/
│   ├── TileCountSystem
│   ├── ScenePriorityRouter
│   └── BackgroundVariantSystem
├── Story/
│   ├── Definitions/                   # dialogue/quest immutable data
│   ├── Registries/                    # conditions/effects/objectives/rewards
│   ├── State/                         # world/player/encounter scopes
│   ├── Runtime/                       # dialogue and quest services
│   ├── Validation/                    # graph/quest validators + migrations
│   └── Networking/                    # typed packet handlers
├── UI/
│   ├── Dialogue/
│   ├── Journal/
│   └── Voting/
└── Destinations/
    └── IWorldDestinationService       # no Subworld dependency in Act 1

Content/
├── Tiles/DeadEarth, Engraft, Kessler, Helix, Sentrix, SharedRuins
├── Walls/...
├── TileEntities/...
├── Biomes/...
├── Items/Resources/...
└── Story/Definitions + Localization
```

The deep-module rule is important: world passes should ask `WorldPlan` where things go; spread and ore should ask `ProtectedRegionRegistry` what they may touch; UI should ask story runtime for snapshots; packet handlers should call story services. No individual boss, NPC, or tile should own a second copy of campaign truth.

## 21. Development sequence that minimizes rework

### Phase 0 — Freeze contracts and build test tooling

- Pin the supported tModLoader stable version in project documentation/build metadata.
- Add world schema/story schema versions and generation-mode data.
- Build a debug worldgen log and seed replay command.
- Build tile adjacency and background seam test scenes.
- Build server packet test helpers and definition validators.

### Phase 1 — One complete dead-Earth vertical slice

- Replace `DeadGrass` with a correct framed terrain sheet and behavior.
- Add dead soil, one dead tree/sapling, two scrub/rubble objects, one wall, and one road material.
- Build one forest far/middle/close set and the four underground textures.
- Generate a safe dead spawn and one short ruined road using `WorldPlan` and protected regions.
- Validate all world sizes/seeds before expanding art.

This slice proves tiles, vegetation, background seams, pass order, spawn safety, protection, and art scale at once.

### Phase 2 — Engraft vertical slice

- Replace the ellipse/top-row conversion with a volumetric Maw Rupture grammar.
- Add framed turf/subsoil/graftstone, cords, one cyst light source, walls, hanging growth, and node placement.
- Use tile-count biome activation and a distinct background.
- Add bounded pre-Hardmode spread and protection checks.
- Rebuild one enemy interaction that uses terrain pressure/tether behavior.

### Phase 3 — Structure pipeline

- Implement a tiny versioned module format and validator.
- Place one multi-room abandoned outpost with terrain blending, chest, terminal tile entity, damaged variant, and protected rectangle.
- Generate one sealed Kessler territory shell; progression only opens/activates it.
- Run multi-seed validation before authoring Helix/Sentrix equivalents.

### Phase 4 — Story platform

- Replace client-side delegate dialogue with a 3-node data-driven graph and server round trip.
- Add one player-scoped quest, one world-scoped quest, save/load, join sync, localization, journal entry, and idempotent reward.
- Add vote protocol only after ordinary choices are proven.

### Phase 5 — One progression resource

- Implement one Kessler resource with framed ore art and protected-host rules.
- Unlock it through one allied quest and one hostile boss/raid fallback.
- Run/resume the deterministic generation job in single-player and dedicated multiplayer.
- Balance density and renewable fallback before adding Helix/Sentrix resources.

### Phase 6 — Scale Act 1, then corporations

- Expand biome kits and background compositions from proven templates.
- Replace placeholder compounds and integrate Nest Warden/Matriarch with the new Engraft world contract.
- Deliver Kessler arrival/invasion/quartermaster/walkframe.
- Freeze Helix/Sentrix and space as later acts until the Act 1 world/story foundation survives multiplayer and compatibility testing.

### Phase 7 — Space proof, much later

- Implement `IWorldDestinationService` using a pinned Subworld Library version.
- Ship one internal handcrafted planet proof and profile it.
- Only then design persistent ship building and procedural star-chart completion content.

## 22. Risk register

| Risk | Severity | Mitigation/gate |
|---|---|---|
| Full overhaul conflicts with Remnants/other generators | High | Explicit generation profiles, detection/warning, compatibility matrix, no universal claim. |
| Worldgen corrupts or omits mandatory structures | High | Deterministic plan, bounded attempts, fallbacks, reservations, multi-seed invariants. |
| Runtime conversion overwrites player work | High | Natural-tile allowlist, persistent protected regions, staged server job, backups/versioning. |
| Client can spoof dialogue rewards/choices | High | Server-authoritative snapshots/tokens and whitelisted effect handlers. |
| Story saves break after IDs change | High | Stable IDs, schema versions, validators and migrations. |
| Bullet hell/invasions exhaust fixed pools | High | Shared NPC/projectile budgets, pooled visuals, caps, stress tests. |
| Huge art backlog before systems stabilize | High | Native-scale vertical slice and reusable tileset contracts first. |
| Large backgrounds seam, detach, or bake the sky | Medium | Native far/mid/close APIs, transparent tileable layers, seam test scene, stable variants. |
| Ores strand late-joining multiplayer players | Medium | World-scaled initial yield plus renewable contract/recycling path. |
| Subworld dependency breaks on loader update | High later | Abstraction, pinned versions, one-planet proof, dedicated-server transition suite. |
| Development PC hides low-memory stalls | Medium | 16 GB/low-memory and dedicated-server profiling with peak working set. |

## 23. Bottom-line engineering decisions

The research supports these defaults:

1. Keep Earth at vanilla small/medium/large dimensions; never make the main world an enormous universe map.
2. Treat Apogean Overhaul as a new-world generator and ship a smaller compatibility profile.
3. Replace the current recolor/ellipse/9×9 prototypes with a deterministic `WorldPlan`, proper tilesets, and protected authored/procedural regions.
4. Generate major corporate shells during world creation and activate them through progression.
5. Make faction resources world-global, server-generated unlocks with allied and hostile routes.
6. Build a custom data-driven quest/dialogue engine whose client is only presentation.
7. Defer Subworld Library until one Act 1 world/story stack is stable, but isolate future destination calls behind an interface now.
8. Profile against ordinary 16 GB users and the engine's fixed entity pools, not only this workstation.

These choices make the project large but tractable: every later biome, corporation, boss arena, quest, ore, ship upgrade, and planet plugs into a tested system rather than inventing its own world state.
