# Exact native-pixel cleanup — offline candidate

The user approved deterministic pixel-level repair and asked that corrections be
retained as reusable skills. The offline creation pass changed no world, runtime
texture, renderer, display setting or game process. A subsequent temporary-mirror
build and native QA pass of these exact hashes is now recorded in
`Art/Validation/WastesExactCutouts-2026-09-05/README.md`. The installed QA package
contains the repairs, but repository Content PNGs remain unchanged. User review
and production promotion are still pending.

## Recipe and evidence

`Tools/Repair-WastesCutouts.ps1` reads hash-pinned archived exports in
`Art/Candidates/WastesMidgroundModules/2026-09-05/Deep-v1/Derived` plus the original
Upper-Cliffs-Matte master. The archived exports match the unchanged QA runtime
assets byte-for-byte. The script writes only this candidate folder. No resizing,
whole-image generation, blur or new lower-fill geometry is involved.

| Asset | Native canvas | Changes | Untouched outside regions | Alpha |
| --- | --- | --- | --- | --- |
| Station | 488x1408 | 954 pixels; 675 missing source pixels restored | Yes | Hard, genuine transparency |
| Foreground | 1448x1915 | 114 exposed timber edge colors | Yes | Entire original mask preserved |

Station's source crop starts at master X1048. The old R-G/B-G chroma rule removed
dark, matte-contaminated branch colors. Two explicit regions recover the original
dark silhouette, with bounded color decontamination. These thresholds belong to
this source only, not a generalized key for new landscapes.

Foreground repair selects three reviewed timber regions and samples adjacent
darker brown material into pale boundary pixels. It never erodes the silhouette
or applies a global brightness rule to grass and rock highlights. Per-file JSON
reports record original/candidate hashes and exact preservation checks.

## Reproducible checks

```powershell
pwsh -NoProfile -File Tools/Repair-WastesCutouts.ps1
pwsh -NoProfile -File Tools/Test-WastesExactCutouts.ps1
pwsh -NoProfile -File Tools/Test-BackgroundReplacementValidator.ps1
pwsh -NoProfile -File Tools/Test-VersionedSkills.ps1
```

The real connectivity CLI fails against the archived baseline and passes against
this candidate. The original broad probe found 12 islands, including a separate
seven-pixel terrain sprig at X84/Y406. Its narrower source-identified tree region
still fails on the old branches and passes on the repair; the broad warning is
retained rather than deleting the sprig to manipulate the score.

Pale-edge warnings fall from389 to275. This is **not complete halo clearance**:
the broad detector also catches legitimate grass/rock highlights. Do not darken
all275 merely to make `-RequireClean` pass. It intentionally remains a review
warning. Seven validator controls pass, including rejecting silhouette erosion
when alpha-mask preservation is required.

Rerunning the repair produces identical PNG hashes. The repository status gate
passes all18 evidence records and all8 installed/Git skill mirrors. The separate
generic skill-creator Python metadata check could not run because the bundled
Python lacks PyYAML; no environment packages were installed to hide that limitation.

`Station-left-before/after`, `Station-left-source`, `Station-right-after`,
`Wood-before/after`, `Station-light` and `Wood-light` are labelled-purpose 4x
nearest-neighbor inspection crops, not higher-resolution art or game captures.
`Upper-after.png` is a native 1x crop. Both light and dark backings were inspected.

The subsequent native pass covers eight camera/lighting cases with zero geometry
failures and actual gameplay screenshots. Real-biome/restoration transitions and
user visual approval remain open. Do not claim these files solved repetition,
deep cave handoff or production regional anchoring. No general-world promotion.

## Retained lesson

Installed and Git-mirrored `tmodloader-background-authoring` now route to
`references/feedback-repairs.md`: pin sources, preserve untouched native pixels,
separate color repair from alpha repair, retain semantic connectivity tests,
keep true highlights, and carry each correction into a reproducible check.
Biome composition stays in the bible; these coordinates stay here/in the recipe.
