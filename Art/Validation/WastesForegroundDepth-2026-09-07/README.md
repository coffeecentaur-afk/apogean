# Foreground depth trial — September 7

**Subsequent user review:** "that looks so much better i love it." The user
accepts this installed foreground response/composition and regards occasional
overlap as minor. Preserve the accepted depth and artwork. The requested sparser
Mid follow-up is tracked separately in `../WastesMidSpacing-2026-09-07/README.md`.
The subsequent spacing package now passes the full eight-case native camera
matrix at2560x1369; see that follow-up README and actual telemetry. Historical
attachment failure below is retained, not the current native outcome.

The user clarified the apparent following/jumping is the combined diagonal
response and perceived closeness, not necessarily an unstable world anchor.
They want Close pushed back toward Mid, while still a little nearer. This
supersedes the earlier requirement for Close to move vertically at tile speed.
Diagnosis therefore changes to a bounded depth-tuning task, not an unproven
coordinate-bug repair. The existing 5,804 fixed-section assertions pass before
this change; that check alone never established movement comfort.

Trial contract: Close horizontal response .20 instead of .30; vertical response
.06 instead of full game-zoom/tile speed. Mid remains .14 horizontal/.03 vertical,
Far remains .055/.012. Close uses a 15% ascent ceiling before the accepted Mid25%
and Far50% ceilings. All are positional caps, never altitude opacity fades.
Terrain anchors remain immutable per absolute section. Their height differences
also project through the Close depth factor. No camera spring, temporal smoothing,
random reshuffle, camera-center terrain resampling, texture change or new biome.

At its own ground-reference view, the soil datum remains 48 world pixels above
ground. Away from that reference, it is now a background plane, not a foreground
tile glued to playable terrain. Source pixels and silhouettes stay unchanged.
Caps still let higher flight leave every surface layer behind.

Feedback command: `pwsh -NoProfile -File Tools/Test-WastesForegroundDepth.ps1`.
It exercises the real modular Close projection with a 16px running rise and
checks for .96px scenery motion, with ceiling, relief and revisit checks.
User movement/composition approval is now accepted. Coverage failures were
retained until the subsequent full eight-case matrix passed; no broader
production gate or untested viewport is implied.

## Checked and installed

- Red baseline: the real modular projection moved21.3333px for a16px camera rise
  at4/3 zoom; the new small-rise test rejected it. Trial response is .96px.
- Passing focused checks:1,735 Close depth;3,150 height/cap;5,799 fixed-section;
  92 camera projection;56 modular layout;72 Close ground/depth;25 restoration.
  Fixed-section count changes because .20 exposes fewer cells than .30; the
  traversal range and requirements were not reduced to force acceptance.
- Ten deliberately broken actual-source variants rejected by their CLI tests,
  including old tile-speed Close, old .30 horizontal depth and uncapped Close.
- `Test-WastesRunningLiveValidator.ps1` accepts one fabricated valid trace and
  rejects four defective traces through the real live-validator CLI. This is
  synthetic validator testing only, not fabricated engine evidence.
- Isolated build: zero warnings/errors, using the exact previously approved QA
  candidate directories; no Content PNG, architecture, palette or scale edits.
- Full Background gate rerun: new depth/motion/anchor controls and existing
  mask/HD checks pass. Overall gate remains RED on the established production
  audit:26 undersized legacy V0 layers and legacy last-row stretching. No audit
  thresholds were weakened; this runtime trial does not finish every biome.
- Installed package SHA256:
  `0391BE79B2AB55861DD507C57BEF03DF00D6AA3DD7D9121C97E6FF5E521BCBF9`.
- Build mirror:
  `C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/a9f61a6caff44e61afc8e33e01312c51/apogean`.
- Pre-install package/player/world/config backup:
  `C:/Users/max_h/AppData/Local/Temp/ApogeanForegroundDepth-89aff07003bb4f2b9a0fd809794bf260`.

## Initial native attempt — historical attachment failure

The existing QA game consumed `qa-save-and-quit`; native main menu was observed
before exiting. Build was installed, then tModLoader restarted through Steam;
the new client log records successful mod loading. Native control then failed
with `foreground window did not report a process id`. Fresh app discovery and
one bounded attachment retry failed too; desktop input stopped. User was asked
to load `Apogee Native Visual V3` with `gg` so the existing scoped test receiver
can run. No new post-build game screenshot, movement result or comfort approval
is claimed. No regular world was opened or modified by the agent.

Next bounded fixture: `run-flat`, `run-diagonal`, `ground-pan-left`,
`ground-pan-right`, `diagonal-left`, `diagonal-right`, `space-ascent`,
`space-descent`, then release. The running paths simulate camera traversal,
not physics:20 seconds across3,600 world pixels, optional64px sinusoidal rise,
then10 seconds stationary. Actual submitted screen positions must match the
new depths within one native-pixel flooring difference. Keep pixel captures
and user feel review separate. Export only allowlisted diagnostic records.

Legacy `WASTES V1 GROUND LOCK` log names remain for parser compatibility but
now check the explicitly changed depth-plane contract, NOT tile-speed locking.
Module failures now report separate matrix/depth/count totals. Historical
coverage failures remain retained in the earlier evidence folders; a new motion
pass alone cannot clear them. The grove reload digest mismatch remains untouched.
