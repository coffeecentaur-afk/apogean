using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Projectiles;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
    // Finite live-tick probe. Only initially empty cells and a newly created,
    // identity-matched falling projectile are owned. No manual projectile AI.
    internal sealed class MawSandPhysicsProbe
    {
        private readonly Rectangle area;
        private readonly Point source, landing;
        private readonly log4net.ILog log;
        private readonly int sand = ModContent.TileType<MawSand>();
        private readonly int falling = ModContent.ProjectileType<MawSandBallFallingProjectile>();
        private readonly bool[] existing = new bool[Main.maxProjectiles];
        private int slot = -1, identity, ticks;
        private bool moved, closed;
        internal bool Finished => closed;
        internal bool Passed { get; private set; }

        internal MawSandPhysicsProbe(Rectangle area, log4net.ILog log)
        {
            this.area=area; this.log=log;
            source=new Point(area.Center.X,area.Top+4);
            landing=new Point(area.Center.X,area.Bottom-2);
            if(Main.netMode!=NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name!="Apogee Native Visual V3" || Main.LocalPlayer.name!="gg")
                throw new InvalidOperationException("Sand probe requires gg/V3/SP.");
            for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++) {
                Tile t=Main.tile[x,y];
                if(t.HasTile || t.WallType!=WallID.None || t.LiquidAmount!=0 || t.HasActuator || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire || t.TileColor!=PaintID.None || t.WallColor!=PaintID.None)
                    throw new InvalidOperationException("Sand probe envelope occupied; nothing cleared.");
            }
            for(int n=0;n<existing.Length;n++)existing[n]=Main.projectile[n].active;
            try {
                for(int x=area.Left;x<area.Right;x++)Place(x,area.Bottom-1,TileID.GrayBrick);
                Place(source.X,source.Y+1,TileID.GrayBrick);
                WorldGen.PlaceTile(source.X,source.Y,sand,mute:true);
                if(!Main.tile[source].HasTile || Main.tile[source].TileType!=sand)throw new InvalidOperationException("Native sand placement failed.");
                WorldGen.KillTile(source.X,source.Y+1,noItem:true);
                WorldGen.SquareTileFrame(source.X,source.Y);
                log.Info($"MAW SAND START: source={source}; expected landing={landing}; empty owned envelope={area}; real game ticks, native support removal.");
                FindProjectile();
            } catch { Cancel(); throw; }
        }
        private static void Place(int x,int y,int type)
        {
            WorldGen.PlaceTile(x,y,type,mute:true);
            if(!Main.tile[x,y].HasTile || Main.tile[x,y].TileType!=type)throw new InvalidOperationException("Native test support placement failed.");
        }
        private void FindProjectile()
        {
            if(slot>=0)return;
            for(int n=0;n<existing.Length;n++) {
                Projectile p=Main.projectile[n];
                if(!existing[n] && p.active && p.type==falling && area.Contains(p.Center.ToTileCoordinates())) {
                    if(slot>=0)throw new InvalidOperationException("Multiple falling projectiles; ambiguous sand result.");
                    slot=n;identity=p.identity;
                }
            }
        }
        internal void Tick()
        {
            if(closed)return;
            try {
                ticks++; FindProjectile();
                if(slot<0 && ticks>5)throw new InvalidOperationException("Support removal spawned no Maw falling projectile.");
                if(slot>=0) {
                    Projectile p=Main.projectile[slot];
                    if(p.active && p.identity==identity && p.type==falling) {
                        if(!area.Contains(p.Center.ToTileCoordinates()))throw new InvalidOperationException("Owned sand projectile left its test envelope.");
                        moved |= p.Center.Y>(source.Y+2)*16;
                    } else {
                        int count=0;
                        for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++)if(Main.tile[x,y].HasTile && Main.tile[x,y].TileType==sand)count++;
                        bool landed=Main.tile[landing].HasTile && Main.tile[landing].TileType==sand;
                        if(!moved || !landed || count!=1 || Main.tile[source].HasTile)
                            throw new InvalidOperationException($"Sand failed to return as exactly one tile: moved={moved}; landed={landed}; count={count}.");
                        Passed=true;
                        log.Info($"MAW SAND PHYSICS PASS: native support loss -> identity {identity} -> downward motion -> exactly one MawSand at {landing}; {ticks} actual game ticks.");
                        Cancel();
                    }
                }
                if(ticks>240)throw new InvalidOperationException("Sand did not settle within 240 game ticks.");
            } catch(Exception ex) { log.Error("MAW SAND PHYSICS FAIL: "+ex.Message); Cancel(); }
        }
        internal void Cancel()
        {
            if(closed)return;closed=true;
            if(slot>=0) {
                Projectile p=Main.projectile[slot];
                if(p.active && p.identity==identity && p.type==falling)p.active=false;
            }
            // Remove only our known support cells and expected sand type inside
            // the original empty envelope. Never clear an unexpected foreign cell.
            for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++) {
                Tile t=Main.tile[x,y];
                bool support=(y==area.Bottom-1 || (x==source.X && y==source.Y+1)) && t.TileType==TileID.GrayBrick;
                if(t.HasTile && (support || t.TileType==sand))t.ClearEverything();
            }
            log.Info("MAW SAND CLEANUP: owned supports/sand/projectile released; caller must independently verify original fixture hash.");
        }
    }
}
