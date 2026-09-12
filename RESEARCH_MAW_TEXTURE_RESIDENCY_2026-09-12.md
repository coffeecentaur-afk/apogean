# Maw texture residency research — 2026-09-12

Research sidecar only. No assets, production code, tools, skills, packages, worlds or settings were changed. No install, build, game launch/control, runtime measurement, full-log read, private-file read, nested agent, commit or push occurred. The main agent owns all builds and the native vine reproduction. This report proposes subsequent work; none of its runtime comparisons has run.

## Conclusions

1. **Verified loading mechanisms:** tML automatically requests assets whose path contains `Backgrounds/`. Apogean also unconditionally registers its 12 material study tile/wall pairs. The saved inventory assigns these disjoint groups **421.0 MiB** and **245.8 MiB** of RGBA8 payload respectively. Being unused in the current scene, called a “candidate,” or behind a drawing condition does not prevent these startup requests. These figures describe exposed payload, not removable bytes or measured VRAM.
2. **Verified duplication opportunity:** ReLogic caches by cleaned request path, not file hash. Redirecting both grass-wall consumers to the existing soil-wall path is a small, concrete candidate for eliminating **one 13.265 MiB base texture**, while retaining every content type and its map. Amber sibling types already share paths correctly. The inventory's **78.7 MiB** total is an upper bound on exact-file duplication opportunity, not guaranteed savings.
3. **Verified allocation behavior, unresolved attribution:** packed atlases bake 64 world-position phases per unique native mask. Native paint can then allocate full-size atlas render targets per type/style/color. In contrast, the installed raw-image reader clears its managed pixel-array reference after upload, and the PNG reader frees its native decoded image on successful upload. The reported **1.7 GB** does not prove persistent CPU/GPU copies: tML adds managed-memory attribution estimates to a separate texture-size estimate.

## Scope and evidence pins

Local paths below are relative to:

`C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean`

Read first: `Art/Validation/MawAnatomy-2026-09-11/README.md`, its `texture-inventory-ca03.json`, and `Tools/Measure-QATextureInventory.ps1`. The inventory tool was **not run**. Numbers below only regroup or perform arithmetic on that existing JSON; no PNG headers, pixels or file hashes were remeasured.

The inventory was captured at `2026-09-12T00:59:51.6304208Z`. It records 444 PNG paths, 348 unique file hashes, 799,949,764 hypothetical per-path RGBA8 bytes (762.9 MiB), and 82,546,304 excess bytes across identical-file groups (78.7 MiB). It excludes mipmaps, simultaneous residency and runtime allocations. The README attributes 1.7 GB to the **whole QA mod**, not the anatomy additions. Its CA03 package pin is `CA03D9D09BD3E92A7980B3CF83ECB4F95AB9A0E59E8D5A65389F6B3EE35593C3`.

Current source HEAD inspected: `8a0ec8a583706361c7926ea8f3d1259f61223939`. A scoped status check showed no modifications to the principal loading/compiler files cited below at inspection time. This is a current-source audit paired with historical inventory; it is **not** verification that every current method was compiled into CA03.

Installed assembly root: `E:/SteamLibrary/steamapps/common/tModLoader/`. The following binaries were inspected as data through the already installed `Libraries/mono.cecil/0.11.6/lib/netstandard2.0/Mono.Cecil.dll`; their game code was not executed.

| Evidence ID | Exact installed path below root | Version / SHA-256 |
|---|---|---|
| TML | `tModLoader.dll` | tML `2026.07.3.0`, Terraria `1.4.4.9`; `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C` |
| RL | `Libraries/ReLogic/1.0.0/ReLogic.dll` | `1.0.0.0+666f69962d3bdffde54fc14025f02634965b4e7c`; `0E4528450CFA6F62FF368D491D75C36A929CEBDC053BDA9B7CD70A8C0A6FED92` |
| FNA | `Libraries/FNA/1.0.0/FNA.dll` | `23.10.0.0`; `DEA46C76655A62B8C205EC13AC2803DDED21683F549B88B80CB41C38DA9F4F1B` |

TML's product version identifies commit `666f69962d3bdffde54fc14025f02634965b4e7c`. All public tML links below pin that commit. The installed native backend file `Libraries/Native/Windows/FNA3D.dll` hashes to `E1AC432BC1554B25AA895C448F853559C1C37C6F6A2BF1A22534EF0146A64134`; only its identity was inspected. Backend selection, driver allocation and actual GPU residency remain unverified. No unrelated logs were needed.

