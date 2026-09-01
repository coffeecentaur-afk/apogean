using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Projectiles
{
	/// <summary>Short-lived organic sentry. It orbits the summoner and lunges at nearby enemies instead of replacing a true minion progression later.</summary>
	public sealed class MawSentry : ModProjectile
	{
		public override string Texture => "apogean/Content/NPCs/Engraft/Mawling";

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 4;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 1800;
			Projectile.sentry = true;
			Projectile.minionSlots = 0.5f;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead) { Projectile.Kill(); return; }

			NPC target = FindTarget(owner.Center);
			Vector2 destination;
			if (target != null)
			{
				destination = target.Center;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(destination) * 7f, 0.12f);
			}
			else
			{
				float orbit = (float)(Main.GameUpdateCount * 0.045 + Projectile.whoAmI);
				destination = owner.Center + orbit.ToRotationVector2() * 54f;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(destination) * 5f, 0.08f);
			}

			Projectile.spriteDirection = Projectile.direction = Projectile.velocity.X >= 0f ? 1 : -1;
			Projectile.rotation = MathHelper.Lerp(Projectile.rotation, MathHelper.Clamp(Projectile.velocity.Y * 0.08f, -0.3f, 0.3f), 0.12f);
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= 6)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
			}
		}

		private static NPC FindTarget(Vector2 center)
		{
			NPC closest = null;
			float closestDistance = 420f;
			foreach (NPC npc in Main.npc)
			{
				if (!npc.CanBeChasedBy()) continue;
				float distance = Vector2.Distance(center, npc.Center);
				if (distance >= closestDistance) continue;
				closest = npc;
				closestDistance = distance;
			}
			return closest;
		}
	}
}
