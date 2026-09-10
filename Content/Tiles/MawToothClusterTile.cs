using System;
using System.Collections.Generic;
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
	// User-approved thorn-style group; not a solid rectangular collider.
	public sealed class MawToothClusterTile : ModTile
	{
		public const string AssetRoot = "Content/Tiles/Diagnostics/MawToothCluster/";
		public const int Size = 64, DrawInset = 4, ContactDamage = 30;
		internal byte[] ContactMask { get; private set; }
		public override string Texture => Mod.Name + "/" + AssetRoot + "cluster-atlas";
		internal static bool Included(Mod mod) => mod.FileExists(AssetRoot + "contact-mask.bin") &&
			(mod.FileExists(AssetRoot + "cluster-atlas.png") || mod.FileExists(AssetRoot + "cluster-atlas.rawimg"));
		public override bool IsLoadingEnabled(Mod mod) => Included(mod);
		public override void SetStaticDefaults()
		{
			ContactMask = Mod.GetFileBytes(AssetRoot + "contact-mask.bin");
			if (ContactMask.Length != 4 * Size * Size || Array.Exists(ContactMask, b => b > 1)) throw new InvalidOperationException("Invalid four-way cluster contact mask.");
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false; Main.tileSolidTop[Type] = false;
			Main.tileNoAttach[Type] = true; Main.tileBlockLight[Type] = false;
			Main.tileLavaDeath[Type] = false; Main.tileWaterDeath[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.TouchDamageImmediate[Type] = 0; // Pixel-mask contact, not whole-cell rectangles.
			MinPick = 0; MineResist = 0.8f; DustType = DustID.Bone; HitSound = SoundID.Dig;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = 4; TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.Origin = new Point16(1, 3);
			TileObjectData.newTile.CoordinateWidth = 16; TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 4;
			TileObjectData.newTile.StyleWrapLimit = 4;
			TileObjectData.newTile.DrawYOffset = DrawInset;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 4, 0);
			TileObjectData.newTile.AnchorTop = AnchorData.Empty;
			TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
			TileObjectData.newTile.AnchorRight = AnchorData.Empty;
			TileObjectData.newTile.LavaDeath = false; TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.Allowed;
			for (int facing = 1; facing < 4; facing++) {
				TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
				TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
				TileObjectData.newAlternate.Origin = PlacementOrigin(facing);
				Point inset = Inset(facing);
				TileObjectData.newAlternate.DrawXOffset = inset.X;
				TileObjectData.newAlternate.DrawYOffset = inset.Y;
				var anchor = new AnchorData(AnchorType.SolidTile, 4, 0);
				if (facing == 1) TileObjectData.newAlternate.AnchorLeft = anchor;
				if (facing == 2) TileObjectData.newAlternate.AnchorTop = anchor;
				if (facing == 3) TileObjectData.newAlternate.AnchorRight = anchor;
				TileObjectData.addAlternate(facing);
			}
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(170, 158, 129), ModContent.GetInstance<MawToothCluster>().DisplayName);
			// DefaultToPlaceableTile owns native one-item recovery; never duplicate in KillMultiTile.
		}
		public override bool CanExplode(int i, int j) => true;
		public override bool IsTileDangerous(int i, int j, Player player) => !Main.tile[i, j].IsActuated;
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			int facing = Main.tile[i, j].TileFrameX / 72;
			offsetY = Inset(facing).Y;
			if (facing == 1 || facing == 3) {
				// Native TileDrawing centers width24 on the16px tile. Asymmetric
				// transparent padding moves the visible pixels into support by4px.
				// Saved frames and item previews still use the upper18px-stride bank.
				width = 24;
				tileFrameX = (short)(facing * 104 + Main.tile[i, j].TileFrameX % 72 / 18 * 26);
				tileFrameY = (short)(72 + Main.tile[i, j].TileFrameY);
			}
		}
		internal static Point Inset(int facing) => facing switch { 1 => new(-4, 0), 3 => new(4, 0), 2 => new(0, -4), _ => new(0, 4) };
		internal static Point16 PlacementOrigin(int facing) => facing switch { 1 => new(0, 1), 2 => new(1, 0), 3 => new(3, 1), _ => new(1, 3) };
		internal static Point Support(Point root, int facing, int cell) => facing switch {
			1 => new(root.X - 1, root.Y + cell), 2 => new(root.X + cell, root.Y - 1),
			3 => new(root.X + 4, root.Y + cell), _ => new(root.X + cell, root.Y + 4)
		};
		internal bool TryOrigin(int i, int j, out Point origin)
		{
			origin = default;
			if (!WorldGen.InWorld(i, j, 1)) return false;
			Tile t = Main.tile[i, j];
			if (!t.HasTile || t.TileType != Type || t.TileFrameX < 0 || t.TileFrameX >= 288 || t.TileFrameY < 0 || t.TileFrameY >= 72 || t.TileFrameX % 18 != 0 || t.TileFrameY % 18 != 0) return false;
			origin = new Point(i - t.TileFrameX % 72 / 18, j - t.TileFrameY / 18);
			return WorldGen.InWorld(origin.X, origin.Y, 1) && WorldGen.InWorld(origin.X + 3, origin.Y + 3, 1);
		}
		internal bool TouchesAt(Rectangle hitbox, Point root)
		{
			if (!TryOrigin(root.X, root.Y, out Point actualRoot) || actualRoot != root) return false;
			int facing = Main.tile[root.X, root.Y].TileFrameX / 72;
			Point inset = Inset(facing);
			int left = Math.Max(0, hitbox.Left - root.X * 16 - inset.X), top = Math.Max(0, hitbox.Top - root.Y * 16 - inset.Y);
			int right = Math.Min(Size, hitbox.Right - root.X * 16 - inset.X), bottom = Math.Min(Size, hitbox.Bottom - root.Y * 16 - inset.Y);
			for (int y = top; y < bottom; y++) for (int x = left; x < right; x++) {
				if (ContactMask[facing * Size * Size + y * Size + x] == 0) continue;
				int i = root.X + x / 16, j = root.Y + y / 16;
				if (TryOrigin(i, j, out Point actual) && actual == root && !Main.tile[i, j].IsActuated) return true;
			}
			return false;
		}
		internal bool Touching(Rectangle hitbox)
		{
			HashSet<Point> checkedRoots = new();
			int left = Math.Max(1, hitbox.Left / 16 - 1), right = Math.Min(Main.maxTilesX - 2, hitbox.Right / 16 + 1);
			int top = Math.Max(1, hitbox.Top / 16 - 1), bottom = Math.Min(Main.maxTilesY - 2, hitbox.Bottom / 16 + 1);
			for (int i = left; i <= right; i++) for (int j = top; j <= bottom; j++)
				if (TryOrigin(i, j, out Point root) && checkedRoots.Add(root) && TouchesAt(hitbox, root)) return true;
			return false;
		}
		internal double ApplyContact(Player player)
		{
			if (player.dead || player.immune || !Touching(player.Hitbox)) return 0;
			return player.Hurt(PlayerDeathReason.ByOther(3), ContactDamage, 0);
		}
	}
	public sealed class MawToothClusterContactPlayer : ModPlayer
	{
		public override bool IsLoadingEnabled(Mod mod) => MawToothClusterTile.Included(mod);
		public override void PostUpdate()
		{
			// Standard local-owner environmental damage; no damage from remote player copies.
			if (!Main.gameMenu && Main.netMode != NetmodeID.Server && Player.whoAmI == Main.myPlayer)
				ModContent.GetInstance<MawToothClusterTile>().ApplyContact(Player);
		}
	}
}
