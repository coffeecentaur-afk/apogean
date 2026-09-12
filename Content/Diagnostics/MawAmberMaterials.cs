using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.Maw;

namespace apogean.Content.Diagnostics
{
    // Test-only material siblings share existing texture assets; production
    // amber, dormant progression and the original gallery remain unchanged.
    public sealed class MawAmberMaterials : ModSystem
    {
        public override void Load()
        {
            if(!MawPackedPreview.Enabled||!Mod.FileExists("Content/Diagnostics/Anatomy/amber-tile-light.bin"))return;
            Mod.AddContent(new MawAmberLitTile());Mod.AddContent(new MawAmberLitWall());
        }
        internal static bool Dormant(int i,int j)=>ModContent.GetInstance<MawAmberLightStudy>().StateAt(i,j)??MawActivityState.IsDormant;
        internal static Vector3 Light(int count,bool dormant,float scale=1f)=>new Vector3(.55f,.31f,.055f)*PackedEmissionMap.Strength(count,dormant)*scale;
    }
    [Autoload(false)]
    public sealed class MawAmberLitTile : ModTile
    {
        private PackedEmissionMap emission;
        public override string Texture=>"apogean/Content/Diagnostics/Materials/amber/Tile";
        public override void Load()=>emission=new PackedEmissionMap(Mod.GetFileBytes("Content/Diagnostics/Anatomy/amber-tile-light.bin"));
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type]=Main.tileBlockLight[Type]=Main.tileLighted[Type]=true;
            DustType=DustID.Dirt;HitSound=SoundID.Dig;MineResist=1.5f;MinPick=59;AddMapEntry(new Color(154,103,37));
        }
        public override void SetSpriteEffects(int i,int j,ref SpriteEffects effects)=>effects=SpriteEffects.None;
        public override void SetDrawPositions(int i,int j,ref int width,ref int offsetY,ref int height,ref short fx,ref short fy)
        {MawPackedPreview.TileDraw("amber",i,j,ref fx,ref fy);}
        internal Vector3 Emission(int i,int j,bool dormant)
        {
            Tile t=Main.tile[i,j];
            if(!t.HasUnactuatedTile||t.TileType!=Type||t.IsTileInvisible)return Vector3.Zero;
            var map=MawTerrainStudies.Tile("amber").Map;
            return map.TryMap(i,j,t.TileFrameX,t.TileFrameY,out short x,out short y)?MawAmberMaterials.Light(emission.Count(x,y),dormant):Vector3.Zero;
        }
        public override void ModifyLight(int i,int j,ref float r,ref float g,ref float b)
        {Vector3 light=Emission(i,j,MawAmberMaterials.Dormant(i,j));r=light.X;g=light.Y;b=light.Z;}
    }
    [Autoload(false)]
    public sealed class MawAmberLitWall : ModWall
    {
        private PackedEmissionMap emission;
        public override string Texture=>"apogean/Content/Diagnostics/Materials/amber/Wall";
        public override void Load()=>emission=new PackedEmissionMap(Mod.GetFileBytes("Content/Diagnostics/Anatomy/amber-wall-light.bin"));
        public override void SetStaticDefaults(){Main.wallHouse[Type]=false;DustType=DustID.Dirt;AddMapEntry(new Color(96,64,23));}
        public override bool PreDraw(int i,int j,SpriteBatch batch)
        {
            if(Main.tile[i,j].IsWallInvisible&&!Main.ShouldShowInvisibleWalls())return false;
            return MawPackedPreview.WallDraw("amber",Type,i,j,batch);
        }
        internal Vector3 Emission(int i,int j,bool dormant)
        {
            Tile t=Main.tile[i,j];
            if(t.WallType!=Type||t.IsWallInvisible||(t.HasUnactuatedTile&&Main.tileSolid[t.TileType]))return Vector3.Zero;
            var map=MawTerrainStudies.Wall("amber").Map;
            return map.TryMap(i,j,t.WallFrameX,t.WallFrameY,out short x,out short y)?MawAmberMaterials.Light(emission.Count(x,y),dormant,.75f):Vector3.Zero;
        }
        public override void ModifyLight(int i,int j,ref float r,ref float g,ref float b)
        {Vector3 light=Emission(i,j,MawAmberMaterials.Dormant(i,j));r=light.X;g=light.Y;b=light.Z;}
    }
}
