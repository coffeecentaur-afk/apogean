# Apogean Authoring Workflow

This is the production rule for adding visual and gameplay content. It prevents a mechanically valid placeholder from silently becoming the foundation for another unfinished family.

## One-family loop

1. **Specify the player promise.** State what the player sees, understands, does, and receives. Name the closest Terraria behavior that must remain familiar.
2. **Contract the engine behavior.** Record the owning tModLoader type, exact dimensions/framing, authority, lifetime, placement bounds, failure behavior, and test fixture.
3. **Build the smallest probe.** Author one tile, tree set, room, background composition, enemy, or boss state that can disprove the riskiest assumption.
4. **Run static gates.** Use `Tools/Invoke-ApogeanContentGate.ps1 -Profile <family>`. New or repaired validators must reject deliberate defects in isolated copies, including failure propagation through their real entrypoint. `Tools/Test-VisualValidatorMutations.ps1` exercises the tree/terrain cases. Static success never approves appearance.
5. **Obtain the requested art approval, then render the deterministic fixture.** Show actual native assets assembled with their engine offsets, not only concept art. Capture ordinary gameplay scale plus the family's failure cases. Use the same production draw/placement path whenever possible.
6. **Record the evidence.** Update `Tools/AuthoringStatus.json` as `fixture-pass`, `integrated`, `polished`, or `rejected`. A rejection keeps its useful technical proof but blocks dependent production work.
7. **Promote or replace.** Expand a family only after its probe passes. Replace rejected art at the same seam instead of layering compensating overlays over it.

## Retaining corrections

Small spawn features must survey the final terrain produced by their owning
conversion pass, before adding their own enclosing StructureMap reservation.
Keep foreign reservations intact. Try finite authored size variants before
declaring no slot; never solve a cramped seed by clearing trees or arbitrary
furniture. Replaceable multi-tile ground cover must fit the entire actual clear
mask, not merely intersect its bounding rectangle. The arrival planner's real
source tests and seven mutations retain this rule; replay the SAME failed seed
after a fix, then inspect native generation and save/reload independently.

Turn each confirmed user correction into a visible requirement and a regression
check where one is possible. Keep reusable method changes in the installed and
Git-mirrored authoring skill; biome identity/composition in the bible; exact source
hashes, coordinates and screenshots in the candidate record. Do not duplicate a
long chronology in all three. `Test-VersionedSkills.ps1` detects mirror drift.

For background cutouts, the skill's `references/feedback-repairs.md` is the shared
repair protocol. Native-size preservation and valid alpha do not certify branch
connectivity or appearance. Every later biome/variant inherits the checks, not
Wastes-specific colors, coordinates or scenery. No claim of automatic learning or
first-time perfection replaces a recorded test and live review.

Deterministic cutouts use the explicit-mask export contract in
`Tools/BackgroundMaskWorkflow.md`: separate source color, reviewed mask and RGBA
output. The Background gate now runs the actual exporter CLI controls and city
candidate pixel checks. A source-specific mask proposal is not a universal color
key, and edge-color preparation must retain its own change record.

Camera regression rule: give parallax sections absolute identities and stable
world anchors; a saved terrain snapshot alone is insufficient if the renderer
samples it at the moving camera. Track the same section through level pans,
repeat boundaries and return trips. Test positional height locks separately from
opacity, and keep depth/coverage failures red even when anchor tests pass.
`Test-WastesFixedSections.ps1`, `Test-WastesHeightLock.ps1` and their mutation
controls retain this correction; the native trace includes section identities,
submitted alpha and independent screen positions. A below-ground whole-world
pan may be terrain-occluded, so telemetry is not a visual acceptance screenshot.

Stable anchors do not certify perceived depth or movement comfort. When the user
reports combined diagonal reactivity, test horizontal AND vertical response of
the same rendered section with a small repeatable rise/fall path. Keep the
projection stateless; do not hide excessive depth response with a camera spring.
`Test-WastesForegroundDepth.ps1` and `Test-WastesRunningLive.ps1` retain this
specific correction. Camera-only traces are not physical-running or user-feel
approval. Ceiling/coverage, routing and art gates remain separate.

Position continuity alone does not certify motion. A hard clamp can keep every
position correct while abruptly changing perceived speed. Test finite-difference
response on both sides of every transition, layer depth ordering, a stopped
camera and reverse/revisit positions. Integrate a continuous response when
changing altitude behavior; do not hide the kink with a time-based spring.
`Test-WastesSmoothFlight.ps1` executes the production arithmetic, while the
native height validator independently reconstructs submitted positions. Its
`flight-turnaround` case includes a mid-flight hold and a return. Historical
hard-cap traces cannot certify the new profile. Numeric/native telemetry passes
still need user comfort review; screenshots cannot prove continuous motion.

## Reusable tooling and dependency mods

