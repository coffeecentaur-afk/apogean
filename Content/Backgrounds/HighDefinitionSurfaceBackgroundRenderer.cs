using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace apogean.Content.Backgrounds
{
	/// <summary>
	/// Draws V0 diagnostic panorama layers outside vanilla's enlarged slots.
	/// V0 still inherits forced minimum background zoom; scale 1 alone is not
	/// verified native screen scale. The staged Wastes V1 path compensates that
	/// zoom and has a separate source-to-live landmark regression.
	/// </summary>
	internal static class HighDefinitionSurfaceBackgroundRenderer
	{
		private readonly record struct Layer(
			Asset<Texture2D> Texture,
			float HorizontalParallax,
			float VerticalParallax,
			float BaseTopScreenRatio);

		private static Dictionary<RuinedBackgroundBiome, Layer[]> layerSets;

		internal static bool Supports(RuinedBackgroundBiome biome) => layerSets?.ContainsKey(biome) == true;

		internal static void Load()
		{
			if (Main.dedServ)
				return;

			layerSets = new Dictionary<RuinedBackgroundBiome, Layer[]>
			{
				[RuinedBackgroundBiome.Forest] = LoadV0Layers(RuinedBackgroundBiome.Forest),
				[RuinedBackgroundBiome.Desert] = LoadV0Layers(RuinedBackgroundBiome.Desert),
				[RuinedBackgroundBiome.Jungle] = LoadV0Layers(RuinedBackgroundBiome.Jungle),
				[RuinedBackgroundBiome.Snow] = LoadV0Layers(RuinedBackgroundBiome.Snow),
				[RuinedBackgroundBiome.Corruption] = LoadV0Layers(RuinedBackgroundBiome.Corruption),
				[RuinedBackgroundBiome.Crimson] = LoadV0Layers(RuinedBackgroundBiome.Crimson),
				[RuinedBackgroundBiome.Hallow] = LoadV0Layers(RuinedBackgroundBiome.Hallow),
				[RuinedBackgroundBiome.Ocean] = LoadV0Layers(RuinedBackgroundBiome.Ocean),
				[RuinedBackgroundBiome.Mushroom] = LoadV0Layers(RuinedBackgroundBiome.Mushroom)
			};
		}

		internal static void Unload()
		{
			layerSets = null;
			WastesLandscapeV1Renderer.Unload();
		}

		internal static void DrawV0(SpriteBatch spriteBatch, RuinedBackgroundBiome biome, float opacity, int styleSlot)
		{
			if (Main.dedServ || Main.mapFullscreen)
				return;
			if (biome == RuinedBackgroundBiome.Forest &&
				WastesLandscapeV1Renderer.EnabledForCurrentWorld)
			{
				WastesLandscapeV1Renderer.Draw(spriteBatch, opacity, styleSlot);
				return;
			}
			if (!Supports(biome))
				throw new ArgumentOutOfRangeException(nameof(biome), biome, "No native-detail surface benchmark is registered.");

			float expectedSurfaceScreenY = Main.screenHeight * 0.52f;
			float actualSurfaceScreenY = (float)(Main.worldSurface * 16.0 - Main.screenPosition.Y);
			float surfaceCameraDelta = actualSurfaceScreenY - expectedSurfaceScreenY;
			// The 2026 preview runtime exposes the current world-sky tint but no
			// longer publishes the old ColorOfSurfaceBackgroundsModified member.
			Color tint = GetReadableTint(Main.ColorOfTheSkies) * MathHelper.Clamp(opacity, 0f, 1f);
			float underfillTop = expectedSurfaceScreenY + surfaceCameraDelta * 0.30f;
			DrawUnderfill(spriteBatch, biome, underfillTop, tint);

			if (layerSets is null || !layerSets.TryGetValue(biome, out Layer[] layers))
				throw new InvalidOperationException("Native-detail surface background assets were not loaded.");

			foreach (Layer layer in layers)
			{
				float top = Main.screenHeight * layer.BaseTopScreenRatio + surfaceCameraDelta * layer.VerticalParallax;
				DrawVerticallyCoveredLayer(spriteBatch, layer.Texture.Value, top, layer.HorizontalParallax, tint);
			}
		}

		private static Layer[] LoadV0Layers(RuinedBackgroundBiome biome)
		{
			string root = $"apogean/Content/Backgrounds/Diagnostics/HD/{biome}ConceptV0";
			return new[]
			{
				new Layer(ModContent.Request<Texture2D>($"{root}_Far"), 0.055f, 0.10f, -0.03f),
				new Layer(ModContent.Request<Texture2D>($"{root}_Mid"), 0.14f, 0.18f, -0.06f),
				new Layer(ModContent.Request<Texture2D>($"{root}_Close"), 0.30f, 0.30f, 0f)
			};
		}

		private static void DrawVerticallyCoveredLayer(
			SpriteBatch spriteBatch,
			Texture2D texture,
			float top,
			float parallax,
			Color tint)
		{
			float width = texture.Width;
			// A normal copy followed by a mirrored copy always joins matching
			// source-edge pixels. The two-image period also keeps the mirror parity
			// stable while the camera crosses one texture width.
			float sequenceOffset = PositiveModulo((float)(Main.screenPosition.X * parallax), width * 2f);
			int sequenceIndex = (int)Math.Floor(sequenceOffset / width) - 1;
			float offsetWithinCopy = sequenceOffset % width;
			float startX = -offsetWithinCopy - width;
			int copies = (int)Math.Ceiling((Main.screenWidth - startX) / width) + 1;

			int destinationTop = (int)Math.Floor(top);
			int textureBottom = destinationTop + texture.Height;
			for (int i = 0; i < copies; i++)
			{
				int destinationX = (int)Math.Floor(startX + i * width);
				SpriteEffects effects = ((sequenceIndex + i) & 1) == 0
					? SpriteEffects.None
					: SpriteEffects.FlipHorizontally;
				spriteBatch.Draw(
					texture,
					new Vector2(destinationX, destinationTop),
					null,
					tint,
					0f,
					Vector2.Zero,
					1f,
					effects,
					0f);

				// Custom background drawing owns vertical coverage. Continue the
				// authored opaque baseline rather than exposing a source-image edge
				// when wings or mounts move the camera above the surface fixture.
				if (textureBottom < Main.screenHeight)
				{
					spriteBatch.Draw(
						texture,
						new Rectangle(destinationX, textureBottom, texture.Width, Main.screenHeight - textureBottom),
						new Rectangle(0, texture.Height - 1, texture.Width, 1),
						tint,
						0f,
						Vector2.Zero,
						effects,
						0f);
				}
			}
		}

		private static void DrawUnderfill(
			SpriteBatch spriteBatch,
			RuinedBackgroundBiome biome,
			float top,
			Color tint)
		{
			int topPixel = Math.Clamp((int)Math.Floor(top), 0, Main.screenHeight);
			if (topPixel >= Main.screenHeight)
				return;

			Color baseColor = biome switch
			{
				RuinedBackgroundBiome.Desert => new Color(55, 42, 27),
				RuinedBackgroundBiome.Jungle => new Color(20, 34, 24),
				RuinedBackgroundBiome.Snow => new Color(35, 43, 48),
				RuinedBackgroundBiome.Corruption => new Color(31, 23, 39),
				RuinedBackgroundBiome.Crimson => new Color(44, 20, 19),
				RuinedBackgroundBiome.Hallow => new Color(38, 31, 49),
				RuinedBackgroundBiome.Ocean => new Color(24, 36, 42),
				RuinedBackgroundBiome.Mushroom => new Color(17, 25, 34),
				_ => new Color(34, 27, 21)
			};
			Vector3 modulation = tint.ToVector3();
			Color underfill = new(baseColor.ToVector3() * modulation);
			// ToVector3 drops alpha. Keep the same fade as the textured layers;
			// RGB already contains opacity through tint (premultiplied blending).
			underfill.A = tint.A;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				new Rectangle(0, topPixel, Main.screenWidth, Main.screenHeight - topPixel),
				underfill);
		}

		private static Color GetReadableTint(Color skyTint)
		{
			if (!Main.dayTime)
				// The authored layers already contain deep value separation. Terraria's
				// midnight sky tint is dark enough to multiply their distant silhouettes
				// almost to black, so retain most of the source luminance while allowing
				// the vanilla sky, stars, and scene lighting to carry the night read.
				return ApplyChannelFloor(skyTint, new Color(230, 235, 255));
			if (Main.eclipse)
				// Keep the eclipse warm and oppressive without discarding the ruins that
				// distinguish this background from a flat black silhouette.
				return ApplyChannelFloor(skyTint, new Color(240, 205, 180));
			return skyTint;
		}

		private static Color ApplyChannelFloor(Color color, Color floor) => new(
			Math.Max(color.R, floor.R),
			Math.Max(color.G, floor.G),
			Math.Max(color.B, floor.B),
			byte.MaxValue);

		private static float PositiveModulo(float value, float modulus)
		{
			float result = value % modulus;
			return result < 0f ? result + modulus : result;
		}
	}

	internal sealed class HighDefinitionSurfaceBackgroundAssetSystem : ModSystem
	{
		public override void Load() => HighDefinitionSurfaceBackgroundRenderer.Load();

		public override void Unload() => HighDefinitionSurfaceBackgroundRenderer.Unload();
	}
}
