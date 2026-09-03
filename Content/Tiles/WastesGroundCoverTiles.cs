using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace apogean.Content.Tiles
{
	internal static class WastesPlantRegistration
	{
		internal static void ApplyCommon(ModTile tile, Color mapColor, bool sways)
		{
			int type = tile.Type;
			Main.tileFrameImportant[type] = true;
			Main.tileCut[type] = true;
			Main.tileNoAttach[type] = true;
			Main.tileLavaDeath[type] = true;
			Main.tileWaterDeath[type] = true;
			if (sways)
				TileID.Sets.SwaysInWindBasic[type] = true;
			TileMaterials.SetForTileId((ushort)type, TileMaterials._materialsByName["Plant"]);
			tile.HitSound = SoundID.Grass;
			tile.DustType = DustID.Dirt;
			tile.AddMapEntry(mapColor);
		}

		internal static AnchorData GroundAnchor(int width) => new(AnchorType.SolidTile, width, 0);
		internal static int[] ValidGround() => [ModContent.TileType<WastesGrass>()];
	}

	/// <summary>A tall, tangled Wastes root. Three horizontal styles share one 2x3 object sheet.</summary>
	public sealed class WastesBristle : ModTile
	{
		public override void SetStaticDefaults()
		{
			WastesPlantRegistration.ApplyCommon(this, new Color(143, 108, 61), sways: true);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(0, 2);
			TileObjectData.newTile.AnchorBottom = WastesPlantRegistration.GroundAnchor(2);
			TileObjectData.newTile.AnchorValidTiles = WastesPlantRegistration.ValidGround();
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 3;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.DrawFlipHorizontal = true;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.addTile(Type);
		}
	}

	/// <summary>A broad three-by-two root mass with restrained amber seed pods. It is woody and does not sway.</summary>
	public sealed class WastesRootShrub : ModTile
	{
		public override void SetStaticDefaults()
		{
			WastesPlantRegistration.ApplyCommon(this, new Color(112, 82, 45), sways: false);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(1, 1);
			TileObjectData.newTile.AnchorBottom = WastesPlantRegistration.GroundAnchor(3);
			TileObjectData.newTile.AnchorValidTiles = WastesPlantRegistration.ValidGround();
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 3;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.DrawFlipHorizontal = true;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.addTile(Type);
		}
	}
}
