using System.IO;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;

namespace apogean.Common.WorldGeneration
{
	public enum ApogeanLandmarkKind : byte
	{
		KesslerCampus,
		HelixCampus,
		SentrixCampus,
		AbandonedKesslerOutpost,
		AbandonedHelixLaboratory,
		CrashedSentrixRelay,
		PrewarTransitRuin,
		MawResearchSite
	}

	public sealed class ApogeanLandmarkPlan
	{
		public ApogeanLandmarkKind Kind { get; }
		public Rectangle Bounds { get; }
		public int Padding { get; }

		public bool IsCampus => Kind is
			ApogeanLandmarkKind.KesslerCampus or
			ApogeanLandmarkKind.HelixCampus or
			ApogeanLandmarkKind.SentrixCampus;

		public Rectangle ReservedBounds
		{
			get
			{
				Rectangle reserved = Bounds;
				reserved.Inflate(Padding, Padding);
				return reserved;
			}
		}

		public ApogeanLandmarkPlan(ApogeanLandmarkKind kind, Rectangle bounds, int padding)
		{
			Kind = kind;
			Bounds = bounds;
			Padding = padding;
		}

		internal TagCompound Save() => new()
		{
			["kind"] = (int)Kind,
			["x"] = Bounds.X,
			["y"] = Bounds.Y,
			["width"] = Bounds.Width,
			["height"] = Bounds.Height,
			["padding"] = Padding
		};

		internal static ApogeanLandmarkPlan Load(TagCompound tag) => new(
			(ApogeanLandmarkKind)tag.GetInt("kind"),
			new Rectangle(tag.GetInt("x"), tag.GetInt("y"), tag.GetInt("width"), tag.GetInt("height")),
			tag.GetInt("padding"));

		internal void NetSend(BinaryWriter writer)
		{
			writer.Write((byte)Kind);
			writer.Write((short)Bounds.X);
			writer.Write((short)Bounds.Y);
			writer.Write((short)Bounds.Width);
			writer.Write((short)Bounds.Height);
			writer.Write((byte)Padding);
		}

		internal static ApogeanLandmarkPlan NetReceive(BinaryReader reader) => new(
			(ApogeanLandmarkKind)reader.ReadByte(),
			new Rectangle(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()),
			reader.ReadByte());
	}
}
