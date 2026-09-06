using System;

namespace apogean.Common.Backgrounds
{
	// Authored QA composition, in native display pixels. World/region selection
	// can choose a different composition later; flight must never reroll this one.
	public static class WastesModularLayout
	{
		public const int MidPeriod = 2655;
		public const int ClosePeriod = 2268;
		public const int MidHeight = 1408;
		public const int CloseHeight = 1915;
		public const int CloseWidth = 1448;
		public const int CloseSoilRow = 330;
		public static int MidOffset(int group) => group switch
		{
			0 => 0, 1 => 936, 2 => 1687,
			_ => throw new ArgumentOutOfRangeException(nameof(group))
		};
		public static int MidWidth(int group) => group switch
		{
			0 => 576, 1 => 391, 2 => 488,
			_ => throw new ArgumentOutOfRangeException(nameof(group))
		};
		public static float Phase(double worldX, int layer)
		{
			int period = layer == 1 ? MidPeriod : ClosePeriod;
			double phase = (worldX * WastesParallaxContract.Horizontal(layer) - (layer == 2 ? 620 : 0)) % period;
			return (float)(phase < 0 ? phase + period : phase);
		}
		public static float Top(double surface, float cameraY, int height, int layer, float zoom)
		{
			if (layer == 1)
				return 220 + WastesCameraProjection.Top(surface, cameraY, height, MidHeight, 1, zoom);
			if (layer == 2)
				return WastesCameraProjection.Top(surface, cameraY, height, CloseHeight, 2, zoom)
					+ WastesCameraProjection.CloseSoilRow - CloseSoilRow;
			throw new ArgumentOutOfRangeException(nameof(layer));
		}
		// Validate only geometry that can actually intersect the viewport. A bank
		// below the screen has correctly left view; it isn't missing coverage.
		public static bool BottomExposed(float top, int textureHeight, int viewportHeight) =>
			top < viewportHeight && top + textureHeight > 0 && top + textureHeight < viewportHeight;
	}
}
