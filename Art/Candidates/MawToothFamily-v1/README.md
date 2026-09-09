# Maw tooth family v1 — offline art review

September 9, 2026. User approves a short/long/wide family direction.
These exported silhouettes need review. No runtime content, installed package,
QA world or ordinary world was changed.

## Review

Open `Native-v1/review.png`: top is 1x exported pixels, bottom is a nearest-
neighbor 2x enlargement of the same hand-assembled bank. Left to right: short,
short, long, wide, short. The 42px ruler is a scale guide, not a rendered player.
Four root rows are buried. Existing Maw dirt and approved quiet bone provide
context. This is NOT a game screenshot, generation/physics proof, final socket
morphology or a proposal for safe ledges.

| Variant | Canvas pixels | Opaque width | Opaque pixels | Art-only atlas |
| --- | --- | --- | --- | --- |
| Short | 16x32 | 10 | 163 | 18x36 |
| Long | 16x48 | 9 | 244 | 18x54 |
| Wide | 32x64 | 20 | 664 | 36x72 |

All share eight colors. Long is pixel-identical to the earlier slim V2 study.
Wide is a complete curved fang, not the rejected collision wedge. Canvas size
does not define solid occupancy.

## Source and mask provenance

Short and wide are original edits made with built-in imagegen, referencing
`../MawTooth-v2/source.png`. Exact prompts: `short-prompt.txt`,
`wide-prompt.txt`. Original `*-source.png` files are retained.
Both returned baked checkerboards, NOT actual transparency.

For these two inspected sources only, `MawToothFamilyStudy.ProposeMask`
selects the largest four-connected warm component and fills each occupied
row's envelope to retain internal pale bone. This is a source-specific mask
proposal, not a general background remover or rule for grayscale materials.
Both source cutouts were visually inspected before native fitting.
The existing exact-mask exporter retains source colors inside reviewed binary
masks and zeroes outside RGB. Cutouts, masks and exporter reports are retained.

Native fitting uniformly samples original source pixels inside those masks
and fits the existing eight-color palette. No row stretching or collision-wedge
recontouring. Wide uses uniform horizontal sample phase 0.7 because phase 0.5
missed its narrow tip. The phase applies to the whole sprite, not selected rows.
Short/long retain phase 0.5. `Native-v1/recipe.json` pins sources, masks, output
sprites, fitting parameters and bank materials.

## Reproduction and checks

`Tools/New-MawToothFamilyStudy.ps1 -Stage Masks` proposes/exports cutouts.
Inspect before `-Stage Native`. Both stages refuse existing candidates; do
not delete reviewed work to rerun. A future revision needs a separately named
output. Existing `MawSlimToothStudy` now supports explicit source masks and
dimensions/phase while preserving its default long output exactly:
`E2C5E9D6EEB2D6D3EA81DE2B3E26C5532D2BD1449847730838E4B24BFC3DA903`.
Art atlases have two-pixel gutters. They are NOT drop-in replacements for the
old four-direction runtime atlas.

All three pass the actual `Test-MawSlimTooth.ps1` CLI with explicit width/
height: dimensions, hard alpha, transparent RGB, palette, intact narrow tip,
continuous shaft and exact atlas reassembly.
`Test-MawSlimToothValidator.ps1` passes separately for all three: one positive
and seven rejected corruptions each (blunt tip, padding, shifted atlas, soft
alpha, severed row, white speckle, wrong width). That is 21 rejected defects;
it is art validation, not native gameplay proof.

Retained failures:

- `Native-null-mask-failed/`: short passed; long aborted on PowerShell's
  empty-string representation of an absent mask. Fixed null-or-empty handling.
- `Native-tip-sample-failed/`: short/long passed; wide failed
  `SEVERED_TOOTH_ROW_0`. Whole-image sample phase correction fixes the export.

## Next gate

User shape review, then matching native render, solid/hurt region, anchoring,
whole-object mining, support loss, save/reload and orientation checks.
The rejected broad wedge's tests do not certify these shapes. No invisible
full-column collider or silent broadening to fit native slopes.

Only afterward trial 50% short / 35% long / 15% wide, seeded bounded placement,
irregular rooted clusters and inward orientation. These weights are provisional,
not implemented or final tuning. Dense teeth, easy removal, no generated safe
ledges/ropes, harmless structural bone. Preserve accepted Wastes/pod/bone art
and known grove reload failure. Acid, entities and bosses remain later slices.
