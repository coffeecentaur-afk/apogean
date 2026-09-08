# Smooth Wastes flight — bounded QA candidate

## Contract

The user reports that scenery looks right during low flight, then suddenly
drops at each hard ceiling. They approve replacing that speed switch with a
broad altitude transition, preserving depth rather than making every layer
move at camera speed. Exact exit heights are flexible. Artwork, horizontal
parallax, fixed section anchors, wider Mid spacing and the accepted pod stay
unchanged. No altitude opacity fade, time-based spring or post-stop drift.

This supersedes the historical Close15% / Mid25% / Far50% caps. The first10%
of ascent retains the existing ground/jump response exactly. Thereafter,
integrate a smoothstep response over the remaining ground-to-Space interval:

```
u = max(0, (normalizedAscent - .1) / .9)
integral = u < 1 ? u^3 - .5*u^4 : u - .5
displacement = ascent*baseRate + viewportExcursion*depth*integral
baseRate (Far, Mid, Close) = (.012, .03, .06)
depth (Far, Mid, Close) = (1, 1.3, 1.6)
```

Excursion places Far's top64px below the viewport at the Space reference.
Position, response and response derivative join continuously. Close response
stays greater than Mid, Mid greater than Far, including the constant-rate tail.
The projection is a function of current camera position, not elapsed time.
No asset scale change. A stopped camera cannot produce catch-up; descending
to an old coordinate produces that coordinate's original scenery position.

## Build and static evidence

- New test against the real old source fails `ALTITUDE_SPEED_KINK`: at the old
  ceilings a100px camera rise changed Far1.2 / Mid3 / Close6px motion into
  133.33px at the QA zoom. The original position checks had missed velocity.
- `Test-WastesSmoothFlight.ps1`:179,811 assertions pass across four surface
  heights, four viewport heights and three zooms; speed/depth order, low motion,
  transition boundaries, geometric exit, no fade and stateless revisit.
- Existing Close-depth1,731 and fixed-section5,799 assertions pass. Old hard-cap
  tests now use the smooth policy; historical cap traces require their commit.
- Ten actual source mutations rejected, including abrupt response, collapsed
  depth order, no exit excursion and early ground movement. Latest scratch:
  `C:/Users/max_h/AppData/Local/Temp/ApogeanHeightMutations-479ac12f09f64402bcb872e140bfe775`.
- Full Background gate keeps its known production-readiness failure:
  26 undersized legacy V0 images and the legacy last-row stretch. Other checks
  passed. This camera correction does not approve those unfinished assets.
- Initial packaging was blocked by the open game (`TML003`); it succeeded
  after a confirmed save/exit. First candidate EDB4B2A0... had incomplete Close
  offscreen diagnostics, not a reported renderer position failure. Retained
  `incomplete-close-trace.log` is **not** final validation evidence.
- Corrected diagnostic observes actual modular Close top before culling,
  including its offscreen hold. Ground-contact and submitted GPU-matrix checks
  remain separate. The replay compares each regional anchor with its own prior
  sample, not neighboring sections. All three layers use an independent oracle.
- Final isolated build: zero warnings/errors,8.59s. Mirror:
  `C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/9d0935fe17494a42ab5fbabd2624c8e1/apogean`.
- Installed package SHA256:
  `8DC9FB30C538BC091ECADCF9FA8C9163927FC554C2D7DBA5A2E20903148BEB36`.
- Pre-change package, saved QA world and gg backup:
  `C:/Users/max_h/AppData/Local/Temp/ApogeanSmoothFlightBackup-7bc6447bb36c4019b05fd04c03ba9971`.

The unchanged candidate inputs are FarCity QA-Package-v1 + Runtime-v1,
MidRuins ScaleStudy-v3 + ComponentAssembly-v1, StationBridge Study,
MidDepth-v2 EdgeReview, and ArrivalPod Native-v3. These remain isolated build
overrides, not production Content promotion.
All410 final-mirror Content PNGs were hashed against the previous pod mirror:
zero differences. The two pre-existing user-modified files retain their initial
SHA256 values and are excluded from this change.

## Native evidence

