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
	/// <summary>First assessment tier: a mobile observer with a long, explicit firing line.</summary>
	public sealed class KesslerSurveyDrone : KesslerNPC
	{
		private const int CycleTicks = 170;
		private const int TelegraphStart = 92;
		private const int FirstShot = 132;

		public override string Texture => $"Terraria/Images/NPC_{NPCID.Probe}";

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Probe];
			AnimationType = NPCID.Probe;
		}

		public override void SetDefaults()
		{
			NPC.width = 34;
			NPC.height = 30;
			NPC.lifeMax = 220;
			NPC.defense = 12;
			NPC.damage = 0;
			NPC.knockBackResist = 0.35f;
			NPC.value = 0f;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
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

			float side = target.Center.X < NPC.Center.X ? 1f : -1f;
			Vector2 station = target.Center + new Vector2(side * 245f, -145f);
			Vector2 desiredVelocity = NPC.DirectionTo(station) * 7.5f;
			NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.045f);
			NPC.rotation = NPC.velocity.X * 0.025f;
			NPC.spriteDirection = NPC.direction = target.Center.X < NPC.Center.X ? -1 : 1;

			int timer = (int)NPC.ai[0];
			if ((timer == FirstShot || timer == FirstShot + 18) && Main.netMode != NetmodeID.MultiplayerClient)
				Fire(target, 11f, 24);
			if (timer == TelegraphStart && Main.netMode != NetmodeID.Server)
				SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.45f, Pitch = 0.25f }, NPC.Center);

			NPC.ai[0] = (timer + 1) % CycleTicks;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			int timer = (int)NPC.ai[0];
			if (timer >= TelegraphStart && timer < FirstShot && TryGetTarget(out Player target))
			{
				float pulse = 0.45f + 0.55f * (timer - TelegraphStart) / (FirstShot - TelegraphStart);
				KesslerTelegraph.Draw(spriteBatch, NPC.Center, target.Center, pulse);
			}
			return true;
		}

		public override Color? GetAlpha(Color drawColor) => Color.Lerp(drawColor, new Color(214, 81, 49), 0.28f);

		public override void ModifyNPCLoot(NPCLoot npcLoot) =>
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<KesslerScrip>(), 1, 1, 2));

		private void Fire(Player target, float speed, int damage)
		{
			Vector2 velocity = NPC.DirectionTo(target.Center) * speed;
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
				ModContent.ProjectileType<KesslerTracer>(), damage, 1f, Main.myPlayer);
			NPC.netUpdate = true;
		}

		private bool TryGetTarget(out Player target)
		{
			target = NPC.target >= 0 && NPC.target < Main.maxPlayers ? Main.player[NPC.target] : null;
			return target is { active: true, dead: false };
		}
	}
}
