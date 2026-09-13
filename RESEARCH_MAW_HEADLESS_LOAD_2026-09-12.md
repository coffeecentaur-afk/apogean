# Maw QA world: dedicated-server load/save risks

Date: 2026-09-12. Repository inspected at `c15adea10ef4ac2c90d6fc041410fc3073ce51b9`.

Main-task follow-up through2026-09-13 00:01UTC: native copied-world testing
confirmed two existing records disappear on the unfixed server save. Eight
other candidate records were already absent in the input; no native-loss claim
for those. A narrow ten-hook repair passes180 actual-hook cases and preserves
all16 original system records through fixed-package save/exit/reload/save.
See `Art/Validation/MawHeadless-2026-09-12/README.md`. The original source-audit
observations below deliberately retain their original evidence boundary.

## Result and evidence boundary

The strongest finding is **silent loss of ten systems' QA metadata on a server save**, conditional on that metadata existing in the input world. This is a source-proven serialization consequence, not an observed smoke-test failure. No unconditional Maw/Anatomy texture-related startup crash was established at this HEAD. One eager `Main.LocalPlayer` access deserves attention, but the runtime's initialized server dummy prevents treating the access alone as proof of a null-reference crash.

This investigation followed the research skill's primary-source and single-note workflow. No additional agents, game/server/decompiler launches, builds, UI actions, save modifications, C# edits, or commits were performed. Only this note was written. Command-line and sandbox preparation remain with the main task. The unrelated dirty localization and world-systems research note were left untouched.

Runtime identity supplied by the main task: `1.4.4.9+2026.07.3.0`, revision `666f69962d3bdffde54fc14025f02634965b4e7c`. A read-only SHA-256 check of [installed tModLoader.dll](E:/SteamLibrary/steamapps/common/tModLoader/tModLoader.dll) independently matched `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C`. Framework references below use that exact official source revision; linked GitHub line numbers were checked against raw source. The selected `.tmod` and copied world were not opened or certified here.

## 1. Server saves omit ten QA systems' existing metadata

tModLoader supplies fresh save data, omits empty system entries, and records then discards a system's data if its save hook throws. On load, a recognized system receives its tag; only an unrecognized system goes into the unloaded-data fallback. Thus a loaded system that declines to load/save its own tag does not get automatic preservation. A save can finish without warning when a guard simply returns. [Pinned WorldIO.cs, lines 507–557](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/IO/WorldIO.cs#L507).

These eight systems load their fixture records using the V3 world name, but their save hooks require `IsQa`, whose first condition is `Main.netMode == NetmodeID.SinglePlayer`. On a server the condition is false even if a player named `gg` connects. The links target the save hooks; the additional line numbers identify the guard and loader in the same file.

| System | Omitted tag | Exact local evidence: save link; guard; load |
| --- | --- | --- |
| ArrivalPodLab | `podFixtureV1` | [343–369](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/ArrivalPodLab.cs:343>); 28–29; 353–369 |
| MawBoneLab | `mawBoneFixtureV1` | [218–227](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawBoneLab.cs:218>); 25–26; 223–227 |
| MawFangLab | `mawFangFixtureV1` | [222–227](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawFangLab.cs:222>); 23; 223–227 |
| MawToothArtLab | `mawToothArtV1` | [147–152](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawToothArtLab.cs:147>); 22; 148–152 |
| MawToothClusterLab | `mawClusterV1` | [178–180](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawToothClusterLab.cs:178>); 24; 179 |
| MawClusterOrientationLab | `mawClusterOrientationV1` | [307–309](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawClusterOrientationLab.cs:307>); 28; 308 |
| MawMaterialLab | `mawMaterialFixtureV1` | [223–232](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawMaterialLab.cs:223>); 25–26; 228–232 |
| MawMaterialFamilyLab | `mawFamilyFixtureV1` | [183–190](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawMaterialFamilyLab.cs:183>); 23–24; 187–190 |

Two more systems reject both loading and saving in server mode:

| System | Omitted data | Exact local evidence |
| --- | --- | --- |
| VegetationVisualLab | `groveCheckpointV1`, `groveLeft`, `groveTop` | [Save/load, lines 85–99](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/VegetationVisualLab.cs:85>); `IsQaWorld` requires single-player at line 43 |
| WastesGroundProfileSystem | `wastesGroundProfileQA2` | [Scope/load/save, lines 15–32](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Backgrounds/WastesGroundProfileSystem.cs:15>); `InScope` requires single-player |

This loses saved bounds, checkpoints, spawn comparisons, and/or curve-validation provenance; it does not itself erase the placed tiles. Subsequent client QA can also lose historical-fixture coverage because [MawShallowTraversalLab.Historical, lines 57–74](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawShallowTraversalLab.cs:57>) reads these systems' in-memory bounds. The ground profile can be recomputed on a later single-player update, but that is a new snapshot of current terrain, not preservation of the old snapshot ([lines 35–51](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Backgrounds/WastesGroundProfileSystem.cs:35>)). Actual input-tag presence remains unmeasured.

