# Authoring tools and dependency strategy — September 9, 2026

Status: researched recommendations and backlog, **not implemented tooling or approved dependencies**. No runtime/art changes, installs, builds, world edits, performance benchmarks or boss-video viewing in this pass. The current slice remains the Maw's easily mined, solid tooth hazards, followed by the approved shallow entrance.

Companion: `RESEARCH_NAMED_LIBRARIES_2026-09-09.md` investigates the libraries/mods named by the user. Existing creative direction remains in `BACKGROUND_BIOME_DIRECTION.md`, `DESIGN_BIBLE.md` and `AUTHORING_WORKFLOW.md`.

## Recommendation

Keep one internally modular Apogean content mod for now. Improve the tools that connect approved art to the exact build and native evidence before adding another framework. Separate distributable mods are justified by a concrete reusable feature, not by the size of the overall ambition. Extracting code into another package does not by itself solve engine limits, rendering cost or multiplayer correctness; those require measurement and explicit contracts.

Distinguish three things:

- **Development tools:** mask/export utilities, atlas previews, validators and test dashboards. Players do not need these.
- **Runtime libraries:** code the shipped mod calls. These may require an installed dependency and supported version range.
- **Other gameplay mods:** optional content or combat changes, not automatically infrastructure for Apogean.

tModLoader supports required `modReferences`, optional `weakReferences`, and separate non-mod `dllReferences`. An optional integration still needs guarded loading and testing; calling another mod's API without declaring/handling the relationship is not safe. [tModLoader build.txt documentation](https://github.com/tModLoader/tModLoader/wiki/build.txt)

## What the repository already has

Read-only audit at base commit `487f83f4caf2cc4e49a813bdd94c7190a953ee68`:

- `build.txt` declares no third-party mod or DLL references; `apogean.csproj` imports tModLoader's build targets with no additional library references. There is no current missing-library evidence.
- `Tools/` contains 162 files. We already have explicit-mask background export, native atlas references, terrain controls, authoring status, isolated builds and multiple native galleries. Another standalone editor is not the first need.
- `Tools/Build-ApogeanIsolated.ps1` accepts numerous separate candidate overrides. Accepted QA art is not all in production `Content/`. Forgetting an option can omit accepted artwork; a clean bare build is not proof of the accepted visual package. This is an observed workflow risk, not a new reproduced regression.
- `Tools/Invoke-ApogeanContentGate.ps1` groups family validators, but its build path does not itself establish the full accepted candidate composition. `Tools/AuthoringStatus.json` separates evidence states for 41 families.
- `.apstructure` templates already have size/entrance/surface metadata and authored fill, wall, platform, framing and object commands. The current stamper frames the shell before placing objects. WorldAtlasPlanner separately consults StructureMap and reserves locations. A new building tool should preserve that ownership.
- The boss specification validator exists, but `Tools/Test-BossAuthoringPipeline.ps1` exercises an example specification, not deliberate invalid inputs or the actual Matriarch encounter. Its success message is not evidence of negative-test coverage, balanced combat or multiplayer safety.

## Highest-value improvements, in order

These are proposals. Each must earn its place with one bounded fixture; do not pause content work to build an entire tool platform.

| Tool improvement | Smallest useful result | Where it runs |
|---|---|---|
| Accepted asset/build manifest | One versioned profile resolves all accepted overrides, records source/mask/export hashes and fails on missing files. A package check verifies the expected runtime texture paths and bytes. | Development only |
| Tooth shape/hazard inspector | Show one fang at player scale with its visible outline, occupied solid/sloped cells, support, contact-damage region and mining target. Test four orientations. | Opt-in native QA |
| Unified QA entrypoint | Wrap existing validators/galleries with candidate and package IDs, allowlisted world, fresh request acknowledgement, results and captures. Do not duplicate existing scripts or hide red checks. | Development/QA only |
| Furniture assembly preview | One reusable room previews frame padding, origins, support, direction and animation; native placement/break/drop/reload confirms the preview. | Development plus native fixture |
| Biome scene records and budget checks | Bind each layer to its own art brief, depth, spacing, anchor, state overlays and runtime path; inspect routing, coverage, repetition and measured cost. | Development plus native fixture |
| Encounter lab, later | Real encounter specification, bad-input tests and one original attack with telemetry/replay controls; then SP/MP validation. | Opt-in QA; content remains in Apogean |

### Asset manifest: prevent the most expensive handoff error

