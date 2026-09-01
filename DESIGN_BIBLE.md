# Apogee Wastes — Design Bible

This document is the binding creative and progression reference for the mod.  A feature that conflicts with it needs an explicit design decision before it is added.

## Pillars

1. **Terraria remains Terraria.** The mod expands the adventure rather than replacing its familiar exploration, building, classes, and boss progression.
2. **A ruined corporate frontier.** The player is a capable lone rider in a poisoned colony world, useful enough for corporations to recruit and dangerous enough for them to fear.
3. **Every threat tells a story.** Mutants are not generic monsters; corporate hardware, biology, and place make their mechanics legible.
4. **Fair pressure, not number inflation.** Encounters are dangerous because they create choices, movement problems, and readable hazards—not because they erase builds with giant health pools.
5. **Content remains obtainable.** Politics change pacing, dialogue, prices, and how gear is acquired. They never quietly delete a whole class's rewards.

## Visual Bible

| Domain | Palette and silhouette | Do not use |
| --- | --- | --- |
| The Engraft / Broodmass | charcoal-black organic mass, sickly ochre and amber cords, dry stringy roots, pale bone, low predatory silhouettes; wet flesh is a restrained accent | Corruption-purple as the dominant read; generic neon-green slime; Crimson-like fields of exposed red meat |
| Kessler Armaments | gunmetal, burnt red, signal orange, dense industrial geometry | clean sci-fi white, organic shapes |
| Helix Genomics | sterile white, muted surgical gray, small toxic-green clinical indicators | warm wilderness growth as its default identity |
| Sentrix Watch | black, cold cyan, blue-white scanning light, precise vertical forms | ragged improvised machinery |

All sprite work uses hard opaque pixel clusters, limited palettes, readable native-scale silhouettes, and no soft anti-aliasing. The generated reference sheet is retained under `Art/Reference/` as inspiration only; it is not a game asset.

### Environmental history

The apocalypse is the world's baseline, not a single optional biome.

- The ordinary forest is mechanically safe but visually exhausted: brown grass, thinned plants, dead trees, broken roads, substations, and settlement remains.
- Desert panoramas preserve collapsed highways, freight routes, and buried logistics works.
- Jungle panoramas preserve failed Helix research stations and containment infrastructure.
- Snow panoramas preserve frozen relays, pipelines, and remote industrial sites.
- Ocean panoramas preserve drowned ports, wrecks, and failed evacuation infrastructure.
- Corruption, Crimson, and Hallow remain immediately recognizable Terraria biomes. Their backgrounds show each force consuming or transforming the same ruined civilization instead of being replaced by the Engraft.
- Concentrated Engraft regions are a second layer of danger: active, biological, ochre-and-charcoal territory growing through the already-dead world.

Each surface biome has at least two authored background compositions. Every composition is a matched transparent far/middle/close parallax set over Terraria's native sky—not a baked panorama. A world's seed selects one composition per biome and that choice remains stable. Terraria's sky and lighting move the same composition through day, night, and solar eclipse so landmarks never jump when time changes. Underground scenery follows tModLoader's separate four-texture transition/ground/rock contract. A later player-facing projector may deliberately cycle a biome's composition; random runtime cycling is forbidden.

## The Engraft

The Engraft is a distributed Broodmass ecosystem, not a recolored Corruption.

- **Maw Nodes** are visible, destructible growth sources. They thicken local contamination and enemy activity.
- **Maw Ruptures** are large, persistent collapsed hollows where players can build their own boss arenas.
- **The Deep Maw** is the later, endgame hive domain.
- New worlds receive one major rupture and smaller outgrowths away from spawn. Ruptures penetrate natural surface and underground terrain rather than repainting only the first soil row. Growth is slow before Hardmode and bounded thereafter.
- It consumes natural terrain only. Player structures, chests, housing, and protected sites are never conversion targets.
- Ordinary Engraft turf does not glow. Amber cysts, Maw Nodes, active organs, and other explicit energy-bearing growths may emit amber light.
- Corruption and Crimson are consumed at a frontier; Hallow pushes back and slows Engraft growth.
- The Engraft is frightening through pounces, larvae, tethers, burrows, and overlapping terrain pressure—not unreasonably large stats.

## Act 1 Progression

1. Explore the Engraft and recover low-tier abandoned corporate salvage.
	- Rend Hook: a charged short lunge with a dangerous commitment window, not a conventional sword.
	- Amber Siphon: a sustained umbilical magic tether with deliberately slow life recovery.
	- Sinew Bow: a familiar ranged anchor so every early item is not a gimmick.
	- Maw Effigy: a mobile hunting sentry for the early summoner branch.
2. Optional Alpha Hunt: a camouflaged hound/reptile apex predator stalks the player, then retreats to a marked Maw Rupture for its final stand.
3. Defeat the required Nest Warden and craft the class-aware Engraft Harness family.
4. Defeat `MATRIARCH-7A-1`, a Helix-labelled regional growth node, in the Engraft. Her visible plate is a critical window; her brood makes a large, killable regeneration ring.
5. Defeat Wall of Flesh. Kessler's impact is announced immediately; its live-fire assessment arrives at the next dawn.
6. Clear Kessler's first invasion, open the Quartermaster's compound, and gain the first corporate dialogue/shop/scrip loop.
7. Complete a pre-mechanical Kessler walkframe contract at a damaged, repairable proving ground.

## Boss Rules

- Broodmass bosses are summoned only in the Engraft and enrage outside it. They do not permanently convert player terrain during a fight.
- Corporate fights happen in authored but player-reusable arenas. Repairs are optional quality-of-life/arena improvements, never requirements for a fair fight.
- Core progression material and one class-appropriate reward are guaranteed. Expert bags supplement, not replace, the core reward. Master rewards are visual prestige.
- Each eligible multiplayer player receives their own boss rewards.

## Factions and Politics

The three corporations are visible from day one through sealed landmarks. Their arrival changes the world in stages: Kessler after Wall of Flesh, Helix after all mechanical bosses, Sentrix after Plantera. The post-Moon-Lord company war is a world-level vote; individual standing and temporary trespass remain per player.

The faceless galactic government treats the CEOs as quota-bound colonial houses. A failed company is erased, stripped, and sold to its rivals. The player is the destabilizing factor.

## Future Boundary

The star chart, mobile ship, company war, CEO routes, post-Moon-Lord Deep Maw, and procedural completion content are deliberately roadmap items. Act 1 establishes their vocabulary without pretending to ship them early.
