# Act 1 Delivery Roadmap

## Shipped foundation

- Faction relation scaffold, sealed compounds, dialogue UI, and a playable Matriarch prototype.
- Renderer-gated Wastes material pipeline: disposable tile, grass, vegetation, terrain-family, and production-property galleries; local native-atlas export; cold client capture; and validated Soil, Grass, Stone, Sand, Ice, Snow, Mud, unsafe-wall, item, falling-projectile, and ground-cover families.
- Reference-driven dead trees now use Terraria's native segmented trunk, branch, and crown renderer with varied heights and branch placement. The rejected whole-tree overlay is gone; a live mid-trunk chop assertion proves that the canopy falls while the lower stump and bounded root flare remain. Tree spacing, rigid ground cover, and grass anchoring pass build/static/live/capture checks.
- Renderer-gated Maw conversion pipeline: native-topology hostile Soil, Grass, Stone, Sand, Ice, Snow, Mud, and Clay families; custom drops and sand behavior; unsafe-wall conversion; and a live four-stage natural source → Maw → Wastes → vanilla purification matrix. The live matrix covers neutral Wastes terrain plus representative Corruption, Crimson, Hallow, jungle, mushroom, and Underworld sources while proving constructed vanilla and unknown modded content are preserved.
- An allow-listed file-request bridge now drives destructive render fixtures from the running single-player client without depending on synthetic game input. It consumes only named Apogean validation fixtures from the Terraria Captures directory and retains the same runtime assertions and capture-camera path as chat commands.
- First approved Wastes forest panorama: a native-sized, transparent far/middle/close parallax decomposition preserving the broadcast spire, ruined skyline, broken highway, settlement remains, and rooted foreground basin from the approved concept. It is live-rendered, seam-checked, and retained as seeded Forest composition V0.
- First approved Wastes desert panorama: an authored far/middle/close decomposition preserving satellite-crowned mesas, eroded industrial skyline, broken elevated transit, train wreckage, hangar, tank, pipelines, and near debris from the approved concept. Edge silhouettes taper into a shared valley so Terraria can repeat each independently scaled layer without a visible vertical cliff. The reusable surface render lab now selects Forest or Desert and forces a stable daytime palette check before capture.
- First approved Wastes jungle panorama: a restrained far/middle/close decomposition preserving the approved overgrown research complex, cracked greenhouse domes, specimen towers, elevated laboratory links, derailed transit, and dark vegetation banks. Atmospheric yellow-olive distance, tarnished midground architecture, and near-black growth remain readable as separate parallax depths in the live renderer.
- First approved Wastes snow panorama: an icy far/middle/close decomposition preserving frozen mountains, broken wind machinery, antenna and control towers, a half-buried industrial bunker, exposed pipelines, amber heat lamps, and sparse dead foreground growth. Its original flat lower safety fill was rejected in-engine and replaced with an irregular palette-stepped snow shelf before approval.
- Validation panoramas explicitly resolve the same global ruined-background slot used by ordinary play and sanitize invalid ModBiome water-style values before invoking Terraria's capture renderer. Live and panorama captures now agree without reintroducing the prior liquid-array crash.
- First approved Wastes underground-depth set: four native-sized cave textures with an opaque, wrap-safe eroded-strata material, distinct shallow/deep palettes, and an in-engine lighting proof. Unique ruined mine landmarks are reserved for sparse world furniture so they do not repeat every 128 pixels.
- First approved Ruined Deep Underworld panorama: an opaque refinery horizon plus hard-alpha broken-span and slag/rail overlays, composited in Terraria's final custom-sky depth band so it replaces all five vanilla Hell layers while remaining behind tiles, lava, entities, and UI. The first live art pass exposed residual generated checker pixels as amber visual noise; the corrected extraction passed the second live render and all static gates.
- First approved Kessler construction slice: native Gray Brick topology recolored into five distinct gunmetal/burnt-red structural materials, two wall fields, a complete room furniture family, warm service lighting, animated power-armour racks, and an animated shield-and-chevron war standard. A native-placement fixture exposed and fixed the shared tileNoAttach defect that had made every corporate room reject furniture.
- First approved Kessler Campus production slice: a compact 152x72 authored compound now occupies only part of its larger event reservation. Connected checkpoint towers, open public passages, a west-side quartermaster frontage, a separate sealed armory bulkhead, a two-deck headquarters, a command crown, native furniture, animated armour racks and standards, and a terrain-keyed foundation replace the rejected giant shell. The shared template loader now resolves and frames structural anchors before placing every registered multi-tile object natively.

## Current environmental gate

1. Continue confirming paint, save/load, and multiplayer synchronization properties in focused client fixtures; placement, framing, merge, slope, mining, light, liquid, drops, and native tree chopping have live proofs.
2. Expand the proven surface Forest/Desert/Jungle/Snow/Corruption/Crimson/Hallow/Ocean/Mushroom V0, underground Forest V0, and Ruined Deep Underworld contracts across the remaining underground biome matrix one bounded composition at a time.
3. Validate the compact Kessler Campus in a freshly generated campaign world, including terrain shoulders, public approach, sealed-to-open bulkhead transition, save/load, and multiplayer synchronization.

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
