using System;

namespace apogean.Content.Diagnostics
{
    // Synthetic render workload, never a player-input or traversal simulation.
    internal static class QACameraSweep
    {
        internal const double PeriodSeconds = 10;
        internal const float AmplitudeX = 160, AmplitudeY = 320;
        internal static (float X, float Y) Offset(double elapsed, bool moving)
        {
            if (!double.IsFinite(elapsed) || elapsed < 0) throw new ArgumentOutOfRangeException(nameof(elapsed));
            if (!moving) return (0, 0);
            double phase = Math.Min(elapsed, 32) * Math.PI * 2 / PeriodSeconds;
            return ((float)(AmplitudeX * Math.Sin(phase)), (float)(AmplitudeY * Math.Sin(phase)));
        }
    }
}
