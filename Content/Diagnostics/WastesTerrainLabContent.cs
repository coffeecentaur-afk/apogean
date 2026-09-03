using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
	public abstract class WastesTerrainCandidateTile : ModTile
	{
		protected abstract Color MapColor { get; }

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			DustType = DustID.Dirt;
			HitSound = SoundID.Dig;
			AddMapEntry(MapColor);
		}
	}

	public sealed class WastesStoneCandidate : WastesTerrainCandidateTile { public override string Texture => "apogean/Content/Tiles/Diagnostics/WastesStoneCandidate"; protected override Color MapColor => new(91, 84, 74); }
	public sealed class WastesSandCandidate : WastesTerrainCandidateTile { public override string Texture => "apogean/Content/Tiles/Diagnostics/WastesSandCandidate"; protected override Color MapColor => new(166, 132, 79); }
	public sealed class WastesIceCandidate : WastesTerrainCandidateTile { public override string Texture => "apogean/Content/Tiles/Diagnostics/WastesIceCandidate"; protected override Color MapColor => new(109, 126, 132); }
	public sealed class WastesSnowCandidate : WastesTerrainCandidateTile { public override string Texture => "apogean/Content/Tiles/Diagnostics/WastesSnowCandidate"; protected override Color MapColor => new(176, 174, 161); }
	public sealed class WastesMudCandidate : WastesTerrainCandidateTile { public override string Texture => "apogean/Content/Tiles/Diagnostics/WastesMudCandidate"; protected override Color MapColor => new(78, 62, 42); }

	public abstract class WastesTerrainCandidateWall : ModWall
	{
		protected abstract Color MapColor { get; }

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			DustType = DustID.Dirt;
			AddMapEntry(MapColor);
		}
	}

	public sealed class WastesStoneWallCandidate : WastesTerrainCandidateWall { public override string Texture => "apogean/Content/Walls/Diagnostics/WastesStoneWallCandidate"; protected override Color MapColor => new(67, 63, 55); }
	public sealed class WastesSandWallCandidate : WastesTerrainCandidateWall { public override string Texture => "apogean/Content/Walls/Diagnostics/WastesSandWallCandidate"; protected override Color MapColor => new(113, 83, 46); }
	public sealed class WastesIceWallCandidate : WastesTerrainCandidateWall { public override string Texture => "apogean/Content/Walls/Diagnostics/WastesIceWallCandidate"; protected override Color MapColor => new(64, 76, 80); }
	public sealed class WastesSnowWallCandidate : WastesTerrainCandidateWall { public override string Texture => "apogean/Content/Walls/Diagnostics/WastesSnowWallCandidate"; protected override Color MapColor => new(138, 140, 132); }
	public sealed class WastesMudWallCandidate : WastesTerrainCandidateWall { public override string Texture => "apogean/Content/Walls/Diagnostics/WastesMudWallCandidate"; protected override Color MapColor => new(67, 51, 35); }
}
