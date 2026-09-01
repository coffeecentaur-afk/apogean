using apogean.Content.Factions;
using Terraria.ModLoader;

namespace apogean.Content.NPCs.Broodmass
{
	/// <summary>
	/// Base class for Broodmass creatures: feral splices, swarm broods, and eventually the
	/// Matriarch's brood-leader tier. Shared behavior (pack aggro, adaptation/enrage on
	/// nearby Broodmass deaths) belongs here once the first concrete NPC exists.
	/// </summary>
	public abstract class BroodmassNPC : ModNPC, IFactionEntity
	{
		public ApogeanFaction Faction => ApogeanFaction.Broodmass;
	}
}
