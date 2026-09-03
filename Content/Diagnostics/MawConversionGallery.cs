using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Items.Placeable;
using apogean.Content.Projectiles;
using apogean.Content.Tiles;
using apogean.Content.Walls;
using apogean.Content.World;

namespace apogean.Content.Diagnostics
{
	/// <summary>
	/// Four-stage renderer and behavior fixture for natural source → Maw → Wastes → vanilla conversion.
	/// All transitions run through the production conversion hooks rather than placing expected tiles directly.
	/// </summary>
	internal static class MawConversionGallery
	{
		private const int CellWidth = 13;
		private const int StageHeight = 8;
		private const int StageCount = 4;
		private const int GuardHeight = 5;

		public static Rectangle Build(Player player, out IReadOnlyList<string> columns, out IReadOnlyList<string> stages)
		{
			Material[] materials = CreateMaterials();
			int width = materials.Length * CellWidth;
			int height = StageCount * StageHeight + GuardHeight;
			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - width / 2, 20, Main.maxTilesX - width - 20);
			int top = Math.Clamp(playerTile.Y - height / 2, 20, Main.maxTilesY - height - 20);
			Rectangle bounds = new(left, top, width, height);

			Clear(bounds);
			for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
			{
				Material material = materials[materialIndex];
				for (int stage = 0; stage < StageCount; stage++)
					PlaceSourceSample(left + materialIndex * CellWidth, top + stage * StageHeight, material);
			}

			// Stage 0 preserves its source. Stages 1–3 become Maw through the registered production map.
			for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
			{
				for (int stage = 1; stage < StageCount; stage++)
					ConvertSampleToMaw(left + materialIndex * CellWidth, top + stage * StageHeight);
				PurifySample(left + materialIndex * CellWidth, top + 2 * StageHeight);
				PurifySample(left + materialIndex * CellWidth, top + 3 * StageHeight);
				PurifySample(left + materialIndex * CellWidth, top + 3 * StageHeight);
			}

			PlaceAndValidatePreservedContent(left, top + StageCount * StageHeight + 1);
			Frame(bounds);
			ValidateResults(left, top, materials);
			ValidateMawRuntimeContracts();
			columns = Array.ConvertAll(materials, material => material.Name);
			stages = ["natural source", "Maw conversion", "one Purity: Wastes", "two Purity: vanilla"];
			return bounds;
		}

		private static void PlaceSourceSample(int left, int top, Material material)
		{
			for (int x = left + 1; x <= left + 10; x++)
			for (int y = top + 1; y <= top + 4; y++)
				Framing.GetTileSafely(x, y).WallType = (ushort)material.SourceWall;

			for (int x = left + 1; x <= left + 7; x++)
				SetTile(x, top + 4, material.SourceTile);
			for (int x = left + 1; x <= left + 4; x++)
				SetTile(x, top + 3, material.SourceTile);
			SetTile(left + 1, top + 2, material.SourceTile);
			SetTile(left + 4, top + 2, material.SourceTile);
			for (int x = left; x < left + CellWidth; x++)
				SetTile(x, top + 5, TileID.GrayBrick);
		}

