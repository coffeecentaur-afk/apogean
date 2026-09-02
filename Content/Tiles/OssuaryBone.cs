using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>Safe structural bone. Animated barbs will be separate hazard tiles.</summary>
	public sealed class OssuaryBone : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMerge[Type][ModContent.TileType<Mawstone>()] = true;
			Main.tileMerge[ModContent.TileType<Mawstone>()][Type] = true;
			DustType = DustID.Bone;
			MineResist = 2.7f;
			MinPick = 59;
			AddMapEntry(new Color(190, 178, 128));
		}

		public override bool CanExplode(int i, int j) => true;
	}
}
