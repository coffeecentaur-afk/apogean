using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Backgrounds;

namespace apogean.Content.Diagnostics
{
	// A bounded live-camera fixture, not an offline reconstruction of the renderer.
	// No terrain changes. Never active outside the explicitly disposable SP world.
	public sealed class WastesLandscapeCameraLab : ModPlayer
	{
		private int remaining;
		private Vector2 camera;
		private Vector2 returnPosition;
		private string scenario;
		private bool previousDay, previousEclipse, previousRain;
		private double previousTime;
		private double previousRainTime;
		private float previousRainStrength;
		internal void Start(string requested)
		{
			if (Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3")
				throw new InvalidOperationException("Landscape camera checks require Apogee Native Visual V3 single-player.");
			if (requested == "release") { Release(); return; }
			if (requested is not ("ground" or "jump" or "wings" or "sky" or "left" or "right" or "sunset" or "night" or "rain" or "eclipse"))
				throw new ArgumentOutOfRangeException(nameof(requested));
			if (remaining == 0)
			{
				returnPosition = Player.position;
				previousDay = Main.dayTime; previousTime = Main.time; previousEclipse = Main.eclipse;
				previousRain = Main.raining; previousRainTime = Main.rainTime; previousRainStrength = Main.maxRaining;
			}
			scenario = requested;
			remaining = 1800;
			RuinedBackgroundSelectionSystem.Instance.ToggleSurfaceConceptRenderLab(RuinedBackgroundBiome.Forest, true);
			float groundY = (float)((Main.worldSurface - 50) * 16);
			float lift = requested == "jump" ? 96 : requested == "wings" ? 1200 : 0;
			float shift = requested == "left" ? -5120 : requested == "right" ? 5120 : 0;
			camera = new Vector2(Main.maxTilesX * 8f - Main.screenWidth / 2f + shift,
				requested == "sky" ? 16 : groundY - Main.screenHeight * .55f - lift);
			Mod.Logger.Info($"WASTES V1 CAMERA: case={scenario}; x={camera.X}; y={camera.Y}; viewport={Main.screenWidth}x{Main.screenHeight}; hold=1800 ticks; production routing=False");
		}

		private void Release()
		{
			if (remaining > 0)
			{
				Player.position = returnPosition;
				Main.dayTime = previousDay; Main.time = previousTime; Main.eclipse = previousEclipse;
				Main.raining = previousRain; Main.rainTime = previousRainTime; Main.maxRaining = previousRainStrength;
			}
			remaining = 0;
			Player.velocity = Vector2.Zero;
			RuinedBackgroundSelectionSystem.Instance.DisableSurfaceConceptRenderLab();
		}

		public override void PostUpdate()
		{
			if (remaining <= 0) return;
			if (Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3") { remaining = 0; return; }
			if (remaining == 1) { Release(); return; }
			remaining--;
			Player.Center = camera + new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
			Player.velocity = Vector2.Zero;
			Player.fallStart = (int)(Player.position.Y / 16f);
			Player.immune = true;
			Player.immuneNoBlink = true;
			Player.immuneTime = 60;
			Main.dayTime = scenario != "night";
			Main.time = scenario == "night" ? 16200d : scenario == "sunset" ? 52500d : 27000d;
			Main.eclipse = scenario == "eclipse";
			Main.raining = scenario == "rain";
			Main.rainTime = Main.raining ? 3600 : 0;
			Main.maxRaining = Main.raining ? .85f : 0;
		}

		public override void ModifyScreenPosition()
		{
			if (remaining > 0) Main.screenPosition = camera;
		}
	}
}
