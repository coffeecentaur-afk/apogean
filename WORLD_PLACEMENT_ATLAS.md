# Apogee Wastes World Placement Atlas

Status: Accepted environmental-foundation contract

Accepted: 2026-09-01

This document converts the binding direction in `DESIGN_BIBLE.md` into a build-ready spatial contract for standard large worlds. It defines what must exist, where each landmark may compete for space, what may be reduced or omitted, and what world-generation validation must prove.

## 1. Supported world contract

- Complete Apogee campaign generation initially supports standard large worlds (`8400 × 2400` tiles).
- Medium-world compact layouts are a later compatibility target, not an Environmental Alpha requirement.
- Small worlds do not generate a silently incomplete campaign.
- Secret-seed support remains part of the separate validation decision. This atlas assumes standard layer ordering.
- Existing worlds never receive automatic landmark retrofits. A new Apogee world is required.

The large-world requirement is deliberate: Apogee must preserve Terraria's landmarks and leave enough unclaimed terrain for building while adding one planetary scar, three permanent Corporate Campuses, five guaranteed ruins, and compatibility exclusions for major content mods.

## 2. Placement classes

### Critical landmarks

Every supported world must contain:

1. Spawn Sanctuary.
2. One primary Maw Rupture in either full or compact form.
3. Kessler Corporate Campus.
4. Helix Corporate Campus.
5. Sentrix Corporate Campus.

If the joint planner cannot place every critical landmark after trying authored compact variants, generation stops with a clear diagnostic. A critical landmark is never silently skipped.

### Guaranteed secondary landmarks

Every supported world must contain:

1. One Maw Outgrowth.
2. One abandoned Kessler outpost.
3. One abandoned Helix laboratory.
4. One crashed Sentrix relay or probe site.
5. One neutral pre-war settlement or transit ruin.
6. One independent Maw research site.

### Optional landmarks

When uncontested space remains, the planner may add:

- a second Maw Outgrowth;
- two to five small road, mine, bunker, laboratory, relay, or settlement fragments;
- purely cosmetic surface debris that does not reserve a structure envelope.

Optional content yields before Terraria, Calamity, another mod's registered structures, and player building space.

## 3. Hard exclusions

Critical Apogee landmarks may not intersect:

- the Spawn Sanctuary;
- either ocean envelope;
- the Dungeon and its protected approach;
- the Jungle Temple and its protected approach;
- the full primary Jungle macro-region;
- the full Underground Desert macro-region;
- Shimmer/Aether;
- any existing `StructureMap` reservation;
- any detected container, important framed object, or protected foreign wall inside the proposed physical footprint;
- known large third-party biome or structure envelopes.

When Calamity is loaded, the Dungeon-side ocean and Abyss column are hard exclusions. The Underground Desert exclusion also protects the Sunken Sea. Apogee may add explicit adapters for other known mods, but never assumes that an unknown generator cooperates with `StructureMap`.

The initial Feeding Wound also avoids the primary Corruption or Crimson. Runtime Maw spread may later contest world evil terrain.

## 4. Joint placement solver

Critical landmarks are solved as one atlas rather than placed greedily.

1. Read final spawn, world layers, vanilla macro-biome bounds, Dungeon side, known protected structures, and loaded compatibility profiles.
2. Generate bounded candidate sets for the Maw, Kessler, Helix, and Sentrix.
3. Reject every candidate that violates a hard exclusion.
4. Score combinations rather than individual points.
5. Select the highest-scoring deterministic combination for the world seed.
6. Register physical envelopes plus generation padding in both Apogee's registry and `GenVars.structures` before cooperating detail/structure passes.
7. Save the selected atlas and its stable hash in world data.

Soft preferences never override safety:

- The Feeding Wound prefers neutral Wastes approximately 900–1400 tiles from spawn and is never closer than 600 tiles.
- Kessler prefers neutral Wastes approximately 700–1300 tiles from spawn on the side opposite the Feeding Wound.
- Helix prefers the Maw side, with its main Campus outside the initial spread buffer and its observation bore pointing toward the Rupture.
- Sentrix prefers the safest remaining upper-sky band, away from spawn, vanilla floating islands, and other structures.
- Horizontal left/right assignments vary by seed.

If no full combination exists, the solver tries compact Maw and Campus envelopes. It does not start deleting content to force a preferred arrangement.

## 5. Maw spatial contract

### Full Rupture

| Feature | Physical target |
| --- | --- |
| Initial surface Maw | 340–440 tiles wide |
| Feeding Wound opening | 70–100 tiles wide |
| Ordinary Gullet clearance | 20–30 tiles |
| Expanded bends | 35–50 tiles |
| Side chambers | 50–90 tiles wide |
| Maximum uninterrupted vertical fall | 40–55 tiles |
| Burning Root cavity | 180–240 wide, 90–130 high |

