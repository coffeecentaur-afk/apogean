using apogean.Content.Backgrounds;
using apogean.Content.Items.Currency;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace apogean
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class apogean : Mod
	{
		public static int KesslerScripCurrencyId { get; private set; } = -1;

		public override void Load()
		{
			RuinedUnderworldSky.EnsureRegistered();
			KesslerScripCurrencyId = CustomCurrencyManager.RegisterCurrency(
				new CustomCurrencySingleCoin(ModContent.ItemType<KesslerScrip>(), 9999L));
		}

		public override void Unload()
		{
			KesslerScripCurrencyId = -1;
		}
	}
}
