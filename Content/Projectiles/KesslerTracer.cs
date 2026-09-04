using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Projectiles
{
	public sealed class KesslerTracer : ModProjectile
	{
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.BulletHighVelocity}";

		public override void SetDefaults()
		{
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.extraUpdates = 1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Lighting.AddLight(Projectile.Center, 0.55f, 0.08f, 0.025f);
			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, -Projectile.velocity * 0.05f, 80, new Color(245, 54, 28), 0.75f);
				dust.noGravity = true;
			}
		}
	}
}
