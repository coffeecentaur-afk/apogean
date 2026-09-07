# Wastes ruined-building and checkpoint tops — pending style review

## Decision and scope

The user interrupted combined midground/depth work because the checkpoint and
ruined-building tops did not match the approved gas station and garage. This
candidate changes only those two tops. The two approved reference PNGs are
byte-for-byte unchanged. Nothing here is installed, built into the QA client,
or promoted to normal backgrounds. No world was opened or modified this turn.

Review `Study/Comparison-Dark.png` and `Study/Comparison-Light.png`: unchanged
Station and garage above, new broken shell and checkpoint below. These are
1024×1000 offline boards containing four 512×460 panels at 1:1 asset pixels,
with the soil datum at panel row340. They are NOT native game screenshots.

Agent observation: the candidates use broader chipped plaster/rust patches and
quieter wall shading. The checkpoint still has a small gray footing rim. Its
footing is not accepted as final terrain. Style equivalence is for user review,
not something inferred from passing alpha or provenance tests.

## Sources and preparation

`PROMPTS.md` records the two built-in image-edit prompts and references. One
candidate was generated per target. Both returned opaque painted checkerboards
and dimensions different from the requested canvas; neither is a transparent
production sprite. Retain these originals and do not regenerate merely to try
to obtain alpha.

| Target | Original size | Nearest-center fit | Panel offset |
| --- | --- | --- | --- |
| BrokenShell | 1322×1190 | 3/8 | +8, −35 |
| Checkpoint | 1323×1189 | 1/3 | +32, +5 |

`Tools/New-WastesMidRuinMasks.ps1 -Family RuinUpperStyle` adds a version-pinned
family without changing previous family inputs. Reviewed component decisions
are in `MaskDecision.json`; proposed masks/previews in `Proposal/`; reviewed
masks, separately prepared RGB, donor records and hard-alpha exports in
`Selected/`. BrokenShell removes components1–16. Checkpoint preserves tiny
roof/wall highlights5–8 and21–22 while removing the recorded background holes.
Dark recesses remain opaque. No generic color-key acceptance is implied.

Edge RGB preparation uses recorded original-source donors within6 source
pixels, and only at kept pixels near the reviewed mask. There are3330 shell
donors and4075 checkpoint donors. The exporter subsequently applies the exact
mask; clear pixels have RGB0. Fitting is explicit reduction/registration, not a
claim that the generator preserved exact previous geometry. No long cliff is
present in this upper-only study.

## Verification

`Tools/Test-WastesRuinUpperStudy.ps1` independently audits original hashes,
approved-reference hashes, mask values, every prepared/exported pixel, bounded
donor provenance and every fitted sample. Positive pass:

- BrokenShell:1,573,180 source/export pixels;235,520 study pixels.
- Checkpoint:1,573,047 source/export pixels;235,520 study pixels.
- No partial alpha or hidden RGB in the exported/fitted assets.
- Three actual CLI fault controls reject with their exact reason:
  `SoftAlpha → SOFT_ALPHA`, `HiddenRGB → HIDDEN_RGB`,
  `ChangedSample → SAMPLE_PROVENANCE`.

`Tools/New-WastesRuinUpperStudy.ps1` reproduces the fits and comparison boards
into a new output directory. `Study/study.json` records all fit/source/reference
hashes. The earlier tool compile issue (missing System.Console reference) was
fixed before the final positive and fault-control passes.

A separate fresh-directory reproduction matched both fitted PNGs, both boards
and `study.json` byte-for-byte. The Status gate passes31 evidence-consistent
families,8 versioned skill snapshots and generator ownership. `git diff --check`
passes. These checks do not replace user style review or a native fixture.

## Next gate

Ask whether these two TOPS match the approved pair closely enough. Do not install
them or continue the combined depth pass before that review. If accepted,
preserve all four uppers, author compatible deeper terrain and quiet placement,
then test the assembled set together in the disposable QA world: ground scale,
horizontal/diagonal travel, ascent/Space/descent, lighting and real biome/
restoration routing. Only after the Wastes baseline passes, proceed to Wastes
mobs per the user's requested order. The existing QA grove persistence digest
mismatch remains unresolved and must not be bypassed.

The previous lower-extension work is parked/rejected separately in
`../WastesMidBank-v1/README.md`; it is not an approved dependency of this study.