		private static void ConvertSampleToMaw(int left, int top)
		{
			for (int x = left + 1; x <= left + 10; x++)
			for (int y = top + 1; y <= top + 4; y++)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				MawConversionSystem.ConvertAt(x, y, tile.HasTile, tile.WallType != WallID.None);
			}
		}

		private static void PurifySample(int left, int top)
		{
			for (int x = left + 1; x <= left + 10; x++)
			for (int y = top + 1; y <= top + 4; y++)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				if (tile.HasTile)
					TileLoader.Convert(x, y, BiomeConversionID.Purity);
				if (tile.WallType != WallID.None)
					WallLoader.Convert(x, y, BiomeConversionID.Purity);
			}
		}

		private static void ValidateResults(int left, int top, Material[] materials)
		{
			for (int index = 0; index < materials.Length; index++)
			{
				Material material = materials[index];
				int sampleLeft = left + index * CellWidth;
				RequireStage(sampleLeft, top, material.SourceTile, material.SourceWall, material.Name, "natural source");
				RequireStage(sampleLeft, top + StageHeight, material.MawTile, material.MawWall, material.Name, "Maw conversion");
				RequireStage(sampleLeft, top + 2 * StageHeight, material.WastesTile, material.WastesWall, material.Name, "one Purity pass");
				RequireStage(sampleLeft, top + 3 * StageHeight, material.VanillaTile, material.VanillaWall, material.Name, "two Purity passes");
			}
		}

		private static void PlaceAndValidatePreservedContent(int left, int y)
		{
			int vanillaX = left + 2;
			SetTile(vanillaX, y, TileID.GrayBrick);
			Tile vanillaStructure = Framing.GetTileSafely(vanillaX, y);
			vanillaStructure.WallType = WallID.Wood;
			vanillaStructure.RedWire = true;
			bool vanillaChanged = MawConversionSystem.ConvertAt(vanillaX, y, convertTile: true, convertWall: true);
			Require(!vanillaChanged && vanillaStructure.TileType == TileID.GrayBrick &&
				vanillaStructure.WallType == WallID.Wood && vanillaStructure.RedWire,
				"Maw conversion consumed constructed vanilla content or its wiring.");

			int corporateX = left + 6;
			int corporateTile = ModContent.TileType<KesslerBlock>();
			int corporateWall = ModContent.WallType<KesslerBulkheadWall>();
			SetTile(corporateX, y, corporateTile);
			Tile corporateStructure = Framing.GetTileSafely(corporateX, y);
			corporateStructure.WallType = (ushort)corporateWall;
			bool corporateChanged = MawConversionSystem.ConvertAt(corporateX, y, convertTile: true, convertWall: true);
			Require(!corporateChanged && corporateStructure.TileType == corporateTile && corporateStructure.WallType == corporateWall,
				"Maw conversion consumed an unregistered modded construction family.");
		}

		private static void RequireStage(int left, int top, int expectedTile, int expectedWall, string material, string stage)
		{
			Tile tileProbe = Framing.GetTileSafely(left + 2, top + 3);
			Tile wallProbe = Framing.GetTileSafely(left + 9, top + 2);
			Require(tileProbe.HasTile && tileProbe.TileType == expectedTile,
				$"{material} {stage} tile was {tileProbe.TileType}, expected {expectedTile}.");
			Require(wallProbe.WallType == expectedWall,
				$"{material} {stage} wall was {wallProbe.WallType}, expected {expectedWall}.");
		}

		private static void ValidateMawRuntimeContracts()
		{
			int dirt = ModContent.TileType<MawDirt>();
			int stone = ModContent.TileType<Mawstone>();
			int grass = ModContent.TileType<MawGrass>();
			int sand = ModContent.TileType<MawSand>();
			int ice = ModContent.TileType<MawIce>();
			int snow = ModContent.TileType<MawSnow>();
			int mud = ModContent.TileType<MawMud>();
			int sandItem = ModContent.ItemType<MawSandBlock>();

			Require(Main.tileSolid[dirt] && Main.tileSolid[stone] && Main.tileSolid[grass] &&
				Main.tileSolid[sand] && Main.tileSolid[ice] && Main.tileSolid[snow] && Main.tileSolid[mud],
				"Every primary Maw terrain material must be solid.");
			Require(TileID.Sets.Stone[stone], "Mawstone must participate in stone behavior.");
			Require(TileID.Sets.Grass[grass] && TileID.Sets.Conversion.Grass[grass],
				"Maw Grass must participate in grass framing and conversion.");
			Require(Main.tileSand[sand] && TileID.Sets.Falling[sand] && TileID.Sets.Suffocate[sand] &&
				TileID.Sets.CanBeDugByShovel[sand], "Maw Sand must implement Terraria sand behavior.");
			Require(TileID.Sets.FallingBlockProjectile[sand]?.FallingProjectileType ==
				ModContent.ProjectileType<MawSandBallFallingProjectile>(),
				"Maw Sand must use its identity-preserving falling projectile.");
			ProjectileID.Sets.FallingBlockTileItemInfo fallingSand =
				ProjectileID.Sets.FallingBlockTileItem[ModContent.ProjectileType<MawSandBallFallingProjectile>()];
			ItemID.Sets.SandgunAmmoInfo sandgunAmmo = ItemID.Sets.SandgunAmmoProjectileData[sandItem];
			Require(fallingSand?.TileType == sand && fallingSand.ItemType == sandItem,
				"Falling Maw Sand must recover its own tile and item.");
			Require(sandgunAmmo?.ProjectileType == ModContent.ProjectileType<MawSandBallGunProjectile>() &&
				sandgunAmmo.BonusDamage == 10, "Maw Sand must be custom Sandgun ammunition.");
			Require(TileID.Sets.IceSkateSlippery[ice] && TileID.Sets.Ices[ice] && TileID.Sets.IcesSlush[ice],
				"Maw Ice must participate in ice movement and framing.");
			Require(TileID.Sets.Snow[snow], "Maw Snow must participate in snow framing.");
			Require(TileLoader.GetItemDropFromTypeAndStyle(dirt) == ModContent.ItemType<MawDirtBlock>() &&
				TileLoader.GetItemDropFromTypeAndStyle(grass) == ModContent.ItemType<MawDirtBlock>() &&
				TileLoader.GetItemDropFromTypeAndStyle(stone) == ModContent.ItemType<MawstoneBlock>() &&
				TileLoader.GetItemDropFromTypeAndStyle(sand) == sandItem &&
				TileLoader.GetItemDropFromTypeAndStyle(ice) == ModContent.ItemType<MawIceBlock>() &&
				TileLoader.GetItemDropFromTypeAndStyle(snow) == ModContent.ItemType<MawSnowBlock>() &&
				TileLoader.GetItemDropFromTypeAndStyle(mud) == ModContent.ItemType<MawMudBlock>(),
				"Every primary Maw terrain tile must drop its own custom material; grass returns Maw Dirt.");
		}

		private static void SetTile(int x, int y, int type)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			tile.HasTile = true;
			tile.TileType = (ushort)type;
			tile.TileFrameX = 0;
			tile.TileFrameY = 0;
			tile.Slope = SlopeType.Solid;
			tile.IsHalfBlock = false;
			tile.LiquidAmount = 0;
		}

		private static void Clear(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
				Framing.GetTileSafely(x, y).ClearEverything();
		}

		private static void Frame(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
			{
				WorldGen.SquareTileFrame(x, y, true);
				WorldGen.SquareWallFrame(x, y, true);
			}
		}

		private static void Require(bool condition, string message)
		{
			if (!condition)
				throw new InvalidOperationException(message);
		}

		private static Material[] CreateMaterials() =>
		[
			new("Wastes Soil", ModContent.TileType<WastesSoil>(), ModContent.TileType<MawDirt>(), TileID.Dirt,
				ModContent.WallType<WastesDirtWallUnsafe>(), ModContent.WallType<MawDirtWallUnsafe>(), WallID.DirtUnsafe),
			new("Wastes Grass", ModContent.TileType<WastesGrass>(), ModContent.TileType<MawGrass>(), TileID.Grass,
				ModContent.WallType<WastesGrassWallUnsafe>(), ModContent.WallType<MawGrassWallUnsafe>(), WallID.GrassUnsafe),
			new("Wastes Stone", ModContent.TileType<WastesStone>(), ModContent.TileType<Mawstone>(), TileID.Stone,
				ModContent.WallType<WastesStoneWallUnsafe>(), ModContent.WallType<MawStoneWallUnsafe>(), WallID.Stone),
			new("Wastes Sand", ModContent.TileType<WastesSand>(), ModContent.TileType<MawSand>(), TileID.Sand,
				ModContent.WallType<WastesSandWallUnsafe>(), ModContent.WallType<MawSandWallUnsafe>(), WallID.Sandstone),
			new("Wastes Ice", ModContent.TileType<WastesIce>(), ModContent.TileType<MawIce>(), TileID.IceBlock,
				ModContent.WallType<WastesIceWallUnsafe>(), ModContent.WallType<MawIceWallUnsafe>(), WallID.IceUnsafe),
			new("Wastes Snow", ModContent.TileType<WastesSnow>(), ModContent.TileType<MawSnow>(), TileID.SnowBlock,
				ModContent.WallType<WastesSnowWallUnsafe>(), ModContent.WallType<MawSnowWallUnsafe>(), WallID.SnowWallUnsafe),
			new("Wastes Mud", ModContent.TileType<WastesMud>(), ModContent.TileType<MawMud>(), TileID.Mud,
				ModContent.WallType<WastesMudWallUnsafe>(), ModContent.WallType<MawMudWallUnsafe>(), WallID.MudUnsafe),
			new("Corrupt Stone", TileID.Ebonstone, ModContent.TileType<Mawstone>(), TileID.Stone,
				WallID.EbonstoneUnsafe, ModContent.WallType<MawStoneWallUnsafe>(), WallID.Stone,
				ModContent.TileType<WastesStone>(), ModContent.WallType<WastesStoneWallUnsafe>()),
			new("Crimson Sand", TileID.Crimsand, ModContent.TileType<MawSand>(), TileID.Sand,
				WallID.CrimsonHardenedSand, ModContent.WallType<MawSandWallUnsafe>(), WallID.Sandstone,
				ModContent.TileType<WastesSand>(), ModContent.WallType<WastesSandWallUnsafe>()),
			new("Hallow Ice", TileID.HallowedIce, ModContent.TileType<MawIce>(), TileID.IceBlock,
				WallID.HallowUnsafe1, ModContent.WallType<MawStoneWallUnsafe>(), WallID.Stone,
				ModContent.TileType<WastesIce>(), ModContent.WallType<WastesStoneWallUnsafe>()),
			new("Jungle Grass", TileID.JungleGrass, ModContent.TileType<MawGrass>(), TileID.Grass,
				WallID.JungleUnsafe, ModContent.WallType<MawGrassWallUnsafe>(), WallID.GrassUnsafe,
				ModContent.TileType<WastesGrass>(), ModContent.WallType<WastesGrassWallUnsafe>()),
			new("Mushroom Grass", TileID.MushroomGrass, ModContent.TileType<MawGrass>(), TileID.Grass,
				WallID.MushroomUnsafe, ModContent.WallType<MawGrassWallUnsafe>(), WallID.GrassUnsafe,
				ModContent.TileType<WastesGrass>(), ModContent.WallType<WastesGrassWallUnsafe>()),
			new("Underworld Ash", TileID.Ash, ModContent.TileType<MawDirt>(), TileID.Dirt,
				WallID.LavaUnsafe1, ModContent.WallType<MawStoneWallUnsafe>(), WallID.Stone,
				ModContent.TileType<WastesSoil>(), ModContent.WallType<WastesStoneWallUnsafe>())
		];

		private readonly record struct Material(
			string Name,
			int SourceTile,
			int MawTile,
			int VanillaTile,
			int SourceWall,
			int MawWall,
			int VanillaWall,
			int ExpectedWastesTile = -1,
			int ExpectedWastesWall = -1)
		{
			public int WastesTile { get; } = ExpectedWastesTile < 0 ? SourceTile : ExpectedWastesTile;
			public int WastesWall { get; } = ExpectedWastesWall < 0 ? SourceWall : ExpectedWastesWall;
		}
	}
}
