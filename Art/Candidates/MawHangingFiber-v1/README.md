# Hanging fiber candidate

September 11, 2026. A six-section, rigid, cuttable strand study, not a rope.

Original image-generation source: `source.png`, an amber/brown root cord on an
explicit magenta matte. `Tools/New-MawHangingFiberCandidate.ps1` derives a binary
mask, uniformly fits the isolated silhouette, quantizes to eight colors, and
uses the established masked export. It does not preserve a fake checkerboard.
Native-v1 holds the separate mask/color master, final transparent 16×96 strand,
and 18×108 sheet (six 16px rows plus 2px framing gutters). All 506 opaque pixels
form one connected strand. Source and atlas hashes are in `recipe.json`.

The seven identical examples in the native fixture are controls, not a proposed
natural placement pattern. Growth is explicit, bounded to those roots, capped
at six segments, dry-only and disabled outside gg/V3/single player. Short strands
end at broken sections; only the complete strand uses the tapered terminal.
No wind-splitting, pickups, automatic spread, climbing or ordinary worldgen.

First 180 native lifecycle checks passed and full strands rendered. A later
saved-exhibit check failed at local44,8; preserve this red state and investigate.
Do not rebuild it or treat isolated scratch checks as saved-scene acceptance.
Evidence and current status: `Art/Validation/MawAnatomy-2026-09-11/README.md`.
