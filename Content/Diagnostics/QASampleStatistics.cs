using System;

namespace apogean.Content.Diagnostics
{
    // Pure scalar math, shared by the native recorder and the standalone test.
    internal static class QASampleStatistics
    {
        internal sealed record Summary(int Count, double MeanMs, double P50Ms,
            double P95Ms, double P99Ms, double MaxMs, int Over33Ms, int Over50Ms);

        internal static Summary Describe(double[] samples, int count)
        {
            if (samples == null || count < 0 || count > samples.Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return new Summary(0, 0, 0, 0, 0, 0, 0, 0);
            double[] ordered = new double[count];
            double total = 0;
            int over33 = 0, over50 = 0;
            for (int i = 0; i < count; i++) {
                double value = samples[i];
                if (!double.IsFinite(value) || value < 0)
                    throw new ArgumentException("Timing samples must be finite and nonnegative.");
                ordered[i] = value; total += value;
                if (value > 1000.0 / 30) over33++;
                if (value > 50) over50++;
            }
            Array.Sort(ordered);
            double Rank(double p) => ordered[Math.Max(0, (int)Math.Ceiling(p * count) - 1)];
            return new Summary(count, total / count, Rank(.5), Rank(.95), Rank(.99), ordered[count - 1], over33, over50);
        }
    }
}
