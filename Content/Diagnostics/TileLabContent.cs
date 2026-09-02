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
