using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;

namespace apogean.Content.Diagnostics
{
	/// <summary>
	/// Exports vanilla atlases from the running client for local framing research.
	/// The exported references live in the user's Captures folder and are never packaged with the mod.
	/// </summary>
	internal static class VanillaAtlasExporter
	{
		internal static string ExportTileLabReferences()
		{
			if (Main.dedServ)
				throw new InvalidOperationException("Vanilla atlas export requires a graphics client.");

			string outputDirectory = Path.Combine(Main.SavePath, "Captures", "ApogeanTileLabReferences");
			Directory.CreateDirectory(outputDirectory);

			// TextureAssets.Tile can still contain MagicPixel when a vanilla tile has not been drawn
			// in the current session. Requesting the content path directly guarantees the real atlas.
			Export(Main.Assets.Request<Texture2D>("Images/Tiles_0", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, "Vanilla-Dirt-Tile.png"));
			Export(Main.Assets.Request<Texture2D>("Images/Wall_2", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, "Vanilla-DirtUnsafe-Wall.png"));
			Export(Main.Assets.Request<Texture2D>($"Images/Tiles_{TileID.Grass}", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, "Vanilla-Grass-Tile.png"));
			Export(Main.Assets.Request<Texture2D>($"Images/Wall_{WallID.GrassUnsafe}", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, "Vanilla-GrassUnsafe-Wall.png"));
			ExportTerrainPair(outputDirectory, "Stone", TileID.Stone, "Stone", WallID.Stone);
			ExportTerrainPair(outputDirectory, "Sand", TileID.Sand, "Sandstone", WallID.Sandstone);
			ExportTerrainPair(outputDirectory, "Ice", TileID.IceBlock, "IceUnsafe", WallID.IceUnsafe);
			ExportTerrainPair(outputDirectory, "Snow", TileID.SnowBlock, "SnowUnsafe", WallID.SnowWallUnsafe);
			ExportTerrainPair(outputDirectory, "Mud", TileID.Mud, "MudUnsafe", WallID.MudUnsafe);
			// Corporate construction needs a structural reference, not a terrain mask.
			// Export the complete vanilla brick atlases so offline generators preserve
			// Terraria's own adjacency silhouettes and wall-frame topology.
			ExportTerrainPair(outputDirectory, "GrayBrick", TileID.GrayBrick, "GrayBrick", WallID.GrayBrick);
			Export(Main.Assets.Request<Texture2D>($"Images/Tiles_{TileID.Trees}", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, "Vanilla-ForestTree-Trunk.png"));
			Export(Main.Assets.Request<Texture2D>("Images/Tree_Branches_0", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, "Vanilla-ForestTree-Branches.png"));
			Export(Main.Assets.Request<Texture2D>("Images/Tree_Tops_0", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, "Vanilla-ForestTree-Tops.png"));
			for (int style = 0; style <= 31; style++)
			{
				Export(Main.Assets.Request<Texture2D>($"Images/Tree_Branches_{style}", AssetRequestMode.ImmediateLoad).Value,
					Path.Combine(outputDirectory, $"Vanilla-TreeStyle-{style:D2}-Branches.png"));
				Export(Main.Assets.Request<Texture2D>($"Images/Tree_Tops_{style}", AssetRequestMode.ImmediateLoad).Value,
					Path.Combine(outputDirectory, $"Vanilla-TreeStyle-{style:D2}-Tops.png"));
			}
			ExportItem(outputDirectory, "DirtBlock", ItemID.DirtBlock);
			ExportItem(outputDirectory, "StoneBlock", ItemID.StoneBlock);
			ExportItem(outputDirectory, "SandBlock", ItemID.SandBlock);
			ExportItem(outputDirectory, "IceBlock", ItemID.IceBlock);
			ExportItem(outputDirectory, "SnowBlock", ItemID.SnowBlock);
			ExportItem(outputDirectory, "MudBlock", ItemID.MudBlock);
			Export(Main.Assets.Request<Texture2D>($"Images/Projectile_{ProjectileID.SandBallFalling}", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, "Vanilla-SandBall-Projectile.png"));
			return outputDirectory;
		}

		private static void ExportTerrainPair(string outputDirectory, string tileName, int tileType, string wallName, int wallType)
		{
			Export(Main.Assets.Request<Texture2D>($"Images/Tiles_{tileType}", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, $"Vanilla-{tileName}-Tile.png"));
			Export(Main.Assets.Request<Texture2D>($"Images/Wall_{wallType}", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, $"Vanilla-{wallName}-Wall.png"));
		}

		private static void ExportItem(string outputDirectory, string name, int itemType)
		{
			Export(Main.Assets.Request<Texture2D>($"Images/Item_{itemType}", AssetRequestMode.ImmediateLoad).Value,
				Path.Combine(outputDirectory, $"Vanilla-{name}-Item.png"));
		}

		private static void Export(Texture2D texture, string path)
		{
			using FileStream stream = File.Create(path);
			texture.SaveAsPng(stream, texture.Width, texture.Height);
		}
	}
}
