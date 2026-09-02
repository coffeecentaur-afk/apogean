using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using apogean.Common.WorldGeneration;
using apogean.Content.Factions;
using apogean.Content.Tiles;

namespace apogean.Content.Structures
{
	/// <summary>
	/// Builds the day-one Campus silhouettes inside locations owned by the saved world atlas.
	/// Production room modules and art can replace this shell language without changing placement,
	/// access, or save data.
	/// </summary>
	public sealed class CompoundGen : ModSystem
	{
		private readonly Dictionary<ApogeanFaction, Rectangle> compoundBounds = new();

		public override void OnWorldLoad() => compoundBounds.Clear();
		public override void OnWorldUnload() => compoundBounds.Clear();

		internal void GenerateWorld(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "Sealing the old corporate territories...";
			compoundBounds.Clear();
			ApogeanWorldPlan plan = ApogeanWorldPlanSystem.Instance.Plan;
			if (plan is null)
				return;

			for (int i = 0; i < plan.Landmarks.Count; i++)
			{
				ApogeanLandmarkPlan landmark = plan.Landmarks[i];
				if (!TryGetFaction(landmark.Kind, out ApogeanFaction faction))
					continue;

				compoundBounds[faction] = landmark.Bounds;
				PlaceCampus(landmark);
			}
		}

		private static void PlaceCampus(ApogeanLandmarkPlan landmark)
		{
			switch (landmark.Kind)
			{
				case ApogeanLandmarkKind.KesslerCampus:
					PlaceKesslerCampus(landmark.Bounds);
					break;
				case ApogeanLandmarkKind.HelixCampus:
					PlaceHelixCampus(landmark.Bounds);
					break;
				case ApogeanLandmarkKind.SentrixCampus:
					PlaceSentrixCampus(landmark.Bounds);
					break;
			}
		}

		private static void PlaceKesslerCampus(Rectangle bounds)
		{
			int bulkhead = ModContent.TileType<LockedBulkhead>();
			int surface = bounds.Top + 70;
			Rectangle bunker = new(bounds.Center.X - 55, surface - 31, 111, 36);
			ClearNaturalInterior(new Rectangle(bounds.Left + 7, surface - 46, bounds.Width - 14, 52));
			PlaceOutline(bunker, bulkhead, 2);

			for (int x = bounds.Left + 8; x < bounds.Right - 8; x++)
				for (int y = surface + 3; y <= surface + 5; y++)
					SetSolidTile(x, y, bulkhead);

			PlaceTower(new Rectangle(bounds.Left + 12, surface - 43, 18, 48), bulkhead);
			PlaceTower(new Rectangle(bounds.Right - 30, surface - 43, 18, 48), bulkhead);
			PlaceHorizontalRun(bounds.Left + 28, bounds.Center.X - 64, surface - 17, bulkhead, 2);
			PlaceHorizontalRun(bounds.Center.X + 64, bounds.Right - 28, surface - 17, bulkhead, 2);
			SetDoor(GetDoorBounds(ApogeanFaction.Kessler, bounds), sealedShut: true);
		}

		private static void PlaceHelixCampus(Rectangle bounds)
		{
			int bulkhead = ModContent.TileType<LockedBulkhead>();
			int surface = bounds.Top + 45;
			int centerX = bounds.Center.X;
			ClearNaturalInterior(new Rectangle(bounds.Left + 20, bounds.Top + 6, bounds.Width - 40, bounds.Height - 18));

			for (int x = centerX - 92; x <= centerX + 92; x++)
			{
				float normalized = (x - centerX) / 92f;
				int domeY = surface - (int)(MathF.Sqrt(MathF.Max(0f, 1f - normalized * normalized)) * 40f);
				SetSolidTile(x, domeY, bulkhead);
				SetSolidTile(x, domeY + 1, bulkhead);
			}

			Rectangle lab = new(bounds.Left + 25, surface + 4, bounds.Width - 50, bounds.Bottom - surface - 16);
			PlaceOutline(lab, bulkhead, 2);
			for (int y = lab.Top + 30; y < lab.Bottom - 10; y += 31)
				PlaceHorizontalRun(lab.Left + 2, lab.Right - 2, y, bulkhead, 2);
			SetDoor(GetDoorBounds(ApogeanFaction.Helix, bounds), sealedShut: true);
		}

