using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Weapons;

namespace apogean.Content.Projectiles
{
	/// <summary>Charges while held, then turns the hook into a brief player-driven melee hitbox.</summary>
	public sealed class RendHookLunge : ModProjectile
	{
		private const int MaximumCharge = 45;
		private const int LungeDuration = 16;

		private bool Lunging => Projectile.ai[0] == 1f;

		public override string Texture => "apogean/Content/Items/Weapons/RendHook";

		public override void SetDefaults()
		{
			Projectile.width = 38;
			Projectile.height = 38;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 90;
			Projectile.ownerHitCheck = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 24;
		}

		public override bool ShouldUpdatePosition() => false;
		public override bool? CanDamage() => Lunging ? null : false;

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead || owner.CCed || owner.HeldItem.type != ModContent.ItemType<RendHook>())
			{
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;
			owner.itemTime = 2;
			owner.itemAnimation = 2;

			if (!Lunging)
			{
				UpdateAim(owner);
				Projectile.ai[1] = MathHelper.Min(MaximumCharge, Projectile.ai[1] + 1f);
				Projectile.Center = owner.MountedCenter + Projectile.velocity * (18f + Projectile.ai[1] * 0.25f);
				owner.ChangeDir(Projectile.velocity.X >= 0f ? 1 : -1);
				owner.itemRotation = Projectile.velocity.ToRotation();

				if (!owner.channel)
				{
					BeginLunge(owner);
				}
				else if (Projectile.ai[1] >= MaximumCharge && Main.rand.NextBool(6))
				{
					Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4f), 8, 8, DustID.Torch, 0f, 0f, 160, new Color(220, 122, 35), 0.7f);
					dust.noGravity = true;
				}
				return;
			}

			Projectile.localAI[0]++;
			Projectile.Center = owner.MountedCenter + Projectile.velocity * 34f;
			owner.ChangeDir(Projectile.velocity.X >= 0f ? 1 : -1);
			owner.itemRotation = Projectile.velocity.ToRotation();
			if (Projectile.localAI[0] >= LungeDuration) Projectile.Kill();
		}

		private void UpdateAim(Player owner)
		{
			if (Projectile.owner != Main.myPlayer) return;
			Vector2 aim = owner.DirectionTo(Main.MouseWorld);
			if (aim == Vector2.Zero) aim = Vector2.UnitX * owner.direction;
			if (Vector2.DistanceSquared(aim, Projectile.velocity) < 0.0004f) return;
			Projectile.velocity = aim;
			Projectile.netUpdate = true;
		}

		private void BeginLunge(Player owner)
		{
			Projectile.ai[0] = 1f;
			Projectile.localAI[0] = 0f;
			float charge = MathHelper.Clamp(Projectile.ai[1] / MaximumCharge, 0.22f, 1f);
			float speed = MathHelper.Lerp(6.5f, 14f, charge);
			owner.velocity = Projectile.velocity * speed;
			int protection = (int)MathHelper.Lerp(2f, 8f, charge);
			owner.immune = true;
			owner.immuneTime = Math.Max(owner.immuneTime, protection);
			Projectile.netUpdate = true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Player owner = Main.player[Projectile.owner];
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), owner.MountedCenter,
				Projectile.Center + Projectile.velocity * 18f, 28f, ref collisionPoint);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			float charge = MathHelper.Clamp(Projectile.ai[1] / MaximumCharge, 0f, 1f);
			Color drawColor = Color.Lerp(lightColor, new Color(255, 164, 58), charge * 0.45f);
			float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
			SpriteEffects effects = Projectile.velocity.X < 0f ? SpriteEffects.FlipVertically : SpriteEffects.None;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, rotation, texture.Size() * 0.5f, 1f, effects);
			return false;
		}
	}
}
