using System;

namespace apogean.Common.Geometry
{
	// Surface order is persistent: floor, left support, ceiling, right support.
	// Choose once at placement, never while drawing or checking existing support.
	public static class MawToothPlacement
	{
		public static int CurveStyle(int surface, int playerFacing, float playerCenterY, float toothCenterY)
		{
			if (surface < 0 || surface > 3) throw new ArgumentOutOfRangeException(nameof(surface));
			if (playerFacing != -1 && playerFacing != 1) throw new ArgumentOutOfRangeException(nameof(playerFacing));
			if (!float.IsFinite(playerCenterY) || !float.IsFinite(toothCenterY)) throw new ArgumentOutOfRangeException(nameof(playerCenterY));
			// Original floor art: back RIGHT, point LEFT. Ceiling reverses that.
			// Original wall art: back BELOW, point ABOVE, on both walls.
			return surface switch {
				0 => playerFacing == 1 ? 0 : 1,
				2 => playerFacing == -1 ? 0 : 1,
				_ => playerCenterY < toothCenterY ? 1 : 0 // exact height: stable tip-up default
			};
		}
	}
}
