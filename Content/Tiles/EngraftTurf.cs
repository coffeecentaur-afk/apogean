using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>Ochre soil sealed by dark living fibres. Ordinary turf is deliberately non-luminous.</summary>
	public sealed class EngraftTurf : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			DustType = DustID.Dirt;
			MineResist = 1.1f;
			AddMapEntry(new Color(137, 91, 31));
		}
	}
}
