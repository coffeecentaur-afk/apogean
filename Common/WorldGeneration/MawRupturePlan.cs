using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace apogean.Common.WorldGeneration
{
	public sealed class MawRupturePlan
	{
		private readonly List<Point16> navigationSpine;

		public Point16 SurfaceCenter { get; }
		public int RadiusX { get; }
		public int RadiusY { get; }
		public bool IsMajor { get; }
		public bool IsCompact { get; }
		public Point16 MatriarchCenter { get; }
		public IReadOnlyList<Point16> NavigationSpine => navigationSpine;
		public bool HasNavigationSpine => navigationSpine.Count > 0;

		public Rectangle GenerationBounds => new(
			SurfaceCenter.X - RadiusX,
			SurfaceCenter.Y - RadiusY,
			RadiusX * 2 + 1,
			RadiusY * 2 + 1);

		public Rectangle ReservedBounds
		{
			get
			{
				if (!IsMajor)
				{
					Rectangle local = GenerationBounds;
					local.Inflate(24, 18);
					return local;
				}

				int left = SurfaceCenter.X - RadiusX - 40;
				int right = SurfaceCenter.X + RadiusX + 41;
				for (int i = 0; i < navigationSpine.Count; i++)
				{
					left = System.Math.Min(left, navigationSpine[i].X - 32);
					right = System.Math.Max(right, navigationSpine[i].X + 33);
				}

				if (MatriarchCenter.X > 0)
				{
					left = System.Math.Min(left, MatriarchCenter.X - 112);
					right = System.Math.Max(right, MatriarchCenter.X + 113);
				}

				int top = System.Math.Max(20, SurfaceCenter.Y - RadiusY - 24);
				int bottom = MatriarchCenter.Y > 0
					? System.Math.Min(Terraria.Main.maxTilesY - 20, MatriarchCenter.Y + 72)
					: Terraria.Main.maxTilesY - 140;
				return new Rectangle(left, top, right - left, bottom - top);
			}
		}

		/// <summary>
		/// The Stomach's narrow continuation through Hell is reserved separately so the broad
		/// Gullet envelope does not monopolize a large slice of the Underworld.
		/// </summary>
		public Rectangle IntestinalDescentBounds
		{
			get
			{
				if (!IsMajor || MatriarchCenter.X <= 0 || MatriarchCenter.Y <= 0)
					return Rectangle.Empty;

				int top = MatriarchCenter.Y + 42;
				int bottom = Terraria.Main.maxTilesY - 14;
				return new Rectangle(MatriarchCenter.X - 28, top, 57, System.Math.Max(1, bottom - top));
			}
		}

		public MawRupturePlan(Point16 surfaceCenter, int radiusX, int radiusY, bool isMajor)
			: this(surfaceCenter, radiusX, radiusY, isMajor, false, default, null)
		{
		}

		public MawRupturePlan(
			Point16 surfaceCenter,
			int radiusX,
			int radiusY,
			bool isMajor,
			bool isCompact,
			Point16 matriarchCenter,
			IEnumerable<Point16> navigationSpine)
		{
			SurfaceCenter = surfaceCenter;
			RadiusX = radiusX;
			RadiusY = radiusY;
			IsMajor = isMajor;
			IsCompact = isCompact;
			MatriarchCenter = matriarchCenter;
			this.navigationSpine = navigationSpine is null ? new List<Point16>() : new List<Point16>(navigationSpine);
		}

		internal TagCompound Save()
		{
			List<TagCompound> savedSpine = new();
			for (int i = 0; i < navigationSpine.Count; i++)
			{
				savedSpine.Add(new TagCompound
				{
					["x"] = (int)navigationSpine[i].X,
					["y"] = (int)navigationSpine[i].Y
				});
			}

			return new TagCompound
			{
				["x"] = (int)SurfaceCenter.X,
				["y"] = (int)SurfaceCenter.Y,
				["radiusX"] = RadiusX,
				["radiusY"] = RadiusY,
				["major"] = IsMajor,
				["compact"] = IsCompact,
				["rootX"] = (int)MatriarchCenter.X,
				["rootY"] = (int)MatriarchCenter.Y,
				["spine"] = savedSpine
			};
		}

		internal static MawRupturePlan Load(TagCompound tag)
		{
			List<Point16> spine = new();
			if (tag.ContainsKey("spine"))
			{
				foreach (TagCompound point in tag.GetList<TagCompound>("spine"))
					spine.Add(new Point16(point.GetInt("x"), point.GetInt("y")));
			}

			return new MawRupturePlan(
				new Point16(tag.GetInt("x"), tag.GetInt("y")),
				tag.GetInt("radiusX"),
				tag.GetInt("radiusY"),
				tag.GetBool("major"),
				tag.ContainsKey("compact") && tag.GetBool("compact"),
				new Point16(tag.ContainsKey("rootX") ? tag.GetInt("rootX") : 0, tag.ContainsKey("rootY") ? tag.GetInt("rootY") : 0),
				spine);
		}

		internal void NetSend(BinaryWriter writer)
		{
			writer.Write(SurfaceCenter.X);
			writer.Write(SurfaceCenter.Y);
			writer.Write((short)RadiusX);
			writer.Write((short)RadiusY);
			writer.Write(IsMajor);
			writer.Write(IsCompact);
			writer.Write(MatriarchCenter.X);
			writer.Write(MatriarchCenter.Y);
			writer.Write((ushort)navigationSpine.Count);
			for (int i = 0; i < navigationSpine.Count; i++)
			{
				writer.Write(navigationSpine[i].X);
				writer.Write(navigationSpine[i].Y);
			}
		}

		internal static MawRupturePlan NetReceive(BinaryReader reader)
		{
			Point16 surface = new(reader.ReadInt16(), reader.ReadInt16());
			int radiusX = reader.ReadInt16();
			int radiusY = reader.ReadInt16();
			bool major = reader.ReadBoolean();
			bool compact = reader.ReadBoolean();
			Point16 root = new(reader.ReadInt16(), reader.ReadInt16());
			int count = reader.ReadUInt16();
			List<Point16> spine = new(count);
			for (int i = 0; i < count; i++)
				spine.Add(new Point16(reader.ReadInt16(), reader.ReadInt16()));
			return new MawRupturePlan(surface, radiusX, radiusY, major, compact, root, spine);
		}
	}
}