**Verified** means inspected source, installed IL, or preserved inventory. **Inference** means a consequence expected from those mechanisms, without a runtime test. **Proposed** means future work and acceptance criteria.

## 1. What actually requests the assets

### Background autoload defeats drawing-only gates

**Verified:** `apogean.cs:9–23` does not disable background autoloading. TML `Mod::.ctor`, IL `001c–0023`, defaults `BackgroundAutoloadingEnabled` to true. `Mod.Autoload()` calls `BackgroundTextureLoader.AutoloadBackgrounds` on clients. Its installed predicate `<AutoloadBackgrounds>b__11_0`, IL `0000–000b`, is simply a case-sensitive `Contains("Backgrounds/")` test on enumerated asset names. It does not check whether any biome selects that image. `AddBackgroundTexture`, IL `0034–003b`, immediately issues an `AsyncLoad` texture request; `ResizeArrays`, IL `0055–007e`, requests the same path and installs its asset in `TextureAssets.Background`. [Pinned autoload entry point][mod-internals].

The content source enumerates package names supported by the asset-reader collection. Enumeration itself is not a universal instruction to load every `Content` image; the background loader and registered content supply the requests. [Pinned TModContentSource][content-source].

| Inventory group | Paths | Existing RGBA8 MiB | Loading significance |
|---|---:|---:|---|
| `Content/Backgrounds/` | 232 | 420.956 | Entire group matches the installed autoload predicate |
| `Content/Backgrounds/Diagnostics/` | 59 | 213.270 | Included in the preceding row |
| `Content/Backgrounds/Diagnostics/HD/` | 28 | 162.008 | Included in both preceding rows; 27 biome layers plus transparent placeholder |
| `Content/Backgrounds/Candidates/` | 16 | 72.953 | Included in the first row |

Do not add nested rows together. Nor is “Diagnostics” synonymous with unused: `Content/Backgrounds/ApogeanSurfaceBackgroundStyles.cs:23–52` uses the HD renderer and its transparent native slot for supported biomes.

Additional requests exist independently of autoload:

- `Content/Backgrounds/HighDefinitionSurfaceBackgroundRenderer.cs:30–46,87–94,213–217`: an automatically loaded `ModSystem` requests Far/Mid/Close for nine biomes at mod load. Making only the draw path conditional would leave these requests intact.
- `Content/Backgrounds/WastesLandscapeV1Renderer.cs:36–72`: client `Load()` chooses city/fallback and complete ruin-bank paths. Its drawing/selection gates do not undo the earlier background autoload.
- `Content/Diagnostics/WastesRuinScaleGallery.cs:16–28`: its own requests occur on gallery draw, but packaged `Content/Backgrounds/Candidates/WastesScaleGallery/*` still match autoload before that draw.
- `Content/Backgrounds/ApogeanUndergroundBackgroundStyles.cs:9–15,35–42` looks up native background slots by path. A change disabling autoload must preserve registration for these paths, surface slot paths and the transparent placeholder.

**Inference:** separating optional direct-draw studies from native background registration can avoid loading unvisited banks. Moving them into another folder that still contains `Backgrounds/` would not work. Disabling background autoload without replacing required slot registration would break `GetBackgroundSlot` consumers.

### Material registration is unconditional even with packed preview off

**Verified:** `Content/Diagnostics/MawTerrainStudies.cs:14–22` always registers `Study_<key>` and `StudyWall_<key>` for all 12 keys. Their `[Autoload(false)]` attributes prevent automatic class discovery, not these explicit `Mod.AddContent` calls. Their `Texture` properties at lines 52 and 88 name the 24 material-bank assets; their `Load()` methods read the corresponding `.bin` maps.

TML `ModTile.SetupContent`, IL `0000–0017`, and `ModWall.SetupContent`, IL `0000–0017`, request each registered texture with `AsyncLoad` and assign the resulting asset to the native texture arrays. Thus absence of study tiles from a world does not avoid the 245.789 MiB bank's startup requests.

`Content/Diagnostics/MawPackedPreview.cs:14–21,35–39` controls production texture substitution and draw mapping. Turning its build symbol off does **not** stop `MawTerrainStudies.Load`. `build.txt:4` excludes `Art/*, Tools/*`, not diagnostic `Content` assets. This is a real default-build opportunity, subject to preserving callers and saved QA content types.

Other exact paths:

