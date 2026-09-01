using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Biomes;
using apogean.Content.NPCs.Broodmass;

namespace apogean.Content.Items.Consumables
{
	/// <summary>Summons the Matriarch. Crafted from her own brood's glands - she comes to reclaim them.</summary>
	public class PheromoneLure : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 30;
			Item.maxStack = 20;
			Item.value = Item.buyPrice(silver: 50);
			Item.rare = ItemRarityID.Orange;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = true;
		}

		// Provisional recipe - retune once overworld Broodmass enemies exist to drop their own reagent.
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.JungleSpores, 10)
				.AddIngredient(ItemID.Stinger, 5)
				.AddTile(TileID.DemonAltar)
				.Register();
		}

		// The Matriarch is a regional growth node, not a boss that can be dragged through a town.
		// The debug command can still force a playtest spawn, but normal progression has to happen
		// in the Engraft's player-built Maw Ruptures.
		public override bool CanUseItem(Player player) =>
			!NPC.AnyNPCs(ModContent.NPCType<Matriarch>()) && player.InModBiome<EngraftBiome>();

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI != Main.myPlayer) return null;

			SoundEngine.PlaySound(SoundID.Roar, player.position);

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<Matriarch>());
			}
			else
			{
				NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI,
					number2: ModContent.NPCType<Matriarch>());
			}

			return true;
		}
	}
}
