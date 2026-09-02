using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace apogean.Common.WorldGeneration
{
	public sealed class ApogeanWorldPlan
	{
		public const int CurrentSchemaVersion = 2;
		public const int SpawnSanctuaryRadiusX = 110;
		public const int SpawnSanctuaryRadiusY = 70;

		private readonly List<MawRupturePlan> mawRuptures = new();
		private readonly List<ApogeanLandmarkPlan> landmarks = new();

		public int SchemaVersion { get; }
		public int PlanSeed { get; }
		public Rectangle SpawnSanctuary { get; }
		public bool IsLegacySafetyPlan { get; }
		public IReadOnlyList<MawRupturePlan> MawRuptures => mawRuptures;
		public IReadOnlyList<ApogeanLandmarkPlan> Landmarks => landmarks;

		private ApogeanWorldPlan(int schemaVersion, int planSeed, Rectangle spawnSanctuary, bool isLegacySafetyPlan)
		{
			SchemaVersion = schemaVersion;
			PlanSeed = planSeed;
			SpawnSanctuary = spawnSanctuary;
			IsLegacySafetyPlan = isLegacySafetyPlan;
		}

		internal static ApogeanWorldPlan CreateWorldGenPlan(UnifiedRandom worldRandom, Func<int, int> findSurface)
		{
			return CreateWorldGenPlanFromSeed(worldRandom.Next(), findSurface);
		}

		internal static ApogeanWorldPlan CreateWorldGenPlanFromSeed(int planSeed, Func<int, int> findSurface)
		{
			if (Main.maxTilesX < 8400 || Main.maxTilesY < 2400)
			{
				throw new InvalidOperationException(
					"The full Apogee campaign currently requires a standard Large world (8400 x 2400 tiles). " +
					"Medium compact support is planned, but incomplete campaign worlds are not generated silently.");
			}

			UnifiedRandom random = new(planSeed);
			int spawnX = ValidSpawnX(Main.spawnTileX) ? Main.spawnTileX : Main.maxTilesX / 2;
			int spawnY = ValidSpawnY(Main.spawnTileY) ? Main.spawnTileY : findSurface(spawnX);
			Rectangle sanctuary = ClampToWorld(new Rectangle(
				spawnX - SpawnSanctuaryRadiusX,
				spawnY - SpawnSanctuaryRadiusY,
				SpawnSanctuaryRadiusX * 2 + 1,
				SpawnSanctuaryRadiusY * 2 + 1));

			ApogeanWorldPlan plan = new(CurrentSchemaVersion, planSeed, sanctuary, false);
			List<Rectangle> occupied = new() { sanctuary };

			MawRupturePlan major = FindRuptureSite(random, findSurface, occupied, 200, 78, true, planSeed);
			if (major is null)
				throw new InvalidOperationException($"Apogee could not place a protected, traversable Maw Rupture on this seed; {MawNavigationPlanner.LastFailureReason}.");
			plan.mawRuptures.Add(major);
			occupied.Add(major.ReservedBounds);
			plan.landmarks.AddRange(WorldAtlasPlanner.PlaceLandmarks(random, findSurface, major, occupied));

			for (int i = 0; i < 2; i++)
			{
				MawRupturePlan outgrowth = FindRuptureSite(random, findSurface, occupied, random.Next(30, 46), random.Next(15, 26), false, planSeed ^ (i + 1) * 104729);
				if (outgrowth is null)
				{
					if (i == 0)
						throw new InvalidOperationException("Apogee could not place the guaranteed Maw Outgrowth on this seed.");
					break;
				}
				plan.mawRuptures.Add(outgrowth);
				occupied.Add(outgrowth.ReservedBounds);
			}

			return plan;
		}

		internal static ApogeanWorldPlan CreateLegacySafetyPlan()
		{
			int spawnX = ValidSpawnX(Main.spawnTileX) ? Main.spawnTileX : Main.maxTilesX / 2;
			int spawnY = ValidSpawnY(Main.spawnTileY) ? Main.spawnTileY : (int)Main.worldSurface;
			Rectangle sanctuary = ClampToWorld(new Rectangle(
				spawnX - SpawnSanctuaryRadiusX,
				spawnY - SpawnSanctuaryRadiusY,
				SpawnSanctuaryRadiusX * 2 + 1,
				SpawnSanctuaryRadiusY * 2 + 1));
			return new ApogeanWorldPlan(CurrentSchemaVersion, Main.worldID, sanctuary, true);
		}

		private static MawRupturePlan FindRuptureSite(
			UnifiedRandom random,
			Func<int, int> findSurface,
			IReadOnlyList<Rectangle> occupied,
			int radiusX,
			int radiusY,
			bool major,
			int routeSeed)
		{
			int edgePadding = major ? 440 : 300;
			int spawnX = ValidSpawnX(Main.spawnTileX) ? Main.spawnTileX : Main.maxTilesX / 2;
			int minimumSpawnDistance = major ? 900 : 360;

			for (int attempt = 0; attempt < 320; attempt++)
			{
				int x;
				if (major)
				{
					int side = random.NextBool() ? 1 : -1;
					x = spawnX + side * random.Next(900, 1401);
				}
				else
				{
					x = random.Next(edgePadding, Main.maxTilesX - edgePadding);
				}

				x = Utils.Clamp(x, edgePadding, Main.maxTilesX - edgePadding);
				if (Math.Abs(x - spawnX) < minimumSpawnDistance)
					continue;
				if (Main.dungeonX > 0 && Math.Abs(x - Main.dungeonX) < (major ? 520 : 280))
					continue;

				int y = findSurface(x);
				if (!IsNeutralSurfaceSite(x, y, radiusX))
					continue;

				MawRupturePlan candidate;
				if (major)
				{
					if (!MawNavigationPlanner.TryCreate(
						x,
						y,
						320,
						unchecked(routeSeed ^ x * 397 ^ attempt * 7919),
						out List<Point16> spine,
						out Point16 matriarchCenter))
						continue;

					candidate = new MawRupturePlan(new Point16(x, y), radiusX, radiusY, true, false, matriarchCenter, spine);
				}
				else
				{
					candidate = new MawRupturePlan(new Point16(x, y), radiusX, radiusY, false);
				}

				if (IntersectsAny(candidate.ReservedBounds, occupied))
					continue;
				if (!major && !WorldAtlasPlanner.CanReserve(candidate.ReservedBounds, 12))
					continue;

				return candidate;
			}

			return null;
		}

		private static bool IsNeutralSurfaceSite(int centerX, int centerY, int radiusX)
		{
			int evilTiles = 0;
			int jungleTiles = 0;
			int desertTiles = 0;
			int samples = 0;
			for (int x = centerX - radiusX; x <= centerX + radiusX; x += 8)
			{
				for (int y = Math.Max(20, centerY - 28); y <= Math.Min(Main.maxTilesY - 20, centerY + 42); y += 6)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (!tile.HasTile)
						continue;
					samples++;
					if (tile.TileType is TileID.Ebonstone or TileID.Crimstone or TileID.CorruptGrass or TileID.CrimsonGrass or TileID.Ebonsand or TileID.Crimsand)
						evilTiles++;
					if (tile.TileType is TileID.JungleGrass or TileID.Mud)
						jungleTiles++;
					if (tile.TileType is TileID.Sand or TileID.HardenedSand or TileID.Sandstone)
						desertTiles++;
				}
			}

			if (samples == 0)
				return false;
			return evilTiles * 25 < samples && jungleTiles * 8 < samples && desertTiles * 6 < samples;
		}

		private static bool IntersectsAny(Rectangle candidate, IReadOnlyList<Rectangle> occupied)
		{
			for (int i = 0; i < occupied.Count; i++)
			{
				if (candidate.Intersects(occupied[i]))
					return true;
			}

			return false;
		}

		private static bool ValidSpawnX(int x) => x >= 40 && x < Main.maxTilesX - 40;
		private static bool ValidSpawnY(int y) => y >= 40 && y < Main.maxTilesY - 40;

		private static Rectangle ClampToWorld(Rectangle rectangle)
		{
			int left = Utils.Clamp(rectangle.Left, 10, Main.maxTilesX - 11);
			int top = Utils.Clamp(rectangle.Top, 10, Main.maxTilesY - 11);
			int right = Utils.Clamp(rectangle.Right, left + 1, Main.maxTilesX - 10);
			int bottom = Utils.Clamp(rectangle.Bottom, top + 1, Main.maxTilesY - 10);
			return new Rectangle(left, top, right - left, bottom - top);
		}

		internal void AddRupture(MawRupturePlan rupture) => mawRuptures.Add(rupture);

		public MawRupturePlan GetMajorRupture()
		{
			for (int i = 0; i < mawRuptures.Count; i++)
			{
				if (mawRuptures[i].IsMajor)
					return mawRuptures[i];
			}

			return null;
		}

		public ApogeanLandmarkPlan GetLandmark(ApogeanLandmarkKind kind)
		{
			for (int i = 0; i < landmarks.Count; i++)
				if (landmarks[i].Kind == kind)
					return landmarks[i];
			return null;
		}

		public uint StableHash()
		{
			uint hash = 2166136261;
			Mix(ref hash, SchemaVersion);
			Mix(ref hash, PlanSeed);
			Mix(ref hash, SpawnSanctuary.X);
			Mix(ref hash, SpawnSanctuary.Y);
			Mix(ref hash, SpawnSanctuary.Width);
			Mix(ref hash, SpawnSanctuary.Height);
			for (int i = 0; i < mawRuptures.Count; i++)
			{
				MawRupturePlan rupture = mawRuptures[i];
				Mix(ref hash, rupture.SurfaceCenter.X);
				Mix(ref hash, rupture.SurfaceCenter.Y);
				Mix(ref hash, rupture.RadiusX);
				Mix(ref hash, rupture.RadiusY);
				Mix(ref hash, rupture.IsMajor ? 1 : 0);
				Mix(ref hash, rupture.IsCompact ? 1 : 0);
				Mix(ref hash, rupture.MatriarchCenter.X);
				Mix(ref hash, rupture.MatriarchCenter.Y);
				for (int point = 0; point < rupture.NavigationSpine.Count; point++)
				{
					Mix(ref hash, rupture.NavigationSpine[point].X);
					Mix(ref hash, rupture.NavigationSpine[point].Y);
				}
			}
			for (int i = 0; i < landmarks.Count; i++)
			{
				ApogeanLandmarkPlan landmark = landmarks[i];
				Mix(ref hash, (int)landmark.Kind);
				Mix(ref hash, landmark.Bounds.X);
				Mix(ref hash, landmark.Bounds.Y);
				Mix(ref hash, landmark.Bounds.Width);
				Mix(ref hash, landmark.Bounds.Height);
				Mix(ref hash, landmark.Padding);
			}

			return hash;
		}

		private static void Mix(ref uint hash, int value)
		{
			hash ^= unchecked((uint)value);
			hash *= 16777619;
		}

		internal TagCompound Save()
		{
			List<TagCompound> savedRuptures = new();
			for (int i = 0; i < mawRuptures.Count; i++)
				savedRuptures.Add(mawRuptures[i].Save());
			List<TagCompound> savedLandmarks = new();
			for (int i = 0; i < landmarks.Count; i++)
				savedLandmarks.Add(landmarks[i].Save());

			return new TagCompound
			{
				["schema"] = SchemaVersion,
				["seed"] = PlanSeed,
				["spawnX"] = SpawnSanctuary.X,
				["spawnY"] = SpawnSanctuary.Y,
				["spawnWidth"] = SpawnSanctuary.Width,
				["spawnHeight"] = SpawnSanctuary.Height,
				["legacy"] = IsLegacySafetyPlan,
				["ruptures"] = savedRuptures,
				["landmarks"] = savedLandmarks
			};
		}

		internal static ApogeanWorldPlan Load(TagCompound tag)
		{
			Rectangle sanctuary = new(
				tag.GetInt("spawnX"),
				tag.GetInt("spawnY"),
				tag.GetInt("spawnWidth"),
				tag.GetInt("spawnHeight"));
			ApogeanWorldPlan plan = new(tag.GetInt("schema"), tag.GetInt("seed"), sanctuary, tag.GetBool("legacy"));
			foreach (TagCompound savedRupture in tag.GetList<TagCompound>("ruptures"))
				plan.mawRuptures.Add(MawRupturePlan.Load(savedRupture));
			if (tag.ContainsKey("landmarks"))
			{
				foreach (TagCompound savedLandmark in tag.GetList<TagCompound>("landmarks"))
					plan.landmarks.Add(ApogeanLandmarkPlan.Load(savedLandmark));
			}
			return plan;
		}

		internal void NetSend(BinaryWriter writer)
		{
			writer.Write((byte)SchemaVersion);
			writer.Write(PlanSeed);
			writer.Write((short)SpawnSanctuary.X);
			writer.Write((short)SpawnSanctuary.Y);
			writer.Write((short)SpawnSanctuary.Width);
			writer.Write((short)SpawnSanctuary.Height);
			writer.Write(IsLegacySafetyPlan);
			writer.Write((byte)mawRuptures.Count);
			for (int i = 0; i < mawRuptures.Count; i++)
				mawRuptures[i].NetSend(writer);
			writer.Write((byte)landmarks.Count);
			for (int i = 0; i < landmarks.Count; i++)
				landmarks[i].NetSend(writer);
		}

		internal static ApogeanWorldPlan NetReceive(BinaryReader reader)
		{
			int schema = reader.ReadByte();
			int seed = reader.ReadInt32();
			Rectangle sanctuary = new(
				reader.ReadInt16(),
				reader.ReadInt16(),
				reader.ReadInt16(),
				reader.ReadInt16());
			ApogeanWorldPlan plan = new(schema, seed, sanctuary, reader.ReadBoolean());
			int count = reader.ReadByte();
			for (int i = 0; i < count; i++)
				plan.mawRuptures.Add(MawRupturePlan.NetReceive(reader));
			int landmarkCount = reader.ReadByte();
			for (int i = 0; i < landmarkCount; i++)
				plan.landmarks.Add(ApogeanLandmarkPlan.NetReceive(reader));
			return plan;
		}
	}
}
