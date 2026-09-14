using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using apogean.Content.Diagnostics;
using apogean.Content.Tiles;
using apogean.Content.Walls;
using P = apogean.Common.WorldGeneration.MawSeedPlan;

namespace apogean.Common.WorldGeneration
{
    // A separately saved, explicitly named fresh-world experiment. Loading never stamps terrain.
    public sealed class MawSeedWorld : ModSystem
    {
        internal const string WorldPrefix = "Apogean Maw Seed QA ";
        internal const string WorldV2Prefix = "Apogean Maw Seed V2 QA ";
        internal const string WorldV3Prefix = "Apogean Maw Seed V3 QA ";
        private const string SaveKey = "mawSeedWorldV1";
        private const int Padding = 12;
        private TagCompound record;
        private Placement pending;
        private Bindings bindings;
        private P validationPlan;
        private bool generatedThisSession;
        private bool? validMetadata;
        private WorldFileData generationFile;
        private string generationName;
        private List<(Point Position, CellState State)> protectedBeforeLegacy;
        private string chestsBeforeLegacy;

        internal static MawSeedWorld Instance => ModContent.GetInstance<MawSeedWorld>();
        private string WorldName => Main.ActiveWorldFileData?.Name ?? Main.worldName;
        internal static bool IsTestWorldName(string name) => TryNameSeed(name, out _);
        private static bool TryNameSeed(string name, out int seed) => TryNameVersion(name, out seed, out _);
        private static bool TryNameVersion(string name, out int seed, out int version)
        {
            seed = 0;
            version = 0;
            if (name == null) return false;
            string prefix;
            if (name.StartsWith(WorldPrefix, StringComparison.Ordinal)) { prefix = WorldPrefix; version = 1; }
            else if (name.StartsWith(WorldV2Prefix, StringComparison.Ordinal)) { prefix = WorldV2Prefix; version = 2; }
            else if (name.StartsWith(WorldV3Prefix, StringComparison.Ordinal)) { prefix = WorldV3Prefix; version = 3; }
            else return false;
            string suffix = name.Substring(prefix.Length);
            return suffix.Length > 0 && suffix.All(c => c >= '0' && c <= '9') &&
                int.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out seed);
        }
        private bool Requested => generationFile != null && ReferenceEquals(generationFile, Main.ActiveWorldFileData) &&
            generationName == WorldName && IsTestWorldName(generationName);
        internal bool GeneratingCandidate => Requested;
        internal string PlanningFailure { get; private set; } = "not requested";
        internal Rectangle Bounds => record == null ? Rectangle.Empty : ReadRect(record, "bounds");
        internal Rectangle AffectedBounds => record == null ? Rectangle.Empty : ReadRect(record, "scope");
        internal int Seed => record?.GetInt("seed") ?? 0;
        internal int LayoutVersion => record?.GetInt("version") ?? 0;
        internal string OriginalDigest => record?.GetString("originalDigest") ?? "";
        internal string SavedDigest => record?.GetString("savedDigest") ?? "";
        internal string NativeChecks => record?.GetString("nativeChecks") ?? "not generated";
        internal bool NativePassed => HasLayout && record.GetBool("nativePassed");
        internal Point Entrance => record == null ? Point.Zero : ReadPoint(record, "entrance");
        internal Point Exit => record == null ? Point.Zero : ReadPoint(record, "exit");
        internal Point ConnectorTarget => record == null ? Point.Zero : ReadPoint(record, "connector");
        internal bool HasLayout
        {
            get
            {
                if (record == null || record.GetString("world") != WorldName) return false;
                if (validMetadata.HasValue) return validMetadata.Value;
                Rectangle b = Bounds, s = AffectedBounds;
                validMetadata = TryNameVersion(WorldName, out int namedSeed, out int namedVersion) &&
                    namedSeed == Seed && LayoutVersion == namedVersion && b.Width == P.Width && b.Height == P.Height &&
                    s.Contains(b) && s.Width <= P.Width + 2 * Padding && s.Height <= P.Height + 64 &&
                    InWorld(s) && record.Get<byte[]>("mask").Length == (s.Width * s.Height + 7) / 8;
                return validMetadata.Value;
            }
        }
        // Reconstruct only on an explicit inspection, never during LoadWorldData or SaveWorldData.
        internal P Blueprint
        {
            get
            {
                Require(HasLayout, "No supported saved seed layout in this world.");
                int[] spine = record.Get<int[]>("spine");
                Require(spine.Length == P.Height, "Saved row profile has invalid dimensions.");
                return P.CreateVersion(LayoutVersion, Seed, (int[])spine.Clone(), record.GetInt("leftSurface"), record.GetInt("rightSurface"));
            }
        }

        public override void PreWorldGen()
        {
            ClearWorld();
            // Dedicated autocreate bypasses CreateNewWorld's generatingWorld flag; Final Cleanup
            // also clears gen before our pass. Only the native generation hook arms this latch.
            if (!IsTestWorldName(WorldName)) return;
            generationFile = Main.ActiveWorldFileData;
            generationName = WorldName;
            Require(Requested, "Named seed generation has no stable world-file identity.");
            Mod.Logger.Info("MAW SEED GENERATION ARMED: " + generationName);
        }
        internal void BeginPlanning()
        {
            if (!Requested) return;
            RequireBindings(); // Named candidate worlds must never silently substitute legacy materials.
            PlanningFailure = "no shallow site inspected";
        }

