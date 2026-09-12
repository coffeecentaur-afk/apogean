using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    // Copies the actual rib roots to an empty scratch pad. Never edits saved
    // ribs. Base alpha is evidence, not a complete grass-overlay render oracle.
    internal static class MawRibJoinChecks
    {
        internal static void Run(Rectangle scene, log4net.ILog log)
        {
            Point pad = new(scene.X+88,scene.Y+62);
            Rectangle area = new(pad.X-3,pad.Y-3,7,7);
            for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++) {
                Tile t=Main.tile[x,y];
                if(t.HasTile||t.WallType!=WallID.None||t.LiquidAmount!=0||t.HasActuator||t.IsActuated||t.RedWire||t.BlueWire||t.GreenWire||t.YellowWire||
                    t.TileColor!=PaintID.None||t.WallColor!=PaintID.None||t.IsTileInvisible||t.IsWallInvisible||t.IsTileFullbright||t.IsWallFullbright)
                    throw new InvalidOperationException("Rib join scratch envelope occupied; no writes.");
            }
            int bone=MawPackedPreview.TileType("bone"),soil=MawPackedPreview.TileType("soil"),grass=MawPackedPreview.TileType("grass"),stone=MawPackedPreview.TileType("stone");
            bool bs=Main.tileMerge[bone][soil],sb=Main.tileMerge[soil][bone],bg=Main.tileMerge[bone][grass],gb=Main.tileMerge[grass][bone];
            bool checksForMerge=TileID.Sets.ChecksForMerge[bone],mergeDirt=Main.tileMergeDirt[bone];
            var cache=new Dictionary<Texture2D,Color[]>();
            int failures=0,comparisons=0,negativeDirt=0,negativeGrass=0;
            void Check(string trial,int actual,int native) {
                if(trial is "installed" or "restored") { comparisons++;if(actual!=native)failures++; }
                if(trial=="no-dirt"&&actual>native)negativeDirt++;
                if(trial=="no-grass"&&actual>native)negativeGrass++;
            }
            void Clear(){for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++)Main.tile[x,y].ClearEverything();}
            int Holes(int x,int y) {
                Tile t=Main.tile[x,y];if(!t.HasTile)return 0;
                Main.instance.LoadTiles(t.TileType);Texture2D texture=TextureAssets.Tile[t.TileType].Value;
                if(!cache.TryGetValue(texture,out Color[] data)){data=new Color[texture.Width*texture.Height];texture.GetData(data);cache.Add(texture,data);}
                short fx=t.TileFrameX,fy=t.TileFrameY;int w=16,h=16,offset=0;TileLoader.SetDrawPositions(x,y,ref w,ref offset,ref h,ref fx,ref fy);
                int holes=0;for(int py=0;py<16;py++)for(int px=0;px<16;px++)if(data[(fy+py)*texture.Width+fx+px].A==0)holes++;
                return holes;
            }
            int Native(int type)=>type==bone||type==stone?TileID.Stone:type==soil?TileID.Dirt:type==grass?TileID.Grass:type;
            try {
                foreach(string trial in new[]{"installed","no-dirt","no-grass","neither","restored"}) {
                    Main.tileMerge[bone][soil]=bs;Main.tileMerge[soil][bone]=sb;
                    Main.tileMerge[bone][grass]=bg && trial is not ("no-grass" or "neither");
                    Main.tileMerge[grass][bone]=gb && trial is not ("no-grass" or "neither");
                    TileID.Sets.ChecksForMerge[bone]=checksForMerge;
                    Main.tileMergeDirt[bone]=mergeDirt && trial is not ("no-dirt" or "neither");
                    foreach(Point root in new[]{new Point(20,27),new Point(23,28),new Point(49,44),new Point(45,46)}) {
                        int saved=0;for(int dx=-1;dx<=1;dx++)for(int dy=-1;dy<=1;dy++)saved+=Holes(scene.X+root.X+dx,scene.Y+root.Y+dy);
                        int[] result=new int[2];
                        for(int native=0;native<2;native++) {
                            Clear();
                            for(int dx=-3;dx<=3;dx++)for(int dy=-3;dy<=3;dy++) {
                                Tile source=Main.tile[scene.X+root.X+dx,scene.Y+root.Y+dy],t=Main.tile[pad.X+dx,pad.Y+dy];
                                if(!source.HasTile)continue;t.HasTile=true;t.TileType=(ushort)(native==1?Native(source.TileType):source.TileType);
                                t.Slope=source.Slope;t.IsHalfBlock=source.IsHalfBlock;
                            }
                            WorldGen.RangeFrame(area.Left,area.Top,area.Right,area.Bottom);
                            for(int dx=-1;dx<=1;dx++)for(int dy=-1;dy<=1;dy++)result[native]+=Holes(pad.X+dx,pad.Y+dy);
                        }
                        log.Info($"MAW RIB JOIN: trial={trial}; root={root.X},{root.Y}; savedBaseHoles={saved}; reframed={result[0]}; native={result[1]}; extra={result[0]-result[1]}; grass overlays excluded.");
                        Check(trial,result[0],result[1]);
                    }
                    foreach(int other in new[]{soil,grass})for(int axis=0;axis<2;axis++)for(int order=0;order<2;order++) {
                        int[] result=new int[2];
                        for(int native=0;native<2;native++) {
                            Clear();for(int dx=-3;dx<=3;dx++)for(int dy=-3;dy<=3;dy++) {
                                bool first=(axis==0?dx<=0:dy<=0)^(order==1);int type=first?bone:other;
                                Tile t=Main.tile[pad.X+dx,pad.Y+dy];t.HasTile=true;t.TileType=(ushort)(native==1?Native(type):type);
                            }
                            WorldGen.RangeFrame(area.Left,area.Top,area.Right,area.Bottom);
                            for(int dx=-1;dx<=1;dx++)for(int dy=-1;dy<=1;dy++)result[native]+=Holes(pad.X+dx,pad.Y+dy);
                        }
                        log.Info($"MAW RIB ENCLOSED: trial={trial}; other={(other==soil?"soil":"grass")}; axis={axis}; order={order}; holes={result[0]}; native={result[1]}; extra={result[0]-result[1]}.");
                        Check(trial,result[0],result[1]);
                    }
                }
            } finally {
                Main.tileMerge[bone][soil]=bs;Main.tileMerge[soil][bone]=sb;Main.tileMerge[bone][grass]=bg;Main.tileMerge[grass][bone]=gb;Clear();
                TileID.Sets.ChecksForMerge[bone]=checksForMerge;Main.tileMergeDirt[bone]=mergeDirt;
            }
            if(failures!=0||comparisons!=24||negativeDirt==0||negativeGrass==0)
                throw new InvalidOperationException($"Rib joins failed: mismatches={failures}; comparisons={comparisons}; negative dirt={negativeDirt},grass={negativeGrass}.");
            log.Info($"MAW RIB JOIN PASS: {comparisons} native comparisons; disabled dirt rule detected={negativeDirt}; disabled grass pair detected={negativeGrass}; all rules restored; scratch empty; saved scene unchanged. Base-alpha only, not grass overlays or art approval.");
        }
    }
}