Standing user authorization (September7): a separate tModLoader mod may be
created for reusable engine/tooling capabilities, and Apogean may depend on it
when that solves a concrete need. A second mod is an available option, not a
requirement for every bug. Build-only atlas tools and agent skills should remain
development tools unless players genuinely need their runtime functionality.
For extraction, record ownership, a small public API, compatible tModLoader and
library versions, install/update instructions, save/network boundaries, and tests
with both the library alone and Apogean installed. Existing art, destructive
operation and publication approval gates still apply; this is not an instruction
to publish a new dependency now. Current Wastes camera fixes stay in Apogean.

## Evidence states

Shared disposable worlds still contain independent evidence. A new background
fixture must preserve existing grove checkpoints, reserve its full destructive
isolation/framing/spray envelope outside the grove, and compare its snapshot
before/after setup. Never clear or rewrite a failing expected digest to make an
unrelated test pass. September7's routing fixture retains the old reload failure
while proving its own bounded checks; those are separate results. Use
`Tools/Test-BackgroundFixturePlacement.ps1` and the native grove-guard records.

| State | Meaning |
| --- | --- |
| `specified` | Player experience and dependencies are agreed. |
| `contracted` | Engine, art, authority, failure, and fixture contracts are written. |
| `fixture-pass` | Static checks, clean build, and deterministic live fixture pass. |
| `integrated` | The production world/progression path passes, including reload and relevant multiplayer behavior. |
| `polished` | Final visual/audio/balance/documentation review is accepted. |
| `rejected` | A live failure or explicit review blocks promotion; retain the evidence and replace the candidate. |

## Family order

The default dependency order is terrain framing → native trees/vegetation → background rendering/routing → faction materials/furniture → complete structure templates → entities → boss encounters → quests/dialogue/progression. Design specifications may run ahead, but unfinished foundations do not gain production dependents.

Installed Codex skills enforce the focused contracts: `$tmodloader-atlas-authoring`, `$tmodloader-tree-authoring`, `$tmodloader-background-authoring`, `$tmodloader-structure-authoring`, `$tmodloader-entity-authoring`, `$tmodloader-boss-authoring`, `$tmodloader-quest-dialogue-authoring`, and `$apogean-content-direction`.

Current approved recovery order (2026-09-05 scope decision): validator reliability → A — Snapped native assembly/approval → grass-corner and ground-cover checks → a reliable, visually acceptable Wastes background baseline → a cohesive generated starting area → Maw entrance/shallow slice → exploration/combat/rewards. A baseline is not final polish. Keep experiments outside ordinary play, preserving the last accepted appearance where one exists.

### Bounded background correction

The user approved one corrective pass for actual regional-ground anchoring, pale cutout rims and disconnected station branches. Check ground movement, diagonal flight in both directions, shallow descent and biome/restoration transitions for gaps or regressions. Show actual game screenshots before accepting the baseline. Additional foreground variants, finer shading, extra ruins and additional scenery families are deferred polish, not prerequisites for the next playable slice.

If that pass still fails, simplify or park the failing layer and report the remaining failure instead of automatically starting another art-generation cycle. A simplified fallback must pass its own fixture before dependent production work continues. Keep rejected evidence and unresolved full-release checks; scope reduction never changes a failed test into a pass.

The starting-area slice includes the approved drop pod and shallow divot (DESIGN_BIBLE.md, Spawn sanctuary). Its later ground-to-orbit relay is specified only and does not expand the current implementation scope.

## Bounded architecture-variation addition (2026-09-06)

The latest user direction authorizes three Wastes Mid ruins total, not three
additional ruins: keep the liked broken shell and draft a low motor depot plus
a narrow checkpoint. Two new concepts is the stop bound for this pass. Update
their separate art/export/runtime evidence; do not repeat generation just to
chase transparency. Reuse the explicit-mask workflow for an approved silhouette.
This overrides the earlier extra-ruins deferral only for this small candidate set.
It does not install new assets, approve all Wastes gates or start every biome.

Each later family starts from `BACKGROUND_BIOME_DIRECTION.md`: historical
architecture and local ecology are separate inputs. In particular, Jungle means
living green canopy reclaiming abandoned Helix infrastructure, not brown Wastes
with laboratory props. Reuse alpha, native-scale, flight, spacing, lighting and
routing tests, never a single recolored Wastes panorama. Review the actual
source/mask and assembled native-scale result before a disposable live fixture.

## Reference discipline

### Pixel language is not object scale

When a correctly sized object looks too realistic beside accepted scenery, keep
its physical size and revise the material drawing: coherent pixel clusters,
grouped damage, clear shading planes and deliberate highlights. Use the accepted
asset unchanged in a native-size comparison. Neither higher resolution nor
shrinking noisy realistic art proves a style match. Record actual generated
dimensions and any fitting separately; do not describe changed geometry as a
lossless repair. One revised object is the approval boundary before batch work.
Example: `Art/Candidates/WastesMidRuins-v2/PixelStyle-v1/README.md`.

