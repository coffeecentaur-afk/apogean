# Maw bone / brittle teeth — material review, September 9

Status: **new art pending review; offline topology and masked-source probes pass**.
Latest September9 follow-up: user approves the localized cooler slate tissue in
`DepthDirection-v1`, preserving the grey/black/brown/amber foundation. The other
new materials remain proposals. `MaskedSource-v2` and `MaskedNative-v2` revise
only the overly speckled bone material, keeping the compiler/native mask intact.
The five-color result is quieter; `MaskedNative-v2/comparison.png` shows identical
frame arrangements at1x/2x. Art approval/native fixture still pending; NOT installed.
Both v1 and v2 pass eleven pipeline checks, but that does not certify visual
quality. Keep the rejected noisy v1 as a reference. These candidates follow the older
illustration/topology checkpoint below; that illustration is not an atlas.
Wayfinder #26 remains open. Nothing here is installed, generated in a world, or
native-gameplay evidence. The user approved the preceding raised entrance layout
(51 tooth groups; rim crests24/22 tiles above ground), not this new artwork.
No safe ledges: players bring rope or break teeth. Supporting bone stays safe.

## Material study

`Bone-and-teeth-concept.png` is one original built-in-imagegen study; its exact
prompt is in `prompt.txt`. It explores broad ivory/ochre structural bone,
recessed dark pores, grouped cracks, sharp lighter teeth and restrained yellow
fibers at their roots. Snapped ends are variation, not a regrowth mechanic.
The curved fragment is a **material example**, not a replacement mouth layout.
Seven visible concept teeth do not replace the approved51-group composition.

Measured output:1536x1024, fully opaque,63,498 RGB colors. The dark backdrop is
intentional, not transparency. The requested8–12-color budget and exact logical
pixel grid were **not** delivered. The mannequin/sample suggest scale but are
not calibrated game measurements. Reject this image as an engine atlas; do not
crop the whole rib into a terrain overlay or silently call it game-ready.
Its material/silhouette direction can be reviewed independently of those limits.
No mask extraction, resampling or automatic installation was performed.

Image SHA256:
`99DEF7C24850A25D3E4154421C4CDE32C2AB26D8FF6DC748B2FD1A6CCBA1C541`.
Original generation file is preserved in the local Codex generated-images
directory; this project copy keeps the reference durable.

## Separate framing probe

`TopologyProbe/OssuaryBone-topology-probe.png` is **native Stone palette-mapped
as a diagnostic**, NOT the artwork above and NOT a proposed final bone texture.
It exercises the existing deterministic palette compiler with real installed
connected-terrain topology instead of the legacy square-cell generator.
Reference: locally exported `Vanilla-Stone-Tile.png`, from the installed
tModLoader1.4.4-era native atlas export workflow. Pinned reference SHA256:
`48907D0C61D9B68997C33FD25B0BAADB0A2D8276B6E759761C14E6B153C917EC`.
Only supply this locally owned native reference; no download is required.
Other native materials are not assumed to use the same mask.

The proposed structural contract is ordinary connected `ModTile` terrain,
288x270,16px frame bodies at18px pitch,16 columns by15 rows. Preserve every
reference alpha pixel, including unused cells and padding. Actual slopes and
Mawstone merges still require native rendering. Do not infer correct frame
indices or appearance from a color-count/alpha test alone.

Measured probe:44,104 opaque pixels, five colors, no soft alpha,111 contoured
frame bodies,74 distinct populated masks; zero reference-alpha mismatches.
The production OssuaryBone sheet remains the known RED baseline:240 identical
opaque square bodies, no contoured frame. Its current pixels and mining rules
are unchanged.

`Tools/Test-OssuaryBonePipeline.ps1` runs the real validator CLI: two positive
controls and ten negatives (recreated full-square frames, lost pixel, added
pixel, white fringe, both known exporter keys, soft alpha, palette overflow,
empty art and wrong dimensions). The square defect is generated in temporary
storage so fixing production later will not invalidate the regression.
All12 controls pass; each negative must fail for its expected reason.
`TopologyProbe/checks.json` records actual outputs and fingerprints.
The focused validator accepts an optional exact native reference and now rejects
the two known visible exporter keys without changing unrelated family rules.

Reproduce from the repo (PowerShell7, local native export required):

```powershell
pwsh -NoProfile -File Tools/Test-OssuaryBonePipeline.ps1 -ReferenceAtlas 'ABSOLUTE-PATH/Vanilla-Stone-Tile.png' -EvidenceDirectory 'ABSOLUTE-REPO/Art/Candidates/MawBone-v1/TopologyProbe'
node Tools/Build-MawEntranceLayout.cjs
```

The pipeline refuses changed reference fingerprints and output outside its
candidate directory. Negative copies stay in a unique local temporary directory.
It never opens a world, builds/installs a package, or edits Content. Image
generation is not run by this test.

## Next bounded gate

Do not reopen the approved mouth shape or localized slate tissue direction.
Review v2's quieter actual texture, then inspect exact exported frames in the
small native fixture before production promotion. No new full-biome redraw or
mob implementation from this study.
The Stone-based diagnostic must not accidentally become the final bone.

Keep exposed brittle teeth separate from hardened structural bone. Tooth
orientation/support, actual damage boundaries, hurt immunity, removal with a
starter pickaxe, rope placement, drops, save/reload and multiplayer still need
their own small native fixture. No hidden damage rectangle behind a large art
overlay. The approved layout is not proof of these behaviors.

After the bone/tooth fixture, adapt the approved shallow mouth to the saved
route/protection planner and remove legacy safe shelves alongside their old
fall-limit assumption. Leave the Stomach, Wastes/pod/divot and optional acid
branch unchanged. Art approval will not mark #26 fixture-pass or integrated.
