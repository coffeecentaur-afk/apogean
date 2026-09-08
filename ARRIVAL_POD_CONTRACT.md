# Arrival pod — bounded starting-area slice

Status: **contracted; Native-v3 footing repair installed in isolated QA, awaiting native review**. This is
the next slice after the accepted Wastes foreground/midground baseline. It
does not require another background redraw or promote that QA bank globally.

Review board and complete prompts: `Art/Candidates/ArrivalPod-v1/README.md`.
User prefers the first board's design/detail and the second board's chunky
pixel treatment. Blend those as **A3**. The initial technical rejection of the
first board as an export is not rejection of its mechanical design; it is now
the primary design reference. Neither source is a native sprite or installed
asset. The user now explicitly accepts the A3 blend: "perfect". Preserve it.
The separate masked/fitted native candidate is recorded in
`Art/Candidates/ArrivalPod-v1/Native-v1/README.md`; static checks pass, native-size
appearance was accepted ("yes"), then explicitly withdrawn after native testing:
"too detailed ... more pixelated". Keep the mechanical architecture, but reduce
microtexture and use larger pixel clusters. A4's coarse direction is accepted;
the subsequent A5 adds requested broad grit, broken plating and shallow impact
ground. September 8: user agrees to the damaged-but-survivable pod and separate
shallow generated divot. This approves direction, not an exact native export.
`Art/Candidates/ArrivalPod-v1/Native-v2/README.md` records the new 80x96 candidate:
40x48 logical grid expanded into uniform 2x2 clusters, 19 used colors, no baked
crater. Its 80x80 fitted silhouette preserves A5's wider proportions within the
existing 5x6 furniture canvas. It is now installed only in the isolated QA build,
with 408 other Content textures unchanged. Existing pod checkpoint reload and
day/player-overlap/night native captures are recorded in
`Art/Validation/ArrivalPod-v2-2026-09-08/README.md`. The user explicitly needs
the in-game view before approval; do not insert another offline approval gate.
Native-v1's historical (not rerun this review)
58 behavior checks, save/reload and screenshots are recorded in
`Art/Validation/ArrivalPod-2026-09-07/README.md`; they do not approve its art.

## Binding decisions

- Latest native review accepts the coarse damaged body but rejects the curved,
  floating underside. Keep upper art unchanged. The pod needs broad flat metal
  contact on flat terrain, not a single curved center touching the ground. V3
  preserves upper88rows, repairs bottom8rows, and draws2px into the soil's native
  top edge. New contact gate supplements, not replaces, native visual review.
- Opened, damaged, unbranded arrival pod; the player has amnesia. Broken
  communications panel hints at future use without an active service now.
- Real, non-solid world furniture behind players, not parallax or a solid
  structure. No collision, damage, spawn suppression or forced introduction.
- User approved immediate normal-pickaxe removal. Recover one intact placeable
  pod item; moving it never changes world spawn or any bed spawn.
- A small, shallow impact divot and sparse debris, not a large crater.
- Visibly broken plating/roof, a bent but attached hatch and broad soot/mud
  suggest a bad landing without destroying the occupant cradle. The crater is
  ordinary editable terrain: taking the pod leaves the scar behind; placing
  the item somewhere else creates no new impact terrain.
- Generate only during new supported-world creation. No on-load, on-death,
  join or respawn regeneration and no migration into existing worlds.
- Relay repair, range, housing-based NPC coverage, shipping, shop/dialogue UI,
  additional relays and ship integration are deferred. No new progression gate.

## Proposed art / furniture contract

The following layout has a statically checked and isolated native-tested
candidate. Its visual approval is superseded; reuse the proven framing and
behavior while revising art, and repeat native appearance checks afterwards.

