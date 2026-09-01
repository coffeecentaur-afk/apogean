using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Walls
{
	/// <summary>World-generated withered flower wall. It is intentionally unsafe for housing.</summary>
	public sealed class DeadFlowerWallUnsafe : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			WallID.Sets.Conversion.Grass[Type] = true;
			DustType = DustID.Dirt;
			AddMapEntry(new Color(102, 73, 38));
		}
	}
}
