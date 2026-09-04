using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Items.Currency
{
	/// <summary>
	/// Non-coin Kessler requisition token. Arrival enemies and contracts issue it; corporate
	/// shops consume it without replacing ordinary coin-based convenience purchases.
	/// </summary>
	public sealed class KesslerScrip : ModItem
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.DefenderMedal}";

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 22;
			Item.maxStack = 9999;
			Item.rare = ItemRarityID.Orange;
			Item.value = 0;
		}
	}
}
