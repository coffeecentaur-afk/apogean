using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using apogean.Content.Factions;
using apogean.Content.Tiles;

namespace apogean.Content.Structures
{
	/// <summary>
	/// Places one sealed compound per corp faction during world generation - visible but
	/// inert, the same "there but not accessible yet" treatment vanilla gives the Jungle
	/// Temple before Plantera. FactionProgression calls UnsealCompound/ReArmCompound once a
	/// faction's invasion is cleared or it turns Enemy late-game.
	/// </summary>
	public class CompoundGen : ModSystem
	{
		private const int CompoundSize = 9;

		private readonly Dictionary<ApogeanFaction, Point16> compoundLocations = new();

		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			int dungeonIndex = tasks.FindIndex(pass => pass.Name.Equals("Dungeon"));
			int insertAt = dungeonIndex >= 0 ? dungeonIndex + 1 : tasks.Count;
			tasks.Insert(insertAt, new PassLegacy("Apogean Compounds", GenerateCompounds));
		}

		private void GenerateCompounds(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "Sealing corporate compounds...";

			foreach (ApogeanFaction faction in FactionProgression.CorpFactions)
			{
				Point16 location = PickCompoundLocation();
				compoundLocations[faction] = location;
				PlaceSealedCompound(location);
			}
		}

		private static Point16 PickCompoundLocation()
		{
			int x = Terraria.WorldGen.genRand.Next((int)(Main.maxTilesX * 0.1), (int)(Main.maxTilesX * 0.9));
			int y = Terraria.WorldGen.genRand.Next((int)Main.worldSurface + 50, (int)Main.rockLayer + 200);
			return new Point16(x, y);
		}

		private static void PlaceSealedCompound(Point16 origin)
		{
			int bulkheadType = ModContent.TileType<LockedBulkhead>();

			for (int x = origin.X; x < origin.X + CompoundSize; x++)
			{
				for (int y = origin.Y; y < origin.Y + CompoundSize; y++)
				{
					if (IsEdge(origin, x, y))
					{
						Terraria.WorldGen.PlaceTile(x, y, bulkheadType, mute: true, forced: true);
					}
					else
					{
						Terraria.WorldGen.KillTile(x, y, noItem: true);
					}
				}
			}
		}

		private static bool IsEdge(Point16 origin, int x, int y) =>
			x == origin.X || x == origin.X + CompoundSize - 1 ||
			y == origin.Y || y == origin.Y + CompoundSize - 1;

		/// <summary>Called on invasion victory: opens the compound and lets an ambassador move in.</summary>
		public static void UnsealCompound(ApogeanFaction faction) => SetCompoundSealed(faction, sealedShut: false);

		/// <summary>Called when a faction turns Enemy late-game: re-arms the compound into a gauntlet entrance.</summary>
		public static void ReArmCompound(ApogeanFaction faction) => SetCompoundSealed(faction, sealedShut: true);

		private static void SetCompoundSealed(ApogeanFaction faction, bool sealedShut)
		{
			CompoundGen instance = ModContent.GetInstance<CompoundGen>();
			if (!instance.compoundLocations.TryGetValue(faction, out Point16 origin)) return;

			int bulkheadType = ModContent.TileType<LockedBulkhead>();
			// TODO: swap for a proper faction-themed door/hazard tile once that art exists.
			int openTileType = TileID.PlatinumBrick;

			for (int x = origin.X; x < origin.X + CompoundSize; x++)
			{
				for (int y = origin.Y; y < origin.Y + CompoundSize; y++)
				{
					if (!IsEdge(origin, x, y)) continue;

					Terraria.WorldGen.KillTile(x, y, noItem: true);
					Terraria.WorldGen.PlaceTile(x, y, sealedShut ? bulkheadType : openTileType, mute: true, forced: true);
				}
			}

			NetMessage.SendTileSquare(-1, origin.X, origin.Y, CompoundSize);
		}

		public override void SaveWorldData(TagCompound tag)
		{
			foreach (ApogeanFaction faction in FactionProgression.CorpFactions)
			{
				if (compoundLocations.TryGetValue(faction, out Point16 location))
				{
					tag[$"compound_{faction}_x"] = (int)location.X;
					tag[$"compound_{faction}_y"] = (int)location.Y;
				}
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			foreach (ApogeanFaction faction in FactionProgression.CorpFactions)
			{
				string xKey = $"compound_{faction}_x";
				string yKey = $"compound_{faction}_y";
				if (tag.ContainsKey(xKey) && tag.ContainsKey(yKey))
				{
					compoundLocations[faction] = new Point16(tag.GetInt(xKey), tag.GetInt(yKey));
				}
			}
		}
	}
}
