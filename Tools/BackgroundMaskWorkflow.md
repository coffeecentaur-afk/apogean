# Explicit background mask workflow

Use this for user-authorized deterministic cutouts. It needs PowerShell7 on Windows and the existing System.Drawing runtime; no paid API, model download or resource pack.

## Roles

- Color master: keep the original PNG.
- Mask: same dimensions, fully opaque black (remove) or white (keep). Edit it with a hard pencil in a pixel editor, or inspect a source-specific segmentation proposal before accepting it. Holes between floors/window frames need explicit mask coverage too.
- Export: exact kept source colors, RGBA0 elsewhere. The exporter performs no color key, resizing, smoothing, outline erosion or automatic object detection.

```powershell
pwsh -NoProfile -File Tools/Export-MaskedBackground.ps1 -SourcePath <master.png> -MaskPath <mask.png> -OutputPath <new-candidate.png>
pwsh -NoProfile -File Tools/Test-MaskedBackgroundExport.ps1
```

Optional SourceSHA256 and MaskSHA256 pin approved inputs. Existing output PNG or sidecar is refused; use a new candidate path. The output directory must already exist. Output sidecar is <new-candidate.png>.report.json. Inputs are PNG; mask and kept source pixels must be fully opaque. This hard-alpha tool is deliberately not a soft-matting/photo exporter. Maximum canvas is32million pixels as a host-tool safety bound, not a Terraria limit.

The tool verifies the encoded PNG in memory before writing and uses create-new file semantics. Input validation failures leave no output. PNG and JSON are separate writes: an external disk/I/O failure during writing may leave a partial candidate; a missing report is not success. No generic claim of transactionality.

## Review and limits

Inspect the mask overlay and exported pixels over dark/light backgrounds at native scale. Automatically suggested masks can misclassify pale concrete, thin branches and window interiors; exporter correctness cannot detect those semantic mistakes.

A painted matte often contaminates edge RGB too. Correct that separately in a versioned color master with a recorded edge-only change set, then export with the unchanged mask. Do not erode the silhouette or darken every highlight to achieve a warning count.

Wastes city example: Tools/New-WastesCityMask.ps1 proposes near-white components only in this pinned source's upper570-row sky/facade band. The source-specific proposal is NOT a universal cutout algorithm. Mask-Decision.json records the reviewed component selection and hashes. The optional finalized preparation changes nearby pale edge colors; every change has an original donor record checked by Tools/Test-WastesCityMask.ps1. Outside these bounded repairs, source pixels are preserved.

## Proof

The first real CLI test intentionally copied the opaque source and failed REMOVE_PIXEL_0_0. The implemented exporter now passes exact expected8×8 output:18 kept pixels,46 transparent pixels, deliberate white/gray art, enclosed window hole and a one-pixel bridge. A second pinned run produces the same PNG hash.

Eleven actual CLI rejection cases cover mask dimensions, gray/colored/soft masks, empty/full masks, kept soft-alpha source, both hash mismatches, existing output, and existing sidecar. Inputs and original output are hash-checked afterward. The city validator additionally checks all recorded prepared-color changes and the exact final mask result; three in-memory defects are rejected separately.

Evidence: Art/Candidates/WastesFarCity-v1/Transparent-v1/README.md. No passing export test grants artwork, runtime canvas, coverage, biome routing or live-render approval.
