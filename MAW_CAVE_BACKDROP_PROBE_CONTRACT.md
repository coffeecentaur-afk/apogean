# Ordinary-cave backdrop seam — bounded QA contract

Independent provisional branch of the September12 pass. Engine evidence is in
`RESEARCH_MAW_CAVE_COMPOSITOR_2026-09-12.md`; art provenance is in
`Art/Candidates/MawCaveBackground-v1/README.md`. No production route is enabled.

## Boundaries

- Only packed QA, single player `gg`, world `Apogee Native Visual V3`.
- At most 195 candidate 96×64 rectangles near spawn, sampled read-only. Require
  dry wall-free openings, solid terrain, and native walls together. Exclude saved
  historical fixture rectangles. The selected scene is an existing ordinary cave,
  not an authored Maw scene and not evidence of natural Maw biome selection.
- No tile/wall writes, teleport, actor creation, saved settings, weather changes,
  new renderer targets, SpriteBatch restart, liquid redraw, detour, or dependency.
- Camera only, fixed world anchor; optional one-tile diagonal pan and temporary
  1.25× world matrix zoom. Release removes these overrides. The player remains
  wherever they were, so this does not prove player occlusion or traversal.
- 64 fixed neutral diagnostic lights, equal in baseline/pattern/art. This does not
  establish native ambient lighting, emitted amber, or awake/dormant rendering.
- One original 1536×1024 study at scale1 world texels; no viewport fitting,
  repetition, parallax, layer blending, stretching or perspective correction.
- `RenderLayers.Background` inherits the game's batch/matrix/sampler. Normal-frame
  callback only. Native walls, solid tiles, and later entities draw afterward.
  Photo `CaptureManager` is explicitly excluded and does not capture overlays.
- Current liquid occupancy is re-read using a reused integral array. Cells within
  two tiles of liquid are excluded, including liquid just outside the rectangle.
  This leaves native wet pixels intact within that conservative mask. Animated
  liquid edge footprints and through-liquid scenery are NOT certified by this.
- Read-only inspection of the installed effect dictionary AND scheduled draw lists
  must succeed; any other scheduled overlay refuses activation. A registered
  `SimpleOverlay` can report visible solely through its shader while never being
  scheduled; it is recorded but not misclassified as a competing draw. No other effect is changed. A bound
  key collision requires a game restart, not replacement. This QA restriction is
  not a production coexistence solution.
- Unload deactivates/detaches the overlay and drops the mod's asset reference.
  The manager retains its inactive registration until engine reset; no shared
  texture is disposed. Do not claim a public unregister lifecycle exists.

## Validation checklist

1. Build with `-MawCaveBackdropStudy`, preserving the 466 pinned base assets and
   separately verified anatomy additions. Only a pinned study PNG is added.
2. `locate`, native normal-frame baseline, `pattern`: visible colored diagnostic
   bands in open dry areas and native opaque tiles/walls still in front.
3. `art`, report after settling: callback/matrix/viewport/sampler/target telemetry,
   actual normal-frame appearance. A beautiful offline master is not this proof.
4. `pan`, `zoom`, then `origin`/`normal-zoom`: world-space alignment under changes.
   Record actual drawToScreen; don't claim the untested cached/direct counterpart.
5. Inspect any liquid exclusion and limits; do not alter the cave to make it pass.
6. `baseline` then `release`: original scenery and original camera return.
   Fingerprints are recorded before/after, not silently rebaselined; ordinary
   simulation can move liquids or grow plants even though the probe writes none.
7. Keep failure causes separate: activation, capture path, matrix, wall occlusion,
   mask, lighting, compatibility, and art quality. Stop this seam experiment after
   the bounded proof; production compositing needs a separate decision and matrix.

Build success is not fixture-pass. Q/R native results are retained in
`Art/Validation/MawCaveBackdrop-2026-09-12/README.md`: the bounded dry overlay
draws in the intended order under one camera shift and alternate zoom, then
releases without changing the selected region. The lighting, art, wet-scene,
production-routing and full viewport matrix remain unapproved. This completes
the seam experiment, not the cave-background family.
