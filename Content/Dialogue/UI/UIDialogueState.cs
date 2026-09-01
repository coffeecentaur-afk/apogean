using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using apogean.Content.Factions;

namespace apogean.Content.Dialogue.UI
{
	/// <summary>
	/// Speaker header, body text, and a scrollable list of choice buttons - replaces vanilla
	/// NPC chat for any IDialogueNPC. No typewriter/animation in this first pass.
	/// </summary>
	public class UIDialogueState : UIState
	{
		private UIPanel headerPanel;
		private UIText speakerText;
		private UIText bodyText;
		private UIList optionList;

		public override void OnInitialize()
		{
			UIElement window = new()
			{
				Width = { Percent = 0.6f },
				MaxWidth = new StyleDimension(700f, 0f),
				Height = new StyleDimension(260f, 0f),
				HAlign = 0.5f,
				VAlign = 1f,
				Top = new StyleDimension(-50f, 0f),
			};
			Append(window);

			headerPanel = new UIPanel
			{
				Width = { Percent = 1f },
				Height = new StyleDimension(36f, 0f),
			};
			window.Append(headerPanel);

			speakerText = new UIText(string.Empty)
			{
				HAlign = 0f,
				VAlign = 0.5f,
			};
			headerPanel.Append(speakerText);

			UIPanel bodyPanel = new()
			{
				Width = { Percent = 1f },
				Height = new StyleDimension(90f, 0f),
				Top = new StyleDimension(44f, 0f),
			};
			window.Append(bodyPanel);

			bodyText = new UIText(string.Empty, 0.9f)
			{
				Width = { Percent = 1f },
				TextOriginX = 0f,
			};
			bodyText.IsWrapped = true;
			bodyPanel.Append(bodyText);

			optionList = new UIList
			{
				Width = { Percent = 1f },
				Height = new StyleDimension(120f, 0f),
				Top = new StyleDimension(142f, 0f),
			};
			window.Append(optionList);
		}

		public void Refresh(DialogueSystem dialogueSystem)
		{
			DialogueNode node = dialogueSystem.CurrentNode;
			optionList.Clear();

			if (node == null)
			{
				speakerText.SetText(string.Empty);
				bodyText.SetText(string.Empty);
				return;
			}

			speakerText.SetText(Language.GetTextValue(node.SpeakerKey));
			bodyText.SetText(Language.GetTextValue(node.TextKey));

			FactionInfo speakerFaction = ResolveSpeakerFaction(dialogueSystem.ActiveNPC);
			if (speakerFaction != null)
			{
				headerPanel.BackgroundColor = speakerFaction.Color * 0.6f;
			}

			Player player = Main.LocalPlayer;
			List<DialogueOption> options = dialogueSystem.GetAvailableOptions(player);
			for (int i = 0; i < options.Count; i++)
			{
				int index = i;
				UITextPanel<string> button = new(Language.GetTextValue(options[i].TextKey), 0.8f)
				{
					Width = { Percent = 1f },
					Height = new StyleDimension(30f, 0f),
				};
				button.OnLeftClick += (_, _) => dialogueSystem.Advance(index, Main.LocalPlayer);
				optionList.Add(button);
			}
		}

		private static FactionInfo ResolveSpeakerFaction(NPC npc)
		{
			return npc?.ModNPC is IFactionEntity factionEntity ? FactionInfo.Get(factionEntity.Faction) : null;
		}
	}
}
