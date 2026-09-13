# Drawing-led Maw: fresh-seed integration candidate

Source direction: MAW_SKETCH_STUDY_2026-09-13.md. The approved V1 scene and its
assets remain unchanged. This pass transfers its anatomy into a seed-aware
upper-Gullet plan, not another asset redesign or a whole-biome completion claim.

## Scope

- A 280x256-tile bounded plan with native tile-sized geometry, a raised irregular
  opening, continuous branching ribs, dense supported four-way thorn clusters,
  thick fiber, sparse side-path amber and short hanging strands.
- Two more widely separated cave groups, one empty cache marker and a smaller
  Node reservation reached through a winding connection. The reservation does
  not create a spreading Node, change the seven-node budget or summon a boss.
- Sampled surface shoulders and a saved route into the existing deep Gullet.
  Active cells describe the write mask; untouched cells are not rectangular
  excavation. Surface native-wall coverage is separate from terrain coverage.
- Only explicitly named fresh `Apogean Maw Seed QA <number>` worlds in the
  accepted packed/anatomy QA build may generate this candidate. Existing saves,
  the approved QA mockup and ordinary new worlds retain their current behavior.
- Deep generation and Stomach/intestinal anatomy remain the legacy path in this
  slice. Backgrounds, new enemies, loot tables and portable acid are not added.

## Required evidence

1. Deterministic plan repeats, several slope/route profiles and negative controls
   for disconnected ribs, wall fringe, blocked routes, objects and pocket roles.
2. Full affected-region preflight before reservation and landmark placement;
   finite candidate search; exact native terrain, slope and object verification.
3. Isolated fresh Large worlds through the installed native server, then actual
   client rendering of a generated candidate. Default isolated mod configs and
   Apogean plus CheatSheet are not a Calamity compatibility test.
4. Native save/reload, timings and memory with scope reported. A 2x3-body flood
   fill proves connected space, not jump physics, ropes, returnability or balance.
5. User gameplay-scale review before normal-world integration. No requirement
   to bury every bone root; no supplied ropes or manufactured safety staircase.

## Review controls

`/mawseed entrance`, `ribs`, `cache`, `node`, `outlet` visit existing positions
once with free movement. `light-on`/`light-off` control a labelled local inspection
lamp, not production brightness. `capture` requests a native capture. `release`
restores the pre-visit position/time. No construction command is exposed.
Review is restricted to gg/Plain, single player and the named generated test
worlds. The request bridge accepts only the closed review allowlist plus save/exit.

## Status

The first native candidate (seed202) generates and survives a server save and
client reload with its original terrain digest unchanged. Gameplay-scale
entrance/rib/cache/node views were inspected. This is NOT production readiness:
seeds101/303 still reject safely during the finite site search, the old outer
Maw shell meets the new materials abruptly, and surface fiber has small bright
seam artifacts. Do not ask the user to approve those defects as final polish.
No new art approval or whole-biome completion is claimed. The final safety build
repeats seed202 with the identical original digest. Seed404 reaches construction
but rejects an unowned non-frame-state change during framing; its failed trial
is retained, not accepted or repaired/rebaselined into a passing world.

### Native candidate evidence

- Final package client reload: saved digest passes at17:30:54 after the earlier
  native client save/exit; the saved digest still equals original685AAD06. No
  generation hook ran during load. This is the generated202 world, not the V3
  sketch fixture. Client resource packs remained as the user configured them;
  no claim of a clean resource-pack compatibility matrix.
  The full pristine geometry check also passes at17:31:20. The final viewport
  capture was opened and inspected: continuous nearby rib/tooth shapes are
  readable, but most of the underground view is naturally dark beyond the lamp.
  It does not establish whole-scene alpha coverage or lighting polish.
- Final installed package:
  `64204068913BF26BBE7CFB6B35132FAA54E878C15EAD3FEBAA914A9EB5285372`.
  Final seed202 replay passes the retained impact-boundary and owned-only rollback
  code, then saves cleanly. Build: zero errors, two pre-existing PaintID warnings.
  Source checks:10 contracts pass;19/19 actual-source negative controls reject.
  Source hash `99FEF7A48EDC8B7C920580F7155B3ADAE3F154F1D82BB4FB9F1179FAE446706E`.
  These checks do not prove runtime behavior on other seeds.
- Final trials: Temp/ApogeanMawSeeds-23c1de7e3c6b45c9b711298079965d2a.
  Seed202 generation arm17:26:14.709 to PostWorldGen17:26:55.472, about40.8s;
  this interval includes whole-world generation and other mod passes, not just
  this280x256 slice. Seed404 rejects at2866,637 after framing; equal tile IDs in
  the error do not imply equal native state. Diagnose the differing field next.
