using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.WorldGeneration;

namespace apogean.Content.Diagnostics
{
    // Reversible native-framing probe, not a repair of the user's played candidate.
    internal static class MawSeedSeamProbe
    {
        internal static void Run(MawSeedWorld world, log4net.ILog log, bool trials=false)
        {
            if (Main.netMode != NetmodeID.SinglePlayer || Main.gameMenu ||
                !MawSeedWorld.IsTestWorldName(Main.ActiveWorldFileData?.Name) || !world.HasLayout ||
                Main.LocalPlayer.name is not ("gg" or "Maw QA Plain"))
                throw new InvalidOperationException("Seed seam probe requires the disposable seed world.");
            var plan = world.Blueprint;
            Rectangle b = world.Bounds;
            int rib = MawAnatomyMaterials.Tile("rib").Type, cap = MawAnatomyMaterials.Tile("cap").Type;
            int grass = MawPackedPreview.TileType("grass"), soil = MawPackedPreview.TileType("soil");
            int buried = 0, missing = 0, surfaceUnbacked = 0;
            for (int x=4;x<MawSeedPlan.Width-4;x++) for(int y=4;y<MawSeedPlan.Height-4;y++) {
                if(plan.Cells[x,y].Key!="rib")continue;
                Tile t=Main.tile[b.X+x,b.Y+y];
                // Surface ridges intentionally do not paint backing over the sky.
                // Use the same explicit backing mask as the native writer, not a
                // separate Ground+8 heuristic that labels exposed lips as buried.
                if(!plan.AllowsWall(x,y)){if(t.WallType==WallID.None)surfaceUnbacked++;continue;}
                buried++;
                if(t.WallType==WallID.None) {
                    if(missing++<12)log.Info($"MAW SEED ROOT PROBE: {b.X+x},{b.Y+y}; plannedWall={plan.Cells[x,y].Wall??"none"}; nativeWall={t.WallType}; slope={t.Slope}; tile={t.TileType}; light={Lighting.GetColor(b.X+x,b.Y+y)}");
                }
            }
            log.Info($"MAW SEED ROOT RESULT: version={world.LayoutVersion}; requiredBackingRibCells={buried}; missingBacking={missing}; exposedSurfaceRibCells={surfaceUnbacked}; read-only, no wall-deletion attribution.");
            // Empty sky above the candidate, searched finitely; a refusal never clears occupied cells.
            Rectangle area=Rectangle.Empty;
            for(int dx=12;dx<80;dx+=12) {
                Rectangle candidate=new(b.Left+dx,b.Top-18,11,11);bool empty=WorldGen.InWorld(candidate.Left,candidate.Top,20);
                for(int x=candidate.Left;x<candidate.Right&&empty;x++)for(int y=candidate.Top;y<candidate.Bottom;y++) {
                    Tile t=Main.tile[x,y];empty&=!t.HasTile&&t.WallType==WallID.None&&t.LiquidAmount==0&&!t.HasActuator&&!t.IsActuated&&!t.RedWire&&!t.BlueWire&&!t.GreenWire&&!t.YellowWire;
                }
                if(empty){area=candidate;break;}
            }
            if(area==Rectangle.Empty)throw new InvalidOperationException("No empty seed seam scratch; no writes.");
            var saved=new State[area.Width,area.Height];
            for(int x=0;x<area.Width;x++)for(int y=0;y<area.Height;y++)saved[x,y]=State.Read(area.X+x,area.Y+y);
            var cache=new Dictionary<Texture2D,Color[]>();
            int AlphaHoles(int x,int y) {
                Tile t=Main.tile[x,y];Main.instance.LoadTiles(t.TileType);
                Texture2D texture=TextureAssets.Tile[t.TileType].Value;
                if(!cache.TryGetValue(texture,out Color[] pixels)){pixels=new Color[texture.Width*texture.Height];texture.GetData(pixels);cache.Add(texture,pixels);}
                short fx=t.TileFrameX,fy=t.TileFrameY;int w=16,h=16,off=0;
                TileLoader.SetDrawPositions(x,y,ref w,ref off,ref h,ref fx,ref fy);
                int n=0;
                // Interior bottom strip, not the exposed top fringe or slope's empty triangle.
                for(int py=12;py<16;py++)for(int px=2;px<14;px++)if(pixels[(fy+py)*texture.Width+fx+px].A==0)n++;
                return n;
            }
            int Sample(bool native,int shape) {
                for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++)Main.tile[x,y].ClearEverything();
                for(int dx=2;dx<9;dx++) {
                    int top=shape==0?3:shape==1?3+(dx>=5?1:0):3+(dx<=4?1:0);
                    for(int dy=top;dy<9;dy++) {
                        Tile t=Main.tile[area.X+dx,area.Y+dy];t.HasTile=true;
                        t.TileType=(ushort)(dy<top+2?(native?TileID.Grass:cap):dy<top+4?(native?TileID.Grass:grass):(native?TileID.Dirt:soil));
                    }
                }
                WorldGen.RangeFrame(area.Left+1,area.Top+1,area.Right-1,area.Bottom-1);
                int count=0;
                for(int dx=3;dx<8;dx++) {
                    int top=shape==0?3:shape==1?3+(dx>=5?1:0):3+(dx<=4?1:0);
                    count+=AlphaHoles(area.X+dx,area.Y+top+1);
                }
                return count;
            }
            int excess=0;
            try {
                for(int shape=0;shape<3;shape++) {
                    int control=Sample(true,shape),actual=Sample(false,shape);
                    log.Info($"MAW SEED CAP PROBE: shape={shape}; nativeBottomHoles={control}; capBottomHoles={actual}; substrate={TileID.Sets.NeedsGrassFramingDirt[cap]}; 5 actual mapped native frames.");
                    excess+=Math.Max(0,actual-control);
                }
                if(trials) {
                    int host=TileID.Sets.NeedsGrassFramingDirt[cap];
                    bool isGrass=TileID.Sets.Grass[cap],needs=TileID.Sets.NeedsGrassFraming[cap],mergeDirt=Main.tileMergeDirt[cap];
                    try {
                        foreach(string trial in new[]{"host-grass","host-soil-control","ordinary-connected","merge-dirt","restored"}) {
                            TileID.Sets.NeedsGrassFramingDirt[cap]=trial=="host-grass"?grass:trial=="host-soil-control"?soil:host;
                            TileID.Sets.Grass[cap]=trial=="ordinary-connected"?false:isGrass;
                            TileID.Sets.NeedsGrassFraming[cap]=trial=="ordinary-connected"?false:needs;
                            Main.tileMergeDirt[cap]=trial=="merge-dirt"||mergeDirt;
                            for(int shape=0;shape<3;shape++)log.Info($"MAW SEED CAP TRIAL: {trial}; shape={shape}; bottomHoles={Sample(false,shape)}.");
                        }
                    } finally {
                        TileID.Sets.NeedsGrassFramingDirt[cap]=host;TileID.Sets.Grass[cap]=isGrass;
                        TileID.Sets.NeedsGrassFraming[cap]=needs;Main.tileMergeDirt[cap]=mergeDirt;
                    }
                }
            } finally {
                for(int x=0;x<area.Width;x++)for(int y=0;y<area.Height;y++)saved[x,y].Restore(area.X+x,area.Y+y);
                for(int x=0;x<area.Width;x++)for(int y=0;y<area.Height;y++)if(State.Read(area.X+x,area.Y+y)!=saved[x,y])throw new InvalidOperationException("Seed seam scratch restoration mismatch.");
                cache.Clear();
                log.Info("MAW SEED SEAM SCRATCH RESTORED: all121 native states; no saved scene reframe or repair.");
            }
            if(excess>0||missing>0)throw new InvalidOperationException($"Seed seam regression: excess bottom alpha={excess}; missing buried backing={missing}.");
            log.Info("MAW SEED SEAM PASS: actual cap frames and buried root backing; viewport visual review still separate.");
        }
        private readonly record struct State(TileTypeData Type,WallTypeData Wall,TileWallWireStateData Bits,LiquidData Liquid,TileWallBrightnessInvisibilityData Coating)
        {
            internal static State Read(int x,int y){Tile t=Main.tile[x,y];return new(t.Get<TileTypeData>(),t.Get<WallTypeData>(),t.Get<TileWallWireStateData>(),t.Get<LiquidData>(),t.Get<TileWallBrightnessInvisibilityData>());}
            internal void Restore(int x,int y){Tile t=Main.tile[x,y];t.Get<TileTypeData>()=Type;t.Get<WallTypeData>()=Wall;t.Get<TileWallWireStateData>()=Bits;t.Get<LiquidData>()=Liquid;t.Get<TileWallBrightnessInvisibilityData>()=Coating;}
        }
    }
}
