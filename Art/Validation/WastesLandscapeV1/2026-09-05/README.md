# Wastes background validation — 2026-09-05

Outcome: **native purification and outgoing fades pass at 1080p and 1440p after reproducing and fixing a stale-opacity defect. Background art remains pending, not promoted.** No production PNG, tree, terrain atlas, progression, or world-generation code changed.

## Live red → green

The actual installed tModLoader v2026.7.3.0 / Terraria 1.4.4.9 client ran `forest-restoration-spray` in **gg → Apogee Native Visual V3**, single-player. It builds an isolated real Wastes floor, then sends 47 owned native `ProjectileID.PureSpray` projectiles through it. Nearby counts changed from living0/Wastes169 to living169/Wastes0. Selection was not forced. These are native projectile/conversion tests, **not manual Clentaminator-input or multiplayer tests**.

| Evidence prefix (JSON + complete draw CSV) | Viewport | Incoming / outgoing partial samples | Mismatch | Result |
| --- | --- | --- | --- | --- |
| `spray-red-original` | 2560×1369 | 19 / 19 | 38 | Fail; outgoing diagnostic preview also contaminated the first measurement |
| `spray-red-production` | 2560×1369 | 19 / 19 | 19 | Fail after removing the diagnostic preview from the starting state |
| `spray-green-1920x1080` | 1920×1080 | 19 / 19 | 0 | Pass; zero missing draws |
| `spray-green-2560x1440` | 2560×1440 | 19 / 19 | 0 | Pass; zero missing draws |

The minimized red trace shows engine opacity falling 0.95 → 0.05 while the outgoing Wastes draw stays at1. The installed loader calls `ModifyFarFades` only on the selected style, but continues drawing outgoing styles. Caching opacity in that hook cannot follow an outgoing fade. The fix reads `Main.bgAlphaFrontLayer[Slot]` at draw time for both the normal custom styles and the diagnostic preview. The engine still advances the array exactly once. The V0 solid underfill now preserves tint alpha instead of discarding it through `ToVector3`; a separate frame-by-frame V0 underfill pixel test remains open.

The recorder identifies the actual production style slot, counts missing draws as failures, and requires both directions. `Tools/Test-ForestSprayLive.ps1` independently recomputes the result from CSV. `Tools/Test-ForestSprayValidator.ps1` passes8 synthetic CLI checks: valid control, stale outgoing opacity despite a green report, missing draw, no outgoing transition, false report, old evidence, forced preview, and NaN. Synthetic files are explicitly not live evidence.

Reproduce in the unpaused, named disposable world:

```powershell
pwsh -File Tools/Request-LiveValidation.ps1 -Fixture forest-restoration-spray
# After the 1400-tick run completes:
pwsh -File Tools/Test-ForestSprayLive.ps1
```

Replay retained evidence without claiming a new test:

```powershell
pwsh -File Tools/Test-ForestSprayLive.ps1 -CaptureDirectory Art/Validation/WastesLandscapeV1/2026-09-05 -EvidenceName spray-red-production -Replay
# Expected failure: incoming19/outgoing19/mismatch19/missing0.
pwsh -File Tools/Test-ForestSprayLive.ps1 -CaptureDirectory Art/Validation/WastesLandscapeV1/2026-09-05 -EvidenceName spray-green-2560x1440 -Replay
```

`spray-fixed-before-*` and `spray-fixed-after-*` are original Windows game captures. The restored scenery is Terraria's selected forest slot as drawn with this installation's existing resource packs (including HD Scenery); it is **not** the new restored-landscape concept. After captures occur after the probe restores the previous weather, hence rain can return. The conspicuous floating floor is the isolated test strip, not generated terrain.

## Travel and join inspection

The original physical sweeps at2560×1369 covered3.455 Far repeats in both directions. Their camera crossed foreground terrain and hid large parts of the artwork. Raising only the physical sweep by2400 world pixels helped some sections but did not remove the problem: ocean endpoints and tall terrain still obscure the scene. This is a fixture limitation, not a visual pass.

| Case | Viewport | Actual sampled travel | Far / Mid / Close repeats | Interpretation |
| --- | --- | --- | --- | --- |
| Raised physical right | 1920×1080 | 128921.0 | 3.462 / 8.813 / 18.885 | Range pass; occluded visual coverage |
| Raised physical left | 1920×1080 | 129064.0 | 3.466 / 8.823 / 18.906 | Range pass; occluded visual coverage |
| Isolated phase right | 2560×1440 | 128425.6 | 3.449 / 8.779 / 18.812 | Range pass; actual renderer with diagnostic horizontal input |
| Isolated phase left | 2560×1440 | 128425.6 | 3.449 / 8.779 / 18.812 | Range pass; actual renderer with diagnostic horizontal input |
| Isolated phase right | 1920×1080 | 129280.0 | 3.472 / 8.838 / 18.938 | Range pass; actual renderer with diagnostic horizontal input |
| Isolated phase left | 1920×1080 | 129280.0 | 3.472 / 8.838 / 18.938 | Range pass; actual renderer with diagnostic horizontal input |

