using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using apogean.Content.Factions;
using apogean.Content.Items.Consumables;
using apogean.Content.Items.Materials;
using apogean.Content.Projectiles;
using apogean.Content.World;

namespace apogean.Content.NPCs.Broodmass
{
	/// <summary>
	/// The Broodmass capstone and the mod's first boss - escaped Helix Product that bred into
	/// a hive queen. The fight is built around three ideas that all come from the fiction:
	/// she ITERATES (adapts to whatever damage class you favour), she is NETWORKED (her brood
	/// shields her through visible tethers), and she is BRANDED (the Helix serial plate is a
	/// literal weak point while she commits to a big attack).
	/// </summary>
	[AutoloadBossHead]
	public class Matriarch : BroodmassNPC
	{
		private const float StateHover = 0f;
		private const float StateSpit = 1f;
		private const float StateCharge = 2f;
		private const float StateSummon = 3f;
		private const float StateSporeBloom = 4f;
		private const float StateEggPods = 5f;
		private const float StateReclaim = 6f;

		private const int ChargeTelegraph = 32;
		private const int ChargeDuration = 44;

		/// <summary>Tethers past this still draw, but stop adding damage reduction.</summary>
		private const int MaxTetherShield = 4;

		/// <summary>Hard ceiling on live brood, so pod hatches can't snowball the arena.</summary>
		public const int MaxBrood = 8;

		/// <summary>
		/// Ceiling on everything one Reclamation can restore. Without this, a full arena of
		/// hatched pods could hand her back most of her health in a single channel.
		/// </summary>
		private const int ReclaimHealCap = 420;

		/// <summary>Frames in Matriarch_Port.png: first half idles, second half dilates.</summary>
		private const int PortFrames = 8;

		// Close-range purge. Contact damage is gone, so this is what stops melee simply
		// parking inside her between dashes - but it is telegraphed and on a cooldown, so
		// the answer is to step out and step back in rather than to stay at range forever.
		private const float PulseRange = 150f;
		private const int PulseProvokeTime = 95;
		private const int PulseWindup = 45;
		private const int PulseCooldown = 260;

		// How long she commits to the two big attacks, and how long the plate stays exposed.
		// These are deliberately long: the exposed plate is the fight's main damage window,
		// so it has to last long enough to be worth fighting through the brood to reach.
		private const int BloomWindup = 80;
		private const int ReclaimWindup = 60;
		private const int ReclaimChannel = 260;

		private ref float State => ref NPC.ai[0];
		private ref float Timer => ref NPC.ai[1];
		private ref float ChargesLeft => ref NPC.ai[2];

		// Damage soaked per class, used to decide what she hardens against next.
		private readonly int[] damageByClass = new int[4];
		private int adaptedClass = -1;
		private int adaptationCooldown;

		/// <summary>Set during big-attack windups: the serial plate is lit and she takes extra damage.</summary>
		private bool plateExposed;

		/// <summary>True only while a dash is actually travelling - gates all contact damage.</summary>
		private bool dashing;

		/// <summary>Healing already granted by the current Reclamation, measured against the cap.</summary>
		private int reclaimHealed;

		private int proximityTimer;
		private int pulseWindup;
		private int pulseCooldown;
		private int outsideEngraftTimer;
		private bool outsideEngraftEnraged;

		private float LifeRatio => NPC.life / (float)NPC.lifeMax;

		/// <summary>1 = hive discipline, 2 = adaptation (shell cracks), 3 = unrestrained growth.</summary>
		private int Phase => LifeRatio > 0.65f ? 1 : LifeRatio > 0.30f ? 2 : 3;

		private int TetheredBrood => Math.Min(NPC.CountNPCS(ModContent.NPCType<Broodling>()), MaxTetherShield);

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			NPCID.Sets.TrailCacheLength[Type] = 6;
			NPCID.Sets.TrailingMode[Type] = 3;
		}

