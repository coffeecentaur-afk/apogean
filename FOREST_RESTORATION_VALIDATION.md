# Local forest restoration — implementation and validation

2026-09-04. Source policy and isolated build pass; **live rendering pending**.

## User contract

Greenifying a local Wastes area must change its background. The preferred final form is gradual vegetation recovery based on local Wastes/living balance. The user explicitly accepted a native green-forest threshold fallback. This pass implements that fallback for **surface Forest routing only**. It does not implement continuous ecological alpha blending or underground restoration routing.

## Rule

- Eligible living tiles: ordinary vanilla Grass.
- Eligible ruined tiles: WastesGrass and legacy DeadGrass.
- Local fraction: living / (living + ruined), using the existing client-side scene sample.
- At least 40 eligible grass tiles are required for a fresh decision. Plain dirt, buildings, ore, jungle grass and evil/Hallow grass are not forest votes. Mowed GolfGrass is not yet included.
- Enter living forest at 65% or more; return to Wastes at 35% or less. Between these thresholds, retain the last state. These are provisional tuning values, not vanilla thresholds.
- With too little new evidence, hold the last sufficient local sample within 120 horizontal tiles so flying above an area does not instantly undo its scenery. Empty samples do not move that evidence anchor. Teleports beyond this range cannot reuse it.
- No campaign state, tiles, world evil percentages, network packets or saved format are changed. Presentation state resets on world load/unload. A mixed 35–65% area can initially choose Wastes again after reloading; it is session hysteresis, not persistent ecology.
- Maw and other detected biomes retain their selection priority. A third-party background already selected by tModLoader remains untouched. Forced QA backgrounds remain deliberate overrides.
- Native forest receives the incoming seed/region-selected vanilla style, not a fixed invented forest variant. Capture routing uses the same policy and caches the native style when scene effects leave it unspecified. Existing water-style range protection remains intact.
- Existing whole-style engine fades handle switching. A percentage-driven terrain-art compositor has not been added.

tModLoader provides nearby counts through TileCountsAvailable(ReadOnlySpan<int>) on clients; no world scan is introduced in drawing. [Primary API](https://docs.tmodloader.net/docs/stable/class_mod_system.html). Background routing/fade contracts remain separate. [GlobalBackgroundStyle](https://docs.tmodloader.net/docs/stable/class_global_background_style.html), [ModSurfaceBackgroundStyle](https://docs.tmodloader.net/docs/stable/class_mod_surface_background_style.html).

## Evidence actually obtained

- Tools/Test-ForestRestoration.ps1: 25 behavioral assertions passed initially, including thresholds, both hysteresis directions, minimum evidence, flight hold, stale evidence/teleport, legacy tiles, negative inputs, overflow and reset.
- Isolated mod build/package: zero errors, zero warnings.
- Tools/Test-WorldVisualIntegrity.ps1: passed, including the existing nine-biome/27-layer HD static contracts. Tools/Test-AuthoringStatus.ps1: ten families passed evidence-state consistency. Neither result approves visual quality or the new opaque concepts.
- The installed client loaded Apogean successfully after the build. Its pre-existing missing icon_small.png warning remains. This is loading evidence, not in-world routing evidence.
- No new in-game screenshot was obtained. Computer Use found the tModLoader window but returned “Computer Use app approval timed out.” No UI input/world entry followed. Do not retry around that approval through another control method.

## Pending live matrix

Only use the named disposable single-player worlds Apogee Native Visual V3 or Apogee Campus Validation. The new destructive fixtures reject other worlds.

The file-request bridge exposes:

1. forest-restoration-wastes: 0% planted green.
2. forest-restoration-mixed: 50% planted green.
3. forest-restoration-green: 100% planted green.

Run in order Wastes → mixed → green → mixed → Wastes to prove both hysteresis directions. The fixture percentages are planted ratios; surrounding terrain may change measured scene counts, so inspect telemetry rather than assuming exact agreement. Each request clears/rebuilds only the existing 190×62 diagnostic footprint, removes forced surface-background selection, waits 300 ticks for metrics/fades, and schedules a named capture.

For every capture record FOREST RESTORATION telemetry, TILE LAB CAPTURE PROBE telemetry, detected biome, measured local ratio, selected slot, rendered screenshot and whether the visual result matches. A diagnostic world with adjacent Jungle/Snow/Maw counts can legitimately route elsewhere; relocate the disposable fixture, never weaken biome priority to make a test pass.

Then test Green Solution on actual Wastes (not only pre-planted fixtures), normal play and capture-camera agreement, both viewport targets, flight above the same patch, teleport away, reload, both boundary directions, night/rain/eclipse and third-party winning backgrounds. Confirm the known DrawLiquid capture crash has not returned.

## Next gate

Get app-control approval and run the local routing matrix. Review the five new concept images separately. Then prepare one approved matching layer set; do not bulk-install all concepts or resume unbounded renderer revisions without this evidence.
