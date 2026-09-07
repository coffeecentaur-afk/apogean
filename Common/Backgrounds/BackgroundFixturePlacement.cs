using System;

namespace apogean.Common.Backgrounds
{
    // Pure placement policy for the disposable background laboratory only.
    public static class BackgroundFixturePlacement
    {
        public const int HalfEnvelope = 191; // 190-wide fixture + 96 on each side
        public const int Clearance = 8; // framing / short-lived spray margin

        public static int Center(int preferred, int worldWidth, int protectedLeft, int protectedRight)
        {
            int minimum = HalfEnvelope + 20;
            int maximum = worldWidth - HalfEnvelope - 20;
            if (maximum < minimum) throw new InvalidOperationException("World too narrow for background fixture.");
            preferred = Math.Clamp(preferred, minimum, maximum);
            if (protectedRight <= protectedLeft) return preferred;
            bool Safe(int x) => x + HalfEnvelope + Clearance <= protectedLeft ||
                                x - HalfEnvelope - Clearance >= protectedRight;
            if (Safe(preferred)) return preferred;
            int left = protectedLeft - HalfEnvelope - Clearance;
            int right = protectedRight + HalfEnvelope + Clearance;
            bool leftFits = left >= minimum && left <= maximum;
            bool rightFits = right >= minimum && right <= maximum;
            if (leftFits && (!rightFits || Math.Abs(preferred - left) <= Math.Abs(preferred - right))) return left;
            if (rightFits) return right;
            throw new InvalidOperationException("No background fixture site clear of preserved grove.");
        }
    }
}
