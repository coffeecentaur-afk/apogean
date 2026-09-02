using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.Biomes;
using apogean.Content.Backgrounds;

namespace apogean.Content.Biomes
{
	/// <summary>The living, bounded Broodmass biome. Its tile count is intentionally local so a few nodes never repaint the entire world.</summary>
	public sealed class EngraftBiome : ModBiome
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
		public override int Music => MusicID.UndergroundCorruption;
		// RuinedGlobalBackgroundStyle already selects the Maw surface art. Do not also bind it
		// through ModBiome until the Maw owns a real ModWaterStyle: tModLoader's capture camera
		// otherwise receives water style -1 and indexes outside its liquid texture array.
		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<EngraftRuinedUndergroundStyle>();
		public override string BestiaryIcon => "Terraria/Images/MapBG28";
		public override string BackgroundPath => "Terraria/Images/MapBG28";
		public override Color? BackgroundColor => new Color(47, 37, 25);

		public override bool IsBiomeActive(Player player) =>
			ModContent.GetInstance<MawTileCountSystem>().MawTileCount >= MawTileCountSystem.BiomeActivationCount;
	}
}
