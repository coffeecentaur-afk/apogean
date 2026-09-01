using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.World;

namespace apogean.Content.Biomes
{
	/// <summary>The living, bounded Broodmass biome. Its tile count is intentionally local so a few nodes never repaint the entire world.</summary>
	public sealed class EngraftBiome : ModBiome
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
		public override int Music => MusicID.UndergroundCorruption;
		public override string BestiaryIcon => "Terraria/Images/MapBG28";
		public override string BackgroundPath => "Terraria/Images/MapBG28";
		public override Color? BackgroundColor => new Color(47, 37, 25);

		public override bool IsBiomeActive(Player player) => EngraftSystem.IsInEngraft(player.Center);
	}
}
