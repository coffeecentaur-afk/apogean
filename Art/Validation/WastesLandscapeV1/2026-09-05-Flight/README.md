# Flight and left-edge correction — 2026-09-05

Scope: unchanged Wastes V1 Far/Mid/Close PNGs, still QA-only. No tree/atlas/world-generation changes. Art remains pending. User specifically approved use/restarts of **gg → Apogee Native Visual V3**.

## Reproduced defect

The first diagnostic build preserved the old projection and vertical rates, adding only a pure math extraction and QA observation. Actual normal-gravity surface batch zoom was 1.3333334 with translation about (0.0052, 0.0052), while `ZoomMatrix` had centered translation (-426.6667, -228.1667) at 2560×1369. Inverting that centered translation shifted submitted layers right. As each repeat's leftmost copy left the enumeration, the next copy began inside the screen. The defect is not a texture-alpha or outgoing-opacity failure.

`Red-diagonal-1369.jpg` is an original Windows game-window capture showing the hard left cut. `Red-telemetry.log` records 19,321 failed layer checks of 59,592 and a maximum 425.67px uncovered left edge, zero right gap. The actual camera crossed 128,640 world pixels and 3.455 / 8.794 / 18.844 Far/Mid/Close periods. The original vertical equation also moved the nearest ledge 360px for a 1200px ascent (352.8px at one clamp boundary).

## Fix contract

- Compensate the surface batch's scale only; do not subtract its already-removed centered zoom translation. Preserve Terraria's gravity effects and style alpha ownership. No SpriteBatch restart.
- Remove the logical camera offset before sampling the fixed `Main.worldSurface - 50` datum for vertical placement. This is a global generated-world reference, not locally sampled terrain or a camera-history filter.
- Close's nominal soil datum (source row488) projects to48 world pixels above `worldSurface - 50`, using gameplay zoom and full vertical world-camera displacement. No screen clamp, no below-ground opacity threshold. This global QA datum is not the actual grove floor or a production regional terrain profile.
- Far/Mid retain .012/.03 vertical response. Normalized altitude spans the fixed ground datum toward `worldSurface * 16 * .35`; this is a presentation reference, not a claim about every engine space predicate. Mid remains fully present through the first third, then smoothsteps to zero at70% ascent. Close leaves the viewport geometrically. Far owns bottom coverage at high altitude.
- The finite PNGs receive native-sized512px lower-strata continuations, alternately reflected vertically so sampled edges meet. This is a **QA coverage guard, not approved final terrain art**. It avoids a stretched last row and extra texture memory, but visible repetition/symmetry must be replaced by authored lower coverage. The production art gate stays closed.
- Permanent QA probe projects the submitted first/last rectangles through `Main.CurrentFrameFlags.Hacks.CurrentBackgroundMatrixForCreditsRoll`; active only during named SP camera fixtures. It measures geometry coverage, not texture-alpha joins or artistic acceptance.
- `diagonal-left/right` continuously traverse the world while ascending4200px from ground and descending to ground. They do not directly edit tiles. Mountains/ocean endpoints can occlude art; the retained leftward screenshots demonstrate that limitation and must not count as full visual coverage.
- Camera-only requests no longer invalidate the saved vegetation fixture checkpoint.

## Reproduction

Run only in the named, unpaused disposable SP world. For reliable startup, the installed client supports `-skipselect "gg:Apogee Native Visual V3"`. **Verify that exact named world exists first and confirm `Loading World` in client.log before requesting anything.** The native switch falls back to the first world if its name is wrong; it is not itself a safety guard.

```powershell
pwsh -File Tools/Request-WastesCameraCheck.ps1 -Case diagonal-left
# Let the 1800-tick sweep and 600-tick endpoint hold finish.
pwsh -File Tools/Request-WastesCameraCheck.ps1 -Case diagonal-right
# After expiry, inspect screenshots and retain the focused log lines.
pwsh -File Tools/Test-WastesProjectionLive.ps1 -LogPath <retained-telemetry.log> -Viewport 2560x1440
pwsh -File Tools/Test-WastesCameraProjection.ps1
```

The retained red log must fail `Test-WastesProjectionLive -Viewport 2560x1369`. The production-math test passes92 checks. `Test-WastesGroundLock.ps1` first failed on the reduced-motion helper (72 instead of1200px), then passed the world-locked helper plus72 viewport/zoom/altitude combinations. Altitude policy passes3003 sampled steps. Validator mutation tests pass7 ground-lock and9 altitude controls. These test anchor, staging and submitted coverage, **not the native underground handoff**. Range-policy14, restoration-policy25 and spray-validator8 checks passed in the preceding seam pass. The current staged renderer compiled with zero warnings/errors. The unchanged-asset export rerun through Windows PowerShell was blocked by its execution policy; do not count it as a fresh pass. Prior static asset evidence remains historical.

