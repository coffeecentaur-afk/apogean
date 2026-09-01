using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Projectiles
{
	/// <summary>
	/// The Matriarch's close-range purge. Fires as an expanding shell rather than a static
	/// blast so it visibly sweeps outward - anyone who stood next to her through the telegraph
	/// is caught, anyone who disengaged is already outside its reach.
	/// </summary>
	public class ContainmentPulse : ModProjectile
	{
		public const int Lifetime = 26;
		public const float MaxRadius = 200f;

		private float Progress => 1f - Projectile.timeLeft / (float)Lifetime;
		private float CurrentRadius => MathHelper.SmoothStep(0f, MaxRadius, Progress);

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = Lifetime;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			// The shell is drawn entirely in dust - a ring of particles walked outward each
			// frame, densest at the leading edge.
			float radius = CurrentRadius;
			int points = 26;

			for (int i = 0; i < points; i++)
			{
				float angle = MathHelper.TwoPi * i / points + Progress * 0.6f;
				Vector2 position = Projectile.Center + angle.ToRotationVector2() * radius;

				Dust dust = Dust.NewDustPerfect(position, DustID.AmberBolt,
					angle.ToRotationVector2() * 1.5f, 120, default, 1.3f * (1f - Progress * 0.4f));
				dust.noGravity = true;
			}
		}

		/// <summary>Radial rather than rectangular, so the shell catches evenly in every direction.</summary>
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 targetCenter = new(targetHitbox.Center.X, targetHitbox.Center.Y);
			return Vector2.Distance(Projectile.Center, targetCenter) <= CurrentRadius;
		}

		// Nothing to draw - the dust shell is the whole effect.
		public override bool PreDraw(ref Color lightColor) => false;
	}
}
