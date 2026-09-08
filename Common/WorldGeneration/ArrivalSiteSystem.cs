using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;
using apogean.Content.Tiles;
using apogean.Content.Walls;

namespace apogean.Common.WorldGeneration
{
	// Candidate-only until the fresh-world matrix passes. Save/load never stamps terrain.
	public sealed class ArrivalSiteSystem : ModSystem
	{
		private ArrivalSite planned;
		private string outcome = "legacy-none", reason = "no-generation-record";
		private Rectangle placedBounds;
		private Point podTopLeft;
		public static ArrivalSiteSystem Instance => ModContent.GetInstance<ArrivalSiteSystem>();
		internal string Outcome => outcome;
		internal Rectangle Bounds => placedBounds;
		internal Point PodTopLeft => podTopLeft;
		internal static Rectangle Rect(ArrivalBounds b) => new(b.X, b.Y, b.Width, b.Height);

		internal void Survey(GenerationProgress progress, GameConfiguration config)
		{
			ApogeanWorldPlan atlas = ApogeanWorldPlanSystem.Instance.Plan;
			Prepare(atlas);
			// The in-memory atlas already protects spawn from our own features. Register its
			// StructureMap claim only after surveying the final Wastes terrain, avoiding self-collision.
			GenVars.structures.AddProtectedStructure(atlas.SpawnSanctuary, 8);
		}

		private void Prepare(ApogeanWorldPlan atlas)
		{
			planned = null; placedBounds = Rectangle.Empty; podTopLeft = Point.Zero;
			outcome = "disabled"; reason = "candidate-not-installed";
			if (!Mod.TryFind<ModTile>(nameof(ArrivalPodTile), out _)) return;
			bool[] allowed = new bool[TileLoader.TileCount];
			foreach (int type in SoilTypes()) allowed[type] = true;
			allowed[ModContent.TileType<DeadTuft>()] = allowed[ModContent.TileType<WastesBristle>()] = allowed[ModContent.TileType<WastesRootShrub>()] = true;
			allowed[TileID.Plants] = allowed[TileID.Plants2] = true;
			string obstacle = "none";
			ArrivalCell ReadForSurvey(int x, int y) {
				ArrivalCell cell = ReadCell(x, y);
				if (cell == ArrivalCell.Blocked) {
					Tile t = Main.tile[x, y];
					obstacle = $"{x},{y}:tile={(t.HasTile ? t.TileType : -1)}:wall={t.WallType}:liquid={t.LiquidAmount}";
				}
				return cell;
			}
			// Called AFTER Wastes, BEFORE this atlas registers its sanctuary. Foreign claims remain intact.
			planned = ArrivalSitePlanner.Find(Main.spawnTileX, Main.spawnTileY, atlas.PlanSeed,
				Main.maxTilesX, Main.maxTilesY, ReadForSurvey,
				b => atlas.SpawnSanctuary.Contains(Rect(b)) && GenVars.structures.CanPlace(Rect(b), allowed, 0),
				detail => { Mod.Logger.Info("ARRIVAL SITE SURVEY: " + detail + "; last-obstacle=" + obstacle); obstacle = "none"; }, CoversFit);
			if (planned == null) { outcome = "skipped"; reason = "no-safe-bounded-slot"; Report(); return; }
			outcome = "planned"; reason = planned.Settled ? "undug-fallback" : "shallow-impact";
			placedBounds = Rect(planned.Envelope); podTopLeft = new(planned.PodLeft, planned.PodTop);
			GenVars.structures.AddProtectedStructure(placedBounds, 0);
			Report();
		}

