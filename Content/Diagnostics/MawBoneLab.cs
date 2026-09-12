using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	/// <summary>Opt-in, finite empty-air material fixture. No generation or ordinary-world writes.</summary>
	public sealed class MawBoneLab : ModSystem
	{
		private Rectangle bounds;
		internal Rectangle PreservedBounds => bounds;
		private string checkpoint;
		private int checks, captureDelay = -1;
		private bool viewing, previousDay, previousRain, previousEclipse;
		private double previousTime;
		private Vector2 previousPosition;
		private static bool IsQa => Main.netMode == NetmodeID.SinglePlayer &&
			Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && Main.LocalPlayer.name == "gg";
		private int Bone => ModContent.TileType<OssuaryBone>();
		private Point At(int x, int y) => new(bounds.X + x, bounds.Y + y);

		internal void Run(string request)
		{
			if (!IsQa) throw new InvalidOperationException("Bone lab requires gg in Apogee Native Visual V3, single-player.");
			string grove = ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot();
			try {
				switch (request) {
					case "build": Build(); break;
					case "reference": Mod.Logger.Info("MAW SPIKE REFERENCE: " + VanillaAtlasExporter.ExportSpikeReference()); break;
					case "test": Test(); break;
					case "day": View(false); break;
					case "night": View(true); break;
					case "capture": RequireFixture(); captureDelay = 60; break;
					case "reload": RequireFixture(); Check(Fingerprint() == checkpoint, "saved-material-state-unchanged"); View(false); break;
					case "release": Release(); break;
					default: throw new InvalidOperationException("Unknown bone fixture request.");
				}
			} finally {
				if (grove != ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot())
					throw new InvalidOperationException("Bone lab changed the preserved grove.");
				Mod.Logger.Info("MAW BONE GROVE GUARD: unchanged; not a grove reload acceptance.");
			}
		}

		private void Build()
		{
			if (!bounds.IsEmpty) throw new InvalidOperationException("Saved bone fixture exists; never rebuild accepted evidence automatically.");
			Rectangle reserved = ModContent.GetInstance<VegetationVisualLab>().PreservedBounds;
			reserved.Inflate(16, 16);
			foreach (int dx in new[] { 640, -720, 800, -880, 960, -1040 }) {
				foreach (int dy in new[] { -100, -150, -200 }) {
					Rectangle candidate = new(Main.spawnTileX + dx, Main.spawnTileY + dy, 80, 38);
					Rectangle envelope = candidate; envelope.Inflate(3, 3);
					if (envelope.Left < 30 || envelope.Top < 40 || envelope.Right > Main.maxTilesX - 30 ||
						envelope.Bottom > Main.maxTilesY - 40 || envelope.Intersects(reserved) || !Empty(envelope)) continue;
					bounds = candidate; break;
				}
				if (!bounds.IsEmpty) break;
			}
			if (bounds.IsEmpty) throw new InvalidOperationException("No empty bone fixture site. No terrain cleared.");
			// Top: native Stone control / candidate bone / native-framed Mawstone merge seam.
			Fill(4, 4, 18, 8, TileID.Stone);
			Fill(28, 4, 18, 8, Bone);
			Fill(52, 4, 10, 8, Bone);
			Fill(62, 4, 10, 8, ModContent.TileType<Mawstone>());
			// Second row: isolated, horizontal, vertical, exterior and interior corners.
			Fill(4, 16, 1, 1, Bone); Fill(8, 16, 6, 1, Bone); Fill(18, 15, 1, 6, Bone);
			Fill(23, 15, 5, 2, Bone); Fill(23, 17, 2, 4, Bone);
			for (int k = 0; k < 5; k++) Fill(33 + k * 4, 17, 3, 3, Bone);
			Fill(57, 16, 5, 4, Bone); Fill(65, 16, 5, 4, Bone);
			Fill(2, 32, 76, 2, TileID.GrayBrick);
			WorldGen.RangeFrame(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			for (int k = 0; k < 4; k++) {
				Tile t = Main.tile[At(33 + k * 4, 17)]; t.Slope = (SlopeType)(k + 1);
			}
			Tile half = Main.tile[At(49, 17)]; half.IsHalfBlock = true;
			for (int x = 57; x < 62; x++) for (int y = 16; y < 20; y++) {
				Tile t = Main.tile[At(x, y)]; t.TileColor = PaintID.BluePaint;
			}
			for (int x = 65; x < 70; x++) for (int y = 16; y < 20; y++) {
				Tile t = Main.tile[At(x, y)]; t.HasActuator = true; t.IsActuated = true;
			}
			checkpoint = Fingerprint();
			Mod.Logger.Info($"MAW BONE BUILD: bounds={bounds}; empty-envelope=True; checksum={checkpoint}");
			View(false);
		}

		private static bool Empty(Rectangle area)
		{
			for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
				Tile t = Main.tile[x, y];
				if (t.HasTile || t.WallType != WallID.None || t.LiquidAmount > 0 || t.HasActuator || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire) return false;
			}
			return true;
		}

		private void Fill(int x, int y, int width, int height, int type)
		{
			for (int i = x; i < x + width; i++) for (int j = y; j < y + height; j++) {
				Point p = At(i, j);
				WorldGen.PlaceTile(p.X, p.Y, type, mute: true);
				if (!Main.tile[p].HasTile || Main.tile[p].TileType != type) throw new InvalidOperationException("Native material placement failed.");
			}
		}

		private void RequireFixture()
		{
			if (bounds.Width != 80 || bounds.Height != 38 || bounds.Left < 30 || bounds.Top < 40 ||
				bounds.Right > Main.maxTilesX - 30 || bounds.Bottom > Main.maxTilesY - 40 || checkpoint == null)
				throw new InvalidOperationException("Missing or invalid saved bone fixture.");
		}

		private void Check(bool pass, string name)
		{
			if (!pass) throw new InvalidOperationException("Bone assertion failed: " + name);
			checks++; Mod.Logger.Info("MAW BONE CHECK PASS: " + name);
		}

		private void Test()
		{
			RequireFixture(); checks = 0;
			Check(Fingerprint() == checkpoint, "unchanged-before-tests");
			var texture = TextureAssets.Tile[Bone].Value;
			Check(texture.Width == 288 && texture.Height == 270, "loaded-native-atlas-dimensions");
			int sampled = 0;
			for (int x = bounds.Left; x < bounds.Right; x++) for (int y = bounds.Top; y < bounds.Bottom; y++) {
				Tile t = Main.tile[x, y]; if (!t.HasTile || t.TileType != Bone) continue;
				if (t.TileFrameX < 0 || t.TileFrameY < 0 || t.TileFrameX + 16 > texture.Width || t.TileFrameY + 16 > texture.Height)
					throw new InvalidOperationException($"Native bone frame outside atlas at {x},{y}: {t.TileFrameX},{t.TileFrameY}");
				sampled++;
			}
			Check(sampled > 200, $"native-frame-bounds-{sampled}-cells");
			ModTile definition = TileLoader.GetTile(Bone);
			Check(Main.tileSolid[Bone] && Main.tileBlockLight[Bone] && !Main.tileLighted[Bone], "solid-light-blocking-not-emissive");
			Check(TileID.Sets.TouchDamageImmediate[Bone] == 0, "structural-bone-is-not-a-contact-hazard");
			Check(definition.MinPick == 59 && definition.MineResist == 2.7f, "existing-platinum-tier-resistance");
			Point p = At(5, 27);
			Check(Empty(new Rectangle(p.X - 1, p.Y - 1, 3, 3)), "empty-mechanical-test-slot");
			try {
				Fill(5, 27, 1, 1, Bone);
				Check(Collision.SolidCollision(p.ToVector2() * 16, 16, 16), "native-solid-collision");
				Tile t = Main.tile[p]; t.HasActuator = true; t.IsActuated = true;
				Check(!Collision.SolidCollision(p.ToVector2() * 16, 16, 16), "actuated-collision-disabled");
				t.IsActuated = false; t.HasActuator = false;
				for (int hit = 0; hit < 30; hit++) Main.LocalPlayer.PickTile(p.X, p.Y, 58);
				Check(Main.tile[p].HasTile, "pick-power-58-cannot-mine");
				int hits = 0;
				for (; hits < 40 && Main.tile[p].HasTile; hits++) Main.LocalPlayer.PickTile(p.X, p.Y, 59);
				Check(!Main.tile[p].HasTile, $"pick-power-59-mines-in-{hits}-hits");
				Fill(5, 27, 1, 1, Bone);
				Check(TileLoader.CanExplode(p.X, p.Y), "native-explosion-permission");
				// Permission above is NOT an actual bomb simulation or an item-drop test.
			} finally { WorldGen.KillTile(p.X, p.Y, noItem: true); }
			Check(Fingerprint() == checkpoint, "test-slot-restored-and-fixture-unchanged");
			Mod.Logger.Info($"MAW BONE MATRIX PASS: {checks} checks; native-programmatic-API-tests; not manual mining, bomb simulation, or multiplayer proof.");
			View(false);
		}

		private string Fingerprint()
		{
			using MemoryStream stream = new(); using BinaryWriter w = new(stream);
			for (int x = bounds.Left; x < bounds.Right; x++) for (int y = bounds.Top; y < bounds.Bottom; y++) {
				Tile t = Main.tile[x, y]; w.Write(t.HasTile);
				if (t.HasTile) {
					w.Write(TileLoader.GetTile(t.TileType)?.FullName ?? $"Terraria/{t.TileType}");
					// Ordinary frames are recomputed on load; validate them separately against the atlas.
					w.Write((byte)t.Slope); w.Write(t.IsHalfBlock); w.Write(t.TileColor); w.Write(t.IsActuated);
					w.Write(t.IsTileInvisible); w.Write(t.IsTileFullbright);
				}
				w.Write(t.WallType); w.Write(t.LiquidAmount); if (t.LiquidAmount > 0) w.Write(t.LiquidType);
				w.Write(t.HasActuator); w.Write(t.RedWire); w.Write(t.BlueWire); w.Write(t.GreenWire); w.Write(t.YellowWire);
			}
			w.Flush(); return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
		}

		private void View(bool night)
		{
			RequireFixture();
			if (!viewing) {
				previousPosition = Main.LocalPlayer.position;
				previousDay = Main.dayTime; previousTime = Main.time; previousRain = Main.raining; previousEclipse = Main.eclipse;
				viewing = true;
			}
			Main.dayTime = !night; Main.time = night ? 16000 : 27000; Main.raining = false; Main.eclipse = false;
			Main.LocalPlayer.Teleport(new Vector2((bounds.X + 40) * 16, (bounds.Y + 32) * 16 - Main.LocalPlayer.height), 1);
			Main.LocalPlayer.velocity = Vector2.Zero;
			Main.NewText("Bone QA: top Stone / bone / bone-Mawstone seam. Lower: edges, slopes, half-block, blue paint, actuation.", Color.Wheat);
		}

		public override void PostUpdateEverything()
		{
			if (!IsQa || captureDelay < 0 || captureDelay-- != 0) return;
			captureDelay = -1;
			// Vanilla capture biome with a validated water slot; no custom drawing or injected lights.
			CaptureManager.Instance.Capture(new CaptureSettings {
				Area = bounds, Biome = new CaptureBiome(0, 0, Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),
				CaptureBackground = true, CaptureEntities = true, UseScaling = true,
				OutputName = "Apogean Maw Bone " + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")
			});
		}

		internal void Release()
		{
			captureDelay = -1;
			if (!viewing) return;
			Main.dayTime = previousDay; Main.time = previousTime; Main.raining = previousRain; Main.eclipse = previousEclipse;
			if (IsQa) Main.LocalPlayer.Teleport(previousPosition, 1);
			viewing = false;
		}
		public override void SaveWorldData(TagCompound tag)
		{
			if (!IsQa || checkpoint == null) return;
			tag["mawBoneFixtureV1"] = new TagCompound { ["x"] = bounds.X, ["y"] = bounds.Y, ["checkpoint"] = checkpoint };
		}
		public override void LoadWorldData(TagCompound tag)
		{
			if (Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3" || !tag.ContainsKey("mawBoneFixtureV1")) return;
			TagCompound saved = tag.GetCompound("mawBoneFixtureV1");
			bounds = new Rectangle(saved.GetInt("x"), saved.GetInt("y"), 80, 38); checkpoint = saved.GetString("checkpoint");
		}
		public override void ClearWorld() { bounds = Rectangle.Empty; checkpoint = null; viewing = false; captureDelay = -1; }
	}
}
