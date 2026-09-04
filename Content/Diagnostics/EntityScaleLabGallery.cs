using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Content.NPCs.Engraft;

namespace apogean.Content.Diagnostics
{
	/// <summary>
	/// A deterministic live-render lineup. It compares Apogean enemies with a
	/// vanilla bird, zombie, and the player instead of accepting file dimensions
	/// as proof of readable gameplay scale.
	/// </summary>
	internal static class EntityScaleLabGallery
	{
		private const int Width = 96;
		private const int Height = 25;
		private static readonly List<(int NpcIndex, Vector2 Center)> actors = new();

		internal static Rectangle Build(Player player)
		{
			Main.dayTime = true;
			Main.time = 27000d;
			Point playerTile = player.Center.ToTileCoordinates();
			int left = Math.Clamp(playerTile.X - Width / 2, 20, Main.maxTilesX - Width - 20);
			int top = Math.Clamp(playerTile.Y - 16, 20, Main.maxTilesY - Height - 20);
			Rectangle bounds = new(left, top, Width, Height);
			int floorY = bounds.Bottom - 3;

			actors.Clear();
			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
				Framing.GetTileSafely(x, y).ClearEverything();

			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = floorY; y < bounds.Bottom; y++)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				tile.HasTile = true;
				tile.TileType = TileID.GrayBrick;
				tile.TileFrameX = 0;
				tile.TileFrameY = 0;
			}

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.townNPC)
					npc.active = false;
			}

			int baseline = floorY * 16;
			SpawnPinned(NPCID.Bird, new Vector2((left + 24) * 16, baseline - 14));
			SpawnPinned(NPCID.Zombie, new Vector2((left + 38) * 16, baseline - 22));
			SpawnPinned(ModContent.NPCType<Mawling>(), new Vector2((left + 56) * 16, baseline - 28));
			SpawnPinned(ModContent.NPCType<GraftHound>(), new Vector2((left + 73) * 16, baseline - 22));

			for (int x = bounds.Left; x < bounds.Right; x++)
			for (int y = bounds.Top; y < bounds.Bottom; y++)
				WorldGen.SquareTileFrame(x, y, true);

			Lighting.Clear();
			player.Teleport(new Vector2((left + 48) * 16f, (floorY - 4) * 16f), TeleportationStyleID.RodOfDiscord);
			player.fallStart = (int)(player.position.Y / 16f);
			player.statLife = player.statLifeMax2;
			player.immune = true;
			player.immuneNoBlink = true;
			player.immuneTime = 3600;
			PinActors();
			return bounds;
		}

		private static void SpawnPinned(int type, Vector2 center)
		{
			NPC npc = NPC.NewNPCDirect(new EntitySource_Misc("ApogeanEntityScaleLab"), center, type);
			npc.Center = center;
			npc.direction = -1;
			npc.spriteDirection = -1;
			npc.friendly = true;
			npc.damage = 0;
			npc.dontTakeDamage = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			actors.Add((npc.whoAmI, center));
		}

		internal static void PinActors()
		{
			foreach ((int npcIndex, Vector2 center) in actors)
			{
				if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
					continue;
				NPC npc = Main.npc[npcIndex];
				if (!npc.active)
					continue;
				npc.Center = center;
				npc.velocity = Vector2.Zero;
				npc.direction = -1;
				npc.spriteDirection = -1;
				npc.friendly = true;
				npc.damage = 0;
				npc.dontTakeDamage = true;
				npc.noGravity = true;
				npc.noTileCollide = true;
			}
		}

		internal static void Clear() => actors.Clear();
	}

	internal sealed class EntityScaleLabSystem : ModSystem
	{
		public override void PostUpdateNPCs() => EntityScaleLabGallery.PinActors();
		public override void OnWorldUnload() => EntityScaleLabGallery.Clear();
	}
}
