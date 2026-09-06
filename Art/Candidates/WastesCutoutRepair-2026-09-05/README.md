# Wastes cutout repair — rejected before installation

The user reaffirmed that the Wastes scenery is biome-specific, with equally
developed but distinct background families required for the Maw and other biomes.
Wastes baseline work remains first; this is not approval for a universal panorama.

## Bounded attempt

One built-in image-edit call targeted the disconnected Station trees; one targeted
the pale upper Foreground silhouette. Full prompts and invariants are in PROMPTS.md.
The images shown in chat are edited candidates, not game captures or accepted assets.

| Asset | Required size | Returned size | Transparent pixels | Verdict |
| --- | --- | --- | --- | --- |
| Station | 488x1408 | 738x2131 | 0 | Rejected |
| Foreground | 1448x1915 | 1091x1442 | 0 | Rejected |

Both calls failed exact dimensions and alpha; the Foreground also visibly contains
a baked checker pattern. Changed size prevents a meaningful same-coordinate pixel
preservation test. No resizing/keying was used to conceal these failures. Outputs
and original tool metadata paths remain local; these checked-in PNGs are unchanged
copies, not corrected exports. No runtime PNG, renderer, world or display setting
was changed. The game was not relaunched for rejected files.

`Tools/Test-BackgroundReplacement.ps1` is a new read-only pre-install check for
dimensions, real transparency, hard alpha and preservation outside explicitly
permitted pixel rectangles. Station/Foreground-report.json retain its actual failed
CLI results and both original/candidate SHA256 hashes. An unchanged runtime Station
passes this narrow export gate while still failing the separate branch-quality
probe; that difference is deliberate. Export correctness does not approve art.

`Tools/Test-BackgroundReplacementValidator.ps1` passes two positive cases
(identical pixels, edits inside the allowed rectangle) and rejects four deliberate
defects through the real CLI (wrong size, opaque backdrop, partial alpha, and edits
outside the approved rectangle). Its tiny temporary diagnostic fixtures are
removed after verification; no game artwork is used as a mutable test fixture.

The existing `Inspect-WastesCutoutFeedback.ps1 -RequireClean` remains red against
the unchanged runtime files: five left and seven right Station branch islands,
and 389 pale Foreground boundary candidates. Not every pale material pixel is bad.
The existing magenta key discards dark branch-edge colors; white-matte extraction
also leaves pale silhouette contamination. Precise repairs must preserve untouched
native pixels, not regenerate/rescale the complete landscape.

The one-pass stopping rule is now reached for this image-edit attempt. Park these
failed candidates. Next proposed action is deterministic pixel-level repair in the
existing export tooling, with explicit approval for that editing method before use;
do not start another whole-image generation loop or promote dependent biome art.

## Routing audit (source evidence, not a new live test)

- HighDefinitionSurfaceBackgroundRenderer.DrawV0 dispatches to Wastes V1 only for
  RuinedBackgroundBiome.Forest and the existing QA-world/Forest-lab condition.
- Real Maw detection takes priority and selects EngraftRuinedBackgroundStyle;
  Engraft is the legacy internal name, not shared Wastes artwork.
- Jungle, snow, desert, evil/Hallow, ocean and mushroom have separate surface
  selectors. Nine V0 families exist but remain provisional art, not this candidate.
- Local restored forest returns the incoming native style. The 25 restoration
  policy checks pass; that is not new live transition/render proof.
- Underground uses separate biome/depth slots; the Ruined Deep has its own sky
  compositor. Existing third-party selections are preserved outside forced QA.
- Forced Forest camera fixtures deliberately bypass normal biome selection and
  must never be presented as proof that every region naturally selects Wastes.

Full baseline acceptance, live transitions, candidate cutouts and production
promotion remain open. Prior dca2d9e terrain-anchor proof is unchanged.
