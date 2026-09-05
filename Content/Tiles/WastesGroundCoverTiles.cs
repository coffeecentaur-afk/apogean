using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
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

	/// <summary>
	/// Draws each brittle multi-tile prop in one rigid coordinate system. Native painted
	/// atlas cells exactly reconstruct the approved whole sprite, while retaining per-cell
	/// paint/coatings and never applying independent wind transforms to its halves.
	/// </summary>
	public abstract class WastesRigidPlantTile : ModTile
	{
		protected abstract int ObjectWidth { get; }
		protected abstract int ObjectHeight { get; }
		protected abstract int VisualStyleCount { get; }
		protected virtual int VisualDrawYOffset => 4;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (TileObjectData.IsTopLeft(i, j))
			{
				Main.instance.TilesRenderer.AddSpecialPoint(
					i,
					j,
					TileDrawing.TileCounterType.CustomNonSolid);
			}

			return false;
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Color ambient = Lighting.GetColor(i, j);
			Vector2 origin = new Vector2(i * 16f, j * 16f + VisualDrawYOffset) - Main.screenPosition;
			for (int dx = 0; dx < ObjectWidth; dx++)
				for (int dy = 0; dy < ObjectHeight; dy++)
				{
					Tile tile = Framing.GetTileSafely(i + dx, j + dy);
					if (!tile.HasTile || tile.TileType != Type || !TileDrawing.IsVisible(tile)) continue;
					Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i + dx, j + dy);
					Color color = tile.IsTileFullbright ? Color.White : ambient;
					// Match native actuation dimming; Tile.actColor is engine-internal.
					if (tile.IsActuated)
						color = new Color((byte)(color.R * 0.4), (byte)(color.G * 0.4), (byte)(color.B * 0.4), color.A);
					Rectangle source = new(tile.TileFrameX, tile.TileFrameY, 16, 16);
					spriteBatch.Draw(texture, origin + new Vector2(dx * 16, dy * 16), source, color);
				}
		}
	}

	/// <summary>A tall, tangled Wastes root. Three horizontal styles share one 2x3 object sheet.</summary>
	public sealed class WastesBristle : WastesRigidPlantTile
	{
		protected override int ObjectWidth => 2;
		protected override int ObjectHeight => 3;
		protected override int VisualStyleCount => 3;

		public override void SetStaticDefaults()
		{
			// Keep the complete two-wide root silhouette rigid. Terraria's basic wind
			// sway transforms each occupied cell separately and visibly splits it.
			WastesPlantRegistration.ApplyCommon(this, new Color(143, 108, 61), sways: false);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(0, 2);
			TileObjectData.newTile.AnchorBottom = WastesPlantRegistration.GroundAnchor(2);
			TileObjectData.newTile.AnchorValidTiles = WastesPlantRegistration.ValidGround();
			TileObjectData.newTile.DrawYOffset = 4;
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
	public sealed class WastesRootShrub : WastesRigidPlantTile
	{
		protected override int ObjectWidth => 3;
		protected override int ObjectHeight => 2;
		protected override int VisualStyleCount => 3;

		public override void SetStaticDefaults()
		{
			WastesPlantRegistration.ApplyCommon(this, new Color(112, 82, 45), sways: false);

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(1, 1);
			TileObjectData.newTile.AnchorBottom = WastesPlantRegistration.GroundAnchor(3);
			TileObjectData.newTile.AnchorValidTiles = WastesPlantRegistration.ValidGround();
			TileObjectData.newTile.DrawYOffset = 4;
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
