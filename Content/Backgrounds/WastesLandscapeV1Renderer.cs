using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using apogean.Common.Backgrounds;
using apogean.Content.Diagnostics;

namespace apogean.Content.Backgrounds
{
	/// <summary>Approved landscape composition, staged through the Forest render lab.</summary>
	internal static class WastesLandscapeV1Renderer
	{
		private static Asset<Texture2D>[] layers;
		private static Asset<Texture2D>[] modular;
		private static bool nativeCity;
		internal static int FarRepeatWidth => layers?[0]?.Value.Width ?? WastesParallaxContract.TextureWidth;
		internal static double RawTextureMiB
		{
			get
			{
				long bytes = 0;
				if (layers != null) foreach (var asset in layers)
					if (asset != null) bytes += (long)asset.Value.Width * asset.Value.Height * 4;
				if (modular != null) foreach (var asset in modular)
					bytes += (long)asset.Value.Width * asset.Value.Height * 4;
				return bytes / 1048576d;
			}
		}
		// Production routing can be exercised without promoting art to real worlds.
		internal static bool EnabledForCurrentWorld =>
			Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" ||
			RuinedBackgroundSelectionSystem.Instance.SurfaceRenderLabBiome == RuinedBackgroundBiome.Forest;
		internal static void Load()
		{
			if (Main.dedServ || layers != null) return;
			// Optional asset exists only in an isolated QA package. Ordinary builds
			// retain the prior candidate; neither path promotes art to other worlds.
			const string cityPath = "apogean/Content/Backgrounds/Candidates/WastesCity/Far";
			nativeCity = ModContent.HasAsset(cityPath);
			layers = new[] {
				ModContent.Request<Texture2D>(nativeCity ? cityPath : "apogean/Content/Backgrounds/Candidates/WastesV1/Far"),
				(Asset<Texture2D>)null, null
			};
			modular = new[] {
				ModContent.Request<Texture2D>("apogean/Content/Backgrounds/Candidates/WastesModules/Highway"),
				ModContent.Request<Texture2D>("apogean/Content/Backgrounds/Candidates/WastesModules/Quiet"),
				ModContent.Request<Texture2D>("apogean/Content/Backgrounds/Candidates/WastesModules/Station"),
				ModContent.Request<Texture2D>("apogean/Content/Backgrounds/Candidates/WastesModules/Foreground-Deep")
			};
		}

		internal static void Unload() { layers = null; modular = null; nativeCity = false; }

