# Kessler Arrival Validation

## Accepted slice

- World: `Apogee Campus QA`
- Seed: `ApogeeCampusQA-2026-09-03`
- Size: 8400 x 2400
- Campus atlas hash: `9DBCB5C1`
- tModLoader: 1.4.4.9 / 2026.7.3.0

Wall of Flesh records Kessler's impact. A daytime clear must observe a night before the next dawn can start the assessment; a nighttime clear starts it at the next dawn. The live-fire audit contains ten targets and escalates to four Reclaimers. Completion awards five Kessler Scrip to every active player, opens the public bulkhead, and spawns Quartermaster Mara Venn at the authored service post.

The exact QA world name enables a deterministic thirty-second pilot: it starts the assessment, protects the player from test damage, forces the elite threshold, completes the quota, and moves the player to Mara's public frontage. No mutation command or pilot behavior runs in an ordinary world.

## Live client proof

- Survey phase: the HUD reported ten remaining targets; the drone acquired the player with a narrow dashed red line and unobtrusive endpoint marker.
- Elite phase: the HUD reported four remaining Reclaimers; a Reclaimer rendered and attacked without body-contact damage.
- Completion: the player received Kessler Scrip, the progression bulkhead opened, and Mara remained separated from the player at her service post.
- Requisitions: Terraria's standard shop route opened with coin and Kessler-Scrip prices.
- Briefing: the vanilla contact panel handed off to the custom dialogue panel; all three root choices rendered and the identity branch advanced to its response and Back option.
- The first implementation drew acquisition geometry using the full `TextureAssets.MagicPixel` dimensions and produced an opaque wedge. The accepted implementation scales desired segment dimensions by the actual texture width and height.
- The first dialogue handoff closed vanilla chat inside `OnChatButtonClicked`, producing a silently caught `GUIChatDrawInner` index error. The accepted handoff defers both chat close and custom UI open to the following `UpdateUI`; repeating the same path produced no silent exception, `IndexOutOfRangeException`, or error in the client log.

## Automated gates

- `dotnet build --no-restore`: pass; one pre-existing lower-case type-name warning.
- `Tools/Test-KesslerArrival.ps1`: pass.
- `Tools/Test-TileLab.ps1`: pass.
- `Tools/Test-SurfaceRegression.ps1`: pass.
- `Tools/Test-WorldVisualIntegrity.ps1`: pass.
- `Tools/Test-VisualContracts.ps1`: pass.

## Deferred art and network gates

- Survey Drone, Reclaimer, and Mara currently use intentionally tinted or vanilla placeholder sprites. Their mechanics are accepted; their production silhouettes are not.
- Explicit multiplayer authority, late-join reconstruction, per-player scrip delivery, and Campus bulkhead synchronization remain required before public release.
- The Wastes surface visual gate remains open for soil/grass continuity, rigid whole-object brush collision, dead-tree density/variation/cut readability, and normal-camera background framing.
