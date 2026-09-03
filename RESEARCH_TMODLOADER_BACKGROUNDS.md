# Apogee background-rendering contract for tModLoader 1.4.4

## Scope and evidence baseline

This report defines the renderer contract for Apogee's next background gate. It is intentionally limited to first-party evidence: the installed tModLoader/Terraria build, the official tModLoader source at that build's exact commit, the official generated API documentation, and official ExampleMod.

The installed executable reports:

- Terraria file version: `1.4.4.9`
- tModLoader product version: `2026.07.3.0` (`2026.07`, stable)
- tModLoader source commit: `666f69962d3bdffde54fc14025f02634965b4e7c`
- Installed assembly: `E:\SteamLibrary\steamapps\common\tModLoader\tModLoader.dll`
- Assembly SHA-256: `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C`
- Installed XML API documentation: `E:\SteamLibrary\steamapps\common\tModLoader\tModLoader.xml`
- XML SHA-256: `15F7D045C68CC768A791F056F9179B7E3A03AE1E1DEABC96B42CFD39726DE7B6`

The commit is also recorded by the installed `RecentGitHubCommits.txt`. All GitHub citations below are pinned to that commit rather than a moving branch.

Terms used below:

- **Confirmed** means directly documented in the API, visible in the pinned official source, or verified in the installed compiled assembly.
- **Inference / gate rule** means an implementation decision derived from those facts. It is not presented as an upstream guarantee.

## Executive conclusions

1. A `ModSurfaceBackgroundStyle` is one style slot that can supply three background-texture slots: far, middle, and close. It does not provide a general four-layer compositor.
2. On this exact tModLoader build, far and middle use `Main.bgAlphaFarBackLayer`, close uses `Main.bgAlphaFrontLayer`, but `ModifyFarFades` is called with the **front** array. Copying ExampleMod's fade loop therefore advances the close fade twice while far/middle advance once.
3. Surface far and middle textures have no per-style parallax or scale parameters. They inherit mutable engine geometry at their insertion points. The safest current topology is the official ExampleMod topology: far `1024x408`, middle `1024x600`, close `952x480`.
4. A `ModUndergroundBackgroundStyle` must fill exactly four cave roles. The dimensions are a hard renderer topology: `160x16`, `160x96`, `160x16`, `160x96`; the repeating textures have a 32-pixel horizontal seam duplicate.
5. The four underground textures already represent both dirt and cavern depth. A separate style switch at `rockLayer` is normally unnecessary and would add a second transition on top of the engine's built-in ground/rock seam.
6. `ModUndergroundBackgroundStyle` is not the five-layer Underworld panorama API. Treating an "Underworld" four-texture set as a complete Hell replacement is incorrect.
7. `CaptureBiome.GetCaptureBiome(-1)` is unsafe on this build when a modded surface style is active without a mod water style: it can construct a capture biome with water style `-1`, which is later used as a liquid-array index.
8. Rendering is client-local. Save and synchronize semantic choices such as `Forest variant 1`; never save runtime background style or texture slot integers.
9. Apogee's next gate should fix routing and capture safety before producing more final art. The current assets already use the official ExampleMod dimensions; dimensions alone do not solve selection, alpha, camera, or seam errors.

## 1. Content registration and the two kinds of slot

### Confirmed facts

`ModBackgroundStyle.Slot` is a runtime ID for a complete surface or underground style. Surface and underground styles use separate loaders and therefore separate style-ID spaces. [`ModBackgroundStyle` and registrations](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L5-L41)

The values returned by `ChooseFarTexture`, `ChooseMiddleTexture`, and `ChooseCloseTexture`, and the values written by `FillTextureArray`, are instead IDs in `TextureAssets.Background`. `BackgroundTextureLoader.GetBackgroundSlot` resolves those texture IDs. [`ModSurfaceBackgroundStyle` hooks](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L43-L83) [`BackgroundTextureLoader`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L13-L77)

Background textures are client-side content. Files under a directory named `Backgrounds` are autoloaded; other paths require explicit registration with `AddBackgroundTexture`. The loader records each texture's actual width and height in `Main.backgroundWidth` and `Main.backgroundHeight`. [`BackgroundTextureLoader` autoload and sizing](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L13-L18) [`background array population`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L64-L95)