## 2. Several newer fixture records should survive, with two qualifications

These hooks save by exact world name plus existing state, without requiring a local player or single-player mode:

- [MawNaturalLab, lines 403–414](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawNaturalLab.cs:403>): `mawNaturalFixtureV1` and inherited `MawPlayableLab`'s `mawPlayableFixtureV1` (key selection at line 21). Only `FinishSand()` is gated by `IsQa`.
- [MawFiberRibLab, lines 192–203](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawFiberRibLab.cs:192>): `mawFiberRibStudyV1`.
- [MawAnatomyLab, lines 146–155](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawAnatomyLab.cs:146>): `mawAnatomyStudyV1`.
- [MawHangingFiberStudy, lines 315–324](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawHangingFiberStudy.cs:315>): `mawHangingFiberV1`.
- [MawAmberLightStudy, lines 114–117](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawAmberLightStudy.cs:114>): `mawAmberStudyV1`.
- [MawShallowTraversalLab, lines 355–369](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawShallowTraversalLab.cs:355>) and [MawRibContourStudy, lines 156–168](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawRibContourStudy.cs:156>): `mawShallowTraversalV1` and `mawRibContourStudyV1`.

First, these loaders and savers compare `Main.ActiveWorldFileData.Name` to `Apogee Native Visual V3`. Changing the world's internal name makes them skip existing data too. A different pathname alone is not the condition being tested.

Second, shallow/contour saving recalculates the current fixture fingerprint while retaining its original creation digest. The fingerprint uses resolved tile/wall names, geometry, coatings, wires, liquids, and important frames ([implementation, lines 76–95](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawShallowTraversalLab.cs:76>)). A save under a package missing candidate content can therefore record placeholder-state fingerprints. This is a conditional provenance risk, not proof that either copied fixture has already changed.

## 3. Maw/Anatomy texture requests: no demonstrated startup failure

At this HEAD, [MawTerrainStudies, lines 23–33, 59–83, 95–107](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawTerrainStudies.cs:23>) registers study tiles/walls and loads CPU `.bin` maps. [MawAnatomyMaterials, lines 12–20 and 39–58](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawAnatomyMaterials.cs:12>) conditionally registers rib/cap/fiber content and loads CPU maps. [MawAmberMaterials, lines 14–17, 28 and 52](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawAmberMaterials.cs:14>) does the same for emission data. None of these startup hooks dereferences a requested texture's `.Value` or dimensions.

