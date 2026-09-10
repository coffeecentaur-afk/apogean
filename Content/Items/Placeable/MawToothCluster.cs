using Terraria;
using Terraria.ModLoader;
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
	}
}
