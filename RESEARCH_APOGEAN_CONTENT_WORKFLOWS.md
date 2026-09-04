# Apogean reusable content authoring workflows

**Research date:** 2026-09-04
**Scope:** a repository-specific playbook for adding content without turning framing, worldgen, multiplayer, or progression failures into late discoveries.

## Evidence, version, and reuse boundary

### VERIFIED ENGINE/SOURCE FACTS

The documentation indexes identified **stable as tModLoader v2026.07** and **preview as v2026.08** when checked. Some generated leaf pages still carry an older v2026.06/v2026.07 footer, so these are rolling URLs rather than immutable snapshots. Use ExampleMod's `stable` branch for released tModLoader and the `1.4.5` branch only when deliberately validating preview behavior. Before adopting a preview-only member, confirm the installed target, build against it, and record the branch/API URL with the change. [Stable docs](https://docs.tmodloader.net/docs/stable/index.html) · [Preview docs](https://docs.tmodloader.net/docs/preview/index.html) · [ExampleMod branch warning](https://github.com/tModLoader/tModLoader/blob/1.4.5/ExampleMod/README.md)

Facts in sections marked **VERIFIED** are statements supported by owner documentation or source. Sections marked **INFERRED DESIGN INSPIRATION** are original recommendations for Apogean, not claims about an engine or another game's internal design rules.

Public source is a reference, not a license to transplant it:

