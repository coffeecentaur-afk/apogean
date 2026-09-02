using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
	public sealed class TileLabPlayer : ModPlayer
	{
		private const string AutomaticWorldName = "Apogee Native Visual V3";
		private int _automaticBuildDelay;
		private int _captureProbeDelay;
		private Rectangle _captureProbeBounds;

		public override void Initialize()
		{
			_automaticBuildDelay = -1;
			_captureProbeDelay = -1;
		}

		public override void OnEnterWorld()
		{
			// This existing disposable validation world is our deterministic client-render harness.
			// Delaying one second lets the player and camera finish settling before the fixture is built.
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
						BuildAndReport(scheduleCaptureProbe: true);
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
				_captureProbeDelay = 180;
			}
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
				CaptureEntities = true,
				UseScaling = true,
				OutputName = "Apogean Tile Lab Capture Probe"
			});
			Main.NewText("Tile Lab capture-camera probe started.", Color.LightSkyBlue);
		}
	}
}
