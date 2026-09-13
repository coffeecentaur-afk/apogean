# Apogee Wastes — Design Bible

This document is the binding creative and progression reference for the mod.  A feature that conflicts with it needs an explicit design decision before it is added.

## Provisional Maw contour evidence

September12 provisional Maw contour evidence: retain the uninterrupted tapered
shaft direction, porous roots and native hammered points. A separate old/new
8/11/12-tile gallery removes squared underside notches; its cortical shading is
still under review. This does not approve a safe stair route, erase shallowV1,
or authorize normal-world replacement. See `MAW_RIB_CONTOUR_CONTRACT.md`.

## Reusable engine/tooling authorization

September13 bounded native ascent confirms ordinary player rope works beside
the candidate rib; this does not install safety ropes or approve full-route
returnability. Keep actual item placement, climb, dismount, mining and natural
hazard readability as separate gates. Preserve the difficult editable descent.

September13 rope lesson: distinguish direct tile creation, native mining hooks,
actual item-use placement and actual player ascent. Native rope persists after
support removal; do not invent a vine-style anchor requirement. Candidate rib/cap
mining proof does not supply missing placeable-item bindings. These are separate
gates, not reasons to install ropes or safe staircases in the Gullet.

September13 evidence discipline: a jump-only QA attempt that does not reach an
exit does not justify a forced gear gate, guaranteed damage or automatic safe
ledges. Preserve the difficult, player-modifiable descent and test rope, mining
and other approaches separately. Missing fixture coordinates must never be
reported as preserved terrain merely because their array slots exist.

September12 validation lesson: preserving existing QA-world records is not
permission to run QA controls. Saves by a plain character or server must retain
checkpoints and fixed background profiles without rebuilding them. Keep
movement/rendering/mutation gates separate, and compare native save contents.
This improves evidence reliability; it is not a lore, multiplayer, or art change.

September7: the user permits a separate reusable engine or tooling mod when
needed, with a downloadable dependency for consumers. Do not infer that every
feature needs a new mod or require players to install development-only tools.
Keep Apogean's content identity separate from shared infrastructure; document
API/version/install/save/multiplayer boundaries and validate the integration.
The workflow records the extraction gate. No library is created by this decision.

September9 follow-up: user wants workflow/dependency options researched and
future boss-code/video analysis retained. `RESEARCH_AUTHORING_TOOLING_2026-09-09.md`
records the audit and proposed small tooling improvements; no dependency is
adopted. Reuse authoring/render/validation methods across biomes, not identical
scenery. The jungle remains a living, overgrown survivor. Boss research is a
later preparation gate, not authorization to skip the current Maw tooth slice.

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

### Authoring evidence gate

September12 Maw inspection separates art readability from natural light: one
explicit QA lamp may reveal contours but cannot approve hazard visibility.
Preserve the current material palette while testing smoother continuous rib
undersides in a separate specimen. Rendering errors must be traced to native
state before changing accepted art or parallax; see `AUTHORING_WORKFLOW.md`.

Current flight correction (September8) supersedes the historical 15%/25%/50%
hard ceilings below. The user reports a sudden scenery speed change at each
ceiling and approves a broad, altitude-based response transition. Preserve the
accepted art, ground composition, horizontal depth and wider Mid spacing. Keep
Close moving more than Mid, and Mid more than Far throughout the climb. No
timed catch-up, spring, opacity fade or shared full-camera-speed lock. Stopping
must stop scenery; descent must retrace the same positions. Exact exit heights
may move so land can leave view smoothly near Space. The QA candidate integrates
a smooth response after the first 10% of ascent. September8 follow-up: the user
accepts the installed motion, "works great". Freeze this bounded motion baseline;
do not reopen tuning without a new defect. Broader integration gates remain
separate. Evidence: `Art/Validation/WastesSmoothFlight-2026-09-08/README.md`.

The user accepts Native-v3 pod appearance, manual pickup/replacement and the
native shallow divot (September8: "it looks great continue"). The bounded
starting scene is fixture-pass; preserve its appearance. Broader integration
and multiplayer checks remain. Proceed to the Maw entrance/shallow baseline.

Latest foreground clarification (September7): the combined diagonal response
feels too near/reactive even with stable section anchors. Push Close back toward
Mid, remaining slightly nearer. The depth trial uses Close .20 horizontal/.06
vertical, with a15% ascent ceiling; Mid .14/.03 and25%, Far .055/.012 and50%
remain unchanged. This supersedes tile-speed vertical Close locking, not stable
section identity. Project terrain relief through the same depth; no spring,
camera-follow catch-up, art redraw or altitude fade. The user now accepts this
movement/composition. Native matrix completion remains separate. Evidence:
`Art/Validation/WastesForegroundDepth-2026-09-07/README.md`.

Follow-up spacing request: reduce Mid landmark frequency and enlarge its quiet
gaps. Preserve the accepted foreground; occasional tall-bank overlap is minor
deferred polish. The spacing-only trial changes the five-landmark period from
6,800 to10,000px, keeping original art/size/order and parallax. No new world is
needed. See `Art/Validation/WastesMidSpacing-2026-09-07/README.md`. The candidate
builds cleanly and passes the bounded eight-case camera matrix at2560x1369.
The user now accepts its appearance: "looks great move on now". Freeze the
approved art, motion and spacing; proceed to the starting-area pod slice.
Ordinary-world promotion and the remaining integration matrix stay separate.

Wastes camera clarification (September7): foreground sections have stable
world/terrain anchors, never a shared ground height resampled under the moving
camera. Midground and distant scenery use maximum-height positional locks and
leave view through camera motion, not altitude opacity. Current QA tuning locks
Mid at25% and Far at50% of the ascent reference; biome transitions and global
reclamation blending stay independent. Details and evidence live in
`Art/Validation/WastesHeightLock-2026-09-07/README.md`. This overrides earlier
altitude-fade staging, not the accepted art or Close→Mid→Far recovery order.

Latest clarification: lower the maximum **flight caps**, not the ground-level
placement. The25%/50% values replace1/3/70% as a bounded tuning trial; the user
has not yet reviewed those exact numbers. No downward base offset, texture
resize, foreground anchor change or reinstated altitude fade. Evidence:
`Art/Validation/WastesLowerCaps-2026-09-07/README.md`.

Every visual or gameplay family advances through `specified` → `contracted` → `fixture-pass` → `integrated` → `polished`; a failed live render or explicit rejection moves it to `rejected`. A clean build, correct PNG dimensions, or passing static validator cannot by itself advance visual status. Promotion requires the family contract, one deterministic in-game fixture, production-path evidence, and the review recorded in `Tools/AuthoringStatus.json`. Work proceeds one dependency family at a time unless a later feature is explicitly labeled as a disposable prototype.

### Environmental history

The approved recovery workflow is recorded in `AUTHORING_WORKFLOW.md`. A — Snapped v3 fulfills the user's v2 approval condition of slightly thicker branches. The exact native textures are installed after a clean build and bounded disposable-grove proof; see `Art/Validation/WastesSnappedA-v3/README.md`. Extended checks add strong wind in both directions, partial night/paint coverage, native wood drops and whole-object ground-cover breaks without changing approved art. V1 was rejected for jagged intact branches and bright bark speckles. Offline comparisons remain separate from live screenshots; the broader production matrix is still open. Grass-corner validation now uses the actual production grass/soil classes with matching vanilla controls, not obsolete candidate tiles.

The subsequent persistence/coating pass verified the same saved grove after reload without rebuilding it, real painted side branches, and accelerated production-sapling growth/clearance cases. Rigid debris must honor native paint, Echo, illuminant and actuation as well as remain visually whole: native painted atlas cells now reconstruct the unchanged approved sprite in one rigid coordinate system. The old unpainted whole-texture draw failed the live comparison. The user subsequently accepted the displayed grass/soil connection and continuation to Wastes backgrounds. See `Art/Validation/WastesCoatings-2026-09-04/README.md`; manual interaction, broader terrain coverage and multiplayer remain distinct from these bounded proofs.

The apocalypse is the world's baseline, not a single optional biome.

