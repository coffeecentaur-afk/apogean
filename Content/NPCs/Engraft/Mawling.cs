using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Biomes;
using apogean.Content.Factions;
using apogean.Content.Items.Materials;

namespace apogean.Content.NPCs.Engraft
{
	/// <summary>Small larval scavenger. It is weak alone but makes the biome dangerous in a crowd.</summary>
	public sealed class Mawling : ModNPC, IFactionEntity
	{
		public ApogeanFaction Faction => ApogeanFaction.Broodmass;

		public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 4;

		public override void SetDefaults()
		{
			NPC.width = 16;
			NPC.height = 16;
			NPC.damage = 14;
			NPC.defense = 1;
			NPC.lifeMax = 38;
			NPC.knockBackResist = 0.9f;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit13 with { Pitch = 0.25f, PitchVariance = 0.2f };
			NPC.DeathSound = SoundID.NPCDeath1 with { Pitch = 0.35f };
			Banner = Item.NPCtoBanner(Type);
			BannerItem = Item.BannerToItem(Banner);
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.InModBiome<EngraftBiome>() ? 0.25f : 0f;

		public override void AI()
		{
			NPC.TargetClosest();
			Player target = Main.player[NPC.target];
			if (!target.active || target.dead) return;

			float wobble = (float)System.Math.Sin((Main.GameUpdateCount + NPC.whoAmI * 18) * 0.08f) * 1.3f;
			Vector2 desired = NPC.DirectionTo(target.Center) * 3.6f + new Vector2(0f, wobble);
			NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.07f);
			NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
			NPC.rotation = MathHelper.Clamp(NPC.velocity.Y * 0.08f, -0.3f, 0.3f);
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter += 0.22f;
			NPC.frameCounter %= Main.npcFrameCount[Type];
			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) =>
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MawFibre>(), 2, 1, 2));

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) =>
			bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("It moves like a discarded organ looking for a body."));
	}
}
