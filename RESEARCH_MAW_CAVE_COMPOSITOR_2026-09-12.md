# Maw cave compositor: installed draw seam research

Date: 2026-09-12. Research only; no runtime, code, art, world, or configuration changes. No game/UI launch, dependency installation, commits, or additional tasks. The only authored file is this note.

## Decision

**Do not extend the Hell `CustomSky` final-depth technique to ordinary caves.** In this installed build, its final callback occurs before the cave black fill and before the actual underground backdrop. A different `minDepth` test cannot move it past those later draws.

**The smallest public candidate for a QA cave probe is an `Overlay` registered in `Overlays.Scene` with `RenderLayers.Background`.** The installed normal-frame renderer invokes that layer immediately after either drawing the cave background directly or compositing `backgroundTarget`, and before native walls, tiles, NPCs, players, projectiles, and foreground water. The public classes and registration/activation methods exist in the official API. [Overlay](https://docs.tmodloader.net/docs/stable/class_overlay.html), [OverlayManager](https://docs.tmodloader.net/docs/stable/class_overlay_manager.html), [Overlays](https://docs.tmodloader.net/docs/stable/class_overlays.html)

There is an important limit: **background water is drawn before the cave backdrop**. Therefore no single insertion point in this sequence is simultaneously after the cave backdrop and before *both* native liquid passes. An opaque full-screen overlay at `Background` does not establish the literal “behind all liquids” requirement. Recommend a small, dry-aperture probe with conservative exclusion of liquid footprints first. That can demonstrate HD cave composition while preserving existing wet pixels; it does not prove a panorama visible through every liquid. Full wet-scene support remains unverified.

Natural unsafe walls also remain real foreground occluders. A correctly placed panorama can be invisible throughout a fully walled Maw chamber. Neither increasing its alpha nor changing sky depth fixes that visibility constraint.

## Evidence baseline and confidence

The installed file was rechecked with `Get-FileHash` and `FileVersionInfo`:

| Field | Observed value |
|---|---|
| DLL | `E:/SteamLibrary/steamapps/common/tModLoader/tModLoader.dll` |
| SHA-256 | `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C` |
| File version | `1.4.4.9` |
| Product version | `1.4.4.9+2026.07.3.0\|2026.07\|stable\|Stable\|666f69962d3bdffde54fc14025f02634965b4e7c\|5250924198908260370` |
| Official source revision | [`666f69962d3bdffde54fc14025f02634965b4e7c`](https://github.com/tModLoader/tModLoader/tree/666f69962d3bdffde54fc14025f02634965b4e7c) |

**Installed evidence** below means method-level decompilation of that exact DLL with the existing `C:/Users/max_h/AppData/Local/Temp/apogean-ilspy/ilspycmd.exe`, version 9.1.0.7988. Decompilation went to stdout/in-memory inspection, without writing decompiled files. The supplied `apogean-background-research` and `apogean-hd-bg-research` directories were empty when inspected.

**Public API/source evidence** means official pages opened in the browser during this research. Stable docs are mutable, and some cached class pages still display v2026.06 while others display v2026.07; relevant signatures were checked against the installed DLL. The pinned tModLoader repository contains patches, not the entire vanilla renderer. Its [Main patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch) corroborates transforms and hook additions; the full ordering below comes from the installed assembly, not a claim that the patch contains the whole method.

**Inference/recommendation** is explicitly identified. **Unknown** means no live proof was obtained; no rendering was run in this sidecar.

## Confirmed installed ordering

### Ordinary underground, normal gameplay frame

The relevant sequence in `Main.DoDraw(GameTime)`, with conditional alternatives combined, is:

1. `DrawBG()` calls `DrawSurfaceBG()`, then conditionally `DrawUnderworldBackground(false)`.
2. `DrawBackgroundBlackFill()` draws the ordinary subterranean black region using the game matrix.
3. The normal world batch begins; `DrawWaters(true)` or `backWaterTarget` is drawn, followed by the `BackgroundWater` overlay layer.
4. The cave backdrop is drawn: `DrawBackground()` when `drawToScreen` is true, otherwise `backgroundTarget` is composited.
5. **`Overlays.Scene.Draw(spriteBatch, RenderLayers.Background)`**.
6. `ScreenDarkness.DrawBack`, then `DoDraw_WallsTilesNPCs()`.
7. Remaining entities and effects, followed by `DrawWaters()` or `waterTarget` and the `ForegroundWater` layer.
8. Final scene filters, then interface drawing.

The fullscreen-map path returns before step 4. Menu/server drawing also exits before this ordinary-world sequence.

Inside `DoDraw_WallsTilesNPCs`, the earliest cached NPC group (`DrawCacheNPCsMoonMoon`) is drawn before `DoDraw_WallsAndBlacks`; both are still after step 5. Walls use `DrawWalls()` directly or the previously rendered `wallTarget`. Nonsolid tiles, waterfalls, other behind-tile NPCs, solid tiles, players, and later NPC groups follow. Thus the proposed seam is before even the early cached NPC group, not merely before ordinary front-facing NPCs.

This is **confirmed installed call ordering**, including the two `drawToScreen` branches. Visual correctness under all effects is a separate question.

### Why final-depth CustomSky fails for caves

Installed `SkyManager.DrawRemainingDepth` dispatches the interval whose minimum is `float.MinValue` and whose maximum is the current depth tracker, then sets that tracker to `float.MinValue`. The public API exposes the depth methods but does not promise “after cave background” semantics. [CustomSky API](https://docs.tmodloader.net/docs/stable/class_custom_sky.html), [SkyManager API](https://docs.tmodloader.net/docs/stable/class_sky_manager.html)

Installed `DrawSurfaceBG()` resets the tracker and calls `DrawRemainingDepth` near its end when the fullscreen map is closed. This happens inside step 1, **before steps 2 and 4**, even when an ordinary cave is on screen. A sky can receive the callback and successfully issue draw calls, yet be covered by the later black fill/backdrop. A logged callback is not evidence of correct cave placement.

Hell is different: `DrawUnderworldBackground` only enters its draw body when the camera bottom reaches `(maxTilesY - 220) * 16`. It resets the tracker, draws five layers from index 4 down to 0, then calls `DrawRemainingDepth`. The per-layer depth requests are 11, 9, 7, 5, 3 in normal drawing; capture's `flat` path uses depth 1. Therefore the final Hell interval follows the five Hell layers. It is still not a universal after-caves callback. More than one final-band call can occur in a frame because surface and Hell reset the tracker separately.

The supported `ModSceneEffect.SpecialVisuals` mechanism registers skies/shaders and activates them through player scene visuals; that establishes activation, not a later draw location. [Pinned ModSceneEffect source](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSceneEffect.cs)

## Minimal alternate seam and its boundaries

**Recommendation, not implemented:** one QA-only subclass of `Terraria.Graphics.Effects.Overlay`, constructed with an explicit `RenderLayers.Background`, bound under a unique diagnostic key in `Overlays.Scene`, and activated only for an enabled ordinary-Maw-cave fixture. Use one already available candidate texture or `TextureAssets.MagicPixel` to establish placement before adding layers. `RenderLayers.All` is a later, separate layer; the installed manager compares layer equality, so it is not a wildcard substitute.

Installed `OverlayManager.Draw` at this call has `beginSpriteBatch = false`. It invokes visible overlays in the existing batch. A basic textured draw needs no new render target, shader, IL patch, or `Begin`/`End`. `EffectManager.Bind` loads an effect immediately if its manager is already loaded. `Activate` calls manager activation before the effect's own activation. The overlay should implement activation/deactivation modes and use its manager-maintained opacity; an unconditional always-visible draw defeats the QA gate.

**Compatibility caveat, installed:** activating an overlay causes other active overlays at the same `EffectPriority` to fade out, even if their render layers differ. `EffectPriority` is distinct from `SceneEffectPriority`. This makes the public seam suitable for a controlled proof, not automatically conflict-free production composition. Check for competing scene overlays in the fixture; no priority choice universally guarantees coexistence. Deactivate the diagnostic effect on exit/unload and start with a fresh session for repeatable lifecycle testing.

| Candidate | Installed consequence | Assessment |
|---|---|---|
| `CustomSky`, final depth | Before cave black fill and cave backdrop | Reject for ordinary cave replacement. |
| `Overlay`, `Landscape` or `BackgroundWater` | Still before the cave backdrop | Too early for an opaque after-cave composition. |
| `Overlay`, `Background` | After cave backdrop; before walls/tiles/entities/front water | Smallest public QA candidate, subject to wet-pixel exclusions. |
| `ModSystem.PostDrawTiles` | After native tile/NPC drawing | Too late for a background. [Pinned ModSystem source](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs) |
| Hook only after `DrawBackground` returns | Sometimes runs while building a cached padded target; also has a separate camera-capture caller | Requires target/camera/cadence handling and still does not solve both liquid passes. Not the first probe. |

No renderer detour, liquid reordering, or sweeping rewrite is recommended on this evidence.

### Liquid qualification

The installed normal path explicitly draws background water before the native cave texture, and foreground water afterward. Waterfalls occur inside the later tile/NPC pass. Preserving the native order means the proposed overlay is naturally behind foreground water and waterfalls, but after the background-water contribution.

**Inference:** an opaque overlay can replace pixels contributed by the earlier liquid pass. Merely observing a front water surface afterward would not prove that every liquid edge survived correctly. `DrawWaters` also updates liquid state and invokes liquid-edge machinery, so redrawing it ad hoc is not a justified one-line repair.

For the bounded probe, draw only within an existing dry aperture and conservatively exclude liquid-bearing cells and an edge margin. If the main task's mask is used later, require evidence that it covers animated surface/edge footprints as well as liquid occupancy. A single `LiquidAmount > 0` cell test is not established here as a pixel-exact liquid mask. The probe may preserve wet pixels by leaving their existing backdrop unchanged; it must not be reported as verified panoramic rendering through liquid.

## Camera, native scale, and render state

**Confirmed installed:** the `Background` overlay inherits a deferred, alpha-blended batch using `Main.GameViewMatrix.TransformationMatrix` (through the deprecated `Main.Transform` alias), `Main.Rasterizer`, no depth testing, and `Main.DefaultSamplerState`. It does not inherit the surface `BackgroundViewMatrix` batch. Public transform properties are exposed by [SpriteViewMatrix](https://docs.tmodloader.net/docs/stable/class_sprite_view_matrix.html).

For stock transforms, the installed zoom calculation is:

```text
forced zoom = max(1, screenWidth / 1920, screenHeight / 1200)
world zoom  = forced zoom * clamp(GameZoomTarget, 1, 2)
background zoom = forced zoom
```

The tModLoader option that removes forced minimum zoom changes the two divisors to 8192. `SystemLoader.ModifyTransformMatrix` can subsequently alter the world matrix. The installed values and the pinned [Main transform patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch) agree.

For the initial proof, use screen-relative **pre-transform world pixels**, subtracting `Main.screenPosition` exactly once for world anchors. Let the existing matrix apply zoom and gravity inversion once. Do not additionally multiply draw positions by game zoom or use `UIScaleMatrix`. Native tile spacing is 16 world pixels; a texture drawn at scale 1 has one texel per pre-transform world pixel. That is not necessarily one framebuffer pixel: stock 2560x1440 at nominal 100% game zoom has forced zoom 4/3, so a source texel spans approximately 1.333 output pixels. A deliberately framebuffer-native panorama would need different scale compensation and is a separate choice.

Installed `SpriteViewMatrix.Rebuild` derives its center/translation from the current graphics viewport, applies flip and zoom, and includes a small 1/256-pixel translation. Use the actual matrix; do not reconstruct an assumed identity matrix. For pan/repeat coverage, inverse-transform the current viewport corners into batch coordinates, or validate an explicitly bounded aperture at each zoom. Keep the texture at its chosen native scale and crop/repeat it; screen-height fitting like the Hell sky's `viewportScale` changes authored feature size.

Sampler facts also differ from an earlier research-note assumption: installed `DefaultSamplerState` is **LinearClamp for direct drawing** and **PointClamp for cached-target composition**. The surface/Hell `DrawBG` batch explicitly uses LinearClamp. “Terraria always point-samples backgrounds” is false for this build. Start by inheriting the batch; if a later pixel-art probe deliberately changes sampling, it must restore the exact caller state and record that choice.

### Offscreen targets are not one capture mechanism

1. **Native cached layers:** `RenderBackground()` draws background water and the cave background to different transparent targets, using a zero-offset batch plus the native offscreen margin. `InitTargets` starts `offScreenRange` at 192 and can reduce it; layer targets include twice that margin. Their scene positions are retained and they may update on different render-count frames. In `DoDraw`, cave-target X placement additionally applies `caveParallax`; wall placement does not. A post-`DrawBackground` hook would inherit this cache behavior. The public `Background` overlay instead executes during final composition with the restored live camera: **do not add `offScreenRange` merely because `drawToScreen == false`.**
2. **Normal screen-filter capture:** when enabled, `Filters.Scene.BeginCapture(screenTarget, ...)` surrounds the normal scene and `EndCapture` occurs after the proposed overlay. This path includes the overlay by ordering. The backdrop remains subject to subsequent scene shaders. A read of `backgroundTarget` alone cannot show the overlay, and `screenTarget` can be stale/unused when that filter-capture path is inactive.
3. **Terraria camera/photo capture:** installed `CaptureCamera.DrawTick` renders chunks through `Main.DrawCapture`, not the normal scene composition sequence. `DrawCapture` contains **no `Overlays.Scene.Draw` calls**. Thus this public overlay does not appear in native camera capture without separate integration. `DrawCapture` also replaces `GameViewMatrix`, sets `offScreenRange` to zero, changes camera position and screen dimensions, uses `CaptureBackground`, and has special surface framing. Chunk targets are 2048x2048; optional scaling limits the output to 4096 on each axis. A camera export is not evidence of normal-frame pixel scale or overlay absence in gameplay.

**Recommendation:** use the existing QA harness only if it captures the completed normal `DoDraw` frame. Verify its actual path and target before interpreting a missing image. A manually bound offscreen target or isolated sky draw is not sufficient: native rendering switches targets, and the matrix depends on the current viewport. Any future capture adapter must preserve/restore render-target bindings, viewport, camera values, and batch state; do not blindly restore to the backbuffer during nested filter capture. No capture adapter was implemented or validated here.

## Visibility behind natural unsafe walls

Installed `WallDrawing.DrawWalls` reads the actual wall ID and draws wall textures in the later native wall pass. Its exclusions include no wall, occlusion by solid tile geometry, invisible-wall settings, lighting conditions, and mod wall draw hooks. It does **not** skip a wall because `Main.wallHouse[wall]` is false.

The official API defines `wallHouse` as safety for enemy spawning and `wallLight` as background light transmission. Neither is a blanket transparency rule. [Main wall fields](https://docs.tmodloader.net/docs/stable/class_main.html) The public wall sets also distinguish transparent wall types. [WallID.Sets](https://docs.tmodloader.net/docs/stable/class_wall_i_d_1_1_sets.html)

**Consequence:** ordinary opaque natural/unsafe wall texels occlude a correctly placed cave panorama just as opaque safe-wall texels do. Wall-free openings, genuine transparent wall pixels, and permitted invisible walls are the opportunities to see it. Mining foreground blocks alone does not establish a wall-free view. Blackness in a dark/walled chamber is not proof that the compositor failed. Do not suppress native walls or change wall/world data to manufacture a passing screenshot; choose an existing lit opening and a nearby opaque-wall control.

## Public underground slots: what they do and do not permit

The public `ModUndergroundBackgroundStyle.FillTextureArray` contract documents these four roles. [Pinned ModBackgroundStyle source](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs), [stable underground-style API](https://docs.tmodloader.net/docs/stable/class_mod_underground_background_style.html)

| Index | Role | Documented image size |
|---|---|---|
| 0 | Sky/ground border | 160x16 |
| 1 | Ground-to-rock background | 160x96 |
| 2 | Ground/rock border | 160x16 |
| 3 | Rock-layer background | 160x96 |

The pinned [ExampleMod underground style](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Biomes/ExampleUndergroundBackgroundStyle.cs) assigns these four indexes. They are vertical roles, not four independent HD parallax planes. There is no public underground counterpart to surface `PreDrawCloseBackground` in the reviewed class.

**Installed detail:** `DrawBackground` actually constructs seven-element internal arrays and passes them to the loader. That does not expand the documented four-role contract. Its tiling subtracts 32 from texture width; a 160-wide texture therefore has a 128-pixel repeat body. Body drawing uses 16-pixel slices and six-row/96-pixel assumptions. The documented duplicate-edge remark is qualified, but the width-minus-32 repeat calculation is directly confirmed. Supplying a large panorama does not turn these slots into an arbitrary HD compositor. Writing indexes 4-6 or resizing vanilla arrays is not recommended.

## Reconciliation with existing Apogean context

Reviewed `RESEARCH_HD_TERRARIA_BACKGROUNDS.md`, `RESEARCH_BACKGROUND_TRANSITIONS_UNDERWORLD.md`, and `Content/Backgrounds/RuinedUnderworldSky.cs` as context, without editing them.

- The HD note's surface benchmark and four-slot cave description do not establish a custom cave draw location or physical 1:1 scale. This note verifies those separately for the installed build.
- The transitions note combines installed 1.4.4.9 observations with a different 1.4.5 development revision. Its Hell conclusion must not be generalized to ordinary caves. The exact installed ordering here supersedes that extrapolation only; unrelated transitions were not re-audited.
- `RuinedUnderworldSky.Draw` checks the final-depth sentinel and its scene effect activates at Underworld height. That is consistent with the Hell evidence. Copying the depth check and changing only activation would not fix cave ordering.
- That sky also fits textures to screen height and changes tile colors through `OnTileColor`. Those are independent behaviors, not requirements of a background compositor, and should not be inherited by a minimal cave-placement probe.

## Smallest recommended bounded live probe — not performed

**Scope:** one enabled-for-QA overlay, one existing texture, one existing lit ordinary-cavern aperture, one camera pan, and one alternate zoom. Keep its drawing confined to the selected cave viewport/region; exclude surface, Hell, map/menu, fully dark unknown areas, and liquid footprints. Do not add production routing or new artwork for this test.

1. At a fixed camera, record the DLL identity, `drawToScreen`, camera/screen dimensions, viewport, actual game/background matrices, sampler, active render target, and the chosen capture path. A temporary in-memory draw counter can establish that the `Background` callback ran. If using a sky as a negative control, record its depth intervals too; do not mistake that counter for visibility proof.
2. A/B draw a tiny unmistakable diagnostic patch in a dry wall-free opening, then the existing Maw candidate at chosen native scale in the same aperture. Let the native later passes cover portions behind a nearby opaque wall, foreground tile, and passing entity. Do not edit the fixture geometry. A clipped patch establishes the seam before any full-viewport compositor is attempted.
3. Pan roughly one tile and repeat at the alternate zoom. Pass only if placement remains aligned with the aperture, no offscreen-margin shift appears, texel scale matches the recorded matrix, and no new edge gap appears. If both direct and cached rendering paths are claimed supported, repeat this same small check under each actual `drawToScreen` value.
4. Include a nearby native liquid edge as an exclusion control: the panorama must not overwrite its excluded pixels. This is a dry-aperture/liquid-preservation result, not a through-liquid result. If a proposed mask exposes any wet region, require an additional wet-edge A/B before expanding the claim.
5. Disable/deactivate the diagnostic effect and verify the original scene returns. Record any competing-overlay fade. Capture the completed normal frame; expect native photo capture to omit this overlay until independently integrated.

**Stop after this probe.** Failure should be classified as callback/activation, wrong frame/capture path, matrix/coverage, wall occlusion, liquid exclusion, lighting, or overlay competition. None justifies a broad renderer rewrite by itself.

## Remaining uncertainty

- The recommended overlay has not been implemented, compiled, or run here. Its normal-frame placement is confirmed from the installed renderer; actual Maw visual quality is not.
- No existing Maw mask was audited, and no pixel-exact liquid footprint or through-liquid composition is established. The strict all-liquid requirement remains open beyond the conservative dry probe.
- No live natural-wall visibility measurement, biome-boundary transition, light response, filter compatibility, or overlay-priority coexistence test was performed.
- The main task's capture harness was not audited. Native photo capture's omission is confirmed; a custom offscreen harness must identify which draw path it actually uses.
- A future tModLoader update or third-party draw detour can change the observed order. Recheck this narrow seam against the supported DLL before promoting a probe.

Strongest evidence: the exact installed `Main.DoDraw` branches converge on `RenderLayers.Background` after cave composition and before `DoDraw_WallsTilesNPCs`, while both final-depth sky dispatches are earlier inside `DrawBG`. The two liquid passes and the missing overlay calls in `DrawCapture` are concrete limits on what that seam alone proves.