- `Content/Diagnostics/MawAnatomyMaterials.cs:12–20,45–47` registers rib/cap/fiber only with packed preview and the anatomy metadata sentinel. The three CA03 PNGs total 37.976 MiB. They are not the entire 1.7 GB concern.
- `Content/Diagnostics/MawStoneMaterialCandidate.cs:12–14` has no loading gate; its 2304×2160 texture represents 18.984 MiB even if no proof specimen is placed.
- `Tools/Build-ApogeanIsolated.ps1:272–299` copies validated anatomy art/maps into the build mirror and adds emission tables. The existing accepted snapshot is checked before these additions. A proposed lean experiment must be a separate manifest/profile, not a redefinition of that preserved baseline.

**Verified timing:** `ModContent.Request<T>` defaults to `AsyncLoad` (RL enum value 2); asynchronous means scheduling now, not loading only when drawn. tML calls `mod.TransferAllAssets()` after post-setup, then `MemoryTracking.Finish()`. [Pinned load sequence][mod-content].

## 2. Sharing identical bytes requires sharing the request key

**Verified:** RL `AssetRepository.Request<T>(string, mode)`, IL `0011–0055`, cleans the path, looks it up in `_assets : Dictionary<string,IAsset>`, and creates a new `Asset<T>` for a missing key. Neither this lookup nor the inspected readers performs content-hash interning. Repeating one canonical path shares its asset; two distinct paths with identical file bytes do not become one asset automatically.

Concrete first target, from the existing inventory:

`Content/Diagnostics/Materials/grass/Wall.png`

`Content/Diagnostics/Materials/soil/Wall.png`

Both are 2176×1598, SHA-256 `4B0F04EAA4B1CB5A054ABC2362F086D2775B67FCF1CBAD11F35C30DC24CCFB1C`. One extra image is 13,908,992 bytes, or **13.264648 MiB**.

**Proposed minimal fix:** make `MawTerrainStudyWall.Texture` and `MawPackedPreview.Texture(key, wall, fallback)` canonicalize grass-wall texture requests to `.../soil/Wall`. Retain the distinct tile/wall types and their metadata/drawing semantics. Check every texture consumer; redirecting only one leaves the other key requested. The unused PNG may remain packaged initially because this directory is not generically autoloaded as backgrounds. Verify the two consumers' frame-coordinate contract before adopting the alias. `Tools/New-PackedMawMaterials.ps1:25–35` already selects soil material and DirtUnsafe reference for both wall outputs, explaining their identical bytes.

Other duplicate groups include diagnostic/ordinary V0 biome backgrounds (e.g. `Content/Backgrounds/Diagnostics/ForestConceptV0_Mid.png` and `Content/Backgrounds/Forest/V0_Mid.png`, 2.344 MiB per extra copy). These are both autoload candidates. Redirecting an explicit renderer request while leaving both paths autoloaded would not eliminate the duplicate allocation. The same rule applies to aliases implemented only at stream-opening time: different repository keys still produce different assets.

**Already correct:** `Content/Diagnostics/MawAmberMaterials.cs:26,50` uses the existing amber Tile/Wall paths. Packed production texture substitutions likewise point at the study bank. Multiple content IDs are not by themselves duplicate base textures. Native painted variants may still be separate because their keys include type identity.

The 78.7 MiB figure is exact-file duplication only. It neither proves all those keys are loaded nor discovers PNGs with equal decoded pixels but different encodings. Savings from deduplication and excluding a bank overlap; do not add both estimates for the same images.

## 3. CPU storage, GPU storage and the 1.7 GB estimate

**Verified:** `MemoryTracking.Update` adds positive `GC.GetTotalMemory(accurate)` deltas to a mod's managed estimate. Negative deltas do not subtract earlier attribution. `Finish` separately sums loaded texture dimensions as width × height × 4; reported total also includes code and sounds. This is not a per-mod working-set or GPU-residency query. [Pinned accounting implementation][memory]; matching TML `Update`, IL `0021–0048`, and `Finish`, texture assignment at `00ad`.

**Inference:** temporary allocations and GC timing can affect the startup attribution; its managed and texture terms cannot establish two persistent copies of each image. The original component values were not preserved here, so the remaining difference cannot be assigned numerically to decoding, code, maps or driver memory.

The actual upload paths narrow the opportunities:

