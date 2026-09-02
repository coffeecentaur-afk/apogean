using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>Shared protection contract for day-one Campus structure blocks.</summary>
	public abstract class ProtectedCorporateTile : ModTile
	{
		protected abstract Color MapColor { get; }
		protected abstract int TileDust { get; }

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileNoAttach[Type] = true;
			MineResist = 100f;
			MinPick = 9999;
			DustType = TileDust;
			AddMapEntry(MapColor);
		}

		public override bool CanExplode(int i, int j) => false;
		public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
	}

	public sealed class KesslerPlating : ProtectedCorporateTile
	{
		protected override Color MapColor => new(89, 72, 67);
		protected override int TileDust => DustID.Titanium;
	}

	public sealed class HelixContainmentPanel : ProtectedCorporateTile
	{
		protected override Color MapColor => new(178, 187, 181);
		protected override int TileDust => DustID.SilverCoin;
	}

	public sealed class SentrixPanel : ProtectedCorporateTile
	{
		protected override Color MapColor => new(40, 66, 81);
		protected override int TileDust => DustID.Electric;
	}

	/// <summary>Mineable environmental shell for abandoned sites; curated salvage comes later.</summary>
	public abstract class RuinStructureTile : ModTile
	{
		protected abstract Color MapColor { get; }
		protected abstract int TileDust { get; }

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			MineResist = 1.4f;
			DustType = TileDust;
			AddMapEntry(MapColor);
		}
	}

	public sealed class KesslerRuinBlock : RuinStructureTile
	{
		protected override Color MapColor => new(92, 69, 58);
		protected override int TileDust => DustID.Iron;
	}

	public sealed class HelixRuinBlock : RuinStructureTile
	{
		protected override Color MapColor => new(151, 158, 148);
		protected override int TileDust => DustID.SilverCoin;
	}

	public sealed class SentrixRuinBlock : RuinStructureTile
	{
		protected override Color MapColor => new(39, 58, 69);
		protected override int TileDust => DustID.Electric;
	}

	public sealed class PrewarConcrete : RuinStructureTile
	{
		protected override Color MapColor => new(104, 96, 82);
		protected override int TileDust => DustID.Stone;
	}

	public sealed class MawResearchBlock : RuinStructureTile
	{
		protected override Color MapColor => new(86, 73, 48);
		protected override int TileDust => DustID.AmberBolt;
	}
}
