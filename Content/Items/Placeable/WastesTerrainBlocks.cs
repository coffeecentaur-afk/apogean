using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Projectiles;
using apogean.Content.Tiles;

namespace apogean.Content.Items.Placeable
{
	public abstract class WastesTerrainBlockItem<TTile> : ModItem where TTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 100;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<TTile>());
			Item.width = 16;
			Item.height = 16;
		}
	}

	public sealed class WastesSoilBlock : WastesTerrainBlockItem<WastesSoil> { }
	public sealed class WastesStoneBlock : WastesTerrainBlockItem<WastesStone> { }
	public sealed class WastesIceBlock : WastesTerrainBlockItem<WastesIce> { }
	public sealed class WastesSnowBlock : WastesTerrainBlockItem<WastesSnow> { }
	public sealed class WastesMudBlock : WastesTerrainBlockItem<WastesMud> { }

	public sealed class WastesSandBlock : WastesTerrainBlockItem<WastesSand>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ItemID.Sets.SandgunAmmoProjectileData[Type] = new(
				ModContent.ProjectileType<WastesSandBallGunProjectile>(), 10);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.width = 12;
			Item.height = 12;
			Item.ammo = AmmoID.Sand;
			Item.notAmmo = true;
		}
	}
}
