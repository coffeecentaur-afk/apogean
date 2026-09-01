using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using apogean.Content.Structures;

namespace apogean.Content.Factions
{
	/// <summary>
	/// Tracks each corp faction's relationship to the player across a world: Dormant until
	/// its arrival milestone is hit, Hostile while its invasion is pending, Contactable once
	/// the invasion is cleared, then Allied/Enemy once the player picks a side late-game.
	/// The Broodmass isn't tracked here - it's the pre-hardmode horde, not a diplomatic actor.
	/// </summary>
	public class FactionProgression : ModSystem
	{
		public const int InvasionKillQuota = 20;

		public static readonly ApogeanFaction[] CorpFactions =
		{
			ApogeanFaction.Kessler,
			ApogeanFaction.Helix,
			ApogeanFaction.Sentrix
		};

		private readonly Dictionary<ApogeanFaction, FactionRelation> relations = new()
		{
			[ApogeanFaction.Kessler] = FactionRelation.Dormant,
			[ApogeanFaction.Helix] = FactionRelation.Dormant,
			[ApogeanFaction.Sentrix] = FactionRelation.Dormant,
		};

		private readonly Dictionary<ApogeanFaction, int> invasionKillsRemaining = new();

		public bool MatriarchDowned { get; private set; }
		public ApogeanFaction? ChosenAlliance { get; private set; }

		public FactionRelation GetRelation(ApogeanFaction faction) =>
			relations.TryGetValue(faction, out FactionRelation relation) ? relation : FactionRelation.Dormant;

		public static void SetMatriarchDowned()
		{
			ModContent.GetInstance<FactionProgression>().MatriarchDowned = true;
		}

		public void RegisterInvasionKill(ApogeanFaction faction)
		{
			if (GetRelation(faction) != FactionRelation.Hostile) return;
			if (!invasionKillsRemaining.TryGetValue(faction, out int remaining)) return;

			remaining--;
			invasionKillsRemaining[faction] = remaining;

			if (remaining <= 0)
			{
				SetContactable(faction);
			}
		}

		/// <summary>
		/// Only succeeds once, and only once all three corp invasions have been cleared.
		/// Sets the chosen faction Allied and the other two Enemy - the hinge point the
		/// endgame gauntlets and CEO fights key off of.
		/// </summary>
		public void SetAlliance(ApogeanFaction chosen)
		{
			if (ChosenAlliance != null) return;

			foreach (ApogeanFaction faction in CorpFactions)
			{
				if (GetRelation(faction) != FactionRelation.Contactable) return;
			}

			ChosenAlliance = chosen;
			foreach (ApogeanFaction faction in CorpFactions)
			{
				relations[faction] = faction == chosen ? FactionRelation.Allied : FactionRelation.Enemy;
				if (faction != chosen)
				{
					CompoundGen.ReArmCompound(faction);
				}
			}
		}

		private void SetHostile(ApogeanFaction faction)
		{
			if (GetRelation(faction) != FactionRelation.Dormant) return;
			relations[faction] = FactionRelation.Hostile;
			invasionKillsRemaining[faction] = InvasionKillQuota;
		}

		private void SetContactable(ApogeanFaction faction)
		{
			relations[faction] = FactionRelation.Contactable;
			CompoundGen.UnsealCompound(faction);
		}

		public override void PostUpdateWorld()
		{
			if (GetRelation(ApogeanFaction.Kessler) == FactionRelation.Dormant && NPC.downedBoss3)
			{
				SetHostile(ApogeanFaction.Kessler);
			}

			if (GetRelation(ApogeanFaction.Helix) == FactionRelation.Dormant &&
				NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
			{
				SetHostile(ApogeanFaction.Helix);
			}

			if (GetRelation(ApogeanFaction.Sentrix) == FactionRelation.Dormant && NPC.downedPlantBoss)
			{
				SetHostile(ApogeanFaction.Sentrix);
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["matriarchDowned"] = MatriarchDowned;
			tag["chosenAlliance"] = ChosenAlliance.HasValue ? (int)ChosenAlliance.Value : -1;

			foreach (ApogeanFaction faction in CorpFactions)
			{
				tag[$"relation_{faction}"] = (int)GetRelation(faction);
				tag[$"quota_{faction}"] = invasionKillsRemaining.TryGetValue(faction, out int remaining) ? remaining : 0;
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			MatriarchDowned = tag.GetBool("matriarchDowned");
			int alliance = tag.GetInt("chosenAlliance");
			ChosenAlliance = alliance < 0 ? null : (ApogeanFaction)alliance;

			foreach (ApogeanFaction faction in CorpFactions)
			{
				string relationKey = $"relation_{faction}";
				relations[faction] = tag.ContainsKey(relationKey)
					? (FactionRelation)tag.GetInt(relationKey)
					: FactionRelation.Dormant;

				string quotaKey = $"quota_{faction}";
				if (tag.ContainsKey(quotaKey))
				{
					invasionKillsRemaining[faction] = tag.GetInt(quotaKey);
				}
			}
		}
	}
}
