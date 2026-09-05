# A — Snapped / native candidate v2

Review only, **not installed or loaded in tModLoader**. `Native-comparison.png` shows the approved A direction at left, actual candidate sprites assembled at 1× in the middle, and the same cap/socket pixels at 4× on the right. Bottom branches are 3× nearest-neighbor. `Native-assembly.png` is the standalone actual-pixel scene. These are offline engine-contract assemblies, not game screenshots.

## Feedback addressed

- Intact branch shafts now have solid, gently tapered contours. Splintering is confined to the outer four pixels, with no abrupt T-post clipping or chips partway along the branch.
- The trunk contained 74 cream/amber pixels; they are now muted brown in the candidate, as are corresponding top/branch flecks. Five opaque browns remain: `#211916`, `#35261f`, `#523827`, `#735033`, `#967041`. These were baked texture flecks, not actual lighting effects.
- Three top options demonstrate plain bark, a small dark knot, and a deeper recessed hollow. Recesses derive from the revised image source and are opaque wood/shadow texture, not holes through collision or alpha. They occur once in a selected top, not in every repeating trunk cell. Final frequency and visual treatment are not approved.

The user added the bark and hollow feedback during the branch revision, so v2 changes candidate bark colors as well as branches. It does **not** claim the trunk pixels are byte-identical to v1: only their alpha topology and geometry are unchanged. Root size, all top fracture silhouettes, native offsets, growth, chopping, and runtime files are unchanged.

## Native export contract

| Role | Dimensions | Preserved or changed |
| --- | --- | --- |
| Trunk/root/cut | 176×264 | Exact v1/native alpha; only the four bright fleck colors map to existing bark brown. |
| Tops | 246×82 | Three 80×80 cells with 2px gutters; exact v1 alpha and centered wood sockets. Recesses stay inside intact wood above the socket band. |
| Branches | 84×126 | Three pairs of 40×40 cells with 2px gutters. Contours revised; left (40,24) and right (0,30) pivots retain their six-pixel offset. Seven-pixel attachment bands. |

`Tools/New-WastesSnappedACandidate.ps1 -Revision 2` owns this directory; it has no promotion switch. It keeps the v1 cap silhouette source and uses the v2 built-in image edit for branch grain and recesses. The revised image again returned a painted pale checkerboard rather than alpha; the inspected extraction removes it. Source branch columns are fit into explicit continuous pixel contours, not blindly installed as a generated sheet. Existing trunk grain remains, with the identified fleck colors removed. The native rendering roles were verified for v1 and are unchanged here.

Exact image-edit prompt and provenance: `PROMPT.md`. Source: `../../Source/Trees/WastesSnappedA-components-source-v2.png`. No paid API fallback used.

## Validation — 2026-09-04

- `AgentSkills/tmodloader-tree-authoring/scripts/Test-TreeSet.ps1`: PASS against the installed-build `Vanilla-ForestTree-Trunk.png` reference, covering native dimensions, alpha topology, hard alpha, palette, sparse branch/top mass, and connected top sockets.
- `Tools/Test-SnappedTreeRevision.ps1`: PASS for v2. Checks all three sheets against the five-color palette, preserves trunk/top alpha, rejects gaps/pinches/reversals in intact branch contours, and verifies both full mirrored sheets and socket bands.
- The same focused test run against the untouched v1 returns failure for its 74 trunk, 59 top and 48 branch flecks plus jagged intact contour steps. This is a negative regression, not a test failure being waived.
- Runtime assets and C# files were not changed. No build, restart, world modification, or live test was performed for this revision.

SHA-256:

| Candidate | Hash |
| --- | --- |
| Trunk | `B925D5D1BD7FFE4B1315E1D441B393D22CC1CAABBFF605F6DCD53EEBFE396433` |
| Tops | `75BED027B9541217BD04C44525B568B4C16194667A61CAA364929D33CA55F394` |
| Branches | `5665DB9A4181C672B4C94CE936583B2F6A05A2D626EA939A30DEDE792D9749D5` |

## Approval boundary

Wait for the user's review of the actual native assembly, including the quieter bark and proposed recesses, before loading it. Then repeat a disposable live grove with wind, root/terrain contacts, slopes, paint, chopping at several heights, acorn growth, reload, multiplayer observation, and fresh-world spacing. No prior candidate's live results certify this version. Grass and backgrounds remain next in the agreed order, not work to bypass this review.
