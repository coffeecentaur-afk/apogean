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
| The Maw / Broodmass | charcoal-black organic mass, sickly ochre and amber cords, dry stringy roots, pale bone, low predatory silhouettes; wet flesh is a restrained accent | Corruption-purple as the dominant read; generic neon-green slime; Crimson-like fields of exposed red meat |
| Kessler Armaments | gunmetal, burnt red, signal orange, dense industrial geometry | clean sci-fi white, organic shapes |
| Helix Genomics | sterile white, muted surgical gray, small toxic-green clinical indicators | warm wilderness growth as its default identity |
| Sentrix Watch | black, cold cyan, blue-white scanning light, precise vertical forms | ragged improvised machinery |

All sprite work uses hard opaque pixel clusters, limited palettes, readable native-scale silhouettes, and no soft anti-aliasing. The generated reference sheet is retained under `Art/Reference/` as inspiration only; it is not a game asset.

### Environmental history

The apocalypse is the world's baseline, not a single optional biome.

- The Wastes replace the ordinary starting forest as a neutral, non-spreading biome: dead terrain and trees, broken roads, substations, and settlement remains. They do not count as world evil.
- Desert panoramas preserve collapsed highways, freight routes, and buried logistics works.
- Jungle panoramas preserve failed Helix research stations and containment infrastructure.
- Snow panoramas preserve frozen relays, pipelines, and remote industrial sites.
- Ocean panoramas preserve drowned ports, wrecks, and failed evacuation infrastructure.
- Corruption, Crimson, and Hallow remain immediately recognizable Terraria biomes. Their backgrounds show each force consuming or transforming the same ruined civilization instead of being replaced by the Engraft.
- The Maw is a second layer of danger: active, biological, ochre-and-charcoal territory growing through the already-dead Wastes.

Each surface biome has at least two authored background compositions. Every composition is a matched transparent far/middle/close parallax set over Terraria's native sky—not a baked panorama. A world's seed selects one composition per biome and that choice remains stable. Terraria's sky and lighting move the same composition through day, night, and solar eclipse so landmarks never jump when time changes. Underground scenery follows tModLoader's separate four-texture transition/ground/rock contract. A later player-facing projector may deliberately cycle a biome's composition; random runtime cycling is forbidden.

### Ruined forest ecology

The Wastes must remain a complete Terraria building biome rather than a set dressing pass.

- Naturally generated forest grass is converted to separate dead grass. Vanilla green grass, grass walls, flower walls, seeds, and related restoration tools remain obtainable and placeable.
- Dead forest trees are real trees: they can be chopped, shaken, planted with acorns, regrown, painted, and harvested for ordinary Wood. Their leafless canopy does not emit peaceful falling leaves.
- Living Trees retain their wood, roots, rooms, doors, chests, and traversal. World generation removes or replaces their green leaf canopy rather than deleting the structure.
- Naturally generated unsafe grass and flower walls receive ruined variants. Player-built safe walls and restored green terrain are never globally rewritten after world generation.
- Spawn remains mechanically safe even though its palette and ecology communicate a dead world.

### Underground and Underworld scenery

Underground backgrounds are routed by both biome and depth. Wastes, desert, snow, jungle, Glowing Mushroom, Dungeon, evil biomes, Hallow, the Maw, and the Underworld never silently fall back to one generic cave set. Each authored set may include ruined mining camps, rails, shelters, research remains, or military infrastructure appropriate to that biome. Until a dedicated ruined set exists, preserving the recognizable vanilla background is preferable to applying the wrong Apogee background.

Surface compositions may crossfade through their parallax layers. Underground backgrounds use hard texture-set selection, so visible borders require authored transition bands, neutral seam textures, or bounded biome hysteresis rather than random switching.

## The Maw

The Maw is the hostile biome created by the distributed Broodmass organism, not a recolored Corruption. The neutral Wastes beneath the rest of the world are a separate biome and do not spread.

