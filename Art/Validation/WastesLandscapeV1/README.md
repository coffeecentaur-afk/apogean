# Wastes landscape V1 — live validation, 2026-09-04

Status: **installed in the disposable QA world/render lab, not promoted to ordinary worlds**. Composition and local deterministic processing are user-authorized. This is one surface family, not the complete landscape plan or final visual acceptance.

## Inputs and export

The approved composition is `Art/Reference/Backgrounds/2026-09-04/Wastes-Surface-Concept.png`. Four original built-in generations and their exact prompts are retained in `Art/Source/Backgrounds/WastesV1/`. Actual source size is 2172×724, not the requested 3072-wide output. No third-party pixels or paid API fallback were used.

`Tools/Export-WastesLandscapeV1.ps1` owns only the three candidate textures. Magenta keying produces hard alpha. Connected minimum-error overlap cuts join the horizontal repeat and separately authored lower ground. A first dithered join was rejected for visible grid noise. Exported source pixels are not enlarged; the lower extension is real soil/rock artwork, not a stretched row.

| Layer | Output | Transparent / opaque pixels | SHA-256 |
| --- | --- | --- | --- |
| Far | 2048×1280 | 680585 / 1940855 | 3AE39A9C65758CF2FF08051B1AE7909583FD815EBA6DE3E34BFC536CEF9350DB |
| Mid | 2048×1280 | 798864 / 1822576 | B16CB9A1DB98338CFBEF4F525D13AE2853B98025C5448B4608C2A95685048812 |
| Close | 2048×1280 | 932141 / 1689299 | D59768591C2FADA8C1903DAE7EF1FA23A472460CCF6BABE408FC95CFE661F3BA |

All three have zero soft-alpha pixels, zero opaque pixels in the tested sky band, zero transparent pixels in the ground band, and identical edge columns. Equality alone does not prove an invisible art join.

## Renderer contract

- Physical viewport, not the temporarily logical `Main.screenWidth/Height` used during surface drawing.
- Native source scale after cancelling `BackgroundViewMatrix.ZoomMatrix` and reciprocal zoom in draw inputs; no global zoom or SpriteBatch state changes. Gravity effects remain owned by Terraria.
- Horizontal parallax: 0.055 / 0.14 / 0.30. Vertical: 0.10 / 0.18 / 0.30.
- Top: `height * (0.57 + layer * 0.025) - 740 + groundDelta * vertical`. Only Close clamps to `height - textureHeight`, since its opaque ground owns bottom coverage. Clamping Far/Mid buried the skyline behind real terrain.
- Positive-modulo repeat without mirrored landmarks. Same geometry at noon, sunset, midnight, rain and eclipse. Continuous sky-derived RGB floors 65/75/98; eclipse 105/88/80; style opacity owns alpha.
- Existing production style fade and local forest-restoration selection are reused in the named QA world. No replacement of third-party biome selection, other biome art, cavern slots, or tree atlases.

Raw RGBA is **30 MiB** (10 per texture), within the 32 MiB family budget. The pre-existing nine-family V0 library is 162.01 MiB; coexistence can reach **192.01 MiB** before engine render targets/other assets. This is an asset-footprint calculation, not a measured GPU residency or hitch profile. Asset requests are lazy for V1 and local references clear on mod unload; tModLoader owns texture lifetime.

## Bugs actually found by testing

