using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using apogean.Content.Config;

namespace apogean.Content.World
{
	/// <summary>
	/// Keeps the starting forest mechanically safe while making the old green world visibly dead.
	/// The pass paints existing vanilla tiles rather than replacing them, preserving compatibility and drops.
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
							tile.TileColor = PaintID.BrownPaint;
							changed++;
							break;
						case TileID.Trees:
							tile.TileColor = WorldGen.genRand.NextBool(3) ? PaintID.GrayPaint : PaintID.BrownPaint;
							changed++;
							break;
						case TileID.Plants:
						case TileID.Plants2:
						case TileID.Vines:
							if (WorldGen.genRand.NextBool(4)) tile.TileColor = PaintID.BrownPaint;
							else WorldGen.KillTile(x, y, noItem: true);
							changed++;
							break;
					}
				}
			}
			return changed;
		}
	}
}
