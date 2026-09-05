using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	/// <summary>Bounded accelerated RandomUpdate tests, not a claim about real-time growth rates.</summary>
	internal static class VegetationGrowthLab
	{
		internal static string Run(Rectangle bounds)
		{
			if (Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3")
				throw new InvalidOperationException("Growth checks require the disposable single-player world.");
			int floor = bounds.Bottom - 3;
			int grass = ModContent.TileType<WastesGrass>();
			int soil = ModContent.TileType<WastesSoil>();
			int sapling = ModContent.TileType<DeadForestSapling>();
			// The left panel replaces only the already-tested ground-cover examples.
			for (int x = bounds.Left + 4; x < bounds.Left + 98; x++)
				for (int y = bounds.Top; y <= floor + 1; y++)
				{
					Main.tile[x, y].ClearEverything();
					if (y >= floor) Place(x, y, y == floor ? grass : soil);
				}

			int flatX = bounds.Left + 12, terraceX = bounds.Left + 36, blockedX = bounds.Left + 60;
			for (int x = terraceX - 4; x <= terraceX + 4; x++)
				for (int y = floor - 3; y <= floor; y++) Place(x, y, y == floor - 3 ? grass : soil);
			Tile leftSlope = Main.tile[terraceX - 4, floor - 3];
			Tile rightSlope = Main.tile[terraceX + 4, floor - 3];
			leftSlope.Slope = SlopeType.SlopeDownRight;
			rightSlope.Slope = SlopeType.SlopeDownLeft;
			for (int x = blockedX - 2; x <= blockedX + 2; x++) Place(x, floor - 4, TileID.GrayBrick);
			for (int x = bounds.Left + 4; x < bounds.Left + 98; x++)
				for (int y = floor - 5; y <= floor + 1; y++) WorldGen.SquareTileFrame(x, y, true);

			int flatCalls = GrowFromUpdates(flatX, floor - 1, sapling);
			int terraceCalls = GrowFromUpdates(terraceX, floor - 4, sapling);
			if (!WorldGen.PlaceObject(blockedX, floor - 1, sapling, mute: true))
				throw new InvalidOperationException("Could not place the blocked-canopy sapling control.");
			for (int attempt = 0; attempt < 256; attempt++) ModContent.GetInstance<DeadForestSapling>().RandomUpdate(blockedX, floor - 1);
			if (!Main.tile[blockedX, floor - 1].HasTile || Main.tile[blockedX, floor - 1].TileType != sapling)
				throw new InvalidOperationException("Blocked sapling grew through the roof or disappeared.");

			int nearX = flatX + 2;
			bool placedNear = WorldGen.PlaceObject(nearX, floor - 1, sapling, mute: true);
			if (placedNear && WorldGen.GrowTree(nearX, floor - 1))
				throw new InvalidOperationException("Tree grew within two tiles of the neighboring trunk.");
			// Native roots belong on full support. Sloped surroundings are valid; a sloped anchor is not.
			int slopeX = bounds.Left + 84;
			Tile slopeAnchor = Main.tile[slopeX, floor];
			slopeAnchor.Slope = SlopeType.SlopeDownRight;
			bool placedSlope = WorldGen.PlaceObject(slopeX, floor - 1, sapling, mute: true);
			if (placedSlope && WorldGen.GrowTree(slopeX, floor - 1))
				throw new InvalidOperationException("Tree grew on a sloped anchor.");
			Lighting.Clear();
			return $"PASS flat RandomUpdate calls={flatCalls}, height={Height(flatX, floor - 1)}; terrace calls={terraceCalls}, height={Height(terraceX, floor - 4)}; roof blocked after 256 updates; two-tile neighbor and sloped anchor rejected. Accelerated hook/API checks, not elapsed-time or manual Acorn-use proof.";
		}

		private static int GrowFromUpdates(int x, int y, int sapling)
		{
			if (!WorldGen.PlaceObject(x, y, sapling, mute: true))
				throw new InvalidOperationException($"Could not place growth sapling at {x},{y}.");
			for (int attempt = 1; attempt <= 256; attempt++)
			{
				ModContent.GetInstance<DeadForestSapling>().RandomUpdate(x, y);
				if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileID.Trees) return attempt;
			}
			throw new InvalidOperationException($"No native growth after 256 accelerated updates at {x},{y}.");
		}

		private static int Height(int x, int rootY)
		{
			int top = rootY;
			while (top > 10 && Main.tile[x, top - 1].HasTile && Main.tile[x, top - 1].TileType == TileID.Trees) top--;
			return rootY - top + 1;
		}

		private static void Place(int x, int y, int type)
		{
			Tile tile = Main.tile[x, y];
			tile.ClearEverything();
			tile.HasTile = true;
			tile.TileType = (ushort)type;
		}
	}
}