- The Wastes replace the ordinary starting forest as a neutral, non-spreading biome: dead terrain and trees, broken roads, substations, and settlement remains. They do not count as world evil.
- Desert panoramas preserve collapsed highways, freight routes, and buried logistics works.
- The Jungle is the surviving green exception: lush canopy and dense living growth reclaim abandoned Helix research stations and containment infrastructure. Human occupation failed; the forest did not. Do not apply Wastes browning, leafless silhouettes or a desolate skyline across it.
- Snow panoramas preserve frozen relays, pipelines, and remote industrial sites.
- Ocean panoramas preserve drowned ports, wrecks, and failed evacuation infrastructure.
- Glowing Mushroom panoramas show fungal reclamation consuming failed hydroponics, conservatories, nutrient gantries, treatment tanks, and waterworks. Cyan and restrained lavender light remain accents inside a dark cobalt ruin silhouette; the center stays open enough to read traversal and combat.
- Corruption, Crimson, and Hallow remain immediately recognizable Terraria biomes. Their backgrounds show each force consuming or transforming the same ruined civilization instead of being replaced by the Engraft.
- The Maw is a second layer of danger: active, biological, ochre-and-charcoal territory growing through the already-dead Wastes.

**2026-09-06 architecture direction:** Wastes/ordinary forest ruins carry Kessler's military-industrial history; infected-biome ruins carry Helix's study/containment history; snow and desert emphasize broken highways, bridges and logistics; sky-island/space remains carry Sentrix's precise surveillance/data architecture. These are environmental-history rules, not new territory, biome-priority, infection-origin or progression rules. Helix studied infected places; this does not establish that Helix created every infection or that Hallow counts as evil. The living Jungle exception overrides any generic desolation treatment. Full per-layer briefs and review boundaries: `BACKGROUND_BIOME_DIRECTION.md`.

Each surface biome has at least two authored background compositions. Every composition is a matched transparent far/middle/close parallax set over Terraria's native sky—not a baked panorama. A world's seed selects one composition per biome and that choice remains stable. Terraria's sky and lighting move the same composition through day, night, and solar eclipse so landmarks never jump when time changes. Underground scenery follows tModLoader's separate four-texture transition/ground/rock contract. A later player-facing projector may deliberately cycle a biome's composition; random runtime cycling is forbidden.

The user's latest clarification reaffirms that the current roadside/cliff scenery belongs to the **Wastes only**. Reuse proven rendering tools across biomes, not the Wastes artwork itself. The Maw and every other supported biome require distinct scenery at a comparable quality target, including their appropriate underground/depth families. Finish the bounded Wastes baseline first; the Maw is the next background family within the existing starting-area → shallow-Maw sequence. Forced Forest QA views are deliberate overrides, not whole-world art direction.

Background corrections are durable requirements for later variants: connected silhouettes, clean material-appropriate edges, stable terrain anchoring, authored flight-visible undersides, quiet intervals between landmarks, and correct biome selection with the confirmed world-wide Wastes recovery signal. Preserve those requirements through the feedback-to-test workflow in AUTHORING_WORKFLOW.md and the versioned background-authoring skill. Reuse the process across families without homogenizing their artwork; every new composition still needs native game review.

September7 reclamation study: `Art/Candidates/WastesReclamation-v1/README.md` holds early/denser Far growth references. The user subsequently approved the shown vegetation style; do not re-ask that choice. Both generated studies change masonry/pixel scale and have opaque backdrops; they are not installable variants. Full recovery still needs broader living vegetation than the denser study. Preserve the original ruin pixels when authoring registered plant layers. `WASTES_RECLAMATION_DIRECTION.md` includes proposed fixed eligible-surface counting and overlapping response curves with offline arithmetic tests. Style approval does not settle terrain policy, the calibrated full-recovery endpoint or native implementation; the installed local Forest fallback remains unchanged.

On 2026-09-04 the user approved the Wastes/restored/Maw landscape concepts and authorized installation, requesting resolution comparable to high-quality background mods. Composition approval does not approve malformed exports. Compare actual texture dimensions and draw scale separately; see `RESEARCH_BACKGROUND_RESOLUTION_TARGET.md`. Do not install painted transparency grids, silently enlarge art and call it more detailed, or increase the GPU budget merely to obtain a larger dimension label. The first new far-layer export failed these checks and remains outside production. With subsequent permission for deterministic local processing, Wastes V1 now has three 2048×1280 hard-alpha layers (30 MiB raw RGBA) staged only in the disposable QA world/Forest lab. Ground, flight, lighting, native-scale and restoration/Jungle routing evidence is in `Art/Validation/WastesLandscapeV1/README.md`; this is not general-world or final artistic acceptance.

Surface masters target native-detail presentation rather than vanilla's enlarged low-resolution slot path. Verify authored pixels against physical screenshot pixels: `Draw(scale:1)` is not sufficient, because Terraria applies forced minimum background zoom at 1440p. Wastes V1 is the first candidate with a measured correction (111 source pixels / 110 live pixels, versus 150 before correction). Require hard alpha, transparent sky regions, independent far/middle/close parallax, authored vertical coverage and visually accepted repeat joins. Night and eclipse tint the same geometry without losing environmental readability. The nine-biome V0 library is already used by ordinary surface styles, but remains unapproved diagnostic art with unresolved coverage and native-scale defects. Promotion additionally requires stable biome crossfades, at least one original alternate composition per biome, transition-hitch testing, and an accepted texture-residency budget.

The 2026-09-05 live purification probe reproduced an outgoing Wastes fade stuck at full opacity. Both1080p and1440p native PureSpray tests now verify real restoration plus19 incoming/19 outgoing intermediate samples after moving opacity reads to draw time. A separate isolated repeat-phase harness prevents foreground mountains from hiding texture joins; it is not world-traversal proof. Repetition, lower rock/soil composition, other scene cases and candidate-specific performance remain open. See `Art/Validation/WastesLandscapeV1/2026-09-05/README.md`. This technical result neither promotes the candidate nor authorizes skipping artistic review.

The close and middle layers frame ordinary ground-level play first. Important ruins cannot sit so low in the texture that they appear only while flying high above the surface; broad negative sky remains, but the composition's readable landmarks must enter the normal camera from typical terrain elevation.

### Ruined forest ecology

The Wastes must remain a complete Terraria building biome rather than a set dressing pass.

- Naturally generated forest terrain is converted to a separate Wastes family: soil, stone, grass, sand, ice, snow, mud, unsafe background walls, dead trees, and dry surface growth. Vanilla green grass, walls, seeds, and related restoration tools remain obtainable and placeable.
- Restoration is deliberately two-step. Purity converts hostile Maw terrain and unsafe walls into neutral Wastes; a second purity pass converts Wastes into the corresponding vanilla dirt, stone, grass, sand, ice, snow, mud, and natural wall family.
- Restored Wastes retain the same ruined city and roadside landscape, progressively reclaimed by real green turf, vines, shrubs and living trees, not replaced by a normal forest panorama or merely tinted green. The user's September7 clarification makes world-wide recovery the intended driver, superseding the earlier local-only preference. Confirmed order is Close leading, Mid following and Far trailing, with overlapping recovery: small amounts of greenery appear across the entire Far city while Close first greens, then grow denser as recovery advances. Far is the visual indicator of world-wide restoration, not a left-to-right wipe or a phase locked behind fully green Mid. All layers reach their fully living variants at full recovery. Keep architecture, placement and terrain anchors stable. `WASTES_RECLAMATION_DIRECTION.md` records confirmed intent, open counting/response-curve rules and the bounded art-first implementation path. The installed local65%/35% native-Forest fallback and its routing tests remain technical evidence only, not this final design. Other biomes and third-party scenes retain priority.
- Dead forest trees are real trees: they can be chopped, shaken, planted with acorns, regrown, painted, and harvested for ordinary Wood. Their leafless canopy does not emit peaceful falling leaves.
- A dead tree's struck segment is the visual cut point. It may leave Terraria's normal bounded stump below the strike, but it must never respond by scaling the same complete silhouette down into a shorter tree. Ordinary Wastes trees use the narrow vanilla trunk/base footprint with no separate root flare. Their broken wooden top has a centered, trunk-width socket with enough vertical overlap that wind sway never exposes the crown joint. Height and branch placement vary materially, and natural density leaves regular readable travel gaps instead of forming copy-pasted walls.
- The user approved **A — Snapped** on 2026-09-04: a jagged broken leader, small subordinate stubs, compact roots, and consistent subdued bark from base to tip. See `Art/Reference/2026-09-04-Wastes-Tree-A-Approval.md`. The previous mismatched terminal forks remain rejected; B and C are not approved. Show the native component sheet and assembled comparison before applying new art to the tree model. Design approval is separate from in-game acceptance. Maw trees require their own subsequent design review.
- Follow-up art constraint: intact branch edges taper continuously; splintering belongs at broken ends, not randomly along the shaft. Bark must not be dotted with white/cream or glowing amber flecks. The user accepted v2's small dark knots and recessed hollow treatment subject to thicker branches. V3 adds one native pixel on each side, keeping branch centerlines and lengths unchanged. Recesses remain wood texture, never changes to collision or transparency.
- Occasional broken trees are intentional environmental evidence, not a second fake tree renderer. Roughly one in seven eligible natural Wastes trees may lose an upper native segment during generation, leaving a short independently choppable storm- or war-snapped trunk. The clear majority remain full-height trees.
- Wastes grass is a distinct tile family whose cap visibly keys into the soil beneath it at flat runs, slopes, ledges, and single-tile steps. A bright seam, hovering fringe, or exposed gap between grass and soil fails the terrain gate.
- Dry twigs and brush are brittle ground objects, not two independently swaying grass halves. They either remain rigid and break on contact or use a deliberately authored whole-object reaction; a collision split down the center is invalid.
- Living Trees retain their wood, roots, rooms, doors, chests, and traversal. World generation removes or replaces their green leaf canopy rather than deleting the structure.
- Naturally generated unsafe grass and flower walls receive ruined variants. Player-built safe walls and restored green terrain are never globally rewritten after world generation.

