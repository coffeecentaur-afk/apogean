using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using apogean.Content.Factions;

namespace apogean.Content.Invasions
{
	public sealed class KesslerAssessmentHud : ModSystem
	{
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int index = layers.FindIndex(layer => layer.Name == "Vanilla: Resource Bars");
			if (index < 0)
				index = layers.Count;

			layers.Insert(index, new LegacyGameInterfaceLayer(
				"Apogean: Kessler Assessment",
				DrawAssessment,
				InterfaceScaleType.UI));
		}

		private static bool DrawAssessment()
		{
			FactionProgression progression = ModContent.GetInstance<FactionProgression>();
			if (!progression.IsKesslerAssessmentActive || Main.gameMenu)
				return true;

			const int width = 360;
			const int height = 14;
			int remaining = progression.GetInvasionKillsRemaining(ApogeanFaction.Kessler);
			float completion = 1f - remaining / (float)FactionProgression.InvasionKillQuota;
			Vector2 origin = new(Main.screenWidth / 2f - width / 2f, 72f);
			string titleKey = remaining <= FactionProgression.KesslerEliteThreshold
				? "Mods.apogean.Kessler.Arrival.HudElite"
				: "Mods.apogean.Kessler.Arrival.Hud";
			string title = Language.GetTextValue(titleKey, remaining);
			Vector2 textSize = FontAssets.MouseText.Value.MeasureString(title);
			Utils.DrawBorderString(Main.spriteBatch, title, new Vector2(Main.screenWidth / 2f - textSize.X / 2f, 46f), Color.White);

			Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)origin.X - 2, (int)origin.Y - 2, width + 4, height + 4), new Color(20, 13, 12) * 0.9f);
			Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)origin.X, (int)origin.Y, width, height), new Color(61, 43, 38) * 0.9f);
			Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)origin.X, (int)origin.Y, (int)(width * completion), height), new Color(202, 63, 38));
			return true;
		}
	}
}
