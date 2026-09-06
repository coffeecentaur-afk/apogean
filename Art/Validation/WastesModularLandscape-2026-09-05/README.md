# Modular Wastes landscape — scoped QA checkpoint

**Latest user review: revisions required.** The user identified pale foreground cutout edges, detached gas-station tree limbs, excessive Close repetition and a soil lip that does not match actual ground/player-head height. The successful geometry runs below do not resolve or overrule those observations.

The user approved continuing this four-design set into foreground-depth completion and disposable-world testing. This is **not final art or ordinary-world promotion**. Source art lives in `Art/Candidates/WastesMidgroundModules/2026-09-05/Deep-v1/`.

## What changed

- Three Mid groups (highway, quiet hill, station) replace the uninterrupted Mid wall. Widths576/391/488, height1408. Layout offsets0/936/1687, period2655; real valleys remain transparent down through the composition.
- One longer Close bank (1448×1915), period2268 and620px phase offset, replaces the uninterrupted Close wall. Its nominal soil socket is row330; the same48-world-pixel ground offset and fully world-locked vertical movement remain.
- The minimum-error upper/lower cut preserves raw source pixels. The upper830 rows remain byte-equivalent by visible color/alpha. The newly generated lower source actually measures1449×1085; cropping one rightmost column, not rescaling, produces829 net additional rows. The full source is retained unchanged.
- Mid and Close have **no lower-row fill, reflection, stretch, or tiled continuation**. The existing Far reference still has its historical repeated lower coverage guard and heavy bands. It remains an explicit art blocker.
- Source dimensions render at native pixel scale through the existing corrected surface batch; no SpriteBatch restart or camera zoom mutation. Fixed layout persists across movement/reload. Regional terrain anchoring is still pending; the QA datum remains `worldSurface - 50`.
- Only the QA-world/Forest-lab route selects this set. Ordinary worlds retain their existing renderer. Four runtime candidate PNGs are hash-checked against reviewed exports; only Far plus those four textures are loaded by this renderer (28.39MiB raw RGBA, not a measured GPU-residency/performance pass).

## Static/build evidence

- Clean isolated tModLoader package build: zero warnings/errors.
- Mid:2,162,688 pixels, exact crop/socket reconstruction, hard alpha, open valleys; six existing negative cases retained.
- Close:2,772,920 pixels, hard alpha, preserved upper830 rows and fresh source pixels below the overlap, exact top/continuation reconstruction. Four bad-asset real CLI controls reject soft alpha, changed upper art, shifted socket and changed lower art.
- Modular layout:56 camera/anchor/phase cases across1080p,1369p and1440p, plus short-depth rejection. Actual CLI negative cases reject old short depth, phase jump and shifted soil socket.
- Live-validator self-test accepts its control and rejects six deliberate bad traces through its actual CLI. This is validator evidence, not an in-game result.
- Generic background static gate checks Far + joined Mid + Close at the actual candidate dimensions (minimum1448px, not a false1920px per-module claim),28.83MiB including the unshipped joined Mid's empty gutters. Cropped runtime Mid reduces that to28.39MiB.
-24 offline studies cover ground, first-third ascent, shallow400px descent and80% ascent with fade deliberately disabled for Mid underside inspection at three sizes. They are not screenshots. A1600px underground descent remains outside this finite-depth proof.

## Live evidence

Original Windows game-window JPGs are unretouched. Windowed captures include the title bar/window border; the 1440p run is fullscreen. Viewport dimensions come from client telemetry, not JPEG height. Camera fixtures do not write terrain. Forced Forest fixture routing does not prove ordinary biome selection. Committed `client-*.log` files are allowlisted QA-only telemetry exports, not full startup/account/system logs; originals remain in the local pre-test archive.

- `Native-ground-1369.jpg`, `Native-shallow-1369.jpg`: actual new Mid/Close composition and shallow-descent extension, with Far visible through valleys.
- `Native-mid-altitude-1369.jpg`: Close has left view and Mid remains. The capture includes a weather transition; do not label it a settled clear-noon image.
- `Native-night-1369.jpg`, `Native-eclipse-1369.jpg`: same geometry under the existing night/eclipse tint. These are bounded samples, not approval of every fade/weather state.
- Both1369p full-world diagonal sweeps pass:0 module failures, maximum projected pixel error0.01px, ground-anchor error0.83px; Far travel3.455 periods each direction. The older generic sweep line still prints Mid/Close repeats using its2048px V1 denominator; those two values are obsolete for modular layout and are not used as proof.
- `Native-diagonal-right-start-1369.jpg` / `Native-diagonal-right-end-1369.jpg` are timed samples during the sweep (the latter is not the exact completed endpoint); terrain can obscure art. Use completed result telemetry for traversal range, not screenshot naming.
- `Native-ground-1080.jpg` and the retaken `Native-shallow-1080.jpg` are valid active-fixture captures. `Expired-shallow-1080.jpg` is explicitly excluded: the short hold expired before capture.
- `Unconsumed-shallow-1440.jpg` is excluded: the fullscreen game was unfocused and had not consumed the request. Its valid replacement is `Native-shallow-1440.jpg`, captured after the log confirmed the below-ground case and its -111.33px Close top. `Post-sweep-return-1440.jpg` shows the player after automatic release, not diagonal-flight proof.

