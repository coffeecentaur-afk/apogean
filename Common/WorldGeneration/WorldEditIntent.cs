using System;

namespace apogean.Common.WorldGeneration
{
	/// <summary>
	/// Describes why Apogee wants to alter terrain. Protected regions block intents rather than
	/// becoming a single global "nothing may happen here" switch.
	/// </summary>
	[Flags]
	public enum WorldEditIntent : ushort
	{
		None = 0,
		MawGeneration = 1 << 0,
		MawSpread = 1 << 1,
		MawOutgrowth = 1 << 2,
		CorporateStructure = 1 << 3,
		RuinStructure = 1 << 4,
		WastesConversion = 1 << 5,
		Restoration = 1 << 6,
		All = ushort.MaxValue
	}
}
