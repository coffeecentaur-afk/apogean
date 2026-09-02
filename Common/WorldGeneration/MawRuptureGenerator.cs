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

			GenerateStomachAndIntestinalDescent((int)currentX, endY, random, wallType, seed, intent);
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
			GenerateStomachAndIntestinalDescent(root.X, root.Y, random, wallType, seed, intent);
			ClearNavigationSpine(rupture, wallType, intent);
			PlaceGulletShelves(rupture, wallType, intent);
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

		private static void GenerateStomachAndIntestinalDescent(
			int centerX,
			int centerY,
			UnifiedRandom random,
			int wallType,
			int seed,
			WorldEditIntent intent)
		{
			// The broad Stomach remains a natural player-built arena. It sits above Hell so the
			// Gullet cannot erase a Wall of Flesh runway or become a free elevator into it.
			CarveEllipse(centerX, centerY, 94, 48, wallType, seed ^ 0x51A7, intent, boneShell: true, openToSky: false);
			for (int branch = -1; branch <= 1; branch++)
			{
				int targetX = centerX + branch * random.Next(45, 76);
				int targetY = Math.Min(centerY + random.Next(18, 35), (int)Main.UnderworldLayer - 14);
				CarveTunnel(centerX, centerY + 8, targetX, targetY, random.Next(5, 8), wallType, seed ^ targetX, intent);
			}

			GenerateIntestinalDescent(centerX, centerY + 43, random, wallType, seed ^ 0x7717, intent);
			SealStomachOutlet(centerX, centerY + 43);
		}

		private static void GenerateIntestinalDescent(
			int centerX,
			int startY,
			UnifiedRandom random,
			int wallType,
			int seed,
			WorldEditIntent intent)
		{
			int currentX = centerX;
			int previousY = startY;
			int endY = Main.maxTilesY - 18;
			for (int y = startY; y <= endY; y += 6)
			{
				if (!TryChooseIntestineStep(centerX, currentX, y, random, out int nextX))
					break;

				CarveTunnel(currentX, previousY, nextX, y, 7, wallType, seed ^ y, intent);
				bool boneBand = ((y - startY) / 6) % 10 == 0;
				CarveEllipse(nextX, y, 8, 7, wallType, seed ^ (y * 397), intent, boneBand, openToSky: false);
				currentX = nextX;
				previousY = y;
			}
		}

		private static bool TryChooseIntestineStep(
			int originX,
			int currentX,
			int y,
			UnifiedRandom random,
			out int chosenX)
		{
			int direction = random.NextBool() ? 1 : -1;
			int[] offsets = { 0, 4 * direction, -4 * direction, 8 * direction, -8 * direction, 12 * direction, -12 * direction };
			for (int i = 0; i < offsets.Length; i++)
			{
				int candidateX = currentX + offsets[i];
				if (Math.Abs(candidateX - originX) > 18)
					continue;
				Rectangle clearance = new(candidateX - 11, y - 8, 23, 17);
				if (ContainsHardObstacle(clearance))
					continue;
				chosenX = candidateX;
				return true;
			}

			chosenX = currentX;
			return false;
		}

		private static void SealStomachOutlet(int centerX, int outletY)
		{
			int mawstone = ModContent.TileType<Mawstone>();
			for (int x = centerX - 8; x <= centerX + 8; x++)
			{
				for (int y = outletY - 1; y <= outletY + 3; y++)
					SetSolidTile(x, y, mawstone);
			}
		}

		private static void PlaceGulletShelves(MawRupturePlan rupture, int wallType, WorldEditIntent intent)
		{
			if (!rupture.HasNavigationSpine)
				return;

			int nextShelfY = rupture.SurfaceCenter.Y + 32;
			int side = (rupture.SurfaceCenter.X & 1) == 0 ? -1 : 1;
			for (int i = 0; i < rupture.NavigationSpine.Count; i++)
			{
				Point16 point = rupture.NavigationSpine[i];
				if (point.Y < nextShelfY || point.Y >= rupture.MatriarchCenter.Y - 66)
					continue;

				float depth = i / (float)Math.Max(1, rupture.NavigationSpine.Count - 1);
				int halfWidth = 11 + (int)(depth * 5f);
				PlaceGulletShelf(point.X, point.Y + 4, halfWidth, side, wallType, intent);
				side *= -1;
				nextShelfY = point.Y + 28;
			}
		}

		private static void PlaceGulletShelf(
			int centerX,
			int y,
			int halfWidth,
			int side,
			int wallType,
			WorldEditIntent intent)
		{
			int bone = ModContent.TileType<OssuaryBone>();
			int startX = side < 0 ? centerX - halfWidth + 2 : centerX;
			int endX = side < 0 ? centerX : centerX + halfWidth - 2;
			for (int x = startX; x <= endX; x++)
			{
				// Alternating wall-grown shelves force lateral corrections and expose the bone
				// palette without creating a fragile full-width gate in a bending tunnel.
				for (int row = 0; row < 1; row++)
				{
					Tile tile = Framing.GetTileSafely(x, y + row);
					if (tile.HasTile || tile.WallType != wallType || !MawTerrainRules.CanPlaceShell(x, y + row, intent))
						continue;
					SetSolidTile(x, y + row, bone);
				}
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
