# Tree contracts

## Texture roles

- Trunk: Terraria's framed trunk, roots, knots, and cut states. Confirm dimensions from the installed build.
- Branches: paired directional choices arranged as `ModTree.GetBranchTextures` expects. Stable tModLoader documents 40×40 branch textures.
- Tops: the choices returned by `ModTree.GetTopTextures`; stable defaults are 80×80. For a dead tree these are sparse wooden crowns, not leaf balls.
- Sapling: a frame-important tile with its own growth conditions and substrate mapping.

## Candidate record

Record these values beside the generator or source art:

- authoritative vanilla references and installed tModLoader version;
- permitted substrate tile IDs;
- trunk, branch, and top dimensions;
- expected tree height and branch-count ranges;
- maximum ordinary base width;
- palette and alpha policy;
- growth, chop, paint, drop, and multiplayer behavior;
- live fixture name and dated accepted screenshot.

## Rejection examples

- root flare floats or covers nearby ground;
- a chop merely reveals a smaller whole-tree sprite;
- top frames contain a broad canopy when the concept says leafless;
- bases overlap at ordinary world-generation spacing;
- every tree has the same branch heights;
- black one-pixel twigs disappear against night skies;
- a static validator passes while the in-game composed tree fails.

## Evidence gate

Acceptance requires authoritative references, deterministic source or generator, a static report, and a live grove screenshot. Keep candidates versioned until all four exist.

## Primary references

- tModLoader stable `ModTree`: https://docs.tmodloader.net/docs/stable/class_mod_tree.html
- ExampleMod stable tree: https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Plants/ExampleTree.cs
- ExampleMod stable sapling: https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Plants/ExampleSapling.cs