The manifest should name the family, native frame contract, source and reviewed mask, palette/pixel scale, runtime destination, anchor/animation metadata, candidate hash and review status. A build profile selects compatible candidates and produces a package receipt. Reject duplicate destinations, missing assets and hash mismatches. First acceptance test: rebuild the currently accepted QA package without a long manually reconstructed override list and verify unchanged accepted textures. Do not silently promote QA assets to production as part of this tool.

### Shapes, tiles and furniture are different contracts

An attractive transparent PNG does not automatically acquire shape-accurate collision. Render masks, solid/sloped tile occupancy, anchoring and contact damage must be checked separately. For the next tooth fixture, occupied cells should closely follow the silhouette; empty space should not be an invisible hazard. Test starter-pick removal, support loss, actual player hurt/cooldown, rope clearance and reload. Native spike behavior is a reference to inspect, not permission to guess its atlas from the connected-bone sheet.

Keep native tile, grass, wall and furniture layouts distinct. Reuse source/mask/provenance handling, but never force all families into one atlas size. Furniture previews must include every orientation and animation frame, with native testing for anchors, placement failure, correct drops, paint/actuation where supported, save/reload and multiplayer when interactive. Preview art alone cannot certify these behaviors.

## Many biomes, one reliable process—not one repeated background

The established camera, mask and evidence pipeline is reusable. The scenery is not. Each biome needs a short visual identity record covering silhouette, open space, materials, ecology, architecture, lighting, vertical rhythm and landmark cadence. Review it beside the existing biomes before expansion.

| Family | Distinct visual direction already agreed |
|---|---|
| Wastes | Dead roadside frontier; Kessler-era industry, gas stations, damaged highways and city remains. Reclamation grows over the same landmarks. |
| Jungle | Living green survivor: dense vegetation swallowing abandoned research structures, not another brown wasteland. |
| Maw | Raised feeding wound, tooth-lined descent, stringy grey/black/brown tissue, amber organs and localized deeper mutations. Organic dormancy, not green purification. |
| Snow / desert | Shared transport history but different forms: frozen bridge spans and relays versus buried roads, eroded supports and open dunes. |
| Sky / space | Sentrix-era elevated docks, spires and orbital infrastructure; not ground-city silhouettes stretched upward. |
| Other infections / underground families | Preserve recognizable local ecology and native biome distinctions. Research ruins can share Helix history without making Crimson, Corruption, Hallow, mushrooms and caves identical. |

Use biome-specific palettes, silhouettes and spacing within a shared pixel scale. Test the actual biome selector, day/night/events, cross-biome edges, horizontal repeats, diagonal travel, high flight and underground handoff. Reclamation/dormancy overlays preserve geometry and anchors. Higher texture resolution alone is not higher quality; compare native apparent pixel size and measure memory/draw cost before increasing it.

## Library candidates and their adoption gates

### StructureHelper — investigate when furnishing authored bases

