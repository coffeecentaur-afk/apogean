# Maw hanging fiber: bounded native lifecycle QA contract

Date: 2026-09-11. Follow-up to `RESEARCH_MAW_FIBER_BLENDING_2026-09-11.md`; no broad survey repeated. This is a proposed QA fixture contract, not production ecology or an implementation. Climbing remains an unresolved user choice and is **off** in this fixture.

Subsequent implementation note: the main pass deliberately chose a smaller
six-section, seven-root art study, not the proposed ten-section production
contract below. Its212 scratch lifecycle checks pass, but one saved strand
changed and remains RED. See `Art/Validation/MawAnatomy-2026-09-11/README.md`
for current evidence. The "unrun" matrix below records the research-stage
contract; it must not override the scoped later results or imply multiplayer
acceptance.

## Evidence and limits

**Verified:** official tag `v2026.07.3.0` resolves to `666f69962d3bdffde54fc14025f02634965b4e7c`. The installed `E:/SteamLibrary/steamapps/common/tModLoader/tModLoader.dll` independently reports that version/commit; SHA-256 is `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C`. [Tag lookup][tag], [official release][release].

Public patches omit unchanged Terraria bodies. To close specific gaps, this pass inspected that first-party DLL's IL using its already-shipped `Libraries/mono.cecil/0.11.6/lib/netstandard2.0/Mono.Cecil.dll`, without executing Terraria. Method/IL locations below identify that evidence; the release link identifies the binary version, not a public full-source listing. Stable docs are mutable; pinned source and matching IL take precedence. **Verified** means inspected, **inference** means derived, and **contract** means proposed acceptance behavior. All gameplay/network tests remain **unrun**.

## Subsequent cut investigation: Solar gear versus capture

The saved study lost the lower three segments of one strand, after an earlier
capture showed all six. This is an unresolved event, not permission to change
the baseline. The following bounded follow-up inspected the same installed
DLL and its pinned patches; it did not reproduce the original event.

