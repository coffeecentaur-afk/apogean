# Provisional shallow Maw traversal — v1

2026-09-12. **Pure-template contract, not native traversal/art/worldgen acceptance.**
Authority: `MAW_AUTONOMOUS_PASS_2026-09-12.md` and the subsequently approved
single disposable connector. No historical exhibit is replaced or repaired.

## Plan and controller seam

`Content/Diagnostics/MawShallowTraversalPlan.cs` has no Terraria dependencies,
asset requests, random seed, registration, world writes or update hooks.
Instantiate once per explicit command, never per frame:

```csharp
var plan = MawShallowTraversalPlan.Create(); // validates before returning
var cells = plan.ExportCells();             // independent Cell[112,104], indexed [x,y]
var clusters = plan.ExportClusters();       // independent eight-object array
plan.ValidateExport(cells, clusters);       // validate these exact arrays before any placement
```

Coordinates are local tiles; rectangles are right/bottom exclusive. Version is
`1`. Exports cannot mutate the plan. `ValidateExport` throws `SHALLOW_PLAN: ...`
on invalid dimensions, materials, roots, sockets, clearance or any remaining
departure from the fixed layout. It is NOT a validator for a subsequently mined
world. Null tile keys mean authored air, not authorization to clear existing land.

| Region | Local bounds / role |
| --- | --- |
| Complete plan | 112×104; outer four columns, top eight rows and bottom four rows empty |
| Geology | (4,12), 104×88; connected banks and enclosed pocket shell |
| Gullet | (28,8), 28×88 before ribs; entrance body anchor (40,14), lower anchor (40,91) |
| Side pocket | (84,28), 20×18; body destination (99,36) |
| Connector | bounding box (54,39), 32×30; exact dogleg is marked by `Cell.Region`, not the whole rectangle |
| Ribs | roots (26,31), (57,55), (26,81); directions +1/−1/+1; lengths 11/12/8 |

One pocket and one winding connector; no optional offshoot in v1. Four widened
4×4 turns join unequal offset rises, a two-tile-wide bare vertical section and
a three-tile-high bare horizontal section. Three four-neighbor-connected shafts
taper to single-cell tips; their first two columns are porous bone embedded in
the bank. They are irregular footholds, not generated platforms or a repeating
safety staircase. Surface treatment is full cap → rooted grass → soil, retaining
side-face grass. Two 3×3 amber tile organs and two 3×3 exposed amber wall patches
are the only light-bearing regions. There are no ropes, strands, liquids or mobs.
The three body anchors are clearance samples, not safe teleport destinations or
guaranteed standing ledges; the native controller must find actual supported footing.

## Reuse existing native content

- Tile keys `soil/grass/stone/bone`: `MawPackedPreview.TileType(key)`.
- `rib/cap`: `MawAnatomyMaterials.Tile(key).Type`; these are provisional QA siblings.
- Tile `amber`: registered `MawAmberLitTile`, **not** the inert generic amber study.
- Walls: `MawPackedPreview.Wall(key).Type`, except `amber` → `MawAmberLitWall`.
  Bone backs cortical shafts; grass backs caps. These are unsafe walls. Preserve
  existing canonical texture aliases and IDs; add no texture/map bank.

Eight tooth roots/variants: `(88,42)/0`, `(28,20)/1`, `(88,28)/2`, `(52,40)/3`,
`(92,42)/4`, `(28,24)/5`, `(92,28)/6`, `(52,44)/7`. These form four adjacent pairs
of existing five-tooth clusters, not new individual-fang physics.
`Surface = Variant % 4` means floor / left support / ceiling / right support;
`Style = Variant / 4`. Native origins relative to root are respectively
`(1,3)/(0,1)/(1,0)/(3,1)`. The exported helpers reproduce
`MawToothClusterTile.PlacementOrigin/Support` and `MawClusterOrientationLab.PlaceNative`.
Each 4×4 footprint is air with four full unsloped support cells plus a second
continuous geological backing layer. No socket is a fabricated floating pad.

