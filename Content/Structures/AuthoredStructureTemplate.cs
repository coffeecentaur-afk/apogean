using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using apogean.Content.Factions;
using apogean.Content.Tiles;
using apogean.Content.Walls;

namespace apogean.Content.Structures
{
	/// <summary>
	/// Loads and stamps immutable, human-authored structure blueprints. World planning owns where a
	/// structure may exist; this module owns every tile, wall, room, fixture, and entrance inside it.
	/// </summary>
	internal sealed class AuthoredStructureTemplate
	{
		private readonly List<StructureCommand> commands = new();

		public int Width { get; private set; }
		public int Height { get; private set; }
		public int SurfaceBaseline { get; private set; } = -1;
		public Rectangle Entrance { get; private set; }

		public static AuthoredStructureTemplate Load(Mod mod, string assetPath)
		{
			string source = Encoding.UTF8.GetString(mod.GetFileBytes(assetPath));
			AuthoredStructureTemplate template = new();
			string[] lines = source.Replace("\r", string.Empty).Split('\n');
			for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
			{
				string line = lines[lineNumber].Trim();
				if (line.Length == 0 || line.StartsWith('#'))
					continue;

				string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				try
				{
					switch (parts[0].ToLowerInvariant())
					{
						case "size":
							template.Width = Number(parts[1]);
							template.Height = Number(parts[2]);
							break;
						case "entrance":
							template.Entrance = Rect(parts, 1);
							break;
						case "surface":
							template.SurfaceBaseline = Number(parts[1]);
							break;
						case "clear":
						case "erase":
							template.commands.Add(new StructureCommand(parts[0], null, Rect(parts, 1), 0, 0));
							break;
						case "wall":
						case "fill":
							template.commands.Add(new StructureCommand(parts[0], parts[1], Rect(parts, 2), 0, 0));
							break;
						case "frame":
							template.commands.Add(new StructureCommand(parts[0], parts[1], Rect(parts, 2), Number(parts[6]), 0));
							break;
						case "platform":
							template.commands.Add(new StructureCommand(parts[0], parts[1], new Rectangle(Number(parts[2]), Number(parts[3]), Number(parts[4]), 1), 0, 0));
							break;
						case "object":
							template.commands.Add(new StructureCommand(parts[0], parts[1], Rect(parts, 2), 0, parts.Length > 6 ? Number(parts[6]) : 0));
							break;
						default:
							throw new FormatException($"Unknown operation '{parts[0]}'.");
					}
				}
				catch (Exception exception)
				{
					throw new FormatException($"Invalid authored structure command at {assetPath}:{lineNumber + 1}: {line}", exception);
				}
			}

			if (template.Width <= 0 || template.Height <= 0)
				throw new FormatException($"Authored structure {assetPath} has no valid size command.");
			if (template.Entrance == Rectangle.Empty)
				throw new FormatException($"Authored structure {assetPath} has no entrance command.");
			return template;
		}

		public AuthoredStructurePlacement Place(Rectangle atlasBounds, bool centerVertically)
		{
			int originX = atlasBounds.Center.X - Width / 2;
			int originY = centerVertically ? atlasBounds.Center.Y - Height / 2 : atlasBounds.Top;
			Point origin = new(originX, originY);
			Rectangle worldBounds = new(originX, originY, Width, Height);

			// Resolve the shell first so every native object sees its final wall and floor anchors.
			// Painting frame-important objects before this pass made them look present while leaving
			// invalid frame data that could vanish, deanimate, or crash during drawing.
			for (int i = 0; i < commands.Count; i++)
				if (!commands[i].Operation.Equals("object", StringComparison.OrdinalIgnoreCase))
					Execute(commands[i], origin);
			FrameRegion(worldBounds);
			for (int i = 0; i < commands.Count; i++)
				if (commands[i].Operation.Equals("object", StringComparison.OrdinalIgnoreCase))
					Execute(commands[i], origin);
			Rectangle entrance = Entrance;
			entrance.Offset(origin);
			int surfaceY = SurfaceBaseline >= 0 ? originY + SurfaceBaseline : -1;
			return new AuthoredStructurePlacement(worldBounds, entrance, surfaceY);
		}

