using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	/// <summary>Destructive renderer fixture for the complete Wastes ground-cover family.</summary>
	internal static class VegetationLabGallery
	{
		private const int Width = 108;
		private const int Height = 28;

		internal static Rectangle Build(Player player)
		{
			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - Width / 2, 20, Main.maxTilesX - Width - 20);
			int top = Math.Clamp(playerTile.Y - 19, 20, Main.maxTilesY - Height - 20);
			Rectangle bounds = new(left, top, Width, Height);
			int floorY = bounds.Bottom - 3;

			Clear(bounds);
			PlaceFloor(bounds, floorY);
			PlaceFamilies(left, floorY);
			Frame(bounds);
			Lighting.Clear();
			player.Teleport(new Vector2((left + Width / 2) * 16f, (floorY - 5) * 16f), TeleportationStyleID.RodOfDiscord);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, bounds.Center.X, bounds.Center.Y, Width + 4);
			return bounds;
		}

		private static void PlaceFamilies(int left, int floorY)
		{
			int grass = ModContent.TileType<WastesGrass>();
			int soil = ModContent.TileType<WastesSoil>();
			int tuft = ModContent.TileType<DeadTuft>();
			int bristle = ModContent.TileType<WastesBristle>();
			int shrub = ModContent.TileType<WastesRootShrub>();

			// A continuous validated Wastes substrate makes failed anchors obvious.
			for (int x = left + 2; x < left + Width - 2; x++)
			{
				SetTile(x, floorY, grass);
				SetTile(x, floorY + 1, soil);
			}
			Frame(new Rectangle(left, floorY - 2, Width, 4));

			for (int style = 0; style < 4; style++)
				RequireObject(left + 7 + style * 7, floorY - 1, tuft, style, "root tuft");

			for (int style = 0; style < 3; style++)
				RequireObject(left + 42 + style * 9, floorY - 1, bristle, style, "bristle");

			for (int style = 0; style < 3; style++)
				RequireObject(left + 71 + style * 11, floorY - 1, shrub, style, "root shrub");
		}

		private static void RequireObject(int x, int y, int type, int randomStyle, string label)
		{
			// These sheets are one item style with several visual placement variants.
			// StyleMultiplier reserves those frames; the PlaceObject random argument selects one.
			if (!WorldGen.PlaceObject(x, y, type, mute: true, style: 0, random: randomStyle))
				throw new InvalidOperationException($"Vegetation Lab could not place {label} variant {randomStyle} at {x},{y}.");
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
				SetTile(x, floorY + 2, TileID.GrayBrick);
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
