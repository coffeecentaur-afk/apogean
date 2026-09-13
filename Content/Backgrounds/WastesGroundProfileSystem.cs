using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using apogean.Common.Backgrounds;
using apogean.Content.Tiles;

namespace apogean.Content.Backgrounds
{
	// QA-only, single-player contract. A later production version must capture
	// during generation and synchronize server-owned data before client drawing.
	public sealed class WastesGroundProfileSystem : ModSystem
	{
		private const string SaveKey = "wastesGroundProfileQA2";
		private WastesGroundProfile profile;
		private static bool InScope => Main.netMode == NetmodeID.SinglePlayer &&
			Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3";
		// Preserve a loaded immutable snapshot on servers; never sample or draw there.
		private static bool OwnsQaSave => Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3";
		private static int ExpectedCount => (Main.maxTilesX - 1) / WastesGroundProfile.StepTiles + 2;
		public override void OnWorldLoad() => profile = null;
		public override void OnWorldUnload() => profile = null;

		public override void LoadWorldData(TagCompound tag)
		{
			if (!OwnsQaSave || !tag.TryGet(SaveKey, out int[] rows) || rows.Length != ExpectedCount) return;
			foreach (int row in rows)
				if (row < 10 || row >= Main.maxTilesY - 10) return;
			profile = new WastesGroundProfile(rows);
		}
		public override void SaveWorldData(TagCompound tag)
		{
			if (OwnsQaSave && profile != null) tag[SaveKey] = profile.CopyRows();
		}

		public override void PostUpdateWorld()
		{
			if (!InScope || profile != null) return;
			// One bounded snapshot outside drawing, never an every-frame tile scan.
			int first = Math.Max(10, (int)(Main.worldSurface * .5));
			int last = Math.Min(Main.maxTilesY - 10, (int)Main.worldSurface + 96);
			int fallback = Math.Clamp(Main.spawnTileY, first, last);
			int[] rows = new int[ExpectedCount];
			int[] neighbors = new int[5];
			for (int i = 0; i < rows.Length; i++)
			{
				for (int n = 0; n < neighbors.Length; n++)
				{
					int x = Math.Clamp(i * WastesGroundProfile.StepTiles + (n - 2) * 4, 10, Main.maxTilesX - 11);
					neighbors[n] = WastesGroundProfile.FindSurface(x, first, last, fallback, IsNaturalSolid);
				}
				Array.Sort(neighbors);
				rows[i] = neighbors[2];
			}
			profile = new WastesGroundProfile(rows);
			Mod.Logger.Info($"WASTES REGIONAL GROUND: samples={rows.Length}; stepTiles={WastesGroundProfile.StepTiles}; centerGroundY={profile.GroundAt(Main.maxTilesX * 8f)}; source=initialQATerrain; saveEnabled=True; scope=singlePlayerQA; artApproval=False");
		}

		internal static bool TryGroundAt(float worldX, out float worldY)
		{
			WastesGroundProfile current = ModContent.GetInstance<WastesGroundProfileSystem>().profile;
			worldY = current?.GroundAt(worldX) ?? 0;
			return InScope && current != null;
		}
		private static bool IsNaturalSolid(int x, int y)
		{
			Tile tile = Main.tile[x, y];
			if (!tile.HasTile || tile.IsActuated || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) return false;
			int type = tile.TileType;
			return TileLoader.GetTile(type) is WastesTerrainTile || type is
				TileID.Dirt or TileID.Grass or TileID.Stone or TileID.Sand or TileID.ClayBlock or
				TileID.Mud or TileID.JungleGrass or TileID.SnowBlock or TileID.IceBlock or
				TileID.CorruptGrass or TileID.CrimsonGrass or TileID.HallowedGrass or
				TileID.Ebonstone or TileID.Crimstone or TileID.Pearlstone;
		}
	}
}
