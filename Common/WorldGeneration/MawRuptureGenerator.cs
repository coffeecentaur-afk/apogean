using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using apogean.Content.Tiles;
using apogean.Content.Walls;

namespace apogean.Common.WorldGeneration
{
	/// <summary>
	/// Generates Maw morphology from a saved rupture plan. The plan owns placement; this module
	/// owns terrain grammar and may be replaced without changing callers or save data.
	/// </summary>
	internal static class MawRuptureGenerator
	{
		public static void Generate(MawRupturePlan rupture, int seed, WorldEditIntent intent)
		{
			UnifiedRandom random = new(seed);
			if (rupture.IsMajor)
				GenerateFeedingWound(rupture, random, seed, intent);
			else
				GenerateOutgrowth(rupture, random, intent);
		}

		private static void GenerateFeedingWound(
			MawRupturePlan rupture,
			UnifiedRandom random,
			int seed,
			WorldEditIntent intent)
		{
			if (rupture.HasNavigationSpine)
			{
				GeneratePlannedFeedingWound(rupture, random, seed, intent);
				return;
			}

			GenerateOutgrowth(rupture, random, intent);

			int wallType = ModContent.WallType<MawWallUnsafe>();
			int startY = rupture.SurfaceCenter.Y - 5;
			int endY = Main.maxTilesY - 185;
			float currentX = rupture.SurfaceCenter.X;
			float horizontalVelocity = random.NextFloat(-0.8f, 0.8f);
			int chamberCountdown = random.Next(80, 121);
			int chamberSide = random.NextBool() ? 1 : -1;

			CarveEllipse(rupture.SurfaceCenter.X, rupture.SurfaceCenter.Y - 2, 34, 17, wallType, seed, intent, boneShell: true, openToSky: true);

			for (int y = startY; y <= endY; y += 7)
			{
				horizontalVelocity = MathHelper.Clamp(horizontalVelocity * 0.82f + random.NextFloat(-0.58f, 0.58f), -2.15f, 2.15f);
				currentX += horizontalVelocity;
				float offset = currentX - rupture.SurfaceCenter.X;
				float leash = rupture.RadiusX * 0.78f;
				if (Math.Abs(offset) > leash)
					horizontalVelocity -= Math.Sign(offset) * 0.9f;

				float depth = Utils.GetLerpValue(startY, endY, y, clamped: true);
				int radiusX = 10 + (int)(depth * 5f) + random.Next(-1, 3);
				int radiusY = 9 + random.Next(-1, 2);
				bool boneBand = ((y - startY) / 7) % 9 is 0 or 1;
				CarveEllipse((int)currentX, y, radiusX, radiusY, wallType, seed, intent, boneBand, openToSky: false);

				chamberCountdown -= 7;
				if (chamberCountdown <= 0 && y < endY - 90)
				{
					int chamberX = (int)currentX + chamberSide * random.Next(31, 52);
					int chamberY = y + random.Next(-8, 9);
					int chamberRadiusX = random.Next(24, 36);
					int chamberRadiusY = random.Next(14, 22);
					CarveTunnel((int)currentX, y, chamberX, chamberY, 6, wallType, seed, intent);
					CarveEllipse(chamberX, chamberY, chamberRadiusX, chamberRadiusY, wallType, seed ^ chamberY, intent, boneShell: true, openToSky: false);
					chamberSide *= -1;
					chamberCountdown = random.Next(92, 151);
				}
			}

			GenerateBurningRoot((int)currentX, endY, random, wallType, seed, intent);
		}

