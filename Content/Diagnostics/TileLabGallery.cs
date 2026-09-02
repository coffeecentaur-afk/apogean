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
		private const int Width = 68;
		private const int Height = 22;

		internal static Rectangle Build(Player player)
		{
			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - Width / 2, 20, Main.maxTilesX - Width - 20);
			int top = Math.Clamp(playerTile.Y - 16, 20, Main.maxTilesY - Height - 20);
			Rectangle bounds = new(left, top, Width, Height);
			int floorY = bounds.Bottom - 2;
			int blockType = ModContent.TileType<TileLabBlock>();
			int wallType = ModContent.WallType<TileLabWall>();

			Clear(bounds);
			PlaceFloor(bounds, floorY);

			// Panel 1: isolated, horizontal, vertical, and cross connections.
			SetTile(left + 3, floorY - 3, blockType);
			for (int x = left + 6; x <= left + 13; x++)
				SetTile(x, floorY - 3, blockType);
			for (int y = floorY - 9; y <= floorY - 2; y++)
				SetTile(left + 16, y, blockType);
			for (int x = left + 19; x <= left + 25; x++)
				SetTile(x, floorY - 6, blockType);
			for (int y = floorY - 9; y <= floorY - 3; y++)
				SetTile(left + 22, y, blockType);

			// Panel 2: a dense field over its matching wall. This is the grid-art detector.
			for (int x = left + 28; x <= left + 42; x++)
			{
				for (int y = floorY - 10; y <= floorY - 2; y++)
			{
					Tile tile = Framing.GetTileSafely(x, y);
					tile.WallType = (ushort)wallType;
					if (x >= left + 30 && x <= left + 40 && y >= floorY - 8 && y <= floorY - 3)
						SetTile(x, y, blockType);
				}
			}

			// Panel 3: half-blocks and all four slope directions.
			for (int x = left + 46; x <= left + 53; x++)
				SetTile(x, floorY - 3, blockType);
			Framing.GetTileSafely(left + 46, floorY - 3).IsHalfBlock = true;
			Framing.GetTileSafely(left + 48, floorY - 3).Slope = SlopeType.SlopeDownLeft;
			Framing.GetTileSafely(left + 49, floorY - 3).Slope = SlopeType.SlopeDownRight;
			Framing.GetTileSafely(left + 51, floorY - 3).Slope = SlopeType.SlopeUpLeft;
			Framing.GetTileSafely(left + 52, floorY - 3).Slope = SlopeType.SlopeUpRight;

			// Panel 4: vanilla dirt merge boundary and a vanilla-water capture-camera control basin.
			for (int x = left + 56; x <= left + 60; x++)
			{
				for (int y = floorY - 7; y <= floorY - 2; y++)
					SetTile(x, y, x <= left + 58 ? blockType : TileID.Dirt);
			}
			for (int x = left + 62; x <= left + 66; x++)
			{
				SetTile(x, floorY - 6, TileID.GrayBrick);
				SetTile(x, floorY - 2, TileID.GrayBrick);
			}
			for (int y = floorY - 6; y <= floorY - 2; y++)
			{
				SetTile(left + 62, y, TileID.GrayBrick);
				SetTile(left + 66, y, TileID.GrayBrick);
			}
			for (int x = left + 63; x <= left + 65; x++)
			{
				for (int y = floorY - 5; y <= floorY - 3; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					tile.LiquidType = LiquidID.Water;
					tile.LiquidAmount = byte.MaxValue;
				}
			}

			Frame(bounds);
			Lighting.Clear();
			player.Teleport(new Vector2((left + 24) * 16f, (floorY - 3) * 16f), TeleportationStyleID.RodOfDiscord);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, bounds.Center.X, bounds.Center.Y, Width + 4);
			return bounds;
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
