# Provisional cave art — September 12

Not production-installed or approved. The pixel study was packaged explicitly
for the Q/R dry cave QA probe only. This branch answers composition and renderer questions;
it does not replace the accepted terrain, Wastes scenery, or Maw background route.

## Preserved sources and derivatives

- `master-v1.png`: built-in image generation, prompt in `prompt-master.txt`.
  1536×1024, opaque by intention, SHA256
  `57E162FB20A74AA5CB74A4F0A3730D32D939B13CBA09CC35FC5D7937EF5B26F3`.
  Actual 63,293 RGB colors, not the requested restricted palette. Composition is
  useful provisionally; repeated arches and illustrated microtexture remain concerns.
- `PixelStudy-v1/master-pixel2.png`: explicit whole-image 2×2 averaging and nearest
  25-color study, NOT an unchanged-pixel repair or higher-resolution render.
  Same dimensions; SHA256
  `D9388320C53DCFFB5E617B05E5521EA264B784552584CBE44391BF24409A5B09`.
  `Tools/New-MawCavePixelStudy.ps1` and its recipe preserve the method and palette.
  More consistent pixel grid, but flattened/banded depths remain an art question.
- `shelf-source-v1.png`: separate built-in generation, `prompt-shelf.txt`.
  1536×1024, SHA256
  `3E8BF03E2E1489147830E65CEC507C30C9AC5A58D6CBAC1A177CE417341ACFA0`.
  Actual alpha: 1,102,583 transparent, 470,281 partial, ZERO fully opaque pixels.
  Therefore the original cannot enter the exact opaque-source exporter directly.
  Its oblique top surface and fine detail do not meet final side-on art direction.

## Alpha decision, not a general segmentation rule

`ShelfMask-v1` retains the failed A255-only proposal: an empty keep mask. It was
never exported or accepted. The updated proposal tool now throws on this outcome.

`ShelfMask-v2` uses this source's A≥128 silhouette and a separately prepared opaque
color master retaining every original RGB value. This changes alpha deliberately,
not color. Main-agent inspection of both complete light/dark previews on
2026-09-12 found the silhouette usable for a technical study: holes remain open,
thin hanging fibers remain, and no white matte outline is evident. Separate falling
rock fragments are in the source; their realism/placement is NOT approved.

`ShelfCutout-v1/shelf-cutout.png` was then made with the existing strict mask exporter:
420,648 kept pixels; 1,152,216 RGBA0 pixels; zero partial alpha; exact prepared-source
RGB preservation and PNG round-trip verified. SHA256
`9C8F09D91B3118358D89E947D7595B3F4B3AFEA9CAADFDAB90328630CACDBE60`.
The report pins both inputs. It proves export invariants, not final visual style.

Do not apply A128 as a universal threshold or substitute quantization for authored
pixel decisions. Preserve both generated sources. Neither shelf nor full master
has a repeat, lighting, routing, or gameplay-readability pass. The pixel study
has only the bounded R camera/zoom proof described below; the shelf is not loaded.

## Renderer investigation

See `RESEARCH_MAW_CAVE_COMPOSITOR_2026-09-12.md`: the ordinary cave seam differs
from the Hell sky. A bounded QA `RenderLayers.Background` overlay was tested,
not a production compositor. Native walls still occlude it;
background liquid precedes it, requiring conservative dry exclusions. Terraria's
photo capture omits this overlay. Normal game-frame inspection is required.

R normal frames show the study behind tiles, opaque walls, pots, webs and rail.
One diagonal camera shift and alternate zoom retain alignment. Palette reduction
looks flat/banded, and the art remains visible in some naturally dark openings;
do not promote it. Two-tile wet exclusions visibly retain the old backdrop near
pools. See `Art/Validation/MawCaveBackdrop-2026-09-12/README.md` for raw reports,
the original Q activation failure, repaired R proof and remaining boundaries.

One 1536×1024 Color texture costs 6,291,456 logical texel bytes, independent of
compressed PNG size; this is not a measured per-mod RAM or VRAM allocation.
