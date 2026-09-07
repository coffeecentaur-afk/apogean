# Act 1 Delivery Roadmap

## Shipped foundation

- Faction relation scaffold, sealed compounds, dialogue UI, and a playable Matriarch prototype.
- Renderer-gated Wastes material pipeline: disposable tile, grass, vegetation, terrain-family, and production-property galleries; local native-atlas export; cold client capture; and validated Soil, Grass, Stone, Sand, Ice, Snow, Mud, unsafe-wall, item, falling-projectile, and ground-cover families.
- The Wastes tree implementation uses native segmentation and chopping with no whole-tree/root-flare overlay. Rejected terminal-fork art has been replaced by the user's approved **A — Snapped v3**, including the requested thicker branches. It is installed at **fixture-pass**, with bounded live proof and explicit remaining production checks in `Art/Validation/WastesSnappedA-v3/README.md`.
- Renderer-gated Maw conversion pipeline: native-topology hostile Soil, Grass, Stone, Sand, Ice, Snow, Mud, and Clay families; custom drops and sand behavior; unsafe-wall conversion; and a live four-stage natural source → Maw → Wastes → vanilla purification matrix. The live matrix covers neutral Wastes terrain plus representative Corruption, Crimson, Hallow, jungle, mushroom, and Underworld sources while proving constructed vanilla and unknown modded content are preserved.
- An allow-listed file-request bridge now drives destructive render fixtures through the running single-player client without depending on synthetic game input. It consumes only named Apogean validation fixtures from the Terraria Captures directory and retains the same runtime assertions and capture-camera path as chat commands.
- Wastes Forest V0 composition candidate: a transparent far/middle/close parallax decomposition carrying a broadcast spire, ruined skyline, broken highway, settlement remains, and rooted foreground basin. It is useful composition and renderer evidence, not approved production art.
- Wastes Desert V0 composition candidate: an authored far/middle/close decomposition carrying satellite-crowned mesas, eroded industrial skyline, broken transit, train wreckage, hangar, tank, pipelines, and near debris. The reusable render lab can select it deterministically, but it still needs the full production camera and transition matrix.
- Wastes Jungle V0 composition candidate: a far/middle/close decomposition carrying an overgrown research complex, cracked greenhouse domes, specimen towers, elevated laboratory links, derailed transit, and dark vegetation banks. Real-biome routing and the complete altitude/pan matrix remain promotion blockers.
- Wastes Snow V0 composition candidate: an icy far/middle/close decomposition carrying frozen mountains, damaged wind machinery, antenna and control towers, a half-buried bunker, exposed pipelines, amber lamps, and sparse dead growth. Its flat diagnostic lower fill was replaced, but final authored vertical coverage is still required.
- Surface renderer V0 benchmark: Forest, Desert, Jungle, Snow, Corruption, Crimson, Hallow, Ocean, and Glowing Mushroom have 27 hard-alpha diagnostic layers with independent parallax, equal outer repeat columns, transparent Terraria sky, and diagnostic lower fill. The library totals 162.01 MiB raw RGBA and no three-layer family exceeds 32 MiB. Earlier 1440p noon/night/eclipse and Mushroom transparency samples are retained, but the later V1 measurement disproved the assumption that scale-1 drawing necessarily gives 1:1 screen pixels. Ordinary styles already use V0; this does not promote its unresolved art, coverage, scaling, alternate-composition or performance gates.
- Validation panoramas explicitly resolve the same global ruined-background slot used by ordinary play and sanitize invalid ModBiome water-style values before invoking Terraria's capture renderer. Live and panorama captures now agree without reintroducing the prior liquid-array crash.
- First approved Wastes forest-depth set: four native-sized cave textures with an opaque, wrap-safe eroded-strata material, distinct shallow/deep palettes, and an in-engine lighting proof. Unique ruined mine landmarks are reserved for sparse world furniture so they do not repeat every 128 pixels.
- First approved Ruined Deep Underworld panorama: an opaque refinery horizon plus hard-alpha broken-span and slag/rail overlays, composited into Terraria's final custom-sky depth band so it replaces all five vanilla Hell layers while remaining behind tiles, lava, entities, and UI. The first live art pass exposed residual checker pixels and amber visual noise; the corrected extraction passed the second live render and all static gates.
- First approved Kessler construction slice: native Gray Brick topology recolored into five distinct gunmetal/burnt-red structural materials, two wall fields, a complete room furniture family, warm service lighting, animated power-armour racks, and an animated shield-and-chevron war standard. A native-placement fixture exposed and fixed the shared tileNoAttach defect that had made every corporate room reject furniture.
- First approved Kessler Campus production slice: a compact 152x72 authored compound now occupies only part of its larger event reservation. Two-stage checkpoint towers, legal passages, a west-side quartermaster frontage, a separate sealed armory bulkhead, stepped logistics/operations/command volumes, a dark-backed platform shaft, native furniture, animated armour racks and standards, and a terrain-keyed foundation replace the rejected giant shell. The shared template loader now resolves and frames structural anchors before placing every registered multi-tile object natively.
- Fresh-world Kessler placement gate: the authored 54-tile surface datum, actual 152-tile footprint, 28-tile terrain-relief ceiling, and Wastes-before-compounds pass order replace the rejected floating placement. Seed `ApogeeCampusQA-2026-09-03` / atlas hash `9DBCB5C1` passes save/load inspection with 152/152 supported columns, 3,588 underlying Wastes cells, complete native furniture counts, an unobstructed public approach, and a sealed/open/re-armed bulkhead cycle.
- First playable Kessler arrival loop: Wall of Flesh signals the impact, the next observed dawn starts a ten-target live-fire assessment, Survey Drones escalate into Reclaimers, and success unseals the public compound, grants five Kessler Scrip to each active player, and stations Quartermaster Mara Venn. Her requisition shop and custom branching briefing both pass client checks. Attacks use narrow dashed acquisition lines, no body-contact damage, and a QA-world-only deterministic harness; bespoke combatant and Quartermaster sprites remain an explicit art gate.
- First dedicated Kessler multiplayer authority pass: Campus and bulkhead rectangles travel through tModLoader world data, progression tile changes and invasion quota updates are server-owned, assessment NPCs deactivate only on server authority, and Mara's authored Campus anchor is corrected on the server and replicated. A real local dedicated server/client run joined the 8400x2400 QA world, received complete tile data, displayed the synchronized ten-target audit HUD, reached the contactable state, and produced no new client or server exceptions.

