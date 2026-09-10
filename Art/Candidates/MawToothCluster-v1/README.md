# Placeable Maw tooth cluster

## Latest: rooted, mirrored four-face attachment (Native-v3)

V2's wall appearance was rejected. The user confirms rounded backs BELOW and
tips curling UP on both walls, with left/right mirrored. V3 keeps the exact
floor and ceiling pixels and mirrors the left wall to produce the right wall.
The original short/long/wide models remain unchanged.

All four faces now embed4px into support. Preview/saved-frame coordinates still
occupy the upper288x72 region (18px cell stride). The atlas is416x144 with a
second placed-wall region atY72:104px styles,26px stride,24px draw cells.
Native TileDrawing centers wider cells;8 transparent pixels on the outside
produce the correct4px inset. No custom draw pass or saved-frame migration.
The same inset is used for item preview and exact-pixel contact.

Six negative CLI controls plus exact59904-pixel/16384-mask validation and8192
centered-cell projections accompany the native checks. Evidence and limits:
[Rooted-v3 validation](../../Validation/MawToothCluster-2026-09-09/Rooted-v3/README.md).
Use Native-v3 with the full override build command in that record. Do not
rebuild the saved room or the user-modified older floor gallery.

## Historical v2: four-face attachment, rejected wall appearance

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
