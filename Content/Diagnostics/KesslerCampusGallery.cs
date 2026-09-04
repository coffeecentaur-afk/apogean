using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Factions;
using apogean.Content.Structures;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	/// <summary>
	/// Disposable full-scale renderer fixture for the authored Kessler campus. It uses the same
	/// template and terrain-integration path as world generation, but clears a known test world so
	/// an older campus cannot leak through the new silhouette.
	/// </summary>
	internal static class KesslerCampusGallery
	{
		private const int Width = 208;
		private const int Height = 96;
		private const int SurfaceOffset = 54;

		internal static Rectangle Build(Mod mod, Player player)
		{
			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - Width / 2, 24, Main.maxTilesX - Width - 24);
			int surfaceY = Math.Clamp(playerTile.Y + 7, 100, Main.maxTilesY - 50);
			int top = surfaceY - SurfaceOffset;
			Rectangle atlasBounds = new(left, top, Width, Height);

			Clear(new Rectangle(left - 12, top, Width + 24, Height));
			PlaceWastesGround(left - 12, left + Width + 12, surfaceY);
			AuthoredStructurePlacement placement = CorporateCampusBlueprints.Place(mod, ApogeanFaction.Kessler, atlasBounds);
			Frame(new Rectangle(left - 12, top, Width + 24, Height));

			player.Teleport(new Vector2((placement.Entrance.Center.X + 0.5f) * 16f,
				(placement.Entrance.Bottom - 2) * 16f), TeleportationStyleID.RodOfDiscord);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, atlasBounds.Center.X, atlasBounds.Center.Y, Width + 28);
			return placement.Bounds;
		}

		private static void Clear(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				for (int y = bounds.Top; y < bounds.Bottom; y++)
					Framing.GetTileSafely(x, y).ClearEverything();
			}
		}

		private static void PlaceWastesGround(int left, int right, int surfaceY)
		{
			int grass = ModContent.TileType<WastesGrass>();
			int soil = ModContent.TileType<WastesSoil>();
			for (int x = left; x < right; x++)
			{
				SetTile(x, surfaceY, grass);
				for (int y = surfaceY + 1; y <= surfaceY + 20; y++)
					SetTile(x, y, soil);
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
