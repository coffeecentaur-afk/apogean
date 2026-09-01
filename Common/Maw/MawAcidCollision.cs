using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Common.Maw
{
	internal static class MawAcidCollision
	{
		public static bool Intersects(Rectangle hitbox)
		{
			int left = Utils.Clamp(hitbox.Left / 16, 1, Main.maxTilesX - 2);
			int right = Utils.Clamp((hitbox.Right - 1) / 16, 1, Main.maxTilesX - 2);
			int top = Utils.Clamp(hitbox.Top / 16, 1, Main.maxTilesY - 2);
			int bottom = Utils.Clamp((hitbox.Bottom - 1) / 16, 1, Main.maxTilesY - 2);
			int acidType = ModContent.TileType<MawAcidPool>();

			for (int x = left; x <= right; x++)
			{
				for (int y = top; y <= bottom; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (!tile.HasTile || tile.TileType != acidType)
						continue;

					Rectangle tileHitbox = new(x * 16, y * 16, 16, 16);
					if (tileHitbox.Intersects(hitbox))
						return true;
				}
			}

			return false;
		}
	}
}
