using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Projectiles
{
	/// <summary>A compact, amber magic shot. Green is reserved for Helix chemistry, not the Engraft's basic spell language.</summary>
	public sealed class AmberBolt : ModProjectile
	{
		public override string Texture => "apogean/Content/Items/Weapons/AmberSiphon";

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 240;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Projectile.rotation += 0.18f;
			if (Main.rand.NextBool(5))
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AmberBolt, 0f, 0f, 120, default, 0.7f);
				dust.noGravity = true;
			}
		}
	}
}
