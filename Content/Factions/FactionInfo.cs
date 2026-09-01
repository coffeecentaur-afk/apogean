using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace apogean.Content.Factions
{
	public class FactionInfo
	{
		public ApogeanFaction Faction { get; }
		public string DisplayName { get; }
		public string Tagline { get; }
		public Color Color { get; }

		public FactionInfo(ApogeanFaction faction, string displayName, string tagline, Color color)
		{
			Faction = faction;
			DisplayName = displayName;
			Tagline = tagline;
			Color = color;
		}

		public static readonly Dictionary<ApogeanFaction, FactionInfo> All = new()
		{
			[ApogeanFaction.Kessler] = new FactionInfo(
				ApogeanFaction.Kessler,
				"Kessler Armaments",
				"The Arsenal",
				new Color(180, 60, 40)),

			[ApogeanFaction.Helix] = new FactionInfo(
				ApogeanFaction.Helix,
				"Helix Genomics",
				"The Bloom",
				new Color(90, 170, 60)),

			[ApogeanFaction.Sentrix] = new FactionInfo(
				ApogeanFaction.Sentrix,
				"Sentrix Watch",
				"The Directive",
				new Color(70, 130, 200)),

			[ApogeanFaction.Broodmass] = new FactionInfo(
				ApogeanFaction.Broodmass,
				"The Broodmass",
				"The Horde",
				new Color(150, 90, 170)),
		};

		public static FactionInfo Get(ApogeanFaction faction) => All.TryGetValue(faction, out FactionInfo info) ? info : null;
	}
}
