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
	/// Four-stage renderer and behavior fixture for Wastes → Maw → Wastes → vanilla conversion.
	/// All transitions run through the production conversion hooks rather than placing expected tiles directly.
	/// </summary>
	internal static class MawConversionGallery
	{
		private const int CellWidth = 13;
		private const int StageHeight = 8;
		private const int StageCount = 4;

		public static Rectangle Build(Player player, out IReadOnlyList<string> columns, out IReadOnlyList<string> stages)
		{
			Material[] materials = CreateMaterials();
			int width = materials.Length * CellWidth;
			int height = StageCount * StageHeight;
			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - width / 2, 20, Main.maxTilesX - width - 20);
			int top = Math.Clamp(playerTile.Y - height / 2, 20, Main.maxTilesY - height - 20);
			Rectangle bounds = new(left, top, width, height);

			Clear(bounds);
			for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
			{
				Material material = materials[materialIndex];
				for (int stage = 0; stage < StageCount; stage++)
					PlaceWastesSample(left + materialIndex * CellWidth, top + stage * StageHeight, material);
			}

			// Stage 0 remains neutral. Stages 1–3 become Maw through the registered production map.
			for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
			{
				for (int stage = 1; stage < StageCount; stage++)
					ConvertSampleToMaw(left + materialIndex * CellWidth, top + stage * StageHeight);
				PurifySample(left + materialIndex * CellWidth, top + 2 * StageHeight);
				PurifySample(left + materialIndex * CellWidth, top + 3 * StageHeight);
				PurifySample(left + materialIndex * CellWidth, top + 3 * StageHeight);
			}

			Frame(bounds);
			ValidateResults(left, top, materials);
			ValidateMawRuntimeContracts();
			columns = Array.ConvertAll(materials, material => material.Name);
			stages = ["Wastes source", "Maw conversion", "one Purity: Wastes", "two Purity: vanilla"];
			return bounds;
		}

		private static void PlaceWastesSample(int left, int top, Material material)
		{
			for (int x = left + 1; x <= left + 10; x++)
			for (int y = top + 1; y <= top + 4; y++)
				Framing.GetTileSafely(x, y).WallType = (ushort)material.WastesWall;

			for (int x = left + 1; x <= left + 7; x++)
				SetTile(x, top + 4, material.WastesTile);
			for (int x = left + 1; x <= left + 4; x++)
				SetTile(x, top + 3, material.WastesTile);
			SetTile(left + 1, top + 2, material.WastesTile);
			SetTile(left + 4, top + 2, material.WastesTile);
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
				RequireStage(sampleLeft, top, material.WastesTile, material.WastesWall, material.Name, "Wastes source");
				RequireStage(sampleLeft, top + StageHeight, material.MawTile, material.MawWall, material.Name, "Maw conversion");
				RequireStage(sampleLeft, top + 2 * StageHeight, material.WastesTile, material.WastesWall, material.Name, "one Purity pass");
				RequireStage(sampleLeft, top + 3 * StageHeight, material.VanillaTile, material.VanillaWall, material.Name, "two Purity passes");
			}
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
			new("Soil", ModContent.TileType<WastesSoil>(), ModContent.TileType<MawDirt>(), TileID.Dirt,
				ModContent.WallType<WastesDirtWallUnsafe>(), ModContent.WallType<MawDirtWallUnsafe>(), WallID.DirtUnsafe),
			new("Grass", ModContent.TileType<WastesGrass>(), ModContent.TileType<MawGrass>(), TileID.Grass,
				ModContent.WallType<WastesGrassWallUnsafe>(), ModContent.WallType<MawGrassWallUnsafe>(), WallID.GrassUnsafe),
			new("Stone", ModContent.TileType<WastesStone>(), ModContent.TileType<Mawstone>(), TileID.Stone,
				ModContent.WallType<WastesStoneWallUnsafe>(), ModContent.WallType<MawStoneWallUnsafe>(), WallID.Stone),
			new("Sand", ModContent.TileType<WastesSand>(), ModContent.TileType<MawSand>(), TileID.Sand,
				ModContent.WallType<WastesSandWallUnsafe>(), ModContent.WallType<MawSandWallUnsafe>(), WallID.Sandstone),
			new("Ice", ModContent.TileType<WastesIce>(), ModContent.TileType<MawIce>(), TileID.IceBlock,
				ModContent.WallType<WastesIceWallUnsafe>(), ModContent.WallType<MawIceWallUnsafe>(), WallID.IceUnsafe),
			new("Snow", ModContent.TileType<WastesSnow>(), ModContent.TileType<MawSnow>(), TileID.SnowBlock,
				ModContent.WallType<WastesSnowWallUnsafe>(), ModContent.WallType<MawSnowWallUnsafe>(), WallID.SnowWallUnsafe),
			new("Mud", ModContent.TileType<WastesMud>(), ModContent.TileType<MawMud>(), TileID.Mud,
				ModContent.WallType<WastesMudWallUnsafe>(), ModContent.WallType<MawMudWallUnsafe>(), WallID.MudUnsafe)
		];

		private readonly record struct Material(
			string Name,
			int WastesTile,
			int MawTile,
			int VanillaTile,
			int WastesWall,
			int MawWall,
			int VanillaWall);
	}
}
