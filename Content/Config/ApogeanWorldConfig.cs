using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace apogean.Content.Config
{
	public sealed class ApogeanWorldConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[DefaultValue(true)]
		public bool RuinedSurface { get; set; } = true;

		[DefaultValue(true)]
		public bool RuinedBiomeBackgrounds { get; set; } = true;
	}
}