### Spawn sanctuary

- New worlds reserve a sanctuary centered on Terraria's final spawn point, initially 110 tiles horizontally and 70 tiles vertically in each direction. It is an Apogee world-edit exclusion, not a peace-zone buff.
- The sanctuary still becomes neutral Wastes and may contain safe cosmetic ruins. Maw generation, Maw spread, Maw outgrowths, and corporate compounds cannot enter it.
- Ordinary Terraria enemies, invasions, blood moons, eclipses, player-summoned bosses, and other normal events remain allowed. The sanctuary never suppresses vanilla spawning or prevents a player from building an arena at spawn.
- Legacy worlds receive the same implicit safety rule without silently generating a new Maw or moving existing terrain. A forced debug command may bypass the rule only for explicit playtesting.
- Spawn remains mechanically safe even though its palette and ecology communicate a dead world.

#### Arrival pod — approved starting-area direction

- September8 current: Native-v3 appearance/flat footing, manual pickup/replacement and generated divot appearance are accepted ("it looks great continue"). Preserve this scene. Candidate-gated new-world QA uses a final Wastes survey, 13-wide/two-deep or compact 7-wide/one-deep bowl, safe undug fallback, unchanged spawn and native furniture placement. Failed placement preserves terrain. Broader production gates remain; older pending-v2/v3 statements below are historical. Next: bounded Maw entrance/shallow area per `MAW_ENTRANCE_CONTRACT.md`. See `Art/Validation/ArrivalDivot-2026-09-08/README.md`.
- The player wakes with amnesia beside an opened, damaged drop pod in a small shallow impact divot. Disturbed soil and sparse debris tell the arrival story without creating a large crater, an escape obstacle or a hazardous spawn. Keep an unobstructed player spawn and travel route.
- The pod is real world furniture behind the player, not parallax artwork. Its initial design has no obvious corporate branding; its broken communications panel hints at later use. Show its native-scale design before production installation; an isolated in-game review is appropriate when the user needs actual scale to judge it.
- Pod art clarification: preserve the seat, hinged open hatch, heat shield and unbranded shell through deliberate pixel clusters. A3's native appearance was initially accepted, then rejected as too detailed after live testing. The user accepts A4's coarser style and now agrees to A5's damaged-but-survivable crash direction: broad soot/mud patches, large chipped edges and exposed framing instead of tiny speckles or intricate panel lines. Native-v2 is a 2x2-grid candidate installed only in isolated QA for the user's requested in-game judgment; day/overlap/night evidence is in Art/Validation/ArrivalPod-v2-2026-09-08, but visual acceptance is pending. Keep the movable pod separate from shallow compressed terrain and scattered debris. Picking up the pod leaves the scar; replacing it neither creates a new scar nor moves spawn. Do not turn the spawn divot into a large crater or obstacle.
- The user explicitly approved immediate removal with a normal pickaxe. The pod drops intact as a placeable item; relocation changes neither Terraria's world spawn nor the player's bed spawn. No first-boss gate or spawn-moving function. Its relay still unlocks later. `ARRIVAL_POD_CONTRACT.md` specifies the proposed 5x6-tile furniture and shallow-divot placement for the next art/fixture gate, not a completed implementation.
- Generate the arrival scene only in a newly generated supported world. Preserve existing player terrain; rejoining or dying does not generate another impact scene.
- Native footing correction: the user likes the coarse damaged pod but rejects its curved, floating underside. Preserve the body and hatch; give the crushed metal base a broad flat contact edge and validate it against actual terrain, not merely a count of lowest opaque pixels. The movable item must also sit convincingly on flat player-built ground without requiring a crater to disguise its footing.

#### Future communications relay — specified, deferred

Repairing the pod eventually makes it the first ground-to-orbit relay for the ship's hologram terminal. Eligible NPCs are connected by their assigned housing locations inside its transmission boundary, rather than transient walking positions. Additional craftable relays support distributed settlements. Pod loss or relocation must never permanently lock out communications.

Remote contact supports dialogue and purchases with shipping costs, not selling or unrestricted remote services. Existing progression, corporate access and individual standing still apply. Signal range, repair unlock, contact availability while an NPC is absent, and shared-world ownership remain later contracts. No relay network or ship UI is part of the starting-area implementation.

### World placement contract

