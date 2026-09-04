using apogean.Content.Backgrounds;
using Terraria.ModLoader;

namespace apogean
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class apogean : Mod
	{
		public override void Load()
		{
			RuinedUnderworldSky.EnsureRegistered();
		}
	}
}
