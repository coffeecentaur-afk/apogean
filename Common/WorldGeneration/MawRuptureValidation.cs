using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
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
			int maximumVerticalFall = MeasureMaximumVerticalFall(rupture, mawWall);
			// Include the noisy three-tile shell, not just the 48-tile carved ellipse.
			int stomachFloorClearance = (int)Main.UnderworldLayer - (rupture.MatriarchCenter.Y + 54);
			int deepestIntestinalInterior = MeasureDeepestIntestinalInterior(rupture, mawWall);
			int requiredIntestinalDepth = Main.maxTilesY - 30;
			int outletPlugTiles = CountStomachOutletPlug(rupture);
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
				maximumVerticalFall,
				stomachFloorClearance,
				deepestIntestinalInterior,
				requiredIntestinalDepth,
				outletPlugTiles,
				obstruction);
		}

		private static int MeasureDeepestIntestinalInterior(MawRupturePlan rupture, int mawWall)
		{
			Rectangle bounds = Rectangle.Intersect(
				rupture.IntestinalDescentBounds,
				new Rectangle(2, 2, Main.maxTilesX - 4, Main.maxTilesY - 4));
			int deepest = rupture.MatriarchCenter.Y;
			for (int y = bounds.Top; y < bounds.Bottom; y++)
			{
				for (int x = bounds.Left; x < bounds.Right; x++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.WallType == mawWall && !tile.HasTile)
						deepest = y;
				}
			}

			return deepest;
		}

		private static int CountStomachOutletPlug(MawRupturePlan rupture)
		{
			int mawstone = ModContent.TileType<Mawstone>();
			int outletY = rupture.MatriarchCenter.Y + 43;
			int count = 0;
			for (int x = rupture.MatriarchCenter.X - 8; x <= rupture.MatriarchCenter.X + 8; x++)
			{
				for (int y = outletY - 1; y <= outletY + 3; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile && tile.TileType == mawstone)
						count++;
				}
			}

			return count;
		}

		private static int MeasureMaximumVerticalFall(MawRupturePlan rupture, int mawWall)
		{
			if (!rupture.HasNavigationSpine)
				return int.MaxValue;

			int minX = int.MaxValue;
			int maxX = int.MinValue;
			for (int i = 0; i < rupture.NavigationSpine.Count; i++)
			{
				minX = Math.Min(minX, rupture.NavigationSpine[i].X - 20);
				maxX = Math.Max(maxX, rupture.NavigationSpine[i].X + 20);
			}

			int top = rupture.SurfaceCenter.Y;
			int bottom = rupture.MatriarchCenter.Y - 66;
			int maximum = 0;
			for (int x = minX; x <= maxX; x++)
			{
				int run = 0;
				int spineIndex = 0;
				for (int y = top; y <= bottom; y++)
				{
					while (spineIndex + 1 < rupture.NavigationSpine.Count && rupture.NavigationSpine[spineIndex + 1].Y <= y)
						spineIndex++;
					if (spineIndex >= rupture.NavigationSpine.Count - 1 && rupture.NavigationSpine[spineIndex].Y < y)
					{
						run = 0;
						continue;
					}

					Point16 current = rupture.NavigationSpine[spineIndex];
					Point16 next = rupture.NavigationSpine[Math.Min(spineIndex + 1, rupture.NavigationSpine.Count - 1)];
					float amount = next.Y == current.Y ? 0f : Utils.GetLerpValue(current.Y, next.Y, y, clamped: true);
					int centerX = (int)MathHelper.Lerp(current.X, next.X, amount);
					if (Math.Abs(x - centerX) > 20)
					{
						run = 0;
						continue;
					}

					Tile tile = Framing.GetTileSafely(x, y);
					bool solid = tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
					if (!solid && tile.WallType == mawWall)
					{
						run++;
						maximum = Math.Max(maximum, run);
					}
					else
					{
						run = 0;
					}
				}
			}

			return maximum;
		}

		private static bool AreAllSpinePointsReached(Rectangle bounds, bool[] visited, MawRupturePlan rupture)
		{
			for (int i = 0; i < rupture.NavigationSpine.Count; i++)
			{
				if (!IsSpineCrossSectionReached(bounds, visited, rupture.NavigationSpine[i]))
					return false;
			}

			return true;
		}

		private static bool IsSpineCrossSectionReached(Rectangle bounds, bool[] visited, Point16 point)
		{
			// Ribs deliberately cover the exact centerline. A waypoint is valid when any player-sized
			// position in the local cross-section is connected to the mouth.
			for (int top = point.Y - 4; top <= point.Y + 2; top++)
			{
				for (int left = point.X - 10; left <= point.X + 9; left++)
				{
					if (!bounds.Contains(left, top))
						continue;
					int index = (top - bounds.Top) * bounds.Width + left - bounds.Left;
					if (visited[index])
						return true;
				}
			}

			return false;
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
				if (IsSpineCrossSectionReached(bounds, visited, point))
					continue;

				int left = point.X - 1;
				int top = point.Y - 2;
				if (!bounds.Contains(left, top))
					return $"first-unreached-spine[{i}]={point.X},{point.Y} outside-validation-bounds";

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
		int MaximumVerticalFall,
		int StomachFloorClearance,
		int DeepestIntestinalInterior,
		int RequiredIntestinalDepth,
		int OutletPlugTiles,
		string Obstruction)
	{
		public const int MaximumAllowedVerticalFall = 120;
		public const int MinimumStomachFloorClearance = 30;
		public const int MaximumStomachFloorClearance = 60;
		public const int MinimumOutletPlugTiles = 70;
		public bool Passed =>
			HasContinuousRoute &&
			LegacyAcidTiles == 0 &&
			MaximumVerticalFall <= MaximumAllowedVerticalFall &&
			StomachFloorClearance is >= MinimumStomachFloorClearance and <= MaximumStomachFloorClearance &&
			DeepestIntestinalInterior >= RequiredIntestinalDepth &&
			OutletPlugTiles >= MinimumOutletPlugTiles;

		public override string ToString() =>
			$"route={(HasContinuousRoute ? "pass" : "blocked")} depth={DeepestReachableY}/{RequiredDepth}; " +
			$"reachable={ReachableCells}; max-fall={MaximumVerticalFall}/{MaximumAllowedVerticalFall}; " +
			$"stomach-clearance={StomachFloorClearance}/{MinimumStomachFloorClearance}-{MaximumStomachFloorClearance}; " +
			$"intestine={DeepestIntestinalInterior}/{RequiredIntestinalDepth}; plug={OutletPlugTiles}/{MinimumOutletPlugTiles}; " +
			$"legacy-acid={LegacyAcidTiles}; maw-water={VanillaLiquidTiles}; {Obstruction}";
	}
}
