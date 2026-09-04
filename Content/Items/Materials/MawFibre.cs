using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Items.Materials
{
	/// <summary>Fibrous amber tissue collected from the Engraft. The first non-boss material in its crafting loop.</summary>
	public sealed class MawFibre : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 50);
			Item.rare = ItemRarityID.Blue;
			Item.material = true;
		}
	}
}
