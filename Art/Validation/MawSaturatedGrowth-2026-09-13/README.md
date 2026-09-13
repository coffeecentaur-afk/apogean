# Z — full-budget local growth measurement

gg / Apogee Native Visual V3 / single player. September13 02:56–03:00UTC.
Installed QA package SHA256:
`FB1E19BE74C97C0A243EDF9ECE320782D7BD5DC860D3FF4750AD74A30D9683F4`.
Zero build warnings/errors,466+7+1 unchanged art/map pins. No policy, sprite,
production scheduling, biome spread or existing-scene edits.

The existing32x32 scratch now has a second seed arrangement: same four11x11
soil islands, but each side midpoint is seeded. Sixteen seeds supply32 targets
per update for four consecutive updates, then16. Only144 cells convert;
final grass160. The original four-seed arrangement is retained unchanged.

## Six actual240-update runs, in order

| Raw report suffix | Seeds / budget | Active updates | Actual peak changes | Max active work ms | Max idle work ms |
|---|---:|---:|---:|---:|---:|
| 025627-659-b32 | 4 /32 |20|8|0.7543|0.0907|
| 025655-554-b32 |16 /32|5|32|0.1872|0.0811|
| 025748-773-b8 |16 /8|18|8|0.1451|0.0755|
| 025808-406-b0 |16 /0|0|0|0|0.0334|
| 025852-323-b32 |16 /32|5|32|0.1781|0.1204|
| 025935-206-b32 |4 /32|20|8|0.0984|0.0857|

All six pass and independently replay. Both full-budget traces are exactly
32,32,32,32,16, then235 idle updates. Disabled remains16 grass with0 changes;
old-layout runs retain156 conversions from4 seeds. This is brief local full-
budget coverage, NOT240 saturated updates, a speedup claim, full-frame time,
whole-world load, low-end performance or multiplayer acceptance. First-call
and warm results are both retained rather than hiding the larger first sample.

Work timing includes1024-cell observation, plan, apply and native framing.
Validation/setup/cleanup/export are excluded. Each32-change work sample records
608 bytes allocated; idle still records32 bytes/update. No allocation fix or
zero-allocation claim. Shapes, paint, coatings, wires and host identities pass
the existing actual-state checks throughout all samples.

All56x56 guards restore the exact native storage digest
`81115FCEACB61196D870A241C12D0F66662EBA5036175A7E51E217AA0651009D`.
Historical digest remains
`D048B208B5EE51C248E51A25A2B7E3BF5D64C019C8108F0CF5A7AE6DE003920F`:
nine known locations retained, nine absent locations unverified.
Normal save03:00:12–13UTC retains16 canonical system records (`z-save-tags.json`).
`z-native-log.json` retains14 scoped records, no relevant caught draw stack.
The older grove baseline mismatch remains RED, preceding these tests; it is
not repaired or certified clean by this export.

## Environment and reusable checks

tML2026.7.3.0 / Terraria1.4.4.9. Same31.1GB reported RAM, RTX3090 setup as W;
loaded content mods are Apogean0.1 and CheatSheet0.7.8.1. Startup announced an
automatic SpiritReforged0.2.2.7→0.2.2.8 Workshop update, but the content-loading
log confirms it is NOT enabled. No update/install was requested by this pass.
Resource packs/settings were not changed. Timings above compare within this
client session; no perfectly controlled cross-session improvement is claimed.

`Tools/Test-MawGrowthLoadPlan.ps1` now tests both layouts, unchanged-cell state,
exact full-budget sequence and strict request grammar.1,978,320 assertions are
mostly unchanged-cell comparisons, not that many independent scenarios.
`Tools/Test-MawGrowthEvidence.ps1` preserves the original27 corrupt-report
controls/seven W replays and adds eight schema2 synthetic budget/layout cases,
six layout/frontier defect controls and a retained saturated partial. New raw
schema2 declares layout; old schema1 is never rewritten. Native stop/interruption
evidence remains W's original partial, not a newly claimed Z interruption.

The status and contracts still withhold production/worldgen/art approval.
