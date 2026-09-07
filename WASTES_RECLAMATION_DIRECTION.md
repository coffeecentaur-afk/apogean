# Wastes reclamation — same place, returning life

Decision source: the user's September7 clarification after viewing the native
purification test. This records art/gameplay intent, not an implemented system.

## Confirmed direction

- Purification retains the Wastes landscape: the same ruined city, gas station,
  garage, checkpoint, collapsed building, roads, hills and cliff formations.
  Nature reclaims civilization; purification does not repair or erase it.
- Add actual living vegetation: green turf patches, moss, vines, shrubs, leafy
  trees and plant growth through damaged buildings. A green tint of the dead
  artwork is insufficient. Preserve readable ruins and their damage at full
  restoration rather than hiding every surface under an opaque forest wall.
- Recovery depends on world-wide purification/restoration progress, not only
  the grass surrounding the current player. It creeps through the composition
  in stages rather than switching the whole scene at a local biome threshold.
- Confirmed order: **Close leads → Mid follows → Far city trails**, with
  overlapping recovery from early progress, not three strictly gated phases.
  Once some Close greenery appears, a smaller amount is already scattered
  across the entire Far city. As Mid recovery develops, Far coverage increases.
  At full recovery all three land layers reach fully living, overgrown variants;
  geography and ruins remain.
- The distant city is the visual indicator of world-wide recovery. Increase
  plant density/coverage throughout it, not a left-to-right green wipe, one
  isolated green building, or a city that stays completely dead until Mid is
  finished. Small amounts of global progress must have a readable, distributed
  effect. Preserve depth contrast and visible ruined architecture.
- Landmark placement, ground anchors, parallax, texture scale and open gaps
  stay stable throughout recovery. Day/night/eclipse still affect the same
  scene, and terrestrial scenery still yields to Space and cave backgrounds.
- This applies to Wastes/restored-Wastes scenery. It does not turn Jungle,
  Maw, desert, snow or other biome families into one green-city panorama.

## Clarifications still open

The user confirmed the order and overlapping city-wide growth. Numeric response
curves, plant coverage at each progress sample and any local influence on Close/
Mid remain to be contracted. Far must represent world-wide progress, not inherit
the current player's local grass ratio or simply copy another layer's opacity.
Do not ask the user to reconfirm the accepted order.

Recommendation, not yet a chosen counting algorithm: use a world-wide Wastes
reclamation measure distinct from Dryad evil percentages. Neutral Wastes are
not evil, and naturally living Jungle does not by itself demonstrate that the
player restored dead land. Decide the eligible terrain/area and denominator,
how restored former-Maw regions count, mining/replacement behavior, and whether
renewed contamination reverses greenery before implementing the counter.
Do not silently equate all non-evil tiles with restored vegetation or require
purifying every underground stone tile to see a green surface city.

## Existing evidence versus requested feature

The local65%/35% native-Forest fallback remains the currently installed behavior
and a useful technical QA control. It is NOT the finished reclamation design.
Its September7 spray/fade/Jungle tests retain their bounded result; they do not
test world-wide progress, vegetation stages or art matching. No runtime, asset,
world or saved-data change is made by recording this direction.

## Smallest implementation path after the design is settled

1. Contract an affordable world-level measure and shared multiplayer/save
   ownership; avoid any world scan in the draw loop. Test counts against small
   known maps, including unknown/modded terrain and no eligible land.
2. Show one unchanged ruin and its matching barren/returning-life/fully-overgrown
   art comparison before installation. Preserve accepted architectural pixels
   except where authored plants genuinely occlude them. Reuse reviewed alpha
   masks and native-scale checks; do not regenerate the whole building.
3. Prototype staged vegetation masks/overlays or registered variants, comparing
   quality and actual texture residency before choosing the production method.
   Reveal stable plant clusters, not camera-random pixels or a blanket green tint.
   Test low progress across multiple separated city regions: growth cannot be
   confined to one end, deferred until Mid is complete, or reshuffled by travel.
