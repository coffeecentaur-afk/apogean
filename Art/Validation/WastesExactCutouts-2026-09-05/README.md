# Exact Wastes cutout repair — native QA evidence

**Subsequent user review:** Station accepted; foreground pale perimeter rejected.
The geometry results below remain historical technical proof, not overall visual
acceptance. See [specific findings and next boundary](FOREGROUND_REVIEW.md).

## Result and boundary

The exact two repaired PNGs passed a clean isolated build (zero warnings/errors)
and a bounded native single-player render check on 2026-09-05. This is **fixture
evidence, not user art approval, production routing proof or full release polish**.
No new art was generated and no renderer, world-generation or tree code changed.

The build override packages the candidates only into its temporary mirror. The
installed `apogean.tmod` contains them; repository `Content` PNGs remain unchanged
pending review. A normal build without the override restores the old QA textures.
Ordinary worlds are still excluded from this V1 renderer. Do not promote all
backgrounds based on these results.

## Reproduce the package

```powershell
pwsh -NoProfile -File Tools/Test-WastesExactCutouts.ps1
pwsh -NoProfile -File Tools/Build-ApogeanIsolated.ps1 -WastesCutoutCandidateDirectory Art/Candidates/WastesCutoutRepair-2026-09-05/Exact-v1
```

The build checks each candidate against its passing JSON export report before
copying it. The unchanged source/repair recipe and offline red/green controls are
documented in `Art/Candidates/WastesCutoutRepair-2026-09-05/Exact-v1/README.md`.

| File | Native canvas | SHA256 in test build |
| --- | --- | --- |
| Station.png | 488x1408 | C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C |
| Foreground-Deep.png | 1448x1915 | 4158255600690F0BFA67931C8D3AE0B5FF5E93B3BE5714D360DCB279DD3CC8D4 |

## Native session

- tModLoader v2026.7.3.0, Terraria 1.4.4.9; disposable **Apogee Native Visual V3**,
  character **gg**. No regular world was opened.
- Actual client viewport 2560x1369; game zoom 1.3333334, unchanged. The native
  window capture includes its title bar, so image height is 1401 pixels.
- Backed up this world's .wld/.twld, gg .plr/.tplr, settings and prior .tmod before
  starting. Local backup: `C:/Users/max_h/AppData/Local/Temp/ApogeanCutoutQA-f2ee90db8a724b4b8b19e2d4b30bf025`.
- The camera bridge drove the game itself, not an offline renderer. Its explicit
  Forest-style override means these holds **do not test real biome routing**.
- Saved through `qa-save-and-quit`, observed the main menu, then exited normally.
  Settings comparison against the backup found no changed values.

## Captures and observations

All JPEGs here are unchanged native window-capture payloads. No crop, recolor,
rescale or generated overlay was applied to these screenshots.

| Capture | What was inspected | Limit |
| --- | --- | --- |
| Native-ground.jpg | Station tree connections and close timber rims at ordinary play scale; near lip at the local ground offset | Small branch details still need user visual review |
| Native-wings.jpg | Near bank exits the view on ascent; deep Mid cliffs remain | Does not approve Far composition |
| Native-shallow.jpg | Near bank remains visible at the top during 400px shallow descent | Terrain obscures the lower screen; not cave-handoff proof |
| Native-night.jpg | Dark tint and edge stability | Not a full moon/lighting matrix |
| Native-eclipse.jpg | Warm eclipse tint and edge stability | One viewport and location |
| Native-rain.jpg | Stable layering under storm dimming | Darkness limits fine branch inspection |
| Native-diagonal-right.jpg | High-flight frame during the rightward sweep; Mid/Close recede | Full traversal is evidenced by telemetry, not one screenshot |
| Native-restored-after-sweep.jpg | Original scene restored after timed camera release | This is **not** a sweep frame |

The repaired Station branches look connected at gameplay scale; pale close timber
rims are less prominent. Existing Far lower strata look repetitive/heavy and
remain deferred art polish, unchanged by this patch. A bounded visual observation
does not certify every edge. The broad offline detector still reports 275 pale
boundary candidates and a separate small terrain sprig; legitimate highlights
must not be erased to manipulate that count.

## Geometry and regression checks

`Live-telemetry.log` contains 58 allowlisted background QA records only, not the
full client startup/account log. The actual CLI passed all eight named cases:
ground, wings, below-ground, night, eclipse, rain, diagonal-left, diagonal-right.

- Zero submitted geometry failures in all eight cases; max matrix pixel error
  0.01px. Max anchor rounding error 0.96px.
- Each physical diagonal sweep covered 128640 world pixels: 3.455 Far periods,
  8.794 Mid periods and 18.844 Close periods. Both coverage checks passed.
- Far edge gaps remained zero in telemetry. The smallest Far bottom margin was
  0.01px during travel. This tests rectangle coverage, not seamless art quality.
- 56 offline modular layout checks plus short-depth rejection passed.
- 90 synthetic ground-anchor combinations and column/interpolation checks passed.
- Actual live-validator CLI accepted its control and rejected six deliberately
  bad traces (missing, zero, bad count, shifted pixels, anchor, short travel).

```powershell
& ./Tools/Test-WastesModularLive.ps1 -LogPath Art/Validation/WastesExactCutouts-2026-09-05/Live-telemetry.log -Cases @('ground','wings','below-ground','night','eclipse','rain','diagonal-left','diagonal-right')
```

That command rechecks **archived** telemetry; it does not launch a fresh test.

## Known failure retained; next gate

The previously recorded preserved-grove digest mismatch recurred at load:
expected `87B951FEB5C9FA9056E12F69F6862C946A7624328210500971B319E56B9929E4`,
actual `EB1D7019867ACEFC06C4B7EF93BBF9DA30AD2AA92FC95B5B5BDDE2454FDA939E`.
Its guard logged an InvalidOperationException and **did not rebuild the grove**.
The game continued; no new engine crash was observed in this session. Do not
reset the expected digest or call this a clean persistence test.

Next: user review of these exact rendered cutouts, then a separately bounded
real-biome/restoration transition check without rebuilding the preserved grove.
The existing restoration fixtures clear a substantial terrain rectangle, so
they were not invoked as an incidental part of this camera-only pass. Earlier
restoration evidence remains historical, not relabeled as fresh repaired-art QA.
Visible deep cave handoff, other viewports, multiplayer, production ground-profile
capture/synchronization and measured GPU/hitch budgets remain open. No new bank
variants, new biome art or starting-area implementation was begun in this pass.
