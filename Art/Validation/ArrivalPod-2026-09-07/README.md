# Arrival pod — native behavior evidence, superseded art

## Current decision

The user accepted the native A3 export, authorized this isolated fixture, then
withdrew visual approval after seeing it: too detailed, needs more pixelation.
A4's coarser design is now accepted as a direction; the requested grittier,
damaged A5 pod and impact divot are review-only under
`../../Candidates/ArrivalPod-v1/A5-ImpactReview/`. Do not install those boards.
Do not confuse the successful behavior tests below with final art approval.
The family remains **contracted**, not integrated or polished.

## Actual scope and results

- tModLoader v2026.7.3.0 / Terraria 1.4.4.9, Windows, gg in the disposable
  **Apogee Native Visual V3**, single-player. No regular worlds opened.
- Native `ModTile`/`TileObjectData`, 5x6 cells, origin (2,5), 16px coordinates,
  2px padding, no custom drawing. Exact 90x108 atlas reconstructs 80x96 art.
- `native.log`: 58 programmatic tests through actual game APIs passed,
  followed by save/reload with the same persistent-state digest. No rebuild.
- All 30 cells separately removed through `Player.PickTile` at copper-pick
  power 35: exactly one intact item, no remaining furniture cells.
- Five missing-floor placement rejections and five support-loss recoveries;
  water/lava state plus framing/recovery; native explosion veto;
  no solid collision; placeable, consumable, lava-safe item metadata;
  re-placement/removal; unchanged world and bed spawn.
- Six fingerprint controls reproduced the old terrain-frame false failure,
  accepted recomputed soil frames, rejected changed pod frames/paint/missing
  ground, and verified every temporary control mutation was restored.
- Static native pipeline and furniture-sheet audits passed. The log validator
  passes real native evidence and rejects nine synthetic defective CLI cases.
  Synthetic mutations are test-tool checks, never native gameplay evidence.

The Status gate passes. The broader Structure gate remains RED on two
unchanged legacy WorldVisualIntegrity expectations: an obsolete exact command
enum literal and the approved DeadForestTree's 5 colors versus a minimum of 8.
The pod pipeline, pod log controls and Helix construction checks pass within
that run. Do not weaken those tests or repaint the accepted tree in this slice.

`day.png`, `inside.png`, `night.png`, `reload.png` are unedited Windows captures
(2560x1401 including title bar, game viewport 2560x1369). They show connected
footing, player drawing in front of the non-solid pod, blue paint and normal
night lighting. Player equipment emits light; the pod does not. They show the
now-rejected Native-v1 appearance, not A4 or A5. Normal world weather can resume
during captures; screenshots are not a controlled weather/performance test.

## Isolation and persistence

Fixture bounds (4476,540,64,24), floor Y559. A finite empty-air search checked
the full 68x28 framing envelope. Three specimens and an empty fourth test
slot stand on a three-row WastesSoil QA platform. No original terrain cleared,
no spawn divot generated. Saved fixture rebuilds are explicitly refused.

Installed package after final clean build (0 warnings/errors):
`62F578114394E2F98C31135A8B69D361C0A172FAFCF149E0D7B7C641A0733802`.
The 408 pre-existing Content PNGs match the preceding accepted package mirror
byte-for-byte. Only the optional pod tile/item images are added in the build
mirror; no candidate image is promoted into repository Content.

Stable checkpoint after test and reload:
`273B29B0F2AF8EAA6628C85793FDA1587E5087B1FA5F6D6B6028F523EBF8BB8B`.
The original failed test is retained in `failed-v1.log`: its digest included
ordinary soil frame coordinates that Terraria recomputes. V2 hashes stable
tile names and persistent state, with frames only for frame-important tiles.
The three original pods' 90 frame cells passed assertions before re-testing.
Old checkpoints remain recorded as legacy data, never silently blessed on
load. The fixture itself was not rebuilt to obtain a passing result.

The **pre-existing grove digest mismatch remains unresolved**. Before/after
guards passed for all pod actions. Expected grove digest starts 87B951FE;
actual during this package starts 65A5229C. Its old hash includes numeric
content IDs, so ID remapping is a possible contributor, not a proven repair.
Do not overwrite its checkpoint or regenerate the grove.

Pre-test package, QA world pair and gg player pair backed up outside the repo:
`C:/Users/max_h/AppData/Local/Temp/ApogeanArrivalPodQA-07c0a49a875d481baffead3dafd9f316`.
The saved QA fixture is retained. Normal position/weather are released on the
QA save-and-quit path. A normal build without the candidate omits this tile;
do not load/save the fixture with it absent and expect a valid comparison.

## Reproduce

Keep the accepted scenery inputs when building the optional pod:

```powershell
pwsh -NoProfile -File Tools/Build-ApogeanIsolated.ps1 -WastesCutoutCandidateDirectory Art/Candidates/WastesFarCity-v1/QA-Package-v1 -WastesCityCandidateDirectory Art/Candidates/WastesFarCity-v1/Runtime-v1 -WastesScaleCandidateDirectory Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3 -WastesDepotAssemblyDirectory Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1 -WastesStationBridgeDirectory Art/Candidates/WastesStationBridge-v1/Study -WastesMidDepthDirectory Art/Candidates/WastesMidDepth-v2/EdgeReview -ArrivalPodCandidateDirectory Art/Candidates/ArrivalPod-v1/Native-v1 -KeepWorkspace
pwsh -NoProfile -File Tools/Request-ArrivalPodValidation.ps1 -Case test
# Save/quit and reopen only the authorized QA world; never use build again.
pwsh -NoProfile -File Tools/Request-ArrivalPodValidation.ps1 -Case reload
pwsh -NoProfile -File Tools/Test-ArrivalPodLive.ps1 -LogPath Art/Validation/ArrivalPod-2026-09-07/native.log -RequireReload
pwsh -NoProfile -File Tools/Test-ArrivalPodLiveValidator.ps1
```

The first packaged attempt failed to register the tile because it checked
only `.png`; tML packs textures as `.rawimg`. Both are now recognized.
The log negative-control runner also needed `${1}`, not `$1` followed by 64
zeros, to avoid parsing the zeros as an oversized regex capture reference.

## Open gates / next work

Approve the damaged coarse concept, make a true native-scale export and
player comparison, then recheck appearance without reopening furniture code.
Manual inventory/drop appearance and item-driven re-placement, sloped/half
anchors, actuator behavior, extended fluid simulation, zoom and multiplayer
remain untested. Hook and direct-liquid probes do not prove full bomb/fluid
simulation. No divot/new-world placement, migration, relay, recipe or shop.
Complete those bounded checks before production spawn integration; use the
existing placement contract, never clear uninspected terrain.

Sources: [official furniture example](https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/Tiles/Furniture/ExampleWorkbench.cs),
[ModTile](https://docs.tmodloader.net/docs/stable/class_mod_tile.html),
[TileObjectData](https://docs.tmodloader.net/docs/stable/class_tile_object_data.html),
[world-save patch](https://github.com/tModLoader/tModLoader/blob/stable/patches/tModLoader/Terraria/IO/WorldFile.cs.patch).
