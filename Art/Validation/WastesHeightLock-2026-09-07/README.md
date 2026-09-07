# Wastes stable sections and height caps — 2026-09-07

## Contract and scope

Latest user instructions supersede the previous altitude-fade policy. Close
sections keep a fixed height in the saved terrain snapshot. Horizontal parallax
must not resample ground at the moving camera and shift all visible sections.
Mid locks at one-third of ground-to-upper-sky ascent; Far at 70%. These initial
positions preserve the previous staging order, but are now **positional caps**,
not opacity thresholds. Above each cap, full camera displacement takes the
opaque layer down and out of view. Descending retraces the same calculation.
Biome/style fades and future global reclamation fades are independent.

Existing artwork is unchanged. Height caps are a camera-center contract at a
fixed viewport/zoom, not a guarantee of identical composition after resizing.
The saved regional profile still applies only to the named QA path; ordinary
world generation/persistence and multiplayer promotion are not implemented here.

## Cause and regression evidence

The old renderer sampled `TryGroundAt(camera center X)` every frame. Even with
an immutable terrain snapshot, crossing a terrain slope moved every Close
section vertically. A reduced repro using that real lookup and synthetic rows
599/639/599 measured a 640px ground-anchor change. No GPU reproduction is claimed
for that reduced control. Each absolute parallax cell now owns a sample location
derived from its midpoint, period and phase offset, independent of camera X.

`Test-WastesHeightLock.ps1` first failed the old height factor (layer0 at upper
sky returned0). It now passes3000 production-policy assertions across five world
surface heights, five viewports and three zooms. `Test-WastesFixedSections.ps1`
passes5804 assertions for sideways travel, seams, return trips, ascent and copied
saved-profile reload, plus a real-renderer wiring guard. Six CLI mutations reject
missing caps, screen following, wrong zoom, returned opacity fading, wrong
anchor locations and a bypassed renderer call. Existing92 projection,56 modular
layout and72 ground-lock combinations also pass. These are arithmetic tests.

Historical Space/altitude-opacity validators remain labeled for old-revision/log
replay only. The active Background gate uses the new height/section tests.

## Native QA scope

The user authorizes only **Apogee Native Visual V3**. No regular worlds, terrain
fixture reset, grove-checkpoint reset or new PNGs. Camera requests are bounded
and restore player/time/weather on release. Required cases are `ground-pan-left`,
`ground-pan-right`, `space-ascent`, and `space-descent`; then release.
`Test-WastesHeightLive.ps1` checks actual submitted opacity, independent cap
positions, same-section anchors, projected ground placement and sweep coverage.

The known grove save/reload mismatch is unrelated and remains unresolved:
expected87B951FE… versus actualEB1D7019…. Do not rebuild or silently accept it.

Private pre-test backup: `C:/Users/max_h/AppData/Local/Temp/ApogeanHeightQA-a289687187ec4a84b2351aefcb234c29`.
Previous package SHA256: `9C15F8C50ADE16D7517CB14A0BBFC63BA7595D084E6102C50BE8189671A17F29`.
Build mirror: `C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/43d4686bad454bac8bc49606eda4e630/apogean`.
Compilation succeeded; two packaging attempts encountered the open-client lock.
After the saved client closed, the same mirror built and packaged with0 warnings
and0 errors. Same six reviewed candidate override directories as the September7
ForestRestoration checkpoint; no accepted asset changed.

Installed QA package SHA256:
`E8E927C32809D7625CE1E3F6A4BFDAFDB963E7B5B4625087201DCA8C8C579D61`.

## Native outcome: motion passes; full fixture rejected on depth

All four bounded cases completed at **2560x1369**, game zoom1.3333334.
`Native-2560x1369.log` contains1472 allowlisted diagnostic records, not the full
client log. Repeated frame checks are not independent viewpoints or FPS evidence.

| Case | Same-cell anchor checks / sections / failures | Height-position failures | Modular failures |
| --- | --- | --- | --- |
| ground-pan-right | 65300 /21 /0 | 0 | 15053 |
| space-ascent | 44943 /3 /0 | 0 | 0 |
| space-descent | 44970 /3 /0 | 0 | 0 |
| ground-pan-left | 66829 /21 /0 | 0 | 10342 |

Submitted altitude-alpha checks pass in all four cases. Both horizontal sweeps
travel128640px, spanning17.016 Close periods. Ground projection has at most0.84px
rounding error; matrix projection has at most0.01px error. The fixed-height
anchors are stable in both directions. Many level-pan views are occluded by
terrain: this is telemetry proof, not a claim of unobstructed visual inspection
of every section. Upward and downward geometry/coverage tests pass at this view.

The combined modular gate includes count, matrix and bottom-depth checks. It
correctly **fails the full run**. Existing1915px Close textures can end above the
screen bottom when a lower camera views a bank anchored on higher terrain.
Example: left-pan cell17 has saved ground9392 and diagnostic closeTop=-1077.50;
that diagnostic uses the legacy488 soil row. Actual modular top is-919.50 after
the158px row correction, so its bottom is995.50, short of1369 by373.50px.
This demonstrates insufficient depth; the combined count is not a per-cause
breakdown. Do not weaken the gate, re-anchor to the player, fade the edge away,
or stretch the last row to report a pass.

`Test-WastesHeightLive.ps1` with its default four cases exits1 on the left-pan
modular result. Selecting only `-Cases space-ascent,space-descent` in PowerShell
passes both flights; that is deliberately **not** a full-suite pass.
`Test-WastesHeightLiveMutations.ps1` passes its unchanged ascent control and
rejects four altered traces (opacity fade, failed position, missing exit,
missing result). Original telemetry is unchanged.

Native captures: `space-endpoint.png` shows land outside the viewport; the log
records Far/Mid still submitted at alpha255, with tops2780.438/5944.972.
`descent-return.png` shows the scenery returning. These are unedited native
window captures including the title bar, not matched-camera before/after pairs.

QA save-and-quit consumed at15:24:20 local. Client reported world validation and
modded-world save; the native main menu was visually verified afterward. Regular
worlds were not opened. The known grove digest mismatch was not bypassed.

## Next bounded work and remaining gates

Keep this family **rejected / liveFixture fail** until Close depth and the cave
handoff are solved and the complete four-case test passes. Preserve approved
building silhouettes; target only lower foundations/coverage. First isolate a
visible low-camera fixture and separate depth/count counters, then establish
the required depth before authoring extensions. Do not restart all background
art or jump to another biome. The existing production gate also remains red
for26 undersized candidate layers and a legacy stretched-row fallback; those
are separate from this motion fix. Other viewports, zoom changes, reversed
gravity, multiplayer and ordinary-world saved terrain profiles remain unproven.
