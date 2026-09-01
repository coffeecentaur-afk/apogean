using Terraria;
using Terraria.ModLoader;

namespace apogean.Content.Dialogue
{
	/// <summary>
	/// Blocks vanilla chat only for NPCs that opt into our dialogue tree via IDialogueNPC.
	/// Everything else keeps its normal chat/shop behavior untouched.
	/// </summary>
	public class DialogueGlobalNPC : GlobalNPC
	{
		public override bool? CanChat(NPC npc) => npc.ModNPC is IDialogueNPC ? false : null;
	}
}
