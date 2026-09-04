using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace apogean.Content.NPCs.Kessler
{
	internal static class KesslerTelegraph
	{
		internal static void Draw(SpriteBatch spriteBatch, Vector2 start, Vector2 end, float intensity)
		{
			Vector2 edge = end - start;
			float length = edge.Length();
			if (length < 2f)
				return;

			Vector2 direction = edge / length;
			const float dashLength = 10f;
			const float dashStride = 24f;
			float rotation = edge.ToRotation();
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Vector2 origin = new(0f, pixel.Height * 0.5f);

			// Broken targeting lines remain readable when several auditors acquire the
			// player at once. Solid beams turn overlapping cues into an opaque wedge.
			for (float distance = 0f; distance < length; distance += dashStride)
			{
				float segmentLength = MathHelper.Min(dashLength, length - distance);
				Vector2 drawPosition = start + direction * distance - Main.screenPosition;
				spriteBatch.Draw(pixel, drawPosition, null,
					new Color(92, 0, 0) * (0.7f * intensity), rotation, origin,
					new Vector2(segmentLength / pixel.Width, 2f / pixel.Height), SpriteEffects.None, 0f);
				spriteBatch.Draw(pixel, drawPosition, null,
					new Color(255, 24, 16) * intensity, rotation, origin,
					new Vector2(segmentLength / pixel.Width, 1f / pixel.Height), SpriteEffects.None, 0f);
			}

			Vector2 target = end - Main.screenPosition;
			int markerSize = intensity > 0.8f ? 6 : 4;
			spriteBatch.Draw(TextureAssets.MagicPixel.Value,
				new Rectangle((int)target.X - markerSize / 2, (int)target.Y - markerSize / 2, markerSize, markerSize),
				new Color(255, 16, 8) * intensity);
		}
	}
}
