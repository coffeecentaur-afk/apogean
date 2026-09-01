using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Walls
{
	/// <summary>World-generated dead turf wall. It is intentionally unsafe for housing.</summary>
	public sealed class DeadGrassWallUnsafe : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			WallID.Sets.Conversion.Grass[Type] = true;
			DustType = DustID.Dirt;
			AddMapEntry(new Color(83, 62, 37));
		}
	}
}
