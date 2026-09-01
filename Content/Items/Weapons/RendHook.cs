using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Materials;
using apogean.Content.Projectiles;

namespace apogean.Content.Items.Weapons
{
	/// <summary>A severed grasping limb that stores tension, then drags its bearer through a short violent lunge.</summary>
	public sealed class RendHook : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 30;
			Item.DamageType = DamageClass.Melee;
			Item.width = 56;
			Item.height = 56;
			Item.useTime = 45;
			Item.useAnimation = 45;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;
			Item.shoot = ModContent.ProjectileType<RendHookLunge>();
			Item.shootSpeed = 1f;
			Item.knockBack = 6.5f;
			Item.value = Item.buyPrice(silver: 70);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1 with { Pitch = -0.35f, Volume = 0.65f };
			Item.reuseDelay = 16;
		}

		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
			int type, int damage, float knockback)
		{
			Vector2 direction = player.DirectionTo(Main.MouseWorld);
			if (direction == Vector2.Zero) direction = Vector2.UnitX * player.direction;
			Projectile.NewProjectile(source, player.MountedCenter, direction, type, damage, knockback, player.whoAmI);
			return false;
		}

		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<MawFibre>(8)
			.AddIngredient<MutagenGland>(3)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
