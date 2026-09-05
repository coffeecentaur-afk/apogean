# Local forest restoration — implementation and validation

2026-09-04. Source policy, isolated build, and the bounded **live restoration-routing fixture pass**. Full production/visual matrix remains pending. No new concept art was installed.

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
- The earlier app-control approval timeout was resolved after the user's explicit continuation. The test character and disposable `Apogee Native Visual V3` world were backed up before entry. No normal world was entered.
- The first live 100%-green fixture **failed**: 169 living / 248 Wastes = 40.5%, with unforced Forest routing. Repeated failure ruled out stale metrics and another biome winning. Older QA platforms below the 190×62 photograph contaminated the scene sample. The fixture now clears a world-clipped 96-tile buffer on every side (382×254 maximum), then rebuilds the original visible strip. The production 65/35 thresholds were not changed to pass the test.
- `Tools/Test-ForestRestorationLive.ps1` went red on that exact failure, then passed after isolation and rebuilding. It consumes fresh telemetry and capture timestamps, asserts the planted/measured ratio within six percentage points, checks unforced Forest selection and expected hysteresis state, and preserves the engine PNG unchanged with a JSON sidecar. It does not claim to judge art or independently recognize the pixels.
- The full five-step sequence passed at a logged **2560×1369 client viewport**. Capture panoramas are **3040×992**, not 2560×1440 viewport evidence. All six forest captures plus the Jungle-priority capture were inspected; normal game views also showed green Forest and the later Jungle route. No new exception or DrawLiquid crash appeared during this run.

| Evidence | Living / Wastes | Fraction | Selected capture slot | Result |
| --- | --- | --- | --- | --- |
| 00-green-regression | 169 / 0 | 1.000 | 10, native forest | Former failing test passes |
| 01-wastes | 0 / 169 | 0.000 | 18, ruined forest | Wastes |
| 02-mixed-from-wastes | 80 / 89 | 0.473 | 18 | Retains Wastes |
| 03-green | 169 / 0 | 1.000 | 10 | Green forest |
| 04-mixed-from-green | 80 / 89 | 0.473 | 10 | Retains green |
| 05-wastes-return | 0 / 169 | 0.000 | 18 | Returns to Wastes |
| 06-jungle-priority | 0 / 0, cached green | 1.000 cached | 20, ruined jungle | Actual ZoneJungle beats cached green Forest |

Slots are observations of this installed mod set, not hard-coded public IDs. Evidence is in [Art/Validation/ForestRestoration/2026-09-04](Art/Validation/ForestRestoration/2026-09-04/README.md). A tileless sample retains the last valid fraction deliberately; the Jungle screenshot proves that cached forest state does not outrank another detected biome.

The authoring skills keep **routing proof separate from art approval**. The Wastes game view still exposes mirrored landmarks, low horizon placement, and stretched lower-row coverage; the restored native slot uses the installed native/resource-pack scenery rather than the new restored-forest concept. These captures do not promote those artworks or approve transitions frame-by-frame.

## Reproducible fixture and remaining matrix

Only use the named disposable single-player worlds Apogee Native Visual V3 or Apogee Campus Validation. The new destructive fixtures reject other worlds.

The file-request bridge exposes:

1. forest-restoration-wastes: 0% planted green.
2. forest-restoration-mixed: 50% planted green.
3. forest-restoration-green: 100% planted green.

Run in order Wastes → mixed → green → mixed → Wastes to prove both hysteresis directions. The fixture percentages are planted ratios; the periodic mixed pattern measures 47.3% in this engine sample. Each request clears the isolated footprint described above, rebuilds the 190×62 strip, removes forced surface-background selection, waits 300 ticks for metrics/fades, and schedules a named capture. This is a destructive QA fixture, never a gameplay conversion command.

Example: `pwsh -NoProfile -File Tools/Test-ForestRestorationLive.ps1 -Fixture forest-restoration-green -ExpectedState Green`. Run while the named disposable single-player world is active and unpaused. `-EvidenceName` optionally saves a new, non-overwritten evidence pair.

For every capture record FOREST RESTORATION telemetry, TILE LAB CAPTURE PROBE telemetry, detected biome, measured local ratio, selected slot, rendered screenshot and whether the visual result matches. A diagnostic world with adjacent Jungle/Snow/Maw counts can legitimately route elsewhere; relocate the disposable fixture, never weaken biome priority to make a test pass.

Then test Green Solution on actual Wastes (not only pre-planted fixtures), normal play and capture-camera agreement, both viewport targets, flight above the same patch, teleport away, reload, both boundary directions, night/rain/eclipse and third-party winning backgrounds. Confirm the known DrawLiquid capture crash has not returned.

## Next gate

The bounded routing sequence is proven. Next verify real Green Solution conversion, flight/cache boundaries, other scene priorities and the remaining camera/lighting matrix. Obtain separate composition review of the five concepts before authoring one matching production layer set; do not bulk-install the opaque concepts. Resume the approved A — Snapped component-sheet gate in the recorded plan order.

## 2026-09-05 update — native conversion and outgoing fade

Native `ProjectileID.PureSpray` now passes actual Wastes-to-grass conversion and both style-fade directions at1920×1080 and2560×1440. Before the fix, the minimized live trace contained19 wrong outgoing frames because opacity was cached in a selected-style-only hook; reading the engine alpha at draw time fixes it without changing thresholds. The recorder filters by actual style slot and requires incoming/outgoing samples with no missing draws. The CLI recomputes samples independently and its synthetic negative controls reject a falsely green report. This is not manual Clentaminator-input proof or multiplayer validation.

Jungle priority was rechecked after purification at1440p: real Jungle metrics selected slot20 with the diagnostic override off despite cached living-forest state. Detailed red/green traces, source-scale controls, repeat-phase screenshots and remaining art/performance gates are in [the September5 record](Art/Validation/WastesLandscapeV1/2026-09-05/README.md). Earlier routing-only limitations above describe their historical sessions; the native conversion gate is no longer untested. Remaining checks include flight/cache boundaries, reload, other scene priorities, the full lighting matrix and ordinary-world integration.
