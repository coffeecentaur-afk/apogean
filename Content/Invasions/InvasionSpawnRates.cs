using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using apogean.Content.Factions;

namespace apogean.Content.Invasions
{
	/// <summary>
	/// While a corp faction is Hostile (its invasion is pending), boosts spawn weight for
	/// that faction's registered NPCs - the same mechanism vanilla invasions use to flood
	/// spawns near the player instead of a scripted wave list.
	/// </summary>
	public class InvasionSpawnRates : GlobalNPC
	{
		private const float HostileSpawnBoost = 0.3f;

		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			FactionProgression progression = ModContent.GetInstance<FactionProgression>();

			foreach (ApogeanFaction faction in FactionProgression.CorpFactions)
			{
				if (progression.GetRelation(faction) != FactionRelation.Hostile) continue;

				foreach (int npcType in InvasionNpcRegistry.SpawnPools[faction])
				{
					pool[npcType] = pool.TryGetValue(npcType, out float weight) ? weight + HostileSpawnBoost : HostileSpawnBoost;
				}
			}
		}
	}
}
