using System;

namespace apogean.Common.Biomes
{
	/// <summary>Client scenery policy; never changes terrain, progression or world purity.</summary>
	public sealed class ForestRestorationState
	{
		public const int MinimumGrassCount = 40;
		public const double EnterLivingFraction = 0.65;
		public const double LeaveLivingFraction = 0.35;
		public const double EvidenceRadiusTiles = 120;

		public long LivingCount { get; private set; }
		public long WastesCount { get; private set; }
		public double LivingFraction { get; private set; }
		public bool HasEvidence { get; private set; }
		public bool UseLivingForest { get; private set; }
		private double evidenceTileX;

		public void Reset()
		{
			LivingCount = WastesCount = 0;
			LivingFraction = 0;
			HasEvidence = UseLivingForest = false;
			evidenceTileX = 0;
		}

		public bool IsLivingAt(double tileX) => HasEvidence && UseLivingForest &&
			Math.Abs(tileX - evidenceTileX) <= EvidenceRadiusTiles;

		public void Observe(int livingGrass, int wastesGrass, int legacyDeadGrass, double tileX)
		{
			if (double.IsNaN(tileX) || double.IsInfinity(tileX))
			{
				Reset();
				return;
			}
			if (HasEvidence && Math.Abs(tileX - evidenceTileX) > EvidenceRadiusTiles)
				Reset();

			LivingCount = Math.Max(0, livingGrass);
			WastesCount = (long)Math.Max(0, wastesGrass) + Math.Max(0, legacyDeadGrass);
			long total = LivingCount + WastesCount;
			if (total < MinimumGrassCount)
				return; // Hold the last local landscape while flying above its grass sample.

			LivingFraction = (double)LivingCount / total;
			if (!HasEvidence)
				UseLivingForest = LivingFraction >= EnterLivingFraction;
			else if (UseLivingForest)
				UseLivingForest = LivingFraction > LeaveLivingFraction;
			else
				UseLivingForest = LivingFraction >= EnterLivingFraction;
			HasEvidence = true;
			evidenceTileX = tileX;
		}
	}
}
