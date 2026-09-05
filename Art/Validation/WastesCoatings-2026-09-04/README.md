# Wastes rigid ground-cover coatings — 2026-09-04

tModLoader 2026.7.3.0 / Terraria 1.4.4.9, single-player disposable
`Apogee Native Visual V3`, character `gg`, 2560×1369 client viewport.
`Live-*.png` are original Windows game-window screenshots. The separate
`Native-production-grass-recheck.png` is Terraria's capture-camera output,
not an illustration or a substitute for ordinary viewport evidence.

## Reproduced defect and narrow fix

The comparison has five groups, left to right: unmodified, Deep Blue paint,
actuated, Echo-coated, illuminant-coated. Each uses the same three production
props (DeadTuft, WastesBristle, WastesRootShrub), plus a floating grass/soil
control panel. Floating panels and tiny corner samples are intentional test
geometry, not world-generation examples.

- **Before:** at 22:39:50, blue control blocks changed but brush stayed brown;
  Echo control blocks vanished but brush remained visible. See
  `Live-before-fix.png`. This disproved a paint-data problem and isolated the
  custom whole-texture draw path, which bypassed paint and coatings.
- The native atlas, excluding its 2px gutters, exactly reproduces every visible
  pixel of the existing whole sprites: 2,048 tuft pixels and 4,608 pixels each
  for bristle/shrub. `Tools/Test-RigidPlantAtlas.ps1` checks all styles and rejects
  alpha/color damage. No approved PNG or generator changed.
- The renderer now draws those native painted atlas cells in **one rigid
  coordinate system**: no separate wind rotation, offset, or scale per half.
  Native visibility controls Echo; illuminant uses full brightness; actuated
  cells retain native 40% RGB dimming with unchanged alpha. Terraria's helper
  for the last operation is internal, so the verified behavior is explicit.
- **After:** clean build (0 warnings/errors), cold restart, 22:47:57 comparison
  at bounds X4111,Y608,170×45. Painted props are blue; Echo props are absent;
  control shapes remain connected and actuated props dim. See
  `Live-after-fix.png`. The native paint cache had time to resolve before capture.
- **Night:** 22:48:19 full-moon midnight sample shows all three illuminant props
  bright while adjacent ground remains dark (`Live-after-fix-midnight.png`).
  Coating does not incorrectly become a light source.
- **Grass:** 22:49:04 rebuilt the production grass gallery; four tiny slope/soil
  junctions, stepped/half-block forms, and larger blocks compared against vanilla
  controls. No white openings observed in the sampled joins. See the Windows
  grass recheck and separate native panorama. No grass asset or renderer changed.

The checked-in surface regression now requires the native painted-texture path,
Echo/fullbright/actuation handling and shared rigid cell coordinates, instead of
requiring the old `_Whole` texture draw. The no-per-cell-sway check remains.
All 14 terrain atlas contracts, rigid pixel comparisons, reported-regression
checks and the corrected surface regression pass individually. The initial full
Terrain run correctly went red on the obsolete whole-texture requirement.
The full Terrain gate then passed on rerun; Tree and Status gates also passed.
The QA world saved normally and the client was closed after the checks.

## Reproduce and remaining limits

In the named disposable world, request `vegetation`, then
`vegetation-view-coatings`, then `vegetation-view-night-fullmoon` through
`Tools/Request-LiveValidation.ps1`. Wait for each log line before inspecting.
`grass` restores temporary weather and builds the production corner gallery.
`qa-save-and-quit` uses Terraria's normal save path. The bridge now waits for
automatic fixture initialization before consuming queued requests, preventing
a startup request from being silently overwritten by the delayed initial grove.

This is bounded single-player evidence, **not polished/general-world acceptance**.
Partial-object coating combinations, Spectre reveal, actual paint-brush/wiring
input, multiplayer replication and generated-world composition remain open.
Final corner/ground-cover visual review is the next user gate before moving the
recovery order to the Wastes background family. Backgrounds visible in these
screenshots remain a separate QA-only candidate, not approved by this terrain test.

## Reference contracts

- [Tile properties](https://docs.tmodloader.net/docs/stable/struct_tile.html):
  painting, actuation, brightness and invisibility are distinct states.
- [tModLoader paint-system patch](https://github.com/tModLoader/tModLoader/blob/stable/patches/tModLoader/Terraria/GameContent/TilePaintSystemV2.cs.patch):
  use the native paint pipeline for registered tile textures.
- Installed engine `TileDrawing.GetTileDrawTexture` / `IsVisible` and internal
  `Tile.actColor` were inspected read-only to verify this version's behavior.
  Local decompiled reference code is not redistributed in this repository.
