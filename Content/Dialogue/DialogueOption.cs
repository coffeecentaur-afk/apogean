using System;
using Terraria;

namespace apogean.Content.Dialogue
{
	public class DialogueOption
	{
		public string TextKey { get; }
		public string TargetNodeId { get; }
		public Func<Player, bool> Condition { get; }
		public Action<Player> OnSelect { get; }

		public DialogueOption(string textKey, string targetNodeId = null, Func<Player, bool> condition = null, Action<Player> onSelect = null)
		{
			TextKey = textKey;
			TargetNodeId = targetNodeId;
			Condition = condition;
			OnSelect = onSelect;
		}

		public bool IsAvailable(Player player) => Condition == null || Condition(player);
	}
}
