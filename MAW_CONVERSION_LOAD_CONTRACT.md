# Bounded native conversion throughput study

This is QA only: no spread rate, new infection mapping or production update hook.
Packed gg/V3/single-player, no active performance recorder or player action.
Two explicit square sizes16 and32; fixed32-cell batch partitions and three
cycles of source→Maw→Wastes→vanilla. At most1024 cells per patch and three
conversion phases per cycle. Dirt/stone/ice/mud and their unsafe natural walls
use real registered TileLoader/WallLoader conversion paths. No sand/liquid,
living trees, chests or furniture are introduced.

Choose an initially empty guarded envelope from a finite candidate list, outside
all recorded study bounds, StructureMap reservations, actors and world margins.
Snapshot value-type tile state, including air flags/frames, for the owned envelope.
No old exhibit is rebuilt/rebaselined. Only that disposable envelope can be
restored. Check history unchanged and exact owned state restoration even on error.

The probe is synchronous, with a2-second soft work deadline checked before each
batch; one native batch cannot be preempted. It cannot benchmark per-update pacing,
real-world spread, GPU, fluid simulation or multiplayer. Timing arrays record
actual conversion-only batch cost; setup, assertions, restoration and serialization
are outside those intervals and the full duration is reported separately. No
screenshots or ordinary frame sample may run concurrently. Stop on the first
failed property or deadline; preserve failures and partial measurements.

Each phase checks expected tile+wall mapping and retained paint, wires, coatings,
actuator, slope and half-block state. Source patterns include flat/all4 slopes/
half-blocks and mixed adjacent materials. An intentionally unconverted cell must
fail the stage checker before the actual conversion runs. Constructed brick/wood
and corporate material are no-op controls. This does not approve those art sets.

Export raw bounded timings and before/after fingerprints, not an invented max
world size or memory minimum. New scheduling/performance fixes require measured
evidence. Growth policy remains a different test from biome conversion.
