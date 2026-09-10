using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using apogean.Common.Geometry;
using apogean.Content.Tiles;

namespace apogean.Content.Items.Placeable
{
	public sealed class MawToothCluster : ModItem
	{
		public override string Texture => Mod.Name + "/" + MawToothClusterTile.AssetRoot + "cluster";
		public override bool IsLoadingEnabled(Mod mod) => MawToothClusterTile.Included(mod);
		public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<MawToothClusterTile>());
			Item.width = 32; Item.height = 32; Item.scale = 0.5f;
			Item.maxStack = 9999; Item.value = 0;
		}
		public override void HoldItem(Player player) => SelectCurve(player);
		public override bool CanUseItem(Player player)
		{
			SelectCurve(player); // Re-evaluate at the click, not only while idle.
			return true;
		}
		internal void SelectCurve(Player player)
		{
			if (Main.netMode == Terraria.ID.NetmodeID.Server || player.whoAmI != Main.myPlayer) return;
			Item.placeStyle = 0;
			if (ModContent.GetInstance<MawToothClusterTile>().VariantCount < 8) return;
			// Read-only native anchor selection, with the exact same origin for both
			// curves. Native preview/place then use this saved style; no post-place flip.
			if (!TileObject.CanPlace(Player.tileTargetX, Player.tileTargetY, Item.createTile, 0, player.direction, out TileObject placement)) return;
			int surface = TileObjectData.GetTileData(Item.createTile, 0, placement.alternate).Style;
			Item.placeStyle = MawToothPlacement.CurveStyle(surface, player.direction, player.Center.Y, placement.yCoord * 16 + MawToothClusterTile.Size / 2f);
		}
	}
}
