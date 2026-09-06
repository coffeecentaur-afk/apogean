using System;

namespace apogean.Common.Backgrounds
{
	// Immutable terrain snapshot. Neither flight nor later player construction
	// resamples it. Interpolation avoids an elevation jump at regional boundaries.
	public sealed class WastesGroundProfile
	{
		public const int StepTiles = 32;
		private readonly int[] rows;
		public WastesGroundProfile(int[] terrainRows)
		{
			if (terrainRows == null || terrainRows.Length < 2)
				throw new ArgumentException("A ground profile needs at least two samples.");
			rows = (int[])terrainRows.Clone();
		}
		public int[] CopyRows() => (int[])rows.Clone();
		public float GroundAt(float worldX)
		{
			float index = Math.Clamp(worldX / (StepTiles * 16f), 0, rows.Length - 1);
			int left = Math.Min((int)index, rows.Length - 2);
			float t = index - left;
			return (rows[left] + (rows[left + 1] - rows[left]) * t) * 16f;
		}

		// The live sampler and tests share the same column traversal. The caller
		// supplies the allowlisted ground predicate, not the player's position.
		public static int FindSurface(int x, int firstY, int lastY, int fallback,
			Func<int, int, bool> isNaturalSolid)
		{
			// Native grass over one soil row is already a valid surface (including
			// the existing grove fixture); requiring a deep column skips that floor.
			for (int y = firstY; y < lastY; y++)
			{
				if (isNaturalSolid(x, y) && isNaturalSolid(x, y + 1)) return y;
			}
			return fallback;
		}
	}
}
