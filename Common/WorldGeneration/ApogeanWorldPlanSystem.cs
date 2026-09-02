using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace apogean.Common.WorldGeneration
{
	/// <summary>
	/// Owns the saved geography contract. Callers ask whether an edit is allowed; they do not
	/// duplicate spawn, rupture, or future structure protection rules.
	/// </summary>
	public sealed class ApogeanWorldPlanSystem : ModSystem
	{
		private const string SaveKey = "apogeanWorldPlan";
		private readonly ProtectedRegionRegistry protections = new();
		private ApogeanWorldPlan plan;

		public static ApogeanWorldPlanSystem Instance => ModContent.GetInstance<ApogeanWorldPlanSystem>();
		public bool HasPlan => plan is not null;
		public ApogeanWorldPlan Plan => plan;
		public IReadOnlyList<ProtectedRegion> ProtectedRegions => protections.Regions;

		public override void OnWorldLoad()
		{
			plan = null;
			protections.Clear();
		}

		public override void OnWorldUnload()
		{
			plan = null;
			protections.Clear();
		}

		public ApogeanWorldPlan CreateWorldGenPlan(UnifiedRandom worldRandom, Func<int, int> findSurface)
		{
			bool created = plan is null;
			plan ??= ApogeanWorldPlan.CreateWorldGenPlan(worldRandom, findSurface);
			RebuildProtections();
			if (created)
				RegisterWithWorldGenerator();
			return plan;
		}

		public bool CanEditTile(int tileX, int tileY, WorldEditIntent intent)
		{
			if (!WorldGen.InWorld(tileX, tileY, 2))
				return false;

			if (plan is null && IsInsideImplicitLegacySanctuary(tileX, tileY) && BlocksSpawnIntent(intent))
				return false;

			return protections.Allows(tileX, tileY, intent);
		}

		public bool CanPlace(Rectangle tileBounds, WorldEditIntent intent)
		{
			if (tileBounds.Left < 2 || tileBounds.Top < 2 ||
				tileBounds.Right >= Main.maxTilesX - 2 || tileBounds.Bottom >= Main.maxTilesY - 2)
				return false;

			if (plan is null && BlocksSpawnIntent(intent) && tileBounds.Intersects(GetImplicitLegacySanctuary()))
				return false;

			return protections.Allows(tileBounds, intent);
		}

		public bool TryAddRuntimeRupture(
			int preferredX,
			int radiusX,
			int radiusY,
			bool bypassProtection,
			Func<int, int> findSurface,
			UnifiedRandom random,
			out MawRupturePlan rupture)
		{
			EnsureRuntimePlan();
			for (int attempt = 0; attempt < 96; attempt++)
			{
				int x = attempt == 0
					? preferredX
					: preferredX + random.Next(-420, 421);
				x = Utils.Clamp(x, radiusX + 40, Main.maxTilesX - radiusX - 40);
				int y = findSurface(x);
				MawRupturePlan candidate = new(new Point16(x, y), radiusX, radiusY, false);
				if (!bypassProtection && !CanPlace(candidate.ReservedBounds, WorldEditIntent.MawOutgrowth))
					continue;

				plan!.AddRupture(candidate);
				RebuildProtections();
				rupture = candidate;
				return true;
			}

			rupture = null;
			return false;
		}

		public IReadOnlyList<string> Validate()
		{
			List<string> failures = new();
			if (plan is null)
			{
				failures.Add("No saved Apogean world plan exists. This is expected only for a legacy world that has not used runtime generation.");
				return failures;
			}

			if (plan.SchemaVersion != ApogeanWorldPlan.CurrentSchemaVersion)
				failures.Add($"World-plan schema {plan.SchemaVersion} does not match runtime schema {ApogeanWorldPlan.CurrentSchemaVersion}.");
			if (plan.SpawnSanctuary.Width <= 0 || plan.SpawnSanctuary.Height <= 0)
				failures.Add("Spawn sanctuary has invalid dimensions.");
			if (!plan.IsLegacySafetyPlan && plan.GetMajorRupture() is null)
				failures.Add("A generated Apogean world has no major Maw Rupture.");
			if (!plan.IsLegacySafetyPlan && (Main.maxTilesX < 8400 || Main.maxTilesY < 2400))
				failures.Add("The full Apogee campaign was generated on an unsupported non-Large world.");

			for (int i = 0; i < plan.MawRuptures.Count; i++)
			{
				MawRupturePlan rupture = plan.MawRuptures[i];
				if (rupture.GenerationBounds.Intersects(plan.SpawnSanctuary))
					failures.Add($"Maw Rupture {i} intersects the spawn sanctuary.");
				if (rupture.GenerationBounds.Left < 10 || rupture.GenerationBounds.Right >= Main.maxTilesX - 10)
					failures.Add($"Maw Rupture {i} crosses a horizontal world edge.");
				if (rupture.IsMajor && plan.SchemaVersion >= 2 && !rupture.HasNavigationSpine)
					failures.Add("The major Maw Rupture has no saved navigation spine.");
			}

			int outgrowths = 0;
			for (int i = 0; i < plan.MawRuptures.Count; i++)
				if (!plan.MawRuptures[i].IsMajor)
					outgrowths++;
			if (!plan.IsLegacySafetyPlan && outgrowths is < 1 or > 2)
				failures.Add($"Expected one guaranteed and at most one optional Maw Outgrowth; found {outgrowths}.");

			if (!plan.IsLegacySafetyPlan && plan.SchemaVersion >= 2)
			{
				foreach (ApogeanLandmarkKind kind in Enum.GetValues<ApogeanLandmarkKind>())
				{
					int count = 0;
					for (int i = 0; i < plan.Landmarks.Count; i++)
						if (plan.Landmarks[i].Kind == kind)
							count++;
					if (count != 1)
						failures.Add($"Expected exactly one {kind} landmark; found {count}.");
				}

				for (int i = 0; i < plan.Landmarks.Count; i++)
				{
					ApogeanLandmarkPlan landmark = plan.Landmarks[i];
					if (landmark.ReservedBounds.Intersects(plan.SpawnSanctuary))
						failures.Add($"{landmark.Kind} intersects the spawn sanctuary.");
					for (int other = i + 1; other < plan.Landmarks.Count; other++)
						if (landmark.ReservedBounds.Intersects(plan.Landmarks[other].ReservedBounds))
							failures.Add($"{landmark.Kind} intersects {plan.Landmarks[other].Kind}.");
				}
			}

			return failures;
		}

		public override void PostWorldGen()
		{
			IReadOnlyList<string> failures = Validate();
			for (int i = 0; i < failures.Count; i++)
				Mod.Logger.Warn($"World-plan validation: {failures[i]}");
			if (failures.Count == 0 && plan is not null)
			{
				Mod.Logger.Info(
					$"World-plan validation passed: schema={plan.SchemaVersion}; hash={plan.StableHash():X8}; " +
					$"ruptures={plan.MawRuptures.Count}; landmarks={plan.Landmarks.Count}");
			}

			MawRupturePlan major = plan?.GetMajorRupture();
			if (major is not null && major.IsMajor)
			{
				MawRuptureValidationReport report = MawRuptureValidation.Inspect(major);
				if (report.Passed)
					Mod.Logger.Info($"Maw-route validation passed: {report}");
				else
					Mod.Logger.Warn($"Maw-route validation failed: {report}");
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (plan is not null)
				tag[SaveKey] = plan.Save();
		}

		public override void LoadWorldData(TagCompound tag)
		{
			plan = tag.ContainsKey(SaveKey) ? ApogeanWorldPlan.Load(tag.GetCompound(SaveKey)) : null;
			RebuildProtections();
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(plan is not null);
			plan?.NetSend(writer);
		}

		public override void NetReceive(BinaryReader reader)
		{
			plan = reader.ReadBoolean() ? ApogeanWorldPlan.NetReceive(reader) : null;
			RebuildProtections();
		}

		private void EnsureRuntimePlan()
		{
			if (plan is not null)
				return;
			plan = ApogeanWorldPlan.CreateLegacySafetyPlan();
			RebuildProtections();
		}

		private void RebuildProtections()
		{
			protections.Clear();
			if (plan is null)
				return;

			protections.Reserve(
				"spawn-sanctuary",
				plan.SpawnSanctuary,
				WorldEditIntent.MawGeneration |
				WorldEditIntent.MawSpread |
				WorldEditIntent.MawOutgrowth |
				WorldEditIntent.CorporateStructure);

			for (int i = 0; i < plan.MawRuptures.Count; i++)
			{
				protections.Reserve(
					$"maw-rupture-{i}",
					plan.MawRuptures[i].ReservedBounds,
					WorldEditIntent.MawOutgrowth |
					WorldEditIntent.CorporateStructure |
					WorldEditIntent.RuinStructure);
			}

			for (int i = 0; i < plan.Landmarks.Count; i++)
			{
				ApogeanLandmarkPlan landmark = plan.Landmarks[i];
				WorldEditIntent blocked = WorldEditIntent.MawGeneration |
					WorldEditIntent.MawSpread |
					WorldEditIntent.MawOutgrowth |
					WorldEditIntent.CorporateStructure |
					WorldEditIntent.RuinStructure;
				protections.Reserve($"landmark-{landmark.Kind}", landmark.ReservedBounds, blocked);
			}
		}

		private void RegisterWithWorldGenerator()
		{
			if (plan is null)
				return;

			GenVars.structures.AddProtectedStructure(plan.SpawnSanctuary, 8);
			for (int i = 0; i < plan.MawRuptures.Count; i++)
				GenVars.structures.AddProtectedStructure(plan.MawRuptures[i].ReservedBounds, 8);
			for (int i = 0; i < plan.Landmarks.Count; i++)
				GenVars.structures.AddProtectedStructure(plan.Landmarks[i].Bounds, plan.Landmarks[i].Padding);
		}

		private static bool BlocksSpawnIntent(WorldEditIntent intent) =>
			(intent & (WorldEditIntent.MawGeneration |
				WorldEditIntent.MawSpread |
				WorldEditIntent.MawOutgrowth |
				WorldEditIntent.CorporateStructure)) != 0;

		private static bool IsInsideImplicitLegacySanctuary(int tileX, int tileY)
			=> GetImplicitLegacySanctuary().Contains(tileX, tileY);

		private static Rectangle GetImplicitLegacySanctuary()
		{
			int spawnX = Main.spawnTileX > 0 ? Main.spawnTileX : Main.maxTilesX / 2;
			int spawnY = Main.spawnTileY > 0 ? Main.spawnTileY : (int)Main.worldSurface;
			return new Rectangle(
				spawnX - ApogeanWorldPlan.SpawnSanctuaryRadiusX,
				spawnY - ApogeanWorldPlan.SpawnSanctuaryRadiusY,
				ApogeanWorldPlan.SpawnSanctuaryRadiusX * 2 + 1,
				ApogeanWorldPlan.SpawnSanctuaryRadiusY * 2 + 1);
		}
	}
}
