# Apogean Tile Lab

## Purpose

The Tile Lab is the mandatory client-render gate for Apogean tiles and walls. It exists because image dimensions, alpha, and palette checks can all pass while a framing atlas still looks wrong in Terraria.

The fixture is intentionally small and destructive. Run it only in a disposable single-player world.

## Controls

- Press `F8` to rebuild the fixture around the local player.
- Run `/apogean tilelab` as an alternative.
- Run `/apogean grasslab` to rebuild the grass-specific side-by-side fixture.
- Run `/apogean vegetationlab` to rebuild the Wastes ground-cover fixture.
- Run `/apogean exportatlases` to export the currently installed vanilla dirt, grass, and natural-wall atlases into `Captures/ApogeanTileLabReferences`.
- The disposable world named `Apogee Native Visual V3` currently rebuilds the vegetation fixture automatically and runs a capture-camera probe after three seconds.

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

The grass increment below was kept isolated in the Tile Lab until it passed the same gate. Broad world-generation work remains frozen.

## Wastes grass slice — 2026-09-02

- Exported the live `Images/Tiles_2` grass atlas and `Images/Wall_63` natural grass-wall atlas from tModLoader.
- Identified the old grass asset's core defect: it used a generic 288x270 terrain sheet, while Terraria grass uses a specialized 288x1980 atlas with many additional exposed-edge and merge frames.
- Recolored the complete live grass topology rather than drawing bordered 16x16 cells. Engine mask pixels remain intact and all alpha is hard-edged.
- Matched the approved Wastes soil colors beneath a separate dry root/straw ramp; no green, purple, flesh, glow, or white-mask leakage remains in the Wastes candidate.
- Rendered vanilla and Wastes suites side-by-side at normal game zoom. Dense fields, enclosed openings, flat caps over soil, side fringe, stair steps, half-blocks, and all four slopes selected coherent frames with no graph-paper repetition.
- The capture camera completed and wrote `Apogean Grass Lab Capture Probe.png`; the client remained in-world and logged no exception or fatal error.
- Promoted `WastesGrass.png` and `WastesGrassWallUnsafe.png` only after the live pass. The static contract now requires both production files to remain pixel-identical to those tested candidates.

This slice proves atlas rendering and basic soil merging. Grass growth, spreading, conversion rules, paint/coating behavior, and world-generation replacement remain deliberately deferred to dedicated tests rather than being inferred from a good screenshot.

## Wastes ground-cover slice — 2026-09-03

- Rebuilt the first undersized pass as four 2x1 root-pile variants, three 2x3 tangled-root variants, and three 3x2 broad-root-mass variants. Their larger footprints preserve the concept art's arches, knots, and overlapping roots at normal game zoom.
- Kept the engine sheets at exact native object dimensions: 144x18, 108x54, and 162x36. All pixels use hard alpha and no sheet exceeds seven opaque colors.
- Registered each family as one tile style with explicit random placement variants. The first client pass exposed an incorrect `PlaceObject` style argument that pushed later variants beyond the sheet; the lab now selects those variants through the `random` argument, matching tModLoader's `StyleMultiplier` contract.
- Packed each continuous logical drawing around tModLoader's hidden two-pixel cell gutters. Rendered all ten variants simultaneously on production Wastes grass; every variant anchored to the correct row and all multi-tile roots assembled without seams or clipping.
- The capture camera wrote `Apogean Vegetation Lab Capture Probe.png` with entities excluded so dropped items, NPCs, and the player cannot obscure the tile evidence. The client log contained no vegetation-lab failure, exception, error, or fatal entry.
- Integrated only this validated family into `RuinedSurfaceSystem`: approximately 70% of decoration attempts select low tufts, 22% select bristles, and 8% select shrubs. The existing overall sparse placement rate remains unchanged.

The approved source is `Art/Reference/WastesGroundCover-reference-v2.png`. The deterministic exporter crops those exact silhouettes, downsamples them by coverage, maps them to a six-color charcoal/umber/ochre/amber ramp, grows a one-pixel exterior outline, and packs the result into the native tile atlases. This replaces the rejected line-and-disc approximation.

The next safe increment is the Wastes tree trunk and canopy family. Broader terrain, structures, and Maw geometry remain frozen.
