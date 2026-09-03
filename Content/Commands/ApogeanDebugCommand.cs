using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using apogean.Common.WorldGeneration;
using apogean.Content.Backgrounds;
using apogean.Content.Diagnostics;
using apogean.Content.Factions;
using apogean.Content.Items.Consumables;
using apogean.Content.Items.Materials;
using apogean.Content.Items.Placeable;
using apogean.Content.Items.Weapons;
using apogean.Content.NPCs.Broodmass;
using apogean.Content.NPCs.Debug;
using apogean.Content.World;

namespace apogean.Content.Commands
{
	/// <summary>
	/// Playtest helpers so testing a fight doesn't require farming its summon materials.
	/// Strip this before any public release.
	/// </summary>
	public class ApogeanDebugCommand : ModCommand
	{
		private static readonly Color Info = new(150, 90, 170);

		public override CommandType Type => CommandType.Chat;
		public override string Command => "apogean";
		public override string Usage => "/apogean <matriarch|lure|gland|engraft [force]|plan|ruin|background|backgroundlab [on|off]|undergroundlab [on|off]|gallery|tilelab|grasslab|vegetationlab|terrainlab|terrainproperties|conversionlab|terrainitems|exportatlases|kit|npc|flags|clear>";
		public override string Description => "Apogean playtest helpers";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = caller.Player;
			string sub = args.Length > 0 ? args[0].ToLowerInvariant() : "help";
			int count = args.Length > 1 && int.TryParse(args[1], out int n) ? n : 1;

			switch (sub)
			{
				case "matriarch":
					if (NPC.AnyNPCs(ModContent.NPCType<Matriarch>()))
					{
						caller.Reply("The Matriarch is already active.", Color.Orange);
						break;
					}
					NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<Matriarch>());
					caller.Reply("Matriarch spawned.", Info);
					break;

				case "lure":
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"),
						ModContent.ItemType<PheromoneLure>(), count);
					caller.Reply($"Gave {count}x Pheromone Lure.", Info);
					break;

