using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;

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
			return outputDirectory;
		}

		private static void Export(Texture2D texture, string path)
		{
			using FileStream stream = File.Create(path);
			texture.SaveAsPng(stream, texture.Width, texture.Height);
		}
	}
}
