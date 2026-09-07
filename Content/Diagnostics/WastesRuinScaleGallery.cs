using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
	// Upper studies ONLY, not deep-foundation parallax modules. Never ordinary routing.
	internal static class WastesRuinScaleGallery
	{
		private static readonly string[] Names = { "Station", "BrokenShell", "MotorDepot", "Checkpoint" };
		private static Asset<Texture2D>[] assets;
		internal static bool Available => ModContent.HasAsset(Path("Checkpoint"));
		private static string Path(string name) => "apogean/Content/Backgrounds/Candidates/WastesScaleGallery/" + name + "-Upper";
		internal static void Unload() => assets = null;
		internal static void Draw(SpriteBatch batch, WastesLandscapeCameraLab lab, float cameraY,
			int width, int height, Vector2 scale, Color tint)
		{
			int selected = lab.ScaleGalleryIndex;
			if (selected < 1 || selected > 3 || Main.netMode != NetmodeID.SinglePlayer ||
				Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3" || !Available) return;
			if (assets == null)
			{
				assets = new Asset<Texture2D>[Names.Length];
				for (int i = 0; i < Names.Length; i++) assets[i] = ModContent.Request<Texture2D>(Path(Names[i]));
			}
			// Ground-level comparison: 120 lower rows are hidden by real terrain,
			// not stretched to fake a foundation. Same soil datum as Close.
			float baseline = Vector2.Transform(new Vector2(0, lab.ScaleGalleryGround - 48 - cameraY),
				Main.GameViewMatrix.ZoomMatrix).Y;
			for (int side = 0; side < 2; side++)
			{
				Texture2D texture = assets[side == 0 ? 0 : selected].Value;
				Vector2 pixel = new((float)Math.Floor(width * .5f + (side == 0 ? -536 : 24)),
					(float)Math.Floor(baseline - 340));
				batch.Draw(texture, pixel * scale, null, tint, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
				lab.ObserveScaleGallery(side, Names[side == 0 ? 0 : selected], pixel,
					Vector2.Transform(pixel * scale, Main.CurrentFrameFlags.Hacks.CurrentBackgroundMatrixForCreditsRoll),
					texture.Width, texture.Height, width, height);
			}
		}
	}
}
