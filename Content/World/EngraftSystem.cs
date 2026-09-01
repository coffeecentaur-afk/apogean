using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using apogean.Content.Tiles;

namespace apogean.Content.World
{
	/// <summary>
	/// Owns Engraft geography. Nodes are finite and saved; they only convert plain, un-walled natural-looking terrain,
	/// deliberately leaving housing, chests and authored structures alone.
	/// </summary>
	public sealed class EngraftSystem : ModSystem
	{
		private const int InitialRadiusX = 160;
		private const int InitialRadiusY = 70;
		private const int BiomeRadiusX = 86;
		private const int BiomeRadiusY = 48;
		private const int MaxTotalNodes = 7;

		private readonly List<Point16> nodes = new();
		private ulong lastGrowthTick;

		public static EngraftSystem Instance => ModContent.GetInstance<EngraftSystem>();
		public IReadOnlyList<Point16> Nodes => nodes;

		public override void OnWorldLoad()
		{
			nodes.Clear();
		}

		public override void OnWorldUnload()
		{
			nodes.Clear();
		}

		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			int jungle = tasks.FindIndex(pass => pass.Name.Equals("Jungle", StringComparison.OrdinalIgnoreCase));
			int insertAt = jungle >= 0 ? jungle + 1 : Math.Max(0, tasks.Count - 1);
			tasks.Insert(insertAt, new PassLegacy("The Engraft", GenerateEngraft));
		}

		private void GenerateEngraft(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "Something is taking root...";
			int spawn = Main.spawnTileX;
			int minimumDistance = Math.Max(420, Main.maxTilesX / 7);
			int x = spawn;
			for (int attempts = 0; attempts < 300; attempts++)
			{
				int candidate = WorldGen.genRand.Next(280, Main.maxTilesX - 280);
				if (Math.Abs(candidate - spawn) >= minimumDistance)
				{
					x = candidate;
					break;
				}
			}

			int y = FindSurface(x);
			CreateRupture(new Point16(x, y), InitialRadiusX, InitialRadiusY, WorldGen.genRand);
			RegisterNode(new Point16(x, Math.Max(10, y - 3)));

			// Two weaker outgrowths give early exploration hooks without turning half a new world hostile.
			for (int i = 0; i < 2; i++)
			{
				int offset = WorldGen.genRand.NextBool() ? WorldGen.genRand.Next(180, 360) : -WorldGen.genRand.Next(180, 360);
				int outgrowthX = Utils.Clamp(x + offset, 220, Main.maxTilesX - 220);
				int outgrowthY = FindSurface(outgrowthX);
				CreateRupture(new Point16(outgrowthX, outgrowthY), 60, 35, WorldGen.genRand);
				RegisterNode(new Point16(outgrowthX, Math.Max(10, outgrowthY - 3)));
			}
		}

		private static int FindSurface(int x)
		{
			for (int y = 80; y < Main.worldSurface + 80; y++)
			{
				if (WorldGen.SolidTile(x, y)) return y;
			}
			return (int)Main.worldSurface;
		}

		private static void CreateRupture(Point16 center, int radiusX, int radiusY, UnifiedRandom random)
		{
			int turf = ModContent.TileType<EngraftTurf>();
			for (int x = center.X - radiusX; x <= center.X + radiusX; x++)
			{
				for (int y = center.Y - radiusY; y <= center.Y + radiusY; y++)
				{
					if (!WorldGen.InWorld(x, y, 12)) continue;
					float dx = (x - center.X) / (float)radiusX;
					float dy = (y - center.Y) / (float)radiusY;
					if (dx * dx + dy * dy > 1f + random.NextFloat(-0.14f, 0.12f)) continue;

					Tile tile = Framing.GetTileSafely(x, y);
					if (!IsConvertibleTerrain(tile)) continue;
					tile.HasTile = true;
					tile.TileType = (ushort)turf;
				}
			}

			MutateSurfaceGrowth(center, radiusX, radiusY, random);
		}

		private static bool IsConvertibleTerrain(Tile tile)
		{
			if (!tile.HasTile || (tile.WallType > WallID.None && Main.wallHouse[tile.WallType])) return false;
			return tile.TileType is TileID.Dirt or TileID.Grass or TileID.Stone or TileID.ClayBlock or TileID.Mud or
				TileID.Sand or TileID.HardenedSand or TileID.Sandstone;
		}

