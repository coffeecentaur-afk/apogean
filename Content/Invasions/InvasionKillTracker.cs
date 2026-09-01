using Terraria;
using Terraria.ModLoader;
using apogean.Content.Factions;

namespace apogean.Content.Invasions
{
	/// <summary>
	/// Counts kills of faction-tagged NPCs toward that faction's invasion quota. Once the
	/// quota hits zero, FactionProgression flips the faction to Contactable and its compound
	/// opens up.
	/// </summary>
	public class InvasionKillTracker : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (npc.ModNPC is not IFactionEntity factionEntity) return;
			ModContent.GetInstance<FactionProgression>().RegisterInvasionKill(factionEntity.Faction);
		}
	}
}
