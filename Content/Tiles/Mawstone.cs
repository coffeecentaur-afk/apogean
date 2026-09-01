using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>Hardened structural tissue. Placeholder art reuses Engraft turf until geometry is approved.</summary>
	public sealed class Mawstone : ModTile
	{
		public override string Texture => "apogean/Content/Tiles/EngraftTurf";

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileMerge[Type][ModContent.TileType<EngraftTurf>()] = true;
			Main.tileMerge[ModContent.TileType<EngraftTurf>()][Type] = true;
			DustType = DustID.AmberBolt;
			MineResist = 2.4f;
			MinPick = 59;
			AddMapEntry(new Color(93, 74, 39));
		}

		public override bool CanExplode(int i, int j) => true;
	}
}
