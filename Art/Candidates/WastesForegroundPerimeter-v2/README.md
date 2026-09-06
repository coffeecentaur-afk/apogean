# Foreground perimeter v2 — offline candidate, not installed

The user authorized a bounded native-pixel perimeter correction while preserving the accepted Station and layer layout.

- Output: Foreground-Deep.png, 1448×1915, SHA256 9D039C003929EC128F43C3DFEFB3F98C22DC15488EB0AA9B0F9151C00D726FE4.
- Source: ../WastesCutoutRepair-2026-09-05/Exact-v1/Foreground-Deep.png, SHA256 4158255600690F0BFA67931C8D3AE0B5FF5E93B3BE5714D360DCB279DD3CC8D4.
- Station remains byte-for-byte unchanged at C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C.
- 1,049 RGB changes, including 418 below the old row440 inspection cutoff. Zero alpha or interior changes; 484,520 transparent pixels and zero partial-alpha pixels.

## Method and checks

Tools/Repair-WastesForegroundPerimeter.ps1 pins both source hashes. It considers only fully opaque pixels exposed to transparency in an eight-neighbor neighborhood. Pale candidates must be at least16 luminance brighter than a nearby supported body color. The donor is the luminance median of the five nearest original fully supported body colors within radius7. Exact donor RGB is copied; no rescaling, averaging, iterative propagation, alpha erosion, or global tint.

This is an asset-specific heuristic, not a universal background remover or proof that every remaining bright edge is wrong. Interior highlights are unchanged; visual review must assess whether intentional perimeter highlights were over-darkened.

Reproduce:
```powershell
pwsh -NoProfile -File Tools/Repair-WastesForegroundPerimeter.ps1 -SelfTest
pwsh -NoProfile -File Tools/Repair-WastesForegroundPerimeter.ps1
```

SelfTest passes a lower-edge correction and protected interior highlight, and rejects three in-memory mutations (alpha, interior, unrecorded edge) through the actual validator. Two normal runs produced the same PNG hash. The general export validator also passes dimensions, transparency, and preserved alpha. The custom validator additionally checks every recorded donor and restricts all changes to the original perimeter.

Perimeter-review.json uses the same broad diagnostic as v1: 417→6 pale-neutral candidates, with 188→1 below row440. These counts are review aids, not counts of proven defects; no zero-candidate requirement is imposed.

Edges-Dark/Light/Warm-Before-After.png were inspected offline. Left is before, right after; each native source pixel is enlarged to2×2 only for labelled inspection. These are NOT game screenshots. Foreground silhouette and body detail remain while pale outlines are reduced.

## Gates and next step

Static invariants pass. User art approval, native rendering, live lighting/joins/routing and production acceptance remain pending. No Content PNG, renderer, build, game process or world changed.

The subsequent checkerboard question concerns the separate Far city generation, not this already-transparent foreground. Do not apply a new cutout mask to this color-only repair.
# Subsequent runtime checkpoint

This exact candidate was later packaged unchanged in the city QA run. See
`Art/Validation/WastesCityRuntime-2026-09-05/README.md` for the bounded native
captures and remaining scene/approval gates. The offline evidence below retains
its original scope; it is not whole-scene visual acceptance.
