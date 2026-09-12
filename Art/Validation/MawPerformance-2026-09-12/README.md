# O — shallow synthetic render workload, September12

Installed candidate SHA256:
`216E80DAB85069BB48E98501CC6B1B468E3E028877FCD64EC955B3CE6DE58B3F`.
Build mirror `c6eda13fd0b948b9acc6445d1eea2ebf/apogean`;0 warnings/errors,
all466 accepted PNG/map pins unchanged. Existing shallowV1 at1116,180,
112x104; no new terrain, art, equipment or world-generation edits.

## Scope

One fresh tML process29864, gg/V3/single-player, 2560x1369, zoom1.3333334,
Ryzen9 7950X/RTX3090/D3D11/31.15GiB usable system RAM. Apogean+CheatSheet,
existing resource packs unchanged. Native `rib2` held view; same64 fixed neutral
inspection lights across the fixture in both arms. Sweep is a synthetic10-second
diagonal camera cycle, +/-160x320 pixels, **not player movement/traversal**.
Actual camera extent is320x640; stationary extent is0x0. The held player,
resolution, zoom, maps and source textures are unchanged across these samples.

Global NPC counts vary naturally (3–5 in timed runs), with0 projectiles at their
endpoints. They are not a zero-actor laboratory. Harpies are visibly present in
the later smoke check. gg has equipment/retaliation effects; these measurements
are not plain-starter gameplay. Read-only scope endpoints do not prove actors
were constant throughout a sample. No NPCs were killed by test instructions.

## Four timed samples (UTC)

Each has2s warmup+30s measurement,1801 update samples/1799 draw intervals,
0 pause/inactive/capture/overflow callbacks. No screenshots, builds or probes
were run during them. Raw JSON is copied unchanged from native exports.

| End UTC | Scenario | Draw p99 / max ms | Update p99 / max ms | NPC endpoints |
|---|---|---|---|---|
|22:08:50|Static|19.853 /20.741|0.573 /1.776|3→4|
|22:09:57|Sweep|19.833 /22.398|0.594 /0.931|4→3|
|22:11:04|Sweep|19.775 /22.839|2.217 /3.276|3→5|
|22:12:11|Static|19.927 /22.822|2.183 /3.449|5→5|

No timed draw/update sample exceeds1000/30 or50ms. Draw means16.665–16.668ms
are callback cadence, **not measured GPU/presentation FPS**. Both later arms have
a higher update tail; these observations do not isolate a camera-sweep cost or
justify a speculative optimization. They establish this bounded scene can be
measured repeatedly on this PC, not a whole-mod performance acceptance.

22:12:49–22:13:02: fifth sweep used for Windows-native visual inspection of the
lights/material coverage, then deliberately stopped. Native result is
`manual-stop; usable=False`, retained alongside the four timed samples. Never
include it in benchmark summaries. Gridlike lighting spots are diagnostic, not
approved ambient cave lighting or new artwork.

## Memory and preservation

During timed endpoints process private bytes range3,607,961,600–3,645,812,736.
This is process-wide memory, not per-mod ownership; no leak or savings claim.
The separate post-timing8-sample process observation ranges3,637,260,288–
3,637,637,120 private bytes. First GPU process row reports1,905,016,832 dedicated,
10,104,832 shared and1,967,702,016 committed bytes; these overlap and must not be
added together or called exact Apogean VRAM residency. No full launch peak or
low-memory test was performed.

Separate22:14 native snapshot retains402 distinct Apogean textures,
771,179,636 logical Color bytes,0 pending assets. It does not allocate/read back
textures or force GC. Full snapshot and8 process samples retained here.

Every timed and stopped run retains shallow interaction hash
`24163E65FC677E84DA67BBE7DC7E1535CEBB4096AE3326516418EC0A6213F5A5`.
22:16 pristine/reload recheck passes11648 exact cells/eight tooth clusters and
all17 historic bounds unchanged. Native Save & Quit preserves both shallow and
contour fingerprints; menu Exit closes normally.37 scoped records,
0 caught background/liquid draw failures. The old grove reload mismatch on gg
entry remains RED; its refusal to rebuild is not a new terrain regression.

## Reuse

`Test-QAPerformanceEvidence.ps1 -Directory Art/Validation/MawPerformance-2026-09-12`
replays raw samples independently and rejects11 optimistic-report mutations.
Camera path gate checks sampled bounds/continuity, stationary control, finite
inputs and bounded expiry. Native coverage and geometry verdicts are separately
checked against their raw ranges/hashes. Tests do not certify appearance or speed.

Next: bounded native material growth/conversion/support cases. Longer lifetime,
world-generation peaks, representative modpacks, actual moving gameplay and
low-end hardware remain open. Do not add scene complexity to manufacture a crash.
