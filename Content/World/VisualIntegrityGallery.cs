using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;
using apogean.Content.Walls;

namespace apogean.Content.World
{
	/// <summary>Deterministic in-world proof fixture for native framing, walls, slopes, and faction identity.</summary>
	internal static class VisualIntegrityGallery
	{
		private const int SwatchWidth = 8;
		private const int SwatchHeight = 7;
		private const int CellWidth = 10;
		private const int CellHeight = 9;
		private const int Columns = 8;

		public static Rectangle Build(Player player, out IReadOnlyList<string> rows)
		{
			GallerySwatch[] swatches = CreateSwatches();
			int rowCount = (swatches.Length + Columns - 1) / Columns;
			int width = Columns * CellWidth;
			int height = rowCount * CellHeight;
			Point playerTile = player.Center.ToTileCoordinates();
			int startX = Math.Clamp(playerTile.X + 14, 20, Main.maxTilesX - width - 20);
			int startY = Math.Clamp(playerTile.Y - 18, 20, Main.maxTilesY - height - 20);
			Rectangle bounds = new(startX, startY, width, height);

			Clear(bounds);
			List<string> rowDescriptions = new();
			for (int index = 0; index < swatches.Length; index++)
			{
				int column = index % Columns;
				int row = index / Columns;
				PlaceSwatch(startX + column * CellWidth, startY + row * CellHeight, swatches[index]);
				if (column == 0)
					rowDescriptions.Add(swatches[index].Name);
				else
					rowDescriptions[row] += $" | {swatches[index].Name}";
			}

			Frame(bounds);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, bounds.Center.X, bounds.Center.Y, Math.Max(bounds.Width, bounds.Height) + 4);
			rows = rowDescriptions;
			return bounds;
		}

		private static void Clear(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				for (int y = bounds.Top; y < bounds.Bottom; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					tile.ClearTile();
					tile.WallType = WallID.None;
					tile.LiquidAmount = 0;
				}
			}
		}

		private static void PlaceSwatch(int left, int top, GallerySwatch swatch)
		{
			for (int x = left + 1; x < left + SwatchWidth - 1; x++)
			{
				for (int y = top + 1; y < top + SwatchHeight - 1; y++)
					Framing.GetTileSafely(x, y).WallType = (ushort)swatch.Wall;
			}

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

			Tile half = Framing.GetTileSafely(left + 2, top + SwatchHeight - 3);
			half.IsHalfBlock = true;
			Tile slope = Framing.GetTileSafely(left + 5, top + SwatchHeight - 3);
			slope.Slope = SlopeType.SlopeDownLeft;
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

		private static GallerySwatch[] CreateSwatches() =>
		[
			new("Waste Soil", ModContent.TileType<WastesSoil>(), ModContent.WallType<WastesDirtWallUnsafe>()),
			new("Waste Stone", ModContent.TileType<WastesStone>(), ModContent.WallType<WastesStoneWallUnsafe>()),
			new("Waste Grass", ModContent.TileType<WastesGrass>(), ModContent.WallType<WastesGrassWallUnsafe>()),
			new("Waste Sand", ModContent.TileType<WastesSand>(), ModContent.WallType<WastesSandWallUnsafe>()),
			new("Waste Ice", ModContent.TileType<WastesIce>(), ModContent.WallType<WastesIceWallUnsafe>()),
			new("Waste Snow", ModContent.TileType<WastesSnow>(), ModContent.WallType<WastesSnowWallUnsafe>()),
			new("Waste Mud", ModContent.TileType<WastesMud>(), ModContent.WallType<WastesMudWallUnsafe>()),
			new("Maw Dirt", ModContent.TileType<MawDirt>(), ModContent.WallType<MawDirtWallUnsafe>()),
			new("Maw Stone", ModContent.TileType<Mawstone>(), ModContent.WallType<MawStoneWallUnsafe>()),
			new("Maw Grass", ModContent.TileType<MawGrass>(), ModContent.WallType<MawGrassWallUnsafe>()),
			new("Maw Sand", ModContent.TileType<MawSand>(), ModContent.WallType<MawSandWallUnsafe>()),
			new("Maw Ice", ModContent.TileType<MawIce>(), ModContent.WallType<MawIceWallUnsafe>()),
			new("Maw Snow", ModContent.TileType<MawSnow>(), ModContent.WallType<MawSnowWallUnsafe>()),
			new("Maw Mud", ModContent.TileType<MawMud>(), ModContent.WallType<MawMudWallUnsafe>()),
			new("Maw Clay", ModContent.TileType<MawClay>(), ModContent.WallType<MawDirtWallUnsafe>()),
			new("Ossuary", ModContent.TileType<OssuaryBone>(), ModContent.WallType<MawStoneWallUnsafe>()),
			new("K Block", ModContent.TileType<KesslerBlock>(), ModContent.WallType<KesslerBulkheadWall>()),
			new("K Trim", ModContent.TileType<KesslerTrim>(), ModContent.WallType<KesslerBulkheadWall>()),
			new("K Floor", ModContent.TileType<KesslerFloor>(), ModContent.WallType<KesslerBulkheadWall>()),
			new("K Glass", ModContent.TileType<KesslerGlass>(), ModContent.WallType<KesslerWindowWall>()),
			new("K Beam", ModContent.TileType<KesslerBeam>(), ModContent.WallType<KesslerBulkheadWall>()),
			new("H Block", ModContent.TileType<HelixBlock>(), ModContent.WallType<HelixLaboratoryWall>()),
			new("H Trim", ModContent.TileType<HelixTrim>(), ModContent.WallType<HelixLaboratoryWall>()),
			new("H Floor", ModContent.TileType<HelixFloor>(), ModContent.WallType<HelixLaboratoryWall>()),
			new("H Glass", ModContent.TileType<HelixGlass>(), ModContent.WallType<HelixObservationWall>()),
			new("H Beam", ModContent.TileType<HelixBeam>(), ModContent.WallType<HelixLaboratoryWall>()),
			new("S Block", ModContent.TileType<SentrixBlock>(), ModContent.WallType<SentrixDataWall>()),
			new("S Trim", ModContent.TileType<SentrixTrim>(), ModContent.WallType<SentrixDataWall>()),
			new("S Floor", ModContent.TileType<SentrixFloor>(), ModContent.WallType<SentrixDataWall>()),
			new("S Glass", ModContent.TileType<SentrixGlass>(), ModContent.WallType<SentrixWindowWall>()),
			new("S Beam", ModContent.TileType<SentrixBeam>(), ModContent.WallType<SentrixDataWall>())
		];

		private readonly record struct GallerySwatch(string Name, int Tile, int Wall);
	}
}
