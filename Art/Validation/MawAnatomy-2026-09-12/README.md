# September 12 bounded autonomous QA

Window and authority: `MAW_AUTONOMOUS_PASS_2026-09-12.md` at repository root.
No ordinary worlds are in scope. Previous September 11 exhibits remain preserved.

## Initial reproduction — unchanged CA03 package

- tML 2026.7.3.0 / `666f699`, native D3D11 / RTX3090; Ryzen 9 7950X,
  33,442,623,488 bytes usable physical RAM; windowed 2560-wide capture.
- `gg` / `Apogee Native Visual V3`, single-player. CheatSheet and Apogean enabled.
  Resource packs in startup log: Halo Covenant Invasion, HD Scenery, Dark Fantasy
  Health/Mana, Better NPCs, Beholder, NONN's Buffs, Calamity Texture Pack, Stormdark UI.
  These are existing conditions, not standalone/modpack compatibility proof.
- At 12:54:23 Central, `anatomy-vines-reload` failed again at local44,8:
  root3 x7440/y180 still has only depths1–3. Six other roots retain six sections.
  At 12:54:42, `anatomy-vines-audit` reported no liquids/actuation and intact root.
- Current-only fingerprint `C0A05A2BED91ABF33B3323A83E6060226480E124EF6AEF0E45E2BE0A648C2F8F`.
  This raw-field hash is not a portable saved-world identity: missing tiles reload
  with cleared residual metadata. Neither this nor the older hash replaces the
  original six-section-per-root contract. No repair or regrowth was performed.
- The separate pre-existing grove mismatch also recurred at world entry; its
  safety check refused rebuilding. No new crash is implied by either guarded failure.
- Static MawAnatomy profile passed, including compiler negative controls and
  hanging-fiber/amber contracts. This is not native/art acceptance.

## Performance protocol

`Tools/Request-QAPerformance.ps1 -Case snapshot` records already-loaded texture
objects without Request/Wait/GetData, plus process/GC memory. It does not measure
driver residency or include native paint targets. Color texel bytes are explicitly
logical payload, not measured VRAM. Runtime object sharing is counted by reference.

`-Case start` warms for two seconds then records 30 seconds. Do not capture,
switch focus, run another QA probe, or change conditions during sampling.
It records world-update hook durations and successive PostDrawTiles intervals,
not GPU/presentation timing or complete engine CPU time. Pause/inactivity, too few
callbacks, overflow and manual stopping make a sample unusable. Raw intervals and
percentiles are exported under the game's `Captures/ApogeanPerformance` folder.
The recorder does not force GC, move the player, edit tiles, or change settings.

Run repeated unchanged-scene samples before claiming an improvement. Keep startup,
ordinary drawing, expensive diagnostic readback and dense stress scenes separate.
Package/source pins and actual new measurements will be appended after native runs.

### Instrumented baseline A (no texture sharing change)

Installed package `26ABEA384B0B2B648F20E4E4165030BE4B447570043EAF3EABB142C43FD96EEB`;
isolated build mirror `92ff8ad33d604536a05b3512e97447f4`, rib v2, unchanged466 pins.
Compile/install passed without warnings. This adds passive recorder code, not art.

- First texture snapshot: 403 distinct loaded Apogean Texture2D objects,
  785,088,628 logical Color texel bytes (~748.72MiB), zero pending requests.
  Study/production soil walls share object258; grass walls share separate object246.
  The two source PNGs AND map binaries are identical. This is a measured object
  duplication opportunity, not yet measured process/driver savings.
- Wastes spawn sample: 1799 draw intervals, mean16.667ms, p9918.161ms,
  max35.759ms, one interval above33.333ms and none above50ms. World-update slice
  mean0.456ms, p990.545ms. No focus/pause/overflow flag. This is one stationary
  sample on this high-end PC, not stress/low-end acceptance.
- Anatomy fixed-view sample: 1799 draw intervals, mean16.668ms, p9919.981ms,
  max32.766ms; update slice mean0.506ms. Same qualifications, not traversal proof.
- `process-26ab-startup.json` actually begins **after** mod-load completion:
  18:02:21–18:02:35UTC versus completed18:02:11UTC. Treat it as late-startup/menu
  settling, NOT full startup peak capture. Its15 records were written successfully,
  but the initial script's final summary failed on an ordered-dictionary projection.
  That summary was corrected and the3-sample menu rerun exited successfully.
- Windows process GPU counters are available. They are process-wide, not Apogean
  ownership. Keep adapter instances and provider labels; do not add committed,
  dedicated and shared as though independent measures of the same allocation.

### Request handoff correction

Native snapshot at13:03:53 initially encountered a sharing IOException, then the
next poll succeeded. The previous FileMode.CreateNew/FileShare.Read writer was
still open when File.ReadAllText tried to open it. The temporary-file control
reproduces that exact sharing-mode failure. `Publish-QARequest.ps1` now writes and
closes a unique staging file, then renames it in the same directory without
overwriting a pending request. Fiber, performance and fang/save senders use it.
Other legacy senders remain to be migrated; this is not a claim they all changed.
Tests cover complete readable publication, old-writer failure and pending refusal.

### Comparison build B — native identity and mechanism proof

Package `DD05792112E625C4B0E66E99260C8DB31A7C93F3C03A8B746BDD457E76F1CFBA`,
mirror `2b98677b22c045b5a068537be06cf56d`; clean build, all466 prior pins retained.
The first attempt (`42a7cf7...`) did not install: the scratch Solar probe asserted
nonexistent `Projectile.channel`. That assertion was removed; the existing player
channel guard remains. AI117 now uses `ProjAIStyleID.SolarEffect`.

