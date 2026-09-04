using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using apogean.Content.Factions;

namespace apogean.Content.Invasions
{
	/// <summary>
	/// Kessler's assessment temporarily owns ordinary surface spawns. It does not follow players
	/// underground, and it does not leave faction enemies in the pool after the event completes.
	/// </summary>
	public sealed class InvasionSpawnRates : GlobalNPC
	{
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			FactionProgression progression = ModContent.GetInstance<FactionProgression>();
			if (!progression.IsKesslerAssessmentActive || !spawnInfo.Player.ZoneOverworldHeight)
				return;

			IReadOnlyDictionary<int, float> kesslerPool = InvasionNpcRegistry.GetSpawnPool(
				ApogeanFaction.Kessler,
				progression);
			if (kesslerPool.Count == 0)
				return;

			pool.Clear();
			foreach ((int npcType, float weight) in kesslerPool)
				pool[npcType] = weight;
		}

		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			FactionProgression progression = ModContent.GetInstance<FactionProgression>();
			if (!progression.IsKesslerAssessmentActive || !player.ZoneOverworldHeight)
				return;

			spawnRate = System.Math.Min(spawnRate, 45);
			maxSpawns = System.Math.Max(maxSpawns, 6);
		}
	}
}
