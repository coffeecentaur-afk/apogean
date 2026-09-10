# Player-directed curves: native QA, September 9

Mechanical fixture PASS, not final art or multiplayer approval. Only gg in
Apogee Native Visual V3 was opened. No normal worlds or world-generation code
changed. Native screenshots of all eight placements were inspected and shown
inline in the Codex session before and after reopening the saved world.

## Verified

- Full isolated build: zero warnings/errors. Installed package SHA256:
  `C03DAA74844D0E7C6E9C34CE9E29F12E0C6168E420CF09B7C1ADAAC2D78B02D2`.
  Mirror: `C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/f10592c5acbd47df8bb972a625723e15/apogean`.
- All415 other Content PNGs match rooted-v3. Only cluster atlas/contact bank
  expanded; no new art. All approved background/pod/bone overrides retained.
- Focused static gate passes:37 policy checks,119808 atlas/padding pixels,
  32768 masks,16384 wall projections, seven real-CLI negative controls, replay.
- At20:18:29, `orientation-curves` passes619 native checks and32768 actual
  texture/contact probes: eight saved variants,24 item-hook/preview/player
  cases, unchanged curve after moving, starter-pick removal through every
  part, one-item recovery, support-loss cleanup, immunity and transparent gaps.
- Eight separate specimens created in preflight-verified empty envelopes.
  Native floor/left/ceiling/right at room-relative x54/66/78/90, y6 for
  banks0..3, y22 for banks4..7; four supporting tiles per specimen.
- Actual save completed20:19:24. Reopened same world20:20:36; at20:20:37,
  `orientation-proof-view` verified all128 saved tile frames, required the
  persisted proof flag, and refused creation/rebuild. Eight variants intact.
- Original display-region digest unchanged through tests and proof creation.
  The preexisting grove mismatch remains RED with the same actual hash.

`native-checks.log` is filtered actual runtime output, including the known
failure; not a synthetic success summary. Timing is client-log local time.

## Why the original reload test failed

`orientation-reload` first checks six historical display coordinates. The
first expected cluster at5866,528 is now empty. Read-only audit found intact
clusters at5862,512 (bank1),5879,504 (bank2),5894,520 (bank3),5870,528 and
5874,528 (bank0), plus another bank0 at5878,528. This establishes layout drift,
not its cause. Do not say we proved who moved it. Five expected displays retain
their exact frames. Original layout assertions remain unchanged/failing; no
display was restored or rebaselined to force a pass.

New `orientation-curves` independently tests an empty6x6 envelope around
5928,516, then verifies old display-region contents unchanged. It deliberately
does NOT certify the historical six-position layout. `orientation-audit` is
read-only. `orientation-proof` builds once only in empty space; subsequent
calls verify. `orientation-proof-view` requires previously saved proof.
Never use `orientation-build` on this room.

Initial diagnostic compilation caught a local-variable shadowing error; renamed
the snapshot variable and rebuilt cleanly before installing. No failed build
was installed. Pre-update save/package backup remains at
`C:/Users/max_h/AppData/Local/Temp/ApogeanCurveBackup-05428e5bdf3643f283348400f11af79a`.
Do not downgrade this now-eight-bank world to a four-bank package.

## Remaining gates

Native API preview/style is proven, not a manual mouse-placement matrix.
Manual all-surface cursor feel, multiplayer, real terrain slopes/irregular
supports, and worldgen are still pending. Supported anchors currently require
four full solid tiles. This is not certification on arbitrary rough terrain.

Artwork is still the old detailed artwork, mechanically mirrored. User wants
simpler teeth and a quieter grey/black/brown mineral material with sparse amber
fissures instead of autumn-leaf-like MawDirt. Show ONE native-scale art proof
before batch export/installation. Preserve individual short/long/wide models;
their final placement/physics remain separate. No acid/mob/boss expansion yet.
