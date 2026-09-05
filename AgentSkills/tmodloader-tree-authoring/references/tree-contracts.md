# Tree contracts

## Texture roles

- Trunk: Terraria's framed trunk, roots, knots, and cut states. Confirm dimensions from the installed build.
- Branches: paired directional choices arranged as `ModTree.GetBranchTextures` expects. Stable tModLoader documents 40×40 branch textures.
- Tops: the choices returned by `ModTree.GetTopTextures`; stable defaults are 80×80. For a dead tree these are sparse wooden crowns, not leaf balls.
- Sapling: a frame-important tile with its own growth conditions and substrate mapping.

### Top socket contract

Terraria rotates and sways a tree top around its bottom-center attachment. Every ordinary 80×80 top frame must therefore carry opaque trunk-width material to the final visible row, center that material on the frame's horizontal midpoint, and continue it upward deeply enough to overlap the trunk while swaying. A top that is attractive at zero wind but exposes sky between its crown and trunk at another wind frame fails.

Count connected wood from the bottom-center anchor, not just wide opaque rows: a transparent row severing an otherwise wide socket must fail. Test the caller's exit code too; an inner script's failure can otherwise be followed by a misleading outer PASS. For the currently verified ordinary renderer, left and right 40×40 branch frames use different pivots, `(40,24)` and `(0,30)`. Preserve that six-pixel difference when deriving paired art; a naive horizontal mirror is insufficient. Verify these values against the installed renderer when changing versions.

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
- top socket is wider than the trunk, off center, too shallow, or visibly detaches during wind sway;
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
