using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace apogean.Common.WorldGeneration
{
	/// <summary>
	/// Finds a protected, traversable spine before the Maw edits terrain. The generator decorates
	/// this route; it never invents a second path around obstacles after placement is committed.
	/// </summary>
	internal static class MawNavigationPlanner
	{
		private const int StepY = 7;
		private const int RouteHalfWidth = 12;
		private const int RouteHalfHeight = 9;
		private const int LookAhead = 42;
		private const int RootHalfWidth = 99;
		private const int RootHalfHeight = 53;
		private static string rootFailureDetail = "no candidate inspected";
		public static string LastFailureReason { get; private set; } = "not attempted";

		public static bool TryCreate(
			int surfaceX,
			int surfaceY,
			int horizontalLeash,
			int seed,
			out List<Point16> spine,
			out Point16 matriarchCenter)
		{
			LastFailureReason = "planning";
			spine = new List<Point16>();
			matriarchCenter = default;
			UnifiedRandom random = new(seed);
			// The Stomach ends the open Gullet above Hell. A separate narrow intestinal descent
			// continues below it, so the Matriarch arena never consumes the Wall of Flesh runway.
			int preferredRootY = Utils.Clamp((int)Main.UnderworldLayer - 95, surfaceY + 420, Main.maxTilesY - RootHalfHeight - 14);
			if (!TryFindRootCenter(surfaceX, preferredRootY, horizontalLeash, random, out int rootX, out int rootY))
			{
				LastFailureReason = $"no protected 188x96 Stomach cavity was available within the route leash above the Underworld ceiling ({rootFailureDetail})";
				return false;
			}

			int currentX = surfaceX;
			int startY = surfaceY - 5;
			for (int y = startY; y < rootY; y += StepY)
			{
				float depth = Utils.GetLerpValue(startY, rootY, y, clamped: true);
				float wave = MathF.Sin(depth * MathHelper.TwoPi * 2.35f + seed * 0.00013f) * 18f;
				int desiredX = (int)MathHelper.Lerp(surfaceX, rootX, depth) + (int)wave;
				if (!TryChooseNextPoint(surfaceX, currentX, desiredX, y, horizontalLeash, random, out int nextX))
				{
					LastFailureReason = $"the 25x19 navigation clearance was blocked near depth Y={y}";
					return false;
				}

				currentX = nextX;
				spine.Add(new Point16(currentX, y));
			}

			spine.Add(new Point16(rootX, rootY));
			matriarchCenter = new Point16(rootX, rootY);
			LastFailureReason = "success";
			return spine.Count > 12;
		}

		private static bool TryFindRootCenter(
			int surfaceX,
			int preferredRootY,
			int horizontalLeash,
			UnifiedRandom random,
			out int rootX,
			out int rootY)
		{
			rootFailureDetail = "no candidate inspected";
			int[] offsets = BuildShuffledOffsets(horizontalLeash - 18, 12, random);
			int minimumRootY = Math.Max(420, (int)Main.UnderworldLayer - 105);
			int maximumRootY = Math.Min(
				Main.maxTilesY - RootHalfHeight - 14,
				Math.Max(minimumRootY, (int)Main.UnderworldLayer - 85));
			int[] depthOffsets = BuildDepthOffsets(preferredRootY, minimumRootY, maximumRootY, 12);
			int bestScore = int.MaxValue;
			int bestX = 0;
			int bestY = 0;
			for (int depthIndex = 0; depthIndex < depthOffsets.Length; depthIndex++)
			{
				int candidateY = depthOffsets[depthIndex];
				for (int i = 0; i < offsets.Length; i++)
				{
					int x = surfaceX + offsets[i];
					Rectangle cavity = new(x - RootHalfWidth, candidateY - RootHalfHeight, RootHalfWidth * 2 + 1, RootHalfHeight * 2 + 1);
					if (!WithinWorld(cavity))
						continue;
					if (ContainsProtectedStructure(cavity, out string detail))
					{
						rootFailureDetail = detail;
						continue;
					}

					Rectangle intestinalDescent = new(
						x - 28,
						candidateY + 42,
						57,
						Main.maxTilesY - candidateY - 56);
					if (!WithinWorld(intestinalDescent) || ContainsProtectedStructure(intestinalDescent, out detail))
					{
						rootFailureDetail = $"intestinal descent: {detail}";
						continue;
					}

					int score = CountHardObstacles(cavity, 2) + Math.Abs(candidateY - preferredRootY);
					if (score >= bestScore)
						continue;
					bestScore = score;
					bestX = x;
					bestY = candidateY;
				}

				if (bestScore == 0)
					break;
			}

			rootX = bestX;
			rootY = bestY;
			return bestScore < int.MaxValue;
		}

		private static int[] BuildDepthOffsets(int preferred, int minimum, int maximum, int step)
		{
			List<int> depths = new() { Utils.Clamp(preferred, minimum, maximum) };
			for (int distance = step; preferred - distance >= minimum || preferred + distance <= maximum; distance += step)
			{
				if (preferred + distance <= maximum)
					depths.Add(preferred + distance);
				if (preferred - distance >= minimum)
					depths.Add(preferred - distance);
			}

			return depths.ToArray();
		}

		private static bool TryChooseNextPoint(
			int surfaceX,
			int currentX,
			int desiredX,
			int y,
			int horizontalLeash,
			UnifiedRandom random,
			out int chosenX)
		{
			float bestScore = float.MaxValue;
			chosenX = 0;
			// Keep consecutive drops shallow enough for a two-by-three Terraria character to follow.
			// Larger course corrections happen over several samples through the look-ahead score.
			int jitter = random.Next(-1, 2);
			for (int delta = -9; delta <= 9; delta += 3)
			{
				int x = currentX + delta + jitter;
				if (Math.Abs(x - surfaceX) > horizontalLeash)
					continue;

				Rectangle clearance = CorridorBounds(x, y);
				if (!WithinWorld(clearance) || ContainsHardObstacle(clearance, 1))
					continue;

				Rectangle lookAhead = CorridorBounds(x, Math.Min(Main.maxTilesY - RouteHalfHeight - 3, y + LookAhead));
				float obstaclePenalty = ContainsHardObstacle(lookAhead, 2) ? 480f : 0f;
				float score = Math.Abs(x - desiredX) * 3f + Math.Abs(delta) * 0.8f + obstaclePenalty;
				if (score >= bestScore)
					continue;

				bestScore = score;
				chosenX = x;
			}

			return bestScore < float.MaxValue;
		}

		private static Rectangle CorridorBounds(int x, int y) =>
			new(x - RouteHalfWidth, y - RouteHalfHeight, RouteHalfWidth * 2 + 1, RouteHalfHeight * 2 + 1);

		private static bool ContainsHardObstacle(Rectangle bounds, int stride)
		{
			for (int x = bounds.Left; x < bounds.Right; x += stride)
			{
				for (int y = bounds.Top; y < bounds.Bottom; y += stride)
				{
					if (MawTerrainRules.IsHardGenerationObstacle(x, y))
						return true;
				}
			}

			return false;
		}

		private static int CountHardObstacles(Rectangle bounds, int stride)
		{
			int count = 0;
			for (int x = bounds.Left; x < bounds.Right; x += stride)
				for (int y = bounds.Top; y < bounds.Bottom; y += stride)
					if (MawTerrainRules.IsHardGenerationObstacle(x, y))
						count++;
			return count;
		}

		private static bool ContainsProtectedStructure(Rectangle bounds, out string detail)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
				for (int y = bounds.Top; y < bounds.Bottom; y++)
					if (MawTerrainRules.IsProtectedStructureObstacle(x, y))
					{
						Tile tile = Framing.GetTileSafely(x, y);
						detail = $"last rejection at {x},{y}: tile={tile.TileType}, wall={tile.WallType}, frameImportant={(tile.HasTile && Main.tileFrameImportant[tile.TileType])}, liquid={tile.LiquidType}/{tile.LiquidAmount}";
						return true;
					}
			detail = "no protected obstacle";
			return false;
		}

		private static bool WithinWorld(Rectangle bounds) =>
			bounds.Left >= 12 && bounds.Top >= 12 &&
			bounds.Right < Main.maxTilesX - 12 && bounds.Bottom < Main.maxTilesY - 12;

		private static int[] BuildShuffledOffsets(int radius, int step, UnifiedRandom random)
		{
			List<int> offsets = new() { 0 };
			for (int offset = step; offset <= radius; offset += step)
			{
				offsets.Add(offset);
				offsets.Add(-offset);
			}

			for (int i = offsets.Count - 1; i > 1; i--)
			{
				int swap = random.Next(1, i + 1);
				(offsets[i], offsets[swap]) = (offsets[swap], offsets[i]);
			}

			return offsets.ToArray();
		}
	}
}
