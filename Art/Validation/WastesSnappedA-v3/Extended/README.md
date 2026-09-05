# Extended grove checks — 2026-09-04

This is a test extension, not a new art revision. All three installed v3 tree PNGs are unchanged.

## Reproduction and safety

- tModLoader 2026.7.3.0 / Terraria 1.4.4.9; single-player **Apogee Native Visual V3**, character **gg**. Viewport 2560×1369 plus window chrome.
- Four native GrowTree calls built the grove at `{X:4111 Y:574 Width:170 Height:45}`. The fourth tree was the existing mid-trunk chop control. Other vertical wooden silhouettes in the screenshots belong to the background, not additional fixture trees.
- World/character backup: `C:/Users/max_h/AppData/Local/Temp/Apogean-TreeMatrix-Backup-c21dfe5ae9254e67bbe34a486afcfeff`.
- The file-request bridge now consumes commands only inside the two named disposable single-player worlds. Vegetation weather/paint additionally requires Native Visual V3 and a registered grove. Other fixture commands invalidate the old grove coordinates.
- Write one allowlisted command to `Captures/ApogeanLiveValidation.request` in the tModLoader save directory. Use `vegetation`, then `vegetation-view-wind-left`, `vegetation-view-wind-right`, `vegetation-view-night`, `vegetation-view-paint`, or `vegetation-view-properties`. `vegetation-view-release` restores temporary clock/weather/paint. Each visual case expires after 3600 simulation ticks; a paused client does not advance that timer.
- The properties command is destructive and must be preceded by a fresh grove. It uses native engine APIs, not synthetic axe input. `qa-save-and-quit` releases temporary state before invoking the ordinary engine save/quit path. Do not rely on force-closing the client to restore temporary state.

## Observed results

| Evidence | Result and limit |
| --- | --- |
| `Live-day-control.png` | Three intact native trees plus cut control; flat anchors and current v3 bark. |
| `Live-wind-left-A/B.png` | Two ordinary Windows captures with HUD **40 mph E**, internal wind −0.8. Tops bend but remain connected; visible branch joints remain attached. |
| `Live-wind-right-A/B.png` | Two captures with HUD **40 mph W**, internal wind +0.8. No observed top/socket or branch/trunk gap. These are sampled frames, not an exhaustive animation sweep. |
| `Live-night.png` | Midnight/new-moon sample. Trees are very dark away from the player's light; this records natural nighttime behavior but does **not** establish the full night readability gate. |
| `Live-deep-blue-paint.png` | Native Deep Blue paint covered 17 cells of the middle tree, including trunk/root/top. This particular tree had no side branches: branch-paint coverage remains open. |
| `Live-properties-after.png` | First tree lost the cut/upper section, retained lower wood, and dropped wood. Temporary blue paint was restored. |

All listed PNGs are unedited Windows game-window captures, not offline reconstructions.

Client log milestones:

- 22:02:39 world enter; 22:02:41 successful grove/reference export; 22:02:44 native capture, water style 0.
- 22:03:09 wind-left; 22:03:41 wind-right; 22:04:07 night; 22:04:19 paint (17 cells).
- 22:04:32: `VEGETATION PROPERTIES: PASS native KillTile woodDelta=3; cut removed above/retained below; 3 whole-prop removals; API fixture, not manual axe/contact or multiplayer proof.`
- The whole-prop assertions removed one interior cell from a 2×1 tuft, 2×3 bristle and 3×2 shrub; no cells of the original object remained in any of the three footprints. This is an engine tile-break test, not a walking/contact animation test.
- 22:07:04 save/quit consumed; 22:07:05 world-save validation; 22:07:06 companion `.twld` updated. Returned to menu and exited normally. No new exception/error appeared in the inspected session log.

Initial and final isolated builds: **0 warnings, 0 errors**. Tree and Terrain static gates passed. Final cleanup invalidates stale fixture bounds and adds an explicit unsupported-upper-trunk assertion; it compiled cleanly after the live session, but was not followed by a second full live matrix.

## Still open

Keep v3 at **fixture-pass**. Full night readability, painted side branches, sloped/stepped substrate and tight spacing, actual axe and shake input, natural random-update growth, preserved-grove reload, multiplayer observers, and fresh-world distribution remain unverified. The QA world currently rebuilds vegetation on entry, so successful save/quit is **not** persistence proof for the same tree geometry. Grass/terrain observations are recorded separately in `Art/Validation/WastesGrassProduction-2026-09-04/README.md`.