## Current environmental gate

**Current:** user likes the garage's cleaner outline/design and Station's broader
pixel clusters, but now rejects Station's rough cutout. Earlier immutable-Station
direction is qualified. One new Station bridge is in
`Art/Candidates/WastesStationBridge-v1/`: inspected hard-alpha extraction and
comparable-size offline boards only. Current runtime art is unchanged. Pair
cohesion remains unaccepted; garage itself is liked. Review the one candidate,
then deep foundations/native checks if selected. No extra garage redraw loop.

**Earlier user review:** overall native ruin scale is not the requested change;
their fine realistic texture is. Station remains the approved pixel-art anchor.
One motor-depot redraw is prepared in `Art/Candidates/WastesMidRuins-v2/PixelStyle-v1/`.
Review its grouped shading at the existing size before revising the other two.
It has offline mask/export/fitting proof only. The earlier native technical
evidence remains valid for the old sprites, not this candidate. No production
promotion or extra biome work; then resume foundations and the existing order.

Latest follow-up supersedes the blanket v1 rejection: user prefers the cleaner
v1 BUILDING over gritty v2, while v2's rougher treatment suits the TERRAIN.
Keep v1 as the working architectural choice; the Station style mismatch remains
acknowledged. No new redraw or blanket art approval. `ComponentAssembly-v1` joins
those exact pixels for its own native ground review. Deep foundations and normal
placement remain gated; do not repeatedly retexture the whole building.

**Latest checkpoint:** three ruin upper studies now have native ground-scale
captures beside gg and the unchanged Station, after the user could not judge the
offline comparison. Checkpoint faces the opposite way without reversing lighting
or the old cliff. `Art/Validation/WastesMidScale-2026-09-06/README.md` records
exact QA build, source sampling and bounds. Await size/facing verdict, then deep
foundations and stable quiet-spaced placement. This is not full-background
approval, flight coverage or ordinary-world promotion. Keep Wastes → starting
area/drop pod → shallow Maw order; Jungle remains lush/overgrown/abandoned.

