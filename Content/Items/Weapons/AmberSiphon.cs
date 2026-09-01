using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Materials;
using apogean.Content.Projectiles;

namespace apogean.Content.Items.Weapons
{
	/// <summary>Coaxes a harmless-looking amber clot into a precise early magic bolt.</summary>
	public sealed class AmberSiphon : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 23;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 5;
			Item.width = 20;
			Item.height = 20;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<AmberBolt>();
			Item.shootSpeed = 8.5f;
			Item.knockBack = 3f;
			Item.value = Item.buyPrice(silver: 70);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item20 with { Pitch = -0.4f };
			Item.autoReuse = true;
		}

		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<MawFibre>(8)
			.AddIngredient<MutagenGland>(3)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
