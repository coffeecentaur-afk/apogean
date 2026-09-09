using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace apogean.Content.Tiles
{
	// Native-render specimens only. Final solid/hurt geometry is a separate contract.
	public abstract class MawToothArtTile : ModTile
	{
		public const string AssetRoot = "Content/Tiles/Diagnostics/MawToothArt/";
		protected abstract string Variant { get; }
		protected abstract int Columns { get; }
		protected abstract int Rows { get; }
		public override string Texture => Mod.Name + "/" + AssetRoot + Variant;
		internal static bool CandidateIncluded(Mod mod)
		{
			foreach (string name in new[] { "short", "long", "wide" })
				if (!mod.FileExists(AssetRoot + name + ".rawimg") && !mod.FileExists(AssetRoot + name + ".png")) return false;
			return true;
		}
		public override bool IsLoadingEnabled(Mod mod) => CandidateIncluded(mod);
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileBlockLight[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.TouchDamageImmediate[Type] = 0;
			DustType = DustID.Bone; HitSound = SoundID.Dig;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = Columns;
			TileObjectData.newTile.Height = Rows;
			TileObjectData.newTile.Origin = new Point16(0, Rows - 1);
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			int[] heights = new int[Rows]; Array.Fill(heights, 16);
			TileObjectData.newTile.CoordinateHeights = heights;
			TileObjectData.newTile.DrawYOffset = 4;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, Columns, 0);
			TileObjectData.newTile.AnchorTop = AnchorData.Empty;
			TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
			TileObjectData.newTile.AnchorRight = AnchorData.Empty;
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(170, 158, 129));
		}
	}
	public sealed class MawShortToothArtTile : MawToothArtTile
	{
		protected override string Variant => "short";
		protected override int Columns => 1;
		protected override int Rows => 2;
	}
	public sealed class MawLongToothArtTile : MawToothArtTile
	{
		protected override string Variant => "long";
		protected override int Columns => 1;
		protected override int Rows => 3;
	}
	public sealed class MawWideToothArtTile : MawToothArtTile
	{
		protected override string Variant => "wide";
		protected override int Columns => 2;
		protected override int Rows => 4;
	}
}
