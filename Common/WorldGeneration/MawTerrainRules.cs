using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;
using apogean.Content.World;

namespace apogean.Common.WorldGeneration
{
	internal static class MawTerrainRules
	{
		public static bool CanConvert(int x, int y, WorldEditIntent intent)
		{
			if (!ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, intent))
				return false;

			Tile tile = Framing.GetTileSafely(x, y);
			return tile.HasTile && IsNaturalTerrain(tile.TileType) && !HasProtectedWall(tile);
		}

		public static bool CanConvertWall(int x, int y, WorldEditIntent intent)
		{
			if (!ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, intent))
				return false;

			Tile tile = Framing.GetTileSafely(x, y);
			return tile.WallType != WallID.None && IsNaturalWall(tile.WallType) && !HasNearbyFrameImportantTile(x, y, 1);
		}

		public static bool CanCarve(int x, int y, WorldEditIntent intent)
		{
			if (!ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, intent))
				return false;

			Tile tile = Framing.GetTileSafely(x, y);
			if (IsProtectedStructureObstacle(x, y))
				return false;
			if (!tile.HasTile || IsNaturalTerrain(tile.TileType) || IsMawStructure(tile.TileType) ||
				CanReplaceTileType(tile.TileType) || IsNaturalCaveClutter(tile.TileType))
				return true;

			// During world creation, loose cave clutter must not turn into route-blocking islands.
			// Structure walls, containers, mod tiles, shimmer, and critical vanilla structure tiles
			// were rejected above, so clearing the remaining vanilla object is safe.
			return WorldGen.generatingWorld && tile.TileType < TileID.Count;
		}

		public static bool CanPlaceShell(int x, int y, WorldEditIntent intent)
		{
			if (!ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, intent))
				return false;

			Tile tile = Framing.GetTileSafely(x, y);
			if (HasProtectedWall(tile) || HasNearbyFrameImportantTile(x, y, 1))
				return false;
			return !tile.HasTile || IsNaturalTerrain(tile.TileType);
		}

		internal static bool IsHardGenerationObstacle(int x, int y)
		{
			return IsProtectedStructureObstacle(x, y);
		}

		internal static bool IsProtectedStructureObstacle(int x, int y)
		{
			if (!WorldGen.InWorld(x, y, 2))
				return true;

			Tile tile = Framing.GetTileSafely(x, y);
			if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Shimmer)
				return true;
			if (HasProtectedWall(tile))
				return true;
			if (!tile.HasTile)
				return false;
			if (Main.tileContainer[tile.TileType])
				return true;
			if (IsMawStructure(tile.TileType))
				return false;
			if (tile.TileType >= TileID.Count)
				return true;

			return IsProtectedVanillaStructureTile(tile.TileType);
		}

		internal static bool CanReplaceTileType(ushort type)
		{
			if (type < TileID.Sets.Ore.Length && TileID.Sets.Ore[type])
				return true;
			if (type < TileID.Sets.IsATreeTrunk.Length && TileID.Sets.IsATreeTrunk[type])
				return true;

			return IsNaturalTerrain(type) || type is
				TileID.Silt or
				TileID.Slush or
				TileID.DesertFossil or
				TileID.Plants or
				TileID.Plants2 or
				TileID.Vines or
				TileID.VineFlowers or
				TileID.Saplings or
				TileID.Cactus;
		}

		private static bool HasProtectedWall(Tile tile)
		{
			if (tile.WallType <= WallID.None)
				return false;
			if (tile.WallType < Main.wallHouse.Length && Main.wallHouse[tile.WallType])
				return true;
			if (tile.WallType < Main.wallDungeon.Length && Main.wallDungeon[tile.WallType])
				return true;
			return tile.WallType is
				WallID.LihzahrdBrickUnsafe or
				WallID.HiveUnsafe or
				WallID.HellstoneBrickUnsafe or
				WallID.ObsidianBrickUnsafe or
				WallID.ObsidianBackUnsafe;
		}

		private static bool HasNearbyFrameImportantTile(int centerX, int centerY, int radius)
		{
			for (int x = centerX - radius; x <= centerX + radius; x++)
			{
				for (int y = centerY - radius; y <= centerY + radius; y++)
				{
					if (!WorldGen.InWorld(x, y, 2))
						continue;
					Tile nearby = Framing.GetTileSafely(x, y);
					if (nearby.HasTile && Main.tileFrameImportant[nearby.TileType])
						return true;
				}
			}

			return false;
		}

		private static bool IsProtectedVanillaStructureTile(ushort type) => type is
			TileID.BlueDungeonBrick or
			TileID.GreenDungeonBrick or
			TileID.PinkDungeonBrick or
			TileID.LihzahrdBrick or
			TileID.Hive;

		internal static bool IsNaturalTerrain(ushort type) =>
			MawConversionSystem.IsConvertibleNaturalTile(type) ||
			MawConversionSystem.IsMawTerrain(type) ||
			type == ModContent.TileType<WastesSoil>() ||
			type == ModContent.TileType<WastesStone>() ||
			type == ModContent.TileType<WastesGrass>() ||
			type == ModContent.TileType<WastesSand>() ||
			type == ModContent.TileType<WastesIce>() ||
			type == ModContent.TileType<WastesSnow>() ||
			type == ModContent.TileType<WastesMud>() ||
			type == ModContent.TileType<DeadGrass>();

		private static bool IsNaturalWall(ushort type) => MawConversionSystem.IsConvertibleNaturalWall(type);

		private static bool IsNaturalCaveClutter(ushort type) => type is
			TileID.BreakableIce or
			TileID.Pots or
			TileID.PotsSuspended or
			TileID.PotsEcho or
			TileID.Stalactite or
			TileID.ExposedGems or
			TileID.GreenMoss or
			TileID.BrownMoss or
			TileID.RedMoss or
			TileID.BlueMoss or
			TileID.PurpleMoss or
			TileID.LongMoss or
			TileID.LavaMoss or
			TileID.KryptonMoss or
			TileID.XenonMoss or
			TileID.ArgonMoss or
			TileID.VioletMoss or
			TileID.RainbowMoss;

		private static bool IsMawStructure(ushort type) =>
			MawConversionSystem.IsMawTerrain(type) ||
			type == ModContent.TileType<EngraftTuft>() ||
			type == ModContent.TileType<MawNode>() ||
			type == ModContent.TileType<MawAcidPool>();
	}
}
