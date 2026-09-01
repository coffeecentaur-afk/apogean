using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>
	/// Seals a corp compound before its faction has arrived - visible from outside, immune to
	/// mining/explosions, and swapped out programmatically (never mined by the player) once
	/// that faction's arrival invasion is cleared. Placeholder texture until real art exists.
	/// </summary>
	public class LockedBulkhead : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileNoAttach[Type] = true;
			MineResist = 100f;
			MinPick = 9999;
			DustType = DustID.Titanium;
			AddMapEntry(new Color(120, 120, 140));
		}

		public override bool CanExplode(int i, int j) => false;

		// Only ever removed by CompoundGen's unseal/re-arm pass, never by the player.
		public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
	}
}