				case "gland":
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"),
						ModContent.ItemType<MutagenGland>(), count);
					caller.Reply($"Gave {count}x Mutagen Gland.", Info);
					break;

				case "engraft":
					bool force = args.Length > 1 && args[1].Equals("force", System.StringComparison.OrdinalIgnoreCase);
					if (EngraftSystem.Instance.TryCreateDebugRupture(player, force, out string failureReason))
						caller.Reply("Created a protected-plan-aware playtest Maw Rupture at the local surface.", new Color(194, 126, 44));
					else
						caller.Reply(failureReason, Color.OrangeRed);
					break;

				case "plan":
					ApogeanWorldPlanSystem worldPlan = ApogeanWorldPlanSystem.Instance;
					if (worldPlan.Plan is null)
					{
						caller.Reply("This legacy world does not yet have a saved Apogean world plan.", Color.Orange);
						break;
					}
					caller.Reply($"World plan v{worldPlan.Plan.SchemaVersion}; hash {worldPlan.Plan.StableHash():X8}; ruptures {worldPlan.Plan.MawRuptures.Count}; landmarks {worldPlan.Plan.Landmarks.Count}; protected regions {worldPlan.ProtectedRegions.Count}.", Info);
					Microsoft.Xna.Framework.Rectangle sanctuary = worldPlan.Plan.SpawnSanctuary;
					caller.Reply($"Spawn sanctuary: X {sanctuary.Left}–{sanctuary.Right - 1}, Y {sanctuary.Top}–{sanctuary.Bottom - 1}. It blocks Maw/corporate edits, not ordinary events or boss summons.", Info);
					MawRupturePlan major = worldPlan.Plan.GetMajorRupture();
					if (major is not null)
						caller.Reply($"Major Maw: mouth {major.SurfaceCenter.X}, {major.SurfaceCenter.Y}; root {major.MatriarchCenter.X}, {major.MatriarchCenter.Y}; spine points {major.NavigationSpine.Count}.", new Color(194, 126, 44));
					for (int i = 0; i < worldPlan.Plan.Landmarks.Count; i++)
					{
						ApogeanLandmarkPlan landmark = worldPlan.Plan.Landmarks[i];
						caller.Reply($"{landmark.Kind}: X {landmark.Bounds.Left}–{landmark.Bounds.Right - 1}, Y {landmark.Bounds.Top}–{landmark.Bounds.Bottom - 1}.", Info);
					}
					System.Collections.Generic.IReadOnlyList<string> failures = worldPlan.Validate();
					if (failures.Count == 0)
						caller.Reply("World-plan validation passed.", Color.LightGreen);
					else
						foreach (string failure in failures) caller.Reply(failure, Color.OrangeRed);
					break;

				case "ruin":
					int changed = RuinedSurfaceSystem.ApplyRuinedSurface();
					caller.Reply($"Ruined the remaining green surface ({changed:N0} tiles changed).", new Color(143, 94, 45));
					break;

				case "background":
					RuinedBackgroundBiome biome = RuinedBackgroundSelectionSystem.DetectBiome(player);
					int variant = RuinedBackgroundSelectionSystem.Instance.Cycle(biome);
					caller.Reply($"{biome} background changed to seeded variant {variant + 1}.", new Color(194, 126, 44));
					break;

				case "backgroundlab":
					bool? requested = args.Length > 1
						? args[1].Equals("on", System.StringComparison.OrdinalIgnoreCase)
						: null;
					bool enabled = RuinedBackgroundSelectionSystem.Instance.ToggleForestConceptRenderLab(requested);
					caller.Reply(enabled
						? "Forest concept render lab enabled. The three diagnostic parallax layers are active locally."
						: "Forest concept render lab disabled. Production background routing restored.",
						enabled ? Color.LightGreen : Color.LightSkyBlue);
					break;

				case "undergroundlab":
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						caller.Reply("The destructive underground renderer lab is single-player/server-host only.", Color.OrangeRed);
						break;
					}
					bool? undergroundRequested = args.Length > 1
						? args[1].Equals("on", System.StringComparison.OrdinalIgnoreCase)
						: null;
					bool undergroundEnabled = RuinedBackgroundSelectionSystem.Instance.ToggleForestUndergroundRenderLab(undergroundRequested);
					if (undergroundEnabled)
					{
						player.GetModPlayer<TileLabPlayer>().BuildUndergroundBackgroundAndReport();
						caller.Reply("Wastes underground render lab enabled and its disposable cavern fixture built.", Color.LightGreen);
					}
					else
					{
						caller.Reply("Wastes underground render lab disabled. Production cave routing restored.", Color.LightSkyBlue);
					}
					break;

				case "gallery":
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						caller.Reply("The destructive visual gallery helper is single-player/server-host only.", Color.OrangeRed);
						break;
					}
					player.GetModPlayer<TileLabPlayer>().BuildMaterialGalleryAndReport(scheduleCaptureProbe: true);
					caller.Reply("Built the native material gallery and scheduled its capture-camera probe. It intentionally clears that debug rectangle.", Color.LightGreen);
					break;

				case "tilelab":
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						caller.Reply("The destructive Tile Lab is single-player/server-host only.", Color.OrangeRed);
						break;
					}
					Rectangle tileLab = TileLabGallery.Build(player);
					caller.Reply($"Built the isolated Tile Lab at X {tileLab.Left}-{tileLab.Right - 1}, Y {tileLab.Top}-{tileLab.Bottom - 1}.", Color.LightGreen);
					break;

				case "grasslab":
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						caller.Reply("The destructive Grass Lab is single-player/server-host only.", Color.OrangeRed);
						break;
					}
					player.GetModPlayer<TileLabPlayer>().BuildGrassAndReport(scheduleCaptureProbe: true);
					break;

				case "vegetationlab":
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						caller.Reply("The destructive Vegetation Lab is single-player/server-host only.", Color.OrangeRed);
						break;
					}
					player.GetModPlayer<TileLabPlayer>().BuildVegetationAndReport(scheduleCaptureProbe: true);
					break;

				case "terrainlab":
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						caller.Reply("The destructive Wastes Terrain Lab is single-player/server-host only.", Color.OrangeRed);
						break;
					}
					player.GetModPlayer<TileLabPlayer>().BuildWastesTerrainAndReport(scheduleCaptureProbe: true);
					break;

				case "terrainproperties":
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						caller.Reply("The destructive Wastes Terrain Property Lab is single-player/server-host only.", Color.OrangeRed);
						break;
					}
					player.GetModPlayer<TileLabPlayer>().BuildWastesTerrainPropertiesAndReport(scheduleCaptureProbe: true);
					break;

				case "terrainitems":
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<WastesSoilBlock>(), 100);
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<WastesStoneBlock>(), 100);
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<WastesSandBlock>(), 100);
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<WastesIceBlock>(), 100);
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<WastesSnowBlock>(), 100);
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<WastesMudBlock>(), 100);
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ItemID.Sandgun);
					caller.Reply("Gave the six Wastes terrain blocks and a Sandgun for placement, mining, and ammo checks.", Color.LightGreen);
					break;

				case "conversionlab":
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						caller.Reply("The destructive Maw Conversion Lab is single-player/server-host only.", Color.OrangeRed);
						break;
					}
					player.GetModPlayer<TileLabPlayer>().BuildMawConversionAndReport(scheduleCaptureProbe: true);
					break;

				case "exportatlases":
					if (Main.dedServ)
					{
						caller.Reply("Atlas export requires a graphics client.", Color.OrangeRed);
						break;
					}
					string atlasDirectory = VanillaAtlasExporter.ExportTileLabReferences();
					caller.Reply($"Exported vanilla Tile Lab references to {atlasDirectory}.", Color.LightGreen);
					break;

				case "kit":
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<RendHook>());
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<SinewBow>());
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<AmberSiphon>());
					player.QuickSpawnItem(player.GetSource_Misc("ApogeanDebug"), ModContent.ItemType<MawEffigy>());
					caller.Reply("Gave the Act 1 Maw playtest weapon set.", new Color(194, 126, 44));
					break;

				case "npc":
					NPC.NewNPC(player.GetSource_Misc("ApogeanDebug"),
						(int)player.Center.X + 80, (int)player.Center.Y,
						ModContent.NPCType<TestAmbassador>());
					caller.Reply("Spawned the dialogue test NPC. Click it.", Info);
					break;

				case "flags":
					FactionProgression progression = ModContent.GetInstance<FactionProgression>();
					caller.Reply($"Matriarch downed: {progression.MatriarchDowned}", Info);
					foreach (ApogeanFaction faction in FactionProgression.CorpFactions)
					{
						FactionInfo info = FactionInfo.Get(faction);
						caller.Reply($"  {info.DisplayName}: {progression.GetRelation(faction)}", info.Color);
					}
					caller.Reply($"Alliance: {progression.ChosenAlliance?.ToString() ?? "none"}", Info);
					break;

				case "clear":
					int cleared = 0;
					foreach (NPC npc in Main.npc)
					{
						if (npc.active && npc.ModNPC?.Mod == Mod) { npc.active = false; cleared++; }
					}
					caller.Reply($"Cleared {cleared} Apogean NPCs.", Info);
					break;

				default:
					caller.Reply(Usage, Color.Yellow);
					break;
			}
		}
	}
}
