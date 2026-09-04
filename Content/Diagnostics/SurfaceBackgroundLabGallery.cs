using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.Tiles;

namespace apogean.Content.Diagnostics
{
	internal enum SurfaceBackgroundLighting
	{
		Noon,
		Midnight,
		Eclipse
	}

	/// <summary>Wall-free, low-profile surface fixture for judging authored parallax layers.</summary>
	internal static class SurfaceBackgroundLabGallery
	{
		private const int Width = 190;
		private const int Height = 62;

		internal static Rectangle Build(
			Player player,
			SurfaceBackgroundLighting lighting = SurfaceBackgroundLighting.Noon,
			bool aerial = false)
		{
			// Geometry remains identical across lighting fixtures. These explicit
			// states prove that Terraria's sky/tint changes without landmark jumps.
			Main.eclipse = false;
			switch (lighting)
			{
				case SurfaceBackgroundLighting.Midnight:
					Main.dayTime = false;
					Main.time = 16200d;
					break;
				case SurfaceBackgroundLighting.Eclipse:
					Main.dayTime = true;
					Main.time = 27000d;
					Main.eclipse = true;
					break;
				default:
					Main.dayTime = true;
					Main.time = 27000d;
					break;
			}
			Main.raining = false;

			Point playerTile = player.Center.ToTileCoordinates();
			int centerX = Math.Clamp(playerTile.X, Width / 2 + 20, Main.maxTilesX - Width / 2 - 20);
			// worldSurface is Terraria's underground transition, not the visible
			// terrain crest. A fixed offset keeps the fixture unambiguously in the
			// surface background band even when the current X contains a tall lab.
			int surfaceY = Math.Clamp((int)Main.worldSurface - 50, 80, Main.maxTilesY - Height - 20);
			int left = centerX - Width / 2;
			int top = Math.Clamp(surfaceY - Height + 8, 20, Main.maxTilesY - Height - 20);
			Rectangle bounds = new(left, top, Width, Height);
			int floorY = bounds.Bottom - 8;
			int sand = ModContent.TileType<WastesSandCandidate>();

			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				tile.ClearEverything();
				tile.WallType = WallID.None;
				tile.WallColor = PaintID.None;
			}

			for (int x = bounds.Left; x < bounds.Right; x++)
			{
				int edgeDistance = Math.Min(x - bounds.Left, bounds.Right - 1 - x);
				int duneRise = edgeDistance < 22 ? (22 - edgeDistance) / 5 : 0;
				int ripple = ((x - bounds.Left) / 17) % 2;
				for (int y = floorY - duneRise - ripple; y < bounds.Bottom; y++)
					SetTile(x, y, sand);
			}

			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
			{
				WorldGen.SquareTileFrame(x, y, true);
				WorldGen.SquareWallFrame(x, y, true);
			}

			Lighting.Clear();
			player.Teleport(new Vector2(bounds.Center.X * 16f, (floorY - (aerial ? 54 : 4)) * 16f), TeleportationStyleID.RodOfDiscord);
			player.fallStart = (int)(player.position.Y / 16f);
			player.statLife = player.statLifeMax2;
			player.immune = true;
			player.immuneNoBlink = true;
			player.immuneTime = 3600;
			for (int npcIndex = 0; npcIndex < Main.maxNPCs; npcIndex++)
			{
				NPC npc = Main.npc[npcIndex];
				if (npc.active && !npc.townNPC)
					npc.active = false;
			}
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendTileSquare(-1, bounds.Center.X, bounds.Center.Y, Width + 6);
			return bounds;
		}

		internal static Rectangle BuildProductionJungleRouting(Player player)
		{
			Rectangle bounds = Build(player);
			int jungleGrass = TileID.JungleGrass;
			int mud = TileID.Mud;

			// Bury a deterministic biome-count bed under the visible Wastes floor.
			// No render-lab override is active: Terraria's own scene metrics must set
			// ZoneJungle and the production global background router must respond.
			for (int x = bounds.Left + 8; x < bounds.Right - 8; x++)
			for (int y = bounds.Bottom - 9; y < bounds.Bottom; y++)
				SetTile(x, y, y == bounds.Bottom - 9 ? jungleGrass : mud);

			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Bottom - 10; y < bounds.Bottom; y++)
				WorldGen.SquareTileFrame(x, y, true);
			return bounds;
		}

		private static void SetTile(int x, int y, int type)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			tile.HasTile = true;
			tile.TileType = (ushort)type;
			tile.TileFrameX = 0;
			tile.TileFrameY = 0;
			tile.Slope = SlopeType.Solid;
			tile.IsHalfBlock = false;
			tile.LiquidAmount = 0;
		}
	}
}