Native owner must frame shell/anchors first, then call `WorldGen.PlaceObject`
at translated root + origin, with the registered cluster type and `style: Style`.
Use the existing automatic anchor selection, then verify the actual variant:
each part's saved frames are `Variant*72 + dx*18`, `dy*18`. Never paint multitile
frames to force placement. Require the eight-bank asset registration; any wrong
placement/variant fails and rolls back only newly owned writes.

## What clearance establishes

Public primary seam, checked September 12:
[tModLoader Player API](https://docs.tmodloader.net/docs/stable/class_player.html)
declares `defaultWidth = 20`, `defaultHeight = 42`; position is the hitbox's
top-left. Native owner must check the actual unmounted standing body too; do not
silently apply this contract to a resized/mounted player.

The plan's conservative two-by-three-cell flood is independently checked with
20×42-pixel AABBs, positioned at tile anchor + (6,3), and swept between adjacent
anchors. Sloped solids are treated as full cells. Whole cluster footprints are
excluded, proving an available spatial route without mandatory tooth contact.
Their native 4px insets enter already-solid support. This is NOT gravity, jump,
hook, mining-input, damage, fall-survival or native slope movement simulation.
Keep actual two-way traversal, equipment comparisons and the decision to mine
as native gates; a point path or a 120-tile drop cap cannot substitute for them.

## Native ownership and preservation gates

Main owns placement, controller, routing, builds and game work. Require packed
QA / gg / V3 / single-player and no active timing sample or held gallery view.
Use a finite search (at most 12 sites), checking the complete tile/wall/liquid/
wire/coating/actuator state and the **larger** framing/effect envelope before any
write. The plan's empty border is not a complete runtime-effect safety guard.
Reject overlap with every saved historical fixture/reservation and active
player/NPC/projectile placement hazard. Reserve before stamping; refuse missing
assets, occupied sites and repeat builds. No broad clear or automatic repair.

Save bounds, version and immutable creation identity once. Separately compare
actual pre-save/post-reload contents after legitimate mining/player construction;
never replace creation expectations or regenerate mined teeth. Preserve the
original 39-segment hanging failure, all prior layouts and their current-state
fingerprints. No hanging growth/allowlist changes. Run destructive mining last;
do not reset the fixture to compare loadouts. Amber inherits existing world
activity here; do not change progression flags or enlarge the old preview region.

Reuse existing rib joins/anatomy physics, cluster orientation/mining/contact,
playable-material and amber controls in their own guarded scopes. Their old
results do not certify this new geometry. Ordinary-view root/tip continuity,
standing-body bends, manual mining/recovery, transparent-gap damage negatives,
unprepared/rope/hook/double-jump/fall-protection comparisons and real reload remain
pending. Final art, larger seed worlds, multiplayer and generator promotion remain
separate. Walls cannot conceal a failed root join.

## Pure evidence and eventual integration

`Tools/Test-MawShallowTraversalPlan.ps1` compiles only this pure source in memory;
it writes no files and does not reference or run Terraria. It checks actual
exports, deterministic layout, independent rib connectivity/taper, socket bank
mapping, swept body paths both ways and ten mutations. One-high/one-wide defects
retain a point path but fail the body path. `-Defect blocked-connector` (also
`missing-rib`, `missing-root`, `unsupported-cluster`, etc.) deliberately exits
nonzero through the real exported-plan validator. No project build is involved.

Executed after 18:44 UTC on September 12: pure suite PASS (11,930 assertions,
including ten rejected exports). Separate actual CLI invocations for blocked
connector, missing rib, missing root and unsupported cluster each exited 1 with
the intended validation reason. These are pure source tests only; native work
had not yet been performed at that pure-test checkpoint. The later native
records are in `Art/Validation/MawShallow-2026-09-12/README.md`; these supersede
that pending statement only for their named placement/body/render checks.

## Bounded entry movement probe

`MawShallowMotionProbe.SetControls` uses the installed tML hook documented in
`tModLoader.xml`: input changes precede native movement; `PostUpdate` observes
the end of actual `Player.Update`. It never assigns position/velocity during
the sequence, changes equipment/health/immunity, holds the camera, or invokes
a substitute gravity solver. Only explicit `motion-entry` starts it after the
normal visit teleport to existing supported footing. It runs right until the
body is over the first rib, releases input, then observes native falling and
landing. Limit360 updates/10seconds; scope/focus/pause/mount/hook/equipment
changes abort. The existing gg loadout is recorded, not called unprepared.
G additionally accepts the native-created Classic `Maw QA Plain` character,
with runtime starter-only inventory/no equipment/buffs and100HP/20mana baseline
checks at start and throughout movement. It never removes/grants items or changes
health. The new character cannot build scenes or run unrelated laboratories.

`Test-MawShallowMotionTrace.ps1` independently rejects incomplete controls,
discontinuous positions, false/unstable landings, missing falling, wrong context
and missing ticks. Its no-argument self-test uses synthetic validator data and
twenty-one rejected defects, never native evidence. Per-sample health must agree
with the final summary; Plain evidence needs actual booleans and100HP bounds.
The native JSON must separately
pass via `-Path`. One successful landing does not certify the winding connector,
full descent, return route, all loadouts, manual feel or difficulty.

## Clean lighting preview

Plain held views may request `light-awake`, `light-dormant` or `light-natural`.
These are unsaved QA-bounds-only emission overrides, not world progression.
Release stops the override; moving rather than held views use actual world state.
Light reports retain global/preview state, baseline and nearby actor counts.
Only zero-actor, starter-baseline comparisons at the same settled camera are
clean controls. Native measurements and visible readability are separate gates.
G proves differing awake/dormant illumination; it does not approve the dim pocket.
New ambient spawns are suppressed during this character's scene visit only;
existing actors are never killed. This is terrain isolation, not enemy balancing.

## I connector-out trial (pending native evidence)

The explicit `motion-connector-out` command uses the unchanged v1 scene, starting
on existing supported terrain at local pixel(1312,646), beside the pocket opening.
No new ledge is added. Native left/right/release inputs steer toward the center
of the two-tile shaft at x70, then toward the lower connector exit at x59 after
the entire body clears y63. A simple stopping-distance estimate decides when to
release input; it is not a substitute physics solver. Terraria still owns actual
velocity, gravity, wall collisions, damage and floor landing. No jump is supplied.
This is only outbound/downward travel, not the return ascent or manual-play proof.

The controller retains the360-update/10-second budget. Its envelope is local
x54..90/y36..72 tiles. Trace schema3 names the route and records both directional
inputs. The independent validator requires the exact supported start, continuous
samples, actual left/falling inputs and12 stable ticks on the existing lower floor
at y69. Old entry schema1/2 evidence remains valid without rewriting any originals.
Three synthetic baselines/28 deliberate defects test the validator, not Terraria.

Held views now hold noon/rain/eclipse as well as the camera; this avoids H's
advancing ambient-sky confound. Free movement does not freeze time or apply the
amber preview. Release restores the pre-visit time/weather and clears1.6× preview.
No changed emission strength is approved for production by this trial.

Canonical integration reference only:
`Common/WorldGeneration/ApogeanWorldGenerationSystem.cs` orders Maw after vanilla
Final Cleanup, then optional Wastes, Arrival Survey, Compounds, Ruins and Arrival.
This template does not enter that sequence. Later integration must respect the
saved atlas/protections and native structure-map reservation, and jointly replace
legacy Gullet shelves and their obsolete fall-cap acceptance rule. No worldgen
code or historical test is changed by this contract.
