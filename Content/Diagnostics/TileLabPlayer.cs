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

			if (RuinedBackgroundSelectionSystem.Instance.ForestUndergroundRenderLabEnabled)
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
					case "desert-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Desert, scheduleCaptureProbe: true);
						break;
					case "jungle-background":
						BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome.Jungle, scheduleCaptureProbe: true);
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
					case "kessler-campus":
						BuildKesslerCampusAndReport(scheduleCaptureProbe: true);
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

		internal void BuildSurfaceBackgroundAndReport(RuinedBackgroundBiome biome, bool scheduleCaptureProbe)
		{
			if (biome is not (RuinedBackgroundBiome.Forest or RuinedBackgroundBiome.Desert or RuinedBackgroundBiome.Jungle or RuinedBackgroundBiome.Snow or RuinedBackgroundBiome.Corruption or RuinedBackgroundBiome.Crimson or RuinedBackgroundBiome.Hallow or RuinedBackgroundBiome.Ocean))
				throw new System.InvalidOperationException($"{biome} has no renderer-approved diagnostic surface set.");

			RuinedBackgroundSelectionSystem.Instance.ToggleSurfaceConceptRenderLab(biome, true);
			Rectangle bounds = SurfaceBackgroundLabGallery.Build(Player);
			Main.NewText($"{biome} V0 surface-background renderer fixture rebuilt.", Color.LightGreen);
			if (!scheduleCaptureProbe)
				return;

			_captureProbeBounds = bounds;
			_captureProbeName = $"Apogean {biome} V0 Surface Background Capture Probe";
			_captureProbeEntities = false;
			_captureProbeDelay = 180;
		}

		internal void BuildUndergroundBackgroundAndReport()
		{
			Rectangle bounds = UndergroundBackgroundLabGallery.Build(Player);
			Main.NewText($"Underground background lab rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
		}

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
				captureBackground = RuinedGlobalBackgroundStyle.ResolveRuinedSurfaceStyle(Main.LocalPlayer);
			}

			// Some ModBiome combinations expose water style -1. CaptureBiome indexes
			// the liquid texture array directly, so always pass a validated slot.
			int captureWater = sceneWater >= 0 && sceneWater < Main.maxLiquidTypes
				? sceneWater
				: Main.waterStyle >= 0 && Main.waterStyle < Main.maxLiquidTypes ? Main.waterStyle : 0;
			CaptureBiome biome = new(captureBackground, captureWater, sceneEffect.tileColorStyle);
			Mod.Logger.Info($"TILE LAB CAPTURE PROBE: scene background={sceneBackground}; capture background={captureBackground}; scene water={sceneWater}; main water={Main.waterStyle}; capture water={biome.WaterStyle}");

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
