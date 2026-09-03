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

namespace apogean.Content.Diagnostics
{
	/// <summary>
	/// Production-only terrain fixture. It checks self-framing, neighboring material seams,
	/// slopes, half blocks, paint, fullbright coatings, unsafe walls, and standing water.
	/// </summary>
	internal static class WastesTerrainPropertyGallery
	{
		private const int CellWidth = 12;
		private const int CellHeight = 14;

		public static Rectangle Build(Player player, out IReadOnlyList<string> labels)
		{
			Material[] materials = CreateMaterials();
			int width = materials.Length * CellWidth;
			Point playerTile = player.Center.ToTileCoordinates();
			int startX = Math.Clamp(playerTile.X - width / 2, 20, Main.maxTilesX - width - 20);
			int startY = Math.Clamp(playerTile.Y - 20, 20, Main.maxTilesY - CellHeight - 20);
			Rectangle bounds = new(startX, startY, width, CellHeight);

			Clear(bounds);
			List<string> names = new(materials.Length);
			for (int index = 0; index < materials.Length; index++)
			{
				PlaceMaterial(startX + index * CellWidth, startY, materials[index]);
				names.Add(materials[index].Name);
			}

			Frame(bounds);
			ValidateRuntimeContracts();
			labels = names;
			return bounds;
		}

		private static void Clear(Rectangle bounds)
		{
			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				tile.ClearEverything();
			}
		}

