using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Factions;
using apogean.Content.Items.Consumables;
using apogean.Content.Items.Materials;
using apogean.Content.Items.Weapons;
using apogean.Content.NPCs.Broodmass;
using apogean.Content.NPCs.Debug;
using apogean.Content.World;

namespace apogean.Content.Commands
{
	/// <summary>
	/// Playtest helpers so testing a fight doesn't require farming its summon materials.
	/// Strip this before any public release.
	/// </summary>
	public class ApogeanDebugCommand : ModCommand
	{
		private static readonly Color Info = new(150, 90, 170);

		public override CommandType Type => CommandType.Chat;
		public override string Command => "apogean";
		public override string Usage => "/apogean <matriarch|lure|gland|engraft|kit|npc|flags|clear>";
		public override string Description => "Apogean playtest helpers";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = caller.Player;
			string sub = args.Length > 0 ? args[0].ToLowerInvariant() : "help";
			int count = args.Length > 1 && int.TryParse(args[1], out int n) ? n : 1;

			switch (sub)
			{
				case "matriarch":
					if (NPC.AnyNPCs(ModContent.NPCType<Matriarch>()))
					{
						caller.Reply("The Matriarch is already active.", Color.Orange);
						break;
					}
					NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<Matriarch>());
					caller.Reply("Matriarch spawned.", Info);
					break;

				case "lure":
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"),
						ModContent.ItemType<PheromoneLure>(), count);
					caller.Reply($"Gave {count}x Pheromone Lure.", Info);
					break;

				case "gland":
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"),
						ModContent.ItemType<MutagenGland>(), count);
					caller.Reply($"Gave {count}x Mutagen Gland.", Info);
					break;

				case "engraft":
					EngraftSystem.Instance.CreateDebugRupture(player);
					caller.Reply("Created a playtest Maw Rupture at the local surface.", new Color(194, 126, 44));
					break;

				case "kit":
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<RendHook>());
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<SinewBow>());
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<AmberSiphon>());
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<MawEffigy>());
					caller.Reply("Gave the Act 1 Engraft playtest weapon set.", new Color(194, 126, 44));
					break;

				case "npc":
					NPC.NewNPC(player.GetSource_Misc("ApogeanDebug"),
						(int)player.Center.X + 80, (int)player.Center.Y,
						ModContent.NPCType<TestAmbassador>());
					caller.Reply("Spawned the dialogue test NPC. Click it.", Info);
					break;

				case "flags":
					FactionProgression progression = ModContent.GetInstance<FactionProgression>();
					caller.Reply($"Matriarch downed: {progression.MatriarchDowned}", Info);
					foreach (ApogeanFaction faction in FactionProgression.CorpFactions)
					{
						FactionInfo info = FactionInfo.Get(faction);
						caller.Reply($"  {info.DisplayName}: {progression.GetRelation(faction)}", info.Color);
					}
					caller.Reply($"Alliance: {progression.ChosenAlliance?.ToString() ?? "none"}", Info);
					break;

				case "clear":
					int cleared = 0;
					foreach (NPC npc in Main.npc)
					{
						if (npc.active && npc.ModNPC?.Mod == Mod) { npc.active = false; cleared++; }
					}
					caller.Reply($"Cleared {cleared} Apogean NPCs.", Info);
					break;

				default:
					caller.Reply(Usage, Color.Yellow);
					break;
			}
		}
	}
}
