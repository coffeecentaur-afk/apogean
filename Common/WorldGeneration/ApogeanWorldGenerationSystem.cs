using System;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using apogean.Content.Config;
using apogean.Content.Structures;
using apogean.Content.World;

namespace apogean.Common.WorldGeneration
{
	/// <summary>
	/// Owns Apogee's pass order. Feature modules implement generation, but none independently
	/// negotiates ordering with vanilla or another Apogee module.
	/// </summary>
	public sealed class ApogeanWorldGenerationSystem : ModSystem
	{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			// Apogee is an additive overhaul, not a Remnants-style replacement of vanilla producers.
			// Plan against the completed vanilla world: the bounded planner can then route around every
			// actual chest, micro-biome, Temple, Dungeon, hive, and Hell building before carving once.
			int finalCleanupIndex = FindPass(tasks, "Final Cleanup");
			int finalizationIndex = finalCleanupIndex >= 0 ? finalCleanupIndex + 1 : tasks.Count;
			tasks.Insert(finalizationIndex++, new PassLegacy("The Maw", EngraftSystem.Instance.GenerateWorld));
			tasks.Insert(finalizationIndex++, new PassLegacy(
				"Apogean Compounds",
				ModContent.GetInstance<CompoundGen>().GenerateWorld));
			tasks.Insert(finalizationIndex++, new PassLegacy(
				"Apogean Ruins",
				ModContent.GetInstance<RuinGen>().GenerateWorld));

			if (ModContent.GetInstance<ApogeanWorldConfig>().RuinedSurface)
				tasks.Insert(finalizationIndex, new PassLegacy("A World Picked Clean", RuinedSurfaceSystem.GenerateWorld));
		}

		private static int FindPass(IReadOnlyList<GenPass> tasks, string name)
		{
			for (int i = 0; i < tasks.Count; i++)
			{
				if (tasks[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
					return i;
			}

			return -1;
		}
	}
}
