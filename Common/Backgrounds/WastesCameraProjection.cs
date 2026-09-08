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
			0 => .012f, 1 => .03f, 2 => .06f,
			_ => throw new ArgumentOutOfRangeException(nameof(layer))
		};

		public static float Top(double surfaceTiles, float cameraY, int height, int textureHeight, int layer, float gameZoom = 1f)
		{
			if (layer == 2)
			{
				return CloseSoilY(surfaceTiles, cameraY, height, gameZoom) - CloseSoilRow;
			}
			float center = cameraY + height * .5f;
			return height * (.57f + layer * .025f) - 740 - height * .05f * Vertical(layer)
				+ FlightDisplacement(surfaceTiles, center, height, layer);
		}

		// Close is scenery just ahead of Mid, not a tile-speed foreground plane.
		// Both camera movement and fixed terrain relief project through its depth.
		// No temporal filter: stopping, returning and reloading never cause catch-up.
		public static float CloseSoilY(double surfaceTiles, float cameraY, int height,
			float gameZoom, float? regionalGround = null)
		{
			float center = cameraY + height * .5f;
			float reference = (float)((surfaceTiles - 50) * 16);
			float ground = regionalGround ?? reference;
			return height * .5f - GroundOffset * gameZoom + (ground - reference) * Vertical(2)
				+ FlightDisplacement(surfaceTiles, center, height, 2);
		}

		// Integrate a smoothstep RESPONSE, rather than lerping two positions or
		// spring-following the camera. Position, speed and acceleration are continuous.
		// The first 10% of ascent preserves ordinary ground/jump parallax exactly.
		// Beyond that, depth order remains Close > Mid > Far, even after leaving view.
		// Space determines enough travel for the city to exit, not a full-speed clamp.
		public static float FlightDisplacement(double surfaceTiles, float cameraCenterY, int height, int layer)
		{
			double rate = Vertical(layer);
			double ground = (surfaceTiles - 50) * 16;
			double span = Math.Max(1, ground - SpaceBoundaryY(surfaceTiles));
			double ascent = ground - cameraCenterY;
			double u = Math.Max(0, (ascent / span - .1) / .9);
			double integral = u < 1 ? u * u * u * (1 - .5 * u) : u - .5;
			double farGroundTop = height * .57 - 740 - height * .05 * Vertical(0);
			double amplitude = 2 * Math.Max(0, height + 64 - farGroundTop - span * Vertical(0));
			double depth = layer switch { 0 => 1, 1 => 1.3, 2 => 1.6, _ => throw new ArgumentOutOfRangeException(nameof(layer)) };
			return (float)(ascent * rate + amplitude * depth * integral);
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
			// Retained for existing diagnostic callers; height no longer fades Mid.
			return 1f;
		}

		// First continuous pixel boundary above which the installed 1.4.4 integer
		// player-center tile qualifies as Space. Preserve the engine's float .35.
		public static float SpaceBoundaryY(double surfaceTiles) =>
			(float)((Math.Floor(surfaceTiles * (double).35f) + 1) * 16);

		// Deliberately independent of player/camera altitude. The projected geometry
		// leaves the viewport instead. Biome/style and future reclamation fades
		// remain separate factors; entering Space must not force alpha to zero.
		public static float LandOpacity(double surfaceTiles, float cameraCenterY, float playerCenterY, int layer)
		{
			if (layer < 0 || layer > 2) throw new ArgumentOutOfRangeException(nameof(layer));
			return 1f;
		}

		public const int LowerStrataHeight = 512;
		public static float CoveredBottom(float top, int textureHeight, int viewportHeight)
		{
			float bottom = (float)Math.Floor(top) + textureHeight;
			return bottom >= viewportHeight ? bottom : bottom + (float)Math.Ceiling((viewportHeight - bottom) / LowerStrataHeight) * LowerStrataHeight;
		}
	}
}
