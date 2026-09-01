using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace apogean.Content.Tiles
{
	/// <summary>A small static snag used in place of living forest, willow, and Sakura canopies.</summary>
	public sealed class DeadTree : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileLighted[Type] = false;
			HitSound = SoundID.Dig;
			DustType = DustID.WoodFurniture;
			AddMapEntry(new Color(76, 61, 46));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.Origin = new Point16(1, 3);
			TileObjectData.newTile.AnchorValidTiles = new[] { ModContent.TileType<DeadGrass>(), TileID.Dirt };
			TileObjectData.addTile(Type);
		}
	}
}
