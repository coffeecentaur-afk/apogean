# Solid brittle fang — bounded v1

Status: bounded native fixture-pass; user art review and traversal pending. No mouth generation
or production art promotion. Current user agreement: teeth should look like
teeth/horns, hurt, and be easily mined; no safe ledges or supplied rope.

One upward fang is 32x48 native pixels (2x3 tiles), a leaning cutting point and
stained root. Four orientations rotate the same prototype geometry. A 3x3
envelope allows all orientations; unoccupied cells remain genuine air, not
invisible solid furniture. The two diagonal cells use native 45-degree slopes.
There is no custom collision engine or drop-through platform.

Native topology reference: installed tModLoader Spikes export and local
Collision/Player inspection. Native immediate touch damage is tile-based, not
an arbitrary sprite-alpha test. The candidate therefore tests a shape-aware
contact check separately from standard native solid/sloped movement.

Proposed fixture tuning, not release balance: 30 base player contact damage,
normal Hurt immunity, no bleeding/debuff/glow, MinPick0/MineResist0.8. Mining any
occupied segment removes the fang as one object and yields one Bone in this
QA prototype. Support loss also removes the object. No enemy-trap damage claim.
Custom placeable item/reusable drops remain outside this first mechanical test.

Engine sheet: 16x16 bodies at18px pitch,3 columns,12 rows =54x216. Each3-row
group is one orientation; empty cells and all padding are transparent. Framing
is explicit and frame-important, NOT the ordinary288x270 connected atlas.
Save local cell coordinates/orientation through tile frames; no TileEntity or
new world-state format for production content. The native lab has its own saved
bounds/checkpoint. Candidate-loaded only; ordinary build has no new tile ID.

Failing checks must precede installation: exact frame mapping/round-trip,
binary alpha, palette, source hashes, visual/physical occupancy and deliberately
bad input rejection. Native proof then covers four orientations, no collision
in empty cells, diagonal faces, mining from each segment, one drop, support loss,
actuation, real player contact and immunity, rope beside the fang, and reload.
Programmatic engine calls and real movement are labelled separately. MP and
manual placement remain pending; no integrated/polished claim from this fixture.

The image generator's large source is NOT game-ready. Record resampling, color
fitting and authored shape changes separately from the exact mask export. Use
the established explicit-mask exporter; never retain a rendered matte or halo.
Final appearance needs native-size user review before mouth integration.
