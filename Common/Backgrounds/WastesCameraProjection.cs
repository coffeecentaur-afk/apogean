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
			// Preserve the approved slow parallax below the ceiling. Above it,
			// evaluate that same composition at the ceiling and project its fixed
			// world position with the remaining camera displacement. Never latch
			// a first-visited frame: descent/teleport/reload must give the same Y.
			float center = cameraY + height * .5f;
			float cappedCenter = Math.Max(center, LockCameraCenterY(surfaceTiles, layer));
			float cappedCamera = cappedCenter - height * .5f;
			float delta = (float)((surfaceTiles - 50) * 16 - cappedCamera) - height * .55f;
			float top = height * (.57f + layer * .025f) - 740 + delta * Vertical(layer);
			return top + (cappedCenter - center) * gameZoom;
		}

		// World-height staging, not an opacity envelope. Mid locks first; Far
		// keeps its slow parallax longer. Lower ceilings make scenery leave view
		// earlier without changing ground composition or dissolving it on ascent.
		public static float LockCameraCenterY(double surfaceTiles, int layer)
		{
			float fraction = layer switch
			{
				0 => .5f, 1 => .25f,
				_ => throw new ArgumentOutOfRangeException(nameof(layer))
			};
			float ground = (float)((surfaceTiles - 50) * 16);
			return ground - Math.Max(1, ground - SpaceBoundaryY(surfaceTiles)) * fraction;
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

		// Deliberately independent of player/camera altitude. The capped geometry
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
