using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Materials;
using apogean.Content.Projectiles;

namespace apogean.Content.Items.Weapons
{
	/// <summary>Maintains a living umbilical line that steals a measured amount of life from one nearby enemy.</summary>
	public sealed class AmberSiphon : ModItem
	{
		public override void SetStaticDefaults() => Item.staff[Type] = true;

		public override void SetDefaults()
		{
			Item.damage = 14;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 2;
			Item.width = 36;
			Item.height = 36;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;
			Item.shoot = ModContent.ProjectileType<UmbilicalTether>();
			Item.shootSpeed = 1f;
			Item.knockBack = 0.5f;
			Item.value = Item.buyPrice(silver: 70);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item103 with { Pitch = -0.55f, Volume = 0.55f };
		}

		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
			int type, int damage, float knockback)
		{
			NPC target = FindTarget(Main.MouseWorld, player.Center);
			if (target == null) return false;

			Projectile.NewProjectile(source, target.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, target.whoAmI);
			return false;
		}

		private static NPC FindTarget(Vector2 cursor, Vector2 playerCenter)
		{
			NPC closest = null;
			float closestToCursor = 96f;
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (!npc.CanBeChasedBy() || Vector2.Distance(playerCenter, npc.Center) > UmbilicalTether.MaximumRange) continue;
				float cursorDistance = Vector2.Distance(cursor, npc.Hitbox.ClosestPointInRect(cursor));
				if (cursorDistance >= closestToCursor) continue;
				closest = npc;
				closestToCursor = cursorDistance;
			}
			return closest;
		}

		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<MawFibre>(8)
			.AddIngredient<MutagenGland>(3)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
