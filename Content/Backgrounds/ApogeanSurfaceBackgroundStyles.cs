using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using apogean.Content.Config;

namespace apogean.Content.Backgrounds
{
	public abstract class ApogeanSurfaceBackgroundStyle : ModSurfaceBackgroundStyle
	{
		protected abstract RuinedBackgroundBiome Biome { get; }

		public override void ModifyFarFades(float[] fades, float transitionSpeed)
		{
			for (int i = 0; i < fades.Length; i++)
			{
				fades[i] = i == Slot
					? Math.Min(1f, fades[i] + transitionSpeed)
					: Math.Max(0f, fades[i] - transitionSpeed);
			}
		}

		public override int ChooseFarTexture()
		{
			int variant = RuinedBackgroundSelectionSystem.Instance.GetVariant(Biome);
			string lighting = Main.eclipse ? "Eclipse" : Main.dayTime ? "Day" : "Night";
			return BackgroundTextureLoader.GetBackgroundSlot(Mod, $"Content/Backgrounds/{Biome}/V{variant}_{lighting}");
		}

		public override int ChooseMiddleTexture() => BackgroundTextureLoader.GetBackgroundSlot(Mod, "Content/Backgrounds/Transparent");
		public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) => false;
	}

	public sealed class ForestRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Forest; }
	public sealed class DesertRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Desert; }
	public sealed class JungleRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Jungle; }
	public sealed class SnowRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Snow; }
	public sealed class CorruptionRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Corruption; }
	public sealed class CrimsonRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Crimson; }
	public sealed class HallowRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Hallow; }
	public sealed class OceanRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Ocean; }
	public sealed class EngraftRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Engraft; }

	public sealed class RuinedGlobalBackgroundStyle : GlobalBackgroundStyle
	{
		public override void ChooseSurfaceBackgroundStyle(ref int style)
		{
			if (Main.gameMenu || !ModContent.GetInstance<ApogeanWorldConfig>().RuinedBiomeBackgrounds) return;
			Player player = Main.LocalPlayer;
			if (player == null || !player.active) return;

			style = RuinedBackgroundSelectionSystem.DetectBiome(player) switch
			{
				RuinedBackgroundBiome.Desert => ModContent.GetInstance<DesertRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Jungle => ModContent.GetInstance<JungleRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Snow => ModContent.GetInstance<SnowRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Corruption => ModContent.GetInstance<CorruptionRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Crimson => ModContent.GetInstance<CrimsonRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Hallow => ModContent.GetInstance<HallowRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Ocean => ModContent.GetInstance<OceanRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Engraft => ModContent.GetInstance<EngraftRuinedBackgroundStyle>().Slot,
				_ => ModContent.GetInstance<ForestRuinedBackgroundStyle>().Slot
			};
		}
	}
}
