using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace apogean.Content.Diagnostics
{
	/// <summary>Bounded weather/paint checks on the existing disposable grove, never a renderer replacement.</summary>
	public sealed class VegetationVisualLab : ModSystem
	{
		private Rectangle bounds;
		private int remaining;
		private string scenario;
		private bool previousDay, previousRain, previousEclipse;
		private double previousTime, previousRainTime;
		private float previousWind, previousTarget, previousRainStrength;
		private readonly List<(Point Position, byte Color)> paint = new();

		internal void Register(Rectangle fixture)
		{
			Release();
			bounds = fixture;
		}

		internal void ClearFixture()
		{
			Release();
			bounds = Rectangle.Empty;
		}

		internal void Start(string requested)
		{
			if (Main.netMode != NetmodeID.SinglePlayer || Main.ActiveWorldFileData?.Name != "Apogee Native Visual V3")
				throw new InvalidOperationException("Vegetation checks require Apogee Native Visual V3 single-player.");
			if (requested == "release") { Release(); return; }
			if (bounds.IsEmpty)
				throw new InvalidOperationException("Build the vegetation fixture first.");
			if (requested == "properties")
			{
				Release();
				Mod.Logger.Info("VEGETATION PROPERTIES: " + VegetationLabGallery.ValidateProperties(bounds));
				return;
			}
			if (requested is not ("day" or "night" or "wind-left" or "wind-right" or "paint"))
				throw new ArgumentOutOfRangeException(nameof(requested));
			Release();
			previousDay = Main.dayTime; previousTime = Main.time;
			previousWind = Main.windSpeedCurrent; previousTarget = Main.windSpeedTarget;
			previousRain = Main.raining; previousRainTime = Main.rainTime;
			previousRainStrength = Main.maxRaining; previousEclipse = Main.eclipse;
			scenario = requested;
			remaining = 3600;
			if (requested == "paint")
			{
				// A single tree, including its native roots and side branches; the other trees remain controls.
				for (int x = bounds.Left + 121; x <= bounds.Left + 125; x++)
					for (int y = bounds.Top; y < bounds.Bottom - 3; y++)
					{
						Tile tile = Main.tile[x, y];
						if (!tile.HasTile || tile.TileType != TileID.Trees) continue;
						paint.Add((new Point(x, y), tile.TileColor));
						WorldGen.paintTile(x, y, PaintID.DeepBluePaint);
					}
				if (paint.Count == 0) { Release(); throw new InvalidOperationException("No tree found at the paint fixture socket."); }
			}
			Mod.Logger.Info($"VEGETATION VISUAL: case={scenario}; bounds={bounds}; viewport={Main.screenWidth}x{Main.screenHeight}; hold=3600 ticks; paintedCells={paint.Count}");
		}

		internal void Release()
		{
			if (remaining <= 0) return;
			foreach (var saved in paint)
			{
				Tile tile = Main.tile[saved.Position.X, saved.Position.Y];
				if (tile.HasTile && tile.TileType == TileID.Trees)
					WorldGen.paintTile(saved.Position.X, saved.Position.Y, saved.Color);
			}
			paint.Clear();
			Main.dayTime = previousDay; Main.time = previousTime;
			Main.windSpeedCurrent = previousWind; Main.windSpeedTarget = previousTarget;
			Main.raining = previousRain; Main.rainTime = previousRainTime;
			Main.maxRaining = previousRainStrength; Main.eclipse = previousEclipse;
			remaining = 0;
			Mod.Logger.Info("VEGETATION VISUAL: released; original clock/weather/paint restored.");
		}

		public override void PostUpdateWorld()
		{
			if (remaining <= 0) return;
			if (remaining == 1) { Release(); return; }
			remaining--;
			Main.dayTime = scenario != "night";
			Main.time = Main.dayTime ? 27000d : 16200d;
			Main.windSpeedCurrent = scenario == "wind-left" ? -.8f : scenario == "wind-right" ? .8f : .2f;
			Main.windSpeedTarget = Main.windSpeedCurrent;
			Main.raining = false; Main.maxRaining = 0; Main.rainTime = 0;
			Main.eclipse = false;
		}

		public override void OnWorldUnload() => ClearFixture();
	}
}
