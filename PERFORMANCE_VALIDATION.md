# Performance acceptance — Apogean

User requirement, September12: Terraria/tModLoader must remain usable as terrain,
structures, backgrounds and later content grow. Memory, dimensions, load time and
stutters are implementation constraints, not an excuse to silently reduce art.

This is a test contract, not a minimum-spec announcement. Current measurements are
on a Ryzen9 7950X / RTX3090 / approximately31.15GiB usable-RAM desktop. They cannot
establish low-end or large-modpack support. The current native process is64-bit;
do not diagnose a supposed4GiB process ceiling without evidence.

## Distinct quantities

- PNG/package size: storage/download cost. Record separately from decompressed
  dimensions and texture format. Recompressing unchanged pixels does not prove a
  runtime-memory improvement.
- Repository textures: loaded distinct Texture2D references and their logical
  texel payload. Do not double-count aliases in Tile/Wall/Background arrays.
- Native paint/render targets: separate lifetime and type/style/color keys. Base
  texture sharing does not imply paint targets also share.
- Process private bytes, working set, managed live estimate, GC committed/heap/
  fragmentation and Windows process GPU counters: different, overlapping scopes.
  Never add them all into one invented memory number. Missing counters are unknown.
- Draw-callback cadence: detects gaps between world draws, not GPU/presentation
  time. World-update hooks measure a slice, not complete engine CPU time.
- World save size and generation time: separate from live frame performance.

## Available tooling

Run `Tools/Invoke-ApogeanContentGate.ps1 -Profile Performance` first. Current tools:

- `Request-QAPerformance.ps1 -Case snapshot`: passive existing-asset inventory.
- `Request-QAPerformance.ps1 -Case start`:2s warmup plus30s bounded sample.
- `Request-QAPerformance.ps1 -Case stop`: aborts, does not pass an incomplete run.
- `Request-QAPerformance.ps1 -Case allocations`: separate synchronous real-hook
  allocation probe, never concurrent with passive recording. Synthetic valid
  draw inputs and existing saved amber emitters; no world writes or forced GC.
  Logs missing cases explicitly. This is not FPS or GPU timing.
- `Measure-QAProcessMemory.ps1`: bounded read-only process/Windows GPU counters;
  explicit observed tModLoader PID, unique output, phase label and sample count.

Native requests are restricted to packed-QA `gg` / `Apogee Native Visual V3` /
single-player. Requests publish only after their file is completely written.
Do not widen this allowlist to get past an ordinary world being open.

Timing runs must not include capture, readback probes, compilation, another QA
command, focus/pause changes or settings changes. Keep screenshots and diagnostics
in separate phases. Preserve invalid runs and their reason, not just successful
samples. An available-but-inactive GPU row is not a measurement of Apogean alone.

## Ordered workload matrix

### Bounded shallow render comparison (September12 provisional)

`shallow-static` and `shallow-sweep` are explicit recorder cases, still restricted
to packed gg/V3/single-player. Select the existing shallow `rib2` held view first.
Both use64 fixed neutral inspection lights across the112x104 fixture. The sweep
adds a smooth10-second diagonal camera cycle (+/-160px horizontal, +/-320px
vertical), without moving the held player. These are synthetic renderer/lighting
workloads, NOT player traversal, natural amber illumination or a worst-case world.

Run static/sweep/sweep/static, with no screenshots, focus changes or other work
during each2s warmup+30s interval. Between runs inspect fresh COMPLETE and exports.
Schema2 records actual camera extent and rejects missing sweep coverage or a
drifting static control; it compares fixture fingerprints before/after. Context
loss, manual stop, pauses, captures and insufficient samples remain invalid.
Keep invalid results. The lights and offsets stop with the bounded recorder.
Screenshot verification happens separately from timing. Record NPC/projectile
counts, held player, zoom, settings and package so unlike scenes are not compared.
The update slice includes diagnostic-light overhead in both arms. This comparison
does not replace dense production growth, generation, multiplayer or soak tests.

| Workload | Needed evidence | Current coverage |
|---|---|---|
| Load/menu | Startup peak from launch, settled snapshots, package/settings pins | Late-startup/menu samples only; no complete launch peak |
| Ordinary terrain drawing | Fixed camera, repeated equal settings, raw cadence/update samples | Baseline Wastes and two anatomy stationary samples |
| Dense Maw drawing | Actual loaded mixed materials/walls, fiber, amber and teeth; normal and dense variants | O shallow fixed64-light scene measured; not scaled density/growth stress |
| Movement | Reproducible horizontal/diagonal traversal, same route/speed/zoom; no scripted camera changes confused with frame spikes | O synthetic camera sweep measured; actual moving gameplay pending |
| Paint/coatings | First new key, repeated same key, multiple keys, return/reload; native target census | Pending |
| Growth/conversion | Fixed number of actual native operations and touched cells; clustered worst case and idle empty case | Pending |
| Generation/save | Smallest supported, normal and cramped legal worlds; fixed seed/version, attempt bounds, operation count, time and peak memory | Pending; generator remains opt-in |
| Lifetime/soak | Repeated enter/leave of the same QA world and regions; compare settled retained allocations across cycles | Pending |
| Multiplayer/modpacks | Server tick cost, independent client memory, representative mod combination | Pending |

Do not simulate a GPU/RAM limit by allocating until the PC crashes. Grow stress
workloads in bounded stages, stop on resource pressure and preserve evidence.
If a workload approaches available system memory, stop increasing density, find
the retained allocation/duplication, and compare a measured fix. A separate test
process is not authority to close unrelated apps or change system settings.

## Comparison and release gate

Use the same candidate art, world, scene, camera, resolution, zoom, packs and mods.
Pin source/build hashes. First establish noise with repeated samples; for a memory
claim use at least three independent process runs per arm, alternating order where
practical. Keep count/object-identity proof separate from noisy process deltas.

Record median/range, p95/p99/max and33.333/50ms exceedance counts, not only mean FPS.
Do not set a hardware minimum from this single PC. Treat new sustained memory
growth or repeated stutters as open regressions until explained; a one-time GC or
texture upload needs an explicit phase, not an automatic leak diagnosis.

A fix must retain gameplay, rendering and save compatibility and pass a real-path
regression with a failing control where practical. Cached content references must
be rebuilt and cleared across mod lifecycle; never manually dispose shared engine
textures. Whole-world scans do not belong in per-frame draw hooks. Growth and
conversion need finite work budgets and must retain unfinished work correctly.

Current dated evidence and caveats: `Art/Validation/MawAnatomy-2026-09-12/README.md`.
O synthetic scene extension: `Art/Validation/MawPerformance-2026-09-12/README.md`.
Loading/paint/atlas mechanisms: `RESEARCH_MAW_TEXTURE_RESIDENCY_2026-09-12.md`.
No dense-scene, lifetime, generation, multiplayer or final-performance approval is
implied by the initial instrumentation checkpoint.
