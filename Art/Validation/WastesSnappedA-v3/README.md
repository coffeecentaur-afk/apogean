# Snapped A v3 — bounded live grove, 2026-09-04

The user accepted the quieter v2 bark/hollows subject to slightly thicker branches. V3 adds one native pixel on each side, preserving the length, centerline and native mirrored offsets. Trunk and top hashes remain identical to v2.

## Evidence obtained

- Both the native tree-set validator and focused palette/contour/pivot regression pass. The added thickness regression checks every authored left-branch column; unchanged v2 fails the thickness requirement. Right-side geometry is covered by the full mirror/pivot check.
- Isolated build completed with **0 warnings, 0 errors**. Candidate PNGs were first copied only into the build mirror, leaving repository Content untouched during initial QA.
- Cold client launch: **tModLoader 2026.7.3.0 / Terraria 1.4.4.9**. Loaded only **Apogee Native Visual V3**, with the established test character `gg`.
- The existing `VegetationLabGallery` places native saplings, calls `WorldGen.GrowTree` four times, and validates a native mid-trunk `WorldGen.KillTile` split: the hit/upper segments disappear and the lower stump remains. This passed without exceptions before reference export and capture.
- `Live-grove-day-wind-A.png` and `Live-grove-day-wind-B.png` are unedited Windows game-window captures. The client viewport was **2560×1369** plus window chrome. HUD readings show **11 mph W** at 12:22 PM and **10 mph W** at 1:02 PM. Inspected the actual grown trees, branch/trunk joints, top sockets and flat-ground roots; no visible gaps in these samples. These are not an extreme-wind animation sweep.
- `Native-capture-probe.png` retains the separate Terraria capture-camera output. Use the Windows captures for ordinary gameplay appearance; capture-camera output is not interchangeable with a viewport screenshot.
- The identical tested PNGs were then copied into the three production tree texture paths. No tree/gameplay C# changed.

Relevant client-log sequence: world enter 21:45:26; vegetation references exported 21:45:27; vegetation capture probe 21:45:30, water style 0, Forest routing, viewport 2560×1369. No new exception was logged through the inspected session segment.

## Safety and remaining scope

No unrelated world was loaded or edited. Before entering QA, the existing character and disposable world files were backed up to `C:/Users/max_h/AppData/Local/Temp/Apogean-SnappedTreeV3-Backup-8e20b73eefe642fdb1a56f06a9da66bd`.

This is **fixture-pass**, not integrated or polished. Night/strong-wind, paint, slope and tight-spacing cases, actual axe hit/drop/shake behavior, natural random-update growth, reload, multiplayer observation and fresh-world distribution have not been repeated for v3. The direct native GrowTree/KillTile proof must not be described as manual acorn planting or axe-input coverage. Prior candidates' results do not close these remaining gates.
