using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
	/// <summary>
	/// Small destructive fixture for client-render validation. Use only in a disposable world.
	/// </summary>
	internal static class TileLabGallery
	{
		private const int Width = 142;
		private const int Height = 24;

		internal static Rectangle Build(Player player)
		{
			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - Width / 2, 20, Main.maxTilesX - Width - 20);
			int top = Math.Clamp(playerTile.Y - 16, 20, Main.maxTilesY - Height - 20);
			Rectangle bounds = new(left, top, Width, Height);
			int floorY = bounds.Bottom - 2;
			int controlTileType = ModContent.TileType<TileLabBlock>();
			int controlWallType = ModContent.WallType<TileLabWall>();
			int candidateTileType = ModContent.TileType<WastesSoilCandidate>();
			int candidateWallType = ModContent.WallType<WastesDirtWallCandidate>();

			Clear(bounds);
			PlaceFloor(bounds, floorY);

			// Two identical suites make atlas/framing defects visible by direct comparison.
			PlaceMaterialSuite(left + 2, floorY, controlTileType, controlWallType);
			PlaceMaterialSuite(left + 66, floorY, candidateTileType, candidateWallType);

			// Candidate-in-water basin also exercises the renderer path that previously crashed.
			for (int x = left + 131; x <= left + 140; x++)
			{
				SetTile(x, floorY - 7, TileID.GrayBrick);
				SetTile(x, floorY - 2, TileID.GrayBrick);
			}
			for (int y = floorY - 7; y <= floorY - 2; y++)
			{
				SetTile(left + 131, y, TileID.GrayBrick);
				SetTile(left + 140, y, TileID.GrayBrick);
			}
			for (int x = left + 132; x <= left + 139; x++)
			{
				for (int y = floorY - 6; y <= floorY - 3; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					tile.LiquidType = LiquidID.Water;
					tile.LiquidAmount = byte.MaxValue;
				}
			}
			SetTile(left + 135, floorY - 4, candidateTileType);
			SetTile(left + 136, floorY - 4, candidateTileType);

			Frame(bounds);
			Lighting.Clear();
			player.Teleport(new Vector2((left + 70) * 16f, (floorY - 3) * 16f), TeleportationStyleID.RodOfDiscord);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, bounds.Center.X, bounds.Center.Y, Width + 4);
			return bounds;
		}

		private static void PlaceMaterialSuite(int left, int floorY, int tileType, int wallType)
		{
			// Isolated, horizontal, vertical, and cross connections.
			SetTile(left + 1, floorY - 3, tileType);
			for (int x = left + 4; x <= left + 11; x++)
				SetTile(x, floorY - 3, tileType);
			for (int y = floorY - 10; y <= floorY - 2; y++)
				SetTile(left + 14, y, tileType);
			for (int x = left + 17; x <= left + 23; x++)
				SetTile(x, floorY - 6, tileType);
			for (int y = floorY - 10; y <= floorY - 3; y++)
				SetTile(left + 20, y, tileType);

			// Dense tile field over its matching natural wall: the repetition/grid detector.
			for (int x = left + 26; x <= left + 40; x++)
			{
				for (int y = floorY - 11; y <= floorY - 2; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					tile.WallType = (ushort)wallType;
					if (x >= left + 28 && x <= left + 38 && y >= floorY - 9 && y <= floorY - 3)
						SetTile(x, y, tileType);
				}
			}

			// Half-block and all four slope directions.
			for (int x = left + 44; x <= left + 51; x++)
				SetTile(x, floorY - 3, tileType);
			Framing.GetTileSafely(left + 44, floorY - 3).IsHalfBlock = true;
			Framing.GetTileSafely(left + 46, floorY - 3).Slope = SlopeType.SlopeDownLeft;
			Framing.GetTileSafely(left + 47, floorY - 3).Slope = SlopeType.SlopeDownRight;
			Framing.GetTileSafely(left + 49, floorY - 3).Slope = SlopeType.SlopeUpLeft;
			Framing.GetTileSafely(left + 50, floorY - 3).Slope = SlopeType.SlopeUpRight;

			// Six-by-six boundary against vanilla dirt verifies bidirectional merge behavior.
			for (int x = left + 54; x <= left + 59; x++)
			{
				for (int y = floorY - 7; y <= floorY - 2; y++)
					SetTile(x, y, x <= left + 56 ? tileType : TileID.Dirt);
			}
		}

		private static void Clear(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				for (int y = bounds.Top; y < bounds.Bottom; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					tile.ClearEverything();
				}
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
