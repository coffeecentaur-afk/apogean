using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>Hardened structural tissue framing the Gullet and Stomach.</summary>
	public sealed class Mawstone : MawNaturalTile
	{
		protected override Color MapColor => new(91, 73, 44);
		protected override int PurifiedTile => ModContent.TileType<WastesStone>();
		protected override int ItemDrop => ItemID.StoneBlock;
		protected override float Resistance => 2.4f;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.tileMerge[Type][ModContent.TileType<EngraftTurf>()] = true;
			Main.tileMerge[ModContent.TileType<EngraftTurf>()][Type] = true;
			MinPick = 59;
		}

		public override bool CanExplode(int i, int j) => true;
	}
}
