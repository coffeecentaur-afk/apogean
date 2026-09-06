# Feedback-driven background repairs

## Persist the lesson, not only the edited picture

Record each correction as a player-visible requirement and the smallest check that
could catch its recurrence. Put general requirements here, biome composition in
the design bible, and candidate dimensions, coordinates, hashes and evidence in
its candidate record. Update the installed skill and the repository mirror together.
A recorded preference is not proof of a fix; keep pending live/art gates explicit.

## Exact-pixel cleanup

When the user approves deterministic local image editing:

1. Inspect the actual runtime PNG and source master. Pin their hashes. Work on a
   candidate copy and reject unexpected source changes; never silently regenerate
   all assets through an older exporter.
2. Define permitted repair regions before editing. Preserve canvas dimensions,
   native scale and every visible pixel outside them. Do not resize a failed AI
   edit back to the target size and describe it as an invariant-preserving repair.
3. Diagnose color and alpha separately. A broad chroma key can erase dark branch
   connections; hard alpha can still contain a bright baked-matte fringe.
4. Restore connections from the source silhouette, not global dilation. For a
   color-only fringe correction, preserve the alpha mask instead of eroding the
   branch. Keep legitimate rock/turf highlights. Source-specific thresholds and
   material palettes must not become a universal key for later biomes.
5. Inspect native crops on both light and dark backings, plus a labelled
   nearest-neighbor magnification. Component tests need semantic regions and
   ground anchors: isolated debris or a separate sprig is not a broken branch.
   Keep broad warnings visible even when a narrower structural test passes.
6. Run the real validator with both passing controls and deliberately broken
   controls. Compare repaired files with the pinned baseline, not just dimensions.
   Freeze the candidate hashes before live QA.

In Apogean, reuse `Tools/Test-BackgroundReplacement.ps1` for exact size, actual
transparency, hard-alpha contracts and unchanged pixels outside allowed rectangles.
Use `-PreserveAlphaMask` for color-only repairs; it is inappropriate for restoring
missing branches. `Tools/Test-BackgroundReplacementValidator.ps1` tests the real
entrypoint, including deliberately eroded silhouettes. Bright-edge warnings are
review aids, never a mandate to darken all pale pixels.

## Camera and family transfer

- Close terrain uses a stable world/regional ground datum, never player altitude.
  Test a thin natural floor as well as deep terrain. Generation-time ground and
  multiplayer ownership still require their own implementation and proof.
- Author the lower extent the camera can expose. Ground lock does not excuse
  truncated bottoms; shallow descent, diagonal joins in both directions, and the
  visible cave handoff must be inspected separately.
- Modular gaps and chunk order are stable. Do not shuffle on movement or
  restoration; vary authored landmarks with quiet intervals.
- Reuse the renderer and tests, not another biome's scenery. Test real biome
  selection and local restoration without forced laboratory overrides.
- Export, connectivity, visual quality, camera geometry, routing, live build and
  performance are separate gates. No passing static check closes the others.

## Provenance

Apogean's `Art/Candidates/WastesCutoutRepair-2026-09-05/Exact-v1/README.md`
records a bounded native-pixel repair and its limits. Its ROI coordinates and
color rules are Wastes-specific. Regional-ground evidence is separate in
`Art/Validation/WastesRegionalGround-2026-09-05/README.md`.
