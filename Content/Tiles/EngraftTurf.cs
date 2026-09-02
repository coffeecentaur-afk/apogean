using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>Ochre soil sealed by dark living fibres. Ordinary turf is deliberately non-luminous.</summary>
	public sealed class EngraftTurf : MawNaturalTile
	{
		protected override Color MapColor => new(137, 91, 31);
		protected override int PurifiedTile => ModContent.TileType<WastesGrass>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			TileID.Sets.Conversion.Grass[Type] = true;
		}
	}
}
