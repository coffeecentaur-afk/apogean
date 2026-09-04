using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	/// <summary>Disposable wall-free cavern used to validate the four-layer underground background contract.</summary>
	internal static class UndergroundBackgroundLabGallery
	{
		private const int Width = 190;
		private const int Height = 86;

		internal static Rectangle Build(Player player, bool underworld)
		{
			int centerX = Math.Clamp(Main.spawnTileX, Width / 2 + 30, Main.maxTilesX - Width / 2 - 30);
			int requestedCenterY = underworld
				? Main.maxTilesY - 150
				: (int)((Main.worldSurface + Main.rockLayer) * 0.5);
			int centerY = Math.Clamp(requestedCenterY, Height / 2 + 30, Main.maxTilesY - Height / 2 - 30);
			Rectangle bounds = new(centerX - Width / 2, centerY - Height / 2, Width, Height);
			int floorY = bounds.Bottom - 8;
			int wastesStone = ModContent.TileType<WastesStone>();

			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
				Framing.GetTileSafely(x, y).ClearEverything();

			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				int rise = (Math.Abs(x - centerX) > Width / 2 - 14) ? 5 : 0;
				for (int y = floorY - rise; y < bounds.Bottom; y++)
					SetTile(x, y, wastesStone);
			}

			// Small known-solid sconces make the background visible through Terraria's
			// underground lighting mask without wallpapering over the scene being tested.
			for (int x = bounds.Left + 14; x < bounds.Right - 14; x += 24)
			{
				SetTile(x, floorY - 2, TileID.GrayBrick);
				WorldGen.PlaceTile(x + 1, floorY - 2, TileID.Torches, mute: true, forced: true);
			}

			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
			{
				WorldGen.SquareTileFrame(x, y, true);
				WorldGen.SquareWallFrame(x, y, true);
			}

			Lighting.Clear();
			player.Teleport(new Vector2(centerX * 16f, (floorY - 3) * 16f), TeleportationStyleID.RodOfDiscord);
			player.fallStart = (int)(player.position.Y / 16f);
			player.statLife = player.statLifeMax2;
			player.immune = true;
			player.immuneTime = 600;
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, bounds.Center.X, bounds.Center.Y, Width + 6);
			return bounds;
		}

		internal static void LightVisibleBackground(Player player)
		{
			Point center = player.Center.ToTileCoordinates();
			for (int x = center.X - 78; x <= center.X + 78; x += 12)
			for (int y = center.Y - 36; y <= center.Y + 18; y += 9)
				Lighting.AddLight(x, y, 0.95f, 0.72f, 0.42f);
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
	}
}
