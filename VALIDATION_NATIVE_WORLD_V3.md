# Native world and tileset validation — V3

Validated 2026-09-01 through 2026-09-03 against tModLoader `v2026.7.3.0` and Terraria `1.4.4.9`.

## Package gates

- `Tools/Test-WorldVisualIntegrity.ps1`: PASS
- `Tools/Test-VisualContracts.ps1`: PASS
- `Tools/Test-SurfaceRegression.ps1`: PASS
- `dotnet build --no-restore`: PASS, zero errors; the existing lowercase `apogean` class-name compiler warning remains.
- Dedicated-server content load: PASS with no Apogean warnings or errors after removing invalid placeholder banner registrations.

## Fresh large-world proof

- Name: `Apogee Native Visual V3`
- Input seed: `APOGEE-NATIVE-TILES-V3`
- Numeric seed: `997890696`
- Size: `8400 × 2400`
- Difficulty: Classic
- Evil: Crimson
- Vanilla + Apogean generation time: `30.607 s`
- Save, validation, reload, and clean server shutdown: PASS

Apogean generation log:

```text
World-plan validation passed: schema=2; hash=603D98E9; ruptures=3; landmarks=8
Maw-route validation passed: route=pass depth=2150/2097; reachable=41039;
max-fall=83/120; stomach-clearance=41/30-60; intestine=2385/2370;
plug=85/70; legacy-acid=0; maw-water=0; all-spine-waypoints-reached
```

## Visual acceptance fixture

Run `/apogean gallery` in a disposable single-player validation world. It clears a reported rectangle to the player's right and places labeled rows covering:

- all Wastes tile/wall families;
- all Maw tile/wall families and Ossuary bone;
- Kessler, Helix, and Sentrix block, trim, floor, glass, beam, and wall combinations;
- connected edges, corners, interior framing, half-blocks, and slopes.

The atlases use the official tModLoader ExampleMod framing topology as an opaque/transparent mask. Every visible pixel, palette, material motif, ruin, amber vein, and corporate detail remains Apogean-authored. No Terraria resource pack is required.

The automatic `Apogee Native Visual V3` fixture additionally grows four native ruined trees, performs a deterministic mid-trunk chop on the fourth tree, and fails immediately unless the canopy is removed while the stump remains. Its capture-camera probe resolves Apogean's post-arbitration global background slot explicitly, so the panorama and ordinary camera must show the same ruined Forest composition.

The surface-background fixture is biome-selectable and currently validates Forest V0, Desert V0, and Jungle V0. It clears a low, wall-free Wastes stage, fixes the world at midday for repeatable palette review, and renders the same production layer files used in ordinary play. Desert V0 passed a 2560×1400 gameplay-viewport check with readable far/middle/close separation, no rectangular transparency holes, and no hard vertical repeat seam. Jungle V0 passed the same check with the distant canopy/lab skyline, overgrown dome-and-transit complex, and close vegetation banks remaining distinct at runtime parallax scales. Approved evidence is retained at `Art/Validation/2026-09-03-DesertV0SurfaceBackgroundRenderLab.jpg` and `Art/Validation/2026-09-03-JungleV0SurfaceBackgroundRenderLab.jpg`.

The vegetation chop assertion accepts Terraria's legitimate short native-tree variants when at least four contiguous trunk cells are present. That is enough to leave supported trunk below and canopy above the tested `rootY - 2` cut, avoiding a random false failure while preserving the actual split-tree contract.

## Maw source-conversion proof

`Tools/Request-LiveValidation.ps1 -Fixture conversion` creates an allow-listed request in the Terraria Captures directory. A running single-player client consumes it, builds the destructive gallery through the production conversion hooks, executes runtime assertions, and schedules the standard capture-camera probe. It does not accept code, paths, or arbitrary commands.

The 2026-09-03 live proof covers thirteen material columns and four stages: natural source → Maw → one Purity pass/Wastes → two Purity passes/vanilla. Sources include Wastes Soil, Grass, Stone, Sand, Ice, Snow, and Mud plus Corrupt Stone, Crimson Sand, Hallow Ice, Jungle Grass, Mushroom Grass, and Underworld Ash. A separate guard row proves that Gray Brick/Wood Wall/red wire and Kessler block/bulkhead content survive conversion unchanged. Runtime tile, wall, drop, sand-fall, Sandgun-ammunition, ice, snow, and two-step purification assertions all passed without client errors.

Archived renderer proof: `Art/Validation/2026-09-03-MawBiomeSourceConversionMatrix.png`.
