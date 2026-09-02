using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
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

			for (int i = 0; i < commands.Count; i++)
				Execute(commands[i], origin);

			FrameRegion(worldBounds);
			Rectangle entrance = Entrance;
			entrance.Offset(origin);
			return new AuthoredStructurePlacement(worldBounds, entrance);
		}

		public AuthoredStructurePlacement GetPlacement(Rectangle atlasBounds, bool centerVertically)
		{
			int originX = atlasBounds.Center.X - Width / 2;
			int originY = centerVertically ? atlasBounds.Center.Y - Height / 2 : atlasBounds.Top;
			Rectangle entrance = Entrance;
			entrance.Offset(originX, originY);
			return new AuthoredStructurePlacement(new Rectangle(originX, originY, Width, Height), entrance);
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
				case "object": PlaceObject(area, ResolveTile(command.Asset), command.Style); break;
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

		private static void PlaceObject(Rectangle area, int tileType, int style)
		{
			for (int column = 0; column < area.Width; column++)
			{
				for (int row = 0; row < area.Height; row++)
				{
					int x = area.Left + column;
					int y = area.Top + row;
					if (!WorldGen.InWorld(x, y, 10))
						continue;
					Tile tile = Framing.GetTileSafely(x, y);
					tile.HasTile = true;
					tile.TileType = (ushort)tileType;
					tile.TileFrameX = (short)((style * area.Width + column) * 18);
					tile.TileFrameY = (short)(row * 18);
					tile.Slope = SlopeType.Solid;
					tile.IsHalfBlock = false;
					tile.LiquidAmount = 0;
				}
			}
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

		private readonly record struct StructureCommand(string Operation, string Asset, Rectangle Area, int Argument, int Style);
	}

	internal readonly record struct AuthoredStructurePlacement(Rectangle Bounds, Rectangle Entrance);

	internal static class CorporateCampusBlueprints
	{
		private static readonly Dictionary<ApogeanFaction, AuthoredStructureTemplate> Templates = new();

		public static AuthoredStructurePlacement Place(Mod mod, ApogeanFaction faction, Rectangle atlasBounds) =>
			Get(mod, faction).Place(atlasBounds, faction == ApogeanFaction.Sentrix);

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