1. An old aerial fixture fell back to the floor before capture. The new bounded `WastesLandscapeCameraLab` holds the camera/player and restores time/weather/position on release. It only permits the explicitly disposable single-player world, changes no terrain, and expires after 1800 ticks.
2. The first night tint brightened at the day/night switch. It was replaced by a continuous sky-derived floor, not a separate fixed bright night image.
3. A scale-1 draw was **not native resolution**: the 111-pixel truck pigment span appeared 150 pixels wide at 1440p. Targeted instrumentation observed a 2560×1440 graphics viewport, temporarily logical 1920×1080 screen, 2048×1280 texture, and background zoom 1.3333334. The [tModLoader Main patch](https://raw.githubusercontent.com/tModLoader/tModLoader/stable/patches/tModLoader/Terraria/Main.cs.patch) also shows the forced minimum background zoom. Cancelling only that zoom reduced the measured span to 110 pixels. The source/capture regression predates the fix. Temporary debug logging was removed.
4. Native-scale side checks exposed a horizon too low to read behind actual terrain. The -480 anchor was raised to -740 and only the closest layer retains the bottom clamp. Empty-gallery visibility is not adequate ground-play evidence.

## Evidence naming and reproduction

`Live/` files are original Windows game-window captures, not offline recreations. Windowed captures include chrome. `Offline/` images are explicitly labelled composition previews and do not count as live evidence.

- Unprefixed captures and `Initial-*`: earlier tint/scale iteration.
- `Native-*`: zoom compensation, before the horizon adjustment.
- `Horizon-*`: current horizon adjustment.

The retained red baseline is `Live/ground-2560x1440.png`. Run `Tools/Test-WastesLandmarkScale.ps1` with this path and ROI 1880,468,220,55: source111/live150, fail. The first green is `Live/Native-ground-2560x1440.png`, ROI1855,775,128,34: source111/live110, pass. The script now defaults to the final `Live/Horizon-ground-2560x1440.png`, ROI1855,515,128,34: **source111/live111, scale1.000**. Pixel-span tolerates three pixels for edge pigment; it does not certify overall artistic quality.

`Tools/Request-WastesCameraCheck.ps1 -Case ground|jump|wings|sky|left|right|sunset|night|rain|eclipse|release` drives the live fixture. Case `sky` checks the handoff to Terraria's space sky. Left/right move 5120 world pixels, **not** 2.5 complete repeats of every parallax layer; the full repeat-travel gate remains open.

## Outstanding promotion gates

The ten current `Horizon` samples exist at **1920×1080 and 2560×1440**: ground, jump, wings, space, left, right, sunset, night, rain and eclipse. No exposed texture bottoms were observed in these samples; space retains the native sky. This is a fixed-position sweep, not a continuous traversal, every camera/lighting combination, or proof of seamless repeated art.

Three additional current captures at each viewport use **unforced production scene selection**. The normal foreground and structures visible outside the gallery are existing content, not approved by this background test.

| Fixture | Nearby living / Wastes grass | Selected background | Evidence |
| --- | --- | --- | --- |
| forest-restoration-wastes | 0 / 169 | Forest slot18, Wastes V1 in QA | Render lab off; native world selection |
| forest-restoration-green | 169 / 0 | Native forest slot10 | Render lab off; living selected true |
| jungle-routing | 0 / 0 | Jungle slot20 | Render lab off; ZoneJungle true, even after living selection |

1440p probes were recorded at 20:19:31/38/46; 1080p probes at 20:27:58, 20:28:05/12 on 2026-09-04. All six capture-camera probes sanitized water style to0 and completed without a logged exception. Fixtures change only the backed-up disposable QA world. They test planted grass and real biome counts, **not firing Green Solution**. Their settled captures do not certify a smooth adjacent-biome fade.

Still required: continuous full repeat-phase travel, actual Green Solution conversion, adjacent fades, the remaining scene-priority cases, gravity/zoom/window variants, and hitch/residency measurement. Art review found flat rock bands exposed during flight and a too-literal soil cross-section; fine noise, repeated landmarks, depth separation and lower joins need further work. Original alternate compositions and matched restoration vegetation masks remain future work; native forest threshold fallback remains approved. No claim that all biome backgrounds, trees, structures, or the Maw are finished.

## Safe handoff

Rerun checks: all three `Test-SurfaceLayerExport` inspections pass; `Test-WastesLandmarkScale` passes; `Test-ForestRestoration` passes25 policy cases; `Test-GeneratorOwnership` passes; `Test-AuthoringStatus` passes11 evidence-consistent families; the existing `Test-BackgroundHdContracts` passes its27-asset static checks, not native screen-scale certification. Latest isolated package build has zero warnings and zero errors. None of these checks overrides the pending production/art gates above.

The authorized restart waited for world generation to finish. The newly generated user world was not opened or changed. Backup `ApogeanLandscapeQA-13d50795371840e8bf2fa56e8dc37f32` in the user's local Temp directory contains the original QA world, gg character and pre-test config. The final QA client was closed; display settings were restored to windowed2560×1369, non-maximized/non-borderless. Camera override was released before closing. This candidate remains QA-only; resuming does not require generating a new world.
