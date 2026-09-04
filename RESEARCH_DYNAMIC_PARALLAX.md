# Dynamic parallax: community proposals and Apogean boundaries

Checked 2026-09-04. Research only; no renderer, assets consumed by the game, world data, or gameplay changes. User separately approved **A — Snapped** from the Wastes deadwood sheet; that decision is recorded in Art/Reference/2026-09-04-Wastes-Tree-A-Approval.md.

## What the linked discussion actually proposes

Lord Garak's 2017 thread uses Kingdom to argue for a stronger near-scenery layer and environmental reactions, including deforestation affecting nearby scenery. Here, dynamic means reactive scenery, not simply scrolling layers or altitude-driven zoom. Posts 2–5 question the implementation burden and seamlessness; the author explicitly accepts a reduced scope. Posts 10 and 18 question the causal logic of distant objects disappearing when nearby objects are removed. Post 15 cautions against obstructive foreground scenery. These are community preferences and criticisms, not verified engine limitations. [Discussion, page 1](https://forums.terraria.org/index.php?threads/dynamic-parallax.61836/)

Post 31 raises the sandbox-specific problems: repeated patterns, terrain-dependent objects, and excavation. Post 37 proposes local fading around removed terrain; that is a suggestion, not demonstrated code. Posts 32–33 distinguish this thread from a released mod and raise performance as an unanswered question. [Discussion, page 2](https://forums.terraria.org/index.php?threads/dynamic-parallax.61836/page-2)

Posts 44–45 show disagreement over foreground/midground terminology. We should name the actual draw relationship instead: scenery behind playable walls/entities versus overlays in front of them. Post 47 prefers smoother, spatially coherent biome borders. We retain the user's existing whole-style fade preference rather than treating this as approval for bespoke adjacency panoramas. [Discussion, page 3](https://forums.terraria.org/index.php?threads/dynamic-parallax.61836/page-3)

All 50 posts were read as text, with quoted duplicates removed. The embedded Kingdom film was not viewed. Direct HTTPS text retrieval succeeded after the web reader failed; no downloaded artwork or video was used.

## Current engine evidence

The stable surface API exposes far/middle texture selection, close texture scale/parallax/placement parameters, and custom close drawing through PreDrawCloseBackground. These are drawing hooks, not an ecological simulation. Reactive regional scenery would require our own conditions, state ownership and persistence rules. [ModSurfaceBackgroundStyle](https://docs.tmodloader.net/docs/stable/class_mod_surface_background_style.html)

Important version nuance: the documented ModifyFarFades convention and ExampleMod increment/decrement fades, but the inspected stable Main patch passes bgAlphaFrontLayer after visibility updates. A compositor must inspect its actual runtime caller rather than accidentally advancing the fade twice. Far/middle and close have distinct engine alpha consumers. [Main patch](https://github.com/tModLoader/tModLoader/blob/stable/patches/tModLoader/Terraria/Main.cs.patch), [background loaders](https://github.com/tModLoader/tModLoader/blob/stable/patches/tModLoader/Terraria/ModLoader/BackgroundLoaders.cs)

Underground routing remains separate. Its API documents four semantic texture entries; the current Main patch allocates larger arrays, so code must not assume the documented four entries describe the complete runtime array. No underground conversion or custom foreground renderer is authorized by this research. [Underground API](https://docs.tmodloader.net/docs/stable/class_mod_underground_background_style.html), [scene API](https://docs.tmodloader.net/docs/stable/class_mod_scene_effect.html)

## Local code audit, not new render evidence

At checkout 6e909f92ace3e7fa500f6d4f96a324754e69b614:

- Content/Backgrounds/HighDefinitionSurfaceBackgroundRenderer.cs already draws three differently moving layers. Adding another layer is not sufficient to repair weak composition.
- That renderer alternates normal/mirrored copies and extends the final texture row vertically. Matching edge pixels cannot prove believable landmark repetition; row extension remains a diagnostic coverage workaround, not finished art.
- It chooses one V0 set per supported biome; its DrawV0 path does not consume the saved variant returned by RuinedBackgroundSelectionSystem.
- Ordinary ApogeanSurfaceBackgroundStyle also calls DrawV0 for supported biomes, not only the explicit render-lab style. Existing prose describing this as diagnostic-only is stale relative to the current routing code. A future routing audit must verify the live loaded build and reconcile this mismatch before claiming production isolation.
- The style reads the provided fade slot into rendererOpacity. This is consistent with the inspected current caller caution, not evidence that every transition has passed.
- No new screenshots, frame-time measurements, memory profiles, or compatibility results were produced for this report.

## Recommended scope; not approved features

1. Finish static composition, authored vertical coverage, biome routing and consistent fading first. Keep far geography stable and near scenery visibly behind playable terrain.
2. If later approved, prototype one clearly motivated change: dim a limited Maw growth/glow layer during the already-established dormancy state. Mountains, ruins and the chosen composition stay put.
3. Consider regional restoration or corporate lights only after the first proof. Do not make every mined block erase an unrelated distant tree, lake, or building.
4. Avoid front-of-player decoration by default. Any later overlay needs a readability/disable contract; atmosphere cannot hide platforms, enemies or telegraphs.
5. Prefer bounded cached region summaries or existing synchronized progression flags over scanning the world during drawing. This is an implementation proposal, not a performance guarantee.
6. Evaluate a low-motion option and quality tiers before promising compatibility with every machine. Measure actual frame-time and texture residency, including repeated crossings.

## Finite next proof

After the approved tree direction is authored and reviewed, show one Wastes background composition at ground level and flight altitude, with its layer breakdown, before broad deployment. Its live checks must include both supported screen sizes, normal zoom, day/night/eclipse, seams across 2.5 repeat widths, and a real Wastes/Maw boundary crossing. Preserve world edits and third-party scene selection. A reactive-state prototype is a separate later decision.

Research does not promote a family to fixture-pass. Design A approval does not approve the previous mismatched tree sprites, B/C, or a new background system.
