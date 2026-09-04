using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using apogean.Content.Items.Currency;
using apogean.Content.NPCs.Kessler;
using apogean.Content.Structures;

namespace apogean.Content.Factions
{
	/// <summary>
	/// Tracks each corp faction's relationship to the player across a world: Dormant until
	/// its arrival milestone is hit, Hostile while its invasion is pending, Contactable once
	/// the invasion is cleared, then Allied/Enemy once the player picks a side late-game.
	/// The Broodmass isn't tracked here - it's the pre-hardmode horde, not a diplomatic actor.
	/// </summary>
	public class FactionProgression : ModSystem
	{
		public const int InvasionKillQuota = 10;
		public const int KesslerEliteThreshold = 4;

		public static readonly ApogeanFaction[] CorpFactions =
		{
			ApogeanFaction.Kessler,
			ApogeanFaction.Helix,
			ApogeanFaction.Sentrix
		};

		private readonly Dictionary<ApogeanFaction, FactionRelation> relations = new()
		{
			[ApogeanFaction.Kessler] = FactionRelation.Dormant,
			[ApogeanFaction.Helix] = FactionRelation.Dormant,
			[ApogeanFaction.Sentrix] = FactionRelation.Dormant,
		};

		private readonly Dictionary<ApogeanFaction, int> invasionKillsRemaining = new();
		private KesslerArrivalState kesslerArrival = new(KesslerArrivalStage.Dormant, false);
		private int quartermasterSpawnTimer;
		private int kesslerQaTimer;
		private bool kesslerQaTeleported;

		public bool MatriarchDowned { get; private set; }
		public ApogeanFaction? ChosenAlliance { get; private set; }
		public KesslerArrivalStage KesslerArrivalStage => kesslerArrival.Stage;
		public bool IsKesslerAssessmentActive => kesslerArrival.Stage == KesslerArrivalStage.AssessmentActive;

		public FactionRelation GetRelation(ApogeanFaction faction) =>
			relations.TryGetValue(faction, out FactionRelation relation) ? relation : FactionRelation.Dormant;

		public int GetInvasionKillsRemaining(ApogeanFaction faction) =>
			invasionKillsRemaining.TryGetValue(faction, out int remaining) ? remaining : 0;

		public static void SetMatriarchDowned()
		{
			FactionProgression progression = ModContent.GetInstance<FactionProgression>();
			progression.MatriarchDowned = true;
			progression.SyncWorldState();
		}

		public override void OnWorldLoad() => ResetState();

		public override void OnWorldUnload() => ResetState();

		public void RegisterInvasionKill(ApogeanFaction faction)
		{
			// NPC death can be observed by clients, but only the server owns the shared
			// invasion quota and its one-time completion reward transition.
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			if (GetRelation(faction) != FactionRelation.Hostile) return;
			if (!invasionKillsRemaining.TryGetValue(faction, out int remaining)) return;

			remaining = System.Math.Max(0, remaining - 1);
			invasionKillsRemaining[faction] = remaining;
			if (faction == ApogeanFaction.Kessler && remaining == KesslerEliteThreshold)
			{
				BroadcastWorldMessage("Mods.apogean.Kessler.Arrival.Escalation", new Color(218, 87, 54));
			}

			if (remaining <= 0)
			{
				SetContactable(faction);
			}
		}

		/// <summary>
		/// Only succeeds once, and only once all three corp invasions have been cleared.
		/// Sets the chosen faction Allied and the other two Enemy - the hinge point the
		/// endgame gauntlets and CEO fights key off of.
		/// </summary>
		public void SetAlliance(ApogeanFaction chosen)
		{
			if (ChosenAlliance != null) return;

			foreach (ApogeanFaction faction in CorpFactions)
			{
				if (GetRelation(faction) != FactionRelation.Contactable) return;
			}

			ChosenAlliance = chosen;
			foreach (ApogeanFaction faction in CorpFactions)
			{
				relations[faction] = faction == chosen ? FactionRelation.Allied : FactionRelation.Enemy;
				if (faction != chosen)
				{
					CompoundGen.ReArmCompound(faction);
				}
			}
			SyncWorldState();
		}

		private void SetHostile(ApogeanFaction faction)
		{
			if (GetRelation(faction) != FactionRelation.Dormant) return;
			relations[faction] = FactionRelation.Hostile;
			invasionKillsRemaining[faction] = InvasionKillQuota;
			SyncWorldState();
		}

		private void SetContactable(ApogeanFaction faction)
		{
			// Completion rewards and compound state changes are edge-triggered. A repeated
			// command, duplicate kill notification, or delayed packet must not pay twice.
			if (GetRelation(faction) != FactionRelation.Hostile)
				return;

			relations[faction] = FactionRelation.Contactable;
			if (faction == ApogeanFaction.Kessler)
			{
				kesslerArrival = new KesslerArrivalState(KesslerArrivalStage.Contactable, true);
				AwardKesslerCompletionScrip();
				BroadcastWorldMessage("Mods.apogean.Kessler.Arrival.Complete", new Color(220, 151, 76));
			}
			CompoundGen.UnsealCompound(faction);
			SyncWorldState();
		}

