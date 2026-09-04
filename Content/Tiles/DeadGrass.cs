using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>Safe forest ground after the biosphere collapse. It does not spread.</summary>
	public sealed class DeadGrass : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.Grass[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.NeedsGrassFramingDirt[Type] = TileID.Dirt;
			TileID.Sets.Conversion.Grass[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			DustType = DustID.Dirt;
			HitSound = SoundID.Dig;
			MineResist = 0.6f;
			RegisterItemDrop(ItemID.DirtBlock);
			AddMapEntry(new Color(112, 82, 48));
		}
	}
}
