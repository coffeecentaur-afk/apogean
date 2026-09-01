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
			int spawnIndex = FindPass(tasks, "Spawn Point");
			if (spawnIndex < 0)
				spawnIndex = FindPass(tasks, "Jungle");
			int mawIndex = spawnIndex >= 0 ? Math.Min(tasks.Count, spawnIndex + 1) : tasks.Count;
			tasks.Insert(mawIndex, new PassLegacy("The Maw", EngraftSystem.Instance.GenerateWorld));

			tasks.Insert(mawIndex + 1, new PassLegacy(
				"Apogean Compounds",
				ModContent.GetInstance<CompoundGen>().GenerateWorld));

			if (ModContent.GetInstance<ApogeanWorldConfig>().RuinedSurface)
			{
				int treesIndex = FindPass(tasks, "Planting Trees");
				int wastesIndex = treesIndex >= 0 ? treesIndex + 1 : Math.Min(tasks.Count, mawIndex + 2);
				tasks.Insert(wastesIndex, new PassLegacy("A World Picked Clean", RuinedSurfaceSystem.GenerateWorld));
			}
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
