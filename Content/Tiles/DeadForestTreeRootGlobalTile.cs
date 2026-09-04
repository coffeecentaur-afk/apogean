using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>
	/// Adds a bounded root flare to the bottom native tree segment. Unlike the rejected
	/// whole-tree overlay, this never scales with height: it disappears with the root tile
	/// and cannot change the way Terraria chops or saves the segmented tree above it.
	/// </summary>
	public sealed class DeadForestTreeRootGlobalTile : GlobalTile
	{
		private static Asset<Texture2D> roots;
		public override void Load() =>
			roots = ModContent.Request<Texture2D>("apogean/Content/Tiles/DeadForestTreeRoots");

		public override void Unload() => roots = null;

		public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
			if (type != TileID.Trees || roots is null)
				return;

			Tile ground = Framing.GetTileSafely(i, j + 1);
			if (!ground.HasTile || (ground.TileType != ModContent.TileType<WastesGrass>() &&
				ground.TileType != ModContent.TileType<DeadGrass>()))
				return;

			int variant = (int)((uint)(i * 1103515245 + j * 12345) % 3u);
			Rectangle source = new(variant * 48, 0, 48, 32);
			Vector2 offscreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			Vector2 position = new Vector2(i * 16 - 16, (j + 1) * 16 - source.Height) -
				Main.screenPosition + offscreen;
			spriteBatch.Draw(roots.Value, position, source, Lighting.GetColor(i, j));
		}
	}
}
