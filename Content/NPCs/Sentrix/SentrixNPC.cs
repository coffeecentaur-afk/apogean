using apogean.Content.Factions;
using Terraria.ModLoader;

namespace apogean.Content.NPCs.Sentrix
{
	/// <summary>
	/// Base class for Sentrix Watch security assets: cyborg enforcers, checkpoint drones,
	/// and indoctrinated "Compliant". Shared behavior (curfew/alert-state AI, loyalty-score
	/// drops) belongs here once the first concrete NPC exists.
	/// </summary>
	public abstract class SentrixNPC : ModNPC, IFactionEntity
	{
		public ApogeanFaction Faction => ApogeanFaction.Sentrix;
	}
}
