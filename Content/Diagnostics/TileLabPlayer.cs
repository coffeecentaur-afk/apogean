using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.World;

namespace apogean.Content.Diagnostics
{
	public sealed class TileLabPlayer : ModPlayer
	{
		private const string AutomaticWorldName = "Apogee Native Visual V3";
		private int _automaticBuildDelay;
		private int _captureProbeDelay;
		private Rectangle _captureProbeBounds;
		private string _captureProbeName;
		private bool _captureProbeEntities;

		public override void Initialize()
		{
			_automaticBuildDelay = -1;
			_captureProbeDelay = -1;
			_captureProbeName = "Apogean Tile Lab Capture Probe";
			_captureProbeEntities = true;
		}

		public override void OnEnterWorld()
		{
			// This existing disposable validation world is our deterministic client-render harness.
			// Delaying one second lets the player and camera finish settling before the active fixture is built.
			_automaticBuildDelay = Main.ActiveWorldFileData?.Name == AutomaticWorldName ? 60 : -1;
		}

		public override void PostUpdate()
		{
			if (_automaticBuildDelay >= 0)
			{
				if (_automaticBuildDelay-- == 0)
				{
					_automaticBuildDelay = -1;
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						try
						{
							BuildWastesTerrainPropertiesAndReport(scheduleCaptureProbe: true);
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

		internal void BuildVegetationAndReport(bool scheduleCaptureProbe)
		{
			Rectangle bounds = VegetationLabGallery.Build(Player);
			Main.NewText($"Vegetation Lab rebuilt at X {bounds.Left}-{bounds.Right - 1}, Y {bounds.Top}-{bounds.Bottom - 1}.", Color.LightGreen);
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

		private void RunCaptureProbe()
		{
			CaptureBiome biome = CaptureBiome.GetCaptureBiome(-1);
			int sceneBackground = Main.LocalPlayer.CurrentSceneEffect.surfaceBackground.value;
			int sceneWater = Main.LocalPlayer.CurrentSceneEffect.waterStyle.value;
			Mod.Logger.Info($"TILE LAB CAPTURE PROBE: scene background={sceneBackground}; scene water={sceneWater}; main water={Main.waterStyle}; capture water={biome.WaterStyle}");

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