		public AuthoredStructurePlacement GetPlacement(Rectangle atlasBounds, bool centerVertically)
		{
			int originX = atlasBounds.Center.X - Width / 2;
			int originY = centerVertically ? atlasBounds.Center.Y - Height / 2 : atlasBounds.Top;
			Rectangle entrance = Entrance;
			entrance.Offset(originX, originY);
			int surfaceY = SurfaceBaseline >= 0 ? originY + SurfaceBaseline : -1;
			return new AuthoredStructurePlacement(new Rectangle(originX, originY, Width, Height), entrance, surfaceY);
		}

		private static void Execute(StructureCommand command, Point origin)
		{
			Rectangle area = command.Area;
			area.Offset(origin);
			switch (command.Operation.ToLowerInvariant())
			{
				case "clear": ClearArea(area, clearWalls: true); break;
				case "erase": ClearArea(area, clearWalls: false); break;
				case "wall": FillWall(area, ResolveWall(command.Asset)); break;
				case "fill": FillTile(area, ResolveTile(command.Asset)); break;
				case "frame": PlaceFrame(area, ResolveTile(command.Asset), command.Argument); break;
				case "platform": FillTile(area, ResolveTile(command.Asset)); break;
				case "object": PlaceObject(area, ResolveTile(command.Asset), command.Alternate); break;
			}
		}

		private static void ClearArea(Rectangle area, bool clearWalls)
		{
			for (int x = area.Left; x < area.Right; x++)
			{
				for (int y = area.Top; y < area.Bottom; y++)
				{
					if (!WorldGen.InWorld(x, y, 10))
						continue;
					Tile tile = Framing.GetTileSafely(x, y);
					ushort preservedWall = tile.WallType;
					tile.ClearEverything();
					if (!clearWalls)
						tile.WallType = preservedWall;
				}
			}
		}

		private static void FillWall(Rectangle area, int wallType)
		{
			for (int x = area.Left; x < area.Right; x++)
				for (int y = area.Top; y < area.Bottom; y++)
					if (WorldGen.InWorld(x, y, 10))
						Framing.GetTileSafely(x, y).WallType = (ushort)wallType;
		}

		private static void FillTile(Rectangle area, int tileType)
		{
			for (int x = area.Left; x < area.Right; x++)
			{
				for (int y = area.Top; y < area.Bottom; y++)
				{
					if (!WorldGen.InWorld(x, y, 10))
						continue;
					Tile tile = Framing.GetTileSafely(x, y);
					tile.HasTile = true;
					tile.TileType = (ushort)tileType;
					tile.TileFrameX = 0;
					tile.TileFrameY = 0;
					tile.Slope = SlopeType.Solid;
					tile.IsHalfBlock = false;
					tile.LiquidAmount = 0;
				}
			}
		}

		private static void PlaceFrame(Rectangle area, int tileType, int thickness)
		{
			for (int x = area.Left; x < area.Right; x++)
			{
				for (int y = area.Top; y < area.Bottom; y++)
				{
					bool edge = x < area.Left + thickness || x >= area.Right - thickness ||
						y < area.Top + thickness || y >= area.Bottom - thickness;
					if (edge)
						FillTile(new Rectangle(x, y, 1, 1), tileType);
				}
			}
		}

		private static void PlaceObject(Rectangle area, int tileType, int alternate)
		{
			const int style = 0;
			TileObjectData data = TileObjectData.GetTileData(tileType, style, alternate)
				?? throw new InvalidOperationException(
					$"Authored object tile {tileType} has no TileObjectData for style {style}, alternate {alternate}.");

			if (data.Width != area.Width || data.Height != area.Height)
				throw new InvalidOperationException(
					$"Authored object tile {tileType} declares {area.Width}x{area.Height}, " +
					$"but TileObjectData requires {data.Width}x{data.Height} for alternate {alternate}.");

			if (!WorldGen.InWorld(area.Left, area.Top, 10) ||
				!WorldGen.InWorld(area.Right - 1, area.Bottom - 1, 10))
				throw new InvalidOperationException($"Authored object tile {tileType} is outside world bounds at {area}.");

			// Remove blueprint platforms or stale fixture cells inside the object's own footprint while
			// retaining its already-authored wall. The supporting floor beneath the rectangle is untouched.
			ClearArea(area, clearWalls: false);
			int originX = area.Left + data.Origin.X;
			int originY = area.Top + data.Origin.Y;
			if (!WorldGen.PlaceObject(originX, originY, tileType, mute: true, style: style, alternate: alternate))
				throw new InvalidOperationException(
					$"Authored object {DescribeTileType(tileType)} could not place at {area} " +
					$"(origin {originX},{originY}; alternate {alternate}). " +
					$"Bottom anchors: {DescribeBottomAnchors(area)}");
		}

