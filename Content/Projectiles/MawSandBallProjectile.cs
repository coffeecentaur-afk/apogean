using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Placeable;
using apogean.Content.Tiles;

namespace apogean.Content.Projectiles
{
	public abstract class MawSandBallProjectile : ModProjectile
	{
		public override string Texture => "apogean/Content/Projectiles/MawSandBallProjectile";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.FallingBlockDoesNotFallThroughPlatforms[Type] = true;
			ProjectileID.Sets.ForcePlateDetection[Type] = true;
		}
	}

	public sealed class MawSandBallFallingProjectile : MawSandBallProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.FallingBlockTileItem[Type] = new(
				ModContent.TileType<MawSand>(), ModContent.ItemType<MawSandBlock>());
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.EbonsandBallFalling);
		}
	}

	public sealed class MawSandBallGunProjectile : MawSandBallProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.FallingBlockTileItem[Type] = new(ModContent.TileType<MawSand>());
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.EbonsandBallGun);
			AIType = ProjectileID.EbonsandBallGun;
		}
	}
}