`GetBackgroundSlot` throws when a key is absent; `TryGetBackgroundSlot` is the non-throwing alternative. [`background lookup methods`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L30-L40)

Style IDs and texture IDs are allocated sequentially while content loads and are reset when loaders unload. They are runtime registration values, not durable world identifiers. [`Loader` allocation and unload`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/Loader.cs#L12-L31) [`typed registration`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/Loader.cs#L39-L69)

### Inferences / gate rules

- Never serialize `ModSurfaceBackgroundStyle.Slot`, `ModUndergroundBackgroundStyle.Slot`, or a `BackgroundTextureLoader` slot.
- Keep semantic world data small and stable: biome enum/name plus a validated variant byte. Resolve all runtime slots after mods load.
- Treat a failed texture lookup as a content-validation failure. A silent `-1` is acceptable only for an intentionally absent surface layer, not as a missing-file workaround.
- No separate resource pack is required. A mod-packaged PNG under Apogee's `Content/Backgrounds/...` tree is loaded into the same background texture array as other mod backgrounds.

## 2. Surface backgrounds

### 2.1 Public style contract

### Confirmed facts

A surface style exposes:

- `ModifyFarFades(float[] fades, float transitionSpeed)`;
- `ChooseFarTexture()`;
- `ChooseMiddleTexture()`;
- `PreDrawCloseBackground(SpriteBatch)`;
- `ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)`.

Returning `false` from `PreDrawCloseBackground` suppresses tModLoader's built-in close-layer draw. The hook receives the active `SpriteBatch`, so it is also the supported seam for a fully custom close draw. [`ModSurfaceBackgroundStyle` API source](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L31-L84) [generated stable API reference](https://docs.tmodloader.net/docs/stable/class_mod_surface_background_style.html)

The official ExampleMod implements one far texture, an animated choice among four middle textures, one close texture, and the documented fade loop. [`ExampleSurfaceBackgroundStyle.cs`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs)

### 2.2 Actual draw order, alpha, parallax, and fill

### Confirmed facts

The pinned loader draws **every registered surface style whose relevant alpha is greater than zero**. This is how old and new styles coexist during a fade. Far and middle read `Main.bgAlphaFarBackLayer[style.Slot]`; close reads `Main.bgAlphaFrontLayer[style]`. [`far and middle implementation`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L195-L278) [`close implementation`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L281-L340)

The insertion order in Terraria's surface renderer is:

1. sky/custom-sky depths and the most distant cloud strata;
2. vanilla far mountains, then modded `DrawFarTexture`;
3. intermediate sky/cloud work and vanilla middle mountains, then modded `DrawMiddleTexture`;
4. large/near clouds;
5. vanilla or modded close layer;
6. fog/change overlays and remaining custom-sky depths.

The pinned patch shows the three modded insertion points. [`close insertion`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L6749-L6757) [`far and middle insertions`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L6787-L6815)

Far and middle each draw the full selected texture and repeat it horizontally. They do **not** vertically tile or stretch it to fill the screen. Both use the engine's current `bgStartX`, `bgTopY`, `bgWidthScaled`, `bgLoops`, and `bgScale`; neither hook receives independent geometry controls. [`far draw call`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L207-L238) [`middle draw call`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L247-L278)

The close path starts with `scale = 1.25`, `parallax = 0.37`, `a = 1800`, and `b = 1750`, lets `ChooseCloseTexture` modify them, then doubles the returned scale before drawing. It computes horizontal stride from the chosen texture's own width, repeats horizontally, and only draws while the camera is above `worldSurface + 1 tile`. It does not vertically tile the close texture. [`close geometry and draw`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L297-L339)

Installed-binary verification adds two non-public implementation details:

- the far insertion uses a `1024 * scale` horizontal stride and normally reaches the hook with parallax `0.15` and effective scale `2.0`;
- the middle insertion inherits geometry prepared for the current vanilla middle-mountain path; in the normal forest path this is normally parallax `0.20` and effective scale `2.30`.

Those values are implementation details, not parameters promised by `ModSurfaceBackgroundStyle`. A vanilla background configuration can affect the mutable state inherited by the middle hook.

The normal world renderer starts its background `SpriteBatch` with linear sampling. Therefore non-integer scaling is normal for backgrounds; one-pixel focal details can blur even though tile art uses hard pixels.

### Inferences / gate rules

- Surface art should be horizontally seamless at the engine's repeat stride.
- A far texture narrower or wider than 1024 pixels can gap or overlap because this build's far stride is based on 1024, not on the mod texture width. Use width 1024 for this gate.
- Use width 1024 for middle textures too. It matches official ExampleMod and the normal inherited middle stride; any other width needs an explicit live proof across every vanilla background configuration Apogee permits.
- The close texture may use another width because its stride is computed from its own width, but all variants in one visual family should use the same width, baseline, and hook parameters.
- Surface backgrounds do not need to be opaque rectangles. Transparent sky is expected, but the silhouette must extend low enough at every tested camera height that the sky does not appear as an accidental gap under the terrain-facing art.
- Do not put critical single-pixel lines in surface backgrounds. Test at non-integer UI/camera scales because the renderer uses linear sampling.

### 2.3 Surface transition behavior and the version-specific fade defect

### Confirmed facts

The installed renderer first advances its back and front alpha arrays, then calls:

```csharp
SurfaceBackgroundStylesLoader.ModifyFarFades(bgStyle, bgAlphaFrontLayer, backgroundLayerTransitionSpeed);
```

The call is visible in the exact pinned patch. [`Main.cs.patch` transition call](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L7135-L7155)

That is inconsistent with the method's name/documentation and with the actual far/middle draw array. [`documented fade contract`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L43-L46) [`loader dispatch`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L182-L193)

Installed-binary verification confirms:

- normal transition speed is `0.05` alpha per draw;
- the engine changes to a non-purity target after 30 qualifying draws and to purity after 60;
- each front and back alpha is clamped to `[0,1]`;
- instant transitions use speed `1.0`;
- ExampleMod's fade loop, if copied on this build, updates the front/close array a second time but never touches the back array supplied to far/middle.

`ChooseFarTexture`, `ChooseMiddleTexture`, and `ChooseCloseTexture` are called during drawing. Changing the returned texture while keeping the same style `Slot` does not change an alpha target and does not initiate a cross-fade. ExampleMod deliberately uses this property for frame animation. [`draw-time texture queries`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L207-L278)

### Inferences / gate rules

- For installed build `2026.07.3.0`, Apogee's `ModifyFarFades` should be a no-op. Let Terraria's already-updated front/back arrays perform the fade once.
- Add a version-pinned regression test. Re-audit this decision when tModLoader changes because upstream may eventually pass the back array as documented.
- A world-seeded variant can share one style class if the variant remains fixed while the world is loaded. A player-facing background changer that must cross-fade between variants needs separate style slots; changing texture IDs inside one slot will snap.
- Texture getters must be pure for a draw: no random selection, no variant mutation, and no `++frameCounter` in a getter. Resolve variant/frame state before drawing and have all three hooks read the same snapshot.

## 3. Binding styles to biomes and selection priority

### Confirmed facts

`ModBiome` is a `ModSceneEffect`. A biome supplies surface and underground styles by overriding `SurfaceBackgroundStyle` and `UndergroundBackgroundStyle`; `IsBiomeActive(Player)` drives the biome flag. `ModBiome` defaults to `BiomeLow` priority and, importantly, music ID `0` rather than `-1`. [`ModBiome` defaults and registration](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBiome.cs#L8-L26) [`biome activation forwarding`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBiome.cs#L81-L98)

Active scene effects are sorted by `Priority + clamp(GetWeight, 0, 1)`. The loader then independently takes the first non-null water style, underground style, surface style, music, map background, and other scene fields. A background, music, and water style can therefore come from different active scene effects. [`weight contract`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSceneEffect.cs#L54-L107) [`field-by-field scene selection`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/SceneEffectLoader.cs#L65-L148)

For surface rendering on this exact build, the effective source order is:

1. a mod style at `BiomeHigh` or above;
2. vanilla ocean, mushroom, desert, Hallow, Corruption, or Crimson;
3. a mod style at `BiomeMedium` or above;
4. vanilla Jungle or Snow;
5. a mod style at `BiomeLow` or above;
6. vanilla forest fallback;
7. every `GlobalBackgroundStyle.ChooseSurfaceBackgroundStyle` hook, last.

[`surface selector insertion points`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L7167-L7212)

For underground rendering, the effective source order is:

1. base horizontal cave style;
2. a mod style at exactly `BiomeLow`;
3. vanilla Snow/Jungle;
4. a mod style at exactly `BiomeMedium`;
5. vanilla ocean/deep evil/deep Hallow/mushroom;
6. a mod style at `BiomeHigh` or above;
7. every `GlobalBackgroundStyle.ChooseUndergroundBackgroundStyle` hook, last.

[`underground selector insertion points`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L6333-L6378)

The enum documentation describes the intended priority bands, but the exact ordering above comes from the renderer patch and should be the test oracle for this pinned build. [`SceneEffectPriority`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/SceneEffectPriority.cs)

`GlobalBackgroundStyle` has no priority or ownership arbitration. Its hooks mutate the final integer by reference after biome and vanilla selection. [`GlobalBackgroundStyle` API](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L86-L126) [`global-hook dispatch`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L343-L378)

### Inferences / gate rules

- Prefer `ModBiome` binding for the Maw and other spatially bounded biomes.
- Override `Music => -1` in a background-only `ModBiome` unless silence/music ID 0 is intentional.
- Reserve `GlobalBackgroundStyle` for Apogee's deliberate world-wide Wastes replacement. Before overriding, preserve another mod's already-selected style unless Apogee explicitly owns that compatibility decision. `ModContent.GetModSurfaceBackgroundStyle(style)` and `GetModUndergroundBackgroundStyle(style)` can distinguish registered mod styles from vanilla values.
- Explicitly exclude Dungeon, Temple, special events, and any other protected visual contexts required by the design. A global hook otherwise wins last regardless of scene priority.
- Avoid equal priority-plus-weight ties between Apogee scene effects. The source comparator contains no secondary semantic tie-breaker.

## 4. Underground and cavern backgrounds

### 4.1 The four-texture contract

### Confirmed facts

`FillTextureArray(int[] textureSlots)` owns these four entries:

| Index | Renderer role | Required dimensions | Repeat/seam contract |
|---:|---|---:|---|
| 0 | border between sky/surface and ground layer | `160x16` | horizontal transition strip |
| 1 | dirt/ground-layer field | `160x96` | rightmost 32 pixels duplicate the far-left 32 |
| 2 | border between ground and rock layers | `160x16` | horizontal transition strip |
| 3 | rock/cavern-layer field | `160x96` | rightmost 32 pixels duplicate the far-left 32 |

This is explicit in both source and generated API documentation. [`ModUndergroundBackgroundStyle.FillTextureArray`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L13-L28) [generated stable API reference](https://docs.tmodloader.net/docs/stable/class_mod_underground_background_style.html)

Official ExampleMod fills exactly those four indices. [`ExampleUndergroundBackgroundStyle.cs`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs)

The installed renderer passes a seven-entry working array. Vanilla prepares deeper background entries 4-6, then tModLoader invokes `FillTextureArray`; the public mod contract still owns only 0-3. [`fill call after vanilla setup`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L6379-L6396)

Installed-binary verification confirms that the repeating period is `texture.Width - 32`, which is 128 pixels for the required 160-pixel texture. The renderer samples the cave textures in 16-pixel cells and applies tile lighting. All four cave roles use `Main.caveParallax`, whose installed default is `0.88`.

### Inferences / gate rules

- Fill all of indices 0-3 on every call. Never write `-1` into an underground slot.
- Never clear or overwrite indices 4-6. They belong to Terraria's deeper background path.
- Enforce the four dimensions exactly in a static test. A wrong field height can produce out-of-range source rectangles; a wrong width changes the repeat period and seam math.
- Author the 32-pixel seam duplicate deliberately. Do not merely make the outermost one-pixel columns equal.
- Because the renderer is tile-lit in 16-pixel cells, important cave landmarks should survive uneven lighting and partial tile occlusion.

### 4.2 Depth behavior and underground transitions

### Confirmed facts

One selected underground style naturally spans both dirt and cavern depths: indices 0/1 handle the surface-to-ground boundary and dirt field, while indices 2/3 handle the ground-to-rock boundary and cavern field. There is no separate public "dirt style" and "cavern style" property.

Terraria remembers `undergroundBackground` and `oldUndergroundBackground`. When the style integer changes, it sets `ugBackTransition` to `1`; the installed binary subtracts `0.25` per draw until zero. The new style is drawn as the base and the old style is drawn over it with the decreasing transition alpha. The pinned patch shows the current/old selection and fills both texture arrays. [`underground current/old transition setup`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L6370-L6396)

`FillTextureArray` is called during rendering for both the current and old style. A texture change within the same style slot does not change `undergroundBackground`, so it snaps without starting `ugBackTransition`.

There is no underground equivalent of `ModifyFarFades`; the `0.25` transition is not exposed by `ModUndergroundBackgroundStyle`.

### Inferences / gate rules

- Use one four-texture set per biome variant. Let entries 0-3 express the dirt-to-cavern depth change.
- Do not route to a second style merely because `Player.ZoneRockLayerHeight` changes. That duplicates the engine's own depth seam and creates an avoidable four-draw cross-fade near `rockLayer`.
- `FillTextureArray` must be deterministic and cheap. It may be called multiple times per render. Do not allocate, mutate variant state, or advance animation there.
- A runtime background changer that requires smooth underground transitions needs separate style slots per variant. A fixed world-seeded variant may safely use one style slot.

### 4.3 Underworld limitation

### Confirmed facts

The public four-texture contract names only sky/ground, ground, ground/rock, and rock roles. It does not expose the dedicated Hell panorama. [`underground API contract`](https://docs.tmodloader.net/docs/stable/class_mod_underground_background_style.html)

Terraria separately stores five selected Underworld layers in `Main.underworldBG`, and `TextureAssets.Underworld` is a separate asset array. [official `Main` reference](https://docs.tmodloader.net/docs/stable/class_main.html) [official `TextureAssets` reference](https://docs.tmodloader.net/docs/stable/class_texture_assets.html)

The installed renderer calls a separate `DrawUnderworldBackground` method that draws those five Underworld textures. A `ModUndergroundBackgroundStyle` continues to affect cave textures 0-3 near that depth, but it does not replace the five-layer Hell panorama.

`ModSceneEffect.SpecialVisuals` officially points mods to `CustomSky`/screen shaders for special visual composition. [`SpecialVisuals` contract](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSceneEffect.cs#L104-L113)

### Inferences / gate rules

- Do not accept an Apogee `UnderworldRuinedUndergroundStyle` as proof that Hell has been replaced. It proves only the cave-array path.
- Keep the true Underworld panorama out of the next basic cave gate. Treat it as a separate renderer slice using a `CustomSky`, a carefully scoped draw hook, or another verified approach.

## 5. Surface texture dimensions and topology

### Confirmed facts

The surface API does not publish mandatory PNG dimensions. It accepts a registered background texture ID and records that texture's actual dimensions. [`surface API`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L31-L84) [`texture sizing`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L64-L77)

PNG-header inspection of official ExampleMod assets at the pinned commit gives:

| Role | Official ExampleMod asset | Dimensions |
|---|---|---:|
| far | [`ExampleBiomeSurfaceFar.png`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Assets/Textures/Backgrounds/ExampleBiomeSurfaceFar.png) | `1024x408` |
| middle | [`ExampleBiomeSurfaceMid0.png`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Assets/Textures/Backgrounds/ExampleBiomeSurfaceMid0.png) | `1024x600` |
| close | [`ExampleBiomeSurfaceClose.png`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Assets/Textures/Backgrounds/ExampleBiomeSurfaceClose.png) | `952x480` |

All four official middle animation frames in this revision use `1024x600`; all four underground assets use the dimensions documented above. The PNGs are 32-bit ARGB and support transparency.

The current Apogee asset inventory already matches these dimensions: 22 far images at `1024x408`, 22 middle images at `1024x600`, 22 close images at `952x480`, 44 underground border images at `160x16`, and 44 underground fields at `160x96`.

### Inferences / gate rules

- Retain the current dimensions for the next gate. Re-authoring to arbitrary sizes would add renderer uncertainty without fixing selection or art quality.
- A surface family's far/middle/close variants must share their respective widths, transparent top treatment, horizon baseline, and ground contact baseline.
- Every far and middle image must tile at x=0/1024. Every close image must tile at x=0/952 unless the close hook is replaced with a custom draw.
- A visually filled background is an art/layout requirement, not an automatic engine fill. The renderer only horizontally repeats a full texture at its computed y position.

## 6. Capture-camera contract and hazards

### 6.1 Confirmed capture behavior

`CaptureSettings.Biome` defaults to `CaptureBiome.DefaultPurity`; callers may replace it. `CaptureCamera` renders a capture in chunks with 128-tile frame buffers and 126-tile interiors, and optional scaling caps the output to 4096 pixels per dimension. These values were verified directly in the installed assembly's `Terraria.Graphics.Capture.CaptureSettings` and `CaptureCamera` types.

One capture is allowed at a time; the installed `CaptureCamera.Capture` throws `InvalidOperationException` if another capture is active. `CaptureManager`/camera initialization is client-specific. The official patch explicitly avoids normal graphics camera initialization on a dedicated server. [`CaptureManager.cs.patch`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/TerrariaNetCore/Terraria/Graphics/Capture/CaptureManager.cs.patch)

During capture, Terraria temporarily replaces the surface alpha arrays, forces the requested `CaptureBiome.BackgroundIndex` to alpha 1, changes `Main.screenPosition` and dimensions for each chunk, and calls the normal surface and Underworld draw methods. The capture's biome choice remains fixed across those chunks; it does not recompute `ModBiome.IsBiomeActive` for each photographed location. This was verified in the installed `Terraria.Main.DrawCapture` implementation.

### 6.2 Confirmed `-1` water-style defect

Scene-effect fields default to `-1`. A biome can supply a surface background but no water style. [`SceneEffectInstance` defaults](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/SceneEffectLoader.cs#L20-L48)

When a modded surface background is active, the tModLoader patch to `CaptureBiome` constructs a new capture biome from:

```csharp
new CaptureBiome(
    sceneEffect.surfaceBackground.value,
    sceneEffect.waterStyle.value,
    sceneEffect.tileColorStyle);
