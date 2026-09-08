using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using apogean.Content.Items.Placeable;

namespace apogean.Content.Tiles
{
	// Loaded only by the explicitly opted-in isolated candidate build until live approval.
	public sealed class ArrivalPodTile : ModTile
	{
		public const string CandidatePath = "Content/Tiles/Diagnostics/ArrivalPodTile";
		public override string Texture => $"{Mod.Name}/{CandidatePath}";
		internal static bool CandidateIncluded(Mod mod) => mod.FileExists(CandidatePath + ".rawimg") || mod.FileExists(CandidatePath + ".png");
		public override bool IsLoadingEnabled(Mod mod) => CandidateIncluded(mod);

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			Main.tileLavaDeath[Type] = false;
			Main.tileWaterDeath[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			DustType = DustID.Iron;
			HitSound = SoundID.Dig;
			MinPick = 0;
			MineResist = 1f;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = 5;
			TileObjectData.newTile.Height = 6;
			TileObjectData.newTile.Origin = new Point16(2, 5);
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16];
			TileObjectData.newTile.DrawYOffset = 0;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 5, 0);
			TileObjectData.newTile.AnchorTop = AnchorData.Empty;
			TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
			TileObjectData.newTile.AnchorRight = AnchorData.Empty;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.Allowed;
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(129, 112, 88), ModContent.GetInstance<ArrivalPod>().DisplayName);
			// Item.createTile provides the one native drop. No KillMultiTile/Item.NewItem duplication.
		}

		// A bomb can remove its floor (recovering the pod), but not vaporize the shell itself.
		public override bool CanExplode(int i, int j) => false;
	}
}
