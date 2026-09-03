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

	/// <summary>A tall, brittle Wastes stalk. Three horizontal styles share one 1x2 object sheet.</summary>
	public sealed class WastesBristle : ModTile
	{
		public override void SetStaticDefaults()
		{
			WastesPlantRegistration.ApplyCommon(this, new Color(143, 108, 61), sways: true);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.AnchorBottom = WastesPlantRegistration.GroundAnchor(1);
			TileObjectData.newTile.AnchorValidTiles = WastesPlantRegistration.ValidGround();
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

	/// <summary>A two-tile root arch with a restrained amber seed pod. It is woody and does not sway.</summary>
	public sealed class WastesRootShrub : ModTile
	{
		public override void SetStaticDefaults()
		{
			WastesPlantRegistration.ApplyCommon(this, new Color(112, 82, 45), sways: false);

			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.AnchorBottom = WastesPlantRegistration.GroundAnchor(2);
			TileObjectData.newTile.AnchorValidTiles = WastesPlantRegistration.ValidGround();
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