		internal void Generate(GenerationProgress progress, GameConfiguration config)
		{
			if (planned == null || outcome != "planned") return;
			progress.Message = "Remembering your arrival...";
			int spawnX = Main.spawnTileX, spawnY = Main.spawnTileY;
			List<CellSnapshot> landing = new();
			for (int x = spawnX - 3; x <= spawnX + 3; x++)
			for (int y = spawnY - 5; y <= spawnY + 4; y++) landing.Add(new CellSnapshot(x, y));
			if (TryStamp(planned, out string failure)) {
				outcome = planned.Settled ? "fallback" : "placed";
				reason = "native-object-and-support-verified";
			} else { outcome = "skipped"; reason = failure; }
			if (spawnX != Main.spawnTileX || spawnY != Main.spawnTileY)
				throw new InvalidOperationException("Arrival site changed world spawn.");
			foreach (CellSnapshot cell in landing)
				if (!cell.MatchesPersistent()) throw new InvalidOperationException("Arrival site changed the spawn landing envelope.");
			Report();
		}

		internal static int[] SoilTypes() => new[] { (int)TileID.Dirt, TileID.Grass,
			ModContent.TileType<WastesSoil>(), ModContent.TileType<WastesGrass>(), ModContent.TileType<DeadGrass>() };

		internal static ArrivalCell ReadCell(int x, int y)
		{
			if (!WorldGen.InWorld(x, y, 10)) return ArrivalCell.Blocked;
			Tile t = Main.tile[x, y];
			if (t.LiquidAmount != 0 || t.HasActuator || t.IsActuated || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire ||
				t.TileColor != PaintID.None || t.WallColor != PaintID.None || t.IsTileInvisible || t.IsWallInvisible || t.IsTileFullbright || t.IsWallFullbright)
				return ArrivalCell.Blocked;
			if (t.WallType != WallID.None && t.WallType != WallID.DirtUnsafe && t.WallType != WallID.GrassUnsafe &&
				t.WallType != WallID.FlowerUnsafe && t.WallType != ModContent.WallType<WastesGrassWallUnsafe>())
				return ArrivalCell.Blocked;
			if (!t.HasTile) return ArrivalCell.Empty;
			if (t.TileType == TileID.Dirt || t.TileType == TileID.Grass || t.TileType == ModContent.TileType<WastesSoil>() ||
				t.TileType == ModContent.TileType<WastesGrass>() || t.TileType == ModContent.TileType<DeadGrass>()) return ArrivalCell.Soil;
			if (t.TileType is TileID.Plants or TileID.Plants2) return ArrivalCell.Cover;
			if (IsReplaceableCover(t.TileType)) return ArrivalCell.Cover;
			return ArrivalCell.Blocked; // Trees, ore, furniture, unknown modded tiles and structures stay untouched.
		}

		private static bool IsReplaceableCover(int type) => type == ModContent.TileType<DeadTuft>() ||
			type == ModContent.TileType<WastesBristle>() || type == ModContent.TileType<WastesRootShrub>();
		private static bool CoversFit(ArrivalSite site)
		{
			Rectangle b = Rect(site.Envelope);
			for (int x = b.Left; x < b.Right; x++)
			for (int y = b.Top; y < b.Bottom; y++) {
				Tile t = Main.tile[x, y];
				if (!t.HasTile || !IsReplaceableCover(t.TileType)) continue;
				TileObjectData data = TileObjectData.GetTileData(t);
				if (data == null || data.CoordinateWidth != 16 || data.CoordinatePadding != 2) return false;
				int left = x - t.TileFrameX % data.CoordinateFullWidth / 18;
				int top = y - t.TileFrameY % data.CoordinateFullHeight / 18;
				if (!ArrivalSitePlanner.CoverFits(site, new(left, top, data.Width, data.Height))) return false;
				// No partially framed cover may be used as an excuse to erase nearby unrelated cells.
				for (int dx = 0; dx < data.Width; dx++)
				for (int dy = 0; dy < data.Height; dy++) {
					Tile part = Main.tile[left + dx, top + dy];
					if (!part.HasTile || part.TileType != t.TileType ||
						part.TileFrameX != t.TileFrameX - (x - left) * 18 + dx * 18 ||
						part.TileFrameY != t.TileFrameY - (y - top) * 18 + dy * 18) return false;
				}
			}
			return true;
		}

