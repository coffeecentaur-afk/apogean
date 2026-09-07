# Approved Station bridge — native ground check

The user answered **yes** to the middle Station candidate on the offline
old-Station/new-Station/garage board. This approves its style direction and
selects that exact upper artwork; it does not approve missing depth or every
lighting/biome case. No new image generation or source-art edits in this pass.

## Actual result

`station-garage-crop.png` is a 1:1 crop of a native tModLoader window screenshot:
approved Station left, real player gg center, unchanged liked garage right.
`station-shell-crop.png` and `station-checkpoint-crop.png` check the other existing
study assets in the same package. `baseline-return-crop.png` shows normal scenery restored,
including the OLD full-depth Station; the new short art is gallery-only.

Original2560x1401 JPEG captures remain private in the pre-run backup's
`NativeCaptures/` directory. Public evidence excludes unrelated overlay/UI
regions: pair crops use(x730,y625,w1080,h360), baseline uses(375,425,1680,475).
Every PNG pixel was compared with the same-coordinate decoded original JPEG
pixel: no resize, repaint, color adjustment or sharpening. These are real game
capture crops, not offline art composites; the generic crop helper's output
label does not identify its input source. No image-generation tool involved.

Agent inspection at native scale: canopy, pump openings and thick dead branches
remain readable and connected. No obvious continuous pale cutout rim in this
daylight view. A real world tree overlaps the garage/checkpoint; it was not
removed for a prettier capture. This is not pixel-exact screenshot-color proof
(captures are JPEG), nighttime proof, or another user-art acceptance claim.

- Client: tModLoader2026.7.3.0 / Terraria1.4.4.9, single-player.
- World: **Apogee Native Visual V3** only; character **gg**.
- Window captures:2560x1401 including titlebar; game viewport2560x1369.
- Same existing ground gallery:512x460 study textures at1 asset pixel per
  display pixel, game zoom1.3333, real20x42 player (~27x56 displayed).
- Actual submitted top-lefts: Station(744.00525,499.0052), ruin(1304.0052,499.0052),
  gallery soil datum839.0052 (viewport coordinates, excluding titlebar).
- Ground-only gallery hides normal Mid/Close and keeps Far, world terrain,
  entities and UI. Real terrain hides the unfinished lower extent; that is
  disclosed fixture isolation, not full-height validation.

## Checks

Fresh `scale-telemetry.log`, independently checked by the existing real CLI:

| Case | Samples/checks | Failures |
| --- | ---: | ---: |
| Station + garage | 29914 draw samples | 0 |
| Station + shell | 25264 draw samples | 0 |
| Station + checkpoint | 27828 draw samples | 0 |
| Normal ground return | 89730 matrix /29910 frame checks | 0 |

Normal return maximum coordinate error0.01px; Close ground-datum error0.83px.
Generic ground-lock counters are zero during the isolated gallery by design,
not aerial proof. Four actual CLI negative controls reject MissingSample,
MissingRelease, WrongAsset, and DrawFailure for the expected reasons.

Isolated build: **0 warnings,0 errors**. Same mod hash before/after captures:
`1CBA441BE9DCF30507F5D6653A05B8B0366FDC48C9B9CAB6837B3D1D5CF48CF6`.
Approved Station upper SHA256:
`81CD2A9EB7CC99E637CFCF2EEF610CB64AA3A8EC90D06A4723B51BF93D0F0861`.
Exact source-to-mirror hashes match. Old full-depth Station is still
`C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C`.

The skill's actual `Test-BackgroundSet.ps1` was run against the Far city, new
Station upper and existing Close, using the current module minimum488x1408.
It correctly rejects: **Mid is512x460; minimum is488x1408**. This expected failure
remains a full-depth blocker, not a waived threshold or a completed family.

```powershell
pwsh -NoProfile -File Tools/Build-ApogeanIsolated.ps1 -WastesCutoutCandidateDirectory Art/Candidates/WastesFarCity-v1/QA-Package-v1 -WastesCityCandidateDirectory Art/Candidates/WastesFarCity-v1/Runtime-v1 -WastesScaleCandidateDirectory Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3 -WastesDepotAssemblyDirectory Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1 -WastesStationBridgeDirectory Art/Candidates/WastesStationBridge-v1/Study -KeepWorkspace
pwsh -NoProfile -File Tools/Test-WastesMidScaleLive.ps1 -LogPath Art/Validation/WastesStationBridge-2026-09-06/scale-telemetry.log
```

New optional build parameter audits the exact Station derivative and copies it
only to the temporary gallery asset path. Repository Content PNGs, all game C#,
normal routing, Far/Close/Highway and the garage are unchanged. No separate
resource pack or new game settings required.

## Safe checkpoint and next step

Pre-run local backup: `ApogeanStationNativeQA-9d6bedc57174410c8b1eed9deb576761`
under local temp (prior mod/config/gg/maps/disposable world). Build mirror:
`ApogeanTmlBuild/27dc089e89424d49aa84daff5e17f365/apogean` under local temp.
Saved only the named QA world and returned to the main menu at22:04:21 local.
No regular world opened. No new draw exception observed. The known old grove
checkpoint mismatch (expected87B951FE… / actualEB1D7019…) again blocked automatic
lab rebuilding; it was **not bypassed or reset**. Persistence remains unresolved.

Next: preserve approved upper pixels and author a compatible deeper cliff below
them, with continuous side contours and no row stretching/mirroring. Contract
the full module's soil/socket mapping separately (the gallery's y340 is NOT
the current full Station's y440). Inspect the join before normal placement;
then test diagonal flight, altitude/Space/descent and light/biome/restoration
matrix. No further Station/garage redesign loop. Wastes baseline still precedes
the arrival-pod/starting-area and shallow-Maw slices.
