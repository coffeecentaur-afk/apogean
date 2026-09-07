# Station style bridge — one offline candidate

## Selected; bounded native check completed

The user answered **yes** to the middle candidate. Freeze the selected
`Study/Station-Upper.png` hash
`81CD2A9EB7CC99E637CFCF2EEF610CB64AA3A8EC90D06A4723B51BF93D0F0861`.
The exact image now has a clean isolated build and fresh daylight ground-gallery
proof in `../../Validation/WastesStationBridge-2026-09-06/README.md`. No new art
generated. Only the temporary gallery received it; the full-depth Station and
repository Content remain unchanged. Style selected, depth/production pending.
The following sections record the earlier preparation, not a current refusal
to proceed or a new request to approve the same offline picture again.

## Latest user direction

The garage has finer detail but the preferred design/cutout. The Station has
larger, fewer pixel clusters, which the user likes, but its perimeter looks
roughly extracted. Aim between them; redoing Station is allowed. This qualifies
the earlier "Station is the immutable style reference" direction. Preserve the
originals, but do not keep treating every part of Station as accepted.

The garage itself is liked. The previous native Station/garage pair is **not
accepted for cohesion**; its technical draw evidence is retained separately.
No further garage redraw this turn. No runtime PNG, game code, installed mod,
native client, or world changed. Wayfinder #11 remains open.

## Candidate and source handling

One built-in image edit, exact prompt in `PROMPT.md`, with original Station upper
as the edit target and the unchanged component-assembled garage as reference.
`Station-original.png` is retained verbatim (SHA256
`E5E81D8B8B612ADF8DA814BA207B5868495239D18F631759E13EF47149357F75`).

The generator returned **1323x1189 fully opaque pixels**, not the requested
1024x920 hard-alpha image or exact logical grid. Its painted checkerboard is
NOT transparency. This original is not engine-ready and is never installed.

`Proposal/` contains explicitly unreviewed mask proposals. After inspection on
dark/light backdrops, `MaskDecision.json` selects the 17 neutral matte components:
exterior, canopy/sign/pump-hose openings, and small gaps between vegetation.
Warm plaster/paint chips and dark shop recesses remain opaque.

`Selected/` separates the reviewed binary mask from source RGB preparation.
Exactly **5,690** retained edge pixels take a recorded original-material donor
within six source pixels. No mask erosion, silhouette enlargement, interior
repainting, or invisible RGB. The exact exporter retains **693,600** opaque
pixels and clears **879,447** pixels; partial alpha is zero. This validates
transparency/provenance, not artistic acceptance.

## Comparable-size study, not a game screenshot

`Study/Comparison-Light.png` and `Comparison-Dark.png`: left original Station,
center new Station, right unchanged garage. Each has a 512x460 art panel drawn
at 1:1 asset pixels. The new source is explicitly resampled nearest-center at
3/8, shifted +40 pixels vertically, giving approximate soil row340. This is
an **approximate geometry fit**, not a lossless repair or proof of an exact
two-pixel drawing grid. The editor may scale the whole board for display.

Agent review: broader canopy paint chips and thicker connected dead branches;
dark-backdrop contours read more continuously. Terrain still has fine-grained
detail and the regenerated structure's proportions shift slightly. Not a claim
of a perfect garage/Station style match. Review this one candidate before more
generation. Upper-only art lacks a deep foundation and cannot replace the
existing 1408px module or inherit its flight/lighting evidence.

## Reproduction and bounded validation

Run from repository root, using new output directories:

```powershell
pwsh -NoProfile -File Tools/New-WastesMidRuinMasks.ps1 -Family StationBridge -DecisionPath Art/Candidates/WastesStationBridge-v1/MaskDecision.json -OutputDirectory <new-mask-directory>
pwsh -NoProfile -File Tools/Export-MaskedBackground.ps1 -SourcePath <new-mask-directory>/Station-EdgeSource.png -MaskPath <new-mask-directory>/Station-Mask.png -OutputPath <new-mask-directory>/Station-Transparent.png
pwsh -NoProfile -File Tools/New-WastesStationBridgeStudy.ps1 -OutputDirectory <new-study-directory>
pwsh -NoProfile -File Tools/Test-WastesStationBridgeStudy.ps1 -StudyDirectory <new-study-directory>
```

Study generator pins the checked-in export and both references. Independent
audit checks all 1,573,047 source/export pixels, every edge donor, and all 235,520
fitted pixels using a separately expressed integer mapping. SoftAlpha,
SampleChanged, and HiddenRGB actual CLI controls reject their expected defects.
The first no-decision proposal exposed an empty-PowerShell-array/null bug in the
existing mask tool; the corrected initialization is verified with both the new
Station and default three-Ruins proposal paths. Reviewed outputs reproduce
exactly. Build/live/production/user-art gates remain pending for this candidate.

Next, if this direction is selected: deep terrain continuation, disposable
native scale/edge/light check, then placement and existing Wastes baseline gates.
Do not infer permission for endless redraws or expand to other biomes first.