- **Maw Nodes** are visible, destructible growth sources. They thicken local contamination and enemy activity.
- The Maw has extremely slow intrinsic frontier growth even without Nodes. Nodes are feeding and amplification organs: each greatly accelerates local spread, enemy density, nest production, and mutation pressure, but destroying every local Node never kills the biome.
- Before the Nest Warden falls, a struck Node's sheath may reveal its amber inner organ but immediately seals without showing misleading normal damage. The Warden's cauterization component makes Nodes genuinely destructible.
- A destroyed Node retracts its visible cords and loose growth in a bounded implosion, then condenses into a local mineable ore core. The innermost roughly 18–24 tile region sterilizes into neutral Wastes while the larger Maw remains; local spread and spawn pressure return to their slow baseline.
- The Nest Warden's **Cautery Brand** ruptures a Node's protective sheath. The collapsed organ leaves **Ossamber**, amber-yellow mineralized Broodmass tissue threaded through an ivory skeletal lattice. Exposed Ossamber can be mined with approximately Platinum-tier pickaxe power or ordinary explosives.
- A major Node leaves roughly 45–60 Ossamber shards; a minor outgrowth leaves roughly 12–20. The material condenses only at the destroyed Node rather than spraying random ore through the world.
- Raw Ossamber occupies the Demonite/Crimtane-to-Necro progression band. MATRIARCH-7A-1 Mutagen Cells stabilize selected Ossamber recipes at approximately Hellstone strength, never beyond the vanilla pre-Wall-of-Flesh ceiling.
- After the first Nest Warden victory, a craftable repeat summon makes Ossamber renewable. Node geodes remain the more efficient first-clear reward, but finite world deposits can never permanently starve multiplayer or late-joining characters.
- Raw Ossamber supports a Necro-tier ranger armor alternative, tools, a grapple, and introductory melee and ranged weapons. Matriarch-catalyzed Brood equipment uses shared body and leg pieces with separate mage and summoner helmets and includes one appropriate weapon route for every class. Melee retains Molten armor as its conventional pre-Wall-of-Flesh armor ceiling.
- Ossamber is Broodmass matter that corporations may study or exploit; it is not one of the corporations' later faction-specific Hardmode ores. The Nest Warden reserves a true optional one-percent chase drop, but no rare drop is required for progression.
- **Brood Nests** are separate reproductive structures. Destroying three awakens the Nest Warden; they do not control biome spread.
- **Maw Ruptures** are large, persistent collapsed hollows where players can build their own boss arenas.
- **The Deep Maw** is the later, endgame hive domain.
- New worlds receive one major rupture and smaller outgrowths away from spawn. The major rupture is an authored vertical scar that penetrates natural surface, underground, cavern, and Underworld terrain rather than repainting only the first soil row. Its terrain, walls, hazards, and scenery change with depth while remaining one continuous landmark. Growth is slow before Hardmode and bounded thereafter.
- The major Rupture uses a **Feeding Wound** grammar: one readable, winding central gullet surrounded by irregular side chambers, braided passages, and pale bone-supported loops. It is neither a straight Corruption chasm nor a field of round Crimson cavities.
- Its depth language progresses from an asymmetric surface mouth, through tendon bridges and amber glands, into broad ossuary chambers and hardened pressure channels, then terminates in a localized **Burning Root** region of the Underworld.
- The surface mouth is implied by geology and composition rather than drawn as literal lips: bone stakes, leaning ruins, cracked terrain, and inward-pointing roots form the gullet silhouette.
- The natural route is traversable with ordinary Terraria ropes, hooks, platforms, and mobility. Frayed surface growth remains approachable, while hardened Mawstone requires approximately Platinum-tier pickaxe power or explosives. Bombs may break ordinary Maw terrain and exposed Ossamber geodes but never Nodes, Brood Nests, sheathed ore cores, or explicit progression membranes.
- Pale bones form arches, stakes, bridges, and structural ribs. Static structural bone is safe terrain; only clearly animated barbs, snapping ribs, and projectile-launching spines deal contact damage.
- Amber glands create strong pools of yellow navigation light separated by genuinely dark passages. Ordinary Maw turf and bone do not glow.
- Bounded authored chambers may contain luminous amber digestive acid. Immersion damages players and non-Maw creatures, leaves a short Corroded debuff, does not destroy dropped items, and does not harm Maw-native enemies. Dormancy dims and stills it but never makes it chemically safe. Its exact engine representation must remain localized and multiplayer-safe.
- **Gullet**, **Ossuary Chambers**, and **Burning Root** are development and lore terms inside the player-facing Maw biome. They become formal map sub-biomes only if later content gives them distinct music, enemies, loot, or mechanics.
- Where the major rupture enters the Underworld, it creates a distinct Maw-Underworld sub-biome rather than globally replacing Hell. This terminus is reserved for a deliberate progression encounter; assigning an existing boss to it requires a separate progression decision.
- It consumes natural terrain only. Player structures, chests, housing, and protected sites are never conversion targets.
- Ordinary Maw turf does not glow. Amber glands, Maw Nodes, active organs, and other explicit energy-bearing growths may emit amber light.
- MATRIARCH-7A-1's defeat forces the network into visible dormancy: amber lighting dims and biological motion and spread fall to their minimum. Wall of Flesh and Hardmode awaken the network again; dormancy never purifies existing Maw terrain.
- The Wall of Flesh is provisionally understood as an ancient planetary immune barrier partially infected by the Broodmass. It remains recognizably the classic horizontal Underworld guardian; its later resprite and attack redesign must preserve that identity while explaining the Maw's Hardmode reawakening.
- Corruption and Crimson are consumed at a frontier; Hallow pushes back and slows Maw growth.
- The Maw is frightening through pounces, larvae, tethers, burrows, and overlapping terrain pressure—not unreasonably large stats.

