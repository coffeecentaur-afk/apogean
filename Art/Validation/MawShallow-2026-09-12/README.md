# Shallow Maw prototype — September 12

Status: **contracted**, not visual/traversal acceptance or world-generation integration.
One new empty-space fixture, gg / Apogee Native Visual V3 / single-player only.

## K–M close-background stability

K (`D1DD27DDA6F9DAA273ADF65486C0430235BBA3162BA74A898281317BE95CA8AE`)
observed the Maw fallback loaded/cached952x480, no exception; five native records
and exact safe-save in `native-k-save.json`. One successful restart did not
disprove the intermittent failure.

L (`CA130BC91ED5329BA8B5B0C1400561BC03DAD22B437E5F86249E77CA0568CDC4`)
reproduced loaded actual952x480/cached0x0/stride0 at16:18:23.685 Central, then
the native line323 divide-by-zero. This occurred before any control command;
the later zero-width control did not inject because width was already zero.
`native-l-zero-cache-red.json` retains13 records and the caught stack. Pristine
and16:20:14 save retained originalF3CC4D2B/actual24163E65.

M (`923AAA55118BD7C6FBD2A2C90D8214A1DF927E77D0E16363A7FF4DB3D9418449`)
repairs only the chosen owned texture's cache from its loaded dimensions, or
skips it while unavailable. At16:24:55, the explicit one-call zero-width control
completed without the exception; restored952/stride2380 follows in the log.
Rib1/rib2/rib3/pocket transitions and reload/pristine pass; safe-save16:26:14–15
retains exact state. `native-m-close-guard-green.json` has33 records and no caught
render failures. Temporary instrumentation was then removed from source. This
is a scoped stability fix, not final artwork, natural lighting or worldgen proof.
All466 art/map pins were unchanged throughout J–M.

## J close inspection — shape correction still needed

Installed hash `F025192E400EF4B93F0DEBB88ABF5F08A9FB66257DF7BD304A588C1CD7F0EFB3`.
`native-j-presave.json` has72 scoped records, including the deliberately refused
amber preview while the lamp was active, plus a separately caught engine draw
failure. Plain100HP/starter baseline, no other actors in the held pocket.

- `j-rib1-inspection.png`, `j-rib2-inspection.png`, `j-rib3-inspection.png`:
  actual native captures with one temporary point light. Squared underside
  notches interrupt the single-bone contour. Shape correction remains pending.
- `j-pocket-inspection.png`: separate ceiling/floor hazards and wall context.
  Inspection light is not proof of natural illumination or traversal difficulty.
- `j-top-natural.png`: cap/rooted transition/soil at noon, no inspection lamp.
  This floating QA fixture is not a generated biome/background-routing proof.

Lamp off restores four natural RGB samples exactly to I noon controls. All11648
cells and old16 fixture bounds unchanged. Native save15:58:50–51Central retained
originalF3CC4D2B and actual24163E65, failedFalse; game exited normally.
The line323 DrawCloseBackground divide-by-zero happened before lamp activation;
it is preserved and remains RED until its own reproduction/fix passes.

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

## G clean-character comparison and safeguards

Package `D23C5B8312AFF6197913349B9F364F72F79915D11F01822FC531A57B9A74B080`
was built from mirror e54b14506c1c4e8db932606e3e988a08 with zero warnings/errors
and466 unchanged art/map pins. A new native-UI-created Classic character,
**Maw QA Plain**, entered V3 at15:00:33 Central. No save-file editing, equipment
stripping or modification of gg. Runtime verifies100 max HP,20 base mana,
starter copper tools only, no armor/accessories/pets/wings/buffs. Cheat Sheet
is still loaded; this is not a vanilla-only modpack certification.

The character skips automatic fixture construction/restoration and accepts only
existing shallow checks and safe exit. A deliberate `maw-shallow-build` request
at15:09:26 was rejected before lab side effects. Pristine checks before/after
pass all11648 cells. Keep that expected exception in `native-g-presave.json`
(92 scoped records), not as an unexplained crash or an omitted failure.

`motion-g-plain-pass-1.json` and `motion-g-plain-pass-2.json` independently pass
119 actual updates,46 airborne,12 settled,0 tooth contact,100→100 HP. Terraria
owns physics/damage; the probe supplies right/release input only. These confirm
the first rib landing with a starter character, not the connector/full descent,
return journey, hook/rope comparison, manual controls or difficulty balance.

