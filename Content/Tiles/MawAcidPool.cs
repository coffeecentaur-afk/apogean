using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.Maw;

namespace apogean.Content.Tiles
{
	/// <summary>
	/// A local animated hazard tile, deliberately not a fifth liquid. Placeholder drawing is
	/// code-native so geometry can be tested before its final connected sprite sheet exists.
	/// </summary>
	public sealed class MawAcidPool : ModTile
	{
		public override string Texture => "apogean/Content/Tiles/EngraftTurf";

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = false;
			Main.tileBlockLight[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			DustType = DustID.TintableDustLighted;
			AddMapEntry(new Color(210, 174, 35));
		}

		public override bool IsTileDangerous(int i, int j, Player player) => true;

		public override bool CanExplode(int i, int j) => false;

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			float activity = MawActivityState.IsDormant ? 0.35f : 1f;
			r = 0.58f * activity;
			g = 0.42f * activity;
			b = 0.055f * activity;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Vector2 offscreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			Vector2 screen = new Vector2(i * 16, j * 16) - Main.screenPosition + offscreen;
			bool dormant = MawActivityState.IsDormant;
			Color body = dormant ? new Color(116, 91, 35) : new Color(196, 154, 25);
			Color shadow = dormant ? new Color(68, 58, 33) : new Color(101, 76, 13);
			Color highlight = dormant ? new Color(153, 126, 51) : new Color(245, 216, 62);
			Color lighting = Lighting.GetColor(i, j);
			body = Color.Lerp(lighting, body, 0.78f);
			shadow = Color.Lerp(lighting, shadow, 0.72f);
			highlight = Color.Lerp(lighting, highlight, 0.86f);

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			spriteBatch.Draw(pixel, new Rectangle((int)screen.X, (int)screen.Y, 16, 16), shadow);
			spriteBatch.Draw(pixel, new Rectangle((int)screen.X + 1, (int)screen.Y + 3, 14, 12), body);

			int acidType = ModContent.TileType<MawAcidPool>();
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile || above.TileType != acidType)
			{
				int wave = dormant ? 0 : (int)((Main.GlobalTimeWrappedHourly * 2f + i * 0.37f) % 2f);
				spriteBatch.Draw(pixel, new Rectangle((int)screen.X + 1, (int)screen.Y + 2 + wave, 14, 2), highlight);
			}

			return false;
		}
	}
}
