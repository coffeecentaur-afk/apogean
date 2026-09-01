using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.NPCs.Broodmass
{
	/// <summary>
	/// Lobbed by the Matriarch, falls, sticks where it lands and hatches a Broodling on a
	/// timer. Deliberately fragile - the whole point is that you CAN clear them first, so the
	/// player chooses between pressuring her and managing the floor.
	/// </summary>
	public class EggPod : BroodmassNPC
	{
		private const int HatchTime = 300;

		private ref float HatchTimer => ref NPC.ai[0];

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 22;
			NPC.height = 24;
			NPC.lifeMax = 35;
			NPC.defense = 0;
			NPC.damage = 0;
			NPC.knockBackResist = 0f;
			NPC.aiStyle = -1;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.value = 0f;
			NPC.HitSound = SoundID.NPCHit13 with { Pitch = -0.2f };
			NPC.DeathSound = SoundID.NPCDeath13 with { Pitch = -0.1f };
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.apogean.Bestiary.EggPod"));
		}

		public override void AI()
		{
			// Fall, then settle. No chasing - it's a placed hazard, not a hunter.
			NPC.velocity.X *= 0.9f;
			if (NPC.velocity.Y < 10f) NPC.velocity.Y += 0.3f;

			HatchTimer++;

			// Pulses faster as it approaches hatching, so the threat is legible without a UI.
			int pulseRate = HatchTimer > HatchTime * 0.7f ? 4 : 10;
			if (HatchTimer % pulseRate == 0)
			{
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.AmberBolt,
					0f, -1f, 150, default, 0.8f);
				dust.noGravity = true;
			}

			if (HatchTimer < HatchTime) return;

			// Respects the shared brood ceiling - a pod that would overflow it just dies off
			// rather than letting a stacked volley flood the arena.
			if (Main.netMode != NetmodeID.MultiplayerClient && Matriarch.CountBrood() < Matriarch.MaxBrood)
			{
				NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y,
					ModContent.NPCType<Broodling>());
			}

			SoundEngine.PlaySound(SoundID.NPCDeath13 with { Pitch = 0.3f }, NPC.Center);

			for (int i = 0; i < 12; i++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.AmberBolt,
					Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextFloat(-2.5f, 2.5f), 150, default, 1.1f);
			}

			NPC.active = false;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life > 0) return;

			for (int i = 0; i < 10; i++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.AmberBolt,
					Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 150, default, 1f);
			}
		}
	}
}
