using System;
using System.Collections.Generic;

namespace apogean.Content.Diagnostics
{
    // Pure, fixed QA template. No Terraria references, asset loading, placement or update hooks.
    internal sealed class MawShallowTraversalPlan
    {
        internal const int Version = 1, Width = 112, Height = 104;
        internal const int TilePixels = 16, BodyWidth = 20, BodyHeight = 42;
        internal enum Zone { Outside, Geology, Gullet, Pocket, Connector }
        internal readonly record struct Point(int X, int Y);
        internal readonly record struct Rect(int X, int Y, int Width, int Height)
        {
            internal int Right => X + Width;
            internal int Bottom => Y + Height;
            internal bool Contains(int x, int y) => x >= X && x < Right && y >= Y && y < Bottom;
        }
        // Null Key means authored air, NOT permission to clear existing world terrain.
        // All non-null keys are solid. Slopes remain conservatively solid in clearance tests.
        internal readonly record struct Cell(string Key = null, byte Slope = 0,
            string Wall = null, Zone Region = Zone.Outside);
        internal readonly record struct Cluster(Point Root, int Variant)
        {
            // Existing persistent bank order: floor, left support, ceiling, right support.
            internal int Surface => Variant % 4;
            internal int Style => Variant / 4;
            internal Point OriginOffset => Surface switch {
                1 => new(0, 1), 2 => new(1, 0), 3 => new(3, 1), _ => new(1, 3)
            };
            internal Point Support(int cell, int depth = 1) => Surface switch {
                1 => new(Root.X - depth, Root.Y + cell),
                2 => new(Root.X + cell, Root.Y - depth),
                3 => new(Root.X + 3 + depth, Root.Y + cell),
                _ => new(Root.X + cell, Root.Y + 3 + depth)
            };
        }
        internal readonly record struct Rib(Point Root, int Direction, int Length);

        internal static Rect Geology => new(4, 12, 104, 88);
        internal static Rect Gullet => new(28, 8, 28, 88);
        internal static Rect Pocket => new(84, 28, 20, 18);
        // Bounding rectangle only; Cell.Region marks the exact bent corridor, not this whole box.
        internal static Rect Connector => new(54, 39, 32, 30);
        internal static Point Entry => new(40, 14);
        internal static Point DescentEnd => new(40, 91);
        internal static Point PocketGoal => new(99, 36);

        private static readonly Rib[] RibSpecs = {
            new(new(26, 31), 1, 11), new(new(57, 55), -1, 12), new(new(26, 81), 1, 8)
        };
        private static readonly Cluster[] ClusterSpecs = {
            new(new(88, 42), 0), new(new(28, 20), 1), new(new(88, 28), 2), new(new(52, 40), 3),
            new(new(92, 42), 4), new(new(28, 24), 5), new(new(92, 28), 6), new(new(52, 44), 7)
        };
        private readonly Cell[,] cells;
        private MawShallowTraversalPlan()
        {
            cells = BuildCells();
            ValidateExport(cells, ClusterSpecs);
        }
        internal static MawShallowTraversalPlan Create() => new();
        internal Cell[,] ExportCells() => (Cell[,])cells.Clone();
        internal Cluster[] ExportClusters() => (Cluster[])ClusterSpecs.Clone();
        internal Rib[] ExportRibs() => (Rib[])RibSpecs.Clone();

        private static int RibTop(Rib rib, int d) => d * d / (2 * rib.Length);
        private static int RibThickness(Rib rib, int d) => 4 - 3 * d / (rib.Length - 1);
        private static byte RibSlope(Rib rib, int d, int h) =>
            d >= 2 && h == 0 && (d == rib.Length - 1 || RibTop(rib, d + 1) > RibTop(rib, d))
                ? (byte)(rib.Direction > 0 ? 1 : 2) : (byte)0;