Follow-up correction: weathering density and pixel grain are separate decisions.
An accepted gritty reference must stay gritty; do not flatten its worn materials
into pristine slabs when reducing photographic noise. Compare native material
patches, not just whole silhouettes. If a fixed-grid resampling study is useful,
label it as resampling with possible thin-feature loss, not authored detail or
proof of a style match (`PixelStyle-v2/README.md`). Stop after the bounded probe.

Evaluate architecture and supporting terrain independently. A user may prefer
readable, quieter building surfaces and rougher ground in the same module.
Record that component choice without claiming a complete style match, and do
not repeatedly retexture both regions to solve a preference about only one.
For deterministic component assembly, pin both inputs, record a reviewed source
selector, preserve their pixels, and check the footing separately from alpha.
An upper-only crop may use explicitly empty padding in a ground gallery, never
as a claim of authored deep coverage. Example: `ComponentAssembly-v1/README.md`.

A reference can have preferred pixel grain but a rejected contour, while another
has preferred architecture but excessive micro-detail. Record these separate
preferences; do not freeze the first reference after the user qualifies it.
Make one targeted bridge candidate, preserving originals and showing unchanged
references at comparable native size. Passing alpha export must not be reported
as solving the style mismatch. `WastesStationBridge-v1/README.md` records this
bounded probe, including the returned opaque matte and explicit resampling.

### Native scale gate, not another offline approval loop

When a user cannot judge scale from a board, supply a bounded native ground
gallery beside the real character, terrain and an accepted landmark. Label any
suppressed scenery, source reduction, upper-only crop and forced routing. Do not
demand offline approval first or promote short studies into deep modules.
Example: `Art/Validation/WastesMidScale-2026-09-06/README.md`.
Changing exposed building sides must preserve scene lighting; keep source/mask
provenance instead of silently mirroring all its lighting and terrain.

Other mods and games are studied for reusable principles: engine ownership, placement safety, encounter readability, functional silhouettes, objective clarity, co-op state, and environmental storytelling. Apogean does not copy source, assets, layouts, names, dialogue, timing, or recognizable encounter combinations. Research provenance and the verified/inferred boundary live in `RESEARCH_APOGEAN_CONTENT_WORKFLOWS.md`.

## Current commands

```powershell
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Status
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Tree
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Background
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Structure
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Boss
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Quest
```

An intentionally red gate is useful: it names what is still missing. Never weaken a production threshold merely to make the report green.

## Background lesson retained — 2026-09-04

Measure a source landmark against its actual Windows game capture. A 111-pixel truck became 150 pixels wide despite `Draw(scale:1)`, because Terraria applies forced minimum background zoom and temporarily logical screen dimensions. The scoped correction produced110 pixels without changing the user's zoom. `Tools/Test-WastesLandmarkScale.ps1` retains this red/green regression; `Art/Validation/WastesLandscapeV1/README.md` records the full bounded proof. Never infer native detail from texture dimensions or draw scale alone. Check composition behind real terrain as well as in an empty gallery, and keep art, coverage, routing and performance acceptance separate.

## Background lesson retained — 2026-09-05

A settled restored scene is not evidence of a correct fade. The native PureSpray fixture reproduced an outgoing style that stayed opaque because its selected-only refresh hook stopped running. Read current engine alpha at draw time, identify which style actually drew, and require both fade directions plus missing-draw rejection. `Test-ForestSprayLive.ps1 -Replay` explicitly labels archived evidence; `Test-ForestSprayValidator.ps1` must reject deliberately defective traces through the real CLI. Physical travel distance also cannot approve hidden artwork: use labelled isolated phase sweeps for texture joins, retaining separate real-world camera/routing tests. No static measurement promotes art.

## Background lesson retained — 2026-09-06

Coverage and visibility are independent contracts. A fully screen-covering city
can still be wrong in Space. Test full submitted RGBA against independent
player/camera classification, ground presence, partial fade and both continuous
flight endpoints. Keep geometry checks even during deliberate invisibility;
their success is not proof of visible continuity. `Test-WastesSpaceHandoff.ps1`
uses the live pure policy; its CLI controls reject always-visible, always-hidden,
abrupt and camera/player-blind alternatives. `Test-WastesSpaceLive.ps1` replays
actual draw samples and completion records; its companion validator rejects
missing samples/endpoints and defective presence/absence. A screenshot after a
30-second hold expires is a return view, not evidence of that held case.

Do not spend extraction effort on an unresolved silhouette. The user has now
liked the first Mid ruin's visual direction, superseding the initial reviewer
objection to its perspective; preserve that approval rather than restarting
its design cycle. Its baked matte and unproven native-scale fit still require
separate export/assembly gates. Never interpret a displayed checkerboard as an
alpha result; inspect encoded pixels.
