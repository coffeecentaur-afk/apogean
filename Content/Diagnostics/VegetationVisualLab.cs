using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	/// <summary>Bounded weather/paint checks on the existing disposable grove, never a renderer replacement.</summary>
	public sealed class VegetationVisualLab : ModSystem
	{
		private Rectangle bounds;
		private int remaining;
		private string scenario;
		private string checkpoint;
		private int previousMoon;
		private bool previousDay, previousRain, previousEclipse;
		private double previousTime, previousRainTime;
		private float previousWind, previousTarget, previousRainStrength;
		private readonly List<(Point Position, byte Color)> paint = new();

		internal void Register(Rectangle fixture)
		{
			Release();
			bounds = fixture;
		}

		internal void ClearFixture()
		{
			Release();
			bounds = Rectangle.Empty;
			checkpoint = null;
		}

		private static bool IsQaWorld => Main.netMode == NetmodeID.SinglePlayer && Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3";

		// A captured baseline is deliberately NOT recomputed during save or load. Otherwise
		// rebuilding the fixture, or saving damaged state, could conceal a persistence failure.
		internal bool RestoreCheckpoint()
		{
			if (!IsQaWorld || checkpoint == null) return false;
			string actual = Fingerprint();
			if (actual != checkpoint)
				throw new InvalidOperationException($"Preserved grove differs after reload: expected={checkpoint}; actual={actual}. No rebuild performed.");
			Mod.Logger.Info($"VEGETATION RELOAD: PASS unchanged trees/props; bounds={bounds}; sha256={actual}; automatic rebuild skipped.");
			Main.NewText("Saved grove matched its checkpoint. No trees were rebuilt.", Color.LightGreen);
			return true;
		}

		private string Fingerprint()
		{
			if (bounds.Width != 170 || bounds.Height != 45 || bounds.Left < 20 || bounds.Top < 20 ||
				bounds.Right > Main.maxTilesX - 20 || bounds.Bottom > Main.maxTilesY - 20)
				throw new InvalidOperationException("Invalid preserved-grove bounds.");
			using MemoryStream stream = new();
			using BinaryWriter writer = new(stream);
			int trees = 0;
			for (int x = bounds.Left; x < bounds.Right; x++)
				for (int y = bounds.Top; y < bounds.Bottom; y++)
				{
					Tile tile = Main.tile[x, y];
					if (!tile.HasTile) continue;
					int type = tile.TileType;
					if (type != TileID.Trees && type != ModContent.TileType<DeadForestSapling>() &&
						type != ModContent.TileType<DeadTuft>() && type != ModContent.TileType<WastesBristle>() &&
						type != ModContent.TileType<WastesRootShrub>()) continue;
					if (type == TileID.Trees) trees++;
					writer.Write(x); writer.Write(y); writer.Write(type);
					writer.Write(tile.TileFrameX); writer.Write(tile.TileFrameY); writer.Write(tile.TileColor);
					writer.Write(tile.IsActuated); writer.Write(tile.IsTileInvisible); writer.Write(tile.IsTileFullbright);
				}
			if (trees < 4) throw new InvalidOperationException("No meaningful grove to checkpoint.");
			writer.Flush();
			return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (!IsQaWorld || checkpoint == null) return;
			tag["groveCheckpointV1"] = checkpoint;
			tag["groveLeft"] = bounds.Left; tag["groveTop"] = bounds.Top;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			if (!IsQaWorld || !tag.ContainsKey("groveCheckpointV1")) return;
			bounds = new Rectangle(tag.GetInt("groveLeft"), tag.GetInt("groveTop"), 170, 45);
			checkpoint = tag.GetString("groveCheckpointV1");
		}

		public override void ClearWorld() => ClearFixture();

		internal void Start(string requested)
		{
			if (Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3")
				throw new InvalidOperationException("Vegetation checks require Apogee Native Visual V3 single-player.");
			if (requested == "release") { Release(); return; }
			if (bounds.IsEmpty)
				throw new InvalidOperationException("Build the vegetation fixture first.");
			if (requested == "checkpoint")
			{
				Release();
				checkpoint = Fingerprint();
				Mod.Logger.Info($"VEGETATION CHECKPOINT: bounds={bounds}; sha256={checkpoint}; waiting for actual save/reload.");
				return;
			}
			if (requested == "coatings")
			{
				Release(); checkpoint = null;
				VegetationCoatingsLab.Build(bounds);
				Main.LocalPlayer.Teleport(new Vector2((bounds.Left + 48) * 16, (bounds.Bottom - 8) * 16), TeleportationStyleID.RodOfDiscord);
				Mod.Logger.Info($"VEGETATION COATINGS: control | deep blue | actuated | echo | illuminant; bounds={bounds}; render inspection required, not a pass.");
				return;
			}
			if (requested == "growth")
			{
				Release(); checkpoint = null;
				Mod.Logger.Info("VEGETATION GROWTH: " + VegetationGrowthLab.Run(bounds));
				Main.LocalPlayer.Teleport(new Vector2((bounds.Left + 48) * 16, (bounds.Bottom - 8) * 16), TeleportationStyleID.RodOfDiscord);
				return;
			}
			if (requested == "properties")
			{
				Release(); checkpoint = null;
				Mod.Logger.Info("VEGETATION PROPERTIES: " + VegetationLabGallery.ValidateProperties(bounds));
				return;
			}
			if (requested is not ("day" or "night" or "night-fullmoon" or "wind-left" or "wind-right" or "paint"))
				throw new ArgumentOutOfRangeException(nameof(requested));
			Release();
			previousDay = Main.dayTime; previousTime = Main.time;
			previousWind = Main.windSpeedCurrent; previousTarget = Main.windSpeedTarget;
			previousRain = Main.raining; previousRainTime = Main.rainTime;
			previousRainStrength = Main.maxRaining; previousEclipse = Main.eclipse;
			previousMoon = Main.moonPhase;
			scenario = requested;
			remaining = 3600;
			if (requested == "paint")
			{
				// Select a genuinely branched native tree. A branchless painted trunk is not branch coverage.
				int rootX = -1, branchCells = 0;
				foreach (int offset in new[] { 104, 123, 142 })
				{
					int count = 0;
					for (int y = bounds.Top; y < bounds.Bottom - 6; y++)
						foreach (int dx in new[] { -1, 1 })
							if (Main.tile[bounds.Left + offset + dx, y].HasTile && Main.tile[bounds.Left + offset + dx, y].TileType == TileID.Trees) count++;
					if (count > branchCells) { rootX = bounds.Left + offset; branchCells = count; }
				}
				if (rootX < 0) { Release(); throw new InvalidOperationException("No branched tree available; rebuild the grove before the paint check."); }
				for (int x = rootX - 2; x <= rootX + 2; x++)
					for (int y = bounds.Top; y < bounds.Bottom - 3; y++)
					{
						Tile tile = Main.tile[x, y];
						if (!tile.HasTile || tile.TileType != TileID.Trees) continue;
						paint.Add((new Point(x, y), tile.TileColor));
						WorldGen.paintTile(x, y, PaintID.DeepBluePaint);
					}
				if (paint.Count == 0) { Release(); throw new InvalidOperationException("No tree found at the paint fixture socket."); }
				Mod.Logger.Info($"VEGETATION PAINT: rootX={rootX}; sideBranchCells={branchCells}; native DeepBlue paint.");
			}
			Mod.Logger.Info($"VEGETATION VISUAL: case={scenario}; bounds={bounds}; viewport={Main.screenWidth}x{Main.screenHeight}; hold=3600 ticks; paintedCells={paint.Count}");
		}

		internal void Release()
		{
			if (remaining <= 0) return;
			foreach (var saved in paint)
			{
				Tile tile = Main.tile[saved.Position.X, saved.Position.Y];
				if (tile.HasTile && tile.TileType == TileID.Trees)
					WorldGen.paintTile(saved.Position.X, saved.Position.Y, saved.Color);
			}
			paint.Clear();
			Main.dayTime = previousDay; Main.time = previousTime;
			Main.windSpeedCurrent = previousWind; Main.windSpeedTarget = previousTarget;
			Main.raining = previousRain; Main.rainTime = previousRainTime;
			Main.maxRaining = previousRainStrength; Main.eclipse = previousEclipse;
			Main.moonPhase = previousMoon;
			remaining = 0;
			Mod.Logger.Info("VEGETATION VISUAL: released; original clock/weather/paint restored.");
		}

		public override void PostUpdateWorld()
		{
			if (remaining <= 0) return;
			if (remaining == 1) { Release(); return; }
			remaining--;
			Main.dayTime = scenario is not ("night" or "night-fullmoon");
			if (scenario == "night-fullmoon") Main.moonPhase = 0;
			Main.time = Main.dayTime ? 27000d : 16200d;
			Main.windSpeedCurrent = scenario == "wind-left" ? -.8f : scenario == "wind-right" ? .8f : .2f;
			Main.windSpeedTarget = Main.windSpeedCurrent;
			Main.raining = false; Main.maxRaining = 0; Main.rainTime = 0;
			Main.eclipse = false;
		}

		public override void OnWorldUnload() => ClearFixture();
	}
}