		internal static bool TryStamp(ArrivalSite site, out string failure)
		{
			if (!ArrivalSitePlanner.Recheck(site, ReadCell, out failure)) return false;
			if (!CoversFit(site)) { failure = "partial-ground-cover"; return false; }
			Rectangle b = Rect(site.Envelope);
			// Copy VALUES, not Tile references: Terraria.Tile is a handle into the live tile map.
			List<CellSnapshot> snapshot = new();
			for (int x = b.Left; x < b.Right; x++)
			for (int y = b.Top; y < b.Bottom; y++) snapshot.Add(new CellSnapshot(x, y));
			try {
				for (int col = 0; col < site.Width; col++) {
					int x = site.Left + col;
					for (int y = b.Top + 2; y < site.Floor[col]; y++) {
						Tile t = Main.tile[x, y];
						if (!t.HasTile) continue;
						t.ClearTile(); // No native kill cascade or item drops during generation.
						t.WallType = WallID.None;
					}
					Tile floor = Main.tile[x, site.Floor[col]];
					floor.Slope = SlopeType.Solid; floor.IsHalfBlock = false;
					if (!site.Settled && site.Floor[col] > site.Floor[0])
						floor.TileType = (ushort)(floor.TileType == TileID.Dirt || floor.TileType == TileID.Grass
							? TileID.Dirt : ModContent.TileType<WastesSoil>());
				}
				WorldGen.RangeFrame(b.Left, b.Top, b.Right, b.Bottom);
				int podType = ModContent.TileType<ArrivalPodTile>();
				if (!WorldGen.PlaceObject(site.CenterX, site.PodTop + 5, podType, mute: true) || !ObjectIntact(new(site.PodLeft, site.PodTop)))
					throw new InvalidOperationException("native-placement-or-support-failed");
				foreach (CellSnapshot cell in snapshot)
					if (!cell.InMutation(site) && !cell.MatchesPersistent())
						throw new InvalidOperationException("edit-outside-declared-mask");
				failure = "none";
				return true;
			} catch (Exception error) {
				foreach (CellSnapshot cell in snapshot) cell.Restore();
				failure = "rolled-back: " + error.Message;
				return false;
			}
		}

		internal static bool ObjectIntact(Point p)
		{
			int pod = ModContent.TileType<ArrivalPodTile>();
			for (int x = 0; x < 5; x++) {
				Tile support = Main.tile[p.X + x, p.Y + 6];
				if (!support.HasTile || !Main.tileSolid[support.TileType] || support.IsHalfBlock || support.Slope != SlopeType.Solid || support.IsActuated) return false;
				for (int y = 0; y < 6; y++) {
					Tile t = Main.tile[p.X + x, p.Y + y];
					if (!t.HasTile || t.TileType != pod || t.TileFrameX != x * 18 || t.TileFrameY != y * 18) return false;
				}
			}
			return true;
		}

