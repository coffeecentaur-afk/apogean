# Player-directed tooth curves — installed QA candidate

September9 UPDATE: full build and native API fixture pass; all eight saved
variants survive actual save/reopen. See
`Art/Validation/MawToothCluster-2026-09-09/PlayerCurves-v4/README.md` for619 native
checks, exact package hash, remaining manual/MP/art gates, and historical
display-layout drift. Use orientation-audit/curves/proof-view, never rebuild.
The staging record below is historical, not current install status.

Confirmed: rounded BACKS follow facing on floors/ceilings, points lean opposite.
On either wall, player below the object center means tip up; above means tip
down. Exact-height tie provisionally points up. Existing placements stay fixed.

This is a mechanical atlas expansion, NOT the requested simpler-art redraw.
Original colors/detail, individual models and4px rooted offsets are preserved.
Legacy saved banks0..3 stay floor/left/ceiling/right; banks4..7 add the opposite
curves. Atlas832x144, upper18px-stride preview cells, lower26px-stride placed-wall
cells centered at24px width. The object itself remains64x64. Item.placeStyle0/1
uses multiplier4/wrap8 and identical four native anchors; both drop one item.
Code still reads the old four-bank package. Never silently downgrade a saved
eight-bank world to an old package after players place new curves.

HoldItem/CanUseItem query the native anchor chooser, then select the saved style
using MawToothPlacement. No post-place flip or player-sensitive support check.
See official [TileObjectData documentation](https://docs.tmodloader.net/docs/stable/class_tile_object_data.html).
Installed tML2026.7.3.0 TileObject.CanPlace was inspected locally: it returns the
chosen alternate and top-left without placing tiles. Native preview is separate;
the extended QA harness checks both. Do not copy decompiled engine implementation.

## Evidence

- Test-MawToothPlacement:37 checks against actual runtime C# (not duplicate math).
- Test-MawClusterPlayerCurves:119808 exact atlas/padding pixels,32768 contacts,
  16384 wall projections independently mapped from the original sprite.
- Test-MawPlayerCurveValidators:seven real-CLI deliberate failures all reject:
  padding, contact, wrong curve, missing inset, metadata, reversed instruction,
  and fixed-up walls. Scratch: ApogeanCurveControls-bb917266427640cf85c0ef64ed0be980.
- Isolated SOURCE compilation passes with0warnings/errors; no package/install.
  New CompileOnly option invokes Compile instead of Build, retaining tML defaults
  but skipping its AfterTargets=Build installer. Initial BuildMod=false attempt
  failed framework inference; do not reuse that approach.

Tools/New-MawClusterPlayerCurves.ps1 reproduces exact curves into a FRESH folder;
deterministic replay and refusal of existing outputs/Content destinations are
also tested. Run all focused checks with
`Tools/Invoke-ApogeanContentGate.ps1 -Profile MawTeeth`. No regenerated anatomy.
At source-staging time, installed package was STILL rooted-v3:
01FF09E0F2B37FEB916EC6D44225C2DD7F7EF767B09F040CD4AB908F0CAA8C41.
No game restart, world edit, Content PNG edit or simplified-art installation.

## Historical install instructions (superseded by live evidence above)

Preserve ALL build overrides in Art/Validation/MawToothCluster-2026-09-09/Rooted-v3/README.md;
change only MawToothClusterCandidateDirectory to this folder. The abbreviated
CompileOnly run is NOT an approved full-package command. Back up QA save first.
In gg / Apogee Native Visual V3 use orientation-reload, NEVER orientation-build.
The expanded harness is prepared, NOT RUN: all eight banks, item hooks, native
preview style, fixed placement after moving, contact and one-item recovery.
Then manual cursor/root inspection and both curves after save/reload; multiplayer
separate. Preserve six old displays and user-edited old floor gallery. Grove
mismatch remains RED/unmodified. Source tests are not native visual evidence.

Art approval reopened: simplify tooth micro-shading without losing whole sharp
silhouettes or any short/long/wide model. The user also rejects the leaf-like Maw
material. This QA room's support tile is MawDirt; don't assume "moss stone" names
its actual tile. Next proof: quiet grey/black/brown mineral body with sparse amber
fissures, not autumn leaves. Show ONE native-scale material/tooth study before
batch export. Individual tooth specimens remain diagnostic art; shared placement
policy is their contract, not a claim they are already placeable hazards.
