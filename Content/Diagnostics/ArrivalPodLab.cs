using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using apogean.Content.Items.Placeable;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	/// <summary>Opt-in furniture probe. Never generates a pod on load or in a regular world.</summary>
	public sealed class ArrivalPodLab : ModSystem
	{
		private Rectangle bounds;
		private string checkpoint;
		private string legacyCheckpoint;
		private bool viewing, previousDay, previousRain, previousEclipse;
		private double previousTime;
		private Vector2 previousPosition;
		private int worldSpawnX, worldSpawnY, bedSpawnX, bedSpawnY;
		private readonly List<string> checks = new();
		private static bool IsQa => Main.netMode == NetmodeID.SinglePlayer &&
			Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && Main.LocalPlayer.name == "gg";
		private int PodType => ModContent.TileType<ArrivalPodTile>();
		private int ItemType => ModContent.ItemType<ArrivalPod>();
		private int FloorY => bounds.Bottom - 5;
		private Point Pod(int index) => new(bounds.Left + 8 + index * 14, FloorY - 6);

		internal void Run(string request)
		{
			if (!IsQa) throw new InvalidOperationException("Pod lab requires gg in the disposable Apogee Native Visual V3 world, single-player.");
			if (!Mod.TryFind<ModTile>(nameof(ArrivalPodTile), out _))
				throw new InvalidOperationException("Rebuild with the approved ArrivalPodCandidateDirectory; normal builds do not load this candidate.");
			VegetationVisualLab grove = ModContent.GetInstance<VegetationVisualLab>();
			string before = grove.CheckpointSnapshot();
			try
			{
				switch (request)
				{
					case "build": Build(); break;
					case "test": Test(); break;
					case "day": View(false, false); break;
					case "inside": View(false, true); break;
					case "night": View(true, false); break;
					case "reload": VerifyReload(); break;
					case "release": Release(); break;
					default: throw new InvalidOperationException("Unknown pod fixture request.");
				}
			}
			finally
			{
				if (before != grove.CheckpointSnapshot()) throw new InvalidOperationException("Pod lab changed the preserved grove.");
				Mod.Logger.Info($"ARRIVAL POD GROVE GUARD: unchanged=True; snapshot={before}; not-a-reload-acceptance");
			}
		}

		private void Build()
		{
			if (!bounds.IsEmpty) throw new InvalidOperationException("Pod fixture already exists; use test/day/reload, never silently rebuild saved evidence.");
			Rectangle grove = ModContent.GetInstance<VegetationVisualLab>().PreservedBounds;
			Rectangle reserved = grove;
			reserved.Inflate(12, 12);
			// Finite empty-air survey. The FULL framing envelope must be empty, not just furniture cells.
			foreach (int dx in new[] { 280, -280, 400, -400, 520, -520 })
			{
				foreach (int dy in new[] { -35, -60, -85, -110 })
				{
					Rectangle candidate = new(Main.spawnTileX + dx, Main.spawnTileY + dy, 64, 24);
					Rectangle envelope = candidate; envelope.Inflate(2, 2);
					if (envelope.Left < 30 || envelope.Top < 40 || envelope.Right > Main.maxTilesX - 30 ||
						envelope.Bottom > Main.maxTilesY - 40 || envelope.Intersects(reserved) || !IsEmpty(envelope)) continue;
					bounds = candidate;
					break;
				}
				if (!bounds.IsEmpty) break;
			}
			if (bounds.IsEmpty) throw new InvalidOperationException("No empty-air pod fixture envelope. No terrain cleared.");
			worldSpawnX = Main.spawnTileX; worldSpawnY = Main.spawnTileY;
			bedSpawnX = Main.LocalPlayer.SpawnX; bedSpawnY = Main.LocalPlayer.SpawnY;
			for (int x = bounds.Left + 2; x < bounds.Right - 2; x++)
			for (int y = FloorY; y <= FloorY + 2; y++)
				WorldGen.PlaceTile(x, y, ModContent.TileType<WastesSoil>(), mute: true);
			WorldGen.RangeFrame(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			for (int index = 0; index < 3; index++) Place(Pod(index));
			// Fourth slot intentionally empty for destructive controls.
			checkpoint = Fingerprint();
			Mod.Logger.Info($"ARRIVAL POD BUILD: bounds={bounds}; full-envelope-empty=True; three-objects=True; checkpoint={checkpoint}");
			View(false, false);
		}

		private static bool IsEmpty(Rectangle area)
		{
			for (int x = area.Left; x < area.Right; x++)
			for (int y = area.Top; y < area.Bottom; y++)
			{
				Tile tile = Main.tile[x, y];
				if (tile.HasTile || tile.WallType != WallID.None || tile.LiquidAmount != 0 || tile.HasActuator ||
					tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire) return false;
			}
			return true;
		}

		private void RequireFixture()
		{
			if (bounds.Width != 64 || bounds.Height != 24 || bounds.Left < 30 || bounds.Top < 40 ||
				bounds.Right > Main.maxTilesX - 30 || bounds.Bottom > Main.maxTilesY - 40)
				throw new InvalidOperationException("Missing/invalid saved pod fixture. Request build in an unused QA world first.");
		}

		private void Place(Point p)
		{
			if (!WorldGen.PlaceObject(p.X + 2, p.Y + 5, PodType, mute: true))
				throw new InvalidOperationException($"Native pod placement failed at {p}.");
			AssertObject(p);
		}

		private void AssertObject(Point p)
		{
			for (int x = 0; x < 5; x++)
			for (int y = 0; y < 6; y++)
			{
				Tile t = Main.tile[p.X + x, p.Y + y];
				if (!t.HasTile || t.TileType != PodType || t.TileFrameX != x * 18 || t.TileFrameY != y * 18)
					throw new InvalidOperationException($"Pod cell/frame mismatch at {p.X + x},{p.Y + y}.");
			}
		}

		private int Cells(Point p)
		{
			int count = 0;
			for (int x = 0; x < 5; x++)
			for (int y = 0; y < 6; y++)
				if (Main.tile[p.X + x, p.Y + y].HasTile && Main.tile[p.X + x, p.Y + y].TileType == PodType) count++;
			return count;
		}

		private List<Item> Drops()
		{
			List<Item> result = new();
			Rectangle area = new(bounds.X * 16, bounds.Y * 16, bounds.Width * 16, bounds.Height * 16);
			area.Inflate(32, 32);
			foreach (Item item in Main.ActiveItems)
				if (item.type == ItemType && area.Intersects(item.Hitbox)) result.Add(item);
			return result;
		}

		private void Check(bool condition, string name)
		{
			if (!condition) throw new InvalidOperationException("Pod assertion failed: " + name);
			checks.Add(name);
			Mod.Logger.Info("ARRIVAL POD CHECK PASS: " + name);
		}

		private void RemovedOnce(Point p, string name)
		{
			WorldGen.RangeFrame(p.X - 1, p.Y - 1, p.X + 6, p.Y + 7);
			List<Item> drops = Drops();
			Check(Cells(p) == 0 && drops.Count == 1 && drops[0].stack == 1, name);
			// Only this newly produced, counted QA drop is consumed. Never clears arbitrary world items.
			drops[0].TurnToAir();
		}

		private void Test()
		{
			RequireFixture();
			checks.Clear();
			Mod.Logger.Info("ARRIVAL POD MATRIX START: native-programmatic-API-tests");
			Point p = Pod(3);
			for (int index = 0; index < 3; index++) AssertObject(Pod(index));
			TestSnapshotContract();
			Check(Cells(p) == 0 && Drops().Count == 0, "clean-test-slot-and-no-preexisting-pod-drops");
			TileObjectData data = TileObjectData.GetTileData(PodType, 0);
			Check(data.Width == 5 && data.Height == 6 && data.CoordinateFullWidth == 90 && data.CoordinateFullHeight == 108 &&
				data.Origin.X == 2 && data.Origin.Y == 5 && data.DrawYOffset == 2, "registered-atlas-and-origin");
			Check(!Main.tileSolid[PodType] && !Main.tileSolidTop[PodType] && !Main.tileTable[PodType] &&
				!Main.tileLighted[PodType], "non-solid-not-housing-or-light");
			for (int x = 0; x < 5; x++)
			for (int y = 0; y < 6; y++)
			{
				Place(p);
				for (int hit = 0; hit < 20 && Cells(p) > 0; hit++) Main.LocalPlayer.PickTile(p.X + x, p.Y + y, 35);
				RemovedOnce(p, $"copper-pick-power-cell-{x}-{y}-one-drop-no-remnants");
			}
			for (int x = 0; x < 5; x++)
			{
				WorldGen.KillTile(p.X + x, FloorY, noItem: true);
				Check(!WorldGen.PlaceObject(p.X + 2, p.Y + 5, PodType, mute: true) && Cells(p) == 0, $"reject-missing-anchor-{x}");
				WorldGen.PlaceTile(p.X + x, FloorY, ModContent.TileType<WastesSoil>(), mute: true);
				Place(p);
				WorldGen.KillTile(p.X + x, FloorY, noItem: true);
				RemovedOnce(p, $"support-loss-{x}-one-drop");
				WorldGen.PlaceTile(p.X + x, FloorY, ModContent.TileType<WastesSoil>(), mute: true);
			}
			foreach (int liquid in new[] { LiquidID.Water, LiquidID.Lava })
			{
				// Direct liquid-state/framing probe, not a long fluid-simulation test.
				for (int x = 0; x < 5; x++)
				for (int y = 0; y < 6; y++)
				{
					Tile liquidCell = Main.tile[p.X + x, p.Y + y];
					liquidCell.LiquidType = liquid;
					liquidCell.LiquidAmount = 255;
				}
				Place(p);
				WorldGen.RangeFrame(p.X - 1, p.Y - 1, p.X + 6, p.Y + 7);
				AssertObject(p);
				Check(true, $"liquid-{liquid}-placement-and-framing");
				WorldGen.KillTile(p.X, p.Y);
				RemovedOnce(p, $"liquid-{liquid}-recovery");
				for (int x = 0; x < 5; x++)
				for (int y = 0; y < 6; y++) Main.tile[p.X + x, p.Y + y].LiquidAmount = 0;
			}
			Place(p);
			Check(!TileLoader.CanExplode(p.X, p.Y), "native-explosion-veto");
			Check(!Collision.SolidCollision(new Vector2(p.X * 16, p.Y * 16), 80, 96), "collision-free-full-footprint");
			Check(new Item(ItemType).createTile == PodType && new Item(ItemType).consumable &&
				ItemID.Sets.IsLavaImmuneRegardlessOfRarity[ItemType], "recoverable-placeable-lava-safe-item");
			WorldGen.KillTile(p.X, p.Y);
			RemovedOnce(p, "re-placement-then-removal-one-drop");
			// Native paint specimen; no custom draw/lighting code.
			Point painted = Pod(1);
			for (int x = 0; x < 5; x++)
			for (int y = 0; y < 6; y++) {
				Tile paintCell = Main.tile[painted.X + x, painted.Y + y];
				paintCell.TileColor = PaintID.BluePaint;
			}
			for (int index = 0; index < 3; index++) AssertObject(Pod(index));
			Check(Main.spawnTileX == worldSpawnX && Main.spawnTileY == worldSpawnY &&
				Main.LocalPlayer.SpawnX == bedSpawnX && Main.LocalPlayer.SpawnY == bedSpawnY, "world-and-bed-spawn-unchanged");
			checkpoint = Fingerprint();
			Mod.Logger.Info($"ARRIVAL POD MATRIX PASS: checks={checks.Count}; checkpoint={checkpoint}; programmatic-native-APIs-not-manual-input; multiplayer-pending");
			View(false, false);
		}

		private void TestSnapshotContract()
		{
			// Reproduce the old verifier defect at its real call site: ordinary terrain
			// frames are recomputed, unlike frame-important furniture frames saved by tML.
			string stable = Fingerprint();
			string legacy = Fingerprint(legacy: true);
			Tile soil = Main.tile[bounds.Left + 3, FloorY + 1];
			short originalFrame = soil.TileFrameX;
			try {
				soil.TileFrameX += 18;
				Check(Fingerprint(legacy: true) != legacy, "legacy-checkpoint-reproduces-terrain-frame-failure");
				Check(Fingerprint() == stable, "saved-state-ignores-recomputed-terrain-frames");
			} finally { soil.TileFrameX = originalFrame; }
			Tile pod = Main.tile[Pod(0).X, Pod(0).Y];
			originalFrame = pod.TileFrameX;
			try {
				pod.TileFrameX += 18;
				Check(Fingerprint() != stable, "saved-state-rejects-pod-frame-damage");
			} finally { pod.TileFrameX = originalFrame; }
			byte paint = pod.TileColor;
			try {
				pod.TileColor = PaintID.RedPaint;
				Check(Fingerprint() != stable, "saved-state-rejects-paint-change");
			} finally { pod.TileColor = paint; }
			try {
				soil.HasTile = false;
				Check(Fingerprint() != stable, "saved-state-rejects-missing-ground");
			} finally { soil.HasTile = true; }
			Check(Fingerprint() == stable, "snapshot-controls-restore-all-cells");
		}

		private string Fingerprint(bool legacy = false)
		{
			RequireFixture();
			using MemoryStream stream = new();
			using BinaryWriter writer = new(stream);
			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
			{
				Tile t = Main.tile[x, y];
				writer.Write(t.HasTile);
				if (t.HasTile) {
					if (legacy) writer.Write(t.TileType);
					else writer.Write(TileLoader.GetTile(t.TileType)?.FullName ?? $"Terraria/{t.TileType}");
					if (legacy || Main.tileFrameImportant[t.TileType]) {
						writer.Write(t.TileFrameX); writer.Write(t.TileFrameY);
					}
					writer.Write(t.TileColor); writer.Write(t.IsActuated);
					if (!legacy) {
						writer.Write((byte)t.Slope); writer.Write(t.IsHalfBlock);
						writer.Write(t.IsTileInvisible); writer.Write(t.IsTileFullbright);
					}
				}
				writer.Write(t.WallType); writer.Write(t.LiquidAmount);
				if (!legacy) {
					if (t.LiquidAmount > 0) writer.Write(t.LiquidType);
					writer.Write(t.HasActuator); writer.Write(t.RedWire); writer.Write(t.BlueWire);
					writer.Write(t.GreenWire); writer.Write(t.YellowWire);
				}
			}
			writer.Flush();
			return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
		}

		private void VerifyReload()
		{
			RequireFixture();
			if (checkpoint == null || Fingerprint() != checkpoint) throw new InvalidOperationException("Saved pod fixture differs or uses the legacy terrain-frame hash. No replacement performed.");
			for (int index = 0; index < 3; index++) AssertObject(Pod(index));
			if (Cells(Pod(3)) != 0) throw new InvalidOperationException("Test slot unexpectedly regenerated a pod.");
			Mod.Logger.Info($"ARRIVAL POD RELOAD PASS: unchanged={checkpoint}; three-objects-and-empty-test-slot=True; no-build-on-load=True");
			View(false, false);
		}

		private void View(bool night, bool inside)
		{
			RequireFixture();
			if (!viewing)
			{
				previousPosition = Main.LocalPlayer.position;
				previousDay = Main.dayTime; previousTime = Main.time;
				previousRain = Main.raining; previousEclipse = Main.eclipse;
				viewing = true;
			}
			Main.dayTime = !night; Main.time = night ? 16000 : 27000;
			Main.raining = false; Main.eclipse = false;
			Point p = Pod(0);
			Main.LocalPlayer.Teleport(new Vector2((p.X + (inside ? 1 : 7)) * 16, FloorY * 16 - Main.LocalPlayer.height), 1);
			Main.LocalPlayer.velocity = Vector2.Zero;
			Main.NewText("Pod QA: original / blue paint / original. Fixture only; no spawn change or relay.", Color.Wheat);
			Mod.Logger.Info($"ARRIVAL POD VIEW: night={night}; inside={inside}; bounds={bounds}; normal-tile-renderer=True");
		}

		internal void Release()
		{
			if (!viewing) return;
			Main.dayTime = previousDay; Main.time = previousTime;
			Main.raining = previousRain; Main.eclipse = previousEclipse;
			if (IsQa) Main.LocalPlayer.Teleport(previousPosition, 1);
			viewing = false;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (!IsQa || bounds.IsEmpty || (checkpoint == null && legacyCheckpoint == null)) return;
			tag["podFixtureV1"] = new TagCompound {
				["left"] = bounds.Left, ["top"] = bounds.Top, ["checkpoint"] = checkpoint ?? legacyCheckpoint,
				["fingerprintVersion"] = checkpoint == null ? 1 : 2, ["legacyCheckpoint"] = legacyCheckpoint ?? "",
				["spawnX"] = worldSpawnX, ["spawnY"] = worldSpawnY, ["bedX"] = bedSpawnX, ["bedY"] = bedSpawnY
			};
		}

		public override void LoadWorldData(TagCompound tag)
		{
			// Player selection may not exist yet; read QA data only, never touch tiles.
			if (Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3" || !tag.ContainsKey("podFixtureV1")) return;
			TagCompound saved = tag.GetCompound("podFixtureV1");
			bounds = new Rectangle(saved.GetInt("left"), saved.GetInt("top"), 64, 24);
			if (saved.GetInt("fingerprintVersion") == 2) {
				checkpoint = saved.GetString("checkpoint");
				legacyCheckpoint = saved.GetString("legacyCheckpoint");
			} else {
				legacyCheckpoint = saved.GetString("checkpoint");
				checkpoint = null; // Explicit matrix/checkpoint run required; never bless current state on load.
			}
			worldSpawnX = saved.GetInt("spawnX"); worldSpawnY = saved.GetInt("spawnY");
			bedSpawnX = saved.GetInt("bedX"); bedSpawnY = saved.GetInt("bedY");
		}

		public override void ClearWorld() { viewing = false; bounds = Rectangle.Empty; checkpoint = null; legacyCheckpoint = null; checks.Clear(); }
	}
}
