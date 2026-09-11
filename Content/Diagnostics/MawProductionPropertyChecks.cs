using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;
using apogean.Content.Walls;
using apogean.Content.World;

namespace apogean.Content.Diagnostics
{
    // Uses shipping tile/wall types and native APIs, not the inert art studies.
    // Synchronous tests own an initially empty 5x5 slot and newly emitted items.
    internal static class MawProductionPropertyChecks
    {
        internal static void Run(Point p, log4net.ILog log)
        {
            if (Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3" || Main.LocalPlayer.name != "gg")
                throw new InvalidOperationException("Properties require gg/V3/single-player.");
            Rectangle area = new(p.X - 2, p.Y - 2, 5, 5);
            for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
                Tile t = Main.tile[x,y];
                if (t.HasTile || t.WallType != WallID.None || t.LiquidAmount != 0 || t.HasActuator || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire || t.TileColor != PaintID.None || t.WallColor != PaintID.None)
                    throw new InvalidOperationException("Property test slot is occupied; nothing cleared.");
            }
            int checks = 0; List<string> failures = new();
            void Check(bool condition, string name) {
                checks++; if (!condition) { failures.Add(name); log.Error("MAW PROPERTY FAIL: " + name); }
            }
            void Reset() {
                // Only two cells this synchronous test creates may be cleared.
                Main.tile[p].ClearEverything(); Main.tile[p.X,p.Y+1].ClearEverything();
            }
            void Set(int type, int wall = WallID.None) {
                Reset(); Tile support=Main.tile[p.X,p.Y+1]; support.HasTile=true; support.TileType=TileID.GrayBrick;
                WorldGen.PlaceTile(p.X,p.Y,type,mute:true);
                if (!Main.tile[p].HasTile || Main.tile[p].TileType != type) throw new InvalidOperationException("Native property placement failed: " + type);
                Main.tile[p].WallType=(ushort)wall;
            }
            int T<T>() where T:ModTile => ModContent.TileType<T>();
            int W<T>() where T:ModWall => ModContent.WallType<T>();
            var materials = new (string Name,int Maw,int Waste,int Pure)[] {
                ("soil",T<MawDirt>(),T<WastesSoil>(),TileID.Dirt),
                ("stone",T<Mawstone>(),T<WastesStone>(),TileID.Stone),
                ("grass",T<MawGrass>(),T<WastesGrass>(),TileID.Grass),
                ("sand",T<MawSand>(),T<WastesSand>(),TileID.Sand),
                ("ice",T<MawIce>(),T<WastesIce>(),TileID.IceBlock),
                ("snow",T<MawSnow>(),T<WastesSnow>(),TileID.SnowBlock),
                ("mud",T<MawMud>(),T<WastesMud>(),TileID.Mud),
                ("clay",T<MawClay>(),T<WastesSoil>(),TileID.Dirt),
                ("legacy-turf",T<EngraftTurf>(),T<WastesGrass>(),TileID.Grass)
            };
            var walls = new (string Name,int Maw,int Waste,int Pure)[] {
                ("soil",W<MawDirtWallUnsafe>(),W<WastesDirtWallUnsafe>(),WallID.DirtUnsafe),
                ("stone",W<MawStoneWallUnsafe>(),W<WastesStoneWallUnsafe>(),WallID.Stone),
                ("grass",W<MawGrassWallUnsafe>(),W<WastesGrassWallUnsafe>(),WallID.GrassUnsafe),
                ("sand",W<MawSandWallUnsafe>(),W<WastesSandWallUnsafe>(),WallID.Sandstone),
                ("ice",W<MawIceWallUnsafe>(),W<WastesIceWallUnsafe>(),WallID.IceUnsafe),
                ("snow",W<MawSnowWallUnsafe>(),W<WastesSnowWallUnsafe>(),WallID.SnowWallUnsafe),
                ("mud",W<MawMudWallUnsafe>(),W<WastesMudWallUnsafe>(),WallID.MudUnsafe),
                ("legacy-gullet",W<MawWallUnsafe>(),W<WastesDirtWallUnsafe>(),WallID.DirtUnsafe)
            };
            bool[] items = new bool[Main.maxItems]; for(int i=0;i<items.Length;i++)items[i]=Main.item[i].active;
            void ClearDrops() {
                for(int i=0;i<items.Length;i++) if(!items[i] && Main.item[i].active && Vector2.Distance(Main.item[i].Center,p.ToVector2()*16)<96) Main.item[i].TurnToAir();
            }
            try {
                foreach(var m in materials) {
                    Set(m.Maw); ModTile tile=TileLoader.GetTile(m.Maw);
                    Check(Collision.SolidCollision(p.ToVector2()*16,16,16),m.Name+"/solid");
                    Tile cell=Main.tile[p];cell.HasActuator=true;cell.IsActuated=true;
                    Check(!Collision.SolidCollision(p.ToVector2()*16,16,16),m.Name+"/actuated-nonsolid");
                    cell.IsActuated=false;cell.HasActuator=false;
                    Check(!Main.tileLighted[m.Maw] && TileID.Sets.TouchDamageImmediate[m.Maw]==0,m.Name+"/safe-nonemissive-material");
                    int expectedDrop=TileLoader.GetItemDropFromTypeAndStyle(m.Maw);
                    Check(expectedDrop>ItemID.None,m.Name+"/registered-item");
                    if(m.Name=="stone") {
                        for(int n=0;n<30;n++)Main.LocalPlayer.PickTile(p.X,p.Y,58);
                        Check(Main.tile[p].HasTile,"stone/pick58-rejected");
                    }
                    int power=m.Name=="stone"?59:35;
                    for(int n=0;n<80 && Main.tile[p].HasTile;n++)Main.LocalPlayer.PickTile(p.X,p.Y,power);
                    Check(!Main.tile[p].HasTile,m.Name+"/native-pick-removal");
                    int dropped=0,foreign=0;
                    for(int i=0;i<items.Length;i++)if(!items[i] && Main.item[i].active && Vector2.Distance(Main.item[i].Center,p.ToVector2()*16)<96) {
                        if(Main.item[i].type==expectedDrop)dropped+=Main.item[i].stack;else foreign+=Main.item[i].stack;
                    }
                    Check(dropped==1 && foreign==0,m.Name+$"/one-real-item-drop(actual={dropped},foreign={foreign})"); ClearDrops();
                    foreach(int conversion in new[]{BiomeConversionID.Purity,BiomeConversionID.PurificationPowder}) {
                        for(int shape=0;shape<6;shape++) {
                            Set(m.Maw);cell=Main.tile[p];cell.Slope=(SlopeType)(shape<5?shape:0);cell.IsHalfBlock=shape==5;
                            cell.TileColor=PaintID.BluePaint;cell.RedWire=true;
                            TileLoader.Convert(p.X,p.Y,conversion);
                            Check(cell.TileType==m.Waste,m.Name+"/first-purity-"+conversion+"/"+shape);
                            TileLoader.Convert(p.X,p.Y,conversion);
                            Check(cell.TileType==m.Pure,m.Name+"/second-purity-"+conversion+"/"+shape);
                            Check((int)cell.Slope==(shape<5?shape:0) && cell.IsHalfBlock==(shape==5) && cell.TileColor==PaintID.BluePaint && cell.RedWire,m.Name+"/conversion-preserves-state-"+conversion+"/"+shape);
                        }
                    }
                    log.Info("MAW PROPERTY MATERIAL FINISHED: "+m.Name);
                }
                foreach(var w in walls)foreach(int conversion in new[]{BiomeConversionID.Purity,BiomeConversionID.PurificationPowder}) {
                    Set(T<MawDirt>(),w.Maw);Tile cell=Main.tile[p];cell.WallColor=PaintID.BluePaint;cell.IsWallFullbright=true;
                    WallLoader.Convert(p.X,p.Y,conversion);Check(cell.WallType==w.Waste,w.Name+"/wall-first-purity-"+conversion);
                    WallLoader.Convert(p.X,p.Y,conversion);Check(cell.WallType==w.Pure,w.Name+"/wall-second-purity-"+conversion);
                    Check(cell.WallColor==PaintID.BluePaint && cell.IsWallFullbright,w.Name+"/wall-preserves-coatings-"+conversion);
                }
                Set(TileID.GrayBrick,WallID.Wood);Tile built=Main.tile[p];built.RedWire=true;
                Check(!MawConversionSystem.ConvertAt(p.X,p.Y,true,true) && Main.tile[p].TileType==TileID.GrayBrick && Main.tile[p].WallType==WallID.Wood && Main.tile[p].RedWire,"conversion-preserves-player-built-brick-wood-wire");
                Set(T<KesslerBlock>(),W<KesslerBulkheadWall>());
                Check(!MawConversionSystem.ConvertAt(p.X,p.Y,true,true) && Main.tile[p].TileType==T<KesslerBlock>() && Main.tile[p].WallType==W<KesslerBulkheadWall>(),"conversion-preserves-corporate-material");
                Check(TileID.Sets.Falling[T<MawSand>()] && TileID.Sets.FallingBlockProjectile[T<MawSand>()]!=null,"sand-falling-registration-only");
                Check(TileID.Sets.NeedsGrassFraming[T<MawGrass>()] && TileID.Sets.NeedsGrassFramingDirt[T<MawGrass>()]==T<MawDirt>(),"grass-native-soil-framing");
                Check(TileID.Sets.NeedsGrassFraming[T<EngraftTurf>()] && TileID.Sets.NeedsGrassFramingDirt[T<EngraftTurf>()]==T<MawDirt>(),"legacy-turf-native-soil-framing");
                // This is a policy check, not a simulated dust/light engine result.
                foreach(var m in materials)Check(TileLoader.GetTile(m.Maw).DustType!=DustID.AmberBolt,m.Name+"/ordinary-mining-not-amber-projectile-dust");
                foreach(var w in walls)Check(WallLoader.GetWall(w.Maw).DustType!=DustID.AmberBolt,w.Name+"/ordinary-wall-not-amber-projectile-dust");
            } finally { Reset(); ClearDrops(); }
            log.Info($"MAW PRODUCTION PROPERTIES: {checks-failures.Count}/{checks} passed; failures={string.Join(",",failures)}. Native API tests, not hand-played, falling simulation, bomb simulation or multiplayer certification.");
            if(failures.Count>0)throw new InvalidOperationException("Production properties failed: "+string.Join(",",failures));
        }
    }
}
