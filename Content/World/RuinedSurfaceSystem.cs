using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
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
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			if (!ModContent.GetInstance<ApogeanWorldConfig>().RuinedSurface) return;
			int trees = tasks.FindIndex(pass => pass.Name.Equals("Planting Trees", StringComparison.OrdinalIgnoreCase));
			int insertAt = trees >= 0 ? trees + 1 : Math.Max(0, tasks.Count - 1);
			tasks.Insert(insertAt, new PassLegacy("A World Picked Clean", GenerateRuinedSurface));
		}

		private static void GenerateRuinedSurface(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "Remembering the world that was...";
			ApplyRuinedSurface();
		}

		public static int ApplyRuinedSurface()
		{
			int changed = 0;
			int deadGrass = ModContent.TileType<DeadGrass>();
			int deadGrassWall = ModContent.WallType<DeadGrassWallUnsafe>();
			int deadFlowerWall = ModContent.WallType<DeadFlowerWallUnsafe>();
			int maximumY = Math.Min(Main.maxTilesY - 20, (int)Main.rockLayer + 100);
			for (int x = 30; x < Main.maxTilesX - 30; x++)
			{
				for (int y = 30; y < maximumY; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
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
						case WallID.LivingLeaf:
							tile.WallType = WallID.None;
							changed++;
							break;
					}

					if (!tile.HasTile) continue;

					switch (tile.TileType)
					{
						case TileID.Grass:
							tile.TileType = (ushort)deadGrass;
							tile.TileColor = PaintID.None;
							changed++;
							break;
						case TileID.Plants:
						case TileID.Plants2:
						case TileID.Vines:
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

			PlantDeadSurface(deadGrass, maximumY);
			return changed;
		}

		private static void PlantDeadSurface(int deadGrass, int maximumY)
		{
			int deadTuft = ModContent.TileType<DeadTuft>();
			for (int x = 35; x < Main.maxTilesX - 35; x++)
			{
				for (int y = 40; y < maximumY; y++)
				{
					Tile ground = Framing.GetTileSafely(x, y);
					if (!ground.HasTile || ground.TileType != deadGrass || Framing.GetTileSafely(x, y - 1).HasTile) continue;

					if (WorldGen.genRand.NextBool(11))
					{
						WorldGen.PlaceTile(x, y - 1, deadTuft, mute: true, forced: true);
					}
					break;
				}
			}
		}
	}
}
