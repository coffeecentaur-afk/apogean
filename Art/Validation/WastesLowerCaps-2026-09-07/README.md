# Lower flight caps — September7

Latest user clarification: lower the **maximum flight-height caps**, not the
ground-level scenery placement. This supersedes the intervening interpretation
about moving scenery downward. No ground-level offset was implemented.

Bounded tuning trial: Mid25% (previously1/3), Far50% (previously70%) of the existing
global ground-to-Space ascent reference. Horizontal parallax, ground composition,
fixed Close anchors, native scale, artwork and biome/restoration opacity are
unchanged. Lower altitude means a larger world-Y coordinate; this is not a
smaller world-Y cap. The artwork remains opaque and moves out of view sooner.
Exact fractions are trial tuning, not yet user-reviewed composition.

The revised actual-policy assertion failed before the change: surface250 Far
cap1945.6 versus required2304. Seven source-mutation controls include returning
to the previous higher ceilings. Live trace validation now rejects the old cap
profile; replay old evidence with its original revision0ec6d6c.

Build succeeds with0 warnings/errors.3150 height assertions,5804 fixed-section
assertions,92 projection,56 layout,72 ground-lock and25 restoration checks pass.
Seven source mutations are rejected. These are not live visual approval.
Mirror: `C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/7fb3cd04010d418aaf4326247f8fd107/apogean`.
Installed package SHA256:
`1F3FE9D74AC218CD551040EE1B7E3FCFFEC9A74938C0126E723F3F95D4E9DD63`.
All six candidate override inputs are identical to the prior height-lock build;
their existing pixel-provenance checks also pass during packaging. No new art.

## Native comparison

Only Apogee Native Visual V3 was opened, with the existing gg character. The
viewport is 2560x1369 at game zoom 1.333; screenshots include the native title bar
(2560x1401). Files are unedited window captures, not offline approximations.

| View | Before | After | Measured result |
| --- | --- | --- | --- |
| Ground | [Before](before-ground.png) | [After](after-ground.png) | Far top 30.923px and Mid top 271.036px are unchanged. |
| Mid flight | [Before](before-mid-altitude.png) | [After](after-mid-altitude.png) | At camera-center Y8054.5, Mid top moves from 338.386px to 397.688px; Far is unchanged. |
| High flight | [Before](before-high-altitude.png) | [After](after-high-altitude.png) | At camera-center Y4912.5, Far top moves from 777.771px to 2346.458px: the lingering city is now entirely below the screen. |

These are identical camera centers in the before/after logs. Clouds, enemies,
weather UI and player animation are not pixel-identical between runs. A read-only
six-sample comparison checks matching camera centers, physically lower caps, and
unchanged ground composition. The QA world's actual caps move from Y7605.333 to
Y8100 for Mid, and Y5428.8 to Y6616 for Far. Larger Y means lower world altitude.

Both complete native space flights pass `Test-WastesHeightLive.ps1` with
`-Cases space-ascent,space-descent`. Ascent: 29,932 position checks, 22,742 capped,
19,926 exited. Descent: 29,940 position checks, 12,785 capped, 9,963 exited.
Alpha, position, fixed-anchor and modular results all have zero failures in these
two cases. All observed altitude-opacity factors stay 1.00000; geometry exits
and returns instead of fading. The unchanged native trace is accepted and five
deliberately incorrect trace variants are rejected, including the old cap profile.

`Before-2560x1369.log` (1,524 records) and `After-2560x1369.log` (1,602 records)
contain allowlisted QA telemetry,
not raw startup/account/system logs. Before includes historical pan/flight tests
from the earlier package plus this turn's three comparison positions. Run old
traces with their original validator revision, not the new cap-profile check.

## Remaining gates

The previous low-view Close depth failure remains open; lowering Mid/Far ceilings
cannot repair that separate layer. Ground still produces modular coverage
failures (14,946 in the complete ground hold; 7,739 in the shorter repeat used
for the screenshot), while ground projection error stays below 1px. A flight-only
pass is not a full fixture pass. The family remains rejected for coverage, not
for user art rejection.

Both diagonal traversals completed their full 128,640px sampled travel and
endpoint hold. Each covered 21 fixed Close sections, with zero alpha, cap-position
or anchor failures. Projection maximum error is 0.01px and ground-lock error is
0.84px. The aggregate modular check still reports 41 rightward and 42 leftward
failures. Its counter combines module counts and finite-depth exposure: this
trace does not isolate every diagonal failure's cause. Keep that diagnosis open;
do not describe diagonal rendering or the full fixture as passed. Actual terrain
also occludes parts of the route. No coverage check was suppressed to pass it.

The overall Background gate remains RED for 26 undersized legacy V0 layers and
the legacy renderer's last-row coverage stretch. The HD contract itself passes
27 layers across 9 biomes (162.01 MiB). No gate was weakened. The existing grove
reload digest mismatch was reported again; no grove reset/rebuild was performed.
No ordinary-world, other-viewport, multiplayer or production-art promotion.
Exact 25%/50% composition tuning still awaits user inspection.

The QA save-and-quit request was consumed at 15:47:31 local time; player/world
backup, world-save validation and modded-world save were logged. The native main
menu was visually verified afterward. The new package remains installed for QA.

Reproduce the passing flight-only validation from a PowerShell prompt:

```powershell
& ./Tools/Test-WastesHeightLive.ps1 -LogPath Art/Validation/WastesLowerCaps-2026-09-07/After-2560x1369.log -Viewport 2560x1369 -Cases space-ascent,space-descent
pwsh -NoProfile -File Tools/Test-WastesHeightLiveMutations.ps1 -LogPath Art/Validation/WastesLowerCaps-2026-09-07/After-2560x1369.log
```

Private pre-test backup:
`C:/Users/max_h/AppData/Local/Temp/ApogeanLowerCaps-07c895df7ede4840a1372617847ff589`.
Previous installed package SHA256:
`E8E927C32809D7625CE1E3F6A4BFDAFDB963E7B5B4625087201DCA8C8C579D61`.