```

It does not sanitize the water value. [`CaptureBiome mod-scene fallback`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Graphics/Capture/CaptureBiome.cs.patch#L22-L34)

The water-style loader's valid arrays are resized from vanilla `Main.maxLiquidTypes` through all registered mod water styles. `-1` is never a valid array index. [`WaterStylesLoader` arrays and selection](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/WaterStyleLoader.cs#L20-L46)

The installed `Main.DrawCapture` uses `CaptureBiome.WaterStyle` to set `Main.liquidAlpha` and later passes it into liquid drawing. Therefore this sequence is unsafe on `2026.07.3.0`:

1. active mod surface style;
2. no active `ModWaterStyle`, so scene water is `-1`;
3. `CaptureBiome.GetCaptureBiome(-1)` returns the mod-scene capture biome;
4. capture drawing uses `-1` as a water-style index.

This directly matches an `IndexOutOfRangeException` in liquid rendering; it is not evidence that the PNG or resource pack is malformed.

### 6.3 Capture-safe gate rule

Do not use `CaptureBiome.GetCaptureBiome(-1)` unvalidated. Build one client-only helper that clamps both scene values and falls back to current vanilla/purity values:

```csharp
private static CaptureBiome GetSafeCaptureBiome()
{
    var scene = Main.LocalPlayer.CurrentSceneEffect;

    int background = scene.surfaceBackground.value;
    if (background < 0 || background >= Main.bgAlphaFrontLayer.Length)
        background = Main.bgStyle >= 0 && Main.bgStyle < Main.bgAlphaFrontLayer.Length
            ? Main.bgStyle
            : 0;

    int water = scene.waterStyle.value;
    if (water < 0 || water >= Main.liquidAlpha.Length)
        water = Main.waterStyle >= 0 && Main.waterStyle < Main.liquidAlpha.Length
            ? Main.waterStyle
            : 0;

    return new CaptureBiome(background, water, scene.tileColorStyle);
}
```

Additional capture rules:

- Never call capture code when `Main.dedServ` is true or `Main.netMode == NetmodeID.Server`.
- Require `CaptureManager.Instance != null` and `!CaptureManager.Instance.IsCapturing` before starting.
- Freeze the selected biome variant and animation frame for the whole capture. A draw-time increment in a texture getter can advance once per capture chunk and create seams in the final image.
- A capture of an area away from the player must pass the desired fixed background/water/tile-color context explicitly. The automatic selection reflects the local player, not the photographed tiles.
- Log the scene background, scene water, resolved capture background, resolved capture water, and all array lengths before each automated probe.

## 7. Multiplayer and save stability

### Confirmed facts

Background texture, surface-style, underground-style, and water-style loaders are client-side autoloaded. Their selectors read `Main.LocalPlayer.CurrentSceneEffect`. [`client-only background loaders`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L18-L19) [`local-player style selection`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L111-L126) [`surface local-player selection`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L165-L180)

`BiomeLoader` evaluates each player's `IsBiomeActive` flag and has explicit bit-array send/receive support. [`BiomeLoader`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BiomeLoader.cs#L15-L54)

`ModSystem.SaveWorldData`/`LoadWorldData` are the supported world-persistence hooks, and `NetSend`/`NetReceive` synchronize world state from server to client. tModLoader requires each pair to be overridden together. [`ModSystem validation`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L21-L29) [`world save/load hooks`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L321-L336) [`world network hooks`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L363-L377)

### Inferences / gate rules

- Biome rendering is per client and per local player. Two players can correctly see different backgrounds at the same time.
- World-selected art variants are server-authoritative semantic data. Save a validated variant ID and send it through `NetSend`; clients should not independently randomize it.
- On load and network receive, clamp or modulo every saved variant against the current variant count. Treat missing/short arrays defensively so old worlds survive new biome families.
- If a background changer mutates a variant during multiplayer, make the mutation on the server and immediately synchronize it. `NetSend` does not itself cause a world-data packet to be sent.
- Keep transient alpha, animation frame, hysteresis timers, and capture state client-local and unsaved.
- Current Apogee's `RuinedBackgroundSelectionSystem` already saves semantic variant bytes and implements `NetSend`/`NetReceive`, which is the correct storage shape. Its debug `Cycle` method still needs an explicit server-authoritative synchronization path before becoming player-facing.

## 8. Apogee code audit against this contract

This is an audit only; no production file is changed by this report.

### Confirmed in the current repository

- [`ApogeanSurfaceBackgroundStyles.cs`](Content/Backgrounds/ApogeanSurfaceBackgroundStyles.cs) uses the official three surface hooks and assets with ExampleMod dimensions.
- [`ApogeanUndergroundBackgroundStyles.cs`](Content/Backgrounds/ApogeanUndergroundBackgroundStyles.cs) fills only indices 0-3 and uses the documented four dimensions.
- [`RuinedBackgroundSelectionSystem.cs`](Content/Backgrounds/RuinedBackgroundSelectionSystem.cs) stores semantic bytes, saves them, and implements world-data networking.
- [`EngraftBiome.cs`](Content/Biomes/EngraftBiome.cs) intentionally avoids a surface style because of the observed capture-water defect.
- [`TileLabPlayer.cs`](Content/Diagnostics/TileLabPlayer.cs) currently calls `CaptureBiome.GetCaptureBiome(-1)` directly.

### Gate status and required corrections

The current uncommitted background WIP already makes `ModifyFarFades` a no-op, declines to replace another mod's surface/underground style on the pinned vanilla-count boundary, and skips the Underworld in the global underground hook. Those are directionally correct but still require the regression and compatibility assertions below. They were not edits made by this research task.

The remaining gate work is:

1. Replace the direct capture-biome call with a sanitized client-only helper and refuse concurrent captures.
2. Resolve one background selection snapshot per update/frame; all far/middle/close hooks must read that same snapshot.
3. Replace global whole-world tile-count activation for the Maw with a local spatial predicate; otherwise one sufficiently large Maw can activate for every player everywhere.
4. Prefer public mod-style lookup over an unexplained hard-coded vanilla count, or explicitly version-gate and test the constant `14`.
5. Continue treating `UnderworldRuinedUndergroundStyle` as cave art only. Create a separate future gate for the true Underworld panorama.
6. Keep the existing PNG dimensions, then improve art within those proven templates.

## 9. Required automated assertions

### Static/content assertions

1. Every configured surface texture exists under a `Backgrounds` path and resolves to a registered slot.
2. Every far texture is exactly `1024x408`; every middle texture is `1024x600`; every close texture is `952x480` for this gate.
3. Every underground set is exactly `[160x16, 160x96, 160x16, 160x96]`.
4. For underground indices 1 and 3, pixels `x=128..159` equal pixels `x=0..31` for every y row.
5. Calling each `Choose*Texture` twice in one update returns the same slot.
6. Calling `FillTextureArray` twice with the same state returns identical indices 0-3 and leaves sentinel values in indices 4-6 unchanged.
7. Every returned texture slot is `>= 0` and `< TextureAssets.Background.Length`.
8. Every selected style slot is within both surface alpha arrays or within the underground loader's registered range.
9. On the pinned build, `ModifyFarFades` leaves its supplied array unchanged.
10. Saved/networked variant IDs are always in `[0, VariantCount)` after load and receive.

### Live renderer assertions

1. At a stable surface target, the selected style's far and front alphas both reach 1 and all other Apogee style alphas reach 0.
2. During a normal transition, far and front alpha for the same style differ by no more than one engine step (`0.05`, plus a small floating-point tolerance).
3. Crossing each tested biome border in both directions changes the target once rather than oscillating every frame.
4. Far, middle, and close resolve the same biome/variant snapshot for a frame.
5. Underground current/old style changes start only when the semantic style changes; simply crossing `rockLayer` within one biome does not switch style.
6. A capture probe logs valid background and water indices before capture and completes without exception.
7. Capturing a region larger than one 126-tile interior chunk produces no horizontal variant/frame seams.
8. Dedicated-server execution never requests a texture, touches `CaptureManager` camera state, or calls capture.

### Visual matrix

Capture each row at 100% and one non-integer zoom, at day and night:

| Context | Required views |
|---|---|
| Wastes forest surface | stationary, horizontal pan, jump/fly vertical pan |
| Wastes to Desert/Jungle/Snow | walk left-to-right and right-to-left through each boundary |
| Wastes to Maw | normal walk, fast mount, teleport into/out of the biome |
| Evil/Hallow overlap | Apogee biome alone and overlapped with each vanilla evil/Hallow |
| dirt layer | immediately below surface and midway to `rockLayer` |
| ground/rock seam | above, centered on, and below the seam |
| cavern | open cavern, narrow tunnel, lit and unlit |
| mushroom/ocean/dungeon | verify routing and protected-context fallback |
| capture camera | player inside area, player outside area, multi-chunk area, water present |
| multiplayer | host/client together, then in different biomes; reconnect and compare variant IDs |

Pass criteria: no crash, no invalid index, no unintentional blank strip, no horizontal repeat seam, no style oscillation, no far/close fade desynchronization, no capture-chunk animation seam, and identical world-selected variants after save/reload and multiplayer reconnect.

## 10. Recommended implementation order

1. Add source/static contract checks for dimensions, underground seam duplication, slot validity, and purity of the fill/choose hooks.
2. Add the safe capture-biome helper and dedicated-server/concurrent-capture guards.
3. Lock the pinned-version surface fade no-op behind a regression assertion.
4. Introduce one immutable per-frame `BackgroundRenderSelection` containing biome, depth family, variant, and optional animation frame.
5. Route one Wastes forest surface set through the selection object and prove the full transition/capture matrix.
6. Route one Wastes underground set and prove all four depth textures plus the built-in ground/rock transition.
7. Only after both reference slices pass, replicate the architecture across Desert, Jungle, Snow, evils, Hallow, Mushroom, Ocean, Maw, and faction spaces.
8. Handle the true Underworld panorama as a separate renderer gate.

This order isolates engine-contract failures from art-quality failures. It also gives every later background family a known-good fixture rather than multiplying an uncertain renderer path.
