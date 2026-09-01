using System;
using System.Collections.Generic;
using System.IO;
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
using apogean.Common.WorldGeneration;
using apogean.Content.Tiles;

namespace apogean.Content.World
{
	/// <summary>
	/// Owns Engraft geography. Nodes are finite and saved; they only convert plain, un-walled natural-looking terrain,
	/// deliberately leaving housing, chests and authored structures alone.
	/// </summary>
	public sealed class EngraftSystem : ModSystem
	{
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

		internal void GenerateWorld(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "Something is taking root...";
			ApogeanWorldPlan plan = ApogeanWorldPlanSystem.Instance.CreateWorldGenPlan(WorldGen.genRand, FindSurface);
			for (int i = 0; i < plan.MawRuptures.Count; i++)
			{
				MawRupturePlan rupture = plan.MawRuptures[i];
				int ruptureSeed = unchecked(plan.PlanSeed ^ ((i + 1) * 73856093));
				CreateRupture(rupture.SurfaceCenter, rupture.RadiusX, rupture.RadiusY, new UnifiedRandom(ruptureSeed), WorldEditIntent.MawGeneration);
				RegisterNode(new Point16(rupture.SurfaceCenter.X, Math.Max(10, rupture.SurfaceCenter.Y - 3)));
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

		private static void CreateRupture(
			Point16 center,
			int radiusX,
			int radiusY,
			UnifiedRandom random,
			WorldEditIntent intent)
		{
			int turf = ModContent.TileType<EngraftTurf>();
			for (int x = center.X - radiusX; x <= center.X + radiusX; x++)
			{
				for (int y = center.Y - radiusY; y <= center.Y + radiusY; y++)
				{
					if (!WorldGen.InWorld(x, y, 12) || !ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, intent)) continue;
					float dx = (x - center.X) / (float)radiusX;
					float dy = (y - center.Y) / (float)radiusY;
					if (dx * dx + dy * dy > 1f + random.NextFloat(-0.14f, 0.12f)) continue;

					Tile tile = Framing.GetTileSafely(x, y);
					if (!IsConvertibleTerrain(x, y, tile, intent)) continue;
					tile.HasTile = true;
					tile.TileType = (ushort)turf;
				}
			}

			MutateSurfaceGrowth(center, radiusX, radiusY, random, intent);
		}

		private static bool IsConvertibleTerrain(int x, int y, Tile tile, WorldEditIntent intent)
		{
			if (!ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, intent)) return false;
			if (!tile.HasTile || (tile.WallType > WallID.None && Main.wallHouse[tile.WallType])) return false;
			return tile.TileType is TileID.Dirt or TileID.Grass or TileID.Stone or TileID.ClayBlock or TileID.Mud or
				TileID.Sand or TileID.HardenedSand or TileID.Sandstone;
		}

		private static void MutateSurfaceGrowth(
			Point16 center,
			int radiusX,
			int radiusY,
			UnifiedRandom random,
			WorldEditIntent intent)
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
					if (!ApogeanWorldPlanSystem.Instance.CanEditTile(x, y, intent)) continue;
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
				if (!IsConvertibleTerrain(x, y, tile, WorldEditIntent.MawSpread)) continue;
				tile.TileType = (ushort)turf;
				NetMessage.SendTileSquare(-1, x, y);
				return;
			}
		}

		/// <summary>Called only when a Hardmode altar breaks. At most four new regions are created after the three worldgen nodes.</summary>
		public void SeedFromDestroyedAltar()
		{
			if (!Main.hardMode || nodes.Count >= MaxTotalNodes) return;
			int preferredX = Main.rand.Next(300, Main.maxTilesX - 300);
			if (!ApogeanWorldPlanSystem.Instance.TryAddRuntimeRupture(
				preferredX,
				48,
				28,
				bypassProtection: false,
				FindSurface,
				Main.rand,
				out MawRupturePlan rupture) || rupture is null)
				return;

			CreateRupture(rupture.SurfaceCenter, rupture.RadiusX, rupture.RadiusY, Main.rand, WorldEditIntent.MawGeneration);
			RegisterNode(new Point16(rupture.SurfaceCenter.X, Math.Max(10, rupture.SurfaceCenter.Y - 3)));
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

		/// <summary>Playtest-only world mutation used by /apogean engraft.</summary>
		public bool TryCreateDebugRupture(Player player, bool bypassProtection, out string failureReason)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				failureReason = "World-generation debug edits must be run by the server or in single player.";
				return false;
			}

			int preferredX = (int)(player.Center.X / 16f);
			if (!ApogeanWorldPlanSystem.Instance.TryAddRuntimeRupture(
				preferredX,
				90,
				45,
				bypassProtection,
				FindSurface,
				Main.rand,
				out MawRupturePlan rupture) || rupture is null)
			{
				failureReason = "No safe Maw site was found nearby. Move away from spawn and protected landmarks, or use /apogean engraft force.";
				return false;
			}

			CreateRupture(
				rupture.SurfaceCenter,
				rupture.RadiusX,
				rupture.RadiusY,
				Main.rand,
				bypassProtection ? WorldEditIntent.None : WorldEditIntent.MawGeneration);
			RegisterNode(new Point16(rupture.SurfaceCenter.X, Math.Max(10, rupture.SurfaceCenter.Y - 3)));
			failureReason = string.Empty;
			return true;
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

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write((byte)nodes.Count);
			for (int i = 0; i < nodes.Count; i++)
			{
				writer.Write(nodes[i].X);
				writer.Write(nodes[i].Y);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			nodes.Clear();
			int count = reader.ReadByte();
			for (int i = 0; i < count; i++)
				nodes.Add(new Point16(reader.ReadInt16(), reader.ReadInt16()));
		}
	}
}
