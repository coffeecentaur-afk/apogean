using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Walls
{
	/// <summary>Placeholder unsafe wall used to make the generated gullet read as one biome volume.</summary>
	public sealed class MawWallUnsafe : ModWall
	{
		public override string Texture => "apogean/Content/Walls/DeadGrassWallUnsafe";

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			DustType = DustID.Dirt;
			AddMapEntry(new Color(68, 55, 31));
		}
	}
}
