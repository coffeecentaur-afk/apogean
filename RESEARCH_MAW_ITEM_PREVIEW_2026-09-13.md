# Maw anatomy item preview: native inventory rendering, no inventory grants

Research only, 2026-09-13 UTC. **Smallest recommended option, inferred/unrun:** one passive `LegacyGameInterfaceLayer` using `InterfaceScaleType.UI`, drawing four separately initialized, unattached `Item` objects through the array overload of `Terraria.UI.ItemSlot.Draw`. Compare `AnatomyItem_rib`, `AnatomyItem_cap`, `ItemID.StoneBlock`, and `ItemID.DirtBlock`. For these four items, context `ItemSlot.Context.InWorld` (31) gives a native slot background and inventory-icon hook dispatch without assigning a gamepad link position. Save/restore `Main.inventoryScale` in `try/finally`; use no input/hover/transfer handlers.

Despite its name, **context 31 still calls `PreDrawInInventory`; it does not call `PreDrawInWorld`.** This would demonstrate native inventory-icon rendering on detached items, not possession, placement, drops, or actual world-item rendering. Nothing was rendered or executed in the game during this pass. Only this note was written, using `apply_patch`; no code, player/world files, game state, UI, builds, installations, or main-task trials were changed. The research skill's primary-source/single-note workflow was used directly; no child-agent capability was available in this task.

## Evidence and version

**Verified:** [installed tModLoader.dll](E:/SteamLibrary/steamapps/common/tModLoader/tModLoader.dll) SHA-256 remains `D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C`, the same v2026.07.3.0 binary and product commit `666f69962d3bdffde54fc14025f02634965b4e7c` pinned in the preceding research. Inspected metadata, IL and decompiled methods using the already installed ICSharpCode.Decompiler 9.1.0.7988 and Mono.Cecil 0.11.6, entirely in memory/stdout. Terraria was not invoked. Some XNA expressions have unresolved-reference annotations; relevant call sites, access modifiers and field stores were checked in IL.

First-party references:

