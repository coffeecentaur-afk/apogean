using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Walls
{
	/// <summary>Deep structural membrane used inside the Gullet and Stomach.</summary>
	public sealed class MawWallUnsafe : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			DustType = DustID.Dirt;
			AddMapEntry(new Color(68, 55, 31));
		}
	}
}
