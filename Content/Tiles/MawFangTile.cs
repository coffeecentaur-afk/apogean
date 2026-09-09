using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.Geometry;

namespace apogean.Content.Tiles
{
	// Candidate only. Authored sloped cells, not a rectangular furniture collision box.
	public sealed class MawFangTile : ModTile
	{
		public const string CandidatePath = "Content/Tiles/Diagnostics/MawFangTile";
		private static bool changing;
		public override string Texture => $"{Mod.Name}/{CandidatePath}";
		internal static bool CandidateIncluded(Mod mod) => mod.FileExists(CandidatePath + ".rawimg") || mod.FileExists(CandidatePath + ".png");
		public override bool IsLoadingEnabled(Mod mod) => CandidateIncluded(mod);
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileNoAttach[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			// Native touch sets use coarse tile rectangles. Contact follows the slope instead.
			TileID.Sets.TouchDamageImmediate[Type] = 0;
			MinPick = 0; MineResist = 0.8f; DustType = DustID.Bone; HitSound = SoundID.Dig;
			AddMapEntry(new Color(158, 143, 116));
		}
		internal static bool Part(int i, int j, int type, out Point origin, out int orientation)
		{
			origin = default; orientation = 0;
			if (!WorldGen.InWorld(i, j, 2)) return false;
			Tile t = Main.tile[i, j];
			if (!t.HasTile || t.TileType != type) return false;
			int part = MawFangShape.Decode(t.TileFrameX, t.TileFrameY);
			if (part < 0) return false;
			orientation = part / 9; origin = new Point(i - part % 3, j - part % 9 / 3);
			return true;
		}
		internal static bool Supported(Point origin, int o)
		{
			for (int k = 0; k < 2; k++) {
				int x = k, y = 3; MawFangShape.Rotate(ref x, ref y, o);
				int i = origin.X + x, j = origin.Y + y;
				if (!WorldGen.InWorld(i, j, 2)) return false;
				Tile t = Main.tile[i, j];
				if (!t.HasTile || t.IsActuated || !Main.tileSolid[t.TileType] || t.IsHalfBlock || t.Slope != SlopeType.Solid) return false;
			}
			return true;
		}
		internal static bool TryPlace(Point origin, int o, int type)
		{
			if (o < 0 || o > 3 || !WorldGen.InWorld(origin.X, origin.Y, 3) || !WorldGen.InWorld(origin.X + 3, origin.Y + 3, 3) || !Supported(origin, o)) return false;
			for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++) if (Main.tile[origin.X + x, origin.Y + y].HasTile) return false;
			changing = true;
			try {
				for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++) {
					int s = MawFangShape.Slope(x, y, o); if (s < 0) continue;
					Tile t = Main.tile[origin.X + x, origin.Y + y];
					t.HasTile = true; t.TileType = (ushort)type; t.IsHalfBlock = false; t.Slope = (SlopeType)s;
					t.TileFrameX = (short)(x * 18); t.TileFrameY = (short)((o * 3 + y) * 18);
				}
			} finally { changing = false; }
			WorldGen.RangeFrame(origin.X - 1, origin.Y - 1, origin.X + 4, origin.Y + 4);
			return true;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			if (!changing && Part(i, j, Type, out Point root, out int o)) {
				bool intact = Supported(root, o);
				for (int x = 0; x < 3 && intact; x++) for (int y = 0; y < 3 && intact; y++) {
					if (MawFangShape.Slope(x, y, o) < 0) continue;
					intact = Part(root.X + x, root.Y + y, Type, out Point other, out int facing) && other == root && facing == o;
				}
				if (!intact) WorldGen.KillTile(i, j);
			}
			return false;
		}
		public override bool Slope(int i, int j) => false;
		public override bool CanExplode(int i, int j) => true;
		public override IEnumerable<Item> GetItemDrops(int i, int j)
		{
			Item bone = new(); bone.SetDefaults(ItemID.Bone); yield return bone;
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (changing || fail || effectOnly || !Part(i, j, Type, out Point root, out int o)) return;
			changing = true;
			try {
				for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++) {
					int a = root.X + x, b = root.Y + y;
					if ((a != i || b != j) && Part(a, b, Type, out Point other, out int facing) && other == root && facing == o)
						WorldGen.KillTile(a, b, noItem: true);
				}
			} finally { changing = false; }
		}
		internal static bool Touching(Player player, int type)
		{
			int left = (int)(player.position.X / 16) - 1, top = (int)(player.position.Y / 16) - 1;
			int right = (int)((player.position.X + player.width) / 16) + 1, bottom = (int)((player.position.Y + player.height) / 16) + 1;
			for (int x = left; x <= right; x++) for (int y = top; y <= bottom; y++) {
				if (!Part(x, y, type, out _, out _)) continue;
				Tile t = Main.tile[x, y]; if (t.IsActuated) continue;
				if (MawFangShape.Touches(player.position.X - x * 16, player.position.Y - y * 16,
					player.position.X + player.width - x * 16, player.position.Y + player.height - y * 16, (int)t.Slope)) return true;
			}
			return false;
		}
		internal static double ApplyContact(Player player, int type)
		{
			if (player.dead || player.immune || !Touching(player, type)) return 0;
			return player.Hurt(PlayerDeathReason.ByOther(3), 30, 0);
		}
	}

	public sealed class MawFangContactPlayer : ModPlayer
	{
		public override bool IsLoadingEnabled(Mod mod) => MawFangTile.CandidateIncluded(mod);
		public override void PostUpdate()
		{
			// Owner-side, same authority as ordinary player environmental damage.
			if (!Main.gameMenu && Main.netMode != NetmodeID.Server && Player.whoAmI == Main.myPlayer)
				MawFangTile.ApplyContact(Player, ModContent.TileType<MawFangTile>());
		}
	}
}
