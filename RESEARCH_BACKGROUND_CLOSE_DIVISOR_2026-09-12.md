# Close-background divide-by-zero: pinned runtime research

2026-09-12. Research sidecar only. Incident supplied by the main agent: caught `System.DivideByZeroException` at `SurfaceBackgroundStylesLoader.DrawCloseBackground(Int32 style)`, `BackgroundLoaders.cs:323`, at 15:53:15 Central immediately after the held `rib1` camera teleport in disposable V3/Plain. The main agent retains the native RED log, reproduction, fixes, and proof. This note does not claim to have reproduced the incident or identified its runtime style argument.

## Main-agent native follow-up (21:18–21:26 UTC)

The earlier rankings below are retained as the research-time hypothesis record.
Package L (`CA130BC91ED5329BA8B5B0C1400561BC03DAD22B437E5F86249E77CA0568CDC4`)
reproduced the failure naturally at16:18:23.685 Central: Engraft style17,
texture490 `Engraft/V0_Close`, loaded actual952x480 but cached0x0, scale1.25,
stride0. The caught line323 exception followed2ms later. This was **before**
the separate control command at16:18:36; that command injected nothing because
the original cached width was already zero. Thus the zero-cache mechanism is
now directly observed, not inferred from the placeholder or camera alone.
See `Art/Validation/MawShallow-2026-09-12/native-l-zero-cache-red.json`.

Package M (`923AAA55118BD7C6FBD2A2C90D8214A1DF927E77D0E16363A7FF4DB3D9418449`)
uses `ApogeanCloseBackgroundDimensions.Resolve` at both owned close selectors.
It skips unavailable/invalid textures with-1 and derives both cached dimensions
from a loaded valid asset. No blocking request, engine patch, artwork/scale or
third-party slot change.565 pure/stub dependency checks cover actual resolver
branches, including ownership, pending assets, partial arrays and stride limits.

Native M passed the rib1 replay and an explicit one-invocation cached-width-zero
control16:24:55.407. The next update restored952, subsequent selector reported
2380 stride, and no caught exception occurred. Rib2/rib3/pocket/rib1 transitions,
reload and11648-cell pristine checks also passed.33 raw records are retained in
`native-m-close-guard-green.json`; caughtRenderFailures is empty. Native safe-save
16:26:14–15 retained the exact interaction state. This fixes this observed close
draw failure, not the historical DrawLiquid crash or broad art/routing gates.

The temporary probe/injection source was removed after the native test; the
small production guard, arithmetic/resolver tests and raw RED/GREEN evidence
remain. Cleanup source compiled at21:29UTC with zero warnings/errors using
`dotnet build -t:Compile -p:ApogeanMawPackedQA=true`; the installed M package hash
was unchanged. A later installed run without instrumentation is still required.

## Verified result

Line 323 is `Main.instance.bgLoops = Main.screenWidth / Main.bgWidthScaled + 2;`. The exception means the **integer divisor `Main.bgWidthScaled` was zero at that division**. It does not identify which texture or style supplied it. The installed DLL contains integer `div` at IL `017d`, with its PDB sequence point starting at IL `016e` mapped to source line 323. [Pinned implementation][loader] [Installed DLL][dll] [PDB][pdb]

**`PreDrawCloseBackground` runs before texture selection, loading, scale initialization, and the divisor calculation.** Returning `false` exits immediately. Consequently, the current Apogean HD branch cannot reach this division in the same invocation after returning `false`. Its 2x2 transparent placeholder also produces stride 5 at the default effective scale, not zero. Increasing its dimensions is not an evidence-supported fix. [Hook contract][contract] [Local style][style] [Installed DLL][dll]

## Version and evidence checks

