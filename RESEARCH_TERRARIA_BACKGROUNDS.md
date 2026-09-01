# Terraria/tModLoader Background Research

This note records the rendering contract used for Apogean background work. It is based on current first-party tModLoader documentation, the official ExampleMod implementation, and the official ExampleMod assets measured locally.

## Surface background contract

- A surface style derives from `ModSurfaceBackgroundStyle` and supplies three independent texture slots: far, middle, and close. The close hook also exposes scale and parallax controls. Returning `false` from `PreDrawCloseBackground` disables tModLoader's close-layer drawing. Source: [tModLoader `ModSurfaceBackgroundStyle` documentation](https://docs.tmodloader.net/docs/stable/class_mod_surface_background_style.html).
- The official ExampleMod returns a far texture, animated middle texture, and close texture. Its `ModifyFarFades` implementation raises its own `Slot` toward 1 and lowers all others toward 0 using the provided transition speed. Source: [official ExampleMod surface background style](https://raw.githubusercontent.com/tModLoader/tModLoader/stable/ExampleMod/Content/Biomes/ExampleSurfaceBackgroundStyle.cs).
- Official ExampleMod surface assets measure:

  | Layer | Dimensions |
  | --- | --- |
  | Far | 1024 × 408 |
  | Middle | 1024 × 600 |
  | Close | 952 × 480 |

- All three ExampleMod layers contain transparent sky above an opaque lower silhouette. Terraria supplies and time-tints the sky behind them. Consequently, baking an opaque sky into the far texture prevents normal sky continuity, while disabling middle/close destroys the intended parallax stack.
- A biome connects its style with `ModBiome.SurfaceBackgroundStyle`. Surface biomes normally include `ZoneSkyHeight` or `ZoneOverworldHeight` in their activation condition. Source: [official ExampleMod surface biome](https://raw.githubusercontent.com/tModLoader/tModLoader/stable/ExampleMod/Content/Biomes/ExampleSurfaceBiome.cs).

## Asset rules adopted by Apogean

1. Every surface variant is one matched `Far`/`Mid`/`Close` set.
2. The top of each layer remains transparent so Terraria's sky reaches the top of the screen and supplies night/eclipse behavior.
3. The bottom row is fully opaque, preventing a visible gap below a repeated layer.
4. Left and right edge pixels match row-by-row, preventing hard seams when a layer repeats.
5. World-seeded variation swaps all three layers together; time of day does not swap composition.

## Underground background contract

- `ModUndergroundBackgroundStyle.FillTextureArray` supplies four slots. Index 0 is the sky/ground transition, index 1 is the ground layer, index 2 is the ground/rock transition, and index 3 is the rock layer. Source: [tModLoader `ModUndergroundBackgroundStyle` documentation](https://docs.tmodloader.net/docs/stable/class_mod_underground_background_style.html).
- Transition textures are 160 × 16. Repeating ground/rock textures are 160 × 96, with the rightmost 32 pixels duplicating the leftmost 32 for wrapping. The official implementation simply fills those four slots. Source: [official ExampleMod underground background style](https://raw.githubusercontent.com/tModLoader/tModLoader/stable/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs).

Underground replacement is therefore a separate asset/runtime pass; stretching surface panoramas downward is not valid.

## Native item-art scale

- Official ExampleMod's [`ExampleSword.png`](https://raw.githubusercontent.com/tModLoader/tModLoader/stable/ExampleMod/Content/Items/Weapons/ExampleSword.png) is 40 × 40 pixels and only uses detail that remains legible at that native size.
- Official ExampleMod's [`ExampleGun.png`](https://raw.githubusercontent.com/tModLoader/tModLoader/stable/ExampleMod/Content/Items/Weapons/ExampleGun.png) is 62 × 32 because its readable silhouette is horizontally long, not because its detail was authored at a high resolution and downscaled.

Apogean weapon art should therefore be drawn directly on its final 36–40 pixel canvas with hard alpha and a small indexed palette. Large generated concept art is useful as shape reference only; downscaling it produces dense visual noise that does not match Terraria's item language.
