# Mushroom V0 ImageGen source notes

## Intent

Mushroom V0 depicts bioluminescent fungal reclamation consuming a failed hydroponics and water-treatment campus. ImageGen established the project-bound `V0-Day-source.png` concept and decomposed it into three depth-specific sources. `Tools/New-MushroomBackgroundPrototype.ps1` performs deterministic alpha/checker removal, crop, hard-palette reduction, nearest-neighbor scaling, clipped vertical placement, lower-edge closure, and exact Terraria export.

## Far extraction prompt role

Create only a cobalt fungal-fog skyline: broken hydroponics towers, low greenhouse domes, giant mushroom silhouettes of varied heights, ventilation stacks, and sparse suspended spores. Keep the upper field open.

## Mid extraction prompt role

Create the readable reclamation campus: collapsed conservatory, broken nutrient-service gantry, rusted treatment tanks and pipes, smaller ruined dome, and varied luminous mushrooms growing through the infrastructure. Preserve a broad lower viewing lane.

## Close extraction prompt role

Create only the dark framing rim: broken nutrient pipes, cracked planters, mycelial banks, snapped greenhouse ribs, dangling roots, and small luminous caps. Keep both sides taller than the central combat lane.

## Project-bound sources

- `V0-Day-source.png`
- `V0-Far-extraction-v1.png`
- `V0-Mid-extraction-v1.png`
- `V0-Close-extraction-v1.png`
- `V0-Mid-extraction-v2.png`
- `V0-Close-extraction-v2.png`

Far and close use true alpha. Mid uses a baked bright-neutral checker preview. The converter handles both contracts while preserving pale cyan highlights. Production files must match diagnostic candidates byte-for-byte before approval.

## Live-render correction

The first renderer pass was rejected because tall objects in both v1 mid and close extractions touched the source's top edge. Terraria exposed those source crops as flat horizontal cutoffs. The v2 edits preserve the same campus and side-framing composition while shortening/recomposing all tall objects below a transparent headroom band and removing isolated floating pixels. The deterministic converter uses v2 for mid and close and restores a small positive atlas offset, so the first visible pixel is an authored curved silhouette rather than a clipped image boundary.
