using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    internal static class MawAnatomyChecks
    {
        // Actual native collision solver; uses only a previously empty scratch
        // pad, never cuts a saved rib or replaces the scene's reference hash.
        internal static void Run(Rectangle scene, log4net.ILog log)
        {
            Point center=new(scene.X+95,scene.Y+72);
            Rectangle area=new(center.X-3,center.Y-3,7,7);
            for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++) {
                Tile t=Main.tile[x,y];
                if(t.HasTile||t.WallType!=WallID.None||t.LiquidAmount!=0||t.HasActuator||t.IsActuated||t.RedWire||t.BlueWire||t.GreenWire||t.YellowWire||
                    t.TileColor!=PaintID.None||t.WallColor!=PaintID.None||t.IsTileInvisible||t.IsWallInvisible||t.IsTileFullbright||t.IsWallFullbright)
                    throw new InvalidOperationException("Anatomy scratch occupied; no writes.");
            }
            int checks=0,negativeHits=0;
            void Require(bool v,string name){if(!v)throw new InvalidOperationException("Anatomy properties: "+name);checks++;}
            void Clear(){for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++)Main.tile[x,y].ClearEverything();}
            int rib=MawAnatomyMaterials.Tile("rib").Type,cap=MawAnatomyMaterials.Tile("cap").Type;
            bool oldSolid=Main.tileSolid[rib];
            try {
                foreach(int type in new[]{rib,cap}) {
                    Require(Main.tileSolid[type]&&!Main.tileSolidTop[type]&&!Main.tileRope[type],"solid, not platform/rope");
                    Require(!Main.tileLighted[type]&&TileID.Sets.TouchDamageImmediate[type]==0&&!TileID.Sets.TouchDamageHot[type]&&!TileID.Sets.TouchDamageBleeding[type],
                        "quiet harmless structural material");
                }
                Require(TileLoader.GetTile(rib).MinPick==59,"structural bone mining gate");
                foreach(string key in new[]{"soil","fibers","bone","membrane","amber","stone"})
                    Require(!Main.wallHouse[MawPackedPreview.Wall(key).Type],"unsafe wall "+key);
                Vector2 origin=center.ToVector2()*16;
                foreach(bool disabled in new[]{false,true}) {
                    Main.tileSolid[rib]=!disabled;
                    for(int shape=0;shape<6;shape++)for(int ox=-4;ox<=20;ox+=4)for(int oy=-4;oy<=20;oy+=4) {
                        Vector4[] results=new Vector4[2];
                        for(int native=0;native<2;native++) {
                            Clear();Tile t=Main.tile[center];t.HasTile=true;t.TileType=(ushort)(native==0?rib:TileID.Stone);
                            t.IsHalfBlock=shape==5;t.Slope=(SlopeType)(shape==5?0:shape);
                            results[native]=Collision.SlopeCollision(origin+new Vector2(ox,oy),Vector2.Zero,2,2);
                        }
                        bool equal=results[0]==results[1];
                        if(disabled){if(!equal)negativeHits++;}else Require(equal,$"native slope {shape}/{ox}/{oy}");
                    }
                }
                Require(negativeHits>0,"disabled-solid negative control");
                Main.tileSolid[rib]=oldSolid;
                foreach(int type in new[]{rib,cap}) {
                    Clear();Tile t=Main.tile[center];t.HasTile=true;t.TileType=(ushort)type;
                    WorldGen.KillTile(center.X,center.Y,noItem:true);
                    Require(!Main.tile[center].HasTile,"native kill does not leave ghost tile");
                }
            } finally {Main.tileSolid[rib]=oldSolid;Clear();}
            log.Info($"MAW ANATOMY PHYSICS PASS: {checks} assertions; 294 native slope/half-block comparisons; non-solid mutation detected in {negativeHits} samples; scratch restored. No player traversal or multiplayer claim.");
        }
    }
}
