using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Backgrounds;
using apogean.Content.Config;
using apogean.Content.World;
using apogean.Common.Biomes;

namespace apogean.Content.Diagnostics
{
	public sealed class TileLabPlayer : ModPlayer
	{
		private const string AutomaticWorldName = "Apogee Native Visual V3";
		private const string AutomaticCampusWorldName = "Apogee Campus Validation";
		private const string LiveValidationRequestFileName = "ApogeanLiveValidation.request";
		private int _automaticBuildDelay;
		private bool _automaticCampusFixture;
		private int _captureProbeDelay;
		private int _liveValidationPollDelay;
		private Rectangle _captureProbeBounds;
		private string _captureProbeName;
		private bool _captureProbeEntities;

		public override void Initialize()
		{
			_automaticBuildDelay = -1;
			_automaticCampusFixture = false;
			_captureProbeDelay = -1;
			_liveValidationPollDelay = 30;
			_captureProbeName = "Apogean Tile Lab Capture Probe";
			_captureProbeEntities = true;
		}

		public override void OnEnterWorld()
		{
			// This existing disposable validation world is our deterministic client-render harness.
			// Delaying one second lets the player and camera finish settling before the active fixture is built.
			string worldName = Main.ActiveWorldFileData?.Name;
			bool automaticWorld = worldName == AutomaticWorldName;
			_automaticCampusFixture = worldName == AutomaticCampusWorldName;
			_automaticBuildDelay = automaticWorld || _automaticCampusFixture ? 60 : -1;
			if (automaticWorld || _automaticCampusFixture)
				RuinedBackgroundSelectionSystem.Instance.ToggleForestConceptRenderLab(true);
		}

		public override void PostUpdate()
		{
			if (_liveValidationPollDelay-- <= 0)
			{
				_liveValidationPollDelay = 30;
				ConsumeLiveValidationRequest();
			}

			if (RuinedBackgroundSelectionSystem.Instance.UndergroundRenderLabBiome.HasValue ||
				RuinedBackgroundSelectionSystem.Instance.UnderworldSkyRenderLabEnabled)
				UndergroundBackgroundLabGallery.LightVisibleBackground(Player);

			if (_automaticBuildDelay >= 0)
			{
				if (_automaticBuildDelay-- == 0)
				{
					_automaticBuildDelay = -1;
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						try
						{
							if (_automaticCampusFixture)
								BuildKesslerCampusAndReport(scheduleCaptureProbe: true);
							else
								BuildVegetationAndReport(scheduleCaptureProbe: true);
						}
						catch (System.Exception exception)
						{
							Mod.Logger.Error("AUTOMATIC WASTES TERRAIN LAB BUILD FAILED", exception);
							Main.NewText("Wastes terrain lab failed to build. The world was left open; see client.log.", Color.OrangeRed);
						}
					}
				}
			}

			if (_captureProbeDelay >= 0 && _captureProbeDelay-- == 0)
			{
				_captureProbeDelay = -1;
				RunCaptureProbe();
			}
		}

		private void ConsumeLiveValidationRequest()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
				return;

			string requestPath = Path.Combine(Main.SavePath, "Captures", LiveValidationRequestFileName);
			if (!File.Exists(requestPath))
				return;

