using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    // Isolated six-section art/behavior candidate. No automatic world growth,
    // recipes, ropes, sway, drops, or replacement of any existing vine type.
    [Autoload(false)]
    public sealed class MawHangingFiber : ModTile
    {
        internal const int MaxLength=6;
        public override string Texture=>"apogean/Content/Diagnostics/Anatomy/hanging-fiber";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type]=true;Main.tileCut[Type]=true;Main.tileNoFail[Type]=true;
            Main.tileSolid[Type]=false;Main.tileSolidTop[Type]=false;Main.tileRope[Type]=false;
            Main.tileLavaDeath[Type]=true;Main.tileWaterDeath[Type]=false;
            TileID.Sets.IsVine[Type]=true;
            DustType=DustID.Dirt;HitSound=SoundID.Grass;AddMapEntry(new Color(108,77,33));
        }
        internal static bool Anchor(Tile t)=>t.HasUnactuatedTile&&!t.BottomSlope&&
            (t.TileType==MawPackedPreview.TileType("grass")||t.TileType==MawPackedPreview.TileType("soil"));
        internal static int Depth(int x,int y,int type)
        {
            if(!WorldGen.InWorld(x,y,30))return -1;
            for(int d=1;d<=MaxLength;d++) {
                Tile above=Main.tile[x,y-d];
                if(Anchor(above))return d;
                if(!above.HasUnactuatedTile||above.TileType!=type||above.Slope!=SlopeType.Solid||above.IsHalfBlock)return -1;
            }
            return -1;
        }
        internal static bool Grow(Point root,int type)
        {
            if(Main.netMode==NetmodeID.MultiplayerClient||!MawHangingFiberStudy.AllowedRoot(root)||!WorldGen.InWorld(root.X,root.Y+MaxLength+1,30))return false;
            Tile anchor=Main.tile[root];if(!Anchor(anchor)||anchor.LiquidAmount!=0)return false;
            for(int d=1;d<=MaxLength;d++) {
                Tile t=Main.tile[root.X,root.Y+d];
                if(t.HasTile) {
                    if(t.TileType!=type||!t.HasUnactuatedTile||t.Slope!=SlopeType.Solid||t.IsHalfBlock||t.LiquidAmount!=0)return false;
                    continue;
                }
                if(t.LiquidAmount!=0||t.HasActuator||t.IsActuated||t.RedWire||t.BlueWire||t.GreenWire||t.YellowWire)return false;
                // Never bridge an existing orphan by growing through a gap.
                for(int below=d+1;below<=MaxLength;below++)if(Main.tile[root.X,root.Y+below].HasTile)return false;
                t.HasTile=true;t.TileType=(ushort)type;t.Slope=SlopeType.Solid;t.IsHalfBlock=false;
                t.CopyPaintAndCoating(Main.tile[root.X,root.Y+d-1]);t.TileFrameX=0;t.TileFrameY=(short)((d-1)*18);
                WorldGen.SquareTileFrame(root.X,root.Y+d);
                if(Main.netMode==NetmodeID.Server)NetMessage.SendTileSquare(-1,root.X,root.Y+d);
                return true;
            }
            return false;
        }
        public override bool TileFrame(int i,int j,ref bool resetFrame,ref bool noBreak)
        {
            Tile t=Main.tile[i,j];int depth=Depth(i,j,Type);
            if(depth<1||!t.HasUnactuatedTile||t.Slope!=SlopeType.Solid||t.IsHalfBlock) {
                WorldGen.KillTile(i,j,noItem:true);return false;
            }
            t.TileFrameX=0;t.TileFrameY=(short)((depth-1)*18);return false;
        }
        public override bool CanDrop(int i,int j)=>false;
        public override void KillTile(int i,int j,ref bool fail,ref bool effectOnly,ref bool noItem)
        {
            var bounds=ModContent.GetInstance<MawHangingFiberStudy>().Bounds;
            if(!bounds.Contains(i,j)||i>=bounds.X+88)return; // Exclude the deliberate cut-test scratch pad.
            var trace=new System.Diagnostics.StackTrace();var names=new System.Collections.Generic.List<string>();
            foreach(var frame in trace.GetFrames()) {var method=frame.GetMethod();names.Add(method?.DeclaringType?.Name+"."+method?.Name);if(names.Count>=10)break;}
            Mod.Logger.Info($"MAW HANGING CUT AUDIT: {i},{j}; fail={fail}; effectOnly={effectOnly}; depth={Depth(i,j,Type)}; liquid={Main.tile[i,j].LiquidAmount}/{Main.tile[i,j].LiquidType}; callers={string.Join(" > ",names)}.");
        }
    }
}
