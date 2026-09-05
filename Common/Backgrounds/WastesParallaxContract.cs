using System;

namespace apogean.Common.Backgrounds
{
	// Shared by the live renderer and its QA sweep. Distances are world pixels;
	// texture pixels remain physical display pixels in the V1 renderer.
	public static class WastesParallaxContract
	{
		public const int TextureWidth = 2048;
		public static float Horizontal(int layer) => layer switch
		{
			0 => .055f, 1 => .14f, 2 => .30f,
			_ => throw new ArgumentOutOfRangeException(nameof(layer))
		};
		public static double Repeats(double travel, int layer) => Math.Abs(travel) * Horizontal(layer) / TextureWidth;
	}
}
