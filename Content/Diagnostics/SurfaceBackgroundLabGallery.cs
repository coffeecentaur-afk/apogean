using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	/// <summary>Wall-free, low-profile surface fixture for judging authored parallax layers.</summary>
	internal static class SurfaceBackgroundLabGallery
	{
		private const int Width = 190;
		private const int Height = 62;

		internal static Rectangle Build(Player player)
		{
			// Keep renderer evidence comparable between runs. Night tint is useful
			// later, but it hides palette and layer-separation mistakes during art QA.
			Main.dayTime = true;
			Main.time = 27000d;
			Main.raining = false;

			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - Width / 2, 20, Main.maxTilesX - Width - 20);
			int top = Math.Clamp(playerTile.Y - 35, 20, Main.maxTilesY - Height - 20);
			Rectangle bounds = new(left, top, Width, Height);
			int floorY = bounds.Bottom - 8;
			int sand = ModContent.TileType<WastesSandCandidate>();

			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
				Framing.GetTileSafely(x, y).ClearEverything();

			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				int edgeDistance = Math.Min(x - bounds.Left, bounds.Right - 1 - x);
				int duneRise = edgeDistance < 22 ? (22 - edgeDistance) / 5 : 0;
				int ripple = ((x - bounds.Left) / 17) % 2;
				for (int y = floorY - duneRise - ripple; y < bounds.Bottom; y++)
					SetTile(x, y, sand);
			}

			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
			{
				WorldGen.SquareTileFrame(x, y, true);
				WorldGen.SquareWallFrame(x, y, true);
			}

			Lighting.Clear();
			player.Teleport(new Vector2(bounds.Center.X * 16f, (floorY - 4) * 16f), TeleportationStyleID.RodOfDiscord);
			player.fallStart = (int)(player.position.Y / 16f);
			player.statLife = player.statLifeMax2;
			player.immune = true;
			player.immuneTime = 600;
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, bounds.Center.X, bounds.Center.Y, Width + 6);
			return bounds;
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
	}
}
