# Arrival pod — flat footing repair

September 8, 2026. User likes Native-v2's appearance but rejects its floating,
curved ground contact. Preserve the approved body/hatch and fix only the footing.
Native-v3 is an isolated QA candidate, not automatic final/production approval.

## Cause and bounded change

Native-v2's bottom row contains only18 opaque pixels of80. Most of the shell
rests several pixels higher. The old `floor >=12` check consequently passes a
visually curved hovering base. The new actual CLI test fails on the old source
with `FLAT_FOOTING_GAP at 8,94`, then passes the repair. Main-body contact must
be uninterrupted across x8..55 on rows94..95; the raised hinged door is separate.

Built-in imagegen edited the underside. `footing-edit.png` is the original
1145x1374 RGB output: its matte is OPAQUE, and its upper-body pixels drifted.
Neither matte nor revised body is used. `New-ArrivalPodFooting.ps1` keeps all
7040 pixels in Native-v2's first88rows EXACTLY and replaces only bottom8rows.
The reviewed interior-metal rectangle (56,1236,964,68) lies entirely within the
generated base. Its center-nearest samples are palette-fitted to32x4 logical
pixels, then expanded2x to64x8 at (2,88). This is explicit lossy component fitting,
not a whole-sprite rescale, color-key extraction or claim of handmade pixels.
Result: unchanged80x96 footprint,19colors,2px grid,64px straight contact.

The 90x108 atlas retains5x6 native cells with16px content and2px padding.
`DrawYOffset` changes0->2 to seat the flat edge into the native soil top's small
0..2px visual recesses, without moving any placement cells, anchors or spawn.
Actual local WastesSoil top-run frames at x18,36,54 were measured; the body
coverage and world appearance still require the native view, not these numbers.
[tModLoader DrawYOffset contract](https://docs.tmodloader.net/docs/stable/class_tile_object_data.html)
and [TileDrawInfo](https://docs.tmodloader.net/docs/stable/class_tile_draw_info.html)
confirm this is a drawing offset, separate from the placed cells. The2px value
is our measured material fit, not a universal furniture rule.

## Reproducible gates

`pwsh -NoProfile -File Tools/Test-ArrivalPodFooting.ps1`:

- Old curved source and an isolated2x2 contact-hole mutation must fail through
  the real flat-foot CLI; new candidate passes all alpha/grid/atlas checks.
- All7040 upper pixels match the previously reviewed source.
- Generator replay matches both PNG hashes; existing output and production
  Content destinations refused; original source/candidate pins unchanged.
- Full old Native-v1/v2 pipeline also rerun: both exports reproduce,11 old
  defects and3 coarse-grid controls rejected,19 pins preserved.

The isolated build pins v3 explicitly and requires the new footing gate. Native
matrix checks the revised2px registered offset. Live evidence belongs in
`Art/Validation/ArrivalPod-Footing-2026-09-08/`, not the source illustration.

| File | SHA256 |
| --- | --- |
| Original v2 native | 822A50B8EE634A671281F2CFC5AA3EE760031E0DA3757246C8216911D83B6407 |
| Generated edit | 83936064AFE1D83FEE4859820301A3A8381DD4B69F525610ED54D0B58D352660 |
| v3 native | 8F0C3E52F1367B8B2AFD71FD490954AA6504C1FDFFB2A173070C7E15E51A912D |
| v3 atlas | 9EA7FE54A2B75C175AF995D7848B8EB9FEBE53BA3F8457525DBDE3C7EADAD678 |

## Exact built-in edit prompt

Use case: precise-object-edit. Edit target: the provided 80x96 native pixel-art broken arrival pod. Change ONLY the bottom 16 native pixel rows of its underside. Preserve the approved upper pod, opened right-hand attached hatch, damaged roof, seat, silhouette, muted warm gray palette, and very coarse 2x2 native pixel clusters. The problem is its curved underbelly only touches the floor in the middle, appearing to float. Make the existing crushed metal heat shield/base terminate in a broad straight horizontal bottom, so the main shell visibly rests flush on flat Terraria terrain. A flat dark worn metal underside, NOT a new rectangular plinth, extra feet or a dirt mound. At native coordinates the straight solid contact should span at least x8..55 at y94..95; no dangling fragments below. Keep the upper 80 rows unchanged, all existing art at the same coordinates. Show only this one isolated sprite on true transparent alpha, no checkerboard, shadow, ground, labels, frame, or new objects. Output as an exact integer-enlarged nearest-neighbor view of the 80x96 canvas (e.g. 800x960), retaining the original native coarse pixel structure with no new fine details, no smoothing or realistic rendering. This is a game-ready art repair, not a new design.