**Current handoff:** `HANDOFF_2026-09-06.md` is the next-session entry point.
Space-handoff implementation now has focused red/green arithmetic and native
submitted-color proof; see `Art/Validation/WastesSpaceHandoff-2026-09-06/README.md`.
**Latest user direction (2026-09-06):** retain the first Mid ruin's liked visual
direction and prepare two distinct alternatives (low motor depot and narrow
checkpoint), making three ruins total. The original remains technically blocked
by its opaque checkerboard and missing native-scale assembly; liking its look
does not install it. Candidate family: `Art/Candidates/WastesMidRuins-v2/`.
`BACKGROUND_BIOME_DIRECTION.md` records the architecture map and the green,
overgrown, abandoned Jungle exception. No other biome is recolored or promoted
by this design pass. Finish Wastes baseline, then starting area and shallow Maw.

**Latest candidate checkpoint:** native city fitting and foreground perimeter-v2
now have a zero-warning isolated build and2560x1369 live evidence in
`Art/Validation/WastesCityRuntime-2026-09-05/README.md`. Station remains byte-identical.
The original opaque generated city is still rejected; its explicit-mask derivative
is fitted to1458x1792 without enlarging or mirroring it. Nine live geometry cases,
including both4.853-period Far sweeps, pass. The user-identified city-in-Space
defect is corrected in the subsequent QA build. **Full visual gate remains open:**
art review, actual-biome/restoration/cave transitions and production proof are
separate from the focused Space fix.
The requested separate Mid broken-building module is concept-first follow-up,
not a replacement for the gas station or authorization for another solid Mid wall.
No ordinary-world art was promoted; the installed package is a QA override.

**Latest visual verdict:** gas station accepted; foreground still rejected for its
pale perimeter. Freeze the exact Station repair; do not repeat its art cycle.
The earlier timber-only correction and upper440-row probe missed wider cliff
edges. Full-perimeter evidence and the requested cracked-earth/demolished-city
Far direction are recorded in `Art/Validation/WastesExactCutouts-2026-09-05/FOREGROUND_REVIEW.md`.
Keep foreground parked pending a bounded material-aware correction and review.
The Far palette/composition study must be shown before replacement; matching
colors is not a substitute for clean edges. Existing camera evidence stays valid.

**Biome-scope clarification:** current Wastes scenery is not a universal-world backdrop. Finish its bounded baseline before the next background family, the Maw; other surface and depth-specific families retain their distinct identities and quality targets (DESIGN_BIBLE.md, Environmental history). The failed whole-image repairs remain parked. The user-approved exact pixel repair now has a clean temporary-mirror build and native2560x1369 proof across eight camera/lighting cases, including both full diagonal sweeps; see `Art/Validation/WastesExactCutouts-2026-09-05/README.md`. The installed QA package contains the exact candidates, not the repository Content files. User art review, real-biome/restoration transitions and general-world promotion remain open. Reusable lessons live in the installed/Git-mirrored background skill, not only this candidate. No new artwork was generated during this bounded pass.

**Latest user decision (2026-09-05):** replace the open-ended background-polish milestone with the bounded baseline correction in AUTHORING_WORKFLOW.md. Finish terrain anchoring, clean/connected cutouts and movement/transition checks, then review native game screenshots. More variants and final detailing are deferred. If the pass fails, simplify or park the failing layer rather than looping through new art. Historical matrix and rejected evidence below remain valid; none are promoted by this scope decision.

**Next starting-area addition, specified only:** the approved unbranded damaged drop pod in a shallow divot, as recoverable/placeable world furniture, with safe spawn clearance and a broken communications panel. Its repaired relay, housing-based NPC signal coverage, additional relays and ship hologram purchasing are later work; see DESIGN_BIBLE.md, Spawn sanctuary. Do not implement them during the background correction.

**Current recovery order, approved after the landscape checkpoint:** prove validator failures first; show the A — Snapped native tree assembly for approval before loading it; close grass corners/ground cover; finish one Wastes background family; validate the generated starting area; then begin the Maw entrance/shallow gameplay slice. Other new content is paused. Historical landscape-first notes below describe the preceding checkpoint, not permission to bypass this order. The user accepted v2 subject to thicker branches; **v3 is now fixture-pass and installed**, with one extra native pixel per branch side and unchanged quiet bark/hollows. See `Art/Validation/WastesSnappedA-v3/README.md` for the clean build, native growth/middle-cut proof and ordinary-wind Windows screenshots. Complete the remaining tree production matrix before calling it integrated or polished, then continue the agreed ground-cover/background order. Offline review art remains under `Art/Candidates/WastesSnappedA-v3/`.

