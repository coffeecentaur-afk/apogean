using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Walls
{
	/// <summary>Unsafe neutral walls for the dead Wastes. A second purity pass restores vanilla terrain.</summary>
	public abstract class WastesNaturalWall : ModWall
	{
		protected abstract Color MapColor { get; }
		protected abstract int RestoredWall { get; }

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			DustType = DustID.Dirt;
			AddMapEntry(MapColor);
		}

		public override void Convert(int i, int j, int conversionType)
		{
			if (conversionType is BiomeConversionID.Purity or BiomeConversionID.PurificationPowder)
				WorldGen.ConvertWall(i, j, RestoredWall);
		}
	}

	public sealed class WastesDirtWallUnsafe : WastesNaturalWall { protected override Color MapColor => new(80, 63, 49); protected override int RestoredWall => WallID.DirtUnsafe; }
	public sealed class WastesStoneWallUnsafe : WastesNaturalWall { protected override Color MapColor => new(79, 73, 65); protected override int RestoredWall => WallID.Stone; }
	public sealed class WastesGrassWallUnsafe : WastesNaturalWall { protected override Color MapColor => new(105, 78, 44); protected override int RestoredWall => WallID.GrassUnsafe; }
	public sealed class WastesSandWallUnsafe : WastesNaturalWall { protected override Color MapColor => new(124, 103, 68); protected override int RestoredWall => WallID.Sandstone; }
	public sealed class WastesIceWallUnsafe : WastesNaturalWall { protected override Color MapColor => new(91, 101, 101); protected override int RestoredWall => WallID.IceUnsafe; }
	public sealed class WastesSnowWallUnsafe : WastesNaturalWall { protected override Color MapColor => new(141, 136, 122); protected override int RestoredWall => WallID.SnowWallUnsafe; }
	public sealed class WastesMudWallUnsafe : WastesNaturalWall { protected override Color MapColor => new(67, 58, 43); protected override int RestoredWall => WallID.MudUnsafe; }
}