Physical ranges use observed draw positions, not planned endpoints. Draw counts include repeated draws within a game tick and are not unique simulation frames. The isolated modes keep the physical camera/player at the clear central fixture and vary only the horizontal input to the same V1 renderer. They do not carve the world or impersonate world traversal. The logs explicitly mark `isolatedPhase=True` and still report `artApproval=False`.

`Tools/Request-WastesCameraCheck.ps1 -Case phase-right` / `phase-left` runs30 seconds of phase travel and10 seconds of endpoint hold. `release` restores the prior position, time/weather and diagnostic override. This mode is guarded to the named QA single-player world. Real-world routing, ordinary-ground composition and altitude tests remain separate. Camera probes now also refresh breath to avoid drowning at physical ocean endpoints.

The twelve `isolated-phase-*` captures were taken at approximately2.5/16.5/32.5 seconds after each request (three per direction and viewport). They show layer movement through distinct phases with authored ground reaching the bottom in these samples. They also show **recognizable repeated roadside buildings/vehicles, heavy soil cross-sections and flat rock bands**. These need an art pass and review. A matched edge column alone is not an invisible join. The wide sweep is not approval of every intermediate frame or the full lighting matrix. The1080p endpoint logs are retained below from `client2.log`; the second client was launched while the saved1440p client was still at its menu, then the old client was closed and confirmed absent before loading the QA world.

```text
[11:01:19.567] [Main Thread/INFO] [apogean]: WASTES V1 CAMERA: case=phase-right; x=66240; y=8990; viewport=1920x1080; hold=2400 ticks; production routing=False
[11:01:59.552] [Main Thread/INFO] [apogean]: WASTES V1 SWEEP: case=phase-right; isolatedPhase=True; drawnFrames=20006; sampledTravel=129280.0; farRepeats=3.472; midRepeats=8.838; closeRepeats=18.938; coveragePass=True; artApproval=False
[11:02:25.703] [Main Thread/INFO] [apogean]: WASTES V1 CAMERA: case=phase-left; x=66240; y=8990; viewport=1920x1080; hold=2400 ticks; production routing=False
[11:03:05.684] [Main Thread/INFO] [apogean]: WASTES V1 SWEEP: case=phase-left; isolatedPhase=True; drawnFrames=20008; sampledTravel=129280.0; farRepeats=3.472; midRepeats=8.838; closeRepeats=18.938; coveragePass=True; artApproval=False
```

`raised-pan-left-*` contains four timed physical-traversal samples including the obscured ocean endpoint. Other early captures taken before request consumption or after expiry have been accurately renamed `before-pan-*` / `after-pan-*`; they are not traversal proof. Original JPEG bytes are preserved, including window chrome and installed UI/resource-pack art.

## Routing, scale and resource checks

- Unforced Jungle after a restored forest still selected Jungle slot20 (`ZoneJungle=True`, render lab off) at1440p, logged10:47:28 local. The cached living-forest state did not override it. `jungle-after-restoration-2560x1440.jpg` shows the existing Jungle V0 artwork, **not** an approved new family. The capture-camera probe used water style0 and completed without an exception.
- The three2048×1280 V1 hashes exactly match the9/4 record. Static layer checks pass at30MiB raw RGBA. The old27-layer V0 asset contract still passes, but does not certify native scale or final coverage.
- The landmark checker previously failed under PowerShell7 because of `System.Drawing` compiler type forwarding. The same pigment measurement now uses the loaded drawing API directly: archived corrected image111/111px passes; archived pre-fix image111/150px still fails. No test thresholds or image pixels changed.
- Forest-restoration state tests pass25 cases; sweep arithmetic passes14 cases. Both are code checks, not screenshots.
- Two isolated builds succeeded with zero warnings/errors; the last C# build was7.81s. The final1440p runtime log contains no exception, only existing missing `icon_small.png` warnings for apogean/CheatSheet.
- One Windows process-memory sample at10:47:18 local (after returning to restored scenery): working set1,968,795,648 bytes; private bytes3,565,719,552; dedicated GPU usage1,508,798,464; shared10,285,056; GPU total committed1,571,119,104. **Whole-client snapshot only:** includes Terraria, active mods, UI/resource packs and render targets. It does not isolate the30MiB candidate, prove leak freedom, or measure transition hitches. The candidate-specific residency/hitch gate remains open.

## Safety and next independent work

Only the approved disposable world was loaded. Fresh backup `C:/Users/max_h/AppData/Local/Temp/Apogean-BackgroundValidation-20260905-ec2259734e0a4121bc02d5a7c211a031` preserves the original QA world pair, gg player pair and pre-test config. QA entry rebuilt the vegetation fixture before these tests; this session does **not** claim saved-grove persistence. Spray/Jungle fixtures changed only the disposable world.

The client was saved and closed, and display settings returned to windowed2560×1369, non-maximized/non-borderless. No production candidate promotion, new world generation, scheduled automation or new dependent content was performed.

Next: complete remaining scene-priority/boundary and altitude/lighting checks; design lower terrain and repetition improvements as reviewable candidates before replacing approved source art. Profile candidate-specific load hitches/residency. Keep continuous restoration-mask blending, other biome families, underground layers and world-scale art acceptance explicitly pending. User is away and authorizes independent checks/tooling while artistic review waits.
