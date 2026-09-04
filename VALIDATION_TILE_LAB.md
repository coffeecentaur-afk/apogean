# Apogean Tile Lab

## Purpose

The Tile Lab is the mandatory client-render gate for Apogean tiles and walls. It exists because image dimensions, alpha, and palette checks can all pass while a framing atlas still looks wrong in Terraria.

The fixture is intentionally small and destructive. Run it only in a disposable single-player world.

## Controls

- Press `F8` to rebuild the fixture around the local player.
- Run `/apogean tilelab` as an alternative.
- Run `/apogean grasslab` to rebuild the grass-specific side-by-side fixture.
- Run `/apogean vegetationlab` to rebuild the Wastes ground-cover fixture.
- Run `/apogean terrainproperties` to build the seven-material production behavior fixture.
- Run `/apogean terrainitems` to receive all six Wastes terrain items plus a Sandgun for manual placement, mining, falling, and ammo checks.
- Run `/apogean exportatlases` to export the installed vanilla terrain, wall, tree, block-item, and falling-sand projectile atlases into `Captures/ApogeanTileLabReferences`.
- The disposable world named `Apogee Native Visual V3` currently rebuilds the production terrain-property fixture automatically and runs a capture-camera probe after three seconds.

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
- A follow-up ground-contact pass set a four-pixel `DrawYOffset` on all three ground-cover families. A fresh live render confirmed that tufts, bristles, and root shrubs visibly enter the grass lip instead of floating above it.
- Integrated only this validated family into `RuinedSurfaceSystem`: approximately 70% of decoration attempts select low tufts, 22% select bristles, and 8% select shrubs. The existing overall sparse placement rate remains unchanged.

The approved source is `Art/Reference/WastesGroundCover-reference-v2.png`. The deterministic exporter crops those exact silhouettes, downsamples them by coverage, maps them to a six-color charcoal/umber/ochre/amber ramp, grows a one-pixel exterior outline, and packs the result into the native tile atlases. This replaces the rejected line-and-disc approximation.

The next safe increment is the Wastes tree trunk and canopy family. Broader terrain, structures, and Maw geometry remain frozen.

## Wastes tree renderer slice — 2026-09-03

- Preserved Terraria's native tree tile, growth, chopping, collision, and world-state behavior while hiding its narrow native atlases behind fully transparent sheets.
- Rendered one wide, reference-derived dead-tree composite from the detected root tile through `GlobalTile.PostDraw`, which keeps ordinary gameplay and Capture Camera on the same draw path.
- Corrected the visible root anchor using the source texture's four-pixel transparent bottom margin plus a nine-pixel soil sink. The new render confirms that the flared roots enter the grass edge rather than hovering above it.
- Rebuilt and passed the Tile Lab, visual-contract, and world-visual-integrity checks after the grounding correction. The tree silhouette remains subject to the player's visual approval before this family expands into multiple variants.

## Wastes terrain-family and identity slice — 2026-09-03

- Exported the installed vanilla Stone, Sand, Ice, Snow, and Mud tile/wall atlases and preserved each material's exact alpha topology while replacing its palette. Production Wastes terrain therefore selects Terraria's native edge, corner, slope, half-block, merge, and dense-field frames rather than drawing rectangular cells.
- Added a seven-column production fixture for Soil, Grass, Stone, Sand, Ice, Snow, and Mud. It renders irregular silhouettes, paint, fullbright coating, unsafe wall patches, standing-water pockets, slopes, half blocks, and cross-material seams.
- Added runtime assertions for material identity: stone classification, grass conversion, falling and suffocating sand, shovel behavior, custom falling projectile, Sandgun ammunition, slippery ice, snow classification, custom tile drops, and neutral non-infectable state.
- Recolored the renderer-exported Terraria 16x16 block-item sprites and 14x14 sand projectile rather than inventing miniature concept art. Mining any Wastes terrain now returns its Wastes item; grass returns Wastes Soil; falling sand and Sandgun sand preserve Wastes identity.
- The mod built with no errors and only the existing lowercase-mod-class warning. Tile Lab, visual-contract, and world-visual-integrity scripts passed. The live client completed `Apogean Wastes Terrain Properties Capture Probe.png`, logged the reference export, and produced no exception or fatal entry.

## Maw conversion and identity slice — 2026-09-03

- Derived hostile Maw Soil, Grass, Stone, Sand, Ice, Snow, Mud, Clay, and seven unsafe-wall atlases from the validated neutral topology instead of inventing a second framing layout. Maw Grass now uses Terraria's complete 288x1980 grass sheet rather than the rejected generic 288x270 terrain sheet.
- Added custom Maw terrain items, drops, falling sand, and Sandgun ammunition so conversion does not lose material identity when blocks are mined, placed, destabilized, or fired.
- Added a four-row, seven-column renderer fixture: neutral Wastes source; Maw conversion through the production allowlist; one Purity pass back to Wastes; and a second Purity pass back to vanilla. Every stage converts both tile and unsafe wall.
- Runtime assertions cover native Maw stone, grass, sand, ice, and snow behavior; custom drops; falling sand; Sandgun ammunition; and the two-step purification chain.
- The mod built cleanly and all Tile Lab, visual-contract, and world-visual-integrity checks passed. A cold live-client pass wrote `Apogean Maw Conversion Matrix Capture Probe.png`; the capture-camera liquid indices stayed valid and the client log contained no exception or fatal entry.