| Stage | Verified behavior | Consequence / uncertainty |
|---|---|---|
| Packing | `ContentConverters.Convert` normally converts PNGs except `icon.png` to `.rawimg`; `ImageIO.ToRaw` writes dimensions and RGBA bytes. | Source PNG compression/palette changes alone do not reduce runtime texture dimensions. Actual CA03 package reader selection was not re-inventoried. [Converter][converter], [ImageIO][image-io] |
| Raw image loading | `ImageIO.ReadRaw` allocates width × height × 4 managed bytes; `RawImgReader` awaits main-thread upload and calls `Texture2D.SetData`. Its installed async state machine clears `<data>5__2` on success at IL `0102–0104` and failure at `00e4–00e6`. | Temporary whole-image CPU payload is verified; a permanent array per texture is not. GC can reclaim unreferenced arrays later. [RawImgReader][raw-reader] |
| PNG loading | Installed RL `PngReader/<FromStream>d__5.MoveNext`, IL `0035–0101`, decodes to a native pointer, uploads it and calls `FNA3D_Image_Free` on success. | The old `_colorProcessingCache` field exists, but this installed successful path does not populate a retained `Color[]` cache. [Pinned NETCORE reader][png-reader] |
| FNA texture | Three-argument constructor passes `mipMap=false, SurfaceFormat.Color`; full constructor creates one level through `FNA3D_CreateTexture2D`. `SetData`, IL `00d7–0110`, pins the caller's array for upload then frees the handle. Texture/Texture2D fields contain native handle, format/levels and dimensions, not a backing pixel array. | Do not assume automatic mipmaps, a managed shadow copy or power-of-two expansion. Native/driver allocation and alignment are still unmeasured. |
| Package cache | `TmodFile.CacheFiles` caches stored entry bytes only when compressed length is at most 128 KiB; compressed entries are read through a DeflateStream. | It is not an unconditional decoded copy of the whole mod. Actual cached entries require measurement. [Pinned TmodFile][tmod-file] |
| Apogean metadata | `PackedMaterialMap.cs:9–30` stores dimensions plus `int[Columns*Rows]`, not RGBA pixels. | Keep map storage distinct from image payload. |

**Verified diagnostic CPU allocations:** `MawMaterialJoinChecks.cs:34–41`, `MawRibJoinChecks.cs:29–40`, `MawMergeCoverageChecks.cs:28–32` and `MawNaturalLab.cs:155–157` allocate full-image `Color[]` readbacks in method-local dictionaries. The first, third and natural-lab routines explicitly clear their caches; the rib cache is also local. This establishes transient probe cost, not a persistent startup texture cache. Retention beyond a completed invocation remains a runtime question.

**Proposed small diagnostic improvement:** if measured probe peaks matter, batch the exact required frame rectangles or retain a compact alpha representation per invocation instead of keeping full RGBA arrays for every atlas. Preserve the current alpha oracle and mutation controls. Do not substitute a compiler self-check for independent native readback.

Nulling Apogean's `Asset<Texture2D>` fields would not by itself unload tracked assets: RL's repository retains its dictionary, and its disposal path owns the loaded values. Do not manually dispose a texture shared by native arrays or amber/production/study users. There is no evidence here justifying an engine patch to drop an assumed permanent CPU copy.

## 4. Atlas expansion and native paint amplification

**Verified:** `Tools/PackedMaterialCompiler.cs.txt:70–99` deduplicates native role masks, then emits **64 phase slots for each mask**. `PackedMaterialMap.cs:33–43` selects the slot using `i % 8 + (j % 8)*8`. This bakes a world-aligned 128×128 material field into native-compatible frame images; it is not a runtime tML atlas expansion.

Ordinary tile slots are 18×18 including guard space, walls 34×34 for a 32-pixel overlapping body. Grass/cap preserve more role-mask combinations. The saved dimensions imply 405 mask rows for each 1152×7290 image, making grass Tile and anatomy cap **32.036 MiB each**. A typical 1152×1350 tile atlas is 5.933 MiB. Transparent pixels and small palettes do not remove texels from these Color textures.

**Proposed later, lossless optimization:** deduplicate identical final rendered slots, not only semantic masks, with an explicit replacement mapping and exhaustive frame/phase/guard equivalence. A particularly narrow candidate is anatomy cap: `Tools/New-MawAnatomyCandidate.ps1:21–30` passes the same fiber field as both material and overlay, so some distinct soil/foliage role masks may become identical pixels. The opportunity is inferred, unquantified, and requires a versioned mapping/compiler change. Current mask deduplication already exists; simply “deduplicate masks” is not a new fix.

Reducing phase count, downscaling, deleting native masks, trimming guard space, shader composition or texture compression are larger behavior/rendering changes. None is the first minimal fix for this task.

