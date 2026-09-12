using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using apogean.Common.Maw;

namespace apogean.Content.Diagnostics
{
    public sealed class MawAmberLightStudy : ModSystem
    {
        internal Rectangle Bounds;
        private bool? preview;
        internal bool? StateAt(int x,int y)=>Main.netMode==NetmodeID.SinglePlayer&&Main.ActiveWorldFileData?.Name=="Apogee Native Visual V3"&&
            Main.LocalPlayer.name=="gg"&&Bounds.Contains(x,y)?preview:null;
        internal void SetPreview(bool? value)=>preview=value;
        private static int AmberTile=>ModContent.TileType<MawAmberLitTile>();
        private static int AmberWall=>ModContent.WallType<MawAmberLitWall>();
        private static bool Empty(Tile t)=>!t.HasTile&&t.WallType==WallID.None&&t.LiquidAmount==0&&!t.HasActuator&&!t.IsActuated&&
            !t.RedWire&&!t.GreenWire&&!t.BlueWire&&!t.YellowWire&&t.TileColor==PaintID.None&&t.WallColor==PaintID.None&&!t.IsTileInvisible&&!t.IsTileFullbright&&!t.IsWallInvisible&&!t.IsWallFullbright;
        private static (int Tile,int Wall) Cell(int x,int y)
        {
            if(x<4||x>=96||y<4||y>=48||(x>=46&&x<54))return(-1,0);
            int local=x<50?x-4:x-54;
            int wall=MawPackedPreview.Wall("stone").Type;
            if(local<2||local>=40||y<6||y>=46)return(MawPackedPreview.TileType("stone"),wall);
            if(x<50&&((x>=12&&x<24&&y>=29&&y<37)||(x>=31&&x<37&&y>=14&&y<20)))return(AmberTile,wall);
            if(x>=64&&x<84&&y>=18&&y<34)wall=AmberWall;
            return(-1,wall);
        }
        internal void Build(Rectangle parent)
        {
            if(!Bounds.IsEmpty)throw new InvalidOperationException("Amber study exists; no rebuild.");
            Rectangle candidate=Rectangle.Empty;
            // Bounded alternatives, read-only until one complete envelope passes.
            // Never clear a cloud, saved fixture or leftover tile metadata to fit.
            foreach(int dx in new[]{0,120,-120,240,-240}) {
                Rectangle site=new(parent.Left+dx,parent.Bottom+20,100,52),envelope=site;envelope.Inflate(3,3);
                if(!WorldGen.InWorld(envelope.Left,envelope.Top,30)||!WorldGen.InWorld(envelope.Right,envelope.Bottom,30))continue;
                bool empty=true;
                for(int x=envelope.Left;x<envelope.Right&&empty;x++)for(int y=envelope.Top;y<envelope.Bottom;y++)if(!Empty(Main.tile[x,y])) {
                    Mod.Logger.Info($"MAW AMBER SITE REJECTED: {site}; first occupied cell {x},{y}; no writes.");empty=false;break;
                }
                if(empty){candidate=site;break;}
            }
            if(candidate.IsEmpty)throw new InvalidOperationException("All five amber envelopes occupied; no writes.");
            Bounds=candidate;
            for(int x=0;x<100;x++)for(int y=0;y<52;y++){
                var c=Cell(x,y);Tile t=Main.tile[Bounds.X+x,Bounds.Y+y];
                if(c.Tile>=0){t.HasTile=true;t.TileType=(ushort)c.Tile;}t.WallType=(ushort)c.Wall;
            }
            WorldGen.RangeFrame(Bounds.Left-1,Bounds.Top-1,Bounds.Right+1,Bounds.Bottom+1);Validate();
            Mod.Logger.Info($"MAW AMBER BUILD: {Bounds}; separated terrain and unsafe-wall light chambers; unchanged atlas art.");
        }
        internal void Validate()
        {
            if(Bounds.Width!=100||Bounds.Height!=52||!WorldGen.InWorld(Bounds.Left,Bounds.Top,30)||!WorldGen.InWorld(Bounds.Right,Bounds.Bottom,30))throw new InvalidOperationException("Missing amber study.");
            for(int x=0;x<100;x++)for(int y=0;y<52;y++){
                var c=Cell(x,y);Tile t=Main.tile[Bounds.X+x,Bounds.Y+y];
                if(t.HasTile!=(c.Tile>=0)||(c.Tile>=0&&t.TileType!=c.Tile)||t.WallType!=c.Wall||t.Slope!=SlopeType.Solid||t.IsHalfBlock||t.LiquidAmount!=0||
                    t.HasActuator||t.IsActuated||t.RedWire||t.GreenWire||t.BlueWire||t.YellowWire||t.TileColor!=PaintID.None||t.WallColor!=PaintID.None||
                    t.IsTileInvisible||t.IsTileFullbright||t.IsWallInvisible||t.IsWallFullbright)throw new InvalidOperationException($"Amber scene differs at{x},{y}; no repair.");
            }
            Mod.Logger.Info("MAW AMBER STATE PASS: all5200 cell states exact; no automatic rebaseline.");
        }
        internal void Sample()
        {
            Validate();
            foreach(Point p in new[]{new Point(20,28),new Point(70,25),new Point(7,8)})
                Mod.Logger.Info($"MAW AMBER LIGHT SAMPLE: preview={preview}; local={p}; nativeLighting={Lighting.GetColor(Bounds.X+p.X,Bounds.Y+p.Y)}; worldDormant={MawActivityState.IsDormant}.");
        }
        internal void Test()
        {
            Validate();bool state=MawActivityState.IsDormant;int checks=0,litTiles=0,litWalls=0;
            var tileType=ModContent.GetInstance<MawAmberLitTile>();var wallType=ModContent.GetInstance<MawAmberLitWall>();
            Point? tileProbe=null,wallProbe=null;
            void Require(bool value,string name){if(!value)throw new InvalidOperationException("Amber check: "+name);checks++;}
            for(int x=Bounds.Left;x<Bounds.Right;x++)for(int y=Bounds.Top;y<Bounds.Bottom;y++){
                Tile t=Main.tile[x,y];
                if(t.HasTile&&t.TileType==AmberTile){var a=tileType.Emission(x,y,false);Require((tileType.Emission(x,y,true)-a*.32f).Length()<.00001f,"tile dormant ratio");if(a.X>0){litTiles++;tileProbe=new(x,y);}}
                if(t.WallType==AmberWall){var a=wallType.Emission(x,y,false);Require((wallType.Emission(x,y,true)-a*.32f).Length()<.00001f,"wall dormant ratio");if(a.X>0){litWalls++;wallProbe=new(x,y);}}
            }
            Require(litTiles>0&&litWalls>0,"actual atlas emission lookup");
            Require(!Main.wallHouse[AmberWall],"not housing-safe");
            foreach(string key in new[]{"soil","stone","bone","grass"})Require(!Main.tileLighted[MawPackedPreview.TileType(key)],"quiet material "+key);
            // Synchronous metadata probes on known emitters: restore before any
            // game tick, and never replace the saved geometry or frame baseline.
            Point tp=tileProbe.Value,wp=wallProbe.Value;Tile pt=Main.tile[tp],pw=Main.tile[wp];
            byte oldPaint=pt.TileColor;bool oldFullbright=pt.IsTileFullbright;
            bool oldInvisible=pt.IsTileInvisible,oldActuated=pt.IsActuated;
            ushort oldType=pw.TileType;bool oldHas=pw.HasTile,oldWallInvisible=pw.IsWallInvisible;
            try {
                Vector3 original=tileType.Emission(tp.X,tp.Y,false);
                pt.IsTileInvisible=true;Require(tileType.Emission(tp.X,tp.Y,false)==Vector3.Zero,"echo tile quiet");pt.IsTileInvisible=oldInvisible;
                pt.IsActuated=true;Require(tileType.Emission(tp.X,tp.Y,false)==Vector3.Zero,"actuated tile quiet");pt.IsActuated=oldActuated;
                pt.TileColor=PaintID.RedPaint;Require(tileType.Emission(tp.X,tp.Y,false)==original,"paint does not manufacture light");pt.TileColor=oldPaint;
                pt.IsTileFullbright=true;Require(tileType.Emission(tp.X,tp.Y,false)==original,"illuminant coating does not amplify emitted light");pt.IsTileFullbright=oldFullbright;
                pw.IsWallInvisible=true;Require(wallType.Emission(wp.X,wp.Y,false)==Vector3.Zero,"echo wall quiet");pw.IsWallInvisible=oldWallInvisible;
                pw.HasTile=true;pw.TileType=TileID.Stone;Require(wallType.Emission(wp.X,wp.Y,false)==Vector3.Zero,"solid cover occludes wall emitter");
            } finally {
                pt.TileColor=oldPaint;pt.IsTileFullbright=oldFullbright;pt.IsTileInvisible=oldInvisible;pt.IsActuated=oldActuated;
                pw.HasTile=oldHas;pw.TileType=oldType;pw.IsWallInvisible=oldWallInvisible;
            }
            bool? oldPreview=preview;try{preview=true;Require(StateAt(Bounds.X,Bounds.Y)==true,"local preview");Require(StateAt(Bounds.Left-1,Bounds.Top)==null,"preview bounded");
                float r=0,g=0,b=0;tileType.ModifyLight(tp.X,tp.Y,ref r,ref g,ref b);
                Require((new Vector3(r,g,b)-tileType.Emission(tp.X,tp.Y,true)).Length()<.00001f,"native tile hook uses scoped dormancy");
                wallType.ModifyLight(wp.X,wp.Y,ref r,ref g,ref b);
                Require((new Vector3(r,g,b)-wallType.Emission(wp.X,wp.Y,true)).Length()<.00001f,"native wall hook uses scoped dormancy");
                preview=false;Require(StateAt(Bounds.X,Bounds.Y)==false,"awake preview");}finally{preview=oldPreview;}
            Require(state==MawActivityState.IsDormant,"world progression unchanged");Validate();
            Mod.Logger.Info($"MAW AMBER PROPERTIES PASS: {checks}; {litTiles} terrain and {litWalls} wall emitter cells; dormant=.32 awake; native hook, echo/actuation/paint/fullbright/solid-cover probes restored. GPU coating appearance, multiplayer and sloped-light coverage remain separate gates.");
        }
        public override void SaveWorldData(TagCompound tag){if(Main.ActiveWorldFileData?.Name=="Apogee Native Visual V3"&&!Bounds.IsEmpty)tag["mawAmberStudyV1"]=new TagCompound{["x"]=Bounds.X,["y"]=Bounds.Y};}
        public override void LoadWorldData(TagCompound tag){if(Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3"||!tag.ContainsKey("mawAmberStudyV1"))return;
            var t=tag.GetCompound("mawAmberStudyV1");Bounds=new(t.GetInt("x"),t.GetInt("y"),100,52);}
        public override void ClearWorld(){Bounds=Rectangle.Empty;preview=null;}
    }
}
