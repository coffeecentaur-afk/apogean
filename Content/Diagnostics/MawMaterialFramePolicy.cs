namespace apogean.Content.Diagnostics
{
    // Stateless texture-bank selection. Never modifies saved framing, geometry,
    // generation or collision. Tests execute this exact policy without Terraria.
    internal static class MawMaterialFramePolicy
    {
        internal const int NativeWidth = 288, NativeHeight = 270, Phases = 8;
        internal const int TextureWidth = NativeWidth * Phases;
        internal const int TextureHeight = NativeHeight * Phases;

        internal static bool TryMap(int i, int j, short nativeX, short nativeY,
            out short drawX, out short drawY)
        {
            drawX = nativeX; drawY = nativeY;
            if (i < 0 || j < 0 || nativeX < 0 || nativeY < 0 ||
                nativeX + 16 > NativeWidth || nativeY + 16 > NativeHeight)
                return false;
            drawX = (short)(nativeX + i % Phases * NativeWidth);
            drawY = (short)(nativeY + j % Phases * NativeHeight);
            return true;
        }
    }
}
