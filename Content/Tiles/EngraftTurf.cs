using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>Charcoal soil webbed with amber fibres. It is terrain first, not a Corruption recolor.</summary>
	public sealed class EngraftTurf : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			DustType = DustID.AmberBolt;
			MineResist = 1.1f;
			AddMapEntry(new Color(72, 55, 31));
		}
	}
}
