namespace apogean.Content.Factions
{
	/// <summary>
	/// Saved campaign state for Kessler's first post-Wall-of-Flesh contact. Explicit values keep
	/// future additions from silently changing existing world data.
	/// </summary>
	public enum KesslerArrivalStage : byte
	{
		Dormant = 0,
		ImpactSignaled = 1,
		AwaitingDawn = 2,
		AssessmentActive = 3,
		Contactable = 4
	}

	public readonly record struct KesslerArrivalState(KesslerArrivalStage Stage, bool SawNight)
	{
		/// <summary>
		/// Pure clock transition used by the world system and the regression harness. A daytime
		/// Wall-of-Flesh clear must pass through a night; a nighttime clear may use the next dawn.
		/// </summary>
		public KesslerArrivalState Observe(bool hardMode, bool dayTime)
		{
			return Stage switch
			{
				KesslerArrivalStage.Dormant when hardMode => this with
				{
					Stage = KesslerArrivalStage.ImpactSignaled
				},
				KesslerArrivalStage.ImpactSignaled => new KesslerArrivalState(
					KesslerArrivalStage.AwaitingDawn,
					SawNight || !dayTime),
				KesslerArrivalStage.AwaitingDawn when !dayTime => this with { SawNight = true },
				KesslerArrivalStage.AwaitingDawn when SawNight && dayTime => new KesslerArrivalState(
					KesslerArrivalStage.AssessmentActive,
					true),
				_ => this
			};
		}
	}
}
