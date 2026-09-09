# Maw entrance v1 — tile-scale layout study

Status: **layout approved September9; native implementation pending**. The user
accepted the raised51-tooth-group,24/22-tile-rim preview. This approves its
composition/working scale, not release tuning, final sprites, liquid mechanics
or generator output. New bone/tooth material review is in `../MawBone-v1/`.
Wayfinder #26. Source direction: `MAW_SKETCH_DIRECTION_2026-09-08.md`.

Latest user direction: **many more teeth and much higher opening ridges**, with
the Sarlacc pit as a composition reference. Carry forward the tooth-packed pit
and raised enclosing rim, not a copied creature, beak, tentacles or new boss.
**No safe ledges; players use rope or break teeth** remains in force. The earlier
low-rim/eight-tooth and two-landing proposals remain in Git history only.

The user's sketch replaces the existing thin dome with an open feeding throat,
inward teeth and asymmetric flanking basins. This study makes that composition
measurable before material artwork or generation changes. The proposed study
occupies 120x112 tiles around the mouth, not the entire Maw. Twenty-four rows
of additional headroom accommodate the raised rim; the existing below-ground
depth stays70 tiles. It must fit the
existing protected reservation; no promise to stamp a flat rectangle into a
world or replace the full navigation spine. Final banks must follow surveyed
surface height and blend into native terrain. The Stomach is unchanged.

## Approved working geometry (not native validation)

- Broad left basin A: 124 reserved cells; smaller right basin B: 62 cells.
  Yellow means **proposed acid basin**, not functioning acid. Raised retaining
  lips keep each below its spill level and separate from the central descent.
  If acid is deferred, do not label ordinary water as acid or restore acid tiles.
- Fifty-one attached brittle tooth clusters, up from eight: nine around the
  raised crown and42 along the inner walls. Mixed lengths and opposite-side
  offsets create a dense fringe instead of occasional isolated spikes. Shapes
  here are coarse footprints, not approved tooth sprites or damage hitboxes.
- Left/right solid rim crests rise24/22 tiles above the surrounding ground
  (previously6/4), before the crown teeth are counted. Thick asymmetric banks
  curl inward without a roof closing the mouth. These heights and the current
  count are accepted working preview choices, not final production tuning.
- No generated resting shelves or safe landing markers. Walls change direction
  gradually without two-tile-wide safe floors in the throat. The player at the
  outside surface is a conservative 2x3-tile scale reference, not a resting point
  inside the descent or a new character/hitbox specification.
- An open winding corridor connects to the existing lower Gullet. Dotted line
  shows available space, **not** a jump trajectory, safe free fall, required
  path or installed rope. The intended options are player-placed rope or mining
  teeth to clear the route. Do not automatically supply either solution. Normal
  player building remains available; no new tool restriction is implied.

## Single-source preview and checks

`entrance-layout.json` owns tile coordinates, rim contours and tooth profiles.
Schema2 derives tooth roots from each actual bank contour, then checks their
connected footprint; no independent image-mask positions. `layout-preview.template.html`
draws the same rasterized cells that `Tools/Build-MawEntranceLayout.cjs` checks;
there is no separate hand-painted geometry being passed off as generator proof.
The builder has no game API, never opens a world, and only writes an explicitly
requested HTML preview outside production assets. It uses built-in Node only.

Run from the repository:

```text
node Tools/Build-MawEntranceLayout.cjs
node Tools/Build-MawEntranceLayout.cjs --preview ABSOLUTE-PATH/maw-entrance-layout.html
```

September8 revised result: PASS. Both basins are enclosed at their proposed
fill levels, all51 tooth clusters are connected and attached to terrain,
the surface scale figure has support, and a2x3 clearance search reaches the
bottom exit. 4,855 clear positions are reachable under that search. Nine
deliberate failures are rejected: floating tooth, obstructed entrance, restored
safe ledge, lowered rim, sparse teeth, leaking pool, blocked exit, false route
annotation and out-of-bounds geometry. Restored shelves, low rims and sparse
teeth must fail specifically for their respective rule, not an unrelated error.
Candidate regression bounds (at least16 tiles of rim rise and40 tooth groups)
prevent accidental return to the rejected silhouette, not set release balance.

