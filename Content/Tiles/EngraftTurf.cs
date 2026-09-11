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
			// Legacy worldgen turf must use the same grass/soil contract as MawGrass.
			TileID.Sets.Grass[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<MawDirt>();
			Main.tileMerge[Type][ModContent.TileType<MawDirt>()] = true;
			Main.tileMerge[ModContent.TileType<MawDirt>()][Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			TileID.Sets.Conversion.Grass[Type] = true;
		}
	}
}
