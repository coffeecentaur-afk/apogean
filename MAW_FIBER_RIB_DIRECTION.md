# Maw living fiber and structural ribs

September 11, 2026 user review after the two-hour native material pass.
Design intake and implementation brief. The initial intake was not runtime
acceptance; the scoped implementation checkpoint below is now available.
Wayfinder #26 remains open. Keep the accepted harsh material palette and the
earlier red native evidence.

## Implementation checkpoint — same-day continuation

The two bone/study-fiber and bone/study-membrane internal join defects are fixed
without changing artwork:420 native trials, missing-rule negative controls and
the six-request suite pass. See MawPlayable-2026-09-11/JOIN_CHECKPOINT.md.
One new fiber/rib study at7596,240 uses actual MawDirt/MawGrass:247 grown grass
cells,63 unchanged bone cells, all6912 cell states preserved after save/reopen.
It has no production growth hook, vines or new worldgen. The new bone-to-soil/
grass root-edge openings, thicker grass artwork and actual traversal remain
open. Art/Validation/MawFiberRib-2026-09-11/README.md owns evidence and next steps.

## Confirmed direction

- Fiber should read primarily as the Maw's living grass, not another isolated
  block of differently textured rock. Thicken the top-surface soil coat, reduce
  its choppy repeated edge, and let the growth wrap exposed floors, side faces
  and ceilings. Fibrous hanging vines are a desired extension.
- Blend adjacent converted materials coherently, including bone/fiber contacts.
  The remaining openings in the current mixed fixture are still defects.
- Pale bone is structural: cavern shell, arches, ribs and bases for larger
  spikes. Bone footholds are allowed; the separate small teeth are the contact
  hazards clustered near the Gullet walls.
- A rib-cage motif may continue down toward the Underworld. The Stomach and its
  enclosed intestinal outlet remain; this is not permission to open a straight
  shaft through the boss cavity into Hell or relocate its saved reservation.
- Descent should reward preparation and remain dangerous. A regular sequence
  of easy, harmless rib-to-rib drops must not solve the whole route. Rope,
  breaking teeth, hooks and suitable mobility/fall-protection equipment remain
  legitimate player solutions. No compulsory damage or inventory-check gate.
- The user likes the overall material direction and suggests better-blending
  and lighting/shadow mods as aesthetic references and potential companions.
  No dependency, mod installation or shader setting has been approved by this
  suggestion alone; compatibility must be tested rather than asserted.

This refines the old **no safe ledges** wording: no manufactured resting
platforms, preplaced rope or regularly spaced safety staircase. It does NOT
ban an occasional naturally usable part of a structural rib. Supporting bone
remains harmless on contact; visible teeth own spike damage.

## Existing implementation, inspected at aaf59dc

- `Content/Tiles/WorldTerrainTiles.cs` already defines separate MawDirt,
  MawGrass and Mawstone-related terrain contracts. MawGrass uses MawDirt as
  its framing substrate. These are not one universal block.
- `Content/World/MawConversionSystem.cs` maps ordinary dirt to MawDirt and
  existing grass to MawGrass. Converting bare dirt alone does not automatically
  implement subsequent living surface growth. Tile conversion and background
  wall conversion are separately registered.
- The natural-preview fiber, membrane and amber samples remain diagnostic
  studies. Their material appearance is not a working vine/grass ecosystem.
- `EngraftSystem.PostUpdateWorld` currently requires at least one Node; it
  attempts local conversion every 900 awake / 5400 dormant game ticks. This
  falls short of the Bible's extremely slow intrinsic growth without Nodes.
  Record and repair that separately; do not silently call the existing loop
  a completed passive-frontier implementation.
- `OssuaryBone` is solid, non-emissive, has MinPick 59 and allows explosives.
  Its material fixture does not prove a complete rib's appearance or traversal.
- `MawRuptureGenerator.PlaceGulletShelves` still makes alternating shelves
  approximately every 28 tiles. `MawRuptureValidationReport.Passed` still uses
  a 120-tile maximum-fall gate. Neither is approval of the new rib layout or
  equipment-sensitive difficulty. Replace generator and validator together
  only after a native shallow-rib proof; preserve the historical evidence.

