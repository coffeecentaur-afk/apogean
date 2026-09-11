using System;

namespace apogean.Content.Diagnostics
{
    // Pure, replayable layout used by BOTH placement and verification. No game
    // globals, random calls, framing, drawing or world writes live here.
    internal static class MawNaturalLayout
    {
        internal const int Width = 128, Height = 104;
        internal sealed class Cell
        {
            internal string Tile, Wall;
            internal byte Slope;
            internal bool Half;
        }
        internal static Cell[,] Create()
        {
            var cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) cells[x, y] = new Cell();
            void Set(int x, int y, string type) {
                if (cells[x, y].Tile != null) throw new InvalidOperationException("Duplicate planned tile");
                cells[x, y].Tile = type;
            }
            for (int x = 4; x < 124; x++) {
                int surface = 12 + (int)(3 * Math.Sin(x * .12)) + Math.Abs(x - 64) / 28;
                for (int y = surface; y < 49; y++) {
                    int center = 65 + (int)(5 * Math.Sin(y * .16));
                    int opening = 13 + (y > 30 ? (y - 30) / 3 : 0);
                    if (Math.Abs(x - center) < opening) {
                        if (Math.Abs(x - center) > opening - 5) cells[x, y].Wall = y < 28 ? "soil" : "stone";
                        continue;
                    }
                    string key = y == surface ? "grass" : y < surface + 8 ? "soil" : "stone";
                    if (y > surface + 2 && x > 8 && x < 29 && y > 23 && y < 35) key = y > 30 ? "mud" : "clay";
                    if (x > 101 && y > surface + 3 && y < 31) key = x < 114 ? "snow" : "ice";
                    if (x > 28 && x < 41 && y > 33) key = "sand";
                    if (y > 24 && Math.Abs(x - center) < opening + 4) key = y % 13 < 8 ? "bone" : "fibers";
                    if (x > 82 && x < 96 && y > 33 && y < 43) key = "membrane";
                    if ((x - 24) * (x - 24) + (y - 39) * (y - 39) < 12 ||
                        (x - 100) * (x - 100) + (y - 37) * (y - 37) < 7) key = "amber";
                    Set(x, y, key);
                }
            }
            string[] materials = { "soil", "stone", "grass", "sand", "mud", "clay", "snow", "ice", "bone", "fibers", "membrane", "amber" };
            for (int x = 8; x < 120; x++) for (int y = 52; y < 57; y++) cells[x, y].Wall = materials[Math.Min(11, (x - 8) / 10)];
            for (int x = 3; x < 125; x++) Set(x, 60, "vanilla-brick");
            for (int row = 0; row < 2; row++) {
                int top = row == 0 ? 66 : 85;
                string grass = row == 0 ? "vanilla-grass" : "grass", soil = row == 0 ? "vanilla-dirt" : "soil";
                for (int index = 0; index < 10; index++) {
                    int x = 5 + index * 12;
                    for (int dx = 0; dx < 7; dx++) for (int dy = 0; dy < 5; dy++) Set(x + dx, top + dy, dy == 0 ? grass : soil);
                    if (index < 8) cells[x + (index < 4 ? 3 : 2), top].Slope = (byte)(1 + index % 4);
                    else if (index == 8) cells[x + 3, top].Half = true;
                    else { Set(x + 2, top - 1, grass); Set(x + 3, top - 1, grass); Set(x + 4, top - 1, grass); Set(x + 3, top - 2, grass); }
                    for (int dx = 0; dx < 9; dx++) Set(x - 1 + dx, top + 8, "vanilla-brick");
                }
            }
            return cells;
        }

        // Only vanilla biology in the CONTROL row is dynamic. The Maw area and
        // all structural geometry, slope/paint/wire/liquid states stay exact.
        internal static bool IsControlBank(int x, int y) => x >= 4 && x <= 122 && y >= 63 && y < 84;
    }
}
