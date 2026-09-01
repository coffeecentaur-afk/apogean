using System;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Common.Biomes
{
	/// <summary>One scene-metric source for every player-facing Maw biome check.</summary>
	public sealed class MawTileCountSystem : ModSystem
	{
		public const int BiomeActivationCount = 160;

		public int MawTileCount { get; private set; }

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
		{
				MawTileCount =
				tileCounts[ModContent.TileType<EngraftTurf>()] +
				tileCounts[ModContent.TileType<Mawstone>()] +
				tileCounts[ModContent.TileType<OssuaryBone>()] +
				tileCounts[ModContent.TileType<MawAcidPool>()];
		}
	}
}
