# Surface background altitude exit — research handoff

Researched 2026-09-05. Scope: Terraria/tModLoader 1.4.4, Calamity's own Astral implementations, and the Wastes Far city's eventual disappearance during ascent. Research only: one new note; no game/UI/build, installations, captures, commits, or changes to existing files. Parent task retains nativecity QA and other documentation work.

## Answer

There is no universal tile height at which every surface land texture leaves view. Distinguish **player Space classification**, **camera-based atmosphere darkening/transparency**, **a layer moving below the viewport**, **an explicit opacity envelope**, and **the downward underground draw guard**. The familiar `cameraY < worldSurface * 16 + 16` guard is a lower-world/depth restriction, never an upper-altitude cutoff. Calamity's custom land layers use that guard alongside positional formulas; its AstralSky has no direct altitude fade.

For Wastes, Far currently lacks an altitude opacity envelope and reconstructs an opaque tint even when the vanilla sky color is transparent. Therefore vanilla sky transparency does not guarantee the city disappears. A project-owned fade ending at our chosen Space reference is appropriate; its tuning must not be described as a borrowed Terraria or Calamity standard.

## Evidence and version pins

- Installed first-party runtime: `E:/SteamLibrary/steamapps/common/tModLoader/tModLoader.dll`, FileVersion `1.4.4.9`, ProductVersion `1.4.4.9+2026.07.3.0|2026.07|stable|Stable|666f69962d3bdffde54fc14025f02634965b4e7c|5250924198908260370`. SHA-256: `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C`.
- Official tML source pinned to that embedded [commit](https://github.com/tModLoader/tModLoader/commit/666f69962d3bdffde54fc14025f02634965b4e7c). Relevant source: [BackgroundLoaders.cs](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs), [ModBackgroundStyle.cs](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs), [Main patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch), [Player patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Player.cs.patch).
- CalamityTeam/CalamityModPublic `1.4.4` resolved through GitHub's API to [1a8cebd27ec5615316b78f71973446b5528d2b78](https://github.com/CalamityTeam/CalamityModPublic/commit/1a8cebd27ec5615316b78f71973446b5528d2b78), committed 2026-08-08 17:55:03 UTC. All Calamity links below use that pin.
- tML's moving `1.4.4` branch resolved to `c3b2e6f7d30e4295a771b97f45e8dc439b0a2f17` (2026-09-04 01:22:39 UTC); it was not substituted for the installed stable version.

Where public patches omit unchanged Terraria implementation, evidence below comes from read-only IL inspection of the legally installed DLL using its already installed `Libraries/mono.cecil/0.11.6/lib/netstandard2.0/Mono.Cecil.dll`. Terraria was not loaded or executed. Method names and IL offsets identify the exact local evidence; these are not claims that full vanilla method bodies appear in GitHub's patches. No forks or redistributed decompiled game sources were used.

## Coordinates and Space classification

Use `S = Main.worldSurface` in **tiles**, `C = Main.screenPosition.Y` in **world pixels**, and `H = Main.screenHeight` in the relevant draw pass. Positive Y points downward; ascending decreases Y. One tile is 16 world pixels. `worldSurface` is the world's surface/underground datum, not the terrain height beneath the player.

Installed `Player.UpdateBiomes()` first computes `point = Center.ToTileCoordinates()` (IL `0000–000b`), then sets:

```text
ZoneSkyHeight = point.Y <= S * (double)0.35f
ZoneOverworldHeight = point.Y <= S && point.Y > S * (double)0.35f
```

Evidence: IL `0410–045a`, particularly constant `0.3499999940395355` at `044b`. The predicate uses the integer **player-center tile**, not camera top, player feet, or 35% of `maxTilesY`. For positive coordinates, the last qualifying tile row is `floor(S * (double)0.35f)`. A continuous `0.35*S` reference is a convenient approximation, not a bit-exact replacement for the tile predicate. Example: if `S=400`, the stored float constant makes row 139 qualify and row 140 fail.

The [stable Player documentation](https://docs.tmodloader.net/docs/stable/class_player.html) describes the top 35% and explicitly warns that vanilla has multiple definitions of Space. Its loose wording about the world must not be interpreted as `0.35*maxTilesY`; the installed implementation supplies the denominator and coordinate anchor.

## Atmosphere: exact camera-height thresholds

Installed `Main.UpdateAtmosphereTransparencyToSkyColor()` (entire method IL `0000–00f9`) computes, expressed mathematically while preserving the source constants:

```text
w = Main.maxTilesX / 4200f
k = w * w
y = (C + integer_divide(H, 2)) / 16f       // camera-center depth, tiles
b = 65f + 10f * k                         // tiles below the world top
atmo = clamp(float((y - b) / (S / 5.0)), 0, 1)
```

It forces `atmo=1` in the game menu or `netMode==2`. Otherwise it multiplies the sky color's R, G and B by `atmo` (byte conversion), preserving its existing alpha until `atmo <= 0.01`, when it replaces the whole color with `Color.Transparent`.

Thus, disregarding floating-point rounding at the boundary:

| Event during ascent | Camera-center depth from world top, tiles |
|---|---:|
| Atmosphere starts darkening | below `b + S/5` |
| Sky color becomes transparent | at or below `b + S/500` |
| `atmo` reaches zero | at or below `b` |

For widths 4200, 6400 and 8400 tiles, `b` is approximately 75, 88.22 and 105 tiles respectively. Add `S/5` for darkening onset, or `S/500` for the transparent-color boundary. Multiply a center-depth threshold by 16 and subtract integer `H/2` to express it as camera-top pixels in that same coordinate system. These are atmosphere thresholds, not universal city silhouettes' exit heights or the `ZoneSkyHeight` boundary.

Example with explicitly assumed `S=400`, width 6400: atmosphere starts darkening near camera-center depth 168.22 tiles; sky color becomes transparent near 89.02 tiles. Space classification starts at player-center tile row 139 or shallower. These events are separate even before camera offsets or zoom are considered.

Installed `Main.DrawBG()` copies `ColorOfTheSkies` into the surface background base color (IL `018b–019a`). The official [far/middle loader](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L195-L279) multiplies that base color by the style's far-back alpha. A custom renderer that constructs its own color can bypass this behavior; darkening RGB is not itself a smooth alpha fade of land geometry.

## Positional exit and tML's default close layer

The official [DrawCloseBackground implementation](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L281-L339) defaults to `scale=1.25`, horizontal `parallax=0.37`, `a=1800`, `b=1750`, then doubles scale. A style can override those values. Its top is:

```text
T = trunc(((-C + screenOff/2) / (16*S)) * a + b) + trunc(scAdj)
```

Here `T`, `screenOff`, `scAdj`, `a`, and `b` are draw-position pixel quantities; parallax and scale are dimensionless. The draw guard is `C < 16*S + 16`. That stops this draw when camera top descends to one tile below `worldSurface`. The close hook's `false` return skips only the loader's close draw; it does not suppress far/middle hooks. [Hook contract](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L64-L82).

For reproducibility, the installed `Main.DrawBG()` initializes these adjustments (IL `0000–00ef`):

```text
R = min(PlayerInput.RealScreenHeight, Main.LogicCheckScreenHeight)
L = C + integer_divide(H, 2) - R/2f
d = max(Main.maxTilesY * 0.15f * 16f - L, 0) * 0.00025f
scAdj = float(16*S) / (L + R) * (0.45f - d*d) * multiplier
multiplier = -500 if maxTilesY <= 1200
             -300 if maxTilesY <= 1800
             -150 otherwise
screenOff = H - 600f
```

These describe the values at that method's initialization; use the actual draw-pass camera and matrix when evaluating a texture's screen position. They are not additional alpha thresholds. The use of `maxTilesY` here is positional correction, separate from the `worldSurface` Space predicate.

**Derived geometry, not an engine cutoff:** under an ordinary downward-positive, unrotated projection, let `q` be the first nontransparent texture row, `s` its effective scale and `Tscreen` its final screen-space top. Its visible content is entirely below the viewport when `Tscreen + s*q >= viewportHeight`. With an opaque first row, this simplifies to `Tscreen >= viewportHeight`. Transparent padding, crop, layer offsets, zoom and viewport height therefore change the exit altitude. Invert gravity or alter the matrix and use transformed bounds instead. Solve this condition for each layer; do not assign all backgrounds a supposed universal exit height.

## Calamity's actual owning-repository examples

[AstralSurfaceBGStyle](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Backgrounds/AstralSurfaceBGStyle.cs) returns Horizon through `ChooseFarTexture` and Far through `ChooseMiddleTexture`; it custom-draws the remaining three layers. Those custom layers use:

```text
M = (-(C + integer_divide(H,2) - 600f) + screenOff/2f)
    / (float(S) * 16f)    // substitutes S=1 only if S==0
T = trunc(M*A + B) + trunc(scAdj) + P + layerOffset
```

| Custom layer | A | B | layerOffset | Effective scale | Horizontal parallax |
|---|---:|---:|---:|---:|---:|
| Middle | 1800 | 1500 | 475 | 2.50 | 0.40 |
| Close | 1950 | 1750 | 175 | 2.62 | 0.43 |
| Front | 2100 | 2000 | 275 | 2.68 | 0.49 |

In normal gameplay, `P=30-180=-150`; menu and world-generation branches differ. Ascending generally pushes these layers down through this placement scheme. Each retains `C < 16*S+16`; none has a direct `ZoneSkyHeight` rejection or its own altitude opacity envelope. Style fades increment/decrement by the supplied transition speed and clamp to [0,1]. The code uses a fixed Astral RGB tint with inherited background alpha; do not equate that with a full inherited sky color.

[AstralDesertSurfaceBGStyle](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Backgrounds/AstralDesertSurfaceBGStyle.cs#L41-L117) uses the same `M` and depth guard, with Middle `(A=1800,B=1500,offset=475)` and Close `(1950,1750,475)`. Its normalized positioning constant is not a fade fraction or upper cutoff.

[AstralSky](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Skies/AstralSky.cs) draws a screen-filling sky in the farthest depth interval, tinted by its own opacity. Activation/biome/monolith state controls opacity changes of `0.02f` per update. There is no direct altitude test in its `Draw` or `Update`. The world-size-dependent `AstralBiomeHeight` calculation at lines 43–51 is **unused** later in the method; it is not an exit height. Its star displacement is `trunc(-C / (16*S - 600) * 200)` pixels, also not a land fade. Its cloud multiplier is `(1-opacity)*0.97+0.03`. External biome activation may still change with location; that does not establish an altitude envelope inside this sky class.

## Wastes observation and our design recommendation

Read-only snapshot of current, already modified project files: `Content/Backgrounds/WastesLandscapeV1Renderer.cs:72–93` constructs light using RGB floors `(65,75,98)` and the opaque three-channel Color constructor, then multiplies by style opacity. Only layer 1 receives `MiddleOpacity(altitude)`; layer 0 Far has no altitude multiplier. `Common/Backgrounds/WastesCameraProjection.cs` uses Far vertical motion `.012`, Mid `.03`, and world-relative Close placement. That explains why Far cannot rely on either vanilla transparent sky tint or rapid positional exit.

**Our recommendation; project policy, not a borrowed standard:** preserve positional departure for Close and the existing Mid transition. Give Far a smooth altitude envelope that reaches exactly zero by the project's Space reference. Using the existing normalized ascent `u`, a concrete starting proposal is:

```text
ground = (S - 50) * 16
spaceReference = S * 16 * 0.35
u = clamp((ground - worldCameraCenterY) / max(1, ground-spaceReference), 0, 1)
t = clamp((u - 0.70) / 0.30, 0, 1)
farOpacity = 1 - t*t*(3 - 2*t)
```

The `0.70` onset and this envelope are our proposed tuning. Multiply the complete layer color, including any city glow/underfill, by this factor. Ensure every land layer has either a verified positional exit or an opacity envelope reaching zero; do not clamp land to the viewport to maintain coverage during flight.

This camera-based policy reaches zero at its continuous reference, not necessarily the precise frame when the player flag changes. If acceptance means literally no city whenever `LocalPlayer.ZoneSkyHeight` is true, use the verified player-center boundary as the final zero-opacity condition (and begin the continuous fade sufficiently before it). State that choice explicitly. Keep camera interpolation/zoom behavior consistent with the current renderer.

## Limits and handoff

- No claim that these Calamity files guarantee a particular visible silhouette at a fixed tile height. No live observations were performed. Exact Wastes exit rows still depend on the active city image, matrix, viewport, zoom and world datum; parent QA owns those checks.
- Scope is ordinary 1.4.4 surface gameplay. Remix, menu, world-generation overrides, inverted gravity and camera modifications need their own evaluation. Player Space, visual camera Space and atmosphere may intentionally disagree.
- The optional [Dynamic Parallax author-thread URL](https://forums.terraria.org/index.php?threads/dynamic-parallax.61836/) redirected to a forum landing page in this research environment; no implementation claim or numeric standard was taken from it.
- Public stable documentation is mutable. Use the embedded runtime commit/hash and pinned owning-repository links above to reproduce this note. IL-derived formulas summarize only the identified installed version, not every Terraria release.
- Only this new file is authored by this research task. Existing `RESEARCH_TMODLOADER_WORLD_SYSTEMS.md`, Localization HJSON, renderer changes and parent QA artifacts remain outside its write scope.
