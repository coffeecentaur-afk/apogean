using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace apogean.Content.Tiles
{
	/// <summary>
	/// Signature three-by-four fixture used to prove each Campus's interior language at native
	/// scale. Full furniture families reuse this contract after the blockout is approved.
	/// </summary>
	public abstract class CorporateFixtureTile : ModTile
	{
		protected abstract Color MapColor { get; }
		protected abstract Color LightColor { get; }
		protected virtual int FrameTicks => 10;

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.Origin = new Point16(1, 3);
			TileObjectData.addTile(Type);
			AddMapEntry(MapColor);
			AnimationFrameHeight = 72;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			frameCounter++;
			if (frameCounter < FrameTicks)
				return;
			frameCounter = 0;
			frame = (frame + 1) % 4;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameX != 18 || tile.TileFrameY != 18)
				return;
			r = LightColor.R / 255f * 0.5f;
			g = LightColor.G / 255f * 0.5f;
			b = LightColor.B / 255f * 0.5f;
		}

		// Campus teardown will switch this to the world-level CEO defeat state when that state exists.
		public override bool CanExplode(int i, int j) => false;
		public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
	}

	public sealed class KesslerPowerArmorRack : CorporateFixtureTile
	{
		protected override Color MapColor => new(124, 65, 44);
		protected override Color LightColor => new(235, 82, 28);
		protected override int FrameTicks => 15;
	}

	public sealed class HelixSymbioteTank : CorporateFixtureTile
	{
		protected override Color MapColor => new(158, 178, 162);
		protected override Color LightColor => new(96, 210, 104);
	}

	public sealed class SentrixHologramCore : CorporateFixtureTile
	{
		protected override Color MapColor => new(49, 100, 125);
		protected override Color LightColor => new(64, 191, 235);
		protected override int FrameTicks => 7;
	}
}
