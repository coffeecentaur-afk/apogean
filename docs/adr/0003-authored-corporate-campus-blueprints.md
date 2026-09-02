# Corporate Campuses are authored whole-building blueprints

Kessler, Helix, and Sentrix Campuses use complete, immutable, versioned blueprints placed inside locations selected by the world atlas. The atlas owns safety and location while each blueprint owns silhouette, rooms, walls, furniture, semantic entrance, and faction identity; terrain blending may remain bounded and procedural, and repeatable ruins may later use Remnants-style authored room modules. This follows Calamity's complete-laboratory model because permanent story headquarters must look deliberate and remain recognizable across seeds, while avoiding the brittle procedural rectangle shells used during blockout.

## Consequences

- Runtime worldgen may place a Campus by blueprint identifier but may not reconstruct its rooms with ad hoc tile loops.
- Blueprint dimensions must fit the compact atlas envelope; larger reservations add breathing room rather than stretching the building.
- Interactive objects, chests, doors, NPC anchors, and later tile entities are semantic blueprint markers finalized after structural cells.
- Building mirroring is forbidden until frame-, slope-, platform-, and multitile-safe transformation exists; deliberately authored variants are preferred.
