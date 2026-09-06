using System;

namespace apogean.Common.Backgrounds
{
	// Pure arithmetic used by the live draw path and camera regression tests.
	public static class WastesCameraProjection
	{
		// DrawBG removes ZoomMatrix's centered translation before drawing.
		// Undo scale only; Terraria still owns the caller's gravity reflection.
		public static float LogicalCoordinate(float pixel, float zoom) => pixel / zoom;

		// Measured nominal soil lip in the unchanged Close.png; relief varies
		// around this row. The three-tile offset concerns soil, not tree tips.
		public const float CloseSoilRow = 488f;
		public const float GroundOffset = 48f;

		public static float Vertical(int layer) => layer switch
		{
			0 => .012f, 1 => .03f, 2 => 1f,
			_ => throw new ArgumentOutOfRangeException(nameof(layer))
		};

		public static float Top(double surfaceTiles, float cameraY, int height, int textureHeight, int layer, float gameZoom = 1f)
		{
			if (layer == 2)
			{
				// Fixed world Y, same vertical camera displacement/zoom as tiles.
				// Never clamp this layer to the screen: flight must leave it below.
				float ground = (float)((surfaceTiles - 50) * 16);
				return (ground - GroundOffset - cameraY - height * .5f) * gameZoom + height * .5f - CloseSoilRow;
			}
			float delta = (float)((surfaceTiles - 50) * 16 - cameraY) - height * .55f;
			float top = height * (.57f + layer * .025f) - 740 + delta * Vertical(layer);
			return top;
		}

		// Normalized ascent from the fixed ground datum toward the upper-sky
		// reference. This is presentation policy, not an enemy/biome predicate.
		public static float Altitude(double surfaceTiles, float cameraCenterY)
		{
			float ground = (float)((surfaceTiles - 50) * 16);
			float sky = (float)(surfaceTiles * 16 * .35);
			return Math.Clamp((ground - cameraCenterY) / Math.Max(1, ground - sky), 0, 1);
		}

		public static float MiddleOpacity(float altitude)
		{
			float t = Math.Clamp((altitude - 1f / 3f) / (.7f - 1f / 3f), 0, 1);
			return 1 - t * t * (3 - 2 * t);
		}

		public const int LowerStrataHeight = 512;
		public static float CoveredBottom(float top, int textureHeight, int viewportHeight)
		{
			float bottom = (float)Math.Floor(top) + textureHeight;
			return bottom >= viewportHeight ? bottom : bottom + (float)Math.Ceiling((viewportHeight - bottom) / LowerStrataHeight) * LowerStrataHeight;
		}
	}
}
