namespace apogean.Content.Diagnostics
{
    // Pure, deny-by-default policy shared by the native harness and its tests.
    internal static class MawShallowQaScope
    {
        internal const string Plain = "Maw QA Plain";
        internal const string World = "Apogee Native Visual V3";
        internal static bool Context(string world, string player, bool singlePlayer, bool menu) =>
            singlePlayer && !menu && world == World && player is "gg" or Plain;

        internal static bool Request(string player, string request) => player == "gg" ||
            player == Plain && request is "qa-save-and-quit" or
                "maw-shallow-test" or "maw-shallow-pristine" or "maw-shallow-audit" or "maw-shallow-reload" or
                "maw-shallow-top" or "maw-shallow-bottom" or "maw-shallow-pocket" or
                "maw-shallow-rib1" or "maw-shallow-rib2" or "maw-shallow-rib3" or
                "maw-shallow-play" or "maw-shallow-motion-entry" or "maw-shallow-motion-connector-out" or "maw-shallow-light" or
                "maw-shallow-light-awake" or "maw-shallow-light-dormant" or "maw-shallow-light-natural" or
                "maw-shallow-light-awake-bright" or "maw-shallow-light-dormant-bright" or
                "maw-shallow-capture" or "maw-shallow-release";

        internal static bool PreviewApplies(bool context, bool held, bool visiting, int x, int y,
            int left, int top, int width, int height) => context && held && visiting &&
            width > 0 && height > 0 && x >= left && y >= top && x < left + width && y < top + height;

        // One provisional light-only comparison, never a global brightness setting.
        internal static float LightScale(bool previewApplies, bool bright) => previewApplies && bright ? 1.6f : 1f;
    }
}
