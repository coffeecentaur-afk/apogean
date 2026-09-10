using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;
using apogean.Content.Items.Placeable;

namespace apogean.Content.Diagnostics
{
    /// <summary>Opt-in art/renderer proof. No recipes or world-generation placement.</summary>
    public sealed class MawStoneMaterialCandidate : MawNaturalTile
    {
        public override string Texture => "apogean/Content/Tiles/Diagnostics/MawStoneMaterialCandidate";
        protected override Color MapColor => new(62, 61, 66);
        protected override int VanillaEquivalent => TileID.Stone;
        protected override int PurifiedTile => ModContent.TileType<WastesStone>();
        protected override int ItemDrop => ModContent.ItemType<MawstoneBlock>();
        protected override int TileDust => DustID.Stone;
        protected override float Resistance => 2.4f;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            TileID.Sets.Stone[Type] = true;
            Main.tileLighted[Type] = false;
            MinPick = 59;
        }
        public override bool CanExplode(int i, int j) => true;
        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
            => spriteEffects = SpriteEffects.None;
        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY,
            ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            if (MawMaterialFramePolicy.TryMap(i, j, tileFrameX, tileFrameY, out short x, out short y))
            { tileFrameX = x; tileFrameY = y; }
        }
    }
}
