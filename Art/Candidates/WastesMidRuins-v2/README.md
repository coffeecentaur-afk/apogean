# Three Wastes Mid ruins — architecture concepts and clean cutouts

2026-09-06. **Review-only, not installed, not game screenshots.**
The user likes the earlier broken shell and requested two or three ruins total.
This pass retained it and generated exactly two new originals with the built-in
image tool: a low motor depot and a narrow checkpoint/communications ruin.
`MotorDepot-PROMPT.md` and `Checkpoint-PROMPT.md` preserve the exact prompts.
Station and the first shell were style references, not edit targets. All original
source files and all accepted runtime scenery are unchanged.

## View the concepts

- [Broken shell — real-alpha derivative](Transparent-v1/BrokenShell.png)
- [Motor depot — real-alpha derivative](Transparent-v1/MotorDepot.png)
- [Checkpoint — real-alpha derivative](Transparent-v1/Checkpoint.png)

`Prepared-v1/*-Dark.png` and `*-Light.png` are offline composites, not native
game proof. The first shape is user-liked; the other two are proposals.

## Transparency and edge proof

All three original PNGs are1024x1536 with zero actual transparent pixels: their
checkerboard is RGB paint. Reused the user-authorized explicit-mask pipeline
instead of repeating generation:

1. `New-WastesMidRuinMasks.ps1` proposes components only for these three pinned
   dark/warm sources. It is not a universal color key. `Mask-Proposal-v1`
   retains initial unreviewed masks, component bounds and pale-rim failure views.
2. `Mask-Decision.json` selects exterior matte and enclosed openings (windows,
   railings, roof steel, antenna loop). Tiny pale material pixels are retained.
3. `Prepared-v1` retains final masks and separate color masters. Only neutral,
   pale pixels within two pixels of a removed region can receive an original
   material-color donor at most six pixels away. Donor coordinates are recorded.
   No outline erosion, broad interior recolor or resize.
4. Existing `Export-MaskedBackground.ps1` performs hash-pinned exact exports.
   PNG sidecars pin source/mask/output hashes and encoded pixel counts.

| Concept | Transparent pixels | Edge RGB changes | Original occupied bounds (inclusive) |
| --- | ---: | ---: | --- |
| Broken shell | 810820 | 5748 | x94–921, y147–1511 |
| Motor depot | 794041 | 3076 | x61–973, y382–1475 |
| Checkpoint | 1102695 | 5318 | x186–747, y33–1509 |

Every export has hard alpha, zero RGB in removed pixels and exact prepared RGB
in kept pixels. Dark proof exposed a continuous pale rim after mask-only removal;
the separate color preparation removes it in reviewed light/dark previews while
retaining thin branches, antenna, railing and window outlines.

`Test-WastesMidRuins.ps1` checks original/export hashes, unchanged interior,
every edge donor/distance and mask/alpha pixels. All three pass. Nine deliberately
defective in-memory cases are rejected (matte leak, kept-pixel mismatch, donor
provenance, for all three originals). The existing exporter's actual CLI positive,
deterministic-repeat and eleven negative tests also pass. Not live-render proof.

A fresh-directory rerun of the source-specific mask/preparation recipe reproduces
all six color-source/mask PNG hashes exactly. Accepted Station, Close-v2 and Far
city hashes still match the prior native QA package. Reproduction does not turn
the source-specific segmentation into a general background-removal algorithm.

## Scale and placement — still open

These are NOT drop-ins for the current391–576px-wide,1408px-tall Mid slots.
The depot is913px wide, the shell828px; even the562px checkpoint has oversized
architectural features compared with Station. The requested generation width was
not reliably followed. The checkpoint's1477px occupied height exceeds Mid too.
Do not quietly squeeze, enlarge or crop them into service.

Next: review the two new silhouettes, then author a measured native-scale
building/cliff/continuation assembly against Station's actual door/storey scale.
Export acceptance does not solve this or prove the tapered bottom will remain
offscreen during flight. Stable quiet-spaced placement, joins, Space fade,
ground/diagonal/lighting/routing and residency checks remain necessary.
No renderer, mod binary, normal world or accepted asset changed in this pass.

## Reproduction and provenance

Run from the repo in PowerShell7 with new output directories; existing output
is refused. Never pass the opaque originals to a runtime build.

```powershell
pwsh -NoProfile -File Tools/New-WastesMidRuinMasks.ps1 -OutputDirectory <new-proposals>
pwsh -NoProfile -File Tools/New-WastesMidRuinMasks.ps1 -OutputDirectory <new-prepared> -DecisionPath Art/Candidates/WastesMidRuins-v2/Mask-Decision.json
pwsh -NoProfile -File Tools/Test-WastesMidRuins.ps1
pwsh -NoProfile -File Tools/Test-MaskedBackgroundExport.ps1
```

Each individual export takes its prepared `*-EdgeSource.png`, `*-Mask.png`, new
output path and both hashes; see `Tools/BackgroundMaskWorkflow.md`. Originals
are retained here and in `WastesMidRuin-v1/`. Built-in generation files:
`exec-03d2a8b0-e6da-4fc5-b77e-5721033c0243.png` (depot) and
`exec-c33f76ca-9cb4-4ce5-855a-ba295713092c.png` (checkpoint).

Biome direction and living-Jungle exception: `BACKGROUND_BIOME_DIRECTION.md`.
Do not recolor these Wastes concepts across all biomes.
