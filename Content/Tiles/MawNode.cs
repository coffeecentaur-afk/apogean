using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.Maw;

namespace apogean.Content.Tiles
{
	/// <summary>A visible local amplifier of Maw pressure. Destroying it reduces local activity without killing the biome.</summary>
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
			float activity = MawActivityState.IsDormant ? 0.32f : 1f;
			r = 0.34f * activity;
			g = 0.16f * activity;
			b = 0.025f * activity;
		}
	}
}
