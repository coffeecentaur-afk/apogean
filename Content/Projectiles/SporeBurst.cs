using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Projectiles
{
	/// <summary>
	/// Phase 3 leavings - drifts slowly and lingers, so the arena fills up the longer the
	/// fight drags on.
	/// </summary>
	public class SporeBurst : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 240;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Projectile.velocity *= 0.97f;
			Projectile.rotation += 0.04f;

			// Fade in, hold, then fade out over the last second.
			Projectile.alpha = Projectile.timeLeft < 60 ? (int)(255 * (1f - Projectile.timeLeft / 60f)) : 0;

			// Sparse and amber-toned to belong to the Engraft rather than Corruption or Helix acid.
			// Several of these linger at once, so the
			// emission rate stays low to keep the arena readable.
			if (Main.rand.NextBool(20))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
					DustID.AmberBolt, 0f, 0f, 180, default, 0.7f);
				dust.noGravity = true;
				dust.velocity *= 0.2f;
			}
		}

		// Deliberately no debuff: the acid poisons, the spores just deny ground. Stacking
		// Poisoned from a lingering field buried the screen in status particles.
	}
}