**Verified additional native allocations:** TML `TilePaintSystemV2/TileRenderTargetHolder.Prepare`, IL `006b–009e`, and `WallRenderTargetHolder.Prepare`, IL `0000–0022`, pass the underlying tile/wall atlas with no source crop. `ARenderTargetHolder.PrepareTextureIfNecessary`, IL `001e–0072`, selects the complete frame and allocates a same-dimension `RenderTarget2D`, with no mipmaps and the backbuffer format. Tile keys contain `TileType`, `TileStyle`, `PaintColor`; wall keys contain `WallType`, `PaintColor`. `Reset()` clears holders/dictionaries; `ARenderTargetHolder.Clear`, IL `0000–0020`, disposes its target.

**Inference:** one painted 7290-pixel-tall atlas can add approximately another 32.036 MiB of color payload when the target format is Color. Different paint/type keys can add more, even where base assets share a canonical path. Actual holder counts, target formats, reset timing and driver bytes must be measured. This is a visit/paint cost, not evidence explaining the earlier menu startup number. Apogean directly participates through `MawTerrainStudies.cs:99–101` and `MawPackedPreview.cs:45–46` wall-paint requests.

## 5. Safe implementation order for the main agent

These are proposals, not changes made by this sidecar.

| Priority | Minimal change | Required boundary and expected effect |
|---|---|---|
| 1 | Canonical grass-wall texture path in the two accessors identified above | Preserve type names, frames, maps and saved fixtures. Expected one fewer base Texture2D, approximately 13.265 MiB logical payload; paint targets can remain distinct. |
| 2 | Add an explicit lean study profile to the isolated-package workflow | Gate unused study registration and all callers together; omit only a declared set of unreachable assets. Merely turning packed preview off does not stop material-study loading. Do not open the preserved fixture with missing content or rewrite its 466-asset baseline. |
| 3 | Separate direct-draw candidate/HD image loading from native background slots | Either relocate direct-only assets to paths without `Backgrounds/`, updating every consumer, or disable autoload and explicitly register the full required native-slot set during loading. Also replace eager HD set construction with a supported-biome manifest and per-needed-set requests; retain both sets during fades. Folder relocation alone does not fix explicit eager requests. |
| 4 | Bound diagnostic readback buffers if their peak is material | Measure separately from normal play. Preserve independent pixel evidence. |
| 5 | Investigate lossless final-slot packing, starting with cap | Version map/compiler together and retain all existing native mask, guard, wall-overlap and negative controls. No claimed byte saving before comparison. |

Start the lean profile with a small, demonstrably unused bank, not deletion of the entire 421/246 MiB groups. Background HD assets are active renderers, material maps are used by packed production types, and diagnostic worlds refer to study types. Lazy requests can reduce startup occupancy but tracked textures remain held after first use; a general eviction scheme is a separate design problem.

## 6. Proposed runtime measurement contract

**Owner: main agent. Status: not implemented or run.** Use its already authorized isolated QA workflow. This report authorizes no new world changes and imposes no dependency on completing the vine reproduction.

### Record and sample without causing the load being measured

Record package SHA-256, source revision/profile, the assembly pins above, enabled mods/versions, graphics backend/adapter/driver, resource packs, window dimensions/zoom, GC mode and exact phase. Use identical settings and actions for each comparison. Produce a bounded machine-readable texture record, not a full log or heap dump.

At each phase record:

- Repository asset name, source identity and actual source extension, `State`, `IsLoaded`, `IsDisposed`, pending count, dimensions, `Format`, `LevelCount`; assign object identity IDs for both Asset and Texture2D. Join names to the existing inventory hashes, labeling unmatched/new paths. Inspect only loaded values and do not `Request`, `Wait`, `GetData`, render a new scene or export screenshots inside the passive sampler.
- Report per-key logical bytes **and** bytes/count by distinct Texture2D reference. Do not count native `TextureAssets.Tile/Wall/Background` aliases again. Use a scoped identity map, release references after sampling, and retain only scalar records. For non-Color/mipped assets calculate their format/level payload separately; never call it measured VRAM.
- Process private bytes and working set; `GC.GetTotalMemory(false)`, GC heap/committed/fragmented bytes and collection counts; tML's managed/textures/code/sounds attribution as a separate estimate. Record a separately labeled post-GC diagnostic sample where useful, with the same protocol in each arm. Do not make forced GC a production fix.
- Native paint holder count, exact type/style/color key, prepared/disposed state, target dimensions/format/levels and distinct target identity. Separate these targets from mod repository textures. Obtain process GPU dedicated/shared usage from an available established counter, recording provider and scope; if unavailable, say so and report only logical allocation estimates.

