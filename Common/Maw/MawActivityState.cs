using Terraria;
using Terraria.ModLoader;
using apogean.Content.Factions;

namespace apogean.Common.Maw
{
	public static class MawActivityState
	{
		/// <summary>Matriarch defeat quiets the Maw only until Wall of Flesh begins Hardmode.</summary>
		public static bool IsDormant =>
			!Main.gameMenu &&
			!Main.hardMode &&
			ModContent.GetInstance<FactionProgression>().MatriarchDowned;
	}
}
