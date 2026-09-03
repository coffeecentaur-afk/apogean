using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Projectiles;
using apogean.Content.Tiles;

namespace apogean.Content.Items.Placeable
{
	public abstract class MawTerrainBlockItem<TTile> : ModItem where TTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 100;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<TTile>());
			Item.width = 16;
			Item.height = 16;
		}
	}

	public sealed class MawDirtBlock : MawTerrainBlockItem<MawDirt> { }
	public sealed class MawstoneBlock : MawTerrainBlockItem<Mawstone> { }
	public sealed class MawIceBlock : MawTerrainBlockItem<MawIce> { }
	public sealed class MawSnowBlock : MawTerrainBlockItem<MawSnow> { }
	public sealed class MawMudBlock : MawTerrainBlockItem<MawMud> { }
	public sealed class MawClayBlock : MawTerrainBlockItem<MawClay> { }

	public sealed class MawSandBlock : MawTerrainBlockItem<MawSand>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ItemID.Sets.SandgunAmmoProjectileData[Type] = new(
				ModContent.ProjectileType<MawSandBallGunProjectile>(), 10);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.width = 12;
			Item.height = 12;
			Item.ammo = AmmoID.Sand;
			Item.notAmmo = true;
		}
	}
}
