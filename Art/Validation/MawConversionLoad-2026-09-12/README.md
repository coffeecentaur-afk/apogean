# P — bounded native conversion load, September 12

Four native runs22:37:02–22:37:41UTC, ordered16/32/32/16. Packed QA gg /
Apogee Native Visual V3 / single-player, tML2026.7.3.0, Terraria1.4.4.9.
Same Ryzen97950X / RTX3090 /31.15GiB host as the O performance record;
Apogean+CheatSheet, existing resource packs unchanged. No screenshots,
performance recorder, settings changes or build ran concurrently with a probe.
This is synchronous conversion throughput, NOT frame timing or a spread scheduler.

Package SHA256:
`C2C0BCD97E9A221D762D0D423B0C9D27F593FEF4D58B16F5809452DBD746D0DB`
Build mirror `400adea2a1144d659a8c80d8706dec14/apogean`.
The packaged assembly/load are verified. The wrapper completion output was lost
at a context transition; do not invent its exit record. All466 previous PNG/map
pins were rechecked unchanged. Rechecking the finished mirror with the strict
pre-study manifest first rejected the seven deliberate anatomy additions; using
its existing AllowAdditionalAssets option verified the original pins. Those seven
are the builder's already documented separate study files, not missing textures.
The earlieraefcbc92 package was never run and is not a native result.

Each patch at1396,60 mixes four natural tile/wall families—dirt, stone, ice and
mud—with flat/all4 slopes/half-blocks, paint, two wires, actuator/actuation and
fullbright/invisibility coatings. Nine phases are three repetitions of source→
Maw→Wastes→vanilla. Actual ConvertAt and TileLoader/WallLoader hooks run in fixed
32-cell batches; every cell is checked after each batch. Constructed GrayBrick/
Woodwall and Kessler materials remain unchanged. Neither global spread nor any
production mapping was changed. Native placement, sand/liquid, trees and
multiplayer are not covered by this setup.

| Run ending UTC | Patch | Cell conversions | Assertions | Total ms | Sum of batch ms | Slowest32-cell batch ms |
|---|---:|---:|---:|---:|---:|---:|
|22:37:02|16×16|2304|5123|34.7467|16.1526|0.6748|
|22:37:23|32×32|9216|20483|63.2770|48.1795|0.7659|
|22:37:40|32×32|9216|20483|55.0817|42.1417|0.6413|
|22:37:41|16×16|2304|5123|22.8504|10.0047|0.4437|

Totals include setup/checks/restoration; batch times exclude those costs.
Per-phase thread allocations INCLUDE validation/string/report overhead and are
not native-conversion-only allocations. Warmup/JIT/logging and ordinary process
variation prevent an optimization claim from the later faster samples. A55–63ms
synchronous large probe is deliberately not a suitable per-frame production task.
Do not derive minimum hardware or world size from this PC.

Every run reports exact value-state restoration of its guarded envelope and
unchanged historical bounds, with matching before/after semantic hashes. No
saved exhibit was reconstructed.22:38 native shallow pristine/reload and contour
reload pass separately. Actual save22:39:21–22:39:22 retains shallow24163E65 and
contour6F7998FF; main-menu exit observed. No relevant caught close-background or
liquid-render failures. The older automatic grove mismatch still appears on
entry and remains RED, not rebuilt or attributed to this probe.

The first run emits a caught InvalidOperationException for the **deliberately
unconverted control cell** (soil/1/0 tile0 wall2). It is expected, precedes the
actual conversion loops and is not a product failure. Native negativeControls=1
in all four reports. Preserve that distinction from unexpected exceptions.

Raw conversion JSON files are copied byte-for-byte; `native-p-conversion.json`
retains22 scoped request/result/save records. `Tools/Test-MawConversionEvidence.ps1`
independently recomputes raw timing statistics, phase order/counts and pass
consistency.16 optimistic synthetic mutations are rejected and an honestly
reported partial failure remains readable. This cannot independently prove
world-state booleans; those are the live assertion results. Static batch planning
has7759 sampled assertions, not7759 distinct gameplay scenarios.

Still pending: incremental update scheduling, clustered growth, real conversion
projectiles, support changes across real ticks, full production generation,
long-session/modpack/low-end behavior and visual approval. Next work should use
this evidence to keep operations bounded, not broaden the live fixture silently.
