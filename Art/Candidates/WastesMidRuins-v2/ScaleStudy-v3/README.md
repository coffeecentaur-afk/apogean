# Native-size upper studies — v3

Latest offline board: four sprites side-by-side at identical soil row. v1 uses
the original checkpoint; v2 uses new facing with a two-row board. Both remain
history. v3 keeps exact v2 sprite pixels with a single-baseline board.
Boards are NOT in-game screenshots.

Each study is512x460, soil datum340. Station uses existing1/1 pixels; the three
large concepts use explicit2/5 nearest-neighbor sampling. No smoothing, runtime
enlargement, horizontal flip or painted backing. Each `*-scale.json` records
source hash, source ground, ratio and offset. Reproduce into a new directory:

```powershell
pwsh -NoProfile -File Tools/New-WastesMidScaleStudy.ps1 -OutputDirectory <new-directory> -CheckpointCandidatePath Art/Candidates/WastesMidRuins-v2/FacingStudy-v1/Selected-v1/Checkpoint-Transparent.png -CheckpointSHA256 4C4E6419ABCD1A6545AC3CB28AFE70A44627389627778ED67DAC344BC7F4699F
pwsh -NoProfile -File Tools/Test-WastesMidScaleStudy.ps1 -CandidateDirectory <new-directory>
```

The user needed actual character/terrain scale. These exact sprites now have a
disposable ground-only gallery: `Art/Validation/WastesMidScale-2026-09-06/README.md`.
They lack deep foundations and are NOT installed as ordinary Mid modules.