Grass/soil study and production wall consumers now share the soil asset path.
Both PNGs and both frame maps must be byte-identical before the build wrapper runs.
Expected native difference: one fewer distinct loaded texture and13,908,992 fewer
logical Color texel bytes (13.264648MiB). This is an expectation, NOT a measured
RAM/VRAM improvement. New PNG/map divergence controls pass through the real CLI.

Recorder review corrected GPU WQL PID wildcard matching and an export path that
could announce completion after failure. Output is now closed in a unique staging
file then renamed; an export failure logs failure, not COMPLETE. Native screenshot
capture invalidates a timing sample. No current proof of dense traversal, worldgen,
long sessions, repeated loading, multiplayer or lower-end hardware.

Baseline second anatomy sample: mean16.6668ms, p9919.9917ms, max21.1515ms;
update mean0.5131ms, p992.2087ms. Both baseline anatomy samples have the same
camera, screen size and zoom; different counts of ambient actors are not pinned.
`anatomy-26ab-native.png` is an unedited native capture. Its blue capture backdrop
differs from the live space view, and two harpies are present: no claim that this
image reproduces normal biome routing or explains the historical strand cut.

At18:16UTC, the game entered `aga` while UI input was interrupted. All game inputs
and QA requests stopped. An async question asks before saving and switching to V3.
`process-dd05-menu.json` contains only three menu process samples; it is not a
world/scene comparison. No QA test was run in `aga`.

Next after QA access: snapshot (expected402 textures/771,179,636 Color bytes),
`anatomy-vines-solar` in its separate guarded scratch envelope, preserve/re-audit
the old RED strand, native anatomy regression and same-view repeated timings.
Record actual outcomes here before advancing status. A Solar mechanism proof
cannot retrospectively attribute the original disappearance.

Follow-up18:30–18:32UTC: user approved saving/switching, then manually saved aga.
The log records validated/modded save at13:23:07 Central and explicit V3/gg entry
at13:26:21–22. No QA commands were run in aga. This B process visited an extra
world before the measurement; do not treat its process-memory values as a matched
fresh-launch A/B comparison.

- `snapshot-20260912-183032-501-5dcd96.json` records **402** distinct textures,
  **771,179,636** logical Color bytes, zero pending. All four soil/grass study
  and production wall consumers resolve to the SAME texture reference257.
  Compared with A this removes exactly one duplicate allocation /13,908,992
  logical Color bytes. No measured driver-residency/RAM reduction is claimed.
- `anatomy-vines-solar` completes after114 checks using actual native
  SolarCounter creation/Kill twice. Outside-envelope control keeps six segments;
  a lower burst removes depths4–6 and retains1–3. Scratch state is restored and
  the original damaged-strand fingerprint is unchanged. Mechanism proof only:
  original event, capture trigger and multiplayer remain unknown.
- `anatomy-vines-audit` still records39 segments, with the same missing suffix
  at root3. No repair, regrowth or rebaseline. `anatomy-test` repeats308 native
  physics assertions and validates all7600 saved cells before and after.

The former instructions above remain the pre-run record, not pending verdicts
for those now-executed checks.

Two B anatomy samples, same camera/screen/zoom as A, complete usable with1799
draw intervals each: means16.6673/16.6674ms, p9918.7636/18.8613ms,
max19.7657/19.9633ms, zero intervals above33.333 or50ms. Update means
0.4319/0.4717ms; p990.5388/1.9071ms. No stationary cadence regression observed;
this is not a causal speedup claim or dense/moving-scene acceptance. Ambient
actors remain uncontrolled. Unedited `anatomy-dd05-native.png` was inspected:
wall material continuity remains, the paler rib still reads as a provisional
banded shaft, and the blue capture backdrop differs from ordinary gameplay.
The scene was saved/validated at13:34:58 Central before shutdown.

### Comparison build C — cached render lookups

Package `78F0B04A2CD63F86A48D24CBB9325EC40119FD1E63D7E47E5F32FB59832C548E`,
mirror `9608e9713b594d4f998ab953b9e33aa4`. Clean build; all466 art/map pins unchanged.
The existing24 registered study instances are cached per content load instead of
constructing lookup names for each rendered/lit cell; cleared on load/unload.
`qa-perf-allocations` runs actual registered draw/light hooks in a separate
read-only synchronous measurement, with direct-map/identity controls and explicit
missing-case reporting.

At13:39:31 Central, all six measured native hook cases report0 current-thread
allocated bytes for4096 calls each after4096 warmup calls, unchanged frame/RGB
outputs, no skipped cases, and24 cached/registered identity matches before/after.
The deliberate former-resolver controls allocate259,416 tile /292,184 wall bytes;
these are resolver controls, NOT an old-native-hook A/B measurement. C snapshot
still reports402 textures/771,179,636 logical Color bytes. No FPS/GPU or retained
memory claim follows. Actual in-process mod unload/reload validation is pending;
the first C load is not that lifecycle proof.

C repeated anatomy cadence samples complete usable with1799 intervals each:
means16.6675/16.6658ms, p9918.7575/18.7874ms, max19.0819/21.8541ms, neither
sample has a33.333/50ms exceedance. Update means0.4568/0.4824ms. Same fixed
camera/display/zoom; no causal speedup or stress conclusion. The full six-request
native material suite passes again, including449 properties, real falling-sand
ticks with cleanup, seams and mutation controls. `native-78f0-materials.json`
contains124 verbatim scoped records, including the allocation result; the exporter
now retains performance records and failures too. The natural/playable sender
uses the same tested atomic publishing path as fiber/performance/fang senders.

Next: actual content-reload cache identity, then the separate provisional shallow
passage. No terrain art, old fixture layout, worldgen or runtime dependency was
promoted by this checkpoint.