## Recommended implementation boundaries

1. **Host terrain:** retain soil, stone, bone, sand, mud and ice identities,
   mining requirements and conversion behavior. Do not turn all stone into
   easy soil merely to obtain grass framing. No whole-world repaint.
2. **Surface growth:** first prove fiber-coated MawDirt on all exposed faces
   using MawGrass's real native framing, including slopes and concave corners.
   Thicker means connected, rooted pixel clusters, not a disconnected fringe
   or an expanded invisible collider. A later sparse coating on stone/bone
   must preserve its host tile and prove its own renderer/merge contract.
3. **Hanging strands:** a separate cuttable vine family anchored to suitable
   underside growth, with a bounded length and growth budget. Recommended
   default: non-solid and non-damaging, not a supplied climbing rope. Grabbing,
   damage or rope-harvest behavior would be separate gameplay decisions.
4. **True background walls:** the vertical face of a solid block is not a
   Terraria `ModWall`. Keep those two meanings separate in implementation.
   Fibrous unsafe-wall treatments need their own overlapping wall atlas and
   conversion proof. Never consume player housing walls or smear foliage across
   open air to hide terrain gaps.
5. **Ecology:** distinguish local grass/vine maturation on already-Maw ground
   from conversion of clean land. Both require bounded authoritative updates,
   save/reload and server/client checks; decorative vines must not become an
   unplanned shortcut across containment trenches. Dormancy reduces activity,
   never purifies or deletes existing fibers. Honor the agreed spread controls
   and world-edit protections without treating their implementation as proven.
6. **Ribs:** short broken arcs, varied attachment depth/length/spacing, uneven
   upper surfaces and exposed ends. Some have useful footing; some are only
   wall structure. Teeth remain readable and separately removable. No rhythmic
   alternating ladder, invisible hazards or forced tooth on every possible
   foothold. Players may engineer their own safe route.

## Native checks before expansion

First finish the observed interface defects using
`Art/Validation/MawPlayable-2026-09-11/NEXT_JOIN_CHECK.md`. Do not rebuild either
saved material fixture or cover holes with an opaque backing wall.

Then ONE separate shallow proof combines these cases:

| Check | Required observation |
| --- | --- |
| Four-face grass | Actual MawDirt/MawGrass, exposed floor/both sides/ceiling, all slopes, half blocks, concave joins and neighbor orders; thick top rooted into soil |
| Mixed materials | Soil/grass/stone/bone/fiber interfaces without sky holes, square outlines or hidden duplicate material draws |
| Vine lifecycle | Valid underside root, absent/invalid support rejection, bounded growth, cut middle/root, cleanup, no floating segments or extra drops |
| Containment | No unintended spread through air, ropes, furniture, housing walls or protected footprints; boundary and dormancy cases tested |
| Purification | Existing two stages plus painted/actuated/sloped cells; cut/purify support cannot leave orphaned hanging strands |
| Plain rendering | No shader or blending mod required for correct silhouettes, frame selection, lighting and hazard visibility |
| Optional visuals | Same saved scene without companions, each separately, then together; versions/settings recorded, frame time measured, gameplay and capture inspected |
| Persistence | Same saved positions/styles after reopening; server-authoritative growth and joins separately tested in multiplayer |
| Rib traversal | Same shallow geometry with unprepared character, deliberate rope/tooth-mining route, hook, double jump, and fall protection tested separately |

Traversal must exercise actual player movement/collision/fall damage, not infer
safety from a two-by-three flood fill or a vertical-distance number alone.
An unprepared player may skillfully avoid harm; do not script unavoidable
damage. Conversely, a fall-protection test must not silently grant immunity to
tooth contact. Do not assume every double jump or accessory behaves identically.

Art direction still gets user review. Renderer, support, growth and route
evidence are development responsibilities, not repeated aesthetic questions.

## Research / next action

`RESEARCH_MAW_FIBER_BLENDING_2026-09-11.md` records primary-source engine and
companion-mod findings. Borrow methods, not assets. Next coding work is the new
study's attachment-edge repair, then the thicker grass/native art pass. The
growth/rib checkpoint does not promote the material gallery to world generation.
