# Forest restoration — live routing evidence

2026-09-04, tModLoader v2026.7.3.0 / Terraria 1.4.4.9. Disposable single-player world: Apogee Native Visual V3. Client viewport: 2560×1369; engine panorama output: 3040×992. PNGs are unchanged engine captures; JSON sidecars hold observed counts, routing and viewport.

| Image | What it proves |
| --- | --- |
| [00-green-regression](00-green-regression.png) | Isolation repair: 169 living grass, zero Wastes, native forest slot 10 |
| [01-wastes](01-wastes.png) | Fully ruined patch selects Wastes slot 18 |
| [02-mixed-from-wastes](02-mixed-from-wastes.png) | 47.3% living retains Wastes |
| [03-green](03-green.png) | Full restoration selects and draws green forest |
| [04-mixed-from-green](04-mixed-from-green.png) | The same 47.3% living retains green after restoration |
| [05-wastes-return](05-wastes-return.png) | Removing living grass returns to Wastes |
| [06-jungle-priority](06-jungle-priority.png) | Real Jungle metrics select Jungle slot 20 despite cached living-forest evidence |

All routes are unforced. Source/build/static checks and this bounded noon/ground sequence pass. This does **not** prove real solution-projectile conversion, both target viewports, flight/pan/weather/crossfade quality, multiplayer priorities, or final art acceptance. The straight floating floor is a diagnostic sample strip, not proposed world generation. Background mirroring and lower-row stretching remain known defects. No new reference concept has been installed.

See [the full validation record](../../../../FOREST_RESTORATION_VALIDATION.md) for the initial red test and exact reproduction steps.
