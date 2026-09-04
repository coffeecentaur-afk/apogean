using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using apogean.Content.Dialogue;
using apogean.Content.Factions;

namespace apogean.Content.NPCs.Kessler
{
	/// <summary>
	/// Fixed service-post representative. Left-click opens the narrative tree; right-click uses
	/// Terraria's stable NPC chat/shop path so requisitions remain compatible with other mods.
	/// </summary>
	public sealed class KesslerQuartermaster : ModNPC, IDialogueNPC, IFactionEntity
	{
		public const string ShopName = "Requisitions";
		private const string Prefix = "Mods.apogean.Kessler.Dialogue.Quartermaster.";

		public override string Texture => $"Terraria/Images/NPC_{NPCID.ArmsDealer}";
		public ApogeanFaction Faction => ApogeanFaction.Kessler;
		public bool SupportsVanillaChat => true;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.ArmsDealer];
			AnimationType = NPCID.ArmsDealer;
		}

		public override void SetDefaults()
		{
			NPC.width = 18;
			NPC.height = 40;
			NPC.lifeMax = 500;
			NPC.defense = 24;
			NPC.damage = 0;
			NPC.friendly = true;
			NPC.townNPC = false;
			NPC.dontTakeDamage = true;
			NPC.knockBackResist = 0f;
			NPC.aiStyle = -1;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.netAlways = true;
		}

		public override void AI()
		{
			NPC.velocity.X = 0f;
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (Content.Structures.CompoundGen.TryGetPublicPost(ApogeanFaction.Kessler, out Point post))
			{
				float targetX = post.X * 16f + 8f;
				if (System.Math.Abs(NPC.Center.X - targetX) > 3f)
				{
					NPC.Center = new Vector2(targetX, NPC.Center.Y);
					NPC.netUpdate = true;
				}
			}
		}

		public override bool CheckActive() => false;

		public override bool CanChat() =>
			ModContent.GetInstance<FactionProgression>().GetRelation(Faction) is FactionRelation.Contactable or FactionRelation.Allied;

		public override string GetChat() => Language.GetTextValue(Prefix + "VanillaChat");

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Language.GetTextValue(Prefix + "RequisitionsButton");
			button2 = Language.GetTextValue(Prefix + "BriefingButton");
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop)
		{
			if (firstButton)
			{
				shop = ShopName;
				return;
			}

			ModContent.GetInstance<DialogueSystem>().RequestOpen(NPC);
		}

		public override void AddShops()
		{
			NPCShop shop = new NPCShop(Type, ShopName)
				.Add(new Item(ItemID.HealingPotion) { shopCustomPrice = Item.buyPrice(silver: 3) })
				.Add(new Item(ItemID.RecallPotion) { shopCustomPrice = Item.buyPrice(silver: 1) })
				.Add(new Item(ItemID.MusketBall) { shopCustomPrice = Item.buyPrice(copper: 8) })
				.Add(new Item(ItemID.WoodenArrow) { shopCustomPrice = Item.buyPrice(copper: 8) })
				.Add(new Item(ItemID.Grenade) { shopCustomPrice = Item.buyPrice(silver: 1, copper: 50) })
				.Add(ScripPrice(ItemID.ObsidianSkinPotion, 2))
				.Add(ScripPrice(ItemID.AmmoBox, 6))
				.Add(ScripPrice(ItemID.SharpeningStation, 6))
				.Add(ScripPrice(ItemID.CrystalBall, 6))
				.Add(ScripPrice(ItemID.BewitchingTable, 6));
			shop.Register();
		}

		public string GetRootNodeId(Player player)
		{
			return ModContent.GetInstance<FactionProgression>().GetRelation(Faction) switch
			{
				FactionRelation.Allied => "allied",
				FactionRelation.Enemy => "enemy",
				_ => "root"
			};
		}

		public Dictionary<string, DialogueNode> BuildTree()
		{
			return new Dictionary<string, DialogueNode>
			{
				["root"] = new DialogueNode("root", Prefix + "Speaker", Prefix + "Root", new List<DialogueOption>
				{
					new(Prefix + "OptionIdentity", "identity"),
					new(Prefix + "OptionAssessment", "assessment"),
					new(Prefix + "OptionScrip", "scrip"),
					new(Prefix + "OptionLeave")
				}),
				["identity"] = Node("identity", "Identity"),
				["assessment"] = Node("assessment", "Assessment"),
				["scrip"] = Node("scrip", "Scrip"),
				["allied"] = Node("allied", "Allied"),
				["enemy"] = new DialogueNode("enemy", Prefix + "Speaker", Prefix + "Enemy", new List<DialogueOption>
				{
					new(Prefix + "OptionLeave")
				})
			};
		}

		private static DialogueNode Node(string id, string textKey) => new(
			id,
			Prefix + "Speaker",
			Prefix + textKey,
			new List<DialogueOption> { new(Prefix + "OptionBack", "root") });

		private static Item ScripPrice(int itemType, int price) => new(itemType)
		{
			shopCustomPrice = price,
			shopSpecialCurrency = global::apogean.Apogean.KesslerScripCurrencyId
		};
	}
}