		public override void PostUpdateWorld()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			RunKesslerQaHarness();
			AdvanceKesslerArrival();
			EnsureKesslerQuartermaster();

			if (GetRelation(ApogeanFaction.Helix) == FactionRelation.Dormant &&
				NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
			{
				SetHostile(ApogeanFaction.Helix);
			}

			if (GetRelation(ApogeanFaction.Sentrix) == FactionRelation.Dormant && NPC.downedPlantBoss)
			{
				SetHostile(ApogeanFaction.Sentrix);
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["matriarchDowned"] = MatriarchDowned;
			tag["chosenAlliance"] = ChosenAlliance.HasValue ? (int)ChosenAlliance.Value : -1;
			tag["kesslerArrivalStage"] = (int)kesslerArrival.Stage;
			tag["kesslerArrivalSawNight"] = kesslerArrival.SawNight;

			foreach (ApogeanFaction faction in CorpFactions)
			{
				tag[$"relation_{faction}"] = (int)GetRelation(faction);
				tag[$"quota_{faction}"] = invasionKillsRemaining.TryGetValue(faction, out int remaining) ? remaining : 0;
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			ResetState();
			MatriarchDowned = tag.GetBool("matriarchDowned");
			int alliance = tag.GetInt("chosenAlliance");
			ChosenAlliance = alliance < 0 ? null : (ApogeanFaction)alliance;

			foreach (ApogeanFaction faction in CorpFactions)
			{
				string relationKey = $"relation_{faction}";
				relations[faction] = tag.ContainsKey(relationKey)
					? (FactionRelation)tag.GetInt(relationKey)
					: FactionRelation.Dormant;

				string quotaKey = $"quota_{faction}";
				if (tag.ContainsKey(quotaKey))
				{
					invasionKillsRemaining[faction] = tag.GetInt(quotaKey);
				}
			}

			if (tag.ContainsKey("kesslerArrivalStage"))
			{
				kesslerArrival = new KesslerArrivalState(
					(KesslerArrivalStage)tag.GetInt("kesslerArrivalStage"),
					tag.GetBool("kesslerArrivalSawNight"));
			}
			else
			{
				// Migrate the original one-step implementation without restarting a cleared invasion.
				kesslerArrival = GetRelation(ApogeanFaction.Kessler) switch
				{
					FactionRelation.Hostile => new KesslerArrivalState(KesslerArrivalStage.AssessmentActive, true),
					FactionRelation.Contactable or FactionRelation.Allied or FactionRelation.Enemy =>
						new KesslerArrivalState(KesslerArrivalStage.Contactable, true),
					_ => new KesslerArrivalState(KesslerArrivalStage.Dormant, false)
				};
			}
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(MatriarchDowned);
			writer.Write((sbyte)(ChosenAlliance.HasValue ? (int)ChosenAlliance.Value : -1));
			writer.Write((byte)kesslerArrival.Stage);
			writer.Write(kesslerArrival.SawNight);
			foreach (ApogeanFaction faction in CorpFactions)
			{
				writer.Write((byte)GetRelation(faction));
				writer.Write(invasionKillsRemaining.TryGetValue(faction, out int remaining) ? remaining : 0);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			ResetState();
			MatriarchDowned = reader.ReadBoolean();
			int alliance = reader.ReadSByte();
			ChosenAlliance = alliance < 0 ? null : (ApogeanFaction)alliance;
			kesslerArrival = new KesslerArrivalState((KesslerArrivalStage)reader.ReadByte(), reader.ReadBoolean());
			foreach (ApogeanFaction faction in CorpFactions)
			{
				relations[faction] = (FactionRelation)reader.ReadByte();
				invasionKillsRemaining[faction] = reader.ReadInt32();
			}
		}

		private void ResetState()
		{
			MatriarchDowned = false;
			ChosenAlliance = null;
			kesslerArrival = new KesslerArrivalState(KesslerArrivalStage.Dormant, false);
			quartermasterSpawnTimer = 0;
			kesslerQaTimer = 0;
			kesslerQaTeleported = false;
			invasionKillsRemaining.Clear();
			foreach (ApogeanFaction faction in CorpFactions)
				relations[faction] = FactionRelation.Dormant;
		}

		private void SyncWorldState()
		{
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.WorldData);
		}

		private void AdvanceKesslerArrival()
		{
			KesslerArrivalState previous = kesslerArrival;
			KesslerArrivalState next = previous.Observe(Main.hardMode, Main.dayTime);
			if (next == previous)
				return;

			kesslerArrival = next;
			if (next.Stage == KesslerArrivalStage.ImpactSignaled)
			{
				BroadcastWorldMessage("Mods.apogean.Kessler.Arrival.Impact", new Color(220, 122, 65));
			}
			else if (next.Stage == KesslerArrivalStage.AssessmentActive)
			{
				SetHostile(ApogeanFaction.Kessler);
				BroadcastWorldMessage("Mods.apogean.Kessler.Arrival.Assessment", new Color(231, 75, 48));
				return;
			}

			SyncWorldState();
		}

		private void AwardKesslerCompletionScrip()
		{
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				if (!player.active)
					continue;
				player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<KesslerScrip>(), 5);
			}
		}

