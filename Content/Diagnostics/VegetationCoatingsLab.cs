using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	/// <summary>Identical native objects in five render states, isolated to the existing QA panel.</summary>
	internal static class VegetationCoatingsLab
	{
		internal static void Build(Rectangle bounds)
		{
			if (Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3")
				throw new InvalidOperationException("Coating checks require the disposable world.");
			int floor = bounds.Bottom - 3;
			int grass = ModContent.TileType<WastesGrass>(), soil = ModContent.TileType<WastesSoil>();
			for (int x = bounds.Left + 4; x < bounds.Left + 98; x++)
				for (int y = bounds.Top; y <= floor + 1; y++)
				{
					Tile tile = Main.tile[x, y]; tile.ClearEverything();
					if (y >= floor) { tile.HasTile = true; tile.TileType = (ushort)(y == floor ? grass : soil); }
				}
			for (int mode = 0; mode < 5; mode++)
			{
				int left = bounds.Left + 6 + mode * 18;
				foreach (var placement in new[] { (X: left + 2, Type: ModContent.TileType<DeadTuft>()),
					(X: left + 6, Type: ModContent.TileType<WastesBristle>()), (X: left + 11, Type: ModContent.TileType<WastesRootShrub>()) })
					if (!WorldGen.PlaceObject(placement.X, floor - 1, placement.Type, mute: true, random: 0))
						throw new InvalidOperationException($"Coating control placement failed: {placement.Type}.");
				// Floating grass/soil sample above each group exposes overlays independently of prop anchors.
				for (int x = left + 3; x < left + 12; x++)
					for (int y = floor - 9; y <= floor - 7; y++)
					{
						Tile tile = Main.tile[x, y]; tile.HasTile = true;
						tile.TileType = (ushort)(y == floor - 9 ? grass : soil);
					}
				for (int x = left; x < left + 15; x++)
					for (int y = floor - 10; y < floor; y++)
					{
						WorldGen.SquareTileFrame(x, y, true);
						Tile tile = Main.tile[x, y];
						if (!tile.HasTile) continue;
						if (mode == 1) WorldGen.paintTile(x, y, PaintID.DeepBluePaint);
						if (mode == 2) tile.IsActuated = true;
						if (mode == 3) tile.IsTileInvisible = true;
						if (mode == 4) tile.IsTileFullbright = true;
					}
			}
			Main.dayTime = true; Main.time = 27000d; Lighting.Clear();
		}
	}
}
