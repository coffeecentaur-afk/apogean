using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Walls
{
	/// <summary>Background construction used inside a sealed day-one corporate Campus.</summary>
	public abstract class ProtectedCorporateWall : ModWall
	{
		protected abstract Color MapColor { get; }
		protected abstract int WallDust { get; }

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = true;
			DustType = WallDust;
			AddMapEntry(MapColor);
		}

		public override bool CanExplode(int i, int j) => false;

		public override void KillWall(int i, int j, ref bool fail) => fail = true;
	}

	public sealed class KesslerBulkheadWall : ProtectedCorporateWall
	{
		protected override Color MapColor => new(53, 50, 48);
		protected override int WallDust => DustID.Titanium;
	}

	public sealed class KesslerWindowWall : ProtectedCorporateWall
	{
		protected override Color MapColor => new(74, 57, 47);
		protected override int WallDust => DustID.Glass;
	}

	public sealed class HelixLaboratoryWall : ProtectedCorporateWall
	{
		protected override Color MapColor => new(102, 112, 107);
		protected override int WallDust => DustID.SilverCoin;
	}

	public sealed class HelixObservationWall : ProtectedCorporateWall
	{
		protected override Color MapColor => new(72, 104, 83);
		protected override int WallDust => DustID.Glass;
	}

	public sealed class SentrixDataWall : ProtectedCorporateWall
	{
		protected override Color MapColor => new(24, 45, 59);
		protected override int WallDust => DustID.Electric;
	}

	public sealed class SentrixWindowWall : ProtectedCorporateWall
	{
		protected override Color MapColor => new(31, 75, 94);
		protected override int WallDust => DustID.Glass;
	}
}