| Property | First candidate |
| --- | --- |
| Footprint | 5 wide × 6 high tiles |
| Unpadded artwork | 80 × 96 native pixels, including hinged hatch and aerial |
| Scale reference | 20 × 42 pixel player collision guide; pod about 2.3× its height |
| Frames / styles | One static opened state; no animation or mirrored alternates |
| Tile coordinates | Five 16-pixel columns; six 16-pixel rows; 2-pixel padding |
| Proposed frame sheet | 90 × 108 pixels including terminal padding; not the concept board |
| Placement origin | Local tile (2,5), bottom-centre |
| Support | Five full floor tiles; later player placement may also support native platforms if validated |
| Art origin | Feet end at the support line; no artificial levitation or ground embedded in the furniture PNG |
| Current ground fit | Flat64px base, `DrawYOffset=2` into the native terrain edge; anchors unchanged |
| Collision / housing | Non-solid, non-solid-top; not a chair, bed, table, light or container |
| Removal | Ordinary pickaxe, no boss/clearance gate; one item per whole object |

Use simple readable pixel clusters, a dark silhouette and a restrained palette
(A3 art target: about 16–20 subdued shades, not a verified export count).
Scorched warm-gray / aged off-white plating, empty dark occupant cradle,
one small dead screen. No company palette/emblem, bright active lights, weapons,
fire, micro-scratches, checkerboard texture, white fringe or realistic painting.
No recognizable copied game pod. Final native pixel sheet must be inspected
against both dark/light backgrounds and beside an actual Terraria player.

Preserve the first concept's layered doorway lip, inset hatch panel, hinges,
seat/restraints, segmented heat shield and broad asymmetric scorch. Spend small
pixel accents on those meaningful parts, not random surface grain. The second
revision supplies cluster scale, not permission to erase construction detail.
The approved A4/A5 concept edits establish the new art direction. The native
candidate then uses explicitly lossy deterministic sampling, not a claim that
a palette filter alone makes finished Terraria art.

The generated board is **concept-only**: any scale annotations are targets.
Do not crop or resize the board into `Content`, assume its pixel grid is exact,
or describe an illustration of a person as native in-game scale proof. Review
an exact native-sized export separately; when the user needs live scale to judge
it, use the isolated native fixture before asking for final appearance approval.

Native-v1 uses the retained color master, a separate explicit mask (including
two narrow hinge corrections), then a labelled lossy nearest-neighbour size and
20-color fit. It is not a lossless conversion or newly drawn fine detail. Its
80x90 fitted silhouette is bottom-aligned inside an 80x96 canvas. Only Art
candidates were written; do not bypass the next visual review or live fixture.

Native-v2 uses the A5 master and a new coordinate-locked mask. The generated mask
incorrectly left interior ink lines/cockpit holes; source-specific base/hinge
joins and enclosed-hole fill correct the selection without repainting source
RGB. Final native pixels pass the shared hard-alpha, connected-footing and
30-cell round-trip checks plus an actual 2x2-grid check. Three coarse-grid
negative controls and two-variant deterministic replay are recorded in its
README. The side-by-side player reference is an unchanged crop of the OLD
native capture beside a separately labelled approximate-screen-scale NEW
candidate, not a new in-game screenshot. The September 8 validation directory
now supplies separate actual Native-v2 in-game captures; user review is pending.

## Bounded placement proposal

1. Read final Terraria spawn after vanilla `Final Cleanup`. Keep its coordinate,
   support and at least a 6×5-tile open landing envelope untouched.
2. Survey a finite set of nearby sites inside the existing 110×70-radius
   sanctuary: centre offsets ±10, ±14, ±18, ±22, ±26, ±30, ±34, ±38 tiles.
   Order deterministically by distance, relief and a saved seed tie-break;
   do not consume an unbounded retry loop or another feature's random stream.
3. Pod footprint is 5×6. Proposed divot edit envelope is at most 13 tiles wide,
   two tiles deep, with an ordinary walk/jump route out. Full floor support
   remains, no open shaft or newly released liquid. Zero-depth settled placement
   is the first fallback when a divot is unsafe. Reject excessive relief.
4. Preflight **every actual edit** and padded object bound against completed
   structures, chests, furniture, unknown mod tiles/walls, liquid, ores and
   protected content. Only a narrow explicit natural-soil/grass/empty-cell
   allowlist may be edited. A new pod does not authorize deleting a Living Tree,
   burying a chest, uprooting arbitrary trees, replacing a structure or moving
   spawn. Clear sparse replaceable ground cover only when explicitly allowed.
