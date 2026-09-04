using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Factions;
using apogean.Content.Items.Currency;
using apogean.Content.Projectiles;

namespace apogean.Content.NPCs.Kessler
{
	/// <summary>Escalation tier: armoured ground infantry firing a telegraphed three-round burst.</summary>
	public sealed class KesslerReclaimer : KesslerNPC
	{
		private const int CycleTicks = 205;
		private const int TelegraphStart = 108;
		private const int FirstShot = 148;

		public override string Texture => $"Terraria/Images/NPC_{NPCID.TacticalSkeleton}";

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.TacticalSkeleton];
			AnimationType = NPCID.TacticalSkeleton;
		}

		public override void SetDefaults()
		{
			NPC.width = 34;
			NPC.height = 48;
			NPC.lifeMax = 430;
			NPC.defense = 20;
			NPC.damage = 0;
			NPC.knockBackResist = 0.18f;
			NPC.value = 0f;
			NPC.aiStyle = -1;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;
		}

		public override void AI()
		{
			if (!ModContent.GetInstance<FactionProgression>().IsKesslerAssessmentActive)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.active = false;
					NPC.netUpdate = true;
				}
				return;
			}

			NPC.TargetClosest(false);
			if (!TryGetTarget(out Player target))
				return;

			float horizontalDistance = System.Math.Abs(target.Center.X - NPC.Center.X);
			float direction = System.Math.Sign(target.Center.X - NPC.Center.X);
			float desiredX = horizontalDistance > 390f ? direction * 2.8f : horizontalDistance < 225f ? -direction * 2.2f : 0f;
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, desiredX, 0.08f);
			NPC.spriteDirection = NPC.direction = target.Center.X < NPC.Center.X ? -1 : 1;
			if (NPC.collideX && NPC.velocity.Y == 0f)
				NPC.velocity.Y = -6.2f;

			int timer = (int)NPC.ai[0];
			if (timer is FirstShot or FirstShot + 12 or FirstShot + 24)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
					Fire(target);
				if (Main.netMode != NetmodeID.Server)
					SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.38f, Pitch = -0.12f }, NPC.Center);
			}
			if (timer == TelegraphStart && Main.netMode != NetmodeID.Server)
				SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.5f, Pitch = -0.15f }, NPC.Center);

			NPC.ai[0] = (timer + 1) % CycleTicks;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			int timer = (int)NPC.ai[0];
			if (timer >= TelegraphStart && timer < FirstShot && TryGetTarget(out Player target))
			{
				float pulse = 0.5f + 0.5f * (timer - TelegraphStart) / (FirstShot - TelegraphStart);
				KesslerTelegraph.Draw(spriteBatch, NPC.Center + new Vector2(0f, -8f), target.Center, pulse);
			}
			return true;
		}

		public override Color? GetAlpha(Color drawColor) => Color.Lerp(drawColor, new Color(188, 68, 43), 0.22f);

		public override void ModifyNPCLoot(NPCLoot npcLoot) =>
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<KesslerScrip>(), 1, 2, 3));

		private void Fire(Player target)
		{
			Vector2 aimPoint = target.Center + target.velocity * 8f;
			Vector2 velocity = NPC.DirectionTo(aimPoint).RotatedByRandom(0.035f) * 12f;
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0f, -8f), velocity,
				ModContent.ProjectileType<KesslerTracer>(), 30, 1.5f, Main.myPlayer);
			NPC.netUpdate = true;
		}

		private bool TryGetTarget(out Player target)
		{
			target = NPC.target >= 0 && NPC.target < Main.maxPlayers ? Main.player[NPC.target] : null;
			return target is { active: true, dead: false };
		}
	}
}