- Complete Apogee campaign generation initially requires a standard large world. Medium support may receive authored compact variants later; small worlds do not silently generate an incomplete campaign.
- All three Corporate Campuses, the Maw Rupture, and guaranteed ruins are permanent day-one landmarks. Progression activates, opens, repopulates, or re-arms them instead of generating large structures into an inhabited world.
- The world planner solves critical landmarks together against the completed vanilla world, registers their envelopes before any Apogee construction, and uses bounded rerouting rather than deleting intersecting structures after generation.
- Major Apogee landmarks exclude the spawn sanctuary, oceans, Dungeon, Jungle Temple, full Jungle and Underground Desert macro-regions, Shimmer, and known Calamity macro-biomes. The Dungeon-side ocean and Abyss column are forbidden when Calamity is loaded.
- Kessler occupies a fortified surface Campus, Helix uses a surface biodome over a larger underground laboratory near but outside the Maw, and Sentrix occupies a sealed floating spire. Their horizontal sides are selected per seed by safety scoring rather than fixed left/right assignments.
- Each Campus is one authored whole-building blueprint placed without stretching inside its saved atlas reservation. Its silhouette, decks, walls, public frontage, progression entrance, reusable arena, and furniture layout are stable across seeds; only its world location and bounded terrain skirt vary.
- Corporate interiors use complete native-scale tile families rather than generic block recolors: structure panel, technical/window wall, platform, chair, table, functional workbench, light, console, storage, and faction-signature animated machinery. Kessler reads as fortified armory infrastructure, Helix as clinical containment, and Sentrix as surveillance/data architecture.
- Kessler's validated native construction language is gunmetal masonry and armour plate with burnt-red structural trim, restrained signal-orange controls, warm amber service lighting, power-armour racks, and a rigid-pole war standard carrying a shield with three chevrons. Rooms stay near Terraria housing scale; broad empty box interiors are invalid.
- Kessler's day-one Campus uses a compact 152x72 authored footprint inside a larger protected event reservation. Paired two-stage checkpoint towers and patrol walks mark the perimeter; their ground passages and west-side quartermaster frontage remain physically open, while a distinct internal bulkhead controls access to the armory. The stepped logistics deck, operations deck, narrow command crown, backed platform shaft, animated standards, and terrain-keyed footing must survive the same production-template render used by world generation.
- Kessler placement keys the reservation to the blueprint's authored surface line at local Y=54, scores terrain across the centered 152-tile building footprint rather than the wider event reservation, and rejects sites with more than 28 tiles of relief. The Wastes conversion pass runs after the atlas is saved but before compounds and ruins, so a new foundation can never masquerade as the original forest surface or strand living terrain beneath the Campus.
- Every auto-framed solid and wall atlas uses Terraria's real edge/corner/isolated framing topology. A filled grid of repeated 16-pixel squares is invalid even when the canvas dimensions are correct. Apogean bundles these textures directly; a separate resource pack is neither required nor accepted as a substitute.
- Native-format atlases are validated in a disposable in-game gallery, not from their source PNG alone. When a Terraria atlas uses opaque frame cells, Apogean preserves its alpha and within-frame luminance continuity before applying an original palette; independently outlining or procedurally filling every cell creates visible bamboo-like seams and is forbidden. Ruined trees stay inside Terraria's segmented trunk, branch, and top contracts so height variation and chopping remain native. The ordinary tree family keeps a narrow vanilla-like base and has no fixed root overlay; top sockets must remain centered and overlap the trunk throughout wind sway. Full-tree overlays, height-scaled composites, and silhouettes that survive after their supporting segment is removed are forbidden. Gameplay and capture-camera output must use the same draw path.
- Every production atlas has one authoritative generator or checked-in source. A focused generator must not rewrite validated assets owned by another family; regeneration is accepted only after static contracts, a clean build, and a fresh in-game capture all pass.
- Authored multi-tile furniture is placed through WorldGen.PlaceObject using its registered TileObjectData origin and alternate, never by painting frame coordinates directly. Corporate floor and trim tiles accept native anchors; a structure that looks solid but rejects its own furniture fails validation.
- Placeable terrain items and physics projectiles inherit the exact pixel topology and scale of the corresponding renderer-exported Terraria item or projectile. A custom terrain identity is incomplete unless mining, item placement, falling-block recovery, and special ammunition behavior all return that same custom material.
- Hostile Maw counterparts derive from each validated neutral Wastes atlas without changing its native frame topology. Soil, grass, stone, sand, ice, snow, mud, clay, unsafe walls, drops, falling-block behavior, and Sandgun behavior remain materially distinct throughout Wastes → Maw conversion and both purification stages.
- Ground Campus blueprints mutate only explicitly authored clear/tile/wall/object cells. Empty reservation cells preserve host terrain, Kessler seals a compact authored footing into the surface, and Helix anchors its walkable dome at the sampled surface while the laboratory extends below. Sentrix alone may clear its full reservation because it is deliberately floating.
- Authored structures execute in two phases: shell tiles, walls, and anchors are framed first; frame-important furniture is then placed through its registered `TileObjectData` origin and alternate. Directly painted furniture frames are forbidden because they bypass Terraria's anchor, animation, and save/load contracts.
- Furniture authored on catwalks must register the platform-compatible `SolidWithTop` anchor in addition to full solid floors. Wall-mounted fixtures require authored backing walls at every anchor cell. Fresh-world acceptance validates these native anchor rules after save/load and exercises any progression door through a complete open/re-arm cycle.
- Every supported world guarantees one abandoned outpost for each corporation, one neutral pre-war settlement or transit ruin, and one independent Maw research site. Additional small ruins are opportunistic and never displace critical Terraria or third-party content.

### Underground and Underworld scenery

Underground backgrounds are routed by both biome and depth. Wastes, desert, snow, jungle, Glowing Mushroom, Dungeon, evil biomes, Hallow, the Maw, and the Underworld never silently fall back to one generic cave set. Each authored set may include ruined mining camps, rails, shelters, research remains, or military infrastructure appropriate to that biome. Until a dedicated ruined set exists, preserving the recognizable vanilla background is preferable to applying the wrong Apogee background.

Surface compositions may crossfade through their parallax layers. Underground backgrounds use hard texture-set selection, so visible borders require authored transition bands, neutral seam textures, or bounded biome hysteresis rather than random switching.

September12 technical cave study does not change this direction: an explicit dry
overlay can be placed behind ordinary cave tiles/walls, but its darkness, water
boundaries and final art are not accepted. Preserve biome-specific geology and
hazard readability; do not make the cave fullbright to expose background art or
erase natural walls to reveal it. The Q/R probe is not a production renderer.
See `MAW_CAVE_BACKDROP_PROBE_CONTRACT.md` for that narrow evidence boundary.

The global ruined Underworld backdrop is called **the Ruined Deep**. It reads as a buried pre-war refinery and transit horizon swallowed by soot, slag, and lava: distant fractured cavern columns and factory silhouettes, middle-depth broken pipe bridges and towers, and a close field of collapsed rails, winches, cables, and amber work lamps. Its palette is near-black soot, burnt umber, oxidized iron, and restrained amber rather than Maw flesh or Corruption purple. Broad negative space keeps combat and foreground tiles readable. The Burning Root and Stomach remain unique world geometry and never repeat as panorama decoration.

The Underworld does not use `ModUndergroundBackgroundStyle`; Terraria draws a separate five-depth Hell panorama. Apogee replaces that panorama through one client-only custom-sky compositor in the final remaining background depth band, after vanilla Hell layers but before tiles, liquids, entities, and UI. Its opaque far layer and hard-alpha middle/close layers tile horizontally at distinct parallax rates. A dedicated Underworld fixture must prove complete widescreen coverage, clean alpha, layer order, and safe gameplay rendering before the compositor may become the production default.

## The Maw

The Maw is the hostile biome created by the distributed Broodmass organism, not a recolored Corruption. The neutral Wastes beneath the rest of the world are a separate biome and do not spread.

September12 cave-layout direction: side caverns should connect through narrow,
winding, player-sized passages to other caverns and/or the main Gullet. Smaller
cave pockets branch from those passages, making full exploration more involved
than following the main descent. Teeth and other established hazards make the
connectors dangerous; do not seal them with impossible collision or introduce
mandatory unavoidable damage. Preserve preparation, mining and player-built
route options. Prove one bounded shallow connector before broad generation.

### Current fiber and rib direction — September 11

September12 decision: hanging fibers stay short, cuttable and non-climbable for
the current Maw pass; they do not supply free rope traversal. Provisional rib/
fiber art may be selected for disposable native studies while the user is away.
Final art/feel and ordinary-world deployment are not thereby approved. See
`MAW_AUTONOMOUS_PASS_2026-09-12.md` for the bounded implementation authority.

September12 lighting evidence: clean starter-character captures confirm ambient
amber dims in dormancy, but the shallow pocket is still hard to read without
player light. This is an unresolved implementation/readability issue, not a new
art direction or permission to flatten the cavern with fullbright lighting.
Retain dark gray/brown geology and sparse warm amber; judge hazards and rib joins
in native views before accepting distribution or promoting the generator.
The provisional1.6× amber trial is not a production brightness decision. A
separate labeled inspection lamp may expose texture/shape defects in held QA
views; it must never be reported as natural hazard readability or saved into
the biome. Keep that art inspection distinct from actual awake/dormant controls.

The user likes the mixed material direction, while requesting thicker, less
choppy **living fiber grass** on surface soil, wrapping exposed floors, sides
and ceilings with a later fibrous-vine family. Preserve separate soil/stone/bone
host identities and fix the observed material joins; growth is not a blanket
retexture or permission for a new infection rate. Bone forms structural cavern
ribs and spike bases, while distinct, easily mined teeth supply contact damage.
Irregular natural rib footholds are allowed. This refines earlier "no safe
ledges" language: no regular safety staircase or supplied ropes, not a ban on
all usable rib surfaces. Preparation through rope, mining, hooks and mobility
should matter; no forced damage or mandatory named accessory. The Stomach and
enclosed outlet remain intact. `MAW_FIBER_RIB_DIRECTION.md` owns the confirmed
intent, recommended boundaries and native test matrix. Optional blending and
lighting mods are references/compatibility candidates, not required remedies
for invalid atlases. No new runtime or generation acceptance from this review.

Implementation checkpoint: a separate QA study now demonstrates bounded
four-sided maturation of already-Maw soil and preserves buried soil, stone and
bone. It does not set the biome's infection rate or implement vines. Three
rough bone arcs are geometry studies, not approved anatomy or a descent route.
The requested thicker rooted grass, attachment-edge repair and equipment-aware
traversal still precede generation. Exact evidence lives in
`Art/Validation/MawFiberRib-2026-09-11/README.md`, not a new art/lore decision.

