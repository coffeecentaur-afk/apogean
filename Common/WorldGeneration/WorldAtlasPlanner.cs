using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace apogean.Common.WorldGeneration
{
	/// <summary>
	/// Solves all day-one foreground landmarks against one occupied-region set. No feature is
	/// allowed to choose a location without seeing the Maw, spawn, and previously accepted sites.
	/// </summary>
	internal static class WorldAtlasPlanner
	{
		private static bool[] allowedReplacementTiles;
		private static string lastFailureReason = "no landmark search has run";
		private enum SurfaceRejection : byte { None, Empty, Relief, Evil, Jungle, Desert }

		public static List<ApogeanLandmarkPlan> PlaceLandmarks(
			UnifiedRandom random,
			Func<int, int> findSurface,
			MawRupturePlan majorRupture,
			List<Rectangle> occupied)
		{
			List<ApogeanLandmarkPlan> landmarks = new();
			int spawnX = Main.spawnTileX > 0 ? Main.spawnTileX : Main.maxTilesX / 2;
			int mawSide = Math.Sign(majorRupture.SurfaceCenter.X - spawnX);
			if (mawSide == 0)
				mawSide = 1;

			List<ApogeanLandmarkPlan> kesslerCandidates = FindSurfaceCampusCandidates(
				ApogeanLandmarkKind.KesslerCampus,
				// Helix's cracked dome and descending laboratory can bridge severe surface relief.
				// Structural tiles and protected regions remain hard exclusions below.
				260,
				120,
				208,
				96,
				40,
				spawnX - mawSide * 1040,
				900,
				70,
				96,
				occupied,
				random,
				findSurface);

			List<ApogeanLandmarkPlan> helixCandidates = FindSurfaceCampusCandidates(
				ApogeanLandmarkKind.HelixCampus,
				240,
				230,
				192,
				184,
				40,
				majorRupture.SurfaceCenter.X + mawSide * 680,
				1050,
				45,
				260,
				occupied,
				random,
				findSurface);

			if (!TryChooseSurfaceCampusPair(kesslerCandidates, helixCandidates, out ApogeanLandmarkPlan kessler, out ApogeanLandmarkPlan helix))
			{
				throw new InvalidOperationException(
					$"Apogee could not fit a non-overlapping Kessler/Helix Campus pair on this seed " +
					$"(Kessler candidates={kesslerCandidates.Count}, Helix candidates={helixCandidates.Count}); {lastFailureReason}.");
			}
			AddRequired(landmarks, occupied, kessler);
			AddRequired(landmarks, occupied, helix);

			AddRequired(landmarks, occupied, FindSkyCampusSite(
				ApogeanLandmarkKind.SentrixCampus,
				220,
				200,
				176,
				160,
				32,
				occupied,
				random));

			AddRequired(landmarks, occupied, FindSurfaceSite(
				ApogeanLandmarkKind.AbandonedKesslerOutpost,
				random.Next(60, 91),
				random.Next(35, 56),
				18,
				spawnX - mawSide * 1700,
				1800,
				22,
				96,
				occupied,
				random,
				findSurface));

			AddRequired(landmarks, occupied, FindUndergroundSite(
				ApogeanLandmarkKind.AbandonedHelixLaboratory,
				random.Next(70, 101),
				random.Next(55, 86),
				18,
				occupied,
				random));

			AddRequired(landmarks, occupied, FindSurfaceSite(
				ApogeanLandmarkKind.CrashedSentrixRelay,
				random.Next(55, 86),
				random.Next(35, 61),
				16,
				spawnX + mawSide * 1900,
				2200,
				24,
				96,
				occupied,
				random,
				findSurface));

			AddRequired(landmarks, occupied, FindSurfaceSite(
				ApogeanLandmarkKind.PrewarTransitRuin,
				random.Next(90, 141),
				random.Next(45, 71),
				18,
				spawnX,
				2600,
				26,
				96,
				occupied,
				random,
				findSurface));

			AddRequired(landmarks, occupied, FindMawResearchSite(
				majorRupture,
				random.Next(70, 111),
				random.Next(55, 91),
				20,
				occupied,
				random,
				findSurface));

			return landmarks;
		}

		private static List<ApogeanLandmarkPlan> FindSurfaceCampusCandidates(
			ApogeanLandmarkKind kind,
			int fullWidth,
			int fullHeight,
			int compactWidth,
			int compactHeight,
			int padding,
			int preferredX,
			int searchRadius,
			int aboveSurface,
			int maxSurfaceRelief,
			IReadOnlyList<Rectangle> occupied,
			UnifiedRandom random,
			Func<int, int> findSurface)
		{
			List<ApogeanLandmarkPlan> candidates = FindSurfaceSites(
				kind,
				fullWidth,
				fullHeight,
				padding,
				preferredX,
				searchRadius,
				aboveSurface,
				maxSurfaceRelief,
				occupied,
				random,
				findSurface,
				96);
			candidates.AddRange(FindSurfaceSites(
				kind,
				compactWidth,
				compactHeight,
				padding,
				preferredX,
				searchRadius,
				aboveSurface,
				maxSurfaceRelief,
				occupied,
				random,
				findSurface,
				96));
			return candidates;
		}

		private static bool TryChooseSurfaceCampusPair(
			IReadOnlyList<ApogeanLandmarkPlan> kesslerCandidates,
			IReadOnlyList<ApogeanLandmarkPlan> helixCandidates,
			out ApogeanLandmarkPlan kessler,
			out ApogeanLandmarkPlan helix)
		{
			int bestScore = int.MaxValue;
			kessler = null;
			helix = null;
			for (int k = 0; k < kesslerCandidates.Count; k++)
			{
				for (int h = 0; h < helixCandidates.Count; h++)
				{
					ApogeanLandmarkPlan kesslerCandidate = kesslerCandidates[k];
					ApogeanLandmarkPlan helixCandidate = helixCandidates[h];
					if (kesslerCandidate.ReservedBounds.Intersects(helixCandidate.ReservedBounds))
						continue;

					int compactPenalty = (kesslerCandidate.Bounds.Width < 260 ? 10000 : 0) +
						(helixCandidate.Bounds.Width < 240 ? 10000 : 0);
					int score = compactPenalty + k + h;
					if (score >= bestScore)
						continue;
					bestScore = score;
					kessler = kesslerCandidate;
					helix = helixCandidate;
				}
			}

			return kessler is not null && helix is not null;
		}

		private static ApogeanLandmarkPlan FindSkyCampusSite(
			ApogeanLandmarkKind kind,
			int fullWidth,
			int fullHeight,
			int compactWidth,
			int compactHeight,
			int padding,
			IReadOnlyList<Rectangle> occupied,
			UnifiedRandom random)
		{
			return FindSkySite(kind, fullWidth, fullHeight, padding, occupied, random) ??
				FindSkySite(kind, compactWidth, compactHeight, padding, occupied, random);
		}

		private static void AddRequired(
			List<ApogeanLandmarkPlan> landmarks,
			List<Rectangle> occupied,
			ApogeanLandmarkPlan landmark)
		{
			if (landmark is null)
				throw new InvalidOperationException($"Apogee could not fit every required day-one landmark on this seed: {lastFailureReason}.");
			if (IntersectsAny(landmark.ReservedBounds, occupied))
				throw new InvalidOperationException($"Apogee atlas collision while committing {landmark.Kind}.");

			landmarks.Add(landmark);
			occupied.Add(landmark.ReservedBounds);
		}

		private static ApogeanLandmarkPlan FindSurfaceSite(
			ApogeanLandmarkKind kind,
			int width,
			int height,
			int padding,
			int preferredX,
			int searchRadius,
			int aboveSurface,
			int maxSurfaceRelief,
			IReadOnlyList<Rectangle> occupied,
			UnifiedRandom random,
			Func<int, int> findSurface)
		{
			List<ApogeanLandmarkPlan> candidates = FindSurfaceSites(
				kind,
				width,
				height,
				padding,
				preferredX,
				searchRadius,
				aboveSurface,
				maxSurfaceRelief,
				occupied,
				random,
				findSurface,
				1);
			return candidates.Count > 0 ? candidates[0] : null;
		}

		private static List<ApogeanLandmarkPlan> FindSurfaceSites(
			ApogeanLandmarkKind kind,
			int width,
			int height,
			int padding,
			int preferredX,
			int searchRadius,
			int aboveSurface,
			int maxSurfaceRelief,
			IReadOnlyList<Rectangle> occupied,
			UnifiedRandom random,
			Func<int, int> findSurface,
			int maxCandidates)
		{
			List<ApogeanLandmarkPlan> candidates = new();
			int occupiedRejections = 0;
			int terrainRejections = 0;
			int structureRejections = 0;
			int emptyRejections = 0;
			int reliefRejections = 0;
			int evilRejections = 0;
			int jungleRejections = 0;
			int desertRejections = 0;
			int edgePadding = 520 + width / 2;
			// A faction has a preferred region, not an absolute side lock. Large content mods can
			// reserve most of that side, so the deterministic fallback must inspect the complete
			// buildable width before deciding that a required day-one campus cannot fit.
			int maximumRadius = Math.Max(searchRadius, Main.maxTilesX);
			bool positiveFirst = random.NextBool();
			HashSet<int> inspected = new();
			for (int distance = 0; distance <= maximumRadius; distance += 20)
			{
				for (int signIndex = 0; signIndex < (distance == 0 ? 1 : 2); signIndex++)
				{
					int sign = signIndex == 0 == positiveFirst ? 1 : -1;
					int centerX = Utils.Clamp(preferredX + sign * distance, edgePadding, Main.maxTilesX - edgePadding);
					if (!inspected.Add(centerX))
						continue;
					int centerY = FindSurfaceBaseline(centerX, width, findSurface);
					Rectangle bounds = new(centerX - width / 2, centerY - aboveSurface, width, height);
					if (IntersectsReserved(bounds, padding, occupied))
					{
						occupiedRejections++;
						continue;
					}
					if (!IsSurfaceWastesCandidate(bounds, findSurface, maxSurfaceRelief, out SurfaceRejection rejection))
					{
						terrainRejections++;
						switch (rejection)
						{
							case SurfaceRejection.Empty: emptyRejections++; break;
							case SurfaceRejection.Relief: reliefRejections++; break;
							case SurfaceRejection.Evil: evilRejections++; break;
							case SurfaceRejection.Jungle: jungleRejections++; break;
							case SurfaceRejection.Desert: desertRejections++; break;
						}
						continue;
					}
					if (!CanReserve(bounds, padding))
					{
						structureRejections++;
						continue;
					}
					candidates.Add(new ApogeanLandmarkPlan(kind, bounds, padding));
					if (candidates.Count >= maxCandidates)
						return candidates;
				}
			}

			if (candidates.Count == 0)
			{
				lastFailureReason = $"{kind} inspected {inspected.Count} surface columns " +
					$"(occupied={occupiedRejections}, terrain={terrainRejections} [empty={emptyRejections}, " +
					$"relief={reliefRejections}, evil={evilRejections}, jungle={jungleRejections}, desert={desertRejections}], " +
					$"protected-structure={structureRejections})";
			}
			return candidates;
		}

		private static ApogeanLandmarkPlan FindSkySite(
			ApogeanLandmarkKind kind,
			int width,
			int height,
			int padding,
			IReadOnlyList<Rectangle> occupied,
			UnifiedRandom random)
		{
			int topMin = 70;
			int topMax = Math.Max(topMin + 1, (int)Main.worldSurface - height - 32);
			for (int attempt = 0; attempt < 480; attempt++)
			{
				int x = random.Next(520, Main.maxTilesX - 520 - width);
				int y = random.Next(topMin, topMax);
				Rectangle bounds = new(x, y, width, height);
				if (HorizontalDistanceFromSpawn(bounds) < 540 || IntersectsReserved(bounds, padding, occupied) || !CanReserve(bounds, padding))
					continue;
				return new ApogeanLandmarkPlan(kind, bounds, padding);
			}

			return null;
		}

		private static ApogeanLandmarkPlan FindUndergroundSite(
			ApogeanLandmarkKind kind,
			int width,
			int height,
			int padding,
			IReadOnlyList<Rectangle> occupied,
			UnifiedRandom random)
		{
			int minY = (int)Main.worldSurface + 90;
			int maxY = Math.Min((int)Main.UnderworldLayer - height - 100, (int)Main.rockLayer + 420);
			for (int attempt = 0; attempt < 520; attempt++)
			{
				Rectangle bounds = new(
					random.Next(420, Main.maxTilesX - 420 - width),
					random.Next(minY, maxY),
					width,
					height);
				if (IntersectsReserved(bounds, padding, occupied) || !CanReserve(bounds, padding))
					continue;
				return new ApogeanLandmarkPlan(kind, bounds, padding);
			}

			return null;
		}

		private static ApogeanLandmarkPlan FindMawResearchSite(
			MawRupturePlan rupture,
			int width,
			int height,
			int padding,
			IReadOnlyList<Rectangle> occupied,
			UnifiedRandom random,
			Func<int, int> findSurface)
		{
			for (int attempt = 0; attempt < 360; attempt++)
			{
				int side = attempt % 2 == 0 ? 1 : -1;
				int centerX = rupture.SurfaceCenter.X + side * random.Next(300, 581);
				centerX = Utils.Clamp(centerX, 520 + width / 2, Main.maxTilesX - 520 - width / 2);
				int surface = findSurface(centerX);
				Rectangle bounds = new(centerX - width / 2, surface + random.Next(35, 111), width, height);
				if (IntersectsReserved(bounds, padding, occupied) || !CanReserve(bounds, padding))
					continue;
				return new ApogeanLandmarkPlan(ApogeanLandmarkKind.MawResearchSite, bounds, padding);
			}

			return null;
		}

		internal static bool CanReserve(Rectangle bounds, int padding)
		{
			if (bounds.Left < 12 || bounds.Top < 12 || bounds.Right >= Main.maxTilesX - 12 || bounds.Bottom >= Main.maxTilesY - 12)
				return false;
			allowedReplacementTiles ??= BuildAllowedReplacementTiles();
			return GenVars.structures.CanPlace(bounds, allowedReplacementTiles, padding);
		}

		private static bool IsSurfaceWastesCandidate(
			Rectangle bounds,
			Func<int, int> findSurface,
			int maxSurfaceRelief,
			out SurfaceRejection rejection)
		{
			int minSurface = int.MaxValue;
			int maxSurface = int.MinValue;
			int evil = 0;
			int jungle = 0;
			int desert = 0;
			int samples = 0;
			for (int x = bounds.Left + 8; x < bounds.Right - 8; x += 12)
			{
				int surface = findSurface(x);
				minSurface = Math.Min(minSurface, surface);
				maxSurface = Math.Max(maxSurface, surface);
				for (int y = Math.Max(20, surface - 5); y <= Math.Min(Main.maxTilesY - 20, surface + 20); y += 4)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (!tile.HasTile)
						continue;
					samples++;
					if (tile.TileType is TileID.Ebonstone or TileID.Crimstone or TileID.CorruptGrass or TileID.CrimsonGrass or TileID.Ebonsand or TileID.Crimsand)
						evil++;
					if (tile.TileType is TileID.JungleGrass or TileID.Mud)
						jungle++;
					if (tile.TileType is TileID.Sand or TileID.HardenedSand or TileID.Sandstone)
						desert++;
				}
			}

			if (samples == 0)
				rejection = SurfaceRejection.Empty;
			else if (maxSurface - minSurface > maxSurfaceRelief)
				rejection = SurfaceRejection.Relief;
			else if (evil * 20 >= samples)
				rejection = SurfaceRejection.Evil;
			else if (jungle * 8 >= samples)
				rejection = SurfaceRejection.Jungle;
			else if (desert * 6 >= samples)
				rejection = SurfaceRejection.Desert;
			else
				rejection = SurfaceRejection.None;

			return rejection == SurfaceRejection.None;
		}

		private static int FindSurfaceBaseline(int centerX, int width, Func<int, int> findSurface)
		{
			List<int> heights = new();
			for (int x = centerX - width / 2 + 8; x < centerX + width / 2 - 8; x += 12)
				heights.Add(findSurface(x));
			if (heights.Count == 0)
				return findSurface(centerX);

			heights.Sort();
			return heights[heights.Count / 2];
		}

		private static int HorizontalDistanceFromSpawn(Rectangle bounds)
		{
			int spawnX = Main.spawnTileX > 0 ? Main.spawnTileX : Main.maxTilesX / 2;
			return Math.Abs(bounds.Center.X - spawnX);
		}

		private static bool IntersectsAny(Rectangle candidate, IReadOnlyList<Rectangle> occupied)
		{
			for (int i = 0; i < occupied.Count; i++)
				if (candidate.Intersects(occupied[i]))
					return true;
			return false;
		}

		private static bool IntersectsReserved(Rectangle bounds, int padding, IReadOnlyList<Rectangle> occupied)
		{
			Rectangle reserved = bounds;
			reserved.Inflate(padding, padding);
			return IntersectsAny(reserved, occupied);
		}

		private static bool[] BuildAllowedReplacementTiles()
		{
			bool[] allowed = (bool[])TileID.Sets.GeneralPlacementTiles.Clone();
			Array.Resize(ref allowed, TileLoader.TileCount);
			for (int type = 0; type < allowed.Length; type++)
				allowed[type] |= MawTerrainRules.CanReplaceTileType((ushort)type);
			return allowed;
		}

		internal static bool CanReplaceForLandmark(ushort type)
		{
			allowedReplacementTiles ??= BuildAllowedReplacementTiles();
			return type < allowedReplacementTiles.Length && allowedReplacementTiles[type];
		}
	}
}