        private static Cell[,] BuildCells()
        {
            var p = new Cell[Width, Height];
            void Fill(Rect r, Func<int, int, Cell> cell)
            {
                for (int x = r.X; x < r.Right; x++)
                    for (int y = r.Y; y < r.Bottom; y++) p[x, y] = cell(x, y);
            }
            Fill(Geology, (x, y) => {
                string key = y == 12 ? "cap" : y == 13 ? "grass" : y < 18 ? "soil" : "stone";
                return new(key, 0, key == "cap" ? "grass" : key, Zone.Geology);
            });
            Fill(Gullet, (x, y) => new(null, 0, y < 18 ? null : "stone", Zone.Gullet));
            Fill(Pocket, (x, y) => new(null, 0, "stone", Zone.Pocket));
            // One connector with two unequal offset rises. Turns are at least 4x4;
            // the lower bare horizontal throat is 3 high, the middle bare rise 2 wide.
            foreach (Rect r in new[] {
                new Rect(54, 66, 18, 3), new Rect(68, 50, 4, 19),
                new Rect(68, 50, 14, 4), new Rect(78, 39, 4, 15), new Rect(78, 39, 8, 4),
                new Rect(68, 65, 4, 4)
            }) Fill(r, (x, y) => new(null, 0, "stone",
                Gullet.Contains(x, y) ? Zone.Gullet : Pocket.Contains(x, y) ? Zone.Pocket : Zone.Connector));
            foreach (int x in new[] { 68, 71 })
                for (int y = 56; y < 63; y++) p[x, y] = new("stone", 0, "stone", Zone.Geology);

            // Retain a short rooted side-face grass treatment; it is not growth simulation.
            for (int y = 18; y < 28; y++) p[27, y] = new("grass", 0, "grass", Zone.Geology);
            foreach (Rib rib in RibSpecs)
                for (int d = 0; d < rib.Length; d++)
                    for (int h = 0; h < RibThickness(rib, d); h++) {
                        int x = rib.Root.X + rib.Direction * d, y = rib.Root.Y + RibTop(rib, d) + h;
                        p[x, y] = new(d < 2 ? "bone" : "rib", RibSlope(rib, d, h), "bone", p[x, y].Region);
                    }
            // Two small organs only. Ordinary cap/soil/stone/bone remain non-emissive.
            foreach (Rect r in new[] { new Rect(25, 48, 3, 3), new Rect(104, 34, 3, 3) })
                Fill(r, (x, y) => new("amber", 0, "amber", Zone.Geology));
            foreach (Rect r in new[] { new Rect(28, 48, 3, 3), new Rect(99, 34, 3, 3) })
                Fill(r, (x, y) => p[x, y] with { Wall = "amber" });
            return p;
        }

        private static void Require(bool condition, string reason)
        {
            if (!condition) throw new InvalidOperationException("SHALLOW_PLAN: " + reason);
        }
        private static bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        private static bool FullGeology(Cell[,] p, Point at) => InBounds(at.X, at.Y) &&
            p[at.X, at.Y].Key != null && p[at.X, at.Y].Slope == 0 && p[at.X, at.Y].Region == Zone.Geology;

        // Call before native writes on the actual exported arrays. This validates the immutable
        // construction template, never a played/mined world or a replacement historical baseline.
        internal void ValidateExport(Cell[,] p, IReadOnlyList<Cluster> clusters)
        {
            Require(p != null && p.GetLength(0) == Width && p.GetLength(1) == Height, "DIMENSIONS");
            Require(clusters != null && clusters.Count == 8, "CLUSTER_COUNT");
            for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) {
                Cell c = p[x, y];
                Require(c.Key is null or "soil" or "grass" or "stone" or "bone" or "rib" or "cap" or "amber", "MATERIAL");
                Require(c.Wall is null or "soil" or "grass" or "stone" or "bone" or "amber", "WALL");
                Require(c.Region >= Zone.Outside && c.Region <= Zone.Connector, "REGION");
                Require(c.Slope <= 2 && (c.Slope == 0 || c.Key == "rib"), "SLOPE");
                if (x < 4 || x >= Width - 4 || y < 8 || y >= Height - 4)
                    Require(c == default, "BOUNDARY");
            }
            for (int x = Geology.X; x < Geology.Right; x++) {
                if (x >= Gullet.X && x < Gullet.Right) continue;
                Require(p[x, 12].Key == "cap" && p[x, 13].Key == "grass" && p[x, 14].Key == "soil", "CAP_TRANSITION");
            }
            int boneCount = 0, ribCount = 0;
            foreach (Rib rib in RibSpecs) {
                for (int d = 0; d < rib.Length; d++) for (int h = 0; h < RibThickness(rib, d); h++) {
                    Cell c = p[rib.Root.X + rib.Direction * d, rib.Root.Y + RibTop(rib, d) + h];
                    Require(c.Key == (d < 2 ? "bone" : "rib") && c.Slope == RibSlope(rib, d, h), "RIB_OR_ROOT");
                    if (d < 2) boneCount++; else ribCount++;
                }
                for (int h = 0; h < 4; h++)
                    Require(FullGeology(p, new(rib.Root.X - rib.Direction, rib.Root.Y + h)), "ROOT_BACKING");
            }
            int actualBone = 0, actualRib = 0, amber = 0;
            foreach (Cell c in p) { if (c.Key == "bone") actualBone++; if (c.Key == "rib") actualRib++; if (c.Key == "amber") amber++; }
            Require(actualBone == boneCount && actualRib == ribCount, "EXTRA_RIB_OR_ROOT");
            Require(amber == 18, "SPARSE_AMBER");

