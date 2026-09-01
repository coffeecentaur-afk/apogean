using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Buffs;
using apogean.Content.Items.Materials;
using apogean.Content.Projectiles;

namespace apogean.Content.Items.Weapons
{
	/// <summary>An unsettling temporary sentry, giving summoners an actual Act 1 branch from the first Engraft materials.</summary>
	public sealed class MawEffigy : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 18;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;
			Item.width = 38;
			Item.height = 38;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<MawSentry>();
			Item.shootSpeed = 0f;
			Item.knockBack = 1.5f;
			Item.value = Item.buyPrice(silver: 70);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44 with { Pitch = -0.55f };
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.UpdateMaxTurrets();
			player.AddBuff(ModContent.BuffType<MawEffigyBuff>(), 18000);
			Projectile.NewProjectile(source, player.Center + new Vector2(player.direction * 28f, -18f), Vector2.Zero, type, damage, knockback, player.whoAmI);
			return false;
		}

		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<MawFibre>(8)
			.AddIngredient<MutagenGland>(3)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
