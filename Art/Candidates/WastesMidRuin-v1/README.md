# Mid ruined building — first draft, 2026-09-06

Status: **review-only concept; not runtime-ready; not installed**.
One built-in image-generation call. Style references were the accepted Far city
color master and exact Station cutout, not targets for replacement. Neither
reference nor any Content PNG was modified. Exact prompt: `PROMPT.md`.

`Concept-original.png` is 1024x1536. Pixel inspection found **zero transparent
pixels and zero partial-alpha pixels**: the checkerboard is painted RGB, not
usable transparency. The file is retained verbatim as a concept, not passed
through a color key or quietly rescaled into service.

## Design verdict

### Latest user review — 2026-09-06

The user likes this building's appearance and asks for two or three ruins total.
Retain this broken shell as the first design. That visual acceptance supersedes
the initial reviewer objection below; do not force another redesign because the
previous agent preferred a flatter view. Alpha and native-scale assembly remain
unresolved technical gates. The two additional proposed shapes and per-biome
direction are in `BACKGROUND_BIOME_DIRECTION.md` and
`Art/Candidates/WastesMidRuins-v2/`. This PNG remains verbatim, not installed.

### Initial reviewer notes (historical, superseded where noted)

- Useful: broken two-storey concrete shell, collapsed wing, exposed floors,
  rusty rebar, sparse dead grass, deep irregular support and exposed drain.
  Warm sooty grays/umbers connect the distant city and roadside vocabulary.
- Fails: too much three-quarter/raised viewing perspective for the agreed side
  view; building is wider than the current 391–576px Mid modules, and its shape
  is not yet an authored runtime assembly. Fine texture needs a native-size
  review. A deep cutout by itself cannot prove flight-safe placement.
- Export fails: opaque matte. The reusable explicit-mask exporter exists, but
  extraction cannot correct perspective. Do not spend more cleanup iterations
  on this draft before resolving that design issue.

Next: review the architecture/silhouette only. If retained, produce a bounded
side-view correction and a separately reviewed mask, then fit a native-scale
upper/cliff/continuation assembly and test both backings, joins and flight.
This earlier checkpoint did not authorize installation or extra variants; the
latest user decision above now authorizes two additional concepts only. Preserve Station and Highway
and the quiet intervals between them. See `Tools/BackgroundMaskWorkflow.md`.
