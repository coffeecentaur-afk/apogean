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
	/// Disposable three-room Kessler cutaway. This stays near vanilla room scale so
	/// every construction material, wall, light, animated fixture, and furniture
	/// family can be judged at gameplay zoom before the Campus consumes it.
	/// </summary>
	internal static class KesslerConstructionGallery
	{
		private const int Width = 92;
		private const int Height = 27;

		internal static Rectangle Build(Player player)
		{
			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - Width / 2, 24, Main.maxTilesX - Width - 24);
			int top = Math.Clamp(playerTile.Y - 14, 80, Main.maxTilesY - Height - 40);
			Rectangle bounds = new(left, top, Width, Height);
			int floorY = bounds.Bottom - 4;

			Clear(new Rectangle(bounds.Left - 4, bounds.Top - 2, bounds.Width + 8, bounds.Height + 8));
			PlaceWastesGround(bounds.Left - 4, bounds.Right + 4, floorY + 3);
			PlaceConstruction(bounds, floorY);
			Frame(new Rectangle(bounds.Left - 4, bounds.Top - 2, bounds.Width + 8, bounds.Height + 8));
			PlaceFurniture(bounds, floorY);
			Lighting.Clear();

			Main.dayTime = true;
			Main.time = 27000.0;
			player.Teleport(new Vector2((bounds.Center.X + 0.5f) * 16f, (floorY - 3) * 16f), TeleportationStyleID.RodOfDiscord);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, bounds.Center.X, bounds.Center.Y, bounds.Width + 12);
			// Include the roof standard in Capture Camera's framing.
			return new Rectangle(bounds.Left, bounds.Top - 2, bounds.Width, bounds.Height + 2);
		}

		private static void PlaceConstruction(Rectangle bounds, int floorY)
		{
			int block = ModContent.TileType<KesslerBlock>();
			int trim = ModContent.TileType<KesslerTrim>();
			int floor = ModContent.TileType<KesslerFloor>();
			int glass = ModContent.TileType<KesslerGlass>();
			int beam = ModContent.TileType<KesslerBeam>();
			int platform = ModContent.TileType<KesslerPlatform>();
			int bulkheadWall = ModContent.WallType<KesslerBulkheadWall>();
			int windowWall = ModContent.WallType<KesslerWindowWall>();

			// Three compact wall fields keep the sample at usable Terraria room scale.
			// The command bay uses a separate field behind its observation slit.
			FillWall(new Rectangle(bounds.Left + 5, bounds.Top + 7, 56, floorY - bounds.Top - 8), bulkheadWall);
			FillWall(new Rectangle(bounds.Left + 61, bounds.Top + 7, 26, floorY - bounds.Top - 8), windowWall);

			FillTiles(new Rectangle(bounds.Left + 3, bounds.Top + 5, bounds.Width - 6, 2), block);
			FillTiles(new Rectangle(bounds.Left + 1, bounds.Top + 3, bounds.Width - 2, 2), trim);
			FillTiles(new Rectangle(bounds.Left + 3, floorY, bounds.Width - 6, 3), block);
			FillTiles(new Rectangle(bounds.Left + 4, floorY - 2, bounds.Width - 8, 2), floor);
			FillTiles(new Rectangle(bounds.Left + 3, bounds.Top + 5, 2, floorY - bounds.Top - 5), block);
			FillTiles(new Rectangle(bounds.Right - 5, bounds.Top + 5, 2, floorY - bounds.Top - 5), block);

			// Structural divisions, a framed observation slit, and a short walkable catwalk.
			FillTiles(new Rectangle(bounds.Left + 31, bounds.Top + 7, 2, floorY - bounds.Top - 9), beam);
			FillTiles(new Rectangle(bounds.Left + 59, bounds.Top + 7, 2, floorY - bounds.Top - 9), beam);
			FillTiles(new Rectangle(bounds.Left + 65, bounds.Top + 9, 18, 3), glass);
			FillTiles(new Rectangle(bounds.Left + 64, bounds.Top + 8, 20, 1), trim);
			FillTiles(new Rectangle(bounds.Left + 64, bounds.Top + 12, 20, 1), trim);
			for (int x = bounds.Left + 37; x < bounds.Left + 55; x++) SetTile(x, floorY - 9, platform);

			// A small isolated framing suite proves corners, a half-block, and a slope.
			FillTiles(new Rectangle(bounds.Left + 74, bounds.Top, 10, 3), block);
			Framing.GetTileSafely(bounds.Left + 76, bounds.Top).IsHalfBlock = true;
			Framing.GetTileSafely(bounds.Left + 81, bounds.Top).Slope = SlopeType.SlopeDownLeft;
		}

		private static void PlaceFurniture(Rectangle bounds, int floorY)
		{
			RequireObject(bounds.Left + 8, floorY - 5, ModContent.TileType<KesslerLocker>(), "locker");
			RequireObject(bounds.Left + 13, floorY - 6, ModContent.TileType<KesslerPowerArmorRack>(), "power-armour rack");
			RequireObject(bounds.Left + 20, floorY - 3, ModContent.TileType<KesslerWorkbench>(), "workbench");
			RequireObject(bounds.Left + 25, floorY - 4, ModContent.TileType<KesslerConsole>(), "console");

			RequireObject(bounds.Left + 39, floorY - 4, ModContent.TileType<KesslerTable>(), "table");
			RequireObject(bounds.Left + 37, floorY - 4, ModContent.TileType<KesslerChair>(), "left chair");
			RequireObject(bounds.Left + 43, floorY - 4, ModContent.TileType<KesslerChair>(), "right chair", alternate: 1);
			RequireObject(bounds.Left + 49, floorY - 4, ModContent.TileType<KesslerConsole>(), "console");
			RequireObject(bounds.Left + 55, floorY - 6, ModContent.TileType<KesslerPowerArmorRack>(), "power-armour rack");

			RequireObject(bounds.Left + 65, floorY - 6, ModContent.TileType<KesslerPowerArmorRack>(), "power-armour rack");
			RequireObject(bounds.Left + 71, floorY - 5, ModContent.TileType<KesslerLocker>(), "locker");
			RequireObject(bounds.Left + 76, floorY - 3, ModContent.TileType<KesslerWorkbench>(), "workbench");
			RequireObject(bounds.Left + 81, floorY - 4, ModContent.TileType<KesslerConsole>(), "console");

			// Lights sit near the occupied plane; roof-only fixtures leave Terraria rooms
			// visually black even when their map colour is correct.
			foreach (int x in new[] { bounds.Left + 10, bounds.Left + 21, bounds.Left + 39, bounds.Left + 51, bounds.Left + 68, bounds.Left + 81 })
				RequireObject(x, floorY - 8, ModContent.TileType<KesslerLight>(), "wall light");

			// The flag anchors to the trim roof at its pole; its cloth remains open-air.
			RequireObject(bounds.Left + 43, bounds.Top - 1, ModContent.TileType<KesslerWarBanner>(), "war banner");
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
				for (int y = surfaceY + 1; y <= surfaceY + 5; y++) SetTile(x, y, soil);
			}
		}

		private static void FillTiles(Rectangle area, int type)
		{
			for (int x = area.Left; x < area.Right; x++)
				for (int y = area.Top; y < area.Bottom; y++) SetTile(x, y, type);
		}

		private static void FillWall(Rectangle area, int type)
		{
			for (int x = area.Left; x < area.Right; x++)
				for (int y = area.Top; y < area.Bottom; y++) Framing.GetTileSafely(x, y).WallType = (ushort)type;
		}

		private static void RequireObject(int left, int top, int type, string label, int style = 0, int alternate = 0)
		{
			TileObjectData data = TileObjectData.GetTileData(type, style, alternate)
				?? throw new InvalidOperationException($"Kessler gallery could not resolve {label} object data.");
			int originX = left + data.Origin.X;
			int originY = top + data.Origin.Y;
			if (!WorldGen.PlaceObject(originX, originY, type, mute: true, style: style, alternate: alternate))
				throw new InvalidOperationException($"Kessler gallery could not place {label} at {left},{top}.");
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
