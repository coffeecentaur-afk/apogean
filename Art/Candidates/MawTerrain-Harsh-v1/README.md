# Maw terrain: harsh material direction v1

Date: 2026-09-09. Status: **concept review pending, not installed**.

The user requested the artwork treatment across all Maw terrain, then clarified:
"the softer design is interesting but not what im looking for".
This board proposes sharp fractured material planes, torn fibers and restrained
amber rather than softer rounded shapes. That interpretation still needs review;
do not record the newly generated board as user-approved.

## Deliverable and provenance

- [Terrain review board](terrain-study.png), generated with the built-in image tool.
- [Exact generation prompt](prompt.txt).
- Image SHA256: `0993BAB7546E10EBE139418E906BB2DA31B1695070FA3F0CE04C963554E7413B`.
- Original retained under Codex generated_images as
  `exec-fbb6d0e4-7ff7-4c4f-8668-208fab46167e.png`.
- Original raster sources were inspected for context (DepthDirection-v1 concept
  and MaskedNative-v2 bone material); neither was an edit target or passed to this
  generation call. Existing assets remain unchanged.

## Coverage and implementation boundaries

| Board material | Existing owner / interpretation |
| --- | --- |
| Soil | MawDirt |
| Stone | Mawstone |
| Grass | MawGrass; related EngraftTurf/tuft treatment still needs its own export |
| Sand | MawSand, preserving falling and Sandgun behavior |
| Mud | MawMud |
| Clay | MawClay |
| Snow | MawSnow |
| Ice | MawIce |
| Bone | OssuaryBone material direction; not a new tooth collision shape |
| Fibers | Organic material motif; not a newly implemented block |
| Membrane | Localized deeper-Maw proposal; not a newly implemented block |
| Amber | Amber growth / node direction; not a new ore, recipe or emitted-light implementation |

The strips under the twelve patches are proposed darker wall treatments. Current
MawNaturalWalls contains seven family-specific unsafe walls, not twelve. The clay,
bone and other proposed wall strips are not evidence of those classes existing.
This board does not finish every conversion target, decorative growth, plant,
item, projectile, ore or depth-specific asset. Their engine-ready sheets remain
work to do after the style gate.

## Inspection

The image was visually inspected: twelve labeled families are present; stone and
ice have angular fractures, soil/mud are distinct, grass uses hanging fibers, and
the lower-contrast wall strips retain the corresponding motifs. Bone and membrane
remain organic rather than ordinary rock. There are no painted transparency grids.
The flat blue-grey review background is intentional and is NOT alpha.

Remaining risks: the board is enlarged concept art, not measured native 16px tile
detail. Some patches retain fine noise, and the large bone/fiber motifs could
repeat poorly if stamped into every tile. Snow's pale crust and ice's blue need
comparison against existing snow terrain at actual game scale. No seam, palette,
alpha, slope, blend, item-scale or native-render pass is claimed.

## Next bounded gate

1. User reviews this harder material direction.
2. Export one soil/stone/grass connected proof with actual native masks, including
   slope-to-dirt transitions and darker walls. Do not crop this board into an atlas.
3. Validate actual family topology, merge seams, native-scale light/dark views,
   mining/drops and purification. Only then extend accepted methods to the
   remaining substrate families and existing organic materials.

Use original color/material sources plus engine frame masks. The existing
New-MawTerrainFamily generator largely palette-remaps Wastes sources and writes
production assets; it was inspected but NOT run. Preserving its frame topology
does not require preserving its repeated material pattern. Grass and wall masks
must retain their own separate contracts. Ordinary soil, stone and supporting
bone remain non-emissive; amber organ emission is a separate gameplay contract.

No Content files, code, live package, saves, placement behavior, world generation,
acid, mobs or bosses changed in this art pass.
