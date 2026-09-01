using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.WorldGeneration;
using apogean.Content.Tiles;

namespace apogean.Common.Maw
{
	internal static class MawAcidDebugBuilder
	{
		public static bool TryPlace(Player player, out string failureReason)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				failureReason = "Acid debug edits must be run by the server or in single player.";
				return false;
			}

			int centerX = player.Center.ToTileCoordinates().X;
			int groundY = FindGround(centerX, player.Bottom.ToTileCoordinates().Y);
			Rectangle bounds = new(centerX - 12, groundY - 4, 25, 6);
			if (!ApogeanWorldPlanSystem.Instance.CanPlace(bounds, WorldEditIntent.MawOutgrowth))
			{
				failureReason = "The test pool would overlap spawn or another protected region. Move into an unprotected Maw test area.";
				return false;
			}

			int stoneType = ModContent.TileType<Mawstone>();
			int acidType = ModContent.TileType<MawAcidPool>();
			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				bool edge = x == bounds.Left || x == bounds.Right - 1;
				for (int y = groundY - 3; y <= groundY; y++)
				{
					if (!WorldGen.InWorld(x, y, 5))
						continue;
					Tile tile = Framing.GetTileSafely(x, y);
					if (edge || y == groundY)
					{
						tile.HasTile = true;
						tile.TileType = (ushort)stoneType;
						tile.Slope = SlopeType.Solid;
						tile.IsHalfBlock = false;
						tile.LiquidAmount = 0;
						continue;
					}

					tile.ClearTile();
					tile.LiquidAmount = 0;
					if (y >= groundY - 2)
					{
						tile.HasTile = true;
						tile.TileType = (ushort)acidType;
					}
				}
			}

			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, centerX, groundY - 2, 30);

			failureReason = string.Empty;
			return true;
		}

		private static int FindGround(int x, int startY)
		{
			for (int y = Utils.Clamp(startY, 10, Main.maxTilesY - 12); y < Main.maxTilesY - 10 && y < startY + 40; y++)
			{
				if (WorldGen.SolidTile(x, y))
					return y;
			}

			return Utils.Clamp(startY + 8, 10, Main.maxTilesY - 12);
		}
	}
}
