using System.Collections.Generic;

namespace apogean.Content.Dialogue
{
	public class DialogueNode
	{
		public string Id { get; }
		public string SpeakerKey { get; }
		public string TextKey { get; }
		public List<DialogueOption> Options { get; }

		public DialogueNode(string id, string speakerKey, string textKey, List<DialogueOption> options)
		{
			Id = id;
			SpeakerKey = speakerKey;
			TextKey = textKey;
			Options = options;
		}
	}
}
