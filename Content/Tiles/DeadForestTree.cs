using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>
	/// A full Terraria tree species rooted in DeadGrass. It preserves chopping,
	/// acorns, regrowth, tree shaking, and ordinary wood without a green canopy.
	/// </summary>
	public sealed class DeadForestTree : ModTree
	{
		private Asset<Texture2D> trunkTexture;
		private Asset<Texture2D> branchTexture;
		private Asset<Texture2D> topTexture;

		public override TreePaintingSettings TreeShaderSettings => new()
		{
			UseSpecialGroups = true,
			SpecialGroupMinimalHueValue = 0.07f,
			SpecialGroupMaximumHueValue = 0.16f,
			SpecialGroupMinimumSaturationValue = 0.25f,
			SpecialGroupMaximumSaturationValue = 0.65f
		};

		public override TreeTypes CountsAsTreeType => TreeTypes.Forest;

		public override void SetStaticDefaults()
		{
			GrowsOnTileId = new[]
			{
				ModContent.TileType<WastesGrass>(),
				ModContent.TileType<DeadGrass>()
			};
			// Use Terraria's segmented tree renderer directly. Each trunk tile now owns
			// its visible bark and branches, so chopping removes the struck segment and
			// the tree above it instead of rescaling one monolithic overlay.
			trunkTexture = ModContent.Request<Texture2D>("apogean/Content/Tiles/DeadForestTree");
			branchTexture = ModContent.Request<Texture2D>("apogean/Content/Tiles/DeadForestTree_Branches");
			topTexture = ModContent.Request<Texture2D>("apogean/Content/Tiles/DeadForestTree_Tops");
		}

		public override Asset<Texture2D> GetTexture() => trunkTexture;
		public override Asset<Texture2D> GetBranchTextures() => branchTexture;
		public override Asset<Texture2D> GetTopTextures() => topTexture;

		public override int SaplingGrowthType(ref int style)
		{
			style = 0;
			return ModContent.TileType<DeadForestSapling>();
		}

		public override int DropWood() => ItemID.Wood;
		public override bool CanDropAcorn() => true;
		public override int CreateDust() => DustID.WoodFurniture;
		public override int TreeLeaf() => -1;

		public override bool Shake(int x, int y, ref bool createLeaves)
		{
			createLeaves = false;
			return true;
		}
	}
}