**Sampler trap verified in installed RL:** despite its name, `AssetRepository.GetLoadedAssets`, IL `0011–001c`, returns all dictionary values. Filter `IsLoaded`; otherwise `Asset<T>.Value` returns its default value for an unloaded asset (`get_Value`, IL `0000–0014`). Never turn placeholder/default textures into evidence of residency.

Suggested phases: process/dependency baseline; mod-load peak; settled menu after the engine's existing transfer completion and `PendingAssets == 0`; first controlled scene before readback probes; after ordinary unpainted drawing; after a separately controlled paint comparison; immediately after existing native pixel probes and after collection; return to menu; optional normal mod unload/reload. Sample a stable interval rather than a single instantaneous reading. Use at least three independent process runs per arm, alternate order, report median/range and peaks. Treat process-wide deltas as comparisons, not exact Apogean ownership.

### Comparisons and deliberate controls

| Comparison | Expected discriminating result |
|---|---|
| Same package, same actions, repeated | Establishes baseline noise; identical named texture object counts should stabilize. |
| Dependencies alone vs full QA, menu only | Separates whole-mod startup delta from engine/dependency baseline; cannot isolate anatomy. |
| Same QA profile with anatomy additions absent vs present, menu only | Isolates that bank while preserving the 24-material base. Never load/save the preserved anatomy world in the absent-content arm. |
| Grass/soil wall original keys vs canonical key | Exactly one fewer loaded base Texture2D; both users reference the canonical object. Compare identical rendered frames and native paint/coating behavior. Do not require two paint/type holders to become one. |
| Repeat request to the same path vs two known identical existing paths | Positive cache-sharing control and negative hash-dedup control. First must preserve object identity/count; second must remain distinct in the original package. |
| Hide/skip drawing a packaged background vs exclude its autoload registration/path and all explicit requests | Drawing-only negative control should leave startup base textures present. Effective loading change must make the selected unused keys absent, not merely invisible. |
| Packed preview off with present unconditionally registered studies vs properly gated lean study profile | First should still load the study bank; second should omit only the manifest's declared textures. Run menu-only when types differ. |
| Smaller/recompressed PNG of unchanged dimensions/pixels vs original | Negative runtime-payload control: after normal conversion/upload, base format/dimensions should be unchanged. Package compression is not a residency result. |
| Unpainted vs one paint key, repeat same key, then another paint/type key | One new full-atlas target where requested; repeated identical key should reuse it; a different key can add a target. Distinguishes paint growth from base-path duplication. |
| Ordinary draw vs readback probe, then separately post-GC | Tests transient CPU arrays: repository base texture count should not grow merely from readback; retained process/GC bytes need explanation if they do not settle. |
| Future slot packing vs original, with deliberately corrupted mapping/guard input | All native frame/phase pixel comparisons must agree; the corrupted control must fail. Logical and live texture dimensions must actually shrink. |

Acceptance for a minimal optimization requires all three: the intended request/object-count difference, matching rendering/content behavior in the main agent's scoped fixture, and a repeatable memory or peak-allocation improvement exceeding baseline noise. Preserve regressions as failed evidence. Do not claim that subtracting inventory rows from 1.7 GB predicts the outcome, or add overlapping bank/dedup/paint savings.

Remaining unknowns are deliberately bounded: actual CA03 loaded keys/reader extensions, post-GC retained CPU bytes, paint target residency, selected native backend behavior and driver/GPU usage. Static evidence establishes where to measure; it does not replace those measurements.

## Primary source references

Installed IL citations above identify exact pinned assembly, type, method and relevant offsets. Public patches can omit unchanged Terraria/ReLogic code; the installed IL is the evidence for the background predicate, FNA constructors, paint target allocation and async buffer clearing.

[mod-internals]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/Mod.Internals.cs#L80-L120
[content-source]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/Assets/TModContentSource.cs#L14-L30
[mod-content]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModContent.cs#L340-L347
[memory]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/Core/MemoryTracking.cs#L11-L83
[converter]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/Core/ContentConverters.cs#L9-L21
[image-io]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/IO/ImageIO.cs#L11-L60
[raw-reader]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/Assets/Readers/RawImgReader.cs#L21-L33
[png-reader]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/TerrariaNetCore/ReLogic/Content/Readers/PngReader.cs.patch
[tmod-file]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/Core/TmodFile.cs#L387-L410
