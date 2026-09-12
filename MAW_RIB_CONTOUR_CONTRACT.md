# Maw rib contour study v1 — separate specimen

This is a geometry-only comparison, not production generation or a replacement
for the immutable shallowV1 traversal scene. Keep its original failed contour
visible and its saved terrain hash intact.

## Candidate and controls

Three rows contain lengths 8, 11 and 12 tiles. Each row, left to right: legacy
right-pointing, candidate right-pointing, legacy left-pointing, candidate
left-pointing. Existing porous bone remains the two-column root; the provisional
cortical material covers the rest. Two stone columns back each root. No walls
conceal contour holes. No new textures, rope, safe route, teeth or worldgen edits.

The old independently rounded top and thickness produce underside notches. The
candidate uses continuous top/bottom boundaries, a monotonic bottom envelope,
and native hammer triangles at both edges. Pixel-center occupancy is continuous
at joins, horizontally mirrored, solid at the insertion, and tapers to a point.
Only lengths8..16 and directions±1 are accepted; generation is finite and never
runs in a drawing/update hook. Runtime collision remains Terraria's solid/slope
collision, not a custom polygon approximation.

Engine reference: [Tile slope properties](https://docs.tmodloader.net/docs/stable/struct_tile.html)
describe top slopes as solid below and bottom slopes as solid above. The native
enum order is solid0, down-left1, down-right2, up-left3, up-right4. The positive-X
upper edge uses1; its lower edge uses4. Native screenshots must still verify this
orientation; the independent offline pixel oracle is not the renderer.

## Reservation and persistence

Only named V3/SP/gg may explicitly build. The124×82 footprint has a12-tile guard,
40-tile world margins, and at most20 candidate positions. Reject any existing
tile/wall/liquid/wire/paint/coating, actor, protected structure, or saved gallery
within its24-tile exclusion. Reserve before writing. Reuse the shallow study's
actual value-storage snapshot helper; rollback only the newly owned empty guard
on placement failure. Retain the failed reservation. Never rebuild/rebaseline.

The old sixteen galleries and shallowV1 are hashed around every request. New
shallow requests also preserve the contour specimen. Creation hash and actual
saved interaction hash are separate. A reload mismatch stays a failure.

## Native matrix

Use `Request-MawRibContourValidation.ps1` for explicit build, pristine, reload,
short/medium/long held row, capture, inspect-on/off and release. Plain can inspect
existing content but cannot construct. Held views fix noon, player and camera;
release restores pre-visit position/time/weather. Four neutral row lights may be
enabled only for the held starter-baseline Plain character, and captures say
`inspection`. They prove neither natural lighting nor difficulty. No gear/health
is changed. Existing actors are never killed; new ambient spawns are suppressed
only during Plain's scoped visit.

Inspect all lengths and mirrors at native1x. Reject remaining detached cells,
wrong slope triangles, visible atlas holes or new texture seams. Compare shape
before considering a second, separately versioned art pass. Limit to two focused
revisions before parking an unresolved art question. Full corridor traversal,
rope/grapple use, multiplayer and worldgen adoption remain separate gates.

## Initial evidence

`Test-MawRibContour.ps1`:642664 assertions across native-shape pixel occupancy,
9 lengths, mirrored triangles, roots/tips, invalid inputs and a missing slice.
All three legacy silhouettes fail the continuity oracle as intended. This count
is mostly pixel comparisons, not hundreds of thousands of independent scenarios.
The request/context gate passes1009 checks. Package N builds0warnings/errors,
466 pinned assets unchanged, SHA256
`F1D5D1103DF3A885C719E175AA5B40AF5A25C9C7D8A1EC62287EED19D23BDD4C`.
Native N placement, exact save/reload, all three row captures and clean-background
transition replay now pass their scoped checks. See
`Art/Validation/MawRibContour-2026-09-12/README.md`. Final material/shape preference,
actual traversal on these candidate ribs and production integration remain open.