### Completed geometry runs

| Actual viewport | Ground / shallow | Left sweep checks | Right sweep checks | Worst anchor error | Far periods / direction |
| --- | --- | --- | --- | --- | --- |
| 1920×1080 | Pass | 64,801 | 60,310 | 0.00px | 3.472 |
| 2560×1369 | Pass | 85,592 | 81,261 | 0.83px | 3.455 |
| 2560×1440 | Pass | 86,123 | 81,444 | 0.67px | 3.455 |

Sweep checks above count submitted module positions/depth; separate frame-count comparisons also pass. All listed module failures are zero. Maximum submitted-pixel error is 0.00px at 1080p and 0.01px at 1369p/1440p. These are geometry checks, not final art acceptance. `Native-ground-final-1440.jpg` is the final brighter ground capture, after confirming the restarted ground hold in telemetry.

## Safety and remaining gates

Only **gg → Apogee Native Visual V3** was explicitly loaded. Pre-test backup: `C:/Users/max_h/AppData/Local/Temp/ApogeanModularBackup-f19e674a5480429eb272f6e8ba287d52` contains QA world/modworld, gg player/modplayer and original config. Initial settings were2560×1369 windowed, nonborderless.

All three QA clients were saved, returned to the menu and exited. The original 2560×1369 windowed/nonborderless settings were restored and read back. No regular world was opened during this checkpoint.

The prior grove-checkpoint warning recurred unchanged (`87B951FE…` expected, `EB1D7019…` actual). Its safeguard refused to rebuild. This is **not** a new engine crash or a tree fix/persistence pass; no checkpoint was reset.

Do not promote beyond contracted while the far-rock composition, art review, regional anchors, visible native cave handoff, complete lighting/adjacent-biome/restoration matrix, inverted gravity, multiplayer and measured residency/hitches remain open. The finite Close extension fixes the requested shallow camera cases; it does not prove indefinite underground coverage. No Maw/faction art was added.

## Follow-up diagnosis from the user's live inspection

`pwsh -NoProfile -File Tools/Inspect-WastesCutoutFeedback.ps1 -RequireClean` currently fails intentionally against the actual runtime PNGs. The selected gas-station tree regions have five left and seven right disconnected opaque components of at least three pixels (8-neighbor connectivity). The foreground probe finds 389 pale upper-boundary candidates, including branch-tip pixels near (436,210); this is a targeted visual-review warning, not a claim that every bright material pixel is wrong. Existing hard-alpha and hash tests cannot certify cutout quality.

The Mid master uses a magenta matte and the exporter removes pixels whenever R-G and B-G both exceed 24. For example, source Station-local (59,377) is RGB102/41/68 but exports as alpha0; nearby dark, matte-contaminated branch-edge pixels are also removed. Thus the key removes some pixels in the thin branch region, and disconnected branch fragments already exist in the exported texture. Preserve a clean, connected silhouette and remove matte contamination without indiscriminate erosion. The exact balance of pre-existing source gaps versus key-induced breaks still needs a controlled replacement comparison.

Foreground extraction retains fully opaque grayish edge pixels after removing the white matte. Their opacity does not make them clean bark edges. Inspect the affected upper artwork on dark, sky-blue and neutral backing before re-export; don't erase every light pixel or thin every branch to hide the rim. Native sampler effects may amplify it but cannot explain the pre-existing disconnected PNG clusters.

`Tools/Inspect-WastesGroundAnchor.ps1 -RequireLocalGround` exercises the real modular projection with two explicit standing-coordinate fixtures. It distinguishes correct 48px placement at the assumed global datum from wrong placement on a surface 320px lower. These are synthetic terrain elevations, not a false claim of live terrain measurement. The next renderer change needs a stable per-region terrain datum, with a dedicated fixed QA floor; sample/cache ground independently of flight, never tie the lip to the moving player's head. Standing head height defines the offset; it is not a screen clamp.

Only one Close artwork is selected for every 2268px period. A physical camera shift of 7560 world pixels returns exactly the same bank at the 0.30 horizontal rate. Repetition therefore needs genuinely different long-bank silhouettes/landmark combinations and quiet variants; flipping this bank is not sufficient. Preview the replacement sequence before updating the in-game art. No further asset generation was performed after this feedback.
