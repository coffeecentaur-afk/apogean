using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;
using apogean.Content.Walls;

namespace apogean.Content.World
{
	/// <summary>
	/// A first-class biome conversion lets world generation and runtime spread share the same
	/// source-to-Maw mapping. It also gives future sprayers/bombs one conversion identifier.
	/// </summary>
	public sealed class MawBiomeConversion : ModBiomeConversion
	{
		public override void PostSetupContent() => MawConversionSystem.Register(Type);
	}

	internal static class MawConversionSystem
	{
		public static int ConversionType => ModContent.GetInstance<MawBiomeConversion>().Type;

		internal static void Register(int conversionType)
		{
			int mawDirt = ModContent.TileType<MawDirt>();
			int mawStone = ModContent.TileType<Mawstone>();
			int mawGrass = ModContent.TileType<MawGrass>();
			int mawSand = ModContent.TileType<MawSand>();
			int mawIce = ModContent.TileType<MawIce>();
			int mawSnow = ModContent.TileType<MawSnow>();
			int mawMud = ModContent.TileType<MawMud>();
			int mawClay = ModContent.TileType<MawClay>();

			RegisterTiles(conversionType, mawDirt,
				TileID.Dirt, TileID.Ash, TileID.LivingWood, TileID.LivingMahogany,
				ModContent.TileType<WastesSoil>());
			RegisterTiles(conversionType, mawStone,
				TileID.Stone, TileID.Ebonstone, TileID.Crimstone, TileID.Pearlstone,
				TileID.Granite, TileID.GraniteBlock, TileID.Marble, TileID.MarbleBlock,
				TileID.DesertFossil, TileID.Coralstone, TileID.Mudstone,
				TileID.GreenMoss, TileID.BrownMoss, TileID.RedMoss, TileID.BlueMoss,
				TileID.PurpleMoss, TileID.LongMoss, TileID.LavaMoss,
				TileID.KryptonMoss, TileID.XenonMoss, TileID.ArgonMoss,
				TileID.VioletMoss, TileID.RainbowMoss,
				ModContent.TileType<WastesStone>());
			RegisterTiles(conversionType, mawGrass,
				TileID.Grass, TileID.CorruptGrass, TileID.CrimsonGrass, TileID.HallowedGrass,
				TileID.JungleGrass, TileID.CorruptJungleGrass, TileID.CrimsonJungleGrass,
				TileID.MushroomGrass, TileID.AshGrass,
				TileID.CorruptThorns, TileID.CrimsonThorns, TileID.JungleThorns,
				TileID.LeafBlock, TileID.LivingMahoganyLeaves,
				TileID.GolfGrass, TileID.GolfGrassHallowed,
				ModContent.TileType<DeadGrass>(), ModContent.TileType<WastesGrass>());
			RegisterTiles(conversionType, mawSand,
				TileID.Sand, TileID.Ebonsand, TileID.Crimsand, TileID.Pearlsand,
				TileID.HardenedSand, TileID.CorruptHardenedSand, TileID.CrimsonHardenedSand, TileID.HallowHardenedSand,
				TileID.Sandstone, TileID.SmoothSandstone,
				TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowSandstone,
				ModContent.TileType<WastesSand>());
			RegisterTiles(conversionType, mawIce,
				TileID.IceBlock, TileID.CorruptIce, TileID.FleshIce, TileID.HallowedIce,
				ModContent.TileType<WastesIce>());
			RegisterTiles(conversionType, mawSnow,
				TileID.SnowBlock, TileID.Slush,
				ModContent.TileType<WastesSnow>());
			RegisterTiles(conversionType, mawMud,
				TileID.Mud, TileID.Silt,
				ModContent.TileType<WastesMud>());
			RegisterTiles(conversionType, mawClay, TileID.ClayBlock);

			int mawDirtWall = ModContent.WallType<MawDirtWallUnsafe>();
			int mawStoneWall = ModContent.WallType<MawStoneWallUnsafe>();
			int mawGrassWall = ModContent.WallType<MawGrassWallUnsafe>();
			int mawSandWall = ModContent.WallType<MawSandWallUnsafe>();
			int mawIceWall = ModContent.WallType<MawIceWallUnsafe>();
			int mawSnowWall = ModContent.WallType<MawSnowWallUnsafe>();
			int mawMudWall = ModContent.WallType<MawMudWallUnsafe>();

			RegisterWalls(conversionType, mawDirtWall,
				WallID.DirtUnsafe, WallID.DirtUnsafe1, WallID.DirtUnsafe2, WallID.DirtUnsafe3, WallID.DirtUnsafe4,
				WallID.LivingWoodUnsafe, ModContent.WallType<WastesDirtWallUnsafe>());
			RegisterWalls(conversionType, mawStoneWall,
				WallID.Stone, WallID.EbonstoneUnsafe, WallID.CrimstoneUnsafe, WallID.PearlstoneBrickUnsafe,
				WallID.GraniteUnsafe, WallID.MarbleUnsafe,
				WallID.CaveUnsafe, WallID.Cave2Unsafe, WallID.Cave3Unsafe, WallID.Cave4Unsafe,
				WallID.Cave5Unsafe, WallID.Cave6Unsafe, WallID.Cave7Unsafe, WallID.Cave8Unsafe,
				WallID.CorruptionUnsafe1, WallID.CorruptionUnsafe2, WallID.CorruptionUnsafe3, WallID.CorruptionUnsafe4,
				WallID.CrimsonUnsafe1, WallID.CrimsonUnsafe2, WallID.CrimsonUnsafe3, WallID.CrimsonUnsafe4,
				WallID.HallowUnsafe1, WallID.HallowUnsafe2, WallID.HallowUnsafe3, WallID.HallowUnsafe4,
				ModContent.WallType<WastesStoneWallUnsafe>());
			RegisterWalls(conversionType, mawGrassWall,
				WallID.GrassUnsafe, WallID.FlowerUnsafe,
				WallID.CorruptGrassUnsafe, WallID.CrimsonGrassUnsafe, WallID.HallowedGrassUnsafe,
				WallID.JungleUnsafe, WallID.JungleUnsafe1, WallID.JungleUnsafe2, WallID.JungleUnsafe3, WallID.JungleUnsafe4,
				WallID.LivingLeaf,
				ModContent.WallType<DeadGrassWallUnsafe>(), ModContent.WallType<DeadFlowerWallUnsafe>(),
				ModContent.WallType<WastesGrassWallUnsafe>());
			RegisterWalls(conversionType, mawSandWall,
				WallID.Sandstone, WallID.HardenedSand,
				WallID.CorruptSandstone, WallID.CrimsonSandstone, WallID.HallowSandstone,
				WallID.CorruptHardenedSand, WallID.CrimsonHardenedSand, WallID.HallowHardenedSand,
				ModContent.WallType<WastesSandWallUnsafe>());
			RegisterWalls(conversionType, mawIceWall, WallID.IceUnsafe, ModContent.WallType<WastesIceWallUnsafe>());
			RegisterWalls(conversionType, mawSnowWall, WallID.SnowWallUnsafe, ModContent.WallType<WastesSnowWallUnsafe>());
			RegisterWalls(conversionType, mawMudWall, WallID.MudUnsafe, ModContent.WallType<WastesMudWallUnsafe>());
		}

