# Validator recovery — 2026-09-04

Scope: repair real test gaps before continuing A — Snapped. No gameplay code, production art, worlds, characters or graphics settings changed.

## Reproduced failures

1. The Terrain content gate only checked that the atlas-validator file existed. Removing an opaque soil/wall pixel, adding white in ordinary grass artwork, introducing soft alpha, resizing the soil PNG, or removing it completely still produced STATIC PASS.
2. Tree sockets were checked by row counts. A complete transparent row through all three sockets, or a one-pixel neck, passed.
3. After fixing socket detection, the real Tree gate still passed: its wrapper continued after the validator exited 1. An additional isolated real-entrypoint test reproduced this before the wrapper was repaired.

## Changes and regression contract

- Terrain now invokes the pixel validator for seven Wastes tile/wall pairs (14 PNGs): soil, grass, stone, sand, ice, snow and mud. Each uses its separately exported vanilla reference for alpha topology and dimensions, plus the authored palette bound and hard-alpha check.
- Grass preserves opaque white **only at the native engine-mask coordinates**. Broadly permitting every white pixel would hide the reported corner problem.
- Top checks traverse edge-connected opaque wood from the bottom-center anchor. Within the ordinary 17×12 socket window, require at least seven connected bottom-row pixels and five at every higher row. Straight and slightly curved valid fixtures pass; severed, necked, missing and off-center fixtures fail.
- Tree readiness runs the Git-versioned validator in a child process and propagates its failing exit code. Installed and versioned skill copies remain synchronized.

Run `pwsh -NoProfile -File Tools/Test-VisualValidatorMutations.ps1` from the repository. The suite executes the actual Tree and Terrain gates inside disposable minimal mirrors; it copies mutable PNGs, never hard-links them. Fifteen checks cover eight tree cases and seven terrain cases. Every intentional rejection must return a nonzero exit code with the relevant diagnostic. Fixtures are retained in the named `Apogean-ValidatorMutations-*` temporary directory for inspection. Windows PowerShell 5.1 also passes the eight-case tree profile.

The original 13-case suite reproduced eight false passes, then passed after repair. The added wrapper case separately reproduced a ninth false pass, then passed after repair. The final combined PowerShell 7 run passed **15/15** checks; its retained fixture is `Apogean-ValidatorMutations-bf1196a0-0034-4538-a99c-477b833bd815`. Windows PowerShell 5.1 passed **8/8** tree cases. The Status gate passed all 12 family records, eight installed/versioned skill comparisons and generator ownership checks. These are validator-behavior checks, not visual acceptance; they do not prove slope rendering, paint, biome conversion, lighting or wind behavior. The candidate's native assembly still requires review before live testing. No C# rebuild or game launch was performed in this checkpoint.
