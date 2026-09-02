using Terraria.ModLoader;

namespace apogean.Content.Backgrounds
{
	public abstract class ApogeanUndergroundBackgroundStyle : ModUndergroundBackgroundStyle
	{
		protected abstract RuinedBackgroundBiome Biome { get; }

		public override void FillTextureArray(int[] textureSlots)
		{
			int variant = RuinedBackgroundSelectionSystem.Instance.GetVariant(Biome);
			for (int i = 0; i < 4; i++)
			{
				textureSlots[i] = BackgroundTextureLoader.GetBackgroundSlot(Mod,
					$"Content/Backgrounds/{Biome}/Underground/V{variant}_{i}");
			}
		}
	}

	public sealed class ForestRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Forest; }
	public sealed class DesertRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Desert; }
	public sealed class JungleRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Jungle; }
	public sealed class SnowRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Snow; }
	public sealed class CorruptionRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Corruption; }
	public sealed class CrimsonRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Crimson; }
	public sealed class HallowRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Hallow; }
	public sealed class OceanRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Ocean; }
	public sealed class MushroomRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Mushroom; }
	public sealed class UnderworldRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Underworld; }
	public sealed class EngraftRuinedUndergroundStyle : ApogeanUndergroundBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Engraft; }
}