5. The sanctuary is already registered with `GenVars.structures` by the world
   plan system. A late naive `CanPlace` inside it would reject the pod against
   our own protection. Survey/check the full pod/divot bound **before** registering
   that owned sanctuary; reserve the accepted pod bound immediately, then add
   the enclosing sanctuary. Persist the planned location and recheck the small
   mutation envelope after Wastes conversion. Do not clear StructureMap entries,
   disable protections or bypass foreign reservations to make it fit.
6. Stamp only the allowed divot cells, frame their support, then place the whole
   registered furniture through `WorldGen.PlaceObject`. Never paint furniture
   frame coordinates manually. Verify full object occupancy/support afterward.
7. If all bounded candidates and the undug fallback fail, preserve the world,
   log a precise placement failure and fail the starting-area **QA acceptance**.
   Do not force a destructive placement or silently call the scene complete.
   A safe no-slot production fallback must be settled from that evidence before
   production promotion; cosmetic failure alone should not erase world creation.

Placement plan is separate from the movable furniture. Mining must not make a
saved plan regenerate the pod. Future relay identity is not designed now.
Installation initially belongs only in a disposable construction fixture;
the user has not approved edits to regular worlds.

## Validation order / stop conditions

1. Review this one design; then inspect an exact native 80×96 assembly and its
   90×108 framed export. Validate hard alpha, padding, silhouette and grounding.
2. Isolated tile/item fixture: place on valid support; reject missing anchors;
   render daylight/night, behind player, with paint where supported. Mine each
   occupied region and remove support: never partial leftovers or duplicate
   drops. Replace recovered item. Test liquid/explosion/actuation behavior
   deliberately and record the agreed safe behavior rather than inheriting the
   corporate furniture's permanently protected base class.
3. Save/reload and multiplayer authority/replication. No shared spawn edits,
   remote duplication, recurring placement, detached hatch or foliage sway.
4. Planner tests: flat, stepped and sloped terrain; cavity/water; nearby chest,
   Living Tree, foreign reservation; no-slot fallback; repeat seed; no edit to
   sanctuary landing envelope or terrain outside declared mutation cells.
5. New disposable large-world matrix, with before/after spawn and object/support
   counts, full scene screenshot, reload proof and normal escape by new gear.
   Existing world load and respawn must be zero-generation negative controls.
6. Only then call the starting scene integrated and move to the shallow Maw.
   If user art approval is pending, safe tooling/fixture preparation can proceed
   but new visuals do not get silently installed.

## Implementation seams inspected

- `Common/WorldGeneration/ApogeanWorldGenerationSystem.cs`: owns post-cleanup
  pass order (Maw atlas → Wastes → compounds → ruins).
- `Common/WorldGeneration/ApogeanWorldPlan.cs`: final-spawn sanctuary, large-world
  admission, shared saved atlas. New plan data needs explicit backward loading.
- `Common/WorldGeneration/ApogeanWorldPlanSystem.cs`: sanctuary registration,
  protection rebuild, save/network; never add on-load scenery generation here.
- `Content/Structures/AuthoredStructureTemplate.cs`: frames shell/support before
  native object placement; reservation is not an excavation rectangle. Its
  existing `PlaceObject` helper clears its footprint unconditionally, so do not
  reuse that clearing operation outside a fully preflighted mutation envelope.
- `Content/Tiles/CorporateFurnitureTiles.cs`: existing registration examples;
  **do not inherit** its unmineable `ProtectedCorporateFurniture` behavior.

Engine reference: [tModLoader TileObjectData](https://docs.tmodloader.net/docs/stable/class_tile_object_data.html)
defines furniture dimensions, coordinate rows/padding, origin and anchors.
Checked against stable v2026.07 on September 7. The proposed 90×108 sheet is our
derived 5×18 by 6×18 layout, not a generic size requirement for every furniture
tile. Existing repository [worldgen decision](docs/adr/0002-post-cleanup-bounded-worldgen.md)
and [template decision](docs/adr/0003-authored-corporate-campus-blueprints.md)
remain binding. No runtime behavior is claimed from this document.