### Earlier material and hazard evidence

September9 latest art correction: the user requested the artwork treatment across
Maw terrain but rejected the softer design. Simplifying native pixel detail must
not automatically soften the material shapes. The proposed response uses brittle
fractures, torn fibers and localized amber in distinct host materials, preserving
the dead grey/black/brown identity and previously accepted local depth accents.
See Art/Candidates/MawTerrain-Harsh-v1/README.md. The user subsequently approved
this twelve-family board and requested implementation as close as possible to it.
That directory's IMPLEMENTATION.md tracks native evidence. Keep each material's
own structure, not one recolored pattern. Original art supplies the pixels and
native frame masks supply topology; grass and overlapping walls have separate
contracts. Diagnostic art does not certify production mechanics or generation.
Ordinary materials remain non-emissive; localized amber and dormancy stay separate.

September9 art clarification: preserve the dead grey/black/brown/amber surface
identity. Explore localized deeper materials and mutations, not just Helix/Maw
palette swaps on identical creatures. Follow-up approval confirms the cooler
slate-blue membrane in `Art/Candidates/MawBone-v1/DepthDirection-v1` as localized
deeper tissue, not an all-blue biome or uniform enemy recolor. Chalky parasite
casings and rusty heat-weathering remain supporting proposals; none of these
create new biome tiers or gameplay. Actual anatomy,
host material and parasite growth should distinguish creatures. Amber remains a
connecting cue rather than a compulsory full-body palette. Original-color
terrain now has a native-mask cutout probe; static export is not art approval.

September9 follow-up: user approves the quieter five-color MaskedNative-v2
structural bone and permits asset creation with native property tests to inform
design. Keep original source color separate from engine-derived alpha masks.
The bounded bone fixture now has native evidence; this is not acceptance of
every asset, finished rib shapes, teeth, or the generated mouth. Supporting
bone remains hardened/safe/non-emissive; exposed brittle teeth are a separate
easy-to-mine contact hazard. See `Art/Validation/MawBone-2026-09-09/README.md`.

September9 attachment clarification: the user likes the cluster and retains
all short/long/wide individual models for a mixed natural hazard family. The
same recoverable cluster must attach to solid floors, ceilings and either
vertical terrain face, pointing into open space. Background-wall-only floating
placement is not the same contract. Keep approved pixels, thorn-style contact
and easy whole-item recovery. Four-way native framing/support/hurt/reload proof
precedes seeded dispersal through the mouth. Acid remains optional/separate.

LATEST user override: choose curves at PLACEMENT time. Floor/ceiling rounded
BACKS follow player facing; sharp points lean OPPOSITE (explicitly confirmed).
On either solid wall: player below object center means point up/back down;
player above means point down/back up. Exact-height tie provisionally points up.
Stay fixed afterward, including other players passing and save/reload. Applies
to all retained short/long/wide/cluster models. Individual specimens still need
their own final placement/hazard implementation. Cluster Native-v4 is now an
installed QA candidate:619 native API checks and actual eight-bank save/reopen
pass. Old frames/roots retained. Manual/MP/art/worldgen gates remain separate.
Read Art/Validation/MawToothCluster-2026-09-09/PlayerCurves-v4/README.md.

ART approval reopened: teeth too detailed; the material called "moss stone"
looks like autumn leaves. Preserve originals and simplify ONE proof's shading
and mineral body before batches. QA room supports are MawDirt; verify the actual
reported material before replacing it. Mechanical mirroring is not a redraw.

Historical fixed wall anatomy: both backs underneath/tips upward, left/right
mirrored. This is now superseded by player-directed curves above.
Roots must overlap the terrain's visible contour, not merely its tile boundary.
Native-v3 uses4px root burial; original individual and floor/ceiling art stays.

Earlier September9: the user accepted the native short/long/wide tooth artwork
("they look great") and requests a densely packed placeable group. Preserve
these pixels and individual assets. A separate five-tooth cluster item uses
explicitly approved THORN-STYLE behavior: hurts players on contact, not a solid
wall, easily mined as one recoverable item. This is an exception for the grouped
item, not silent replacement of the earlier solid individual-fang experiment.
Empty spaces around the teeth must not hurt. Test native whole-object pickup,
re-placement, support loss and immunity; worldgen remains a later gate. No safe
ledges or progression/recipe changes are implied by placeability.

Historical tooth agreement: use recognizable authored tooth/fang/horn silhouettes,
not square blocks merely textured as bone. The first implementation is solid,
with exposed contact damage and starter-pick removal, not a drop-through
platform. Visible shape, solid occupancy and hurt region require separate
native checks; transparent pixels do not define collision automatically.
Damage amount, drop and exact mining speed remain tuning to contract. Keep
supporting bone separate and harmless; no safe ledges or preplaced ropes.

September9 native art review supersedes the pending verdict below: the user
rejects the broad fang because it reads as a broken fragment. Next candidate
uses a16x48 one-block-wide canvas, sharp continuous taper, extended shaft and
an embedded root. Preserve the original tooth shape rather than recontouring
it to fit a collision wedge. `Art/Candidates/MawTooth-v2/README.md` records the
exact-pixel offline study; shape approval and matching native physics remain
separate. No automatic generation or installation from static art tests.

September9 follow-up: develop a short16x32, long16x48 and wide32x64 family.
Wide must read as a complete heavier fang, not the rejected broad wedge.
`Art/Candidates/MawToothFamily-v1/README.md` owns the source art contract.
The user's subsequent request authorizes an IN-GAME appearance review before
final art approval: `Art/Validation/MawToothFamily-2026-09-09/README.md` records
the unchanged sprites in a native, harmless/nonsolid QA-only gallery. This is
not a change to the final solid tooth hazard agreement. Review remains pending;
do not substitute offline mockups or old wedge physics for the new native proof.
Shared palette, embedded roots, continuous pointed silhouettes; long unchanged.
After shape approval and matching native physics/mining proof, trial bounded
seeded selection at50/35/15 in irregular inward-facing clusters. These weights
are provisional. No new generation/installation from passing art checks,
invisible full-column collider, silent wedge reshaping or safe ledges.

