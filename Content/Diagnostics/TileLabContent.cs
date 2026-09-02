using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
	/// <summary>
	/// A deliberately stock-shaped control tile used to validate tModLoader framing before
	/// custom Apogean terrain art is allowed into world generation.
	/// </summary>
	public sealed class TileLabBlock : ModTile
	{
		public override string Texture => "apogean/Content/Tiles/Diagnostics/TileLabBlock";

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			DustType = DustID.Stone;
			HitSound = SoundID.Tink;
			AddMapEntry(new Color(200, 200, 200), CreateMapEntryName());
		}
	}

	/// <summary>A stock-shaped control wall paired with <see cref="TileLabBlock"/>.</summary>
	public sealed class TileLabWall : ModWall
	{
		public override string Texture => "apogean/Content/Walls/Diagnostics/TileLabWall";

		public override void SetStaticDefaults()
		{
			DustType = DustID.Stone;
			AddMapEntry(new Color(125, 125, 135), CreateMapEntryName());
		}
	}

	/// <summary>
	/// A Wastes material candidate kept out of world generation until it passes the
	/// Tile Lab's framing, merge, slope, liquid, and dense-field checks.
	/// </summary>
	public sealed class WastesSoilCandidate : ModTile
	{
		public override string Texture => "apogean/Content/Tiles/Diagnostics/WastesSoilCandidate";

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			DustType = DustID.Dirt;
			HitSound = SoundID.Dig;
			MineResist = 0.65f;
			AddMapEntry(new Color(101, 74, 48), CreateMapEntryName());
		}
	}

	/// <summary>A natural-wall candidate paired with <see cref="WastesSoilCandidate"/>.</summary>
	public sealed class WastesDirtWallCandidate : ModWall
	{
		public override string Texture => "apogean/Content/Walls/Diagnostics/WastesDirtWallCandidate";

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			DustType = DustID.Dirt;
			AddMapEntry(new Color(67, 49, 33), CreateMapEntryName());
		}
	}

	public sealed class TileLabKeybindSystem : ModSystem
	{
		internal static ModKeybind BuildTileLab { get; private set; }

		public override void Load()
		{
			BuildTileLab = KeybindLoader.RegisterKeybind(Mod, "Build Tile Lab", "F8");
		}

		public override void Unload()
		{
			BuildTileLab = null;
		}
	}
}
