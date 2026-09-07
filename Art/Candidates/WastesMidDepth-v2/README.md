# Approved architecture, deeper Mid terrain — QA candidate

The user said **“looks good to me”** to the corrected checkpoint and broken-shell
upper comparison. Together with the selected Station bridge and liked garage,
these are now the four fixed building designs. This is not approval of new cliff
joins, distribution, every lighting state, or the whole Wastes background.

No new image-generation request in this pass. `New-WastesMidDepthAssembly.ps1`
combines the exact approved upper derivatives with selected **lower geology
only** from the previously rejected `WastesMidBank-v1/Generated` sources. Those
whole images remain rejected; their changed buildings are never used here.

## Pixel contract

- Four final512×1408 modules in `EdgeReview/`: Station, MotorDepot, BrokenShell,
  Checkpoint. Their architecture is not resized, mirrored or re-shaded.
- Upper512×460 study moves down100px. Every pixel through output row440 is
  identical to the shifted approved upper (225792 audited pixels per asset).
  Study soil row340 becomes module soil row440. The remaining footing CAN
  change; do not describe all460 source rows as immutable.
- Native-source lower cliff is reduced with recorded nearest-center sampling.
  Rational scale, translation, source hashes and seam ranges are in
  `EdgeReview/assembly.json`. There is no row stretching or invented detail.
- A bounded continuous seam selects between two sources only within the
  permitted lower footing. Per-column cut and source-selector masks are retained.
- Reviewed hard masks plus limited RGB-only edge preparation remove the lower
  source matte. See [Mask review](MaskReview.md). Export uses the established
  `Export-MaskedBackground.ps1`, not an inferred runtime color key.

`Proposal/` is the first pre-edge-repair assembly and is superseded, not installed.
The recipe's UNREVIEWED label describes generation-time output; the separate
review below records the later decision without rewriting that provenance.

## Checks and reproduction

`Test-WastesMidDepthAssembly.ps1` independently checks every exported pixel,
upper/source hashes, binary alpha, clear RGB, lower sample/donor provenance and
opaque cliff support to the bottom. Four actual CLI faults are rejected for
the expected reasons: changed architecture, soft alpha, missing support and
wrong source sample. This verifies assembly, not aesthetic quality.
An independent fresh-directory assembly/export reproduced all four final PNG
hashes exactly. The renderer's native tests are separate from this repeat check.

```powershell
pwsh -NoProfile -File Tools/Test-WastesMidDepthAssembly.ps1
pwsh -NoProfile -File Tools/Test-WastesRuinLayout.ps1
```

The optional isolated-build parameter `-WastesMidDepthDirectory
Art/Candidates/WastesMidDepth-v2/EdgeReview` audits and installs these four into
a temporary QA asset folder. Normal Content PNGs remain unchanged. An all-or-none
loader selects the optional bank only in the existing QA/render-lab path.

## Stable composition

September7 follow-up supersedes only this original distribution: user requests
fewer Mid pieces and wider gaps after approving foreground depth. The new
10,000px-period spacing trial preserves all artwork and is tracked at
`Art/Validation/WastesMidSpacing-2026-09-07/README.md`. The6,800px record below
remains historical evidence, not the current trial's placement contract.

The6800px Mid period alternates highway, Station, garage, shell and checkpoint
with five quiet hills, plus true open intervals (minimum184px between bounding
boxes). Camera movement never rerolls the arrangement. The long existing Close
bank keeps its independent period and ground anchor. Overlap is allowed, not
dynamically avoided. Negative coordinates and wrap boundaries are tested against
independent absolute world-cell enumeration (176 cases, plus three CLI faults).

The loaded combined bank is **36.74MiB raw RGBA**, up from28.36MiB. This is not
measured GPU residency or frame-time proof. A three-texture coverage check is
23.29MiB but does NOT represent the expanded set. Aggregate memory/performance
and broader viewport/biome/restoration checks remain promotion gates.

## Review limits

The Station and garage lower geology share similar pipe/strata motifs; increased
spacing reduces immediate repetition but does not make them four unique geology
designs. Sparse pale lower-edge specks and the Station's footing texture change
remain native-review targets. Do not start another whole-building redraw loop.
Current native evidence is recorded separately under
`Art/Validation/WastesMidDepth-2026-09-06/README.md`.

Finish the combined Wastes baseline and resolve any required user review before
Wastes mobs. Other biome art, gameplay worldgen and ordinary-world promotion
remain outside this candidate. Wayfinder #11 stays open.