Use `Test-WastesGroundLockLive.ps1` and `Test-WastesAltitudeLive.ps1` with one retained viewport/session log in addition to the projection validator. The altitude validator requires ground, wings, mid-altitude, high-altitude and below-ground holds. It rejects missing/invalid observations, early Mid fade, missing Far stage, a following Close layer, and incorrect shallow-descent displacement.

## Capture provenance and interpretation

- `Red-*`: original projection defect.
- `Green-*`: intermediate scale-only/reduced-motion build. These are superseded, not the final motion candidate.
- `Before-ground-lock-*` and `Locked-*`: intermediate ground-lock build before the user rejected its32px descent fade. Historical only.
- `Staged-*`: latest ground-locked, altitude-staged, no-early-descent-fade build. Original game-window captures, no retouching or upscaling. PNG art is unchanged.

Shallow descent is visibly retained in `Staged-below-ground-*`. `Staged-underground-1080.jpg` is terrain-obscured: the surface draw path still submits geometry behind native terrain, so this is **not proof of a correct cave transition**. A visible, wall-free crossing remains necessary before promotion. The layered soil is still too repetitive/heavy; mechanical passes do not overturn the user's art critique.

### Completed latest-build measurements

| Viewport | Left/right layer checks | Geometry failures | Max ground-anchor error | Far periods each direction |
|---|---|---|---|---|
|1920x1080|59,544 /59,955|0 /0|0.00px|3.472|
|2560x1440|59,496 /60,057|0 /0|0.67px|3.455|

Both logs also pass the five-hold altitude validator and the ground/wings/diagonal engine-matrix validator. At1080p a400px descent moves the nominal soil from546 to146px. At1440p gameplay zoom is4/3, so the corresponding screen displacement is about533px. Mid opacity near one-third ascent is0.998/0.997, reaching0 at the80%-ascent hold. No exposed rectangle edges were measured. Opaque-world occlusion and authored alpha-edge quality remain outside those numerical claims.

The1369px windowed viewport has retained original-defect/intermediate evidence and pure-math coverage, **not a fresh latest-build live pass**. The final QA game was saved and closed, and original2560x1369 windowed/nonborderless settings were restored after the1440p run.

Engine reference: installed `Main.Draw` removes the centered background translation before `DrawBG`, then restores screen dimensions/position. Gameplay zoom remains separate. Public [SpriteViewMatrix API](https://docs.tmodloader.net/docs/stable/class_sprite_view_matrix.html) exposes the separate zoom/transform matrices. No engine source/assets are redistributed here.

## Safety record

Backup before testing: `%TEMP%/ApogeanFlightBackup-37ea85b0e7464c41b31a36cf36a04803` contains QA world/modworld, gg player/modplayer and config. After switching display mode, UI menu navigation accidentally opened `aga`. No QA fixture was requested there and no terrain was edited. The newly launched client was terminated before saving when UI close input failed. Both `aga.wld` and `aga.twld` retained their original SHA256 and September 3 write times; no rollback was required. Subsequent runs use verified explicit named startup. Do not claim this session never opened a regular world.

At18:37 the saved grove matched checkpoint87B951FE... after reload. At18:42, after the camera-test session/save, it instead reportedEB1D7019...; the safeguard refused to rebuild and left the QA world open. Cause is unresolved: camera code has no tile writes, but live-world simulation/other interactions and save/reload effects have not been differentiated. Do not silently reset the checkpoint, claim byte-identical vegetation preservation for the full run, or mislabel this caught QA diagnostic as an engine crash. Retain and compare per-cell evidence before the next vegetation test.

## Remaining art / validation gates

Latest user feedback supersedes the continuous Mid soil strip: use modular hill/ruin groups, transparent through-gaps, occasional joined groups and quiet sections between major landmarks. Far must remain visible through those gaps. Do not reuse `DrawLowerStrata` on modular Mid; it would defeat the requested transparency. Keep the current test evidence as a mechanical checkpoint, not as acceptance of this rejected Mid composition. Preview original candidates before installation. Ground/flight comfort needs user review; local terrain sampling, gravity inversion, multiplayer, full lighting/adjacent-biome matrix, visible cave handoff and measured texture residency remain open. No general-world promotion.

The subsequent underside clarification requires deeper cliffs, slopes and compatible lower continuation pieces beneath occupied chunks, while preserving open valleys between groups. A fixed world/region anchor is preferred, but it cannot hide unfinished art: inspect full assemblies at ground, first-third ascent, high flight and diagonal joins. Existing Mid motion/coverage is historical mechanical evidence, not implementation of that new contract. The [upper-silhouette concept](../../../Candidates/WastesMidgroundModules/2026-09-05/README.md) is review-only and explicitly fails runtime alpha/underside requirements.
