# Maw entrance v1 — tile-scale layout study

Status: **candidate / user review pending**. Not a game screenshot, final pixel
art, accepted dimensions, liquid implementation or generator output.
Wayfinder #26. Source direction: `MAW_SKETCH_DIRECTION_2026-09-08.md`.

The user's sketch replaces the existing thin dome with an open feeding throat,
inward teeth and asymmetric flanking basins. This study makes that composition
measurable before material artwork or generation changes. The proposed study
occupies 120x88 tiles around the mouth, not the entire Maw. It must fit the
existing protected reservation; no promise to stamp a flat rectangle into a
world or replace the full navigation spine. Final banks must follow surveyed
surface height and blend into native terrain. The Stomach is unchanged.

## Proposed geometry

- Broad left basin A: 124 reserved cells; smaller right basin B: 62 cells.
  Yellow means **proposed acid basin**, not functioning acid. Raised retaining
  lips keep each below its spill level and separate from the central descent.
  If acid is deferred, do not label ordinary water as acid or restore acid tiles.
- Eight attached brittle tooth clusters: three on raised exterior banks, five
  facing into the shallow descent. Shapes here are coarse tile footprints,
  not approved tooth sprites or a promise of full-rectangle damage hitboxes.
- Two dry, wall-grown landing ledges; no free-standing placed platforms.
  The player at the left lip is a conservative 2x3-tile clearance reference,
  not a new character/hitbox specification. Outlined figures mark dry spaces.
- An open winding corridor connects to the existing lower Gullet. Dotted line
  shows available space, **not** a jump trajectory, safe free fall, required
  path or installed rope. Normal tools/rope/platforms still need live playtest.

## Single-source preview and checks

`entrance-layout.json` owns tile coordinates. `layout-preview.template.html`
draws the same rasterized cells that `Tools/Build-MawEntranceLayout.cjs` checks;
there is no separate hand-painted geometry being passed off as generator proof.
The builder has no game API, never opens a world, and only writes an explicitly
requested HTML preview outside production assets. It uses built-in Node only.

Run from the repository:

```text
node Tools/Build-MawEntranceLayout.cjs
node Tools/Build-MawEntranceLayout.cjs --preview ABSOLUTE-PATH/maw-entrance-layout.html
```

September8 result: PASS. Both basins are enclosed at their proposed fill levels,
all eight tooth clusters are connected and attached to solid terrain, the three
standing volumes are dry with solid non-hazard support, and a 2x3 clearance
search reaches the bottom exit. 3,322 clear standing positions are reachable
under that search. Six deliberate failures are rejected: floating tooth,
obstructed landing, leaking pool, blocked exit, false route annotation and
out-of-bounds geometry. This is grid flood/clearance validation, **not Terraria
movement, gravity, jump, grapple, damage or multiplayer simulation**.

Preview inspected in installed Edge headless at 736px and 360px, light and dark:
209 rendered cell-run/player rectangles, no page errors, no horizontal overflow.
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

1. User reviews the open mouth, basin positions and player-relative scale.
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
