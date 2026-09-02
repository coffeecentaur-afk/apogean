using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using apogean.Common.WorldGeneration;
using apogean.Content.Tiles;

namespace apogean.Content.Structures
{
	/// <summary>
	/// Places authored first-pass silhouettes for every guaranteed atlas site. Curated containers
	/// and progression loot remain a later content pass, but no ruin is allowed to read as an
	/// unexplained generic rectangle.
	/// </summary>
	public sealed class RuinGen : ModSystem
	{
		internal void GenerateWorld(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "Unearthing the abandoned world...";
			ApogeanWorldPlan plan = ApogeanWorldPlanSystem.Instance.Plan;
			if (plan is null)
				return;

			for (int i = 0; i < plan.Landmarks.Count; i++)
			{
				ApogeanLandmarkPlan landmark = plan.Landmarks[i];
				if (landmark.IsCampus)
					continue;
				PlaceRuin(landmark, new UnifiedRandom(unchecked(plan.PlanSeed ^ (int)landmark.Kind * 486187739)));
			}
		}

		private static void PlaceRuin(ApogeanLandmarkPlan landmark, UnifiedRandom random)
		{
			switch (landmark.Kind)
			{
				case ApogeanLandmarkKind.AbandonedKesslerOutpost:
					PlaceKesslerOutpost(landmark.Bounds, random);
					break;
				case ApogeanLandmarkKind.AbandonedHelixLaboratory:
					PlaceHelixLaboratory(landmark.Bounds, random);
					break;
				case ApogeanLandmarkKind.CrashedSentrixRelay:
					PlaceSentrixRelay(landmark.Bounds, random);
					break;
				case ApogeanLandmarkKind.PrewarTransitRuin:
					PlacePrewarTransit(landmark.Bounds, random);
					break;
				case ApogeanLandmarkKind.MawResearchSite:
					PlaceMawResearchSite(landmark.Bounds, random);
					break;
			}
		}

		private static void PlaceKesslerOutpost(Rectangle bounds, UnifiedRandom random)
		{
			int tileType = ModContent.TileType<KesslerRuinBlock>();
			Rectangle work = new(bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);
			ClearNaturalInterior(work);
			// Surface landmarks use the same above-surface offset as their atlas reservation. Keeping
			// the floor on that datum prevents a bunker-shaped ruin from appearing mysteriously buried.
			int floorY = bounds.Top + 22;
			int roofY = Math.Max(work.Top + 4, floorY - 20);
			PlaceHorizontalRun(work.Left + 2, work.Right - 3, floorY, tileType, 2);
			PlaceVerticalRun(work.Left + 3, roofY, floorY, tileType, 2);
			PlaceVerticalRun(work.Right - 5, roofY + 3, floorY, tileType, 2);
			PlaceBrokenRun(work.Left + 3, work.Right - 5, roofY, tileType, 2, random, 0.22f);

			Rectangle guardPost = new(work.Left + 7, roofY - 8, 12, 10);
			PlaceBrokenOutline(guardPost, tileType, random, 0.16f);
			PlaceVerticalRun(guardPost.Center.X, guardPost.Top - 7, guardPost.Top, tileType, 1);
			PlaceFixture3x4(work.Right - 14, floorY - 4, ModContent.TileType<KesslerPowerArmorRack>());
		}

		private static void PlaceHelixLaboratory(Rectangle bounds, UnifiedRandom random)
		{
			int tileType = ModContent.TileType<HelixRuinBlock>();
			Rectangle work = new(bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);
			ClearNaturalInterior(work);
			int split = work.Center.X;
			Rectangle upper = new(work.Left + 2, work.Top + 2, Math.Max(22, split - work.Left + 4), Math.Max(18, work.Height / 2));
			Rectangle lower = new(split - 8, work.Center.Y - 3, Math.Max(24, work.Right - split + 5), Math.Max(20, work.Bottom - work.Center.Y + 1));
			PlaceBrokenOutline(upper, tileType, random, 0.19f);
			PlaceBrokenOutline(lower, tileType, random, 0.12f);
			PlaceHorizontalRun(upper.Right - 6, lower.Left + 7, work.Center.Y, tileType, 2);
			PlaceFixture3x4(upper.Left + 7, upper.Bottom - 6, ModContent.TileType<HelixSymbioteTank>());
			PlaceFixture3x4(lower.Right - 12, lower.Bottom - 6, ModContent.TileType<HelixSymbioteTank>());
		}

		private static void PlaceSentrixRelay(Rectangle bounds, UnifiedRandom random)
		{
			int tileType = ModContent.TileType<SentrixRuinBlock>();
			Rectangle work = new(bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);
			ClearNaturalInterior(work);
			int leftX = work.Left + 3;
			int rightX = work.Right - 4;
			int surfaceY = bounds.Top + 24;
			int leftY = surfaceY - 2;
			int rightY = work.Top + 8;
			PlaceThickLine(leftX, leftY, rightX, rightY, tileType, 2);
			PlaceThickLine(leftX + 4, leftY, rightX, rightY + 6, tileType, 1);

			Rectangle relayPod = new(work.Center.X - 9, work.Center.Y - 5, 18, 13);
			PlaceBrokenOutline(relayPod, tileType, random, 0.28f);
			int padY = surfaceY;
			PlaceHorizontalRun(work.Center.X - 14, work.Center.X + 14, padY, tileType, 2);
			PlaceFixture3x4(work.Center.X - 1, padY - 4, ModContent.TileType<SentrixHologramCore>());
		}