**2026-09-04 user priority override:** landscape concepts and scoped Dynamic Parallax implementation come first in this pass. Five review-only Wastes/restored/Maw surface/cavern concepts are saved in `Art/Reference/Backgrounds/2026-09-04/`. The local forest-restoration threshold fallback passes build, 25 policy checks, the unforced live Wastes → mixed → green → mixed → Wastes sequence, and Jungle priority after restoration. Seven captured PNGs and telemetry are in `Art/Validation/ForestRestoration/2026-09-04/`; the actual client viewport was 2560×1369. A failing fixture exposed leftover terrain in the scene scan, fixed through QA isolation without changing production thresholds. Full solution/flight/lighting/viewport/priority coverage and visual acceptance remain pending. Continuous proportional art blending, Maw dormancy glow masks, and production layer extraction remain pending. Resume the A — Snapped tree gate below after this bounded landscape proof.

> Current status is evidence-gated. Tree and background visual candidates are not approved merely because their engine paths compile.

**Landscape approval follow-up:** composition and deterministic local processing are approved. The malformed first export remains rejected, while four new actual 2172×724 source images now produce three unscaled 2048×1280 Wastes V1 layers with hard alpha and separately authored lower strata. The 30 MiB family is installed only in `Apogee Native Visual V3` and the Forest render lab. Live tests found and corrected forced 1440p zoom, abrupt night tint and a buried horizon. Current 1080p and 1440p ground/jump/wings/space, left/right and noon/sunset/night/rain/eclipse samples are recorded; unforced Wastes → native green forest → Jungle routing also passes at both sizes. The user's newly generated world was not modified. See `Art/Validation/WastesLandscapeV1/README.md` for evidence, source-scale regression and exact limitations. Continuous repeat travel, actual solution conversion, adjacent fades/scene priorities, flat lower-band art, residency/hitches and user visual acceptance remain open. Do not promote this candidate to ordinary worlds until those gates close; then resume A — Snapped. No paid API fallback was used.

1. A — Snapped v3 has the user's art condition fulfilled, native exports and clean builds. Extended live proof covers sampled ±40 mph wind, native wood drops and three whole-prop removals. `Art/Validation/WastesSnappedA-v3/Persistence/README.md` adds painted side branches, accelerated production-sapling growth on flat/terraced ground, blocked/sloped/close-neighbor rejection, and the same saved-grove digest after reload without rebuilding. Finish night readability, manual axe/shake/Acorn input, elapsed-time growth, multiplayer and fresh-world distribution. B/C remain excluded; broken-tree frequency is unchanged. Maw trees remain a separate design family.
2. Production grass-corner fixture re-run: no white gaps in the bounded vanilla-control comparison. Coating checks reproduced and fixed rigid brush ignoring paint/Echo; native painted atlas cells now share one rigid transform, preserving the existing shape and no-split behavior. Midnight illuminant and actuation samples pass. See `Art/Validation/WastesCoatings-2026-09-04/README.md`. **User accepted the displayed grass/soil connection and continuation to backgrounds.** The next active family is Wastes V1 backgrounds; actual terrain interaction, partial coatings, wider merges and multiplayer remain separate open checks.
3. Wastes background continuation is paused for the user's next visual session: the full-world pan and real PureSpray restoration/fade harness compile but have not run in-world. Resume from `Art/Validation/WastesLandscapeV1/README.md` (2026-09-04 overnight checkpoint); do not repeat the accepted grass pass. Promote the native-detail surface benchmark only after authored vertical coverage replaces final-row stretching, biome-boundary crossfades, original alternate compositions, complete ground/aerial/pan/day/night/weather/transition routing captures, repeated hitch checks, and texture-residency profiling pass. Then expand the underground Forest V0 and Ruined Deep contracts into the remaining underground biome matrix one bounded composition at a time.
4. Preserve the proven dedicated multiplayer contract for the compact Kessler Campus and arrival assessment. Joining clients receive Campus/door coordinates through world data, while quota transitions, NPC retirement, Quartermaster anchoring, and bulkhead mutation remain server-authoritative. Next Kessler work may tune combat and replace sprites, but cannot move shared progression ownership to clients.

Machine-readable evidence and blockers live in `Tools/AuthoringStatus.json`. `Tools/Invoke-ApogeanContentGate.ps1` runs the corresponding static contracts; no script is allowed to promote a family without the named live fixture and visual review required by `AUTHORING_WORKFLOW.md`.

