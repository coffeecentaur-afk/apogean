# Native grove persistence and growth — 2026-09-04

Installed v3 tree PNGs are unchanged. tModLoader 2026.7.3.0 / Terraria 1.4.4.9,
single-player `Apogee Native Visual V3`, character `gg`, 2560×1369 viewport.
All `Live-*.png` files are original Windows captures including window chrome,
not assembled illustrations. The orange distant stems belong to the background.

## Observed results

- 22:31:22: Deep Blue applied to 21 native tree cells at root X4215, including
  three side-branch cells. The tree's root, trunk, cap, and both branch directions
  use the native paint path (`Live-branched-paint.png`). Original paint restored.
- 22:31:38: full-moon midnight sample (`Live-fullmoon.png`). Silhouettes remain
  visible, but fine bark is very dark without a light source. This is not a claim
  that every background/night pairing has passed readability review.
- 22:31:50: native `KillTile` produced four Wood, removed the struck/upper cells,
  and retained the lower trunk. Each of three brittle multi-tile prop families
  disappeared as a whole when one of its cells was removed. These are API checks,
  not a manual axe/player-contact test.
- 22:32:01: production sapling `RandomUpdate` grew a six-cell flat-ground tree
  after 25 accelerated calls, and an eleven-cell terrace tree after one call.
  A low stone roof prevented growth across 256 calls. The two-tile neighbor
  exclusion and sloped-anchor rejection passed. A flat planting area with
  sloped terrace edges supported a correctly rooted tree (`Live-growth-garden.png`).
  This validates the actual hook under accelerated stimulation, not elapsed-time
  growth probability or manual Acorn use. Native random variation is preserved;
  the fixture does not replace or reseed Terraria's global generator.
- 22:32:15: checkpoint captured bounds X4111,Y574,170×45 and SHA-256
  `A7BC78434F0D6FD9A3106BB88EDB1C94CA02ED813A4A724C38EB05684C5D97D6`.
- 22:32:31: native world and companion file saved; returned to the main menu.
- 22:33:56: reopened the same world. The digest of vegetation positions, tile
  types, native frame coordinates, paint and coatings matched exactly. The
  automatic lab explicitly skipped its destructive rebuild. The changed
  camera position reflects Terraria spawning the character, not a rebuilt grove
  (`Live-after-reload.png`).

## Reproduce

Use `Tools/Request-LiveValidation.ps1` with `vegetation`, then the
`vegetation-view-paint`, `vegetation-view-night-fullmoon`,
`vegetation-view-properties`, `vegetation-view-growth`,
`vegetation-view-checkpoint` and `qa-save-and-quit` fixtures in that order,
waiting for each request's log result. Reopen the exact disposable world.
A mismatched checkpoint throws before rebuilding; it does not repair itself.
Rebuilding another fixture explicitly discards the checkpoint.

Scope remains fixture-pass: fresh-world distribution, normal axe/shake input,
real-time planting/growth and multiplayer observation remain open. Current
checkpoint checks vegetation, not every terrain cell or the player's inventory.