		private static string DescribeTileType(int tileType)
		{
			ModTile modTile = TileLoader.GetTile(tileType);
			return modTile is null ? tileType.ToString(CultureInfo.InvariantCulture) : $"{modTile.FullName} ({tileType})";
		}

		private static string DescribeBottomAnchors(Rectangle area)
		{
			StringBuilder result = new();
			for (int x = area.Left; x < area.Right; x++)
			{
				if (result.Length > 0)
					result.Append("; ");
				Tile tile = Framing.GetTileSafely(x, area.Bottom);
				bool solid = tile.HasTile && tile.TileType < Main.tileSolid.Length && Main.tileSolid[tile.TileType];
				bool solidTop = tile.HasTile && tile.TileType < Main.tileSolidTop.Length && Main.tileSolidTop[tile.TileType];
				bool noAttach = tile.HasTile && tile.TileType < Main.tileNoAttach.Length && Main.tileNoAttach[tile.TileType];
				result.Append(CultureInfo.InvariantCulture,
					$"{x},{area.Bottom}=type:{tile.TileType},has:{tile.HasTile},solid:{solid},solidTop:{solidTop}," +
					$"noAttach:{noAttach},slope:{tile.Slope},half:{tile.IsHalfBlock},actuated:{tile.IsActuated}");
			}
			return result.ToString();
		}

		private static void FrameRegion(Rectangle area)
		{
			for (int x = area.Left - 1; x <= area.Right; x++)
			{
				for (int y = area.Top - 1; y <= area.Bottom; y++)
				{
					if (!WorldGen.InWorld(x, y, 10))
						continue;
					WorldGen.SquareTileFrame(x, y, true);
					WorldGen.SquareWallFrame(x, y, true);
				}
			}
		}

		private static int ResolveTile(string name) => name switch
		{
			nameof(KesslerBlock) => ModContent.TileType<KesslerBlock>(),
			nameof(KesslerTrim) => ModContent.TileType<KesslerTrim>(),
			nameof(KesslerFloor) => ModContent.TileType<KesslerFloor>(),
			nameof(KesslerGlass) => ModContent.TileType<KesslerGlass>(),
			nameof(KesslerBeam) => ModContent.TileType<KesslerBeam>(),
			nameof(HelixBlock) => ModContent.TileType<HelixBlock>(),
			nameof(HelixTrim) => ModContent.TileType<HelixTrim>(),
			nameof(HelixFloor) => ModContent.TileType<HelixFloor>(),
			nameof(HelixGlass) => ModContent.TileType<HelixGlass>(),
			nameof(HelixBeam) => ModContent.TileType<HelixBeam>(),
			nameof(SentrixBlock) => ModContent.TileType<SentrixBlock>(),
			nameof(SentrixTrim) => ModContent.TileType<SentrixTrim>(),
			nameof(SentrixFloor) => ModContent.TileType<SentrixFloor>(),
			nameof(SentrixGlass) => ModContent.TileType<SentrixGlass>(),
			nameof(SentrixBeam) => ModContent.TileType<SentrixBeam>(),
			nameof(KesslerPlating) => ModContent.TileType<KesslerPlating>(),
			nameof(HelixContainmentPanel) => ModContent.TileType<HelixContainmentPanel>(),
			nameof(SentrixPanel) => ModContent.TileType<SentrixPanel>(),
			nameof(KesslerPlatform) => ModContent.TileType<KesslerPlatform>(),
			nameof(HelixPlatform) => ModContent.TileType<HelixPlatform>(),
			nameof(SentrixPlatform) => ModContent.TileType<SentrixPlatform>(),
			nameof(KesslerChair) => ModContent.TileType<KesslerChair>(),
			nameof(HelixChair) => ModContent.TileType<HelixChair>(),
			nameof(SentrixChair) => ModContent.TileType<SentrixChair>(),
			nameof(KesslerTable) => ModContent.TileType<KesslerTable>(),
			nameof(HelixTable) => ModContent.TileType<HelixTable>(),
			nameof(SentrixTable) => ModContent.TileType<SentrixTable>(),
			nameof(KesslerWorkbench) => ModContent.TileType<KesslerWorkbench>(),
			nameof(HelixWorkbench) => ModContent.TileType<HelixWorkbench>(),
			nameof(SentrixWorkbench) => ModContent.TileType<SentrixWorkbench>(),
			nameof(KesslerLight) => ModContent.TileType<KesslerLight>(),
			nameof(HelixLight) => ModContent.TileType<HelixLight>(),
			nameof(SentrixLight) => ModContent.TileType<SentrixLight>(),
			nameof(KesslerConsole) => ModContent.TileType<KesslerConsole>(),
			nameof(HelixConsole) => ModContent.TileType<HelixConsole>(),
			nameof(SentrixConsole) => ModContent.TileType<SentrixConsole>(),
			nameof(KesslerLocker) => ModContent.TileType<KesslerLocker>(),
			nameof(HelixLocker) => ModContent.TileType<HelixLocker>(),
			nameof(SentrixLocker) => ModContent.TileType<SentrixLocker>(),
			nameof(KesslerPowerArmorRack) => ModContent.TileType<KesslerPowerArmorRack>(),
			nameof(KesslerWarBanner) => ModContent.TileType<KesslerWarBanner>(),
			nameof(HelixSymbioteTank) => ModContent.TileType<HelixSymbioteTank>(),
			nameof(SentrixHologramCore) => ModContent.TileType<SentrixHologramCore>(),
			_ => throw new KeyNotFoundException($"Unknown authored-structure tile '{name}'.")
		};

