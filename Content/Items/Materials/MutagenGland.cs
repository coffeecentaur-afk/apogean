using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Items.Materials
{
	/// <summary>
	/// Harvested from Broodmass tissue. The base crafting material for early Apogean gear -
	/// and, later, the thing Helix will want back.
	/// </summary>
	public class MutagenGland : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 9999;
			Item.value = Item.sellPrice(silver: 8);
			Item.rare = ItemRarityID.Orange;
			Item.material = true;
		}
	}
}
