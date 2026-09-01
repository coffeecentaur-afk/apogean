using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using apogean.Content.Tiles;
using apogean.Content.Walls;

namespace apogean.Common.WorldGeneration
{
	/// <summary>
	/// Checks the generated major rupture at the same clearance a player needs. This is kept as
	/// production validation because a build-only test cannot observe vanilla world-generation
	/// features intersecting the Maw.
	/// </summary>
	internal static class MawRuptureValidation
	{
		public static MawRuptureValidationReport Inspect(MawRupturePlan rupture)
		{
			Rectangle bounds = Rectangle.Intersect(
				rupture.ReservedBounds,
				new Rectangle(2, 2, Main.maxTilesX - 4, Main.maxTilesY - 4));
			int width = bounds.Width;
			int height = bounds.Height;
			bool[] visited = new bool[width * height];
			Queue<Point> frontier = new();
			int mawWall = ModContent.WallType<MawWallUnsafe>();
			int legacyAcid = ModContent.TileType<MawAcidPool>();
			int legacyAcidTiles = 0;
			int vanillaLiquidTiles = 0;

			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				for (int y = bounds.Top; y < bounds.Bottom; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile && tile.TileType == legacyAcid)
						legacyAcidTiles++;
					if (tile.WallType == mawWall && tile.LiquidAmount > 0)
						vanillaLiquidTiles++;
				}
			}

			int startTop = Math.Max(bounds.Top, rupture.SurfaceCenter.Y - 4);
			int startBottom = Math.Min(bounds.Bottom - 1, rupture.SurfaceCenter.Y + 28);
			int startLeft = Math.Max(bounds.Left, rupture.SurfaceCenter.X - 34);
			int startRight = Math.Min(bounds.Right - 1, rupture.SurfaceCenter.X + 34);
			for (int x = startLeft; x <= startRight; x++)
			{
				for (int y = startTop; y <= startBottom; y++)
				{
					if (CanOccupy(x, y, mawWall, rupture, requireMawInterior: true))
						Enqueue(x, y, bounds, visited, frontier);
				}
			}

			int deepestReachableY = rupture.SurfaceCenter.Y;
			int visitedCells = 0;
			while (frontier.Count > 0)
			{
				Point point = frontier.Dequeue();
				visitedCells++;
				deepestReachableY = Math.Max(deepestReachableY, point.Y);
				TryVisit(point.X - 1, point.Y, bounds, visited, frontier, mawWall, rupture);
				TryVisit(point.X + 1, point.Y, bounds, visited, frontier, mawWall, rupture);
				TryVisit(point.X, point.Y - 1, bounds, visited, frontier, mawWall, rupture);
				TryVisit(point.X, point.Y + 1, bounds, visited, frontier, mawWall, rupture);
			}

			int requiredDepth = Main.maxTilesY - 205;
			return new MawRuptureValidationReport(
				deepestReachableY >= requiredDepth,
				deepestReachableY,
				requiredDepth,
				visitedCells,
				legacyAcidTiles,
				vanillaLiquidTiles);
		}

		private static void TryVisit(
			int x,
			int y,
			Rectangle bounds,
			bool[] visited,
			Queue<Point> frontier,
			int mawWall,
			MawRupturePlan rupture)
		{
			if (!bounds.Contains(x, y))
				return;
			int index = (y - bounds.Top) * bounds.Width + x - bounds.Left;
			if (visited[index] || !CanOccupy(x, y, mawWall, rupture, requireMawInterior: true))
				return;
			visited[index] = true;
			frontier.Enqueue(new Point(x, y));
		}

		private static void Enqueue(int x, int y, Rectangle bounds, bool[] visited, Queue<Point> frontier)
		{
			int index = (y - bounds.Top) * bounds.Width + x - bounds.Left;
			if (visited[index])
				return;
			visited[index] = true;
			frontier.Enqueue(new Point(x, y));
		}

		private static bool CanOccupy(
			int left,
			int top,
			int mawWall,
			MawRupturePlan rupture,
			bool requireMawInterior)
		{
			bool hasMawInterior = false;
			for (int x = left; x <= left + 1; x++)
			{
				for (int y = top; y <= top + 2; y++)
				{
					if (!WorldGen.InWorld(x, y, 2))
						return false;
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.WallType == mawWall || IsSurfaceMouth(x, y, rupture))
						hasMawInterior = true;
					if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
						return false;
				}
			}

			return !requireMawInterior || hasMawInterior;
		}

		private static bool IsSurfaceMouth(int x, int y, MawRupturePlan rupture)
		{
			float dx = (x - rupture.SurfaceCenter.X) / 36f;
			float dy = (y - rupture.SurfaceCenter.Y) / 20f;
			return dx * dx + dy * dy <= 1f;
		}
	}

	internal readonly record struct MawRuptureValidationReport(
		bool HasContinuousRoute,
		int DeepestReachableY,
		int RequiredDepth,
		int ReachableCells,
		int LegacyAcidTiles,
		int VanillaLiquidTiles)
	{
		public bool Passed => HasContinuousRoute && LegacyAcidTiles == 0;

		public override string ToString() =>
			$"route={(HasContinuousRoute ? "pass" : "blocked")} depth={DeepestReachableY}/{RequiredDepth}; " +
			$"reachable={ReachableCells}; legacy-acid={LegacyAcidTiles}; maw-water={VanillaLiquidTiles}";
	}
}
