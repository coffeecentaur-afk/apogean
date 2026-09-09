using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	public sealed class MawToothArtLab : ModSystem
	{
		private Rectangle bounds;
		private bool viewing, previousDay, previousRain, previousEclipse;
		private double previousTime;
		private Vector2 previousPosition;
		private int captureDelay = -1, checks;
		private static bool IsQa => Main.netMode == NetmodeID.SinglePlayer && Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && Main.LocalPlayer.name == "gg";
		private int[] Types => new[] { ModContent.TileType<MawShortToothArtTile>(), ModContent.TileType<MawLongToothArtTile>(), ModContent.TileType<MawWideToothArtTile>() };
		private static readonly int[] Locations = { 7, 14, 20, 34, 37, 40, 43, 47 };
		private static readonly int[] Variants = { 0, 1, 2, 0, 0, 1, 2, 0 };
		private static int Floor(int x) => x >= 20 && x < 46 ? 20 : x >= 10 && x < 52 ? 21 : 22;
		internal void Run(string request)
		{
			if (!IsQa || !MawToothArtTile.CandidateIncluded(Mod)) throw new InvalidOperationException("Tooth art requires gg / V3 / single-player and its candidate package.");
			string grove = ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot();
			try {
				switch (request) {
					case "build": Build(); break;
					case "test": Test(); break;
					case "day": View(false); break;
					case "night": View(true); break;
					case "capture": RequireFixture(); captureDelay = 45; break;
					case "reload": Test(); View(false); break;
					case "release": Release(); break;
					default: throw new InvalidOperationException("Unknown tooth-art request.");
				}
			} finally {
				if (grove != ModContent.GetInstance<VegetationVisualLab>().CheckpointSnapshot()) throw new InvalidOperationException("Tooth art changed the preserved grove.");
				Mod.Logger.Info("MAW TOOTH ART GROVE GUARD: unchanged; historical reload failure remains separate.");
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
			if (!bounds.IsEmpty) throw new InvalidOperationException("Existing art fixture: use reload, never replace.");
			Rectangle reserved = ModContent.GetInstance<VegetationVisualLab>().PreservedBounds; reserved.Inflate(20, 20);
			foreach (int dx in new[] { 1100, -1160, 1250, -1300 }) {
				foreach (int dy in new[] { -100, -150, -210 }) {
					Rectangle trial = new(Main.spawnTileX + dx, Main.spawnTileY + dy, 64, 28);
					Rectangle envelope = trial; envelope.Inflate(4, 4);
					if (envelope.Left < 40 || envelope.Top < 40 || envelope.Right >= Main.maxTilesX - 40 || envelope.Bottom >= Main.maxTilesY - 40 || envelope.Intersects(reserved) || !Empty(envelope)) continue;
					bounds = trial; break;
				}
				if (!bounds.IsEmpty) break;
			}
			if (bounds.IsEmpty) throw new InvalidOperationException("No empty art site; no terrain cleared.");
			int bone = ModContent.TileType<OssuaryBone>(), soil = ModContent.TileType<MawDirt>();
			for (int x = 2; x < 62; x++) for (int y = Floor(x); y < 27; y++)
				WorldGen.PlaceTile(bounds.X + x, bounds.Y + y, x >= 53 ? TileID.Stone : soil, mute: true);
			foreach (int x in Locations) for (int k = 0; k < 2; k++) {
				Tile t = Main.tile[bounds.X + x + k, bounds.Y + Floor(x)];
				t.TileType = (ushort)bone;
			}
			WorldGen.RangeFrame(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			int[] types = Types;
			for (int n = 0; n < Locations.Length; n++)
				Check(WorldGen.PlaceObject(bounds.X + Locations[n], bounds.Y + Floor(Locations[n]) - 1, types[Variants[n]], mute: true), "native-place-" + n);
			for (int x = 56; x < 59; x++) WorldGen.PlaceTile(bounds.X + x, bounds.Y + 21, TileID.Spikes, mute: true);
			Mod.Logger.Info($"MAW TOOTH ART BUILD: bounds={bounds}; eight native objects, empty-envelope=True; art only.");
			Test(); View(false);
		}
		private void RequireFixture()
		{
			if (bounds.Width != 64 || bounds.Height != 28 || bounds.Left < 40 || bounds.Top < 40 || bounds.Right >= Main.maxTilesX - 40 || bounds.Bottom >= Main.maxTilesY - 40) throw new InvalidOperationException("Missing/invalid tooth art fixture.");
		}
		private void Check(bool pass, string name)
		{
			if (!pass) throw new InvalidOperationException("Tooth art check failed: " + name);
			checks++; Mod.Logger.Info("MAW TOOTH ART CHECK PASS: " + name);
		}
		private void Test()
		{
			RequireFixture(); checks = 0; int[] types = Types;
			int[] widths = { 1, 1, 2 }, heights = { 2, 3, 4 }, pixels = { 163, 244, 664 };
			for (int v = 0; v < 3; v++) {
				int type = types[v]; TileObjectData data = TileObjectData.GetTileData(type, 0);
				Check(data != null && data.Width == widths[v] && data.Height == heights[v] && data.CoordinateWidth == 16 && data.CoordinatePadding == 2 && data.DrawYOffset == 4, "registered-frame-contract-" + v);
				Check(!Main.tileSolid[type] && !Main.tileSolidTop[type] && !Main.tileBlockLight[type] && TileID.Sets.TouchDamageImmediate[type] == 0, "art-only-nonsolid-no-hurt-" + v);
				Main.instance.LoadTiles(type);
				var texture = TextureAssets.Tile[type].Value;
				Check(texture.Width == widths[v] * 18 && texture.Height == heights[v] * 18, "loaded-atlas-size-" + v);
				Color[] colors = new Color[texture.Width * texture.Height]; texture.GetData(colors);
				int opaque = 0; bool hard = true;
				foreach (Color c in colors) { if (c.A == 255) opaque++; else if (c.A != 0 || c.R != 0 || c.G != 0 || c.B != 0) hard = false; }
				Check(hard && opaque == pixels[v], "loaded-native-alpha-count-" + v);
			}
			for (int n = 0; n < Locations.Length; n++) {
				int v = Variants[n], x = bounds.X + Locations[n], y = bounds.Y + Floor(Locations[n]) - heights[v];
				for (int i = 0; i < widths[v]; i++) for (int j = 0; j < heights[v]; j++) {
					Tile t = Main.tile[x + i, y + j];
					Check(t.HasTile && t.TileType == types[v] && t.TileFrameX == i * 18 && t.TileFrameY == j * 18, $"placed-frame-{n}-{i}-{j}");
				}
				for (int i = 0; i < widths[v]; i++) {
					Tile support = Main.tile[x + i, y + heights[v]];
					Check(support.HasTile && Main.tileSolid[support.TileType] && !support.IsActuated && !support.IsHalfBlock && support.Slope == SlopeType.Solid, $"root-support-{n}-{i}");
				}
			}
			Mod.Logger.Info($"MAW TOOTH ART MATRIX PASS: {checks} native art checks. No final solid/hurt/mining/multiplayer certification.");
		}
		private void View(bool night)
		{
			RequireFixture();
			if (!viewing) { previousPosition = Main.LocalPlayer.position; previousDay = Main.dayTime; previousTime = Main.time; previousRain = Main.raining; previousEclipse = Main.eclipse; viewing = true; }
			Main.dayTime = !night; Main.time = night ? 16000 : 27000; Main.raining = false; Main.eclipse = false;
			Main.LocalPlayer.Teleport(new Vector2((bounds.X + 28) * 16, (bounds.Y + 20) * 16 - Main.LocalPlayer.height), 1);
			Main.LocalPlayer.velocity = Vector2.Zero;
			Main.NewText("Tooth ART gallery: short / long / wide on left; grouped family right; vanilla spikes far right. Specimens do not hurt or block.", Color.Wheat);
		}
		public override void PostUpdateEverything()
		{
			if (!IsQa || captureDelay < 0 || captureDelay-- != 0) return;
			captureDelay = -1;
			CaptureManager.Instance.Capture(new CaptureSettings { Area = bounds,
				Biome = new CaptureBiome(0, 0, Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),
				CaptureBackground = true, CaptureEntities = true, UseScaling = true,
				OutputName = "Apogean Maw Tooth Art " + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") });
		}
		internal void Release()
		{
			captureDelay = -1; if (!viewing) return;
			Main.dayTime = previousDay; Main.time = previousTime; Main.raining = previousRain; Main.eclipse = previousEclipse;
			if (IsQa) Main.LocalPlayer.Teleport(previousPosition, 1);
			viewing = false;
		}
		public override void SaveWorldData(TagCompound tag) { if (IsQa && !bounds.IsEmpty) tag["mawToothArtV1"] = new TagCompound { ["x"] = bounds.X, ["y"] = bounds.Y }; }
		public override void LoadWorldData(TagCompound tag)
		{
			if (Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3" && tag.ContainsKey("mawToothArtV1")) {
				TagCompound saved = tag.GetCompound("mawToothArtV1"); bounds = new Rectangle(saved.GetInt("x"), saved.GetInt("y"), 64, 28);
			}
		}
		public override void ClearWorld() { bounds = Rectangle.Empty; viewing = false; captureDelay = -1; }
	}
}
