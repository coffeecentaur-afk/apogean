# Arrival divot — bounded new-world evidence

User accepts Native-v3 pod appearance, pickup and replacement and requests the
shallow impact divot next. This slice changes terrain generation, not pod art,
background motion or other scenery. Scene review and production promotion are
separate. No regular world was opened or retrofitted.

## Contract and implementation

- Production planner source is engine-free and tested directly, not reimplemented
  in the validator. Final spawn, near-to-far offsets 10..38 by4 on each side,
  seed parity tie-break, at most48 attempts. Does not consume worldgen RNG.
- Full13-wide/two-deep bowl; compact7-wide/one-deep bowl for cramped sites;
  undug7-wide fallback last. Maximum excavation2 blocks, step height1, five
  solid pod anchors and three additional soil layers. No water/cavity release.
- Survey after Wastes conversion, before our enclosing StructureMap sanctuary
  registration. Other atlas/foreign reservations stay active. Reserve the full
  accepted padded footprint immediately. Own atlas already excludes landmarks
  from spawn throughout. Stamp after compounds/ruins and recheck the site first.
- Explicit natural-soil/wall whitelist; trees, piles, sunflowers, ores, chests,
  buildings, unknown modded cells, wires, actuators, coatings and liquid veto.
  Own DeadTuft/Bristle/RootShrub can clear only as complete framed objects.
- Copy tile **values**, never live Tile handles, for rollback. Clear only the
  actual bowl/headroom mask, frame natural support, place the registered 5x6
  pod through WorldGen.PlaceObject. Do not paint furniture frame coordinates.
- Actual 7x10 spawn landing snapshot and spawn coordinates remain unchanged.
  PostWorldGen checks30 native object cells, full supports, dry air and a
  traversable stepped exit. No new shrapnel artwork; natural nearby debris stays.
- Separate `arrivalSiteV1` outcome record; loading/network receiving it never
  generates terrain or replaces a mined/moved pod. No relay implementation.

## Static / build

- 561 assertions through actual planner source: shape, every padded-cell
  obstacle, partial-cover refusal, cavities, bounds,64 seeds, compact/undug
  fallback and finite no-slot behavior.
- Seven real-source mutations are rejected for their intended reason: no compact
  variant, ignored obstruction/support/reservation/spawn, excessive excavation,
  and cutting part of a ground-cover object.
- Live validator accepts one complete sequence and rejects seven defects:
  skipped site, later failed object check, liquid, moved spawn, missing postgen,
  no native view and no reload-view sequence. These controls are synthetic;
  the logs below are actual engine evidence.
- Generation test package: `60372CB0CA6B11E2587EF3FC6B92F9B3FAF85E31485E57DF7079650437938375`.
  Build:0 warnings/errors. All410 Content PNGs match the previously accepted
  smooth-flight mirror byte-for-byte. Candidate inclusion remains explicit.
- Build mirror: `C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/be4abd6e559c46e5b8f4f052234a1e4e/apogean`.
- Previous package retained in `C:/Users/max_h/AppData/Local/Temp/ApogeanDivotBackup-32ebd9f790dc40cf8387f98f2018f7ff`.

Final installed package: `B8EE0FD875802AB5AAD4BAC37A925884A002E441825DF4BEA344FCE53C0F2BC0`.
Mirror: `C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/3aec32f1740f433e8f926bfb108f81a7/apogean`.
This final0-warning/error rebuild changes only the QA view teleport to sample
the higher bowl shoulder instead of placing gg at the lower pod floor. Native
inspection exposed that diagnostic positioning defect. All410 images and all
four generation C# files hash-identical to the generation-tested package above;
no new terrain-generation behavior is claimed from this presentation repair.

Final-build A3 view is recorded in `seed-A-final-native.log` and the unedited
`seed-A-final-native.png`: saved coordinates unchanged,30-cell/support check
passes, gg stands on the higher shoulder rather than inside it. Replaying this
client log against `seed-A-final-postgen.log` with `-RequireView` passes. The
final screenshot shows the two-block bowl with the accepted pod art; no new
tree/background changes. Left this disposable world open for user inspection.

## Fixed-seed generation (Large, Classic, Apogean only)

Both seeds were selected before testing. Failed runs are retained, not replaced
by convenient new seeds. Each correction was replayed against the same seed.