## Act 1 Progression

1. Explore the Maw and recover low-tier abandoned corporate salvage.
	- Rend Hook: a charged short lunge with a dangerous commitment window, not a conventional sword.
	- Amber Siphon: a sustained umbilical magic tether with deliberately slow life recovery.
	- Sinew Bow: a familiar ranged anchor so every early item is not a gimmick.
	- Maw Effigy: a mobile hunting sentry for the early summoner branch.
2. Optional Alpha Hunt: a camouflaged hound/reptile apex predator stalks the player, then retreats to a marked Maw Rupture for its final stand.
3. Defeat the required Nest Warden, recover the Cautery Brand, collapse Maw Nodes into local Ossamber geodes, and unlock raw Ossamber utility equipment and the ranger armor route.
4. Defeat `MATRIARCH-7A-1`, a Helix-labelled regional growth node, in the Maw. Her visible plate is a critical window; her brood makes a large, killable regeneration ring. Her Mutagen Cells stabilize the mage/summoner Brood Harness family and the final Maw weapon route for every class.
5. Defeat Wall of Flesh. Kessler's impact is announced immediately; its live-fire assessment arrives at the next dawn.
6. Clear Kessler's first invasion, open the Quartermaster's compound, and gain the first corporate dialogue/shop/scrip loop.
7. Complete a pre-mechanical Kessler walkframe contract at a damaged, repairable proving ground.

## Boss Rules

- Broodmass bosses are summoned only in the Maw and enrage outside it. They do not permanently convert player terrain during a fight.
- Corporate fights happen in authored but player-reusable arenas. Repairs are optional quality-of-life/arena improvements, never requirements for a fair fight.
- Core progression material and one class-appropriate reward are guaranteed. Expert bags supplement, not replace, the core reward. Master rewards are visual prestige.
- Each eligible multiplayer player receives their own boss rewards.

## Factions and Politics

The three corporations are visible from day one through sealed landmarks. Their arrival changes the world in stages: Kessler after Wall of Flesh, Helix after all mechanical bosses, Sentrix after Plantera. The post-Moon-Lord company war is a world-level vote; individual standing and temporary trespass remain per player.

The faceless galactic government treats the CEOs as quota-bound colonial houses. A failed company is erased, stripped, and sold to its rivals. The player is the destabilizing factor.

## Future Boundary

The star chart, mobile ship, company war, CEO routes, post-Moon-Lord Deep Maw, and procedural completion content are deliberately roadmap items. Act 1 establishes their vocabulary without pretending to ship them early.
