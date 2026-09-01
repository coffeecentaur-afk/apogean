using Terraria;
using Terraria.ModLoader;
using apogean.Common.Maw;
using apogean.Content.Buffs;

namespace apogean.Content.Players
{
	public sealed class MawAcidPlayer : ModPlayer
	{
		private const int LifeRegenPenalty = 24;

		public bool InAcid { get; internal set; }

		public override void ResetEffects() => InAcid = false;

		public override void PostUpdate()
		{
			if (!MawAcidCollision.Intersects(Player.Hitbox))
				return;

			InAcid = true;
			Player.AddBuff(ModContent.BuffType<CausticDigestion>(), 12);
		}

		public override void UpdateBadLifeRegen()
		{
			if (!InAcid)
				return;

			if (Player.lifeRegen > 0)
				Player.lifeRegen = 0;
			Player.lifeRegenTime = 0;
			Player.lifeRegen -= LifeRegenPenalty;
		}
	}
}
