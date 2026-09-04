# Ocean V0 ImageGen source notes

## Intent

Ocean V0 depicts a drowned industrial port and failed evacuation route. The authored reference remains `V0-Day-source.png`. ImageGen was used in generation mode to separate that identity into three depth-specific sources; `Tools/New-OceanBackgroundPrototype.ps1` performs deterministic checker removal, crop, hard-palette reduction, nearest-neighbor scaling, lower-edge closure, and exact Terraria export.

## Far extraction prompt role

Create only the distant atmospheric layer: a low hazy harbor skyline with sea towers, skeletal cranes, a broken cable mast, fog-softened platforms, and partially submerged evacuation transports. Keep the upper field open.

## Mid extraction prompt role

Create the readable drowned shipyard: broken container seawall, derelict dock cranes, battered warehouse and control towers, and two wrecked evacuation transports at different angles in storm water. Preserve negative space through the center.

## Close extraction prompt role

Create only the dark framing rim: snapped concrete pilings, twisted rebar, dock machinery, barnacled wreckage, dangling cable, and sparse dead-coral accents. Keep both sides taller than the broad central combat lane.

## Project-bound sources

- `V0-Far-extraction-v1.png`
- `V0-Mid-extraction-v1.png`
- `V0-Close-extraction-v1.png`

The generated files use a baked bright neutral checker preview. The converter removes only very bright near-neutral pixels so surf, steel highlights, and the muted atmospheric silhouettes survive alpha extraction. Production files must match diagnostic candidates byte-for-byte before approval.
