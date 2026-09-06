# Wastes terrestrial-to-Space handoff — native QA

Focused result: **Space presence/absence fixture passes**, not whole-background
art or production acceptance. The previous failing city-in-Space screenshot
remains in `../WastesCityRuntime-2026-09-05/sky.png`.

## Change and cause

Far had no upper-altitude opacity envelope. Its dark RGB-floor tint also made
the sky's alpha alone insufficient to hide it. `WastesCameraProjection.LandOpacity`
now multiplies the renderer's full submitted color for all three land layers.
The first red arithmetic run reproduced old Far opacity1 for a Space player.
No anchors, texture pixels, ecology, world generation or NPC behavior changed.

Policy: retain Far through70% of global ground-to-Space ascent, smoothstep to0
by the installed integer-center Space boundary, using the higher of camera and
player altitude. Mid retains its earlier fade; Close keeps exiting by world
motion with this additional upper safety net. No new downward fade. The70%
choice belongs to Apogean, not Terraria. See the version-pinned research note.

## Build and exact inputs

- tModLoader2026.7.3.0, Terraria1.4.4.9, disposable SP **Apogee Native Visual V3**,
  8400x2400, playergg. Ordinary worlds were not opened.
- Isolated build:0 warnings,0 errors. Command:

  ```powershell
  pwsh -NoProfile -File Tools/Build-ApogeanIsolated.ps1 -WastesCutoutCandidateDirectory Art/Candidates/WastesFarCity-v1/QA-Package-v1 -WastesCityCandidateDirectory Art/Candidates/WastesFarCity-v1/Runtime-v1
  ```

- Installed apogean.tmod SHA256:
  `6B2F8A1D4796A3D92E40AD76BD38A6EC494C636091866EACB900FAA34F1DE733`.
  This build includes the pre-existing local working tree; unrelated localization
  and world-systems research edits are excluded from this change's commit.
- Far1458x1792 SHA256 `D61106D719B292675607B8D9275BDCA73BE7CE01D9405606CF04EB8D1C966FE6`.
- Station488x1408 SHA256 `C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C`.
- Close1448x1915 SHA256 `9D039C003929EC128F43C3DFEFB3F98C22DC15488EB0AA9B0F9151C00D726FE4`.
- Pre-run backups: local temp `ApogeanSpaceQA-785a16bc9f2c47a790ca3b1debd3df38`.
  These include the prior mod, disposable world/player and display config.
- Both sessions ended by normal save/validation and menu Exit. Original
  2560x1369 window dimensions were restored after the1080p check.

## Evidence, separately scoped

| Check | Result |
|---|---|
| Actual pure policy |18180 checks: ground, shallow descent, early/late ascent, camera/player offsets, monotonicity and stateless return|
| Policy CLI controls |Unchanged passes; always-visible, always-hidden, abrupt-cut, camera-blind and player-blind implementations fail|
| Native2560x1369 |Ground, partial fade, Space boundary, high Space and continuous20-second ascent/descent all pass|
| Native1920x1080 |Same six cases pass, including real full-color and zero-color endpoints|
| Live-log CLI controls |Both logs accepted; visible Space, missing render samples, missing completion, no intermediate fade and no ground evidence rejected|
| Separate geometry |Submitted positions, clipping coverage and world-ground anchors checked independently; invisible geometry is not visible-art evidence|

Both full diagonal sweeps pass at both viewports:128640px /4.853 Far periods
at2560 and129280px /4.877 periods at1920. Maximum left/right geometry gaps0px;
module projection error<=0.01px, ground rounding<=0.97px. Surface terrain can
obscure joins, and endpoint screenshots do not replace the live telemetry.

At the partial hold Far submits factor~0.499 and alpha127. At the Space boundary
and upper Space, every land layer submits factor0 and alpha0. Ground submits1
and255. The native depth meter reads839' Space at entry; these feet are specific
to this world, not a universal threshold. Actual zoom differs: background/game
zoom1.3333 at2560x1369,1 at1920x1080. Source images were not resized.

Each flight interpolates over1200 ticks plus600 endpoint ticks. Telemetry includes
per-second Far samples; the replay requires monotone intermediates and explicit
1-to0 /0-to1 endpoints. Samples can repeat within an update because multiple
draws occurred. No FPS or frame-time claim follows from those counts.

## Screenshot index

All PNGs are unretouched native window captures, including titlebar/UI. They are
not offline mockups, and no video or uninterrupted human-observed flight is claimed.

- `ground.png`, `ground-1080.png`: normal low-altitude art remains visible.
- `space-fade.png`, `space-fade-1080.png`: acknowledged partial transition.
- `space-edge.png`, `space-edge-1080.png`: clear native sky at Space entry.
- `sky.png`, `sky-1080.png`: upper Space, stars, no ruined city or land layers.
- `ascent-space.png`:2560 ascent endpoint; `descent-return.png`:2560 descent
  endpoint with full Far/Mid. The return is the global ground datum above the
  actual local floor, not the identical ground-hold composition.
- `diagonal-left-endpoint.png`, `diagonal-left-1080.png`: sweep endpoint holds,
  not active-flight captures. Forced Forest scenery over an ocean is intentional
  in this camera lab and is not real ocean-routing evidence.
- `below-ground-1080.png`: shallow descent; foreground still present above,
  real foreground terrain/walls obscure much of the view. Not a visible deep
  cave-handoff approval.
- `expired-hold-return-1080.png`: deliberately retained **non-evidence**. The
  first1080 fade capture was taken after the30-second hold returned. It was
  relabeled and the acknowledged case rerun before the valid fade capture.

## Reproduce

Replay `telemetry-2560x1369.log` or `telemetry-1920x1080.log` using the matching
viewport argument; these are filtered native QA records, not full client logs.

```powershell
pwsh -NoProfile -File Tools/Test-WastesSpaceHandoff.ps1
pwsh -NoProfile -File Tools/Test-WastesSpaceValidator.ps1
pwsh -NoProfile -File Tools/Test-WastesSpaceLive.ps1 -LogPath Art/Validation/WastesSpaceHandoff-2026-09-06/telemetry-1920x1080.log -Viewport 1920x1080
pwsh -NoProfile -File Tools/Test-WastesSpaceLiveValidator.ps1 -LogPath Art/Validation/WastesSpaceHandoff-2026-09-06/telemetry-1920x1080.log -Viewport 1920x1080
```

For fresh live proof, load only the named disposable world with the exact QA
build and use `Request-WastesCameraCheck.ps1 -Case` for the six cases. Wait for
the new acknowledgement; preserve full continuous-flight completion. A static
hold may be released after samples accrue, but a flight must reach both endpoints.

## Still open

User comfort/art review; real-biome/restoration transitions with this exact
candidate; visible cave handoff; other world sizes, gravity and multiplayer;
candidate-specific memory/performance. Raw texture data remains28.36MiB, not
total memory. There is no general-world asset promotion. The existing grove
checkpoint digest mismatch recurred on load; its guard performed no rebuild.
This unrelated vegetation failure remains open and was not reset to get a pass.

Broad `Background` gate remains RED: existing source-art dimensions fall short
of its production contracts and the old generic renderer still stretches its
last row. Mask/export, city-mask, Space, restoration-policy and HD-contract
stages pass; no production threshold was weakened for this focused change.

The additional Mid ruin is a separate rejected-export draft, not installed:
`../../Candidates/WastesMidRuin-v1/README.md`. Preserve the accepted Station.
