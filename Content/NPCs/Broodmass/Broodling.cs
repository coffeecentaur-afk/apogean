using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.NPCs.Broodmass
{
	/// <summary>
	/// The Matriarch's spawn - fast, fragile, and only dangerous in numbers. Normally hunts
	/// the player; during Reclamation it breaks off to orbit her as a living shield.
	/// </summary>
	public class Broodling : BroodmassNPC
	{
		private const float ModeHunt = 0f;
		private const float ModeOrbit = 1f;

		// Wide enough that a melee player can stand inside the ring and reach both her and the
		// guards - a tight ring made the channel a ranged-only check.
		private const float OrbitRadius = 260f;

		private ref float Mode => ref NPC.ai[0];
		private ref float OrbitAngle => ref NPC.ai[1];
		private ref float HostIndex => ref NPC.ai[2];

		/// <summary>Counts frames spent buried or walled off, so a bad spawn can't strand one forever.</summary>
		private int stuckTimer;

		public bool IsOrbiting => Mode == ModeOrbit;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.TrailCacheLength[Type] = 4;
			NPCID.Sets.TrailingMode[Type] = 3;
		}

		public override void SetDefaults()
		{
			// Hitbox covers the body only - the sprite is 54px wide because of the wingspan,
			// and wings shouldn't be hurtboxes.
			NPC.width = 30;
			NPC.height = 22;
			NPC.lifeMax = 60;
			NPC.damage = 26;
			NPC.defense = 4;
			NPC.knockBackResist = 0.4f;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			// Boss-summoned adds pay nothing - otherwise a long fight prints money.
			NPC.value = 0f;
			// Wet squelch dropped well below its natural pitch - reads as engineered meat
			// rather than a normal critter.
			NPC.HitSound = SoundID.NPCHit13 with { Pitch = -0.5f, PitchVariance = 0.2f };
			NPC.DeathSound = SoundID.NPCDeath13 with { Pitch = -0.4f, PitchVariance = 0.2f };
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.apogean.Bestiary.Broodling"));
		}

		/// <summary>Called by the Matriarch to pull this one into her protective ring.</summary>
		public void BeginOrbit(NPC host, float angle)
		{
			Mode = ModeOrbit;
			HostIndex = host.whoAmI;
			OrbitAngle = angle;
			NPC.netUpdate = true;
		}

		public override void AI()
		{
			if (!NPC.HasValidTarget) NPC.TargetClosest();

			Player target = Main.player[NPC.target];

			if (!target.active || target.dead)
			{
				NPC.velocity.Y -= 0.2f;
				if (NPC.timeLeft > 60) NPC.timeLeft = 60;
				return;
			}

			if (Mode == ModeOrbit && OrbitAI()) return;

			HuntAI(target);
		}

		/// <summary>Returns false if the host is gone and it should fall back to hunting.</summary>
		private bool OrbitAI()
		{
			NPC host = Main.npc[(int)HostIndex];
			if (!host.active || host.type != ModContent.NPCType<Matriarch>())
			{
				Mode = ModeHunt;
				NPC.noTileCollide = false;
				return false;
			}

			// Phases through terrain while ringing her, so the shield can't be broken by
			// geometry instead of by the player.
			NPC.noTileCollide = true;
			OrbitAngle += 0.05f;

			Vector2 destination = host.Center + OrbitAngle.ToRotationVector2() * OrbitRadius;
			NPC.Center = Vector2.Lerp(NPC.Center, destination, 0.2f);
			NPC.velocity = Vector2.Zero;

			NPC.spriteDirection = NPC.direction = host.Center.X > NPC.Center.X ? -1 : 1;
			NPC.rotation = MathHelper.Lerp(NPC.rotation, 0f, 0.15f);
			return true;
		}

		private void HuntAI(Player target)
		{
			// A bad spawn can drop one inside terrain where nothing can reach it. Track how
			// long it has been buried or walled off and let it phase out rather than leaving
			// an unkillable straggler tethered to the boss.
			bool buried = Collision.SolidCollision(NPC.position, NPC.width, NPC.height);
			bool hasLineOfSight = Collision.CanHitLine(NPC.position, NPC.width, NPC.height,
				target.position, target.width, target.height);

			if (buried) stuckTimer += 3;
			else if (!hasLineOfSight) stuckTimer++;
			else stuckTimer = 0;

			NPC.noTileCollide = stuckTimer > 60;

			// Drift is seeded off whoAmI so a whole brood doesn't fly the identical line.
			float wobble = (float)System.Math.Sin((Main.GameUpdateCount + NPC.whoAmI * 20) * 0.05f) * 2.2f;
			Vector2 desired = NPC.DirectionTo(target.Center) * 6.5f + new Vector2(0f, wobble);

			NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.045f);
			NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;

			// Stays upright like a bat and banks into a dive - never rolls past level, so
			// diving at the player can't flip it upside down mid-flap.
			float bank = MathHelper.Clamp(NPC.velocity.Y * 0.05f, -0.4f, 0.4f);
			NPC.rotation = MathHelper.Lerp(NPC.rotation, bank * NPC.direction, 0.15f);
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter += 0.22f;
			NPC.frameCounter %= Main.npcFrameCount[Type];
			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
		}
	}
}