Solar retaliation can cut `tileCut` tiles. The hurt path requires Solar armor
and a remaining shield; the dash path additionally needs the owning player's
active Solar dash to contact an eligible hostile NPC. Both can create and
immediately kill SolarCounter (608). Its native 160×160 damage rectangle can
reach `Damage → CutTiles → CutTilesAt → WorldGen.KillTile`. Tile-collision being
disabled on that projectile does not disable cutting. Mod hooks can veto it.
See the [pinned hurt branch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Player.cs.patch#L5981),
[dash branch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Player.cs.patch#L4190),
and [cutting hooks](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Projectile.cs.patch#L1261).

Unchanged method evidence: `OnHurt_Part2` IL0007–00b2,
`DashMovement`0287–04df, `SetDefaults2`1859–18a7, `Kill`5bfc–5c0b,
`Damage`0711–0744 and `CutTilesAt`00aa–011a. The rectangle scan has no separate
circular-distance or line-of-sight test. Mere shield proximity is not a trigger.

No native capture-to-Solar attack path was found. Capture pauses normal update
while active (`Main.DoUpdate`0167–0173), but its draw path can frame an unframed
loaded section (`DrawCapture`04bc; `SectionTileFrameWithCheck`0023–0047).
Framing may clean unsupported vines; `tileCut` alone does not cause that.
One cut at depth4 followed by ordinary suffix cleanup fits the observed shape,
but a Solar rectangle overlapping the upper prefix should cut it too. We need
the exact player location and trigger/callsite, not an inference from equipment.
Passive saved-root cut logging is installed; deliberate scratch cuts are
excluded. Keep the old specimen RED until a recorded event explains it.

## What ExampleVine actually supplies

**Verified:** growth requires source `HasUnactuatedTile`, `j < worldSurface - 1`, `GrowMoreVines`, a successful 1/70 anchor or 1/7 tip roll, and an unoccupied destination whose `LiquidType` is not lava. Its upward search examines `j` through `j-9`. It rejects encountered bottom slopes, but neither requires a continuous vine path nor checks the distant anchor's actuation. Half blocks are not excluded. It copies paint/coating from the immediate parent, reframes, and broadcasts the new cell on the server. [Pinned ExampleVine, growth][example].

**Inference:** intact strands grown by that loop reach **10 segments**, excluding the anchor: a nine-segment tip can create segment ten; a ten-segment tip cannot find the root within its search. This is a growth limit, not cleanup of manually placed overlength strands. Its surface restriction and chance values are example policy, not engine requirements. [ExampleVine][example].

**Verified caveat:** `GrowMoreVines` is not simply “fewer than 60 vines.” Matching IL scans `x-4..x+4`, `y-6..y+10`, requires a 30-tile world margin, and rejects a **weighted score greater than 60**. Vines below the source with a clear `CanHitLine` path add distance weights; mushroom vines have special weighting. Thus it is neither a length cap nor a fixed fixture population cap. This qualifies the abbreviated public documentation. (`WorldGen.GrowMoreVines`, IL `0000–0103`; [public helper documentation][world].)

## Support loss and middle cuts

**Verified:** `TileFrame` runs when a tile is placed or neighboring tiles change. `TileLoader` calls the tile hook and then global hooks; their combined false result skips subsequent default framing. The hooks precede the default `noBreak`/frame-important early return. Returning false does **not** itself destroy anything, and `noBreak` does not prevent a hook from explicitly calling `KillTile`. [ModTile][modtile], [TileLoader][loader], [WorldGen patch][world].

**Verified native chain mechanism:** successful ordinary `KillTile` clears the cell before calling `SquareTileFrame`. That frames the surrounding 3×3, including the cell immediately below. A hanging tile whose upper support is now absent destroys itself during framing; its own kill frames the next cell. This propagates downward synchronously through an ordinary short intact chain, without waiting for `RandomUpdate`. A middle cut retains the supported upper prefix and removes the cut cell and detached suffix. Evidence: `KillTile` IL `0b78–0c5a`; `SquareTileFrame` IL `0000–0069`; default `IsVine` validation in `TileFrame` IL `7fc8–81dd`. [KillTile contract and vine framing context][world], [matching release][release].

**Verified:** native vine validation treats missing/actuated/bottom-sloped upper support as invalid. It also converts vines for recognized vanilla supports; custom anchor eligibility still needs a hook. ExampleVine adds conversion/fallback behavior rather than a bespoke whole-column deletion loop. [WorldGen][world], [ExampleVine][example].

**Contract:** use a cuttable 1×1 foreground tile: `tileCut` and `IsVine` on; `tileSolid`, `tileSolidTop`, and `tileRope` off; no damage/debuff behavior or item reward. Validate only this QA fiber against its own fiber type and `MawGrass`/`MawDirt`. An unrelated solid block, wall, bone, tooth, other vine, or air is not support. Kill invalid cells through `WorldGen.KillTile`; return false afterward to avoid continuing default framing against the removed cell. On valid cells, retain native framing unless the main implementation deliberately owns static frame selection. Do not adopt ExampleVine's global conversion of arbitrary other vines or its vanilla-vine fallback.

**Verified triggers:** native `SlopeTile` and `PoundTile` reframe neighbors; successful actuator `DeActive`/`ReActive` also reframe and send the anchor cell. Direct field assignments need an explicit framing call. Frame processing can be skipped during generation and near world edges, so the fixture must execute after generation and inside safe bounds. (IL: `SlopeTile 006c`, `PoundTile 0080`, `Wiring.DeActive 01e5–01fe`, `ReActive 0010–0029`, `TileFrame 0002–0065`; [release][release].)

## Shape, actuation, liquids, and appearance

**Verified:** `BottomSlope` means `SlopeUpLeft`/`SlopeUpRight`; `TopSlope` means `SlopeDownLeft`/`SlopeDownRight`, whose underside is flat. `IsHalfBlock` is separate. An actuated tile still has `HasTile`; `HasUnactuatedTile` additionally rejects actuation. An actuated occupant is therefore not an empty destination. Liquid type alone is insufficient to establish wetness: check `LiquidAmount`. [Pinned Tile API][tile].

**Contract:** permit full, half-block, and top-sloped Maw anchors with a flat underside; reject both bottom slopes. Fiber segments themselves remain unsloped and unactuated. Before growth, verify a continuous eligible column to the root, not just the presence of a root somewhere above. For this QA pass choose **dry-only growth** (`LiquidAmount == 0` for parent and destination); that is deliberately stricter than ExampleVine. Zero liquid with stale lava-type bits counts as dry.

**Verified:** native liquid processing uses `CheckLavaDeath` for lava and `CheckWaterDeath` for non-lava liquid, with `TileObjectData` overriding the `Main` flags when present. It kills through `KillTile` and sends the destruction on the server. (`Liquid.AddWater` IL `0046–004e`, `0123–0169`; [death flags][main].) **Contract:** lava death on, water death off, with no conflicting object-data override. Existing fibers survive water/honey/shimmer contact without further wet growth; lava removes affected segments and detached descendants. Confirm through actual liquid updates, not merely assigning a liquid-type field. Player effects of the liquid itself are outside the fiber's harmlessness assertion.

**Verified:** rope behavior uses `Main.tileRope`, separately from `IsVine` and `VineThreads`. In this binary, ordinary vine IDs `52, 62, 115, 205, 382, 528, 636, 638` are absent from vanilla rope initialization. `VineRope` is the distinct tile `353`. `Player.FindPulley` and `CanMoveForwardOnRope` consult the rope array. (`Main.Initialize_TileAndNPCData1_Part2` IL `03fe–0469`; [rope field][main], [vine sets][sets], [release][release].) **Contract:** leave rope registration off; do not assume hanging vines are climbable. Enabling it later needs separate traversal and rope-framing tests.

**Verified:** `VineThreads` plus root registration in `PreDraw` selects wind/player-interaction drawing. For rigid fibers, omit that set/registration and allow ordinary drawing (`PreDraw` defaults true). Normal `DrawSingleTile` obtains the painted texture, applies the light/fullbright override, and checks visibility. Paint/coating copying affects foreground data only; existing descendants are not automatically repainted when an ancestor changes. `PostDraw` still executes for echo-hidden tiles, so any later overlay must check `TileDrawing.IsVisible`. [Sets][sets], [draw hooks][block], [paint copying][tilepatch], [renderer][draw]. Matching IL: `DrawSingleTile 00e6, 0538, 05a8`; `GetTileDrawTexture 010c–0126`.

## Authority and explicit bounds

**Verified:** normal world growth runs in single player or on the server. `Main.DoUpdateInWorld` skips `WorldGen.UpdateWorld` for `netMode == 1` (IL `0594–05b7`); both underground and overground update paths dispatch `TileLoader.RandomUpdate`. The loader itself checks occupancy, not network authority, so directly invoking it from a QA shortcut does not gain a client guard. [World update dispatch][world], [loader][loader], [world-hook documentation][system], [release][release].

**Contract:** fixture growth and forced attempts execute only in single player/server. Use the same eligibility path for natural and forced attempts; forcing may bypass the probability roll only. Restrict root initiation and tip extension to **at most eight explicitly registered QA roots** in the fixture, with **L = 10** segments per root: at most **80 fiber cells**. Add at most one downward cell per successful attempt. No growth elsewhere or when QA is disabled; no production spreading or worldgen seeding. Support maintenance remains active when growth is disabled. Reject overlength/orphaned cells during fixture validation as well as blocking new growth. Keep the root, scan, destination, and framing neighborhood in bounds; use a 30-tile world margin for this fixture.

**Verified:** `SendTileSquare(-1, i, j)` synchronizes **one tile**. The rectangular overload takes top-left coordinates plus width/height; it is not centered. Manual tile mutations require synchronization. [Pinned NetMessage overloads][net]. **Contract:** after growth, frame then sync the new cell. For QA-triggered removals/conversions, sync the authoritative changed span after cleanup, including all removed descendants and changed support. Do not assume one root packet is a whole-chain snapshot or add unconditional broadcasts to every client-side `TileFrame`. Ordinary player cuts use the native interaction path; test its convergence independently.

## Acceptance matrix — all unrun

Each row is a required result for the proposed fixture, not a claim that current Apogean code already passes.

| Case | Required result |
|---|---|
| Enable/disable and bounds | Growth only at registered roots while enabled. Repeated forced attempts stop at 10 per strand and 80 total; an eleventh injected segment is rejected during validation. |
| Anchor matrix | Test both Maw types as full, half, each top slope, and each bottom slope. Only flat undersides support fibers. A background wall alone never anchors one. |
| Obstructed/invalid path | Occupied or actuated destination blocks growth. Air gap, actuated intermediate segment, or foreign tile cannot be skipped to find a distant root. |
| Cut positions | Cut segment 1, a middle segment, and segment 10. Only the detached suffix disappears; upper prefix remains. No second cut or random tick needed. |
| Root loss/shape/actuation | Actual root removal, invalid conversion, bottom-sloping, or successful actuation clears the strand after normal framing. Reactivation never resurrects deleted cells. |
| Two allowed substrates | Direct MawGrass↔MawDirt conversion plus framing retains support. Separately test block swap: if `ReplaceTileBreakDown` is enabled as in the example, native replacement intentionally breaks the vine below, even for an otherwise allowed replacement. [Set semantics][sets] |
| Liquids | Amounts 1 and 255 of water/honey/lava/shimmer block new wet growth. Existing non-lava fibers survive; lava cuts/cleans the suffix. Zero amount with stale liquid type remains eligible. |
| Harmless/traversable | Player/NPC movement and projectiles encounter no solid/platform collision, tile damage, debuff, or rope grab. Cuts yield no fixture items, bait, or optional cordage reward. |
| Rigid rendering | No wind or player-contact sway; one sprite per segment. Verify root/tip seams, normal/deep/negative paint, Illuminant Coating, and Echo Coating with Echo Sight off/on. Parent coating propagates only to newly grown cells. |
| Persistence/network | Save/reload preserves valid length and coatings. Dedicated server plus two clients agree after growth, middle cut, conversion, actuation, and lava; late join has no detached/duplicate cells. Repeat with clients using different echo visibility. |

Remaining unknowns are live QA outcomes, final artwork/frame selection, and whether the user eventually wants climbing. No runtime/tool files, builds, UI, commits, pushes, dependencies, or agents were created/changed/run by this sidecar. The sole authored file is this note.

[tag]: https://api.github.com/repos/tModLoader/tModLoader/commits/v2026.07.3.0
[release]: https://github.com/tModLoader/tModLoader/releases/tag/v2026.07.3.0
[example]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/ExampleMod/Content/Tiles/Plants/ExampleVine.cs
[world]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/WorldGen.cs.patch
[modtile]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModTile.cs
[loader]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/TileLoader.cs
[tile]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Tile.TML.cs
[tilepatch]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Tile.cs.patch
[main]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch
[sets]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ID/TileID.cs.patch
[block]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModBlockType.cs
[draw]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/GameContent/Drawing/TileDrawing.cs.patch
[system]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs
[net]: https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/NetMessage.cs.patch
