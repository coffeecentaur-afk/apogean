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

This specification does not declare the world-counter/art prototype started or
complete, or silently expand today's bounded cave/performance checks. Keep the
approved barren architecture and existing test evidence while planning this
separate restoration branch. Do not present fallback success as its completion.
