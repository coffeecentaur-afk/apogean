using Microsoft.Xna.Framework;

namespace apogean.Common.WorldGeneration
{
	public sealed class ProtectedRegion
	{
		public string Id { get; }
		public Rectangle Bounds { get; }
		public WorldEditIntent BlockedEdits { get; }

		public ProtectedRegion(string id, Rectangle bounds, WorldEditIntent blockedEdits)
		{
			Id = id;
			Bounds = bounds;
			BlockedEdits = blockedEdits;
		}

		public bool Blocks(int tileX, int tileY, WorldEditIntent intent) =>
			(BlockedEdits & intent) != 0 && Bounds.Contains(tileX, tileY);

		public bool Blocks(Rectangle candidate, WorldEditIntent intent) =>
			(BlockedEdits & intent) != 0 && Bounds.Intersects(candidate);
	}

	internal sealed class ProtectedRegionRegistry
	{
		private readonly System.Collections.Generic.List<ProtectedRegion> regions = new();

		public System.Collections.Generic.IReadOnlyList<ProtectedRegion> Regions => regions;

		public void Clear() => regions.Clear();

		public void Reserve(string id, Rectangle bounds, WorldEditIntent blockedEdits)
		{
			if (bounds.Width <= 0 || bounds.Height <= 0 || blockedEdits == WorldEditIntent.None)
				return;

			regions.Add(new ProtectedRegion(id, bounds, blockedEdits));
		}

		public bool Allows(int tileX, int tileY, WorldEditIntent intent)
		{
			for (int i = 0; i < regions.Count; i++)
			{
				if (regions[i].Blocks(tileX, tileY, intent))
					return false;
			}

			return true;
		}

		public bool Allows(Rectangle candidate, WorldEditIntent intent)
		{
			for (int i = 0; i < regions.Count; i++)
			{
				if (regions[i].Blocks(candidate, intent))
					return false;
			}

			return true;
		}
	}
}
