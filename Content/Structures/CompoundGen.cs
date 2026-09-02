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
	/// Drops complete authored Campus blueprints into locations owned by the saved world atlas. The
	/// planner chooses safe envelopes; it never knows or reconstructs a building's internal layout.
	/// </summary>
	public sealed class CompoundGen : ModSystem
	{
		private readonly Dictionary<ApogeanFaction, Rectangle> compoundBounds = new();
		private readonly Dictionary<ApogeanFaction, Rectangle> doorBounds = new();

		public override void OnWorldLoad()
		{
			compoundBounds.Clear();
			doorBounds.Clear();
		}

		public override void OnWorldUnload()
		{
			compoundBounds.Clear();
			doorBounds.Clear();
		}

		internal void GenerateWorld(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "Reactivating the old corporate campuses...";
			compoundBounds.Clear();
			doorBounds.Clear();
			ApogeanWorldPlan plan = ApogeanWorldPlanSystem.Instance.Plan;
			if (plan is null)
				return;

			for (int i = 0; i < plan.Landmarks.Count; i++)
			{
				ApogeanLandmarkPlan landmark = plan.Landmarks[i];
				if (!TryGetFaction(landmark.Kind, out ApogeanFaction faction))
					continue;

				AuthoredStructurePlacement placement = CorporateCampusBlueprints.Place(Mod, faction, landmark.Bounds);
				compoundBounds[faction] = landmark.Bounds;
				doorBounds[faction] = placement.Entrance;
				SetDoor(faction, placement.Entrance, sealedShut: true);
			}
		}

		public static void UnsealCompound(ApogeanFaction faction) => SetCompoundSealed(faction, sealedShut: false);

		public static void ReArmCompound(ApogeanFaction faction) => SetCompoundSealed(faction, sealedShut: true);

		private static void SetCompoundSealed(ApogeanFaction faction, bool sealedShut)
		{
			CompoundGen instance = ModContent.GetInstance<CompoundGen>();
			if (!instance.compoundBounds.TryGetValue(faction, out Rectangle bounds))
				return;

			Rectangle door = instance.GetDoorBounds(faction, bounds);
			SetDoor(faction, door, sealedShut);
			NetMessage.SendTileSquare(-1, door.Center.X, door.Center.Y, Math.Max(door.Width, door.Height) + 4);
		}

		private Rectangle GetDoorBounds(ApogeanFaction faction, Rectangle bounds)
		{
			if (doorBounds.TryGetValue(faction, out Rectangle savedDoor))
				return savedDoor;
			return CorporateCampusBlueprints.GetPlacement(Mod, faction, bounds).Entrance;
		}

		private static void SetDoor(ApogeanFaction faction, Rectangle door, bool sealedShut)
		{
			int bulkhead = GetCampusTile(faction);
			for (int x = door.Left; x < door.Right; x++)
			{
				for (int y = door.Top; y < door.Bottom; y++)
				{
					if (!WorldGen.InWorld(x, y, 10))
						continue;
					if (sealedShut)
					{
						Tile tile = Framing.GetTileSafely(x, y);
						tile.HasTile = true;
						tile.TileType = (ushort)bulkhead;
						tile.TileFrameX = 0;
						tile.TileFrameY = 0;
						tile.Slope = SlopeType.Solid;
						tile.IsHalfBlock = false;
						tile.LiquidAmount = 0;
					}
					else
					{
						Framing.GetTileSafely(x, y).ClearTile();
					}
					WorldGen.SquareTileFrame(x, y, true);
				}
			}
		}

		private static int GetCampusTile(ApogeanFaction faction) => faction switch
		{
			ApogeanFaction.Kessler => ModContent.TileType<KesslerBlock>(),
			ApogeanFaction.Helix => ModContent.TileType<HelixBlock>(),
			ApogeanFaction.Sentrix => ModContent.TileType<SentrixBlock>(),
			_ => ModContent.TileType<LockedBulkhead>()
		};

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
				Rectangle door = GetDoorBounds(faction, bounds);
				tag[$"compound_{faction}_door_x"] = door.X;
				tag[$"compound_{faction}_door_y"] = door.Y;
				tag[$"compound_{faction}_door_width"] = door.Width;
				tag[$"compound_{faction}_door_height"] = door.Height;
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			compoundBounds.Clear();
			doorBounds.Clear();
			foreach (ApogeanFaction faction in FactionProgression.CorpFactions)
			{
				string xKey = $"compound_{faction}_x";
				string yKey = $"compound_{faction}_y";
				if (!tag.ContainsKey(xKey) || !tag.ContainsKey(yKey))
					continue;
				int width = tag.ContainsKey($"compound_{faction}_width") ? tag.GetInt($"compound_{faction}_width") : 9;
				int height = tag.ContainsKey($"compound_{faction}_height") ? tag.GetInt($"compound_{faction}_height") : 9;
				Rectangle bounds = new(tag.GetInt(xKey), tag.GetInt(yKey), width, height);
				compoundBounds[faction] = bounds;

				string doorXKey = $"compound_{faction}_door_x";
				if (tag.ContainsKey(doorXKey))
				{
					doorBounds[faction] = new Rectangle(
						tag.GetInt(doorXKey),
						tag.GetInt($"compound_{faction}_door_y"),
						tag.GetInt($"compound_{faction}_door_width"),
						tag.GetInt($"compound_{faction}_door_height"));
				}
			}
		}
	}
}