The route is a saved curved navigation spine with ledges, shelves, direction changes, shell bounds, side-chamber anchors, and optional digestive-basin envelopes. Decorative teeth, bones, glands, loose terrain, and later ore placement never own the route.

The Burning Root penetrates a localized part of the Underworld. Its Matriarch cavity is a broad natural hollow with no generated platforms and no mandatory repair objective. Players prepare it with ordinary Terraria building tools.

### Compact Rupture

The compact variant preserves:

- a readable Feeding Wound;
- one connected Gullet route;
- the major depth gates;
- required Brood Nests;
- the Burning Root Matriarch cavity at the Underworld ceiling.

It removes side chambers first, reduces shell and surface width second, and reduces the amount of Root extending into Hell third. It never removes the progression route merely to report generation success.

### Outgrowths

- One Outgrowth is guaranteed; a second is opportunistic.
- Each target footprint is 60–90 tiles wide and 30–50 tiles deep.
- An Outgrowth may contain enemies and one Maw Node.
- An Outgrowth never contains progression-required Brood Nests and never creates another Gullet.
- Future medium-world support permits at most one Outgrowth; unsupported small worlds receive none.

### Digestive basins

The terrain plan may reserve shallow basin geometry, but Environmental Alpha places no fake solid acid tiles. A later experiment may fill selected basins with real water and an isolated amber presentation/hazard predicate. If unrelated Maw water cannot remain visually and mechanically ordinary, the experiment is rejected and the basins remain dry or use ordinary water.

## 6. Corporate Campus envelopes

Dimensions describe maximum physical envelopes. They include courtyards, air gaps, terrain, and projecting platforms rather than solid tile rectangles.

| Campus | Full envelope | Compact floor | Primary layer | Generation padding |
| --- | ---: | ---: | --- | ---: |
| Kessler | 260 × 120 | 208 × 96 | Surface Wastes | 40 tiles |
| Helix | 240 × 230 | 192 × 184 | Surface and underground | 40 tiles |
| Sentrix | 220 × 200 | 176 × 160 | Upper sky | 32 tiles |

Compact layouts retain public frontage, progression route, reusable arena, command space, and every required NPC anchor. Optional rooms and redundant routes are removed first.

### Kessler Armaments

- Concrete military perimeter, guard towers, gate, courtyard, headquarters, manufacturing/security wing, reusable arena, and landing zone.
- Crossing the perimeter gate enters Kessler Corporate Territory.
- The public forecourt and Quartermaster service frontage are reachable before the main building opens.
- Kessler and the Feeding Wound prefer opposite sides of spawn.

### Helix Genomics

- A cracked surface biodome and reception frontage sit above the larger underground laboratory.
- No military perimeter wall; containment glass, decontamination doors, and sealed tissue infrastructure establish its boundary.
- A protected observation bore points toward the Maw but terminates 40–80 tiles short behind containment bulkheads.
- The main Campus prefers a safe distance outside the Maw's initial footprint; the bore supplies the visual relationship.

### Sentrix Watch

- A precise vertical floating spire with multiple landing-pad projections, maintenance ledges, public lobby, security/analysis wings, reusable arena, and command core.
- Players may reach exterior pads with early mobility, but all entrances remain sealed until progression opens them.
- Arrival activates a ground transit beacon for reliable access.
- Exterior caches may contain Gravitation/Featherfall Potions, rope, wire, recall supplies, ammunition, Sentrix scrap, lore, cosmetics, and one modest exploration sidegrade.
- Exterior caches never contain core Sentrix weapons, armor, progression keys, or irreplaceable items.

## 7. Campus access states

| State | Accessible | Protected |
| --- | --- | --- |
| Day-one sealed | Exterior approach, public frontage, approved caches | Doors, interior, wiring, machinery, arena, command wing |
| Arrival complete | Quest/shop lobby and communication NPC | Specialist, arena, manufacturing, executive, command wings |
| Clearance earned | Testing arena and approved specialist wings | Executive and command areas |
| Company war | Raid route, security rooms, second-in-command branch, arena/command route | Structural tiles remain indestructible until CEO defeat |
| CEO defeated | Entire site may be entered, salvaged, mined, or dismantled | No permanent Apogee no-build shell remains |

Before CEO defeat, protected Campus tiles resist ordinary mining, explosions, block swap, actuation, and biome conversion. A narrow Kessler security apron and Helix decontamination strip resist conversion without creating large purity zones. Removing the Campus after CEO defeat removes that resistance.

