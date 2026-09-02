using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Walls
{
	public abstract class MawNaturalWall : ModWall
	{
		protected abstract Color MapColor { get; }
		protected abstract int PurifiedWall { get; }

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			DustType = DustID.AmberBolt;
			AddMapEntry(MapColor);
		}

		public override void Convert(int i, int j, int conversionType)
		{
			if (conversionType is BiomeConversionID.Purity or BiomeConversionID.PurificationPowder)
				WorldGen.ConvertWall(i, j, PurifiedWall);
		}
    }

	public sealed class MawDirtWallUnsafe : MawNaturalWall { protected override Color MapColor => new(72, 54, 35); protected override int PurifiedWall => ModContent.WallType<WastesDirtWallUnsafe>(); }
	public sealed class MawStoneWallUnsafe : MawNaturalWall { protected override Color MapColor => new(64, 57, 45); protected override int PurifiedWall => ModContent.WallType<WastesStoneWallUnsafe>(); }
	public sealed class MawGrassWallUnsafe : MawNaturalWall { protected override Color MapColor => new(96, 68, 31); protected override int PurifiedWall => ModContent.WallType<WastesGrassWallUnsafe>(); }
	public sealed class MawSandWallUnsafe : MawNaturalWall { protected override Color MapColor => new(112, 83, 40); protected override int PurifiedWall => ModContent.WallType<WastesSandWallUnsafe>(); }
	public sealed class MawIceWallUnsafe : MawNaturalWall { protected override Color MapColor => new(83, 85, 67); protected override int PurifiedWall => ModContent.WallType<WastesIceWallUnsafe>(); }
	public sealed class MawSnowWallUnsafe : MawNaturalWall { protected override Color MapColor => new(126, 113, 84); protected override int PurifiedWall => ModContent.WallType<WastesSnowWallUnsafe>(); }
	public sealed class MawMudWallUnsafe : MawNaturalWall { protected override Color MapColor => new(58, 49, 30); protected override int PurifiedWall => ModContent.WallType<WastesMudWallUnsafe>(); }
}