		private static void MutateSurfaceGrowth(Point16 center, int radiusX, int radiusY, UnifiedRandom random)
		{
			int turf = ModContent.TileType<EngraftTurf>();
			int tuft = ModContent.TileType<EngraftTuft>();
			int minX = Math.Max(12, center.X - radiusX);
			int maxX = Math.Min(Main.maxTilesX - 12, center.X + radiusX);
			int minY = Math.Max(12, center.Y - radiusY);
			int maxY = Math.Min(Main.maxTilesY - 12, center.Y + radiusY);

			for (int x = minX; x <= maxX; x++)
			{
				for (int y = minY; y <= maxY; y++)
				{
					Tile ground = Framing.GetTileSafely(x, y);
					if (!ground.HasTile || ground.TileType != turf) continue;

					Tile above = Framing.GetTileSafely(x, y - 1);
					if (above.HasTile && above.TileType is TileID.Plants or TileID.Plants2 or TileID.Vines or TileID.Saplings or TileID.Trees)
					{
						WorldGen.KillTile(x, y - 1, noItem: true);
						above = Framing.GetTileSafely(x, y - 1);
					}

					if (!above.HasTile && ground.Slope == SlopeType.Solid && !ground.IsHalfBlock && random.NextBool(7))
					{
						WorldGen.PlaceTile(x, y - 1, tuft, mute: true, forced: true);
					}
				}
			}
		}

		public static bool IsInEngraft(Vector2 worldPosition)
		{
			int centerX = (int)(worldPosition.X / 16f);
			int centerY = (int)(worldPosition.Y / 16f);
			int turf = ModContent.TileType<EngraftTurf>();
			int found = 0;
			for (int x = centerX - BiomeRadiusX; x <= centerX + BiomeRadiusX; x += 4)
			{
				for (int y = centerY - BiomeRadiusY; y <= centerY + BiomeRadiusY; y += 4)
				{
					if (!WorldGen.InWorld(x, y, 2)) continue;
					if (Framing.GetTileSafely(x, y).TileType == turf && ++found >= 52) return true;
				}
			}
			return false;
		}

		public override void PostUpdateWorld()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			RemoveBrokenNodes();

			if (Main.GameUpdateCount - lastGrowthTick < 900 || nodes.Count == 0) return;
			lastGrowthTick = Main.GameUpdateCount;
			SpreadOnce();
		}

		private void SpreadOnce()
		{
			Point16 node = nodes[Main.rand.Next(nodes.Count)];
			int turf = ModContent.TileType<EngraftTurf>();
			for (int attempt = 0; attempt < 30; attempt++)
			{
				int x = node.X + Main.rand.Next(-54, 55);
				int y = node.Y + Main.rand.Next(-28, 29);
				if (!WorldGen.InWorld(x, y, 12)) continue;
				Tile tile = Framing.GetTileSafely(x, y);
				if (!IsConvertibleTerrain(tile)) continue;
				tile.TileType = (ushort)turf;
				NetMessage.SendTileSquare(-1, x, y);
				return;
			}
		}

		/// <summary>Called only when a Hardmode altar breaks. At most four new regions are created after the three worldgen nodes.</summary>
		public void SeedFromDestroyedAltar()
		{
			if (!NPC.downedBoss3 || nodes.Count >= MaxTotalNodes) return;
			int x = Main.rand.Next(300, Main.maxTilesX - 300);
			int y = FindSurface(x);
			CreateRupture(new Point16(x, y), 48, 28, Main.rand);
			RegisterNode(new Point16(x, Math.Max(10, y - 3)));
			ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromLiteral("The Engraft stirs beneath the broken altar."), new Color(194, 126, 44));
		}

		private void RegisterNode(Point16 location)
		{
			if (!nodes.Contains(location)) nodes.Add(location);
			Tile tile = Framing.GetTileSafely(location.X, location.Y);
			tile.HasTile = true;
			tile.TileType = (ushort)ModContent.TileType<MawNode>();
		}

		private void RemoveBrokenNodes()
		{
			int nodeType = ModContent.TileType<MawNode>();
			nodes.RemoveAll(point => !WorldGen.InWorld(point.X, point.Y, 2) ||
				!Framing.GetTileSafely(point.X, point.Y).HasTile || Framing.GetTileSafely(point.X, point.Y).TileType != nodeType);
		}

		/// <summary>Playtest-only world mutation used by /apogean engraft. It never overwrites protected walls or furniture.</summary>
		public void CreateDebugRupture(Player player)
		{
			int x = (int)(player.Center.X / 16f);
			int y = FindSurface(x);
			CreateRupture(new Point16(x, y), 90, 45, Main.rand);
			RegisterNode(new Point16(x, Math.Max(10, y - 3)));
		}

		public override void SaveWorldData(TagCompound tag)
		{
			List<TagCompound> savedNodes = new();
			foreach (Point16 node in nodes)
			{
				savedNodes.Add(new TagCompound { ["x"] = (int)node.X, ["y"] = (int)node.Y });
			}
			tag["engraftNodes"] = savedNodes;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			nodes.Clear();
			foreach (TagCompound saved in tag.GetList<TagCompound>("engraftNodes"))
			{
				nodes.Add(new Point16(saved.GetInt("x"), saved.GetInt("y")));
			}
		}
	}
}
