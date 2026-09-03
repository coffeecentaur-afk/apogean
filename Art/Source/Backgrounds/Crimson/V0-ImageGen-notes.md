# Crimson V0 ImageGen source notes

## Intent

Crimson V0 depicts a ruined medical and research colony that is being physically swallowed by the biome. The authored reference remains `V0-Day-source.png`. ImageGen was used in generation mode to decompose that composition into three depth-specific source plates; `Tools/New-CrimsonBackgroundPrototype.ps1` performs deterministic checker-preview removal, crop, hard-palette reduction, nearest-neighbor scaling, lower-edge closure, and exact Terraria layer export.

## Far extraction prompt role

Create only the distant oppressive layer: a crimson city of narrow broken towers beneath a monumental shadowed central megastructure, with hanging inverted landmasses, organic drips, red cloud haze, and broad empty sky above. Avoid readable near architecture and foreground growth.

## Mid extraction prompt role

Create the readable architecture layer: cylindrical glass observation towers, medical crosses, connected laboratory spans, catwalks, and low arched greenhouse shells invaded by dark crimson biomass, hooked ribs, and tendons. Preserve visible steel and sparse cold laboratory lights, plus a broad central viewing lane.

## Close extraction prompt role

Create only the nearest organic frame: near-black hooked ribs, sinewy roots, thorn spines, fleshy sacs or eyes, and sparse dim red nodes. Tall masses rise mainly at the left and right edges while the middle remains low and open for combat visibility.

## Project-bound sources

- `V0-Far-extraction-v1.png`
- `V0-Mid-extraction-v1.png`
- `V0-Close-extraction-v1.png`

The source family's checker preview contains both light and dark neutral greys. The converter therefore removes low-chroma neutral pixels above a luminance floor while retaining dark steel and red-chroma biomass. Production files must match the diagnostic candidates byte-for-byte before approval.
