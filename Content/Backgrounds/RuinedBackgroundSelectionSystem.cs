using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using apogean.Content.Biomes;

namespace apogean.Content.Backgrounds
{
	public enum RuinedBackgroundBiome : byte
	{
		Forest,
		Desert,
		Jungle,
		Snow,
		Corruption,
		Crimson,
		Hallow,
		Ocean,
		Mushroom,
		Underworld,
		Engraft,
		Count
	}

	/// <summary>Stores one stable visual history for each biome. Time changes lighting, never landmark placement.</summary>
	public sealed class RuinedBackgroundSelectionSystem : ModSystem
	{
		private const int VariantCount = 2;
		private readonly byte[] variants = new byte[(int)RuinedBackgroundBiome.Count];
		public bool ForestConceptRenderLabEnabled { get; private set; }
		public bool ForestUndergroundRenderLabEnabled { get; private set; }

		public static RuinedBackgroundSelectionSystem Instance => ModContent.GetInstance<RuinedBackgroundSelectionSystem>();

		public override void OnWorldLoad()
		{
			ForestConceptRenderLabEnabled = false;
			ForestUndergroundRenderLabEnabled = false;
			for (int i = 0; i < variants.Length; i++)
			{
				int mixedSeed = unchecked(Main.worldID * 397 ^ (i + 17) * 7919);
				variants[i] = (byte)((mixedSeed & int.MaxValue) % VariantCount);
			}
		}

		public int GetVariant(RuinedBackgroundBiome biome) => variants[(int)biome] % VariantCount;

		public bool ToggleForestConceptRenderLab(bool? enabled = null)
		{
			ForestConceptRenderLabEnabled = enabled ?? !ForestConceptRenderLabEnabled;
			return ForestConceptRenderLabEnabled;
		}

		public bool ToggleForestUndergroundRenderLab(bool? enabled = null)
		{
			ForestUndergroundRenderLabEnabled = enabled ?? !ForestUndergroundRenderLabEnabled;
			return ForestUndergroundRenderLabEnabled;
		}

		public override void OnWorldUnload()
		{
			ForestConceptRenderLabEnabled = false;
			ForestUndergroundRenderLabEnabled = false;
		}

		public int Cycle(RuinedBackgroundBiome biome)
		{
			int index = (int)biome;
			variants[index] = (byte)((variants[index] + 1) % VariantCount);
			return variants[index];
		}

		public override void SaveWorldData(TagCompound tag) => tag["ruinedBackgroundVariants"] = variants;

		public override void LoadWorldData(TagCompound tag)
		{
			if (!tag.TryGet("ruinedBackgroundVariants", out byte[] saved)) return;
			Array.Copy(saved, variants, Math.Min(saved.Length, variants.Length));
		}

		public override void NetSend(BinaryWriter writer)
		{
			for (int i = 0; i < variants.Length; i++) writer.Write(variants[i]);
		}

		public override void NetReceive(BinaryReader reader)
		{
			for (int i = 0; i < variants.Length; i++) variants[i] = reader.ReadByte();
		}

		public static RuinedBackgroundBiome DetectBiome(Player player)
		{
			if (player.InModBiome<EngraftBiome>()) return RuinedBackgroundBiome.Engraft;
			if (player.ZoneUnderworldHeight) return RuinedBackgroundBiome.Underworld;
			if (player.ZoneGlowshroom) return RuinedBackgroundBiome.Mushroom;
			if (player.ZoneBeach) return RuinedBackgroundBiome.Ocean;
			if (player.ZoneHallow) return RuinedBackgroundBiome.Hallow;
			if (player.ZoneCrimson) return RuinedBackgroundBiome.Crimson;
			if (player.ZoneCorrupt) return RuinedBackgroundBiome.Corruption;
			if (player.ZoneJungle) return RuinedBackgroundBiome.Jungle;
			if (player.ZoneSnow) return RuinedBackgroundBiome.Snow;
			if (player.ZoneDesert) return RuinedBackgroundBiome.Desert;
			return RuinedBackgroundBiome.Forest;
		}
	}
}