Held pocket views use the same camera and no NPC/projectile in the panel.
`g-pocket-awake.png` and `g-pocket-dormant.png` are unedited native captures.
The preview changes only the QA bounds while held; globalDormant staysTrue.
Release clears the unsaved override. Natural dormancy and forced dormancy
produce the same four sampled RGB values. At local99,35: awake89/50/18 versus
dormant28/18/18; at28,49:114/64/11 versus36/20/7. Registered emission maxima
are(.55,.31,.055) awake and(.176,.0992,.0176) dormant,16/18 tile and16/36 wall
emitters. This verifies a real lighting difference, NOT adequate visibility.
The pocket teeth remain too dim for approval without player lighting.
`g-rib1-awake.png` and `g-rib2-awake.png` expose the same limitation; rib1 is
visible in incoming daylight, rib2 is substantially obscured. The held character
position is not traversal evidence. Existing actors are never destroyed; new
ambient spawns are suppressed only during this character's QA visit.

Pristine checks and15:10:07–08 native save retain interaction digest24163E65
and original creationF3CC4D2B. Menu and normal game exit visually verified.

Tool regression: trace validation now checks per-sample health, integer values,
actual boolean baseline fields and agreement with the final health summary.
A modified in-memory copy of the F native trace previously passed despite
last-sample1HP/header500HP; it now fails. All raw originals remain unchanged.
Two synthetic validator baselines and21 rejected defects pass separately.
The old structural-wall test now recognizes the specific packed-preview wrapper
and verifies its production fallback branch (468×180 native dirt-wall topology).
Three resolver positives/five negatives pass. This does not certify packed art;
the other broad background/world-integrity failures remain open.

## H provisional light-strength comparison

Package `7268E441BC2E419B2BD100E300C69461D7D61F0238F7A400A2BE0CE3B3877401`
entered V3/Plain at15:19:05 Central and passed exact reload. Same fixed geometry,
466 unchanged art/map pins. Held-view-only `light-*-bright` requests multiply
the existing amber emission by1.6; no production brightness or progression edit.
The pure scope matrix passes951 checks, including exclusion outside the scene.

Unedited native `h-pocket-awake-bright.png` and `h-pocket-dormant-bright.png`
are comparison evidence, not final art approval. Stronger amber helps locally,
but several teeth and most of the connector remain difficult to see. Increasing
one distant light does not establish adequate hazard lighting everywhere.
At local99,35, awake142/80/18 and dormant45/25/5 were measured with Plain baseline
and zero other actors. Max emission is(.88,.496,.088) versus(.2816,.15872,.02816),
preserving the32% dormant ratio. Both commands leave globalDormant=True.

`light-natural` at15:27:43 restores scale1 and actual dormant emission exactly.
The red-channel controls return to G's36/36/28/26. Other sampled channels differ:
the held camera did not freeze the advancing world clock, so changing ambient
sky light confounds an exact cross-run RGB comparison. Retain this limitation;
the next harness must hold time as well as camera for controlled art comparisons.
Release clears preview strength/state. Neither1.6 brightness nor new art is
promoted. Native H save/preservation records are archived separately.

## I outbound connector movement

Package `6772E191152F1DD54E469BDD5C9AB13FDFC350E73294DE24E350704D1D2FB861`,
mirror c14449df02604389bb253b4284c2b990, zero warnings/errors and466 unchanged pins.
V3/Plain entered15:35:38 Central; actual saved-state reload passed15:35:55.
Two runs at15:36:20 and15:36:55 both pass232 actual/controlled updates,66 falling,
12 settled, zero tooth contact and100→100 HP. The unmodified files are
`motion-i-connector-pass-1.json` and `motion-i-connector-pass-2.json`.
Both independently pass the route-aware trace validator. The actual samples
show the body stopping against the first shaft wall, reaching the middle floor,
passing the two-tile throat and settling on the lower floor. No tile was cleared,
body snapped, velocity assigned during travel or jump/rope/equipment supplied.

Entry regression `motion-i-entry-pass.json` also passes119 updates/46 falling/
12 settled/zero contact/100HP. Native movement and exported-validator checks are
separate from manual feel, difficulty, return ascent and full Gullet traversal.
The controlled lighting follow-up and I pre-save checks are recorded next.

I held-noon lighting sequence restores all four sampled RGBA colors exactly
after natural → awake1.6× → natural (15:38:57 /15:40:01 /15:41:45 Central).
Natural samples are36/20/7,36/20/9,28/18/18,26/17/17; zero other actors and
unchanged starter baseline throughout. `i-pocket-awake-bright-noon.png` is the
unedited controlled capture. It remains locally underlit away from the organ;
this does not resolve the visual gate. H's clock-confounded comparison remains
retained, not rewritten. Subsequent inspection lighting will be explicitly
labeled as an art diagnostic, never a natural-light or difficulty pass.

Pristine passes15:42:34 and native save15:42:56–57 retains originalF3CC4D2B /
actual24163E65. `native-i-presave.json` retains68 scoped verbatim records.
