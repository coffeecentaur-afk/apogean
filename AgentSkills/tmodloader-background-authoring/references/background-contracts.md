# Background contracts

## Candidate record

Record:

- source master and extraction version;
- asset path and dimensions per layer;
- intended viewport sizes and draw scale;
- top/baseline anchors;
- horizontal and vertical parallax factors;
- repeat strategy and edge proof;
- lighting/tint states;
- biome selection priority and transition owner;
- raw RGBA memory per loaded set;
- fixture names and accepted screenshots.

## Visual matrix

At minimum, inspect:

| Axis | Cases |
|---|---|
| Camera | ground, ordinary jump, wings, top of surface sky |
| Horizontal | origin, each join, 2.5 widths left/right |
| Lighting | noon, sunset, midnight, rain, eclipse |
| Routing | forced fixture, real biome detection, boundary transition |
| Viewport | 1920×1080 and 2560×1440 when supported |

## Rejection examples

- landmark abruptly repeats at the texture edge;
- mirrored towers or roads make the join visible;
- last source row is stretched into a large color slab;
- the terrain layer ends when the player flies;
- Forest art wins while the player has real Jungle scene metrics;
- alpha contains a checkerboard image;
- runtime uses a diagnostic placeholder while tests inspect a different production path;
- nearest-neighbor enlargement is described as higher fidelity.

## Evidence gate

Acceptance requires source/master provenance, a static asset report, screenshots for the full visual matrix, routing telemetry, and measured memory. Build success alone carries no visual evidence.

## Engine-specific contracts

- `ModSurfaceBackgroundStyle` owns far/middle/close slots and fade behavior.
- `ModUndergroundBackgroundStyle.FillTextureArray` owns the four semantic transition/ground/rock slots; documented sheets repeat with a 32-pixel right-edge copy of the left edge.
- `CustomSky` owns `Activate`, `Deactivate`, `Reset`, `Update`, `Draw`, and `IsActive`. It is client presentation, never campaign authority.
- Primary references: https://docs.tmodloader.net/docs/stable/class_mod_surface_background_style.html, https://docs.tmodloader.net/docs/stable/class_mod_underground_background_style.html, and https://docs.tmodloader.net/docs/stable/class_custom_sky.html
