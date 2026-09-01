using Terraria;
using Terraria.ModLoader;
using apogean.Content.Projectiles;

namespace apogean.Content.Buffs
{
	/// <summary>Visible ownership state for the Effigy. Removing the buff dismisses every active growth.</summary>
	public sealed class MawEffigyBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<MawSentry>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
				return;
			}

			player.DelBuff(buffIndex);
			buffIndex--;
		}
	}
}
