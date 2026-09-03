using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>
	/// Keeps Terraria's real TileID.Trees column for chopping, shaking, regrowth,
	/// networking, and drops, while drawing the much wider ruined-tree silhouette
	/// required by the Wastes art direction.
	/// </summary>
	internal static class DeadForestTreeVisuals
	{
		private const int MaximumTreeHeight = 32;

		internal static bool IsRoot(int i, int j)
		{
			if (!WorldGen.InWorld(i, j, 2))
				return false;

			Tile tile = Main.tile[i, j];
			if (!tile.HasTile || tile.TileType != TileID.Trees)
				return false;

			Tile ground = Main.tile[i, j + 1];
			return ground.HasTile && IsWastesTreeSoil(ground.TileType);
		}

		internal static int FindTop(int i, int rootY)
		{
			int top = rootY;
			while (top > 1 && rootY - top < MaximumTreeHeight)
			{
				Tile above = Main.tile[i, top - 1];
				if (!above.HasTile || above.TileType != TileID.Trees)
					break;
				top--;
			}
			return top;
		}

		private static bool IsWastesTreeSoil(ushort tileType) =>
			tileType == ModContent.TileType<WastesGrass>() ||
			tileType == ModContent.TileType<DeadGrass>();
	}

	/// <summary>
	/// Draws one reference-faithful tree sprite per real Terraria tree root from
	/// inside the tile renderer, including capture-camera render targets.
	/// </summary>
	public sealed class DeadForestTreeRenderer : GlobalTile
	{
		private Asset<Texture2D> _treeTexture;

		public override void Load()
		{
			if (!Main.dedServ)
				_treeTexture = ModContent.Request<Texture2D>("apogean/Content/Tiles/DeadForestTreeOverlay", AssetRequestMode.ImmediateLoad);
		}

		public override void Unload() => _treeTexture = null;

		public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
			if (Main.dedServ || type != TileID.Trees || _treeTexture?.Value == null || !DeadForestTreeVisuals.IsRoot(i, j))
				return;

			Tile root = Main.tile[i, j];
			if (root.IsTileInvisible)
				return;

			Texture2D texture = _treeTexture.Value;
			int treeTop = DeadForestTreeVisuals.FindTop(i, j);
			int trunkTiles = j - treeTop + 1;
			int drawHeight = System.Math.Clamp((trunkTiles + 1) * 16, 176, 288);
			float scale = drawHeight / (float)texture.Height;
			Vector2 captureOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			Vector2 drawPosition = new((i + 0.5f) * 16f, (j + 1) * 16f + 4f);
			drawPosition = drawPosition - Main.screenPosition + captureOffset;
			Color lightColor = Lighting.GetColor(i, System.Math.Clamp(treeTop + trunkTiles / 2, 1, Main.maxTilesY - 2));
			SpriteEffects effects = ((i * 397) & 1) == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			spriteBatch.Draw(
				texture,
				drawPosition,
				null,
				lightColor,
				0f,
				new Vector2(texture.Width * 0.5f, texture.Height),
				scale,
				effects,
				0f);
		}
	}
}
