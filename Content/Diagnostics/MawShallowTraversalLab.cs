using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using apogean.Content.Tiles;
using P = apogean.Content.Diagnostics.MawShallowTraversalPlan;

namespace apogean.Content.Diagnostics
{
    // Explicit disposable prototype, not a world-generation pass. All scans are command-time.
    public sealed class MawShallowTraversalLab : ModSystem
    {
        private const string SaveKey = "mawShallowTraversalV1";
        private Rectangle bounds;
        private string creation, savedState;
        private bool failed, viewing, visiting, oldDay, oldRain, oldEclipse;
        private double oldTime;
        private Vector2 oldPosition;
        private int panel, captureDelay = -1;
        private bool? previewDormant;
        private bool previewBright, inspectionLamp;
        internal static bool IsQa => MawShallowQaScope.Context(Main.ActiveWorldFileData?.Name,
            Main.LocalPlayer.name, Main.netMode == NetmodeID.SinglePlayer, Main.gameMenu);
        internal bool PlainVisit => IsQa && MawPackedPreview.Enabled && visiting && Main.LocalPlayer.name == MawShallowQaScope.Plain;
        internal bool? StateAt(int x, int y) => MawShallowQaScope.PreviewApplies(IsQa, viewing, visiting,
            x,y,bounds.X,bounds.Y,bounds.Width,bounds.Height) ? previewDormant : null;
        internal float LightScaleAt(int x,int y) => MawShallowQaScope.LightScale(
            MawShallowQaScope.PreviewApplies(IsQa && MawPackedPreview.Enabled,viewing,visiting,
                x,y,bounds.X,bounds.Y,bounds.Width,bounds.Height),previewBright);
        private bool InspectionActive => MawShallowQaScope.InspectionApplies(PlainVisit,viewing,visiting,
            MawShallowMotionProbe.PlainBaseline(Main.LocalPlayer),inspectionLamp);
        private static MawToothClusterTile Teeth => ModContent.GetInstance<MawToothClusterTile>();
        internal Rectangle MotionBounds => bounds;
        internal Rectangle PreservedBounds => bounds;
        internal bool PerformanceReady => IsQa && viewing && visiting && !failed && creation != null && captureDelay < 0;
        private static int TileType(string key) => key switch {
            "rib" or "cap" => MawAnatomyMaterials.Tile(key).Type,
            "amber" => ModContent.TileType<MawAmberLitTile>(), _ => MawPackedPreview.TileType(key)
        };
        private static int WallType(string key) => key == null ? WallID.None :
            key == "amber" ? ModContent.WallType<MawAmberLitWall>() : MawPackedPreview.Wall(key).Type;
        private Point At(int x, int y) => new(bounds.X + x, bounds.Y + y);
        private Rectangle Envelope { get { Rectangle r = bounds; r.Inflate(12, 12); return r; } }
        private static Rectangle Pixels(Rectangle r) => new(r.X * 16, r.Y * 16, r.Width * 16, r.Height * 16);
        internal static bool Empty(Tile t) => !t.HasTile && t.WallType == WallID.None && t.LiquidAmount == 0 &&
            !t.HasActuator && !t.IsActuated && !t.RedWire && !t.BlueWire && !t.GreenWire && !t.YellowWire &&
            t.TileColor == PaintID.None && t.WallColor == PaintID.None && !t.IsTileInvisible && !t.IsWallInvisible &&
            !t.IsTileFullbright && !t.IsWallFullbright;

