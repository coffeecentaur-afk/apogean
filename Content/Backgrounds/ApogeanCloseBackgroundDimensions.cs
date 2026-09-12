using Terraria;
using Terraria.GameContent;

namespace apogean.Content.Backgrounds
{
    internal static class ApogeanCloseBackgroundDimensions
    {
        // Only called with an Apogean-owned selector slot. Async loading can leave
        // cached dimensions at zero even once IsLoaded is true (tML 2026.07).
        // Do not block the render thread or change another mod's metadata.
        internal static int Resolve(int slot, float scale)
        {
            if (slot < 0 || slot >= TextureAssets.Background.Length ||
                slot >= Main.backgroundWidth.Length || slot >= Main.backgroundHeight.Length) return -1;
            var asset = TextureAssets.Background[slot];
            if (asset?.IsLoaded != true) return -1;
            var texture = asset.Value;
            if (!CloseBackgroundDimensions.Usable(true, texture.Width, texture.Height, scale)) return -1;
            Main.backgroundWidth[slot] = texture.Width;
            Main.backgroundHeight[slot] = texture.Height;
            return slot;
        }
    }
}