		private static void RegisterTiles(int conversionType, int target, params int[] sources)
		{
			for (int i = 0; i < sources.Length; i++)
				TileLoader.RegisterSimpleConversion(sources[i], conversionType, target, purification: false);
		}

		private static void RegisterWalls(int conversionType, int target, params int[] sources)
		{
			for (int i = 0; i < sources.Length; i++)
				WallLoader.RegisterSimpleConversion(sources[i], conversionType, target, purification: false);
		}

		public static bool IsConvertibleNaturalTile(ushort type) => type is
			TileID.Dirt or TileID.Ash or TileID.LivingWood or TileID.LivingMahogany or
			TileID.Stone or TileID.Ebonstone or TileID.Crimstone or TileID.Pearlstone or
			TileID.Granite or TileID.GraniteBlock or TileID.Marble or TileID.MarbleBlock or
			TileID.DesertFossil or TileID.Coralstone or TileID.Mudstone or
			TileID.GreenMoss or TileID.BrownMoss or TileID.RedMoss or TileID.BlueMoss or
			TileID.PurpleMoss or TileID.LongMoss or TileID.LavaMoss or TileID.KryptonMoss or
			TileID.XenonMoss or TileID.ArgonMoss or TileID.VioletMoss or TileID.RainbowMoss or
			TileID.Grass or TileID.CorruptGrass or TileID.CrimsonGrass or TileID.HallowedGrass or
			TileID.JungleGrass or TileID.CorruptJungleGrass or TileID.CrimsonJungleGrass or
			TileID.MushroomGrass or TileID.AshGrass or TileID.CorruptThorns or TileID.CrimsonThorns or
			TileID.JungleThorns or TileID.LeafBlock or TileID.LivingMahoganyLeaves or
			TileID.GolfGrass or TileID.GolfGrassHallowed or
			TileID.Sand or TileID.Ebonsand or TileID.Crimsand or TileID.Pearlsand or
			TileID.HardenedSand or TileID.CorruptHardenedSand or TileID.CrimsonHardenedSand or TileID.HallowHardenedSand or
			TileID.Sandstone or TileID.SmoothSandstone or TileID.CorruptSandstone or TileID.CrimsonSandstone or TileID.HallowSandstone or
			TileID.IceBlock or TileID.CorruptIce or TileID.FleshIce or TileID.HallowedIce or
			TileID.SnowBlock or TileID.Slush or TileID.Mud or TileID.Silt or TileID.ClayBlock;

