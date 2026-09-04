using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using apogean.Content.Tiles;
using apogean.Content.Walls;

namespace apogean.Content.Diagnostics
{
	/// <summary>
	/// Disposable, gameplay-scale Helix material cutaway. This fixture exists so
	/// connected atlas topology, low-contrast walls, glass, furniture, and the
	/// animated specimen tank are reviewed before the full campus is regenerated.
	/// </summary>
	internal static class HelixConstructionGallery
	{
		private const int Width = 92;
		private const int Height = 29;

		internal static Rectangle Build(Player player)
		{
			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - Width / 2, 24, Main.maxTilesX - Width - 24);
			int top = Math.Clamp(playerTile.Y - 15, 80, Main.maxTilesY - Height - 40);
			Rectangle bounds = new(left, top, Width, Height);
			int floorY = bounds.Bottom - 4;

			Clear(new Rectangle(bounds.Left - 4, bounds.Top - 2, bounds.Width + 8, bounds.Height + 8));
			PlaceWastesGround(bounds.Left - 4, bounds.Right + 4, floorY + 3);
			PlaceConstruction(bounds, floorY);
			Frame(new Rectangle(bounds.Left - 4, bounds.Top - 2, bounds.Width + 8, bounds.Height + 8));
			PlaceFurniture(bounds, floorY);
			Lighting.Clear();

