using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace apogean.Content.Diagnostics
{
    // Passive native inventory rendering of detached objects. No item grants,
    // item-use, hover/transfer handlers, world entities or terrain edits.
    public sealed class MawItemPreview : ModSystem
    {
        private Item[] items;
        private long started;
        private int worldId, draws;
        private bool collecting;
        private readonly List<object> hooks = new();
        private object firstFrame;
        private readonly string[] labels = { "Cortical rib", "Full fiber", "Stone control", "Dirt control" };
        private bool Context => Main.netMode == NetmodeID.SinglePlayer && !Main.gameMenu &&
            Main.ActiveWorldFileData?.Name == MawShallowQaScope.World && Main.LocalPlayer.name == "gg" && Main.worldID == worldId;

        internal void Start()
        {
            worldId = Main.worldID;
            if (items != null || !Context || !MawAnatomyMaterials.Available || Main.LocalPlayer.dead)
                throw new InvalidOperationException("Item preview requires idle anatomy-enabled gg/V3/SP.");
            items = new[] { new Item(MawAnatomyMaterials.Item("rib").Type, 1, 0),
                new Item(MawAnatomyMaterials.Item("cap").Type, 1, 0), new Item(ItemID.StoneBlock, 1, 0), new Item(ItemID.DirtBlock, 1, 0) };
            draws = 0; firstFrame = null; hooks.Clear(); started = Stopwatch.GetTimestamp();
            Mod.Logger.Info("MAW ANATOMY ITEM PREVIEW START: four detached items; native ItemSlot context31; UI-scaled,90seconds; no inventory grant or world draw.");
        }

        internal void Observe(MawAnatomyItem item, Vector2 position, Rectangle fallbackFrame, Vector2 fallbackOrigin, float scale)
        {
            if (!collecting || items == null) return;
            int index = Array.FindIndex(items, candidate => ReferenceEquals(candidate, item.Item));
            if (index < 0 || index > 1) return;
            var tile = MawAnatomyMaterials.Tile(index == 0 ? "rib" : "cap");
            Rectangle frame = tile.IconFrame;
            hooks.Add(new { Index = index, ItemType = item.Type, Name = item.FullName,
                CenterX = position.X, CenterY = position.Y, Scale = scale,
                SourceX = frame.X, SourceY = frame.Y, SourceWidth = frame.Width, SourceHeight = frame.Height,
                FallbackWidth = fallbackFrame.Width, FallbackHeight = fallbackFrame.Height,
                FallbackOriginX = fallbackOrigin.X, FallbackOriginY = fallbackOrigin.Y });
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (items == null) return;
            if (!Context || Stopwatch.GetElapsedTime(started).TotalSeconds > 90) { Cancel("context-or-timeout"); return; }
            int anchor = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (anchor < 0) { Cancel("missing-ui-layer"); return; }
            layers.Insert(anchor, new LegacyGameInterfaceLayer("Apogean: Passive Maw item preview", Draw, InterfaceScaleType.UI));
        }

        private bool Draw()
        {
            if (items == null || !Context) return true;
            float originalScale = Main.inventoryScale;
            Item mouseItem = Main.mouseItem, hoverItem = Main.HoverItem;
            int mouseType = mouseItem.type, mouseStack = mouseItem.stack;
            bool inventoryOpen = Main.playerInventory, mouseInterface = Main.LocalPlayer.mouseInterface;
            int selected = Main.LocalPlayer.selectedItem;
            try {
                Main.inventoryScale = 1f;
                collecting = draws == 0;
                if (collecting) hooks.Clear();
                Vector2 origin = new(100, 230);
                var batch = Main.spriteBatch;
                batch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(80, 160, 810, 230), new Color(15, 18, 24, 245));
                Utils.DrawBorderString(batch, "Native inventory icons - QA only", origin + new Vector2(0, -52), Color.White);
                Utils.DrawBorderString(batch, "Detached items; no pickup, placement or world-item proof", origin + new Vector2(0, 116), Color.LightGray, 0.7f);
                for (int i = 0; i < items.Length; i++) {
                    Vector2 position = origin + new Vector2(i * 192, 0);
                    ItemSlot.Draw(batch, items, ItemSlot.Context.InWorld, i, position, Color.White);
                    Utils.DrawBorderString(batch, labels[i], position + new Vector2(0, 64), Color.White, 0.8f);
                }
                if (collecting) {
                    if (hooks.Count != 2) throw new InvalidOperationException("Expected exactly two candidate inventory draw hooks.");
                    firstFrame = new { GameUpdate = Main.GameUpdateCount, UiScale = Main.UIScale,
                        OriginalInventoryScale = originalScale, SlotScale = 1f, Context = 31,
                        SlotTextureWidth = TextureAssets.InventoryBack.Value.Width, SlotTextureHeight = TextureAssets.InventoryBack.Value.Height,
                        SlotLeft = 100, SlotTop = 230, SlotSpacing = 192, Hooks = hooks.ToArray() };
                }
                if (!ReferenceEquals(mouseItem, Main.mouseItem) || !ReferenceEquals(hoverItem, Main.HoverItem) ||
                    Main.mouseItem.type != mouseType || Main.mouseItem.stack != mouseStack || Main.playerInventory != inventoryOpen ||
                    Main.LocalPlayer.mouseInterface != mouseInterface || Main.LocalPlayer.selectedItem != selected)
                    throw new InvalidOperationException("Native preview changed a sampled UI interaction field.");
                draws++;
                if (draws == 1) Mod.Logger.Info("MAW ANATOMY ITEM PREVIEW FRAME: both custom inventory hooks observed; sampled UI interaction fields unchanged; scale restored in finally.");
            } catch (Exception exception) {
                Mod.Logger.Error("MAW ANATOMY ITEM PREVIEW DRAW FAILED", exception);
                Cancel("draw-failure");
            } finally {
                collecting = false;
                Main.inventoryScale = originalScale;
            }
            return true;
        }

        public override void PostUpdateEverything()
        {
            if (items != null && (!Context || Stopwatch.GetElapsedTime(started).TotalSeconds > 90)) Cancel("context-or-timeout");
        }
        internal void Cancel(string reason)
        {
            if (items == null) return;
            items = null; collecting = false;
            var record = new { Schema = 1, Reason = reason, Draws = draws, FirstFrame = firstFrame,
                Scope = "Native ItemSlot.Draw on detached items; sampled UI fields only; no actual world-item, inventory possession or item-use proof." };
            try {
                string path = Path.Combine(Main.SavePath, "Captures", $"maw-item-preview-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}.json");
                using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
                JsonSerializer.Serialize(stream, record, new JsonSerializerOptions { WriteIndented = true });
                Mod.Logger.Info($"MAW ANATOMY ITEM PREVIEW STOP: {reason}; draws={draws}; evidence={path}");
            } catch (Exception exception) { Mod.Logger.Error("MAW ANATOMY ITEM PREVIEW EXPORT FAILED", exception); }
            firstFrame = null; hooks.Clear();
        }
        public override void OnWorldUnload() => Cancel("world-unload");
        public override void Unload() { items = null; firstFrame = null; hooks.Clear(); }
    }
}