		public static bool IsConvertibleNaturalWall(ushort type) => type is
			WallID.DirtUnsafe or WallID.DirtUnsafe1 or WallID.DirtUnsafe2 or WallID.DirtUnsafe3 or WallID.DirtUnsafe4 or
			WallID.LivingWoodUnsafe or WallID.Stone or WallID.EbonstoneUnsafe or WallID.CrimstoneUnsafe or
			WallID.PearlstoneBrickUnsafe or WallID.GraniteUnsafe or WallID.MarbleUnsafe or
			WallID.CaveUnsafe or WallID.Cave2Unsafe or WallID.Cave3Unsafe or WallID.Cave4Unsafe or
			WallID.Cave5Unsafe or WallID.Cave6Unsafe or WallID.Cave7Unsafe or WallID.Cave8Unsafe or
			WallID.CorruptionUnsafe1 or WallID.CorruptionUnsafe2 or WallID.CorruptionUnsafe3 or WallID.CorruptionUnsafe4 or
			WallID.CrimsonUnsafe1 or WallID.CrimsonUnsafe2 or WallID.CrimsonUnsafe3 or WallID.CrimsonUnsafe4 or
			WallID.HallowUnsafe1 or WallID.HallowUnsafe2 or WallID.HallowUnsafe3 or WallID.HallowUnsafe4 or
			WallID.GrassUnsafe or WallID.FlowerUnsafe or WallID.CorruptGrassUnsafe or WallID.CrimsonGrassUnsafe or
			WallID.HallowedGrassUnsafe or WallID.JungleUnsafe or WallID.JungleUnsafe1 or WallID.JungleUnsafe2 or
			WallID.JungleUnsafe3 or WallID.JungleUnsafe4 or WallID.LivingLeaf or
			WallID.Sandstone or WallID.HardenedSand or WallID.CorruptSandstone or WallID.CrimsonSandstone or
			WallID.HallowSandstone or WallID.CorruptHardenedSand or WallID.CrimsonHardenedSand or WallID.HallowHardenedSand or
			WallID.IceUnsafe or WallID.SnowWallUnsafe or WallID.MudUnsafe ||
			type == ModContent.WallType<WastesDirtWallUnsafe>() ||
			type == ModContent.WallType<WastesStoneWallUnsafe>() ||
			type == ModContent.WallType<WastesGrassWallUnsafe>() ||
			type == ModContent.WallType<WastesSandWallUnsafe>() ||
			type == ModContent.WallType<WastesIceWallUnsafe>() ||
			type == ModContent.WallType<WastesSnowWallUnsafe>() ||
			type == ModContent.WallType<WastesMudWallUnsafe>();

		public static bool ConvertAt(int x, int y, bool convertTile, bool convertWall)
		{
			bool changed = false;
			if (convertTile && Framing.GetTileSafely(x, y).HasTile)
			{
				ushort before = Framing.GetTileSafely(x, y).TileType;
				TileLoader.Convert(x, y, ConversionType);
				changed |= Framing.GetTileSafely(x, y).TileType != before;
			}
			if (convertWall && Framing.GetTileSafely(x, y).WallType != WallID.None)
			{
				ushort before = Framing.GetTileSafely(x, y).WallType;
				WallLoader.Convert(x, y, ConversionType);
				changed |= Framing.GetTileSafely(x, y).WallType != before;
			}
			if (changed)
			{
				WorldGen.SquareTileFrame(x, y, true);
				WorldGen.SquareWallFrame(x, y, true);
			}
			return changed;
		}

		public static bool IsMawTerrain(ushort type) => type == ModContent.TileType<MawDirt>() ||
			type == ModContent.TileType<Mawstone>() ||
			type == ModContent.TileType<MawGrass>() ||
			type == ModContent.TileType<MawSand>() ||
			type == ModContent.TileType<MawIce>() ||
			type == ModContent.TileType<MawSnow>() ||
			type == ModContent.TileType<MawMud>() ||
			type == ModContent.TileType<MawClay>() ||
			type == ModContent.TileType<EngraftTurf>() ||
			type == ModContent.TileType<OssuaryBone>();
	}
}
