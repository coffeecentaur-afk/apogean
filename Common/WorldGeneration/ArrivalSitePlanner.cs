using System;
using System.Collections.Generic;

namespace apogean.Common.WorldGeneration
{
	// Engine-free survey: reads a classified grid; cannot edit tiles or consume WorldGen.genRand.
	public enum ArrivalCell { Empty, Soil, Cover, Blocked }
	public readonly struct ArrivalBounds
	{
		public readonly int X, Y, Width, Height;
		public int Right => X + Width;
		public int Bottom => Y + Height;
		public ArrivalBounds(int x, int y, int width, int height) { X = x; Y = y; Width = width; Height = height; }
		public bool Intersects(ArrivalBounds b) => X < b.Right && Right > b.X && Y < b.Bottom && Bottom > b.Y;
	}

	public sealed class ArrivalSite
	{
		public int CenterX { get; }
		public int Width => Floor.Length;
		public int Left => CenterX - Width / 2;
		public int PodLeft => CenterX - 2;
		public int PodTop => Floor[Width / 2] - 6;
		public bool Settled { get; }
		public int[] Surface { get; }
		public int[] Floor { get; }
		public ArrivalBounds Envelope { get; }
		public ArrivalSite(int centerX, int[] surface, int[] floor, bool settled)
		{
			CenterX = centerX; Surface = surface; Floor = floor; Settled = settled;
			int low = int.MaxValue, high = int.MinValue;
			foreach (int y in surface) low = Math.Min(low, y);
			foreach (int y in floor) high = Math.Max(high, y);
			Envelope = new ArrivalBounds(Left - 2, low - 8, Width + 4, high - low + 13);
		}
	}

	public static class ArrivalSitePlanner
	{
		private static readonly int[] Shape = { 0, 0, 1, 1, 2, 2, 2, 2, 2, 1, 1, 0, 0 };
		private static readonly int[] CompactShape = { 0, 1, 1, 1, 1, 1, 0 };
		public static ArrivalSite Find(int spawnX, int spawnY, int seed, int worldWidth, int worldHeight,
			Func<int, int, ArrivalCell> read, Func<ArrivalBounds, bool> available, Action<string> report, Func<ArrivalSite, bool> objectsFit = null)
		{
			// All divot sites before the zero-dig fallback. Stable near-to-far ordering; seed chooses side only.
			int firstSide = (seed & 1) == 0 ? 1 : -1;
			for (int mode = 0; mode < 3; mode++)
			for (int distance = 10; distance <= 38; distance += 4)
			foreach (int side in new[] { firstSide, -firstSide })
			{
				int x = spawnX + side * distance;
				ArrivalSite site = At(x, spawnY, mode == 2, worldWidth, worldHeight, read, out string reason, mode == 0 ? 13 : 7);
				ArrivalBounds landing = new ArrivalBounds(spawnX - 3, spawnY - 5, 7, 10);
				if (site != null && site.Envelope.Intersects(landing)) { site = null; reason = "spawn-envelope"; }
				if (site != null && !available(site.Envelope)) { site = null; reason = "reserved"; }
				if (site != null && objectsFit != null && !objectsFit(site)) { site = null; reason = "partial-ground-cover"; }
				report?.Invoke($"x={x}; variant={mode}; {(site == null ? reason : "accepted")}");
				if (site != null) return site;
			}
			return null;
		}

		public static bool CoverFits(ArrivalSite site, ArrivalBounds cover)
		{
			int affected = 0;
			for (int x = cover.X; x < cover.Right; x++)
			for (int y = cover.Y; y < cover.Bottom; y++)
				if (x >= site.Left && x < site.Left + site.Width && y >= site.Envelope.Y + 2 && y < site.Floor[x - site.Left]) affected++;
			return affected == 0 || affected == cover.Width * cover.Height;
		}

		public static ArrivalSite At(int centerX, int spawnY, bool settled, int width, int height,
			Func<int, int, ArrivalCell> read, out string reason, int size = 13)
		{
			if (size != 13 && size != 7) throw new ArgumentOutOfRangeException(nameof(size));
			int[] shape = size == 13 ? Shape : CompactShape;
			reason = "world-margin";
			if (centerX < 40 || centerX >= width - 40 || spawnY < 70 || spawnY >= height - 70) return null;
			int[] surface = new int[size], floor = new int[size];
			int baseline = int.MinValue;
			for (int column = 0; column < size; column++)
			{
				int x = centerX - size / 2 + column, y = spawnY - 24;
				while (y <= spawnY + 24 && read(x, y) is ArrivalCell.Empty or ArrivalCell.Cover) y++;
				if (y > spawnY + 24 || read(x, y) != ArrivalCell.Soil) { reason = "surface-obstacle"; return null; }
				surface[column] = y;
				baseline = Math.Max(baseline, y - (settled ? 0 : shape[column]));
			}
			for (int column = 0; column < size; column++)
			{
				floor[column] = baseline + (settled ? 0 : shape[column]);
				int cut = floor[column] - surface[column];
				if (cut < 0 || cut > (settled ? 0 : 2)) { reason = "excess-relief"; return null; }
			}
			ArrivalSite result = new ArrivalSite(centerX, surface, floor, settled);
			if (!Recheck(result, read, out reason)) return null;
			return result;
		}

		public static bool Recheck(ArrivalSite site, Func<int, int, ArrivalCell> read, out string reason)
		{
			ArrivalBounds b = site.Envelope;
			// The padded framing/liquid-neighbor envelope must contain only explicitly allowed nature.
			for (int x = b.X; x < b.Right; x++)
			for (int y = b.Y; y < b.Bottom; y++)
				if (read(x, y) == ArrivalCell.Blocked) { reason = "unsafe-envelope"; return false; }
			for (int column = 0; column < site.Width; column++)
			{
				int x = site.Left + column;
				for (int y = b.Y; y < site.Surface[column]; y++)
					if (read(x, y) is not (ArrivalCell.Empty or ArrivalCell.Cover)) { reason = "changed-headroom"; return false; }
				for (int y = site.Surface[column]; y <= site.Floor[column] + 3; y++)
					if (read(x, y) != ArrivalCell.Soil) { reason = "cavity-or-changed-support"; return false; }
			}
			reason = "safe";
			return true;
		}
	}
}