        internal bool TryPlan(MawRupturePlan rupture, Func<int, int> findSurface, IReadOnlyList<Rectangle> occupied)
        {
            if (!Requested) return true;
            RequireBindings();
            try
            {
                Require(rupture.IsMajor && rupture.HasNavigationSpine, "Candidate needs the saved major route.");
                Rectangle bounds = new(rupture.SurfaceCenter.X - P.Width / 2, rupture.SurfaceCenter.Y - P.SurfaceY, P.Width, P.Height);
                int[] spine = new int[P.Height];
                for (int y = 0; y < spine.Length; y++) spine[y] = SpineX(rupture, bounds.Y + y) - bounds.X;
                int left = findSurface(bounds.Left + 4) - bounds.Top;
                int right = findSurface(bounds.Right - 5) - bounds.Top;
                Require(TryNameVersion(generationName, out int seed, out int version), "Unsupported candidate name.");
                P plan = P.CreateVersion(version, seed, spine, left, right);
                Point exit = At(bounds, plan.Exit);
                Point16 next = rupture.NavigationSpine.FirstOrDefault(p => p.Y >= bounds.Bottom + 4);
                Require(next.Y > 0 && next.Y <= bounds.Bottom + 18, "No nearby saved deep waypoint for the outlet.");
                Point target = new(next.X - 1, next.Y - 2);
                List<Point> connector = Connector(exit, target);
                Rectangle scope = Rectangle.Union(bounds, new Rectangle(Math.Min(exit.X, target.X), exit.Y,
                    Math.Abs(exit.X - target.X) + 2, target.Y - exit.Y + 3));
                scope.Inflate(Padding, Padding);
                Require(InWorld(scope) && rupture.ReservedBounds.Contains(scope), "Shallow scope escapes the major reservation.");
                VerifySurfaceClearance(plan, bounds);
                Require(!occupied.Any(r => r.Intersects(scope)), "Shallow scope overlaps another planned site.");
                var placement = new Placement(rupture, plan, bounds, scope, seed, spine, left, right, target, connector);
                byte[] impact = ImpactMask(scope, MakeMask(placement));
                foreach (Rectangle span in Spans(scope, impact))
                    Require(WorldAtlasPlanner.CanReserve(span, 0), "Shallow impact overlaps a StructureMap reservation or disallowed terrain.");
                for (int x = scope.Left; x < scope.Right; x++) for (int y = scope.Top; y < scope.Bottom; y++)
                {
                    if (Owned(scope, impact, x, y)) Require(!ProtectedCell(x, y), $"Protected impact at {x},{y}: active={Main.tile[x,y].HasTile}; tile={Main.tile[x,y].TileType}; wall={Main.tile[x,y].WallType}.");
                }
                foreach (Point position in connector) for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 3; dy++)
                {
                    Point cell = new(position.X + dx, position.Y + dy);
                    if (!bounds.Contains(cell)) continue;
                    P.Cell desired = plan.Cells[cell.X - bounds.X, cell.Y - bounds.Y];
                    Require(!desired.Write || desired.Key == null, "Exact exit connector crosses planned solid anatomy.");
                }
                pending = placement with { ApprovedImpact = impact };
                PlanningFailure = "accepted";
                return true;
            }
            catch (Exception error) when (error is ArgumentException || error is InvalidOperationException)
            {
                PlanningFailure = error.Message;
                return false; // The existing finite 320-site search owns retries; no cell-level skipping.
            }
        }

        internal bool IsCandidate(MawRupturePlan rupture) => rupture != null && rupture.IsMajor && HasLayout &&
            record.GetInt("mouthX") == rupture.SurfaceCenter.X && record.GetInt("mouthY") == rupture.SurfaceCenter.Y;

        internal void BeforeLegacy(MawRupturePlan rupture)
        {
            if (!Requested) return;
            Require(pending != null && ReferenceEquals(pending.Rupture, rupture), "Missing planned pre-legacy protection scope.");
            protectedBeforeLegacy = new();
            Rectangle s = pending.Scope;
            for (int x = s.Left; x < s.Right; x++) for (int y = s.Top; y < s.Bottom; y++)
                if (ProtectedCell(x, y)) protectedBeforeLegacy.Add((new Point(x, y), CellState.Read(x, y)));
            chestsBeforeLegacy = ChestSignature(s);
        }
        private void VerifyProtected(Rectangle scope)
        {
            Require(protectedBeforeLegacy != null && chestsBeforeLegacy != null, "Missing pre-legacy protection evidence.");
            foreach (var sample in protectedBeforeLegacy)
                Require(CellState.Read(sample.Position.X, sample.Position.Y).Equals(sample.State), $"Protected neighbor changed at {sample.Position}.");
            Require(ChestSignature(scope) == chestsBeforeLegacy, "Existing neighboring chest records changed.");
        }
        private static bool ProtectedCell(int x, int y)
        {
            Tile t = Main.tile[x, y];
            bool unknownModWall = t.WallType >= WallID.Count &&
                WallLoader.GetWall(t.WallType) is not (MawNaturalWall or MawWallUnsafe or WastesNaturalWall);
            return MawTerrainRules.IsHardGenerationObstacle(x, y) || Wired(t) || unknownModWall ||
                t.HasTile && Main.tileFrameImportant[t.TileType] && !MawTerrainRules.CanReplaceTileType(t.TileType) &&
                !CaveClutter(t.TileType) && t.TileType != ModContent.TileType<EngraftTuft>();
        }

        private static void VerifySurfaceClearance(P plan, Rectangle bounds)
        {
            if (plan.LayoutVersion < 3) return;
            // V3 owns air down from local row4. Never cut off terrain that crosses
            // its upper boundary and leave a suspended roof outside the edit mask.
            for (int x = 4; x < P.Width - 4; x++)
            {
                P.Cell cell = plan.Cells[x, 4];
                if (!cell.Write || cell.Key != null) continue;
                Tile above = Main.tile[bounds.X + x, bounds.Y + 3];
                Require(!above.HasTile && above.WallType == WallID.None && above.LiquidAmount == 0,
                    $"Surface clearance crosses terrain at {bounds.X+x},{bounds.Y+3}; choose another site.");
            }
        }

        internal void ApplyPlanned(MawRupturePlan rupture)
        {
            if (!Requested) return;
            RequireBindings();
            Placement p = pending;
            Require(p != null && ReferenceEquals(p.Rupture, rupture) && record == null, "No unique preflighted shallow plan for this rupture.");
            Require(ApogeanWorldPlanSystem.Instance.CanPlace(p.Scope, WorldEditIntent.MawGeneration), "Shallow reservation now conflicts with the atlas.");
            VerifyProtected(p.Scope);
            VerifySurfaceClearance(p.Plan, p.Bounds);
            byte[] mask = MakeMask(p);
            byte[] impact = ImpactMask(p.Scope, mask);
            Require(p.ApprovedImpact != null && p.ApprovedImpact.Length == impact.Length, "Missing approved impact.");
            for (int i = 0; i < impact.Length; i++)
                Require((impact[i] & ~p.ApprovedImpact[i]) == 0, "Legacy decoration expanded beyond the preflighted impact.");
            // Only the freshly generated candidate scope is captured, after the legacy writer finishes.
            var original = new CellState[p.Scope.Width, p.Scope.Height];
            for (int x = p.Scope.Left; x < p.Scope.Right; x++) for (int y = p.Scope.Top; y < p.Scope.Bottom; y++)
            {
                if (Owned(p.Scope, impact, x, y)) Require(!ProtectedCell(x, y), $"Unexpected protected impact after legacy generation at {x},{y}.");
                original[x - p.Scope.X, y - p.Scope.Y] = CellState.Read(x, y);
            }
            var createdChests = new List<int>();
            record = MakeRecord(p, mask);
            validMetadata = null;
            validationPlan = p.Plan;
            try
            {
                for (int x = 0; x < P.Width; x++) for (int y = 0; y < P.Height; y++)
                {
                    P.Cell cell = p.Plan.Cells[x, y];
                    if (cell.Write) WriteCell(p.Bounds.X + x, p.Bounds.Y + y, cell);
                }
                // Removing a support can invalidate a whole natural rubble object. Claim and
                // remove its complete footprint up front, not its fragments during framing.
                byte[] terrainMask = MakeMask(p, includeNaturalClutter: false);
                for (int x = p.Scope.Left; x < p.Scope.Right; x++) for (int y = p.Scope.Top; y < p.Scope.Bottom; y++)
                    if (Owned(p.Scope, mask, x, y) && !Owned(p.Scope, terrainMask, x, y))
                    {
                        Require(Requested && NaturalDecoration(Main.tile[x, y]), "Cleanup footprint changed after preflight.");
                        Tile tile = Main.tile[x, y];
                        tile.HasTile = false; tile.TileType = TileID.Dirt; tile.TileFrameX = tile.TileFrameY = 0;
                    }
                foreach (Point position in p.Connector) for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 3; dy++)
                {
                    Point cell = new(position.X + dx, position.Y + dy);
                    // The pure plan's inactive padding stays untouched. The legacy route must already
                    // provide that seam; verification below rejects a closed seam rather than clearing it.
                    if (!p.Bounds.Contains(cell)) WriteCell(cell.X, cell.Y, new P.Cell(null, "legacy", Write: true));
                }
                FrameOwned(p.Scope, mask);
                foreach (P.Tooth tooth in p.Plan.Teeth)
                {
                    Point at = At(p.Bounds, tooth.Root);
                    Point16 origin = MawToothClusterTile.PlacementOrigin(tooth.Face);
                    Require(WorldGen.PlaceObject(at.X + origin.X, at.Y + origin.Y, bindings.Teeth.Type, mute: true, style: tooth.Variant / 4),
                        $"Native tooth placement failed at {at}.");
                }
                foreach (P.Point chest in p.Plan.Chests)
                {
                    Point at = At(p.Bounds, chest);
                    int id = WorldGen.PlaceChest(at.X, at.Y + 1, TileID.Containers, false, 0);
                    Require(id >= 0, $"Native chest placement failed at {at}.");
                    createdChests.Add(id);
                    Main.chest[id].name = "Maw seed QA cache (empty)";
                }
                foreach (P.Strand strand in p.Plan.Fibers) for (int d = 1; d <= strand.Length; d++)
                {
                    int x = p.Bounds.X + strand.Root.X, y = p.Bounds.Y + strand.Root.Y + d;
                    Tile tile = Main.tile[x, y];
                    Require(!tile.HasTile, $"Fiber footprint occupied at {x},{y}.");
                    tile.HasTile = true; tile.TileType = (ushort)bindings.Fiber;
                    tile.TileFrameX = 0; tile.TileFrameY = (short)((d - 1) * 18);
                    WorldGen.TileFrame(x, y, resetFrame: true);
                }
                FrameOwned(p.Scope, mask);
                // Framing observes final topology; it does not confer ownership over inactive cells.
                // Restore only their cache/state changes, never a desired cell or a failed object.
                VerifyProtectedCells();
                for (int x = p.Scope.Left; x < p.Scope.Right; x++) for (int y = p.Scope.Top; y < p.Scope.Bottom; y++)
                {
                    if (Owned(p.Scope, mask, x, y)) continue;
                    CellState before = original[x - p.Scope.X, y - p.Scope.Y], after = CellState.Read(x, y);
                    Require(after.SameNonFrameState(before), $"Framing changed unowned terrain/state at {x},{y}: before={before.Describe()}; after={after.Describe()}.");
                    if (!Owned(p.Scope, impact, x, y)) Require(after.Equals(before),
                        $"Framing escaped its impact halo at {x},{y}: before={before.Describe()};{before.DescribeFrames()}; after={after.Describe()};{after.DescribeFrames()}.");
                    // Amount/type already match; retain live skip/check flags so the native
                    // liquid queue remains consistent while restoring original frame state.
                    (before with { Liquid = after.Liquid }).Restore(x, y);
                }
                // Ignore only the newly created QA cache when comparing pre-existing containers.
                VerifyProtectedCells();
                Require(ChestSignature(p.Scope, createdChests) == chestsBeforeLegacy, "Existing neighboring chest records changed during candidate framing.");
                string checks = InspectNative(p.Plan);
                string digest = Fingerprint();
                record["originalDigest"] = digest; record["savedDigest"] = digest;
                record["nativeChecks"] = checks; record["nativePassed"] = true;
                generatedThisSession = true;
                Mod.Logger.Info($"MAW SEED BUILD: seed={Seed}; version={LayoutVersion}; bounds={Bounds}; exit={Exit}; connector={ConnectorTarget}; {checks}; digest={digest}. Node pocket reserved only.");
            }
            catch
            {
                foreach (int id in createdChests) Main.chest[id] = null;
                for (int x = p.Scope.Left; x < p.Scope.Right; x++) for (int y = p.Scope.Top; y < p.Scope.Bottom; y++)
                    if (Owned(p.Scope, mask, x, y)) original[x - p.Scope.X, y - p.Scope.Y].Restore(x, y);
                record["nativePassed"] = false;
                record["nativeChecks"] = "failed; owned shallow scope rolled back; generation rejected";
                throw;
            }
            finally { pending = null; }
        }

        internal string VerifyOriginal()
        {
            Require(HasLayout, "No supported saved seed layout.");
            string checks = InspectNative(Blueprint);
            Require(OriginalDigest.Length == 64 && Fingerprint() == OriginalDigest, "Seed creation digest differs; no repair or rebaseline.");
            string report = "MAW SEED ORIGINAL PASS: " + checks + "; digest=" + OriginalDigest;
            Mod.Logger.Info(report); return report;
        }
        internal string VerifySaved()
        {
            Require(HasLayout && SavedDigest.Length == 64, "No saved seed-state digest.");
            Require(Fingerprint() == SavedDigest, "Seed saved-state digest differs; no repair or rebaseline.");
            string report = "MAW SEED SAVED PASS: digest=" + SavedDigest + "; interaction state only, not pristine geometry";
            Mod.Logger.Info(report); return report;
        }

        public override void PostWorldGen()
        {
            if (generationFile == null) return;
            try
            {
                Require(Requested && generatedThisSession && NativePassed, "Named seed generation did not produce its required candidate; no legacy fallback.");
                string checks = VerifyOriginal();
                MawRuptureValidationReport route = MawRuptureValidation.Inspect(ApogeanWorldPlanSystem.Instance.Plan.GetMajorRupture());
                Require(route.Passed, "Candidate full-depth validation failed: " + route);
                Mod.Logger.Info($"MAW SEED POST WORLDGEN PASS: {checks}; full-route={route}");
            }
            catch { if (record != null) record["nativePassed"] = false; throw; }
            finally { DisarmGeneration(); }
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if (record == null) return;
            // Read-only digest of the current interaction state; creation evidence is immutable.
            // Persist the record even if the digest read fails, so tML cannot silently drop ownership.
            try { if (HasLayout && NativePassed) record["savedDigest"] = Fingerprint(); }
            catch (Exception error) { Mod.Logger.Warn("MAW SEED save digest unavailable: " + error.Message); }
            tag[SaveKey] = record;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            DisarmGeneration();
            record = tag.ContainsKey(SaveKey) ? tag.GetCompound(SaveKey) : null;
            pending = null; validationPlan = null; generatedThisSession = false; validMetadata = null;
        }
        public override void ClearWorld()
        {
            DisarmGeneration();
            protectedBeforeLegacy = null; chestsBeforeLegacy = null;
            record = null; pending = null; validationPlan = null; bindings = null; validMetadata = null;
            generatedThisSession = false; PlanningFailure = "not requested";
        }
        private void DisarmGeneration() { generationFile = null; generationName = null; }
        public override void OnWorldLoad() => DisarmGeneration();
        public override void OnWorldUnload() => ClearWorld();

        internal bool IsInterior(int x, int y, MawRupturePlan rupture)
        {
            if (record == null || !Bounds.Contains(x, y) || !IsCandidate(rupture)) return false;
            validationPlan ??= Blueprint;
            P.Cell cell = validationPlan.Cells[x - Bounds.X, y - Bounds.Y];
            if (!cell.Write) return false;
            RequireBindings();
            ushort wall = Main.tile[x, y].WallType;
            return wall == WallID.None && cell.Wall == null || bindings.Walls.Values.Contains(wall);
        }
        internal bool IsHazard(int x, int y, MawRupturePlan rupture) => record != null && Bounds.Contains(x, y) && IsCandidate(rupture) &&
            Main.tile[x, y].HasUnactuatedTile && Main.tile[x, y].TileType == ModContent.TileType<MawToothClusterTile>();

        private string InspectNative(P plan)
        {
            RequireBindings();
            plan.Validate();
            Rectangle bounds = Bounds;
            var objects = new Dictionary<Point, (int Type, int X, int Y)>();
            foreach (P.Tooth tooth in plan.Teeth)
            {
                Point root = At(bounds, tooth.Root);
                for (int dx = 0; dx < 4; dx++) for (int dy = 0; dy < 4; dy++)
                    objects.Add(new(root.X + dx, root.Y + dy), (bindings.Teeth.Type, tooth.Variant * 72 + dx * 18, dy * 18));
                for (int i = 0; i < 4; i++)
                {
                    Point support = At(bounds, tooth.Support(i)); Tile tile = Main.tile[support];
                    Require(tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] &&
                        tile.Slope == SlopeType.Solid && !tile.IsHalfBlock, $"Unsupported tooth at {root}.");
                }
            }
            foreach (P.Point chest in plan.Chests)
            {
                Point at = At(bounds, chest); int id = Chest.FindChest(at.X, at.Y);
                Require(id >= 0 && Main.chest[id].item.All(i => i == null || i.IsAir), $"Missing/nonempty original cache at {at}.");
                for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 2; dy++) objects.Add(new(at.X + dx, at.Y + dy), (TileID.Containers, dx * 18, dy * 18));
                for (int dx = 0; dx < 2; dx++) Require(Main.tile[at.X + dx, at.Y + 2].HasUnactuatedTile &&
                    Main.tileSolid[Main.tile[at.X + dx, at.Y + 2].TileType], "Cache lost its floor.");
            }
            foreach (P.Strand strand in plan.Fibers) for (int d = 1; d <= strand.Length; d++)
            {
                Point at = new(bounds.X + strand.Root.X, bounds.Y + strand.Root.Y + d);
                Require(MawHangingFiber.Depth(at.X, at.Y, bindings.Fiber) == d, $"Unsupported fiber at {at}.");
                objects.Add(at, (bindings.Fiber, 0, (d - 1) * 18));
            }
            int cells = 0, frames = 0, lights = 0;
            foreach (Point at in objects.Keys) Require(plan.Cells[at.X - bounds.X, at.Y - bounds.Y].Write, "Native object escapes the write mask.");
            for (int x = 0; x < P.Width; x++) for (int y = 0; y < P.Height; y++)
            {
                P.Cell cell = plan.Cells[x, y]; if (!cell.Write) continue;
                Point at = new(bounds.X + x, bounds.Y + y); Tile tile = Main.tile[at];
                bool obj = objects.TryGetValue(at, out var expected);
                Require(tile.HasTile == (cell.Key != null || obj) && tile.WallType == bindings.Wall(cell.Wall) && Clean(tile), $"Native cell/artifact mismatch at {at}.");
                if (tile.HasTile) Require(tile.TileType == (obj ? expected.Type : bindings.Tile(cell.Key)) && !tile.IsHalfBlock &&
                    (byte)tile.Slope == cell.Slope && (!obj || tile.TileFrameX == expected.X && tile.TileFrameY == expected.Y), $"Native tile/frame/slope mismatch at {at}.");
                if (!plan.AllowsWall(x, y)) Require(tile.WallType == WallID.None, $"Surface wall fringe at {at}.");
                if (cell.Key != null && !obj)
                {
                    Require(bindings.Maps[cell.Key].TryMap(at.X, at.Y, tile.TileFrameX, tile.TileFrameY, out _, out _), $"Unmapped native tile frame at {at}."); frames++;
                    float r = 0, g = 0, b = 0; TileLoader.GetTile(tile.TileType)?.ModifyLight(at.X, at.Y, ref r, ref g, ref b);
                    Require(ValidLight(r, g, b) && (cell.Key == "amber" || r == 0 && g == 0 && b == 0), $"Unexpected terrain emission at {at}."); lights++;
                }
                if (cell.Wall != null)
                {
                    Require(MawTerrainStudies.Wall(cell.Wall).Map.TryMap(at.X, at.Y, tile.WallFrameX, tile.WallFrameY, out _, out _), $"Unmapped native wall frame at {at}."); frames++;
                    float r = 0, g = 0, b = 0; WallLoader.GetWall(tile.WallType)?.ModifyLight(at.X, at.Y, ref r, ref g, ref b);
                    Require(ValidLight(r, g, b) && (cell.Wall == "amber" || r == 0 && g == 0 && b == 0), $"Unexpected wall emission at {at}."); lights++;
                }
                cells++;
            }
            int routePositions = VerifyNativeRoute(plan);
            int branchPositions = plan.LayoutVersion >= 2 ? VerifyNativeRoute(plan, true) : 0;
            foreach (Point position in Connector(Exit, ConnectorTarget))
                Require(BodyClear(position), $"Exact exit-to-spine connector obstructed at {position}.");
            return $"cells={cells}; objects={objects.Count}; frames={frames}; light-hooks={lights}; reachable={routePositions}; branch-only={branchPositions}; cache=1-empty; node=1-reservation; visual-alpha/manual-traversal=unverified";
        }

        private int VerifyNativeRoute(P plan, bool branchOnly = false)
        {
            Rectangle b = Bounds; var seen = new HashSet<Point>(); var queue = new Queue<Point>();
            void Visit(Point at)
            {
                if (!b.Contains(at) || !b.Contains(at.X + 1, at.Y + 2) || seen.Contains(at)) return;
                for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 3; dy++)
                {
                    int x = at.X + dx - b.X, y = at.Y + dy - b.Y;
                    if (!plan.Cells[x, y].Write || branchOnly &&
                        (!plan.Cells[x, y].Side || plan.IsCentralShaft(x, y))) return;
                }
                if (!BodyClear(at)) return; seen.Add(at); queue.Enqueue(at);
            }
            Visit(branchOnly ? At(b, plan.LowerRejoinEntrance) : Entrance);
            while (queue.Count > 0)
            {
                Point p = queue.Dequeue(); Visit(new(p.X - 1, p.Y)); Visit(new(p.X + 1, p.Y)); Visit(new(p.X, p.Y - 1)); Visit(new(p.X, p.Y + 1));
            }
            if (branchOnly)
            {
                Require(seen.Contains(At(b, plan.LowerRejoinExit)), "Native side route cannot reach lower rejoin without using central shaft.");
                return seen.Count;
            }
            Require(seen.Contains(Exit), "Native entrance cannot reach exact exit.");
            if (plan.LayoutVersion >= 2) Require(seen.Contains(At(b, plan.LowerRejoinEntrance)) &&
                seen.Contains(At(b, plan.LowerRejoinExit)), "Native side-route junction disconnected from main descent.");
            foreach (P.Point c in plan.Chests) Require(seen.Contains(new(b.X + c.X, b.Y + c.Y - 2)), "Native cache is unreachable.");
            foreach (P.Point c in plan.NodeSites) Require(seen.Contains(new(b.X + c.X, b.Y + c.Y - 1)), "Native node reservation is unreachable.");
            return seen.Count;
        }
        private bool BodyClear(Point at)
        {
            for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 3; dy++)
            {
                Tile tile = Main.tile[at.X + dx, at.Y + dy];
                if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] || tile.TileType == bindings.Teeth.Type)) return false;
            }
            return true;
        }

        private void WriteCell(int x, int y, P.Cell cell)
        {
            Require(Requested, "Seed writer is fresh-world-only.");
            Tile tile = Main.tile[x, y]; tile.ClearEverything();
            if (cell.Key != null) { tile.HasTile = true; tile.TileType = (ushort)bindings.Tile(cell.Key); tile.Slope = (SlopeType)cell.Slope; }
            tile.WallType = (ushort)bindings.Wall(cell.Wall);
        }
        private void RequireBindings()
        {
            if (bindings != null) return;
            Require(MawPackedPreview.Enabled && MawAnatomyMaterials.Available && MawToothClusterTile.Included(Mod), "Named Maw seed QA generation needs packed/anatomy/cluster assets.");
            bindings = new Bindings(Mod);
        }
        private sealed class Bindings
        {
            internal readonly Dictionary<string, int> Tiles = new(), Walls = new();
            internal readonly Dictionary<string, PackedMaterialMap> Maps = new();
            internal readonly MawToothClusterTile Teeth;
            internal readonly int Fiber;
            internal Bindings(Mod mod)
            {
                Teeth = ModContent.GetInstance<MawToothClusterTile>();
                Fiber = ModContent.Find<ModTile>("apogean/MawHangingFiber").Type;
                foreach (string key in new[] { "soil", "stone", "grass", "rib", "cap", "amber" })
                {
                    Tiles[key] = key is "rib" or "cap" ? MawAnatomyMaterials.Tile(key).Type :
                        key == "amber" ? ModContent.TileType<MawAmberLitTile>() : MawPackedPreview.TileType(key);
                    Maps[key] = key is "rib" or "cap" ? new PackedMaterialMap(mod.GetFileBytes("Content/Diagnostics/Anatomy/" + key + ".bin")) : MawTerrainStudies.Tile(key).Map;
                }
                foreach (string key in new[] { "soil", "stone", "grass", "amber" })
                    Walls[key] = key == "amber" ? ModContent.WallType<MawAmberLitWall>() : MawPackedPreview.Wall(key).Type;
                Walls["legacy"] = ModContent.WallType<MawWallUnsafe>();
            }
            internal int Tile(string key) => Tiles[key];
            internal int Wall(string key) => key == null ? WallID.None : Walls[key];
        }

        private string Fingerprint()
        {
            Rectangle scope = AffectedBounds; byte[] mask = record.Get<byte[]>("mask");
            using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
            writer.Write(LayoutVersion); writer.Write(Seed);
            for (int x = scope.Left; x < scope.Right; x++) for (int y = scope.Top; y < scope.Bottom; y++)
            {
                if (!Owned(scope, mask, x, y)) continue;
                Tile tile = Main.tile[x, y]; writer.Write(x); writer.Write(y); writer.Write(tile.HasTile);
                if (tile.HasTile)
                {
                    writer.Write(TileLoader.GetTile(tile.TileType)?.FullName ?? "Terraria/" + tile.TileType);
                    writer.Write((byte)tile.Slope); writer.Write(tile.IsHalfBlock);
                    if (Main.tileFrameImportant[tile.TileType]) { writer.Write(tile.TileFrameX); writer.Write(tile.TileFrameY); }
                }
                writer.Write(WallLoader.GetWall(tile.WallType)?.FullName ?? "Terraria/" + tile.WallType);
                writer.Write(tile.TileColor); writer.Write(tile.WallColor); writer.Write(tile.HasActuator); writer.Write(tile.IsActuated);
                writer.Write(tile.RedWire); writer.Write(tile.GreenWire); writer.Write(tile.BlueWire); writer.Write(tile.YellowWire);
                writer.Write(tile.IsTileInvisible); writer.Write(tile.IsWallInvisible); writer.Write(tile.IsTileFullbright); writer.Write(tile.IsWallFullbright);
                writer.Write(tile.LiquidAmount); if (tile.LiquidAmount != 0) writer.Write(tile.LiquidType);
            }
            // Track native container records independently of their tile footprints, including player edits.
            foreach (Chest chest in Main.chest)
            {
                if (chest == null || !scope.Contains(chest.x, chest.y) || !Owned(scope, mask, chest.x, chest.y)) continue;
                writer.Write(chest.x); writer.Write(chest.y); writer.Write(chest.name ?? "");
                foreach (Item item in chest.item)
                {
                    TagIO.Write(ItemIO.Save(item ?? new Item()), writer);
                }
            }
            writer.Flush(); return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
        }
        private TagCompound MakeRecord(Placement p, byte[] mask) => new()
        {
            ["world"] = generationName, ["version"] = p.Plan.LayoutVersion, ["seed"] = p.Seed,
            ["bounds"] = RectTag(p.Bounds), ["scope"] = RectTag(p.Scope), ["spine"] = p.Spine,
            ["leftSurface"] = p.LeftSurface, ["rightSurface"] = p.RightSurface,
            ["mouthX"] = (int)p.Rupture.SurfaceCenter.X, ["mouthY"] = (int)p.Rupture.SurfaceCenter.Y,
            ["entrance"] = PointTag(At(p.Bounds, p.Plan.Entrance)), ["exit"] = PointTag(At(p.Bounds, p.Plan.Exit)),
            ["connector"] = PointTag(p.Target), ["mask"] = mask, ["nativePassed"] = false,
            ["originalDigest"] = "", ["savedDigest"] = "", ["nativeChecks"] = "not verified"
        };
        private static byte[] MakeMask(Placement p, bool includeNaturalClutter = true)
        {
            var mask = new byte[(p.Scope.Width * p.Scope.Height + 7) / 8];
            void Set(int x, int y) { int i = (y - p.Scope.Y) * p.Scope.Width + x - p.Scope.X; mask[i / 8] |= (byte)(1 << (i % 8)); }
            for (int x = 0; x < P.Width; x++) for (int y = 0; y < P.Height; y++) if (p.Plan.Cells[x, y].Write) Set(p.Bounds.X + x, p.Bounds.Y + y);
            foreach (Point at in p.Connector) for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 3; dy++)
                if (!p.Bounds.Contains(at.X + dx, at.Y + dy)) Set(at.X + dx, at.Y + dy);
            if (includeNaturalClutter)
            {
                byte[] halo = ImpactMask(p.Scope, mask);
                var seen = new HashSet<Point>(); var queue = new Queue<Point>();
                void Visit(int x, int y)
                {
                    if (!p.Scope.Contains(x, y) || !NaturalDecoration(Main.tile[x, y])) return;
                    Point at = new(x, y); if (!seen.Add(at)) return;
                    Require(x > p.Scope.Left + 2 && x < p.Scope.Right - 3 && y > p.Scope.Top + 2 && y < p.Scope.Bottom - 3,
                        "Natural decoration component escapes bounded cleanup scope.");
                    Require(seen.Count <= 2048, "Natural decoration component exceeds cleanup budget.");
                    Set(x, y); queue.Enqueue(at);
                }
                for (int x = p.Scope.Left; x < p.Scope.Right; x++) for (int y = p.Scope.Top; y < p.Scope.Bottom; y++)
                    if (Owned(p.Scope, halo, x, y)) Visit(x, y);
                while (queue.Count > 0)
                {
                    Point at = queue.Dequeue();
                    for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++) Visit(at.X + dx, at.Y + dy);
                }
            }
            return mask;
        }
        private static bool NaturalDecoration(Tile tile) => tile.HasTile &&
            (CaveClutter(tile.TileType) || tile.TileType == ModContent.TileType<EngraftTuft>());
        private static bool Owned(Rectangle scope, byte[] mask, int x, int y)
        {
            int i = (y - scope.Y) * scope.Width + x - scope.X; return (mask[i / 8] & (1 << (i % 8))) != 0;
        }
        private static byte[] ImpactMask(Rectangle scope, byte[] writes)
        {
            var result = new byte[writes.Length];
            for (int x = scope.Left; x < scope.Right; x++) for (int y = scope.Top; y < scope.Bottom; y++)
            {
                if (!Owned(scope, writes, x, y)) continue;
                for (int dx = -2; dx <= 2; dx++) for (int dy = -2; dy <= 2; dy++)
                {
                    int ax = x + dx, ay = y + dy;
                    Require(scope.Contains(ax, ay), "Impact escaped snapshot padding.");
                    int i = (ay - scope.Y) * scope.Width + ax - scope.X;
                    result[i / 8] |= (byte)(1 << (i % 8));
                }
            }
            return result;
        }
        private static IEnumerable<Rectangle> Spans(Rectangle scope, byte[] mask)
        {
            for (int y = scope.Top; y < scope.Bottom; y++)
                for (int x = scope.Left; x < scope.Right; x++)
                {
                    if (!Owned(scope, mask, x, y)) continue;
                    int start = x;
                    while (x + 1 < scope.Right && Owned(scope, mask, x + 1, y)) x++;
                    yield return new Rectangle(start, y, x - start + 1, 1);
                }
        }
        private static void FrameOwned(Rectangle scope, byte[] mask)
        {
            // Start native framing only at owned centers. Square wrappers also start
            // work on the unowned ring, whose recursive reframing can escape impact.
            // Native recursion remains subject to the unchanged exact halo assertion.
            foreach (Rectangle span in Spans(scope, mask)) for (int x = span.Left; x < span.Right; x++)
            {
                WorldGen.TileFrame(x, span.Y, resetFrame: true);
                Framing.WallFrame(x, span.Y, resetFrame: true);
            }
        }
        private void VerifyProtectedCells()
        {
            foreach (var sample in protectedBeforeLegacy)
                Require(CellState.Read(sample.Position.X, sample.Position.Y).Equals(sample.State), $"Protected neighbor changed at {sample.Position}.");
        }
        private static string ChestSignature(Rectangle scope, ICollection<int> ignore = null)
        {
            using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
            for (int i = 0; i < Main.chest.Length; i++)
            {
                Chest c = Main.chest[i];
                if (c == null || !scope.Contains(c.x, c.y) || ignore?.Contains(i) == true) continue;
                writer.Write(i); writer.Write(c.x); writer.Write(c.y); writer.Write(c.name ?? "");
                foreach (Item item in c.item) TagIO.Write(ItemIO.Save(item ?? new Item()), writer);
            }
            writer.Flush(); return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
        }
        private static List<Point> Connector(Point start, Point end)
        {
            var points = new List<Point>(); Point at = start; points.Add(at);
            // Four-connected top-left positions give an exact swept 2x3 body path, including the seam.
            int dy = end.Y - start.Y; Require(dy > 0 && dy <= 48 && Math.Abs(end.X - start.X) <= 24, "Unbounded outlet connector.");
            for (int y = start.Y + 1; y <= end.Y; y++)
            {
                int x = start.X + (end.X - start.X) * (y - start.Y) / dy;
                while (at.X != x) { at.X += Math.Sign(x - at.X); points.Add(at); }
                at.Y = y; points.Add(at);
            }
            return points;
        }
        private static int SpineX(MawRupturePlan rupture, int y)
        {
            var spine = rupture.NavigationSpine;
            if (y <= spine[0].Y) return spine[0].X;
            for (int i = 1; i < spine.Count; i++) if (y <= spine[i].Y)
            {
                Point16 a = spine[i - 1], b = spine[i];
                return a.X + (b.X - a.X) * (y - a.Y) / Math.Max(1, b.Y - a.Y);
            }
            return spine[spine.Count - 1].X;
        }
        // Natural rubble is frame-important too; it is not furniture or a landmark.
        // This exception is limited to the opt-in fresh-world candidate preflight.
        private static bool CaveClutter(ushort type) => type is TileID.Pots or TileID.PotsSuspended or TileID.PotsEcho or TileID.Stalactite or TileID.ExposedGems or TileID.LongMoss
            or TileID.SmallPiles or TileID.LargePiles or TileID.LargePiles2
            or TileID.ImmatureHerbs or TileID.MatureHerbs or TileID.BloomingHerbs or TileID.Sunflower
            or TileID.WaterDrip or TileID.LavaDrip or TileID.HoneyDrip or TileID.SandDrip;
        private static bool Wired(Tile t) => t.HasActuator || t.IsActuated || t.RedWire || t.GreenWire || t.BlueWire || t.YellowWire;
        private static bool Clean(Tile t) => !Wired(t) && t.LiquidAmount == 0 && t.TileColor == PaintID.None && t.WallColor == PaintID.None &&
            !t.IsTileInvisible && !t.IsWallInvisible && !t.IsTileFullbright && !t.IsWallFullbright;
        private static bool ValidLight(float r, float g, float b) => float.IsFinite(r) && float.IsFinite(g) && float.IsFinite(b) && r >= 0 && g >= 0 && b >= 0;
        private static bool InWorld(Rectangle r) => r.Width > 0 && r.Height > 0 && WorldGen.InWorld(r.Left, r.Top, 30) && WorldGen.InWorld(r.Right, r.Bottom, 30);
        private static Point At(Rectangle b, P.Point p) => new(b.X + p.X, b.Y + p.Y);
        private static TagCompound RectTag(Rectangle r) => new() { ["x"] = r.X, ["y"] = r.Y, ["w"] = r.Width, ["h"] = r.Height };
        private static Rectangle ReadRect(TagCompound tag, string key) { TagCompound r = tag.GetCompound(key); return new(r.GetInt("x"), r.GetInt("y"), r.GetInt("w"), r.GetInt("h")); }
        private static TagCompound PointTag(Point p) => new() { ["x"] = p.X, ["y"] = p.Y };
        private static Point ReadPoint(TagCompound tag, string key) { TagCompound p = tag.GetCompound(key); return new(p.GetInt("x"), p.GetInt("y")); }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException("MAW SEED: " + message); }
        private sealed record Placement(MawRupturePlan Rupture, P Plan, Rectangle Bounds, Rectangle Scope, int Seed,
            int[] Spine, int LeftSurface, int RightSurface, Point Target, List<Point> Connector, byte[] ApprovedImpact = null);
        private readonly record struct CellState(TileTypeData Type, WallTypeData Wall, TileWallWireStateData State,
            LiquidData Liquid, TileWallBrightnessInvisibilityData Coating)
        {
            // Cover every value compared by SameNonFrameState, including liquid
            // bookkeeping and all coating bits; frame coordinates/caches are excluded.
            internal string Describe() => $"tile={Type.Type};wall={Wall.Type};bits={State.NonFrameBits:X8};" +
                $"hasTile={State.HasTile};actuated={State.IsActuated};actuator={State.HasActuator};" +
                $"tileColor={State.TileColor};wallColor={State.WallColor};halfBlock={State.IsHalfBlock};slope={State.Slope};" +
                $"redWire={State.RedWire};blueWire={State.BlueWire};greenWire={State.GreenWire};yellowWire={State.YellowWire};" +
                $"liquidAmount={Liquid.Amount};liquidType={Liquid.LiquidType};skipLiquid={Liquid.SkipLiquid};checkingLiquid={Liquid.CheckingLiquid};" +
                $"coating={Coating.Data:X2};tileInvisible={Coating.IsTileInvisible};wallInvisible={Coating.IsWallInvisible};" +
                $"tileFullbright={Coating.IsTileFullbright};wallFullbright={Coating.IsWallFullbright}";
            internal string DescribeFrames() => $"tileFrameX={State.TileFrameX};tileFrameY={State.TileFrameY};tileFrameNumber={State.TileFrameNumber};" +
                $"wallFrameX={State.WallFrameX};wallFrameY={State.WallFrameY};wallFrameNumber={State.WallFrameNumber}";
            // SkipLiquid/CheckingLiquid are transient native queue bookkeeping, not water
            // content. The unowned restore caller preserves their current values.
            internal bool SameNonFrameState(CellState other) => Type.Equals(other.Type) && Wall.Equals(other.Wall) &&
                Liquid.Amount == other.Liquid.Amount && Liquid.LiquidType == other.Liquid.LiquidType &&
                Coating.Equals(other.Coating) && State.NonFrameBits == other.State.NonFrameBits;
            internal static CellState Read(int x, int y) { Tile t = Main.tile[x, y]; return new(t.Get<TileTypeData>(), t.Get<WallTypeData>(), t.Get<TileWallWireStateData>(), t.Get<LiquidData>(), t.Get<TileWallBrightnessInvisibilityData>()); }
            internal void Restore(int x, int y) { Tile t = Main.tile[x, y]; t.Get<TileTypeData>() = Type; t.Get<WallTypeData>() = Wall; t.Get<TileWallWireStateData>() = State; t.Get<LiquidData>() = Liquid; t.Get<TileWallBrightnessInvisibilityData>() = Coating; }
        }
    }
}
