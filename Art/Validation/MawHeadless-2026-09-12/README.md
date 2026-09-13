# Headless copied-world persistence — September12

Scope: existing large8400×2400 V3, isolated copies, localhost17777, no connected
players, apogean0.1 + existing CheatSheet0.7.8.1 only. Native client stayed closed.
Runtime1.4.4.9+2026.07.3.0 /666f699; framework SHA256
`D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C`.

## A: retained real failure

Unfixed R package SHA256
`019B55154B7AEB8C8FD2DFEDAF900183F37A384DE02AA4AC16ADC3F686D73DAE`.
Sandbox `c30171cd232e45d3ad8967568889d163`, process11688, session23:39–23:43UTC.
Both manual save and save-on-exit completed with no logged exception.
The original `.twld` contains16 system records. The copied save loses exactly
`VegetationVisualLab` and `WastesGroundProfileSystem`; the other14 canonical
system digests match. `ModLoader/UnloadedSystem` also matches, so the two records
were not merely transferred into an unloaded-system payload.

`tags-a.json` is the first real parser result. `tags-a-replay.json` repeats it
with14 canonical-digest controls. `tags-a-red.json` runs the same comparison
with `-RequireUnchanged`, exits1 and retains the failure. No file-string search
or guessed NBT format was used. The reader initially tried an unsupported
IDictionary cast; corrected to TagCompound's key/value enumeration before the
first report. That tool exception never wrote either input save.

Eight older QA records were absent BEFORE this server test. Their source has
the same single-player/gg-only save pattern, but this run did not erase them.
No automatic reconstruction or past-cause attribution is justified.

## Narrow repair and B

Ten QA systems now preserve existing metadata by V3 world ownership. The two
server-blocked loaders read existing data; their sampling/drawing/interaction
guards remain single-player-only. Eight older save hooks no longer depend on
gg or network mode. No terrain, art, collision, production progression, player
authority, or fixture creation change.

Actual-hook regression `hooks-before.json`:84/180 failures (including explicit
no-player stand-in exceptions; not claimed as native server crashes).
`hooks-after.json`:180/180 pass. Every case performs two round trips and checks
ordinary/null worlds where reachable. Minimal stubs are disclosed in the file.

Fixed package B SHA256
`B9BD5B8816109E3D2262A671AEDB8560A0677440871BA298D11EE2881D3AB18D`,
build mirror `6d726d17277049b4b0224ce9af6c6953/apogean`.
Isolated wrapper clean0warnings/errors;466 base pins plus the same7 anatomy
and one cave-study additions. No accepted pixel changes.
Fresh sandbox `e0e14f4c82fb464c9f34fb5b08035915` uses the unchanged original
client world, not A's damaged copy. Process18748 first loads23:55UTC, saves
23:56:35 and exits23:58:05. All16 canonical system records match after both
saves (`tags-b-save1.json`, `tags-b-exit1.json`). Fresh process33384 reloads the
same written copy23:58UTC and saves/exits00:00:54UTC September13. All16 records
still match the original (`tags-b-reload.json`, `-RequireUnchanged` exit0).
The two B processes' running/exited exports have zero logged error stacks and
unchanged source inputs. Exact contour6F7998FF and shallow24163E65 interaction
fingerprints persist in all native save logs. This is not a whole-world tile
comparison, and cannot recover the already-absent eight older records.

## Resource observations, not acceptance limits

A15 one-second idle samples: peak observed private bytes1,040,371,712.
A15 after-save samples:1,438,072,832. B15 after-save samples:1,437,769,728.
These are fresh-process observations, not repeated active workloads or leak
evidence. No GPU rows were available; empty rows do not mean zero VRAM.
Do not claim per-mod resource savings, player-loaded performance, low-end
compatibility or a RAM limit from these figures. Native startup attribution
and whole-process private bytes measure different things.

Every lifecycle export retains the actual scoped logs, caught errors and
listener/source guards. Native SSDP discovery is disclosed despite disabled
UPnP forwarding flags. Both server-owned written copies remain disposable.
See `MAW_HEADLESS_QA_CONTRACT.md` for repeatable commands and evidence limits.
