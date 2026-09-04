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
			// The installed 1.4.4.9+2026.07 runtime passes its already-updated
			// front-layer alpha array to this hook. Advancing it again makes the close
			// layer finish before far/middle. The engine owns the whole-style fade.
		}

		public override int ChooseFarTexture()
		{
			int variant = RuinedBackgroundSelectionSystem.Instance.GetVariant(Biome);
			return BackgroundTextureLoader.GetBackgroundSlot(Mod, $"Content/Backgrounds/{Biome}/V{variant}_Far");
		}

		public override int ChooseMiddleTexture()
		{
			int variant = RuinedBackgroundSelectionSystem.Instance.GetVariant(Biome);
			return BackgroundTextureLoader.GetBackgroundSlot(Mod, $"Content/Backgrounds/{Biome}/V{variant}_Mid");
		}

		public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
		{
			int variant = RuinedBackgroundSelectionSystem.Instance.GetVariant(Biome);
			return BackgroundTextureLoader.GetBackgroundSlot(Mod, $"Content/Backgrounds/{Biome}/V{variant}_Close");
		}
	}

	public sealed class ForestRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Forest; }
	public sealed class DesertRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Desert; }
	public sealed class JungleRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Jungle; }
	public sealed class SnowRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Snow; }
	public sealed class CorruptionRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Corruption; }
	public sealed class CrimsonRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Crimson; }
	public sealed class HallowRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Hallow; }
	public sealed class OceanRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Ocean; }
	public sealed class MushroomRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Mushroom; }
	public sealed class EngraftRuinedBackgroundStyle : ApogeanSurfaceBackgroundStyle { protected override RuinedBackgroundBiome Biome => RuinedBackgroundBiome.Engraft; }

	/// <summary>Renderer-only style for comparing one authored surface decomposition in-engine.</summary>
	public sealed class SurfaceConceptRenderLabBackgroundStyle : ModSurfaceBackgroundStyle
	{
		private static RuinedBackgroundBiome Biome =>
			RuinedBackgroundSelectionSystem.Instance.SurfaceRenderLabBiome ?? RuinedBackgroundBiome.Forest;

		public override void ModifyFarFades(float[] fades, float transitionSpeed) { }

		public override int ChooseFarTexture() =>
			BackgroundTextureLoader.GetBackgroundSlot(Mod, $"Content/Backgrounds/Diagnostics/{Biome}ConceptV0_Far");

		public override int ChooseMiddleTexture() =>
			BackgroundTextureLoader.GetBackgroundSlot(Mod, $"Content/Backgrounds/Diagnostics/{Biome}ConceptV0_Mid");

		public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) =>
			BackgroundTextureLoader.GetBackgroundSlot(Mod, $"Content/Backgrounds/Diagnostics/{Biome}ConceptV0_Close");
	}

	public sealed class RuinedGlobalBackgroundStyle : GlobalBackgroundStyle
	{
		internal static int ResolveRuinedSurfaceStyle(Player player)
		{
			if (RuinedBackgroundSelectionSystem.Instance.SurfaceRenderLabBiome.HasValue)
				return ModContent.GetInstance<SurfaceConceptRenderLabBackgroundStyle>().Slot;

			return RuinedBackgroundSelectionSystem.DetectBiome(player) switch
			{
				RuinedBackgroundBiome.Desert => ModContent.GetInstance<DesertRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Jungle => ModContent.GetInstance<JungleRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Snow => ModContent.GetInstance<SnowRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Corruption => ModContent.GetInstance<CorruptionRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Crimson => ModContent.GetInstance<CrimsonRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Hallow => ModContent.GetInstance<HallowRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Ocean => ModContent.GetInstance<OceanRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Mushroom => ModContent.GetInstance<MushroomRuinedBackgroundStyle>().Slot,
				RuinedBackgroundBiome.Engraft => ModContent.GetInstance<EngraftRuinedBackgroundStyle>().Slot,
				_ => ModContent.GetInstance<ForestRuinedBackgroundStyle>().Slot
			};
		}

		public override void ChooseSurfaceBackgroundStyle(ref int style)
		{
			if (Main.gameMenu || !ModContent.GetInstance<ApogeanWorldConfig>().RuinedBiomeBackgrounds) return;
			Player player = Main.LocalPlayer;
			if (player == null || !player.active) return;
			if (RuinedBackgroundSelectionSystem.Instance.SurfaceRenderLabBiome.HasValue)
			{
				style = ResolveRuinedSurfaceStyle(player);
				return;
			}

			// The whole-world Wastes treatment replaces only Terraria's built-in
			// panoramas. A third-party ModBiome has already won priority arbitration
			// by this point and must keep its own background outside the Maw.
			if (ModContent.GetModSurfaceBackgroundStyle(style) != null) return;

			style = ResolveRuinedSurfaceStyle(player);
		}

		public override void ChooseUndergroundBackgroundStyle(ref int style)
		{
			if (Main.gameMenu || !ModContent.GetInstance<ApogeanWorldConfig>().RuinedBiomeBackgrounds) return;
			Player player = Main.LocalPlayer;
			if (player == null || !player.active) return;
			if (RuinedBackgroundSelectionSystem.Instance.UndergroundRenderLabBiome.HasValue)
			{
				style = ModContent.GetInstance<UndergroundRenderLabBackgroundStyle>().Slot;
				return;
			}

			if (player.ZoneUnderworldHeight ||
				player.ZoneDungeon ||
				ModContent.GetModUndergroundBackgroundStyle(style) != null) return;

			style = RuinedBackgroundSelectionSystem.DetectBiome(player) switch
			{
				RuinedBackgroundBiome.Mushroom => ModContent.GetInstance<MushroomRuinedUndergroundStyle>().Slot,
				RuinedBackgroundBiome.Desert => ModContent.GetInstance<DesertRuinedUndergroundStyle>().Slot,
				RuinedBackgroundBiome.Jungle => ModContent.GetInstance<JungleRuinedUndergroundStyle>().Slot,
				RuinedBackgroundBiome.Snow => ModContent.GetInstance<SnowRuinedUndergroundStyle>().Slot,
				RuinedBackgroundBiome.Corruption => ModContent.GetInstance<CorruptionRuinedUndergroundStyle>().Slot,
				RuinedBackgroundBiome.Crimson => ModContent.GetInstance<CrimsonRuinedUndergroundStyle>().Slot,
				RuinedBackgroundBiome.Hallow => ModContent.GetInstance<HallowRuinedUndergroundStyle>().Slot,
				RuinedBackgroundBiome.Ocean => ModContent.GetInstance<OceanRuinedUndergroundStyle>().Slot,
				RuinedBackgroundBiome.Engraft => ModContent.GetInstance<EngraftRuinedUndergroundStyle>().Slot,
				_ => ModContent.GetInstance<ForestRuinedUndergroundStyle>().Slot
			};
		}
	}
}
