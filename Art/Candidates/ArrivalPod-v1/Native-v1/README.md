# Arrival pod A3 — native-size candidate, not installed

The user approved A3 with "perfect". Its architecture and style are frozen.
This directory contains the next, separate native-size review, not a new design
or a live Terraria capture. No runtime tile/item or world-generation code changed.

## Deliverables

- `ArrivalPod.png`: true RGBA **80×96**, 20 opaque colors, hard alpha. Fitted
  silhouette 80×90, centred horizontally and bottom-aligned at Y=95, leaving
  six empty top rows. Actual opaque contact at bottom: 19 pixels.
- `ArrivalPod_Tile.png`: **90×108**, five columns and six rows of 16×16 art,
  each followed by two transparent padding pixels, including terminal padding.
  One static style/frame. Placement origin proposal remains local tile (2,5).
- `preview.png`: actual 1× pixels on left; exact 4× enlargements on light/dark
  mattes. The empty 20×42 rectangle is a collision-size guide, NOT a rendered
  character. It is an offline assembly, not a screenshot or runtime scale proof.
- `mask-proposal.png`: generated same-canvas selection proposal; not approved
  production art. `MASK_PROMPT.md` contains the complete built-in edit prompt.
- `mask.png`: exact black/white mask after explicit normalization and hinge repair.
- `cutout.png`: 1254×1254 RGBA master retaining **every kept source RGB pixel**.
- `cutout.png.report.json` and `native-report.json`: dimensions, changes and hashes.

## Method and losses

Source is the unchanged accepted `../design-a3-blend.png`, SHA256
`8A8860FA62D702B3464BC3450443ED0688130E3F4B8192DA8F0F4E6E992511CB`.
The built-in image editor produced a same-size monochrome mask, not a redraw.
The source-specific tool makes it binary at R+G+B >=384. The model treated
black hinge hardware as exterior space; strips X752–794/Y575–621 and
X769–807/Y783–835 restore 450 mask pixels, without changing source RGB. These
are **this source's** selections, not a generic color key or new painted detail.
The final mask keeps 533,419 pixels and removes 1,039,097. No text or ground
shadow is intended as part of the furniture. The two native hinges remain
attached and background can still show through the genuine exterior opening.

Existing `Tools/Export-MaskedBackground.ps1` performs the exact RGBA export.
Mask semantics were inspected against the displayed cutout/native assembly;
pixel equality alone cannot prove selection correctness.

`Tools/New-ArrivalPodNative.ps1` then fits the 827×940 selected bound using
nearest-neighbour centre sampling and a fixed, source-directed 20-color palette.
**That step is lossy.** It changes size and colors and can lose thin marks;
do not call it a lossless transplant, an authored native redraw, or automatic
style matching. It preserves the concept's construction rather than asking
the model to generate another pod. The actual 80×96 result is shown for review
before anything can be installed. No board or label was cropped into Content.

The proposed tile layout follows the existing furniture coordinate/padding
contract recorded in `ARRIVAL_POD_CONTRACT.md` and the shared structure skill's
`Test-FurnitureSheet.ps1`. An actual registered `TileObjectData` drawing path,
offsets, floor anchors, pickup, support loss and networking are NOT yet tested.

## Static evidence

Before authoring the files, `Test-ArrivalPodNative.ps1` failed with
`NATIVE_ART_MISSING`. The candidate now passes:

- actual PNG dimensions; 20 colors; no partial alpha or hidden matte RGB;
- no opaque pure white/magenta export-key pixels;
- all 30 tile cells reconstruct all 7,680 source pixels exactly;
- every padding cell, including terminal rows/columns, is transparent zero;
- bottom-row contact and one eight-connected silhouette, including aerial/hatch;
- shared `Test-FurnitureSheet.ps1` with 5×6 tiles and a 20-color limit.

`Test-ArrivalPodNativePipeline.ps1` calls the actual validator for the valid
baseline and 11 defective/missing inputs: absent assets, wrong native size,
soft alpha, hidden matte RGB, key color, empty silhouette, excessive colors,
floating base, detached pixels, frame drift and dirty padding. All are rejected
for their expected reasons. It also checks refusal to overwrite candidate
outputs or write inside Content, reruns the exporter in a new directory,
reproduces five image hashes and checks nine originals are unchanged.

The shared hard-mask export suite passes its valid cases and 11 rejection
controls. The new native pipeline is included in the Structure gate. These
are static/tool tests, not game builds or fixture acceptance.

The full **Structure gate remains RED**, independently of this pod candidate.
Pod pipeline and Helix checks pass, but `Test-WorldVisualIntegrity.ps1` rejects
an old exact `ValidateSet(...)` command-list expectation and the existing
five-color `DeadForestTree.png` against its minimum-eight-color rule. The
validator, command source and tree PNG are unchanged from HEAD in this turn.
Do not repaint the accepted tree or weaken these checks to promote the pod.
Status gate passes all 37 family records, eight skill mirrors and generator
ownership. Full release/structure acceptance is not claimed.

Installed `apogean.tmod` remains SHA256
`FE1B5E8B284FE8A21CF3029964967997396D8226F32F1A9D5E0F0642655669FA`.
No game was launched, rebuilt, restarted or edited for this offline pass.

Reproduce to an **existing empty directory**, never over the reviewed candidate:

```powershell
pwsh -NoProfile -File Tools/New-ArrivalPodNative.ps1 -OutputDirectory <empty-directory>
pwsh -NoProfile -File Tools/Test-ArrivalPodNative.ps1 -Directory <empty-directory>
pwsh -NoProfile -File Tools/Test-ArrivalPodNativePipeline.ps1
```

The exporter refuses occupied outputs. PNG/report writes are separate; an I/O
failure can leave a partial directory, which is not accepted output. Use a new
directory rather than deleting or silently repairing an existing candidate.

## Next gate

Native-size appearance approval, then isolated native furniture placement,
mining/re-placement, support-loss and save/reload checks. Preserve A3 and the
accepted backgrounds. If scale cannot be judged offline, use the authorized
disposable QA world with a labelled native fixture after agreeing to load it;
do not repeat concept art to answer an engine-scale question. Fresh-world
arrival placement and multiplayer follow; relay work stays deferred.