		private static void GeneratePlannedFeedingWound(
			MawRupturePlan rupture,
			UnifiedRandom random,
			int seed,
			WorldEditIntent intent)
		{
			GenerateOutgrowth(rupture, random, intent);
			int wallType = ModContent.WallType<MawWallUnsafe>();
			CarveEllipse(
				rupture.SurfaceCenter.X,
				rupture.SurfaceCenter.Y - 1,
				44,
				22,
				wallType,
				seed,
				intent,
				boneShell: true,
				openToSky: true);

			int chamberCountdown = random.Next(84, 126);
			int chamberSide = random.NextBool() ? 1 : -1;
			for (int i = 0; i < rupture.NavigationSpine.Count; i++)
			{
				Point16 point = rupture.NavigationSpine[i];
				if (i > 0)
				{
					Point16 previous = rupture.NavigationSpine[i - 1];
					CarveTunnel(previous.X, previous.Y, point.X, point.Y, 8, wallType, seed ^ (i * 104729), intent);
				}
				float depth = i / (float)Math.Max(1, rupture.NavigationSpine.Count - 1);
				int radiusX = 11 + (int)(depth * 5f) + random.Next(-1, 2);
				int radiusY = 9 + random.Next(0, 3);
				bool boneBand = i % 11 is 0 or 1;
				CarveEllipse(point.X, point.Y, radiusX, radiusY, wallType, seed ^ (i * 7919), intent, boneBand, openToSky: false);

				chamberCountdown -= i == 0 ? 0 : point.Y - rupture.NavigationSpine[i - 1].Y;
				if (chamberCountdown > 0 || depth > 0.86f)
					continue;

				int chamberX = point.X + chamberSide * random.Next(34, 51);
				int chamberY = point.Y + random.Next(-7, 8);
				int chamberRadiusX = random.Next(25, 38);
				int chamberRadiusY = random.Next(15, 23);
				Rectangle chamberBounds = new(
					chamberX - chamberRadiusX - 4,
					chamberY - chamberRadiusY - 4,
					(chamberRadiusX + 4) * 2 + 1,
					(chamberRadiusY + 4) * 2 + 1);
				if (!ContainsHardObstacle(chamberBounds))
				{
					CarveTunnel(point.X, point.Y, chamberX, chamberY, 6, wallType, seed ^ chamberX, intent);
					CarveEllipse(chamberX, chamberY, chamberRadiusX, chamberRadiusY, wallType, seed ^ chamberY, intent, boneShell: true, openToSky: false);
				}

				chamberSide *= -1;
				chamberCountdown = random.Next(96, 145);
			}

			Point16 root = rupture.MatriarchCenter.X > 0
				? rupture.MatriarchCenter
				: rupture.NavigationSpine[rupture.NavigationSpine.Count - 1];
			GenerateBurningRoot(root.X, root.Y, random, wallType, seed, intent);
			ClearNavigationSpine(rupture, wallType, intent);
		}

		private static void GenerateOutgrowth(MawRupturePlan rupture, UnifiedRandom random, WorldEditIntent intent)
		{
			int turf = ModContent.TileType<EngraftTurf>();
			Point16 center = rupture.SurfaceCenter;
			for (int x = center.X - rupture.RadiusX; x <= center.X + rupture.RadiusX; x++)
			{
				for (int y = center.Y - rupture.RadiusY; y <= center.Y + rupture.RadiusY; y++)
				{
					if (!WorldGen.InWorld(x, y, 12) || !MawTerrainRules.CanConvert(x, y, intent))
						continue;

					float dx = (x - center.X) / (float)rupture.RadiusX;
					float dy = (y - center.Y) / (float)rupture.RadiusY;
					if (dx * dx + dy * dy > 1f + random.NextFloat(-0.14f, 0.12f))
						continue;

					SetSolidTile(x, y, turf);
				}
			}

			MutateSurfaceGrowth(rupture, random, intent);
		}

		private static void MutateSurfaceGrowth(MawRupturePlan rupture, UnifiedRandom random, WorldEditIntent intent)
		{
			int turf = ModContent.TileType<EngraftTurf>();
			int tuft = ModContent.TileType<EngraftTuft>();
			Rectangle bounds = rupture.GenerationBounds;
			for (int x = Math.Max(12, bounds.Left); x < Math.Min(Main.maxTilesX - 12, bounds.Right); x++)
			{
				for (int y = Math.Max(12, bounds.Top); y < Math.Min(Main.maxTilesY - 12, bounds.Bottom); y++)
				{
					if (!ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, intent))
						continue;
					Tile ground = Framing.GetTileSafely(x, y);
					if (!ground.HasTile || ground.TileType != turf)
						continue;

					Tile above = Framing.GetTileSafely(x, y - 1);
					if (above.HasTile && above.TileType is TileID.Plants or TileID.Plants2 or TileID.Vines or TileID.Saplings or TileID.Trees)
					{
						WorldGen.KillTile(x, y - 1, noItem: true);
						above = Framing.GetTileSafely(x, y - 1);
					}

					if (!above.HasTile && ground.Slope == SlopeType.Solid && !ground.IsHalfBlock && random.NextBool(7))
						WorldGen.PlaceTile(x, y - 1, tuft, mute: true, forced: true);
				}
			}
		}

		private static void GenerateBurningRoot(
			int centerX,
			int centerY,
			UnifiedRandom random,
			int wallType,
			int seed,
			WorldEditIntent intent)
		{
			// Keep the arena envelope consistent with the planner's protected reservation.
			// Edge noise and the three roots still make each seed's silhouette distinct.
			CarveEllipse(centerX, centerY, 94, 48, wallType, seed ^ 0x51A7, intent, boneShell: true, openToSky: false);
			for (int branch = -1; branch <= 1; branch++)
			{
				int targetX = centerX + branch * random.Next(45, 76);
				int targetY = Math.Min(centerY + random.Next(25, 51), Main.maxTilesY - 22);
				CarveTunnel(centerX, centerY + 8, targetX, targetY, random.Next(5, 8), wallType, seed ^ targetX, intent);
			}
		}

