using Terraria;
using Terraria.ModLoader;
using apogean.Content.NPCs.Broodmass;

namespace apogean.Content.NPCs
{
	/// <summary>
	/// Stops ordinary world spawns while an Apogean boss is alive, the way vanilla bosses
	/// behave - the arena should only contain the boss and what it summons.
	/// </summary>
	public class BossSpawnSuppression : GlobalNPC
	{
		public static bool BossActive => NPC.AnyNPCs(ModContent.NPCType<Matriarch>());

		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			if (!BossActive) return;

			spawnRate = int.MaxValue;
			maxSpawns = 0;
		}
	}
}