One first-pass crown root on the steep outside bank failed its connectivity
check. Its placement was corrected to follow the contour; the gate was not
relaxed. All final footprints are connected, supported and non-overlapping.

The focused test checks for exposed, non-hazard two-tile floors with three tiles
of clear headroom inside this candidate's descent window (x40–83, y46–111).
Surface banks and basin floors are outside that resting-shelf constraint.
One-tile raster steps and dangerous tooth projections are not guaranteed safe
rests. This is grid clearance, **not Terraria movement, rope placement, mining,
gravity, jump, grapple, damage or multiplayer simulation**. The installed legacy
shelf generator and drop-limit validator remain unchanged; adapt those together
when the approved entrance reaches native implementation.

Preview inspected in installed Edge headless at 736px and 360px, light and dark:
411 rendered rectangles, no page errors or horizontal overflow; only the
outside-surface scale figure remains.
Readable labels remain in screen pixels; geometry scales to available width.
Local preview/captures live in this task's visualization directory, not Content.

## Bone atlas baseline — deliberately red

`Tools/Test-OssuaryBoneAtlas.ps1` follows the current conventional texture path
by default (the tile class presently has no override). Basic atlas checks pass:
288x270, 61,440 opaque pixels, eight colors, no soft alpha or pure-white pixels.
But all240 populated16x16 frame bodies have **the same full-square alpha mask**:
zero contoured frames, one unique mask. The focused edge-diversity gate fails.
The source contains no exterior/isolated contours for the renderer to choose;
this corroborates the previously captured speckled-rectangle appearance.

Positive control: installed native Stone export passes the same necessary
edge-diversity test (183 occupied frames, 72 full, 111 contoured, 74 masks).
This is **not** an assertion that every material must copy Stone's exact alpha
topology, or that square tiles are illegal in tModLoader. Organic structural
bone needs proper exterior silhouettes and frame mapping as an art contract;
the test deliberately cannot certify texture quality, merges or gameplay.

Historical `Tools/New-EnvironmentalAlphaPixelArt.ps1` uses LockedBulkhead as a
recolor/noise template for OssuaryBone. Do not run that broad legacy generator:
it would touch unrelated accepted families and is not an organic atlas author.
Current production palette/dimensions differ from its present template/output
settings, so that source is provenance context, not a proved byte-for-byte
reproduction of today's PNG. No atlas was changed by this baseline.

OssuaryBone currently sets bidirectional merge flags with Mawstone, remains safe,
uses MinPick59/MineResist2.7 and can explode. Flags alone are not a native merge
test. The pending brittle tooth must remain a separate easy-mining hazard.
Do not weaken structural bone or silently make all bone damage the player.

## Next gate

1. Layout review complete September9: open mouth, dense teeth and raised rims.
2. Author a focused structural bone candidate and a clearly distinct brittle
   tooth candidate; preserve a verified framing contract instead of recoloring
   every square in the old sheet. Show genuinely new art before installing it.
3. Prove isolated/exterior/interior/merge/slope frames and tooth support,
   orientation, actual contact damage, immunity timing and easy removal in the
   disposable native fixture. Save/reload and multiplayer remain separate gates.
4. Adapt only the approved shallow mouth to the saved route and protection
   planner. Do not stamp this bounded schematic into regular worlds or treat
   its grid-clearance test as a world-generation compatibility test.

Acid #27 is optional and separate. Confirmed mixing direction is neutralization
then ordinary Terraria reactions; contact quantity, transport and harm/render
agreement remain unproved. No new NPCs, liquid ID, package or world changes here.
