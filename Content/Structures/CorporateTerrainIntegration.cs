using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Factions;
using apogean.Content.Tiles;

namespace apogean.Content.Structures
{
	/// <summary>
	/// Bounded post-stamp terrain work. Blueprints own the building; this pass only guarantees
	/// physical contact at the authored surface datum and repairs natural shoulders around it.
	/// </summary>
	internal static class CorporateTerrainIntegration
	{
		public static void BlendGroundCampus(ApogeanFaction faction, AuthoredStructurePlacement placement)
		{
			if (placement.SurfaceY < 0)
				return;

			if (faction == ApogeanFaction.Helix)
				PlaceHelixSurfaceDome(placement);
			SealFoundation(faction, placement);
			RestoreTerrainShoulders(placement);
			FrameContactBand(placement.Bounds, placement.SurfaceY);
		}

		public static void SealFoundation(ApogeanFaction faction, AuthoredStructurePlacement placement)
		{
			if (faction != ApogeanFaction.Kessler)
				return;

			int block = ModContent.TileType<KesslerBlock>();
			int floor = ModContent.TileType<KesslerFloor>();
			for (int x = placement.Bounds.Left; x < placement.Bounds.Right; x++)
			{
				SetSolid(x, placement.SurfaceY, floor);
				SetSolid(x, placement.SurfaceY + 1, floor);
				for (int y = placement.SurfaceY + 2; y <= placement.SurfaceY + 8; y++)
					SetSolid(x, y, block);
			}
		}

		public static void RestoreTerrainShoulders(AuthoredStructurePlacement placement)
		{
			int wastesSoil = ModContent.TileType<WastesSoil>();
			for (int offset = 0; offset < 8; offset++)
			{
				int leftX = placement.Bounds.Left + offset;
				int rightX = placement.Bounds.Right - 1 - offset;
				int topY = placement.SurfaceY + 1 + offset / 2;
				for (int y = topY; y <= placement.SurfaceY + 11; y++)
				{
					FillNaturalGap(leftX, y, wastesSoil);
					FillNaturalGap(rightX, y, wastesSoil);
				}
			}
		}

		public static void PlaceHelixSurfaceDome(AuthoredStructurePlacement placement)
		{
			int floor = ModContent.TileType<HelixFloor>();
			int centerX = placement.Bounds.Center.X;
			for (int x = centerX - 22; x <= centerX + 22; x++)
			{
				if (placement.Entrance.Contains(x, placement.SurfaceY))
					continue;
				SetSolid(x, placement.SurfaceY, floor);
			}
		}

		private static void FillNaturalGap(int x, int y, int tileType)
		{
			if (!WorldGen.InWorld(x, y, 10))
				return;
			Tile tile = Framing.GetTileSafely(x, y);
			if (!tile.HasTile)
				SetSolid(x, y, tileType);
		}

		private static void SetSolid(int x, int y, int tileType)
		{
			if (!WorldGen.InWorld(x, y, 10))
				return;
			Tile tile = Framing.GetTileSafely(x, y);
			tile.HasTile = true;
			tile.TileType = (ushort)tileType;
			tile.TileFrameX = 0;
			tile.TileFrameY = 0;
			tile.Slope = SlopeType.Solid;
			tile.IsHalfBlock = false;
			tile.LiquidAmount = 0;
		}

		private static void FrameContactBand(Rectangle bounds, int surfaceY)
		{
			for (int x = bounds.Left - 2; x <= bounds.Right + 2; x++)
			{
				for (int y = surfaceY - 2; y <= surfaceY + 13; y++)
				{
					if (!WorldGen.InWorld(x, y, 10))
						continue;
					WorldGen.SquareTileFrame(x, y, true);
					WorldGen.SquareWallFrame(x, y, true);
				}
			}
		}
	}
}