		private static void PlaceSentrixCampus(Rectangle bounds)
		{
			int bulkhead = ModContent.TileType<LockedBulkhead>();
			int centerX = bounds.Center.X;
			Rectangle spire = new(centerX - 18, bounds.Top + 8, 37, bounds.Height - 16);
			PlaceOutline(spire, bulkhead, 2);
			for (int y = bounds.Top + 42, pad = 0; y < bounds.Bottom - 24; y += 42, pad++)
			{
				bool left = pad % 2 == 0;
				int startX = left ? centerX - 86 : centerX + 18;
				int endX = left ? centerX - 18 : centerX + 86;
				PlaceHorizontalRun(startX, endX, y, bulkhead, 3);
			}
			SetDoor(GetDoorBounds(ApogeanFaction.Sentrix, bounds), sealedShut: true);
		}

		private static void PlaceTower(Rectangle bounds, int tileType)
		{
			ClearNaturalInterior(bounds);
			PlaceOutline(bounds, tileType, 2);
		}

		private static void PlaceOutline(Rectangle bounds, int tileType, int thickness)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				for (int y = bounds.Top; y < bounds.Bottom; y++)
				{
					if (x < bounds.Left + thickness || x >= bounds.Right - thickness ||
						y < bounds.Top + thickness || y >= bounds.Bottom - thickness)
						SetSolidTile(x, y, tileType);
				}
			}
		}

		private static void PlaceHorizontalRun(int startX, int endX, int y, int tileType, int thickness)
		{
			if (startX > endX)
				(startX, endX) = (endX, startX);
			for (int x = startX; x <= endX; x++)
				for (int row = 0; row < thickness; row++)
					SetSolidTile(x, y + row, tileType);
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

		public static void UnsealCompound(ApogeanFaction faction) => SetCompoundSealed(faction, sealedShut: false);

		public static void ReArmCompound(ApogeanFaction faction) => SetCompoundSealed(faction, sealedShut: true);

		private static void SetCompoundSealed(ApogeanFaction faction, bool sealedShut)
		{
			CompoundGen instance = ModContent.GetInstance<CompoundGen>();
			if (!instance.compoundBounds.TryGetValue(faction, out Rectangle bounds))
				return;

			Rectangle door = GetDoorBounds(faction, bounds);
			SetDoor(door, sealedShut);
			NetMessage.SendTileSquare(-1, door.Center.X, door.Center.Y, Math.Max(door.Width, door.Height) + 4);
		}

		private static Rectangle GetDoorBounds(ApogeanFaction faction, Rectangle bounds)
		{
			return faction switch
			{
				ApogeanFaction.Kessler => new Rectangle(bounds.Center.X - 3, bounds.Top + 62, 7, 9),
				ApogeanFaction.Helix => new Rectangle(bounds.Center.X - 3, bounds.Top + 37, 7, 10),
				ApogeanFaction.Sentrix => new Rectangle(bounds.Center.X + 16, bounds.Top + 37, 5, 9),
				_ => Rectangle.Empty
			};
		}

		private static void SetDoor(Rectangle door, bool sealedShut)
		{
			int bulkhead = ModContent.TileType<LockedBulkhead>();
			for (int x = door.Left; x < door.Right; x++)
			{
				for (int y = door.Top; y < door.Bottom; y++)
				{
					if (sealedShut)
						SetSolidTile(x, y, bulkhead);
					else
						Framing.GetTileSafely(x, y).ClearTile();
				}
			}
		}

		private static bool TryGetFaction(ApogeanLandmarkKind kind, out ApogeanFaction faction)
		{
			faction = kind switch
			{
				ApogeanLandmarkKind.KesslerCampus => ApogeanFaction.Kessler,
				ApogeanLandmarkKind.HelixCampus => ApogeanFaction.Helix,
				ApogeanLandmarkKind.SentrixCampus => ApogeanFaction.Sentrix,
				_ => ApogeanFaction.None
			};
			return faction != ApogeanFaction.None;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			foreach ((ApogeanFaction faction, Rectangle bounds) in compoundBounds)
			{
				tag[$"compound_{faction}_x"] = bounds.X;
				tag[$"compound_{faction}_y"] = bounds.Y;
				tag[$"compound_{faction}_width"] = bounds.Width;
				tag[$"compound_{faction}_height"] = bounds.Height;
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			compoundBounds.Clear();
			foreach (ApogeanFaction faction in FactionProgression.CorpFactions)
			{
				string xKey = $"compound_{faction}_x";
				string yKey = $"compound_{faction}_y";
				if (!tag.ContainsKey(xKey) || !tag.ContainsKey(yKey))
					continue;
				int width = tag.ContainsKey($"compound_{faction}_width") ? tag.GetInt($"compound_{faction}_width") : 9;
				int height = tag.ContainsKey($"compound_{faction}_height") ? tag.GetInt($"compound_{faction}_height") : 9;
				compoundBounds[faction] = new Rectangle(tag.GetInt(xKey), tag.GetInt(yKey), width, height);
			}
		}
	}
}
