using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace apogean.Content.Tiles
{
	/// <summary>A non-glowing surface tendril that replaces ordinary green plants inside a rupture.</summary>
	public sealed class EngraftTuft : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileWaterDeath[Type] = false;
			HitSound = SoundID.Grass;
			DustType = DustID.Dirt;
			AddMapEntry(new Color(156, 101, 35));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Origin = Point16.Zero;
			TileObjectData.newTile.AnchorValidTiles = new[] { ModContent.TileType<EngraftTurf>() };
			TileObjectData.addTile(Type);
		}
	}
}
