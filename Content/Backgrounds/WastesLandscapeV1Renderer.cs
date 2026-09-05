using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace apogean.Content.Backgrounds
{
	/// <summary>Approved landscape composition, staged through the Forest render lab.</summary>
	internal static class WastesLandscapeV1Renderer
	{
		private static Asset<Texture2D>[] layers;
		// Production routing can be exercised without promoting art to real worlds.
		internal static bool EnabledForCurrentWorld =>
			Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" ||
			RuinedBackgroundSelectionSystem.Instance.SurfaceRenderLabBiome == RuinedBackgroundBiome.Forest;
		internal static void Load()
		{
			if (Main.dedServ || layers != null) return;
			layers = new[] {
				ModContent.Request<Texture2D>("apogean/Content/Backgrounds/Candidates/WastesV1/Far"),
				ModContent.Request<Texture2D>("apogean/Content/Backgrounds/Candidates/WastesV1/Mid"),
				ModContent.Request<Texture2D>("apogean/Content/Backgrounds/Candidates/WastesV1/Close")
			};
		}

		internal static void Unload() => layers = null;

		internal static void Draw(SpriteBatch batch, float opacity)
		{
			if (Main.dedServ || Main.mapFullscreen || layers == null || opacity <= 0f) return;
			// Terraria's surface pass uses logical screen dimensions and a forced
			// minimum background zoom (e.g. 4/3 at 1440p). Counter only its zoom,
			// not gravity effects, so an authored pixel remains a display pixel.
			// Do not End/Begin the caller's batch or mutate global zoom settings.
			int width = batch.GraphicsDevice.Viewport.Width;
			int height = batch.GraphicsDevice.Viewport.Height;
			Matrix inverseZoom = Matrix.Invert(Main.BackgroundViewMatrix.ZoomMatrix);
			Vector2 scale = Vector2.One / Main.BackgroundViewMatrix.Zoom;
			float groundDelta = (float)((Main.worldSurface - 50) * 16 - Main.screenPosition.Y) - height * .55f;
			// A continuous sky-derived floor avoids brightening abruptly when
			// dayTime flips at dusk. Alpha belongs to the style fade, not sky tint.
			Color sky = Main.ColorOfTheSkies;
			Color light = new Color(Math.Max((int)sky.R, 65), Math.Max((int)sky.G, 75), Math.Max((int)sky.B, 98));
			if (Main.eclipse) light = new Color(105, 88, 80);
			Color tint = light * MathHelper.Clamp(opacity, 0, 1);
			for (int i = 0; i < layers.Length; i++)
			{
				float horizontal = i == 0 ? .055f : i == 1 ? .14f : .30f;
				float vertical = i == 0 ? .10f : i == 1 ? .18f : .30f;
				Texture2D texture = layers[i].Value;
				// The camera anchors the ground line, not the image top. Clamp only
				// at the lower surface transition to retain authored bottom coverage.
				float top = height * (.57f + i * .025f) - 740 + groundDelta * vertical;
				// Only the closest opaque terrain must reach the bottom. Clamping
				// every layer would bury the distant skyline behind real surface tiles.
				if (i == layers.Length - 1) top = Math.Max(height - texture.Height, top);
				float phase = (float)(Main.screenPosition.X * horizontal % texture.Width);
				if (phase < 0) phase += texture.Width;
				for (float x = -phase; x < width; x += texture.Width)
				{
					Vector2 position = Vector2.Transform(new Vector2((int)Math.Floor(x), (int)Math.Floor(top)), inverseZoom);
					batch.Draw(texture, position, null, tint, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
				}
			}
		}
	}
}
