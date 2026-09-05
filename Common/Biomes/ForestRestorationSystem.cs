using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Common.Biomes
{
	/// <summary>Uses the engine's local scene sample, not a world scan or tile mutation.</summary>
	public sealed class ForestRestorationSystem : ModSystem
	{
		public ForestRestorationState State { get; } = new ForestRestorationState();
		internal int LastNativeSurfaceStyle { get; set; }

		public override void ClearWorld()
		{
			State.Reset();
			LastNativeSurfaceStyle = 0;
		}

		public override void OnWorldLoad() => ClearWorld();
		public override void OnWorldUnload() => ClearWorld();

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
		{
			if (Main.dedServ || Main.gameMenu || Main.LocalPlayer == null || !Main.LocalPlayer.active)
				return;

			// Jungle/evil/Hallow grass and plain dirt are intentionally not forest votes.
			State.Observe(tileCounts[TileID.Grass],
				tileCounts[ModContent.TileType<WastesGrass>()],
				tileCounts[ModContent.TileType<DeadGrass>()], Main.LocalPlayer.Center.X / 16d);
		}
	}
}
