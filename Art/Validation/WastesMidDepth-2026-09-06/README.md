# Combined Wastes Mid bank — bounded native evidence

The user approved the corrected checkpoint and broken-shell TOPS. This pass
keeps all four building designs, assembles deeper terrain beneath them and
installs a QA-only spaced bank. Source, mask review and reproducible recipe:
[Candidate](../../Candidates/WastesMidDepth-v2/README.md).

## What the pictures show

- `ground.png`: garage left, broken shell right, real player near center.
- `checkpoint-sequence.png`: shell and checkpoint in the real draw sequence;
  an existing Close tree overlaps the checkpoint. It was not erased.
- `station-sequence.png`: highway and approved Station in that same sequence.
- `below-ground.png`: Close remains at the ground datum during400px descent.
  Real soil/walls obscure deeper scenery; this is NOT visible cave-handoff proof.
- `space-fade.png`, `space-edge.png`, `sky.png`: partial terrestrial fade and
  complete absence at the native Space boundary and upper sky.
- `night.png`, `eclipse.png`, `rain.png`: bounded ground lighting samples, not
  every landmark under every weather state.

All are1:1 crops of native2560×1401 window JPEG captures, not generated images
or offline reconstructions. Game viewport2560×1369, zoom1.3333. Background
assets draw at1 source pixel per display pixel. Private originals stay in the
local backup's `NativeCaptures/`; crops omit unrelated overlays. `Test-Crops.ps1`
compares every published pixel with the same-coordinate decoded original. No
resize, repaint, sharpening or recoloring. JPEG input is not pixel-exact proof
of texture RGB. Some private captures occurred before a request took effect or
after a hold expired; those are deliberately NOT published as that case.

Agent inspection: the buildings keep their approved silhouette and scale.
Open intervals expose Far; no recurring rectangular matte or continuous pale
building rim is obvious in these ground samples. Foreground overlap is real.
Sparse lower-edge specks, reused lower pipe motifs and overall density remain
review/polish targets; the ground views do not reveal every deep cliff pixel.
Night is deliberately dark and still needs user readability judgment.

## Technical result

Clean isolated build: **0 warnings,0 errors**. Installed QA mod SHA256:
`72D2CD4FB60D92B1099E740CC912069B9AC1D657DA1F4541AB64C1A3EB2320F7`.

Fresh camera records are in `telemetry.log`; this is an allowlisted export,
not a copy of account/system startup diagnostics. LAND sample/results were
added to the exporter so actual submitted-color Space checks are retained.

- Four exported512×1408 candidates pass the independent720896-pixel audit
  each;225792 immutable upper pixels each. All four final PNG hashes reproduce
  exactly from a fresh assembly/export directory.
-176 independent layout cases; actual CLI faults MissingGroup, PhaseJump and
  WrongAsset reject for the expected reasons. Architecture/SoftAlpha/Support/
  Sample mutations likewise reject through the actual assembly CLI.
- Existing56 modular-layout and92 camera-projection arithmetic checks pass.
- Ground, shallow descent, both diagonal directions and both isolated phase
  sweeps pass submitted counts/positions and the fixed Close anchor. Maximum
  submitted-coordinate error0.01px; maximum anchor rounding0.96px.
- Each full sweep spans128640 camera-input pixels,4.853 Far periods and
  **2.648 new Mid periods**, with zero reported failures. Phase sweeps change
  the renderer's horizontal sample only; diagonal sweeps move the actual camera.
  Neither is a claim of manual flight-input testing.
- Ground / space-fade / space-edge / sky / full ascent / full descent pass the
  actual submitted-color validator. Terrestrial alpha is0 at Space, partial
  Far samples occur en route, and opacity changes monotonically in both flights.
- Lighting records cover night, eclipse and rain at the default ground view.
  Some holds end via the next bounded request; results reflect actual recorded
  draws, not an assumed30-second duration.

```powershell
pwsh -NoProfile -File Tools/Test-WastesModularLive.ps1 -LogPath Art/Validation/WastesMidDepth-2026-09-06/telemetry.log
pwsh -NoProfile -File Tools/Test-WastesSpaceLive.ps1 -LogPath Art/Validation/WastesMidDepth-2026-09-06/telemetry.log -Viewport 2560x1369
```

## Scope, budget and remaining gates

The expanded loaded set reports **36.74MiB raw RGBA**, versus28.36MiB before
the additional ruins. A representative Far/new-Mid/Close triple is23.29MiB and
passes its dimension/budget check, but that is NOT the entire set. No32MiB
aggregate limit was silently waived. GPU residency, frame-time comparison and
an aggregate production budget remain unresolved. No memory/performance promise.

Current status is bounded native QA evidence, not polished/integrated Wastes:

1. User review of the full-depth joins and combined spacing remains separate
   from the already accepted upper style. Do not ask them to approve the same
   four building designs again or regenerate those designs.
2. Recheck unforced greenification and Jungle/other scene priority with this
   exact bank. This run forces the Forest lab while testing camera behavior.
3. Visible cave handoff, additional viewport/zoom conditions, saved profile,
   multiplayer/gravity and measured performance remain open.
4. The old QA grove digest mismatch still blocks automatic terrain-lab rebuild:
   expected87B951FE… / actualEB1D7019…. It was not bypassed, reset or rebuilt.

No new background-render exception was observed. The known grove error remains
an error; do not describe the complete client log as clean. Steam automatically
updated CalamityOverhaul0.9206→0.9207 on this restart; this is a changed dependency
relative to the previous run, not an Apogean code change or broad compatibility
certification.

## Local checkpoint

Only **Apogee Native Visual V3**, single-player, player **gg**, was opened.
No ordinary world was opened or modified. Camera tests did not carve terrain;
ordinary simulation/autosave continued in the disposable world.
Save/quit completed at23:45:19 local and main menu was visually verified.
Installed mod hash remained identical after the run. All ten published crops
pass the decoded-pixel comparison;649 QA-only telemetry records are retained.

Backup under local temp: `ApogeanMidDepthQA-e98329ef95de4f4198596e59e11da7c3`
(previous mod, config, gg/maps, disposable world). Previous mod hash:
`1CBA441BE9DCF30507F5D6653A05B8B0366FDC48C9B9CAB6837B3D1D5CF48CF6`.
Build mirror: `ApogeanTmlBuild/0dfadca99fe9439c805627f990902e21/apogean`.
Independent repeat output: `ApogeanDepthRepeat-b299d7c7cf3043dc9453467bc7cee0bf`.

Rebuild with the existing city/cutout/scale/depot/Station options plus
`-WastesMidDepthDirectory Art/Candidates/WastesMidDepth-v2/EdgeReview`.
Normal repository Content PNGs remain unchanged. The optional bank is all-or-none
and still restricted to the existing QA/render-lab route. Wayfinder #11 remains
open. Finish the Wastes baseline checks before Wastes mobs; do not start other
biome production or the arrival-pod/shallow-Maw slice from this evidence alone.
