# Coarse arrival pod — native appearance review

User explicitly requested an in-game view before judging appearance. This is
authorization for an isolated Native-v2 review, not final art approval.

## Package and preserved fixture

- Built with all six previously accepted Wastes candidate inputs, substituting
  ONLY ArrivalPod-v1/Native-v2 for Native-v1. Zero warnings/errors.
- Installed QA package SHA256: `CF11FF1CBA3189F8F2BD8612A90375AC3A1353D26013924D7BDA8B3ED48AFCE8`.
- Isolated mirror: `C:/Users/max_h/AppData/Local/Temp/ApogeanTmlBuild/ddc579a60d304e4981aed877517dbf24/apogean`.
- Compared to the previous mirror: 408 existing Content PNGs unchanged; only
  Diagnostics/ArrivalPodTile.png and ArrivalPodItem.png differ. Repository
  Content remains unchanged; candidate hashes are pinned by the build option.
- Pre-run package, disposable world pair and gg character pair backed up to
  `C:/Users/max_h/AppData/Local/Temp/ApogeanArrivalPodV2QA-5d7b55d282504d6a86c374043be0cbfc`.
- Opened only gg / Apogee Native Visual V3, single-player, tModLoader 2026.7.3.0
  / Terraria 1.4.4.9. Existing platform at (4476,540,64,24), floor Y559.
- On load, the pod `reload` check passed unchanged checkpoint
  `273B29B0F2AF8EAA6628C85793FDA1587E5087B1FA5F6D6B6028F523EBF8BB8B`:
  three objects, empty fourth slot, no rebuild or generation on load.
- Known older grove reload mismatch (expected 87B951FE..., actual 65A5229C...)
  remains unchanged and preserved, not repaired or rebased by the pod test.

## Native appearance evidence

- `day.png`: original 2560x1401 game-window capture. New coarse damaged pod
  beside gg; the middle copy is blue-painted, not another design. All three
  objects draw on the preserved platform without visible separated cells or
  a white cutout rim at this view. Final artistic acceptance belongs to user.
- `day-detail.png`: exact 460x190, 1x crop of day.png at (1100,615), made with
  Tools/Preview-BackgroundCrop.ps1. No resizing, repainting or compositing.
  This is a crop of the ACTUAL new native render, not context-scale.png.
- `inside.png`: original game capture after acknowledged inside view. Player
  draws in front of the pod interior; the attached hatch stays with the object.
- `night.png`: original capture after acknowledged night view. Player equipment
  lights nearby objects orange; this is not pod self-emission. The farther copy
  is darker. Not an equipment-free photometric comparison.
- `ready.png`: fresh final daylight view. Left gg beside the pod in the running
  disposable world for direct inspection, without saving/quitting or rebuilding
  the fixture. Normal time/weather continue after a diagnostic view command.
- `native-pod.log`: exact selected records from the current client.log, retaining
  the known grove failure as well as the pod reload, view and guard results.
  It is an excerpt, not a claim the full session was error-free.

The bounded reload/day/overlap/night/day sequence did not change the grove
snapshot. That guard means unchanged DURING this sequence; it does not accept
the older grove's failed reload baseline.

## Scope and next gate

Only the isolated build selector changed; no runtime C# changed. The new build
ran the candidate audits and completed with zero warnings/errors. Compare proof
above confirms all other 408 Content textures are unchanged. Native-v1's 58
mechanical game-API checks remain historical evidence; they were NOT rerun or
relabeled as new tests here. No new pod crash was observed; the pre-existing
grove diagnostic error remains open.

User appearance review is pending. Do not request offline approval again: the
user explicitly needed this native view to judge scale. Next: user judgment,
remaining manual pickup/inventory/re-placement appearance, actuator/sloped-anchor
and multiplayer cases, then bounded new-world divot integration. No spawn divot,
regular-world retrofit, relay, multiplayer proof or production promotion yet.
