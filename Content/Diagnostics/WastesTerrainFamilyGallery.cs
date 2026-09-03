using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
	/// <summary>Paired vanilla/control and Wastes candidate renderer fixture for five natural materials.</summary>
	internal static class WastesTerrainFamilyGallery
	{
		private const int SwatchWidth = 8;
		private const int SwatchHeight = 7;
		private const int CellWidth = 10;

		public static Rectangle Build(Player player, out IReadOnlyList<string> labels)
		{
			Swatch[] swatches = CreateSwatches();
			int width = swatches.Length * CellWidth;
			Point playerTile = player.Center.ToTileCoordinates();
			int startX = Math.Clamp(playerTile.X - width / 2, 20, Main.maxTilesX - width - 20);
			int startY = Math.Clamp(playerTile.Y - 18, 20, Main.maxTilesY - SwatchHeight - 20);
			Rectangle bounds = new(startX, startY, width, SwatchHeight);

			Clear(bounds);
			List<string> names = new(swatches.Length);
			for (int index = 0; index < swatches.Length; index++)
			{
				PlaceSwatch(startX + index * CellWidth, startY, swatches[index]);
				names.Add(swatches[index].Name);
			}
			Frame(bounds);
			labels = names;
			return bounds;
		}

		private static void Clear(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				tile.ClearTile();
				tile.WallType = WallID.None;
				tile.LiquidAmount = 0;
			}
		}

		private static void PlaceSwatch(int left, int top, Swatch swatch)
		{
			for (int x = left + 1; x < left + SwatchWidth - 1; x++)
			for (int y = top + 1; y < top + SwatchHeight - 1; y++)
				Framing.GetTileSafely(x, y).WallType = (ushort)swatch.Wall;

			for (int offsetX = 0; offsetX < SwatchWidth; offsetX++)
			{
				SetTile(left + offsetX, top + SwatchHeight - 1, swatch.Tile);
				if (offsetX is >= 2 and <= 5)
					SetTile(left + offsetX, top + SwatchHeight - 3, swatch.Tile);
			}
			for (int y = top + 2; y < top + SwatchHeight; y++)
			{
				SetTile(left, y, swatch.Tile);
				SetTile(left + SwatchWidth - 1, y, swatch.Tile);
			}

			Framing.GetTileSafely(left + 2, top + SwatchHeight - 3).IsHalfBlock = true;
			Framing.GetTileSafely(left + 5, top + SwatchHeight - 3).Slope = SlopeType.SlopeDownLeft;
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
			for (int y = bounds.Top; y < bounds.Bottom; y++)
			{
				WorldGen.SquareTileFrame(x, y, true);
				WorldGen.SquareWallFrame(x, y, true);
			}
		}

		private static Swatch[] CreateSwatches() =>
		[
			new("Vanilla Stone", TileID.Stone, WallID.Stone),
			new("Wastes Stone", ModContent.TileType<WastesStoneCandidate>(), ModContent.WallType<WastesStoneWallCandidate>()),
			new("Vanilla Sand", TileID.Sand, WallID.Sandstone),
			new("Wastes Sand", ModContent.TileType<WastesSandCandidate>(), ModContent.WallType<WastesSandWallCandidate>()),
			new("Vanilla Ice", TileID.IceBlock, WallID.IceUnsafe),
			new("Wastes Ice", ModContent.TileType<WastesIceCandidate>(), ModContent.WallType<WastesIceWallCandidate>()),
			new("Vanilla Snow", TileID.SnowBlock, WallID.SnowWallUnsafe),
			new("Wastes Snow", ModContent.TileType<WastesSnowCandidate>(), ModContent.WallType<WastesSnowWallCandidate>()),
			new("Vanilla Mud", TileID.Mud, WallID.MudUnsafe),
			new("Wastes Mud", ModContent.TileType<WastesMudCandidate>(), ModContent.WallType<WastesMudWallCandidate>())
		];

		private readonly record struct Swatch(string Name, int Tile, int Wall);
	}
}