		private static void PlaceMaterial(int left, int top, Material material)
		{
			// Irregular tile silhouette with no wall behind it, so tile edges cannot be mistaken
			// for a rectangular material swatch.
			for (int y = top + 1; y <= top + 5; y++)
				SetTile(left, y, material.Tile);
			for (int x = left; x <= left + 6; x++)
				SetTile(x, top + 5, material.Tile);
			for (int x = left + 2; x <= left + 5; x++)
				SetTile(x, top + 3, material.Tile);
			SetTile(left + 2, top + 2, material.Tile);
			SetTile(left + 5, top + 2, material.Tile);

			Tile halfBlock = Framing.GetTileSafely(left + 2, top + 2);
			halfBlock.IsHalfBlock = true;
			Tile slope = Framing.GetTileSafely(left + 5, top + 2);
			slope.Slope = SlopeType.SlopeDownLeft;

			// Paint and coating probes remain supported even for falling sand.
			Framing.GetTileSafely(left + 3, top + 3).TileColor = PaintID.OrangePaint;
			Framing.GetTileSafely(left + 5, top + 3).IsTileFullbright = true;
			if (material.Tile == ModContent.TileType<WastesSand>())
			{
				for (int x = left; x <= left + 6; x++)
					SetTile(x, top + 6, ModContent.TileType<WastesStone>());
			}

			// A separate wall patch exposes wall framing without filling the tile silhouette.
			for (int x = left + 1; x <= left + 5; x++)
			for (int y = top + 7; y <= top + 10; y++)
				Framing.GetTileSafely(x, y).WallType = (ushort)material.Wall;
			Framing.GetTileSafely(left + 3, top + 8).WallColor = PaintID.OrangePaint;

			// Two-tile water pocket.
			for (int y = top + 8; y <= top + 11; y++)
			{
				SetTile(left + 7, y, material.Tile);
				SetTile(left + 10, y, material.Tile);
			}
			for (int x = left + 7; x <= left + 10; x++)
				SetTile(x, top + 11, material.Tile);
			for (int x = left + 8; x <= left + 9; x++)
			{
				Tile liquid = Framing.GetTileSafely(x, top + 10);
				liquid.LiquidType = LiquidID.Water;
				liquid.LiquidAmount = byte.MaxValue;
			}

			// A continuous one-tile floor touches the neighboring material, exposing seam behavior.
			for (int x = left; x < left + CellWidth; x++)
				SetTile(x, top + 12, material.Tile);
			if (material.Tile == ModContent.TileType<WastesSand>())
			{
				for (int x = left; x < left + CellWidth; x++)
					SetTile(x, top + 13, ModContent.TileType<WastesStone>());
			}
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

		private static void ValidateRuntimeContracts()
		{
			int soil = ModContent.TileType<WastesSoil>();
			int stone = ModContent.TileType<WastesStone>();
			int grass = ModContent.TileType<WastesGrass>();
			int sand = ModContent.TileType<WastesSand>();
			int ice = ModContent.TileType<WastesIce>();
			int snow = ModContent.TileType<WastesSnow>();
			int mud = ModContent.TileType<WastesMud>();
			int soilItem = ModContent.ItemType<WastesSoilBlock>();
			int stoneItem = ModContent.ItemType<WastesStoneBlock>();
			int sandItem = ModContent.ItemType<WastesSandBlock>();
			int iceItem = ModContent.ItemType<WastesIceBlock>();
			int snowItem = ModContent.ItemType<WastesSnowBlock>();
			int mudItem = ModContent.ItemType<WastesMudBlock>();

			Require(Main.tileSolid[soil] && Main.tileSolid[stone] && Main.tileSolid[grass] &&
				Main.tileSolid[sand] && Main.tileSolid[ice] && Main.tileSolid[snow] && Main.tileSolid[mud],
				"Every Wastes terrain material must be solid.");
			Require(TileID.Sets.Stone[stone], "Wastes Stone must participate in stone behavior.");
			Require(TileID.Sets.Grass[grass] && TileID.Sets.Conversion.Grass[grass],
				"Wastes Grass must participate in grass framing and conversion.");
			Require(Main.tileSand[sand] && TileID.Sets.Conversion.Sand[sand] &&
				TileID.Sets.Falling[sand] && TileID.Sets.Suffocate[sand] && TileID.Sets.CanBeDugByShovel[sand],
				"Wastes Sand must implement Terraria sand behavior.");
			Require(TileID.Sets.FallingBlockProjectile[sand]?.FallingProjectileType ==
				ModContent.ProjectileType<WastesSandBallFallingProjectile>(),
				"Wastes Sand must fall as its identity-preserving projectile.");
			ProjectileID.Sets.FallingBlockTileItemInfo fallingSand =
				ProjectileID.Sets.FallingBlockTileItem[ModContent.ProjectileType<WastesSandBallFallingProjectile>()];
			ProjectileID.Sets.FallingBlockTileItemInfo firedSand =
				ProjectileID.Sets.FallingBlockTileItem[ModContent.ProjectileType<WastesSandBallGunProjectile>()];
			ItemID.Sets.SandgunAmmoInfo sandgunAmmo = ItemID.Sets.SandgunAmmoProjectileData[sandItem];
			Require(fallingSand?.TileType == sand && fallingSand.ItemType == sandItem,
				"Falling Wastes Sand must replace itself or drop its own item.");
			Require(firedSand?.TileType == sand && firedSand.ItemType == ItemID.None,
				"Sandgun-fired Wastes Sand must place its tile without duplicating ammo.");
			Require(sandgunAmmo?.ProjectileType == ModContent.ProjectileType<WastesSandBallGunProjectile>() &&
				sandgunAmmo.BonusDamage == 10,
				"The Wastes Sand item must be registered as custom Sandgun ammunition.");
			Require(TileID.Sets.IceSkateSlippery[ice] && TileID.Sets.Ices[ice] && TileID.Sets.IcesSlush[ice],
				"Wastes Ice must participate in ice movement and framing.");
			Require(TileID.Sets.Snow[snow], "Wastes Snow must participate in snow framing.");
			Require(TileLoader.GetItemDropFromTypeAndStyle(soil) == soilItem &&
				TileLoader.GetItemDropFromTypeAndStyle(grass) == soilItem &&
				TileLoader.GetItemDropFromTypeAndStyle(stone) == stoneItem &&
				TileLoader.GetItemDropFromTypeAndStyle(sand) == sandItem &&
				TileLoader.GetItemDropFromTypeAndStyle(ice) == iceItem &&
				TileLoader.GetItemDropFromTypeAndStyle(snow) == snowItem &&
				TileLoader.GetItemDropFromTypeAndStyle(mud) == mudItem,
				"Every Wastes terrain tile must drop a Wastes item; grass must return Wastes Soil.");
			Require(!TileID.Sets.Infectable[soil] && !TileID.Sets.Infectable[stone] &&
				!TileID.Sets.Infectable[grass] && !TileID.Sets.Infectable[sand] &&
				!TileID.Sets.Infectable[ice] && !TileID.Sets.Infectable[snow] && !TileID.Sets.Infectable[mud],
				"Neutral Wastes terrain must not inherit vanilla evil spread.");
		}

		private static void Require(bool condition, string message)
		{
			if (!condition)
				throw new InvalidOperationException(message);
		}

		private static Material[] CreateMaterials() =>
		[
			new("Soil", ModContent.TileType<WastesSoil>(), ModContent.WallType<WastesDirtWallUnsafe>()),
			new("Grass", ModContent.TileType<WastesGrass>(), ModContent.WallType<WastesGrassWallUnsafe>()),
			new("Stone", ModContent.TileType<WastesStone>(), ModContent.WallType<WastesStoneWallUnsafe>()),
			new("Sand", ModContent.TileType<WastesSand>(), ModContent.WallType<WastesSandWallUnsafe>()),
			new("Ice", ModContent.TileType<WastesIce>(), ModContent.WallType<WastesIceWallUnsafe>()),
			new("Snow", ModContent.TileType<WastesSnow>(), ModContent.WallType<WastesSnowWallUnsafe>()),
			new("Mud", ModContent.TileType<WastesMud>(), ModContent.WallType<WastesMudWallUnsafe>())
		];

		private readonly record struct Material(string Name, int Tile, int Wall);
	}
}
