using apogean.Content.Factions;
using Terraria.ModLoader;

namespace apogean.Content.NPCs.Kessler
{
	/// <summary>
	/// Base class for Kessler Armaments war machines: turret drones, salvaged walkers,
	/// Reclaimer combat synths. Shared behavior (target-lock telegraphs, requisition-style
	/// drops, etc.) belongs here once the first concrete NPC exists.
	/// </summary>
	public abstract class KesslerNPC : ModNPC, IFactionEntity
	{
		public ApogeanFaction Faction => ApogeanFaction.Kessler;
	}
}
