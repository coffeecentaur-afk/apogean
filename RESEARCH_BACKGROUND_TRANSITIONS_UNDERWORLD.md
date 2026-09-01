# Apogean Background Transitions, Underground Routing, and Underworld Research

Status: source-audited technical sidecar; no implementation changes

Scope: surface-background transitions, underground background routing, Underworld rendering, client/multiplayer state, and an Engraft vertical-scar architecture

Excluded: world generation, tile art production, dialogue/UI, and gameplay balance

## Source baseline and evidence labels

Two tModLoader baselines matter here:

- The current development source reviewed was tModLoader branch `1.4.5` at commit [`2534f5682a46661c9aec633bea0852020e4fa796`](https://github.com/tModLoader/tModLoader/tree/2534f5682a46661c9aec633bea0852020e4fa796). Its loader hooks and patches are the primary source for current behavior.
- The locally referenced assembly available to the project identifies itself as `tModLoader 1.4.4.9`. The official generated API reference also identifies the Terraria assembly as 1.4.4.9. [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html)

The development source already has 16 vanilla surface-background IDs, while the 1.4.4.9 API reference still exposes arrays initialized with 14 entries. [Current `SurfaceBackgroundID` patch](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ID/SurfaceBackgroundID.cs.patch) [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html) Apogean must therefore use named IDs and loader-assigned slots rather than hard-coded “mod backgrounds start at 14” assumptions.

Labels used below:

- **Confirmed—current source:** directly visible in the pinned 1.4.5 tModLoader source.
- **Confirmed—installed 1.4.4.9:** directly visible in the locally referenced assembly and consistent with the official 1.4.4.9 API reference.
- **Source-audit result:** a named implementation was searched for and was not present in the reviewed repository revision.
- **Inference:** follows from confirmed control flow but is not promised by the public API.
- **Recommendation:** proposed Apogean policy, not engine behavior.

## Executive answer

The existing surface transition problem is not a lack of parallax support. tModLoader already keeps separate alpha arrays and draws every active surface style with the correct far, middle, and close parallax path. The important current-source defect is that the hook named `ModifyFarFades` is called with `Main.bgAlphaFrontLayer`, while modded far and middle layers are drawn using `Main.bgAlphaFarBackLayer`; the close layer uses `bgAlphaFrontLayer`. [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6952-L6973) [Current `BackgroundLoaders.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L199-L324)

That creates two practical failure modes:

1. Copying ExampleMod's fade loop causes the front/close alpha to be advanced a second time after vanilla already advanced it, so the close layer finishes its transition faster than far/middle. [Current ExampleMod surface style](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs) [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6952-L6973)
2. If `ChooseFarTexture`, `ChooseMiddleTexture`, or `ChooseCloseTexture` returns a different texture while the style's `Slot` remains unchanged, no alpha target changes. The texture changes immediately at the style's existing alpha, which is a true same-slot snap. The loader asks each active style for its current texture every draw. [Current `BackgroundLoaders.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L199-L324)

The Underworld is a separate rendering system. `ModUndergroundBackgroundStyle` officially owns four cave textures, not the five-layer Hell panorama. [Official `ModUndergroundBackgroundStyle` reference](https://docs.tmodloader.net/docs/preview/class_mod_underground_background_style.html) Terraria stores five selected Underworld layers in `Main.underworldBG` and draws from `TextureAssets.Underworld`, which is a different asset array. [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html) [Official `TextureAssets` reference](https://docs.tmodloader.net/docs/preview/class_texture_assets.html)

There is no dedicated `ModUnderworldBackgroundStyle` or documented “replace Hell panorama” hook in the reviewed tModLoader API. The supported mod-visual mechanism is a depth-aware `CustomSky`; an opaque custom sky can cover the vanilla Hell panorama and draw Apogean's own parallax layers, but complete replacement relies on current draw ordering and must be regression-tested on every supported tModLoader version. [Official `CustomSky` reference](https://docs.tmodloader.net/docs/stable/class_custom_sky.html) [Official `SkyManager` reference](https://docs.tmodloader.net/docs/preview/class_sky_manager.html)

## 1. How surface background transitions actually work

### 1.1 Two alpha arrays, three modded layer hooks

`Main` owns `bgAlphaFarBackLayer` and `bgAlphaFrontLayer`. [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html) In the current loader:

| Layer | Texture hook | Alpha used by tModLoader | Parallax path |
|---|---|---|---|
| Far | `ChooseFarTexture()` | `Main.bgAlphaFarBackLayer[style.Slot]` | Engine far-background placement and tiling |
| Middle | `ChooseMiddleTexture()` | `Main.bgAlphaFarBackLayer[style.Slot]` | Engine middle-background placement and tiling |
| Close | `ChooseCloseTexture(...)` | `Main.bgAlphaFrontLayer[style.Slot]` | Engine close-background placement; hook can adjust scale/parallax |

All three rows are confirmed in `SurfaceBackgroundStylesLoader`. [Current `BackgroundLoaders.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L199-L324)

The engine updates its back and front visibility arrays before drawing, and current tModLoader then calls `ModifyFarFades(bgStyle, bgAlphaFrontLayer, transitionSpeed)`. [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6952-L6973) This is inconsistent with both the hook name and its documentation, which says the method controls far-background transparency. [Current `ModBackgroundStyle.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L34-L86)

**Recommendation:** On the currently pinned branch, implement `ModifyFarFades` as a no-op and let Terraria's built-in back/front visibility updates cross-fade distinct style slots. Keep a regression test around this method because a future tModLoader fix may change the passed array to match the documentation.

### 1.2 What `GlobalBackgroundStyle` does—and does not do

`GlobalBackgroundStyle.ChooseSurfaceBackgroundStyle(ref style)` and `ChooseUndergroundBackgroundStyle(ref style)` are final mutation hooks. Terraria first resolves vanilla and `ModBiome` candidates, then invokes every registered global hook against the chosen integer style. There is no priority parameter or built-in arbitration between global hooks. [Current `ModBackgroundStyle.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L89-L121) [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6985-L7028)

A global hook changing from style slot A to style slot B does not inherently snap. The engine retains both slots' alphas during the transition and draws each nonzero style. [Current `BackgroundLoaders.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L199-L324)

**Confirmed—installed 1.4.4.9:** ordinary surface changes have a short built-in target delay and then move the alpha arrays by a fixed amount per draw. Underground styles instead set `ugBackTransition` to 1 and subtract 0.25 per draw, producing only about four rendered transition steps. `Main.bgDelay`, both alpha arrays, and `ugBackTransition` are exposed in the official assembly reference. [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html)

### 1.3 Why a layer can still appear to snap

| Symptom | Confirmed or likely cause | Correct response |
|---|---|---|
| Close finishes before far/middle | Apogean repeats ExampleMod's full fade loop on the front alpha array after Terraria already updated it. | No-op the hook on the pinned branch; test after tModLoader upgrades. |
| Far, middle, or close texture changes instantly without a style transition | A variant/day/night selector changed the texture returned by the same style slot. | Give independently transitioning visual states separate style slots, or blend through a custom compositor. |
| Cross-fade is visible but landmarks “jump” horizontally | The two textures have different widths, scale, parallax, or landmark phase, so their tiled origins do not align. | Standardize dimensions, baseline, repeat seam, scale, and parallax across a style family. |
| Underground change looks nearly instant | Vanilla's installed transition has roughly four draw steps. | Add client-side spatial hysteresis so the target does not flap; use a custom compositor only if a longer fade is essential. |
| Any transition is instant after teleport/load | Terraria exposes `instantBGTransitionCounter` and can force immediate background convergence during transition-sensitive states. [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html) | Accept instant convergence on world/teleport boundaries; do not persist alpha state. |

The first two causes follow directly from current tModLoader's call and draw paths. [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6952-L6973) [Current `BackgroundLoaders.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L199-L324)

### 1.4 Hooks available for smooth behavior

- `ModBiome.SurfaceBackgroundStyle` and `.UndergroundBackgroundStyle` participate in scene-effect priority and weight selection. [Current `ModSceneEffect.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/ModSceneEffect.cs) [Current ExampleMod surface biome](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/ExampleMod/Content/Biomes/ExampleSurfaceBiome.cs)
- `ModSceneEffect.GetWeight` resolves competition between active mod scene effects at the same priority; it is not a fade value. [Current `ModSceneEffect.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/ModSceneEffect.cs#L70-L109)
- `GlobalBackgroundStyle` can replace the final chosen slot, fill underground textures, and modify the fade array passed by the loader. [Official `GlobalBackgroundStyle` reference](https://docs.tmodloader.net/docs/preview/class_global_background_style.html)
- `PreDrawCloseBackground` can suppress tModLoader's close-layer draw so a style can render its own close background. [Current `ModBackgroundStyle.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L63-L86)
- `CustomSky.Draw(spriteBatch, minDepth, maxDepth)` supports depth-aware visual layers and an explicit `Update` method suitable for time-based intensity. [Official `CustomSky` reference](https://docs.tmodloader.net/docs/stable/class_custom_sky.html) `SkyManager` invokes skies in depth ranges and exposes `DrawToDepth`, `DrawRemainingDepth`, and cloud/tile-color processing. [Official `SkyManager` reference](https://docs.tmodloader.net/docs/preview/class_sky_manager.html)

**Recommendation:** Use stable style slots plus a client-side target resolver and let the engine keep parallax and cross-fading. Do not manually redraw all surface layers merely to add hysteresis. Custom drawing should be reserved for same-slot weather/time blending or the Underworld, where the normal style API is insufficient.

## 2. Exact surface routing and priority

The surface selector first asks the local player's active mod scene effect for a modded style and priority. A `BiomeHigh` or greater mod style wins before vanilla biome routing. Otherwise Terraria evaluates vanilla conditions, allows a `BiomeMedium` mod style after the high-priority vanilla group, then allows a `BiomeLow` mod style after Jungle/Snow but before the forest fallback. The current source inserts those three priority gates explicitly. [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6985-L7028) The intended priority bands are documented by `SceneEffectPriority`. [Current `SceneEffectPriority.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/SceneEffectPriority.cs)

Confirmed routing order, from strongest to weakest:

| Order | Surface candidate | Full panoramic style? | Notes |
|---:|---|---|---|
| 1 | Mod scene effect at `BiomeHigh` or above | Yes | Also includes `Environment`, `Event`, and boss priorities because they compare above `BiomeHigh`. |
| 2 | Ocean/beach | Yes | Selects Ocean or its evil/Hallow treatment. |
| 3 | Surface Glowing Mushroom | Yes | Vanilla Mushroom style. |
| 4 | Desert | Yes | Pure Desert or an evil/Hallow desert variant. Current 1.4.5 declares separate Corrupt and Crimson desert IDs in addition to the shared good/evil desert family. [Current `SurfaceBackgroundID` patch](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ID/SurfaceBackgroundID.cs.patch) |
| 5 | Hallow | Yes | Vanilla Hallow style. |
| 6 | Corruption/Crimson | Yes | Crimson can win when blood tiles dominate the evil count. |
| 7 | Mod scene effect at `BiomeMedium` | Yes | Intended to beat Jungle, Graveyard, and Snow but not the high group. |
| 8 | Jungle | Yes | Vanilla Jungle style. |
| 9 | Snow | Yes | Vanilla Snow style. |
| 10 | Mod scene effect at `BiomeLow` | Yes | Intended to beat ordinary Overworld/Night but not the biome groups above. |
| 11 | Forest world-region variant | Yes | Chooses among the world-generated forest background sets by horizontal region. |
| Final | Every `GlobalBackgroundStyle` hook | Whatever slot it assigns | Unconditionally mutates the result; no priority information is supplied. |

The named vanilla surface styles are defined by `SurfaceBackgroundID`; the current development branch additionally contains Corrupt Desert, Crimson Desert, and Empty entries. [Current `SurfaceBackgroundID` patch](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ID/SurfaceBackgroundID.cs.patch)

### Graveyard is not a separate surface panorama

Graveyard is absent from the surface-style selector. Instead, Terraria's fog power uses the greater of cloud alpha and `GraveyardVisualIntensity * 0.92`, and the Graveyard scene filter is managed separately. [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6700-L6724) [Current `SceneState.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/SceneState.cs.patch)

**Recommendation:** A ruined Graveyard should normally remain the selected underlying biome panorama and receive a fog/color/sky overlay. Giving Graveyard a wholly separate panorama would erase useful location identity such as Snow Graveyard versus Forest Graveyard.

## 3. Exact underground routing and micro-biome distinctions

tModLoader has 22 vanilla underground style IDs. [Current `BackgroundLoaders.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L103-L139)

Confirmed selector order in the reviewed implementation:

1. Start with the world-generated horizontal cave style and apply a `BiomeLow` mod style.
2. Snow overrides that choice while above the deepest Hell transition.
3. Jungle overrides the base/Snow choice when its tile count wins; a `BiomeMedium` mod style then gets a chance to override.
4. Ocean caves select normal, Corrupt, Crimson, or Hallow ocean styles.
5. In the deeper cavern band, Corruption, Crimson, or Hallow select dedicated cavern art; Snow combinations select dedicated evil/Hallow ice-cavern styles.
6. Glowing Mushroom overrides the preceding vanilla choices.
7. A `BiomeHigh` or greater mod style overrides all of those.
8. Every `GlobalBackgroundStyle.ChooseUndergroundBackgroundStyle` hook mutates the final style last.

The low/medium/high insertion points and final global hook are visible in the current `Main` patch. [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6304-L6348) The installed assembly confirms the full vanilla order and the normal/evil/Hallow Snow and Ocean combinations; its exposed state includes `caveBackStyle`, `caveBackX`, `undergroundBackground`, and `ugBackTransition`. [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html)

### Actual panoramic styles versus wall-driven micro-biomes

| Biome or condition | Surface panorama | Underground panorama | What actually supplies identity |
|---|---|---|---|
| Glowing Mushroom | Yes when it reaches the surface | Yes; dedicated vanilla underground style | Full panorama plus biome walls/tiles |
| Jungle | Yes | Yes; dedicated underground Jungle style | Full panorama plus walls/tiles |
| Snow | Yes | Yes; dedicated Ice style and deep evil/Hallow ice variants | Full panorama plus walls/tiles |
| Desert | Yes | **No dedicated vanilla underground style slot** | Underground Desert's unsafe walls, tiles, lighting, and structures draw over a generic cave/depth background |
| Corruption | Yes | Yes in the deep cavern band | Full panorama; upper underground may retain base cave art behind evil walls |
| Crimson | Yes | Yes in the deep cavern band | Full panorama; upper underground may retain base cave art behind evil walls |
| Hallow | Yes | Yes in the deep cavern band | Full panorama; upper underground may retain base cave art behind Hallow walls |
| Ocean | Yes | Yes; normal and evil/Hallow ocean cave variants | Full panorama |
| Granite | No | **No vanilla full-screen style selected** | Granite walls/tiles, lighting, water, and spawns; `ZoneGranite` is a biome flag, not a panorama selector |
| Marble | No | **No vanilla full-screen style selected** | Marble walls/tiles and spawns; `ZoneMarble` is not consulted by the background selector |
| Spider Nest | No | **No vanilla full-screen style selected** | Spider unsafe wall and local content; there is no `ZoneSpider` branch in the panorama selector |
| Graveyard | No separate panorama | No separate panorama | Fog/filter intensity and scene behavior over the underlying biome |
| Dungeon/Temple | No dedicated surface panorama | No dedicated cave-style branch | Their dense unsafe walls visually replace the open cave background; music/map identity is separate |

The “no dedicated style” entries are a source-audit result from the current selector: Desert, Granite, Marble, Spider, Graveyard, Dungeon, and Temple do not appear as underground style branches, while Snow, Jungle, Ocean, evil/Hallow, and Mushroom do. [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6304-L6348) The official bestiary API having distinct Granite, Spider, Dungeon, Temple, and Underground Desert map backgrounds does not imply those are live parallax styles; map/bestiary backgrounds are a different system. [Official bestiary biome-tag reference](https://docs.tmodloader.net/docs/preview/class_bestiary_database_n_p_cs_populator_1_1_common_tags_1_1_spawn_conditions_1_1_biomes.html)

### Mod-biome routing

`SceneEffectLoader` evaluates active mod scene effects, sorts them by corrected weight—priority plus bounded `GetWeight`—and independently selects the first available surface background, underground background, music, water style, and other scene fields. [Current `SceneEffectLoader.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/SceneEffectLoader.cs) The surface and underground background loaders then read `Main.LocalPlayer.CurrentSceneEffect`, so drawing is local-player/client-facing. [Current `BackgroundLoaders.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L103-L187)

**Recommendation:** Use a `ModBiome` for the Engraft scar itself. Reserve `GlobalBackgroundStyle` for Apogean's explicit “replace the whole world's ordinary backgrounds” mode, and leave an already selected third-party mod style unchanged outside the Engraft. A global hook is too late to participate politely in priority arbitration.

## 4. The four-texture underground contract

The documented contract is exact:

| Index | Role | Documented dimensions |
|---:|---|---:|
| 0 | Border between sky and ground layers | 160 × 16 |
| 1 | Background between rock and ground layers | 160 × 96 |
| 2 | Border between ground and rock layers | 160 × 16 |
| 3 | Background in the rock layer | 160 × 96 |

The documentation also notes that the rightmost 32 pixels appear to duplicate the leftmost 32 pixels. [Official `ModUndergroundBackgroundStyle` reference](https://docs.tmodloader.net/docs/preview/class_mod_underground_background_style.html) ExampleMod assigns exactly these four indexes. [Current ExampleMod underground style](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs)

**Inference:** Terraria effectively repeats a 128-pixel body with a 32-pixel overlap used to hide the seam during horizontal parallax. Every authored set should preserve that seam structure and share the same visual horizon across its four files.

The runtime currently passes a larger internal array because Terraria also tracks deeper/Underworld-transition textures, but the public contract grants a mod control over indexes 0–3 only. [Current `ModBackgroundStyle.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L12-L31) Writing undocumented indexes 4–6 is implementation-dependent and must not be used as Apogean's Hell solution.

### Preserving per-biome variants

A seed-stable variant may safely choose its four textures dynamically if the variant never changes while the world is loaded. If a player cycles the variant while the same `ModUndergroundBackgroundStyle.Slot` remains selected, Terraria sees no style-ID change and does not start `ugBackTransition`; the texture set snaps. This follows from the selector comparing the new integer style to `Main.undergroundBackground`, while `FillTextureArray` is called later for the selected integer. [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6304-L6364)

**Recommendation:**

- For world-seed variants that remain fixed, one style class per biome may return the saved variant's four textures.
- For a player-triggered background changer that must visibly cross-fade, assign each variant a distinct style slot or perform a custom compositor fade.
- Never change only one of the four files. Treat a variant as an atomic four-texture set.
- Keep width, seam, border height, and depth horizon identical among variants so even a valid cross-fade does not slide.

## 5. Underworld/Hell: separate renderer, separate solution

### 5.1 What the normal underground style does not replace

Terraria exposes `Main.underworldBG` as an array of five selected layer indexes, while `TextureAssets.Underworld` is a separate array of Underworld textures. [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html) [Official `TextureAssets` reference](https://docs.tmodloader.net/docs/preview/class_texture_assets.html) The installed renderer draws those five layers with different depth/parallax factors when the camera reaches roughly the bottom 220 tiles, then lets `SkyManager` draw remaining depth. This is confirmed in the locally referenced 1.4.4.9 assembly; the public API exposes the same five-layer state and depth-aware sky manager. [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html) [Official `SkyManager` reference](https://docs.tmodloader.net/docs/preview/class_sky_manager.html)

`ModUndergroundBackgroundStyle.FillTextureArray` officially controls only the cave-layer indexes 0–3. [Official `ModUndergroundBackgroundStyle` reference](https://docs.tmodloader.net/docs/preview/class_mod_underground_background_style.html) A high-priority underground style can therefore change cave art while the player is at Underworld depth, but it does not replace the five-layer Hell panorama drawn by `DrawUnderworldBackground`.

### 5.2 Supported and unsupported approaches

| Approach | Status | Result |
|---|---|---|
| `ModUndergroundBackgroundStyle` | Supported, but wrong subsystem | Replaces four cave textures; does not replace the Hell panorama. |
| `CustomSky` registered with `SkyManager` and activated through `ManageSpecialBiomeVisuals` | Supported mod visual API | Can draw depth-aware layers and fade intensity; an opaque final-depth composition can cover the vanilla panorama. |
| Assigning `TextureAssets.Underworld` elements or mutating `Main.underworldBG` | Public fields but no dedicated mod contract | Global, conflict-prone, vulnerable to world-style resets, and not recommended. |
| IL detour of `DrawUnderworldBackground` | Technically possible, unsupported high-coupling implementation | Exact suppression/replacement, but version-fragile and likely to conflict with other visual mods. |

`ModSceneEffect.SpecialVisuals` explicitly directs mods to register a `CustomSky` or screen shader and activate it with `Player.ManageSpecialBiomeVisuals`. [Current `ModSceneEffect.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/ModSceneEffect.cs#L102-L114) ExampleMod's sky implementation demonstrates maintaining an intensity over time rather than switching one frame to the next. [ExampleMod `PuritySpiritSky.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/ExampleMod/Old/NPCs/PuritySpirit/PuritySpiritSky.cs)

The last ExampleMod link is historical API sample code, not a current content example; the current public API still exposes the same `CustomSky.Update`, `Draw`, activation, cloud-alpha, and tile-color methods. [Official `CustomSky` reference](https://docs.tmodloader.net/docs/stable/class_custom_sky.html)

**Recommendation:** Implement Engraft Hell as a `CustomSky` compositor with its own 0–1 client-side intensity. At the final depth range, draw an opaque Engraft base that covers vanilla Hell, then draw three to five authored layers using `screenPosition.X * parallax`, fixed repeating widths, and the same point sampling used by Terraria. This keeps ordinary Hell intact outside the scar. Treat the exact “draw after the vanilla panorama” depth condition as a version-tested adapter, because the public API guarantees depth ranges but does not document Terraria's private Hell-layer call order.

## 6. Day, night, weather, Graveyard, and events

### Day/night

Normal gameplay does not select a second panorama merely because `Main.dayTime` changes. Surface art is tinted through `Main.ColorOfTheSkies`/surface background colors, while the same style texture remains selected; day/night directly selects a style only for the game menu in the reviewed renderer. `Main.dayTime` and `Main.time` are the official world-clock state. [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html)

**Recommendation:** Keep landmarks identical across day/night. Use sky color, lighting, emissive windows, clouds, and a custom overlay for night. If fully different night files are required, give day and night distinct style slots so the engine can cross-fade; changing the texture returned by one slot will snap.

### Rain, clouds, wind, and fog

Terraria tracks `cloudAlpha`, `cloudBGAlpha`, current/target wind, and atmosphere state independently of the biome panorama. [Official `Main` reference](https://docs.tmodloader.net/docs/preview/class_main.html) `CustomSky.GetCloudAlpha` can suppress or retain vanilla clouds, and `OnTileColor` can alter the scene's tile color. [Official `CustomSky` reference](https://docs.tmodloader.net/docs/stable/class_custom_sky.html)

**Recommendation:** Weather should be an overlay state, not another complete set of biome style slots. The Engraft can multiply cloud color, add particulate layers, or alter a sky's opacity without moving landmarks or resetting parallax phase.

### Graveyard and events

Graveyard uses a filter/fog intensity over the current biome rather than a unique panorama. [Current `Main.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/Main.cs.patch#L6700-L6724) [Current `SceneState.cs.patch`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/SceneState.cs.patch) Sandstorm, Hell, Eclipse-at-surface, Space, and Shimmer occupy the `Environment` priority band; invasions and moon events occupy `Event`; boss bands are stronger still. [Current `SceneEffectPriority.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/SceneEffectPriority.cs)

**Recommendation:** Let event skies and boss visuals remain composable. Engraft surface/underground styles should usually be `BiomeHigh`; use `Environment` only for the Engraft's Underworld compositor or an authored sealed interior that must override ordinary biome visuals. A global hook that always forces Engraft/ruined art can accidentally erase stronger third-party event backgrounds because it runs after priority resolution.

## 7. Multiplayer and saved variant state

Background texture loaders and both background-style loaders are client-side autoloaded, and their selectors read `Main.LocalPlayer.CurrentSceneEffect`. [Current `BackgroundLoaders.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L15-L18) [Current `BackgroundLoaders.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L103-L187) Each player's mod-biome flags are evaluated and can be serialized over the network, but scene-effect selection and rendering remain local to that player's camera. [Current `BiomeLoader.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BiomeLoader.cs) [Current `SceneEffectLoader.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/SceneEffectLoader.cs)

Therefore:

- Fade alpha, boundary dwell timers, last visual state, custom-sky intensity, and parallax phase are client-only transient state. They should not be saved or networked.
- The scar's actual bounds, world progression, and selected world-art variant are server/world-authoritative state and should be saved and sent to joining clients.
- A player's camera can be in a different biome from another player's camera; never use one static fade value as shared multiplayer state.
- Dedicated servers should not request textures or register client graphics. The background loaders themselves are marked `ModSide.Client`. [Current `BackgroundLoaders.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L15-L18)

**Recommendation for seed-stable variants:** Derive defaults once from a stable world seed/hash, save them under stable biome-name keys, and network the resolved bytes/IDs. Do not index saved data solely by enum ordinal: inserting or reordering enum values can silently move an old world's variants. The client may cache resolved texture slots, but the authoritative variant identity belongs to the world.

## 8. What Remnants and Terraria Overhaul actually do

### Remnants

Revision reviewed: [`9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389`](https://github.com/lazy-wombat/Remnants/tree/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389).

Remnants promotes several wall-driven vanilla micro-biomes into true mod scene effects. Its `MarbleCave` and `GraniteCave` are `BiomeHigh` `ModBiome` classes with dedicated `ModUndergroundBackgroundStyle` instances; `Beehive` does the same for a hive background. [Remnants `Biomes.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/Biomes/Biomes.cs) The Marble and Granite styles assign their texture to all four documented slots. [Remnants `marblecave.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/Biomes/Backgrounds/marblecave.cs) [Remnants `granitecave.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/Biomes/Backgrounds/granitecave.cs)

This is useful precedent for Apogean: a micro-biome can deliberately become a full panoramic biome by defining a `ModBiome`, choosing an appropriate priority, and supplying the four-file cave contract.

Remnants also contains a `GrowthUG` style that writes internal indexes 4 and 5, beyond the documented contract, but no active biome references that class in the reviewed source. [Remnants `GrowthUG.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/Biomes/Backgrounds/GrowthUG.cs) It is not evidence that those indexes are a supported Underworld replacement technique.

Remnants registers and activates `SulfuricVentsSky`, but the sky's `Draw` implementation is commented out; the active visual is its registered screen filter, not a custom parallax Underworld background. [Remnants `Remnants.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Remnants.cs) [Remnants `Skies.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/Biomes/Skies.cs) Its Underworld prototype biomes provide music/state but no custom Hell panorama. [Remnants `Biomes.cs`](https://github.com/lazy-wombat/Remnants/blob/9c2cbf9cd2edcd8ae18a297357c4bcdc2870a389/Content/Biomes/Biomes.cs)

### Terraria Overhaul

Revision reviewed: [`e202f4719bdff845035e352ba70169b7022cbe09`](https://github.com/Mirsario/TerrariaOverhaul/tree/e202f4719bdff845035e352ba70169b7022cbe09), manifest version 5.0.0.38. [Terraria Overhaul `build.txt`](https://github.com/Mirsario/TerrariaOverhaul/blob/e202f4719bdff845035e352ba70169b7022cbe09/build.txt)

**Source-audit result:** This revision contains no implementation of `ModSurfaceBackgroundStyle`, `ModUndergroundBackgroundStyle`, `GlobalBackgroundStyle`, background selection hooks, `CustomSky`, `ManageSpecialBiomeVisuals`, `TextureAssets.Underworld`, or `Main.underworldBG`. [Terraria Overhaul source tree](https://github.com/Mirsario/TerrariaOverhaul/tree/e202f4719bdff845035e352ba70169b7022cbe09)

Terraria Overhaul is therefore not a current primary-source precedent for background routing, smooth biome-background blending, or Hell replacement. Its absence from this subsystem is itself important: compatibility testing with Overhaul should focus on its lighting, ambience, camera, weather, and rendering changes, not on competing background-style classes.

## 9. Recommended Engraft vertical-scar architecture

### 9.1 One authoritative region, four visual states

Model the Engraft as one world-authoritative horizontal scar with a client-resolved vertical visual state:

```text
EngraftScarRegion (saved + networked)
└── local player/camera position
    ├── Surface/sky       -> EngraftSurfaceStyle
    ├── Dirt/underground  -> EngraftUndergroundStyle, slots 0/1 emphasized
    ├── Cavern            -> EngraftUndergroundStyle, slots 2/3 emphasized
    └── Underworld        -> EngraftHellSky compositor
```

The surface and underground entries should come from an Engraft `ModBiome` at `BiomeHigh`, while the Underworld compositor should activate only when both the scar region and `ZoneUnderworldHeight` are true. `ModBiome` is the official container for background, water, music, map, and other scene effects. [Current `ModBiome.cs`](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/patches/tModLoader/Terraria/ModLoader/ModBiome.cs) ExampleMod demonstrates separating surface and underground activation by player height. [Current ExampleMod surface biome](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/ExampleMod/Content/Biomes/ExampleSurfaceBiome.cs) [Current ExampleMod underground biome](https://github.com/tModLoader/tModLoader/blob/2534f5682a46661c9aec633bea0852020e4fa796/ExampleMod/Content/Biomes/ExampleUndergroundBiome.cs)

### 9.2 Client-side hysteresis resolver

Maintain a local `VisualTarget` rather than calculating a different style directly on every draw call:

1. Compute signed horizontal distance to the saved scar boundary using the local player's camera/center.
2. Enter Engraft only after crossing an inner threshold; leave only after crossing a wider outer threshold.
3. Require the candidate target to remain stable for a short dwell interval before changing the selected style slot.
4. Once selected, return one stable slot and let Terraria's alpha arrays perform the cross-fade.
5. Reset only transient target/fade state on world unload, player change, or teleport.

**Recommendation:** Start with an 8–16 tile spatial dead band and roughly 0.15–0.30 seconds of dwell, then tune by playtest speed. These are project values, not engine constants. The purpose is to prevent a player or camera standing on one tile boundary from restarting transitions repeatedly.

Do not place this state mutation inside `ChooseFarTexture`/`ChooseMiddleTexture`/`ChooseCloseTexture`; those methods are draw-time texture queries and may be called multiple times. Resolve the target in a client update hook, then have every texture hook read the same immutable-for-that-frame state.

### 9.3 Preserve vanilla and third-party biome identity

Outside the Engraft scar:

- Leave Glowing Mushroom, Jungle, Snow, Ocean, evil/Hallow, and any selected mod underground style intact unless the full ruined-world configuration explicitly promises to replace them.
- Let Granite, Marble, Spider, Underground Desert, Dungeon, and Temple continue using their walls by default; add a full panorama only when Apogean intentionally promotes one into a panoramic biome.
- In a global hook, detect that the incoming style is already modded and leave it unchanged unless Engraft is the winning Apogean scene. tModLoader exposes lookup for a mod underground style by ID through `ModContent`. [Official `ModContent` reference](https://docs.tmodloader.net/docs/stable/class_mod_content.html)
- Keep Graveyard as a composited fog/filter state over whichever ruined biome is underneath.

### 9.4 Surface asset/state families

Use separate style slots for independently transitioning landmark sets, not for every weather tint:

```text
EngraftSurface_VariantA  -> FarA, MidA, CloseA
EngraftSurface_VariantB  -> FarB, MidB, CloseB
Day/night/weather        -> color, emissive, clouds, and CustomSky overlays
```

All files within a family should use identical canvas width, repeat seam, vertical baseline, scale, and per-layer parallax. This preserves phase while the engine draws both slots during a transition. `ChooseCloseTexture` permits scale/parallax changes, but unnecessary differences make cross-fades visibly slide. [Official `ModSurfaceBackgroundStyle` reference](https://docs.tmodloader.net/docs/preview/class_mod_surface_background_style.html)

### 9.5 Underground and Underworld state families

Each Engraft underground variant is one atomic four-texture contract. The scar can use different artwork in the dirt and cavern zones because indexes 1 and 3 are separate, while indexes 0 and 2 provide the two vertical seams. [Official `ModUndergroundBackgroundStyle` reference](https://docs.tmodloader.net/docs/preview/class_mod_underground_background_style.html)

At Underworld depth, fade the Engraft custom sky's intensity using its `Update` method. When intensity reaches zero, `IsActive` may remain true until the fade completes, matching the pattern shown by ExampleMod's historical custom sky. [Official `CustomSky` reference](https://docs.tmodloader.net/docs/stable/class_custom_sky.html) The sky should not alter `Main.underworldBG` or global Underworld assets.

## 10. Implementation and regression checklist

Before replacing the current background implementation, verify:

1. Surface transitions between every pair of Apogean style slots show far, middle, and close together for at least one overlap frame.
2. `ModifyFarFades` is tested against the exact supported tModLoader version; fail the test if the loader begins passing a different alpha array.
3. Variant cycling uses a new slot or an explicit compositor and never changes a texture behind an unchanged fully opaque slot.
4. All surface variants share dimensions, seam, baseline, scale, and parallax.
5. Every underground set has valid 160×16, 160×96, 160×16, and 160×96 files with matching 32-pixel wrap seams.
6. Mushroom, Jungle, Snow, Ocean, evil/Hallow, Graveyard, Granite, Marble, Spider, Underground Desert, Dungeon, and Temple are tested at both sides of the Engraft boundary.
7. A third-party `BiomeHigh` background remains visible outside Engraft; test Remnants Marble and Granite specifically.
8. Underworld Engraft fades in/out without changing vanilla Hell globally and without covering foreground tiles, walls, liquids, projectiles, or UI.
9. Blood Moon, Eclipse, rain, sandstorm, Graveyard, invasion, boss sky, and monolith effects remain composable.
10. Two multiplayer clients standing in different visual states see independent fades but agree on the saved scar bounds and world variant.
11. Joining a server mid-transition begins from a sensible local fade without requiring transient fade synchronization.
12. Test both the installed 1.4.4.9 target and any future 1.4.5 migration because the current development source has already changed vanilla background-ID counts.

## Final recommendation

Use `ModBiome` plus stable style slots for Engraft surface and cave backgrounds, a client-side hysteresis resolver for boundary stability, the engine's own alpha/parallax draw paths for normal cross-fades, and a `CustomSky` compositor only for the Engraft Underworld and true dynamic overlays. Make the current `ModifyFarFades` override a version-gated no-op rather than copying ExampleMod's loop, because current tModLoader passes it the front alpha array. Preserve world-selected variants as compact server-authoritative IDs, while keeping fades and custom-sky intensity entirely client-local.
