using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.WorldGeneration;
using apogean.Content.Backgrounds;

namespace apogean.Content.Diagnostics
{
	// Read-only verification / view of a NEW, separately named world. No terrain generation on entry.
	public sealed class ArrivalSiteLab : ModPlayer
	{
		private int poll;
		public override void OnEnterWorld() {
			if (Main.ActiveWorldFileData?.Name?.StartsWith("Apogee Arrival QA ", StringComparison.Ordinal) == true && Player.name == "gg")
				Mod.Logger.Info($"ARRIVAL SITE LOAD: outcome={ArrivalSiteSystem.Instance.Outcome}; no-generation-on-entry=True");
		}
		public override void PostUpdate()
		{
			if (Main.netMode != NetmodeID.SinglePlayer || Player.whoAmI != Main.myPlayer || Player.name != "gg" ||
				Main.ActiveWorldFileData?.Name?.StartsWith("Apogee Arrival QA ", StringComparison.Ordinal) != true || poll-- > 0) return;
			poll = 30;
			string path = Path.Combine(Main.SavePath, "Captures", "ApogeanArrivalSite.request");
			if (!File.Exists(path)) return;
			string request = File.ReadAllText(path).Trim();
			File.Delete(path);
			if (request == "save-quit") { WorldGen.SaveAndQuit(); return; }
			if (request != "view") { Mod.Logger.Error("ARRIVAL SITE unknown QA request"); return; }
			ArrivalSiteSystem site = ArrivalSiteSystem.Instance;
			if (site.Outcome is not ("placed" or "fallback")) { Mod.Logger.Error("ARRIVAL SITE cannot view unplaced scene"); return; }
			Point pod = site.PodTopLeft;
			bool intact = ArrivalSiteSystem.ObjectIntact(pod);
			Main.dayTime = true; Main.time = 27000; Main.raining = false; Main.eclipse = false;
			RuinedBackgroundSelectionSystem.Instance.ToggleForestConceptRenderLab(true);
			// The shoulder can be higher than the pod floor. Keep the two-tile-wide
			// character above that actual support, not embedded in the bowl's rim.
			int standX = pod.X + 7, floorY = pod.Y + 6;
			while (floorY > site.Bounds.Top + 3 &&
				(WorldGen.SolidTile(standX, floorY - 1) || WorldGen.SolidTile(standX + 1, floorY - 1))) floorY--;
			Player.Teleport(new Vector2(standX * 16, floorY * 16 - Player.height), 1);
			Player.velocity = Vector2.Zero;
			Mod.Logger.Info($"ARRIVAL SITE VIEW: world={Main.ActiveWorldFileData.Name}; outcome={site.Outcome}; bounds={site.Bounds}; pod={pod}; object30={intact}; spawn={Main.spawnTileX},{Main.spawnTileY}; native-tiles=True; background-QA-override=True");
		}
	}
}
