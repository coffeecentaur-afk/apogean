using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using apogean.Common.Geometry;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	// A finite separate empty-air test. Never clears a worldgen region or the saved grove.
	public sealed class MawFangLab : ModSystem
	{
		private Rectangle bounds;
		internal Rectangle PreservedBounds => bounds;
		private bool viewing, previousDay, previousRain, previousEclipse;
		private double previousTime;
		private Vector2 previousPosition;
		private int captureDelay = -1, checks;
		private static bool IsQa => Main.netMode == NetmodeID.SinglePlayer && Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && Main.LocalPlayer.name == "gg";
		private int Fang => ModContent.TileType<MawFangTile>();
		private Point At(int x, int y) => new(bounds.X + x, bounds.Y + y);
		internal void Run(string request)
		{
			if (!IsQa || !MawFangTile.CandidateIncluded(Mod)) throw new InvalidOperationException("Candidate fang lab requires gg / V3 / single-player.");
			string grove = ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot();
			try {
				switch (request) {
					case "build": Build(); break;
					case "test": Test(); break;
					case "day": View(false); break;
					case "night": View(true); break;
					case "capture": RequireFixture(); captureDelay = 30; break;
					case "reload": RequireFixture(); CheckFixtures(); View(false); break;
					case "release": Release(); break;
					default: throw new InvalidOperationException("Unknown fang lab request.");
				}
			} finally {
				if (grove != ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot()) throw new InvalidOperationException("Fang lab changed preserved grove.");
				Mod.Logger.Info("MAW FANG GROVE GUARD: unchanged; existing grove reload mismatch is not accepted.");
			}
		}
		private static bool Empty(Rectangle area)
		{
			for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
				Tile t = Main.tile[x, y];
				if (t.HasTile || t.WallType != WallID.None || t.LiquidAmount > 0 || t.HasActuator || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire) return false;
			}
			return true;
		}
		private void Build()
		{
			if (!bounds.IsEmpty) throw new InvalidOperationException("Saved fang fixture exists; no automatic replacement.");
			Rectangle reserved = ModContent.GetInstance<VegetationVisualLab>().PreservedBounds; reserved.Inflate(20, 20);
			foreach (int dx in new[] { 760, -820, 920, -1000 }) {
				foreach (int dy in new[] { -100, -150, -210 }) {
					Rectangle candidate = new(Main.spawnTileX + dx, Main.spawnTileY + dy, 46, 30);
					Rectangle envelope = candidate; envelope.Inflate(4, 4);
					if (envelope.Left < 40 || envelope.Top < 40 || envelope.Right > Main.maxTilesX - 40 || envelope.Bottom > Main.maxTilesY - 40 || envelope.Intersects(reserved) || !Empty(envelope)) continue;
					bounds = candidate; break;
				}
				if (!bounds.IsEmpty) break;
			}
			if (bounds.IsEmpty) throw new InvalidOperationException("No empty fang fixture site; no terrain cleared.");
			for (int x = 1; x < 45; x++) for (int y = 27; y < 29; y++) WorldGen.PlaceTile(At(x, y).X, At(x, y).Y, TileID.GrayBrick, mute: true);
			for (int o = 0; o < 4; o++) {
				Point p = At(8 + o * 8, 12); Supports(p, o, true);
				Check(MawFangTile.TryPlace(p, o, Fang), "display-place-" + o);
			}
			for (int x = 5; x < 9; x++) WorldGen.PlaceTile(At(x, 26).X, At(x, 26).Y, TileID.Spikes, mute: true);
			for (int y = 8; y < 24; y++) WorldGen.PlaceTile(At(40, y).X, At(40, y).Y, TileID.Rope, mute: true);
			WorldGen.RangeFrame(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			Mod.Logger.Info($"MAW FANG BUILD: bounds={bounds}; empty-envelope=True; no worldgen integration.");
			View(false);
		}
		private static void Supports(Point root, int o, bool place)
		{
			for (int k = 0; k < 2; k++) {
				int x = k, y = 3; MawFangShape.Rotate(ref x, ref y, o);
				if (place) WorldGen.PlaceTile(root.X + x, root.Y + y, TileID.GrayBrick, mute: true);
				else WorldGen.KillTile(root.X + x, root.Y + y, noItem: true);
			}
		}
		private void RequireFixture()
		{
			if (bounds.Width != 46 || bounds.Height != 30 || bounds.Left < 40 || bounds.Top < 40 || bounds.Right >= Main.maxTilesX - 40 || bounds.Bottom >= Main.maxTilesY - 40) throw new InvalidOperationException("Invalid/missing fang fixture.");
		}
		private void Check(bool pass, string name)
		{
			if (!pass) throw new InvalidOperationException("Fang assertion failed: " + name);
			checks++; Mod.Logger.Info("MAW FANG CHECK PASS: " + name);
		}
		private void CheckFixtures()
		{
			for (int o = 0; o < 4; o++) {
				Point root = At(8 + o * 8, 12);
				for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++) {
					Tile t = Main.tile[root.X + x, root.Y + y]; int s = MawFangShape.Slope(x, y, o);
					if (s < 0) Check(!t.HasTile, $"air-cell-{o}-{x}-{y}");
					else Check(MawFangTile.Part(root.X + x, root.Y + y, Fang, out Point p, out int f) && p == root && f == o && (int)t.Slope == s, $"frame-slope-{o}-{x}-{y}");
				}
			}
		}
		private int Count(Point root)
		{
			int count = 0;
			for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++) if (Main.tile[root.X + x, root.Y + y].HasTile && Main.tile[root.X + x, root.Y + y].TileType == Fang) count++;
			return count;
		}
		private void CheckNativeSlope(Point root, int o)
		{
			// Independent native movement solver, not the shape-aware Hurt predicate.
			for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++) {
				Tile t = Main.tile[root.X + x, root.Y + y]; if (!t.HasTile || t.TileType != Fang || t.Slope == SlopeType.Solid) continue;
				Vector2 origin = new((root.X + x) * 16, (root.Y + y) * 16);
				Vector2 inside, outside;
				switch ((int)t.Slope) {
					case 1: inside = new(2, 12); outside = new(12, 1); break;
					case 2: inside = new(12, 12); outside = new(1, 1); break;
					case 3: inside = new(2, 2); outside = new(12, 12); break;
					default: inside = new(12, 2); outside = new(1, 12); break;
				}
				Vector2 solid = origin + inside, air = origin + outside;
				Vector4 hit = Collision.SlopeCollision(solid, Vector2.Zero, 2, 2);
				Vector4 clear = Collision.SlopeCollision(air, Vector2.Zero, 2, 2);
				Check(hit.Y != solid.Y, $"native-slope-resolves-solid-{o}-{x}-{y}");
				Check(clear.X == air.X && clear.Y == air.Y, $"native-slope-clear-triangle-{o}-{x}-{y}");
				t.IsActuated = true;
				try {
					Vector4 inactive = Collision.SlopeCollision(solid, Vector2.Zero, 2, 2);
					Check(inactive.X == solid.X && inactive.Y == solid.Y, "native-actuated-slope-does-not-collide");
				} finally { t.IsActuated = false; }
			}
		}
		private void Test()
		{
			RequireFixture(); checks = 0; CheckFixtures();
			for (int o = 0; o < 4; o++) CheckNativeSlope(At(8 + o * 8, 12), o);
			var texture = TextureAssets.Tile[Fang].Value;
			Check(texture.Width == 54 && texture.Height == 216, "loaded-atlas-54x216");
			var tile = TileLoader.GetTile(Fang);
			Check(Main.tileSolid[Fang] && !Main.tileSolidTop[Fang] && tile.MinPick == 0 && tile.MineResist == 0.8f && TileID.Sets.TouchDamageImmediate[Fang] == 0, "solid-easy-mining-shape-aware-hazard");
			Point trial = At(18, 21); Rectangle envelope = new(trial.X - 2, trial.Y - 2, 7, 7);
			Check(Empty(envelope), "empty-mechanical-trial-envelope");
			bool[] existingItems = new bool[Main.maxItems]; for (int n = 0; n < Main.maxItems; n++) existingItems[n] = Main.item[n].active;
			try {
				for (int o = 0; o < 4; o++) {
					Supports(trial, o, true);
					for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++) {
						if (MawFangShape.Slope(x, y, o) < 0) continue;
						Check(MawFangTile.TryPlace(trial, o, Fang), $"trial-place-{o}-{x}-{y}");
						Check(!MawFangTile.TryPlace(trial, o, Fang), "occupied-placement-rejected");
						int hits = 0; while (hits++ < 10 && Count(trial) > 0) Main.LocalPlayer.PickTile(trial.X + x, trial.Y + y, 35);
						Check(Count(trial) == 0 && hits <= 5, $"starter-pick-whole-fang-{o}-{x}-{y}-hits-{hits - 1}");
						Check(CollectTestDrops(existingItems, trial) == 1, "one-bone-no-duplicate-drops");
					}
					Check(MawFangTile.TryPlace(trial, o, Fang), "support-trial-place");
					Supports(trial, o, false); WorldGen.RangeFrame(trial.X - 1, trial.Y - 1, trial.X + 4, trial.Y + 4);
					Check(Count(trial) == 0, "support-loss-cleans-object-" + o);
					Check(CollectTestDrops(existingItems, trial) == 1, "support-loss-one-drop");
				}
				Supports(trial, 0, true); Check(MawFangTile.TryPlace(trial, 0, Fang), "contact-trial-place");
				Player probe = new() { whoAmI = Main.myPlayer, width = 2, height = 2, statLife = 200, statLifeMax = 200, statLifeMax2 = 200 };
				probe.position = trial.ToVector2() * 16 + new Vector2(12, 1);
				Check(!MawFangTile.Touching(probe, Fang), "empty-side-of-diagonal-does-not-hurt");
				probe.position = trial.ToVector2() * 16 + new Vector2(2, 12);
				Check(MawFangTile.Touching(probe, Fang), "solid-side-of-diagonal-contacts");
				Tile tip = Main.tile[trial]; tip.IsActuated = true;
				Check(!MawFangTile.Touching(probe, Fang), "actuated-tip-not-dangerous"); tip.IsActuated = false;
				int before = probe.statLife; double damage = MawFangTile.ApplyContact(probe, Fang);
				Check(damage > 0 && probe.statLife < before && !probe.dead, $"native-Hurt-health-loss-{before - probe.statLife}");
				int after = probe.statLife; MawFangTile.ApplyContact(probe, Fang);
				Check(probe.statLife == after && probe.immune, "native-Hurt-immunity-prevents-double-hit");
				Check(TileLoader.CanExplode(trial.X, trial.Y), "explosion-permission-only");
			} finally {
				// This envelope was proven empty; remove only test-owned fang/support tiles.
				for (int x = envelope.Left; x < envelope.Right; x++) for (int y = envelope.Top; y < envelope.Bottom; y++) {
					Tile t = Main.tile[x, y]; if (t.HasTile && (t.TileType == Fang || t.TileType == TileID.GrayBrick)) WorldGen.KillTile(x, y, noItem: true);
				}
				CollectTestDrops(existingItems, trial);
			}
			Check(Empty(envelope), "mechanical-envelope-restored"); CheckFixtures();
			Mod.Logger.Info($"MAW FANG MATRIX PASS: {checks} native programmatic checks. Hurt uses an isolated Player instance; manual movement/rope/bombs/multiplayer not certified.");
			View(false);
		}
		private static int CollectTestDrops(bool[] before, Point root)
		{
			int count = 0;
			for (int n = 0; n < Main.maxItems; n++) {
				Item item = Main.item[n];
				if (!before[n] && item.active && item.type == ItemID.Bone && Vector2.Distance(item.Center, root.ToVector2() * 16) < 120) { count += item.stack; item.active = false; }
			}
			return count;
		}
		private void View(bool night)
		{
			RequireFixture();
			if (!viewing) { previousPosition = Main.LocalPlayer.position; previousDay = Main.dayTime; previousTime = Main.time; previousRain = Main.raining; previousEclipse = Main.eclipse; viewing = true; }
			Main.dayTime = !night; Main.time = night ? 16000 : 27000; Main.raining = false; Main.eclipse = false;
			Main.LocalPlayer.Teleport(new Vector2((bounds.X + 24) * 16, (bounds.Y + 27) * 16 - Main.LocalPlayer.height), 1);
			Main.LocalPlayer.velocity = Vector2.Zero;
			Main.NewText("Fang QA: four orientations above. Native spikes left / rope right. Candidate, not generated content.", Color.Wheat);
		}
		public override void PostUpdateEverything()
		{
			if (!IsQa || captureDelay < 0 || captureDelay-- != 0) return;
			captureDelay = -1;
			CaptureManager.Instance.Capture(new CaptureSettings { Area = bounds,
				Biome = new CaptureBiome(0, 0, Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),
				CaptureBackground = true, CaptureEntities = true, UseScaling = true,
				OutputName = "Apogean Maw Fang " + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") });
		}
		internal void Release()
		{
			captureDelay = -1; if (!viewing) return;
			Main.dayTime = previousDay; Main.time = previousTime; Main.raining = previousRain; Main.eclipse = previousEclipse;
			if (IsQa) Main.LocalPlayer.Teleport(previousPosition, 1); viewing = false;
		}
		public override void SaveWorldData(TagCompound tag) { if (IsQa && !bounds.IsEmpty) tag["mawFangFixtureV1"] = new TagCompound { ["x"] = bounds.X, ["y"] = bounds.Y }; }
		public override void LoadWorldData(TagCompound tag) {
			if (Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && tag.ContainsKey("mawFangFixtureV1")) {
				TagCompound saved = tag.GetCompound("mawFangFixtureV1"); bounds = new Rectangle(saved.GetInt("x"), saved.GetInt("y"), 46, 30);
			}
		}
		public override void ClearWorld() { bounds = Rectangle.Empty; viewing = false; captureDelay = -1; }
	}
}