		private static void PlacePrewarTransit(Rectangle bounds, UnifiedRandom random)
		{
			int tileType = ModContent.TileType<PrewarConcrete>();
			Rectangle work = new(bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);
			ClearNaturalInterior(work);
			int platformY = bounds.Top + 26;
			PlaceBrokenRun(work.Left + 2, work.Right - 3, platformY, tileType, 3, random, 0.08f);
			int canopyY = Math.Max(work.Top + 5, platformY - 18);
			for (int x = work.Left + 9; x < work.Right - 7; x += 22)
				PlaceVerticalRun(x, canopyY, platformY, tileType, 2);
			PlaceBrokenRun(work.Left + 7, work.Right - 7, canopyY, tileType, 2, random, 0.30f);
			PlaceHorizontalRun(work.Left + 2, work.Left + 16, platformY - 8, tileType, 2);
			PlaceHorizontalRun(work.Right - 17, work.Right - 3, platformY - 8, tileType, 2);
		}

		private static void PlaceMawResearchSite(Rectangle bounds, UnifiedRandom random)
		{
			int tileType = ModContent.TileType<MawResearchBlock>();
			Rectangle work = new(bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);
			ClearNaturalInterior(work);
			int floorY = work.Bottom - 5;
			Rectangle bunker = new(work.Left + 3, Math.Max(work.Top + 6, floorY - 24), work.Width - 6, 25);
			PlaceBrokenOutline(bunker, tileType, random, 0.16f);
			PlaceHorizontalRun(bunker.Left + 2, bunker.Right - 3, bunker.Top + 9, tileType, 2);
			// A broad missing strip reads as the shattered observation window aimed at the Maw.
			for (int x = bunker.Center.X - 8; x <= bunker.Center.X + 8; x++)
				for (int y = bunker.Top + 1; y <= bunker.Top + 5; y++)
					Framing.GetTileSafely(x, y).ClearTile();
			PlaceVerticalRun(bunker.Left + 8, bunker.Top - 9, bunker.Top, tileType, 1);
			PlaceVerticalRun(bunker.Left + 12, bunker.Top - 5, bunker.Top, tileType, 1);
		}

		private static void PlaceBrokenOutline(Rectangle room, int tileType, UnifiedRandom random, float roofCollapse)
		{
			for (int x = room.Left; x < room.Right; x++)
			{
				for (int y = room.Top; y < room.Bottom; y++)
				{
					bool edge = x <= room.Left + 1 || x >= room.Right - 2 || y <= room.Top + 1 || y >= room.Bottom - 2;
					if (!edge)
						continue;
					float collapse = y <= room.Top + 1 ? roofCollapse : roofCollapse * 0.28f;
					if (random.NextFloat() < collapse)
						continue;
					SetSolidTile(x, y, tileType);
				}
			}
		}

		private static void PlaceBrokenRun(int startX, int endX, int y, int tileType, int thickness, UnifiedRandom random, float gapChance)
		{
			for (int x = startX; x <= endX; x++)
			{
				if (random.NextFloat() < gapChance)
					continue;
				for (int row = 0; row < thickness; row++)
					SetSolidTile(x, y + row, tileType);
			}
		}

		private static void PlaceHorizontalRun(int startX, int endX, int y, int tileType, int thickness)
		{
			for (int x = startX; x <= endX; x++)
				for (int row = 0; row < thickness; row++)
					SetSolidTile(x, y + row, tileType);
		}

		private static void PlaceVerticalRun(int x, int startY, int endY, int tileType, int thickness)
		{
			for (int y = startY; y <= endY; y++)
				for (int column = 0; column < thickness; column++)
					SetSolidTile(x + column, y, tileType);
		}

		private static void PlaceThickLine(int startX, int startY, int endX, int endY, int tileType, int thickness)
		{
			int steps = Math.Max(Math.Abs(endX - startX), Math.Abs(endY - startY));
			for (int step = 0; step <= steps; step++)
			{
				float amount = steps == 0 ? 0f : step / (float)steps;
				int x = (int)MathHelper.Lerp(startX, endX, amount);
				int y = (int)MathHelper.Lerp(startY, endY, amount);
				for (int offset = -thickness; offset <= thickness; offset++)
					SetSolidTile(x, y + offset, tileType);
			}
		}

		private static void PlaceFixture3x4(int topLeftX, int topLeftY, int tileType)
		{
			for (int column = 0; column < 3; column++)
			{
				for (int row = 0; row < 4; row++)
				{
					Tile tile = Framing.GetTileSafely(topLeftX + column, topLeftY + row);
					tile.HasTile = true;
					tile.TileType = (ushort)tileType;
					tile.TileFrameX = (short)(column * 18);
					tile.TileFrameY = (short)(row * 18);
					tile.Slope = SlopeType.Solid;
					tile.IsHalfBlock = false;
					tile.LiquidAmount = 0;
				}
			}
		}

		private static void ClearNaturalInterior(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				for (int y = bounds.Top; y < bounds.Bottom; y++)
				{
					if (!WorldGen.InWorld(x, y, 10))
						continue;
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile && !WorldAtlasPlanner.CanReplaceForLandmark(tile.TileType))
						continue;
					tile.ClearTile();
					tile.LiquidAmount = 0;
				}
			}
		}

		private static void SetSolidTile(int x, int y, int tileType)
		{
			if (!WorldGen.InWorld(x, y, 10))
				return;
			Tile tile = Framing.GetTileSafely(x, y);
			tile.HasTile = true;
			tile.TileType = (ushort)tileType;
			tile.Slope = SlopeType.Solid;
			tile.IsHalfBlock = false;
			tile.LiquidAmount = 0;
		}
	}
}