		public override void SetDefaults()
		{
			NPC.width = 120;
			NPC.height = 100;
			NPC.lifeMax = 4500;
			NPC.damage = 40;
			NPC.defense = 14;
			NPC.knockBackResist = 0f;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.boss = true;
			NPC.value = Item.buyPrice(gold: 2, silver: 50);
			// Pitched well down so she reads as large; the wet sample plus the low end gives
			// the mechanical-gurgle character rather than a generic monster grunt.
			NPC.HitSound = SoundID.NPCHit13 with { Pitch = -0.85f, PitchVariance = 0.15f };
			NPC.DeathSound = SoundID.NPCDeath13 with { Pitch = -0.9f };
			Music = MusicID.Boss2;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("Mods.apogean.Bestiary.Matriarch")
			});
		}

		// ---------------------------------------------------------------- AI

		public override void AI()
		{
			if (!NPC.HasValidTarget) NPC.TargetClosest();

			Player target = Main.player[NPC.target];

			if (!target.active || target.dead)
			{
				NPC.velocity.Y -= 0.4f;
				if (NPC.timeLeft > 60) NPC.timeLeft = 60;
				return;
			}

			NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
			Timer++;
			plateExposed = false;
			dashing = false;

			if (adaptationCooldown > 0) adaptationCooldown--;
			UpdateTerritoryPressure();

			UpdateContainmentPulse(target);

			switch (State)
			{
				case StateHover: HoverAI(target); break;
				case StateSpit: SpitAI(target); break;
				case StateCharge: ChargeAI(target); break;
				case StateSummon: SummonAI(); break;
				case StateSporeBloom: SporeBloomAI(); break;
				case StateEggPods: EggPodsAI(target); break;
				case StateReclaim: ReclaimAI(); break;
			}
		}

		/// <summary>
		/// MATRIARCH-7A-1 is a regional growth node. Outside the Engraft she panics into a brief,
		/// hazardous escape response, then withdraws rather than allowing an all-world town fight.
		/// No tiles are converted here: combat never vandalizes a player's build.
		/// </summary>
		private void UpdateTerritoryPressure()
		{
			if (EngraftSystem.IsInEngraft(NPC.Center))
			{
				outsideEngraftTimer = 0;
				outsideEngraftEnraged = false;
				return;
			}

			outsideEngraftTimer++;
			if (outsideEngraftTimer == 120)
			{
				outsideEngraftEnraged = true;
				Announce("Territory.Enraged");
			}

			if (outsideEngraftEnraged && outsideEngraftTimer % 75 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Vector2 velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
					ModContent.ProjectileType<SporeBurst>(), 24, 0f, Main.myPlayer);
			}

			if (outsideEngraftTimer >= 540)
			{
				NPC.velocity.Y -= 0.25f;
				NPC.timeLeft = Math.Min(NPC.timeLeft, 90);
			}
		}

		private void HoverAI(Player target)
		{
			Vector2 destination = target.Center + new Vector2(0f, -220f);
			Vector2 desired = NPC.DirectionTo(destination) * (Phase == 3 ? 9f : Phase == 2 ? 7f : 5.5f);
			NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.05f);

			int hoverTime = Phase == 3 ? 40 : Phase == 2 ? 60 : 85;
			if (Timer < hoverTime) return;

			SwitchState(PickNextAttack());
		}

		/// <summary>
		/// Weighted by what the fight needs: she rebuilds her tether shield when it's thin,
		/// reclaims brood to heal only when she's desperate, and leans harder on charges late.
		/// </summary>
		private float PickNextAttack()
		{
			int brood = NPC.CountNPCS(ModContent.NPCType<Broodling>());

			// Enraged and surrounded by her own spawn - wall up behind them and regenerate.
			// Available through all of phase 3 so the fight's centrepiece actually gets seen.
			if (Phase == 3 && brood >= 2 && Main.rand.NextBool(3))
			{
				return StateReclaim;
			}

			// Shield is thin: restock, either directly or by seeding pods.
			if (brood < (Phase == 1 ? 2 : Phase == 2 ? 4 : MaxTetherShield))
			{
				if (Main.rand.NextBool(2)) return Phase >= 2 ? StateEggPods : StateSummon;
				if (Main.rand.NextBool(2)) return StateSummon;
			}

			if (Phase >= 2 && Main.rand.NextBool(4)) return StateSporeBloom;

			return Main.rand.NextBool(Phase == 1 ? 3 : 2) ? StateCharge : StateSpit;
		}

		private void SpitAI(Player target)
		{
			NPC.velocity *= 0.94f;

			if (Timer == 22)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int shots = Phase == 1 ? 3 : Phase == 2 ? 5 : 7;
					float spread = MathHelper.ToRadians(Phase == 1 ? 14f : 24f);
					Vector2 aim = NPC.DirectionTo(target.Center) * (Phase == 3 ? 10f : 8f);

					for (int i = 0; i < shots; i++)
					{
						float offset = MathHelper.Lerp(-spread, spread, i / (float)(shots - 1));
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, aim.RotatedBy(offset),
							ModContent.ProjectileType<AcidGlob>(), 24, 2f, Main.myPlayer);
					}
				}

				PlayGurgle(0.1f);
			}

			if (Timer >= 46) SwitchState(StateHover);
		}

		private void ChargeAI(Player target)
		{
			if (ChargesLeft <= 0) ChargesLeft = Phase;

			float cycle = ChargeTelegraph + ChargeDuration;
			float local = Timer % cycle;

			if (local < ChargeTelegraph)
			{
				NPC.velocity *= 0.88f;
			}
			else if (local == ChargeTelegraph)
			{
				NPC.velocity = NPC.DirectionTo(target.Center) * (Phase == 3 ? 17f : Phase == 2 ? 14f : 11f);
				SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.7f, Volume = 0.75f }, NPC.Center);
				PlayGurgle(-0.3f);
			}
			else
			{
				// Only the travelling portion of a dash is dangerous - see CanHitPlayer.
				dashing = true;
				NPC.velocity *= 0.985f;
			}

			if (local >= cycle - 1)
			{
				ChargesLeft--;
				if (ChargesLeft <= 0)
				{
					SwitchState(StateHover);
					return;
				}
			}

			// Sheds spores mid-dash once enraged, so the arena closes in over time.
			if (Phase == 3 && Timer % 14 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center,
					Main.rand.NextVector2Circular(3f, 3f), ModContent.ProjectileType<SporeBurst>(), 20, 0f, Main.myPlayer);
			}
		}

		private void SummonAI()
		{
			NPC.velocity *= 0.9f;

			if (Timer == 26 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				int count = Math.Min(Phase == 1 ? 1 : 2, MaxBrood - CountBrood());
				for (int i = 0; i < count; i++)
				{
					Vector2 spawn = FindOpenSpawn();
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawn.X, (int)spawn.Y, ModContent.NPCType<Broodling>());
				}
			}

			if (Timer >= 50) SwitchState(StateHover);
		}

		/// <summary>
		/// Big committed attack: she stops dead, the serial plate lights up (free damage
		/// window), then vents a full ring of lingering spores that denies the ground around
		/// her. Punishes staying close, rewards punishing the windup.
		/// </summary>
		private void SporeBloomAI()
		{
			NPC.velocity *= 0.85f;

			if (Timer < BloomWindup)
			{
				plateExposed = true;

				if (Timer % 5 == 0)
				{
					Dust dust = Dust.NewDustDirect(NPC.Center - new Vector2(28f, 22f), 56, 26,
						DustID.AmberBolt, 0f, -1.5f, 120, default, 1.2f);
					dust.noGravity = true;
				}

				if (Timer == 1) PlayGurgle(-0.5f);
				return;
			}

			if (Timer == BloomWindup)
			{
				SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.6f }, NPC.Center);
				PlayGurgle(-0.2f);

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int count = Phase == 3 ? 14 : 10;
					for (int i = 0; i < count; i++)
					{
						Vector2 velocity = (MathHelper.TwoPi * i / count).ToRotationVector2() * Main.rand.NextFloat(3.5f, 5.5f);
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
							ModContent.ProjectileType<SporeBurst>(), 20, 0f, Main.myPlayer);
					}
				}
			}

			if (Timer >= BloomWindup + 30) SwitchState(StateHover);
		}

		/// <summary>Lobs egg pods across the arena - delayed pressure the player can pre-empt.</summary>
		private void EggPodsAI(Player target)
		{
			NPC.velocity *= 0.92f;

			if (Timer == 30 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				// Pods count against the same ceiling, so a pod volley can't stack the arena
				// past what the brood cap allows once they all hatch.
				int headroom = MaxBrood - CountBrood() - CountPods();
				int pods = Math.Min(Phase == 3 ? 4 : 3, headroom);
				for (int i = 0; i < pods; i++)
				{
					// Fan them out around the player so they can't all be ignored from one spot.
					float spreadX = MathHelper.Lerp(-260f, 260f, i / (float)(pods - 1));
					Vector2 landing = target.Center + new Vector2(spreadX, -80f);
					NPC pod = NPC.NewNPCDirect(NPC.GetSource_FromAI(), landing, ModContent.NPCType<EggPod>());
					pod.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -2f);
				}
			}

			if (Timer == 30) PlayGurgle(0.2f);
			if (Timer >= 58) SwitchState(StateHover);
		}

		/// <summary>
		/// The fight's centrepiece. She halts and her brood breaks off the hunt to ring her as
		/// a living shield while she regenerates. Her serial plate stays lit the whole time, so
		/// the whole channel is one long punish window - the player has to weave through the
		/// orbiting brood to reach it. Every broodling still circling when the channel ends is
		/// absorbed for a bigger chunk, so killing the ring is how you deny the heal.
		/// </summary>
		private void ReclaimAI()
		{
			NPC.velocity *= 0.86f;
			plateExposed = true;

			if (Timer == 1)
			{
				SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.9f, Volume = 0.6f }, NPC.Center);
				Announce("Reclamation.Start");
				FormProtectiveRing();
				reclaimHealed = 0;
			}

			if (Timer < ReclaimWindup) return;

			if (Timer < ReclaimWindup + ReclaimChannel)
			{
				// Pods hatching mid-channel would otherwise ignore the ring and go hunting.
				// Re-form periodically so late arrivals are pulled in and the ring re-spaces.
				if (Timer % 20 == 0 && HasLooseBrood()) FormProtectiveRing();

				// Trickle regen scaled by how much of the ring survives - killing guards cuts
				// the heal immediately rather than only at the end.
				int guards = CountOrbitingBrood();
				if (guards > 0 && Timer % 6 == 0)
				{
					Heal(guards * 2);
				}

				if (Timer % 14 == 0 && guards > 0)
				{
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.AmberBolt,
						0f, -2f, 140, default, 1.1f);
					dust.noGravity = true;
				}
				return;
			}

			if (Timer == ReclaimWindup + ReclaimChannel)
			{
				AbsorbRing();
			}

			if (Timer >= ReclaimWindup + ReclaimChannel + 45) SwitchState(StateHover);
		}

		/// <summary>
		/// Applies healing against the per-Reclamation budget and returns what was actually
		/// granted, so a packed arena can't convert into an unbeatable heal.
		/// </summary>
		private int Heal(int amount)
		{
			int allowed = Math.Min(amount, ReclaimHealCap - reclaimHealed);
			if (allowed <= 0) return 0;

			reclaimHealed += allowed;
			NPC.life = Math.Min(NPC.life + allowed, NPC.lifeMax);
			return allowed;
		}

		/// <summary>Pulls every broodling out of its hunt and spaces them evenly around her.</summary>
		private void FormProtectiveRing()
		{
			int total = CountBrood();
			if (total == 0) return;

			int placed = 0;
			foreach (NPC other in Main.npc)
			{
				if (!other.active || other.ModNPC is not Broodling brood) continue;

				brood.BeginOrbit(NPC, MathHelper.TwoPi * placed / total);
				placed++;
			}
		}

		private void AbsorbRing()
		{
			int consumed = 0;
			foreach (NPC other in Main.npc)
			{
				if (!other.active || other.ModNPC is not Broodling brood || !brood.IsOrbiting) continue;

				other.active = false;
				consumed++;
			}

			if (consumed == 0)
			{
				// The player cleared the ring - she gets nothing and the recovery stings.
				Announce("Reclamation.Denied");
				return;
			}

			int heal = Heal(consumed * 70);
			if (heal > 0) CombatText.NewText(NPC.Hitbox, CombatText.HealLife, heal);
			PlayGurgle(-0.4f);

			for (int i = 0; i < 16; i++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.AmberBolt,
					Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 140, default, 1.1f);
			}
		}

		public static int CountBrood()
		{
			int count = 0;
			foreach (NPC other in Main.npc)
			{
				if (other.active && other.ModNPC is Broodling) count++;
			}
			return count;
		}

		private static int CountPods()
		{
			int count = 0;
			foreach (NPC other in Main.npc)
			{
				if (other.active && other.ModNPC is EggPod) count++;
			}
			return count;
		}

		private static bool HasLooseBrood()
		{
			foreach (NPC other in Main.npc)
			{
				if (other.active && other.ModNPC is Broodling { IsOrbiting: false }) return true;
			}
			return false;
		}

		private static int CountOrbitingBrood()
		{
			int count = 0;
			foreach (NPC other in Main.npc)
			{
				if (other.active && other.ModNPC is Broodling { IsOrbiting: true }) count++;
			}
			return count;
		}

		/// <summary>
		/// Picks a spawn point that isn't buried in terrain. She hovers in open air, so her
		/// own centre is a safe fallback when every candidate is solid.
		/// </summary>
		private Vector2 FindOpenSpawn()
		{
			for (int attempt = 0; attempt < 8; attempt++)
			{
				Vector2 candidate = NPC.Center + Main.rand.NextVector2Circular(90f, 60f);
				if (!Collision.SolidCollision(candidate - new Vector2(15f), 30, 30))
				{
					return candidate;
				}
			}
			return NPC.Center;
		}

		/// <summary>
		/// She is only dangerous to touch while a dash is actually travelling. Simply floating
		/// into her does nothing, which is what makes her approachable for melee - otherwise
		/// the only class that has to stand inside her hitbox pays for it constantly.
		/// </summary>
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => dashing;

		/// <summary>
		/// Runs alongside whatever she is doing rather than occupying a slot in her rotation:
		/// loitering in her personal space charges a purge, which fires after a visible
		/// telegraph. Deliberately suppressed during her committed attacks - those windows are
		/// meant to be free damage, and pulsing through them would undo the point of them.
		/// </summary>
		private void UpdateContainmentPulse(Player target)
		{
			if (pulseCooldown > 0) pulseCooldown--;

			if (pulseWindup > 0)
			{
				pulseWindup--;

				// Warning shell drawn at the radius it will actually reach, contracting inward
				// so the danger zone is legible before anything can hurt you.
				float telegraph = pulseWindup / (float)PulseWindup;
				for (int i = 0; i < 3; i++)
				{
					float angle = Main.rand.NextFloat(MathHelper.TwoPi);
					Vector2 position = NPC.Center + angle.ToRotationVector2() * (ContainmentPulse.MaxRadius * telegraph);
					Dust dust = Dust.NewDustPerfect(position, DustID.AmberBolt,
						NPC.DirectionTo(position) * -1.2f, 150, default, 1.1f);
					dust.noGravity = true;
				}

				if (pulseWindup == 0) FireContainmentPulse();
				return;
			}

			bool canPulse = State == StateHover || State == StateSpit
				|| State == StateSummon || State == StateEggPods;

			if (!canPulse || pulseCooldown > 0)
			{
				proximityTimer = 0;
				return;
			}

			if (NPC.Distance(target.Center) <= PulseRange) proximityTimer++;
			else proximityTimer = 0;

			if (proximityTimer < PulseProvokeTime) return;

			proximityTimer = 0;
			pulseWindup = PulseWindup;
			SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.7f, Volume = 0.8f }, NPC.Center);
		}

		private void FireContainmentPulse()
		{
			pulseCooldown = PulseCooldown;
			SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.8f, Volume = 0.7f }, NPC.Center);
			PlayGurgle(-0.5f);

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
					ModContent.ProjectileType<ContainmentPulse>(), 34, 6f, Main.myPlayer);
			}
		}

		private void PlayGurgle(float pitch)
		{
			SoundEngine.PlaySound(SoundID.NPCHit13 with { Pitch = -0.75f + pitch, Volume = 0.7f }, NPC.Center);
			SoundEngine.PlaySound(SoundID.NPCHit4 with { Pitch = -0.9f + pitch, Volume = 0.35f }, NPC.Center);
		}

		private void SwitchState(float next)
		{
			State = next;
			Timer = 0f;
			ChargesLeft = 0f;
			NPC.netUpdate = true;
		}

		// ------------------------------------------------- adaptive plating

		/// <summary>
		/// Helix's whole business was iterating on Product, and she does it mid-fight: the
		/// damage class that has hurt her most gets resisted, so a one-weapon run stalls out.
		/// </summary>
		private void RecordDamage(int damageClass, int amount)
		{
			if (damageClass < 0) return;

			damageByClass[damageClass] += amount;
			if (adaptationCooldown > 0) return;

			int worst = 0;
			for (int i = 1; i < damageByClass.Length; i++)
			{
				if (damageByClass[i] > damageByClass[worst]) worst = i;
			}

			// Only adapt once one class is clearly dominant, so mixed builds are never punished.
			if (damageByClass[worst] < 400 || worst == adaptedClass) return;

			adaptedClass = worst;
			adaptationCooldown = 600;
			Array.Clear(damageByClass);

			Announce($"Adapt.{ClassKey(worst)}");
			SoundEngine.PlaySound(SoundID.Item4 with { Pitch = -0.5f }, NPC.Center);
		}

		private static int ClassifyItem(Item item)
		{
			if (item.CountsAsClass(DamageClass.Melee)) return 0;
			if (item.CountsAsClass(DamageClass.Ranged)) return 1;
			if (item.CountsAsClass(DamageClass.Magic)) return 2;
			if (item.CountsAsClass(DamageClass.Summon)) return 3;
			return -1;
		}

		private static int ClassifyProjectile(Projectile projectile)
		{
			if (projectile.CountsAsClass(DamageClass.Melee)) return 0;
			if (projectile.CountsAsClass(DamageClass.Ranged)) return 1;
			if (projectile.CountsAsClass(DamageClass.Magic)) return 2;
			if (projectile.CountsAsClass(DamageClass.Summon)) return 3;
			return -1;
		}

		private static string ClassKey(int damageClass) => damageClass switch
		{
			0 => "Melee",
			1 => "Ranged",
			2 => "Magic",
			_ => "Summon"
		};

		private void Announce(string key)
		{
			string text = Language.GetTextValue($"Mods.apogean.Matriarch.{key}");
			if (Main.netMode == NetmodeID.Server)
			{
				Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), new Color(150, 220, 150));
			}
			else
			{
				Main.NewText(text, new Color(150, 220, 150));
			}
		}

		/// <summary>Tether shield plus adaptation resistance, minus the exposed-plate window.</summary>
		private void ApplyDefences(int damageClass, ref NPC.HitModifiers modifiers)
		{
			if (TetheredBrood > 0)
			{
				modifiers.FinalDamage *= 1f - 0.08f * TetheredBrood;
			}

			if (damageClass >= 0 && damageClass == adaptedClass)
			{
				modifiers.FinalDamage *= 0.6f;
			}

			if (plateExposed)
			{
				modifiers.FinalDamage *= 1.85f;
			}
		}

		public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
			=> ApplyDefences(ClassifyItem(item), ref modifiers);

		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
			=> ApplyDefences(ClassifyProjectile(projectile), ref modifiers);

		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
			=> RecordDamage(ClassifyItem(item), damageDone);

		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
			=> RecordDamage(ClassifyProjectile(projectile), damageDone);

		// ------------------------------------------------------------ drawing

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter += Phase == 3 ? 0.28f : 0.16f;
			NPC.frameCounter %= Main.npcFrameCount[Type];
			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
		}

		/// <summary>
		/// Port and tethers draw *behind* her body, so the iris only shows where it extends
		/// past her silhouette and the cords appear to run underneath her rather than being
		/// painted over her carapace.
		/// </summary>
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle dot = new(0, 0, 1, 1);

			// Cords run out of the iris port on her underside rather than her centre of mass.
			Vector2 port = NPC.Center - screenPos + new Vector2(0f, 38f);

			int drawn = 0;

			foreach (NPC other in Main.npc)
			{
				if (!other.active || other.type != ModContent.NPCType<Broodling>()) continue;

				Vector2 delta = (other.Center - screenPos) - port;

				// Every broodling is connected, but only the first few actually shield her.
				// Shielding cords are thicker and brighter; the surplus is thinner and dimmer
				// but still clearly a cord - too faint and it just reads as a rendering bug.
				bool shielding = drawn < MaxTetherShield;
				float thickness = shielding ? 4f : 2.5f;
				float alpha = shielding ? 0.65f : 0.45f;

				spriteBatch.Draw(pixel, port, dot, new Color(100, 52, 34) * alpha, delta.ToRotation(),
					new Vector2(0f, 0.5f), new Vector2(delta.Length(), thickness), SpriteEffects.None, 0f);

				spriteBatch.Draw(pixel, port, dot, new Color(214, 172, 92) * (shielding ? 0.6f : 0.35f),
					delta.ToRotation(), new Vector2(0f, 0.5f), new Vector2(delta.Length(), 1.5f),
					SpriteEffects.None, 0f);

				drawn++;
			}

			DrawIrisPort(spriteBatch, port, drawColor);
			return true;
		}

		/// <summary>Plate glow sits on top of her body - it's a targeting tell, not anatomy.</summary>
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (!plateExposed) return;

			Vector2 plate = NPC.Center - screenPos - new Vector2(0f, 18f);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, plate, new Rectangle(0, 0, 1, 1),
				new Color(140, 255, 160) * (0.35f + 0.25f * (float)Math.Sin(Main.GameUpdateCount * 0.25f)),
				0f, new Vector2(0.5f, 0.5f), new Vector2(34f, 14f), SpriteEffects.None, 0f);
		}

		/// <summary>
		/// The iris hatch on her underside that cords run out of and pods drop from. It is part
		/// of her body, so it is always drawn - it just idles shut when nothing is being born.
		/// </summary>
		private void DrawIrisPort(SpriteBatch spriteBatch, Vector2 port, Color drawColor)
		{
			bool birthing = State == StateEggPods || State == StateSummon;

			Texture2D texture = ModContent.Request<Texture2D>(Texture + "_Port").Value;
			int frameHeight = texture.Height / PortFrames;

			// Dilation frames are the back half of the sheet; idle breathes through the front.
			int frame = birthing
				? PortFrames / 2 + (int)(Main.GameUpdateCount / 5 % (PortFrames / 2))
				: (int)(Main.GameUpdateCount / 12 % (PortFrames / 2));

			Rectangle source = new(0, frame * frameHeight, texture.Width, frameHeight);

			// Uses the same drawColor the body is tinted with, so the port sits in her lighting
			// instead of reading as a separate, differently-lit decal stuck on top.
			spriteBatch.Draw(texture, port, source, drawColor, 0f,
				new Vector2(texture.Width / 2f, frameHeight / 2f), 1f, SpriteEffects.None, 0f);
		}

		// --------------------------------------------------------------- loot

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<MatriarchBag>()));
			npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(),
				ModContent.ItemType<MutagenGland>(), 1, 12, 20, 1));
		}

		public override void OnKill()
		{
			FactionProgression.SetMatriarchDowned();
		}

		public override void BossLoot(ref int potionType)
		{
			potionType = ItemID.HealingPotion;
		}
	}
}
