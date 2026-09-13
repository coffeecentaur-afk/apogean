namespace apogean.Content.Diagnostics
{
    internal static class MawRopeClimbScope
    {
        internal const string PlayerName = "Maw QA Rope";
        internal static bool Context(string world, string player, bool singlePlayer, bool menu) =>
            !menu && singlePlayer && world == "Apogee Native Visual V3" && player == PlayerName;
        internal static bool Request(string request) => request is "qa-save-and-quit" or
            "maw-rope-climb-vanilla" or "maw-rope-climb-rib" or "maw-rope-climb-none" or "maw-rope-climb-stop";
        internal static bool Variant(string variant) => variant is "vanilla" or "rib" or "none";
        internal static bool Up(int tick) => tick >= 0 && tick < 180;
        internal static bool Complete(int samples) => samples == 210;
    }
}
