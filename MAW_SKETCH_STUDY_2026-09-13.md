# Drawing-led Maw mouth study

User source: `C:/Users/max_h/Downloads/maw mouth design ms paint rough.png`.
The source stays local. This is a separate disposable QA construction, not a
replacement for shallow V1 or a production world-generation change.

## Confirmed direction

- Yellow marks hazardous, easily mined teeth; it does not request yellow bone art.
- White marks continuous structural ribs. Their branching roots enter the host
  wall using the SAME rib material, not a dedicated porous joining material.
- Vary rib length, direction, curvature and depth. No evenly alternating stairs.
- Orange marks thick surface fiber, fibrous cave margins and hanging strands.
- Concentrate amber organs around side paths as discovery cues, not down the
  entire main shaft. Ordinary terrain stays non-emissive.
- Two empty vanilla chests mark possible side-cave loot sites. No loot table,
  reward balance, new item or main-shaft chest is implied.

## Generation transfer direction — September 13 follow-up

The user's explanation refines the accepted reference, not a request to rebuild
it: the Maw is escaped bioengineering that grows opportunistically, not tidy
architecture. Bone breaking through the upper ground is intentional. Uneven,
partially developed growth belongs in its silhouette; disconnected rendering,
unsupported objects and accidental atlas seams do not.

- Keep the V1 scene unchanged as the approved reference. Do not reinstate the
  discarded deeper-root experiment or require every root endpoint to be buried.
  Structural ribs still need a connected attachment to their host terrain.
- Preserve raised lips, dense easily mined teeth, irregular continuous ribs,
  thick surface fiber, wall/ceiling fiber and winding side passages. No regular
  safety staircase or supplied ropes. Connectivity alone is not traversal proof.
- Space independent side-cave groups farther apart than the small mockup's
  reward pockets. Measure separation between their outer envelopes, not merely
  center points; retain solid terrain between groups except planned connectors.
  Vary spacing within a bounded seed-driven plan rather than alternating rooms
  at a fixed interval. Exact distances remain test tuning, not settled design.
- A side cave can connect to a smaller area with a different purpose. Some
  pockets should be Maw Node sites, not another chest room. A Node is a feeding
  and spread-amplification organ, NOT a Brood Nest/boss-summoning object. Layout
  reservations do not implement Node behavior or change its progression lock.
- Chests stay sparse, in selected side caves. The two empty QA chests demonstrate
  placement, not a quota per generated segment or an approved loot table. Not
  every nook needs a reward object; amber can guide exploration without loot.
- Keep fiber's soft terrain transition, but avoid a visible backing-wall fringe
  above the exposed surface. Treat solid terrain, native background walls and
  decorative fiber as separate coverage masks. Underground walls remain; test
  actual rendered wall edges at slopes/lips rather than only tile coordinates.
- Preserve the full-size route into the Stomach above Hell, followed by its
  narrow enclosed intestinal descent. Do not turn the Stomach into an open
  Wall of Flesh obstruction or enlarge its protected reservation casually.
- Each seed should share this anatomical language, not duplicate the entire
  200x136 scene. Adapt mouth contours, branching rib roots and cave connections
  around the saved navigation route and protected landmarks at native tile size.
  Use finite placement attempts and explicit failures; never erase a landmark
  or pack rejected caves closer together to meet a content count.

Implementation order and evidence boundary:

1. Transfer the accepted contour/rib/cave rules to a deterministic, bounded plan
   with deliberate surface-wall coverage and explicit pocket roles. Keep proposed
   node/cache reservations separate from their future gameplay payloads.
2. Replace the legacy repeating shelf generator and its outdated traversal
   assumptions together. Use the same placement path in a new disposable test
   and fresh-world integration; do not ship a QA-only look with different runtime
   construction. Validate footprint, supported hazards, native slopes, route
   clearance, landmark preservation and unchanged saved worlds independently.
3. Compare several seeds/world sizes, player-scale appearance, practical rope
   and mining routes, save/reload, generation time and memory. Do not promote the
   entire biome based on one approved entrance or a pure-plan flood fill.
4. Then build the Maw's own layered scenery for surface, underground, caverns
   and Burning Root. Reuse the Wastes' camera/alpha/coverage validation methods,
   not its city artwork. Keep biome routing distinct, deep bottoms covered and
   height transitions smooth. Later awake/dormant lighting must dim the same
   anatomy rather than rearrange the landscape.

Source inspection confirms this transfer is NOT implemented yet:
`Common/WorldGeneration/MawRuptureGenerator.cs` still uses countdown-spaced
elliptical chambers and `PlaceGulletShelves`; the accepted drawing lives in the
separate `Content/Diagnostics/MawSketchPlan.cs`. Keep this distinction visible
until the actual production path and its validations change together.

