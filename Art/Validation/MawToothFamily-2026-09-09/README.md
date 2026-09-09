# Maw tooth family — native appearance probe

User likes the tooth shapes but cannot judge whether their detail fits Terraria
without seeing them in game. This authorizes a disposable native art fixture
before final visual approval. Do not repeat the offline approval loop.

## Bounded contract (before implementation)

- Preserve the exact short16x32, long16x48 and wide32x64 exports from
  Art/Candidates/MawToothFamily-v1/Native-v1; no regeneration or pixel changes.
- Optional QA-only ModTile types use native TileObjectData and native tile
  drawing:16px cells,2px padding, heights2/3/4, widths1/1/2. Four-pixel draw
  inset embeds roots. Check that inset in the actual capture.
- These are NON-SOLID, NON-DAMAGING art specimens for this review, not a
  substitute for the separately agreed solid, easily mined tooth hazard.
  No invisible collision box; no custom renderer, glow, magnification or wind.
- Normal lighting and real gg player scale; compare individual and grouped
  teeth on native Maw dirt/quiet bone and against a vanilla spike reference.
- New finite empty-envelope gallery in gg / Apogee Native Visual V3 / SP only.
  No clearing occupied terrain, replacing old fixtures, worldgen edits or
  ordinary-world use. Preserve the known grove reload failure and accepted art.
- Test actual registered dimensions, loaded atlas pixels, frame coordinates,
  harmless/non-solid policy, anchoring and saved fixture after reload.
- Capture actual day/night images. Label native capture backdrop differences
  and retain a real gameplay view. No claim that art review tests final physics.

Topology reference: existing ArrivalPodTile and installed tModLoader2026.7.3.0.
Official reference checked September9:
https://docs.tmodloader.net/docs/stable/class_tile_object_data.html
This documents multitile coordinate/anchor metadata, not custom fang collision.

## Results — September9, 2026

- Installed QA package SHA256:
  `D9F8581F5314A51416DE27EC91C81BE005B26DC0DD615A75D69A96FCDC374A98`.
  Corrected one initial compile error (SlopeType belongs to Terraria.ID, not
  Terraria.Enums); subsequent isolated build:0 warnings,0 errors.
- Compared the prior installed-package mirror with this build: all411 prior
  Content PNGs are byte-identical; only3 new art atlases were added. No
  repository Content PNGs, source tooth art, normal worlds or worldgen changed.
- Built one64x28 gallery at5296,500 in gg / Apogee Native Visual V3 / SP.
  Empty envelope was checked before placement; no previous fixture was cleared.
  Eight complete objects: separate short/long/wide on the left, a mixed group
  on the right. The gray root pads are artificial lab supports, not final gums
  or world-generation composition. Vanilla spikes at the far right still hurt.
-52 actual native checks passed at16:43:27, then again at16:48:51 after the
  actual16:44:35 save and16:48:30 world reload. They check registered framing,
  non-solid/non-hazard policy, loaded atlas dimensions/hard alpha/opaque counts,
  each placed frame and full-tile root support. The new static source guards
  first failed with MISSING_NATIVE_ART_TILE before implementation, then passed.
  These guards are not a complete negative-control matrix or physics suite.
- Day/night captures were inspected at native1024x448 resolution. Complete
  tips/shafts, root contact and absence of frame grids were visible. Fine grain
  remains on the larger tooth; artistic fit is awaiting the user's in-game
  verdict, not certified by numerical tests. No redraw was made this turn.
- The known preserved-grove reload check remains RED. Expected digest87B951FEB5
  differs from actual97024EEE4A (full values in native-checks.log); the actual
  digest was identical before and after this save/reload. No rebaseline or
  repair was attempted. Every tooth operation's grove snapshot was unchanged.

## Captures and limits

- `native-day.png`: unedited native game CaptureManager output,
  original `Apogean Maw Tooth Art 20260909-214400.png`.
- `native-night.png`: same, original `Apogean Maw Tooth Art 20260909-214419.png`.
- `native-checks.log`: scoped export from the actual client log, including the
  unresolved grove check rather than filtering it out.

The capture uses a known-safe vanilla water/background capture route to avoid
the previous DrawLiquid capture failure. Its green forest/HD-resource-pack
backdrop differs from the Wastes seen in the real gameplay window. These images
prove native tile appearance, NOT final Maw background routing or mouth layout.
The real window was also inspected; its screenshot is retained locally only at
`C:/Users/max_h/AppData/Local/Temp/ApogeanToothArt-gameplay-day.png` because it
contains personal overlays. No art was composited over a game screenshot.
Night uses existing player/equipment lighting, not a new tooth glow.

Appearance-only tiles are harmless and non-solid. Solid geometry, hurt regions,
mining/drop behavior, wall/ceiling orientation, multiplayer and production
generation remain separate, unverified work. The rejected broad wedge's old
physics tests do not certify these new silhouettes. Do not silently reshape
the fangs or add an invisible full-column collider to make that work.

## Reproduce / resume

Retain ALL accepted candidate build options:

```powershell
pwsh -NoProfile -File Tools/Build-ApogeanIsolated.ps1 `
  -WastesCutoutCandidateDirectory Art/Candidates/WastesFarCity-v1/QA-Package-v1 `
  -WastesCityCandidateDirectory Art/Candidates/WastesFarCity-v1/Runtime-v1 `
  -WastesScaleCandidateDirectory Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3 `
  -WastesDepotAssemblyDirectory Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1 `
  -WastesStationBridgeDirectory Art/Candidates/WastesStationBridge-v1/Study `
  -WastesMidDepthDirectory Art/Candidates/WastesMidDepth-v2/EdgeReview `
  -ArrivalPodCandidateDirectory Art/Candidates/ArrivalPod-v1/Native-v3 `
  -MawBoneCandidateDirectory Art/Candidates/MawBone-v1/MaskedNative-v2 `
  -MawFangCandidateDirectory Art/Candidates/MawTooth-v1/Native-v1 `
  -MawToothArtCandidateDirectory Art/Candidates/MawToothFamily-v1/Native-v1 `
  -KeepWorkspace
pwsh -NoProfile -File Tools/Test-MawToothArtFixture.ps1
pwsh -NoProfile -File Tools/Request-MawToothArtValidation.ps1 -Case reload
```

The fixture already exists: use `reload`, `day`, `night`, `capture`, `release`
or `save-and-quit`, not `build`. Observe consumption before another request.
Backups of the prior package and QA world/player were made before installation:
`C:/Users/max_h/AppData/Local/Temp/ApogeanToothArtBackup-52710d8ecaad470bab333b8efb8c76f9`.

Next gate: user judges detail at actual game scale. Keep the gallery available;
then implement matching native hazard/mining proof if the art is accepted.
No mouth generation, acid, mobs, dependencies or boss work in this slice.
