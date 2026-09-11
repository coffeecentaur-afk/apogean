using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using apogean.Content.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Walls
{
	/// <summary>Deep structural membrane used inside the Gullet and Stomach.</summary>
	public sealed class MawWallUnsafe : ModWall
	{
		// Keep the structural wall's saved identity and unsafe behavior, but use the
		// already validated native Maw soil atlas. The legacy same-name PNG has
		// tile-sized gutters inside wall frames and draws a transparent grid.
		// This is the readable terrain baseline, not final fibrous membrane art.
		public override string Texture => MawPackedPreview.Texture("soil",true,"apogean/Content/Walls/MawDirtWallUnsafe");
		public override bool PreDraw(int i,int j,SpriteBatch spriteBatch) => MawPackedPreview.WallDraw("soil",Type,i,j,spriteBatch);

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			DustType = DustID.Dirt;
			AddMapEntry(new Color(68, 55, 31));
		}

		public override void Convert(int i, int j, int conversionType)
		{
			if (conversionType is BiomeConversionID.Purity or BiomeConversionID.PurificationPowder)
				WorldGen.ConvertWall(i, j, ModContent.WallType<WastesDirtWallUnsafe>());
		}
	}
}