## Wastes forest background composition V0 — 2026-09-03

- Researched the exact installed tModLoader background renderer rather than relying on generic examples. Surface art now follows its 1024x408 far, 1024x600 middle, and 952x480 close contracts; ordinary underground art remains on its separate four-texture contract, and the Underworld is excluded because it has a separate five-layer renderer.
- Decomposed the approved `Art/Source/Backgrounds/Forest/V0-Day-source.png` into transparent parallax artwork that preserves its broadcast spire and skyline, broken highways and settlement, and rooted foreground basin. The cleanup exporter removes generated checkerboard pixels, downsamples with nearest-neighbor sampling, maps each layer to at most ten opaque colors, seals its lower edge, and enforces a matching horizontal seam.
- Static checks require exact dimensions, hard 0/255 alpha, bounded palettes, a transparent upper field, an opaque lower edge, and a wrap-safe first/last column. Global routing now preserves third-party surface and underground background slots instead of overriding another mod's selected style.
- Corrected the surface fade hook for the installed 2026.07 renderer: tModLoader already advances the active style's front alpha before calling `ModifyFarFades`, so the mod no longer advances the same array a second time. The ordinary underground selector also no longer attempts to replace the Underworld panorama.
- Enabled the composition only in a temporary renderer build, loaded `Apogee Native Visual V3`, and verified the actual game window before promotion. The live render showed the large broadcast spire, distant city, broken infrastructure, and settlement with no checkerboard, hard rectangular sky fill, or inter-layer gap. Evidence is saved at `Art/Validation/2026-09-03-ForestConceptRenderLab.jpg`.
- Restored the diagnostic toggle to off by default and promoted the validated assets to production Forest composition V0. Build, Tile Lab, visual-contract, and world-visual-integrity checks all passed; the client log contained no exception, fatal, or liquid-index crash.

## Wastes forest underground composition V0 — 2026-09-03

- Implemented the documented four-texture cave contract: 160x16 surface/ground transition, 160x96 shallow field, 160x16 ground/rock transition, and 160x96 deep field. Every image is fully opaque, uses no more than eight colors, matches its first and last core columns, and duplicates x0–31 at x128–159 for Terraria's 128-pixel repeat stride.
- Rejected the first live pass even though its static checks succeeded. It embedded a complete collapsed mine shelter in the 160x96 field; the renderer stamped that scene every 128 pixels and normal cave lighting reduced it to repeated dark rings. The rejected proof remains at `Art/Validation/2026-09-03-ForestUndergroundRenderLab-REJECTED.jpg`.
- Rebuilt the set as seamless dry strata with eroded bands, mineral flecks, cracks, and fragments of collapsed supports. Large narrative landmarks such as shelters, minecarts, rails, cables, and research ruins will be sparse world-generated walls or furniture rather than repeating cave texture content.
- Added `/apogean undergroundlab [on|off]`, which creates a disposable wall-free cavern and enables the exact diagnostic texture set. Its diagnostic light grid exists only while the lab is enabled, making the full repeat pattern visible without changing production cave lighting.
- A second cold client pass showed a continuous Terraria-scale back wall with no transparent holes, miniature-room repetition, horizontal seam, or crash. The client log contained no exception, fatal, or invalid-index entry. Evidence is saved at `Art/Validation/2026-09-03-ForestUndergroundRenderLab.jpg`.
- Restored the lab to off by default and promoted the renderer-approved files to `Content/Backgrounds/Forest/Underground/V0_0.png` through `V0_3.png`. Static tests now require the production copies to remain byte-identical to those candidates.

The surface and ordinary-cave matrix continues one bounded biome composition at a time. The completed multi-biome surface pass and Forest cave proof establish the repeatable renderer contract required before faction galleries begin.

## Ruined Deep Underworld panorama V0 — 2026-09-03