- Installed DLL ProductVersion: `1.4.4.9+2026.07.3.0|2026.07|stable|Stable|666f69962d3bdffde54fc14025f02634965b4e7c|5250924198908260370`; FileVersion `1.4.4.9`.
- DLL SHA-256: `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C`.
- PDB SourceLink names the same full commit. Its `BackgroundLoaders.cs` SHA-256 is `A98C113F18C692F7DD4B8AACB7D8C3AD3A27E4F8E7F5D18CD63A002EDC449612`; the official pinned raw file matches after LF-to-CRLF normalization. Use actual source line numbers below; the web extractor collapses some blank lines.
- Read-only IL inspection used the already installed `Libraries/mono.cecil/0.11.6/lib/netstandard2.0/Mono.Cecil.dll`. No game assembly was executed. `rg` found an existing [IL-based research reference][prior], but no saved decompiled source in the searched repo/ModSources/install locations. No applicable `AGENTS.md` was found in the repository or its ancestor chain.

## Exact execution order

This order is independently verified in the pinned file and installed method IL. [Pinned source, lines 281-340][loader] [Installed DLL/PDB][dll]

| Source line | Operation |
|---|---|
| 283-289 | Exit during loader/menu loading or when this style's front alpha is nonpositive. |
| 291-295 | Resolve style; call `PreDrawCloseBackground`; exit if null or false. Hook IL `002f`, exit IL `0036`. |
| 297-302 | Set scale 1.25, parallax 0.37, a=1800, b=1750; then call `ChooseCloseTexture` with references. |
| 304-309 | Reject negative/out-of-range texture slots; call `LoadBackground`. |
| 311-312 | Double scale; convert cached width times scale to an integer stride. |
| 314 | Call `SkyManager.DrawToDepth`, an intervening callback boundary. |
| 316-320 | Calculate camera placement and menu override. |
| 323 | Divide screen width by the shared integer stride. |
| 325-339 | Only now check surface camera depth and draw repeated close textures. |

Thus camera depth does not protect line 323. `Main.DrawSurfaceBG` calls this loader for modded style indices while iterating the front-alpha array (installed IL `119b-11bc`); an outgoing style may still participate. The current selected biome alone does not establish the failing style. [Main patch, lines 6754-6755][mainpatch] [Installed DLL][dll]