Its documented workflow authors structures in-game using wands, saves files, then generates them through its API. This is directly relevant to beautiful fixed buildings. Normal API use introduces a runtime dependency; calling it a development-only tool would be misleading unless we explicitly export into our own format. [StructureHelper authoring guide](https://github.com/ScalarVector1/StructureHelper/wiki/Getting-Started)

Gate: round-trip one existing furnished room, including preserve-versus-clear space, furniture origins, chests/tile entities, wiring, paint and actuation as applicable. Check failed placement, reload and multiplayer. Retain Apogean's finite location planner and protected reservations. Licensing/reuse permissions must be checked before copying or bundling code; this pass does not establish permission to transplant it. [tModLoader StructureMap API](https://docs.tmodloader.net/docs/stable/class_structure_map.html)

### SubworldLibrary — credible future planet candidate

Its API provides separate subworld dimensions, generation tasks and lifecycle hooks. `ShouldSave` defaults false, so persistent destinations need explicit save behavior. The author documents multiplayer subservers and subworld saves tied to their main world. This enables a plausible planet prototype, not a promise of unlimited memory or automatic compatibility. [SubworldLibrary API](https://github.com/jjohnsnaill/SubworldLibrary/wiki), [author Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=2785100219)

Gate, during the space phase: one small persistent destination; inventory and return travel, shared story, death/disconnect/late join, protected ship return, backup/restore and measured memory with multiple occupied destinations. No adoption needed for Act 1's tooth or background work.

### Luminance — optional later utilities, not a boss designer

The release documentation offers state machines, easing, rope simulation and graphics/sound utilities, and explains required mod referencing. It distinguishes release from potentially unreleased main-branch code and documents possible API changes. MIT licensing is listed in the repository; retain applicable notices if reusing code. [Release README](https://raw.githubusercontent.com/LucilleKarma/Luminance/release/README.md), [repository/license](https://github.com/LucilleKarma/Luminance)

Gate: identify one expensive reusable feature our own implementation lacks, test that released version, and compare maintenance/compatibility cost. A utility library does not make attacks readable, balance class counterplay or synchronize an encounter automatically. The current simple hazard does not justify it.

### User-named mods

The companion report verifies WombatQOL as Wombat's General Improvements: gameplay/building content, not a general authoring library. SerousCommonLib is a verified UI/utility library and the likely intended name for "Serious Common Library"; that name mapping remains an inference. A separate Wombat's Combat identity is unresolved. [Wombat author page](https://steamcommunity.com/sharedfiles/filedetails/?id=2877696929&l=english), [Serous author page](https://steamcommunity.com/sharedfiles/filedetails/?id=2908170107&l=english)

ModLiquid Library is a relevant acid candidate: collision/damage extension points and example bucket/custom-merge code exist. Those are source-level capabilities, not proven Apogean behavior. Its liquid engine/network hooks make the separate portability/mixing/save/network spike in #27 essential; include the native capture-camera path because of our prior DrawLiquid crash, not just ordinary screenshots. Do not promise it solves the requested acid design until those checks pass. See the companion report's pinned API/example sources and limitations. [ModLiquid author repository](https://github.com/Lion8cake/ModLiquidLib)

## When a separate Apogean library would make sense

Start with internal boundaries: world placement, visual asset loading, background projection, conversion, combat states and quest/world persistence. Extract a runtime library only when an identified consumer needs a stable reusable API, the benefit exceeds versioning burden, and the following are explicit:

1. Ownership of content IDs, world/player save keys and network messages.
2. Supported tModLoader/dependency versions, licensing and update policy.
3. Behavior when the optional integration is absent, disabled or removed.
4. Client/server requirements and dedicated-server-safe asset loading.
5. Migration and recovery tests for existing saves.

A development-only companion is lower risk if it cleanly contains QA commands without becoming necessary to load player worlds. Do not put production tile IDs into a disposable QA dependency. Conversely, moving production assets to a required pack merely for organizational neatness adds installation/version coordination without demonstrated benefit.

## Deferred boss/AI research — retain for the right time

Wayfinder: [#28 — Prepare boss-reference research and encounter validation](https://github.com/coffeecentaur-afk/apogean/issues/28). This is explicitly deferred, not an active boss implementation.

The user explicitly wants this remembered, not implemented now. Before the first new/reworked encounter, select a bounded set of references: an official ExampleMod state-machine example, one relevant open-source mod encounter, and one visual fight reference. Record exact version/difficulty/loadout/arena and video timestamps where available. Analyze tell → attack commitment → dodge/counterplay → recovery, not only particle appearance. Reuse principles, not another mod's assets, attack sequence or branding.

Technical baseline: ExampleMod shows explicit NPC states/timers, animation separation and server-owned random choices with synchronization requests. `SendExtraAI`/`ReceiveExtraAI` carry additional state during NPC synchronization; they do not remove the need to decide authority and timing. [Example custom AI](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/ExampleCustomAISlimeNPC.cs), [ModNPC API](https://docs.tmodloader.net/docs/stable/class_mod_n_p_c.html)

Apogean's Matriarch already has a state machine; do not claim we need a library to start one. Convert the actual encounter into a tested specification rather than treating the generic example validator as evidence of coverage. Later telemetry should cover telegraph duration, class access, contact-damage windows, projectile/minion/healing caps, failure cleanup, death/revive integration and multiplayer late join/desync.

Respawn's documented Titanfall action-block approach is a useful process reference: isolate playable ideas before assembling the full experience. Our adaptation is one original attack with clear counterplay in a disposable arena before full boss phases. The session description was read; the video was not watched in this pass. [Respawn GDC session](https://www.gdcvault.com/play/1025105/Designing-Unforgettable-Titanfall-Single-Player)

## What the user can help with

No new software purchase, resource pack or library installation is needed now. The most useful inputs, when that slice arrives, are two or three reference links with the specific liked moment/shape, must-support mod lists, native-scale visual verdicts and an occasional multiplayer partner. Do not ask the user to solve atlas layout or dependency plumbing.

Next implementation remains one solid tooth/fang asset and its honest native fixture. Fit the small manifest/QA improvements around that work; do not replace it with an open-ended engine project. No claim of first-time perfection: the objective is fewer regressions and faster, evidence-backed correction.
