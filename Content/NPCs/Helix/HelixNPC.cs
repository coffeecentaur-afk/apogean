using apogean.Content.Factions;
using Terraria.ModLoader;

namespace apogean.Content.NPCs.Helix
{
	/// <summary>
	/// Base class for Helix Genomics "Product": escaped splice specimens and gene-harvester
	/// drones. Shared behavior (mutation-themed status effects, harvestable drops) belongs
	/// here once the first concrete NPC exists.
	/// </summary>
	public abstract class HelixNPC : ModNPC, IFactionEntity
	{
		public ApogeanFaction Faction => ApogeanFaction.Helix;
	}
}
