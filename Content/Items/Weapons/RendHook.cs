using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Materials;

namespace apogean.Content.Items.Weapons
{
	/// <summary>Early melee tool made from a Graft Hound's hooked forelimb.</summary>
	public sealed class RendHook : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 27;
			Item.DamageType = DamageClass.Melee;
			Item.width = 20;
			Item.height = 20;
			Item.useTime = 22;
			Item.useAnimation = 22;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5.5f;
			Item.value = Item.buyPrice(silver: 70);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1 with { Pitch = -0.2f };
			Item.autoReuse = true;
		}

		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<MawFibre>(8)
			.AddIngredient<MutagenGland>(3)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
