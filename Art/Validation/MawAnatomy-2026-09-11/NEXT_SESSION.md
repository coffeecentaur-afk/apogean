# Next session — preserve evidence, advance one remaining gate

Repo: Apogean ModSources, not the unrelated PrismLauncher working directory.
Read this directory's README, current ACT1_ROADMAP and the applicable authoring
skills. The two-hour September11 work window ends01:09:50UTC September12.
No further overnight schedule is authorized by this checkpoint.

## What to review

The anatomy scene compares full fiber → rooted soil → soil, and new cortical
ribs against the retained porous bone. The first rib surface is too horizontally
striated; v2 is a separate chalky alternative. User approval is still needed
for final texture, not for whether masks/collision work.

Amber captures show the same sealed tile/wall rooms awake and dormant. No
global activity flag was changed. This is native lighting proof, not a finished
cavern or performance approval. Six existing matching unsafe wall swatches
are separate from roots so they cannot conceal seams.

## Preserved fixtures

| Scene | Bounds | Allowed next action |
| --- | --- | --- |
| Original natural |6276,420 /128×104|Read/test only|
| Original playable |7156,360 /128×104|Warm with natural, then existing suite|
| Fiber/rib growth |7596,240 /96×72|View/test/reload/joins; never mature again|
| Anatomy |7396,80 /100×76|View/capture/test/reload; never rebuild|
| Hanging fibers |7396,176 /100×20|Audit/probe; strict saved-state test remains RED|
| Amber rooms |7276,216 /100×52|Test/reload/awake/dormant/sample; never rebuild|

Anatomy digest:
`B510E9D585CAC1CE7E9FED527E5FB8DDFA9DC211CB8D0105FE67DF91209F7FD9`.
Hanging root3 at7440,180 is missing depths4–6. Do not grow/repair it or call
39 segments its new expected baseline. Audit fingerprint is not acceptance.
The unrelated earlier grove reload mismatch also remains RED.

## Start safely

Use only gg / Apogee Native Visual V3 / single player. Build while tModLoader
is closed after a real save. The standard QA command defaults to rib v1; the
explicit final comparison command is:

```powershell
pwsh -NoProfile -File Tools/Build-CurrentQAPackage.ps1 -PackedMawPreview -MawAnatomyStudy -MawAnatomyCandidateDirectory Art/Candidates/MawRibSurface-v2/Native-v1 -KeepWorkspace
```

After opening V3, queue ONE request at a time; wait for its matching native
REQUEST/COMPLETE or failure in client.log. CreateNew refuses pending-request
overwrite. Request accepted by the script is not evidence it ran. If unfocused,
the game can pause; focus via approved native UI, not another copy of a request.

```powershell
pwsh -NoProfile -File Tools/Request-MawFiberValidation.ps1 -Case anatomy-view
# After completion and the frame is actually visible:
pwsh -NoProfile -File Tools/Request-MawFiberValidation.ps1 -Case anatomy-capture
```

Other read/test cases: anatomy-test, anatomy-reload, anatomy-vines-audit,
anatomy-vines-probe, anatomy-amber-test, anatomy-amber-reload,
anatomy-amber-awake, anatomy-amber-dormant, anatomy-amber-sample.
`anatomy-vines-probe` restores its scratch and preserves the bad exhibit; a
passing scratch test must never clear the scene's RED status.

Save with `Tools/Request-LiveValidation.ps1 -Fixture qa-save-and-quit`, then
verify menu plus native save completion. All regular worlds remain out of scope.

## Next implementation boundary

1. Record a saved-vine cut event/callsite, or reproduce the candidate cause in a
   new bounded scratch. Solar retaliation can cut vines, but has not been shown
   to cause this event. Do not change breakability to hide it. See the dedicated
   vine research follow-up for native IL/primary-source evidence.
2. Settle the rib/cap art. Keep shaft identity continuous; don't replace every
   bone mass with smooth cortex. Keep the liked side fiber.
3. Combine reviewed materials with existing easily mined thorn-style teeth in
   ONE shallow Gullet traversal proof. No regular staircase or preplaced rope.
   Test real unprepared/rope/mining/hook/double-jump/fall-protection approaches.
   Do not infer traversal from the 308 solver assertions or frozen screenshots.
4. Only then revise the legacy shelf generator and its120-tile fall validator
   together. No broad seed/worldgen promotion from this art laboratory.

Climbable natural fibers remain a gameplay choice; current fibers are cuttable,
not rope. Random growth/spread, protection adapters, multiplayer, echo/coating
rendering, sloped amber coverage and memory budget remain separate gates.

Startup reports1.7GB attributed RAM for the whole Apogean QA content bank. This
is not a measured incremental cost of today's features, a VRAM measurement or
a performance pass. Record and profile residency before multiplying this bank
across more biomes. Reuse approved texture objects; do not hide this cost.
