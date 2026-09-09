# Approved quiet bone — bounded native material fixture

September9 user approves MaskedNative-v2 and permits asset creation/native tests.
This checkpoint covers structural bone ONLY. No new teeth, mouth generation,
ordinary-world conversion, acid, enemies or final rib morphology are claimed.

## Actual captures

- `native-day.png`: top row native Stone control (left), approved bone (middle),
  bone meeting Mawstone (right). Lower row isolated/strip/corner specimens, four
  slopes, half-block, blue paint and actuated bone. Native output1280x608.
- `native-night.png`: same specimens after reload, midnight. Bone stays dark;
  gg's solar equipment produces the warm light. No injected fixture light.

These are Terraria CaptureManager images, not offline atlas previews. The
native forest/HD scenery backdrop is capture-selected; these images do NOT
validate Maw/Wastes background routing or parallax. Native Windows client was
2560x1369 with the user's existing resource packs and CheatSheet enabled.
Tile framing remains native; there is no custom renderer hiding seams.

Inspection found no white grid or disconnected edges in this sample. Some
diagonal material repetition remains visible in broad filled areas: not final
organic rib/arch art. Night is deliberately dark, not proof of every shading
case. Candidate art approval is not native user approval of this fixture.

## Reproducible fixture

`Content/Diagnostics/MawBoneLab.cs` requires single-player, player `gg`, and
world `Apogee Native Visual V3`. It surveys a finite set of empty80x38 areas,
checking an extra3-tile envelope and avoiding the preserved grove. It never
clears occupied terrain or automatically rebuilds an existing fixture.
Saved bounds: X4836,Y500,width80,height38. A small GrayBrick floor supports the
player; this artificial test platform is NOT part of the mouth design.

Run `Tools/Request-MawBoneValidation.ps1 -Case build` once, then `test`, `day`,
`capture`, `night`, `capture` separately. Wait for each request to be consumed;
unfocused Terraria can pause. The helper atomically refuses an existing request.
`release` restores the pre-view player position/time/weather. Save/quit through
the existing QA handler, reopen the same world, then run `reload` and `test`.
Never open a regular world to test this package.
Final release/save completed12:55:21; tModLoader was visually verified back at
the main menu. The saved fixture remains available; temporary view state is off.

13 native programmatic checks passed at12:47:50 and again12:50:47 after actual
save completion12:49:35 and reload12:50:20:

1. Fixture unchanged before tests.
2. Loaded atlas288x270.
3. All340 sampled bone frame rectangles inside the loaded atlas.
4. Solid, light-blocking, not emissive.
5. Structural bone has no immediate contact damage.
6. Existing MinPick59/MineResist2.7 definition retained.
7. Empty temporary mechanical slot.
8. Native solid collision blocks the16x16 probe.
9. Actuation disables collision for the same probe.
10. Thirty native PickTile calls at power58 cannot remove the test block.
11. Power59 removes it in five hits.
12. Native explosion permission returns true.
13. Temporary slot cleaned, fixture restored unchanged.

`reload` also passes the saved material fingerprint. Frames are independently
bounds-checked because ordinary native framing can recompute after load.
Checksum: `822725435F3193A47D6A420FF579F7F62F930B36DECA45C741F162C0E08DA8AE`.
Programmatic engine calls are NOT manual mining, an actual bomb, item drops,
starter-character traversal, network behavior or exhaustive settings proof.

Known unrelated preserved-grove reload mismatch remains RED:
expected87B951FE..., actualBA35565A..., identical across both loads. No rebuild
or checkpoint rebasing occurred. Bone's before/after grove guards are unchanged,
which is narrower than saying the grove's reload test passes.

## Package and asset provenance

The original-source/mask workflow was rerun this session: all11 controls pass,
including deterministic exports and rejected damaged masks/pixels/sockets,
incorrect hashes/dimensions and unsafe output. See `masked-workflow-checks.json`.
Exact candidate/native-alpha comparison and the41-family authoring-state audit
also pass. These static checks do not replace the native evidence above.

- Candidate SHA256:
  `5B4721D3A914EE5AEFD9D56AF95C0F883052BFB633005BC842E5CD101A5CF19E`.
- Installed QA package SHA256:
  `6493031FC90A71B131ED793F44F4A252F4AC6EE73DE94523DC0A919AF07DE985`.
- Production bone remains the old RED atlas, SHA256:
  `E4642E92EEED642EFFCF5E835A7D6CF2B14ECB83CF7A094D808EBF2FC19C0906`.

Isolated build finishes with zero warnings/errors. All410 Content PNGs compared
against the accepted mirror: bone is the only changed image,409 identical.
The override uses the same existing tile ID and properties; it affects every
OssuaryBone drawn by this QA package, not just the fixture. No regular worlds
were opened. Backups of the previous package, V3 and gg were taken before tests
at `%TEMP%/ApogeanQA-20260909-Bone/` and are not committed player/world data.

Keep accepted optional Wastes/pod assets in every build:

```powershell
pwsh -NoProfile -File Tools/Build-ApogeanIsolated.ps1 `
 -WastesCutoutCandidateDirectory Art/Candidates/WastesFarCity-v1/QA-Package-v1 `
 -WastesCityCandidateDirectory Art/Candidates/WastesFarCity-v1/Runtime-v1 `
 -WastesScaleCandidateDirectory Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3 `
 -WastesDepotAssemblyDirectory Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1 `
 -WastesStationBridgeDirectory Art/Candidates/WastesStationBridge-v1/Study `
 -WastesMidDepthDirectory Art/Candidates/WastesMidDepth-v2/EdgeReview `
 -ArrivalPodCandidateDirectory Art/Candidates/ArrivalPod-v1/Native-v3 `
 -MawBoneCandidateDirectory Art/Candidates/MawBone-v1/MaskedNative-v2 `
 -KeepWorkspace
```

Stop/save the game before rebuilding. The isolated builder installs its package;
do not mistake a bare build for this accepted-asset composition.

## Next

Separate brittle teeth: establish actual native spike topology, author original
matching art, prove four-way attachments, easy mining and actual contact damage
in the disposable fixture. Then integrate the accepted raised-mouth layout
with the saved route/protection plan; remove legacy shelves/fall-limit together.
Preserve the Stomach, accepted Wastes/pod/divot, and optional acid#27 boundary.
