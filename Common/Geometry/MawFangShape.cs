using System;

namespace apogean.Common.Geometry
{
	// Pure shared frame/shape contract. No content IDs, world state, or image dependency.
	public static class MawFangShape
	{
		public static void Rotate(ref int x, ref int y, int orientation, int extent = 2)
		{
			for (int i = 0; i < orientation; i++) { int next = extent - y; y = x; x = next; }
		}
		public static int Slope(int x, int y, int orientation)
		{
			if (x < 0 || y < 0 || x > 2 || y > 2 || orientation < 0 || orientation > 3) return -1;
			Rotate(ref x, ref y, (4 - orientation) % 4);
			int slope = (x == 0 && y == 0) || (x == 1 && y == 1) ? 1 :
				(x == 0 && y == 1) || (y == 2 && x < 2) ? 0 : -1;
			if (slope > 0) { int[] rotations = { 1, 3, 4, 2 }; slope = rotations[orientation]; }
			return slope;
		}
		public static int Decode(int frameX, int frameY)
		{
			if (frameX < 0 || frameX >= 54 || frameY < 0 || frameY >= 216 || frameX % 18 != 0 || frameY % 18 != 0) return -1;
			int x = frameX / 18, y = frameY / 18 % 3, o = frameY / 54;
			return Slope(x, y, o) < 0 ? -1 : o * 9 + y * 3 + x;
		}
		public static bool PixelSolid(int x, int y, int slope)
		{
			if (x < 0 || y < 0 || x >= 16 || y >= 16) return false;
			return slope == 0 || (slope == 1 && y >= x) || (slope == 2 && y >= 15 - x) ||
				(slope == 3 && y <= 15 - x) || (slope == 4 && y <= x);
		}
		// Axis-aligned player rectangle against a solid cell/45-degree triangle.
		// Coordinates local to one cell. Tolerance only bridges native contact rounding.
		public static bool Touches(float left, float top, float right, float bottom, int slope, float tolerance = 0.6f)
		{
			if (slope < 0 || slope > 4 || right + tolerance < 0 || bottom + tolerance < 0 || left - tolerance > 16 || top - tolerance > 16) return false;
			float x0 = Math.Max(0, left - tolerance), y0 = Math.Max(0, top - tolerance);
			float x1 = Math.Min(16, right + tolerance), y1 = Math.Min(16, bottom + tolerance);
			return slope == 0 || (slope == 1 && y1 >= x0) || (slope == 2 && x1 + y1 >= 16) ||
				(slope == 3 && x0 + y0 <= 16) || (slope == 4 && y0 <= x1);
		}
	}
}
