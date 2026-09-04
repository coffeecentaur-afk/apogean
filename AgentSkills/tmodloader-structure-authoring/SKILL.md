---
name: tmodloader-structure-authoring
description: Design, author, place, or validate tModLoader buildings, laboratories, faction bases, arenas, ruins, furniture sets, and authored world-generation templates. Use when structures float, cut terrain poorly, lose furniture, repeat placeholder blocks, collide with landmarks, or need Terraria-scale architectural polish.
---

# tModLoader Structure Authoring

Treat a structure as a authored Terraria location, not a rectangle filled with custom blocks. Architecture, material atlases, furniture, terrain integration, world placement, protection, and progression access are separate contracts.

## Workflow

1. Read the world planner, placement pass order, protected regions, template parser, terrain integration, tile/wall/furniture classes, and current blueprint.
2. Establish the placement envelope and collision policy before drawing: world sizes, legal layers/biomes, fallback scaling, protected landmarks, mod compatibility, and entrance surface. Use a finite candidate budget, call `GenVars.structures.CanPlace` using the final padded bounds, and protect the accepted footprint immediately.
3. Draw a cutaway silhouette at Terraria tile scale. Identify navigation, room purpose, sightlines, gates, arena space, and destruction state.
4. Finish one material/furniture family through its native atlas and live gallery before using it across a campus. Use `$tmodloader-atlas-authoring` for connected materials.
5. Author a fixed template when art direction matters. Use procedural generation only for controlled variation inside the validated envelope.
6. Validate every command and object footprint. Write shell tiles and walls, frame the shell, then place frame-important furniture through `WorldGen.PlaceObject`. Run `scripts/Test-FurnitureSheet.ps1` for each sheet.
7. Place the template through the same path world generation uses. Inspect terrain contact, entrance reachability, furniture survival, wiring/traps, protection, map visibility, biome counts, and multiplayer sync.
8. Test smallest legal world, cramped legal placement, biome boundary, and compatibility world. Promote only when both a focused construction gallery and a fresh-world placement pass.

## Structure contracts

- Surface buildings meet terrain through a designed foundation and restored shoulders; empty clearance margins are a generation defect.
- Clearing occurs only where authored air is required. A bounding-box clear is not terrain integration.
- Every room has a gameplay or storytelling purpose. Large unbroken wall fields need structure, lighting, or depth.
- Use block, trim, floor, beam, glass, wall, platform, and furniture roles intentionally. Recoloring one atlas into every role is a prototype only.
- Furniture placement uses `TileObjectData` and final anchors. Frame the shell before placing frame-important objects.
- Locked buildings communicate access without becoming invisible. Protection changes only at an explicit progression state.
- Fixed templates may scale or choose variants by world size; never distort tiles.
- Placement logs record candidate rejection and the chosen bounds.
- Persist `placed`, `fallback`, or `skipped` plus the chosen bounds/template version. If a critical landmark skips, progression needs an explicit safe fallback.

Read [structure-contracts.md](references/structure-contracts.md) before adding a new structure family.