## QA implementation boundary

200×136 tile authored plan, existing accepted material banks, native solid/sloped
ribs, native four-way thorn clusters, cuttable/non-climbable short fibers and
native chest placement. One bounded empty-air reservation in V3; reject occupied
or protected sites. Never rebuild an existing study or refresh a failed baseline.
Keep old scenes and normal worlds intact. Build shell/frame, then hazards/chests.
Rollback only the new envelope and new empty chest records on construction failure.

Default visit gives FREE movement. Optional explicitly labeled inspection lighting
works without stripping inventory; it is not an ambient-light or difficulty test.
There is no held camera or automatic movement. Exit releases the view and light.
New data must survive ordinary save/reload, including headless saves, without
granting construction permission. Build is restricted to gg in V3 single-player;
the existing Plain character may visit, light, inspect and leave this scene.

Static checks: deterministic complete plan, rooted connected bone, no dedicated
bone joining key, thick cap over rooted soil, bounded supported tooth footprints,
short anchored fibers, two side-cave chest footprints, amber side-path distribution,
and conservative 2×3-body connectivity excluding entire hazard rectangles.
Native checks: actual placed types/slopes/objects, scene dimensions, preservation,
save/reload and screenshots at ordinary scale. Spatial connectivity alone does
not certify jump/rope returnability or balance. User reviews appearance in game.

## Accepted checkpoint — September 13

The user reviewed the native scene and said, "That looked literally perfect."
Freeze the version they saw. An uninstalled experiment to bury the uppermost
root farther into the hillside was discarded after that approval; V2 was never
built in game. The exposed upper root remains part of this approved specimen.
Do not restart a speculative geometry/art polish loop.

Built in V3 at (1496,250), 200×136 tiles, after two empty-site candidates.
Six ribs, forty supported thorn clusters, seventeen 2–6-section hanging fiber
strands, two actual empty chest records. No production generation changes.
The design/atlas/structure workflow kept existing textures, native object
placement, explicit preservation checks and visual acceptance separate.

Installed package SHA256:
`201EA7F76CF3F7F5CD0694DC38305FB3C01C9D236F8DCE25384D0AD3339EC12D`.
Build mirror:
`C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/1a154bdaca3149d281f43b6daf73423b/apogean`.
The two sketch C# files match that mirror byte-for-byte. Full build: zero
errors, two PaintID readability warnings. No texture changes or dependencies.

Evidence:

- Pure plan: 27,200 deterministic cells, three rejected defects, root-to-tip
  connectivity and conservative main/side route clearance.
- Scope: 1,033 checks; shared save/exit dispatch: 80 checks plus eight
  omitted-release controls.
- Native construction 00:50:29 local: all 27,200 tile/wall/slope/object cells
  match; 40 native clusters and two empty native chest records.
- Native save 00:51:53: original and saved semantic digest both
  `A0267FFCCA42E2D41F2BB89608D6BBA364ADF8F9AC1F36120409F41165D01374`.
- Native reload: pristine pass 00:53:54 and saved-state pass 00:54:03.
- During commands, nine nonempty historical regions remain semantically
  unchanged; nine empty/missing historical slots cannot be certified.
- `Art/Validation/MawSketch-save-2026-09-13.json`: sixteen old saved ModSystem
  records unchanged; one new MawSketchLab record added. This is metadata
  evidence, not a whole-world tile comparison.
- Native full-scene capture:
  `Art/Validation/MawSketch-native-natural-2026-09-13.png` (unchanged engine PNG).
  Ordinary lighting leaves much of it dark; this is not a fullbright art proof.
  The SVG/PNG layout pair in Art/Validation is only a geometric diagram.
- Native side-cave visit 00:54:12 succeeded; actual empty chest, cave margins
  and hanging strands were inspected at player scale without freezing movement.

The existing grove checkpoint mismatch still reports on gg entry; it predates
this task and was not repaired/rebaselined. The earlier shallow scene's latest
user interaction digest `2ADAC67F…` was retained rather than reset to an older
test snapshot.

Visits: `/mawsketch entrance`, `ribs`, `cave`, `lower` (each with the
same command prefix). Free movement, no held camera. Optional
`/mawsketch light-on` is a local diagnostic lamp; `light-off` disables it.
`/mawsketch release` returns to the pre-visit position. No inventory stripping.

Next: adapt this accepted spatial grammar into bounded seed-aware production
plans, preserving landmarks and difficult rope/grapple-oriented traversal.
Do not infer full-route balance, world-scale performance, final loot or acid
mechanics from this visual approval. Broader old red gates remain separate.

Native chest coordinate contract consulted:
[tModLoader WorldGen API](https://docs.tmodloader.net/docs/stable/class_world_gen.html)
(PlaceChest uses the bottom-left tile coordinate).