			string request = "unread";
			try
			{
				request = File.ReadAllText(requestPath).Trim().ToLowerInvariant();
				File.Delete(requestPath);
				switch (request)
				{
					case "conversion":
						BuildMawConversionAndReport(scheduleCaptureProbe: true);
						break;
					case "vegetation":
						BuildVegetationAndReport(scheduleCaptureProbe: true);
						break;
					case "wastes-terrain":
						BuildWastesTerrainAndReport(scheduleCaptureProbe: true);
						break;
					case "wastes-properties":
						BuildWastesTerrainPropertiesAndReport(scheduleCaptureProbe: true);
						break;
					case "material":
						BuildMaterialGalleryAndReport(scheduleCaptureProbe: true);
						break;
					case "grass":
						BuildGrassAndReport(scheduleCaptureProbe: true);
						break;
					case "entity-scale":
						BuildEntityScaleAndReport(scheduleCaptureProbe: true);
						break;
					case "forest-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Forest, scheduleCaptureProbe: true);
						break;
					case "forest-background-aerial":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Forest, scheduleCaptureProbe: true, aerial: true);
						break;
					case "forest-background-night":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Forest, scheduleCaptureProbe: true, SurfaceBackgroundLighting.Midnight);
						break;
					case "forest-background-eclipse":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Forest, scheduleCaptureProbe: true, SurfaceBackgroundLighting.Eclipse);
						break;
					case "desert-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Desert, scheduleCaptureProbe: true);
						break;
					case "jungle-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Jungle, scheduleCaptureProbe: true);
						break;
					case "jungle-routing":
						BuildProductionJungleRoutingAndReport(scheduleCaptureProbe: true);
						break;
					case "forest-restoration-wastes":
						BuildForestRestorationAndReport(0);
						break;
					case "forest-restoration-mixed":
						BuildForestRestorationAndReport(50);
						break;
					case "forest-restoration-green":
						BuildForestRestorationAndReport(100);
						break;
					case "snow-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Snow, scheduleCaptureProbe: true);
						break;
					case "corruption-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Corruption, scheduleCaptureProbe: true);
						break;
					case "crimson-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Crimson, scheduleCaptureProbe: true);
						break;
					case "hallow-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Hallow, scheduleCaptureProbe: true);
						break;
					case "ocean-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Ocean, scheduleCaptureProbe: true);
						break;
					case "mushroom-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Mushroom, scheduleCaptureProbe: true);
						break;
					case "underworld-background":
						BuildUndergroundBackgroundAndReport(RuinedBackgroundBiome.Underworld, scheduleCaptureProbe: true);
						break;
					case "kessler-construction":
						BuildKesslerConstructionAndReport(scheduleCaptureProbe: true);
						break;
					case "helix-construction":
						BuildHelixConstructionAndReport(scheduleCaptureProbe: true);
						break;
					case "kessler-campus":
						BuildKesslerCampusAndReport(scheduleCaptureProbe: true);
						break;
					case "kessler-world":
						InspectKesslerWorldAndReport(scheduleCaptureProbe: true);
						break;
					default:
						throw new System.InvalidOperationException($"Unknown live-validation fixture '{request}'.");
				}

				Mod.Logger.Info($"LIVE VALIDATION REQUEST CONSUMED: {request}");
			}
			catch (System.Exception exception)
			{
				Mod.Logger.Error($"LIVE VALIDATION REQUEST FAILED: {request}", exception);
				Main.NewText($"Live validation '{request}' failed. See client.log.", Color.OrangeRed);
			}
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (TileLabKeybindSystem.BuildTileLab?.JustPressed != true)
				return;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				Main.NewText("The destructive Tile Lab is single-player/server-host only.", Color.OrangeRed);
				return;
			}

			BuildAndReport(scheduleCaptureProbe: false);
		}

		private void BuildAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = TileLabGallery.Build(Player);
			Main.NewText($"Tile Lab rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}. Press F8 to reset it.", Color.LightGreen);
			if (scheduleCaptureProbe)
			{
				try
				{
					string referenceDirectory = VanillaAtlasExporter.ExportTileLabReferences();
					Mod.Logger.Info($"TILE LAB REFERENCES EXPORTED: {referenceDirectory}");
				}
				catch (System.Exception exception)
				{
					Mod.Logger.Error("TILE LAB REFERENCE EXPORT FAILED", exception);
					Main.NewText("Tile Lab built, but the optional vanilla-atlas export failed. See client.log.", Color.OrangeRed);
				}
				_captureProbeBounds = bounds;
				_captureProbeName = "Apogean Tile Lab Capture Probe";
				_captureProbeEntities = true;
				_captureProbeDelay = 180;
			}
		}

		internal void BuildGrassAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = GrassLabGallery.Build(Player);
			Main.NewText($"Grass Lab rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Grass Lab Capture Probe";
			_captureProbeEntities = true;
			_captureProbeDelay = 180;
		}

		internal void BuildEntityScaleAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = EntityScaleLabGallery.Build(Player);
			Main.NewText("Entity scale lab: vanilla bird | zombie | Mawling | Graft Hound.", Color.LightGreen);
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Entity Scale Capture Probe";
			_captureProbeEntities = true;
			_captureProbeDelay = 180;
		}

		internal void BuildMaterialGalleryAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = VisualIntegrityGallery.Build(Player, out System.Collections.Generic.IReadOnlyList<string> rows);
			Main.NewText($"Material gallery rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
			foreach (string row in rows)
				Main.NewText(row, new Color(150, 90, 170));
			if (!scheduleCaptureProbe)
				return;

			try
			{
				string referenceDirectory = VanillaAtlasExporter.ExportTileLabReferences();
				Mod.Logger.Info($"MATERIAL GALLERY REFERENCES EXPORTED: {referenceDirectory}");
			}
			catch (System.Exception exception)
			{
				Mod.Logger.Error("MATERIAL GALLERY REFERENCE EXPORT FAILED", exception);
			}

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Material Gallery Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildWastesTerrainAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = WastesTerrainFamilyGallery.Build(Player, out System.Collections.Generic.IReadOnlyList<string> labels);
			Main.NewText($"Wastes terrain lab rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
			Main.NewText(string.Join(" | ", labels), new Color(150, 90, 170));
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Wastes Terrain Family Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildWastesTerrainPropertiesAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = WastesTerrainPropertyGallery.Build(Player, out System.Collections.Generic.IReadOnlyList<string> labels);
			Main.NewText($"Wastes production property lab rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
			Main.NewText(string.Join(" | ", labels), new Color(194, 126, 44));
			Main.NewText("Runtime contracts passed: framing, native drops, sand/falling/ammo identity, ice/snow identity, and neutral spread state.", Color.LightGreen);
			if (!scheduleCaptureProbe)
				return;

			try
			{
				string referenceDirectory = VanillaAtlasExporter.ExportTileLabReferences();
				Mod.Logger.Info($"WASTES TERRAIN REFERENCES EXPORTED: {referenceDirectory}");
			}
			catch (System.Exception exception)
			{
				Mod.Logger.Error("WASTES TERRAIN REFERENCE EXPORT FAILED", exception);
			}

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Wastes Terrain Properties Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildMawConversionAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = MawConversionGallery.Build(Player,
				out System.Collections.Generic.IReadOnlyList<string> columns,
				out System.Collections.Generic.IReadOnlyList<string> stages);
			Main.NewText($"Maw conversion lab rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
			Main.NewText(string.Join(" | ", columns), new Color(194, 126, 44));
			Main.NewText(string.Join(" → ", stages), Color.LightGreen);
			Main.NewText("Runtime contracts passed: native Maw materials, drops, sand physics/ammo, and two-step purification.", Color.LightGreen);
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Maw Conversion Matrix Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildVegetationAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = VegetationLabGallery.Build(Player);
			Main.NewText($"Vegetation Lab rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
			Main.NewText("Runtime contract passed: a native mid-trunk chop removed the canopy and retained the stump.", Color.LightGreen);
			if (!scheduleCaptureProbe)
				return;

			try
			{
				string referenceDirectory = VanillaAtlasExporter.ExportTileLabReferences();
				Mod.Logger.Info($"VEGETATION LAB REFERENCES EXPORTED: {referenceDirectory}");
			}
			catch (System.Exception exception)
			{
				Mod.Logger.Error("VEGETATION LAB REFERENCE EXPORT FAILED", exception);
			}

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Vegetation Lab Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildKesslerCampusAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = KesslerCampusGallery.Build(Mod, Player);
			Main.NewText($"Kessler Campus renderer fixture rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Kessler Campus Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void InspectKesslerWorldAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = KesslerWorldGallery.Inspect(Mod, Player, out string report);
			Main.NewText("Fresh-world Kessler Campus contracts passed.", Color.LightGreen);
			Mod.Logger.Info($"KESSLER FRESH-WORLD VALIDATION: {report}");
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Kessler Fresh World Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildKesslerConstructionAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = KesslerConstructionGallery.Build(Player);
			Main.NewText($"Kessler native construction gallery rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
			Main.NewText("Gallery includes block topology, walls, glass, trim, beams, furniture, lighting, power armour, and the animated war banner.", new Color(218, 91, 43));
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Kessler Native Construction Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildHelixConstructionAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = HelixConstructionGallery.Build(Player);
			Main.NewText($"Helix native construction gallery rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
			Main.NewText("Gallery includes native-connected ceramic, trim, floors, smoked glass, walls, furniture, lighting, and animated symbiote tanks.", new Color(111, 213, 133));
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Helix Native Construction Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildSurfaceBackgroundAndReport(
			RuinedBackgroundBiome biome,
			bool scheduleCaptureProbe,
			SurfaceBackgroundLighting lighting = SurfaceBackgroundLighting.Noon,
			bool aerial = false)
		{
			if (biome is not (RuinedBackgroundBiome.Forest or RuinedBackgroundBiome.Desert or RuinedBackgroundBiome.Jungle or RuinedBackgroundBiome.Snow or RuinedBackgroundBiome.Corruption or RuinedBackgroundBiome.Crimson or RuinedBackgroundBiome.Hallow or RuinedBackgroundBiome.Ocean or RuinedBackgroundBiome.Mushroom))
				throw new System.InvalidOperationException($"{biome} has no renderer-approved diagnostic surface set.");

			RuinedBackgroundSelectionSystem.Instance.ToggleSurfaceConceptRenderLab(biome, true);
			Rectangle bounds = SurfaceBackgroundLabGallery.Build(Player, lighting, aerial);
			string altitude = aerial ? "aerial" : "ground";
			Main.NewText($"{biome} V0 {lighting} {altitude} surface-background renderer fixture rebuilt.", Color.LightGreen);
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = $"Apogean {biome} V0 {lighting} Surface Background Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildProductionJungleRoutingAndReport(bool scheduleCaptureProbe)
		{
			RuinedBackgroundSelectionSystem.Instance.DisableSurfaceConceptRenderLab();
			Rectangle bounds = SurfaceBackgroundLabGallery.BuildProductionJungleRouting(Player);
			Main.NewText("Production Jungle routing fixture rebuilt; no background override is active.", Color.LightGreen);
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = "Apogean Production Jungle Routing Capture Probe";
			_captureProbeEntities = false;
			// Scene metrics and GlobalBackgroundStyle update asynchronously.
			_captureProbeDelay = 300;
		}

		private void BuildForestRestorationAndReport(int greenPercent)
		{
			string worldName = Main.ActiveWorldFileData?.Name;
			if (Main.netMode != NetmodeID.SinglePlayer ||
				(worldName != AutomaticWorldName && worldName != AutomaticCampusWorldName))
				throw new System.InvalidOperationException("Restoration fixtures require a named disposable QA world.");

			RuinedBackgroundSelectionSystem.Instance.DisableSurfaceConceptRenderLab();
			_captureProbeBounds = SurfaceBackgroundLabGallery.BuildForestRestoration(Player, greenPercent);
			_captureProbeName = $"Apogean Forest Restoration {greenPercent} Percent Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 300;
			Main.NewText($"Local forest restoration fixture: {greenPercent}% planted green; production routing, no forced background.", Color.LightGreen);
		}

		internal void BuildUndergroundBackgroundAndReport(RuinedBackgroundBiome biome, bool scheduleCaptureProbe)
		{
			bool underworld = biome == RuinedBackgroundBiome.Underworld;
			if (underworld)
			{
				RuinedBackgroundSelectionSystem.Instance.ToggleUndergroundConceptRenderLab(biome, false);
				RuinedBackgroundSelectionSystem.Instance.ToggleUnderworldSkyRenderLab(true);
			}
			else
			{
				RuinedBackgroundSelectionSystem.Instance.ToggleUnderworldSkyRenderLab(false);
				RuinedBackgroundSelectionSystem.Instance.ToggleUndergroundConceptRenderLab(biome, true);
			}
			Rectangle bounds = UndergroundBackgroundLabGallery.Build(Player, underworld);
			string renderer = underworld ? "Underworld custom-sky" : "underground-background";
			Main.NewText($"{biome} V0 {renderer} renderer fixture rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = underworld
				? "Apogean Underworld Custom Sky Capture Probe"
				: $"Apogean {biome} V0 Underground Background Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildUndergroundBackgroundAndReport() =>
			BuildUndergroundBackgroundAndReport(RuinedBackgroundBiome.Forest, scheduleCaptureProbe: false);

		private void RunCaptureProbe()
		{
			var sceneEffect = Main.LocalPlayer.CurrentSceneEffect;
			int sceneBackground = sceneEffect.surfaceBackground.value;
			int sceneWater = sceneEffect.waterStyle.value;
			int captureBackground = sceneBackground;
			if (ModContent.GetModSurfaceBackgroundStyle(sceneBackground) == null &&
				ModContent.GetInstance<ApogeanWorldConfig>().RuinedBiomeBackgrounds)
			{
				// GlobalBackgroundStyle changes the live renderer after scene-effect
				// arbitration, so CurrentSceneEffect still contains a vanilla slot. The
				// capture camera only sees CurrentSceneEffect; resolve the same ruined
				// slot explicitly so live and panorama validation cannot disagree.
				int nativeStyle = sceneBackground >= 0 ? sceneBackground :
					ModContent.GetInstance<ForestRestorationSystem>().LastNativeSurfaceStyle;
				captureBackground = RuinedGlobalBackgroundStyle.ResolveRuinedSurfaceStyle(Main.LocalPlayer, nativeStyle);
			}

			// Some ModBiome combinations expose water style -1. CaptureBiome indexes
			// the liquid texture array directly, so always pass a validated slot.
			int captureWater = sceneWater >= 0 && sceneWater < Main.maxLiquidTypes
				? sceneWater
				: Main.waterStyle >= 0 && Main.waterStyle < Main.maxLiquidTypes ? Main.waterStyle : 0;
			CaptureBiome biome = new(captureBackground, captureWater, sceneEffect.tileColorStyle);
			RuinedBackgroundBiome detected = RuinedBackgroundSelectionSystem.DetectBiome(Main.LocalPlayer);
			ForestRestorationState restoration = ModContent.GetInstance<ForestRestorationSystem>().State;
			Mod.Logger.Info($"FOREST RESTORATION: living={restoration.LivingCount}; wastes={restoration.WastesCount}; " +
				$"fraction={restoration.LivingFraction:F3}; evidence={restoration.HasEvidence}; " +
				$"living selected={restoration.IsLivingAt(Main.LocalPlayer.Center.X / 16d)}; output={_captureProbeName}");
			Mod.Logger.Info(
				$"TILE LAB CAPTURE PROBE: scene background={sceneBackground}; capture background={captureBackground}; " +
				$"detected biome={detected}; render lab={RuinedBackgroundSelectionSystem.Instance.SurfaceRenderLabBiome?.ToString() ?? "off"}; " +
				$"zones jungle={Main.LocalPlayer.ZoneJungle}, snow={Main.LocalPlayer.ZoneSnow}, desert={Main.LocalPlayer.ZoneDesert}; " +
				$"scene water={sceneWater}; main water={Main.waterStyle}; capture water={biome.WaterStyle}");
			Mod.Logger.Info($"TILE LAB VIEWPORT: width={Main.screenWidth}; height={Main.screenHeight}; output={_captureProbeName}");

			CaptureManager.Instance.Capture(new CaptureSettings
			{
				Area = _captureProbeBounds,
				Biome = biome,
				CaptureBackground = true,
				CaptureEntities = _captureProbeEntities,
				UseScaling = true,
				OutputName = _captureProbeName
			});
			Main.NewText("Tile Lab capture-camera probe started.", Color.LightSkyBlue);
		}
	}
}
