using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Backgrounds;
using apogean.Common.Backgrounds;

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
		private int panTick, drawnFrames;
		private float panStart, panEnd, sampledX, drawnMin, drawnMax;
		private RuinedBackgroundBiome? previousLab;
		private bool Panning => scenario is "pan-left" or "pan-right";
		private bool PhaseSweep => scenario is "phase-left" or "phase-right";
		private bool Sweeping => Panning || PhaseSweep;
		private const int PanDuration = 1800;
		private bool previousDay, previousEclipse, previousRain;
		private double previousTime;
		private double previousRainTime;
		private float previousRainStrength;
		internal void Start(string requested)
		{
			if (Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3")
				throw new InvalidOperationException("Landscape camera checks require Apogee Native Visual V3 single-player.");
			if (requested == "release") { Release(); return; }
			if (requested is not ("ground" or "jump" or "wings" or "sky" or "left" or "right" or "sunset" or "night" or "rain" or "eclipse" or "pan-left" or "pan-right" or "phase-left" or "phase-right"))
				throw new ArgumentOutOfRangeException(nameof(requested));
			if (remaining > 0) Release();
			if (remaining == 0)
			{
				returnPosition = Player.position;
				previousDay = Main.dayTime; previousTime = Main.time; previousEclipse = Main.eclipse;
				previousRain = Main.raining; previousRainTime = Main.rainTime; previousRainStrength = Main.maxRaining;
				previousLab = RuinedBackgroundSelectionSystem.Instance.SurfaceRenderLabBiome;
			}
			scenario = requested;
			remaining = Sweeping ? PanDuration + 600 : 1800;
			panTick = drawnFrames = 0;
			drawnMin = float.PositiveInfinity; drawnMax = float.NegativeInfinity;
			RuinedBackgroundSelectionSystem.Instance.ToggleSurfaceConceptRenderLab(RuinedBackgroundBiome.Forest, true);
			float groundY = (float)((Main.worldSurface - 50) * 16);
			// Reduce terrain occlusion, without carving the world. Some mountains
			// still obscure this route; a phase sweep isolates the texture joins.
			float lift = Panning ? 2400 : requested == "jump" ? 96 : requested == "wings" ? 1200 : 0;
			float shift = requested == "left" ? -5120 : requested == "right" ? 5120 : 0;
			camera = new Vector2(Main.maxTilesX * 8f - Main.screenWidth / 2f + shift,
				requested == "sky" ? 16 : groundY - Main.screenHeight * .55f - lift);
			if (Sweeping)
			{
				float left = 1600, right = Main.maxTilesX * 16f - Main.screenWidth - 1600;
				bool rightward = requested is "pan-right" or "phase-right";
				panStart = rightward ? left : right;
				panEnd = rightward ? right : left;
				sampledX = panStart;
				if (Panning) camera.X = panStart;
			}
			Mod.Logger.Info($"WASTES V1 CAMERA: case={scenario}; x={camera.X}; y={camera.Y}; viewport={Main.screenWidth}x{Main.screenHeight}; hold={remaining} ticks; production routing=False");
		}

		// Isolated join inspection, explicitly NOT a real-world traversal. Only
		// the horizontal input to the actual production renderer changes. It
		// stays inactive in ordinary worlds and preserves the physical camera.
		internal float BackgroundSampleX(float actualX) => remaining > 0 && PhaseSweep ? sampledX : actualX;

		internal void ObserveDraw(float actualX)
		{
			if (remaining <= 0 || !Sweeping) return;
			drawnFrames++;
			drawnMin = Math.Min(drawnMin, actualX); drawnMax = Math.Max(drawnMax, actualX);
		}

		internal void Release()
		{
			if (remaining <= 0) return;
			if (Sweeping)
			{
				double distance = drawnFrames == 0 ? 0 : drawnMax - drawnMin;
				Mod.Logger.Info($"WASTES V1 SWEEP: case={scenario}; isolatedPhase={PhaseSweep}; drawnFrames={drawnFrames}; sampledTravel={distance:F1}; farRepeats={WastesParallaxContract.Repeats(distance, 0):F3}; midRepeats={WastesParallaxContract.Repeats(distance, 1):F3}; closeRepeats={WastesParallaxContract.Repeats(distance, 2):F3}; coveragePass={WastesParallaxContract.Repeats(distance, 0) >= 2.5}; artApproval=False");
			}
			if (remaining > 0)
			{
				Player.position = returnPosition;
				Main.dayTime = previousDay; Main.time = previousTime; Main.eclipse = previousEclipse;
				Main.raining = previousRain; Main.rainTime = previousRainTime; Main.maxRaining = previousRainStrength;
			}
			remaining = 0;
			Player.velocity = Vector2.Zero;
			RuinedBackgroundSelectionSystem.Instance.DisableSurfaceConceptRenderLab();
			if (previousLab.HasValue)
				RuinedBackgroundSelectionSystem.Instance.ToggleSurfaceConceptRenderLab(previousLab.Value, true);
		}

		public override void PostUpdate()
		{
			if (remaining <= 0) return;
			if (Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3") { remaining = 0; return; }
			if (remaining == 1) { Release(); return; }
			remaining--;
			if (Sweeping)
			{
				sampledX = MathHelper.Lerp(panStart, panEnd, Math.Min(panTick++ / (float)PanDuration, 1f));
				if (Panning) camera.X = sampledX;
			}
			Player.Center = camera + new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
			Player.velocity = Vector2.Zero;
			Player.fallStart = (int)(Player.position.Y / 16f);
			Player.immune = true;
			Player.immuneNoBlink = true;
			Player.immuneTime = 60;
			Player.breath = Player.breathMax;
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