4. Test endpoints and intermediate progress, layer order, no landmark jumps,
   lighting, both diagonal directions, biome/Space/cave handoffs, reload and
   clients receiving the same world state. Native screenshots remain mandatory.

## September7 bounded follow-through — offline only

The subsequent `go` authorized first Far city growth studies and an offline
response calculator. See `Art/Candidates/WastesReclamation-v1/README.md`. No
runtime counter, installation, world modification or native QA occurred. The
generated studies are not pixel-registered and cannot replace the original art.
Preserve existing architecture and cave/performance work; arithmetic tests and
historical fallback tests do not prove global reclamation.

### Proposed measurement contract — not implemented or approved

Use **restored eligible surface area / original eligible surface area**, separate
from Dryad evil percentages. Store a generation-time map of restorable surface
plots rather than requiring every underground stone tile to be cleansed. Plot
size, soil depth, sample density and restored-plot classification remain open.
The offline calculator takes supplied counts; it deliberately does not guess them.

| Case | Recommendation | Remaining decision |
|---|---|---|
| Generated Wastes | Register actual converted surface land, not whole bounding rectangles. | Finalize after relevant generation passes and protected-structure exclusions. |
| Surface Maw | Maw to Wastes is containment, not greenery; later compatible living terrain counts. | Define eligible surface footprint and native-evil overlap, not the entire underground Gullet volume. |
| Untouched living biomes | Jungle does not grant recovery credit for simply existing. | Respect original biome/configuration boundaries. |
| Mining/building/unknown blocks | Never shrink the denominator or credit empty/constructed land as green. | Decide how retained plot state and bounded nearby soil evidence allow building without making full recovery impossible. |
| Reinfection | Recommend lowering recovery when eligible land becomes infected again; boss kills alone do not lock it green. | Choose sampling delay and visual smoothing; this policy is not yet user-approved. |
| Old worlds | Missing baseline is unknown, not zero damage or full recovery. | Explicit migration policy; original terrain cannot be assumed perfectly reconstructible. |
| No eligible area | Report unavailable, avoiding division by zero and a fabricated green city. | Preserve ordinary biome routing until a meaningful measure exists. |

Proposed ownership: server-owned world ecology, versioned persistence and shared
client values. Track changed plots plus bounded reconciliation outside drawing;
clients interpolate published progress. Contract update/residency budgets before
integration. This is not an implemented or measured network/performance path.
Start the first fixture with global-only values; optional local Close/Mid influence
must never distort Far's global signal.

Local code evidence: `Content/World/RuinedSurfaceSystem.cs` identifies forest
surface columns and converts selected depths but does not retain this baseline.
`Common/Biomes/ForestRestorationState.cs` has local grass hysteresis and a position
radius; it is not a global counter. Do not rename its fraction and call it global.
Existing world-plan save/network hooks are possible integration points only.

### Proposed overlapping response curves

For global recovery R in [0,1]: Close=`1-(1-R)^3`, Mid=`1-(1-R)^2`, Far=`R`.
These are vegetation-stage activation weights, **not green pixel percentages**.
Reveal stable plant clusters across the panorama; do not fade the whole ruin
layer or shift landmarks. The formula does not itself prove readable distributed
growth at low progress, seamless art, or correct scenery selection.

| Global recovery | Close weight | Mid weight | Far weight |
|---:|---:|---:|---:|
| 0% | 0% | 0% | 0% |
| 10% | 27.1% | 19% | 10% |
| 25% | 57.8% | 43.8% | 25% |
| 50% | 87.5% | 75% | 50% |
| 75% | 98.4% | 93.8% | 75% |
| 100% | 100% | 100% | 100% |

Run `Tools/Get-WastesReclamationProfile.ps1 -EligibleSurfaceUnits 100 -RestoredSurfaceUnits 25`.
`Tools/Test-WastesReclamationProfile.ps1` passes1227 offline assertions, including
four invalid-count rejection cases, endpoints, overlap, monotonicity, equal ratios,
falling recovery, large counts and tiny positive ratios. No terrain classification,
mining-exploit, migration, save/network, art coverage or native-render proof is
claimed. Numeric tuning and the counting policy remain proposals.
