using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Content.Items.Placeable
{
	public sealed class ArrivalPod : ModItem
	{
		public override string Texture => $"{Mod.Name}/Content/Tiles/Diagnostics/ArrivalPodItem";
		public override bool IsLoadingEnabled(Mod mod) => ArrivalPodTile.CandidateIncluded(mod);

		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
			ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<ArrivalPodTile>());
			Item.width = 26;
			Item.height = 32;
			Item.scale = 1f / 3f; // Reuse approved 80x96 art; dropped pickup is not furniture-sized.
			Item.maxStack = 99;
			Item.value = 0;
		}
	}
}
