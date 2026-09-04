# Structure contracts

## Placement record

Record world-size variants, dimensions, legal region, reserved margins, protected-landmark distances, pass order, fallback behavior, entrance bounds, surface baseline, and teardown/protection state.

## Architectural review

- silhouette reads from the map and gameplay camera;
- entrance is visible and reachable at its intended progression;
- rooms have distinct purposes and navigable scale;
- vertical travel includes platforms, doors, or lifts that work with Terraria movement;
- walls, trim, beams, windows, and lighting create depth rather than graph paper;
- furniture survives framing and reload;
- the foundation touches varied terrain without floating or burying the entrance;
- the structure does not consume dungeon, temple, oceans, spawn sanctuary, or another reserved landmark;
- destruction/protection state matches progression.

## Live matrix

Render a construction gallery first, then the complete authored template, then a fresh generated world. Validate reload, multiplayer host/client, map, paint, actuators where supported, mining protection, and post-unlock teardown.

## Inspiration rule

Study other mods and games for functional grammar—navigation rhythm, material hierarchy, encounter staging, silhouette, and environmental storytelling. Produce original layouts, assets, names, and encounter logic. Keep a short reference ledger naming the borrowed principle rather than copying an implementation.

## Engine and case-study references

- tModLoader `StructureMap`: https://docs.tmodloader.net/docs/stable/class_structure_map.html
- tModLoader `WorldGen`: https://docs.tmodloader.net/docs/stable/class_world_gen.html
- ExampleMod furniture atlas contract: a 2×1 workbench with `CoordinateHeights = [18]` uses a 36×20 sheet; frame height includes the final two-pixel padding row. https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Furniture/ExampleWorkbench.cs
- ExampleMod world-generation pass: https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/ExampleOre.cs
- Calamity public structure placement is an architecture case study only: https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/World/DraedonStructures.cs
- Remnants procedural macro-layout plus authored modules is a case study only: https://github.com/lazy-wombat/Remnants/blob/main/Content/World/Dungeons.cs
