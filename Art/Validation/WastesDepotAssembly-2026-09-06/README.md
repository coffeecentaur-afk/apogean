# Preferred depot building + rougher terrain — native ground check

`motor-depot.jpg` is an actual, unretouched tModLoader window capture: accepted
Station left, real player gg center, component-assembled depot right. The depot
retains the earlier quieter building pixels and the later rougher ground.
No new building generation. An existing world tree overlaps its right bay; the
fixture did not remove the tree or change tiles to improve the screenshot.

Agent visual review: footing remains attached, without a visible horizontal
alpha gap in this ground view. Cleaner building/grittier ground is the requested
component choice, but the Station pixel-style match remains unapproved. This is
not a claim that the assets now have identical texture language.

## What actually ran

- Cold client launch, tModLoader2026.7.3.0 / Terraria1.4.4.9, disposable
  **Apogee Native Visual V3**, player **gg**, single-player only.
- Same existing ground gallery path:2560x1369 viewport, zoom1.3333,1 asset pixel
  per display pixel, actual20x42 player body (~27x56 displayed before equipment).
- Four optional512x460 study textures. Depot is432 source rows plus28 empty
  padding rows; real terrain hides the padding at this fixed camera. No bottom
  stretch or invented deep-foundation detail. The other three studies unchanged.
- Forced gallery temporarily suppresses normal Mid/Close, retains Far and world
  tiles/entities; ordinary `ground` case restores normal scenery. No renderer or
  game C# source changed in this assembly pass.
- `broken-shell.jpg` / `checkpoint.jpg`: package regression checks of the existing
  unmodified studies. `baseline-return.jpg`: ordinary scenery restored.
- All images are native-window JPEG2560x1401 including the titlebar, not lossless
  source-color tests or offline mockups.

## Evidence

`scale-telemetry.log` retains this fresh run only. Independent existing live
validator passes all three paired draw/release cases and the normal return:

| Case | Samples/checks | Failures |
| --- | ---: | ---: |
| Depot | 29942 draw samples | 0 |
| Shell | 29954 draw samples | 0 |
| Checkpoint | 29968 draw samples | 0 |
| Normal return | 89802 matrix /29934 frame checks | 0 |

Ground return max submitted-coordinate error0.01px; Close ground datum error
0.83px. Gallery's generic ground-lock counters are intentionally zero, not a
failed aerial test. Four real CLI negative controls reject missing samples,
missing releases, wrong assets and failed draws with their expected reasons.

Source and deterministic pixel-selection recipe:
`../../Candidates/WastesMidRuins-v2/ComponentAssembly-v1/README.md`.
All235520 output pixels traced; three actual static CLI defects rejected. Fresh
generation reproduces all PNG/JSON bytes after correcting unordered JSON output.
Station, Far and Close are unchanged; accepted source hashes are enforced by
the independent assembly validator.

Isolated build passed with0 warnings/errors. QA `.tmod` SHA256:
`669431392200EC99E47A09017B7E0EA6F5F1D3CAF9AF703BB422DDCA775EAEC8`.
Depot asset SHA256:
`AD30DA51EDF8CAE32822AFF52D67B28733D1AC965AF185BC7D01A4E1DEAEF890`.
The same mod hash was checked after native capture. Only a temporary build-mirror
gallery texture is overridden, not repository Content or ordinary routing.

```powershell
pwsh -NoProfile -File Tools/Build-ApogeanIsolated.ps1 -WastesCutoutCandidateDirectory Art/Candidates/WastesFarCity-v1/QA-Package-v1 -WastesCityCandidateDirectory Art/Candidates/WastesFarCity-v1/Runtime-v1 -WastesScaleCandidateDirectory Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3 -WastesDepotAssemblyDirectory Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1
pwsh -NoProfile -File Tools/Test-WastesMidScaleLive.ps1 -LogPath Art/Validation/WastesDepotAssembly-2026-09-06/scale-telemetry.log
```

## Safety and remaining boundary

Pre-run mod/config/gg/disposable world backup: local temp
`ApogeanDepotNativeQA-e25ebf782ca44d9cab8025f8902d2271`.
The old grove checkpoint mismatch (expected87B951FE… / actualEB1D7019…) recurred
on loading, blocked automatic lab reconstruction and was NOT bypassed/reset.
No new draw exception observed. This pass does not repair that existing issue.
The allow-listed QA save/quit request ran after the normal return; no regular
world was opened. Source inputs and old candidates remain recoverable unchanged.

Combined art approval is still pending. This is not a1408px-deep module,
production integration, seeded spacing, flight/diagonal/Space coverage, new
lighting proof or a full Background production-gate pass. Do not start another
whole-building redraw. Next is review of this concrete combination, then the
remaining depth/placement gate; retain Wastes → starting-area/drop-pod → Maw.
