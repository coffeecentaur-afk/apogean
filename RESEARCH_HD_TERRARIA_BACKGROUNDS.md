# HD Terraria Backgrounds: Visual and Technical Benchmark for Apogean

Research date: 2026-09-01
Wayfinder ticket: `coffeecentaur-afk/apogean#7` — “Study HD Backgrounds and comparable panorama art”

## Executive conclusion

The phrase **“HD Backgrounds” is ambiguous**. I could not verify a Terraria Workshop or Mod Browser item with that exact title from the first-party surfaces searched. The strongest likely match is Just JK’s **HD Scenery**, because its author describes high-definition sky, celestial, liquid, and ocean-background replacements. However, its official page does not establish that it is a complete, layered biome-panorama system. This report therefore treats HD Scenery as a likely fidelity reference, not a positively identified implementation reference. [HD Scenery author page](https://steamcommunity.com/workshop/filedetails/?id=2892508247)

For Apogean, the more useful combined benchmark is:

- **Backgrounds o’ Plenty** for Terraria-native panorama composition, biome-specific landmarks, and pixel-detail density.
- **Re-Logic’s newer Terraria backgrounds** for environmental storytelling and recognizable biome silhouettes.
- **Calamity** for a source-verifiable custom parallax renderer, repeat behavior, glow layers, and surface fades.
- **Remnants** for integrating background atmosphere with dense, authored world geometry without stealing focus from the playable layer.
- **tModLoader’s own API and ExampleMod** for the actual surface and underground texture contracts.

Apogean should not imitate or redistribute any of these projects’ artwork. It should adopt their *systemic lessons*: native-scale pixel clusters, large readable silhouettes, one environmental story per panorama, stable landmark geometry across time states, horizontally repeatable canvases, whole-style cross-fades at biome borders, and emissive overlays only where the fiction justifies light.

## Source boundary and confidence

Only first-party sources are used:

- official Steam Workshop pages and author-posted screenshots;
- Re-Logic’s official Terraria announcement;
- official tModLoader documentation, source, and ExampleMod;
- the official Calamity organization’s public release mirror;
- the Remnants author’s public repository and Workshop page.

No fan descriptions, videos, wikis, or extracted private assets are used. The Calamity repository states that its visual assets are proprietary; they are examined here only as a technical and visual benchmark, not as reusable material. [Calamity public mirror and rights notice](https://github.com/CalamityTeam/CalamityModPublic)

The sections below deliberately separate **verified fact**, **visual inference**, and **Apogean recommendation**.

## Verified facts

### 1. The likely “HD Backgrounds” reference: HD Scenery

The Workshop item is titled **HD Scenery**, created by Just JK. The author categorizes it as High Resolution/Overhaul/Other Packs and describes replacements for the sun, clouds, sky, moon, stars, liquids, rain, ocean background, waterfalls, blocks, walls, and several celestial objects. Its official screenshots include a broad mountain-and-lake panorama and an ocean panorama with much smoother, higher-frequency shading than standard Terraria backgrounds. The page does not publish source code, asset dimensions, layer assignments, or a verified author repository. [HD Scenery author page and screenshots](https://steamcommunity.com/workshop/filedetails/?id=2892508247)

Consequently, these statements are **not verified** and must not be assumed:

- that the mountain screenshot is split into far/middle/close textures;
- that every biome has a unique panorama;
- that separate day and night panoramas exist;
- that the promotional screenshot dimensions equal the source-asset dimensions;
- that the assets use a custom renderer rather than vanilla resource-pack slots.

### 2. Backgrounds o’ Plenty is a panorama-focused native-style reference

The author states that Backgrounds o’ Plenty revamps older pre-1.4 backgrounds to align them with Terraria’s newer art style. The public gallery shows named, compositionally distinct scenes such as Eucalyptus Jungle, Pearlforest, Hallowed Hills, Infected Desert, and Hills. The changelog records updates by background family rather than simple global recolors. [Backgrounds o’ Plenty author page and gallery](https://steamcommunity.com/sharedfiles/filedetails/?id=2971754944)

The same author reports that losslessly compressing the pack by roughly 55% reduced lag on first-time background transitions. The author also explains that a single background can be removed by deleting its `Background_283.png`, confirming that the pack targets individual Terraria background image slots. [Backgrounds o’ Plenty changelog and author comments](https://steamcommunity.com/sharedfiles/filedetails/?id=2971754944)

In a later author comment, Shashwambam explicitly praises Terraria 1.4.5 backgrounds for their **colors, composition, and level of detail** and says future work will draw heavy inspiration from them. That is first-party evidence for the pack’s stated art-direction standard, not merely an outside interpretation. [Backgrounds o’ Plenty author comment](https://steamcommunity.com/sharedfiles/filedetails/?id=2971754944)

### 3. Re-Logic treats backgrounds as handcrafted biome stories

Re-Logic announced a suite of more than ten handcrafted biome backgrounds by Luna/RunicPixels and highlighted “Fairy Circles, Ancient Bones, Frozen Rivers, and Gnarled Corrupt Trees.” The official montage shows that each scene is organized around a memorable environmental subject rather than a palette swap: a flower-ring clearing, a fossil field, a frozen river valley, and a corruption forest dominated by twisted tree silhouettes. [Re-Logic, “Bringing Backgrounds to the Foreground”](https://store.steampowered.com/news/posts/?appgroupname=Terraria&appids=105600&enddate=1748551151&feed=steam_community_announcements)

### 4. tModLoader’s native surface contract is far/middle/close plus fades

`ModSurfaceBackgroundStyle` exposes:

- `ChooseFarTexture()`;
- `ChooseMiddleTexture()`;
- `ChooseCloseTexture(ref scale, ref parallax, ...)`;
- `PreDrawCloseBackground(SpriteBatch)`, which can replace the normal closest-layer draw;
- `ModifyFarFades(float[] fades, float transitionSpeed)`, intended to move the active style toward alpha 1 and other styles toward alpha 0.

This means whole-style fading is a supported engine behavior, while truly custom spatial stitching belongs in custom drawing code—most naturally the close layer. [tModLoader `ModSurfaceBackgroundStyle` API](https://docs.tmodloader.net/docs/stable/class_mod_surface_background_style.html), [official `ModBackgroundStyle` source](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs)

ExampleMod demonstrates the intended fade loop, selection of far/middle/close texture slots, and even frame-based middle-layer animation. [Official ExampleMod surface background](https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs)

The underground contract is different and stricter. The documented core slots are:

- index 0: ground/sky border, 160×16;
- index 1: between ground and rock, 160×96;
- index 2: ground/rock border, 160×16;
- index 3: rock layer, 160×96.

tModLoader’s source notes that the rightmost 32 pixels appear to duplicate the leftmost 32 pixels for repeat continuity. [Official `ModUndergroundBackgroundStyle` source](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs)

### 5. Calamity proves a practical custom multi-layer implementation

Calamity’s official public release mirror contains an Astral surface implementation with a normal far and middle selection plus a custom close-background draw. The source labels five visual depths: horizon, far, middle, close, and front. Its custom draw repeats each 1024-pixel-wide texture enough times to cover the screen (`screenWidth / scaledWidth + 2`) and uses progressively stronger parallax for nearer layers. It also draws separate glow textures over the middle, close, and front layers while carrying the current background alpha into those draws. [Calamity `AstralSurfaceBGStyle`](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Backgrounds/AstralSurfaceBGStyle.cs)

The corresponding public assets have these source dimensions:

| Astral asset | Dimensions |
|---|---:|
| Horizon | 1024×435 |
| Far | 1024×492 |
| Middle | 1024×600 |
| Close | 1024×700 |
| Front | 1024×600 |
| Middle/close/front glow masks | same dimensions as their base layers |

These dimensions were read from the image files in Calamity’s official release mirror. The directory also shows native underground assets using 160×16 and 160×96 contracts. [Calamity background assets](https://github.com/CalamityTeam/CalamityModPublic/tree/1.4.4/Backgrounds), [Calamity `AstralUndergroundBGStyle`](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Backgrounds/AstralUndergroundBGStyle.cs)

Calamity’s implementation is evidence that 1024-wide authored panoramas, multiple custom close depths, repeat loops, biome tinting, and independent emissive masks are viable. It is **not** evidence that Apogean should copy Calamity’s exact scale, parallax constants, palette, or artwork.

### 6. Remnants favors integration with playable world geometry

Remnants’ official Workshop description presents it as a complete world reconstruction, and its author-posted screenshots emphasize dense terrain silhouettes, open sightlines, structural framing, and backgrounds that support rather than dominate the playable layer. [Remnants official Workshop page and screenshots](https://steamcommunity.com/sharedfiles/filedetails/?id=3696148536)

The author’s public source includes simple underground styles for granite, marble, and honeycomb biomes. Each uses a 160×96 image in all four slots. That is a deliberately economical implementation: the identity comes primarily from world geometry, tile art, walls, and lighting, while the background remains a low-frequency support layer. [Remnants background source directory](https://github.com/lazy-wombat/Remnants/tree/main/Content/Biomes/Backgrounds), [Remnants granite background implementation](https://github.com/lazy-wombat/Remnants/blob/main/Content/Biomes/Backgrounds/granitecave.cs)

The public repository does **not** support attributing a Calamity-like five-layer custom surface renderer to Remnants. Its value here is composition and integration, not a verified advanced surface-background architecture.

## Visual inference from official screenshots and public assets

The following observations are visual analysis, not author-stated facts.

### HD Scenery

- The mountain-and-lake image uses broad atmospheric bands: bright clouds, pale distant mountains, darker near mountains, a forest belt, and reflective water.
- Shading is smoother and more illustrative than Terraria’s normal sprite density. It reads as “HD” primarily through more tonal steps and softer cloud/mountain modeling.
- This fidelity can create a scale mismatch: a very smooth panorama may make hard-edged Terraria sprites look pasted over the scene.
- Its strongest lesson for Apogean is **large atmospheric depth bands**, not its softness or rendering density.

### Backgrounds o’ Plenty

- Each named scene has a dominant landmark language: willow canopies, multicolored pearl trees, fossilized remains, crystalline fields, or ecological clearings.
- Depth is created by reducing contrast, saturation, and edge complexity with distance. Far mountains are simple; middle vegetation carries biome identity; the nearest band holds the most texture and silhouette complexity.
- Repetition is disguised by irregular landmark spacing and alternating large/small masses, not by filling every gap with detail.
- The scenes remain recognizably pixel art at full-screen presentation. Pixel clusters describe leaves, bark, rock faces, and ground texture without becoming miniature sprite-sheet noise.

### Re-Logic’s newer backgrounds

- The composition is narrative. “What happened here?” can be answered from one glance: animals formed a clearing, enormous creatures died, a river froze through a valley, or corruption twisted a forest.
- The closest background silhouettes partially frame the playable terrain, while middle layers carry the signature landmark and far layers establish climate/geology.
- The corruption example demonstrates that a dark biome does not need uniform darkness. Distinct value bands keep the giant trees readable.

### Calamity

- Its Astral assets use sparse, large shapes and limited bright accents. The glow masks are reserved for embedded energy rather than duplicating the whole painting at full brightness.
- Separate close/front depths let foreground silhouettes move convincingly without forcing the far skyline to move too quickly.
- The 1024-wide repeat unit is large enough to hold a composition, but still requires seam-aware landmark placement because ultrawide displays can reveal repetition.

### Remnants

- Background silhouettes echo the scale and direction of foreground terrain. In the jungle screenshot, distant canopy masses and branches reinforce vertical jungle shafts without matching the foreground tile detail one-for-one.
- Local lighting and darkness preserve exploration focus. The background is visible enough to establish place but does not flatten caves or expose hidden routes.

## Apogean recommendations

### 1. Adopt “native panoramic,” not literal HD

Apogean should target **high-composition, Terraria-native pixel art**:

- hard pixel edges and deliberate clusters;
- no anti-aliased painterly blur in source assets;
- broad forms before texture;
- fewer tonal steps in close silhouettes than HD Scenery;
- detail density comparable to Backgrounds o’ Plenty and Re-Logic’s modern backgrounds;
- one memorable environmental story per variant.

“HD” should mean a scene that remains interesting across a wide screen, not a higher-frequency painting that competes with Terraria’s sprites.

### 2. Give every depth a distinct job

| Layer | Apogean job | Examples |
|---|---|---|
| Far | Climate, horizon, continental damage | ash cloud shelf, broken skyline, distant corporate orbital debris, mountain scar |
| Middle | Biome identity and primary landmark | ruined highway, collapsed research dome, dead forest basin, Maw root field |
| Close | Tactile framing and immediate danger | ashen trunks, exposed rebar, dry root/tumbleweed tangles, fibrous Maw membranes |
| Optional glow | Fictionally luminous material only | amber Maw vesicles, emergency beacons, exposed reactor windows |

Do not make nine recolors of one forest composition. Shared far-horizon families are acceptable where geography should connect, but middle and close silhouettes must identify the biome without relying on hue.

### 3. Use whole-style fades at biome borders

The user’s desired transition is a fade, not literal panorama stitching. Use tModLoader’s background-style fade path for the complete far/middle/close set. Do not author special “half forest/half desert” panorama files for every adjacency; that creates a combinatorial art burden and still fails when three biome influences overlap.

If a later close-layer treatment needs spatial continuity, limit it to a neutral transitional foreground—dust, low roots, fog, or debris—and draw it through `PreDrawCloseBackground`. The principal compositions should still cross-fade as complete styles.

Transition acceptance requirements:

- no black, transparent, or sky-colored gap during a switch;
- far, middle, and close identities fade in the same direction;
- no single layer visibly snaps while the others fade;
- no rapid style thrashing while standing on a biome boundary;
- underground transitions use authored border bands or selection hysteresis because the underground API does not expose the same three-layer surface fade model.

### 4. Keep landmark geometry stable across time states

Use one base composition for day, night, rain, eclipse, and other lighting states. Change:

- palette/tint;
- sky and celestial treatment;
- fog/rain/dust overlays;
- emissive-mask intensity;
- a small number of event-only silhouettes where justified.

Do not move ruins, mountains, trees, or roots between day and night. Stable geography makes the world feel authored and avoids doubling every panorama asset. A separate full nighttime painting should be reserved for a scene whose silhouette genuinely changes, not used by default.

### 5. Standardize the surface canvas before producing the full library

The current Apogean surface assets already use a 1024-pixel repeat width, close to Calamity’s verified convention. Keep 1024 as the prototype width so art can improve without simultaneously changing renderer behavior. During the benchmark, retain the current Apogean heights unless a coverage test fails:

- far: 1024×408;
- middle: 1024×600;
- close: 952×480 currently, but normalize new work to **1024 pixels wide** so all layers share a predictable repeat unit.

Every layer must tile horizontally. Test the first and last 64 pixels side-by-side while drawing, and also test two copies of the entire image. Avoid unique landmark edges at the seam. Use transparent empty space only in close layers where the layer below is intentionally visible.

For underground art, follow the documented 160×16 / 160×96 contract rather than stretching surface panoramas into caves.

### 6. Make the ruined Earth and the Maw readable without color

For the ruined surface:

- use broad ash-brown and smoke-gray value groups;
- retain harvestable trees in the playable layer, but use dead/charred trunk masses in close backgrounds;
- represent wall foliage as dry root balls, wind-knotted brush, or tumbleweed tangles rather than recolored leaf clouds;
- distribute ruins as recognizable infrastructure fragments, not random gray rectangles.

For the Maw:

- use hooked, tensile, fibrous silhouettes rather than Crimson-like sacs and veins;
- reserve yellow/amber for contamination and internal energy;
- make its middle landmark a geological wound or root convergence, not a purple forest recolor;
- allow only selected amber organs to glow; ordinary black soil and dead tissue remain non-emissive.

If grayscale thumbnails of Forest and Maw cannot be distinguished, the compositions have failed even if the palettes differ.

### 7. Preserve gameplay readability

The closest background should not share the same value, hue, and edge frequency as common enemies, projectiles, platforms, or player-built blocks. Keep high-contrast one-pixel accents out of large background regions. The playable tile layer must remain the sharpest and most contrast-rich plane except for sparse emissive landmarks.

## Finite visual benchmark plan

This benchmark intentionally stops after a small, decisive proof. It must be completed before mass-producing all biome backgrounds.

### Scope

Produce only two surface scenes:

1. **Ruined Forest / dead overworld** — validates Terraria-native ruins, ashen trees, dry-root wall language, and safe-spawn readability.
2. **The Maw** — validates the mod’s signature fibrous contamination, amber glow restraint, and hostile silhouette language.

Round A contains one variant per scene:

- 2 far layers;
- 2 middle layers;
- 2 close layers;
- at most 2 matching glow masks, only if the Maw composition contains luminous organs or machinery.

Round B adds the second variant only after Round A passes. Total benchmark maximum: **12 base images plus 4 optional glow masks**. No other biome panorama enters production before the benchmark is accepted.

### Three-pass limit

1. **Composition pass** — grayscale silhouettes and repeat seam only. No detailed texture.
2. **Color pass** — final palette families, depth separation, day/night tint test, and restrained material texture.
3. **In-game polish pass** — seam repair, layer offsets, contrast cleanup, and event lighting.

After pass 3, each scene is either accepted or its specification is rewritten. Do not cycle through unbounded “one more polish” revisions.

### Required test matrix

Capture the same camera position under:

| Axis | Cases |
|---|---|
| Resolution | 1920×1080 and 2560×1080 |
| Zoom | normal play zoom and the highest supported user zoom used by the test machine |
| Time | noon, midnight, dawn/dusk |
| Weather/event | clear, rain, solar eclipse |
| Motion | stationary and a 10-second horizontal run |
| Boundary | centered in biome and crossing Forest ↔ Maw in both directions |

### Pass/fail gates

A scene passes only when all gates pass:

1. **Identity:** A grayscale 320-pixel-wide thumbnail is correctly identifiable as Ruined Forest or Maw without its label.
2. **Layer separation:** Far, middle, and close remain distinguishable in grayscale; the close layer never erases the middle landmark across most of the viewport.
3. **Repeat:** Two side-by-side copies show no line, jump, empty strip, or visibly duplicated edge landmark.
4. **Transition:** Crossing the boundary produces a coherent fade with no snap, black frame, gap, or rapid oscillation.
5. **Readability:** The player, platforms, common enemy silhouettes, and representative hostile projectiles remain immediately readable at noon and midnight.
6. **Time stability:** Landmark positions do not change between time states; only lighting, sky, overlays, and justified emissive elements change.
7. **Native style:** At 100% zoom, the art uses hard pixel clusters and does not contain soft anti-aliasing or smooth-painted texture that makes Terraria sprites look pasted on.
8. **Performance:** Ten repeated boundary crossings produce no persistent frame-rate reduction and no repeatable first-transition hitch visible to the tester. PNGs must be losslessly optimized before this test, because Backgrounds o’ Plenty’s author specifically identified asset compression as transition-relevant.
9. **Story:** Each scene communicates one sentence without explanatory text. Proposed targets: “civilization burned here and nature never recovered” for Ruined Forest; “a living wound has anchored itself through the planet” for the Maw.

### Acceptance artifact

For each scene, retain one review sheet containing:

- the three source layers on transparency;
- a doubled-width seam preview;
- noon and midnight in-game screenshots;
- one Forest ↔ Maw transition capture;
- the one-sentence environmental story;
- the palette and any glow colors.

Once both variants of both scenes pass, freeze the canvas, layer-role, seam, tint, and review-sheet conventions as the Apogean background art specification. Only then expand to desert, jungle, snow, ocean, world evils, Hallow, underground, and Underworld families.

## Implemented native-detail benchmark — 2026-09-04

The first in-engine proof deliberately exceeded the original 1024-wide planning assumption. Forest, Desert, Jungle, Snow, Corruption, Crimson, Hallow, Ocean, and Glowing Mushroom now each have a transparent V0 far/middle/close set exported at approximately 1,672–2,161 pixels wide and 728–941 pixels high. A custom `PreDrawCloseBackground` compositor repeats those textures at 1:1 source scale, assigns 0.055 / 0.14 / 0.30 horizontal parallax, anchors vertical motion to `Main.worldSurface`, and fills only the lower horizon beneath the transparent layers. This avoids vanilla's enlargement path and keeps the authored high-frequency pixel clusters intact at 2560×1440.

Automated contracts now enforce all of the following across 27 layers:

- at least 1,600×700 source dimensions and no axis above 4,096 pixels;
- hard alpha only, a transparent top-center sky sample, and preservation of authored transparent regions;
- exact equality between the first and final pixel columns at every row;
- at least 128 sampled opaque colors per layer;
- no more than 32 MiB raw RGBA for the three layers visible in one biome;
- no more than 256 MiB raw RGBA for the complete diagnostic library.

The accepted library is 162.01 MiB raw RGBA. Live 2560×1440 fixtures verified all nine noon compositions, Forest at midnight and during a solar eclipse, and Mushroom after fixing an exporter defect that had flattened its transparent sky to opaque black. The same fixtures proved that night and eclipse retain the same landmark geometry while a conservative luminance floor keeps the ruins readable.

This is still a renderer benchmark, not the production background switch. The current tModLoader session reported 785.9 MiB for the whole Apogean mod after loading all existing content; that number cannot be attributed to these backgrounds alone, but it is too high to accept without a dedicated before/after residency profile. Production promotion therefore remains gated on style crossfades, one original alternate composition per biome, repeated transition-hitch testing, lossless asset optimization, and a measured resident-memory strategy. Source masters and validation captures remain excluded from the packaged mod through `buildIgnore`.

## Decision for issue 7

The research ticket can be considered resolved at the specification level with this decision:

> Apogean will use 1024-wide, horizontally repeatable, Terraria-native pixel panoramas with unique far/middle/close roles; whole-style biome fades; stable geometry across day/night; optional material-specific glow masks; and a two-biome, three-pass visual benchmark before full-library production.

The exact “HD Backgrounds” identity remains unconfirmed. HD Scenery is the likely fidelity reference, while Backgrounds o’ Plenty is the stronger verified panorama-art reference. That ambiguity does not block implementation because the resulting benchmark is grounded in official screenshots, public source, and tModLoader’s actual renderer contract.
