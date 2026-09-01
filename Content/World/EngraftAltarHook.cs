using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.World
{
	/// <summary>Hardmode altar destruction seeds a bounded number of new Engraft outgrowths alongside the vanilla evil/Hallow consequence.</summary>
	public sealed class EngraftAltarHook : GlobalTile
	{
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly || Main.netMode == NetmodeID.MultiplayerClient) return;
			// Vanilla uses DemonAltar's tile type for both evil altar variants; their frame selects the look.
			if (type == TileID.DemonAltar)
			{
				EngraftSystem.Instance.SeedFromDestroyedAltar();
			}
		}
	}
}
