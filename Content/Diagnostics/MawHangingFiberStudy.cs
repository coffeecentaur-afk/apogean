using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace apogean.Content.Diagnostics
{
    public sealed class MawHangingFiberStudy : ModSystem
    {
        internal Rectangle Bounds;
        private int steps;
        private Point? probe;
        internal static int Fiber=>ModContent.Find<ModTile>("apogean/MawHangingFiber").Type;
        private Point Root(int n)=>new(Bounds.X+8+n*12,Bounds.Y+4);
        internal static bool AllowedRoot(Point p)
        {
            if(Main.netMode!=NetmodeID.SinglePlayer||Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3"||Main.LocalPlayer.name!="gg")return false;
            var lab=ModContent.GetInstance<MawHangingFiberStudy>();
            if(lab.probe==p)return true;
            if(lab.Bounds.IsEmpty)return false;
            for(int n=0;n<7;n++)if(lab.Root(n)==p)return true;return false;
        }
        private static bool Empty(Tile t)=>!t.HasTile&&t.WallType==WallID.None&&t.LiquidAmount==0&&!t.HasActuator&&!t.IsActuated&&!t.RedWire&&!t.BlueWire&&!t.GreenWire&&!t.YellowWire&&
            t.TileColor==PaintID.None&&t.WallColor==PaintID.None&&!t.IsTileInvisible&&!t.IsWallInvisible&&!t.IsTileFullbright&&!t.IsWallFullbright;
        internal void Build(Rectangle parent)
        {
            if(!Bounds.IsEmpty)throw new InvalidOperationException("Hanging-fiber study exists; no rebuild.");
            Rectangle candidate=new(parent.Left,parent.Bottom+20,100,20),envelope=candidate;envelope.Inflate(3,3);
            if(!WorldGen.InWorld(envelope.Left,envelope.Top,30)||!WorldGen.InWorld(envelope.Right,envelope.Bottom,30))throw new InvalidOperationException("Vine bounds.");
            for(int x=envelope.Left;x<envelope.Right;x++)for(int y=envelope.Top;y<envelope.Bottom;y++)
                if(!Empty(Main.tile[x,y]))throw new InvalidOperationException("Vine exhibit envelope occupied; no writes.");
            Bounds=candidate;steps=0;
            for(int n=0;n<7;n++) {
                Point root=Root(n);
                for(int dx=-1;dx<=1;dx++)for(int dy=-1;dy<=0;dy++) {
                    Tile t=Main.tile[root.X+dx,root.Y+dy];t.HasTile=true;
                    t.TileType=(ushort)MawPackedPreview.TileType(dy==0?"grass":"soil");
                }
            }
            WorldGen.RangeFrame(Bounds.Left,Bounds.Top,Bounds.Right,Bounds.Bottom);
            Validate();Mod.Logger.Info($"MAW HANGING BUILD: {Bounds}; seven roots; separate empty envelope; no natural growth timer.");
        }
        internal void Grow()
        {
            Validate();if(steps>=MawHangingFiber.MaxLength)return;
            for(int n=0;n<7;n++)if(!MawHangingFiber.Grow(Root(n),Fiber))throw new InvalidOperationException("Registered root did not grow.");
            steps++;Validate();
        }
        internal void Validate()
        {
            if(Bounds.Width!=100||Bounds.Height!=20||steps<0||steps>MawHangingFiber.MaxLength||
                !WorldGen.InWorld(Bounds.Left,Bounds.Top,30)||!WorldGen.InWorld(Bounds.Right,Bounds.Bottom,30))
                throw new InvalidOperationException("Missing/invalid hanging fixture.");
            int count=0;
            for(int x=0;x<100;x++)for(int y=0;y<20;y++) {
                int expected=-1,depth=0;
                for(int n=0;n<7;n++) {
                    int rx=8+n*12;
                    if(Math.Abs(x-rx)<=1&&y>=3&&y<=4)expected=MawPackedPreview.TileType(y==4?"grass":"soil");
                    if(x==rx&&y>4&&y<=4+steps){expected=Fiber;depth=y-4;}
                }
                Tile t=Main.tile[Bounds.X+x,Bounds.Y+y];
                if(expected<0){if(!Empty(t))throw new InvalidOperationException($"Unexpected hanging cell {x},{y}");continue;}
                if(!t.HasUnactuatedTile||t.TileType!=expected||t.Slope!=SlopeType.Solid||t.IsHalfBlock||t.WallType!=WallID.None||t.LiquidAmount!=0||
                    t.HasActuator||t.RedWire||t.BlueWire||t.GreenWire||t.YellowWire||t.TileColor!=PaintID.None||t.WallColor!=PaintID.None||t.IsTileInvisible||t.IsWallInvisible||t.IsTileFullbright||t.IsWallFullbright)
                    throw new InvalidOperationException($"Hanging exhibit changed at {x},{y}; has={t.HasTile},type={t.TileType},actuated={t.IsActuated},slope={t.Slope},half={t.IsHalfBlock},liquid={t.LiquidAmount}/{t.LiquidType},paint={t.TileColor},frame={t.TileFrameX},{t.TileFrameY}; no repair.");
                if(depth>0){if(t.TileFrameX!=0||t.TileFrameY!=(depth-1)*18)throw new InvalidOperationException("Hanging frame/depth mismatch.");count++;}
            }
            Mod.Logger.Info($"MAW HANGING STATE PASS: all2000 cells; steps={steps}; fibers={count}; exact ordered frames; no auto-rebaseline.");
        }
        internal Rectangle CheckedBounds()
        {
            if(Bounds.Width!=100||Bounds.Height!=20||!WorldGen.InWorld(Bounds.Left,Bounds.Top,30)||!WorldGen.InWorld(Bounds.Right,Bounds.Bottom,30))throw new InvalidOperationException("Missing hanging bounds.");
            return Bounds;
        }
        private string Fingerprint()
        {
            CheckedBounds();var text=new System.Text.StringBuilder();
            for(int x=Bounds.Left;x<Bounds.Right;x++)for(int y=Bounds.Top;y<Bounds.Bottom;y++) {
                Tile t=Main.tile[x,y];text.Append($"{t.HasTile},{t.TileType},{t.WallType},{t.Slope},{t.IsHalfBlock},{t.TileFrameX},{t.TileFrameY},{t.WallFrameX},{t.WallFrameY},{t.LiquidAmount},{t.LiquidType},{t.HasActuator},{t.IsActuated},{t.RedWire},{t.BlueWire},{t.GreenWire},{t.YellowWire},{t.TileColor},{t.WallColor},{t.IsTileInvisible},{t.IsWallInvisible},{t.IsTileFullbright},{t.IsWallFullbright};");
            }
            return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text.ToString())));
        }
        internal void Audit()
        {
            CheckedBounds();
            for(int n=0;n<7;n++){Point root=Root(n);var s=new System.Text.StringBuilder();for(int d=0;d<=7;d++) {
                Tile t=Main.tile[root.X,root.Y+d];s.Append($"d{d}[has={t.HasTile},type={t.TileType},act={t.IsActuated},slope={t.Slope},liquid={t.LiquidAmount}/{t.LiquidType},frame={t.TileFrameX},{t.TileFrameY}] ");}
                Mod.Logger.Info($"MAW HANGING AUDIT: root{n} {root}: {s}");}
            Mod.Logger.Info("MAW HANGING AUDIT: current-only fingerprint="+Fingerprint()+"; not a replacement baseline.");
        }
        internal void Test(bool strictScene=true)
        {
            if(strictScene)Validate();else CheckedBounds();string before=Fingerprint();
            Point root=new(Bounds.X+92,Bounds.Y+4);Rectangle scratch=new(root.X-2,root.Y-2,5,12);
            for(int x=scratch.Left;x<scratch.Right;x++)for(int y=scratch.Top;y<scratch.Bottom;y++)if(!Empty(Main.tile[x,y]))throw new InvalidOperationException("Vine scratch occupied.");
            probe=root;int checks=0;
            void Require(bool condition,string name){if(!condition)throw new InvalidOperationException("Hanging check: "+name);checks++;}
            void Clear(){for(int x=scratch.Left;x<scratch.Right;x++)for(int y=scratch.Top;y<scratch.Bottom;y++)Main.tile[x,y].ClearEverything();}
            void Anchor(int type){Clear();Tile t=Main.tile[root];t.HasTile=true;t.TileType=(ushort)type;}
            void Mature(){for(int n=0;n<MawHangingFiber.MaxLength;n++)Require(MawHangingFiber.Grow(root,Fiber),"growth step");Require(!MawHangingFiber.Grow(root,Fiber),"length cap");}
            int Count(){int count=0;for(int y=1;y<=7;y++)if(Main.tile[root.X,root.Y+y].HasTile)count++;return count;}
            try {
                Require(!Main.tileSolid[Fiber]&&!Main.tileSolidTop[Fiber]&&!Main.tileRope[Fiber]&&Main.tileCut[Fiber]&&!TileID.Sets.VineThreads[Fiber],"cuttable rigid non-rope");
                foreach(string substrate in new[]{"soil","grass"}) {
                    int type=MawPackedPreview.TileType(substrate);
                    for(int shape=0;shape<6;shape++) {
                        Anchor(type);Tile shapedAnchor=Main.tile[root];shapedAnchor.Slope=(SlopeType)(shape==5?0:shape);shapedAnchor.IsHalfBlock=shape==5;
                        Require(MawHangingFiber.Grow(root,Fiber)==(shape!=3&&shape!=4),"flat-underside matrix");
                    }
                    foreach(int cut in new[]{1,3,6}) {
                        Anchor(type);Mature();WorldGen.KillTile(root.X,root.Y+cut,noItem:true);
                        Require(Count()==cut-1,"middle cut keeps prefix only");
                    }
                    Anchor(type);Mature();WorldGen.KillTile(root.X,root.Y,noItem:true);Require(Count()==0,"root removal cascades");
                    Anchor(type);Mature();Main.tile[root].TileType=TileID.Dirt;WorldGen.SquareTileFrame(root.X,root.Y);Require(Count()==0,"purified support clears");
                    Anchor(type);Mature();Tile changed=Main.tile[root];changed.IsActuated=true;WorldGen.SquareTileFrame(root.X,root.Y);Require(Count()==0,"actuated support clears");
                    Anchor(type);Mature();changed=Main.tile[root];changed.Slope=SlopeType.SlopeUpLeft;WorldGen.SquareTileFrame(root.X,root.Y);Require(Count()==0,"bottom-sloped support clears");
                    Anchor(type);Mature();Main.tile[root].TileType=(ushort)MawPackedPreview.TileType(substrate=="soil"?"grass":"soil");
                    WorldGen.SquareTileFrame(root.X,root.Y);Require(Count()==6,"allowed direct conversion retains fiber");
                }
                foreach(int liquid in new[]{LiquidID.Water,LiquidID.Lava,LiquidID.Honey,LiquidID.Shimmer})foreach(byte amount in new byte[]{1,255}) {
                    Anchor(MawPackedPreview.TileType("grass"));Tile t=Main.tile[root.X,root.Y+1];t.LiquidType=liquid;t.LiquidAmount=amount;
                    Require(!MawHangingFiber.Grow(root,Fiber),"wet destination denied");
                }
                // Exercise native liquid contact, not just the declared flags.
                foreach(int liquid in new[]{LiquidID.Water,LiquidID.Lava,LiquidID.Honey,LiquidID.Shimmer}) {
                    Anchor(MawPackedPreview.TileType("grass"));Mature();
                    Tile contact=Main.tile[root.X,root.Y+3];contact.LiquidType=liquid;contact.LiquidAmount=255;
                    Liquid.AddWater(root.X,root.Y+3);
                    Require(Count()==(liquid==LiquidID.Lava?2:6),"native liquid contact/detached suffix");
                }
                Anchor(MawPackedPreview.TileType("grass"));Tile staleLiquid=Main.tile[root.X,root.Y+1];staleLiquid.LiquidType=LiquidID.Lava;
                Require(MawHangingFiber.Grow(root,Fiber),"zero-amount stale liquid is dry");
                Anchor(MawPackedPreview.TileType("grass"));Tile a=Main.tile[root];a.TileColor=PaintID.RedPaint;a.IsTileInvisible=true;a.IsTileFullbright=true;
                Mature();for(int d=1;d<=6;d++){Tile t=Main.tile[root.X,root.Y+d];Require(t.TileColor==a.TileColor&&t.IsTileInvisible&&t.IsTileFullbright,"parent coating copied");}
                Anchor(MawPackedPreview.TileType("grass"));Mature();Main.tile[root.X,root.Y+3].ClearTile();Require(!MawHangingFiber.Grow(root,Fiber),"gap does not skip orphan");
                Anchor(MawPackedPreview.TileType("grass"));Mature();Tile overflow=Main.tile[root.X,root.Y+7];overflow.HasTile=true;overflow.TileType=(ushort)Fiber;
                WorldGen.SquareTileFrame(root.X,root.Y+7);Require(!overflow.HasTile,"overlength injected segment rejected");
                Require(!MawHangingFiber.Grow(new Point(root.X+1,root.Y),Fiber),"unregistered root denied");
            } finally {probe=null;Clear();if(before!=Fingerprint())throw new InvalidOperationException("Hanging probe changed preserved cells.");}
            if(strictScene)Validate();
            Mod.Logger.Info($"MAW HANGING LIFECYCLE PASS: {checks} native checks; support/cut/coating/length, dry-growth gates and native Liquid.AddWater contact; scratch restored. strictScene={strictScene}; a scratch-only pass does not clear a red saved exhibit. Fluid-flow time evolution and multiplayer remain untested.");
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name=="Apogee Native Visual V3"&&!Bounds.IsEmpty)tag["mawHangingFiberV1"]=new TagCompound{["x"]=Bounds.X,["y"]=Bounds.Y,["steps"]=steps};
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if(Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3"||!tag.ContainsKey("mawHangingFiberV1"))return;
            var saved=tag.GetCompound("mawHangingFiberV1");Bounds=new(saved.GetInt("x"),saved.GetInt("y"),100,20);steps=saved.GetInt("steps");
        }
        public override void ClearWorld(){Bounds=Rectangle.Empty;steps=0;probe=null;}
    }
}
