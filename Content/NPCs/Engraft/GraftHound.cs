using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Biomes;
using apogean.Content.Factions;
using apogean.Content.Items.Materials;

namespace apogean.Content.NPCs.Engraft
{
	/// <summary>A recognizable colony hound silhouette warped into a low, pouncing Engraft predator.</summary>
	public sealed class GraftHound : ModNPC, IFactionEntity
	{
		private ref float PounceTimer => ref NPC.ai[0];
		public ApogeanFaction Faction => ApogeanFaction.Broodmass;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 4;
		}

		public override void SetDefaults()
		{
			NPC.width = 44;
			NPC.height = 26;
			NPC.damage = 22;
			NPC.defense = 5;
			NPC.lifeMax = 95;
			NPC.knockBackResist = 0.45f;
			NPC.aiStyle = -1;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.HitSound = SoundID.NPCHit13 with { Pitch = -0.45f, PitchVariance = 0.15f };
			NPC.DeathSound = SoundID.NPCDeath1 with { Pitch = -0.2f };
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) =>
			spawnInfo.Player.InModBiome<EngraftBiome>() && spawnInfo.Player.ZoneOverworldHeight ? 0.14f : 0f;

		public override void AI()
		{
			NPC.TargetClosest();
			Player player = Main.player[NPC.target];
			if (!player.active || player.dead) return;

			float direction = player.Center.X > NPC.Center.X ? 1f : -1f;
			NPC.direction = NPC.spriteDirection = (int)direction;
			PounceTimer++;

			if (NPC.collideY && PounceTimer >= 88f)
			{
				NPC.velocity = new Vector2(direction * 7.5f, -6.2f);
				PounceTimer = 0f;
				SoundEngine.PlaySound(SoundID.NPCHit13 with { Pitch = -0.65f, Volume = 0.5f }, NPC.Center);
			}
			else
			{
				NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, direction * 2.1f, 0.08f);
			}

			if (NPC.velocity.Y < 10f) NPC.velocity.Y += 0.3f;
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter += NPC.velocity.LengthSquared() > 2f ? 0.2f : 0.08f;
			NPC.frameCounter %= Main.npcFrameCount[Type];
			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MawFibre>(), 1, 2, 4));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MutagenGland>(), 5));
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("A colony hound, after the Maw found a use for every bone it had."));
		}
	}
}
