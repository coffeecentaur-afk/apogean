using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using apogean.Content.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Walls
{
	public abstract class MawNaturalWall : ModWall
	{
		public override string Texture => MawPackedPreview.Texture(MawPackedPreview.WallKey(Name),true,base.Texture);
		public override bool PreDraw(int i,int j,SpriteBatch spriteBatch) => MawPackedPreview.WallDraw(MawPackedPreview.WallKey(Name),Type,i,j,spriteBatch);
		protected abstract Color MapColor { get; }
		protected abstract int PurifiedWall { get; }
		protected virtual int WallDust => DustID.Dirt;

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			DustType = WallDust;
			AddMapEntry(MapColor);
		}

		public override void Convert(int i, int j, int conversionType)
		{
			if (conversionType is BiomeConversionID.Purity or BiomeConversionID.PurificationPowder)
				WorldGen.ConvertWall(i, j, PurifiedWall);
		}
    }

	public sealed class MawDirtWallUnsafe : MawNaturalWall { protected override Color MapColor => new(72, 54, 35); protected override int PurifiedWall => ModContent.WallType<WastesDirtWallUnsafe>(); }
	public sealed class MawStoneWallUnsafe : MawNaturalWall { protected override Color MapColor => new(64, 57, 45); protected override int PurifiedWall => ModContent.WallType<WastesStoneWallUnsafe>(); protected override int WallDust => DustID.Stone; }
	public sealed class MawGrassWallUnsafe : MawNaturalWall { protected override Color MapColor => new(96, 68, 31); protected override int PurifiedWall => ModContent.WallType<WastesGrassWallUnsafe>(); }
	public sealed class MawSandWallUnsafe : MawNaturalWall { protected override Color MapColor => new(112, 83, 40); protected override int PurifiedWall => ModContent.WallType<WastesSandWallUnsafe>(); protected override int WallDust => DustID.Sand; }
	public sealed class MawIceWallUnsafe : MawNaturalWall { protected override Color MapColor => new(83, 85, 67); protected override int PurifiedWall => ModContent.WallType<WastesIceWallUnsafe>(); protected override int WallDust => DustID.Ice; }
	public sealed class MawSnowWallUnsafe : MawNaturalWall { protected override Color MapColor => new(126, 113, 84); protected override int PurifiedWall => ModContent.WallType<WastesSnowWallUnsafe>(); protected override int WallDust => DustID.Snow; }
	public sealed class MawMudWallUnsafe : MawNaturalWall { protected override Color MapColor => new(58, 49, 30); protected override int PurifiedWall => ModContent.WallType<WastesMudWallUnsafe>(); protected override int WallDust => DustID.Mud; }
}
