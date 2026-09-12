# Shallow Maw prototype — September 12

Status: **contracted**, not visual/traversal acceptance or world-generation integration.
One new empty-space fixture, gg / Apogee Native Visual V3 / single-player only.

## First native build D

Installed package SHA256:
`8737E1D2FDD62C84ABD49F82D95C2E2061F88EF0999F50CD103C34CCB0741DCB`.
Clean build, zero warnings/errors, all466 accepted PNG/map pins unchanged.

At14:11:18 Central the native controller placed the first legal site:
`(1116,180),112×104`; protected effect envelope `(1104,168),136×128`.
Creation SHA256 `F3CC4D2B7672EFA22C657E047CD71F1D18418F3442B8BFD3699568E966885FF9`.
No old scene was rebuilt, no natural terrain cleared, no generator changed.
The native value-storage snapshot survived a tile/wire/coating mutation before
placement.11648 cells and all eight cluster variants match the pure template.

At14:11:38,2947 native20×42 solid-collision/tooth-contact probes connected the
entry to lower Gullet and the side chamber. This is spatial adjacency, NOT
gravity, jumping, falling, rope/grapple use, manual traversal or survival proof.
All16 historical bounds remained semantically unchanged across each command.
Their pre-existing failures are neither repaired nor rebaselined.

`native-d-first.json` retains23 verbatim native records.
`d-upper.png` and `d-lower.png` are unedited native CaptureManager images.
These were inspected alongside ordinary Sky game-window views. The scene is
deliberately in empty sky space; the visible vanilla sky is not proposed Maw
biome routing. Captures use the existing default capture biome and are not
proof of correct environmental sky color.

## What the first view does and does not establish

- The existing material banks, dense tooth pairs and layered surface render.
- The structural ribs are continuous tapered wedges, distinct from thin teeth.
  Their join texture and final anatomical silhouette remain provisional.
- Upper light reaches the first rib; the side chamber and lower ribs are too
  dark in these wide views to approve. Do not treat unseen detail as a pass.
- No artificial fullbright, extra lighting or progression change was applied.

Next controller candidate adds closer pocket/rib views and read-only amber
emission/native-light diagnostics. A bounded actual-player input probe will
test only the entry walk/fall/landing. Its existing gg equipment is not an
unprepared control. These additions are not yet native-verified.

The immutable original-layout validator and a separate saved interaction-state
digest support later legitimate mining. A post-play pristine failure must not
be repaired or used to reset the scene. Save/reopen proof is still pending.

## E native reload, lighting, close views and failed motion

Package `3A1A0F2A0B466639D49A2F295D6D7E3948197D7DDF1A26FA29536E554FEB0717`
entered V3/gg directly at14:28:12–13 Central. `reload` passed14:28:23;
`pristine` passed14:28:44. All11648 cells and16 historical bounds remained
unchanged. Native save14:32:08–09 retains actual interaction digest
`24163E65FC677E84DA67BBE7DC7E1535CEBB4096AE3326516418EC0A6213F5A5`.
`native-e-presave.json` contains61 verbatim records, including failures.

`e-pocket.png`, `e-rib1.png`, `e-rib2.png`, `e-rib3.png` are unedited native
captures. The real game-window views show Solar armor illumination/retaliation
and nearby harpies: these are compromised art-lighting controls, not final
appearance acceptance. Rib undersides still show stepped notches. No texture
was replaced. The chamber's world activity is dormant:16/18 tile cells and
16/36 wall cells emit, strongest RGB vector(.176,.0992,.0176). Visible pocket
samples near the Solar-equipped player are bright orange, not isolated amber
proof. No progression/dormancy/brightness flags were changed.

Two identical entry tests stopped outside the bounded entry envelope:
`motion-e-fail-1.json` and `motion-e-fail-2.json`,128 native updates each,
77 airborne, zero settled, life500→500. The independent trace validator rejects
the actual files (`MOTION_TRACE: incomplete`). At center x32 the controller
released right input; native momentum carried top-left x to614.99, past the
first rib ending before x592, before the body reached its surface. This is an
input-controller failure, not evidence of missing rib solidity. A single-factor
F candidate releases at center x30; same terrain/loadout/physics, no velocity
override. Its native result is pending. Keep both failed runs permanently.

## F single-factor movement correction

Package `8FC971961123883B4392C144E93D777F0A0660A4E564BA7E5DC3C0DABBEAED58`,
isolated mirror8665ae4229834b6f9354cc2a50a51499, clean build/466 unchanged pins.
Two actual runs14:37:19 and14:38:00 Central pass113 updates each,50 airborne,
12 settled, no tooth contact, life500→500. Both raw files `motion-f-pass-1.json`
and `motion-f-pass-2.json` independently pass `Test-MawShallowMotionTrace -Path`.
The earlier release fixes this controller's overshoot without a velocity snap,
terrain widening or collision change. This is NOT difficulty/unprepared-player
approval; the same endgame gg equipment was retained in all four comparisons.
At14:38:14 all11648 cells remain pristine. `native-f-motion.json` retains19
native records, including preservation and release. Original E failures stay.

Workflow note: an accidental `-Family MawShallow` invocation was silently treated
as the default All profile by the non-advanced PowerShell gate. Added strict
parameter binding; the same actual invocation now exits1 before running anything.
The documented `-Profile MawShallow` passes. The accidental broad run also flags
the legacy structural-wall resolver and background production-readiness tests;
neither is declared green or used to overwrite accepted QA art.