All nine completed cases pass: space-ascent, space-descent, flight-turnaround,
run-flat, run-diagonal, diagonal-left, diagonal-right, ground-pan-left and
ground-pan-right. `native-2560x1369.log` contains757 allowlisted QA records,
not the raw startup log. Both ground pans retain15 stable Close section anchors;
both directions have zero Far edge gaps. No test fixture was rebuilt.

Only gg / Apogee Native Visual V3, tModLoader2026.7.3.0, viewport2560x1369,
game/background zoom1.3333334. Camera fixtures move the camera/player without
building terrain. Do not call these physical-input flight tests. Window captures
include the title bar; unedited pixels, not concept renders or motion video.

The first session's `ground-before.png` and `ascent-sample.png` show unchanged
ground composition and a sampled climb respectively. `space-hold.png` comes
from the final diagnostic build and shows all three land layers outside view.
`midflight-hold.png` shows the stationary middle-altitude composition;
`descent-sample.png` and `run-diagonal-sample.png` show later native samples.
The whole-world `diagonal-right-sample.png` is terrain/liquid-obscured and is
not a useful visual approval of the background. Submitted geometry is checked
independently; do not present that dark capture as proof of attractive scenery.

Ascent, descent and the10s rise /5s mid-hold /10s return /5s ground-hold case
pass the independent native replay. All three layers report the correct
positions; the Close trace includes projected offscreen sections rather than
pretending culled sections were GPU draws. The separate modular validator
checks the submitted draw matrix, depths and counts.

Both3,600px running-camera cases pass27 independently replayed position
differences each. Flat max error0.600px;128px diagonal rise/fall max error0.940px.
All completed modular cases report zero matrix/depth/count failures and at
most0.01px pixel-position error. Each whole-world diagonal sweeps128,640px:
4.853 Far periods,1.801 expanded Mid periods and11.344 Close periods.

Native validator controls: two unchanged traces accepted, seven deliberate
defects rejected (old cap profile, fade, failed position, missing exit/result,
missing mid-hold and missing return). Scratch:
`C:/Users/max_h/AppData/Local/Temp/ApogeanHeightTrace-ce66cb2069e349f7a6652a4c9cc6cac6`.
Removed hold samples cannot masquerade as a hold merely because rise/descent
later revisit the same coordinate; consecutive hold samples must remain within
the logging interval. Never alter the original trace to obtain a pass.

The existing grove reload guard remains failed (expected87B951FE... versus
actual43FE618C...). Its exception says **No rebuild performed**; no fixture
rebaseline or repair was requested. This is not a clean-log/persistence claim.
The pod art approval is recorded separately from its remaining behavior gates.

Replay from the repository root in PowerShell (fresh child processes avoid
Add-Type name collisions between independent static harnesses):

```powershell
$log='Art/Validation/WastesSmoothFlight-2026-09-08/native-2560x1369.log'
& ./Tools/Test-WastesHeightLive.ps1 -LogPath $log -Viewport 2560x1369 -Cases @('space-ascent','space-descent','flight-turnaround','ground-pan-left','ground-pan-right')
& ./Tools/Test-WastesRunningLive.ps1 -LogPath $log
& ./Tools/Test-WastesModularLive.ps1 -LogPath $log -Cases @('space-ascent','space-descent','flight-turnaround','run-flat','run-diagonal','diagonal-left','diagonal-right','ground-pan-left','ground-pan-right')
& ./Tools/Test-WastesHeightLiveMutations.ps1 -LogPath $log
pwsh -NoProfile -File Tools/Test-WastesSmoothFlight.ps1
pwsh -NoProfile -File Tools/Test-WastesForegroundDepth.ps1
pwsh -NoProfile -File Tools/Test-WastesFixedSections.ps1
```

`ground-after-release.png` verifies return to the original ground location
after camera overrides expire. It is nighttime, not a matching day-art comparison.
The existing pod `day` view then leaves gg beside the accepted pods in daylight
for user review. No pod rebuild/retest; its within-session grove guard passes.
Client remains running, camera sequences inactive, no pending request.

## Remaining limits

Native motion comfort requires user review. Other native viewports, zoom/gravity,
multiplayer, physical running/flight, real-biome and deep-cave routing, measured
performance and production-ground-profile coverage remain separate. No
purification rendering, new artwork, ordinary-world retrofit, divot or Maw work.
Do not restart another art loop if the remaining issue is camera tuning.
