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
			int maximumY = Math.Min(Main.maxTilesY - 20, (int)Main.worldSurface + 80);
			for (int x = 30; x < Main.maxTilesX - 30; x++)
			{
				for (int y = 30; y < maximumY; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
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
					}

					if (tile.HasTile && TileID.Sets.IsATreeTrunk[tile.TileType] && IsForestTreeColumn(x, y, deadGrass))
					{
						WorldGen.KillTile(x, y, noItem: true);
						changed++;
					}
				}
			}

			PlantDeadSurface(deadGrass, maximumY);
			return changed;
		}

		private static bool IsForestTreeColumn(int x, int y, int deadGrass)
		{
			for (int checkY = y; checkY < Math.Min(Main.maxTilesY - 10, y + 80); checkY++)
			{
				Tile check = Framing.GetTileSafely(x, checkY);
				if (check.HasTile && TileID.Sets.IsATreeTrunk[check.TileType]) continue;
				return check.HasTile && check.TileType is TileID.Grass || check.HasTile && check.TileType == deadGrass;
			}
			return false;
		}

		private static void PlantDeadSurface(int deadGrass, int maximumY)
		{
			int deadTuft = ModContent.TileType<DeadTuft>();
			int deadTree = ModContent.TileType<DeadTree>();
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
					else if (x % 43 == 0 && IsClearForTree(x, y))
					{
						WorldGen.PlaceObject(x, y - 1, deadTree, mute: true);
					}
					break;
				}
			}
		}

		private static bool IsClearForTree(int x, int groundY)
		{
			for (int checkX = x - 1; checkX <= x + 1; checkX++)
			{
				for (int checkY = groundY - 4; checkY < groundY; checkY++)
				{
					if (Framing.GetTileSafely(checkX, checkY).HasTile) return false;
				}
			}
			return true;
		}
	}
}
