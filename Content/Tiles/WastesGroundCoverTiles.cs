using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
	/// Draws each brittle multi-tile prop as one logical sprite. Terraria normally draws each
	/// occupied tile cell independently; this renderer removes the visible centre split and
	/// guarantees that player contact can never bend one half away from the other.
	/// </summary>
	public abstract class WastesRigidPlantTile : ModTile
	{
		private Asset<Texture2D> wholeTexture;

		protected abstract int ObjectWidth { get; }
		protected abstract int ObjectHeight { get; }
		protected abstract int VisualStyleCount { get; }
		protected virtual int VisualDrawYOffset => 4;

		public override void Load()
		{
			if (!Main.dedServ)
				wholeTexture = ModContent.Request<Texture2D>(Texture + "_Whole");
		}

		public override void Unload() => wholeTexture = null;

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
			if (wholeTexture is null)
				return;

			Tile tile = Framing.GetTileSafely(i, j);
			int framedObjectWidth = ObjectWidth * 18;
			int style = System.Math.Abs(tile.TileFrameX / framedObjectWidth) % VisualStyleCount;
			int logicalWidth = ObjectWidth * 16;
			int logicalHeight = ObjectHeight * 16;
			Rectangle source = new(style * logicalWidth, 0, logicalWidth, logicalHeight);
			Vector2 position = new Vector2(i * 16f, j * 16f + VisualDrawYOffset) - Main.screenPosition;
			spriteBatch.Draw(wholeTexture.Value, position, source, Lighting.GetColor(i, j));
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