		private void EnsureKesslerQuartermaster()
		{
			if (GetRelation(ApogeanFaction.Kessler) is not (FactionRelation.Contactable or FactionRelation.Allied))
				return;
			if (++quartermasterSpawnTimer < 180)
				return;
			quartermasterSpawnTimer = 0;

			int quartermasterType = ModContent.NPCType<KesslerQuartermaster>();
			if (NPC.AnyNPCs(quartermasterType) || !CompoundGen.TryGetPublicPost(ApogeanFaction.Kessler, out Point post))
				return;

			NPC.NewNPC(new EntitySource_WorldEvent(), post.X * 16 + 8, post.Y * 16, quartermasterType);
		}

		private static void BroadcastWorldMessage(string key, Color color)
		{
			if (Main.netMode == NetmodeID.Server)
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key), color);
			else if (Main.netMode == NetmodeID.SinglePlayer)
				Main.NewText(Language.GetTextValue(key), color);
		}

		internal void DebugSignalKesslerImpact()
		{
			relations[ApogeanFaction.Kessler] = FactionRelation.Dormant;
			invasionKillsRemaining.Remove(ApogeanFaction.Kessler);
			kesslerArrival = new KesslerArrivalState(KesslerArrivalStage.ImpactSignaled, false);
			CompoundGen.ReArmCompound(ApogeanFaction.Kessler);
			BroadcastWorldMessage("Mods.apogean.Kessler.Arrival.Impact", new Color(220, 122, 65));
			SyncWorldState();
		}

		internal void DebugBeginKesslerAssessment()
		{
			kesslerArrival = new KesslerArrivalState(KesslerArrivalStage.AssessmentActive, true);
			relations[ApogeanFaction.Kessler] = FactionRelation.Hostile;
			invasionKillsRemaining[ApogeanFaction.Kessler] = InvasionKillQuota;
			CompoundGen.ReArmCompound(ApogeanFaction.Kessler);
			BroadcastWorldMessage("Mods.apogean.Kessler.Arrival.Assessment", new Color(231, 75, 48));
			SyncWorldState();
		}

		internal void DebugCompleteKesslerAssessment()
		{
			invasionKillsRemaining[ApogeanFaction.Kessler] = 0;
			SetContactable(ApogeanFaction.Kessler);
			quartermasterSpawnTimer = 180;
			EnsureKesslerQuartermaster();
		}

		/// <summary>
		/// Deterministic visual-QA pilot for the disposable render world. Ordinary worlds can never
		/// enter this path. It keeps the weak test character alive, exposes the assessment long enough
		/// for capture, then moves the player to the authored public post for the dialogue/shop pass.
		/// </summary>
		private void RunKesslerQaHarness()
		{
			if (!Main.worldName.Equals("Apogee Campus QA", System.StringComparison.Ordinal))
				return;

			kesslerQaTimer++;
			if (kesslerQaTimer == 180)
			{
				DebugBeginKesslerAssessment();
				Main.NewText("QA PILOT: Kessler assessment active for 30 seconds.", Color.LightGreen);
			}

			if (IsKesslerAssessmentActive)
			{
				for (int i = 0; i < Main.maxPlayers; i++)
				{
					Player player = Main.player[i];
					if (!player.active)
						continue;
					player.immune = true;
					player.immuneTime = System.Math.Max(player.immuneTime, 2);
				}

				if (kesslerQaTimer == 900)
				{
					for (int i = 0; i < InvasionKillQuota - KesslerEliteThreshold; i++)
						RegisterInvasionKill(ApogeanFaction.Kessler);
					SpawnKesslerQaElite();
					Main.NewText("QA PILOT: escalation threshold reached; Reclaimer deployed.", Color.LightGreen);
				}

				if (kesslerQaTimer == 1980)
				{
					DebugCompleteKesslerAssessment();
					Main.NewText("QA PILOT: assessment complete; transferring to the public post.", Color.LightGreen);
				}
			}

			if (KesslerArrivalStage != KesslerArrivalStage.Contactable || kesslerQaTeleported ||
				!CompoundGen.TryGetPublicPost(ApogeanFaction.Kessler, out Point post))
				return;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				if (!player.active)
					continue;
				player.Teleport(new Vector2((post.X - 4) * 16f, (post.Y - 3) * 16f), TeleportationStyleID.RodOfDiscord);
				player.statLife = player.statLifeMax2;
				if (Main.netMode == NetmodeID.Server)
					NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, (post.X - 4) * 16f, (post.Y - 3) * 16f, TeleportationStyleID.RodOfDiscord);
			}
			kesslerQaTeleported = true;
		}

		private static void SpawnKesslerQaElite()
		{
			if (NPC.AnyNPCs(ModContent.NPCType<KesslerReclaimer>()))
				return;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				if (!player.active || player.dead)
					continue;

				NPC.NewNPC(new EntitySource_WorldEvent(),
					(int)player.Center.X + 260,
					(int)player.Center.Y - 80,
					ModContent.NPCType<KesslerReclaimer>());
				return;
			}
		}
	}
}
