# Motor depot — pixel-language review

The user finds the native-size ruins too realistic compared with the accepted
gas station. Retain their general architectural size and basic designs; redraw
their material language. One depot correction is the bounded probe before
touching the shell or checkpoint. Station, Highway, Far and Close remain unchanged.

## Inspect

`Selected/Comparison-Dark.png` and `Selected/Comparison-Light.png` show the
unchanged Station, previous depot, and new depot at 1:1 preview pixels. These
are **offline comparisons, not new game screenshots**. All are upper-only crops;
their cut-off bottom edges are not acceptable complete background modules.

The new depot groups concrete into broad shaded faces, separates rust into
readable patches, and simplifies the cliff/rubble. It is a fresh style-transfer
redraw, not a pixelation filter. Appearance is pending user review. Pixel-cluster
style is separate from building size, canvas resolution and alpha correctness.

## Source and preparation

- One built-in image edit; exact prompt and reference roles: `PROMPT.md`.
- Original `MotorDepot-original.png`: actual1317x1194, entirely opaque, SHA256
  `6CBDBE740DBD73ACF85D50672F3ED45C63887AD010A505BEB90FF73861BCB034`.
  The requested1024x928, strict2x pixel grid and alpha were **not** delivered.
  The original remains unchanged and is not a usable runtime export.
- Agent inspected all seven proposed near-white components. Component1 is the
  exterior checkerboard;2 is the tiny roof/vent gap;3–5 are broken roof-steel
  openings;6–7 are exterior wall/grass slits. Retain dark doorways and recesses.
  The reviewed mask removes942706 pixels;629792 remain. No silhouette erosion.
- A separate color master repairs3241 near-neutral edge pixels using recorded
  nearby original material donors within6 source pixels, only within2 pixels of
  the reviewed edge. `Selected/edge-changes.json` records every coordinate and
  donor. This source-specific repair is not a general pale-color removal rule.
- The existing exact mask exporter produces real hard alpha and zero RGB below
  alpha0. `MotorDepot-Transparent.png` SHA256:
  `E1568D8F7D0E9DC8C2E7697F6514B684DD5EE34585BF5213E739B8E68D985630`.
- Explicit uniform1/3 center-nearest sampling, offset(32,38), yields the
  **512x432** upper study. New ground y~904 maps to339; vent y~488 to201;
  facade x185..1160 maps to94..418, close to the previous native study.
  This is approximate landmark registration, not exact geometry preservation or
  a fulfilled generated-size contract. No art is enlarged or called new detail.
- Fitted `Selected/MotorDepot-Upper.png` SHA256:
  `CA51A9C972F61DCD5E598AB523FD00A50416D5B672A7064CD786A3777D66045B`.
  Do not supply this shorter sprite to the existing512x460 scale-gallery pack.

## Reproduce and verify

```powershell
# Run from the repository root; destination must not already exist.
& ./Tools/New-WastesMidStyleStudy.ps1 -OutputDirectory <new-directory> -RemoveComponents 1,2,3,4,5,6,7 -Finalize
pwsh -NoProfile -File Tools/Test-WastesMidStyleStudy.ps1
```

All1572498 original/mask/export pixels,3241 donor changes and221184 fitted
pixels pass independent checks. The actual validator CLI rejects MatteLeak
(RGBA_EXPORT), WrongSample(STUDY_SAMPLE), InteriorColor(COLOR_PROVENANCE)
and AlphaErode(MASK_CHANGED); mutations touch decoded copies only.
An initial missing C# console reference failed compilation; fixed before the
positive and reason-checked negative runs. That failed compile is not test proof.
Fresh-directory reproduction is byte-identical for every Selected output.
Accepted Station/Close/Far pins pass. No content build, native session, installed
mod, renderer change, new world or production promotion occurred in this pass.

## Next boundary

Show this bounded style correction first. If accepted, use it and Station as the
visual language for the other two existing ruin designs. Native review must
follow a correctly sized QA assembly; old size screenshots do not validate new
art. Deep foundations, stable quiet-spaced placement, flight/routing/lighting and
production gates remain separate. Do not generate more unrelated concepts.