            var hazards = new bool[Width, Height];
            int variants = 0;
            foreach (Cluster cluster in clusters) {
                Require(cluster.Variant >= 0 && cluster.Variant < 8, "CLUSTER_VARIANT");
                Require((variants & (1 << cluster.Variant)) == 0, "CLUSTER_DUPLICATE");
                variants |= 1 << cluster.Variant;
                for (int dx = 0; dx < 4; dx++) for (int dy = 0; dy < 4; dy++) {
                    int x = cluster.Root.X + dx, y = cluster.Root.Y + dy;
                    Require(InBounds(x, y), "CLUSTER_BOUNDS");
                    Require(p[x, y].Key == null && p[x, y].Region is Zone.Gullet or Zone.Pocket && !hazards[x, y], "CLUSTER_FOOTPRINT");
                    hazards[x, y] = true;
                }
                // Four full supports plus a second geological backing layer. No fabricated air pad.
                for (int cell = 0; cell < 4; cell++) for (int depth = 1; depth <= 2; depth++)
                    Require(FullGeology(p, cluster.Support(cell, depth)), "CLUSTER_ANCHOR");
            }
            // Exclude entire cluster rectangles, not just guessed tooth pixels: an optional
            // hazard must not be the only route. This is spatial connectivity, not movement AI.
            bool[,] reached = ReachableBodies(p, hazards, Entry);
            Require(reached[DescentEnd.X, DescentEnd.Y] && reached[PocketGoal.X, PocketGoal.Y], "BODY_PATH");
            for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++)
                Require(p[x, y] == cells[x, y], "CELL_LAYOUT");
            for (int n = 0; n < ClusterSpecs.Length; n++) Require(clusters[n] == ClusterSpecs[n], "CLUSTER_LAYOUT");
        }

        private static bool[,] ReachableBodies(Cell[,] p, bool[,] hazards, Point start)
        {
            const int columns = (BodyWidth + TilePixels - 1) / TilePixels;
            const int rows = (BodyHeight + TilePixels - 1) / TilePixels;
            bool Fits(int x, int y)
            {
                if (x < 0 || y < 0 || x + columns > Width || y + rows > Height) return false;
                for (int dx = 0; dx < columns; dx++) for (int dy = 0; dy < rows; dy++)
                    if (p[x + dx, y + dy].Key != null || p[x + dx, y + dy].Region == Zone.Outside || hazards[x + dx, y + dy]) return false;
                return true;
            }
            var seen = new bool[Width, Height];
            var queue = new Queue<Point>();
            if (Fits(start.X, start.Y)) { queue.Enqueue(start); seen[start.X, start.Y] = true; }
            void Visit(int x, int y)
            {
                if (!Fits(x, y) || seen[x, y]) return;
                seen[x, y] = true; queue.Enqueue(new(x, y));
            }
            while (queue.Count != 0) {
                Point at = queue.Dequeue();
                Visit(at.X - 1, at.Y); Visit(at.X + 1, at.Y); Visit(at.X, at.Y - 1); Visit(at.X, at.Y + 1);
            }
            return seen;
        }
    }
}
