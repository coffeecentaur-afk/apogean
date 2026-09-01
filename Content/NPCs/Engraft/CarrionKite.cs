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
	/// <summary>A bird-shaped carrier that makes the Engraft sky unsafe without becoming an upside-down homing missile.</summary>
	public sealed class CarrionKite : ModNPC, IFactionEntity
	{
		public ApogeanFaction Faction => ApogeanFaction.Broodmass;

		public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 4;

		public override void SetDefaults()
		{
			NPC.width = 22;
			NPC.height = 18;
			NPC.damage = 19;
			NPC.defense = 3;
			NPC.lifeMax = 58;
			NPC.knockBackResist = 0.55f;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit5 with { Pitch = -0.45f };
			NPC.DeathSound = SoundID.NPCDeath1 with { Pitch = 0.1f };
			Banner = Item.NPCtoBanner(Type);
			BannerItem = Item.BannerToItem(Banner);
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) =>
			spawnInfo.Player.InModBiome<EngraftBiome>() && spawnInfo.Player.ZoneOverworldHeight ? 0.08f : 0f;

		public override void AI()
		{
			NPC.TargetClosest();
			Player target = Main.player[NPC.target];
			if (!target.active || target.dead) return;

			Vector2 offset = new(target.Center.X + (NPC.whoAmI % 2 == 0 ? 160f : -160f), target.Center.Y - 110f);
			float wave = (float)System.Math.Sin((Main.GameUpdateCount + NPC.whoAmI * 27) * 0.035f) * 64f;
			offset.X += wave;
			Vector2 desired = NPC.DirectionTo(offset) * 5.2f;
			NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.045f);
			NPC.spriteDirection = NPC.direction = NPC.velocity.X >= 0f ? 1 : -1;
			NPC.rotation = MathHelper.Lerp(NPC.rotation, MathHelper.Clamp(NPC.velocity.Y * 0.045f, -0.24f, 0.24f) * NPC.direction, 0.12f);
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter += 0.18f;
			NPC.frameCounter %= Main.npcFrameCount[Type];
			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) =>
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MawFibre>(), 1, 1, 3));

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) =>
			bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("The silhouette is still a bird until it opens its beak."));
	}
}