- [Pinned ItemSlot patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/UI/ItemSlot.cs.patch), corroborated by [official ItemSlot API](https://docs.tmodloader.net/docs/stable/class_item_slot.html).
- [Pinned ModItem draw contracts](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModItem.cs#L1100) and [ItemLoader dispatch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ItemLoader.cs#L1846).
- [Pinned ModSystem interface contracts](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModSystem.cs#L233), [Main interface patch](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/Main.cs.patch#L5350), and [LegacyGameInterfaceLayer API](https://docs.tmodloader.net/docs/stable/class_legacy_game_interface_layer.html).
- Existing local implementation: [MawAnatomyMaterials.cs](<C:/Users/max_h/OneDrive/Documents/My Games/Terraria/tModLoader/ModSources/apogean/Content/Diagnostics/MawAnatomyMaterials.cs:79>), including dynamic registration, `IconFrame`, defaults and both draw overrides. This is source inspection, not confirmation of the currently loaded mod artifact.

Binary locators used below:

| Ref | Native methods / metadata tokens |
| --- | --- |
| B1 | `ItemSlot.Draw(ref Item)` `0x06001549`; `Draw(Item[])` `0x0600154A`; `DrawItemIcon` `0x06001550`; `DrawItem_GetColorAndScale` `0x06001551`; `GetGamepadPointForSlot` `0x06001552`; `GetItemLight` item/type overloads `0x06001563/0x06001564` |
| B2 | `ItemLoader.PreDrawInInventory/PostDrawInInventory` `0x06001F13/0x06001F14`; `Item.GetAlpha` `0x060002A8`; `Main.DrawItemIcon` `0x06000558`; `Main.GetItemDrawFrame` `0x06000491` |
| B3 | `Main.SetupDrawInterfaceLayers` `0x0600051A`; `DrawInterface` `0x0600051B`; `DrawInterface_33_MouseText` `0x06000528`; `GameInterfaceLayer.Draw` `0x060014FF`; `LegacyGameInterfaceLayer` constructor/DrawSelf `0x0600158F/0x06001590` |
| B4 | `Main.DrawItem_GetBasics/DrawItem_AnimateSlot/DrawItem/DrawItems` `0x060004DB/0x060004DC/0x060004DD/0x060004DE`; `ItemLoader.PreDrawInWorld/PostDrawInWorld` `0x06001F11/0x06001F12`; `GlobalItem.PostDrawInWorld` `0x06001D37` |
| B5 | `Item(int,int,int)` `0x06000255`; `Item.ResetStats` `0x060002A7`; `ModContent.TryFind` overloads `0x06002108/0x06002109`; `ItemLoader.SetDefaults`; `ItemSlot.Context` and `ItemID` constants |

## Detached items and exact overload semantics — verified

The installed public signatures are:

```csharp
ItemSlot.Draw(SpriteBatch spriteBatch, Item[] inv, int context, int slot,
              Vector2 position, Color lightColor = default(Color));
ItemSlot.Draw(SpriteBatch spriteBatch, ref Item inv, int context,
              Vector2 position, Color lightColor = default(Color));
```

The array overload immediately reads `inv[slot]`; it does not require that array to be the player's inventory. It draws an icon when `type > 0 && stack > 0`; the item's `active` flag is not its icon-drawing gate. It neither inserts that item into a player/world collection nor performs a transfer. Use initialized, nonnull entries and valid indexes. Some contexts inspect additional slots or player state, so arbitrary context/short-array combinations are not interchangeable. B1.

The `ref Item` overload stores the reference in a shared static `singleSlotArray[0]`, calls the array overload at slot zero, then assigns that array element back to the caller's reference. It does not clone the item. A reentrant use of this shared helper could replace the scratch reference. **Prefer an owned four-element array** to avoid that shared scratch and make all four identities explicit. Passing a player inventory reference, the registration template's `Item`, or a `ContentSamples` object directly would not create an independent preview item. B1/B5.

Resolve the dynamic items by full registered names, `apogean/AnatomyItem_rib` and `apogean/AnatomyItem_cap`, through `ModContent.TryFind<ModItem>`; use each result's `Type`. Both are instances of the same dynamically registered C# class, so a single generic class-type lookup is not a selector for rib versus cap. Their registration is gated by `MawAnatomyMaterials.Available`; failed lookup means unavailable, not permission to register content from the drawing callback. Local source and B5.

`new Item(resolvedType, stack: 1, prefix: 0)` initializes a detached object through `SetDefaults`; prefix zero skips the constructor's prefix call. `ItemLoader.SetDefaults` creates the per-item ModItem instance and applies defaults/global defaults. The local class uses `CloneNewInstances=true` and preserves its readonly key. This is object initialization, not `Item.NewItem`, an inventory grant, or an `OnSpawn` workflow. Cache these objects for the bounded preview and discard their references afterward; no refund or inventory cleanup is appropriate. These initialization semantics are **verified**; registration availability and the complete loaded GlobalItem behavior remain **unrun**. B5 and local source.

Use the two vanilla controls as initialized items too: `ItemID.StoneBlock=3` and `ItemID.DirtBlock=2` are verified constants. Stone is particularly useful because both candidate items declare `Texture => "Terraria/Images/Item_3"`; if the inventory override were skipped, a candidate could misleadingly look like the stone control. Native control assets exist at the installed Terraria `Content/Images/Item_3.xnb` and `Item_2.xnb`; their compressed headers were read, but no texture decoding or pixel comparison was performed. Exact loaded texture dimensions remain a runtime observation, not a measured result here.

## Position, origin and scale — verified

`ItemSlot.Draw`'s `position` is the **top-left of the slot background**, in the active SpriteBatch's coordinate space. The native background is drawn there with origin zero and scale `s = Main.inventoryScale`. Let `B` be the chosen background texture's size. The icon helper receives:

```text
icon center = slot top-left + B * s / 2
```

It then passes that center as the `position` argument to `PreDrawInInventory`, together with the current item texture frame, its center as `origin`, computed colors, and the final icon scale. The caller should not pre-center the position passed to `ItemSlot.Draw`; the hook should not subtract the supplied origin from its received position. B1/B2; the pinned ModItem documentation explicitly confirms these meanings.

The default ItemSlot icon size limit is 32 texture pixels. `DrawItem_GetColorAndScale` computes a fit multiplier of 1 when the frame fits, otherwise `32 / max(frame.Width, frame.Height)`. Final scale is `Main.inventoryScale * fitMultiplier * itemLightScale`; the latter comes from native `GetItemLight`, including applicable pulse sets. These calculations use the **item texture frame**, not `Item.width/height` and not the custom tile-atlas frame. B1.

The existing `MawAnatomyItem.PreDrawInInventory` is consistent with this API: it draws its own 16×16 `IconFrame` directly at the received center, with origin `(8,8)`, rotation zero, supplied `drawColor`, and supplied final scale, then returns false. Ignoring the supplied fallback frame/origin is appropriate when substituting a different 16×16 source rectangle. Its expected drawn extent is 16 times final scale in SpriteBatch coordinates. The hook does not draw a secondary `itemColor` overlay. The source's `Item.width=Item.height=16` does not itself determine inventory icon sizing. **Position/origin correctness is statically verified; visual centering and loaded frame/scale values are unrun.** B1/B2 and local source lines 95–105.

A UI layer uses `Main.UIScaleMatrix`, so logical UI coordinates and dimensions are further transformed by the user's UI scale. Setting `Main.inventoryScale=1f` gives a reproducible full-size logical slot comparison, not necessarily one physical display pixel per texture pixel. Use identical `s`, context, `Color.White`, and stack one for all four. For exact slot spacing, derive `B` from the loaded background instead of assuming a hardcoded size.

**Recommended, unrun:** snapshot `Main.inventoryScale` immediately before the row, set the chosen comparison value, and restore the snapshot in a local `finally`, even on missing-asset or draw-hook exceptions. The inspected native Draw method only reads/copies that global; it does not restore a value assigned by its caller. Do not reset it to a guessed standard such as 1 or 0.85 afterward. A same-frame restoration is necessary before later native interface work. Do not also change `Main.UIScale`, mouse coordinates, or the game's view matrices.

## Read-only limits, hover and context choice

**Verified:** neither Draw overload calls `Handle`, `LeftClick`, `RightClick`, `MouseHover`, or `OverrideHover`. The inspected Draw bodies do not assign `Main.mouseItem`, `Main.HoverItem`, `Main.hoverItemName`, a player's `mouseInterface`, or player inventory entries. A passive layer can draw labels without installing hover/input behavior. B1.

However, native drawing is not globally side-effect-free:

| Context / operation | Verified behavior and implication |
| --- | --- |
| `InventoryItem` 0, including an unattached array | Computes a gamepad link equal to `slot`; Draw subsequently calls `UILinkPointNavigator.SetPosition`. This occurs even without gamepad use. Slots below 10 also get hotbar numbering/selection-dependent presentation and inventory glow state can influence appearance. |
| Chest/bank/shop/equipment contexts | Assign corresponding live navigation-link positions; detached array ownership does not prevent it. |
| `CraftingMaterial` 22 | Can consume/reset `DrawGoldBGForCraftingMaterial` and obtain a gamepad link from crafting shortcut globals. |
| `ChatItem` 14 / `MouseItem` 21 | Return no gamepad link; suppress the slot background, but still call the inventory icon path. These are viable icon-only alternatives. |
| **`InWorld` 31, for these four block items** | Returns gamepad link -1; takes the ordinary background branch; avoids inventory glow and hotbar branches; does not consume the crafting flag or enter the context-28 hover-highlight branch. It still dispatches inventory hooks. A Shellphone-specific context-31 texture substitution exists but does not apply to these four types. |
| `ref Item` overload | Writes shared `singleSlotArray[0]` and writes the reference back. Prefer the array overload. |
| Asset/color/draw helpers | `LoadItem`, the candidate's `LoadTiles`, and texture access can populate asset caches. `Item.GetAlpha` calls mod/global color hooks. Pre/Post inventory draw hooks can themselves have arbitrary effects. |

Thus context 31 plus the detached array is the smallest statically supported **player/world-state-preserving** native slot preview for this specific comparison. It is not proof of purity for arbitrary installed hooks. It also does not reproduce a selected hotbar slot's decorations; label it a passive native icon/slot comparison. Do not try to obtain purity by snapshotting and rolling back the entire `Main` state. B1/B2.

Native hook order is GlobalItem Pre hooks, then the item's ModItem Pre hook, combining results without short-circuiting; then vanilla texture rendering only if the aggregate is true; then ModItem Post followed by GlobalItem Post hooks. A false candidate Pre suppresses the fallback texture but does not skip Post. `ItemSlot.DrawItemIcon` calls Pre at `IL_0098` and Post at `IL_01C8`. An exception is different from returning false; Post is not a guaranteed finally. B2 and pinned ItemLoader.

**Do not substitute `Main.DrawItemIcon`.** Its inspected public implementation draws the item texture directly and does not dispatch these inventory Pre/Post hooks, so the candidates would use their stone fallback. `ItemSlot.DrawItemIcon` is the public center-position icon helper that does dispatch them; the recommended row nevertheless uses actual `ItemSlot.Draw` as requested. B1/B2.

## Interface placement

**Recommended, unrun:** insert one `LegacyGameInterfaceLayer` immediately before `"Vanilla: Mouse Text"`, explicitly specifying `InterfaceScaleType.UI`. The constructor defaults to `Game`, so omitting the scale type would use the world zoom matrix. `GameInterfaceLayer.Draw` establishes the appropriate zoom/matrix, begins `Main.spriteBatch`, invokes the callback, and ends the batch. The callback should use that active batch, restore its temporary inventory scale, and return true so later interface layers continue. No nested `Begin/End` is needed. B3 and official layer API.

`PostDrawInterface(SpriteBatch)` also has an already begun UI batch on this binary: `DrawInterface_33_MouseText` calls `SystemLoader.PostDrawInterface` at its start, inside the UI-scaled Mouse Text layer and before native hover text. It is usable as a minimal existing diagnostics hook, but the official contract recommends `ModifyInterfaceLayers` for new work. Neither approach requires changing inventory visibility, consuming clicks, assigning `mouseInterface`, or creating a UIState. All code/setup for either option is outside this research-only pass. B3 and pinned ModSystem/Main sources.

## World-item inspection without adding entities

**Verified native path:** `Main.DrawItems()` iterates the 400 `Main.item` slots and calls `Main.DrawItem(item, index)`. `DrawItem` is **protected**, not a public arbitrary-item preview API; its setup helpers are private. It rejects inactive/air items, resolves world animation/frame, samples world lighting, derives rotation primarily from velocity, applies item-light adjustments, dispatches PreDrawInWorld, conditionally draws vanilla layers, then dispatches PostDrawInWorld. Some item types also advance global per-world-slot animation counters or emit dust/use random state. Calling `DrawItems` as an extra preview pass would redraw live items and is not a detached read-only seam. B4.

Three distinct evidence levels:

| Option | Evidence and limit |
| --- | --- |
| **Passive actual-world observation** | A future narrowly filtered `GlobalItem.PostDrawInWorld(Item, SpriteBatch, Color, Color, float, float, int)` can observe an already-existing rib/cap item during the engine's normal world draw without adding entities. Record identity, real slot, position/size and supplied render parameters, without mutating the item or drawing another copy. No matching existing item means no actual-world evidence. Verify `Main.item[whoAmI]` is the same object for this label; a callback manually invoked elsewhere is not sufficient. |
| **Detached hook simulation** | The public ModItem hook can be invoked on a separately initialized preview object. For this exact inspected override, `whoAmI` is unused; it draws at that object's `Item.Center - Main.screenPosition` using the provided rotation/scale and recomputed `Item.GetAlpha(lightColor)`. Positioning only the detached object can place the simulated sprite on a preview panel. This exercises that hook's drawing logic, not native world enumeration, frame selection, lighting, zoom, drop physics or pickup. |
| **Public loader dispatch simulation** | `ItemLoader.PreDrawInWorld/PostDrawInWorld` accept an arbitrary Item, but only dispatch hooks; they do not supply vanilla rendering between them. They also expose a `whoAmI` that other GlobalItems may treat as a valid world-slot index. No universally safe fake/sentinel index is established. Do not pass an arbitrary live slot or -1 and call this a verified safe native world render. |

The actual-world observer signature is verified in B4 and the [pinned GlobalItem contract](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/GlobalItem.cs). These proposed observations/simulations are **unrun**. No protected-method reflection, fake Main instance, world-slot insertion/swap, Item.NewItem, or inventory drop is needed or recommended. The smallest option for this task remains the inventory row; actual-world evidence should wait for an already-existing eligible entity under the main task's scope.

Native world sprite placement is horizontally centered and bottom-aligned: frame center is `Item.Bottom - Main.screenPosition - (0, frame.Height/2)`. The existing candidate world hook uses `Item.Center`; with its 16×16 item hitbox and 16×16 custom atlas frame, those centers coincide at the ordinary scale-one baseline. It also ignores supplied `alphaColor` and recomputes alpha from `lightColor`, so a successful basic preview would not validate native shimmer fading or all special lighting effects. B4 and local source lines 107–109. None of this establishes an actual rendered drop.

## Limits and handoff

No screenshot, texture decoding, in-game callback trace, loaded registration lookup, scale/hover preservation test, or world draw was performed. The complete loaded GlobalItem set was not audited, and local source may differ from the loaded build. Verified findings cover this binary's APIs/control flow and the current source's coordinate handling. Recommended preview construction, absence of interference during main's trials, and all visual judgments remain **inferred/unrun**. Only this research note is delivered; no native preview code was deployed.
