using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Weapons;

namespace apogean.Content.Projectiles
{
	/// <summary>A restrained life drain: strong positional utility, deliberately slow healing.</summary>
	public sealed class UmbilicalTether : ModProjectile
	{
		public const float MaximumRange = 540f;
		private const int ManaInterval = 30;
		private const int HealInterval = 36;

		private NPC Target => Projectile.ai[0] >= 0 && Projectile.ai[0] < Main.maxNPCs ? Main.npc[(int)Projectile.ai[0]] : null;

		public override string Texture => "apogean/Content/Items/Weapons/AmberSiphon";

		public override void SetDefaults()
		{
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 2;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 18;
		}

		public override bool ShouldUpdatePosition() => false;

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			NPC target = Target;
			if (!owner.active || owner.dead || owner.CCed || !owner.channel || owner.HeldItem.type != ModContent.ItemType<AmberSiphon>() ||
				target == null || !target.active || !target.CanBeChasedBy() || Vector2.Distance(owner.Center, target.Center) > MaximumRange)
			{
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = 2;
			Projectile.Center = target.Center;
			owner.heldProj = Projectile.whoAmI;
			owner.itemTime = 2;
			owner.itemAnimation = 2;
			owner.ChangeDir(target.Center.X >= owner.Center.X ? 1 : -1);
			owner.itemRotation = owner.DirectionTo(target.Center).ToRotation();

			Projectile.localAI[0]++;
			if (Projectile.localAI[0] % ManaInterval == 0 && Projectile.owner == Main.myPlayer && !owner.CheckMana(2, true))
			{
				Projectile.Kill();
			}
		}

		public override bool? CanHitNPC(NPC target) => target.whoAmI == (int)Projectile.ai[0] ? null : false;

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Projectile.owner != Main.myPlayer) return;
			Player owner = Main.player[Projectile.owner];
			Projectile.localAI[1] += Projectile.localNPCHitCooldown;
			if (Projectile.localAI[1] < HealInterval || owner.statLife >= owner.statLifeMax2) return;

			Projectile.localAI[1] = 0f;
			owner.Heal(1);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player owner = Main.player[Projectile.owner];
			NPC target = Target;
			if (target == null || !target.active) return false;

			Vector2 start = owner.MountedCenter - Main.screenPosition;
			Vector2 end = target.Center - Main.screenPosition;
			Vector2 difference = end - start;
			float length = difference.Length();
			if (length < 1f) return false;

			Vector2 direction = difference / length;
			Vector2 normal = new(-direction.Y, direction.X);
			const int segments = 20;
			Vector2 previous = start;
			for (int i = 1; i <= segments; i++)
			{
				float progress = i / (float)segments;
				float wave = (float)System.Math.Sin(progress * MathHelper.Pi * 4f + Main.GlobalTimeWrappedHourly * 5f) * 3f * (float)System.Math.Sin(progress * MathHelper.Pi);
				Vector2 current = Vector2.Lerp(start, end, progress) + normal * wave;
				DrawSegment(previous, current, new Color(48, 22, 18), 5f);
				DrawSegment(previous, current, new Color(183, 92, 28), 2f);
				previous = current;
			}

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() * 0.5f;
			Main.EntitySpriteDraw(texture, owner.MountedCenter - Main.screenPosition, null, lightColor, owner.itemRotation + MathHelper.PiOver4,
				origin, 1f, owner.direction < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None);
			return false;
		}

		private static void DrawSegment(Vector2 start, Vector2 end, Color color, float width)
		{
			Vector2 delta = end - start;
			Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, start, null, color, delta.ToRotation(), Vector2.Zero,
				new Vector2(delta.Length(), width), SpriteEffects.None);
		}
	}
}