Historical September9 prototype implementation: a32x48 solid/sloped fang with four
orientations now passes a bounded native fixture. Proposed QA tuning is30
contact damage, three starter-pick(power35) hits, one Bone drop for the whole
object. These values are not user-approved final balance. Exact silhouette,
framing, support, collision and Hurt evidence are recorded in
`Art/Validation/MawFang-2026-09-09/README.md`; art review, real traversal and
multiplayer remain. No generator change or new dependency; the mouth's dense
teeth and no-safe-ledges composition still await integration.

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
- Supported large worlds receive one major Rupture, one guaranteed Maw Outgrowth, and a second Outgrowth only when uncontested space remains. Outgrowths are small regional patches, not additional Gullets, and required Brood Nests remain inside the primary Maw. The major Rupture is an authored vertical scar that penetrates natural surface, underground, cavern, and Underworld terrain rather than repainting only the first soil row. Its terrain, walls, hazards, and scenery change with depth while remaining one continuous landmark. Growth is slow before Hardmode and bounded thereafter.
- The major Rupture uses a **Feeding Wound** grammar: one readable, winding central gullet surrounded by irregular side chambers, braided passages, and pale bone-supported loops. It is neither a straight Corruption chasm nor a field of round Crimson cavities.
- Its depth language progresses from an asymmetric surface mouth, through tendon bridges and amber glands, into broad ossuary chambers and hardened pressure channels, then terminates in a localized **Burning Root** region of the Underworld.
- The initial surface Maw occupies roughly 340–440 tiles, with a 70–100 tile Feeding Wound. The Gullet ordinarily preserves 20–30 clear tiles, opens to 35–50 around bends, and uses 50–90 tile side chambers. Players use rope, tooth-mining, hooks and mobility to negotiate it. September11 permits irregular structural rib footholds, not a regular sequence of guaranteed safe drops.
- The Burning Root contains **the Stomach**, a roughly 180–240 by 90–130 tile natural Matriarch cavity whose lowest shell remains approximately 30–60 tiles above the Underworld ceiling (targeting about 40). It has no generated platforms or mandatory repair objective; players clear and build it like a large evil-biome boss space.
- The Stomach ends the Gullet rather than opening directly into Hell. A narrow, enclosed intestinal descent continues below it toward the world floor; players deliberately breach its Platinum-tier Mawstone wall if they want to enter ordinary Underworld terrain.
- Do not add regularly alternating ossuary shelves, manufactured safe landing platforms or preplaced rope to solve the Gullet for the player. Irregular natural ribs may support deliberate landings; teeth and geometry keep descent hazardous. Traversal validation must test the intended rope/mining/mobility options, not force safe shelves to satisfy a drop-length cap. The existing generator/validator still need this correction when the new shape is implemented.
- Latest entrance direction: substantially taller raised "lips"/rim ridges and a dense abundance of teeth around the crown and down the throat, with the Sarlacc pit as a composition reference. Keep original Maw anatomy/materials and the two asymmetrical side basins; no copied creature, beak or additional mechanics. September9: the user approves the preview's51 tooth groups and rim crests24/22 tiles above local ground as the working composition, not final release tuning or native evidence. No safe ledges; rope or breaking teeth remains the intended player solution. New bone/tooth material art has its own review in `Art/Candidates/MawBone-v1/`; never promote an illustration or the separate Stone-topology diagnostic as final tiles.
- A compact Rupture preserves the Feeding Wound, navigable Gullet, and Matriarch cavity at the Underworld ceiling while reducing width, side chambers, and the amount of Root that penetrates Hell. Only failure to fit this coherent minimum may reject world generation.
- The surface mouth is implied by geology and composition rather than drawn as literal lips: bone stakes, leaning ruins, cracked terrain, and inward-pointing roots form the gullet silhouette.
- September8 user sketch: the entrance reads as an open tooth-lined throat between irregular raised banks, with a broad acid-water basin on one side and a smaller basin on the other. This is the next composition reference, not approval of the current thin dome or a dimensioned worldgen template. Yellow with orange squiggles means acid water. `MAW_SKETCH_DIRECTION_2026-09-08.md` records confirmed input, interpretations and the four new creature references; their AI, names and progression remain proposals.
- The natural route is traversable with ordinary Terraria ropes, hooks, platforms, and mobility. Frayed surface growth remains approachable, while hardened Mawstone requires approximately Platinum-tier pickaxe power or explosives. Bombs may break ordinary Maw terrain and exposed Ossamber geodes but never Nodes, Brood Nests, sheathed ore cores, or explicit progression membranes.
- Pale bones form arches, stakes, bridges, and structural ribs. Supporting structural bone stays safe. September8 clarification supersedes the previous blanket static-bone rule: clearly exposed **Maw teeth** hurt on contact like Dungeon spikes but are easily mineable. Keep these brittle hazards separate from hardened Mawstone/structural bone; ordinary starter-tool removal is the proposed tuning, with damage, drops and exact mining speed still to contract. Teeth do not need animation to be dangerous.
- September11 evening refinement: a rib should read as one continuous bone, not tiled bone fragments. Preserve porous bone for masses; test quieter cortical shafts and native tapered slopes without hiding real join gaps behind walls. The surface hierarchy is full fiber coat, rooted fiber/soil transition, then soil; liked side-face fiber remains. Hanging fiber is cuttable growth, with climbability still a separate unresolved choice. Matching unsafe walls and localized amber organs must retain their landscape during dormancy; no automatic safe rope route or worldgen promotion follows from a material fixture.
- Amber glands create strong pools of yellow navigation light separated by genuinely dark passages. Ordinary Maw turf and bone do not glow.
- Mining ordinary soil, stone, ice, snow or mud sheds matching non-emissive debris; digging must not reveal caves with a falling magical light trail. Amber organs own their glow and particles separately. Material art approval does not approve new emissive gameplay.
- Authored chambers may reserve digestive basins, but Environmental Alpha does not generate fake solid acid blocks and no progression depends on acid. A later prototype may use real amber-styled water only if its visuals and damage predicate can be isolated from ordinary player-built or naturally flowing Maw water. If that isolation is not clean, the basins remain dry or hold ordinary water while amber organs provide the digestive imagery.
- September8 portable-acid clarification: the user wants to move acid outside the Maw for enemy traps, retaining water-like movement and hazard until neutralized. A fixed hazardous region cannot fulfill that use. Follow-up agreement selects neutralization on mixing followed by ordinary Terraria liquid reactions, not literal conversion into the other liquid. Neutralized quantity, collection, enemy/boss immunities and damage remain uncontracted. Portability is the desired target, not a proven engine feature or a requirement to delay the entrance. No new chemical ID, terrain corrosion, biome spread or infinite acid source is implied. See the latest acid section in `MAW_SKETCH_DIRECTION_2026-09-08.md` and the portable addendum in `RESEARCH_MAW_BASIN_REVISIT_2026-09-08.md`.
- Any later regional depth-pressure or breath-depletion mechanic is separate from a digestive basin or liquid. It must have its own biome/depth predicate, equipment counters, multiplayer authority, and name.
- **Gullet**, **Ossuary Chambers**, **the Stomach**, **intestinal descent**, and **Burning Root** are development and lore terms inside the player-facing Maw biome. They become formal map sub-biomes only if later content gives them distinct music, enemies, loot, or mechanics.
- Where the major rupture enters the Underworld, it creates a distinct Maw-Underworld sub-biome rather than globally replacing Hell. This terminus is reserved for a deliberate progression encounter; assigning an existing boss to it requires a separate progression decision.
- Its atlas reservation owns only the authored route, shell, chambers, and basin envelopes. It consumes allowlisted natural terrain inside that plan, reroutes around protected or foreign structures, and never treats player structures, chests, housing, or protected sites as disposable conversion targets.
- Ordinary Maw turf does not glow. Amber glands, Maw Nodes, active organs, and other explicit energy-bearing growths may emit amber light.
- MATRIARCH-7A-1's defeat forces the network into visible dormancy: amber lighting dims and biological motion and spread fall to their minimum. Wall of Flesh and Hardmode awaken the network again; dormancy never purifies existing Maw terrain.
- The user approved reusing the layered-state visual method for Maw sleep/awakening: preserve the same Maw geology, bones, ruins and camera anchors while registered organ/light/detail layers change activity. Dormancy and reclamation are separate state inputs; sleeping Maw does not become green or count as purified. `MAW_DORMANCY_VISUAL_DIRECTION.md` records proposed subdued versus active cues and the separate art/runtime gates. Reuse the method, not Wastes city textures or its numeric curves. No new triggers or progression rules; implementation remains after the current Wastes/starting-area work.
- The Wall of Flesh is provisionally understood as an ancient planetary immune barrier partially infected by the Broodmass. It remains recognizably the classic horizontal Underworld guardian; its later resprite and attack redesign must preserve that identity while explaining the Maw's Hardmode reawakening.
- Corruption and Crimson are consumed at a frontier; Hallow pushes back and slows Maw growth.
- Maw conversion is an explicit allowlist shared by initial generation and runtime spread. It preserves ores, player housing walls, chests, furniture, Dungeon, Temple, hive, corporate structures, and unknown modded terrain by default while converting natural dirt/stone/grass/jungle/mushroom/ash/sand/ice/snow/mud/clay/silt/slush/moss/fossil/marble/granite/living-wood/leaf/thorn families and their unsafe walls into authored Maw counterparts.
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

