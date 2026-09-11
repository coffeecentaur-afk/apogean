using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace apogean.Content.Diagnostics
{
    // One separately owned native study. No worldgen hook, timer, vine or
    // infection spread. Existing material galleries are never edited here.
    public sealed class MawFiberRibLab : ModSystem
    {
        private const int Width = 96, Height = 72, Budget = 8;
        private const string SaveKey = "mawFiberRibStudyV1";
        private Rectangle bounds;
        private string originalDigest;
        private int steps, captureDelay = -1;
        private bool viewing, oldDay, oldRain, oldEclipse;
        private double oldTime;
        private Vector2 oldPosition;
        private static bool IsQa => Main.netMode == NetmodeID.SinglePlayer &&
            Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && Main.LocalPlayer.name == "gg";
        private sealed class Cell { internal string Key; internal byte Slope; internal bool Half; }
        private Point At(int x, int y) => new(bounds.X+x,bounds.Y+y);
        private static int Type(string key) => MawPackedPreview.TileType(key);

        private static Cell[,] Original()
        {
            var plan = new Cell[Width,Height];
            for(int x=0;x<Width;x++)for(int y=0;y<Height;y++)plan[x,y]=new Cell();
            for(int x=4;x<59;x++)for(int y=12;y<67;y++) {
                if(x>=24 && x<45)continue;
                plan[x,y].Key = y<16 || (x>=21 && x<24) || (x>=45 && x<48) ? "soil" : "stone";
            }
            // Deliberately unequal broken arcs. No regular ladder, tooth damage
            // on structural bone, or automatically supplied ropes/platforms.
            void Rib(int x,int y,int direction,int[] heights) {
                for(int d=0;d<heights.Length;d++) for(int thick=0;thick<(d<heights.Length-2?3:2);thick++)
                    plan[x+d*direction,y+heights[d]+thick].Key="bone";
            }
            Rib(20,27,1,new[]{1,0,0,0,1,1,2,3,4,6,7});
            Rib(49,44,-1,new[]{0,0,0,1,2,2,3});
            Rib(20,57,1,new[]{0,0,1,2,4});
            // Connected topsoil seeds and a four-sided isolated soil specimen.
            plan[10,12].Key=plan[52,12].Key="grass";
            for(int x=67;x<=77;x++)for(int y=14;y<=24;y++)plan[x,y].Key="soil";
            plan[72,14].Key="grass";
            // A disconnected island must not be colonized across the air gap.
            for(int x=80;x<84;x++)for(int y=14;y<18;y++)plan[x,y].Key="soil";
            // Four native slope directions plus a half block on separate banks.
            for(int k=0;k<5;k++) {
                int top=31+k*7;
                for(int x=68;x<=77;x++)for(int y=top;y<top+4;y++)plan[x,y].Key="soil";
                plan[70,top].Key="grass";
                if(k<4)plan[72,top].Slope=(byte)(k+1);else plan[72,top].Half=true;
            }
            return plan;
        }
        private static MawFiberGrowthPolicy.Cell[,] PolicyCells(Cell[,] plan)
        {
            var result=new MawFiberGrowthPolicy.Cell[Width,Height];
            for(int x=0;x<Width;x++)for(int y=0;y<Height;y++) {
                Cell c=plan[x,y];
                var host=c.Key switch { null=>MawFiberGrowthPolicy.Host.Air,"soil"=>MawFiberGrowthPolicy.Host.Soil,
                    "grass"=>MawFiberGrowthPolicy.Host.Grass,_=>MawFiberGrowthPolicy.Host.Other };
                result[x,y]=new(host,false,c.Slope!=0||c.Half);
            }
            return result;
        }
        private static Cell[,] Expected(int count)
        {
            if(count<0||count>256)throw new InvalidOperationException("Invalid saved maturation step count.");
            var plan=Original();
            for(int n=0;n<count;n++)foreach(var p in MawFiberGrowthPolicy.Plan(PolicyCells(plan),Budget,true))plan[p.X,p.Y].Key="grass";
            return plan;
        }
        private static string Digest(Cell[,] plan)
        {
            using var bytes=new MemoryStream();using var writer=new BinaryWriter(bytes);
            for(int x=0;x<Width;x++)for(int y=0;y<Height;y++) {
                Cell c=plan[x,y];writer.Write(c.Key??"air");writer.Write(c.Slope);writer.Write(c.Half);
            }
            writer.Flush();return Convert.ToHexString(SHA256.HashData(bytes.ToArray()));
        }
        private void Validate()
        {
            if(bounds.Width!=Width||bounds.Height!=Height||bounds.Left<40||bounds.Top<40||
                bounds.Right>Main.maxTilesX-40||bounds.Bottom>Main.maxTilesY-40||originalDigest!=Digest(Original()))
                throw new InvalidOperationException("Missing/changed fiber-rib study contract; no rebuild/rebaseline.");
            Cell[,] expected=Expected(steps);int grass=0,bone=0;
            for(int x=0;x<Width;x++)for(int y=0;y<Height;y++) {
                Tile t=Main.tile[At(x,y)];Cell c=expected[x,y];
                if(t.HasTile!=(c.Key!=null)||(c.Key!=null && (t.TileType!=Type(c.Key)||(byte)t.Slope!=c.Slope||t.IsHalfBlock!=c.Half))||
                    t.WallType!=WallID.None||t.LiquidAmount!=0||t.HasActuator||t.IsActuated||t.RedWire||t.BlueWire||t.GreenWire||t.YellowWire||
                    t.TileColor!=PaintID.None||t.WallColor!=PaintID.None||t.IsTileInvisible||t.IsTileFullbright||t.IsWallInvisible||t.IsWallFullbright)
                    throw new InvalidOperationException($"Fiber/rib study differs at {x},{y}; preserved, no repair.");
                if(c.Key=="grass")grass++;if(c.Key=="bone")bone++;
            }
            Mod.Logger.Info($"MAW FIBER STATE PASS: steps={steps}; grass={grass}; structuralBone={bone}; all6912 cells exact against original+replayed bounded steps. No traversal/art acceptance.");
        }
        private void Build()
        {
            if(!bounds.IsEmpty||originalDigest!=null)throw new InvalidOperationException("Fiber/rib study already exists; refusing rebuild.");
            Rectangle grove=ModContent.GetInstance<VegetationVisualLab>().PreservedBounds;grove.Inflate(20,20);
            foreach(int dx in new[]{3400,-3400,3600,-3600}) {
                foreach(int dy in new[]{-360,-440,-280}) {
                    Rectangle site=new(Main.spawnTileX+dx,Main.spawnTileY+dy,Width,Height),envelope=site;envelope.Inflate(5,5);
                    if(envelope.Left<40||envelope.Top<40||envelope.Right>Main.maxTilesX-40||envelope.Bottom>Main.maxTilesY-40||envelope.Intersects(grove))continue;
                    bool empty=true;
                    for(int x=envelope.Left;x<envelope.Right;x++)for(int y=envelope.Top;y<envelope.Bottom;y++) {
                        Tile t=Main.tile[x,y];
                        empty &= !t.HasTile && t.WallType==WallID.None && t.LiquidAmount==0 && !t.HasActuator && !t.IsActuated &&
                            !t.RedWire && !t.BlueWire && !t.GreenWire && !t.YellowWire && t.TileColor==PaintID.None && t.WallColor==PaintID.None &&
                            !t.IsTileInvisible && !t.IsTileFullbright && !t.IsWallInvisible && !t.IsWallFullbright;
                    }
                    if(empty){bounds=site;break;}
                }
                if(!bounds.IsEmpty)break;
            }
            if(bounds.IsEmpty)throw new InvalidOperationException("No empty study envelope; nothing cleared.");
            var plan=Original();originalDigest=Digest(plan);steps=0;
            for(int x=0;x<Width;x++)for(int y=0;y<Height;y++)if(plan[x,y].Key!=null) {
                Tile t=Main.tile[At(x,y)];t.HasTile=true;t.TileType=(ushort)Type(plan[x,y].Key);
                t.Slope=(SlopeType)plan[x,y].Slope;t.IsHalfBlock=plan[x,y].Half;
            }
            WorldGen.RangeFrame(bounds.Left-1,bounds.Top-1,bounds.Right+1,bounds.Bottom+1);
            Mod.Logger.Info($"MAW FIBER BUILD: {bounds}; original={originalDigest}; empty separate envelope; no old gallery or worldgen changed.");
            Validate();View();
        }
        private void Grow()
        {
            Validate();if(steps>=256)throw new InvalidOperationException("Study step limit reached.");
            // Validate before writes, then the same pure two-phase policy. This
            // explicit QA command is not a production RandomUpdate hook.
            var pending=MawFiberGrowthPolicy.Plan(PolicyCells(Expected(steps)),Budget,true);
            foreach(var p in pending)Main.tile[At(p.X,p.Y)].TileType=(ushort)Type("grass");
            steps++;
            foreach(var p in pending)WorldGen.SquareTileFrame(bounds.X+p.X,bounds.Y+p.Y);
            Validate();Mod.Logger.Info($"MAW FIBER STEP PASS: {pending.Count}/{Budget} conversions; only exposed already-Maw soil; current step={steps}.");
        }
        internal void Run(string request)
        {
            if(!IsQa||!MawPackedPreview.Enabled)throw new InvalidOperationException("Fiber/rib study requires packed gg/V3/SP.");
            string grove=ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot();
            Mod.Logger.Info("MAW FIBER REQUEST: "+request);
            try {
                switch(request) {
                    case "build":Build();break;
                    case "step":Grow();break;
                    case "mature":for(int n=0;n<16;n++)Grow();break;
                    case "test":case "reload":Validate();break;
                    case "view":Validate();View();break;
                    case "capture":Validate();captureDelay=60;break;
                    case "release":Release();break;
                    default:throw new InvalidOperationException("Unknown fiber study request.");
                }
            } finally {
                if(grove!=ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot())throw new InvalidOperationException("Fiber study changed preserved grove.");
                Mod.Logger.Info("MAW FIBER GROVE GUARD: unchanged; old cross-reload failure remains separate/RED.");
            }
            Mod.Logger.Info("MAW FIBER COMPLETE: "+request);
        }
        private Vector2 ViewPosition()=>new((bounds.X+36)*16,(bounds.Y+36)*16-Main.LocalPlayer.height);
        private void View()
        {
            if(!viewing){oldPosition=Main.LocalPlayer.position;oldTime=Main.time;oldDay=Main.dayTime;oldRain=Main.raining;oldEclipse=Main.eclipse;viewing=true;}
            Main.LocalPlayer.Teleport(ViewPosition(),1);Main.LocalPlayer.velocity=Vector2.Zero;
            Main.dayTime=true;Main.time=27000;Main.raining=false;Main.eclipse=false;
            Main.NewText("Maw fiber/rib study: real soil/grass, irregular safe bone. Right: four-face growth, air-gap island and slopes. Not worldgen or traversal approval.",Color.Wheat);
        }
        internal void Release()
        {
            captureDelay=-1;if(!viewing)return;
            Main.dayTime=oldDay;Main.time=oldTime;Main.raining=oldRain;Main.eclipse=oldEclipse;
            if(IsQa)Main.LocalPlayer.Teleport(oldPosition,1);viewing=false;
        }
        public override void PostUpdateEverything()
        {
            if(!IsQa)return;
            if(viewing){Main.LocalPlayer.position=ViewPosition();Main.LocalPlayer.velocity=Vector2.Zero;}
            if(captureDelay<0||captureDelay--!=0)return;captureDelay=-1;
            CaptureManager.Instance.Capture(new CaptureSettings{Area=bounds,Biome=new CaptureBiome(0,0,Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),
                CaptureBackground=true,CaptureEntities=true,UseScaling=true,OutputName="Apogean Maw Fiber Rib "+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")});
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name=="Apogee Native Visual V3"&&originalDigest!=null)
                tag[SaveKey]=new TagCompound{["x"]=bounds.X,["y"]=bounds.Y,["steps"]=steps,["original"]=originalDigest};
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3"||!tag.ContainsKey(SaveKey))return;
            var saved=tag.GetCompound(SaveKey);bounds=new(saved.GetInt("x"),saved.GetInt("y"),Width,Height);
            steps=saved.GetInt("steps");originalDigest=saved.GetString("original");
        }
        public override void ClearWorld(){bounds=Rectangle.Empty;originalDigest=null;steps=0;viewing=false;captureDelay=-1;}
    }
}