Apogee exposes a protected-region compatibility query for highway and world-edit mods. It cannot guarantee protection against another mod that directly rewrites tile memory while deliberately bypassing tModLoader hooks.

## 8. Corporate combat reuse

Each Campus contains one major reconfigurable combat arena rather than separate empty rooms for every encounter.

- The corporation's progression test uses the arena first.
- The company war changes traps, doors, and hazards around the same space.
- A short persistent-security route leads to the second-in-command branch and the CEO.
- Cleared security rooms do not endlessly respawn ordinary defenders.
- Dialogue may bypass the second-in-command fight.
- The first-clear raid target is five to eight minutes total: one to two minutes of traversal, zero to two minutes for the second-in-command branch, and approximately three-and-a-half to five minutes for the CEO.
- CEO defeat globally unlocks the Campus for dismantling because the company-war result is a world state.

## 9. Ruin inventory

Guaranteed foreground ruins use small authored envelopes and yield to all critical landmarks:

| Ruin | Target envelope | Placement identity |
| --- | ---: | --- |
| Abandoned Kessler outpost | 60–90 × 35–55 | Surface Wastes or ordinary underground edge |
| Abandoned Helix laboratory | 70–100 × 55–85 | Ordinary underground/cavern outside the Maw |
| Crashed Sentrix relay | 55–85 × 35–60 | Surface Wastes with clear sky exposure |
| Pre-war settlement/transit ruin | 90–140 × 45–70 | Surface Wastes away from spawn sanctuary |
| Independent Maw research site | 70–110 × 55–90 | Outside the Maw with a safe observation relationship |

Each corporation's guaranteed abandoned site may carry an early sidegrade weapon route. Active Campus frontage is limited to lore, consumables, scrap, cosmetics, and modest utility.

Two to five optional fragments use 24–60 tile envelopes and may be skipped individually. Environmental backgrounds carry most of the civilization density so the playable world remains open for building.

## 10. Arrival foreshadowing

The world contains every Campus before its corporation arrives. Progression uses an Orbital Omen instead of generating a new building:

1. The corporation's prerequisite is completed.
2. A distant craft or signal appears in the upper-sky presentation.
3. Radio noise, lights, and a world message provide at least one full in-game day of warning.
4. The existing Campus changes to its arrival state.
5. Deliberate interaction with its landing beacon begins the invasion.

Orbital headquarters and star-chart destinations are separate later worlds. Only the Sentrix spire and temporary omen consume primary-world sky space.

## 11. Generation passes

The implementation is split into explicit phases:

1. **Macro survey:** collect final spawn, layer boundaries, vanilla macro-biomes, loaded compatibility profiles, and existing shared reservations.
2. **Atlas solve:** generate and jointly score critical candidate combinations.
3. **Early reservation:** register the chosen Maw spine/chambers and Campus/ruin envelopes.
4. **Maw terrain:** construct shell, route, chambers, walls, teeth, glands, and dry basin geometry from the saved plan.
5. **Authored structures:** place Campuses and guaranteed ruins from modules inside their reservations.
6. **Optional details:** attempt second Outgrowth and optional ruins without displacing protected content.
7. **Finalization:** validate the route, frame structures, place approved liquids, and allow normal liquid settlement.
8. **Post-world validation:** emit a deterministic report and reject invalid campaign worlds.

No phase receives permission to erase an unknown container, framed structure, protected wall, Dungeon, Temple, hive, living tree, or third-party landmark merely because it intersects a preferred candidate.

## 12. Acceptance gates

A generated campaign world passes only when all of the following are true:

- world size and layer model are supported;
- the saved plan has a stable deterministic hash for the same mod set and seed;
- all five critical landmarks exist exactly once;
- all guaranteed secondary landmarks exist exactly once, except the optional second Outgrowth;
- no hard-exclusion intersection exists;
- a 2×3 player-clearance flood fill connects the Feeding Wound to the Matriarch cavity;
- route decorations never close the saved navigation spine;
- no required vertical fall exceeds the approved envelope;
- the full or compact Rupture classification is recorded;
- every Campus public frontage is reachable without entering a sealed interior;
- every Campus progression route and reusable arena fit inside its physical envelope;
- no legacy fake acid tiles remain in normal world generation;
- no protected foreign container or frame-important object was deleted;
- the Spawn Sanctuary remains free of Maw, Outgrowth, and Campus edits;
- ordinary Wastes terrain and enough unreserved surface remain available for player building;
- vanilla-only and Calamity compatibility profiles complete without critical omission.

`RESEARCH_REMNANTS_WORLDGEN_ARCHITECTURE.md` explains why this atlas uses early ownership and reservations without adopting Remnants' total world-generation replacement model.
