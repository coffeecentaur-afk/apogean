using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.WorldGeneration;
using apogean.Content.Backgrounds;
using apogean.Content.Tiles;
using apogean.Content.Walls;

namespace apogean.Content.Diagnostics
{
	// Observe saved terrain only. No fixture construction, framing, lighting injection or camera override.
	public sealed class MawEntranceLab : ModPlayer
	{
		private int poll;
		public override void PostUpdate()
		{
			if (Main.netMode != NetmodeID.SinglePlayer || Player.whoAmI != Main.myPlayer || Player.name != "gg" ||
				Main.ActiveWorldFileData?.Name?.StartsWith("Apogee Arrival QA ", StringComparison.Ordinal) != true || poll-- > 0) return;
			poll = 30;
			string directory = Path.Combine(Main.SavePath, "Captures");
			string path = Path.Combine(directory, "ApogeanMawEntrance.request");
			if (!File.Exists(path)) return;
			string request = File.ReadAllText(path).Trim();
			File.Delete(path);
			if (request is not ("survey" or "surface" or "shallow")) { Mod.Logger.Warn("MAW ENTRANCE: refused unknown request"); return; }
			MawRupturePlan rupture = ApogeanWorldPlanSystem.Instance.Plan?.GetMajorRupture();
			if (rupture == null || !rupture.HasNavigationSpine) { Mod.Logger.Warn("MAW ENTRANCE: no saved major route"); return; }
			try {
				string evidence = Export(directory, rupture);
				if (request != "survey") {
					int targetY = rupture.SurfaceCenter.Y + (request == "surface" ? -4 : 56);
					Point standing = FindStanding(rupture.SurfaceCenter.X, targetY);
					// Remove the deliberately forced Forest bank from the previous pod view.
					RuinedBackgroundSelectionSystem.Instance.ToggleForestConceptRenderLab(false);
					Main.dayTime = true; Main.time = 27000; Main.raining = false; Main.eclipse = false;
					Player.Teleport(new Vector2(standing.X * 16, standing.Y * 16 - Player.height), 1);
					Player.velocity = Vector2.Zero;
					Mod.Logger.Info($"MAW ENTRANCE VIEW: case={request}; feet={standing}; native-routing=True; no-terrain-edits=True");
				}
				Mod.Logger.Info($"MAW ENTRANCE: evidence={Path.GetFileName(evidence)}; saved-mouth={rupture.SurfaceCenter}; no-terrain-edits=True");
			} catch (Exception error) { Mod.Logger.Error("MAW ENTRANCE QA FAILED: " + error.Message); }
		}

		private static Point FindStanding(int centerX, int targetY)
		{
			Point best = default; int score = int.MaxValue;
			// Bounded native floor search; no flight/freeze or terrain pad to improve the screenshot.
			for (int x = centerX - 64; x <= centerX + 64; x++)
			for (int y = targetY - 22; y <= targetY + 22; y++) {
				if (!WorldGen.InWorld(x, y, 12)) continue;
				if (!WorldGen.SolidTile(x, y) || !WorldGen.SolidTile(x + 1, y)) continue;
				bool clear = true;
				for (int dx = 0; dx < 2; dx++) for (int dy = 1; dy <= 4; dy++) {
					Tile t = Main.tile[x + dx, y - dy];
					if (t.LiquidAmount > 0 || (t.HasTile && !t.IsActuated && Main.tileSolid[t.TileType])) clear = false;
				}
				int value = Math.Abs(y - targetY) * 4 + Math.Abs(x - centerX);
				if (clear && value < score) { score = value; best = new(x, y); }
			}
			if (score == int.MaxValue) throw new InvalidOperationException("No dry native standing point; nothing was carved.");
			return best;
		}

		private static string Export(string directory, MawRupturePlan rupture)
		{
			Rectangle bounds = new(rupture.SurfaceCenter.X - 120, rupture.SurfaceCenter.Y - 40, 241, 181);
			if (!WorldGen.InWorld(bounds.Left, bounds.Top, 12) || !WorldGen.InWorld(bounds.Right, bounds.Bottom, 12))
				throw new InvalidOperationException("Survey would exceed world margin.");
			Dictionary<string, int> tiles = new(), walls = new();
			string[] collision = new string[bounds.Height], bone = new string[bounds.Height];
			int boneType = ModContent.TileType<OssuaryBone>(), membrane = ModContent.WallType<MawWallUnsafe>();
			int membraneCells = 0, liquids = 0;
			for (int row = 0; row < bounds.Height; row++) {
				char[] occupied = new char[bounds.Width], skeletal = new char[bounds.Width];
				for (int col = 0; col < bounds.Width; col++) {
					Tile t = Main.tile[bounds.X + col, bounds.Y + row];
					occupied[col] = t.HasTile && !t.IsActuated && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType] ? '#' : '.';
					skeletal[col] = t.HasTile && t.TileType == boneType ? 'B' : '.';
					if (t.HasTile) Count(tiles, TileLoader.GetTile(t.TileType)?.FullName ?? "Terraria/" + t.TileType);
					if (t.WallType != WallID.None) Count(walls, WallLoader.GetWall(t.WallType)?.FullName ?? "Terraria/" + t.WallType);
					if (t.WallType == membrane) membraneCells++;
					if (t.LiquidAmount > 0) liquids++;
				}
				collision[row] = new(occupied); bone[row] = new(skeletal);
			}
			var route = MawRuptureValidation.Inspect(rupture);
			var report = new {
				schema = 1, world = Main.ActiveWorldFileData.Name, planHash = ApogeanWorldPlanSystem.Instance.Plan.StableHash().ToString("X8"),
				left = bounds.X, top = bounds.Y, width = bounds.Width, height = bounds.Height,
				mouthX = rupture.SurfaceCenter.X, mouthY = rupture.SurfaceCenter.Y,
				tileCounts = tiles, wallCounts = walls, structuralWallCells = membraneCells, liquidCells = liquids,
				collisionRows = collision, boneRows = bone, routeReport = route.ToString(), routePassed = route.Passed,
				scope = "Existing generated cells. Conservative full-cell collision mask, not real movement or art acceptance. No terrain writer."
			};
			string path = Path.Combine(directory, "MawEntrance-" + Guid.NewGuid().ToString("N") + ".json");
			using FileStream output = new(path, FileMode.CreateNew, FileAccess.Write);
			JsonSerializer.Serialize(output, report, new JsonSerializerOptions { WriteIndented = true });
			return path;
		}
		private static void Count(Dictionary<string, int> counts, string key) => counts[key] = counts.TryGetValue(key, out int n) ? n + 1 : 1;
	}
}
