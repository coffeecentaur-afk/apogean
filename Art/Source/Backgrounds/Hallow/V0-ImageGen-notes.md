# Hallow V0 ImageGen source notes

## Intent

Hallow V0 depicts an abandoned radio-astronomy campus crystallized into something beautiful and dangerous. The authored reference remains `V0-Day-source.png`. ImageGen was used in generation mode to decompose that composition into three depth-specific sources; `Tools/New-HallowBackgroundPrototype.ps1` performs deterministic layer-specific checker removal, crop, hard-palette reduction, nearest-neighbor scaling, lower-edge closure, and exact Terraria export.

## Far extraction prompt role

Create only the distant atmospheric layer: a pale blue-lavender observatory city with slender antenna towers, bridge fragments, tiny satellite silhouettes, a suspended research island, and sparse pastel crystalline growth. Keep the skyline low and the upper sky open.

## Mid extraction prompt role

Create the readable radio-astronomy campus: communications mast, smaller broken dish, wrecked elevated monorail carriage, central geodesic observatory, low research labs, and a huge shattered dish overtaken by cyan, lavender, pink, and pearl crystals. Preserve a broad viewing lane.

## Close extraction prompt role

Create only the dark framing rim: broken observatory machinery, angular rubble, shattered dish ribs, crystal-tree silhouettes, and sparse bright pastel facets. Tall masses stay at both sides while the central combat view remains low and open.

## Project-bound sources

- `V0-Far-extraction-v1.png`
- `V0-Mid-extraction-v1.png`
- `V0-Close-extraction-v1.png`

Far and mid use bright neutral checker previews; close uses a blue-grey checker. The converter applies a checker contract per layer so pale architecture and crystal highlights survive alpha extraction. Production files must match diagnostic candidates byte-for-byte before approval.
