using System.IO;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace apogean.Common.WorldGeneration
{
	public sealed class MawRupturePlan
	{
		public Point16 SurfaceCenter { get; }
		public int RadiusX { get; }
		public int RadiusY { get; }
		public bool IsMajor { get; }

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

				int x = SurfaceCenter.X - RadiusX - 40;
				int y = System.Math.Max(20, SurfaceCenter.Y - RadiusY - 24);
				return new Rectangle(x, y, (RadiusX + 40) * 2 + 1, Terraria.Main.maxTilesY - y - 140);
			}
		}

		public MawRupturePlan(Point16 surfaceCenter, int radiusX, int radiusY, bool isMajor)
		{
			SurfaceCenter = surfaceCenter;
			RadiusX = radiusX;
			RadiusY = radiusY;
			IsMajor = isMajor;
		}

		internal TagCompound Save() => new()
		{
			["x"] = (int)SurfaceCenter.X,
			["y"] = (int)SurfaceCenter.Y,
			["radiusX"] = RadiusX,
			["radiusY"] = RadiusY,
			["major"] = IsMajor
		};

		internal static MawRupturePlan Load(TagCompound tag) => new(
			new Point16(tag.GetInt("x"), tag.GetInt("y")),
			tag.GetInt("radiusX"),
			tag.GetInt("radiusY"),
			tag.GetBool("major"));

		internal void NetSend(BinaryWriter writer)
		{
			writer.Write(SurfaceCenter.X);
			writer.Write(SurfaceCenter.Y);
			writer.Write((short)RadiusX);
			writer.Write((short)RadiusY);
			writer.Write(IsMajor);
		}

		internal static MawRupturePlan NetReceive(BinaryReader reader) => new(
			new Point16(reader.ReadInt16(), reader.ReadInt16()),
			reader.ReadInt16(),
			reader.ReadInt16(),
			reader.ReadBoolean());
	}
}