The other variable divisions in the close method use floating point; their zero denominators do not produce this integer divide-by-zero. The integer half-stride calculation divides by constant 2. [C# arithmetic rules][arithmetic] [Installed DLL][dll]

## Dimensions, return contract, and ranked hypotheses

The surface API specifies no fixed minimum PNG dimensions. For a path that returns true and selects a valid texture, the practical arithmetic requirement is a finite positive effective scale and **`(int)(cachedWidth * (2f * returnedScale)) >= 1`**, with the product representable as an integer. Height is not this divisor. Floating-to-integer conversion truncates toward zero. At unchanged returned scale 1.25, any positive integer cached width passes this condition: width 1 gives stride 2; width 2 gives stride 5. A positive product below 1 truncates to zero. [Surface API][contract] [Numeric conversions][conversions] [Installed DLL][dll]

`false` suppresses the entire engine close path after the hook. `ChooseCloseTexture` defaults to `-1`; a negative return skips engine close rendering if that selector is reached. Far and middle selectors also accept negative returns, but are separate passes; their placeholder choices do not feed the close selector's width calculation. [Hook defaults][contract] [Installed DLL][dll]

Local observations:

- [Transparent.png][transparent] is **2x2**, all four pixels RGBA zero, SHA-256 `63A8AB9023A545DD49154C1FD41D4DA58A381EAB6708C5176080F09E8611F215`. Its [generator][generator] explicitly creates 2x2.
- Both production and render-lab styles return the transparent slot for all three HD selectors, leave scale unchanged, and return false after `DrawV0`. [Local style, lines 23-104][style]
- `Supports` tests dictionary membership. Its nine registered biomes exclude Engraft, whose production style therefore takes the true/non-HD path. Both local Engraft close PNGs are **952x480**, implying stride 2380 with correct cached dimensions. This is a concrete route to inspect, not proof that the teleport selected it. [Renderer, lines 26-46][renderer] [Engraft V0][engraft0] [Engraft V1][engraft1]

Evidence-adjusted ranking of the proposed hypotheses:

1. **Slot/loading metadata: strongest remaining lead, unconfirmed.** Installed `Utils.Width(Asset<Texture2D>)` returns 0 when `IsLoaded` is false (IL `0000-0009`). `BackgroundTextureLoader.ResizeArrays` requests asynchronously and stores that width (IL `0055-0071`). Installed `Main.LoadBackground` refreshes dimensions only for state `NotLoaded=0`; `Loading=1` and `Loaded=2` return without refresh (IL `0000-0018`, `00a2`). Therefore a cached zero can survive even after the texture becomes loaded. This establishes a mechanism, not that its preconditions occurred here. Missing keys throw earlier; invalid numeric slots return earlier. Capture a valid slot with cached zero to establish this lead. [Installed DLL][dll] [ReLogic enum metadata][relogic] [Async request contract][request]
2. **Placeholder width/scale truncation: contradicted for the current HD path.** False exits first, and 2x2 with scale 1.25 would be safe anyway. A different true-returning style could choose a subpixel effective stride; neither current Apogean selector changes scale. [Local style][style] [Installed DLL][dll]
3. **Previous transition scale carried forward: weak.** Each continuing call resets scale to 1.25 and recomputes stride. A teleport can expose a different participating style, but does not itself scale the width. An intervening sky callback could overwrite shared `bgWidthScaled` after its assignment; that is a separate unverified mutation/reentrancy hypothesis. [Installed DLL][dll]

## Narrow regression seam for the main agent

Suggested work, not implemented here:

- Instrument the **non-HD close-slot resolver** and record style type/slot, hook result, texture name/slot, asset state, actual versus cached dimensions, returned scale, computed stride, alpha, and camera position. Capture outgoing participants too. If widths are positive before sky drawing but the divisor is zero afterward, inspect that callback boundary. Confirm the loaded mod matches current source.
- At that resolver, allow a texture only when its slot, loaded dimensions, and computed stride are usable; use the established `-1` return for an unavailable fallback. If a loaded asset has stale cached dimensions, repair only the proven affected Apogean background metadata during initialization or the narrow resolver. Asset loading alone may leave the cached width unchanged. [Request documentation][request] [Installed DLL][dll]
- Regress HD suppression (false means close selector never runs), valid fallback (952 -> 2380), cached-zero fallback rejection, and subpixel/invalid scale rejection. Assert positive cached height for rendering validity separately. The existing [HD asset validator][validator] checks authored layers and renderer text, but does not cover this engine metadata/return contract.
- Main retains the native held-rib1 V3/Plain replay and log acceptance. A source/asset check cannot clear the reported runtime RED.

Only this new research file was written. No code/assets, builds, game/UI, dependencies, commits, existing world-systems research, or localization were changed.

[loader]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs#L281-L340
[contract]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBackgroundStyle.cs#L34-L83
[request]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModContent.cs#L104-L130
[mainpatch]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L6750-L6758
[arithmetic]: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/arithmetic-operators#division-operator-
[conversions]: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions#explicit-numeric-conversions
[dll]: E:/SteamLibrary/steamapps/common/tModLoader/tModLoader.dll
[pdb]: E:/SteamLibrary/steamapps/common/tModLoader/tModLoader.pdb
[relogic]: E:/SteamLibrary/steamapps/common/tModLoader/Libraries/ReLogic/1.0.0/ReLogic.dll
[style]: <C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Backgrounds/ApogeanSurfaceBackgroundStyles.cs:23>
[renderer]: <C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Backgrounds/HighDefinitionSurfaceBackgroundRenderer.cs:26>
[transparent]: <C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Backgrounds/Diagnostics/HD/Transparent.png>
[generator]: <C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Tools/New-HdSurfaceBackgroundPrototypes.ps1:182>
[engraft0]: <C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Backgrounds/Engraft/V0_Close.png>
[engraft1]: <C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Backgrounds/Engraft/V1_Close.png>
[prior]: <C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/RESEARCH_BACKGROUND_ALTITUDE_HANDOFF.md:18>
[validator]: <C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Tools/Test-BackgroundHdContracts.ps1:9>
