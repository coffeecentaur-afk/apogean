using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
    // Native-scale visual candidates only. No recipes, generation, spread or
    // replacement of production materials. Sand/amber gameplay is NOT cloned here.
    public sealed class MawTerrainStudies : ModSystem
    {
        internal static readonly string[] Materials = {
            "soil", "stone", "grass", "sand", "mud", "clay", "snow", "ice", "bone", "fibers", "membrane", "amber"
        };
        public override void Load()
        {
            foreach (string key in Materials) {
                Mod.AddContent(new MawTerrainStudyTile(key));
                Mod.AddContent(new MawTerrainStudyWall(key));
            }
        }
        public override void PostSetupContent()
        {
            if (!MawPackedPreview.Enabled) return;
            // These two exact relations were isolated by native corner probes.
            // No blanket merge-all and no new rule for already-correct pairs.
            int bone = MawPackedPreview.TileType("bone");
            foreach (string key in new[] { "fibers", "membrane" }) {
                int tissue = Tile(key).Type;
                Main.tileMerge[bone][tissue] = true;
                Main.tileMerge[tissue][bone] = true;
            }
            // Native rib-root probes isolate two additional requirements:
            // substrate joins use the dirt path; rooted grass needs its pair.
            Main.tileMergeDirt[bone] = true;
            int grass = MawPackedPreview.TileType("grass");
            Main.tileMerge[bone][grass] = Main.tileMerge[grass][bone] = true;
        }
        internal static MawTerrainStudyTile Tile(string key) => (MawTerrainStudyTile)ModContent.Find<ModTile>("apogean/Study_" + key);
        internal static MawTerrainStudyWall Wall(string key) => (MawTerrainStudyWall)ModContent.Find<ModWall>("apogean/StudyWall_" + key);
    }

    [Autoload(false)]
    public sealed class MawTerrainStudyTile : ModTile
    {
        private readonly string key;
        internal PackedMaterialMap Map { get; private set; }
        public MawTerrainStudyTile(string key) => this.key = key;
        public override string Name => "Study_" + key;
        public override string Texture => "apogean/Content/Diagnostics/Materials/" + key + "/Tile";
        public override void Load() => Map = new PackedMaterialMap(Mod.GetFileBytes("Content/Diagnostics/Materials/" + key + "/Tile.bin"));
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;
            Main.tileLighted[Type] = false;
            TileID.Sets.ChecksForMerge[Type] = true;
            DustType = DustID.Stone; HitSound = SoundID.Dig; MineResist = 1;
            AddMapEntry(new Color(80, 72, 62));
            if (key == "grass") {
                int soil = MawTerrainStudies.Tile("soil").Type;
                TileID.Sets.Grass[Type] = true;
                TileID.Sets.NeedsGrassFraming[Type] = true;
                TileID.Sets.NeedsGrassFramingDirt[Type] = soil;
                Main.tileMerge[Type][soil] = true; Main.tileMerge[soil][Type] = true;
            }
        }
        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) => spriteEffects = SpriteEffects.None;
        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY,
            ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            if (Map.TryMap(i, j, tileFrameX, tileFrameY, out short x, out short y)) {
                tileFrameX = x; tileFrameY = y;
            }
        }
    }

    [Autoload(false)]
    public sealed class MawTerrainStudyWall : ModWall
    {
        private readonly string key;
        internal PackedMaterialMap Map { get; private set; }
        public MawTerrainStudyWall(string key) => this.key = key;
        public override string Name => "StudyWall_" + key;
        public override string Texture => "apogean/Content/Diagnostics/Materials/" + key + "/Wall";
        public override void Load() => Map = new PackedMaterialMap(Mod.GetFileBytes("Content/Diagnostics/Materials/" + key + "/Wall.bin"));
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = false;
            DustType = DustID.Stone; AddMapEntry(new Color(42, 38, 34));
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var tile = Main.tile[i, j];
            if (!Map.TryMap(i, j, tile.WallFrameX, tile.WallFrameY, out short x, out short y)) return true;
            var texture = TextureAssets.Wall[Type].Value;
            if (tile.WallColor != PaintID.None)
                texture = Main.instance.TilePaintSystem.TryGetWallAndRequestIfNotReady(Type, tile.WallColor) ?? texture;
            var color = tile.IsWallFullbright ? Color.White : Lighting.GetColor(i, j);
            var offset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            spriteBatch.Draw(texture, new Vector2(i * 16 - 8, j * 16 - 8) - Main.screenPosition + offset,
                new Rectangle(x, y, 32, 32), color);
            return false;
        }
    }
}