		private void Report() => Mod.Logger.Info($"ARRIVAL SITE: version=1; outcome={outcome}; reason={reason}; bounds={placedBounds}; pod={podTopLeft}; spawn={Main.spawnTileX},{Main.spawnTileY}; spawn-unchanged=True");
		public override void PostWorldGen()
		{
			if (planned == null || outcome is not ("placed" or "fallback")) return;
			bool route = true, dry = true;
			for (int col = 0; col < planned.Width; col++) {
				int x = planned.Left + col, floor = planned.Floor[col];
				for (int y = floor - 3; y < floor; y++) {
					Tile t = Main.tile[x, y];
					if (t.HasTile && Main.tileSolid[t.TileType]) route = false;
					if (t.LiquidAmount != 0) dry = false;
				}
				if (col > 0 && Math.Abs(floor - planned.Floor[col - 1]) > 1) route = false;
			}
			bool intact = ObjectIntact(podTopLeft);
			Mod.Logger.Info($"ARRIVAL SITE POSTGEN: outcome={outcome}; object30={intact}; route={route}; dry={dry}; version=1");
			if (!intact || !route || !dry) Mod.Logger.Error("ARRIVAL SITE QA FAILED: final terrain/object changed after stamp.");
		}
		public override void ClearWorld() { planned = null; outcome = "legacy-none"; reason = "no-generation-record"; placedBounds = Rectangle.Empty; podTopLeft = Point.Zero; }
		public override void SaveWorldData(TagCompound tag)
		{
			if (outcome == "legacy-none") return;
			tag["arrivalSiteV1"] = new TagCompound { ["outcome"] = outcome, ["reason"] = reason,
				["x"] = placedBounds.X, ["y"] = placedBounds.Y, ["w"] = placedBounds.Width, ["h"] = placedBounds.Height,
				["podX"] = podTopLeft.X, ["podY"] = podTopLeft.Y };
		}
		public override void LoadWorldData(TagCompound tag)
		{
			if (!tag.ContainsKey("arrivalSiteV1")) return;
			TagCompound data = tag.GetCompound("arrivalSiteV1");
			outcome = data.GetString("outcome"); reason = data.GetString("reason");
			placedBounds = new(data.GetInt("x"), data.GetInt("y"), data.GetInt("w"), data.GetInt("h"));
			podTopLeft = new(data.GetInt("podX"), data.GetInt("podY"));
			planned = null; // Record is history, never a regeneration command after pickup/reload.
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write(outcome); writer.Write(reason); writer.Write(placedBounds.X); writer.Write(placedBounds.Y);
			writer.Write(placedBounds.Width); writer.Write(placedBounds.Height); writer.Write(podTopLeft.X); writer.Write(podTopLeft.Y);
		}
		public override void NetReceive(BinaryReader reader) {
			outcome = reader.ReadString(); reason = reader.ReadString(); placedBounds = new(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			podTopLeft = new(reader.ReadInt32(), reader.ReadInt32()); planned = null;
		}

		// The preflight rejects all wires, liquids, coatings and non-natural content. Preserve every
		// field changed by this writer/framer, including old natural slopes and recomputed frames.
		private readonly struct CellSnapshot
		{
			private readonly int x, y, wallX, wallY, tileFrame, wallFrame;
			private readonly ushort type, wall;
			private readonly short frameX, frameY;
			private readonly bool has, half;
			private readonly SlopeType slope;
			public CellSnapshot(int x, int y) {
				this.x = x; this.y = y; Tile t = Main.tile[x, y];
				type = t.TileType; wall = t.WallType; has = t.HasTile; half = t.IsHalfBlock; slope = t.Slope;
				frameX = t.TileFrameX; frameY = t.TileFrameY; wallX = t.WallFrameX; wallY = t.WallFrameY;
				tileFrame = t.TileFrameNumber; wallFrame = t.WallFrameNumber;
			}
			public void Restore() {
				Tile t = Main.tile[x, y]; t.TileType = type; t.WallType = wall; t.HasTile = has; t.IsHalfBlock = half; t.Slope = slope;
				t.TileFrameX = frameX; t.TileFrameY = frameY; t.WallFrameX = wallX; t.WallFrameY = wallY; t.TileFrameNumber = tileFrame; t.WallFrameNumber = wallFrame;
			}
			public bool MatchesPersistent() {
				Tile t = Main.tile[x, y];
				return t.HasTile == has && (!has || t.TileType == type) && t.WallType == wall && t.IsHalfBlock == half && t.Slope == slope;
			}
			public bool InMutation(ArrivalSite site) => x >= site.Left && x < site.Left + site.Width &&
				y >= site.Envelope.Y + 2 && y <= site.Floor[x - site.Left];
		}
	}
}
