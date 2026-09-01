using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Tiles
{
	/// <summary>A visible local source of Engraft pressure. Destroying it removes one spread source from the world.</summary>
	public sealed class MawNode : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLighted[Type] = true;
			DustType = DustID.AmberBolt;
			MineResist = 2.5f;
			MinPick = 35;
			AddMapEntry(new Color(179, 103, 26));
		}

		public override void HitWire(int i, int j) { }

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.34f;
			g = 0.16f;
			b = 0.025f;
		}
	}
}