- Kessler's concrete perimeter, towers, and gate define its territory. Players may reach its public forecourt and Quartermaster service frontage while the main facility remains progression-sealed.
- Helix presents a cracked surface biodome and reception frontage above a protected underground laboratory. A sealed observation bore points toward the Maw but stops short until later clearance.
- Sentrix is a vertical floating spire with exterior landing platforms that can be reached early. Its doors remain sealed until arrival; a later ground transit beacon provides reliable access. Exterior caches contain travel supplies, scrap, lore, cosmetics, and at most one modest exploration sidegrade—never core Sentrix progression gear.
- Arrival opens only public quest and shop space. Clearance opens testing and specialist wings; the company war reconfigures the same Campus into a short raid ending in its reusable combat arena.
- A hostile Campus raid targets roughly five to eight minutes on a first clear: one to two minutes of persistent security traversal, an optional one-to-two-minute second-in-command confrontation, and a roughly three-and-a-half-to-five-minute CEO encounter. It never pads duration with endlessly respawning guards.
- Corporate structure blocks, gates, conversion barriers, and a narrow defensive apron resist mining, explosions, actuation, and biome conversion until that corporation's CEO is defeated. Afterward the whole Campus becomes dismantlable so players may reclaim its land for building and transit projects.
- Each arrival is foreshadowed by an Orbital Omen: a temporary upper-sky craft or signal appears after the prerequisite, then the existing Campus activates and its invasion begins after deliberate player contact rather than an unavoidable surprise.
- Kessler is the first exception to passive contact: Wall of Flesh immediately announces its impact, then the first dawn observed after an intervening night begins a short live-fire assessment. The encounter contains ten total targets, escalates from Survey Drones to four Reclaimers, uses readable dashed acquisition lines, and deals damage through attacks rather than body collision. Clearing it grants each active participant five Kessler Scrip, opens the public frontage, and stations Quartermaster Mara Venn while the internal armory remains progression-controlled.
- Corporate progression is server-authoritative in multiplayer. Clients receive saved Campus and bulkhead bounds as world data and render replicated state, but cannot advance shared kill quotas, retire event NPCs, reposition authored contacts, or mutate progression-controlled structure tiles locally.
- Mara's standard Terraria contact panel owns requisitions and compatibility-friendly shop access. A separate Briefing button hands off to Apogee's branching dialogue UI on the following UI update; closing vanilla chat during the original chat-button draw frame is forbidden because it can leave stale GUI indices.

The faceless galactic government treats the CEOs as quota-bound colonial houses. A failed company is erased, stripped, and sold to its rivals. The player is the destabilizing factor.

## Wastes landscape motion and spacing — 2026-09-05 feedback

The user likes the current dead trees and war-torn ledge language. Preserve them. **Clarification: the nearest soil lip sits about three tiles (48 world pixels) above the fixed ground reference and is vertically world-locked, not screen-locked.** As the player flies up, this ledge moves down and out of view, exposing the middle landscape. A reduced camera response is not equivalent and was rejected. Keep independent horizontal depths and subtle vertical response only for the more distant layers. The current QA implementation uses the fixed `worldSurface - 50` reference; matching a generated regional terrain-height profile remains a production gate. Do not resample from player altitude or continuously chase the nearest block. Each client owns its camera presentation, not shared landscape state.

The midground becomes the dominant scenery around the first third of ascent, then gradually yields to the distant skyline; its terrain is not literally suspended one-third up the world. Keep the Close layer present during shallow descent. It must not blink away at a player-height threshold: the ground anchor stays fixed, and eventual cave/underground takeover belongs to the native depth transition after surface scenery leaves view. Verify this transition in a visible, wall-free route rather than counting a terrain-obscured screenshot as proof.

Concentrate conspicuous sediment bands in the nearest ledge. **Latest midground direction: separate authored landscape chunks, not a continuous sediment wall.** A hill with broken highway, a gas-station rise, quiet eroded mounds and occasional joined hill groups leave genuine transparent gaps through which Far remains visible. Gaps remain transparent below the hill baseline too; the renderer must not refill them with an opaque Mid strip or lower-strata continuation. Keep drooping dead grass and broken surface edges. Add two or three quiet/open or lightly broken sections between major landmarks. Layout must be stable by world/region, not rerolled while flying, crossing a chunk boundary or reloading. Use consistent scale and authored compatible junctions for joined pieces; never mirror text/recognizable ruins as a substitute for variants.

This requires original reviewable concepts before new textures are installed. Final runtime dimensions, alpha, seams and live composition need their own proof. Mirrored lower-strata continuation in the current QA renderer is a coverage guard only, not an approved final art solution; the modular Mid implementation must remove that guard from Mid while preserving Far coverage. Restoration should reuse the stable arrangement with restored material/vegetation variants, not shuffle the hills when greenifying.

**Flight-visible underside clarification:** modular hills cannot be shallow cutouts with flat chopped bottoms. Prefer a stable world/region height reference plus authored deeper cliff faces, descending slopes and compatible continuation pieces beneath each occupied hill. A fixed anchor is not a substitute for drawing every underside the camera can expose. Join occupied chunks with matched left/right elevations and lower continuation sockets; cap an exposed end with an eroded slope or cliff, never a rectangle. Preserve the open valleys between groups all the way through the composition; deeper cliff pieces must not reconnect everything into the rejected full-width Mid wall. Far owns the view through those valleys. Do not stretch rows, repeat mirrored sediment, move hills with the player, or use a sudden fade to hide an unfinished bottom. Calibrate the proposed Mid anchor and asset extent against ground, first-third ascent and high-flight views before adopting a new vertical projection; the existing .03-response renderer has not implemented this new modular contract. Review the actual upper/cliff/continuation assembly from ground and elevated viewpoints before installation. `Art/Candidates/WastesMidgroundModules/2026-09-05/README.md` records the upper-silhouette-only concept and its rejected export properties.

Any renderer revision must test combined diagonal flight through repeat boundaries in both directions, not only horizontal panning plus separate fixed-height screenshots. Validate submitted geometry through the actual surface batch transform at 1080p, 1440p and the user's windowed viewport. Source-sized sprites alone do not prove correct screen placement. Visual approval, terrain occlusion, authored joins, routing/fades and multiplayer remain separate evidence gates.

**Layered-painting foreground follow-up:** Close also uses joined landscape sections and open valleys, but its sections are substantially longer than the smaller Mid hills. Stagger the authored composition so normal ground-height gaps often reveal Mid terrain/ruins. Some overlap is welcome and unavoidable as independent parallax rates shift the layers; never move a chunk dynamically to avoid another chunk. Close retains its fixed world-ground anchor and drawn lower extent. The review layout in `Art/Candidates/WastesMidgroundModules/2026-09-05/Deep-v1/` uses a1448px near bank against391–576px Mid groups. These dimensions illustrate the requested scale relationship, not a final generation distribution. Current candidate art remains uninstalled pending approval and foreground depth completion.

### Modular landscape QA continuation

The user authorized the four-design set to proceed into depth completion and disposable-world render checks. A1448×1915 Close bank now supplies829 fresh native lower rows; original upper830 rows remain unchanged. Three1408px Mid groups and the longer Close bank use stable spaced layouts and no reflected/stretched lower fill. Only the existing Far retains its provisional coverage guard and remains too banded. Current runtime allocation is28.39MiB raw RGBA, not a GPU profiling result. Native screenshots and explicit partial gates are in `Art/Validation/WastesModularLandscape-2026-09-05/README.md`. General-world promotion and the visible cave handoff remain unapproved; no Maw/faction dependent work advances from these static or bounded camera passes.

### Latest background review constraint

**Latest partial acceptance:** the gas-station module's repaired tree silhouettes
and appearance are accepted; preserve that exact artwork. The foreground still
fails for pale grass/cliff cutout rims even though its timber was cleaned. Inspect
the whole perimeter and retain deliberate material highlights while removing
unwanted pale outlining; do not hide leakage by changing the layer behind it.

**Transparency workflow correction:** generated checkerboard RGB is never an
acceptable transparency deliverable. Separate authored color from a reviewed
cutout mask and validate the exported alpha before reviewing it as a usable
layer. Preserve fine connections and intentional pale materials; a global gray
key or darker scenery behind a fringe is not the solution. The v2 foreground
color-only repair and failed-alpha Far concept are recorded separately in their
candidate READMEs; neither is installed or live-approved. The reusable hard-mask
exporter now has actual CLI regression proof; the city has a separate masked
candidate with bounded edge-color preparation. See Tools/BackgroundMaskWorkflow.md.
Actual runtime dimensions and in-game/art approval remain separate gates.

**Native city fitting update:** the masked city now has an optional1458x1792
QA-only runtime derivative, native-pixel edge joins and extended source-ground
texture, not enlarged detail. Station and foreground-v2 hashes are preserved.
See `Art/Validation/WastesCityRuntime-2026-09-05/README.md`; geometry passes are
not artistic or Space-handoff approval.

