using System.Collections.Generic;
using Terraria;

namespace apogean.Content.Dialogue
{
	/// <summary>
	/// Implemented by any ModNPC that should use Apogean's branching dialogue UI instead of
	/// vanilla chat. NPCs that don't implement this - vanilla town NPCs, any other mod's
	/// NPCs - are left completely untouched by this system.
	/// </summary>
	public interface IDialogueNPC
	{
		/// <summary>
		/// Most dialogue actors suppress vanilla chat. Merchant-like actors may retain right-click
		/// chat/shop while left-click continues to open the branching Apogean conversation.
		/// </summary>
		bool SupportsVanillaChat => false;

		/// <summary>Built once and cached by DialogueSystem; keyed by DialogueNode.Id.</summary>
		Dictionary<string, DialogueNode> BuildTree();

		/// <summary>Lets the same NPC open on a different node depending on quest/job state.</summary>
		string GetRootNodeId(Player player);
	}
}
