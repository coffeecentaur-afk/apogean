# Copied-world headless QA contract

This is a load/save/persistence probe, not multiplayer gameplay approval.
It is part of the bounded September12 autonomous pass. Never expose a server,
install dependencies, or use ordinary world/player files for this test.

## Ownership

`Tools/New-MawHeadlessSandbox.ps1 -ExpectedModSha256 <reviewed-package-hash>`
pins the installed framework and explicitly selected package, refuses a running
tModLoader process, and copies the existing V3 `.wld`/`.twld`, enabled list and
the two already-installed mods into a unique temporary directory. Verify every
copy and unchanged source hash. It prepares only; it never launches the server.
No player saves, ordinary worlds, configuration edits or auto-created world.

Launch the returned installed-runtime command with its exact save/mod/world
paths, `-server -nosteam -noupnp -ip 127.0.0.1 -port 17777 -players 1`.
Confirm the owning PID's listener is loopback-only. Native SSDP discovery can
still be logged despite `-noupnp`; preserve that fact, not a zero-network claim.
Do not use a platform launch script that downloads or updates dependencies.
Exit via the owned console's normal `exit` command, not process termination.

The manifest and scoped evidence are checked in. World/mod copies and full
runtime logs stay in the temporary sandbox. Never copy a server-written save
back over the client QA world as an implicit part of a smoke test.

## Separate gates

1. Clean load: correct runtime, package, local mod source, world size/name and
   no caught load failure. Native startup RAM attribution is not per-mod peak.
2. Lifecycle: explicit save and normal exit, retained exception stacks, correct
   PID and loopback state, every original input unchanged at that checkpoint.
3. Metadata: real installed `TagIO.FromFile` reads both companion saves.
   Compare all system entries using a typed, length-prefixed canonical digest;
   compound key order is irrelevant, lists/numeric bytes/types are preserved.
   `-RequireUnchanged` must reject a real missing-record result. Never replace
   missing data with defaults or infer preservation from zero exceptions.
4. Reload: start the same written copy in a fresh process and save again.
   Retain the first-run log before native logging rotates it. Compare again.
5. Resources: raw process samples are observations, not hardware limits. A
   no-client idle server is not an active simulation/load/stress benchmark.

System-tag equality does not prove entire-world tile equivalence, entity
synchronization, client rendering, network ownership, all modpacks or low-end
performance. Original scene fingerprints are separate evidence.

## Persistence rule learned from the failure

World-owned QA metadata must survive saving with a different test character,
with no selected player, and on a server. **Preservation is not permission to
operate a lab.** Load/save existing records by world ownership and existing
state. Keep mutation, movement, rendering and initial sampling behind their
original single-player/player-name guards. Do not create missing fixtures or
recalculate checkpoints to make a test green. Ordinary/null worlds remain
excluded. Malformed-data migration and recovery need a separate explicit check.

`Tools/Test-QAWorldPersistence.ps1` extracts the ten actual load/save bodies
and predicates with minimal environment/TagCompound stand-ins and the real
immutable ground-profile class. Its180 cases cover two round trips, network
modes, gg/plain/no selected player, absent metadata, ordinary/null-world guards.
This is a fast regression seam, not a replacement for native TagIO evidence.
`Tools/Invoke-ApogeanContentGate.ps1 -Profile Persistence` runs it unattended.

Eight old fixture records were already absent in the pre-server client file.
Their synthetic regression coverage does not establish their former geometry,
cause/time of loss or recovery. Never rebaseline/rebuild them automatically.
Record missing provenance and audit recoverable copies separately.
