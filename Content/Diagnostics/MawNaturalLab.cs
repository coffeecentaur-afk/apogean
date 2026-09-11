using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace apogean.Content.Diagnostics
{
    // A bounded renderer regression, NOT a world-generation template. Existing
    // terrain/galleries are never cleared, repaired, moved or rebaselined here.
    public sealed class MawNaturalLab : ModSystem
    {
        private const int Width = MawNaturalLayout.Width, Height = MawNaturalLayout.Height;
        private Rectangle bounds;
        private string checkpoint;
        private bool viewing, oldDay, oldRain, oldEclipse;
        private double oldTime;
        private Vector2 oldPosition;
        private int panel, captureDelay = -1;
        private static bool IsQa => Main.netMode == NetmodeID.SinglePlayer &&
            Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && Main.LocalPlayer.name == "gg";
        private Point At(int x, int y) => new(bounds.X + x, bounds.Y + y);
        private static int Material(string key) => MawTerrainStudies.Tile(key).Type;
        private static int ResolveTile(string key) => key switch {
            "vanilla-grass" => TileID.Grass, "vanilla-dirt" => TileID.Dirt,
            "vanilla-brick" => TileID.GrayBrick, _ => Material(key)
        };

        internal void Run(string request)
        {
            if (!IsQa) throw new InvalidOperationException("Natural lab requires gg/V3/single-player.");
            string grove = ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot();
            try {
                switch (request) {
                    case "build": Build(); break;
                    case "test": Test(); break;
                    case "reload": Test(); break;
                    case "negative": RejectMutations(); break;
                    case "properties": Properties(); break;
                    case "natural": panel = 0; View(false); break;
                    case "corners": panel = 1; View(false); break;
                    case "night": View(true); break;
                    case "capture": RequireFixture(); captureDelay = 60; break;
                    case "release": Release(); break;
                    default: throw new InvalidOperationException("Unknown natural-lab request.");
                }
            } finally {
                if (grove != ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot())
                    throw new InvalidOperationException("Natural lab changed the preserved grove.");
                Mod.Logger.Info("MAW NATURAL GROVE GUARD: unchanged; old reload failure is still separate/RED.");
            }
        }

        private static bool Empty(Rectangle area)
        {
            for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
                Tile t = Main.tile[x, y];
                if (t.HasTile || t.WallType != WallID.None || t.LiquidAmount != 0 || t.HasActuator ||
                    t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire) return false;
            }
            return true;
        }

        private void Set(int x, int y, int type)
        {
            Point p = At(x, y);
            Tile t = Main.tile[p];
            if (t.HasTile) throw new InvalidOperationException($"Refusing occupied fixture cell {p}.");
            // These are inert study cells and native dirt/grass controls. Explicit
            // state avoids random plant placement and native grass growth RNG.
            t.HasTile = true; t.TileType = (ushort)type;
            t.TileFrameX = t.TileFrameY = 0; t.Slope = SlopeType.Solid; t.IsHalfBlock = false;
        }

        private void Build()
        {
            if (!bounds.IsEmpty || checkpoint != null) throw new InvalidOperationException("Saved fixture exists; refusing rebuild.");
            Rectangle grove = ModContent.GetInstance<VegetationVisualLab>().PreservedBounds; grove.Inflate(16, 16);
            foreach (int dx in new[] { 2080, -2240, 2320, -2480, 2560, -2720 }) {
                foreach (int dy in new[] { -180, -240, -300 }) {
                    Rectangle site = new(Main.spawnTileX + dx, Main.spawnTileY + dy, Width, Height);
                    Rectangle envelope = site; envelope.Inflate(4, 4);
                    if (envelope.Left < 40 || envelope.Top < 40 || envelope.Right > Main.maxTilesX - 40 ||
                        envelope.Bottom > Main.maxTilesY - 40 || envelope.Intersects(grove) || !Empty(envelope)) continue;
                    bounds = site; break;
                }
                if (!bounds.IsEmpty) break;
            }
            if (bounds.IsEmpty) throw new InvalidOperationException("No empty natural-lab envelope; nothing cleared.");

            var plan = MawNaturalLayout.Create();
            for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) {
                var cell = plan[x, y];
                if (cell.Tile != null) {
                    Set(x, y, ResolveTile(cell.Tile)); Tile t = Main.tile[At(x, y)];
                    t.Slope = (SlopeType)cell.Slope; t.IsHalfBlock = cell.Half;
                }
                if (cell.Wall != null) Main.tile[At(x, y)].WallType = (ushort)MawTerrainStudies.Wall(cell.Wall).Type;
            }
            WorldGen.RangeFrame(bounds.Left - 1, bounds.Top - 1, bounds.Right + 1, bounds.Bottom + 1);
            for (int x = bounds.Left; x < bounds.Right; x++) for (int y = bounds.Top; y < bounds.Bottom; y++) WorldGen.SquareWallFrame(x, y);
            checkpoint = Fingerprint();
            Mod.Logger.Info($"MAW NATURAL BUILD: {bounds}; empty envelope; checkpoint={checkpoint}");
            panel = 0; View(false);
        }

        private void RequireFixture()
        {
            if (bounds.Width != Width || bounds.Height != Height || bounds.Left < 40 || bounds.Top < 40 ||
                bounds.Right > Main.maxTilesX - 40 || bounds.Bottom > Main.maxTilesY - 40 || string.IsNullOrEmpty(checkpoint))
                throw new InvalidOperationException("Missing/invalid natural fixture.");
        }

        private void Test()
        {
            RequireFixture();
            string before = Fingerprint();
            var plan = MawNaturalLayout.Create();
            // Reconstruct the original creation digest from the original layout,
            // not today's world. Never replace a failed saved digest with today's.
            if (Fingerprint(plan) != checkpoint) throw new InvalidOperationException($"Original-layout contract changed: saved={checkpoint}; reconstructed={Fingerprint(plan)}. No rebaseline.");
            int dynamicControlCells = ValidateLayout(plan);
            int draws = 0, walls = 0, pixels = 0;
            var seen = new HashSet<string>();
            var cache = new Dictionary<Texture2D, Color[]>();
            Color[] Data(Texture2D texture) {
                if (!cache.TryGetValue(texture, out var data)) { data = new Color[texture.Width * texture.Height]; texture.GetData(data); cache.Add(texture, data); }
                return data;
            }
            for (int x = bounds.Left; x < bounds.Right; x++) for (int y = bounds.Top; y < bounds.Bottom; y++) {
                Tile t = Main.tile[x, y];
                if (t.HasTile && TileLoader.GetTile(t.TileType) is MawTerrainStudyTile tile) {
                    seen.Add(tile.Name);
                    short fx = t.TileFrameX, fy = t.TileFrameY; int width = 16, height = 16, offset = 0;
                    if (!tile.Map.TryMap(x, y, fx, fy, out short expectedX, out short expectedY)) throw new InvalidOperationException($"Unmapped {tile.Name} {fx},{fy}");
                    TileLoader.SetDrawPositions(x, y, ref width, ref offset, ref height, ref fx, ref fy);
                    if (fx != expectedX || fy != expectedY || width != 16 || height != 16 || offset != 0) throw new InvalidOperationException("Actual draw geometry mismatch");
                    Texture2D texture = TextureAssets.Tile[t.TileType].Value;
                    if (texture.Width != tile.Map.Width || texture.Height != tile.Map.Height) throw new InvalidOperationException("Loaded atlas dimensions mismatch");
                    int nativeType = tile.Name switch { "Study_soil" => TileID.Dirt, "Study_grass" => TileID.Grass, "Study_sand" => TileID.Sand,
                        "Study_mud" => TileID.Mud, "Study_snow" => TileID.SnowBlock, "Study_ice" => TileID.IceBlock, _ => TileID.Stone };
                    Main.instance.LoadTiles(nativeType);
                    Texture2D native = TextureAssets.Tile[nativeType].Value;
                    Color[] actual = Data(texture), reference = Data(native);
                    for (int py = 0; py < 16; py++) for (int px = 0; px < 16; px++) {
                        Color a = actual[(fy + py) * texture.Width + fx + px];
                        Color b = reference[(t.TileFrameY + py) * native.Width + t.TileFrameX + px];
                        if (a.A != b.A || (a.A != 0 && ((a.R > 245 && a.G > 245 && a.B > 245) || (a.R > 245 && a.B > 245 && a.G < 10))))
                            throw new InvalidOperationException($"NATIVE_MASK_OR_EXPORT_KEY {tile.Name} at {x},{y} pixel {px},{py} native {t.TileFrameX},{t.TileFrameY}");
                        pixels++;
                    }
                    if (!Main.tileSolid[t.TileType] || Main.tileLighted[t.TileType]) throw new InvalidOperationException("Art study unexpectedly lost solidity or gained emission");
                    draws++;
                }
                if (WallLoader.GetWall(t.WallType) is MawTerrainStudyWall wall) {
                    if (!wall.Map.TryMap(x, y, t.WallFrameX, t.WallFrameY, out _, out _)) throw new InvalidOperationException("Unmapped wall frame");
                    walls++;
                }
            }
            for (int index = 0; index < 10; index++) {
                int x = 5 + index * 12;
                for (int dx = 0; dx < 7; dx++) for (int dy = -2; dy < 5; dy++) {
                    if (plan[x + dx, 66 + dy].Tile == null) continue; // Native control plants may occupy originally empty air.
                    Tile vanilla = Main.tile[At(x + dx, 66 + dy)], maw = Main.tile[At(x + dx, 85 + dy)];
                    if (vanilla.HasTile != maw.HasTile || vanilla.Slope != maw.Slope || vanilla.IsHalfBlock != maw.IsHalfBlock)
                        throw new InvalidOperationException($"Paired corner geometry drift in case {index}");
                }
            }
            if (seen.Count != 12 || draws < 2000 || walls < 500) throw new InvalidOperationException("Natural fixture incomplete");
            if (Fingerprint() != before) throw new InvalidOperationException("Read-only test changed fixture");
            Mod.Logger.Info($"MAW NATURAL MATRIX PASS: {draws} actual tile draws; {pixels} loaded alpha/key pixels; {walls} wall frames; 10 paired mixed-substrate cases; 12 materials; {dynamicControlCells} vanilla-only biological control changes; original checkpoint={checkpoint}. Visual contact/repetition still requires screenshots. No gameplay promotion.");
            cache.Clear();
        }

        private int ValidateLayout(MawNaturalLayout.Cell[,] plan)
        {
            int biological = 0, unexpected = 0;
            for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) {
                var cell = plan[x, y]; Tile t = Main.tile[At(x, y)];
                int expected = cell.Tile == null ? -1 : ResolveTile(cell.Tile);
                int actual = t.HasTile ? t.TileType : -1;
                bool controlGrowth = MawNaturalLayout.IsControlBank(x, y) &&
                    ((expected == TileID.Dirt && actual == TileID.Grass) ||
                     (expected == -1 && actual is TileID.Plants or TileID.Plants2 or TileID.Vines));
                bool bad = (actual != expected && !controlGrowth) || (byte)t.Slope != cell.Slope || t.IsHalfBlock != cell.Half ||
                    t.TileColor != PaintID.None || t.IsActuated || t.IsTileInvisible || t.IsTileFullbright ||
                    t.WallType != (cell.Wall == null ? WallID.None : MawTerrainStudies.Wall(cell.Wall).Type) ||
                    t.WallColor != PaintID.None || t.IsWallInvisible || t.IsWallFullbright || t.LiquidAmount != 0 ||
                    t.HasActuator || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire;
                if (bad) { if (unexpected++ < 20) Mod.Logger.Error($"MAW NATURAL CELL DIFF: {At(x,y)} expected={cell.Tile ?? "empty"},slope{cell.Slope},half{cell.Half}; actual={actual},slope{(byte)t.Slope},half{t.IsHalfBlock}; wall={t.WallType}"); }
                else if (controlGrowth) biological++;
            }
            if (unexpected > 0) throw new InvalidOperationException($"Natural layout has {unexpected} non-biological mismatches. No repair attempted.");
            Mod.Logger.Info($"MAW NATURAL LAYOUT: all authored cells match original plan; {biological} ordinary vanilla control growth cells. Maw cells remain exact.");
            return biological;
        }

        private void RejectMutations()
        {
            RequireFixture(); var plan = MawNaturalLayout.Create();
            if (Fingerprint(plan) != checkpoint) throw new InvalidOperationException("Original plan changed; controls refused.");
            ValidateLayout(plan);
            string before = Fingerprint(); int checks = 0;
            void Check(string name, Point p, Action<Tile> mutate, Action<Tile> restore, bool shouldPass = false) {
                bool rejected = false;
                try {
                    mutate(Main.tile[p]);
                    try { ValidateLayout(plan); } catch (InvalidOperationException) { rejected = true; }
                } finally { restore(Main.tile[p]); }
                if (Fingerprint() != before) throw new InvalidOperationException("Mutation control failed to restore exact state");
                if (rejected == shouldPass) throw new InvalidOperationException("Validator mutation survived or positive rejected: " + name);
                checks++; Mod.Logger.Info("MAW NATURAL EXPECTED CONTROL PASS: " + name);
            }
            Point soil = At(7, 87); bool oldHas = Main.tile[soil].HasTile; ushort oldType = Main.tile[soil].TileType;
            Check("missing-maw-soil", soil, t => t.HasTile = false, t => t.HasTile = oldHas);
            Check("greenification-not-ignored-in-maw", soil, t => t.TileType = TileID.Grass, t => t.TileType = oldType);
            byte oldPaint = Main.tile[soil].TileColor;
            Check("unexpected-paint", soil, t => t.TileColor = PaintID.BluePaint, t => t.TileColor = oldPaint);
            Point slope = At(8, 85); SlopeType oldSlope = Main.tile[slope].Slope;
            Check("slope-changed", slope, t => t.Slope = SlopeType.Solid, t => t.Slope = oldSlope);
            Point wall = At(20, 54); ushort oldWall = Main.tile[wall].WallType;
            Check("wall-missing", wall, t => t.WallType = WallID.None, t => t.WallType = oldWall);
            Point control = At(7, 68); ushort controlType = Main.tile[control].TileType;
            Check("foreign-control-material", control, t => t.TileType = TileID.Stone, t => t.TileType = controlType);
            Point air = At(1, 1); ushort airType = Main.tile[air].TileType; bool airHas = Main.tile[air].HasTile;
            Check("plant-outside-control", air, t => { t.HasTile = true; t.TileType = TileID.Plants; }, t => { t.HasTile = airHas; t.TileType = airType; });
            Point controlAir = At(4, 63); ushort caType = Main.tile[controlAir].TileType; bool caHas = Main.tile[controlAir].HasTile;
            if (caHas) throw new InvalidOperationException("Control positive slot no longer empty");
            Check("native-control-plant-allowed", controlAir, t => { t.HasTile = true; t.TileType = TileID.Plants; }, t => { t.HasTile = caHas; t.TileType = caType; }, true);
            ValidateLayout(plan);
            Mod.Logger.Info($"MAW NATURAL MUTATIONS PASS: {checks} native cell controls; exact starting state restored. Original saved hash not reset.");
        }

        private void Properties()
        {
            RequireFixture(); var plan = MawNaturalLayout.Create();
            if (Fingerprint(plan) != checkpoint) throw new InvalidOperationException("Original plan changed; properties refused.");
            ValidateLayout(plan); string before = Fingerprint();
            try { MawProductionPropertyChecks.Run(At(8, 99), Mod.Logger); }
            finally {
                if (Fingerprint() != before) throw new InvalidOperationException("Property checks changed saved fixture.");
                Mod.Logger.Info("MAW PROPERTIES RESTORE: original fixture unchanged; no saved hash reset.");
            }
        }

        private string Fingerprint(MawNaturalLayout.Cell[,] plan = null)
        {
            using MemoryStream stream = new(); using BinaryWriter w = new(stream);
            for (int x = bounds.Left; x < bounds.Right; x++) for (int y = bounds.Top; y < bounds.Bottom; y++) {
                Tile t = Main.tile[x, y];
                if (plan != null) {
                    var cell = plan[x - bounds.X, y - bounds.Y]; w.Write(cell.Tile != null);
                    if (cell.Tile != null) {
                        int type = ResolveTile(cell.Tile); w.Write(TileLoader.GetTile(type)?.FullName ?? $"Terraria/{type}");
                        w.Write(cell.Slope); w.Write(cell.Half); w.Write((byte)0); w.Write(false); w.Write(false); w.Write(false);
                    }
                    w.Write(cell.Wall == null ? "Terraria/0" : MawTerrainStudies.Wall(cell.Wall).FullName);
                    w.Write((byte)0); w.Write(false); w.Write(false); w.Write((byte)0);
                    for (int n = 0; n < 5; n++) w.Write(false);
                    continue;
                }
                w.Write(t.HasTile);
                if (t.HasTile) { w.Write(TileLoader.GetTile(t.TileType)?.FullName ?? $"Terraria/{t.TileType}"); w.Write((byte)t.Slope); w.Write(t.IsHalfBlock); w.Write(t.TileColor); w.Write(t.IsActuated); w.Write(t.IsTileInvisible); w.Write(t.IsTileFullbright); }
                w.Write(WallLoader.GetWall(t.WallType)?.FullName ?? $"Terraria/{t.WallType}"); w.Write(t.WallColor); w.Write(t.IsWallInvisible); w.Write(t.IsWallFullbright);
                w.Write(t.LiquidAmount); if (t.LiquidAmount > 0) w.Write(t.LiquidType);
                w.Write(t.HasActuator); w.Write(t.RedWire); w.Write(t.BlueWire); w.Write(t.GreenWire); w.Write(t.YellowWire);
            }
            w.Flush(); return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
        }

        private void View(bool night)
        {
            RequireFixture();
            if (!viewing) { oldPosition = Main.LocalPlayer.position; oldDay = Main.dayTime; oldTime = Main.time; oldRain = Main.raining; oldEclipse = Main.eclipse; viewing = true; }
            Main.dayTime = !night; Main.time = night ? 16000 : 27000; Main.raining = false; Main.eclipse = false;
            Main.LocalPlayer.Teleport(new Vector2((bounds.X + 64) * 16, (bounds.Y + (panel == 0 ? 60 : 93)) * 16 - Main.LocalPlayer.height), 1);
            Main.LocalPlayer.velocity = Vector2.Zero;
            Main.NewText(panel == 0 ? "Maw material context — inert art study; not generated Gullet geometry." : "Native Grass/Dirt above; Maw Grass/Soil below. All four slopes both orders, half, stairs.", Color.Wheat);
        }

        public override void PostUpdateEverything()
        {
            if (!IsQa || captureDelay < 0 || captureDelay-- != 0) return; captureDelay = -1;
            CaptureManager.Instance.Capture(new CaptureSettings {
                Area = new Rectangle(bounds.X, bounds.Y + (panel == 0 ? 0 : 63), Width, panel == 0 ? 62 : 33),
                Biome = new CaptureBiome(0, 0, Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),
                CaptureBackground = true, CaptureEntities = true, UseScaling = true,
                OutputName = $"Apogean Maw Natural {panel} " + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")
            });
        }
        internal void Release()
        {
            captureDelay = -1; if (!viewing) return;
            Main.dayTime = oldDay; Main.time = oldTime; Main.raining = oldRain; Main.eclipse = oldEclipse;
            if (IsQa) Main.LocalPlayer.Teleport(oldPosition, 1); viewing = false;
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if (Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && checkpoint != null)
                tag["mawNaturalFixtureV1"] = new TagCompound { ["x"] = bounds.X, ["y"] = bounds.Y, ["checkpoint"] = checkpoint };
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if (Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3" || !tag.ContainsKey("mawNaturalFixtureV1")) return;
            TagCompound saved = tag.GetCompound("mawNaturalFixtureV1"); bounds = new Rectangle(saved.GetInt("x"), saved.GetInt("y"), Width, Height); checkpoint = saved.GetString("checkpoint");
        }
        public override void ClearWorld() { bounds = Rectangle.Empty; checkpoint = null; viewing = false; captureDelay = -1; panel = 0; }
    }
}
