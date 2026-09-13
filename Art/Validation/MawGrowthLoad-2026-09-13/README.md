# W — native per-update fiber growth

September13, 01:55–01:58UTC. QA only; `MAW_GROWTH_LOAD_CONTRACT.md`.
Installed package SHA256:
`1D7DC30C1B226BB359C21DEEB5BD6E368C9968DE1DE4388E8840542145028784`.
Isolated mirror `ApogeanTmlBuild/c860fc726ee84055bb07d54600a729b8/apogean`,
zero warnings/errors, 466 accepted pins plus7 anatomy/1 cave-study additions.
No approved asset or production growth scheduler changed.

## Actual results

All complete runs contain240 consecutive native game-update IDs. On each update
the same1024 native cells are observed, at most the requested number of exposed
soil cells mature, and only changed cells are natively framed. Buried soil,
unseeded soil and bone remain; native slopes, half block, paint/wires/coatings
are checked separately. Every enabled run ends with160 grass cells from4 seeds.

| Raw filename suffix | Budget | Active updates | Conversions | Active work maximum, ms |
| --- | ---: | ---: | ---: | ---: |
| 015554-563-b1 | 1 | 156 | 156 | 0.7411 |
| 015621-943-b8 | 8 | 20 | 156 | 0.1374 |
| 015647-776-b32 | 32 | 20 | 156 | 0.1020 |
| 015700-694-b0 | 0 | 0 | 0 | n/a |
| 015713-609-b8 | 8 | 20 | 156 | 0.0965 |
| 015724-459-b1 | 1 | 156 | 156 | 0.1000 |

The32-budget arm actually peaks at **8 changes per update**, because this seed
layout exposes only eight simultaneous frontier cells. It is NOT a saturated
32-change benchmark. No hidden extra growth is used to manufacture that load.
First1-budget run has the largest work sample; warm-up is a possible explanation,
not an isolated cause. Idle work peaks at0.3453ms in the32-budget arm.
Every idle update still allocates32 bytes for the plan list; not zero-allocation.
Work means native read/plan/apply/frame only, not total game frame, guard checks,
validation, setup, cleanup or export. No generic FPS/minimum-hardware guarantee.

`015753-025-b1` is an intentional explicit-stop trial:31 updates/31 conversions,
reason `new-command`, incomplete/pass=false. Exact cleanup succeeds. Independent
replay accepts it only as partial evidence, not a seventh successful full run.

## Preservation and retained warnings

Each temporary32x32 patch at1396,60 restores its entire56x56 guard exactly:
`81115FCEACB61196D870A241C12D0F66662EBA5036175A7E51E217AA0651009D`.
Nine known/nine missing historical bounds; aggregate known-state hash unchanged:
`D048B208B5EE51C248E51A25A2B7E3BF5D64C019C8108F0CF5A7AE6DE003920F`.
This before/after equality does NOT certify original historical baselines.

The known grove warning recurs on entry before any growth request:
`Preserved grove differs after reload: expected=87B951FEB5C9FA9056E12F69F6862C946A7624328210500971B319E56B9929E4; actual=C6A92EB1C69623BDC9F3D35518DF0298557B2642D05F2923C70C36761AB652FB. No rebuild performed.`
It remains RED; no rebaseline or automatic repair. Verbatim full source log is
retained locally under
`C:/Users/max_h/AppData/Local/Temp/ApogeanGrowth-6c22af5590b84a5f8383cdec974fac57/W-client.log`.
`W-native-log.json` preserves16 scoped Maw records and0 matching caught draw
stacks, NOT a clean entire-client-log claim.

Normal save01:58:08UTC retains all16 existing canonical system digests in
`W-save-tags.json`, comparing the pre-W copy in that same temporary directory.
Shallow actual24163E65 and contour6F7998FF remain unchanged. Client returned to
menu then exited normally; ordinary worlds were not opened.

## Reproduce / limits

`Test-MawGrowthEvidence.ps1 -Path <raw JSONs>` independently replays all samples,
endpoints, active/idle summaries, allocations, fingerprints and verdicts. Four
synthetic fixtures,27 corrupted-report controls and partial evidence pass.
`Test-MawGrowthLoadPlan.ps1`:989092 mostly unchanged-cell comparisons.
Performance/Persistence gates pass; save dispatch checks69 cases/5 missing-release
controls. Counts are not visual approval.

Still unproven: saturated32 work, spatial scaling/chunk scheduler, real-world
protection adapters, production growth/dormancy cadence, infection, vines,
multiplayer, long-session memory and low-end performance. This runs locally
within the existing material-family workflow, not a new library dependency.
