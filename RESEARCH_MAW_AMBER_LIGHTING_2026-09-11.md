# Bounded amber terrain and wall lighting

September 11, 2026. Scope: one native diagnostic, not a new liquid, background
panorama, shader dependency, or production progression change.

The approved amber material already contains discrete ochre/gold organs. Reuse
that actual atlas instead of painting another glow texture. A build-only tool
counts explicit amber palette roles intersected with each native opaque frame.
For walls it samples the central 16×16 region of the overlapping 32×32 artwork,
using the compiler's existing 62% wall-color transform. Small CPU lookup tables
are enough; no per-frame image reads or duplicate atlas bank are needed.

`ModTile.ModifyLight` supplies terrain emission; the tile must be registered as
lighted. `ModWall.ModifyLight` supplies wall emission. These hooks feed native
lighting, so a bright painted pixel alone is not a terrain light. [Tile API](https://docs.tmodloader.net/docs/stable/class_mod_tile.html),
[wall API](https://docs.tmodloader.net/docs/stable/class_mod_wall.html).

The prototype uses cell-level emission, not per-pixel light rays. Its count is
from the unsloped native frame: testing trimmed slope highlights is still a
separate gate. Quiet soil, stone, bone, fiber and dust remain non-emissive.
Actuated/echo-hidden candidate tiles and occluded candidate walls emit nothing;
this is a prototype policy requiring coating/visibility review before shipping.
Custom wall drawing should honor `Main.ShouldShowInvisibleWalls`, not blindly
draw an echo-coated wall. [Main visibility API](https://docs.tmodloader.net/docs/stable/class_main.html),
[tile visibility API](https://docs.tmodloader.net/docs/stable/class_tile_drawing.html).

Default activity reads the existing `MawActivityState.IsDormant`: Matriarch
defeated, before Hardmode. The diagnostic can compare .32 and 1.0 brightness
only inside its own saved chambers without changing that world progression.
These are comparison values, not approved final lighting or transition timing.
Dormancy does not purify terrain, stop all spread, or change encounter strength.

Required evidence: actual awake/dormant native captures under the same conditions,
native light samples after settling, exact fixture state before/after save/reopen,
safe/unsafe wall flags, paint/echo/actuation and separate multiplayer checks.
Do not call a lookup-unit-test pass GPU or art approval.
