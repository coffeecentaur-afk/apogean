using System;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.WorldGeneration;
using apogean.Content.Factions;
using apogean.Content.Structures;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	/// <summary>
	/// Non-destructive visual inspection of the Kessler Campus at its saved atlas location. The
	/// temporary door cycle is restricted to the disposable QA world and always finishes re-armed.
	/// </summary>
	internal static class KesslerWorldGallery
	{
		private const string RequiredWorldName = "Apogee Campus QA";

		internal static Rectangle Inspect(Mod mod, Player player, out string report)
		{
			if (Main.ActiveWorldFileData?.Name != RequiredWorldName)
				throw new InvalidOperationException(
					$"The real-world Campus inspection may run only in the disposable '{RequiredWorldName}' world.");

			ApogeanWorldPlan plan = ApogeanWorldPlanSystem.Instance.Plan
				?? throw new InvalidOperationException("The loaded QA world has no saved Apogean atlas.");
			ApogeanLandmarkPlan landmark = plan.GetLandmark(ApogeanLandmarkKind.KesslerCampus)
				?? throw new InvalidOperationException("The loaded QA world has no saved Kessler Campus landmark.");
			AuthoredStructurePlacement placement = CorporateCampusBlueprints.GetPlacement(
				mod, ApogeanFaction.Kessler, landmark.Bounds);

			int kesslerBlock = ModContent.TileType<KesslerBlock>();
			int kesslerFloor = ModContent.TileType<KesslerFloor>();
			ValidateRectangleType(placement.Entrance, kesslerBlock, "saved sealed bulkhead");

			Rectangle publicOpening = new(
				placement.Bounds.Left + 38,
				placement.Bounds.Top + 43,
				3,
				9);
			ValidateRectangleEmpty(publicOpening, "day-one public frontage");

			for (int x = placement.Bounds.Left; x < placement.Bounds.Right; x++)
			{
				ValidateType(x, placement.SurfaceY, kesslerFloor, "upper foundation course");
				ValidateType(x, placement.SurfaceY + 1, kesslerFloor, "lower foundation course");
			}

			int lights = CountTileCells(placement.Bounds, ModContent.TileType<KesslerLight>());
			int racks = CountTileCells(placement.Bounds, ModContent.TileType<KesslerPowerArmorRack>());
			int standards = CountTileCells(placement.Bounds, ModContent.TileType<KesslerWarBanner>());
			if (lights < 10 || racks < 24 || standards < 24)
				throw new InvalidOperationException(
					$"Saved Campus fixture counts are incomplete (lights={lights}, racks={racks}, standards={standards}).");

			(int supportedColumns, int wastesCells) = InspectTerrainContact(placement);
			int requiredSupport = placement.Bounds.Width * 3 / 4;
			if (supportedColumns < requiredSupport || wastesCells < 80)
				throw new InvalidOperationException(
					$"Saved Campus terrain integration is incomplete (supported={supportedColumns}/" +
					$"{placement.Bounds.Width}, wastes-cells={wastesCells}).");

			// Exercise the same saved bounds used by progression, then restore the day-one state.
			CompoundGen.UnsealCompound(ApogeanFaction.Kessler);
			ValidateRectangleEmpty(placement.Entrance, "temporarily opened progression bulkhead");
			CompoundGen.ReArmCompound(ApogeanFaction.Kessler);
			ValidateRectangleType(placement.Entrance, kesslerBlock, "re-armed progression bulkhead");

			Rectangle capture = placement.Bounds;
			// Stay below the capture renderer's viewport-sized target limit. Oversized requests render
			// valid tiles but leave black sky bands, which makes otherwise sound evidence misleading.
			capture.Inflate(2, 2);
			capture = Rectangle.Intersect(capture, new Rectangle(10, 10, Main.maxTilesX - 20, Main.maxTilesY - 20));
			player.Teleport(new Vector2(
				placement.Bounds.Center.X * 16f,
				(placement.SurfaceY - 2) * 16f), TeleportationStyleID.RodOfDiscord);

			StringBuilder summary = new();
			summary.Append($"atlas={landmark.Bounds}; campus={placement.Bounds}; surface={placement.SurfaceY}; ");
			summary.Append($"public={publicOpening}; sealed={placement.Entrance}; ");
			summary.Append($"native-cells lights={lights}, racks={racks}, standards={standards}; ");
			summary.Append($"terrain supported={supportedColumns}/{placement.Bounds.Width}, wastes={wastesCells}; door-cycle=pass");
			report = summary.ToString();
			return capture;
		}

		private static (int SupportedColumns, int WastesCells) InspectTerrainContact(AuthoredStructurePlacement placement)
		{
			int wastesSoil = ModContent.TileType<WastesSoil>();
			int wastesStone = ModContent.TileType<WastesStone>();
			int wastesGrass = ModContent.TileType<WastesGrass>();
			int supported = 0;
			int wastes = 0;
			for (int x = placement.Bounds.Left; x < placement.Bounds.Right; x++)
			{
				bool foundSupport = false;
				for (int y = placement.SurfaceY + 9; y <= placement.SurfaceY + 36; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (!tile.HasTile)
						continue;

					foundSupport |= Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
					if (tile.TileType == wastesSoil || tile.TileType == wastesStone || tile.TileType == wastesGrass)
						wastes++;
				}

				if (foundSupport)
					supported++;
			}

			return (supported, wastes);
		}

		private static int CountTileCells(Rectangle bounds, int tileType)
		{
			int count = 0;
			for (int x = bounds.Left; x < bounds.Right; x++)
				for (int y = bounds.Top; y < bounds.Bottom; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile && tile.TileType == tileType)
						count++;
				}
			return count;
		}

		private static void ValidateRectangleType(Rectangle area, int tileType, string label)
		{
			for (int x = area.Left; x < area.Right; x++)
				for (int y = area.Top; y < area.Bottom; y++)
					ValidateType(x, y, tileType, label);
		}

		private static void ValidateType(int x, int y, int tileType, string label)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			if (!tile.HasTile || tile.TileType != tileType)
				throw new InvalidOperationException(
					$"Kessler {label} failed at {x},{y}: expected tile {tileType}, found " +
					$"{(tile.HasTile ? tile.TileType : -1)}.");
		}

		private static void ValidateRectangleEmpty(Rectangle area, string label)
		{
			for (int x = area.Left; x < area.Right; x++)
			{
				for (int y = area.Top; y < area.Bottom; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile)
						throw new InvalidOperationException(
							$"Kessler {label} is obstructed at {x},{y} by tile {tile.TileType}.");
				}
			}
		}
	}
}
