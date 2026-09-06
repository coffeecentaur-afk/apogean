# User review: preserve Station; foreground perimeter still fails

The user accepts the gas station and likes the layered Wastes direction. They
still reject the prominent pale foreground rim and request a more demolished-city,
cracked-earth Far layer with a related darker, dusty palette.

The Station repair is frozen at SHA256
`C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C`.
That acceptance does not approve the foreground or all background rendering.
The previous movement evidence remains valid; its geometry pass is not a clean
edge result. No PNG, renderer, game process or world changed in this review pass.

## Findings

- The original Close exporter keyed a baked neutral-white matte, removed one
  exposed fringe layer, and promoted remaining source colors to full opacity.
  See `Tools/Preview-WastesModularMid.ps1:40`.
- The subsequent Exact-v1 correction changed only 114 pixels in three timber
  rectangles. It did not correct the whole turf/cliff perimeter.
- The old broad edge probe stopped at row440. The new full-perimeter inspection
  of the exact QA candidate finds417 pale-neutral opaque boundary candidates,
  including188 below that cutoff, zero partial-alpha pixels and zero nonblack RGB
  pixels under alpha0. These thresholds differ from the older275-candidate probe;
  the counts are not a before/after comparison.
- Example: a long exposed side at X1351/Y457–472 retains fully opaque neutral
  grays around RGB130/129/130 down to111/111/115. These colors exist in the asset
  before game tinting; changing the Far color cannot remove them from the PNG.
- These are material-review candidates, **not417 proven matte pixels**. Retain
  deliberate stone highlights and pale grass where appropriate. Source matte
  provenance plus these opaque edges supports incomplete edge-color cleanup as
  a contributor, not a measured exclusion of every possible GPU sampling effect.

Reproduce the deliberately red read-only review probe:

```powershell
pwsh -NoProfile -File Tools/Inspect-WastesForegroundPerimeter.ps1 -RequireNoReviewCandidates
pwsh -NoProfile -File Tools/Test-WastesForegroundPerimeter.ps1
```

`Foreground-full-perimeter.json` pins the exact source hash. The CLI controls
separate dark edges, pale upper edges, pale lower edges and hidden transparent RGB.
No test silently marks a flagged material as acceptable. Full image/GPU tests
remain separate.

The four CLI controls, existing export/connectivity regressions, all18 status
records and all8 installed/Git skill mirrors pass. The general skill metadata
checker could not run because bundled Python lacks PyYAML; no package was
installed and that check is not claimed passed. Only the focused repair reference
changed, not skill frontmatter or invocation settings.

## Next bounded decision

Keep the accepted Station and overall layer arrangement unchanged. Park the
foreground as visually rejected; inspect and recolor the specific exposed
grass/soil/rock perimeter using material-appropriate colors on a candidate with
the same alpha silhouette. Preview against both a dark backing and the intended
Far palette before any new live installation. If a satisfactory narrow repair
cannot preserve the silhouette, propose a simplified foreground rather than
automatically launching another whole-background generation cycle.

The distant city revision is an art specification, not a camouflage patch:
collapsed buildings and ruined industrial silhouettes above cracked dusty terrain;
low-contrast soot/umber/smoky-gray distance, related to the ochre Near/Mid layers.
Reduce the repeated rock-wall bands. Preserve quiet intervals and transparent sky.
Show one composition study first; do not expand into other biome families.

API references checked: [sampler state](https://docs.monogame.net/api/Microsoft.Xna.Framework.Graphics.SamplerState.html)
and [blend state](https://docs.monogame.net/api/Microsoft.Xna.Framework.Graphics.BlendState.html)
describe independent sampling/blending controls. They do not establish which
state the live FNA/tModLoader batch used. No sampler/blend change is justified by
these docs alone, and none was made.
