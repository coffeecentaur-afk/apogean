# Maw mixed-terrain verification — September 11

## Work window

User authorizes autonomous testing and implementation from 17:55 to 19:55 UTC
(12:55–14:55 America/Chicago). Only gg / Apogee Native Visual V3 / single-player
may be changed. Preserve ordinary worlds and all earlier fixture checkpoints.

The twelve-material Packed-v2 native gallery was accepted by the user. Keep its
artwork. This pass tests actual mixed terrain, not another style redraw.

## Ordered checks

1. A separate bounded natural material patch; no clearing existing terrain.
2. Paired vanilla/Maw full-grass + slope + soil junctions, mirrored cases,
   half blocks, terraces and material/wall joins. Isolated slopes do not count.
3. Native renderer/frame checks, screenshots and persistence without rebuilding.
4. Fix evidence-supported defects; retain failing captures/negative controls.
5. Continue toward production properties only where those checks support it.

No acid, new mob/boss, new art direction or ordinary-world generation is implied.
Art-only study sand/amber do not prove falling sand or organ lighting. The old
grove reload mismatch remains RED; never clear or rebaseline it.

## Results

Native mixed-context pass at 18:18:56 UTC after a real save, client restart and
reopen. Fixture created ONCE in empty air at X6276 Y420, 128x104 tiles. Original
creation digest remains
`C3675BAB526B888485F7AB5AE951C76778B36E9F222CAA051E5E341B508C846D`.

- 3,697 actual tile draws; 946,432 loaded alpha/export-key pixel comparisons.
- 850 actual wall frames; all twelve materials present.
- Ten paired grass/soil corners: four slope directions in two neighbor orders,
  a half block and a stepped corner. Native screenshots show continuous dirt
  support without the previously reported white corner pinholes.
- No atlas pixels changed: all 466 prior QA PNG/frame-map files match the pinned
  September9 build. This is byte continuity, not approval of every old asset.
- Day, actual reopened day, and night captured. Ordinary material darkens under
  native lighting. Player equipment casts orange light; it is not material glow.

The initial whole-hash test failed twice. Diagnosis: vanilla grass spread and
plants/vines grew in the explicit native comparison bank. A pure replay of the
ORIGINAL layout reproduces the original saved digest; all authored Maw cells,
slopes, coatings, walls and wires still match. The cell-level validator permits
only normal Dirt-to-Grass and plant/vine additions in that control bank (80
cells at the passing check), not arbitrary terrain changes. No hash reset,
fixture rebuild or broad ignore rule was used. Deliberate mutation controls and
shipping block properties are separate tests described below.

### Native mutations and first production run

At18:35 UTC, eight cell controls passed: missing Maw soil, foreign grass in Maw,
paint, altered slope, missing wall, foreign control material, plant outside the
control bank all rejected; a native plant inside the control bank allowed.
Every mutation restored the exact starting state. The log's deliberately caught
exceptions during these controls are expected, not unexplained crashes.

Installed property-test package:
`195C3BD7794F46E63E04A4FAD4E348BD9DBC698AFF3310C13292C418BA8E8EF1`.
Initial shipping-property result: **428/441 pass, 13 fail**. Native placement,
solid/actuated collision, mining, exactly one real dropped item, both purity
types across six shapes, preserved paint/wires, seven wall families, and
protected constructed/corporate material checks pass. Thirteen configured dust
assignments still use AmberBolt on ordinary nonemissive terrain/walls. These
policy failures are retained for the repair/retest, not reset. Falling sand is
only registration-checked here. No simulated bomb, dust-light engine or manual
multiplayer claim is implied. The test uses an initially empty5x5 trial space
inside the fixture, removes only its two created cells/new drops, and verifies
the fixture digest is exactly unchanged afterward.

## Native captures (unaltered copies)

| File | Original capture | SHA256 |
| --- | --- | --- |
| natural-day.png | Apogean Maw Natural 0 20260911-181008.png | 8257AC83CA1C0FB30BB74650D5812F8BCD13FFC5FE6975F73EA3B2E8554519FB |
| corners-day-initial.png | Apogean Maw Natural 1 20260911-181106.png | 2CDB8969C99BD75237B5237EF71CC3886CE01DAA26DBBB343893DF2B307CCF19 |
| corners-day-reopened.png | Apogean Maw Natural 1 20260911-182140.png | 77C4560376AE2F613EF37FE85D5C0F67A0C48ACB8EF817F4A865D8A29869C1A1 |
| corners-night.png | Apogean Maw Natural 1 20260911-182714.png | 499A3B9D4B87EA3BBC143EC1C72C576240EC144A24BC24BFAE04194686C6B0F5 |

Material islands and the grey diagnostic walkways are a test matrix, NOT a new
Gullet generation design or safe ledges. Sand/amber here are inert art studies.
This record does not certify shipping behavior, wall pixel masks, painted
lighting combinations, multiplayer, biome generation or full memory acceptance.

## Repeatable commands

`Tools/Build-CurrentQAPackage.ps1 -KeepWorkspace` assembles the reviewed override
set and runs byte continuity BEFORE packaging/install. Use `-CompileOnly` when
only preparing a mirror while the game is running. Never call `build` again on
the saved native fixture. `Request-MawNaturalValidation.ps1` accepts `test`,
`negative`, `properties`, `natural`, `corners`, `night`, `capture`, `reload`,
`release`; every request is gg/V3/single-player guarded.

`Test-QAAssetSnapshotMutations.ps1` ran seven real CLI cases: valid, changed,
missing, unexpected, duplicate, traversal and invalid hash. All behaved as
expected. New snapshots use CreateNew and cannot replace an old baseline.

Known old grove reload mismatch remains RED and preserved; within-request
guards pass. Existing resource packs, including HD Scenery and Calamity Texture
Pack, were retained, and native alpha comparisons use the actually loaded game
textures. No clean-room resource-pack compatibility claim is made.
