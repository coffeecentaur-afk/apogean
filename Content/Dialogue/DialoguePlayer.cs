using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace apogean.Content.Dialogue
{
	/// <summary>
	/// Detects left-clicks on IDialogueNPC NPCs and opens our custom dialogue UI, standing in
	/// for the vanilla town-NPC talk trigger that DialogueGlobalNPC.CanChat disables for them.
	/// </summary>
	public class DialoguePlayer : ModPlayer
	{
		private const float InteractionRange = 16f * 6f;

		public override void PostUpdate()
		{
			if (Player.whoAmI != Main.myPlayer) return;

			DialogueSystem dialogueSystem = ModContent.GetInstance<DialogueSystem>();
			if (dialogueSystem.IsOpen) return;
			if (Player.talkNPC != -1 || Player.sign != -1 || Player.dead) return;
			if (!Main.mouseLeft || !Main.mouseLeftRelease) return;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.active || npc.ModNPC is not IDialogueNPC) continue;
				if (Player.Distance(npc.Center) > InteractionRange) continue;

				Rectangle hitbox = new((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
				if (!hitbox.Contains((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y)) continue;

				dialogueSystem.Open(npc, Player);
				Main.mouseLeftRelease = false;
				break;
			}
		}
	}
}
