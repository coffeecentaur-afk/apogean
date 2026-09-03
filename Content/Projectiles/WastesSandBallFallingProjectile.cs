using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Placeable;
using apogean.Content.Tiles;

namespace apogean.Content.Projectiles
{
	/// <summary>Shared Terraria-native behavior and art for both forms of airborne Wastes Sand.</summary>
	public abstract class WastesSandBallProjectile : ModProjectile
	{
		public override string Texture => "apogean/Content/Projectiles/WastesSandBallProjectile";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.FallingBlockDoesNotFallThroughPlatforms[Type] = true;
			ProjectileID.Sets.ForcePlateDetection[Type] = true;
		}
	}

	/// <summary>Preserves Wastes Sand identity while it uses Terraria's native falling-sand AI.</summary>
	public sealed class WastesSandBallFallingProjectile : WastesSandBallProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.FallingBlockTileItem[Type] = new(
				ModContent.TileType<WastesSand>(), ModContent.ItemType<WastesSandBlock>());
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.EbonsandBallFalling);
		}
	}

	/// <summary>The recover-free ranged form fired when Wastes Sand is loaded into a Sandgun.</summary>
	public sealed class WastesSandBallGunProjectile : WastesSandBallProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.FallingBlockTileItem[Type] = new(ModContent.TileType<WastesSand>());
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.EbonsandBallGun);
			AIType = ProjectileID.EbonsandBallGun;
		}
	}
}
