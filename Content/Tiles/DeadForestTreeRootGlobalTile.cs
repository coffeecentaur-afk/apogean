using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>
	/// Reserved compatibility type for old worlds that loaded the former root-overlay hook.
	/// It intentionally draws nothing: Wastes trees retain Terraria's ordinary trunk width.
	/// </summary>
	public sealed class DeadForestTreeRootGlobalTile : GlobalTile
	{
	}
}