		internal static void Draw(SpriteBatch batch, float opacity, int styleSlot)
		{
			if (Main.dedServ || Main.mapFullscreen || layers == null || opacity <= 0f) return;
			WastesLandscapeCameraLab cameraLab = Main.LocalPlayer.GetModPlayer<WastesLandscapeCameraLab>();
			float sampledX = cameraLab.BackgroundSampleX(Main.screenPosition.X);
			cameraLab.ObserveDraw(sampledX);
			ModContent.GetInstance<ForestSprayVisualLab>().ObserveWastesDraw(opacity, styleSlot);
			// Terraria's surface pass uses logical screen dimensions and a forced
			// minimum background zoom (e.g. 4/3 at 1440p). Counter only its zoom,
			// not gravity effects, so an authored pixel remains a display pixel.
			// Do not End/Begin the caller's batch or mutate global zoom settings.
			int width = batch.GraphicsDevice.Viewport.Width;
			int height = batch.GraphicsDevice.Viewport.Height;
			Vector2 scale = Vector2.One / Main.BackgroundViewMatrix.Zoom;
			// DrawBG temporarily shifts screenPosition into its logical view. Remove
			// that offset for the world-surface datum, not the sprite positions.
			float worldCameraY = Main.screenPosition.Y - Main.BackgroundViewMatrix.Translation.Y;
			float altitude = WastesCameraProjection.Altitude(Main.worldSurface, worldCameraY + height * .5f);
			// A continuous sky-derived floor avoids brightening abruptly when
			// dayTime flips at dusk. Alpha belongs to the style fade, not sky tint.
			Color sky = Main.ColorOfTheSkies;
			Color light = new Color(Math.Max((int)sky.R, 65), Math.Max((int)sky.G, 75), Math.Max((int)sky.B, 98));
			if (Main.eclipse) light = new Color(105, 88, 80);
			Color tint = light * MathHelper.Clamp(opacity, 0, 1);
			for (int i = 0; i < layers.Length; i++)
			{
				if (i > 0)
				{
					DrawModular(batch, i, sampledX, worldCameraY, width, height, scale,
						i == 1 ? tint * WastesCameraProjection.MiddleOpacity(altitude) : tint, cameraLab);
					continue;
				}
				float horizontal = WastesParallaxContract.Horizontal(i);
				Texture2D texture = layers[i].Value;
				float top = WastesCameraProjection.Top(Main.worldSurface, worldCameraY, height, texture.Height, i, Main.GameViewMatrix.Zoom.Y);
				// Close leaves the camera by world movement, never by a below-ground
				// fade. Mid yields gradually to Far after the first third of ascent.
				Color layerTint = i == 1 ? tint * WastesCameraProjection.MiddleOpacity(altitude) : tint;
				float phase = (float)(sampledX * horizontal % texture.Width);
				if (phase < 0) phase += texture.Width;
				Vector2 first = Vector2.Zero, end = Vector2.Zero;
				for (float x = -phase; x < width; x += texture.Width)
				{
					Vector2 position = new(
						WastesCameraProjection.LogicalCoordinate((int)Math.Floor(x), Main.BackgroundViewMatrix.Zoom.X),
						WastesCameraProjection.LogicalCoordinate((int)Math.Floor(top), Main.BackgroundViewMatrix.Zoom.Y));
					batch.Draw(texture, position, null, layerTint, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
					float coveredBottom = (float)Math.Floor(top) + texture.Height;
					// City candidate owns its full native-depth coverage. Do not reflect
					// the skyline or silently substitute the old strata guard below it.
					if (!nativeCity)
						coveredBottom = DrawLowerStrata(batch, texture, position.X, coveredBottom, height, scale, layerTint);
					if (x == -phase) first = position;
					end = new Vector2(position.X + texture.Width * scale.X,
						coveredBottom * scale.Y);
				}
				cameraLab.ObserveProjection(i, first, end, Main.CurrentFrameFlags.Hacks.CurrentBackgroundMatrixForCreditsRoll, width, height);
				if (i == 2) cameraLab.ObserveGroundLock(top, worldCameraY, height);
			}
		}

		private static void DrawModular(SpriteBatch batch, int layer, float sampledX, float cameraY,
			int width, int height, Vector2 scale, Color tint, WastesLandscapeCameraLab lab)
		{
			// Use physical world X, not the diagnostic repeat-phase override. Every
			// vertical view over this region receives the same saved terrain datum.
			float centerX = Main.screenPosition.X - Main.BackgroundViewMatrix.Translation.X + width * .5f;
			bool hasRegionalGround = WastesGroundProfileSystem.TryGroundAt(centerX, out float ground);
			float top = WastesModularLayout.Top(Main.worldSurface, cameraY, height, layer, Main.GameViewMatrix.Zoom.Y,
				hasRegionalGround ? ground : null);
			int period = layer == 1 ? WastesModularLayout.MidPeriod : WastesModularLayout.ClosePeriod;
			float phase = WastesModularLayout.Phase(sampledX, layer);
			int depth = layer == 1 ? WastesModularLayout.MidHeight : WastesModularLayout.CloseHeight;
			int submitted = 0;
			// The previous period can contribute a trailing group at the left edge.
			// Gaps are real negative space; no opaque fill or reflected strata here.
			for (float start = -phase - period; start < width; start += period)
			{
				int count = layer == 1 ? 3 : 1;
				for (int group = 0; group < count; group++)
				{
					Texture2D texture = modular[layer == 1 ? group : 3].Value;
					float x = start + (layer == 1 ? WastesModularLayout.MidOffset(group) : 0);
					if (x + texture.Width <= 0 || x >= width || top >= height || top + depth <= 0) continue;
					Vector2 pixel = new((float)Math.Floor(x), (float)Math.Floor(top));
					Vector2 position = pixel * scale;
					batch.Draw(texture, position, null, tint, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
					lab.ObserveModularProjection(layer, position, pixel, texture.Height,
						Main.CurrentFrameFlags.Hacks.CurrentBackgroundMatrixForCreditsRoll, width, height);
					submitted++;
				}
			}
			lab.ObserveModularFrame(layer, sampledX, top, submitted, width, height);
			if (layer == 2)
				lab.ObserveGroundLock(top + WastesModularLayout.CloseSoilRow - WastesCameraProjection.CloseSoilRow,
					cameraY, height, hasRegionalGround ? ground : null);
		}

		// Bounded QA continuation of existing opaque rock/soil, never a stretched
		// last row or additional GPU asset. Alternating vertical reflection makes
		// source edges meet exactly. Repeated strata still need the art-review gate.
		// Terraria continues to own the surface-to-underground style handoff.
		private static float DrawLowerStrata(SpriteBatch batch, Texture2D texture, float logicalX,
			float baseY, int viewportHeight, Vector2 scale, Color tint)
		{
			const int rows = WastesCameraProjection.LowerStrataHeight;
			Rectangle source = new(0, texture.Height - rows, texture.Width, rows);
			int first = Math.Max(0, (int)Math.Floor(-baseY / rows));
			float bottom = baseY;
			for (int index = first; baseY + index * rows < viewportHeight; index++)
			{
				Vector2 position = new(logicalX, (baseY + index * rows) * scale.Y);
				batch.Draw(texture, position, source, tint, 0, Vector2.Zero, scale,
					index % 2 == 0 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
				bottom = baseY + (index + 1) * rows;
			}
			return bottom;
		}
	}
}
