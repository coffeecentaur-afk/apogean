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
		private const int Width = 170;
		private const int Height = 45;

		internal static Rectangle Build(Player player)
		{
			// Keep visual captures deterministic and readable. This fixture validates
			// silhouettes and terrain seams, which are obscured by Terraria's night tint.
			Main.dayTime = true;
			Main.time = 27000d;

			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - Width / 2, 20, Main.maxTilesX - Width - 20);
			int top = Math.Clamp(playerTile.Y - 23, 20, Main.maxTilesY - Height - 20);
			Rectangle bounds = new(left, top, Width, Height);
			int floorY = bounds.Bottom - 3;

			Clear(bounds);
			ClearLooseItems(bounds);
			PlaceFloor(bounds, floorY);
			PlaceFamilies(left, floorY);
			PlaceTrees(left, floorY);
			Frame(bounds);
			Lighting.Clear();
			// Center the camera on the isolated tree suite while retaining the complete
			// ground-cover family at the left edge of a 2560px validation capture.
			player.Teleport(new Vector2((left + 128) * 16f, (floorY - 5) * 16f), TeleportationStyleID.RodOfDiscord);
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

		private static void PlaceTrees(int left, int floorY)
		{
			int sapling = ModContent.TileType<DeadForestSapling>();
			// Four well-separated growth calls prove Terraria's native height, branch,
			// and crown variation without recreating the production forest pileup. The
			// last tree is sacrificed to a deterministic mid-trunk chopping assertion.
			int[] treeX = { left + 104, left + 123, left + 142, left + 160 };
			foreach (int x in treeX)
			{
				if (!WorldGen.PlaceObject(x, floorY - 1, sapling, mute: true))
					throw new InvalidOperationException($"Vegetation Lab could not place a dead-tree sapling at {x},{floorY - 1}.");
				if (!WorldGen.GrowTree(x, floorY - 1))
					throw new InvalidOperationException($"Vegetation Lab could not grow a dead forest tree at {x},{floorY - 1}.");
			}

			ValidateMidTrunkChop(treeX[^1], floorY - 1);
		}

		private static void ValidateMidTrunkChop(int x, int rootY)
		{
			int topY = rootY;
			while (topY > 10)
			{
				Tile candidate = Framing.GetTileSafely(x, topY - 1);
				if (!candidate.HasTile || candidate.TileType != TileID.Trees)
					break;
				topY--;
			}

			int height = rootY - topY + 1;
			// Native tree growth legitimately produces short variants. Four trunk
			// cells still leaves a segment above and below a rootY - 2 chop, so the
			// fixture can prove Terraria's native split behavior without randomly
			// rejecting a valid small tree.
			if (height < 4)
				throw new InvalidOperationException($"Vegetation Lab tree at {x},{rootY} was too short for a mid-trunk chop proof.");

			int cutY = rootY - Math.Max(2, height / 2);
			WorldGen.KillTile(x, cutY, noItem: true);

			Tile removedSegment = Framing.GetTileSafely(x, cutY);
			if (removedSegment.HasTile && removedSegment.TileType == TileID.Trees)
				throw new InvalidOperationException($"Native tree segment at {x},{cutY} survived the chop proof.");

			Tile stump = Framing.GetTileSafely(x, cutY + 1);
			if (!stump.HasTile || stump.TileType != TileID.Trees)
				throw new InvalidOperationException($"Dead tree chop proof removed the stump below {x},{cutY}.");

			for (int y = topY; y < cutY; y++)
			{
				Tile unsupported = Framing.GetTileSafely(x, y);
				if (unsupported.HasTile && unsupported.TileType == TileID.Trees)
					throw new InvalidOperationException($"Dead tree chop proof left an unsupported trunk segment at {x},{y}.");
			}
		}

		private static void ClearLooseItems(Rectangle bounds)
		{
			Rectangle worldPixels = new(bounds.X * 16, bounds.Y * 16, bounds.Width * 16, bounds.Height * 16);
			for (int i = 0; i < Main.maxItems; i++)
			{
				Item item = Main.item[i];
				if (item.active && worldPixels.Contains(item.Center.ToPoint()))
					item.active = false;
			}
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
