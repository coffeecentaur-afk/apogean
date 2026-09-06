# Modular landscape study — Deep v1

**Latest review: revisions required.** User found pale foreground cutout edges, detached gas-station tree limbs, excessive Close repetition and the wrong local-ground/head-height placement. These remain unresolved. Structural tests and the bounded camera matrix are not art approval; see the new red reproduction probes in the live-evidence record.

**Current: installed only in the disposable QA-world/Forest-lab renderer after the user's go-ahead. Not promoted to ordinary worlds or final art.** Still four terrain designs: highway hill, quiet pipe hill, station hill, and a longer foreground bank. A fifth built-in image-generation call added only the foreground's lower extension. The final bank is1448×1915 with829 new native rows; the approved upper830 rows are unchanged. There are now24 offline camera studies, not24 designs. See [live evidence and limits](../../../../Validation/WastesModularLandscape-2026-09-05/README.md) and [extension prompt](FOREGROUND_DEPTH_PROMPT.md).

The earlier measurements below document the original review-only checkpoint. Its short-foreground blocker is superseded for ground and400px shallow descent at1080p/1369p/1440p, **not** for arbitrary underground depth/cave handoff. Current runtime raw RGBA is28.39MiB. `Foreground-Bank.png` remains the original1086px source export; `Foreground-Deep.png` is the1915px runtime candidate. No Mid/Close reflected fill is used.

## Start here

- [Layered ground view](Derived/Layered-Ground-1080.png): new long foreground bank offset against the smaller Mid groups.
- [Same assembly, elevated camera](Derived/Layered-Elevated-1080.png): ground-locked Close leaves view; Mid takes visual prominence.
- [Mid without an altitude fade, 1440p stress view](Derived/High-NoFade-Stress-1440.png): artwork has to cover the exposed region without relying on fading to hide its bottom.

These are labelled offline compositions at 1 source pixel per output pixel, **not tModLoader screenshots**. Real tiles, entities, lighting, engine matrix effects and production scene selection are not reproduced. The grey banded Far terrain is the unchanged earlier reference and remains unapproved; it is not the new Mid layer. Its quieting is still required.

## Original review checkpoint components

| Component | Actual exported pixels | Role |
|---|---|---|
| Highway |576 x 1408|Broken road and connected descending shoulder|
| Quiet |391 x 1408|Low pipe hill between larger landmarks|
| Station |488 x 1408|Abandoned fuel station on a cliff|
| Foreground bank |1448 x 1086|Long connected near bank, about 2.5x the widest Mid group|

Mid groups use 360px open intervals; the cycle ends with a 480px interval. Thus an open valley, quiet hill and another open valley separate the major highway/station landmarks. This is one review layout, not a production variation library. The foreground uses a longer bank and an 820px open interval, offset from Mid; different .30/.14 horizontal rates permit natural overlaps. Do not track individual landmarks every frame to prevent overlaps.

The complete source contains a 256-row overlap between original upper/lower cliff art. A minimum-error connected cut selects existing source pixels, never blends, enlarges, mirrors or synthesizes a sediment fill. Only the first640 continuation rows are used: the generator joined the valleys farther down, so that portion is rejected. The resulting1408px extent preserves two fully open source-column gaps of40/41px, before the additional assembly spacing.

`Joined-Top.png` and `Joined-Continuation.png` split that accepted export at row768 and reconstruct it exactly. These are compatible pieces of **this** assembly, not evidence that arbitrary future hills share compatible sockets.

## Static evidence and limits

`pwsh -NoProfile -File Tools/Preview-WastesModularMid.ps1` reproduces the files without changing anything in Content. It uses PowerShell's bundled drawing assemblies without altering execution policy.

`pwsh -NoProfile -File Tools/Test-WastesModularMid.ps1` checks all2,162,688 Joined pixels: dimensions, hard alpha, absence of remaining magenta, open valleys, exact top/lower reconstruction and all three cropped modules. It passes. Six in-memory negative controls fail as intended. Each was additionally exercised through the real CLI with `-Mutation 1` through `6`, returning nonzero for SOFT_ALPHA, MATTE, VALLEY_CLOSED, SOCKET_SHIFT, GROUP_DIMENSIONS and GROUP_SHIFT respectively. No source files are mutated by those controls.

Mid exports retain813,629 transparent pixels with zero partial alpha. The foreground cleanup accidentally baked a neutral near-white checkerboard; the candidate-only exporter removes that matte and1,196 exposed light low-chroma fringe pixels. Its export has484,934 transparent pixels and zero partial alpha. Internal material colors are not recolored. Edge aesthetics still need review; alpha counts do not approve a silhouette.

The three cropped Mid textures occupy7.815MiB raw RGBA; the foreground bank about5.999MiB. With one existing10MiB Far texture the proposed set is about23.814MiB before additional Close depth, alternates or mipmaps. This is an allocation estimate, not measured game/GPU residency. Joined/debug/source images must not all become runtime textures.

Eight Mid views cover ground, first-third ascent, shallow descent and high-flight/no-fade at1080p/1440p, using the existing pure projection helper plus a proposed220px Mid art-alignment offset. The .03 Mid response is unchanged in this comparison; the newly discussed regional Mid-anchor behavior is **not implemented or approved**. The two new-foreground views cover only1080p ground/elevated composition, nominal soil row330, while retaining the actual Close world-lock calculation.

**Foreground depth remains a known blocker:** this bank is22px short at1440p ground,242px short at1080p shallow descent and about555px short at1440p shallow descent under those assumptions. The renderer helper throws if asked to submit that exposed bottom. Author at least another640 native rows of compatible lower Close terrain before those cases, then recheck the full depth/cave handoff. Do not stretch, mirror, fade early or silently suppress those failing cases. The current eight old-Close reference views include its existing labelled QA continuation guard; that guard is not reused on new modular Close or Mid.

Further gates: foreground continuation and edge polish; quieter Far art; horizontal/end-slope review; real foreground-terrain occlusion; regional ground datum; final Mid anchoring; live rotation/gravity/viewport/routing/lighting checks; performance/residency; restoration geometry; art approval. No additional biome or faction family was advanced.

## Provenance

Original source masters are retained unchanged:

- `Upper-Cliffs-Matte.png`: built-in generated `exec-5b15e8ab-9ef3-4b00-9e48-4d7c0a3e1324.png`,1536x1024.
- `Lower-Cliffs-Matte.png`: `exec-6d8cc842-2a60-44c7-949a-d90a3b812d1a.png`,1536x1024.
- `Foreground-Cleanup-Source.png`: `exec-3abff4e0-0eaa-4d13-bf5a-cc3ebddbf167.png`,1448x1086. This replaces the fringe-heavy draft for preview; the rejected first foreground draft is not a runtime asset.

Exact prompts are in [PROMPTS.md](PROMPTS.md). The requested source dimensions were not consistently honored by generation; actual dimensions above are authoritative and nothing is upscaled to claim more detail. No game was started, rebuilt or modified during this art-only pass. Existing QA grove persistence and cave-transition issues remain separate unresolved work.
