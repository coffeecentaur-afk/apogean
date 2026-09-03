using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
	/// <summary>
	/// Destructive grass-only renderer fixture. The left suite is vanilla grass/dirt;
	/// the right suite is the isolated Wastes grass/soil candidate.
	/// </summary>
	internal static class GrassLabGallery
	{
		private const int Width = 120;
		private const int Height = 25;

		internal static Rectangle Build(Player player)
		{
			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - Width / 2, 20, Main.maxTilesX - Width - 20);
			int top = Math.Clamp(playerTile.Y - 17, 20, Main.maxTilesY - Height - 20);
			Rectangle bounds = new(left, top, Width, Height);
			int floorY = bounds.Bottom - 2;

			Clear(bounds);
			PlaceFloor(bounds, floorY);
			PlaceGrassSuite(left + 2, floorY, TileID.Grass, TileID.Dirt, WallID.GrassUnsafe);
			PlaceGrassSuite(
				left + 62,
				floorY,
				ModContent.TileType<WastesGrassCandidate>(),
				ModContent.TileType<WastesSoilCandidate>(),
				ModContent.WallType<WastesGrassWallCandidate>());

			Frame(bounds);
			Lighting.Clear();
			player.Teleport(new Vector2((left + Width / 2) * 16f, (floorY - 4) * 16f), TeleportationStyleID.RodOfDiscord);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, bounds.Center.X, bounds.Center.Y, Width + 4);
			return bounds;
		}

		private static void PlaceGrassSuite(int left, int floorY, int grassType, int soilType, int wallType)
		{
			// Dense patch over its natural wall exposes repeated cells and every outer edge.
			for (int x = left + 1; x <= left + 17; x++)
			{
				for (int y = floorY - 12; y <= floorY - 2; y++)
				{
					Framing.GetTileSafely(x, y).WallType = (ushort)wallType;
					if (x >= left + 3 && x <= left + 15 && y >= floorY - 9 && y <= floorY - 3)
						SetTile(x, y, grassType);
				}
			}

			// Flat exposed grass over soil verifies the characteristic grass cap and side fringe.
			for (int x = left + 21; x <= left + 34; x++)
			{
				SetTile(x, floorY - 5, grassType);
				SetTile(x, floorY - 4, soilType);
				SetTile(x, floorY - 3, soilType);
				SetTile(x, floorY - 2, soilType);
			}

			// Stair-step terrain exercises exposed tops and vertical sides in one compact mound.
			for (int column = 0; column < 8; column++)
			{
				int x = left + 38 + column;
				int topY = floorY - 3 - Math.Min(column, 4);
				SetTile(x, topY, grassType);
				for (int y = topY + 1; y <= floorY - 2; y++)
					SetTile(x, y, soilType);
			}

			// Half block and all four slopes must retain clean grass silhouettes.
			for (int x = left + 49; x <= left + 56; x++)
				SetTile(x, floorY - 3, grassType);
			Framing.GetTileSafely(left + 49, floorY - 3).IsHalfBlock = true;
			Framing.GetTileSafely(left + 51, floorY - 3).Slope = SlopeType.SlopeDownLeft;
			Framing.GetTileSafely(left + 52, floorY - 3).Slope = SlopeType.SlopeDownRight;
			Framing.GetTileSafely(left + 54, floorY - 3).Slope = SlopeType.SlopeUpLeft;
			Framing.GetTileSafely(left + 55, floorY - 3).Slope = SlopeType.SlopeUpRight;
		}

		private static void Clear(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				for (int y = bounds.Top; y < bounds.Bottom; y++)
					Framing.GetTileSafely(x, y).ClearEverything();
			}
		}

		private static void PlaceFloor(Rectangle bounds, int floorY)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				SetTile(x, floorY, TileID.GrayBrick);
				SetTile(x, floorY + 1, TileID.GrayBrick);
			}
		}

		private static void SetTile(int x, int y, int type)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			tile.HasTile = true;
			tile.TileType = (ushort)type;
			tile.TileFrameX = 0;
			tile.TileFrameY = 0;
			tile.Slope = SlopeType.Solid;
			tile.IsHalfBlock = false;
		}

		private static void Frame(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				for (int y = bounds.Top; y < bounds.Bottom; y++)
				{
					WorldGen.SquareTileFrame(x, y, true);
					WorldGen.SquareWallFrame(x, y, true);
				}
			}
		}
	}
}