- The [Calamity public-source license](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/LICENSE.md) permits reference use but places conditions on copied code and prohibits redistribution. This document extracts architecture lessons only.
- No repository-root README or license was found at the expected paths in the public [Remnants repository](https://github.com/lazy-wombat/Remnants) when checked. Treat it as view-only design evidence unless the owner supplies permission.
- Borderlands and Titanfall/Titanfall 2 are inspiration only. Never copy assets, names, factions, dialogue, objective text, level layouts, encounter scripts, terminology, or code.

## The reusable authoring loop

Every tree, tile family, structure, background, NPC, boss, or progression feature starts with the same compact contract:

| Contract field | Required decision |
|---|---|
| Player promise | What can the player notice, learn, unlock, or safely ignore? |
| Engine owner | Which existing type/system owns selection, placement, framing, AI, or saving? |
| Authority | Client cosmetic, server-owned entity/world state, or synchronized state? |
| Lifetime | Per draw, entity lifetime, world lifetime, or permanent save data? |
| Bounds | Exact footprint/radius, edge padding, retry limit, and fallback/skip result. |
| Version | Schema/version and migration behavior for saves or authored templates. |
| Proof | Fixture plus single-player, reload, dedicated-server, and deterministic-seed checks. |

Promote content through these stages:

1. **Contract:** identify the existing Apogean owner and the riskiest assumption.
2. **Minimal probe:** build the smallest tree/object/room/attack/background that can falsify that assumption.
3. **Fixture:** put the probe in the relevant `Content/Diagnostics` gallery or deterministic test world before making an asset family.
4. **Authority pass:** label every state write as client visual, server entity, or persistent world data; remove dual ownership.
5. **Failure pass:** force invalid anchors, occupied footprints, missing pass names, late join, reload, cancellation, and despawn.
6. **Promotion:** only then turn the probe into a kit, template, or encounter vocabulary.

Extend the repository's current seams instead of making parallel systems:

| Concern | Existing Apogean seam |
|---|---|
| Worldgen pass ordering | `ApogeanWorldGenerationSystem` |
| Planned landmark/save/network intent | `ApogeanWorldPlanSystem` |
| Authored structure validation/placement | `AuthoredStructureTemplate` and `Content/Structures/Blueprints` |
| Native tree behavior | `DeadForestTree` and `DeadForestSapling` |
| Multi-tile object conventions | `CorporateFurnitureTiles` |
| Background choice/rendering | `RuinedBackgroundSelectionSystem` and the surface/underground style classes |
| Persistent faction progress | `FactionProgression` and its owning world system |
| Visual regression fixtures | `Content/Diagnostics` galleries |

## 1. Trees, terrain tiles, walls, and furniture

### VERIFIED ENGINE/SOURCE FACTS

- A `ModTree` describes a tree species growing on one or more soil tile types while using the shared vanilla tree tile. It supplies trunk/top/branch textures, sapling and wood drops, growth soil, shake behavior, and foliage framing. Branch textures are expected as 40×40 and tops default to 80×80; `SetTreeFoliageSettings` chooses the source frame/offsets. [Stable `ModTree`](https://docs.tmodloader.net/docs/stable/class_mod_tree.html) · [Preview `ModTree`](https://docs.tmodloader.net/docs/preview/class_mod_tree.html) · [stable ExampleTree](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Plants/ExampleTree.cs) · [stable ExampleSapling](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Plants/ExampleSapling.cs)
- `TileObjectData` is the placement contract for multi-tiles: copy a matching template first, then set dimensions, coordinate heights, origin, anchors, direction, style wrapping, and alternates, and call `addTile` last. `GetTileData(Tile)`, `TopLeft`, and `IsTopLeft` avoid duplicating frame arithmetic after placement. [Stable `TileObjectData`](https://docs.tmodloader.net/docs/stable/class_tile_object_data.html) · [Preview `TileObjectData`](https://docs.tmodloader.net/docs/preview/class_tile_object_data.html) · [ExampleChair](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Furniture/ExampleChair.cs) · [TileObjectDataShowcase](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/TileObjectDataShowcase.cs)
- `DrawFlipHorizontal` affects placement preview; a placed object that should face a direction must apply the corresponding sprite effect in its tile drawing hook. [Stable `TileObjectData`](https://docs.tmodloader.net/docs/stable/class_tile_object_data.html) · [ExampleChair](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Furniture/ExampleChair.cs)
- Returning `false` from `ModWall.WallFrame` makes the mod responsible for `WallFrameNumber`, `WallFrameX`, and `WallFrameY`. The documented style value behaves like a neighbor bitmask; fully surrounded walls use styles 15–19. Unsafe housing walls should register their item drop deliberately. [Stable `ModWall`](https://docs.tmodloader.net/docs/stable/class_mod_wall.html)
- The same wall hooks are present in the current [preview `ModWall` API](https://docs.tmodloader.net/docs/preview/class_mod_wall.html); stable ExampleMod's [ExampleWall](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Walls/ExampleWall.cs) is the owner-maintained baseline for housing, fallback, dust, and map registration.
- The supported conversion path is `WorldGen.ConvertTile`/`ConvertWall`; those methods handle framing and multiplayer synchronization. Runtime manual placement must otherwise perform the appropriate frame and tile-sync work. [Stable `WorldGen`](https://docs.tmodloader.net/docs/stable/class_world_gen.html)
- ExampleMod keeps custom frame logic isolated in a dedicated example rather than mixing it into generic object placement. [ExampleCustomFramingTile](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/ExampleCustomFramingTile.cs)

### Apogean workflow

1. **Choose the native abstraction.** A harvestable/growing tree starts from `Content/Tiles/DeadForestTree.cs` and `DeadForestSapling.cs`. A prop starts from a matching `TileObjectData.Style*` example. A terrain/wall family owns an explicit merge/framing contract.
2. **Write the physical contract before art:** solid/platform/attachable, slope and half-block behavior, anchors, housing safety, mined/protected state, drops, conversion, paint, actuators, liquids, light, map color, and interaction.
3. **Author from engine dimensions.** Make each sheet satisfy the API/template geometry. Keep style/alternate data near the tile type, not hidden in placement code.
4. **Place via the engine contract.** Player and worldgen placement use the same registered object data; authored structures call `WorldGen.PlaceObject` rather than writing arbitrary frame values.
5. **Frame in dependency order.** For a stamped room: write shell tiles/walls, frame the shell, then place furniture so its anchors inspect final floor/wall state. Frame the bounded perimeter once more after blending.
6. **Prove a family, not one happy frame.** The gallery should show every edge/corner, isolated tile, internal hole, slopes/half-blocks, every style and direction, paint, actuator, liquid exposure, player placement/break, and blueprint placement.

### Failure modes and required response

- **Decorative fake tree:** chopping, shaking, acorns, regrowth, paint, and branch/top behavior diverge. Rebuild it as `ModTree` instead of patching more behavior onto a monolithic multi-tile.
- **Soil/sapling disagreement:** the tree cannot grow or grows into the wrong species. Keep `GrowsOnTileId`, sapling style, drops, and `SetStaticDefaults` registrations as one reviewed contract.
- **Raw frame math:** styles or alternates break after a sheet change. Resolve `TileObjectData` and top-left coordinates at runtime.
- **Origin/anchor mismatch:** source art looks valid but placement fails or breaks incorrectly. Add the object to the gallery and stamp it through `AuthoredStructureTemplate` before expanding the set.
- **Indestructible dead-end:** `CanKillTile` prevents player recovery and developer testing. Reserve indestructibility for a real progression contract and retain an intentional debug/admin recovery path.
- **Unframed bulk write:** seams and invalid objects appear after generation. Use shell → frame → object placement and keep all edits bounded.

## 2. Surface/underground backgrounds and skies

### VERIFIED ENGINE/SOURCE FACTS

- Current preview counterparts are available for [surface styles](https://docs.tmodloader.net/docs/preview/class_mod_surface_background_style.html), [underground styles](https://docs.tmodloader.net/docs/preview/class_mod_underground_background_style.html), and [`CustomSky`](https://docs.tmodloader.net/docs/preview/class_custom_sky.html). Keep preview experiments isolated until they compile against the installed target.
- A `ModSceneEffect` selects eligible surface/underground styles; priority resolves competing scene effects. [Stable `ModSceneEffect`](https://docs.tmodloader.net/docs/stable/class_mod_scene_effect.html) · [ExampleSurfaceBiome](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Biomes/ExampleSurfaceBiome.cs)
- `ModSurfaceBackgroundStyle` chooses far, middle, and close textures. In `ModifyFarFades`, the style should move its own slot toward 1 and competing slots toward 0 by the supplied transition speed. Returning `false` from `PreDrawCloseBackground` suppresses the normal close layer and transfers responsibility to custom drawing. [Stable `ModSurfaceBackgroundStyle`](https://docs.tmodloader.net/docs/stable/class_mod_surface_background_style.html) · [ExampleSurfaceBackgroundStyle](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs)
- `ModUndergroundBackgroundStyle.FillTextureArray` assigns four semantic slots: sky/ground border, ground-to-rock, rock-to-ground, and rock. The documented sheets use a 32-pixel horizontal repeat seam where the rightmost 32 pixels duplicate the leftmost 32. [Stable `ModUndergroundBackgroundStyle`](https://docs.tmodloader.net/docs/stable/class_mod_underground_background_style.html) · [ExampleUndergroundBackgroundStyle](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs)
- `CustomSky` has an independent lifecycle: `Activate`, `Deactivate`, `Reset`, `Update`, `Draw`, and `IsActive`. It is a client-rendering effect, not gameplay state. [Stable `CustomSky`](https://docs.tmodloader.net/docs/stable/class_custom_sky.html) · [BackgroundTextureLoader](https://docs.tmodloader.net/docs/stable/class_background_texture_loader.html)

### Apogean workflow

1. **Separate selection from rendering.** Compute biome/depth/progression/world-variant once in `RuinedBackgroundSelectionSystem`, then give the same immutable selection to surface, underground, and sky renderers.
2. **Choose lifetime deliberately.** Save/network a per-world variant when continuity matters. Never save camera position or fade. Never choose a new random texture per draw.
3. **Keep layer roles stable.** Surface art remains matched far/middle/close parallax; day/night/eclipse share geometry unless the contract explicitly changes it. Underground textures honor the four engine slots and repeat seam.
4. **Keep custom sky independent.** `RuinedUnderworldSky` owns activation/deactivation and visual interpolation. No boss, biome, loot, or world flag may rely on whether a client has drawn the sky.
5. **Fixture the transition matrix:** surface/underground/cavern/Underworld thresholds, biome edges, day/night/eclipse, zoom, map/capture, death/teleport, reload, late join, and dedicated server. Use `SurfaceBackgroundLabGallery` and `UndergroundBackgroundLabGallery`.

### Failure modes

- **Per-frame random choice:** shimmer, capture seams, and client disagreement. Seed or persist at the correct lifetime.
- **Competing fade slots left active:** backgrounds double-expose during transitions. Drive the selected slot up and every other slot down.
- **Panorama forced through a style hook:** parallax or activation is wrong. Give a true sky/panorama its own lifecycle and test it separately.
- **Client graphics on server:** dedicated-server crashes or load faults. Guard texture, sky-manager, and draw-only access with the repository's client-safe pattern.
- **Color-only biome distinction:** players lose the read under lighting/color-vision differences. Preserve silhouette, depth frequency, and landmark rhythm as redundant signals.

## 3. Authored structures and world generation

### VERIFIED ENGINE/SOURCE FACTS

- `ModSystem.ModifyWorldGenTasks` is the hook for inserting/removing/reordering generation passes. The stable ExampleMod ore example finds a named pass and inserts a bounded `GenPass` relative to it. [Stable `ModSystem`](https://docs.tmodloader.net/docs/stable/class_mod_system.html) · [Preview `ModSystem`](https://docs.tmodloader.net/docs/preview/class_mod_system.html) · [stable ExampleOre](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/ExampleOre.cs)
- The worldgen overlap registry is `GenVars.structures`. `StructureMap.CanPlace` checks a rectangle/padding against protected structures and disallowed existing tiles; `AddProtectedStructure` records the accepted footprint. It is not a campaign-state database. [Stable `StructureMap`](https://docs.tmodloader.net/docs/stable/class_structure_map.html) · [Preview `StructureMap`](https://docs.tmodloader.net/docs/preview/class_structure_map.html) · [preview source patch](https://github.com/tModLoader/tModLoader/blob/1.4.5/patches/tModLoader/Terraria/WorldBuilding/StructureMap.cs.patch)
- `WorldGen.PlaceObject` resolves registered styles/alternates for furniture. Runtime world edits are server-owned and require the documented synchronization path; conversion helpers frame and synchronize their bounded changes. [Stable `WorldGen`](https://docs.tmodloader.net/docs/stable/class_world_gen.html) · [Stable `NetMessage`](https://docs.tmodloader.net/docs/stable/class_net_message.html)
- `ModSystem` provides `ClearWorld`, `SaveWorldData`/`LoadWorldData`, and `NetSend`/`NetReceive`. Owner documentation explicitly recommends defensive loading because saved data can be missing or old. [Stable `ModSystem`](https://docs.tmodloader.net/docs/stable/class_mod_system.html)

### VERIFIED PUBLIC-SOURCE CASE STUDIES

- Calamity separates pass orchestration from feature generators in [WorldgenManagementSystem](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Systems/World/WorldgenManagementSystem.cs). Its [DraedonStructures](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/World/DraedonStructures.cs) source demonstrates finite candidate searches, placement checks, schematic placement, protection padding, and explicit fallback behavior. This supports the pattern; it is not a tModLoader guarantee.
- Remnants' [dungeon generator](https://github.com/lazy-wombat/Remnants/blob/main/Content/World/Dungeons.cs) is evidence for combining procedural macro-layout with reusable authored modules. Its implementation and assets remain that project's work.

### Apogean workflow

1. **Plan:** derive deterministic landmark bounds, access route, seed, variant, and padding in `ApogeanWorldPlan`. Persist/network compact intent and results, not a second copy of the tile grid.
2. **Reserve:** reject world-edge, spawn, dungeon/temple, route, and protected-structure conflicts. Call `CanPlace` with the final padded footprint. Use a finite candidate budget.
3. **Stamp:** load a versioned `Content/Structures/Blueprints/*.apstructure`. Let `AuthoredStructureTemplate` validate dimensions/anchors, write the shell, frame it, then call `WorldGen.PlaceObject` for registered objects.
4. **Integrate:** blend terrain, preserve or create a traversable entrance, frame the perimeter, apply liquids/wires/decor in an explicit order, and protect the accepted rectangle plus padding.
5. **Record:** save `placed`, `fallback`, or `skipped` with location, seed, template version, and reason. A skipped landmark must have a progression-safe alternative.
6. **Validate:** prove the intended entrance/route, no out-of-world access, no overwritten anchors, no accidental liquid trap, no overlap, and a useful diagnostic view.

For permanent campuses, generate architecture on day one and let progression activate, open, populate, or rearm it. Do not silently stamp a large late-game structure over player-owned space.

### Failure modes

- **Pass-name drift:** a named vanilla pass moves or disappears across versions. Handle `FindPass == -1`, choose a documented fallback insertion or safe skip, and log the decision.
- **Unbounded search:** a crowded/small world hangs generation. Every search has a candidate count, measurable rejection reasons, and fallback/skip.
- **Protection after the fact:** later passes overwrite the landmark. Reserve first and protect immediately after successful placement.
- **Raw furniture frames:** the object looks right but has invalid anchors/style state. Use `WorldGen.PlaceObject` through the template loader.
- **Boss-death whole-world mutation:** a synchronous conversion stalls the server and is hard to resume. Use bounded server jobs with saved version/seed/cursor and tile synchronization.
- **Template upgrade restamps player edits:** distinguish immutable generation shell, explicitly repairable authored pieces, and player-owned space; migrations must be opt-in or narrowly targeted.
- **Landmark missing but flag advanced:** the campaign is bricked. Progression checks the recorded placement result and exposes a safe alternate interaction/reward path.

## 4. NPCs, bosses, projectiles, and networking

### VERIFIED ENGINE/SOURCE FACTS

- `ModNPC.AI` runs on server and clients. The four `NPC.ai` floats are automatically synchronized; `NPC.localAI` is not. The server is responsible for authoritative NPC changes, and `NPC.netUpdate` requests synchronization when nondeterministic or server-only state changes. [Stable `ModNPC`](https://docs.tmodloader.net/docs/stable/class_mod_n_p_c.html) · [Stable `NPC`](https://docs.tmodloader.net/docs/stable/class_n_p_c.html)
- State outside `NPC.ai` is written by the server in `SendExtraAI` and read by clients in `ReceiveExtraAI`. The payload travels with `SyncNPC`, including creation, join, and `netUpdate` synchronization. [Stable `ModNPC`](https://docs.tmodloader.net/docs/stable/class_mod_n_p_c.html) · [Preview `ModNPC`](https://docs.tmodloader.net/docs/preview/class_mod_n_p_c.html)
- Projectiles expose analogous ExtraAI hooks synchronized with `SyncProjectile`. [Stable `ModProjectile`](https://docs.tmodloader.net/docs/stable/class_mod_projectile.html) · [Preview `ModProjectile`](https://docs.tmodloader.net/docs/preview/class_mod_projectile.html)
- ExampleMod demonstrates a compact custom state machine in [ExampleCustomAISlimeNPC](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/ExampleCustomAISlimeNPC.cs), server-owned attack/projectile decisions plus ExtraAI in [ExampleWorm](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/ExampleWorm.cs), and a body/minion ownership split in the [MinionBoss example](https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/NPCs/MinionBoss).
- Calamity's public [Cryogen source](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/NPCs/Cryogen/Cryogen.cs) is a mature case study in serializing extra phase/teleport/invulnerability/local state and setting `netUpdate` at transitions. It is an implementation example, not an engine contract or code donor.

### Apogean workflow

1. **Write the encounter contract:** spawn/entry condition, arena assumptions, phase states, telegraph, player counterplay, failure/despawn recovery, rewards, and exact world-progress transition.
2. **Minimize authoritative state:** use `NPC.ai` for compact phase/timer/target/seed values; use a versioned ExtraAI payload only for state that does not fit. Derive presentation locally when it is deterministic.
3. **Make transitions explicit:** one authority chooses attack, phase, spawn, hit, reward, and world flag. Set `netUpdate` on authoritative transitions, not every tick.
4. **Separate simulation from presentation:** the server owns outcomes and random choices; clients interpolate and render synchronized telegraphs. A telegraph may be client-drawn, but its start time/target/seed comes from authority.
5. **Separate lifetimes:** encounter instance state dies or resets with the encounter. Permanent unlock state uses the existing `FactionProgression`/world-system save and network pattern.
6. **Test the network matrix:** single player, host-and-play, dedicated server, two clients, late join in every phase, player death/respawn, boss despawn, reconnect, and reward duplication.

### Failure modes

- **Each peer chooses an attack:** visible desync and duplicate spawns. Choose once on the server, synchronize compact state/seed, and derive visuals.
- **ExtraAI pair incomplete:** fields silently differ. Review sender/receiver in one change and test late join.
- **Transition without `netUpdate`:** clients remain in the old phase. Set it exactly when server-owned state changes.
- **`netUpdate` every tick:** bandwidth hides an overgrown state model. Reduce to transition state plus deterministic local simulation/interpolation.
- **Client awards progress/reward:** duplication or bypass. Only the server commits world flags, items, and encounter completion.
- **Transient phase saved as world progress:** reload semantics become corrupt. Persist only durable campaign truth and define how an interrupted encounter resets.

## 5. Progression contracts and commercial-game inspiration

### INFERRED DESIGN INSPIRATION — source-grounded, not engine fact

The following observations come from first-party publisher/developer material or direct developer talks/interviews. The Apogean rules beneath each observation are original translations.

#### Borderlands 3: explicit content and progression contracts

**Source observations**

- 2K's launch material describes Borderlands 3 as FPS action combined with RPG progression and calls out level synchronization across differing levels/mission progress plus instanced loot. That is first-party evidence that co-op eligibility, ownership, and reward behavior are explicit progression concerns. [Take-Two/2K release](https://ir.take2games.com/node/26351/pdf)
- Gearbox developer Andrew Bair's GDC session treats accessibility as something embedded during production and discusses redundant loot and communication cues such as beams, sounds, and subtitles. [GDC: Baked In Accessibility](https://www.gdcvault.com/play/1026602/Baked-In-Accessibility-How-Features)
- In a direct developer interview, Gearbox producer Chris Brock describes establishing vision/rules early, checking ideas against them, investing in tools to enable more content, and using focused cross-functional teams. [Game Developer interview](https://www.gamedeveloper.com/production/q-a-producing-a-bigger-i-borderlands-i-)

No owner-hosted Borderlands 3 quest-design specification was found. Therefore the following contract schema is an Apogean inference, not an attribution to Gearbox:

| Apogean contract field | Question it must answer |
|---|---|
| Availability | Which world/faction state exposes it, and how does the player see that? |
| Entry | Where/how is it accepted or activated? |
| Objective | One concrete player action and target; avoid hidden compound verbs. |
| Spatial proof | Which route, room, prop, enemy, or structure visibly carries the objective? |
| Completion | What server-observable event completes it exactly once? |
| Reward | Item/access/choice; who owns it; what happens with full inventory? |
| Co-op | Late join, mixed progress, per-player versus world ownership, disconnect/rejoin. |
| Failure/retry | Death, despawn, abandonment, duplicate activation, missing landmark. |
| Persistence | Save key/version and what reload preserves or resets. |
| Next action | A visible route, interaction, or decision after resolution. |

Reusable lessons:

1. **Make the state legible twice.** Pair a world/spatial change with UI/dialogue/audio or reward evidence; a private flag is not a player-facing progression system.
2. **Design co-op before content.** Mark every contract field as per-player or per-world and define mixed-progress/late-join behavior before implementation.
3. **Bake access into the fixture.** Objective/reward cues need redundant shape, motion, sound, and text—not color alone—and should be tested while the feature is still a minimal probe.
4. **Use rules to scale content.** A faction contract template and cross-disciplinary vertical slice should precede a large quest/facility batch.
5. **Keep optional work valuable but non-hostage-taking.** Side routes may add character, resources, or alternate access; core campaign recovery must not depend on finding an obscure optional trigger.

Failure modes: silent unlocks, world flags that disagree with individual ownership, rewards that vanish on full inventory/disconnect, inaccessible objectives after a landmark skip, chains with no restart point, and content whose only direction is a long text prompt.

#### Titanfall/Titanfall 2: functional military-industrial language

**Source observations**

- Respawn's first Titanfall announcement defines the universe through contrasts: small versus giant, natural versus industrial, and human versus machine, with identity emerging from the intersection of pilot and Titan play. [EA/Respawn announcement](https://news.ea.com/press-releases/press-releases-details/2013/Respawn-Entertainment-Unveils-Titanfall/default.aspx)
- Respawn designer Christopher Dionne's GDC talk describes rapid, playable action blocks as the method used to discover and develop memorable single-player ideas. [GDC: Designing Unforgettable Titanfall Single Player Levels](https://www.gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player)
- Respawn designer Carlos Pineda's GDC talk notes that long Titan engagements need different reasoning than low-time-to-kill shooter encounters; the accompanying slides emphasize abilities with readable setup, commitment/cost, counterplay, and punish/reversal opportunities. [GDC: Solving Titan-Sized Problems](https://www.gdcvault.com/play/1024056/Solving-Titan-Sized-Problems-Evolving) · [speaker slides](https://media.gdcvault.com/gdc2017/Presentations/Pineda_Carlos_Solving_Titan_Sized.pdf)
- In a direct interview, Titanfall lead artist Joel Emslie connects the visual language to real military hardware, gameplay function, and animation rather than decorative concept alone. [Prima developer interview](https://primagames.com/eguides/titanfall-eguide/behind-the-scenes/art)

Apogean translation:

1. **Design by readable contrasts, not imitation.** Each faction gets original values on mass (light/heavy), geometry (horizontal/vertical), surface finish, maintenance/decay, motion, warning language, and natural/industrial intrusion.
2. **Require function behind every silhouette.** A support reaches a load, conduit reaches a machine, gantry reaches a work area, barricade shapes movement, vent implies flow, and warning treatment marks a real hazard or interaction.
3. **Prototype encounter action blocks.** Before final art, build one-room grayboxes that prove entry, objective, vertical route, retreat/recovery, cover rhythm, hazard, telegraph sightline, and exit. Promote only layouts that create a distinct decision.
4. **Specify every boss ability as a combat contract:** tell, target rule, commitment window, movement/position cost, player counterplay, punish/reversal opportunity, aftermath, and phase purpose.
5. **Keep faction readability redundant:** silhouette + material rhythm + motion/telegraph + sound. Palette alone is insufficient.

The existing faction language can implement those rules without borrowing another game's look: Kessler remains dense industrial gunmetal/burnt red/signal orange; Helix remains sterile white/gray/green; Sentrix remains black/cyan, precise, and vertical.

Failure modes: purposeless surface greebles, impossible supports/circulation, every faction sharing the same gray box, color-only team reads, arenas with no recovery route, instant high-impact attacks without a tell/commitment, and “inspired” spaces that reproduce a recognizable prop, layout, faction, or encounter.

## Required pre-merge gates

- Build against the declared stable target; document preview experiments separately.
- Regenerate the same seed and compare planned landmark records, variants, placement result, and rejection diagnostics.
- Exercise the relevant gallery and capture before/after evidence for art, framing, and background work.
- Test reload and dedicated-server late join for every new saved/synchronized state.
- Test multi-tile placement by player and blueprint, including every alternate, anchor, break, and re-place path.
- Force placement failure and verify bounded fallback/skip plus a progression-safe result.
- Test biome/depth transitions and capture for backgrounds; verify dedicated-server safety.
- Test every boss/NPC transition with two clients and late join; verify server-owned progress and rewards.
- Verify commercial inspiration remains abstract and all final names, assets, layouts, objectives, dialogue, and encounters are original.

## Source ledger

### tModLoader API and ExampleMod — primary

- [Stable API](https://docs.tmodloader.net/docs/stable/index.html) · [Preview API](https://docs.tmodloader.net/docs/preview/index.html) · [ExampleMod branch warning](https://github.com/tModLoader/tModLoader/blob/1.4.5/ExampleMod/README.md)
- Trees: [`ModTree` stable](https://docs.tmodloader.net/docs/stable/class_mod_tree.html), [preview](https://docs.tmodloader.net/docs/preview/class_mod_tree.html), [ExampleTree](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Plants/ExampleTree.cs), [ExampleSapling](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Plants/ExampleSapling.cs)
- Tiles/walls/furniture: [`TileObjectData` stable](https://docs.tmodloader.net/docs/stable/class_tile_object_data.html), [preview](https://docs.tmodloader.net/docs/preview/class_tile_object_data.html), [`ModTile`](https://docs.tmodloader.net/docs/stable/class_mod_tile.html), [`ModWall`](https://docs.tmodloader.net/docs/stable/class_mod_wall.html), [ExampleChair](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Furniture/ExampleChair.cs), [showcase](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/TileObjectDataShowcase.cs), [custom framing](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/ExampleCustomFramingTile.cs)
- Backgrounds/skies: [`ModSceneEffect`](https://docs.tmodloader.net/docs/stable/class_mod_scene_effect.html), [surface style](https://docs.tmodloader.net/docs/stable/class_mod_surface_background_style.html), [underground style](https://docs.tmodloader.net/docs/stable/class_mod_underground_background_style.html), [`CustomSky`](https://docs.tmodloader.net/docs/stable/class_custom_sky.html), [ExampleSurfaceBiome](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Biomes/ExampleSurfaceBiome.cs), [surface example](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs), [underground example](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs)
- Structures/worldgen: [`ModSystem` stable](https://docs.tmodloader.net/docs/stable/class_mod_system.html), [preview](https://docs.tmodloader.net/docs/preview/class_mod_system.html), [`StructureMap` stable](https://docs.tmodloader.net/docs/stable/class_structure_map.html), [preview](https://docs.tmodloader.net/docs/preview/class_structure_map.html), [`WorldGen`](https://docs.tmodloader.net/docs/stable/class_world_gen.html), [ExampleOre](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/ExampleOre.cs)
- NPC/projectile/networking: [`ModNPC` stable](https://docs.tmodloader.net/docs/stable/class_mod_n_p_c.html), [preview](https://docs.tmodloader.net/docs/preview/class_mod_n_p_c.html), [`NPC`](https://docs.tmodloader.net/docs/stable/class_n_p_c.html), [`ModProjectile`](https://docs.tmodloader.net/docs/stable/class_mod_projectile.html), [`NetMessage`](https://docs.tmodloader.net/docs/stable/class_net_message.html), [custom-AI example](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/ExampleCustomAISlimeNPC.cs), [worm example](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/ExampleWorm.cs), [minion-boss example](https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/NPCs/MinionBoss)

### Mature public mod source — case studies

- Calamity: [repository](https://github.com/CalamityTeam/CalamityModPublic), [license](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/LICENSE.md), [worldgen orchestration](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Systems/World/WorldgenManagementSystem.cs), [structure placement](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/World/DraedonStructures.cs), [Cryogen networking](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/NPCs/Cryogen/Cryogen.cs)
- Remnants: [repository](https://github.com/lazy-wombat/Remnants), [dungeon generator](https://github.com/lazy-wombat/Remnants/blob/main/Content/World/Dungeons.cs)

### First-party/developer creative sources

- Borderlands 3: [2K/Take-Two launch material](https://ir.take2games.com/node/26351/pdf), [Gearbox GDC accessibility talk](https://www.gdcvault.com/play/1026602/Baked-In-Accessibility-How-Features), [direct Gearbox producer interview](https://www.gamedeveloper.com/production/q-a-producing-a-bigger-i-borderlands-i-)
- Titanfall/Titanfall 2: [EA/Respawn announcement](https://news.ea.com/press-releases/press-releases-details/2013/Respawn-Entertainment-Unveils-Titanfall/default.aspx), [Respawn action-block GDC talk](https://www.gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player), [Respawn Titan-combat GDC talk](https://www.gdcvault.com/play/1024056/Solving-Titan-Sized-Problems-Evolving), [speaker slides](https://media.gdcvault.com/gdc2017/Presentations/Pineda_Carlos_Solving_Titan_Sized.pdf), [direct lead-artist interview](https://primagames.com/eguides/titanfall-eguide/behind-the-scenes/art)

### Repository anchors

- `Content/Tiles/DeadForestTree.cs` and `Content/Tiles/DeadForestSapling.cs`
- `Content/Tiles/CorporateFurnitureTiles.cs` and `Content/Structures/AuthoredStructureTemplate.cs`
- `Common/WorldGeneration/ApogeanWorldGenerationSystem.cs` and `Common/WorldGeneration/ApogeanWorldPlanSystem.cs`
- `Content/Backgrounds/*`, `Content/NPCs/*`, and `Content/Diagnostics/*`
