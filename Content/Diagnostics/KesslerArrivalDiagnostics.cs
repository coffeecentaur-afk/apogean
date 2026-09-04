using System;
using apogean.Content.Factions;

namespace apogean.Content.Diagnostics
{
	internal static class KesslerArrivalDiagnostics
	{
		internal static string ValidateClock()
		{
			KesslerArrivalState state = new(KesslerArrivalStage.Dormant, false);
			state = state.Observe(hardMode: false, dayTime: true);
			Require(state.Stage == KesslerArrivalStage.Dormant, "pre-hardmode must remain dormant");

			state = state.Observe(hardMode: true, dayTime: true);
			Require(state.Stage == KesslerArrivalStage.ImpactSignaled, "hardmode must signal impact first");
			state = state.Observe(hardMode: true, dayTime: true);
			Require(state.Stage == KesslerArrivalStage.AwaitingDawn && !state.SawNight, "daytime impact must await a night");
			state = state.Observe(hardMode: true, dayTime: true);
			Require(state.Stage == KesslerArrivalStage.AwaitingDawn, "same-day dawn cannot start assessment");
			state = state.Observe(hardMode: true, dayTime: false);
			Require(state.Stage == KesslerArrivalStage.AwaitingDawn && state.SawNight, "night observation must be saved");
			state = state.Observe(hardMode: true, dayTime: true);
			Require(state.Stage == KesslerArrivalStage.AssessmentActive, "first dawn after night must start assessment");

			KesslerArrivalState nightClear = new KesslerArrivalState(KesslerArrivalStage.Dormant, false)
				.Observe(hardMode: true, dayTime: false)
				.Observe(hardMode: true, dayTime: false);
			Require(nightClear.Stage == KesslerArrivalStage.AwaitingDawn && nightClear.SawNight, "night clear must arm next dawn");
			Require(nightClear.Observe(hardMode: true, dayTime: true).Stage == KesslerArrivalStage.AssessmentActive,
				"night clear must start at the next dawn");

			return "day-clear and night-clear transition tables pass";
		}

		private static void Require(bool condition, string failure)
		{
			if (!condition)
				throw new InvalidOperationException("Kessler arrival clock failed: " + failure + ".");
		}
	}
}
