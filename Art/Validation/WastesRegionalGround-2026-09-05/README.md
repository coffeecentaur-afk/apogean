# Regional-ground correction — bounded QA

This continues the user-approved background baseline correction. No artwork,
drop-pod object, relay network or ordinary-world generation changed. Pod/relay
direction and the finite stopping rule are recorded in the Bible and workflow.

## Contract

The near soil reference is 48 world pixels above a saved terrain-height profile,
not `worldSurface - 50` and not the player's current height. The profile samples
natural solid terrain every 32 tiles, takes a five-column median, and interpolates
between regions. It is immutable after capture and written into QA world data.
The draw path performs no terrain scans. Mid and Far artwork/motion are unchanged.

This first implementation operates only in single-player `Apogee Native Visual
V3`. The existing generic Forest lab retains its old reference outside that world.
Production generation-time capture and server/client synchronization are deferred
integration requirements. Natural blocks deliberately placed before initial QA
capture cannot be distinguished from original terrain; this is not a production
terrain-history reconstruction algorithm. Unknown/modded surfaces use a bounded
spawn-height fallback. No world tiles are edited by this system.

## Red/green evidence

- Original real projection entrypoint: the second standing coordinate fixture
  produced a 320px height error. The regional-input fix passes 90 combinations
  of two elevations, three viewports, three zooms and five ascent/descent offsets.
- Actual CLI negative controls: `GlobalDatum` rejects 45/90 cases;
  `ShiftedSocket` rejects 90/90 cases. Snapshot-copy ownership, interpolation,
  edge clamps and bounded column selection are checked separately.
- The first live sampler incorrectly required eight consecutive natural blocks.
  It skipped the existing grove's thin grass/soil floor and chose Y=11248 below it.
  A two-row floor over a deeper cavern reproduces that failure in the shared
  column scanner (`COLUMN_SURFACE_MISSED`). Accepting a two-row natural surface
  fixes the test. The new QA2 save key ignores the rejected QA1 snapshot.
- Corrected live center reference: Y=10368. The previous global guess was Y=9584.
  Camera fixtures now position ground views from the same regional reference;
  diagonal test flight crosses real regions while retaining its ascent pattern.
- Clean isolated build: zero warnings/errors. Existing 56 modular-layout checks
  and 72 legacy projection/coverage cases remain green.

## Live scope and remaining gates

`Native-ground.jpg`, `Native-wings.jpg` and `Native-shallow.jpg` are unretouched
Windows game-window captures at the user's 2560x1369 viewport. The new Close bank
is present at ground height, leaves view on a 1200-world-pixel ascent, and remains
above the shallow underground view. Terrain/depth rendering obscures the lower
shallow view; it is not proof of a complete cave-background handoff.

All five live cases (`ground`, `wings`, `below-ground`, `diagonal-left`,
`diagonal-right`) pass the existing modular validator. Both diagonal sweeps travel
128640 world pixels, cross 3.455 Far periods, and report zero module/coverage
failures, maximum projection error 0.01px and ground-anchor rounding error 0.96px.
`Live-telemetry.log` retains only allowlisted QA records, not account/startup logs.
These camera-controlled cases do not prove manual movement feel or artwork quality.

The QA world was saved through the existing safe-quit command, then tModLoader
returned to its menu and was exited normally. Ordinary worlds were not opened.
Before/after configuration comparison confirms unchanged 2560x1369 dimensions,
windowed/non-borderless mode, Zoom=1.00 and UIScale=1.15.

Pale cutout rims and detached Station branches remain unchanged and failed.
Extra banks and finer composition are deferred polish under the new scope, not
new art approved by these tests. Other viewport live runs, real biome/restoration
transitions, saved-profile reload proof, gravity, multiplayer, and GPU profiling
remain pending for this candidate. No integrated/polished promotion.

The existing grove-checkpoint mismatch recurred; its safeguard refused to rebuild.
It is not a new engine crash, and this pass does not claim to repair that checkpoint.
Backups of the QA world, player and original display configuration are local under
`ApogeanGroundQA-1322cb5a3a32405db1a8b5384be8ef13` in Windows Temp.
