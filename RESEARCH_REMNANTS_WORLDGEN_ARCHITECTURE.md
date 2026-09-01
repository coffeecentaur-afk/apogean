# Remnants World-Generation Architecture Audit

Date: 2026-09-01

Audited project: [lazy-wombat/Remnants](https://github.com/lazy-wombat/Remnants)

Audited commit: [`9c2cbf9`](https://github.com/lazy-wombat/Remnants/tree/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389)

## Question

Can Apogee generate the Maw without blindly deleting every chest, hive, tree, ore vein, and minor structure that intersects a late world-generation corridor?

Yes. Remnants demonstrates a better planning model, but it also demonstrates an important compatibility boundary: Remnants is effectively a full world-generation replacement. Apogee should borrow its early ownership map and structure-reservation discipline, not copy its wholesale removal of vanilla generation.

## Verified findings

### 1. Remnants owns the terrain pipeline

Remnants does not place a large biome after vanilla has finished and then repair the collisions. It inserts custom terrain and cave passes, then removes many vanilla terrain-detail passes, including dirt/rock mixing, grass, clay, silt, cave systems, lakes, wet jungle, oasis, and ore passes. See [`TerrainPasses.ModifyWorldGenTasks`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Terrain.cs#L30-L84).

It similarly replaces primary and secondary biome production. Ice, jungle, desert, underworld, corruption, beaches, mushroom patches, marble, granite, spider caves, gem caves, ocean caves, and shimmer are removed or replaced by Remnants-owned passes. See [`BiomePasses.ModifyWorldGenTasks`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Biomes.cs#L32-L83).

This means Remnants avoids many conflicts by controlling the producers. It is not evidence that a late additive pass can safely erase arbitrary vanilla or modded content.

### 2. Remnants creates an authoritative biome map before features

Remnants inserts `BiomeMapSetup` at the beginning of the task list and populates its biome map immediately after terrain. The map uses 50-tile cells plus world-sized blend and material-noise arrays. See [`BiomeMap.ModifyWorldGenTasks` and setup fields](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Biomes.cs#L191-L251).

Later code asks `FindBiome(x, y)` instead of inferring ownership from whichever tiles or walls survived. Noise-displaced lookup coordinates soften borders while preserving one authoritative biome identity. See [`FindBiome`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Biomes.cs#L281-L299) and [`BiomeMapPopulation`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Biomes.cs#L306-L334).

This is the strongest reusable idea for Apogee: the Maw needs a saved ownership and navigation plan before terrain conversion, walls, liquids, backgrounds, structures, and validation are applied.

### 3. Remnants replaces conflicting structure passes

Remnants removes vanilla surface chests, buried chests, and micro-biomes. It replaces floating islands, pyramids, hives, living trees, and traps with its own generators. See [`StructurePasses.ModifyWorldGenTasks`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L37-L73).

It also replaces the dungeon and jungle temple passes. See [`DungeonPasses.ModifyWorldGenTasks`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Dungeons.cs#L34-L55).

That is why a Remnants hive or giant tree can respect Remnants' biome layout: both sides were authored inside the same pipeline. Apogee cannot assume that relationship with every vanilla or third-party generator.

### 4. Remnants uses `StructureMap` as a shared reservation system

Before placing custom structures, Remnants frequently calls `GenVars.structures.CanPlace(...)`. After successful placement, it immediately calls `GenVars.structures.AddProtectedStructure(...)`. Giant-tree rooms show the check and registration at [`Structures.cs` lines 965-1120](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L965-L1120). Beehives check biome ownership and `CanPlace` before registering their footprint at [`Structures.cs` lines 4223-4281](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/World/Structures.cs#L4223-L4281).

This matches the official tModLoader [`StructureMap`](https://docs.tmodloader.net/docs/stable/class_structure_map.html) contract: generators should check whether a region can be placed and register important areas so later cooperating generators avoid them.

Important limitation: `StructureMap` is cooperative. It cannot retroactively prevent an earlier feature, and it cannot force another mod's generator to check the map. Apogee therefore needs both shared reservations and its own fallback validation/rerouting.

### 5. Remnants uses real liquids and water styles

Remnants assigns vanilla liquid types and amounts during generation rather than simulating every pool with solid tiles. Its water-style implementation is a normal tModLoader `ModWaterStyle`; the acid style supplies waterfall, splash, and droplet visuals. See [`Acid.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/Biomes/Waters/Acid.cs#L1-L22) and the official [`ModWaterStyle`](https://docs.tmodloader.net/docs/stable/class_mod_water_style.html) documentation.

The audited code does **not** prove that Remnants' current Sulfuric Vents actively select this acid water style, so it should not be treated as a complete hazard reference. It does confirm that animated, flowing liquid presentation belongs in the water-style layer. Damage, debuffs, and local acid ownership remain separate gameplay responsibilities.

## What Apogee should adopt

Apogee should use a hybrid additive architecture. It should remain compatible with ordinary Terraria generation and other content mods while giving the Maw deterministic ownership of the space it truly needs.

### Phase A: early Maw planning

Run a planning pass after vanilla has established macro-biome locations but before hives, living trees, chests, and micro-biomes are placed.

The planner should:

- evaluate multiple candidate horizontal bands;
- hard-exclude spawn sanctuary, oceans, dungeon, temple reservation, and other non-negotiable landmarks;
- prefer waste/forest or another approved host biome instead of cutting through the jungle by default;
- create a deterministic curved navigation spine from the Feeding Wound to the Burning Root;
- create chamber, branch, shell, and acid-basin footprints around that spine;
- store an ownership mask or compact segments in `MawRupturePlan`;
- register only the required spine/chambers in `GenVars.structures` and Apogee's own protected-region registry.

### Phase B: terrain construction

Run the main Maw terrain pass after ordinary terrain and caves exist. Convert and carve only inside the saved ownership mask.

The generator should guarantee a winding, playable route rather than a straight freefall. The route can include platforms, ledges, short drops, and chambers, but every segment must preserve at least player-sized clearance and a connected path. Decorative shell thickness and teeth must never close the navigation spine.

Ordinary ore outside the spine may remain as geological inclusions. Ore, silt, and loose tiles inside the reserved spine should be converted or cleared because the route was claimed before they were generated. This is targeted ownership, not indiscriminate deletion.

### Phase C: structure coexistence

Temple and dungeon remain hard exclusions. If a protected or unknown structure intersects a proposed Maw segment, the generator should attempt a bounded local reroute around it. If no safe reroute exists, reject that candidate plan and choose another site.

For features that honor `StructureMap`, the early reservation prevents overlap. For features that do not, a late validator detects foreign framed tiles, containers, walls, and registered regions. The validator must not simply erase them.

This preserves jungle hives, giant trees, cabins, chests, and third-party structures unless the Maw's plan legitimately found clear space before their placement.

### Phase D: liquids and finalization

Replace the current solid acid-pool simulation with actual vanilla water in explicitly owned Maw acid basins. A Maw water style supplies amber-yellow rendering. A separate player system applies digestion damage/debuffs only when the player is wet and inside an acid-owned Maw region.

After major structure passes, run a narrow finalization pass that:

- validates route connectivity;
- clears only natural debris that appeared inside the pre-reserved navigation spine;
- reroutes around protected or foreign structures rather than destroying them;
- places acid-basin water after geometry is stable;
- allows Terraria's later liquid-settling pass to resolve flow.

Water entering from outside can then behave like water. It should only become hazardous where the saved acid-basin/biome rule says it is digestive fluid, avoiding a fake block beside real water.

### Phase E: runtime identity and validation

Biome walls, backgrounds, lighting, music, and water style should use the saved Maw ownership map plus explicit scene priority. Raw tile counts alone are insufficient where the Maw intersects or approaches another biome.

Deterministic generation tests should verify:

- a connected route from surface mouth to the underworld destination;
- no required route cell narrower than the selected clearance envelope;
- no temple, dungeon, ocean, or spawn-sanctuary overlap;
- no foreign containers or frame-important structures inside the owned spine;
- no legacy solid acid tiles;
- acid water exists only in declared basins;
- world generation completes for multiple seeds and all vanilla world sizes.

## Decision

The previous idea that the Maw should consume all ordinary intersecting chests, hives, trees, ore, silt, and microstructures is rejected.

The replacement is an early plan-and-reserve system with a deterministic navigation spine, shared `StructureMap` registration, bounded rerouting, a later terrain pass, and a final validation/liquid pass. This gives the Maw a strong authored silhouette while preserving Terraria's world and avoiding Remnants' total-overhaul compatibility cost.
