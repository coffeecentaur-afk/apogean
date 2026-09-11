using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
    // Compare INNER join masks against native controls. An atlas matching its
    // own native frame does not prove the engine chose the right join frame.
    // This measures base draws; it does not certify every native overlay.
    internal static class MawMergeCoverageChecks
    {
        internal static void Run(Point p,log4net.ILog log)
        {
            if(!MawPackedPreview.Enabled || Main.netMode!=NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3" || Main.LocalPlayer.name!="gg")
                throw new InvalidOperationException("Join check requires packed QA, gg/V3/SP.");
            Rectangle area=new(p.X-2,p.Y-2,5,5);
            for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++) {
                Tile t=Main.tile[x,y];
                if(t.HasTile || t.WallType!=WallID.None || t.LiquidAmount!=0 || t.HasActuator || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire || t.TileColor!=PaintID.None || t.WallColor!=PaintID.None)
                    throw new InvalidOperationException("Join trial occupied; nothing cleared.");
            }
            var cache=new Dictionary<Texture2D,Color[]>();
            int Alpha(int x,int y,int px,int py) {
                Tile t=Main.tile[x,y]; Main.instance.LoadTiles(t.TileType);
                Texture2D texture=TextureAssets.Tile[t.TileType].Value;
                if(!cache.TryGetValue(texture,out var data)){data=new Color[texture.Width*texture.Height];texture.GetData(data);cache.Add(texture,data);}
                short fx=t.TileFrameX,fy=t.TileFrameY;int w=16,h=16,off=0;
                TileLoader.SetDrawPositions(x,y,ref w,ref off,ref h,ref fx,ref fy);
                return data[(fy+py)*texture.Width+fx+px].A;
            }
            void Clear() { for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++)Main.tile[x,y].ClearEverything(); }
            int Sample(int a,int b,bool horizontal) {
                Clear();
                for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++) {
                    Tile t=Main.tile[x,y];t.HasTile=true;
                    t.TileType=(ushort)(x==area.Left || x==area.Right-1 || y==area.Top || y==area.Bottom-1 ? TileID.GrayBrick : (horizontal?y<=p.Y:x<=p.X)?a:b);
                }
                WorldGen.RangeFrame(area.Left,area.Top,area.Right,area.Bottom);
                int holes=0;
                for(int edge=0;edge<4;edge++)for(int along=4;along<12;along++) {
                    if(horizontal) { if(Alpha(p.X,p.Y,along,12+edge)==0)holes++; if(Alpha(p.X,p.Y+1,along,edge)==0)holes++; }
                    else { if(Alpha(p.X,p.Y,12+edge,along)==0)holes++; if(Alpha(p.X+1,p.Y,edge,along)==0)holes++; }
                }
                return holes;
            }
            var materials=new (string Key,int Native)[]{("stone",TileID.Stone),("sand",TileID.Sand),("mud",TileID.Mud),("clay",TileID.ClayBlock),("snow",TileID.SnowBlock),("ice",TileID.IceBlock)};
            int soil=ModContent.TileType<MawDirt>();
            int mud=ModContent.TileType<MawMud>();
            bool originalDirt=TileID.Sets.Dirt[soil];
            bool originalMud=TileID.Sets.Mud[mud];
            int baselineExcess=-1,negativeExcess=-1,mudExcess=-1;
            try {
              foreach(string trial in new[]{"baseline","missing-soil-classifications","missing-mud-classification"}) {
                TileID.Sets.Dirt[soil]=trial=="missing-soil-classifications"?false:originalDirt;
                TileID.Sets.Mud[mud]=trial=="baseline"?originalMud:false;
                int extra=0,checks=0;
                foreach(var m in materials)foreach(bool horizontal in new[]{false,true}) {
                    int native=Sample(TileID.Dirt,m.Native,horizontal);
                    int maw=Sample(soil,MawPackedPreview.TileType(m.Key),horizontal);
                    checks++;if(maw>native)extra++;
                    log.Info($"MAW SEAM SAMPLE: trial={trial}; soil/{m.Key}; axis={(horizontal?"Y":"X")}; native base gaps={native}/64; Maw base gaps={maw}/64; excess={maw>native}.");
                }
                log.Info($"MAW SEAM BASEMASK RESULT: trial={trial}; {checks-extra}/{checks} no excess compared with native controls; {extra} require draw/merge review. Not whole-scene visual acceptance.");
                if(trial=="baseline")baselineExcess=extra;
                else if(trial=="missing-soil-classifications")negativeExcess=extra;
                else mudExcess=extra;
              }
              if(baselineExcess!=0)throw new InvalidOperationException("Actual Maw joins still exceed native base-mask gaps.");
              if(negativeExcess<8)throw new InvalidOperationException("Missing-classification mutation failed to expose the original eight defects.");
              if(mudExcess<2)throw new InvalidOperationException("Missing-mud mutation failed to expose the two remaining defects.");
              log.Info("MAW SEAM REGRESSION PASS: 12 native comparisons; missing soil classifications rejected in eight joins; missing mud rejected in two. Complete scene acceptance remains separate.");
            } finally { TileID.Sets.Dirt[soil]=originalDirt;TileID.Sets.Mud[mud]=originalMud;Clear();cache.Clear(); }
        }
    }
}
