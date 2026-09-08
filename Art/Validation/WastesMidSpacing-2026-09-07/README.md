# Wider Wastes Mid intervals — accepted bounded baseline

## User acceptance — September 7

"looks great move on now" approves the installed wider spacing. Preserve the
current artwork, projection and layout. Proceed to the starting-area/drop-pod
slice; do not reopen foreground overlap or another background-art iteration.
This acceptance does not promote the optional bank into ordinary worlds or
erase the remaining production matrix below.

The user accepts the farther-back foreground: "that looks so much better i love
it." That approves the0391BE79 package's movement/composition; the later native
matrix below is separate technical evidence. Foreground occasionally obscuring Mid
is minor deferred polish, not authorization to change its height, speed or art.
The only new request is fewer Mid pieces with larger gaps.

## Contract

Keep all five landmarks, five quiet hills, their original order, native size,
artwork, tint, depths, and flight caps. Expand the sequence from6,800 to10,000
display pixels at unchanged .14 Mid parallax:32% fewer pieces per travel distance.
Landmark spacing increases from1,360 to2,000px; quiet-hill offset from760 to1,150px.
Clear bounding-box gaps increase from184–248px to459–638px, including wraparound.
These are transparent intervals, not newly filled terrain. Close remains .20/.06
with15% cap; Mid25% and Far50% caps remain unchanged. No asset resizing,
recoloring, runtime randomization, terrain generation, save migration or new world.
This remains the optional QA bank, not an ordinary-world art promotion.

## Evidence

- Before edit, the new layout expectation rejects the real old layout with
  `LANDMARK_DENSITY_NOT_REDUCED`.
- After edit, `Test-WastesRuinLayout.ps1` passes176 independent layout cases
  at1920/2560 widths, across negative coordinates and repeat boundaries, with
  original asset identities/sizes and wider gaps.
- `Test-WastesRuinLayoutValidator.ps1` rejects missing groups, phase jumps,
  incorrect assets and the old dense layout through the actual CLI. Background
  gate includes this check for later iterations.
- Foreground1,735 depth,3,150 cap and5,799 fixed-section checks still pass.
- First installation attempt was blocked by TML003: running tModLoader held
  `apogean.tmod` open. After the user closed it, the same six-input isolated
  build compiled and installed with zero warnings and zero errors. No PNG,
  foreground projection, world generation or ordinary-world routing changed.
- Same approved candidate bank passed its source/alpha/assembly audits during
  the attempted build. No PNG changed.

Accepted package and backup SHA256:
`0391BE79B2AB55861DD507C57BEF03DF00D6AA3DD7D9121C97E6FF5E521BCBF9`.
Backup: `C:/Users/max_h/AppData/Local/Temp/ApogeanMidSpacing-0ad62737f0de4fd6a3e5b1dcc398dc75/apogean.tmod`.
Installed spacing package SHA256:
`FE1B5E8B284FE8A21CF3029964967997396D8226F32F1A9D5E0F0642655669FA`.
Successful build mirror:
`C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/ab92c49b7afe46ed9b62c379af310f5e/apogean`.

## Native checks — bounded matrix passes

September7, installed FE1B5E8B package, tModLoader2026.7.3.0, single-player
`Apogee Native Visual V3` with `gg`. Client viewport2560x1369, game/background
zoom1.3333334. Nine unedited window screenshots include the title bar; no
offline compositing, scaling or art substitution. No ordinary world opened.

| Completed case | Result |
| --- | --- |
| run-flat | PASS;3,600px camera traverse;0.600px max response error |
| run-diagonal | PASS;3,600px traverse/128px vertical range;0.940px max error |
| ground-pan-left / ground-pan-right | Both PASS;15 stable Close sections each |
| diagonal-left / diagonal-right | Both PASS;far edge/bottom coverage and modular counts |
| space-ascent / space-descent | Both PASS;Mid/Far cap, geometric exit/return, no altitude alpha fade |

All eight completed cases report zero matrix, depth and count failures, at most
0.01px matrix-position error. Ground and diagonal sweeps each cover128,640 world
pixels:4.853 Far periods,1.801 expanded Mid periods,11.344 Close periods. Do not
describe this as2.5 Mid periods. Native running checks are **simulated camera
paths**, not physical player running; each has27 independently replayed position
differences. They supplement the user's prior movement acceptance.

Replay the1,639 allowlisted records in `native-telemetry.log` with:

```powershell
pwsh -NoProfile -File Tools/Test-WastesRunningLive.ps1 -LogPath Art/Validation/WastesMidSpacing-2026-09-07/native-telemetry.log -Viewport 2560x1369
pwsh -NoProfile -File Tools/Test-WastesHeightLive.ps1 -LogPath Art/Validation/WastesMidSpacing-2026-09-07/native-telemetry.log -Viewport 2560x1369
pwsh -NoProfile -File Tools/Test-WastesProjectionLive.ps1 -LogPath Art/Validation/WastesMidSpacing-2026-09-07/native-telemetry.log -Viewport 2560x1369
```

All three pass. Earlier low-camera coverage and aggregate diagonal failures no
longer reproduce in this exact matrix; retained historical reports are not erased.
The last `right` hold was a short composition inspection, explicitly released
after its screenshot, not an additional full-duration matrix case.

### Visual evidence and limits

- `spacing-review-right.png`: bridge toward left, gas station toward right,
  wider open interval, unchanged tall foreground bank. Main review image.
- `run-flat.png`, `run-diagonal.png`, `ground-pan-right.png`: native ground
  composition around the bridge; foreground still sometimes obscures Mid.
- `space-ascent.png`: surface scenery has left view during ascent.
- `space-descent.png`: layers return with deep foundations visible, no flat
  lower edge visible in this view.
- `ground-pan-left.png` and `diagonal-right.png`: terrain/liquid-obscured
  views, **not** art-coverage screenshots. Kept to make the limit explicit.
- `diagonal-left.png`: descending traversal with world-tree terrain visible;
  Far is visible, nearer layers outside view at this point. This is not proof
  of real-biome routing: the camera lab forces the Forest candidate bank.

The known grove reload guard still reports the same expected87B951FE/actual
EB1D7019 digest mismatch. No grove rebuild, digest substitution or terrain
repair performed. It is not a new spacing crash. Camera checks completed,
then released; QA saved and native main menu observed. Regular worlds untouched.

### Exporter repair discovered by replay

Original-log diagonal replay passed; exported-log replay failed
`missing actual engine matrix sample`. The exporter allowed `V1 PROJECTION:`
but silently dropped `V1 PROJECTION SAMPLE:`. Minimal CLI regression first failed
`MISSING_ALLOWED_RECORD`, then passed after allowing that exact record type.
`Test-WastesTelemetryExport.ps1` now checks eight record types, exact payload and
order, private/unknown startup exclusion, source preservation and overwrite
rejection; included in the Background gate. Synthetic test inputs are kept
outside native evidence. Re-exporting the real log makes the unchanged native
validators pass; no result fabricated or validation threshold weakened. Tool-only
repair, so no new game rebuild was required.

## Next boundary

Native fixture is passed at this viewport, not all-biome production acceptance.
The user has accepted the **new spacing** and requested the next slice.
Grove persistence, additional viewports/zoom/gravity,
multiplayer, real-biome/deep-cave routing and measured performance remain open.
Full Background gate was rerun: all component checks, including the new export
regression, pass except the established production-readiness audit. That audit
remains RED (26 undersized V0 layers and last-row
stretching); this spacing trial does not fix those or promote the optional bank
into normal-world generation. Foreground overlap remains minor deferred polish.
