# Wastes Mid ruins — native ground-scale review

Actual tModLoader window captures, not offline mockups. **Scale-only fixture**:
the user could not judge the offline board without seeing the character in-game.
This pass supplies that reference; it does not approve final composition.

## Inspect

- `checkpoint.jpg`: existing Station on the left; opposite-facing checkpoint on
  the right; actual player gg stands between them.
- `motor-depot.jpg`: same view with the low service-bay ruin.
- `broken-shell.jpg`: same view with the two-storey broken shell.
- `baseline-return.jpg`: normal Mid/Close scenery restored after the gallery.

Images are unretouched native-window JPEGs at2560x1401 including the titlebar.
Game viewport2560x1369, game/background zoom1.3333. They are scale/silhouette
evidence, not lossless pixel-color analysis. The standing player has a20x42
world-pixel body (about27x56 display pixels before equipment). Assets render
one source pixel per display pixel, retaining the existing background contract.
An existing tree partly overlaps each right-hand ruin; no tree was removed.

## Fixture scope

Only **Apogee Native Visual V3** single-player accepts the three `scale-*`
cases. They read the existing floor at world center, position the real player
on it, fix daytime and draw two studies behind normal terrain/entities. Soil
lips share a datum48 world pixels above that floor. Normal Mid/Close are
temporarily suppressed; Far and terrain remain. Holds expire after1800 ticks,
restoring prior player/time/scene state. The fixture writes no tiles.

These512x460 upper studies are deliberately **not**1408px-deep Mid modules.
Their lower120 rows are hidden by the flat QA floor. No aerial/diagonal view,
deep-foundation continuity, ordinary placement, restoration or biome-routing
claim follows. Fixed gallery positions are not seeded placement.

## Provenance and build

- Inputs: `../../Candidates/WastesMidRuins-v2/ScaleStudy-v3/`. Ruins use an
  explicit nearest-neighbor2/5 reduction; Station remains1/1. Not new detail.
- One targeted checkpoint edit exposes the other side while retaining upper-left
  light. Reviewed source-specific mask preserves dark recesses. Every pixel in
  source rows740–1535 matches the old clean cliff. Original files retained.
- Isolated build:0 warnings/errors. Installed QA mod SHA256:
  `D17A0EEFEADA32A1FA255691FDE31EFB219910253CAE7E07646AFD21BB9BA78F`.
- Existing Station, Close-v2 and Far-city hashes unchanged. Optional scale art
  enters only the temporary build mirror, not repository Content/normal routing.
- Four additional512x460 RGBA textures cost3.59375MiB raw. The prior28.36MiB
  scene counter excludes this diagnostic cache; combined raw texture data is
  approximately31.95MiB. Not measured GPU memory or performance.

```powershell
pwsh -NoProfile -File Tools/Build-ApogeanIsolated.ps1 -WastesCutoutCandidateDirectory Art/Candidates/WastesFarCity-v1/QA-Package-v1 -WastesCityCandidateDirectory Art/Candidates/WastesFarCity-v1/Runtime-v1 -WastesScaleCandidateDirectory Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3
pwsh -NoProfile -File Tools/Request-WastesCameraCheck.ps1 -Case scale-checkpoint
# Also scale-depot and scale-shell. Wait for a fresh log acknowledgement.
pwsh -NoProfile -File Tools/Request-WastesCameraCheck.ps1 -Case ground
pwsh -NoProfile -File Tools/Request-LiveValidation.ps1 -Fixture qa-save-and-quit
```

## Checks and remaining gates

`Test-WastesMidScaleStudy.ps1` passes alpha, exact source sampling, scale/datum
and preserved-cliff checks. Four real CLI fault runs reject matte leakage,
wrong sampling, a changed cliff and shifted datum (mutations are in-memory).
`Test-WastesMidScaleLive.ps1` accepts filtered native `scale-telemetry.log`:
both expected assets drawn in all three cases, zero position/dimension failures,
completed nonzero sample counts and a passing normal-module return. Its actual
CLI rejects missing sample, missing release, wrong asset and draw-failure traces.
Telemetry validates submitted coordinates/dimensions, not aesthetic quality.

Ground return:89364 module matrix checks,29788 module frame checks,0 failures;
max pixel error0.01. Prior Close ground-lock probe passes within0.83px. Gallery
generic ground-lock/module counters are zero by design, not coverage proof.
Existing18180 Space-policy checks and their five defect controls pass again.

Known pre-existing issue: grove checkpoint expected87B951FE… vs actualEB1D7019…
again prevented automatic rebuild on load. No reset/rebuild was performed.
No new gallery exception observed. Broad Background production gate remains
separate from this bounded fixture.

Full Background gate rerun: mask/export, city mask, Space policy/controls,
restoration policy and HD contracts pass. Production readiness remains RED for
existing source-dimension targets and the generic final-row stretch. No gate
threshold was weakened, and this fixture does not fix those production issues.

Pre-run mod/config and disposable world/player backups: local temp
`ApogeanMidScaleQA-67a033dc4ef84d9688257947c7a58ef3`. Allow-listed save/quit
completed and validated the world at20:27:05 local. The client returned to its
menu. Exit inputs did not close it, so it was left at the saved main menu, not
force-killed. No ordinary world was opened.

Next: user judges sizes/facing, then deep foundations and stable quiet-spaced
placement. Complete flight/lighting/routing/residency checks before promotion.
Keep Wastes → starting area/drop pod → shallow Maw order. Jungle remains a
separate green, overgrown survivor brief.
