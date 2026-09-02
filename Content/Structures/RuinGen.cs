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

namespace apogean.Content.Structures
{
	/// <summary>
	/// Places readable blockout ruins for every guaranteed atlas site. The modules intentionally
	/// contain no progression loot yet; room art and curated containers are a later content pass.
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
			Rectangle bounds = landmark.Bounds;
			int tileType = GetRuinTile(landmark.Kind);
			Rectangle room = new(bounds.Left + 5, bounds.Top + 5, bounds.Width - 10, bounds.Height - 10);
			ClearNaturalInterior(room);

			for (int x = room.Left; x < room.Right; x++)
			{
				for (int y = room.Top; y < room.Bottom; y++)
				{
					bool edge = x <= room.Left + 1 || x >= room.Right - 2 || y <= room.Top + 1 || y >= room.Bottom - 2;
					if (!edge)
						continue;
					float collapse = y < room.Top + 3 ? 0.28f : 0.11f;
					if (random.NextFloat() < collapse)
						continue;
					SetSolidTile(x, y, tileType);
				}
			}

			int floorCount = Math.Max(1, room.Height / 24);
			for (int floor = 1; floor < floorCount; floor++)
			{
				int y = room.Top + floor * room.Height / floorCount;
				int breachCenter = random.Next(room.Left + 8, room.Right - 8);
				for (int x = room.Left + 2; x < room.Right - 2; x++)
				{
					if (Math.Abs(x - breachCenter) <= random.Next(2, 5))
						continue;
					SetSolidTile(x, y, tileType);
				}
			}
		}

		private static int GetRuinTile(ApogeanLandmarkKind kind) => kind switch
		{
			ApogeanLandmarkKind.AbandonedKesslerOutpost => TileID.GrayBrick,
			ApogeanLandmarkKind.AbandonedHelixLaboratory => TileID.MarbleBlock,
			ApogeanLandmarkKind.CrashedSentrixRelay => TileID.MeteoriteBrick,
			ApogeanLandmarkKind.PrewarTransitRuin => TileID.StoneSlab,
			ApogeanLandmarkKind.MawResearchSite => TileID.Mudstone,
			_ => TileID.StoneSlab
		};

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
