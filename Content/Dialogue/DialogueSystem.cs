using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using apogean.Content.Dialogue.UI;

namespace apogean.Content.Dialogue
{
	/// <summary>
	/// Holds the single active conversation and drives the custom dialogue UI. Opened by
	/// DialoguePlayer's click detection, populated by an IDialogueNPC's tree.
	/// </summary>
	public class DialogueSystem : ModSystem
	{
		private UserInterface userInterface;
		private UIDialogueState dialogueState;

		public NPC ActiveNPC { get; private set; }
		public Dictionary<string, DialogueNode> ActiveTree { get; private set; }
		public DialogueNode CurrentNode { get; private set; }

		public bool IsOpen => ActiveNPC != null;

		public override void Load()
		{
			if (Main.dedServ) return;

			dialogueState = new UIDialogueState();
			dialogueState.Activate();
			userInterface = new UserInterface();
		}

		public override void Unload()
		{
			userInterface = null;
			dialogueState = null;
		}

		public void Open(NPC npc, Player player)
		{
			if (npc.ModNPC is not IDialogueNPC dialogueNpc) return;

			ActiveTree = dialogueNpc.BuildTree();
			string rootId = dialogueNpc.GetRootNodeId(player);
			if (!ActiveTree.TryGetValue(rootId, out DialogueNode root)) return;

			ActiveNPC = npc;
			CurrentNode = root;
			dialogueState.Refresh(this);
			userInterface.SetState(dialogueState);
			Main.playerInventory = false;
		}

		public void Advance(int optionIndex, Player player)
		{
			if (CurrentNode == null) return;

			List<DialogueOption> available = GetAvailableOptions(player);
			if (optionIndex < 0 || optionIndex >= available.Count) return;

			DialogueOption chosen = available[optionIndex];
			chosen.OnSelect?.Invoke(player);

			if (chosen.TargetNodeId == null || !ActiveTree.TryGetValue(chosen.TargetNodeId, out DialogueNode next))
			{
				Close();
				return;
			}

			CurrentNode = next;
			dialogueState.Refresh(this);
		}

		public List<DialogueOption> GetAvailableOptions(Player player)
		{
			List<DialogueOption> available = new();
			if (CurrentNode == null) return available;

			foreach (DialogueOption option in CurrentNode.Options)
			{
				if (option.IsAvailable(player))
				{
					available.Add(option);
				}
			}

			return available;
		}

		public void Close()
		{
			ActiveNPC = null;
			ActiveTree = null;
			CurrentNode = null;
			userInterface?.SetState(null);
		}

		public override void UpdateUI(GameTime gameTime)
		{
			if (IsOpen)
			{
				userInterface?.Update(gameTime);
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int chatIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: NPC Chat"));
			if (chatIndex == -1) return;

			layers.Insert(chatIndex + 1, new LegacyGameInterfaceLayer(
				"Apogean: Dialogue",
				delegate
				{
					if (IsOpen)
					{
						userInterface?.Draw(Main.spriteBatch, new GameTime());
					}

					return true;
				},
				InterfaceScaleType.UI));
		}
	}
}
