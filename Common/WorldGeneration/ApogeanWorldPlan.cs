using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace apogean.Common.WorldGeneration
{
	public sealed class ApogeanWorldPlan
	{
		public const int CurrentSchemaVersion = 1;
		public const int SpawnSanctuaryRadiusX = 110;
		public const int SpawnSanctuaryRadiusY = 70;

		private readonly List<MawRupturePlan> mawRuptures = new();

		public int SchemaVersion { get; }
		public int PlanSeed { get; }
		public Rectangle SpawnSanctuary { get; }
		public bool IsLegacySafetyPlan { get; }
		public IReadOnlyList<MawRupturePlan> MawRuptures => mawRuptures;

		private ApogeanWorldPlan(int schemaVersion, int planSeed, Rectangle spawnSanctuary, bool isLegacySafetyPlan)
		{
			SchemaVersion = schemaVersion;
			PlanSeed = planSeed;
			SpawnSanctuary = spawnSanctuary;
			IsLegacySafetyPlan = isLegacySafetyPlan;
		}

		internal static ApogeanWorldPlan CreateWorldGenPlan(UnifiedRandom worldRandom, Func<int, int> findSurface)
		{
			int planSeed = worldRandom.Next();
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

			MawRupturePlan major = FindRuptureSite(random, findSurface, occupied, 170, 76, true);
			plan.mawRuptures.Add(major);
			occupied.Add(major.ReservedBounds);

			for (int i = 0; i < 2; i++)
			{
				MawRupturePlan outgrowth = FindRuptureSite(random, findSurface, occupied, 62, 36, false);
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
			bool major)
		{
			int edgePadding = major ? 440 : 300;
			int spawnX = ValidSpawnX(Main.spawnTileX) ? Main.spawnTileX : Main.maxTilesX / 2;
			int minimumSpawnDistance = major ? Math.Max(620, Main.maxTilesX / 7) : 360;

			for (int attempt = 0; attempt < 320; attempt++)
			{
				int x = random.Next(edgePadding, Main.maxTilesX - edgePadding);
				if (Math.Abs(x - spawnX) < minimumSpawnDistance)
					continue;
				if (Main.dungeonX > 0 && Math.Abs(x - Main.dungeonX) < (major ? 520 : 280))
					continue;

				int y = findSurface(x);
				MawRupturePlan candidate = new(new Point16(x, y), radiusX, radiusY, major);
				if (IntersectsAny(candidate.ReservedBounds, occupied))
					continue;

				return candidate;
			}

			int fallbackX = spawnX < Main.maxTilesX / 2
				? Main.maxTilesX - edgePadding - radiusX
				: edgePadding + radiusX;
			fallbackX = Utils.Clamp(fallbackX, edgePadding, Main.maxTilesX - edgePadding);
			return new MawRupturePlan(new Point16(fallbackX, findSurface(fallbackX)), radiusX, radiusY, major);
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

			return new TagCompound
			{
				["schema"] = SchemaVersion,
				["seed"] = PlanSeed,
				["spawnX"] = SpawnSanctuary.X,
				["spawnY"] = SpawnSanctuary.Y,
				["spawnWidth"] = SpawnSanctuary.Width,
				["spawnHeight"] = SpawnSanctuary.Height,
				["legacy"] = IsLegacySafetyPlan,
				["ruptures"] = savedRuptures
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
			return plan;
		}
	}
}