| Engine seed / string | Final world | Actual shape | Pod top-left | Spawn | Postgen |
| --- | --- | --- | --- | --- | --- |
|840595838 / ApogeanArrivalQA-20260908-A|Apogee Arrival QA A3|13-wide,2-deep|4224,422|4200,428|object30/route/dry True|
|1424807740 / ApogeanArrivalQA-20260908-B|Apogee Arrival QA B2|7-wide,1-deep|4165,524|4205,528|object30/route/dry True|

Final telemetry: `seed-A-final-postgen.log`, `seed-B-postgen.log`. Exporter
retains only allowlisted arrival records, never whole client/server logs.
Server worlds retained under the task-owned temp `ApogeanArrivalQA-20260908-v3`.
Servers were localhost-only, stopped after save completion. Pipes produce a
known Console.Clear/input-handle warning; that is not hidden as a clean raw log.

Historical failures:

- `initial-no-slot.log`: full13-only survey before Wastes safely skipped seedA.
  Final-terrain survey and explicit compact fallback fixed it; planner fixture
  reproduced the missing-compact defect before the source fix.
- `seed-A-postgen.log`, `seed-A-native.log`, `seed-A-native.png`: intermediate
  package6167F6B4, A2 compact variant. Historical native proof, not final build.
- `seed-B-no-slot.log`: intermediate package safely skipped B due trees/piles
  and our own replaceable ground cover. Whole-object-aware clear admission fixed
  B. The final run also rejects a partially intersecting plant at another site.

## Native inspection

Only disposable arrival QA worlds and character gg are used. The `view` request
teleports beside an already generated pod, sets daylight and enables the existing
QA Wastes bank; it never carves a fixture or touches ordinary worlds. This is not
proof of globally promoted background routing. Ordinary enemies remain active.
`seed-B-native.png` is an unedited actual screenshot, with a passing slime in
front of the pod; the pod is grounded in a small soil notch, not a new overlay.
`seed-B-reload.png` follows save/quit and reopening the same world without
regeneration. `seed-B-native.log` records load/view/load/view with identical
saved coordinates and30-cell support checks passing both times. Actual captures
show the same grounded pod/soil notch; this is not a full-world persistence hash
or a pickup-then-reload test. Replaying generation plus client logs with
`-ClientLogPath .../seed-B-native.log -RequireView -RequireReload` passes.

Status gate passes. The complete Structure gate runs the new checks successfully
but remains red on two unchanged WorldVisualIntegrity expectations: the old
exact command ValidateSet string and an8-color minimum contradicting the
accepted5-color tree. Those tests were not weakened or silently repaired.

## Reproduction

From the repository, run:

```powershell
pwsh -NoProfile -File Tools/Test-ArrivalSitePlanner.ps1
pwsh -NoProfile -File Tools/Test-ArrivalSiteMutations.ps1
pwsh -NoProfile -File Tools/Test-ArrivalSiteLiveValidator.ps1
pwsh -NoProfile -File Tools/Test-ArrivalSiteLive.ps1 -LogPath Art/Validation/ArrivalDivot-2026-09-08/seed-A-final-postgen.log
pwsh -NoProfile -File Tools/Test-ArrivalSiteLive.ps1 -LogPath Art/Validation/ArrivalDivot-2026-09-08/seed-B-postgen.log
```

Use `Build-ApogeanIsolated.ps1` with the previously accepted Wastes bank options
and `-ArrivalPodCandidateDirectory Art/Candidates/ArrivalPod-v1/Native-v3`.
Generate a **new** supported Large world with the listed seed and isolated Mods
directory. Never overwrite a failed world, wipe StructureMap or relocate spawn
to obtain a pass. View requests require a world name starting `Apogee Arrival QA `.

## Scope still open

User divot appearance review; wider seed/compatibility/world-settings matrix;
forced native placement-failure rollback; pickup-then-reload of a generated pod;
real fresh-character traversal; multiplayer authority and actuator/sloped-anchor
cases. The small scenario proves placement, not every production case. Safe
no-slot outcomes remain explicit failures of scene QA; do not force destruction.
Legacy WorldVisualIntegrity/background readiness and the existing grove reload
mismatch remain separate, unrepaired and unbypassed. No ordinary-world asset
promotion and no dependent Maw content installed yet.