Framework setup does request the declared textures on tiles and walls, then runs their static defaults. [ModTile.cs, lines 123–127](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModTile.cs#L123), [ModWall.cs, lines 67–73](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModWall.cs#L67). This is expected: the server asset repository forces requests to `DoNotLoad` when readers are absent, including requests originally specifying immediate loading. Holding an asset handle therefore does not imply GPU allocation or failure. [AssetRepository patch, lines 176–209 and 253–255](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/ReLogic/Content/AssetRepository.cs.patch#L176).

The direct cave-art request and dimension check are inside an explicitly single-player, packed-QA command gate ([MawCaveBackdropProbe, lines 37–56](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawCaveBackdropProbe.cs:37>)). Normal startup does not reach them. Other inspected visual startup paths explicitly guard `Main.dedServ`: [WastesGrass.Load, lines 90–96](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Tiles/WorldTerrainTiles.cs:90>), [HD background loader, lines 30–33](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Backgrounds/HighDefinitionSurfaceBackgroundRenderer.cs:30>), [RuinedUnderworldSky registration, lines 28–31](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Backgrounds/RuinedUnderworldSky.cs:28>), and [DialogueSystem.Load, lines 26–32](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Dialogue/DialogueSystem.cs:26>).

**Concrete conditional startup failure:** the study system unconditionally reads all twelve tile/wall map pairs. Missing or malformed binary data can fail mod loading even headlessly; the parsers enforce magic, dimensions, lengths, and indexes ([PackedMaterialMap, lines 12–29](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/PackedMaterialMap.cs:12>), [PackedEmissionMap, lines 12–19](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/PackedEmissionMap.cs:12>)). All 24 study `.bin` paths exist in the inspected checkout; their packed-package contents were not validated. Anatomy gates on `rib.bin` before also reading `cap.bin`; amber gates on the tile emission binary before also reading the wall binary. Partial candidate packages can therefore enter registration and then fail. This is package-dependent, not an established defect in the chosen smoke package.

Content identity also depends on [the packed-QA compile switch](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawPackedPreview.cs:14>) and package file gates such as [MawToothClusterTile, lines 23–28](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Tiles/MawToothClusterTile.cs:23>). tModLoader remaps saved tile/wall identities by mod and name, using placeholders for unavailable content and retaining positional identity for later saves. Missing content alone is therefore not proof of permanent tile deletion, but it changes the loaded representation and can affect the fixture fingerprints above. [TileIO_Basic.cs, lines 45–108 and 130–158](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/IO/TileIO_Basic.cs#L45).

## 4. Main.LocalPlayer: a reachable assumption, not a proven crash

[MawShallowTraversalLab.IsQa, lines 29–30](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawShallowTraversalLab.cs:29>) passes `Main.LocalPlayer.name` into `Context` as an argument. C# evaluates that expression before [Context's single-player check, lines 8–9](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawShallowQaScope.cs:8>). Consequently both [shallow PostUpdateEverything, lines 342–344](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawShallowTraversalLab.cs:342>) and [contour PostUpdateEverything, lines 145–147](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawRibContourStudy.cs:145>) read the server player slot before returning. This differs from the short-circuit guards in the eight save-gated labs.

The official source defines `LocalPlayer` as `player[myPlayer]` and documents the last player slot as the server dummy. [Main patch, lines 781–793 and 1099–1105](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L781). Content loading initializes `Main.player[255] = new Player()`. [ModContent.cs, line 354](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModContent.cs#L354). A normal post-load server update should therefore evaluate false without throwing. Calling the predicate in an incompletely initialized environment could fail; no such failure was observed here.

`MawShallowTraversalLab.Release` also calls `Main.LocalPlayer.GetModPlayer` before checking whether a visit exists ([lines 335–340](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawShallowTraversalLab.cs:335>)). It is not that system's unload hook; `ClearWorld` only resets fields. The inspected live-request dispatcher rejects server mode before dispatch ([TileLabPlayer, lines 113–117](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/TileLabPlayer.cs:113>)). Do not report this as an automatic server-unload crash without a triggering call stack.

## 5. Core state and unload behavior

The framework calls `OnWorldLoad` before loading mod world data, then `PostWorldLoad` afterward. [WorldFile patch, lines 89–99](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/IO/WorldFile.cs.patch#L89). Thus the inspected core systems' initialization clears do not erase data after deserialization. Their save/load hooks have no local-player gate:

| Core data | Local save/load evidence |
| --- | --- |
| World plan and rebuilt protections | [ApogeanWorldPlanSystem, lines 190–200](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Common/WorldGeneration/ApogeanWorldPlanSystem.cs:190>) |
| Maw node positions | [EngraftSystem, lines 205–222](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/World/EngraftSystem.cs:205>) |
| Corporate campus/door rectangles | [CompoundGen, lines 156–197](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Structures/CompoundGen.cs:156>) |
| Progression, alliance, quotas, arrival stage | [FactionProgression, lines 164–216](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Factions/FactionProgression.cs:164>) |
| Arrival-site historical record | [ArrivalSiteSystem, lines 207–222](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Common/WorldGeneration/ArrivalSiteSystem.cs:207>) |
| Background variant array | [RuinedBackgroundSelectionSystem, lines 101–106](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Backgrounds/RuinedBackgroundSelectionSystem.cs:101>) |

No server-only omission was found in those paths. This is not a malformed-save or whole-mod compatibility certification. If world updates run, ordinary server-owned behavior can change state: [EngraftSystem, lines 98–106 and 157–161](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/World/EngraftSystem.cs:98>) removes invalid nodes and performs timed spread, while [FactionProgression, lines 143–161](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Factions/FactionProgression.cs:143>) advances progression. Whether the main task's no-client smoke ticks the world is outside this investigation.

Inspected diagnostic unload paths are inert in a fresh headless session: [cave probe, lines 200–206](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawCaveBackdropProbe.cs:200>) releases only an active probe; [performance recorder, lines 244–245](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/QAPerformanceLab.cs:244>) exports only an active recording. Their activation gates require single-player QA. Framework `OnWorldUnload` logs hook exceptions and continues, so a clean exit alone would not establish error-free unload. [SystemLoader.cs, lines 180–190](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/SystemLoader.cs#L180).

## Implication for the main task's disposable smoke

Evaluate successful startup, world load, save completion, and preservation separately. A zero-error server run can still omit the ten QA records above. Compare their input/output tag presence, retained core state, and candidate identities; account separately for shallow/contour fingerprint changes. The `.twld` is essential: the loader returns without custom data if it is absent, and saving writes a new companion file. [WorldIO.cs, lines 26–72](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/IO/WorldIO.cs#L26). Keep the server-written result disposable; it is not established as a lossless replacement for the client QA world.

The older [V3 validation note, lines 3–22](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/VALIDATION_NATIVE_WORLD_V3.md:3>) reports an earlier successful server load/save. That is historical evidence, not a rerun or proof that today's diagnostic tags survive. No observed failure or new pass is claimed by this note.
