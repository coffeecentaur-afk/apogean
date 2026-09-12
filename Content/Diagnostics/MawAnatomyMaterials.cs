using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    public sealed class MawAnatomyMaterials : ModSystem
    {
        internal static bool Available;
        public override void Load()
        {
            Available=MawPackedPreview.Enabled && Mod.FileExists("Content/Diagnostics/Anatomy/rib.bin");
            if(!Available)return;
            Mod.AddContent(new MawAnatomyTile("rib"));Mod.AddContent(new MawAnatomyTile("cap"));
            // The isolated build validates and copies the complete candidate set.
            // Do not infer content registration from the source PNG extension:
            // the packer can transcode textures before the mod is loaded.
            Mod.AddContent(new MawHangingFiber());
        }
        internal static MawAnatomyTile Tile(string key)=>(MawAnatomyTile)ModContent.Find<ModTile>("apogean/Anatomy_"+key);
        public override void PostSetupContent()
        {
            if(!Available)return;
            int rib=Tile("rib").Type,cap=Tile("cap").Type;
            Main.tileMergeDirt[rib]=true;
            foreach(string key in new[]{"soil","grass","stone","bone"}) {
                int host=MawPackedPreview.TileType(key);
                Main.tileMerge[rib][host]=Main.tileMerge[host][rib]=true;
            }
            int grass=MawPackedPreview.TileType("grass"),soil=MawPackedPreview.TileType("soil");
            Main.tileMerge[cap][grass]=Main.tileMerge[grass][cap]=true;
            Main.tileMerge[cap][soil]=Main.tileMerge[soil][cap]=true;
        }
        public override void Unload()=>Available=false;
    }

    [Autoload(false)]
    public sealed class MawAnatomyTile : ModTile
    {
        private readonly string key;
        private PackedMaterialMap map;
        public MawAnatomyTile(string key)=>this.key=key;
        public override string Name=>"Anatomy_"+key;
        public override string Texture=>"apogean/Content/Diagnostics/Anatomy/"+key;
        public override void Load()=>map=new PackedMaterialMap(Mod.GetFileBytes("Content/Diagnostics/Anatomy/"+key+".bin"));
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type]=Main.tileBlockLight[Type]=true;
            TileID.Sets.ChecksForMerge[Type]=true;
            DustType=key=="rib"?DustID.Bone:DustID.Dirt;HitSound=SoundID.Dig;
            MineResist=key=="rib"?2.7f:1.15f;MinPick=key=="rib"?59:0;
            AddMapEntry(key=="rib"?new Color(150,135,113):new Color(142,99,28));
            if(key=="cap") {
                TileID.Sets.Grass[Type]=TileID.Sets.NeedsGrassFraming[Type]=true;
                TileID.Sets.NeedsGrassFramingDirt[Type]=MawPackedPreview.TileType("soil");
            }
        }
        public override void SetSpriteEffects(int i,int j,ref SpriteEffects effects)=>effects=SpriteEffects.None;
        public override void SetDrawPositions(int i,int j,ref int width,ref int offsetY,ref int height,ref short tileFrameX,ref short tileFrameY)
        {
            if(map.TryMap(i,j,tileFrameX,tileFrameY,out short x,out short y)){tileFrameX=x;tileFrameY=y;}
        }
    }
}
