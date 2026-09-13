# Maw pass — what to review next

The Maw now has a saved shallow traversal study, a separate smoother-rib
comparison, matching material/item tests and measured local growth workloads.
These are QA studies, **not a finished or newly generated production biome**.
No new world is needed for review. Use Apogee Native Visual V3 only; I can
position the existing scenes when you return.

The authorized work window ends September12 at11:46:58PM Central
(September13 04:46:58UTC). Exact technical evidence is in
[the pass ledger](MAW_AUTONOMOUS_PASS_2026-09-12.md).
Unattended work was paused at10:59PM Central at the review boundary; the QA
world is saved, the game is closed and the heartbeat is paused.

## First: rib shape and material

Open the [medium rib comparison](Art/Validation/MawRibContour-2026-09-12/n-medium-inspection.png),
then the [short](Art/Validation/MawRibContour-2026-09-12/n-short-inspection.png)
and [long](Art/Validation/MawRibContour-2026-09-12/n-long-inspection.png) versions.
In each image, left to right:
**old right-facing, new right-facing, old left-facing, new left-facing**.
These are actual16px-per-tile game captures with the starter player for scale.
The lamps are diagnostic, not the Maw's natural lighting.

The new underside removes square notches and ends in native hammered points.
My assessment: the outline is better, but the pale internal bands and dark
porous-root join can still read as separate layers. Does it look like one rib
to you? No speculative replacement texture has overwritten the accepted bank.
This study is separate from the saved shallow scene and is not yet worldgen.

## Second: shallow passage readability and navigation

Use **Maw QA Plain** for the clean starter-player comparison. Do not rebuild
the scene or use a regular world. The existing shallow study has
three irregular ribs, eight tooth clusters, thick upper fiber, matching walls,
amber organs and one winding side connector. Temporary inspection lamps are
clearly distinguished from actual awake/dormant amber light.

The entry and downward connector pass bounded native motion tests. Two
six-second jump-only return attempts did not reach the upper exit; that does
not prove it impossible or impose a specific equipment requirement. A separate
native comparison confirms climbing preplaced ordinary rope beside the rib
matches vanilla support. Actual placement and full-route return still need
testing. No rope or safety staircase was added to production generation.

The lower rib/pocket are still hard to read under natural light. Inspect the
[lit pocket](Art/Validation/MawShallow-2026-09-12/j-pocket-inspection.png) as an
anatomy reference, not a natural-light pass. In game, check whether teeth are
legible with ordinary player lighting, the thick fiber blends into soil/walls,
and the narrow connector feels dangerous without an unavoidable hit.

## Third: small material icons and actual placement

The [native icon comparison](Art/Validation/MawItemPreview-2026-09-13/aa-native-icons.jpg)
shows rib/full-fiber beside vanilla stone/dirt at the same slot scale. The
matching drop bindings pass two native mine–replace–mine trials, one matching
item per break. The icon panel itself grants nothing and does not prove mouse
pickup, ground-item rendering or normal player placement.

For the next placement test, put an **ordinary Rope stack in gg's hotbar**.
The harness correctly refused without one; bow ammunition was initially
mistaken for rope. I did not grant items or label that refusal a placement pass.

## What improved underneath

- A naturally reproduced background cache mismatch could cause division by zero.
  The owned selector now uses loaded dimensions or refuses unavailable textures;
  native replay and the deliberately invalid-cache control passed.
- A copied-world headless save silently omitted two QA metadata records. The
  narrow save/load ownership fix preserves all16 existing records through native
  save/exit/reload and later Plain-character saves. No ordinary save was replaced.
- Historical accounting now separates9 known fixture locations from9 absent
  ones. Sixteen existing saved-system records remained unchanged in the latest
  native save. This is not whole-world equality, and missing history is not
  reconstructed. The old grove mismatch and39-versus42 fiber specimen remain
  recorded rather than silently repaired.
- Motion traces preserve failed attempts, health changes and limits. One later
  entry regression began at18HP after a Demon Eye attacked at nighttime spawn;
  it is labeled18→18, not disguised as a100HP test.
- Shared wall assets and cached material lookups reduce duplicated resources
  and hot-path work. Moving-scene timing and small growth/conversion patches
  were measured on this RTX3090/32GB-class machine. The32-change growth test
  sustained that cap for four updates per repeat, not a whole-world stress run.
- QA command senders now preserve pending commands and publish closed complete
  files. Stale tree/fixture tests were repaired against accepted asset hashes;
  no art was altered to make them pass.

## Still provisional

The separate cave-backdrop experiment is parked: its flat/banded appearance
and wet boundaries need work. The broader background-production gate remains
RED for native-detail source size/lower coverage. No low-end certification,
multiplayer gameplay, enemies, bosses or acid expansion was completed here.

Next order: review the rib/readability choices; finish ordinary rope placement,
manual material interaction and a practical return route; then consider one
opt-in fresh-seed entrance—not a whole-biome replacement. The amnesia pod,
corporate progression and later space plan are unchanged.

All QA commands stayed in disposable worlds. Earlier, aga was opened during
an interrupted UI sequence and then saved/exited with your approval; no QA
commands ran there. The latest QA session was normally saved and closed.