- Verified against the installed renderer that Hell bypasses `ModUndergroundBackgroundStyle` and draws its own five-depth panorama. The replacement therefore uses a client-only `CustomSky` in `SkyManager.DrawRemainingDepth`'s final `float.MinValue` depth band, after those five layers and before gameplay tiles and entities.
- Added a dedicated `underworld-background` request fixture at Underworld depth. It enables only the diagnostic compositor, builds a disposable traversal deck with lights, and exercises the same live client and capture path without changing ordinary worlds.
- Authored a 1024x576 opaque far panorama, a 1024x576 hard-alpha middle overlay, and a 1024x576 hard-alpha close overlay. They preserve the approved Ruined Deep language: distant refinery ruins, fractured cavern vaults, broken pipe spans, slag terraces, rail fragments, winches, hanging cable, and restrained amber lamps.
- Rejected the first live art pass because pale pixels from the image-generation checker preview survived the alpha conversion and quantized into dense amber streaks. Expanded neutral-light preview removal, regenerated both transparent overlays, and confirmed that the false flecks disappeared without erasing cables or structural highlights.
- The accepted live render completely replaces the vanilla Hell panorama across a 2560x1400 viewport while remaining behind the fixture deck, player, enemies, lava, minimap, and UI. Evidence is archived at `Art/Validation/2026-09-03-RuinedDeepV0UnderworldRenderLab.png`.
- Capture Camera completes without a crash and includes the same custom-sky draw path. Oversized Underworld captures retain Terraria's native `DrawUnderworldBackground(flat: true)` viewport framing and its surrounding black field; the mod does not distort the production panorama to disguise that vanilla camera limitation.
- `Test-TileLab.ps1`, `Test-VisualContracts.ps1`, `Test-SurfaceRegression.ps1`, and the packaged tModLoader build pass. The only compiler diagnostic remains the pre-existing lowercase mod-class warning.

## Kessler native construction gallery — 2026-09-03

- Exported the installed Gray Brick tile and wall atlases and preserved their real adjacency topology while mapping them to Kessler's gunmetal, burnt-red, signal-orange, and amber palette. The focused generator owns only the Kessler construction family.
- Added a compact three-bay fixture containing structure block, trim, floor, glazing, beams, bulkhead/window walls, platforms, chairs, table, workbench, lockers, consoles, wall lights, three animated power-armour racks, and the animated shield-and-chevron war standard.
- Rejected the first render because the 104×42 gallery remained a giant dark shell. The replacement is 92×27, lowers its lights to the occupied plane, and uses near-Terraria-scale room proportions.
- Proper WorldGen.PlaceObject placement exposed the real furniture failure: all corporate structure tiles were registered tileNoAttach, so every native anchor was rejected. The shared material contract now accepts anchors, and the fixture resolves each registered origin/alternate through TileObjectData and throws if any placement fails.
- The accepted client render shows the complete furniture row, differentiated bulkhead/window fields, observation slit, walkable platform, animated fixtures, and roof standard. The old bright graph-paper/bamboo grid is absent. Evidence is archived at Art/Validation/2026-09-03-KesslerNativeConstructionRenderLab.png; static world-visual and pixel contracts plus the packaged build pass.

The next production gate is a compact authored Kessler Campus blueprint that consumes this validated set. The existing oversized dark Campus shell is a rejected blockout, not a base to polish.

## Kessler authored Campus — 2026-09-03

- Replaced the rejected 208x96 tiered shell with a compact 152x72 compound centered inside the existing protected atlas reservation. The reservation still leaves room for patrols, the arrival event, and the walkframe assessment without forcing the building itself to become an oversized box.
- Split public and progression spaces physically: checkpoint passages and the west quartermaster frontage are open, while a separate internal 3x10 bulkhead controls armory access. Connected patrol walks, support beams, guard towers, the two-deck headquarters, and its command crown all meet the surface footing.
- Rebuilt authored object placement as a two-pass production path. Shell tiles and walls are framed before each object resolves its registered `TileObjectData` dimensions, origin, and alternate through `WorldGen.PlaceObject`; bad dimensions or rejected anchors now throw instead of drawing plausible but invalid frame rectangles.
- The production-template capture shows native chairs, workbenches, consoles, lockers, warm wall lights, three animated power-armour racks, and both animated war standards in place. The building is terrain-anchored and the public checkpoint openings remain traversable. Evidence is archived at `Art/Validation/2026-09-03-KesslerCampusProductionRender.png`.
- The first disposable fresh-world attempt rejected a floating Campus: its saved atlas datum was 60 tiles above natural terrain, the planner tolerated 96 tiles of relief, and the late Wastes pass mistook the new foundation for the original forest surface. Kessler now keys its reservation to the blueprint's authored 54-tile surface line, scores only the actual 152-tile footprint with a 28-tile relief ceiling, and converts Wastes before placing compounds and ruins.
- Fresh-world seed `ApogeeCampusQA-2026-09-03` produced atlas hash `9DBCB5C1`. After save/load, the stricter fixture confirmed all 152 Campus columns have solid terrain contact, counted 3,588 Wastes cells beneath the footprint, preserved 15 light cells, 36 power-armour-rack cells, and 32 standard cells, kept the public frontage empty, and completed a sealed → open → re-armed bulkhead cycle. The platform-compatible locker anchor and missing Sentrix wall-light backing found during this pass are fixed. Evidence is archived at `Art/Validation/2026-09-03-KesslerFreshWorldProduction.png`.
- `dotnet build --no-restore`, Tile Lab, Surface Regression, World Visual Integrity, and Visual Contracts pass. Fresh single-player generation and save/load acceptance are complete; explicit multiplayer synchronization remains a separate network gate.
