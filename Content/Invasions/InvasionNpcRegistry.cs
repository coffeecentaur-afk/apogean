using System.Collections.Generic;
using apogean.Content.Factions;

namespace apogean.Content.Invasions
{
	/// <summary>
	/// Maps each corp faction to the NPC types that spawn during its arrival invasion
	/// (Kessler's strike team, Helix's recon team, Sentrix's lockdown squad). Empty until
	/// those concrete NPCs exist - add each one's NPCType here once it's built.
	/// </summary>
	public static class InvasionNpcRegistry
	{
		public static readonly Dictionary<ApogeanFaction, List<int>> SpawnPools = new()
		{
			[ApogeanFaction.Kessler] = new List<int>(),
			[ApogeanFaction.Helix] = new List<int>(),
			[ApogeanFaction.Sentrix] = new List<int>(),
		};
	}
}
