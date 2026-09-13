# Maw ordinary-rope item-use hook verification

Research only, 2026-09-13 UTC. Read the existing `RESEARCH_MAW_ROPE_INTERACTION_2026-09-13.md` first. **Verdict: the proposed public-hook seam is structurally valid for ordinary rope on the pinned binary.** `SetControls` supplies use input; `PreItemCheck` can replace the target before native item-use and placement validation; `PostItemCheck` can observe and restore it on normal return. One update of use input followed by neutral input permits a single native placement without changing timers or granting items. Keeping subsequent item checks aimed at a separate empty, unsupported parking cell is a useful additional containment measure.

**This is verified static control flow, not a successful gameplay test.** All proposed input sequences, parking behavior in the loaded mod set, consumption observations, and cleanup behavior remain **inferred/unrun**. In particular, the hook pair does **not** provide guaranteed restoration if native item code throws. No implementation, pure scope/envelope code, worlds, players, UI, builds, git operations, or child agents were used or changed. The research skill supplied the primary-source/single-note workflow; the explicit no-agent instruction governed this pass. The only authored file is this note.

## Runtime and primary evidence

- **Verified:** [installed binary](E:/SteamLibrary/steamapps/common/tModLoader/tModLoader.dll), 21,875,712 bytes; SHA-256 `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C`, exactly matching this task's supplied hash.
- Product version: `1.4.4.9+2026.07.3.0|2026.07|stable|Stable|666f69962d3bdffde54fc14025f02634965b4e7c|5250924198908260370`. The [official release](https://github.com/tModLoader/tModLoader/releases/tag/v2026.07.3.0) identifies this version; source links below pin the product commit.
- Binary inspection used already installed ICSharpCode.Decompiler 9.1.0.7988 and bundled Mono.Cecil 0.11.6. Assembly metadata/IL and decompiled method bodies were read in memory/stdout. No Terraria assembly was loaded for gameplay execution, no gameplay method was invoked, and no dependency was installed. Some decompiled XNA/vector expressions have unresolved-reference annotations; the pertinent scalar conditions and call sites were also inspected in IL.
- Official [ModPlayer hook contracts](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModPlayer.cs#L240), [Pre/Post contracts](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModPlayer.cs#L524), [Player hook/timer patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Player.cs.patch#L6933), and [PlayerLoader dispatch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/PlayerLoader.cs#L544) corroborate the binary. The [stable API](https://docs.tmodloader.net/docs/stable/class_mod_player.html) and [official animation wiki](https://github.com/tModLoader/tModLoader/wiki/Player-Item-Animation) are mutable background documentation; installed IL governs exact ordering and timing here.

The B references below identify evidence in that exact DLL. Tokens are metadata tokens, not callable probe APIs.

| Evidence | Methods / metadata tokens |
| --- | --- |
| B1: caller and hook boundaries | `Main.DoUpdateInWorld` `0x06000443`; `Player.Update(int)` `0x06000A8A`; `ItemCheckWrapped` `0x06000AB0`; `ItemCheck` `0x06000B6C`; `PlayerLoader.PreItemCheck/PostItemCheck` `0x060026DE/0x060026DF` |
| B2: native placement | `ItemCheck_Inner` `0x06000B6D`; `ItemCheck_OwnerOnlyCode` `0x06000B70`; `PlaceThing` `0x06000B1E`; `PlaceThing_Tiles` `0x06000B21`; `PlaceThing_Tiles_PlaceIt` `0x06000B29`; `BlockPlacementForAssortedThings` `0x06000B3B`; `CheckLavaBlocking` `0x06000B40`; `CheckRopeUsability` `0x06000B41` (last four abbreviated from `PlaceThing_Tiles_*`) |
| B3: use and timers | `ItemCheck_CheckCanUse` `0x06000BCE`; `ItemCheck_StartActualUse` `0x06000BCA`; `TryAllowingItemReuse` `0x06000BD9`; `CanAutoReuseItem` `0x06000BDA`; `ApplyReuseDelay` `0x06000BD6`; `ApplyItemTime` `0x06000997`; `ApplyItemAnimation` overloads `0x0600099A/0x0600099B`; `SetItemTime` `0x06000996` |
| B4: targeting and input interference | `SmartCursorHelper.SmartCursorLookup` `0x06003AF3`; `Player.SmartSelectLookup` `0x06000A2F`; `ForceForwardCursor/ForceSmartSelectCursor` `0x06000AB1/0x06000AB2`; `UpdatePlacementPreview/FigureOutWhatToPlace` `0x06000B6E/0x06000B6F`; `ItemCheck_ManageRightClickFeatures` `0x06000AAC`; `PlayerInput.ShouldFastUseItem` getter `0x060038DB` |
| B5: ordinary rope defaults | `Item.ResetStats` `0x060002A7`; `SetDefaults1` `0x0600027B`, case 965; `SetDefaults(int,bool,ItemVariant)` `0x060002A5`; `ApplyItemAnimationCompensationsToVanillaItems` `0x06000304`; `TileObjectData.Initialize/CustomPlace` `0x06001A3B/0x06001A3C`; `TileID.Sets` static initializer |

## Exact relevant caller order — verified

On the normal active-player path, with intervening unrelated work omitted:

```text
Main.DoUpdateInWorld -> active player.Update(i)
  -> input handling -> PlayerLoader.SetControls -> ModPlayer.SetControls
  -> later input restrictions / hotbar handling
  -> mouse-derived tileTargetX/Y, bounds adjustment, native target adjustments
  -> SmartCursorHelper.SmartCursorLookup -> SmartInteractLookup
  -> remaining player updates, including movement
  -> ItemCheck_ManageRightClickFeatures (conditional)
  -> ItemCheckWrapped
       -> optional forced cursors -> LockOnHelper.SetUP
       -> ItemCheck
            -> all PlayerLoader.PreItemCheck hooks
            -> ItemCheck_Inner, only if aggregate result is true
            -> all PlayerLoader.PostItemCheck hooks
       -> recipe refresh if stack changed -> LockOnHelper.SetDOWN
       -> unwind optional forced cursors
  -> PlayerFrame / remaining work -> PlayerLoader.PostUpdate
```

B1/B4 IL anchors: `DoUpdateInWorld` calls `Player.Update` at `IL_004F`. Inside `Player.Update`, `SetControls` is `IL_1010`; initial X/Y stores are `IL_1D31/IL_1D4E`; smart-cursor lookup is `IL_2025`; right-click handling is `IL_8578`; `ItemCheckWrapped` is `IL_857F`; `PostUpdate` is `IL_860F`. Inside `ItemCheck`, Pre is `IL_0001`, Inner is `IL_0009`, and Post is `IL_000F`. The two `ItemCheck` call sites in `ItemCheckWrapped` (`IL_0090/IL_0098`) are alternative branches, not two invocations per update.

**Consequences:** setting targets in `SetControls` is too early. Taking the target snapshot inside `PreItemCheck` captures the value after native cursor preparation, including the wrapper's temporary smart-select target. `PreItemCheck` also runs before `UpdatePlacementPreview`, `ItemCheck_CheckCanUse`, and the later actual placement checks. Public metadata confirms `Player.tileTargetX/Y` are static public integers. They are shared state, not per-player storage. B1/B2/B4.

`SetControls` is local-client input control. Pre/Post item hooks also dispatch for other player contexts, so a probe must explicitly gate the local player and its own trial identity. `PlayerLoader.PreItemCheck` evaluates every registered hook using boolean AND; an earlier false does not skip later hooks. Another hook can veto Inner even when this probe returns true. Post still dispatches after that veto, and after a normal early return from Inner. The loaders catch individual Pre/Post hook exceptions; this does not make the surrounding native Inner exception-safe. B1 and pinned contracts above.

## Native validation is downstream — verified

`ItemCheck_Inner` calls `ItemCheck_CheckCanUse` before starting a new animation. This preserves nonempty-item checks, combined mod/player item-use vetoes and `noItems`. An out-of-reach or unsupported rope target can still start an animation: actual tile placement is decided later. `ItemCheck_OwnerOnlyCode` returns for nonlocal players before `PlaceThing`; its local `PlaceThing` call is `IL_0280`. `PlaceThing` calls `PlaceThing_Tiles` at `IL_0043` only when `noBuilding` is false. B2/B3.

`PlaceThing_Tiles` performs these operations in order (B2):

1. Read the selected inventory item and require nonnegative `createTile`; reject targets outside the native reach rectangle.
2. Evaluate target lava, then gamepad-torch, wand, rope-extension and flexible-wand checks; consider tile replacement if enabled.
3. Read the resulting target tile; require allowed occupancy/lava state, `itemTime == 0`, `itemAnimation > 0`, and **`controlUseItem == true`**.
4. Resolve the tile/style, consult `TileLoader.CanPlace`, and perform native custom-object or ordinary adjacency validation.
5. If accepted, call `PlaceThing_Tiles_PlaceIt` once. For ordinary rope it reaches native `WorldGen.PlaceTile` with `forced=false`; success applies normal item time and placement callbacks. Inner subsequently performs native consumption.

For exact reach, let `pX/pY` be player position in pixels, `w/h` the hitbox dimensions, `rX/rY` the native tile ranges, `b` the selected item's `tileBoost`, and `k` the player's `blockRange`. The original target must satisfy all four inclusive inequalities:

```text
pX/16 - rX - b - k <= targetX <= (pX+w)/16 + rX + b - 1 + k
pY/16 - rY - b - k <= targetY <= (pY+h)/16 + rY + b - 2 + k
```

These are floating-point bounds in native code, not a Euclidean-distance test. Pre's target injection does not skip them. It does occur after the earlier target clamp, however: an invalid injected world coordinate can be indexed by placement preview before reach rejection. Absolute world bounds and the required neighbor footprint must already be valid; their pure envelope implementation belongs to the separate main task. B1/B2/B4.

Ordinary rope is item 965, creating tile 213. Native `TileObjectData.Initialize` does not register 213, so its native placement uses the generic adjacency branch. It is also absent from `CanPlaceNextToNonSolidTile`. Accepted support is an active cardinal neighbor that is solid, a beam, rope or track; a wall on the target or any cardinal neighbor also suffices. Diagonal-only support does not suffice. Native occupancy has cuttable/breakable exceptions, so an arbitrary occupied cell is not automatically a safe negative control. B2/B5.

## One placement, native timers and existing inventory

**Verified execution order inside Inner:** after initial item/fast-use handling, decrement positive animation, decrement positive item time, consider smart selection, allow native auto-reuse, apply pending reuse delay, update placement preview, test input/availability and start animation, assign `releaseUseItem = !controlUseItem`, run hold/use styles and local item actions, then consumption. IL uses branches between these blocks; ascending IL offsets alone do not describe execution order. B2/B3; the pinned timer patch independently documents the reordered counters.

Animation start requires use input, release permission, zero animation and a nonzero use style, followed by `ItemCheck_CheckCanUse`. `TryAllowingItemReuse` sets release permission when `CanAutoReuseItem` accepts the item. Rope has native `autoReuse=true`, so maintaining `releaseUseItem=false` is not a reliable brake. Supply the input and let the engine manage release transitions. No probe assignment to `releaseUseItem`, `itemTime`, `itemAnimation`, their maxima, or `reuseDelay` is needed. B3/B5.

Rope defaults are `useTime=8`, `useAnimation=15`, `tileBoost=3`, consumable, nonchanneling, `shoot=0`, and no wall placement. Native animation compensation reduces that animation to 14 because rope is auto-reusable and `noMelee=false`; this happens before `ItemLoader.SetDefaults`, which can change the loaded item. Native animation and use time both incorporate `tileSpeed` and combined hook multipliers. **14/8 is therefore a verified unmodified baseline, not a promise about the loaded mod set.** B3/B5 and inspected `CombinedHooks.TotalUseTime/TotalAnimationTime`.

**Inferred/unrun single-pulse sequence:** with the existing rope selected, observe neutral input until animation, item time and reuse delay are all zero. Revalidate readiness in Pre; supply use for one authorized update aimed at the trial cell; observe tile, stack and effective timers in Post; use neutral input and the parking target for every remaining item-check sample until all timers settle. Keep Pre returning true so native timers advance. Do not hold use until animation ends: its shorter use timer permits another placement during the same animation. B2/B3 establish these predicates, but the sequence has not been executed.

Illustrative unmodified successful trace, inferred from those predicates:

| Sample | Use input / target during Inner | Expected state after Inner |
| --- | --- | --- |
| Pulse | true / trial cell | animation 14, item time 8, release false; one rope created and normally one consumed |
| Next update | false / parking cell | animation 13, item time 7, release true; no additional placement |
| Eight updates after pulse | false / parking cell | animation 6, item time 0; no placement |
| Fourteen updates after pulse | false / parking cell | animation 0, item time 0; eligible to finish if reuse delay is also zero |

There is one native placement call in this ordinary branch per invocation, not a loop placing multiple rope segments. `ApplyItemTime` normally sets current/max time together and increments `ItemUsesThisAnimation`. Later Inner consumes when current time equals its nonzero maximum, the item is consumable, and consumption is allowed. `ItemLoader.ConsumeItem` can veto consumption; `ItemLoader.UseItem` can affect timer application. Accordingly, a qualifying future trial should require both exactly one intended tile change and exactly one stack decrement under the actual loaded hooks. Animation alone or stack delta alone is insufficient. B2/B3.

Honor the main task's inventory policy: use the existing `gg` hotbar rope, never grant or refund it, and restore the selected slot only after timers settle. The existence and contents of `gg` were not inspected in this research. Slot identity, item type/stack and loaded effective timings need observation in the future trial; switching the slot during draining changes the item used by Inner's next inventory lookup. B2 supports that hazard; these trial rules are **inferred/unrun**.

## Parking cell at patch (2,2)

**Verified hazard:** animation and item time keep counting after use input becomes false. However, residual timers alone cannot place ordinary rope: the placement branch independently requires current use input. In contrast, `CheckRopeUsability` runs before the timer/use gate, so aiming at the newly placed rope can still scan down and temporarily change `tileTargetY` even during a neutral drain sample. B2/B3.

**Inferred/unrun conclusion:** keep Pre true and redirect every subsequent sample to the separate preflighted empty, unsupported parking cell at patch-relative `(2,2)`. This is compatible with native timer draining and avoids both the newly existing rope target and the outside-patch cursor during Inner. It provides two native barriers: false use blocks placement, and genuinely unsupported adjacency blocks placement even if a later input path unexpectedly raises use. An empty parking target also prevents the rope-extension scan from entering. B2/B4.

The parking claim requires the resolved absolute cell to remain in-world, empty, dry, and unsupported for ordinary tile 213 at every relevant sample. In particular, neither it nor its four cardinal neighbors may have a wall, and no cardinal neighbor may provide the active solid/beam/rope/track support described above. A fully empty, wall-free cardinal neighborhood is a simple sufficient case. The trial's successful rope placement must not make the parking cell supported. `(2,2)` by itself establishes none of those facts; the main task's separate envelope/preflight must establish them. No duplicate scope or placement code is supplied here.

Restore the saved cursor target after **each** item-check invocation, including parking samples; on the next sample, take a fresh snapshot and inject parking again. Holding parking globally between updates is unnecessary for this seam and would affect native work outside Inner. If the parking preconditions or identity fail, the future run cannot claim containment merely because a previous preflight passed. Arbitrary other-mod changes are outside the static parking proof.

## Smart cursor and other retargeting

- **Verified:** normal `SmartCursorLookup` runs before Pre and can assign both public tile targets plus `Main.SmartCursorX/Y`. No repeat of that lookup appears in the inspected ordinary-rope Inner/placement path. `UpdatePlacementPreview` reads the injected targets and does not replace them. B1/B4.
- **Verified:** `ItemCheckWrapped` may temporarily force a forward mouse cursor or a smart-select cursor before Pre. The latter also saves/replaces tile targets and restores its own snapshot after Post. Saving at Pre and restoring at Post composes with this normal wrapper order. These cursor facilities are not a reason to snapshot earlier in SetControls. B1/B4.
- **Verified:** Inner itself can run `SmartSelectLookup` when animation/reuse delay are zero and either use is false or `selectItemOnNextUse` is true. Smart selection can change the inventory slot, including through `controlTorch`, radial selection or pending `nonTorch` restoration. On a slot change, native code refreshes its held-item reference, sets release permission and zeros item time. For a stable ordinary-rope trial, exclude these states and recheck the held item; do not mistake this native selection change for the intended rope-use trace. B2/B4.
- **Verified:** `PlayerInput.ShouldFastUseItem` can set `controlUseItem=true` inside Inner, after Pre. Neutral input in SetControls alone therefore does not prove neutral input at placement. Exclude active fast-use and alt/right-click features; observe relevant state. The parking cell is additional containment, not evidence those features were absent. Native `delayUseItem` and frozen/webbed/stoned restrictions can instead clear use after SetControls; do not override those restrictions to force a positive result. B1/B2/B4.
- **Verified:** targeting an active rope enters the native downward scan through contiguous rope, platform or track toward an empty cell, subject to world-bottom/lava conditions. `CheckRopeUsability` stores the result in `tileTargetY` at `IL_017D`. Initial native reach is tested **before** this redirection and is not repeated in `PlaceThing_Tiles`. Consequently an extension endpoint can lie beyond the original reach rectangle and a small target patch. Empty trial/parking cells avoid this branch; testing extension requires separate ownership of its full scan path. B2.

These observations concern the unchanged native rope path. Other mods' Pre, HoldItem, UseStyle, placement or consumption hooks can interfere; no loaded-mod ordering or behavior was executed or certified.

## Target restoration and its limits

**Verified:** `Player.ItemCheck` has no exception handlers and no `finally`. Normal completion, a Pre veto, and Inner's normal early return all reach Post. An exception escaping Inner skips Post. `Main.DoUpdateInWorld` catches player-update exceptions and rethrows unless `ignoreErrors` is enabled; neither branch restores these targets. `Player.Update` has a finally for its current-player context, not for this injected target pair. B1/B2.

**Inferred/unrun restoration discipline:** fully validate before assignment; snapshot both target integers in Pre and record ownership of that exact invocation. Restore that snapshot in a local `finally` around this probe's Post observations, so a failed observer does not leak the target. If this probe's Pre throws after assignment, it must restore its own snapshot before the exception escapes: PlayerLoader otherwise swallows the hook exception and can continue Inner. Restore even if another Pre hook vetoed Inner. Restoration must not depend on seeing the injected coordinates unchanged, because native rope extension can legitimately change Y.

Do not treat independent Pre/Post hook lists as an automatically nested resource stack. Another target-writing hook can replace the input or overwrite a later restoration; reentrant ItemCheck calls can also invalidate a single saved pair. A controlled probe needs exclusive target ownership/nonreentrancy and its own recorded status. Once that invocation has ended, do not blindly replay a stale snapshot in a different player/world or a later update whose cursor has already been recomputed. Invalidate stale trial state on context loss; that is not proof of synchronous exception cleanup.

**Concrete limitation:** this public Pre/Post composition can provide normal-return restoration and protect against its own observer exceptions. It cannot guarantee immediate target restoration across arbitrary native/other-mod exceptions without an encompassing boundary that this research neither adds nor authorizes. A missing Post, target-ownership conflict, hook interference, or abnormal update must invalidate the trial; none is a Maw material failure. The proposed parking target does not remove this exception limitation.

## What remains unrun

No supported/unsupported trial, parking drain, out-of-reach attempt, inventory decrement, slot restoration, exception path, or loaded-hook compatibility check was run. No assertion is made about ordinary worlds/players, manual mouse targeting, multiplayer, or safe live deployment. The next authorized implementation can use the verified hook order and the pulse/parking constraints above, while retaining the separate main task's ownership envelope. Positive and negative observations are still required before upgrading this from a statically supported seam to demonstrated native item-use evidence.
