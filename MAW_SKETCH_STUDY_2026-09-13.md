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

## Implementation boundary

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
