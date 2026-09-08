# Arrival pod — exact coarse A5 candidate

September 8, 2026. **Installed in isolated QA package for user-requested native
appearance review; not approved or promoted to production.**

The user needs an in-game view to judge scale. This exact unchanged candidate
now renders in the existing Apogee Native Visual V3 fixture. Build passes with
zero warnings/errors; saved pod checkpoint is unchanged. Day/player-overlap/night
captures and scope are recorded in
[native review evidence](../../../Validation/ArrivalPod-v2-2026-09-08/README.md).
All 408 other Content PNGs match the prior package mirror. No worldgen or divot.
The static preparation below is history; context-scale.png is still offline,
not the new live evidence. Do not require another offline approval loop.

The user agrees to the damaged-but-survivable pod and shallow generated impact
divot. The opened hatch, occupant cradle, broken roof, broad scorch and dirty
base remain recognizable. The divot is real terrain separate from furniture:
moving the item leaves the impact scar and never moves spawn.

## Review files

- `preview.png`: actual 80x96 pixels on the left; exact 4x pixels on dark/light
  to the right. The small outline is a **20x42 collision guide**, not a player
  sprite. No smoothing or checkerboard matte in the asset.
- `context-scale.png`: unchanged 320x200 crop from the September 7 native
  `inside.png` (source origin 1180,590), including the real player and OLD pod.
  NEW candidate occupies a separate labelled panel. Its approximate 4/3 screen
  scale is inferred from 14-tile (224-world-pixel) fixture spacing in that
  capture. This is an offline comparison, NOT a native render, lighting proof
  or precise camera calibration for the new art.
- `ArrivalPod.png`: 80x96 transparent native candidate, 19 used opaque colors.
  Fitted silhouette is 80x80 with a 16px transparent upper margin. The squarer
  A5 proportions are preserved within integer fitting; it was not stretched
  to fill the full height. Bottom row has 18 opaque contact pixels.
- `ArrivalPod_Tile.png`: 90x108 atlas: 5x6 cells, each 16x16 with 2px padding.
  Every cell reconstructs the native image exactly; padding is transparent.

## How it was made — reproducible, with explicit loss

Built-in image edit produced `mask-proposal.png`, not replacement color art.
The first request failed with HTTP 500; one retry succeeded. No API/CLI fallback.
Full final prompt: [MASK_PROMPT.md](MASK_PROMPT.md).

The proposal kept coordinates but incorrectly drew black interior lines and
removed part of the dark cockpit. The deterministic preparation closes three
reviewed A5-only source-coordinate base/hinge joins and fills enclosed mask holes
using a four-connected exterior flood. Source pixels are not repainted. The
resulting `mask.png` and exact-source-color `cutout.png` were visually inspected.
The independent ground, grasses and scattered fragments are excluded. Attached
soil remains on the damaged base, not as a flat movable terrain patch.

The cutout THEN undergoes **lossy** centre nearest-neighbour fitting into a
40x48 logical canvas and a fixed 20-color, weighted-nearest material palette,
with no dithering. Expand each logical pixel to exactly 2x2 native pixels.
Only 19 palette colors are used. This is a declared sampling reduction of the
approved concept; it is not a lossless export or independently hand-pixelled
sprite. Shared atlas packing never rescales individual furniture cells.

```powershell
# Destination must exist and contain none of the generated output files.
pwsh -NoProfile -File Tools/New-ArrivalPodNative.ps1 -Variant Native-v2 -OutputDirectory <new-empty-candidate-directory>
pwsh -NoProfile -File Tools/Test-ArrivalPodNative.ps1 -Directory <candidate-directory> -PixelClusterSize 2
pwsh -NoProfile -File Tools/Test-ArrivalPodNativePipeline.ps1
```

## Checks actually run

- Static: dimensions, RGBA hard alpha/zero hidden matte RGB, color budget,
  one eight-connected silhouette, grounded base, 30-cell round-trip, clean
  padding, every opaque AND transparent 2x2 cluster identical.
- Red control first: original valid Native-v1 fails `COARSE_PIXEL_GRID` at
  (31,6). This proves fewer colors alone cannot satisfy the new pixel rule.
- Full CLI pipeline: both original/coarse variants and repeat exports pass;
  11 original missing/defective input cases plus 3 coarse controls rejected
  (old dense art, a single changed color pixel, a single alpha hole).
- Both variants refuse existing output and production Content paths. Five
  original image hashes and six coarse image/two report hashes reproduce;
  all 19 pinned source/output files remain unchanged.
- An early host-script compile failed for a missing `System.Collections`
  reference, before writing images. The explicit reference fixed it. This
  was not a game crash or a tModLoader build.
- Shared mask-export regression passes the actual PNG export, deterministic
  pins and 11 rejected input/output defects. Status gate passes all 37 family
  states, 8 versioned skills and generator ownership.
- The broader Structure gate was also run: pod static replay, archived native
  log validator and Helix construction checks pass. It remains RED on the same
  two pre-existing WorldVisualIntegrity expectations: an obsolete exact
  Request-LiveValidation enum literal and DeadForestTree's 5 colors versus an
  expected minimum of 8. Neither unrelated test nor accepted tree was changed.

The old Native-v1's 58 game-API checks do NOT certify new appearance. At the
static-preparation checkpoint no game/world/Content/installed package changed.
The subsequent user-requested native review above adds explicit Native-v2 build
pins and uses the existing fixture without rebuilding it. Mechanical checks
are not relabeled as newly executed; artistic acceptance remains pending.

## Pinned inputs and outputs

| File | SHA256 |
| --- | --- |
| A5 master | `3EEC46AFFC7BB149B6A38528AFB93ABB79E27D24E61D97995A5A18768AEB1C67` |
| Mask proposal | `962B2BDAB1B4D381E1B8B8F4C4EB26777BE4D57D95751A5B460013F630527BB6` |
| Prepared mask | `5F511ED81595102D60D92CE822F0D090F1E1D57DF317AF56B14BB2069AA853BF` |
| Exact-color cutout | `B61B625BCBEACBC43FDD34AE9C031E5427792A635E3357FFE4574FE508E6B734` |
| Native art | `822A50B8EE634A671281F2CFC5AA3EE760031E0DA3757246C8216911D83B6407` |
| Framed atlas | `6A60495AC6F473FFF6237B9BFDAC1AB88622CD2AC3794C148C4100605F1A65DC` |
| Historical capture | `43EF92098AE961F7C25CC06535752FDD4DF3B9DC144469C6B90057ED08EE69F4` |

`native-report.json` and `cutout.png.report.json` record the geometry, mask
corrections, declared loss, hashes and exact-color export proof.

## Reusable lesson

Separate art direction, transparency, logical pixel density, frame packing and
live appearance into distinct checks. An opaque color master can survive an
imperfect AI selection if corrections are bounded and source RGB is retained.
Enclosed-hole fill is appropriate here because the cockpit has a dark backing;
it is **not** a general rule for furniture with real see-through windows. Never
reuse A5-specific mask coordinates on another source. Preserve independent
terrain and non-movable debris outside the placeable object's atlas.
