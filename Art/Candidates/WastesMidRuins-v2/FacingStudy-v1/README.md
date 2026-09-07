# Checkpoint opposite-side study

One built-in image edit, exact prompt in `PROMPT.md`, original output retained.
It exposes the left side wall with upper-left lighting instead of flipping the
whole original. New output is opaque and changed the lower cliff despite the
prompt: it is NOT a runtime asset.

`New-CheckpointFacingCandidate.ps1` uses only its upper740 rows and restores
all796 lower rows from the old clean Checkpoint. `Mask-Proposal` retains the
unreviewed dark-component proposal. `Selected-v1/mask-record.json` records the
agent-selected exterior/open-window/railing IDs. Dark doors, recesses and glass
stay opaque. This is source-specific, not a general black color key.
No erosion or global recolor.

Final component IDs:1,2,3,7,13,155,165,270,271,311,313,403,417,490,520.
Run the recipe in a NEW directory, then the existing masked exporter with its
source and mask hashes. `Selected-v1` retains that export and sidecar.
Final transparent SHA256:
`4C4E6419ABCD1A6545AC3CB28AFE70A44627389627778ED67DAC344BC7F4699F`.

`Test-WastesMidScaleStudy.ps1` verifies every composite, mask and export pixel
and lower-cliff invariance. Native studies use this cutout, not opaque output.
Original-concept approval is separate from facing/size approval. Native proof:
`Art/Validation/WastesMidScale-2026-09-06/README.md`.
