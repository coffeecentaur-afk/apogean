# Wastes landscape V1 — source ownership

Original built-in image generations based on the user-approved Wastes composition in `Art/Reference/Backgrounds/2026-09-04/Wastes-Surface-Concept.png`. No third-party background pixels are included.

- Far-Matte.png: slate foothills and ruined skyline.
- Mid-Matte.png: damaged concrete highway, wrecked vehicle, small utility station.
- Close-Matte.png: sparse snapped trunks, brush, pipes and roots.
- Ground-Source.png: opaque lower soil/rock strata for authored flight coverage.

Each source is actually **2172×724**. Prompt requests for larger dimensions did not determine the tool's delivered dimensions; these files are not 3072-wide or 4K originals. Exact prompts are in PROMPTS.md.

The user explicitly approved local deterministic processing. `Tools/Export-WastesLandscapeV1.ps1` alone owns `Content/Backgrounds/Candidates/WastesV1/{Far,Mid,Close}.png`. It keys magenta, joins the horizontal edges through a connected minimum-error overlap cut, gives the cut-off highway a broken end, and packs the separately authored lower strata. Every output is 2048×1280 RGBA, with source pixels unscaled. Added height comes from the ground source, not an enlarged source or a stretched last row. Far ground is tone-matched to slate.

The first local dithered overlap produced a visible grid and was replaced by connected seam cuts before live testing. Static edge equality is only a guard: internal key fringes, joins and camera coverage still require live inspection. Runtime files are staged in the Forest render lab and actual Forest routing of the disposable `Apogee Native Visual V3` world, not ordinary worlds. See `Art/Validation/WastesLandscapeV1/README.md` for the measured source-scale correction and remaining promotion gates.

Raw RGBA memory: 10 MiB per texture, 30 MiB per family. Existing nine-family HD V0 assets remain 162.01 MiB; entering this lab can therefore bring the two families of assets together to 192.01 MiB, excluding engine targets and other textures. Cached Asset references are cleared on unload; tModLoader owns actual texture lifetime.
