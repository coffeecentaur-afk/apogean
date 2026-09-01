using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Projectiles
{
	/// <summary>Matriarch's zoning shot - arcs, sticks nothing, just punishes standing still.</summary>
	public class AcidGlob : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.16f;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			// Up to 7 of these fly at once in later phases, so the trail stays thin.
			if (Main.rand.NextBool(9))
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.JungleSpore,
					0f, 0f, 120, default, 0.9f);
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 4; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.JungleSpore,
					Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f), 120, default, 1f);
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			target.AddBuff(BuffID.Poisoned, 180);
		}
	}
}
