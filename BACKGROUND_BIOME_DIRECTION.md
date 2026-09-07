# Background architecture and ecology direction

Decision source: the user's 2026-09-06 request for two or three different Mid
ruined buildings, faction-based environmental history, and subsequent Jungle
clarification. This is an approved direction with proposed examples, not proof
that these backgrounds are implemented. Existing candidate evidence remains
valid within its original scope. The Bible owns lore; this card owns the briefs.

## Biome briefs

| Family | Historical architecture | Living/dead read and proposed layer vocabulary |
| --- | --- | --- |
| Wastes / ordinary forest history | Kessler military-industrial | Dead roadside frontier. Far: subdued ruined industrial city. Mid: low service buildings, checkpoint and broken shell, existing Station/Highway. Close: longer broken-earth banks and dead vegetation. Restored forest recovers greenery, not repaired civilization; the current native-forest fallback remains valid. |
| Jungle | Abandoned Helix study/containment sites | **The green survivor.** Far: layered living canopy and a few barely visible research silhouettes. Mid: cracked conservatories, vine-swallowed labs and broken elevated walkways separated by lush growth. Close: roots, ferns, mossy ledges and dense living foliage. Ruins are subordinate to nature; greenery is not a small accent on a desolate field. |
| Maw and infected biomes | Helix research/containment history | Maw: amber/stringy growth engulfing clinical structures. Corruption: purple menace through abandoned study sites. Crimson: its native red organic identity around containment remains. Hallow: luminous transformation through abandoned observation sites. Different palettes/ecologies and structures, not one laboratory panorama with a hue shift. |
| Desert | Abandoned transport/logistics; owner unspecified | Sand-buried interchange ramps, fallen bridge spans, freight/service ruins. Low, open, dry silhouettes with long quiet intervals. Do not invent a required faction owner for every highway. |
| Snow | Abandoned transport/logistics; owner unspecified | Snow-laden bridge piers, broken elevated roads, frozen relays and exposed pipelines. Snow/ice and winter visibility must distinguish it from desert/Wastes. |
| Sky islands / space | Sentrix surveillance/data history | Precise spires, broken docking structures and suspended observation platforms. Sky/island art is not a land layer stretched into Space. Actual ship/star-chart systems stay future scope. |

The Jungle surviving does not make it a safe biome, prove that every other living
organism died, or authorize changing progression/enemy difficulty. Abandoned is
about the human infrastructure; overgrown is about the thriving ecosystem.
Helix studied infections; this card does not assert that it created them all.
Hallow's existing classification is unchanged. Other-mod priority is unchanged.

Ocean, Mushroom, Dungeon and Underworld retain their separate existing Bible
briefs until specifically revised. Do not extrapolate this map into unapproved
faction ownership. In particular Mushroom's fungal hydroponics/waterworks remain
their own visual subject, not the jungle canopy or a generic dead cave.

## Three Wastes Mid ruins — bounded candidate set

1. **Broken shell:** keep the original liked two-storey collapsed-building
   concept in `Art/Candidates/WastesMidRuin-v1/`. Existing output is opaque;
   visual acceptance is not alpha/runtime acceptance.
2. **Motor depot:** low, wide, single-storey service bays and broken roof steel;
   horizontal silhouette, no gas pumps duplicating the Station.
3. **Checkpoint/communications ruin:** narrow stepped observation post and short
   broken antenna; vertical silhouette, not another wide shell or skyscraper.

The user accepted these three basic designs, then questioned their identical
facing. One checkpoint redraw exposes its opposite side while retaining
upper-left lighting and the original cliff. Native ground-scale comparison with
the real character and Station is available after the user could not judge
an offline board: `Art/Validation/WastesMidScale-2026-09-06/README.md`.
Size/facing and full integration remain separate review gates.
Latest feedback accepts the general look/scale but requests a less realistic,
more deliberately pixel-art material style matching Station. Do not enlarge
the buildings or redraw Station. One depot style probe is ready outside Content
in `PixelStyle-v1`; shell/checkpoint restyling waits for that review. The same
distinction between pixel grain and physical size applies to later biome art.
Sources remain under `Art/Candidates/WastesMidRuins-v2/`. No new traversable structure, faction
mechanic, NPC or inventory item is part of this background work.

Placement must be stable across camera travel, time, reload and clients. A
future seeded layout mixes authored variants with quiet stretches; it must not
shuffle at random each frame or replace scenery when the player looks away.
Do not show identical building landmarks in consecutive slots. Preserve existing
Station/Highway landmarks and leave at least two quiet/gap slots between dense
ruin groups in the initial sequence proposal; adjust only after native review.
This is a spacing design, not a claim that the current fixed three-group Mid
renderer already supports a seeded multi-variant sequence.

## Reusable production checklist

1. Record biome ecology, architectural history and protected accepted assets.
2. Review distinct silhouettes and material palette. Do not copy references.
3. Inspect actual PNG alpha. Use a separately reviewed hard mask where needed;
   keep source colors/provenance and inspect thin elements on light/dark grounds.
4. Assemble at actual draw scale. The existing Mid slots are 391–576px wide and
   1408px tall; a generated concept is not automatically a drop-in. Solve the
   building scale, deep support and joins explicitly, without silently stretching
   art or disguising a low-detail enlargement as higher resolution.
5. Use stable world-ground anchors and authored undersides. Test quiet intervals,
   both horizontal directions, diagonal flight and shallow descent. Add both
   presence at ground level and absence in Space; coverage alone is insufficient.
6. Check day/night/rain/eclipse and native biome/restoration priority. Green
   restored areas cannot retain the wrong Wastes treatment. Jungle must read
   alive in all lighting states, not turn into dead Wastes when night tint applies.
7. Record static, build, native fixture, production and user review separately.
   No untouched biome advances because another biome passed these tests.

Current order remains: bounded Wastes baseline and this small concept addition,
then generated starting area/drop pod, then shallow Maw. Jungle direction is
recorded now; its full art/implementation pass is not silently moved ahead of
that dependency order.