        internal static Rectangle[] Historical() => new[] {
            ModContent.GetInstance<VegetationVisualLab>().PreservedBounds,
            ModContent.GetInstance<ForestSprayVisualLab>().PreservedBounds,
            ModContent.GetInstance<ArrivalPodLab>().PreservedBounds,
            ModContent.GetInstance<MawBoneLab>().PreservedBounds,
            ModContent.GetInstance<MawFangLab>().PreservedBounds,
            ModContent.GetInstance<MawToothArtLab>().PreservedBounds,
            ModContent.GetInstance<MawToothClusterLab>().PreservedBounds,
            ModContent.GetInstance<MawClusterOrientationLab>().PreservedBounds,
            ModContent.GetInstance<MawMaterialLab>().PreservedBounds,
            ModContent.GetInstance<MawMaterialFamilyLab>().PreservedBounds,
            ModContent.GetInstance<MawNaturalLab>().PreservedBounds,
            ModContent.GetInstance<MawPlayableLab>().PreservedBounds,
            ModContent.GetInstance<MawFiberRibLab>().PreservedBounds,
            ModContent.GetInstance<MawAnatomyLab>().PreservedBounds,
            ModContent.GetInstance<MawHangingFiberStudy>().Bounds,
            ModContent.GetInstance<MawAmberLightStudy>().Bounds,
            ModContent.GetInstance<MawRibContourStudy>().PreservedBounds
        };
        internal static string Fingerprint(IEnumerable<Rectangle> areas)
        {
            using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
            foreach (Rectangle r in areas) {
                w.Write(r.X); w.Write(r.Y); w.Write(r.Width); w.Write(r.Height);
                for (int x = r.Left; x < r.Right; x++) for (int y = r.Top; y < r.Bottom; y++) {
                    Tile t = Main.tile[x, y]; w.Write(t.HasTile);
                    if (t.HasTile) {
                        w.Write(TileLoader.GetTile(t.TileType)?.FullName ?? "Terraria/" + t.TileType);
                        w.Write((byte)t.Slope); w.Write(t.IsHalfBlock);
                        if (Main.tileFrameImportant[t.TileType]) { w.Write(t.TileFrameX); w.Write(t.TileFrameY); }
                    }
                    w.Write(WallLoader.GetWall(t.WallType)?.FullName ?? "Terraria/" + t.WallType);
                    w.Write(t.TileColor); w.Write(t.WallColor); w.Write(t.IsActuated); w.Write(t.HasActuator);
                    w.Write(t.IsTileInvisible); w.Write(t.IsWallInvisible); w.Write(t.IsTileFullbright); w.Write(t.IsWallFullbright);
                    w.Write(t.RedWire); w.Write(t.BlueWire); w.Write(t.GreenWire); w.Write(t.YellowWire);
                    w.Write(t.LiquidAmount); if (t.LiquidAmount != 0) w.Write(t.LiquidType);
                }
            }
            w.Flush(); return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
        }
        private static string PlanIdentity(P.Cell[,] cells, P.Cluster[] clusters)
        {
            using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
            w.Write(P.Version); w.Write(P.Width); w.Write(P.Height);
            foreach (var c in cells) { w.Write(c.Key ?? "air"); w.Write(c.Slope); w.Write(c.Wall ?? "air"); w.Write((int)c.Region); }
            foreach (var t in clusters) { w.Write(t.Root.X); w.Write(t.Root.Y); w.Write(t.Variant); }
            w.Flush(); return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
        }
        internal static bool ActorsAbsent(Rectangle envelope)
        {
            Rectangle area = Pixels(envelope); area.Inflate(96, 96);
            foreach (Player p in Main.ActivePlayers) if (area.Intersects(p.Hitbox)) return false;
            foreach (NPC n in Main.ActiveNPCs) if (area.Intersects(n.Hitbox)) return false;
            foreach (Projectile p in Main.ActiveProjectiles) if (area.Intersects(p.Hitbox)) return false;
            foreach (Item i in Main.ActiveItems) if (area.Intersects(i.Hitbox)) return false;
            return true;
        }
        // Terraria Tile is a handle into shared storage, not a value snapshot.
        internal readonly record struct CellState(TileTypeData Type, WallTypeData Wall,
            TileWallWireStateData State, LiquidData Liquid, TileWallBrightnessInvisibilityData Coating)
        {
            internal static CellState Read(int x, int y) {
                Tile t = Main.tile[x, y];
                return new(t.Get<TileTypeData>(), t.Get<WallTypeData>(), t.Get<TileWallWireStateData>(),
                    t.Get<LiquidData>(), t.Get<TileWallBrightnessInvisibilityData>());
            }
            internal void Restore(int x, int y) {
                Tile t = Main.tile[x, y]; t.Get<TileTypeData>() = Type; t.Get<WallTypeData>() = Wall;
                t.Get<TileWallWireStateData>() = State; t.Get<LiquidData>() = Liquid;
                t.Get<TileWallBrightnessInvisibilityData>() = Coating;
            }
        }
        private void Build()
        {
            if (Main.LocalPlayer.name != "gg") throw new InvalidOperationException("Plain QA cannot construct scenes.");
            if (!bounds.IsEmpty || creation != null) throw new InvalidOperationException("Shallow scene already reserved; no rebuild or rebaseline.");
            var plan = P.Create(); var cells = plan.ExportCells(); var clusters = plan.ExportClusters();
            plan.ValidateExport(cells, clusters);
            if (Teeth.VariantCount != 8) throw new InvalidOperationException("Eight-variant cluster bank required.");
            foreach (var c in cells) { if (c.Key != null) _ = TileType(c.Key); _ = WallType(c.Wall); }
            var history = Historical(); int attempts = 0;
            foreach (int dx in new[] { -3080, -2800, -2520, -1960 }) {
                foreach (int dy in new[] { -420, -300, -540 }) {
                    attempts++;
                    Rectangle site = new(Main.spawnTileX + dx, Main.spawnTileY + dy, P.Width, P.Height), guard = site;
                    guard.Inflate(12, 12);
                    if (guard.Left < 40 || guard.Top < 40 || guard.Right > Main.maxTilesX - 40 || guard.Bottom > Main.maxTilesY - 40) continue;
                    bool reject = false;
                    foreach (Rectangle old in history) { if (old.IsEmpty) continue; Rectangle reserved = old; reserved.Inflate(24, 24); reject |= reserved.Intersects(guard); }
                    if (reject || !ActorsAbsent(guard)) continue;
                    if (GenVars.structures != null && !GenVars.structures.CanPlace(guard)) continue;
                    for (int x = guard.Left; x < guard.Right; x++) for (int y = guard.Top; y < guard.Bottom; y++) reject |= !Empty(Main.tile[x, y]);
                    if (reject) continue;
                    bounds = site; break;
                }
                if (!bounds.IsEmpty) break;
            }
            if (bounds.IsEmpty) throw new InvalidOperationException($"No empty/protected shallow site in {attempts} attempts; nothing cleared.");
            creation = PlanIdentity(cells, clusters); // Reservation survives failure; never rebuild a saved specimen.
            GenVars.structures?.AddProtectedStructure(Envelope);
            Rectangle envelope = Envelope;
            CellState[,] original = new CellState[envelope.Width, envelope.Height];
            for (int x = 0; x < envelope.Width; x++) for (int y = 0; y < envelope.Height; y++) original[x, y] = CellState.Read(envelope.X + x, envelope.Y + y);
            try {
                // Actual native value-copy round trip in owned empty air before bulk writes.
                int px = envelope.Left, py = envelope.Top; CellState check = CellState.Read(px, py);
                try { Tile probe = Main.tile[px, py]; probe.HasTile = true; probe.TileType = TileID.Stone; probe.BlueWire = true; probe.IsTileInvisible = true; }
                finally { check.Restore(px, py); }
                if (CellState.Read(px, py) != check || !Empty(Main.tile[px, py])) throw new InvalidOperationException("Native shallow snapshot restoration failed.");
                Mod.Logger.Info("MAW SHALLOW SNAPSHOT PASS: native value storage survived a temporary tile/wire/coating mutation; restored before placement.");
                for (int x = 0; x < P.Width; x++) for (int y = 0; y < P.Height; y++) {
                    var c = cells[x, y]; Tile t = Main.tile[bounds.X + x, bounds.Y + y];
                    if (c.Key != null) { t.HasTile = true; t.TileType = (ushort)TileType(c.Key); t.Slope = (SlopeType)c.Slope; }
                    if (c.Wall != null) t.WallType = (ushort)WallType(c.Wall);
                }
                WorldGen.RangeFrame(bounds.Left - 1, bounds.Top - 1, bounds.Right + 1, bounds.Bottom + 1);
                foreach (var cluster in clusters) {
                    Point root = At(cluster.Root.X, cluster.Root.Y); var origin = cluster.OriginOffset;
                    if (!WorldGen.PlaceObject(root.X + origin.X, root.Y + origin.Y, Teeth.Type, mute: true, style: cluster.Style))
                        throw new InvalidOperationException("Native cluster placement failed: " + cluster.Variant);
                }
                ValidatePristine();
                Mod.Logger.Info($"MAW SHALLOW BUILD: bounds={bounds}; envelope={envelope}; attempts={attempts}; original={creation}; existing textures only; no worldgen.");
            } catch {
                failed = true;
                for (int x = 0; x < envelope.Width; x++) for (int y = 0; y < envelope.Height; y++) original[x, y].Restore(envelope.X + x, envelope.Y + y);
                Mod.Logger.Error("MAW SHALLOW BUILD ROLLBACK: restored only the newly reserved empty envelope; failure reservation retained.");
                throw;
            }
        }
        private P Require()
        {
            if (failed || bounds.Width != P.Width || bounds.Height != P.Height || !WorldGen.InWorld(bounds.Left, bounds.Top, 30) || !WorldGen.InWorld(bounds.Right, bounds.Bottom, 30))
                throw new InvalidOperationException("Missing/failed shallow scene; no automatic rebuild.");
            var plan = P.Create();
            if (creation != PlanIdentity(plan.ExportCells(), plan.ExportClusters())) throw new InvalidOperationException("Shallow creation contract changed; no rebaseline.");
            return plan;
        }
        private void ValidatePristine()
        {
            var plan = Require(); var cells = plan.ExportCells(); var clusters = plan.ExportClusters();
            var teeth = new Dictionary<Point, (int fx, int fy)>();
            foreach (var c in clusters) for (int dx = 0; dx < 4; dx++) for (int dy = 0; dy < 4; dy++)
                teeth.Add(new(c.Root.X + dx, c.Root.Y + dy), (c.Variant * 72 + dx * 18, dy * 18));
            for (int x = 0; x < P.Width; x++) for (int y = 0; y < P.Height; y++) {
                var c = cells[x, y]; Tile t = Main.tile[bounds.X + x, bounds.Y + y];
                bool tooth = teeth.TryGetValue(new(x, y), out var frame);
                bool bad = t.HasTile != (tooth || c.Key != null) || t.WallType != WallType(c.Wall) || t.LiquidAmount != 0 ||
                    t.HasActuator || t.IsActuated || t.RedWire || t.BlueWire || t.GreenWire || t.YellowWire || t.TileColor != PaintID.None || t.WallColor != PaintID.None ||
                    t.IsTileInvisible || t.IsWallInvisible || t.IsTileFullbright || t.IsWallFullbright;
                if (t.HasTile) bad |= (c.Key == null && !tooth) || t.TileType != (tooth ? Teeth.Type : c.Key == null ? -1 : TileType(c.Key)) || t.IsHalfBlock || (byte)t.Slope != c.Slope ||
                    (tooth && (t.TileFrameX != frame.fx || t.TileFrameY != frame.fy));
                if (bad) throw new InvalidOperationException($"Shallow original layout differs at{x},{y}; preserve legitimate play separately, never repair it.");
            }
            Mod.Logger.Info($"MAW SHALLOW PRISTINE PASS: {P.Width * P.Height} exact cells; eight native clusters/all variants; immutable creation={creation}. Not art or traversal approval.");
        }
        private void BodyClearance()
        {
            ValidatePristine();
            if (Main.LocalPlayer.width != P.BodyWidth || Main.LocalPlayer.height != P.BodyHeight || Main.LocalPlayer.mount.Active)
                throw new InvalidOperationException("Unmounted20x42 body required for shallow clearance.");
            var cells = P.Create().ExportCells(); var seen = new bool[P.Width, P.Height]; var queue = new Queue<Point>(); int probes = 0;
            bool Fits(int x, int y) {
                if (x < 0 || y < 0 || x + 2 > P.Width || y + 3 > P.Height) return false;
                for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 3; dy++) if (cells[x + dx, y + dy].Region == P.Zone.Outside) return false;
                Point p = At(x, y); Vector2 pos = p.ToVector2() * 16 + new Vector2(6, 3); probes++;
                return !Collision.SolidCollision(pos, 20, 42) && !Teeth.Touching(new((int)pos.X, (int)pos.Y, 20, 42));
            }
            void Visit(int x, int y) { if (x < 0 || y < 0 || x >= P.Width || y >= P.Height || seen[x, y] || !Fits(x, y)) return; seen[x, y] = true; queue.Enqueue(new(x, y)); }
            Visit(P.Entry.X, P.Entry.Y);
            while (queue.Count != 0) { Point p = queue.Dequeue(); Visit(p.X - 1, p.Y); Visit(p.X + 1, p.Y); Visit(p.X, p.Y - 1); Visit(p.X, p.Y + 1); }
            if (!seen[P.DescentEnd.X, P.DescentEnd.Y] || !seen[P.PocketGoal.X, P.PocketGoal.Y]) throw new InvalidOperationException("Native standing-body spatial route blocked.");
            Mod.Logger.Info($"MAW SHALLOW BODY PASS: {probes} native20x42 collision/contact probes; entry connected to lower Gullet and pocket. Spatial adjacency only, NOT swept physics, jumping or fall survival.");
        }
        internal void Run(string request)
        {
            if (!IsQa || !MawAnatomyMaterials.Available || !MawPackedPreview.Enabled || ModContent.GetInstance<QAPerformanceLab>().Recording)
                throw new InvalidOperationException("Shallow commands require idle packed gg or Maw QA Plain/V3/SP.");
            if (!MawShallowQaScope.Request(Main.LocalPlayer.name, "maw-shallow-" + request))
                throw new InvalidOperationException("Shallow request denied for this QA character.");
            if (Main.LocalPlayer.GetModPlayer<MawShallowMotionProbe>().Active && request != "release")
                throw new InvalidOperationException("Finish or release the bounded motion probe before another shallow command.");
            Rectangle[] old = Historical(); string before = Fingerprint(old);
            Mod.Logger.Info("MAW SHALLOW REQUEST: " + request);
            try {
                switch (request) {
                    case "build": Build(); break;
                    case "test": BodyClearance(); break;
                    case "light": LightReport(); break;
                    case "inspect-on": case "inspect-off":
                        Require();
                        if (!viewing || !PlainVisit || !MawShallowMotionProbe.PlainBaseline(Main.LocalPlayer))
                            throw new InvalidOperationException("Inspection lamp requires held Plain QA view; no equipment is modified.");
                        inspectionLamp = request == "inspect-on"; previewDormant=null; previewBright=false;
                        Mod.Logger.Info($"MAW SHALLOW INSPECTION: lamp={inspectionLamp}; one neutral inspection light at held player; NOT natural illumination, torch-equipment or difficulty evidence; no world tiles changed.");
                        break;
                    case "light-awake": case "light-dormant": case "light-natural":
                    case "light-awake-bright": case "light-dormant-bright":
                        Require();
                        if (inspectionLamp) throw new InvalidOperationException("Turn inspection lamp off before natural amber comparisons.");
                        if (!viewing || Main.LocalPlayer.name != MawShallowQaScope.Plain || !MawShallowMotionProbe.PlainBaseline(Main.LocalPlayer))
                            throw new InvalidOperationException("Lighting preview requires held plain-character view with starter-only loadout/no buffs.");
                        previewDormant = request == "light-natural" ? null : request is "light-dormant" or "light-dormant-bright";
                        previewBright = request is "light-awake-bright" or "light-dormant-bright";
                        Mod.Logger.Info($"MAW SHALLOW LIGHT MODE: preview={previewDormant?.ToString() ?? "natural"}; scale={LightScaleAt(bounds.X,bounds.Y)}; only bounds={bounds}; globalDormant={apogean.Common.Maw.MawActivityState.IsDormant} unchanged. Wait for lighting to settle before measuring.");
                        break;
                    case "pristine": ValidatePristine(); break;
                    case "audit": Require(); Mod.Logger.Info($"MAW SHALLOW AUDIT: original={creation}; actual={Fingerprint(new[] { bounds })}; saved={savedState}; no repair."); break;
                    case "reload": Require(); if (savedState == null || savedState != Fingerprint(new[] { bounds })) throw new InvalidOperationException("Shallow pre-save/post-load state mismatch; retained."); Mod.Logger.Info("MAW SHALLOW RELOAD PASS: actual contents match saved interaction state; original contract retained separately."); break;
                    case "top": case "bottom": case "pocket": case "rib1": case "rib2": case "rib3":
                        Require(); panel = request switch { "top" => 0, "bottom" => 1, "pocket" => 2, "rib1" => 3, "rib2" => 4, _ => 5 }; StartVisit(true); break;
                    case "play": Require(); StartVisit(false); break;
                    case "motion-entry":
                        ValidatePristine(); StartVisit(false); Main.LocalPlayer.GetModPlayer<MawShallowMotionProbe>().Start(bounds); break;
                    case "motion-connector-out":
                        ValidatePristine(); StartVisit(false, true); Main.LocalPlayer.GetModPlayer<MawShallowMotionProbe>().Start(bounds, true); break;
                    case "capture": Require(); if (!viewing) throw new InvalidOperationException("Choose top/bottom held view before native capture."); captureDelay = 90; break;
                    case "release": Release(); break;
                    default: throw new InvalidOperationException("Unknown shallow request.");
                }
            } finally {
                if (before != Fingerprint(old)) throw new InvalidOperationException("Historical fixture state changed during shallow command; not repaired.");
                Mod.Logger.Info($"MAW SHALLOW PRESERVATION: all{old.Length} historical bounds semantically unchanged during command, including old failed fixtures.");
            }
            Mod.Logger.Info("MAW SHALLOW COMPLETE: " + request);
        }
        private void LightReport()
        {
            Require();
            if(!viewing) throw new InvalidOperationException("Choose a held view before comparing panel lighting; free movement does not apply a preview.");
            int tiles = 0, walls = 0, litTiles = 0, litWalls = 0; Vector3 strongest = Vector3.Zero;
            bool worldDormant = apogean.Common.Maw.MawActivityState.IsDormant;
            bool dormant = previewDormant ?? worldDormant;
            var tile = ModContent.GetInstance<MawAmberLitTile>(); var wall = ModContent.GetInstance<MawAmberLitWall>();
            for (int x = bounds.Left; x < bounds.Right; x++) for (int y = bounds.Top; y < bounds.Bottom; y++) {
                Tile t = Main.tile[x, y]; Vector3 a = Vector3.Zero, b = Vector3.Zero;
                if (t.HasTile && t.TileType == tile.Type) { tiles++; a = tile.Emission(x, y, dormant); if (a.X > 0) litTiles++; }
                if (t.WallType == wall.Type) { walls++; b = wall.Emission(x, y, dormant); if (b.X > 0) litWalls++; }
                strongest = Vector3.Max(strongest, Vector3.Max(a, b));
            }
            int actors=0;
            Rectangle view=Pixels(Panel);
            foreach(NPC n in Main.ActiveNPCs) if(view.Intersects(n.Hitbox))actors++;
            foreach(Projectile p in Main.ActiveProjectiles) if(view.Intersects(p.Hitbox))actors++;
            Mod.Logger.Info($"MAW SHALLOW LIGHT: worldDormant={worldDormant}; preview={previewDormant?.ToString() ?? "natural"}; scale={LightScaleAt(bounds.X,bounds.Y)}; inspection={InspectionActive}; day={Main.dayTime}; time={Main.time}; emitters tile={litTiles}/{tiles}, wall={litWalls}/{walls}; strongest={strongest}; plainBaseline={MawShallowMotionProbe.PlainBaseline(Main.LocalPlayer)}; otherActorsInView={actors}. Inspection light is NOT natural illumination; actors/player light can invalidate comparisons.");
            foreach (Point local in new[] { new Point(28,49), new Point(31,49), new Point(99,35), new Point(98,35) }) {
                Point p = At(local.X, local.Y);
                Mod.Logger.Info($"MAW SHALLOW LIGHT SAMPLE: local={local}; rendered={Lighting.GetColor(p.X,p.Y)}; camera={Main.screenPosition}; only visible warm samples certify illumination.");
            }
        }
        private Rectangle Panel => panel switch {
            0 => new(bounds.X, bounds.Y, P.Width, 56), 1 => new(bounds.X, bounds.Y + 48, P.Width, 56),
            2 => new(bounds.X + 62, bounds.Y + 20, 50, 54),
            3 => new(bounds.X + 12, bounds.Y + 16, 56, 40),
            4 => new(bounds.X + 24, bounds.Y + 40, 56, 40),
            _ => new(bounds.X + 12, bounds.Y + 64, 56, 40)
        };
        private Vector2 ViewPosition => (panel switch {
            0 => At(40,27), 1 => At(40,78), 2 => At(98,36),
            3 => At(35,28), 4 => At(42,51), _ => At(34,78)
        }).ToVector2() * 16;
        private void StartVisit(bool hold, bool connectorOut = false)
        {
            Vector2 destination = hold ? ViewPosition : (connectorOut ? At(82,43) : At(20,12)).ToVector2() * 16 - new Vector2(0, Main.LocalPlayer.height);
            if (Collision.SolidCollision(destination, Main.LocalPlayer.width, Main.LocalPlayer.height) || Teeth.Touching(new((int)destination.X, (int)destination.Y, Main.LocalPlayer.width, Main.LocalPlayer.height)))
                throw new InvalidOperationException("Visit destination obstructed; not clearing it.");
            if (!hold && !Collision.SolidCollision(destination + new Vector2(0, 2), Main.LocalPlayer.width, Main.LocalPlayer.height)) throw new InvalidOperationException("Entry footing missing; not creating a ledge.");
            if (!visiting) { oldPosition = Main.LocalPlayer.position; oldDay = Main.dayTime; oldTime = Main.time; oldRain = Main.raining; oldEclipse = Main.eclipse; visiting = true; }
            viewing = hold; if(!hold) inspectionLamp=false;
            Main.LocalPlayer.Teleport(destination, 1); Main.LocalPlayer.velocity = Vector2.Zero;
            Main.dayTime = true; Main.time = 27000; Main.raining = false; Main.eclipse = false;
            Main.NewText(hold ? "Shallow Maw prototype — HELD CAMERA, not traversal proof." : "Shallow Maw prototype — free controls; hazardous teeth. No equipment or safety route supplied.", Color.Wheat);
        }
        internal bool TryCamera(out Vector2 position)
        {
            position = default; if (!IsQa || !viewing) return false;
            position = Panel.Center.ToVector2() * 16 - new Vector2(Main.screenWidth, Main.screenHeight) * .5f; return true;
        }
        internal void Release()
        {
            if (!Main.gameMenu) Main.LocalPlayer.GetModPlayer<MawShallowMotionProbe>().Cancel("scene-release");
            captureDelay = -1; viewing = false; previewDormant = null; previewBright=inspectionLamp=false; if (!visiting) return;
            if (IsQa) { Main.LocalPlayer.Teleport(oldPosition, 1); Main.LocalPlayer.velocity = Vector2.Zero; }
            Main.dayTime = oldDay; Main.time = oldTime; Main.raining = oldRain; Main.eclipse = oldEclipse; visiting = false;
        }
        public override void PostUpdateEverything()
        {
            if (!IsQa) return;
            if (viewing) {
                Main.LocalPlayer.position = ViewPosition; Main.LocalPlayer.velocity = Vector2.Zero;
                // A stationary camera alone is not a controlled lighting comparison.
                Main.dayTime = true; Main.time = 27000; Main.raining = false; Main.eclipse = false;
                if (InspectionActive) Lighting.AddLight(Main.LocalPlayer.Center,1.2f,1.1f,.9f);
            }
            if (captureDelay < 0 || captureDelay-- != 0) return;
            CaptureManager.Instance.Capture(new CaptureSettings { Area = Panel, Biome = new CaptureBiome(0, 0, Main.LocalPlayer.CurrentSceneEffect.tileColorStyle),
                CaptureBackground = true, CaptureEntities = true, UseScaling = true, OutputName = "Apogean Maw Shallow " + panel + (InspectionActive ? "-inspection" : "") + " " + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") });
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if (Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3" || creation == null) return;
            if (WorldGen.InWorld(bounds.Left, bounds.Top, 30) && WorldGen.InWorld(bounds.Right, bounds.Bottom, 30)) savedState = Fingerprint(new[] { bounds });
            else { failed = true; savedState ??= "invalid-bounds"; }
            tag[SaveKey] = new TagCompound { ["x"] = bounds.X, ["y"] = bounds.Y, ["version"] = P.Version, ["original"] = creation, ["savedState"] = savedState, ["failed"] = failed };
            Mod.Logger.Info($"MAW SHALLOW SAVE: original={creation}; actualInteractionState={savedState}; failed={failed}. Creation is never replaced by interaction state.");
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if (Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3" || !tag.ContainsKey(SaveKey)) return;
            var t = tag.GetCompound(SaveKey); bounds = new(t.GetInt("x"), t.GetInt("y"), P.Width, P.Height);
            creation = t.GetString("original"); savedState = t.GetString("savedState"); failed = t.GetBool("failed") || t.GetInt("version") != P.Version;
        }
        public override void ClearWorld() { bounds = Rectangle.Empty; creation = savedState = null; failed = viewing = visiting = previewBright = inspectionLamp = false; previewDormant = null; captureDelay = -1; }
    }

    // Art/traversal fixture isolation only, not production spawn balancing.
    // Existing actors are never killed or despawned by this hook.
    public sealed class MawShallowAmbientGate : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player.whoAmI != Main.myPlayer || !(ModContent.GetInstance<MawShallowTraversalLab>().PlainVisit || ModContent.GetInstance<MawRibContourStudy>().PlainVisit)) return;
            spawnRate = int.MaxValue; maxSpawns = 0;
        }
    }
}
