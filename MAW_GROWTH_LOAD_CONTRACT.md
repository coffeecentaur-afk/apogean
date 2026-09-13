# Bounded per-update Maw fiber measurement

September13, QA only. Reuse the existing four-face `MawFiberGrowthPolicy`, actual
Maw soil/turf and native `SquareTileFrame`. No infection frontier, production
timer, generation, vines or approved texture changes.

An idle gg/V3/single-player explicit request reserves dry empty32x32 scratch with
12-tile empty halo, excluding historical/protected structures and actors. Four
11x11 soil islands each start with one grass seed. Only their exposed40-cell
perimeters may mature; interiors, a separate soil cell and bone control remain.
Four corners carry native slopes and one side carries a half block. Retain paint,
wires and coatings, and validate native properties separately from framing.

Each native `PostUpdateWorld` may run ONE plan with a1/8/32 conversion budget;
disabled control uses0. Read a fixed1024-cell snapshot, plan without cascading,
apply then frame only selected cells. Stop after240 distinct game updates or
eight seconds. Expected enabled endpoint160 grass from4seeds,156conversions;
disabled stays4. Idle and active work timings are separate. This is a fixed
small workload, not a full-world scheduler or minimum hardware certification.

Second explicit layout (September13): `maw-growth-load-saturated-{0|1|8|32}`
keeps exactly the same soil, structural control, shapes and ownership. Seed the
midpoint of all four sides of each island, giving16 initial grass cells. The
first four32-budget updates must each convert32 cells, then16 on update5,
ending at160 total after144 conversions. Remaining updates are idle; this is
four full-budget updates, NOT240 saturated updates or whole-world stress.
Original four-seed requests/layout/evidence remain unchanged. Schema2 names
the layout and independently validates its exact expected change sequence;
schema1 retains its original narrower evidence. No production policy change.

Record game-update IDs, actual changes/grass count, read/plan/apply/frame time,
thread allocation and separate validation time for EVERY update. Setup, guard
checks, serialization and restoration are outside work timing. Log aggregate
statistics from raw samples; independent replay rejects missing/duplicate ticks,
over-budget work, false endpoint, contradictory hashes and optimistic summaries.
Run matched1/8/32/0 cases, repeating an arm before claiming stable timing.

No player relocation, item grants or stat changes. Refuse overlapping motion/
performance samples; stop on another request, context loss, pause/inactive window,
death, newly nearby actors, capture or timeout. Restore exact owned native cells
before export even on partial failure, preserving all known history. Empty/missing
history remains unverified. Do not use a successful report as art approval.

No production adoption until correctness, per-update cost and wider scale have
separate evidence. A single synchronous operation can still overrun a time budget;
record it rather than claiming hard real-time guarantees from a cell cap.
