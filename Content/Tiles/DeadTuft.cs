using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace apogean.Content.Tiles
{
	public sealed class DeadTuft : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			HitSound = SoundID.Grass;
			DustType = DustID.Dirt;
			AddMapEntry(new Color(121, 91, 54));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Origin = Point16.Zero;
			TileObjectData.newTile.AnchorValidTiles = new[] { ModContent.TileType<DeadGrass>(), TileID.Dirt };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.addTile(Type);
		}
	}
}
