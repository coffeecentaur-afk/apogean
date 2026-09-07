using System;

namespace apogean.Common.Backgrounds
{
    // Optional QA bank. Five distinct landmarks separated by a quiet hill and
    // two open intervals. No per-frame randomness, mirroring, or joined Mid fill.
    public static class WastesRuinLayout
    {
        // Wider intervals, not smaller art:32% fewer landmarks than the
        // original6,800px sequence. Preserve independent Mid/Close parallax.
        public const int Period = 10000;
        public const int Count = 10;
        public static int Offset(int group)
        {
            if (group < 0 || group >= Count) throw new ArgumentOutOfRangeException(nameof(group));
            return group / 2 * 2000 + (group % 2 == 0 ? 0 : 1150);
        }
        // Array: Highway, Quiet, Station, MotorDepot, BrokenShell, Checkpoint.
        public static int Asset(int group)
        {
            if (group < 0 || group >= Count) throw new ArgumentOutOfRangeException(nameof(group));
            return group % 2 == 1 ? 1 : group == 0 ? 0 : group / 2 + 1;
        }
        public static int Width(int group) => Asset(group) switch { 0 => 576, 1 => 391, _ => 512 };
        public static float Phase(double worldX)
        {
            double phase = worldX * WastesParallaxContract.Horizontal(1) % Period;
            return (float)(phase < 0 ? phase + Period : phase);
        }
    }
}
