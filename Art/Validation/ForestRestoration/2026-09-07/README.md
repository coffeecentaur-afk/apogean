# Approved Wastes Mid bank — restoration and Jungle routing

2026-09-07, native tModLoader2026.7.3.0 / Terraria1.4.4.9. This continues the
four-building bank in [the previous native pass](../../WastesMidDepth-2026-09-06/README.md).
No artwork, production routing thresholds or ordinary-world assets changed.

## Result

The unforced Wastes → mixed → green → mixed → Wastes sequence passed. Actual
vanilla PureSpray then restored the terrain and the native forest background.
Jungle subsequently beat the cached green-Forest state, and Wastes returned
when the Jungle fixture was replaced with Wastes. These are real scene counts,
not injected SceneMetrics or forced render-lab selection.

| Evidence | Living / Wastes | State / observed slot |
| --- | --- | --- |
| MidBank-01-Wastes | 0 / 169 | Wastes /18 |
| MidBank-02-MixedWastes | 80 / 89 | Retains Wastes /18 |
| MidBank-03-Green | 169 / 0 | Native green Forest /10 |
| MidBank-04-MixedGreen | 80 / 89 | Retains green /10 |
| MidBank-05-WastesReturn | 0 / 169 | Wastes /18 |
| MidBank-06-GreenBeforeSpray | 169 / 0 | Green /10, setup for both fade directions |
| MidBank-07-Jungle | 0 / 0, cached green | Actual ZoneJungle /20, render lab off |
| MidBank-08-WastesAfterJungle | 0 / 169 | Wastes /18, ZoneJungle false |

IDs are observations for this mod set, not permanent public slot assignments.
The mixed ratio is47.3% in the actual scene sample. Native Forest uses the
installed native/resource-pack scenery (HD Scenery is enabled), NOT new green
versions of the ruined buildings. The approved65%/35% threshold fallback is
unchanged; continuous ecological artwork blending is still deferred.

`Apogean-ForestSpray.json` and `.csv` retain the fresh run:1400 update ticks,
47 owned vanilla PureSpray projectiles,11,592 draw observations,19 incoming and
19 outgoing partial-alpha samples,0 missing draws,0 opacity mismatches.
The fixture started green, rebuilt Wastes, then let actual spray AI convert it.
This is not manual Clentaminator-input, multiplayer, FPS or GPU-residency proof.

## Native pictures and interpretation

- `native-wastes.png`: initial Wastes scene with the accepted garage and Far city.
- `native-spray-green.png`: native Forest after actual purification completed.
- `native-jungle.png`: existing overgrown Jungle scenery wins instead of Wastes.
- `native-wastes-return.png`: Wastes returns after replacing Jungle terrain.

These are unchanged1:1 crops `(160,400,2020,700)` of native2560×1401 window JPEG
captures; client viewport2560×1369. `Test-Crops.ps1` independently compares every
published pixel with its decoded original. No generated scene, resizing,
sharpening or recoloring. JPEG input is not exact source-texture RGB evidence.
The `MidBank-*.png` files are intact3040×992 engine capture-camera panoramas,
not native viewport screenshots; their JSON sidecars/log retain measured routing.

The raised diagnostic strip is at tileY599, while the existing regional-ground
profile is preserved. The Close ledge therefore sits below this artificial
platform. This test does NOT approve natural-world ground alignment. Empty
space below the strip is deliberately cleared QA terrain, not a worldgen hole.
Agent inspection confirms the expected distinct scenery in the native views.
Jungle repetition/lower coverage is existing V0 art, not newly accepted artwork.
Weather readouts vary outside the spray hold; this is not a controlled lighting
matrix or visual approval of every depth join.

## Test-harness repair, not a grove reset

Before this pass, background-routing requests called `ClearFixture()` on the
saved vegetation lab. That would erase its checkpoint record. They now release
only temporary state and retain the record; a pure placement policy keeps the
190-tile-wide strip,96-tile isolation margins and8-tile framing/spray clearance
entirely to one side of the preserved170-tile grove. An impossible site rejects
instead of falling through onto it. No ordinary world-generation rule changed.

72 bounded placement cases pass across small/medium/large world widths, along
with empty-protection stability and impossible-site rejection. Two CLI-injected
bad placement outputs (ignore grove / ignore isolation) reject as overlap or
world-margin failures.25 restoration policy assertions and8 spray-validator
CLI cases also pass. The clean isolated QA build has0 warnings and0 errors.

All9 background request guards retained the same expected/actual pair:

- Expected: `87B951FEB5C9FA9056E12F69F6862C946A7624328210500971B319E56B9929E4`
- Actual: `EB1D7019867ACEFC06C4B7EF93BBF9DA30AD2AA92FC95B5B5BDDE2454FDA939E`

The known reload mismatch still appeared on entry. It remains unresolved and
was neither accepted nor reset. Guards compare before/after fixture setup;
the later Jungle and Wastes guards also show the same snapshot after the timed
spray finished. This is not a fresh saved-grove reload pass. No new rendering
exception was observed, but the entire client log is not error-free.

`telemetry.log` contains35 allowlisted QA records only, excluding account/system
startup diagnostics. The exporter now retains restoration, capture-routing and
grove-guard records in addition to the existing modular-camera records.

## Reproduce and resume

```powershell
pwsh -NoProfile -File Tools/Test-BackgroundFixturePlacement.ps1
pwsh -NoProfile -File Tools/Test-ForestRestoration.ps1
pwsh -NoProfile -File Tools/Test-ForestSprayValidator.ps1
pwsh -NoProfile -File Tools/Test-ForestSprayLive.ps1 -CaptureDirectory Art/Validation/ForestRestoration/2026-09-07 -Replay
```

For new live evidence, run `Test-ForestRestorationLive.ps1` with the five
fixtures/states in the table; use distinct `-EvidenceName` values. Set green
before `Request-LiveValidation.ps1 -Fixture forest-restoration-spray`. Allow
1400 update ticks, validate the fresh report, then request `jungle-routing`
and return to Wastes. Only the named disposable single-player world is allowed.

Current installed QA mod SHA256:
`9C15F8C50ADE16D7517CB14A0BBFC63BA7595D084E6102C50BE8189671A17F29`.
Build mirror: `C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/0bddcd6037554618a242a4727e817131/apogean`.
Pre-entry backup and private native originals:
`C:/Users/max_h/AppData/Local/Temp/ApogeanRoutingQA-284990cf64de47adad6e118f9bc52a14/`.
It holds the old mod, config, QA world/modworld and gg player/modplayer; originals
are under `NativeCaptures/`. The previous build/hash remains documented in the
September6 evidence. Same six optional candidate inputs were passed to the
isolated builder, including `-WastesMidDepthDirectory Art/Candidates/WastesMidDepth-v2/EdgeReview`.

Saved only **Apogee Native Visual V3** and **gg** at13:33:11 local; native main
menu verified. No regular world entered. No settings or enabled-mod list changed
by this pass. Approved art and the two unrelated dirty files remain untouched.

Next: visible cave handoff and measured performance, then remaining viewport /
real-biome / persistence / multiplayer checks needed for promotion. The36.74MiB
raw bank remains an unresolved aggregate budget, not a profiling result. Keep
Wayfinder#11 open, preserve all four approved building designs, and do not start
another art loop. Wastes mobs follow the bounded baseline; no production
promotion or whole-family art acceptance is claimed here.
