using System.Collections.Generic;
using Terraria.ModLoader;
using apogean.Content.Factions;
using apogean.Content.NPCs.Kessler;

namespace apogean.Content.Invasions
{
	/// <summary>
	/// Resolves faction spawn pools lazily, after content IDs exist. Arrival phase selection stays
	/// here instead of leaking concrete NPC knowledge into the progression state machine.
	/// </summary>
	public static class InvasionNpcRegistry
	{
		public static IReadOnlyDictionary<int, float> GetSpawnPool(
			ApogeanFaction faction,
			FactionProgression progression)
		{
			if (faction != ApogeanFaction.Kessler || !progression.IsKesslerAssessmentActive)
				return Empty;

			if (progression.GetInvasionKillsRemaining(faction) > FactionProgression.KesslerEliteThreshold)
			{
				return new Dictionary<int, float>
				{
					[ModContent.NPCType<KesslerSurveyDrone>()] = 1f
				};
			}

			return new Dictionary<int, float>
			{
				[ModContent.NPCType<KesslerSurveyDrone>()] = 0.25f,
				[ModContent.NPCType<KesslerReclaimer>()] = 0.75f
			};
		}

		private static readonly IReadOnlyDictionary<int, float> Empty = new Dictionary<int, float>();
	}
}
