# Arrival pod — bounded starting-area slice

Status: **specified; art review and native implementation pending**. This is
the next slice after the accepted Wastes foreground/midground baseline. It
does not require another background redraw or promote that QA bank globally.

Review board and complete prompts: `Art/Candidates/ArrivalPod-v1/README.md`.
One overly detailed first board was rejected; the coarser revision is awaiting
user design review. Neither image is a native sprite or installed asset.

## Binding decisions

- Opened, damaged, unbranded arrival pod; the player has amnesia. Broken
  communications panel hints at future use without an active service now.
- Real, non-solid world furniture behind players, not parallax or a solid
  structure. No collision, damage, spawn suppression or forced introduction.
- User approved immediate normal-pickaxe removal. Recover one intact placeable
  pod item; moving it never changes world spawn or any bed spawn.
- A small, shallow impact divot and sparse debris, not a large crater.
- Generate only during new supported-world creation. No on-load, on-death,
  join or respawn regeneration and no migration into existing worlds.
- Relay repair, range, housing-based NPC coverage, shipping, shop/dialogue UI,
  additional relays and ship integration are deferred. No new progression gate.

## Proposed art / furniture contract

The following numbers are design targets to be proved, not engine evidence.

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
| Collision / housing | Non-solid, non-solid-top; not a chair, bed, table, light or container |
| Removal | Ordinary pickaxe, no boss/clearance gate; one item per whole object |

Use simple readable pixel clusters, a dark silhouette and about 12–16 subdued
colors. Scorched warm-gray / aged off-white plating, empty dark occupant cradle,
one small dead screen. No company palette/emblem, bright active lights, weapons,
fire, micro-scratches, checkerboard texture, white fringe or realistic painting.
No recognizable copied game pod. Final native pixel sheet must be inspected
against both dark/light backgrounds and beside an actual Terraria player.

The generated board is **concept-only**: any scale annotations are targets.
Do not crop or resize the board into `Content`, assume its pixel grid is exact,
or describe an illustration of a person as native in-game scale proof. Review
an exact native-sized export separately before loading it.

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
