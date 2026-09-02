# Apogean Tile Lab

## Purpose

The Tile Lab is the mandatory client-render gate for Apogean tiles and walls. It exists because image dimensions, alpha, and palette checks can all pass while a framing atlas still looks wrong in Terraria.

The fixture is intentionally small and destructive. Run it only in a disposable single-player world.

## Controls

- Press `F8` to rebuild the fixture around the local player.
- Run `/apogean tilelab` as an alternative.
- Run `/apogean exportatlases` to export the currently installed vanilla dirt tile and natural-wall atlases into `Captures/ApogeanTileLabReferences`.
- The disposable world named `Apogee Native Visual V3` rebuilds it automatically and runs a capture-camera probe after three seconds.

## What the fixture validates

1. Isolated tile frame.
2. Horizontal and vertical connections.
3. Four-way junction.
4. Dense connected field over a matching wall.
5. Half-block and all four slope directions.
6. Custom-tile to vanilla-dirt boundary.
7. Vanilla water inside the rendered and capture-camera area.

The dense field is the primary detector for the repeating graph-paper failure seen in the first world tiles.

## Automated checks

Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Test-TileLab.ps1
dotnet build .\apogean.csproj --nologo
```

The contract compares the control PNGs pixel-for-pixel with the known tModLoader framing references. Wastes candidates must have native dimensions, hard alpha, five or fewer opaque colors, no magenta-key leakage, and the exact opaque-pixel count of the live-exported vanilla topology. The promoted production files must match the renderer-tested candidates pixel-for-pixel.

It also rejects a custom `ModBiome` surface background paired with an unset water style, because that combination can send water-style index `-1` through Terraria's capture-camera liquid renderer.

## Client result — 2026-09-02

- tModLoader `v2026.7.3.0`, Terraria `1.4.4.9`.
- Block and wall loaded from the mod package.
- All seven fixture cases rendered.
- Connected shapes used the expected frames; no per-tile graph-paper repetition appeared.
- Capture camera completed and wrote `Apogean Tile Lab Capture Probe.png` to the tModLoader `Captures` directory.
- Capture probe values were `scene background=-1`, `scene water=-1`, `main water=0`, `capture water=0`.
- No fatal or index error appeared in the validation client log.

## Findings

The original terrain failure was in the atlas generator, not tile registration. It painted every 16x16 framing cell as an individually bordered miniature block. Terraria then selected those frames correctly, which amplified the bad art into a visible grid.

The reported `Main.DrawLiquid` crash was separate. The Maw biome selected a modded surface background while leaving its scene water style unset. The biome-level surface background binding is disabled until a real Maw `ModWaterStyle` and its three required liquid atlases are implemented; the existing global background selector still supplies the surface art.

## Wastes soil slice — 2026-09-02

- Exported `Images/Tiles_0` and `Images/Wall_2` from the live client. The first `TextureAssets.Tile` attempt returned `MagicPixel`; direct `Main.Assets` requests produced the real 288x270 tile and 468x180 wall atlases.
- Generated a five-color dry-umber candidate while preserving the live atlas alpha topology. No tile-cell borders are drawn by the generator.
- Rendered stock control and Wastes candidate suites side-by-side at normal game zoom.
- Isolated, connected, dense, sloped, half-block, dirt-merge, wall, water, and capture-camera cases rendered without a gap, graph-paper grid, or exception.
- Capture probe values remained `scene background=-1`, `scene water=-1`, `main water=0`, `capture water=0`.
- Promoted only `WastesSoil.png` and `WastesDirtWallUnsafe.png`. All other terrain families and world-generation geometry remain frozen.

The concept reference is `Art/Reference/WastesSoilMaterial-reference-v1.png`. It was generated in new-image mode with this direction: "16-bit side-view sandbox terrain material reference for dry Wastes soil; seven colors maximum; brown, ochre, and charcoal; sparse amber; dry roots; no green, purple, flesh, glow, or per-cell grid." It guided the value and hue family; it was not used as an engine atlas.

## Production gate

No terrain atlas, wall atlas, furniture sheet, or structure palette may enter world generation until it:

1. Passes its static asset contract.
2. Builds with no new compiler warnings.
3. Renders in a focused fixture at normal game zoom.
4. Passes slopes, merges, paint/coating, lighting, water, map, and capture-camera cases that apply to it.
5. Receives an archived client screenshot and a short written result.

The next safe increment is Wastes grass edging over this proven soil base. It must remain isolated in the Tile Lab until it passes the same gate. Broad world-generation work remains frozen.
