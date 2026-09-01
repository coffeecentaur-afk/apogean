namespace apogean.Content.Factions
{
	/// <summary>
	/// Implemented by NPCs, items, or other content tied to one of the Wastes' factions,
	/// so faction-wide systems (aggro, reputation, themed loot, etc.) can query it generically.
	/// </summary>
	public interface IFactionEntity
	{
		ApogeanFaction Faction { get; }
	}
}
