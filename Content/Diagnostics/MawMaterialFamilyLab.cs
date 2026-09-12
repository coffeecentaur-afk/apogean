using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace apogean.Content.Diagnostics
{
    public sealed class MawMaterialFamilyLab : ModSystem
    {
        private Rectangle bounds;
        internal Rectangle PreservedBounds => bounds;
        private string checkpoint;
        private int row, captureDelay = -1;
        private bool viewing, oldDay, oldRain, oldEclipse;
        private double oldTime;
        private Vector2 oldPosition;
        private static bool IsQa => Main.netMode == NetmodeID.SinglePlayer &&
            Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && Main.LocalPlayer.name == "gg";
        private Point At(int x, int y) => new(bounds.X + x, bounds.Y + y);

        internal void Run(string request)
        {
            if (!IsQa) throw new InvalidOperationException("Family lab requires gg/V3/single-player.");
            string grove = ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot();
            try {
                switch (request) {
                    case "build": Build(); break;
                    case "test": Test(); break;
                    case "reload": RequireFixture(); Test(); break;
                    case "row0": row = 0; View(false); break;
                    case "row1": row = 1; View(false); break;
                    case "row2": row = 2; View(false); break;
                    case "night": View(true); break;
                    case "capture": RequireFixture(); captureDelay = 60; break;
                    case "release": Release(); break;
                    default: throw new InvalidOperationException("Unknown family request.");
                }
            } finally {
                if (grove != ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot())
                    throw new InvalidOperationException("Family lab altered the preserved grove.");
                Mod.Logger.Info("MAW FAMILY GROVE GUARD: unchanged; existing grove reload failure remains separate.");
            }
        }
        private static bool Empty(Rectangle area)
        {
            for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
                Tile t = Main.tile[x, y];
                if (t.HasTile || t.WallType != WallID.None || t.LiquidAmount != 0 || t.HasActuator || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire) return false;
            }
            return true;
        }
        private void Fill(int x, int y, int width, int height, int type)
        {
            for (int i = x; i < x + width; i++) for (int j = y; j < y + height; j++) {
                Point p = At(i, j); WorldGen.PlaceTile(p.X, p.Y, type, mute: true);
                if (!Main.tile[p].HasTile || Main.tile[p].TileType != type) throw new InvalidOperationException("Family placement failed.");
            }
        }
        private void Build()
        {
            if (!bounds.IsEmpty) throw new InvalidOperationException("Saved family fixture exists; refusing replacement.");
            Rectangle grove = ModContent.GetInstance<VegetationVisualLab>().PreservedBounds; grove.Inflate(16,16);
            foreach (int dx in new[] { 1600, -1760, 1840, -2000, 2080, -2240 }) {
                foreach (int dy in new[] { -120, -180, -240, -300 }) {
                    Rectangle site = new(Main.spawnTileX + dx, Main.spawnTileY + dy, 152, 94);
                    Rectangle envelope = site; envelope.Inflate(3, 3);
                    if (envelope.Left < 30 || envelope.Top < 40 || envelope.Right > Main.maxTilesX - 30 ||
                        envelope.Bottom > Main.maxTilesY - 40 || envelope.Intersects(grove) || !Empty(envelope)) continue;
                    bounds = site; break;
                }
                if (!bounds.IsEmpty) break;
            }
            if (bounds.IsEmpty) throw new InvalidOperationException("No empty family site; terrain was not cleared.");
            for (int n = 0; n < 12; n++) {
                string key = MawTerrainStudies.Materials[n];
                int type = MawTerrainStudies.Tile(key).Type, wall = MawTerrainStudies.Wall(key).Type;
                int x = 4 + n % 4 * 37, y = 3 + n / 4 * 30;
                Fill(x, y, 28, key == "grass" ? 1 : 8, type);
                if (key == "grass") Fill(x, y + 1, 28, 7, MawTerrainStudies.Tile("soil").Type);
                for (int i = x; i < x + 28; i++) for (int j = y + 10; j < y + 15; j++) {
                    Point p = At(i, j); WorldGen.PlaceWall(p.X, p.Y, wall, true);
                    if (Main.tile[p].WallType != wall) throw new InvalidOperationException("Family wall placement failed.");
                }
                for (int k = 0; k < 7; k++) Fill(x + k * 4, y + 18, 3, 3, type);
                Fill(x, y + 24, 30, 2, TileID.GrayBrick);
            }
            WorldGen.RangeFrame(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
            for (int x = bounds.Left; x < bounds.Right; x++) for (int y = bounds.Top; y < bounds.Bottom; y++)
                WorldGen.SquareWallFrame(x, y);
            for (int n = 0; n < 12; n++) {
                int x = 4 + n % 4 * 37, y = 3 + n / 4 * 30;
                for (int k = 0; k < 4; k++) { Tile t = Main.tile[At(x + k * 4, y + 18)]; t.Slope = (SlopeType)(k + 1); }
                Tile half = Main.tile[At(x + 16, y + 18)]; half.IsHalfBlock = true;
                for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) {
                    Tile paint = Main.tile[At(x + 20 + i, y + 18 + j)]; paint.TileColor = PaintID.BluePaint;
                    Tile actuated = Main.tile[At(x + 24 + i, y + 18 + j)]; actuated.HasActuator = true; actuated.IsActuated = true;
                }
            }
            checkpoint = Fingerprint();
            Mod.Logger.Info($"MAW FAMILY BUILD: {bounds}; empty envelope; {checkpoint}");
            row = 0; View(false);
        }
        private void RequireFixture()
        {
            if (bounds.Width != 152 || bounds.Height != 94 || bounds.Left < 30 || bounds.Top < 40 ||
                bounds.Right > Main.maxTilesX - 30 || bounds.Bottom > Main.maxTilesY - 40 || string.IsNullOrEmpty(checkpoint))
                throw new InvalidOperationException("Missing/invalid family fixture.");
        }
        private void Test()
        {
            RequireFixture();
            if (Fingerprint() != checkpoint) throw new InvalidOperationException("Family fixture drift; no repair attempted.");
            int tiles = 0, walls = 0;
            for (int x = bounds.Left; x < bounds.Right; x++) for (int y = bounds.Top; y < bounds.Bottom; y++) {
                Tile t = Main.tile[x, y];
                if (t.HasTile && TileLoader.GetTile(t.TileType) is MawTerrainStudyTile tile) {
                    short originalX = t.TileFrameX, originalY = t.TileFrameY;
                    if (!tile.Map.TryMap(x, y, originalX, originalY, out short px, out short py))
                        throw new InvalidOperationException($"Unmapped tile {tile.Name} at {x},{y}: {originalX},{originalY}");
                    var texture = TextureAssets.Tile[t.TileType].Value;
                    if (texture.Width != tile.Map.Width || texture.Height != tile.Map.Height) throw new InvalidOperationException("Loaded tile dimensions");
                    short fx=originalX,fy=originalY;int width=16,height=16,offset=0;
                    TileLoader.SetDrawPositions(x,y,ref width,ref offset,ref height,ref fx,ref fy);
                    if(fx!=px||fy!=py||width!=16||height!=16||offset!=0||t.TileFrameX!=originalX||t.TileFrameY!=originalY)
                        throw new InvalidOperationException("Native draw policy altered geometry or saved frames.");
                    tiles++;
                }
                if (WallLoader.GetWall(t.WallType) is MawTerrainStudyWall wall) {
                    var texture = TextureAssets.Wall[t.WallType].Value;
                    if (!wall.Map.TryMap(x,y,t.WallFrameX,t.WallFrameY,out _,out _) ||
                        texture.Width != wall.Map.Width || texture.Height != wall.Map.Height)
                        throw new InvalidOperationException($"Unmapped wall {wall.Name} frame {t.WallFrameX},{t.WallFrameY}");
                    walls++;
                }
            }
            if (tiles < 3000 || walls != 1680) throw new InvalidOperationException("Incomplete family fixture");
            if (Fingerprint() != checkpoint) throw new InvalidOperationException("Family test mutated fixture");
            Mod.Logger.Info($"MAW FAMILY DRAW MATRIX PASS: 12 materials; {tiles} native tile draws; {walls} wall frames. Art/renderer fixture only: no sand falling, amber emission, drops, conversion or multiplayer acceptance.");
            View(false);
        }
        private string Fingerprint()
        {
            using MemoryStream stream = new(); using BinaryWriter writer = new(stream);
            for (int x=bounds.Left;x<bounds.Right;x++)for(int y=bounds.Top;y<bounds.Bottom;y++) {
                Tile t=Main.tile[x,y];writer.Write(t.HasTile);
                if(t.HasTile){writer.Write(TileLoader.GetTile(t.TileType)?.FullName??$"Terraria/{t.TileType}");writer.Write((byte)t.Slope);writer.Write(t.IsHalfBlock);writer.Write(t.TileColor);writer.Write(t.IsActuated);writer.Write(t.IsTileInvisible);writer.Write(t.IsTileFullbright);}
                writer.Write(WallLoader.GetWall(t.WallType)?.FullName??$"Terraria/{t.WallType}");writer.Write(t.WallColor);writer.Write(t.IsWallInvisible);writer.Write(t.IsWallFullbright);
                writer.Write(t.LiquidAmount);if(t.LiquidAmount>0)writer.Write(t.LiquidType);
                writer.Write(t.HasActuator);writer.Write(t.RedWire);writer.Write(t.BlueWire);writer.Write(t.GreenWire);writer.Write(t.YellowWire);
            }
            writer.Flush();return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
        }
        private void View(bool night)
        {
            RequireFixture();
            if(!viewing){oldPosition=Main.LocalPlayer.position;oldDay=Main.dayTime;oldTime=Main.time;oldRain=Main.raining;oldEclipse=Main.eclipse;viewing=true;}
            Main.dayTime=!night;Main.time=night?16000:27000;Main.raining=false;Main.eclipse=false;
            Main.LocalPlayer.Teleport(new Vector2((bounds.X+79)*16,(bounds.Y+row*30+27)*16-Main.LocalPlayer.height),1);
            Main.LocalPlayer.velocity=Vector2.Zero;
            Main.NewText($"Maw art studies row {row+1}: {string.Join(" / ",MawTerrainStudies.Materials,row*4,4)}. Materials / walls / slopes, half, paint, actuator. Gameplay not promoted.",Color.Wheat);
        }
        public override void PostUpdateEverything()
        {
            if(!IsQa||captureDelay<0||captureDelay--!=0)return;captureDelay=-1;
            CaptureManager.Instance.Capture(new CaptureSettings {
                Area=new Rectangle(bounds.X,bounds.Y+row*30,152,30),Biome=new CaptureBiome(0,0,Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),
                CaptureBackground=true,CaptureEntities=true,UseScaling=true,
                OutputName=$"Apogean Maw Family row{row+1} "+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")
            });
        }
        internal void Release()
        {
            captureDelay=-1;if(!viewing)return;
            Main.dayTime=oldDay;Main.time=oldTime;Main.raining=oldRain;Main.eclipse=oldEclipse;
            if(IsQa)Main.LocalPlayer.Teleport(oldPosition,1);viewing=false;
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if(IsQa&&checkpoint!=null)tag["mawFamilyFixtureV1"]=new TagCompound{["x"]=bounds.X,["y"]=bounds.Y,["checkpoint"]=checkpoint};
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3"||!tag.ContainsKey("mawFamilyFixtureV1"))return;
            TagCompound saved=tag.GetCompound("mawFamilyFixtureV1");bounds=new Rectangle(saved.GetInt("x"),saved.GetInt("y"),152,94);checkpoint=saved.GetString("checkpoint");
        }
        public override void ClearWorld(){bounds=Rectangle.Empty;checkpoint=null;viewing=false;captureDelay=-1;row=0;}
    }
}
