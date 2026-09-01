using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Materials;

namespace apogean.Content.Items.Weapons
{
	/// <summary>A flexible bow that serves the early ranged route without restricting ammunition choice.</summary>
	public sealed class SinewBow : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 20;
			Item.height = 20;
			Item.useTime = 21;
			Item.useAnimation = 21;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAmmo = AmmoID.Arrow;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.shootSpeed = 9f;
			Item.knockBack = 3f;
			Item.value = Item.buyPrice(silver: 70);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item5 with { Pitch = -0.15f };
			Item.autoReuse = true;
		}

		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<MawFibre>(8)
			.AddIngredient<MutagenGland>(3)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
