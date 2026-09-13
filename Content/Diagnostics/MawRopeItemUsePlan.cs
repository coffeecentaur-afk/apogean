namespace apogean.Content.Diagnostics
{
    internal static class MawRopeItemUsePlan
    {
        internal const int Side = 32, Updates = 60;
        internal static bool Context(string world, string player, bool single, bool menu) =>
            single && !menu && world == "Apogee Native Visual V3" && player == "gg";
        internal static bool Variant(string value) => value is "vanilla" or "rib" or "cap" or "wall" or "diagonal" or "none" or "far";
        internal static bool ExpectedPlacement(string value) => value is "vanilla" or "rib" or "cap" or "wall";
        internal static int TargetX(string value) => value == "far" ? 28 : 11;
        internal const int TargetY = 23;
        internal static bool Use(int tick, bool occupied) => tick == 1 && !occupied;
        internal static bool Floor(int x, int y) => y == 26 && x >= 5 && x <= 12;
        internal static bool Support(string value, int x, int y) =>
            value is not ("none" or "wall") && x == TargetX(value)-1 && y == (value == "diagonal" ? 22 : 23);
        internal static bool Wall(string value, int x, int y) => value == "wall" && x == 11 && y == 23;
    }
}