- First successful package6B84A315, seed202: bounds5424,361 /280x256;
  28,973 planned cells,1,606 object cells,37,670 frame/light-hook checks,
  13,158 reachable body positions. One empty cache and one reserved Node pocket.
- Original digest685AAD06082CA3F56E1E81EE57DFE37D5767C3C4C94EC987B2FEB38D5A7EEC73
  matches at PostWorldGen and after loading the copied world in the native client.
  Full legacy deep route also passes, with41 tiles Stomach clearance. Maximum
  fall212 remains diagnostic: no rope/grapple/manual-returnability certification.
- Whole-region native capture20260913-222342 is mostly unlit/offscreen; retained
  as a failed visual-evidence attempt, not a complete overview. The review tool
  now captures only the actually visible candidate region, labels inspection
  lighting, and never changes tile paint/fullbright state.
- Review world is a NEW pair named Apogean_Maw_Seed_QA_202 in Worlds. Neither
  the accepted Apogee Native Visual V3 nor regular worlds were opened or edited.
- First pass logs: Temp/ApogeanMawSeeds-5f04f1b854ab412abefec594b3c69d51/seed-202.
  Seed101/303 failures: Temp/ApogeanMawSeeds-61c61c080f714f6fb8d392cd8190881d.
  Seed101 retains a real pressure-plate protection rejection; neither trial
  weakens structure/trap protection to force placement.
- Client observation while playing this large world: roughly2.5GiB working set,
  2.62GiB peak sampled. This is not a minimum hardware requirement, an allocation
  benchmark, an FPS result or a multiplayer stress test.

### Boundary corrections retained

- Frame-important does not mean furniture: explicitly classified vanilla rubble,
  herbs, sunflowers and natural drip decoration separately. Unknown mod content,
  containers, wiring, structure walls and reservations remain protected.
- Own natural Maw/Wastes walls are terrain, including walls just made by the
  legacy generator. Other mod wall types remain protected.
- Removing support invalidated half of a two-cell SmallPiles object. The bounded
  edit mask now claims complete neighboring natural-decoration components before
  framing, leaving their background walls unchanged. A2048-cell budget and scope
  edge rejection prevent unbounded cleanup.
- Preflight impact is retained; post-legacy cleanup may not exceed it. Rollback
  restores only owned candidate cells, never damaged protected evidence. Native
  unowned non-frame-state assertions and existing chest signatures remain gates.

### Retained test history

- Pure plan: final source `6AA2BB994A3847EEFA8A93B8E467CF0E1BCF3DD9324EDC01300A1969153A6058`
  passes 292 generated cases across 128 distinct seeds, six route and six shoulder
  profiles; deterministic repeats match. All 31 corrupted-plan and nine invalid
  input controls reject. Runtime:55.77s for the complete pure suite, not worldgen.
- Earlier reports are retained:26 construction failures, then one isolated
  approach-component failure. Fixes attach shallow ribs to actual ground, keep
  the first cave below high shoulders, and select an already connected natural
  inspection arrival. The final fix changes only the arrival coordinate, not
  terrain or hazards. Flood-fill still does not simulate player movement.
- First native package `5397E7856A7F4E0548204508CCEE5430900FA35C768FE0623C0FC4A47417DD7B`
  generated legacy worlds101/202 in39.411s/37.554s. These are **failed integration
  trials**, not candidate passes: the dedicated autocreate path omitted the
  `WorldGen.generatingWorld` flag, silently skipping the new branch.
- Installed runtime DLL `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C`:
  dedicated autocreate calls GenerateWorld directly; CreateNewWorld owns the
  other flag. `gen` is already false after Final Cleanup. A transient identity
  latch armed only by PreWorldGen now gates this writer; load/save never arm it,
  and PostWorldGen rejects a requested but missing candidate and always disarms.
- Package `BEA302838C47EC5373C923AF087A398A9B0B04E9D68C6042DB2E015C96DDCBA8`
  correctly arms native generation, but seeds101/202/303 fail the broad rectangular
  site preflight. No partial candidate save is accepted. The next correction
  distinguishes the edit/impact shape from the larger snapshot rectangle while
  preserving protected structures and the same finite site-search budget.
- First trial archives:
  `%TEMP%/ApogeanMawSeeds-c7d2cbffd6bb48909cecd04d2e1df795` and
  `%TEMP%/ApogeanMawSeeds-cb00e4704b3649b2a547b9dbd94a779d`.
  Existing worlds and the accepted V1 fixture were never opened.
