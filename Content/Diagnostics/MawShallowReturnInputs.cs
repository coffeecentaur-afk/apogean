using System;

namespace apogean.Content.Diagnostics
{
    // One deliberately simple, bounded jump-only attempt in the fixed V1 connector.
    // This selects ordinary inputs, not a physics simulation or proof of impossibility.
    internal static class MawShallowReturnInputs
    {
        internal readonly record struct Input(bool Right, bool Left, bool Jump);
        internal static Input Decide(float bodyX, float bodyY, float velocityX, int tick)
        {
            if (!float.IsFinite(bodyX) || !float.IsFinite(bodyY) || !float.IsFinite(velocityX) ||
                tick < 0 || tick >= 360 || bodyX < 54 * 16 || bodyX > 90 * 16 || bodyY < 36 * 16 || bodyY > 72 * 16)
                return default;
            float target = (bodyY + 42 <= 43 * 16 ? 82 : bodyY + 42 <= 54 * 16 ? 80 : 70) * 16;
            float delta = target - (bodyX + 10);
            float stopping = Math.Max(3, velocityX * velocityX / .2f + 2);
            bool coast = delta * velocityX > 0 && Math.Abs(delta) <= stopping;
            // Six fixed24-update hold /36-update release cycles; no added jump ability.
            return new(!coast && delta > 3, !coast && delta < -3, Math.Abs(delta) <= 12 && tick % 60 < 24);
        }
    }
}