		private static int ResolveWall(string name) => name switch
		{
			nameof(KesslerBulkheadWall) => ModContent.WallType<KesslerBulkheadWall>(),
			nameof(KesslerWindowWall) => ModContent.WallType<KesslerWindowWall>(),
			nameof(HelixLaboratoryWall) => ModContent.WallType<HelixLaboratoryWall>(),
			nameof(HelixObservationWall) => ModContent.WallType<HelixObservationWall>(),
			nameof(SentrixDataWall) => ModContent.WallType<SentrixDataWall>(),
			nameof(SentrixWindowWall) => ModContent.WallType<SentrixWindowWall>(),
			_ => throw new KeyNotFoundException($"Unknown authored-structure wall '{name}'.")
		};

		private static int Number(string value) => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

		private static Rectangle Rect(string[] parts, int start) => new(
			Number(parts[start]), Number(parts[start + 1]), Number(parts[start + 2]), Number(parts[start + 3]));

		private readonly record struct StructureCommand(string Operation, string Asset, Rectangle Area, int Argument, int Alternate);
	}

	internal readonly record struct AuthoredStructurePlacement(Rectangle Bounds, Rectangle Entrance, int SurfaceY);

	internal static class CorporateCampusBlueprints
	{
		private static readonly Dictionary<ApogeanFaction, AuthoredStructureTemplate> Templates = new();

		public static AuthoredStructurePlacement Place(Mod mod, ApogeanFaction faction, Rectangle atlasBounds)
		{
			AuthoredStructurePlacement placement = Get(mod, faction).Place(atlasBounds, faction == ApogeanFaction.Sentrix);
			if (faction is ApogeanFaction.Kessler or ApogeanFaction.Helix)
				CorporateTerrainIntegration.BlendGroundCampus(faction, placement);
			return placement;
		}

		public static AuthoredStructurePlacement GetPlacement(Mod mod, ApogeanFaction faction, Rectangle atlasBounds) =>
			Get(mod, faction).GetPlacement(atlasBounds, faction == ApogeanFaction.Sentrix);

		private static AuthoredStructureTemplate Get(Mod mod, ApogeanFaction faction)
		{
			if (Templates.TryGetValue(faction, out AuthoredStructureTemplate template))
				return template;

			string name = faction switch
			{
				ApogeanFaction.Kessler => "KesslerCampus.apstructure",
				ApogeanFaction.Helix => "HelixCampus.apstructure",
				ApogeanFaction.Sentrix => "SentrixCampus.apstructure",
				_ => throw new ArgumentOutOfRangeException(nameof(faction), faction, null)
			};
			template = AuthoredStructureTemplate.Load(mod, $"Content/Structures/Blueprints/{name}");
			Templates[faction] = template;
			return template;
		}
	}
}
