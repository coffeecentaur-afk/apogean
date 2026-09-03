using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	public abstract class WastesTerrainTile : ModTile
	{
		protected abstract Color MapColor { get; }
		protected virtual int ItemDrop => ItemID.DirtBlock;
		protected virtual int TileDust => DustID.Dirt;
		protected virtual float Resistance => 0.65f;
		protected abstract int RestoredTile { get; }

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			DustType = TileDust;
			HitSound = SoundID.Dig;
			MineResist = Resistance;
			if (ItemDrop > ItemID.None)
				RegisterItemDrop(ItemDrop);
			AddMapEntry(MapColor);
		}

		public override void Convert(int i, int j, int conversionType)
		{
			if (conversionType is BiomeConversionID.Purity or BiomeConversionID.PurificationPowder)
				WorldGen.ConvertTile(i, j, RestoredTile);
		}
	}

	public sealed class WastesSoil : WastesTerrainTile { protected override Color MapColor => new(99, 74, 50); protected override int RestoredTile => TileID.Dirt; }
	public sealed class WastesStone : WastesTerrainTile { protected override Color MapColor => new(91, 79, 68); protected override int RestoredTile => TileID.Stone; protected override int ItemDrop => ItemID.StoneBlock; protected override int TileDust => DustID.Stone; protected override float Resistance => 1f; }
	public sealed class WastesGrass : WastesTerrainTile
	{
		protected override Color MapColor => new(128, 94, 48);
		protected override int RestoredTile => TileID.Grass;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			TileID.Sets.Grass[Type] = true;
			TileID.Sets.Conversion.Grass[Type] = true;
			Main.tileMerge[Type][ModContent.TileType<WastesSoil>()] = true;
			Main.tileMerge[ModContent.TileType<WastesSoil>()][Type] = true;
		}
	}
	public sealed class WastesSand : WastesTerrainTile { protected override Color MapColor => new(149, 121, 72); protected override int RestoredTile => TileID.Sand; protected override int ItemDrop => ItemID.SandBlock; protected override int TileDust => DustID.Sand; protected override float Resistance => 0.5f; }
	public sealed class WastesIce : WastesTerrainTile
	{
		protected override Color MapColor => new(105, 121, 126);
		protected override int RestoredTile => TileID.IceBlock;
		protected override int ItemDrop => ItemID.IceBlock;
		protected override int TileDust => DustID.Ice;
		public override void SetStaticDefaults() { base.SetStaticDefaults(); TileID.Sets.IceSkateSlippery[Type] = true; }
	}
	public sealed class WastesSnow : WastesTerrainTile { protected override Color MapColor => new(173, 166, 145); protected override int RestoredTile => TileID.SnowBlock; protected override int ItemDrop => ItemID.SnowBlock; protected override int TileDust => DustID.Snow; }
	public sealed class WastesMud : WastesTerrainTile { protected override Color MapColor => new(79, 68, 48); protected override int RestoredTile => TileID.Mud; protected override int ItemDrop => ItemID.MudBlock; protected override int TileDust => DustID.Mud; }

	public abstract class MawNaturalTile : ModTile
	{
		protected abstract Color MapColor { get; }
		protected abstract int PurifiedTile { get; }
		protected virtual int ItemDrop => ItemID.DirtBlock;
		protected virtual int TileDust => DustID.AmberBolt;
		protected virtual float Resistance => 1.15f;

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			DustType = TileDust;
			HitSound = SoundID.Dig;
			MineResist = Resistance;
			if (ItemDrop > ItemID.None)
				RegisterItemDrop(ItemDrop);
			AddMapEntry(MapColor);
		}

		public override void Convert(int i, int j, int conversionType)
		{
			if (conversionType is BiomeConversionID.Purity or BiomeConversionID.PurificationPowder)
				WorldGen.ConvertTile(i, j, PurifiedTile);
		}
	}

	public sealed class MawDirt : MawNaturalTile { protected override Color MapColor => new(89, 67, 39); protected override int PurifiedTile => ModContent.TileType<WastesSoil>(); }
	public sealed class MawGrass : MawNaturalTile
	{
		protected override Color MapColor => new(142, 99, 28);
		protected override int PurifiedTile => ModContent.TileType<WastesGrass>();
		public override void SetStaticDefaults() { base.SetStaticDefaults(); TileID.Sets.Conversion.Grass[Type] = true; }
	}
	public sealed class MawSand : MawNaturalTile { protected override Color MapColor => new(151, 111, 40); protected override int PurifiedTile => ModContent.TileType<WastesSand>(); protected override int ItemDrop => ItemID.SandBlock; protected override int TileDust => DustID.Sand; }
	public sealed class MawIce : MawNaturalTile
	{
		protected override Color MapColor => new(112, 111, 77);
		protected override int PurifiedTile => ModContent.TileType<WastesIce>();
		protected override int ItemDrop => ItemID.IceBlock;
		public override void SetStaticDefaults() { base.SetStaticDefaults(); TileID.Sets.IceSkateSlippery[Type] = true; }
	}
	public sealed class MawSnow : MawNaturalTile { protected override Color MapColor => new(173, 153, 104); protected override int PurifiedTile => ModContent.TileType<WastesSnow>(); protected override int ItemDrop => ItemID.SnowBlock; protected override int TileDust => DustID.Snow; }
	public sealed class MawMud : MawNaturalTile { protected override Color MapColor => new(78, 62, 34); protected override int PurifiedTile => ModContent.TileType<WastesMud>(); protected override int ItemDrop => ItemID.MudBlock; protected override int TileDust => DustID.Mud; }
	public sealed class MawClay : MawNaturalTile { protected override Color MapColor => new(120, 88, 52); protected override int PurifiedTile => ModContent.TileType<WastesSoil>(); protected override int ItemDrop => ItemID.ClayBlock; }
}