Community background research is recorded in `RESEARCH_DYNAMIC_PARALLAX.md`. The user subsequently approved landscape-first work and local restoration-reactive scenery; the source implementation, bounded live proof and remaining matrix are recorded in `FOREST_RESTORATION_VALIDATION.md`. Ordinary surface styles already route supported biomes through the HD V0 compositor despite older diagnostic-only prose. That routing is not a visual promotion; existing coverage, seam and art-quality gates remain open.

## Current flight correction checkpoint — 2026-09-05

User feedback reopened the Wastes V1 camera/art gate: excessive vertical movement, left-side repeat cutoff during diagonal flight, over-frequent roadside landmarks, and sediment repeated through multiple depths. The live red diagonal fixture reproduced a425.67px maximum left gap. The ground-locked, altitude-staged correction now passes1080p/1440p submitted-geometry sweeps and five altitude holds; shallow descent visibly retains Close. See `Art/Validation/WastesLandscapeV1/2026-09-05-Flight/README.md`. Do not claim the terrain-obscured deeper capture proves cave handoff, or the later changed QA grove checkpoint proves persistence.

Next within this same family: preview **modular transparent midground** hill/ruin groups, replacing the uninterrupted soil wall. Include deeper cliff/sloped undersides and compatible continuation pieces, with fixed region-height anchors validated at ground and elevated viewpoints. Height locking cannot excuse an exposed flat cut. Keep gaps open beside/between the deeper pieces. The upper-silhouette concept in `Art/Candidates/WastesMidgroundModules/2026-09-05/` is not approved or engine-ready (baked checkerboard; shallow bottoms). After art approval, contract and implement stable placement, real through-gaps, compatible joined groups, Far-owned coverage and restoration variants. No background asset promotion or dependent biome/faction work until this gate passes. Accepted tree art stays unchanged; the separate QA checkpoint mismatch needs diagnosis, not an automatic rebuild.

Next art pass: original quiet/open roadside variants between major landmarks; detailed sediment concentrated in the nearest ledge; quieter overlapping middle ledges with exposed pipes/rock; preserve drooping dead grass. Show candidates before installation. Do not expand to Maw backgrounds while this family's visual gate is open.

Modular follow-up: `Art/Candidates/WastesMidgroundModules/2026-09-05/Deep-v1/README.md` retains four review designs (three Mid hills plus a longer Close bank), ten offline compositions, exact upper/lower reconstruction and six negative CLI controls. The user requests long joined foreground sections staggered against Mid, allowing natural overlaps. No runtime PNG changed. The new Close bank needs an authored lower continuation before1440p/descent; quieter Far art and visual approval remain open. Pause additional image generation for this set at the review boundary.

Latest modular continuation: user said go after the four-design composition checkpoint. Foreground now has an authored1915px depth; new Mid/Close candidates are staged only in the existing QA-world/Forest-lab path. Static asset/socket/hash and camera gates pass, build clean, with original live captures under `Art/Validation/WastesModularLandscape-2026-09-05/`. Do not confuse the new shallow-descent coverage with native cave-handoff proof. Keep this family contracted pending quieter Far art, review, complete camera/routing/lighting matrix and performance. Additional landscape designs and dependent biomes remain out of this pass.

Background acceptance override from latest user inspection: the modular candidate needs clean cutout rims, connected station trees, genuine Close variants and an actual regional-ground anchor. These are failed visual requirements despite passing viewport-coverage arithmetic. Retain the bounded live evidence and red cutout/local-ground probes; fix this family before expanding into Maw/factions. Display settings were restored after the QA-only run.

## Vertical slices

1. **Engraft foundation** — biome identity, Maw Nodes/Ruptures, bounded conversion, three initial enemies, debug tools, and art direction.
2. **Pre-Hardmode loop** — materials, class-aware Engraft Harness, early weapons, Alpha Hunt, Nest Warden, and Matriarch rework.
3. **Kessler arrival** — next-dawn invasion, Quartermaster encounter/shop, scrip/clearance groundwork, and walkframe contract.
4. **Playtest polish** — multiplayer safety, tuning, accessibility settings, loot guarantees, and asset replacement.

## Act 2 frozen roadmap

- Helix harvest event after the mechanical bosses.
- Sentrix lockdown after Plantera.
- Moon Lord triggers the company war.
- Alliance and Independence Protocol both lead to a ship and the handcrafted star chart.
- The Deep Maw becomes the first post-war, interplanetary Broodmass threat.
