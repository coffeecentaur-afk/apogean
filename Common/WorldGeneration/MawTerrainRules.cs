using Terraria;
using Terraria.ID;

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

		public static bool CanCarve(int x, int y, WorldEditIntent intent)
		{
			if (!ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, intent))
				return false;

			Tile tile = Framing.GetTileSafely(x, y);
			if (HasProtectedWall(tile) || HasNearbyFrameImportantTile(x, y, 1))
				return false;
			return !tile.HasTile || IsNaturalTerrain(tile.TileType);
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

		private static bool HasProtectedWall(Tile tile)
		{
			if (tile.WallType <= WallID.None)
				return false;
			if (tile.WallType < Main.wallHouse.Length && Main.wallHouse[tile.WallType])
				return true;
			if (tile.WallType < Main.wallDungeon.Length && Main.wallDungeon[tile.WallType])
				return true;
			return tile.WallType == WallID.LihzahrdBrickUnsafe;
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

		private static bool IsNaturalTerrain(ushort type) => type is
			TileID.Dirt or
			TileID.Grass or
			TileID.Stone or
			TileID.ClayBlock or
			TileID.Mud or
			TileID.JungleGrass or
			TileID.MushroomGrass or
			TileID.Sand or
			TileID.HardenedSand or
			TileID.Sandstone or
			TileID.SnowBlock or
			TileID.IceBlock or
			TileID.CorruptIce or
			TileID.FleshIce or
			TileID.Ebonstone or
			TileID.Crimstone or
			TileID.Ebonsand or
			TileID.Crimsand or
			TileID.CorruptGrass or
			TileID.CrimsonGrass or
			TileID.GraniteBlock or
			TileID.MarbleBlock or
			TileID.Ash or
			TileID.Hellstone;
	}
}
