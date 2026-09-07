# Gritty depot — material and pixel-grid study

Latest user clarification: the accepted gas station feels gritty AND pixel-art.
V1 overcorrected by making concrete, paint and rock too smooth/clean. Do not
treat less texture as the target. Preserve dirt, decay and irregular chipped
surfaces while controlling the visible pixel grain. Station remains unchanged.

One built-in material edit (`PROMPT.md`) restores weathered red paint, missing
plaster, soot, brickwork and crumbly cliff texture. No new design was generated
for the shell or checkpoint. The new depot is **not installed or approved**.

## Review images

- `Selected/Comparison-Dark.png` / `Comparison-Light.png`: accepted Station,
  rejected clean v1, and gritty redraw, at approximately existing object size.
  Agent review finds the redraw's weathering closer but its sampled texture
  still finer than Station. This is not a claim of a successful style match.
- `GridReview/Comparison-Dark.png` / `Comparison-Light.png`: same composition
  but the gritty redraw is sampled on a fixed two-display-pixel grid. This is a
  **resampling experiment**, not newly hand-drawn detail. It makes pixel grain
  explicit but can alias thin wires/edges; no silhouette-preservation claim.

All boards are **offline**, not new in-game screenshots. No world, native
client, installed mod, renderer, production atlas or accepted scenery changed.
The short bottoms are study crops, not authored deep foundations.

## Actual source and export

Original:1316x1195 RGB, fully opaque. Generation did not preserve requested
1317x1194 dimensions, alpha, exact roof height or strict pixel-grid constraints.
Original SHA256 `C47B826925B5AF405CF51BDEFA315991F10D07B87ECBE6234086ECCA0E10C10B`.

Reused the existing mask/study tool with a second explicitly pinned recipe;
the v1 recipe and every old output remain reproducible. All16 proposed bright
components were inspected: exterior sky, tiny gaps between roof stubs/grass,
and the exposed openings between broken roof beams. No dark recess is removed.
The mask removes835637 pixels and retains736983. A separate prepared source
records4008 bounded edge-color donor changes, without eroding the mask. Exact
export has hard alpha and zero hidden RGB. Source/mask/report remain in Selected.
Transparent SHA256 `F682C085F139FBAA475A9CF0BD70AF8955A315958B2E7E536AE27A94E43E391D`.

Upper study512x432: uniform1/3 center-nearest sampling at offset(32,58). Source
ground around846 maps to340. The vent is approximately14px higher than v1;
this is approximate landmark fitting, not exact geometry preservation.
Upper SHA256 `B7EABF4B753EB4FCB8F12C1D28A216D6AF613C9CE606E70D1C260AF7182A29E6`.
GridReview samples every6 source pixels into2x2 output blocks at the same offset.
Grid SHA256 `8995B665B0895670E8949EFF4CE2F5A8E1B9220771D8C6A1352B68033AF612B4`.

## Verification and limits

```powershell
# From repository root; each output directory must be new.
& ./Tools/New-WastesMidStyleStudy.ps1 -Version v2 -OutputDirectory <new-mask-output> -RemoveComponents (1..16) -Finalize
pwsh -NoProfile -File Tools/New-WastesMidStyleStudy.ps1 -Version v2 -OutputDirectory <new-grid-output> -GridReviewOnly
pwsh -NoProfile -File Tools/Test-WastesMidStyleStudy.ps1 -Version v2 -GridReviewDirectory Art/Candidates/WastesMidRuins-v2/PixelStyle-v2/GridReview
```

Independent audit passes1572620 source/mask/export pixels,4008 donor records,
221184 fitted pixels and221184 grid pixels. Five actual CLI defects reject the
expected reason: matte leakage, wrong source sample, non-recorded interior
color, eroded mask and a broken two-pixel block. V1 positive regression passes.
Fresh-directory generation reproduces every v1/v2 Selected file and every grid
file byte-for-byte. Accepted Station/Close/Far hashes pass unchanged.

These checks establish pixel provenance, not artistic approval. Stop at this
review boundary. Do not start an automatic regeneration loop or reinterpret a
grit correction as permission to simplify/clean the wasteland. A future accepted
design still needs native-size assembly, deep coverage and actual game proof.
