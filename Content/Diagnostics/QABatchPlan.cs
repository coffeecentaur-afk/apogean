using System;
using System.Collections.Generic;

namespace apogean.Content.Diagnostics
{
    // Hard bounds for synchronous native throughput probes, not a game scheduler.
    internal static class QABatchPlan
    {
        internal readonly record struct Batch(int Start, int Count);
        internal static List<Batch> Create(int total, int budget)
        {
            if (total < 0 || total > 1024) throw new ArgumentOutOfRangeException(nameof(total));
            if (budget < 1 || budget > 32) throw new ArgumentOutOfRangeException(nameof(budget));
            var result = new List<Batch>();
            for (int start=0; start<total; start+=budget) result.Add(new(start,Math.Min(budget,total-start)));
            return result;
        }
    }
}
