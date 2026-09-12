using System;

namespace apogean.Content.Backgrounds
{
    // Pure arithmetic contract for tML's integer repeated-texture stride.
    internal static class CloseBackgroundDimensions
    {
        internal static bool Usable(bool loaded, int width, int height, float scale)
        {
            if (!loaded || width <= 0 || height <= 0 || !float.IsFinite(scale) || scale <= 0) return false;
            // Match the native single-precision multiplication before conversion;
            // compare in double so int.MaxValue does not round up to 2^31.
            float stride = width * (scale * 2f);
            return float.IsFinite(stride) && stride >= 1f && (double)stride <= int.MaxValue;
        }
    }
}
