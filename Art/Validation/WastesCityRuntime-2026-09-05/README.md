# Wastes city native runtime fitting — 2026-09-05

## Verdict and latest user feedback

The city cutout is now packaged and visibly renders at native scale alongside
the accepted Station and foreground perimeter-v2 candidate. Ground, wings,
night, rain and eclipse captures were inspected. There is no obvious white
cutout fringe in the ground/wings views. Foreground art has not been regenerated.

**Not a complete visual pass:** `sky.png` shows the city still present in Space.
The user explicitly requested an eventual exit for every terrestrial layer.
This is the next functional correction, not permission to promote the current
high-altitude behavior. The user also requests a separate broken-building Mid
module tying into the city, with concept review before installation. Keep open
valleys and the accepted gas station; do not turn Mid into a solid city wall.

The lower Far extension reuses recognizable rubble patches. Native texture
quilting is not newly painted detail. Additional city variants, contrast/depth
polish, real-biome/restoration transitions, underground handoff, gravity,
multiplayer and performance profiling remain separate gates. Only2560x1369
was rendered this run;1080p/1440p have static arithmetic checks, not new captures.

## Exact package

- Far: `Art/Candidates/WastesFarCity-v1/Runtime-v1/Far.png`,1458x1792,
  SHA256 `D61106D719B292675607B8D9275BDCA73BE7CE01D9405606CF04EB8D1C966FE6`.
- Station: frozen488x1408,
  `C7017CE5572D987B7F1F7A5AEFAC4BB441ECAA9BAAD1EAF81D676E044BEDF58C`.
- Foreground: perimeter-v2,1448x1915,
  `9D039C003929EC128F43C3DFEFB3F98C22DC15488EB0AA9B0F9151C00D726FE4`.
- Highway/Quiet unchanged. Actual five-texture raw RGBA total28.36MiB;
  this is not measured GPU residency or the entire mod's memory.
- Installed QA `.tmod` SHA256
  `90B22717110C357EECB8397AE51839C4DA1208C3E908EE11BAC4F5C7D67B4B8E`.

Build: `Tools/Build-ApogeanIsolated.ps1` with
`-WastesCutoutCandidateDirectory Art/Candidates/WastesFarCity-v1/QA-Package-v1`
and `-WastesCityCandidateDirectory Art/Candidates/WastesFarCity-v1/Runtime-v1`.
Succeeded with zero warnings/errors. Copies enter the temporary build mirror,
not repository Content PNGs. A normal build without overrides returns to the
old QA artwork. Native city uses its own depth; the old reflected strata guard
is disabled for this optional asset. Runtime access remains the existing named
disposable-world/Forest-render-lab scope, not general-world deployment.

## Static proof

`Tools/New-WastesCityRuntime.ps1` fits the pinned1586x992 masked source with a
128px minimum-error edge overlap and source-ground texture quilting. No resize,
mirroring, interpolation, new color palette, or silhouette erosion. The protected
1330x928 rectangle remains identical. The1792px depth is authored from reused
native ground pixels, not a renderer-stretched last row.

`Tools/Test-WastesCityRuntime.ps1` independently checks native-pixel constraints,
opaque lower coverage, clear hidden RGB, hard alpha, allowed source donors,
42 projection/coverage cases and four deliberately invalid in-memory images.
Minimum tested bottom margin413px. The isolated builder re-runs this audit and
checks the candidate report hash before packaging. Existing camera92,
modular56 and parallax14 static checks also pass.

## Live proof, with limits

tModLoader2026.7.3.0 / Terraria1.4.4.9, `gg`, disposable
**Apogee Native Visual V3**8400x2400. Native window2560x1401 contains a32px
titlebar and1369px client viewport. Screenshots are unretouched native window
captures; they include the game's UI/overlay. No panorama reconstruction.

- Cases: ground, wings, sky, night, eclipse, rain, below-ground,
  diagonal-left, diagonal-right.
- Both complete physical diagonal sweeps:128640 world pixels,
  Far4.853 periods, Mid6.783, Close17.016. Telemetry now uses actual periods
  (1458/2655/2268), not the legacy2048 width for all layers.
- Zero submitted-geometry failures. Max module pixel error0.01px and
  ground-anchor error0.96px. These do not certify every visible art seam.
- Shallow below-ground surface scenery remains above the ground line; opaque
  foreground terrain occludes much of this view. It is not full cave-handoff proof.
- `unverified-paused-night.png` is explicitly excluded: the client was paused
  when rain was first requested. The request was repeated, acknowledged in the
  log, and actual rain captured in `rain.png`. `post-diagonal-return.png` is
  after the case ended, not a screenshot of active diagonal flight. The complete
  sweep evidence is telemetry; ground/wings views provide the direct art checks.
- The existing automatic grove checkpoint guard logged its known digest
  mismatch and performed no rebuild. No new background exception appeared.
  This run cannot be claimed as a clean vegetation persistence test.

The in-engine save-and-quit request completed, including validating the world
save. No ordinary world was opened. Pre-test backups of installed mod, disposable
world, character and config are in the local temp directory
`ApogeanCityQA-83d4f72445cc4c74b06f6c04328afe73`.

Replay archived geometry only:

```powershell
./Tools/Test-WastesModularLive.ps1 -LogPath Art/Validation/WastesCityRuntime-2026-09-05/telemetry.log -Cases @('ground','wings','sky','night','rain','eclipse','below-ground','diagonal-left','diagonal-right')
./Tools/Test-WastesProjectionLive.ps1 -LogPath Art/Validation/WastesCityRuntime-2026-09-05/telemetry.log -Viewport '2560x1369'
```
