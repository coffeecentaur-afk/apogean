# Dry cave backdrop seam — Q/R native evidence

September12, 23:12–23:29UTC. **Bounded technical proof only; no production
background or final art acceptance.** Uses existing cave at4660,689,96×64 in
disposable `Apogee Native Visual V3`, player `gg`, single player. No tile/wall
write, player teleport, weather edit, saved zoom change or ordinary-world test.
The player remains at the surface and ordinary simulation continues there.

## Build identity

- Installed tModLoader1.4.4.9+2026.07.3.0, revision
  `666f69962d3bdffde54fc14025f02634965b4e7c`.
- Runtime DLL SHA256
  `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C`.
- Q package `A6A5AF99CFEC3554C484A7B12DF0C63A8DE1702F68BAF3BB41ADD64192091B6F`,
  mirror `efc6657f72794eb6b7b84527561872db`.
- R package `019B55154B7AEB8C8FD2DFEDAF900183F37A384DE02AA4AC16ADC3F686D73DAE`,
  mirror `85fa920080764f21a49ff6435b9df159`.
- Both isolated wrappers completed with0 warnings/errors. All466 pinned base
  assets retained; seven separately verified anatomy additions plus one explicit
  `-MawCaveBackdropStudy` PNG. No production art changed. PNG source/provenance:
  `Art/Candidates/MawCaveBackground-v1/README.md`.
- Study1536×1024 SHA256
  `D9388320C53DCFFB5E617B05E5521EA264B784552584CBE44391BF24409A5B09`.
  6,291,456 logical Color texel bytes; not measured incremental RAM/VRAM.

## Failure retained, not erased

Q selected the cave and held its normal baseline. Activation refused before any
draw because the first guard treated a registered shader's `IsVisible` value as
proof it occupied the manager's draw list. `native-q-guard-refusal.json` preserves
the failure. Q locate/release JSON retain zero callbacks and equal fingerprints.

Installed code inspection establishes that `SimpleOverlay.IsVisible` reads its
shader, not manager membership. R logs Sandstorm/Blizzard as visible/FadeOut with
opacity0 but **scheduled=false**. Read-only reflection now checks actual scheduled
membership and still rejects any other scheduled effect. It neither alters nor
evicts those weather effects. Unknown manager layout and an occupied probe key
remain hard refusals. This is deliberately conservative QA, not a coexistence API.

## Observed normal frames

The main agent inspected baseline, pattern, art, diagonal pan, alternate zoom,
restored origin/zoom, returned baseline and released player-camera frames through
Windows Computer Use. Those frames are displayed in the task, **not saved as image
files in this evidence directory**. These JSON files are telemetry, not screenshots.
Native photo CaptureManager omits this overlay; no photo-based visual proof is claimed.

- Cyan/rose pattern and then the candidate draw behind the existing solid stone,
  opaque native walls, pots, cobwebs and minecart rail. The existing brick structure
  at the right stays opaque. No terrain was removed to expose the candidate.
- Pan(+16,−16) world pixels and temporary1.25 matrix multiplier preserve observed
  tile/background alignment. Origin and normal zoom restore the original view.
  This is a single shift, **not** continuous long-distance parallax/repeat testing.
- Pools retain their native backdrop with a conservative two-tile margin. The
  transition is visibly blocky and the scenery does not continue through water.
- The palette-reduced study reads flat/banded. Some dark openings retain bright,
  block-shaped portions of art: native foreground occlusion is not the same as
  giving custom art a valid ambient-light response. Final art/lighting remains open.
- Baseline restores original brown cave scenery, release restores the surface
  player camera, and the game saves/exits normally. No gameplay/traversal claim.

## Raw R reports (13 files, copied byte-exact from Captures)

Command reports may contain the previous sampled `lastFrame`; do not interpret
command acknowledgement as a settled renderer result. The four settled reports:

| Report suffix | Camera | Matrix zoom | Draw path |
| --- | --- | --- | --- |
|232051-358-2fb0ac|74048,10851|1.3333334|cached, Point|
|232143-541-7fc36b|74064,10835|1.3333334|cached, Point|
|232653-542-7eb829|74064,10835|1.6666667|cached, Point|
|232730-742-0e934f|74048,10851|1.3333334|cached, Point|

All four: viewport2560×1369, one current render target, `drawToScreen=false`,
`offScreenRange=192`, 5730 dry cells,414 excluded,103 draw runs. Code subtracts
camera once and inherits the matrix; no off-screen margin is applied a second time.
Every R report has equal original/current selected-region fingerprints:
`F651FA609E4139509751F9086914205C2B440E9068B4A297B090E31B7689C60E`.
Search sampled130 candidate windows in2.7909ms. This is one search, not a worldgen benchmark.

`native-r.json`:17 verbatim scoped records,0 relevant caught draw failures. Save
at23:28:32UTC retains contour`6F7998FF...` and shallow interaction`24163E65...`.
The unrelated historic grove restore refusal still exists and was not repaired
or rebaselined. The scoped export is not a clean-all-logs assertion.

## Regression and remaining gates

`-Profile MawCave` runs the dry prefix-mask oracle (430080 comparisons across70
layouts and a deliberately broken boundary mutant),80 overlay membership cases
plus a rejected visible-only mutant, and scoped evidence-export tests retaining
four draw-failure kinds while excluding unrelated records. These are source/stub
tests, not independent native runs or pixel-perfect liquid tests.

Still needed before a production decision: own lighting/dormancy and darkness,
wet/animated liquid coverage, repeat/layer composition, other viewports and direct
draw mode, normal biome routing, player/NPC overlap, compatibility, performance
and complete load/unload lifecycle. The inactive QA overlay binding persists in
the manager until engine reset; no public unregister is claimed. Preserve the
master and shelf studies; stop speculative art redraws pending review. The narrow
seam question is answered. Do not promote this as the finished Maw cave renderer.