**Latest altitude/scene direction:** all terrestrial scenery must eventually
leave the view during ascent. The ruined city must not persist as the Space
background. Close remains world-ground locked and exits by ordinary camera
movement; Mid yields next; Far must hand off to the native sky/space layer too.
Use the version-pinned conventions in `RESEARCH_BACKGROUND_ALTITUDE_HANDOFF.md`. Preserve
night/eclipse geometry and restoration fades; do not use an abrupt cutoff or
change land anchors to follow the player. This does not alter descending cave
handoff requirements.

**Space-handoff implementation (2026-09-06, QA scope):** a shared, stateless
land-opacity envelope multiplies the entire submitted color, not just sky alpha.
Far stays visible through 70% of the global ground-to-Space ascent, then fades
smoothly to zero by the installed engine's integer player-center Space boundary.
The higher of player and camera altitude controls that guarantee. The 70% onset
is Apogean presentation policy, not a universal Terraria rule. Close keeps its
regional world-ground anchoring; Mid keeps its earlier camera-altitude staging.
There is no new below-ground cutoff and no world/ecology mutation. Native
presence/absence, gradual return and independent geometry evidence live in
`Art/Validation/WastesSpaceHandoff-2026-09-06/README.md`. This fixes the tested
Space defect, not every production background/art/transition gate.

**Additional Mid artifacts, 2026-09-06 user update:** the user likes the original
broken-building concept and requests two or three ruins to reduce repetition.
Use three total: retain that two-storey broken shell; propose a low motor depot
and a narrow checkpoint/communications ruin. They are Kessler-history scenery,
not new traversable buildings. Stable placements alternate distinct silhouettes
with quiet terrain/gaps; no camera/time-driven random reshuffling. Preserve
Station, Highway, Far city and current Close artwork. Concept approval does not
approve the original's baked checkerboard, runtime dimensions or camera coverage.
Review separate masks and native-scale assemblies before installation; never
enlarge a cropped Far building or turn Mid into a solid city wall. This bounded
two-concept addition supersedes the earlier extra-ruins deferral, not the other
baseline gates or the starting-area/Maw order.

**Mid scale follow-up:** basic ruin shapes accepted; one opposite-side checkpoint
redraw preserves scene lighting and the old cliff, rather than globally mirroring.
The user needed real character scale. Native ground-only pairs now exist in
`Art/Validation/WastesMidScale-2026-09-06/README.md`. Explicit2/5 upper studies
are not approved1408px-deep modules. Size/facing review, foundations and full
parallax/routing proof still precede integration. Accepted Station remains intact.

**Mid material-style correction:** the user likes the general size/look but
finds the ruins too realistic and finely textured beside Station. Station is the
fixed pixel-language reference: readable color clusters, grouped damage, clearer
planes and restrained shading. More canvas pixels or smaller realistic detail
are not improvements. Retain building scale and identities; revise one depot
first, review it, then transfer the accepted language to shell/checkpoint. Keep
Station unchanged. `Art/Candidates/WastesMidRuins-v2/PixelStyle-v1/` is the
offline probe, not a new native render or complete deep module.

**Clarification after v1 review:** gritty is desirable. V1's smooth beige slabs
and neat rock faces lost the ruined-world character. Match Station's weathered
pixel patchwork: chipped paint, dirty mortar, soot, rust and broken materials.
Do not equate more pixel-art with cleaner or emptier surfaces. The v2 depot
material/grid study is review-only; a fixed grid alone cannot approve style.

**Depot component preference:** between those depot versions, retain v1's
cleaner building as the working choice. The grittier v2 treatment suits terrain
better than architecture. Terrain may have denser, rougher texture while building
surfaces remain easier to read. This is a relative preference, not confirmation
that either depot matches Station's pixel style. Keep Station unchanged; stop
whole-building redraws. `ComponentAssembly-v1` now combines unchanged v1 upper
pixels with v2's rougher ground at the footing. This remains a short ground-scale
review candidate, not automatic art approval or a full-height Mid module.

**Latest Station/garage correction:** the user likes the garage's design and
cleaner contour, but Station's broader/fewer pixel clusters. Station's cutout is
now criticized; its earlier blanket reference status is superseded. Keep both
originals intact, not aesthetically frozen. Aim for an in-between: one Station
redraw may bridge the two without another garage-redraw loop. This is about
pixel grain AND edge quality, not just adding/removing grit. The previous native
pair is unaccepted for cohesion, not a rejection of the garage itself.
The user subsequently selected the middle `WastesStationBridge-v1` candidate.
Preserve that exact approved upper artwork; do not restart its style cycle.
Its unchanged512x460 derivative now has a clean build and bounded native
daylight ground-gallery evidence beside the garage/shell/checkpoint. This is
not the full1408px Mid module. Author compatible deeper terrain before ordinary
placement and full flight/lighting/routing proof. See
`Art/Validation/WastesStationBridge-2026-09-06/README.md`.

**Latest two-top correction:** native ground evidence did not mean the user
accepted checkpoint/broken-shell style. They stopped combined depth work on
that mismatch. Freeze the selected Station and garage; revise only those other
tops, compare all four at intended scale, and obtain style review before any
installation. `Art/Candidates/WastesRuinUpperStyle-v1/README.md` records the
corrected pair, now selected by the user's “looks good to me.” All four upper
designs are fixed. `WastesMidDepth-v2` assembles their exact architecture through
study soil row340 into512x1408 QA modules (100px downward translation); only lower
footing/geology changes. Sparse stable placement alternates five landmarks with
quiet hills and genuine gaps. Native test evidence remains separate from style
acceptance; do not reopen approved building design or promote normal worlds from
static masks/build success. A failed whole-image depth experiment is retained separately,
not approved: generated lower extensions changed upper art and framing, while
the reuse probe exposed joins. Short masked studies are not full-depth modules.

**September7 evidence, not a new design decision:** the exact four-building bank
passes unforced local greenification, native spray fades and Jungle priority /
return in the disposable world. Native Forest is still the accepted threshold
fallback, not new reclaimed-city artwork. Keep the approved building art fixed;
visible cave handoff, performance and remaining production checks are separate.
See `Art/Validation/ForestRestoration/2026-09-07/README.md`.

**Requested Far revision, concept first:** replace the dominant repetitive gray
rock-wall impression with cracked dusty earth and a demolished city/industrial
horizon: collapsed building shells, broken urban infrastructure and quiet rubble
intervals. Keep distance darker and lower-contrast, using soot, dusty umber and
smoky gray related to the ochre roadside foreground/midground. Preserve the
approved highway/station/tree language and terrain-locked layer layout. Palette
cohesion must retain depth rather than making all layers equally bright or brown.
Restoration changes ecology, not the ruined city's history. This is a direction
for a reviewable composition, not approval of unmade runtime art.

**Subsequent user-approved scope:** close the functional/visual baseline in one bounded corrective pass, then proceed to the starting-area slice. Actual regional anchoring, clean connected cutouts and gap-free movement/transitions remain acceptance requirements. Extra long-bank variants and final composition detail are deferred polish; the variation requirement below remains a release target, not a blocker for the playable baseline. See AUTHORING_WORKFLOW.md, Bounded background correction, for the stopping rule.

**Evidence update (2026-09-05, not a new art decision):** the exact Station/Close edge repair has native QA screenshots and passing geometry across eight camera/lighting cases, including both physical diagonal sweeps. See `Art/Validation/WastesExactCutouts-2026-09-05/README.md`. This does not accept the overall composition, real-biome/restoration transitions, cave handoff or general-world deployment. Preserve the existing design requirements and defer additional variants rather than restarting an art-generation loop.

The current modular Wastes iteration requires revision: pale cutout rims and disconnected gas-station tree branches must not ship. Hard alpha alone is insufficient; preserve connected bark silhouettes and inspect light/dark backings. One repeated long foreground bank is insufficient variation; author genuinely different long-bank and quiet sections, not mirrored duplicates. The nominal near soil lip should sit roughly three world tiles above **actual regional ground**, around a standing player's head. That is an elevation/offset requirement, never a instruction to follow the player's head during flight. Keep each section's ground datum stable while the player moves vertically; the old `worldSurface - 50` QA arithmetic is not a local-ground implementation. The camera tests pass their old global-anchor contract, not this visual requirement. No dependent biome promotion until revisions are reviewed.

## Future Boundary

The star chart, mobile ship, company war, CEO routes, post-Moon-Lord Deep Maw, and procedural completion content are deliberately roadmap items. Act 1 establishes their vocabulary without pretending to ship them early.
