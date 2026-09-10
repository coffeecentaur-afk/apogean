# Placeable Maw tooth cluster

## Latest: four-face attachment

User accepts the grouped appearance, wants floor/ceiling/both solid-wall
placement, and explicitly retains all three individual models for future
mixed distribution. Native-v1 is retained; Native-v2 keeps its upright pixels
identical, with three exact quarter-turn derivatives. No redraw/resampling.

Engine contract: 4x4 TileObjectData, 16px cells +2px padding, 288x72 horizontal
atlas, StyleMultiplier4, automatic anchor alternates at style0. Origins are
(1,3)/(0,1)/(1,0)/(3,1); root offsets (0,4)/(0,0)/(0,-4)/(0,0).
Side roots are flush. Native SetDrawPositions explicitly applies the placed
alternate's Y offset; installed TileLoader otherwise uses the base style's +4.
No custom tile draw path; retain native paint/Echo/light handling.
Each alternate requires four full solid supporting cells on its attached face.
No floating background-wall-only or sloped/platform anchoring in this version.
Existing upright frame coordinates and item identity remain valid.

Damage uses the orientation's exact alpha mask and draw offset; non-solid,
actuated segments harmless, standard immunity,30 provisional base damage.
Mining any occupied segment returns one same cluster item regardless of facing.
No new recipe, worldgen, mob/acid or separate library. Future generation mixes
individual teeth with these groups after the individual physics gate passes.

Topology checked against installed tML2026.7.3.0 and official TileObjectData
alternate/StyleMultiplier documentation (2026-09-09):
https://docs.tmodloader.net/docs/stable/class_tile_object_data.html

Static v2 gate:16384 independent alpha/contact comparisons, original upright
hash, eight colors, exact rotations and zero padding. Eight deliberate corrupt
exports fail through the actual CLI, including an unrotated wall variant.
Native evidence: `Art/Validation/MawToothCluster-2026-09-09/README.md`.

## Historical floor-only probe

September9: user accepts the native short/long/wide tooth artwork and requests
a crammed-together placeable group. Follow-up confirms hazardous and easily mined.
Preserve all individual source sprites. One five-fang floor group is the bounded
candidate, not new generation or all wall/ceiling variants.

## Contract

-64x64 source assembled by integer placement of approved pixels; 4x4 furniture
  with16px cells,2px padding,4px root inset. No redraw or resampling of world art.
- Real placeable item; native placement/preview, floor anchor, whole-object
  pickup from any part and one recovered cluster. No cheap recipe or worldgen
  injected ahead of progression; QA supply only for the first test.
-30 contact damage is provisional tuning; starter-pick removal. Hurt mask
  follows actual tooth pixels, including native draw offset and actuation.
  Empty air is safe. Native immunity prevents one hit per tooth every tick.
- User confirms THORN-STYLE cluster: non-solid, contact-hazardous, no invisible
  wall. This separate placeable group does not silently change the individual
  solid-fang experiment. Four-wide full solid floor support; no platforms in v1.
- Optional isolated package, disposable gg/V3/SP gallery only. Static red test,
  atlas reassembly/alpha/mask/provenance defects, native placement/mining/support/
  hurt/cooldown and save/reload evidence precede promotion. Preserve old QA data,
  accepted backgrounds/pod/individual teeth and unresolved grove reload failure.

Topology: native TileObjectData as in the validated ArrivalPodTile and
MawToothArtTile. Native art acceptance does not approve a new collision model.
Source/reference API: https://docs.tmodloader.net/docs/stable/class_tile_object_data.html
