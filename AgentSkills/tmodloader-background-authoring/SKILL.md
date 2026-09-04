---
name: tmodloader-background-authoring
description: Author, route, render, or validate high-detail tModLoader surface, cavern, underground, and custom-sky backgrounds. Use when parallax layers split, repeat visibly, end during flight, route to the wrong biome, look low resolution, tint poorly, or expose transparent gaps.
---

# tModLoader Background Authoring

A background is a camera system with art inputs. Art quality, layer extraction, renderer coverage, biome routing, and transition behavior must pass independently.

## Workflow

1. Read the style/sky classes, selection priority, renderer, source master, layer extraction script, and latest screenshots.
2. Define the camera contract: target resolutions, surface anchor, layer order, horizontal/vertical parallax, repeat method, night/eclipsed tint, fade ownership, and memory budget. Keep `ModSceneEffect` selection, `ModSurfaceBackgroundStyle` slots, underground slots, and any `CustomSky` lifecycle as distinct owners.
3. Keep a full source master. Derive Far, Mid, and Close layers with transparent sky; never paint checkerboards into alpha.
4. Run `scripts/Test-BackgroundSet.ps1`. It checks assets, not routing or visual quality.
5. Render deterministic ground and aerial fixtures at every supported viewport. Pan at least 2.5 texture widths left and right and move from ground to the maximum expected flight altitude.
6. Render noon, midnight, rain, eclipse, and a transition into each adjacent biome. Add a production-routing fixture that uses real biome counts without a forced diagnostic override.
7. Inspect at 1x. Reject texture edges, mirrored landmark pairs, stretched last rows, empty lower screens, native placeholder bleed-through, and detail that exists only because a low-resolution image was enlarged.
8. Promote only after the static report, visual matrix, routing telemetry, and memory budget all pass.

## Rendering contracts

- Source pixels render near 1:1 at the target viewport. Scaling a small layer is not a higher-detail render.
- Horizontal coverage uses authored seamless edges, an overlap blend, or multiple compatible variants. Mirroring is acceptable only when the landmarks do not reveal symmetry.
- Vertical coverage is authored. A one-row stretch can be a temporary diagnostic guard, not final terrain art.
- The renderer owns every pixel below the layer's baseline or delegates that region explicitly.
- Keep parallax landmarks stable during fades; one style owns the fade value.
- Custom styles do not override a third-party `ModBiome` that already won selection.
- Underground, cavern, and underworld backgrounds are separate contracts, not recolored surface layers.
- Keep each loaded biome set inside a measured GPU-memory budget and unload cached assets cleanly.
- For vanilla-style surface fades, the selected background slot approaches 1 while every competing slot approaches 0. A custom compositor must not leave the engine's competing layer opaque beneath it.

Read [background-contracts.md](references/background-contracts.md) before adding a renderer or biome family.