			Main.dayTime = true;
			Main.time = 27000d;
			player.Teleport(new Vector2((bounds.Center.X + 0.5f) * 16f, (floorY - 3) * 16f), TeleportationStyleID.RodOfDiscord);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, bounds.Center.X, bounds.Center.Y, bounds.Width + 12);
			return new Rectangle(bounds.Left, bounds.Top - 2, bounds.Width, bounds.Height + 2);
		}

		private static void PlaceConstruction(Rectangle bounds, int floorY)
		{
			int block = ModContent.TileType<HelixBlock>();
			int trim = ModContent.TileType<HelixTrim>();
			int floor = ModContent.TileType<HelixFloor>();
			int glass = ModContent.TileType<HelixGlass>();
			int beam = ModContent.TileType<HelixBeam>();
			int platform = ModContent.TileType<HelixPlatform>();
			int laboratoryWall = ModContent.WallType<HelixLaboratoryWall>();
			int observationWall = ModContent.WallType<HelixObservationWall>();

			FillWall(new Rectangle(bounds.Left + 5, bounds.Top + 7, 55, floorY - bounds.Top - 8), laboratoryWall);
			FillWall(new Rectangle(bounds.Left + 60, bounds.Top + 7, 27, floorY - bounds.Top - 8), observationWall);

			FillTiles(new Rectangle(bounds.Left + 3, bounds.Top + 5, bounds.Width - 6, 2), block);
			FillTiles(new Rectangle(bounds.Left + 1, bounds.Top + 3, bounds.Width - 2, 2), trim);
			FillTiles(new Rectangle(bounds.Left + 3, floorY, bounds.Width - 6, 3), block);
			FillTiles(new Rectangle(bounds.Left + 4, floorY - 2, bounds.Width - 8, 2), floor);
			FillTiles(new Rectangle(bounds.Left + 3, bounds.Top + 5, 2, floorY - bounds.Top - 5), block);
			FillTiles(new Rectangle(bounds.Right - 5, bounds.Top + 5, 2, floorY - bounds.Top - 5), block);

			FillTiles(new Rectangle(bounds.Left + 31, bounds.Top + 7, 2, floorY - bounds.Top - 9), beam);
			FillTiles(new Rectangle(bounds.Left + 59, bounds.Top + 7, 2, floorY - bounds.Top - 9), beam);
			FillTiles(new Rectangle(bounds.Left + 64, bounds.Top + 9, 18, 3), glass);
			FillTiles(new Rectangle(bounds.Left + 63, bounds.Top + 8, 20, 1), trim);
			FillTiles(new Rectangle(bounds.Left + 63, bounds.Top + 12, 20, 1), trim);
			for (int x = bounds.Left + 35; x < bounds.Left + 57; x++)
				SetTile(x, floorY - 7, platform);

			// Isolated framing proof: outside corner, half block, and slope all use
			// the same native topology as Terraria's Gray Brick reference.
			FillTiles(new Rectangle(bounds.Left + 74, bounds.Top, 10, 3), block);
			Framing.GetTileSafely(bounds.Left + 76, bounds.Top).IsHalfBlock = true;
			Framing.GetTileSafely(bounds.Left + 81, bounds.Top).Slope = SlopeType.SlopeDownLeft;
		}

		private static void PlaceFurniture(Rectangle bounds, int floorY)
		{
			RequireObject(bounds.Left + 8, floorY - 5, ModContent.TileType<HelixLocker>(), "locker");
			RequireObject(bounds.Left + 14, floorY - 6, ModContent.TileType<HelixSymbioteTank>(), "symbiote tank");
			RequireObject(bounds.Left + 21, floorY - 3, ModContent.TileType<HelixWorkbench>(), "workbench");
			RequireObject(bounds.Left + 25, floorY - 4, ModContent.TileType<HelixConsole>(), "console");

			RequireObject(bounds.Left + 38, floorY - 4, ModContent.TileType<HelixTable>(), "table");
			RequireObject(bounds.Left + 36, floorY - 4, ModContent.TileType<HelixChair>(), "left chair");
			RequireObject(bounds.Left + 43, floorY - 4, ModContent.TileType<HelixChair>(), "right chair", alternate: 1);
			RequireObject(bounds.Left + 50, floorY - 6, ModContent.TileType<HelixSymbioteTank>(), "symbiote tank");

			RequireObject(bounds.Left + 65, floorY - 6, ModContent.TileType<HelixSymbioteTank>(), "symbiote tank");
			RequireObject(bounds.Left + 71, floorY - 5, ModContent.TileType<HelixLocker>(), "locker");
			RequireObject(bounds.Left + 77, floorY - 3, ModContent.TileType<HelixWorkbench>(), "workbench");
			RequireObject(bounds.Left + 81, floorY - 4, ModContent.TileType<HelixConsole>(), "console");

			foreach (int x in new[] { bounds.Left + 10, bounds.Left + 24, bounds.Left + 40, bounds.Left + 54, bounds.Left + 69, bounds.Left + 83 })
				RequireObject(x, floorY - 9, ModContent.TileType<HelixLight>(), "wall light");
		}

		private static void Clear(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
				for (int y = bounds.Top; y < bounds.Bottom; y++)
					Framing.GetTileSafely(x, y).ClearEverything();
		}

		private static void PlaceWastesGround(int left, int right, int surfaceY)
		{
			int grass = ModContent.TileType<WastesGrass>();
			int soil = ModContent.TileType<WastesSoil>();
			for (int x = left; x < right; x++)
			{
				SetTile(x, surfaceY, grass);
				for (int y = surfaceY + 1; y <= surfaceY + 5; y++)
					SetTile(x, y, soil);
			}
		}

		private static void FillTiles(Rectangle area, int type)
		{
			for (int x = area.Left; x < area.Right; x++)
				for (int y = area.Top; y < area.Bottom; y++)
					SetTile(x, y, type);
		}

		private static void FillWall(Rectangle area, int type)
		{
			for (int x = area.Left; x < area.Right; x++)
				for (int y = area.Top; y < area.Bottom; y++)
					Framing.GetTileSafely(x, y).WallType = (ushort)type;
		}

		private static void RequireObject(int left, int top, int type, string label, int style = 0, int alternate = 0)
		{
			TileObjectData data = TileObjectData.GetTileData(type, style, alternate)
				?? throw new InvalidOperationException($"Helix gallery could not resolve {label} object data.");
			int originX = left + data.Origin.X;
			int originY = top + data.Origin.Y;
			if (!WorldGen.PlaceObject(originX, originY, type, mute: true, style: style, alternate: alternate))
				throw new InvalidOperationException($"Helix gallery could not place {label} at {left},{top}.");
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
			tile.LiquidAmount = 0;
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
