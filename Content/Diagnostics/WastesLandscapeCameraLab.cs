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
		private int projectionChecks, projectionFailures;
		private float worstLeft, worstRight, minimumBottom;
		private float groundCameraY;
		private int lockChecks;
		private float lockError;
		private int moduleChecks, moduleFailures, moduleFrameChecks;
		private float modulePixelError;
		private int landChecks, landFailures, spaceChecks, groundPresenceChecks, partialFarChecks;
		private float flightGroundCenter, flightSpaceCenter;
		private bool SpaceFlight => scenario is "space-ascent" or "space-descent";
		private RuinedBackgroundBiome? previousLab;
		private bool Diagonal => scenario is "diagonal-left" or "diagonal-right";
		private bool Panning => scenario is "pan-left" or "pan-right" || Diagonal;
		private bool PhaseSweep => scenario is "phase-left" or "phase-right";
		private bool Sweeping => Panning || PhaseSweep;
		private const int PanDuration = 1800;
		private bool previousDay, previousEclipse, previousRain;
		private double previousTime;
		private double previousRainTime;
		private float previousRainStrength;
		private int galleryChecks, galleryFailures;
		internal int ScaleGalleryIndex => remaining <= 0 ? 0 : scenario switch
		{
			"scale-shell" => 1, "scale-depot" => 2, "scale-checkpoint" => 3, _ => 0
		};
		internal float ScaleGalleryGround { get; private set; }
		internal void Start(string requested)
		{
			if (Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3")
				throw new InvalidOperationException("Landscape camera checks require Apogee Native Visual V3 single-player.");
			if (requested == "release") { Release(); return; }
			if (requested is not ("ground" or "jump" or "wings" or "sky" or "left" or "right" or "sunset" or "night" or "rain" or "eclipse" or "pan-left" or "pan-right" or "phase-left" or "phase-right" or "diagonal-left" or "diagonal-right" or "mid-altitude" or "high-altitude" or "below-ground" or "underground" or "space-fade" or "space-edge" or "space-ascent" or "space-descent" or "scale-shell" or "scale-depot" or "scale-checkpoint"))
				throw new ArgumentOutOfRangeException(nameof(requested));
			if (requested.StartsWith("scale-", StringComparison.Ordinal) && !WastesRuinScaleGallery.Available)
				throw new InvalidOperationException("Scale study assets require an isolated QA build; no fallback art will be shown.");
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
			galleryChecks = galleryFailures = 0;
			projectionChecks = projectionFailures = 0;
			lockChecks = 0; lockError = 0;
			moduleChecks = moduleFailures = moduleFrameChecks = 0; modulePixelError = 0;
			landChecks = landFailures = spaceChecks = groundPresenceChecks = partialFarChecks = 0;
			worstLeft = worstRight = 0;
			minimumBottom = float.PositiveInfinity;
			drawnMin = float.PositiveInfinity; drawnMax = float.NegativeInfinity;
			RuinedBackgroundSelectionSystem.Instance.ToggleSurfaceConceptRenderLab(RuinedBackgroundBiome.Forest, true);
			float shift = requested == "left" ? -5120 : requested == "right" ? 5120 : 0;
			float groundY = WastesGroundProfileSystem.TryGroundAt(Main.maxTilesX * 8f + shift, out float regionalY)
				? regionalY : (float)((Main.worldSurface - 50) * 16);
			// Reduce terrain occlusion, without carving the world. Some mountains
			// still obscure this route; a phase sweep isolates the texture joins.
			float lift = Panning ? 2400 : requested == "jump" ? 96 : requested == "wings" ? 1200 : 0;
			float ascent = groundY - (float)(Main.worldSurface * 16 * .35);
			if (requested == "mid-altitude") lift = ascent / 3;
			if (requested == "high-altitude") lift = ascent * .8f;
			if (requested == "below-ground") lift = -400;
			if (requested == "underground") lift = -1600;
			camera = new Vector2(Main.maxTilesX * 8f - Main.screenWidth / 2f + shift,
				requested == "sky" ? 16 : groundY - Main.screenHeight * .55f - lift);
			groundCameraY = groundY - Main.screenHeight * .55f;
			if (ScaleGalleryIndex > 0)
			{
				// Read actual terrain; never clear or rebuild tiles for a scale view.
				int x = Main.maxTilesX / 2;
				int y = Math.Max(20, (int)Main.worldSurface - 180);
				while (y < Main.maxTilesY - 200 && !WorldGen.SolidTile(x, y)) y++;
				ScaleGalleryGround = y * 16;
				camera.Y = ScaleGalleryGround - Main.screenHeight * .62f;
			}
			flightGroundCenter = (float)((Main.worldSurface - 50) * 16);
			flightSpaceCenter = WastesCameraProjection.SpaceBoundaryY(Main.worldSurface);
			if (requested == "space-fade") camera.Y = MathHelper.Lerp(flightGroundCenter, flightSpaceCenter, .85f) - Main.screenHeight * .5f;
			if (requested == "space-edge") camera.Y = flightSpaceCenter - 1 - Main.screenHeight * .5f;
			if (Sweeping)
			{
				float left = 1600, right = Main.maxTilesX * 16f - Main.screenWidth - 1600;
				bool rightward = requested is "pan-right" or "phase-right" or "diagonal-right";
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

		internal void ObserveScaleGallery(int side, string asset, Vector2 expected, Vector2 actual,
			int textureWidth, int textureHeight, int width, int height)
		{
			if (ScaleGalleryIndex == 0) return;
			bool failed = Vector2.Distance(expected, actual) > 1.1f || textureWidth != 512 || textureHeight != 460;
			if (failed) galleryFailures++;
			if (galleryChecks++ < 2)
				Mod.Logger.Info($"WASTES SCALE SAMPLE: case={scenario}; side={side}; asset={asset}; viewport={width}x{height}; texture={textureWidth}x{textureHeight}; pixel={actual}; soil={actual.Y + 340}; gameZoom={Main.GameViewMatrix.Zoom}; player={Player.width}x{Player.height}; failed={failed}; scope=ground-scale-only; production=False");
		}

		internal void ObserveDraw(float actualX)
		{
			if (remaining <= 0 || !Sweeping) return;
			drawnFrames++;
			drawnMin = Math.Min(drawnMin, actualX); drawnMax = Math.Max(drawnMax, actualX);
		}

		// Observe the actual color submitted to the renderer, not just a forecast.
		// Geometry continues to be checked independently even when land is invisible.
		internal void ObserveLandOpacity(int layer, float cameraCenter, float factor, byte alpha,
			float styleOpacity, int width, int height)
		{
			if (remaining <= 0) return;
			landChecks++;
			bool inSpace = (int)(Player.Center.Y / 16f) <= Main.worldSurface * (double).35f;
			bool cameraSpace = (int)(cameraCenter / 16f) <= Main.worldSurface * (double).35f;
			bool failed = factor < 0 || factor > 1;
			if (inSpace || cameraSpace)
			{
				spaceChecks++;
				failed |= alpha != 0 || factor != 0;
			}
			float ground = (float)((Main.worldSurface - 50) * 16);
			if (cameraCenter >= ground && Player.Center.Y >= ground && styleOpacity >= .99f)
			{
				groundPresenceChecks++;
				failed |= factor != 1 || alpha < 250;
			}
			if (layer == 0 && factor > .05f && factor < .95f && alpha > 0 && styleOpacity >= .99f) partialFarChecks++;
			if (failed) landFailures++;
			if (landChecks <= 3 || (SpaceFlight && layer == 0 && panTick % 60 == 0) || (failed && landFailures == 1))
				Mod.Logger.Info($"WASTES LAND SAMPLE: case={scenario}; layer={layer}; viewport={width}x{height}; cameraCenter={cameraCenter:F2}; playerCenter={Player.Center.Y:F2}; space={inSpace}; cameraSpace={cameraSpace}; factor={factor:F5}; submittedAlpha={alpha}; styleOpacity={styleOpacity:F3}; failed={failed}");
		}

		// Permanent QA probe: project the *submitted* geometry through the engine's
		// actual batch matrix. Unlike a sprite-size test, this catches shifted edges.
		internal void ObserveProjection(int layer, Vector2 first, Vector2 end, Matrix matrix, int width, int height)
		{
			if (remaining <= 0) return;
			Vector2 a = Vector2.Transform(first, matrix), b = Vector2.Transform(end, matrix);
			float left = Math.Min(a.X, b.X), right = Math.Max(a.X, b.X);
			projectionChecks++;
			worstLeft = Math.Max(worstLeft, left);
			worstRight = Math.Max(worstRight, width - right);
			// Far owns coverage when altitude has faded Mid out. All surface layers
			// use native-size opaque lower strata until the engine depth handoff.
			if (layer == 0) minimumBottom = Math.Min(minimumBottom, Math.Max(a.Y, b.Y) - height);
			bool failed = left > 1 || right < width - 1 || (layer == 0 && Math.Max(a.Y, b.Y) < height - 1);
			if (failed) projectionFailures++;
			if (projectionChecks == 1 || (failed && projectionFailures == 1))
				Mod.Logger.Info($"WASTES V1 PROJECTION SAMPLE: case={scenario}; layer={layer}; viewport={width}x{height}; camera={Main.screenPosition}; zoom={Main.BackgroundViewMatrix.Zoom}; zoomTranslation={Main.BackgroundViewMatrix.ZoomMatrix.Translation}; batch={matrix}; left={left:F2}; right={right:F2}; bottom={Math.Max(a.Y,b.Y):F2}; opacityOwnsFade=True; failed={failed}");
		}

		internal void ObserveGroundLock(float top, float worldCameraY, int height, float? regionalGround = null)
		{
			if (remaining <= 0) return;
			float worldGround = regionalGround ?? (float)((Main.worldSurface - 50) * 16);
			// Independent engine projection of the world-space soil datum.
			Vector2 expected = Vector2.Transform(new Vector2(0, worldGround - 48 - worldCameraY), Main.GameViewMatrix.ZoomMatrix);
			float soil = (float)Math.Floor(top) + WastesCameraProjection.CloseSoilRow;
			lockError = Math.Max(lockError, Math.Abs(expected.Y - soil));
			if (lockChecks++ == 0)
				Mod.Logger.Info($"WASTES V1 GROUND SAMPLE: case={scenario}; viewport={Main.instance.GraphicsDevice.Viewport.Width}x{height}; referenceY={worldGround}; soilY={soil:F2}; expectedY={expected.Y:F2}; gameZoom={Main.GameViewMatrix.Zoom.Y}; closeTop={top:F2}; cameraY={worldCameraY:F2}; altitude={WastesCameraProjection.Altitude(Main.worldSurface, worldCameraY + height * .5f):F3}; midOpacity={WastesCameraProjection.MiddleOpacity(WastesCameraProjection.Altitude(Main.worldSurface, worldCameraY + height * .5f)):F3}; datum={(regionalGround.HasValue ? "savedRegionalTerrainQA" : "worldSurfaceMinus50")}; artApproval=False");
		}

		internal void ObserveModularProjection(int layer, Vector2 submitted, Vector2 expected, int depth,
			Matrix matrix, int width, int height)
		{
			if (remaining <= 0) return;
			Vector2 actual = Vector2.Transform(submitted, matrix);
			float error = Math.Max(Math.Abs(actual.X - expected.X), Math.Abs(actual.Y - expected.Y));
			modulePixelError = Math.Max(modulePixelError, error);
			moduleChecks++;
			if (error > 1.1f || WastesModularLayout.BottomExposed(expected.Y, depth, height)) moduleFailures++;
		}

		internal void ObserveModularFrame(int layer, float worldX, float top, int submitted, int width, int height)
		{
			if (remaining <= 0) return;
			// Independent absolute-cell enumeration detects a dropped trailing cell.
			// An intentional valley at either screen edge is NOT a coverage failure.
			bool bank = layer == 1 && WastesLandscapeV1Renderer.HasRuinBank;
			int period = bank ? 6800 : layer == 1 ? 2655 : 2268, depth = layer == 1 ? 1408 : 1915;
			double origin = worldX * WastesParallaxContract.Horizontal(layer) - (layer == 2 ? 620 : 0);
			int expected = 0;
			if (top < height && top + depth > 0)
				for (int cell = (int)Math.Floor(origin / period) - 1; cell <= (int)Math.Floor((origin + width) / period) + 1; cell++)
					for (int group = 0; group < (bank ? 10 : layer == 1 ? 3 : 1); group++)
					{
						int offset = bank ? (group / 2 * 1360 + (group % 2 == 1 ? 760 : 0)) : layer == 1 ? WastesModularLayout.MidOffset(group) : 0;
						double x = cell * period - origin + offset;
						int span = bank ? (group % 2 == 1 ? 391 : group == 0 ? 576 : 512) : layer == 1 ? WastesModularLayout.MidWidth(group) : 1448;
						if (x < width && x + span > 0) expected++;
					}
			moduleFrameChecks++;
			if (expected != submitted) moduleFailures++;
			if (moduleFrameChecks <= 2)
				Mod.Logger.Info($"WASTES MODULAR SAMPLE: case={scenario}; layer={layer}; viewport={width}x{height}; top={top:F2}; submitted={submitted}; expected={expected}; rgbaMiB={WastesLandscapeV1Renderer.RawTextureMiB:F2}; farWidth={WastesLandscapeV1Renderer.FarRepeatWidth}; ruinBank={bank}; period={period}; scope=QA; artApproval=False");
		}

		internal void Release()
		{
			if (remaining <= 0) return;
			if (ScaleGalleryIndex > 0)
				Mod.Logger.Info($"WASTES SCALE RESULT: case={scenario}; samples={galleryChecks}; failures={galleryFailures}; artApproval=False; flightCoverage=False");
			Mod.Logger.Info($"WASTES LAND RESULT: case={scenario}; viewport={Main.screenWidth}x{Main.screenHeight}; checks={landChecks}; spaceChecks={spaceChecks}; groundChecks={groundPresenceChecks}; partialFarChecks={partialFarChecks}; failures={landFailures}; artApproval=False");
			Mod.Logger.Info($"WASTES MODULAR RESULT: case={scenario}; matrixChecks={moduleChecks}; frameChecks={moduleFrameChecks}; failures={moduleFailures}; maxPixelError={modulePixelError:F2}; artApproval=False");
			Mod.Logger.Info($"WASTES V1 PROJECTION: case={scenario}; checks={projectionChecks}; failures={projectionFailures}; maxLeftGap={worstLeft:F2}; maxRightGap={worstRight:F2}; minFarBottomMargin={minimumBottom:F2}; artApproval=False");
			Mod.Logger.Info($"WASTES V1 GROUND LOCK: case={scenario}; checks={lockChecks}; maxError={lockError:F2}; pass={lockChecks > 0 && lockError <= 1.1f}; artApproval=False");
			if (Sweeping)
			{
				double distance = drawnFrames == 0 ? 0 : drawnMax - drawnMin;
				double farRepeats = WastesParallaxContract.Repeats(distance, 0, WastesLandscapeV1Renderer.FarRepeatWidth);
				Mod.Logger.Info($"WASTES V1 SWEEP: case={scenario}; isolatedPhase={PhaseSweep}; drawnFrames={drawnFrames}; sampledTravel={distance:F1}; farRepeats={farRepeats:F3}; midRepeats={WastesParallaxContract.Repeats(distance, 1, WastesLandscapeV1Renderer.MidRepeatWidth):F3}; closeRepeats={WastesParallaxContract.Repeats(distance, 2, WastesModularLayout.ClosePeriod):F3}; coveragePass={farRepeats >= 2.5}; artApproval=False");
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
			if (SpaceFlight)
			{
				// Twenty seconds through the transition, then a ten-second endpoint.
				float progress = Math.Min(panTick++ / 1200f, 1f);
				float start = scenario == "space-ascent" ? 0 : 1.04f;
				float end = scenario == "space-ascent" ? 1.04f : 0;
				camera.Y = MathHelper.Lerp(flightGroundCenter, flightSpaceCenter,
					MathHelper.Lerp(start, end, progress)) - Main.screenHeight * .5f;
			}
			if (Sweeping)
			{
				sampledX = MathHelper.Lerp(panStart, panEnd, Math.Min(panTick++ / (float)PanDuration, 1f));
				if (Panning) camera.X = sampledX;
				if (Diagonal)
				{
					float progress = Math.Min(panTick / (float)PanDuration, 1f);
					if (WastesGroundProfileSystem.TryGroundAt(camera.X + Main.screenWidth * .5f, out float regionalY))
						groundCameraY = regionalY - Main.screenHeight * .55f;
					// Continuous ground-to-high-sky-to-ground diagonal flight. Terrain
					// may occlude artwork; submitted-geometry checks remain independent.
					camera.Y = groundCameraY - 4200 * (1 - Math.Abs(2 * progress - 1));
				}
			}
			Player.Center = camera + new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
			if (ScaleGalleryIndex > 0) Player.Bottom = new Vector2(Main.maxTilesX * 8f, ScaleGalleryGround);
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
