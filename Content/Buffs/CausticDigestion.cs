using Terraria;
using Terraria.ModLoader;
using apogean.Content.Players;

namespace apogean.Content.Buffs
{
	public sealed class CausticDigestion : ModBuff
	{
		public override string Texture => "Terraria/Images/Buff_20";

		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
			Main.pvpBuff[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<MawAcidPlayer>().InAcid = true;
		}
	}
}
