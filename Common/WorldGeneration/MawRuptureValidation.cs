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

			int requiredDepth = rupture.MatriarchCenter.Y > 0
				? rupture.MatriarchCenter.Y - 8
				: Main.maxTilesY - 205;
			bool allSpinePointsReached = AreAllSpinePointsReached(bounds, visited, rupture);
			bool hasContinuousRoute = deepestReachableY >= requiredDepth && allSpinePointsReached;
			string obstruction = hasContinuousRoute
				? "all-spine-waypoints-reached"
				: DescribeObstruction(bounds, visited, deepestReachableY, mawWall, rupture);
			return new MawRuptureValidationReport(
				hasContinuousRoute,
				deepestReachableY,
				requiredDepth,
				visitedCells,
				legacyAcidTiles,
				vanillaLiquidTiles,
				obstruction);
		}

		private static bool AreAllSpinePointsReached(Rectangle bounds, bool[] visited, MawRupturePlan rupture)
		{
			for (int i = 0; i < rupture.NavigationSpine.Count; i++)
			{
				Terraria.DataStructures.Point16 point = rupture.NavigationSpine[i];
				int left = point.X - 1;
				int top = point.Y - 2;
				if (!bounds.Contains(left, top))
					return false;
				int index = (top - bounds.Top) * bounds.Width + left - bounds.Left;
				if (!visited[index])
					return false;
			}

			return true;
		}

		private static string DescribeObstruction(
			Rectangle bounds,
			bool[] visited,
			int deepestY,
			int mawWall,
			MawRupturePlan rupture)
		{
			int frontierX = rupture.SurfaceCenter.X;
			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				int index = (deepestY - bounds.Top) * bounds.Width + x - bounds.Left;
				if (index >= 0 && index < visited.Length && visited[index])
				{
					frontierX = x;
					break;
				}
			}

			int blockedY = deepestY + 1;
			string footprint = "missing-maw-interior";
			for (int x = frontierX; x <= frontierX + 1; x++)
			{
				for (int y = blockedY; y <= blockedY + 2; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
					{
						ModTile modTile = TileLoader.GetTile(tile.TileType);
						string tileName = modTile?.FullName ?? "vanilla";
						footprint = $"solid tile={tile.TileType} ({tileName}) wall={tile.WallType} at {x},{y}";
					}
				}
			}

			int nearestIndex = -1;
			int nearestDistance = int.MaxValue;
			for (int i = 0; i < rupture.NavigationSpine.Count; i++)
			{
				int distance = Math.Abs(rupture.NavigationSpine[i].Y - deepestY);
				if (distance >= nearestDistance)
					continue;
				nearestDistance = distance;
				nearestIndex = i;
			}

			if (nearestIndex < 0)
				return $"frontier={frontierX},{deepestY}; {footprint}; no-spine";

			Terraria.DataStructures.Point16 spine = rupture.NavigationSpine[nearestIndex];
			bool spineOpen = CanOccupy(spine.X - 1, spine.Y - 2, mawWall, rupture, requireMawInterior: true);
			string firstUnreached = DescribeFirstUnreachedSpinePoint(bounds, visited, mawWall, rupture);
			return $"frontier={frontierX},{deepestY}; {footprint}; spine[{nearestIndex}]={spine.X},{spine.Y} open={spineOpen}; {firstUnreached}";
		}

		private static string DescribeFirstUnreachedSpinePoint(
			Rectangle bounds,
			bool[] visited,
			int mawWall,
			MawRupturePlan rupture)
		{
			for (int i = 0; i < rupture.NavigationSpine.Count; i++)
			{
				Terraria.DataStructures.Point16 point = rupture.NavigationSpine[i];
				int left = point.X - 1;
				int top = point.Y - 2;
				if (!bounds.Contains(left, top))
					return $"first-unreached-spine[{i}]={point.X},{point.Y} outside-validation-bounds";

				int index = (top - bounds.Top) * bounds.Width + left - bounds.Left;
				if (visited[index])
					continue;

				bool open = CanOccupy(left, top, mawWall, rupture, requireMawInterior: true);
				Tile center = Framing.GetTileSafely(point.X, point.Y);
				ModTile modTile = center.HasTile ? TileLoader.GetTile(center.TileType) : null;
				string centerName = center.HasTile ? modTile?.FullName ?? "vanilla" : "empty";
				return $"first-unreached-spine[{i}]={point.X},{point.Y} open={open} center={center.TileType} ({centerName}) wall={center.WallType}";
			}

			return "all-spine-waypoints-reached";
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
		int VanillaLiquidTiles,
		string Obstruction)
	{
		public bool Passed => HasContinuousRoute && LegacyAcidTiles == 0;

		public override string ToString() =>
			$"route={(HasContinuousRoute ? "pass" : "blocked")} depth={DeepestReachableY}/{RequiredDepth}; " +
			$"reachable={ReachableCells}; legacy-acid={LegacyAcidTiles}; maw-water={VanillaLiquidTiles}; {Obstruction}";
	}
}