		private static void ClearNavigationSpine(MawRupturePlan rupture, int wallType, WorldEditIntent intent)
		{
			// Re-open every connector after side chambers and shell decoration have finished. Clearing
			// only the sampled waypoints can leave a one-tile plug between two otherwise open points.
			for (int pointIndex = 1; pointIndex < rupture.NavigationSpine.Count; pointIndex++)
			{
				Point16 previous = rupture.NavigationSpine[pointIndex - 1];
				Point16 point = rupture.NavigationSpine[pointIndex];
				CarveTunnel(previous.X, previous.Y, point.X, point.Y, 8, wallType, pointIndex * 104729, intent);
			}

			for (int pointIndex = 0; pointIndex < rupture.NavigationSpine.Count; pointIndex++)
			{
				Point16 point = rupture.NavigationSpine[pointIndex];
				for (int x = point.X - 2; x <= point.X + 2; x++)
				{
					for (int y = point.Y - 4; y <= point.Y + 4; y++)
					{
						if (!MawTerrainRules.CanCarve(x, y, intent))
							continue;
						Tile tile = Framing.GetTileSafely(x, y);
						tile.ClearTile();
						tile.LiquidAmount = 0;
						tile.WallType = (ushort)wallType;
					}
				}
			}
		}

		private static bool ContainsHardObstacle(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x += 2)
			{
				for (int y = bounds.Top; y < bounds.Bottom; y += 2)
				{
					if (MawTerrainRules.IsHardGenerationObstacle(x, y))
						return true;
				}
			}

			return false;
		}

		private static void CarveTunnel(
			int startX,
			int startY,
			int endX,
			int endY,
			int radius,
			int wallType,
			int seed,
			WorldEditIntent intent)
		{
			int steps = Math.Max(Math.Abs(endX - startX), Math.Abs(endY - startY));
			steps = Math.Max(1, steps / 3);
			for (int step = 0; step <= steps; step++)
			{
				float amount = step / (float)steps;
				int x = (int)MathHelper.Lerp(startX, endX, amount);
				int y = (int)MathHelper.Lerp(startY, endY, amount);
				CarveEllipse(
					x,
					y,
					radius,
					radius - 1,
					wallType,
					seed + step,
					intent,
					boneShell: false,
					openToSky: false,
					placeShell: false);
			}
		}

		private static void CarveEllipse(
			int centerX,
			int centerY,
			int radiusX,
			int radiusY,
			int wallType,
			int seed,
			WorldEditIntent intent,
			bool boneShell,
			bool openToSky,
			bool placeShell = true)
		{
			int mawstone = ModContent.TileType<Mawstone>();
			int bone = ModContent.TileType<OssuaryBone>();
			int shellThickness = 3;
			for (int x = centerX - radiusX - shellThickness; x <= centerX + radiusX + shellThickness; x++)
			{
				for (int y = centerY - radiusY - shellThickness; y <= centerY + radiusY + shellThickness; y++)
				{
					if (!WorldGen.InWorld(x, y, 12))
						continue;

					float normalizedX = (x - centerX) / (float)Math.Max(1, radiusX);
					float normalizedY = (y - centerY) / (float)Math.Max(1, radiusY);
					float distance = normalizedX * normalizedX + normalizedY * normalizedY;
					float noise = EdgeNoise(x, y, seed) * 0.12f;
					if (distance <= 1f + noise)
					{
						if (!MawTerrainRules.CanCarve(x, y, intent))
							continue;
						Tile tile = Framing.GetTileSafely(x, y);
						tile.ClearTile();
						tile.LiquidAmount = 0;
						if (openToSky && y < Main.worldSurface + 3)
							tile.WallType = WallID.None;
						else
							tile.WallType = (ushort)wallType;
					}
					else if (placeShell && distance <= 1.34f + noise && MawTerrainRules.CanPlaceShell(x, y, intent))
					{
						bool useBone = boneShell && EdgeNoise(x, y, seed ^ 0x2C13) > 0.18f;
						SetSolidTile(x, y, useBone ? bone : mawstone);
					}
				}
			}
		}

		private static float EdgeNoise(int x, int y, int seed)
		{
			uint value = unchecked((uint)(x * 374761393 + y * 668265263 + seed * 69069));
			value = (value ^ (value >> 13)) * 1274126177u;
			value ^= value >> 16;
			return (value / (float)uint.MaxValue) * 2f - 1f;
		}

		private static void SetSolidTile(int x, int y, int tileType)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			tile.HasTile = true;
			tile.TileType = (ushort)tileType;
			tile.Slope = SlopeType.Solid;
			tile.IsHalfBlock = false;
			tile.LiquidAmount = 0;
		}
	}
}
