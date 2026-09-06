# Far city v1 — composition only; transparency export failed

One built-in image_gen call produced City-Concept.png using the approved Station as a palette/style reference only. The Station itself was not edited. Exact prompt: PROMPT.md.

SHA256: 583BE8E5801C37887CFFC9A9E73FC1FD1A4B4AA1E2E3DA346FDFF0F1E3C40AF7.

The requested canvas was2048×1280; the returned image is1586×992 and contains ZERO transparent pixels. Export-check.json fails DIMENSIONS_CHANGED and NO_TRANSPARENT_PIXELS. The apparent checkerboard is painted RGB, not a viewer transparency grid. No resizing, keying, replacement, build or live installation was performed.

The demolished buildings, industrial ruins and warm cracked earth explore the requested direction. Side-view suitability, distant-layer contrast, repetition, authored lower extent and compositional acceptance remain open. This is not an engine-ready panorama.

## User correction: separate artwork from transparency

The user challenged the repeated reliance on generation to supply alpha. Diagnosis: the requested transparency was not present in the returned file. The pre-install test caught it, but displaying a failed output without a resolved export still wastes review cycles.

Existing background-authoring instructions require actual alpha; multiple older prototype scripts instead use source-specific color keys. Those keys are not a safe general solution for gray ruins or thin dark branches.

Proposed next bounded tool step (NOT implemented here): a reusable explicit-mask exporter. Preserve source artwork and dimensions, author/review a separate same-size keep/remove mask, then deterministically write RGBA. Validate preserved kept pixels, transparent sky and window holes, thin connections and dark/light backing previews. A learned segmentation tool may propose a mask, but neither it nor a color threshold can be assumed correct without inspection. The difficult part is identifying the intended silhouette and recovering contaminated edge colors, not writing alpha0. Mask export and edge-color cleanup remain separate operations.

Do not generate another city merely to retry transparency. Do not quietly threshold all grays or rescale this concept into the production contract. No mask/exporter is claimed complete, no new software was installed, and no production asset was changed.
