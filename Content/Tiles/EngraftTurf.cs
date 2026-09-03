using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Placeable;

namespace apogean.Content.Tiles
{
	/// <summary>Ochre soil sealed by dark living fibres. Ordinary turf is deliberately non-luminous.</summary>
	public sealed class EngraftTurf : MawNaturalTile
	{
		protected override Color MapColor => new(137, 91, 31);
		protected override int VanillaEquivalent => TileID.Grass;
		protected override int PurifiedTile => ModContent.TileType<WastesGrass>();
		protected override int ItemDrop => ModContent.ItemType<MawDirtBlock>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			TileID.Sets.Conversion.Grass[Type] = true;
		}
	}
}
