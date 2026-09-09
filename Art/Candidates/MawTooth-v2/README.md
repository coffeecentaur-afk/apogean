# Slim intact tooth — art review, September 9

**Contracted, static art checks pass; user review pending. Not installed.**
The user rejected v1's complete tooth because it looked like a broken fragment.
Its source had been recontoured into a broad native collision wedge. Retain the
old mechanical evidence, but do not promote or relabel that appearance as an
approved broken-tooth variant.

Current direction: one-block-wide canvas, a sharp leaning tip, continuous taper,
long lower shaft and an embedded root. Original weathered ivory/gray/brown with
restrained amber; no square pedestal, broad shard, glow or white speckling.

## What to review

`Native-v1/review.png` displays exact exported pixels at1x and3x nearest-neighbor.
Left: whole tooth. Center: the same tooth with four root pixels hidden by actual
approved bone/Maw dirt atlas samples. Right:42px player-height ruler, not a
rendered character. `native-assembly.png` is the unlabelled1x280x100 composition.
This is an OFFLINE assembly, not a screenshot or engine framing/lighting proof.
The terrain strip is a scale/material reference, not a proposed safe ledge.

The sprite canvas is16x48; its actual opaque bounding width is9px,244 opaque
pixels and8 colors. It fits within one tile rather than being stretched to fill
every horizontal pixel. Its tip and lower shaft survive the same sampling scale.
The narrowness still needs visual judgment; an automated width test cannot
decide whether it reads well in play.

## Provenance and fitting

Built-in image generation, no paid API fallback. Full exact prompt: `prompt.txt`.
Original source `source.png`:724x2172, SHA256
`C29FC9C9CD74EA976CADFD866E5513B98C618D30F6F27810A9B8E4A321FE5B7A`.
The source has real alpha, including partial-alpha interior/edge pixels; it is
NOT native-size hard-alpha art. Its requested logical pixel grid was not exact.

`Tools/New-MawSlimToothStudy.ps1` uses the source alpha128 boundary, bounding
box201,128,370,1949, then ONE uniform nearest sample grid (40.6041667 source
pixels per output pixel), centered in16x48. It fits8 muted colors and binarizes
alpha. No source-row warping or collision-derived silhouette. Reduction/palette
fitting are lossy preparation, not a lossless extraction claim. The existing
explicit-mask exporter then preserves those prepared pixels exactly, with
alpha0/RGB0 elsewhere. No checkerboard removal or new external dependency.

Tooth SHA256: `E2C5E9D6EEB2D6D3EA81DE2B3E26C5532D2BD1449847730838E4B24BFC3DA903`.
Source, sampler, terrain and output pins: `Native-v1/recipe.json`.
Mask/export assertions: `Native-v1/tooth.png.report.json`.

## Static proof and engine boundary

The validation CLI initially failed on missing candidate art. Final checks pass:
16x48 canvas, unbroken connected rows, sharp tip, continuous lower shaft, no
holes, binary alpha, zero hidden matte RGB,8 colors and exact reassembly from
the three16px cells of the18x54 padded upright art sheet. Actual CLI negative
controls reject widened dimensions, a severed row, blunt tip, soft alpha, white
speckle, opaque atlas padding and a missing atlas pixel. Candidate unchanged.
These detect specific structural defects, not automatic artistic quality.

`upright-art-atlas.png` is an ART-ONLY framing study. It is deliberately NOT
compatible with the old54x216 four-way `MawFangTile` contract and is never copied
into Content. No new build, runtime registration, collision code, orientation
variants, saved-world mutation, live test or multiplayer claim in this pass.
Installed package remains6A5670B5. Accepted Wastes/pod/bone assets unchanged.

Next: user review, then a bounded native rendering/physical-shape probe. The
old45-degree wedge cannot certify this long slim taper. Do not restore the wide
shape to make old tests pass or introduce an invisible full-column wall. Any
necessary collision compromise needs explicit review before integration. Keep
easy mining/one object/contact damage and the no-safe-ledges direction; no
generation/acid/entity expansion until that gate.

## Reproduce

```powershell
pwsh -NoProfile -File Tools/New-MawSlimToothStudy.ps1 -OutputDirectory 'ABSOLUTE-REPO/Art/Candidates/MawTooth-v2/NEW-DIRECTORY'
pwsh -NoProfile -File Tools/Test-MawSlimTooth.ps1 -CandidateDirectory Art/Candidates/MawTooth-v2/Native-v1
pwsh -NoProfile -File Tools/Test-MawSlimToothValidator.ps1
```

The generator refuses existing outputs and paths outside this candidate family.
Mutation tests retain only disposable copies under a unique OS temp directory.
