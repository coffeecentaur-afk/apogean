using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using apogean.Common.WorldGeneration;
using apogean.Content.Config;
using apogean.Content.Tiles;
using apogean.Content.Walls;

namespace apogean.Content.World
{
	/// <summary>
	/// Keeps the starting forest mechanically safe while making the old green world visibly dead.
	/// Vanilla paint cannot replace canopy art, so this pass uses actual dead ground and vegetation tiles.
	/// </summary>
	public sealed class RuinedSurfaceSystem : ModSystem
	{
		// The replacement species has a stark leafless silhouette. Vanilla forest
		// spacing is too dense for it, so conversion keeps at least this many tiles
		// between roots instead of rendering an unreadable copied thicket.
		private const int MinimumDeadTreeSpacing = 12;

		internal static void GenerateWorld(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "Remembering the world that was...";
			ApplyRuinedSurface();
		}

		public static int ApplyRuinedSurface()
		{
			int changed = 0;
			int wastesGrass = ModContent.TileType<WastesGrass>();
			int wastesSoil = ModContent.TileType<WastesSoil>();
			int wastesStone = ModContent.TileType<WastesStone>();
			int wastesSand = ModContent.TileType<WastesSand>();
			int deadGrassWall = ModContent.WallType<WastesGrassWallUnsafe>();
			int deadFlowerWall = ModContent.WallType<WastesGrassWallUnsafe>();
			int maximumY = Math.Min(Main.maxTilesY - 20, (int)Main.rockLayer + 100);
			bool[] wastesColumn = new bool[Main.maxTilesX];
			int[] surfaceY = new int[Main.maxTilesX];
			for (int x = 30; x < Main.maxTilesX - 30; x++)
			{
				surfaceY[x] = FindForestSurface(x, maximumY);
				wastesColumn[x] = surfaceY[x] > 0;
			}

			for (int x = 30; x < Main.maxTilesX - 30; x++)
			{
				for (int y = 30; y < maximumY; y++)
				{
					if (!ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, WorldEditIntent.WastesConversion)) continue;
					Tile tile = Framing.GetTileSafely(x, y);
					if (wastesColumn[x])
					{
						switch (tile.WallType)
						{
							case WallID.GrassUnsafe:
								tile.WallType = (ushort)deadGrassWall;
								changed++;
								break;
							case WallID.FlowerUnsafe:
								tile.WallType = (ushort)deadFlowerWall;
								changed++;
								break;
						}
					}
					if (tile.WallType == WallID.LivingLeaf)
					{
						tile.WallType = WallID.None;
						changed++;
					}

					if (!tile.HasTile) continue;

					switch (tile.TileType)
					{
						case TileID.Grass:
							if (!wastesColumn[x]) break;
							tile.TileType = (ushort)wastesGrass;
							tile.TileColor = PaintID.None;
							changed++;
							break;
						case TileID.Dirt:
						case TileID.ClayBlock:
							if (!wastesColumn[x] || y > surfaceY[x] + 52) break;
							tile.TileType = (ushort)wastesSoil;
							tile.TileColor = PaintID.None;
							changed++;
							break;
						case TileID.Stone:
							if (!wastesColumn[x] || y > surfaceY[x] + 68) break;
							tile.TileType = (ushort)wastesStone;
							tile.TileColor = PaintID.None;
							changed++;
							break;
						case TileID.Sand:
							if (!wastesColumn[x] || y > surfaceY[x] + 40) break;
							tile.TileType = (ushort)wastesSand;
							tile.TileColor = PaintID.None;
							changed++;
							break;
						case TileID.Plants:
						case TileID.Plants2:
						case TileID.Vines:
							if (!wastesColumn[x]) break;
							WorldGen.KillTile(x, y, noItem: true);
							changed++;
							break;
						case TileID.VanityTreeSakura:
						case TileID.VanityTreeYellowWillow:
							// Vanity trees do not share TileID.Trees. Convert their trunks so the
							// DeadForestTree registered for DeadGrass can render and regrow them.
							tile.TileType = TileID.Trees;
							changed++;
							break;
						case TileID.LeafBlock:
							// Living Trees keep their wood, rooms, roots, and loot, but lose the
							// serene green canopy and its falling-leaf emitter.
							tile.ClearTile();
							changed++;
							break;
					}
				}
			}

			ThinDeadForest(wastesGrass, surfaceY);
			PlantDeadSurface(wastesGrass, maximumY);
			return changed;
		}

		private static void ThinDeadForest(int wastesGrass, int[] surfaceY)
		{
			int lastKeptRootX = -MinimumDeadTreeSpacing;
			for (int x = 30; x < Main.maxTilesX - 30; x++)
			{
				int groundY = surfaceY[x];
				if (groundY <= 1)
					continue;

				Tile ground = Framing.GetTileSafely(x, groundY);
				Tile root = Framing.GetTileSafely(x, groundY - 1);
				if (!ground.HasTile || ground.TileType != wastesGrass || !root.HasTile || root.TileType != TileID.Trees)
					continue;

				if (x - lastKeptRootX >= MinimumDeadTreeSpacing)
				{
					lastKeptRootX = x;
					continue;
				}

				// Tree kill logic clears the connected portion above the root while
				// suppressing world-generation item drops.
				WorldGen.KillTile(x, groundY - 1, noItem: true);
			}
		}

		private static int FindForestSurface(int x, int maximumY)
		{
			for (int y = 35; y < Math.Min(maximumY, Main.worldSurface + 100); y++)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				if (!tile.HasTile)
					continue;
				if (tile.TileType == TileID.Grass || tile.TileType == ModContent.TileType<DeadGrass>() ||
					tile.TileType == ModContent.TileType<WastesGrass>())
					return y;
				if (tile.TileType < TileID.Sets.IsATreeTrunk.Length && TileID.Sets.IsATreeTrunk[tile.TileType])
					continue;
				if (Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
					return 0;
			}
			return 0;
		}

		private static void PlantDeadSurface(int wastesGrass, int maximumY)
		{
			int rootTuft = ModContent.TileType<DeadTuft>();
			int bristle = ModContent.TileType<WastesBristle>();
			int rootShrub = ModContent.TileType<WastesRootShrub>();
			for (int x = 35; x < Main.maxTilesX - 35; x++)
			{
				for (int y = 40; y < maximumY; y++)
				{
					if (!ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, WorldEditIntent.WastesConversion)) continue;
					Tile ground = Framing.GetTileSafely(x, y);
					if (!ground.HasTile || ground.TileType != wastesGrass || Framing.GetTileSafely(x, y - 1).HasTile) continue;

					if (WorldGen.genRand.NextBool(11))
						TryPlaceWastesPlant(x, y - 1, rootTuft, bristle, rootShrub);
					break;
				}
			}
		}

		private static void TryPlaceWastesPlant(int x, int y, int rootTuft, int bristle, int rootShrub)
		{
			int roll = WorldGen.genRand.Next(100);
			int type;
			int variants;

			if (roll < 70)
			{
				type = rootTuft;
				variants = 4;
			}
			else if (roll < 92)
			{
				type = bristle;
				variants = 3;
			}
			else
			{
				type = rootShrub;
				variants = 3;
			}

			WorldGen.PlaceObject(x, y, type, mute: true, style: 0, random: WorldGen.genRand.Next(variants));
		}
	}
}
