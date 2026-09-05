using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.Biomes;
using apogean.Content.Backgrounds;

namespace apogean.Content.Diagnostics
{
	// Destructive fixture only in the explicitly disposable single-player world.
	// Terrain changes come from owned vanilla PureSpray AI, not invented scene counts.
	public sealed class ForestSprayVisualLab : ModSystem
	{
		private bool active, sawWaste, sawGreen;
		private int tick, column, spawned, partialFrames, mismatchedFrames, frames;
		private float lastOpacity = -1;
		private Rectangle bounds;
		private Vector2 holdPosition;
		private bool previousDay, previousEclipse, previousRain;
		private double previousTime;
		private readonly StringBuilder samples = new();
		private static bool IsQa => Main.netMode == NetmodeID.SinglePlayer && Main.ActiveWorldFileData?.Name == "Apogee Native Visual V3";

		internal void Start()
		{
			if (!IsQa) throw new InvalidOperationException("Spray check requires Apogee Native Visual V3 single-player.");
			Stop();
			previousDay = Main.dayTime; previousTime = Main.time;
			previousEclipse = Main.eclipse; previousRain = Main.raining;
			RuinedBackgroundSelectionSystem.Instance.DisableSurfaceConceptRenderLab();
			bounds = SurfaceBackgroundLabGallery.BuildForestRestoration(Main.LocalPlayer, 0);
			holdPosition = Main.LocalPlayer.position;
			tick = spawned = partialFrames = mismatchedFrames = frames = 0;
			column = bounds.Left + 2;
			sawWaste = sawGreen = false; lastOpacity = -1;
			samples.Clear().AppendLine("tick,living,wastes,restored,engineOpacity,drawOpacity");
			active = true;
			Mod.Logger.Info($"FOREST SPRAY START: bounds={bounds}; nativeProjectile=PureSpray; forcedBackground=False; duration=1400 ticks");
		}

		internal void ObserveWastesDraw(float opacity)
		{
			if (active) lastOpacity = opacity;
		}

		public override void PostUpdateWorld()
		{
			if (!active) return;
			if (!IsQa) { active = false; return; }
			tick++;
			Player player = Main.LocalPlayer;
			player.position = holdPosition; player.velocity = Vector2.Zero;
			player.fallStart = (int)(player.position.Y / 16);
			player.immune = player.immuneNoBlink = true; player.immuneTime = 60;
			Main.dayTime = true; Main.time = 27000; Main.eclipse = Main.raining = false;
			if (tick >= 300 && tick % 12 == 0 && column < bounds.Right - 2)
			{
				int floor = bounds.Bottom - 8;
				int id = Projectile.NewProjectile(player.GetSource_Misc("Apogean QA purification"),
					new Vector2(column * 16 + 8, (floor - 2) * 16 + 8), new Vector2(0, 12),
					ProjectileID.PureSpray, 0, 0, player.whoAmI);
				if (id >= 0 && id < Main.maxProjectiles)
				{
					// Eight AI calls cross the eight-row floor, then expire inside the
					// cleared isolation margin; no world-wide solution stream.
					Main.projectile[id].timeLeft = 8;
					spawned++;
				}
				column += 4;
			}
			if (tick >= 1400) Stop();
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			if (!active) return;
			ForestRestorationState state = ModContent.GetInstance<ForestRestorationSystem>().State;
			int slot = ModContent.GetInstance<ForestRuinedBackgroundStyle>().Slot;
			float engineOpacity = Main.bgAlphaFrontLayer[slot];
			if (state.HasEvidence && tick > 120)
			{
				sawWaste |= !state.UseLivingForest && state.WastesCount > 40;
				sawGreen |= state.UseLivingForest && state.LivingCount > 40;
			}
			if (engineOpacity > .001f && engineOpacity < .999f && lastOpacity >= 0)
			{
				partialFrames++;
				if (Math.Abs(lastOpacity - engineOpacity) > .001f) mismatchedFrames++;
			}
			frames++;
			samples.AppendLine(FormattableString.Invariant($"{tick},{state.LivingCount},{state.WastesCount},{state.UseLivingForest},{engineOpacity:F6},{lastOpacity:F6}"));
			lastOpacity = -1;
		}

		internal void Stop()
		{
			if (!active) return;
			active = false;
			Main.dayTime = previousDay; Main.time = previousTime;
			Main.eclipse = previousEclipse; Main.raining = previousRain;
			string directory = Path.Combine(Main.SavePath, "Captures");
			Directory.CreateDirectory(directory);
			File.WriteAllText(Path.Combine(directory, "Apogean-ForestSpray.csv"), samples.ToString());
			bool pass = tick >= 1400 && sawWaste && sawGreen && partialFrames > 0 && mismatchedFrames == 0;
			File.WriteAllText(Path.Combine(directory, "Apogean-ForestSpray.json"), JsonSerializer.Serialize(new
			{
				pass, tick, spawned, frames, sawWaste, sawGreen, partialFrames, mismatchedFrames,
				viewport = $"{Main.screenWidth}x{Main.screenHeight}",
				forcedBackground = false, projectile = "Terraria.ID.ProjectileID.PureSpray",
				manualClentaminatorInputTested = false, utc = DateTime.UtcNow
			}, new JsonSerializerOptions { WriteIndented = true }));
			Mod.Logger.Info($"FOREST SPRAY RESULT: pass={pass}; tick={tick}; spawned={spawned}; sawWaste={sawWaste}; sawGreen={sawGreen}; partialFrames={partialFrames}; mismatchedFrames={mismatchedFrames}");
		}

		public override void OnWorldUnload() { active = false; samples.Clear(); }
	}
}
