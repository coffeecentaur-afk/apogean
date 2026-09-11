using System;
using System.Collections.Generic;

namespace apogean.Content.Diagnostics
{
    // Bounded QA maturation on already-Maw soil. Not clean-land infection,
    // worldgen, a timer or a new shipping spread rate. Plan first, apply second:
    // newly grown cells cannot cascade again in the same step.
    internal static class MawFiberGrowthPolicy
    {
        internal enum Host { Air, Soil, Grass, Other }
        internal readonly record struct Cell(Host Material, bool Blocked = false, bool Partial = false);
        internal readonly record struct Site(int X, int Y);
        private static readonly Site[] Directions = { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) };

        internal static List<Site> Plan(Cell[,] cells, int budget, bool enabled)
        {
            if (cells == null) throw new ArgumentNullException(nameof(cells));
            if (budget < 0 || budget > 32) throw new ArgumentOutOfRangeException(nameof(budget));
            var pending = new List<Site>();
            if (!enabled || budget == 0) return pending;
            // One-cell observation border is read-only. No hidden writes beyond
            // the supplied ownership envelope or diagonal jumps over trenches.
            for (int x = 1; x < cells.GetLength(0) - 1; x++)
                for (int y = 1; y < cells.GetLength(1) - 1; y++) {
                    Cell target = cells[x, y];
                    if (target.Material != Host.Soil || target.Blocked) continue;
                    bool exposed = target.Partial, seeded = false;
                    foreach (Site direction in Directions) {
                        Cell neighbor = cells[x + direction.X, y + direction.Y];
                        exposed |= neighbor.Material == Host.Air;
                        seeded |= neighbor.Material == Host.Grass && !neighbor.Blocked;
                    }
                    if (exposed && seeded) {
                        pending.Add(new Site(x, y));
                        if (pending.Count == budget) return pending;
                    }
                }
            return pending;
        }
    }
}
