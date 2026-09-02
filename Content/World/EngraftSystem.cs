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
using apogean.Common.Maw;
using apogean.Content.Tiles;

namespace apogean.Content.World
{
	/// <summary>
	/// Owns Maw geography. Nodes are finite and saved; they only convert plain, un-walled natural-looking terrain,
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
			nodes.Clear();
			ApogeanWorldPlan plan = ApogeanWorldPlanSystem.Instance.CreateWorldGenPlan(WorldGen.genRand, FindSurface);
			for (int i = 0; i < plan.MawRuptures.Count; i++)
			{
				MawRupturePlan rupture = plan.MawRuptures[i];
				int ruptureSeed = unchecked(plan.PlanSeed ^ ((i + 1) * 73856093));
				MawRuptureGenerator.Generate(rupture, ruptureSeed, WorldEditIntent.MawGeneration);
				RegisterNode(new Point16(rupture.SurfaceCenter.X, Math.Max(10, rupture.SurfaceCenter.Y - 3)));
			}
		}

		private static int FindSurface(int x)
		{
			for (int y = 80; y < Main.worldSurface + 80; y++)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				if (!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
					continue;
				if (tile.TileType < TileID.Sets.IsATreeTrunk.Length && TileID.Sets.IsATreeTrunk[tile.TileType])
					continue;
				if (Main.tileFrameImportant[tile.TileType] || tile.TileType == TileID.Cactus)
					continue;
				return y;
			}
			return (int)Main.worldSurface;
		}

		public static bool IsInEngraft(Vector2 worldPosition)
		{
			int centerX = (int)(worldPosition.X / 16f);
			int centerY = (int)(worldPosition.Y / 16f);
			int found = 0;
			for (int x = centerX - BiomeRadiusX; x <= centerX + BiomeRadiusX; x += 4)
			{
				for (int y = centerY - BiomeRadiusY; y <= centerY + BiomeRadiusY; y += 4)
				{
					if (!WorldGen.InWorld(x, y, 2)) continue;
					ushort tileType = Framing.GetTileSafely(x, y).TileType;
					if (IsMawTerrain(tileType) && ++found >= 52) return true;
				}
			}
			return false;
		}

		private static bool IsMawTerrain(ushort tileType) =>
			tileType == ModContent.TileType<EngraftTurf>() ||
			tileType == ModContent.TileType<Mawstone>() ||
			tileType == ModContent.TileType<OssuaryBone>() ||
			tileType == ModContent.TileType<MawAcidPool>();

		public override void PostUpdateWorld()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			RemoveBrokenNodes();

			ulong growthInterval = MawActivityState.IsDormant ? 5400UL : 900UL;
			if (Main.GameUpdateCount - lastGrowthTick < growthInterval || nodes.Count == 0) return;
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
				if (!MawTerrainRules.CanConvert(x, y, WorldEditIntent.MawSpread)) continue;
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

			MawRuptureGenerator.Generate(rupture, Main.rand.Next(), WorldEditIntent.MawGeneration);
			RegisterNode(new Point16(rupture.SurfaceCenter.X, Math.Max(10, rupture.SurfaceCenter.Y - 3)));
			SyncRuntimeRupture(rupture);
			ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromLiteral("The Maw stirs beneath the broken altar."), new Color(194, 126, 44));
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

			MawRuptureGenerator.Generate(
				rupture,
				Main.rand.Next(),
				bypassProtection ? WorldEditIntent.None : WorldEditIntent.MawGeneration);
			RegisterNode(new Point16(rupture.SurfaceCenter.X, Math.Max(10, rupture.SurfaceCenter.Y - 3)));
			SyncRuntimeRupture(rupture);
			failureReason = string.Empty;
			return true;
		}

		private static void SyncRuntimeRupture(MawRupturePlan rupture)
		{
			if (Main.netMode != NetmodeID.Server)
				return;
			int size = Math.Max(rupture.GenerationBounds.Width, rupture.GenerationBounds.Height) + 8;
			NetMessage.SendTileSquare(-1, rupture.SurfaceCenter.X, rupture.SurfaceCenter.Y, size);
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
