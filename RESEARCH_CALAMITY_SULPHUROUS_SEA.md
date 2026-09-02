# Calamity Sulphurous Sea: Acid Water, Breath, and Abyss Pressure

> **Decision superseded 2026-09-01.** The source findings remain valid, but the earlier custom acid-tile recommendation is no longer binding. `WORLD_PLACEMENT_ATLAS.md` removes fake acid tiles from normal world generation and treats any real-water acid basin as an optional locality prototype.

## Scope and evidence baseline

**Proven — versions audited.** Calamity findings are pinned to the official `CalamityTeam/CalamityModPublic` `1.4.4` branch at commit [`1a8cebd27ec5615316b78f71973446b5528d2b78`](https://github.com/CalamityTeam/CalamityModPublic/commit/1a8cebd27ec5615316b78f71973446b5528d2b78). tModLoader API findings are pinned to the locally installed build's official source commit [`666f69962d3bdffde54fc14025f02634965b4e7c`](https://github.com/tModLoader/tModLoader/commit/666f69962d3bdffde54fc14025f02634965b4e7c). No community wiki or third-party explanation is used.

**Bottom line.** Calamity does **not** add a custom physical acid liquid. The Sulphurous Sea and Abyss contain vanilla `LiquidID.Water`. A `ModWaterStyle` and Calamity's client-only renderer patches make that water look sulphuric, while separate `ModPlayer` code interprets being underwater in the Sulphurous Sea or first Abyss layer as an acid-exposure meter. Abyss depth, breath loss, defense loss, and the combat debuff named Hadopelagic Pressure are separate systems.

## 1. Is it real water, a water style, or custom tiles?

**Proven.** It is real vanilla water. Sulphurous Sea generation explicitly places `LiquidID.Water`, fills it to `byte.MaxValue`, and later runs Calamity's water-settling routine. Abyss generation likewise converts lava to water and fills enclosed air pockets with `LiquidID.Water`. ([Sulphurous Sea water placement](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/SulphurousSea.cs#L405-L417), [explicit water type](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/SulphurousSea.cs#L735-L744), [settling](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/SulphurousSea.cs#L1197-L1203), [Abyss filling](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/World/Abyss.cs#L550-L560))

**Proven.** `SulphuricWater` inherits `ModWaterStyle`; it selects textures, waterfall style, splash dust, droplets, rain, hair color, per-vertex color, light, and a foam post-draw effect. It does not register a fifth liquid. ([SulphuricWater](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Waters/SulphuricWater.cs#L16-L54), [foam and light](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Waters/SulphuricWater.cs#L54-L130), [tModLoader `ModWaterStyle`](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModWaterStyle.cs#L9-L96))

**Proven.** Calamity adds a client-only `SpecialLiquidDrawingSystem` that IL-hooks Terraria's liquid renderer and hooks lighting, slopes, old-water drawing, and waterfalls. Those hooks alter color, emitted light, and post-draw effects for the currently selected style; they still operate on water tiles and do not add liquid physics. ([renderer hook registration and light selection](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Systems/Graphic/LiquidSystem/SpecialLiquidDrawingSystem.cs#L18-L64), [normal-liquid IL injection](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Systems/Graphic/LiquidSystem/SpecialLiquidDrawingSystem.cs#L84-L166))

## 2. Biome detection and scene rendering

**Proven.** The Sulphurous Sea biome activates from either at least 300 nearby sulphurous terrain tiles, or a world-edge positional test while the player is not in the Abyss. The positional test uses the Abyss side of the world, an outer 435-tile horizontal band, vertical limits, and excludes subworlds. ([biome predicate](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/BiomeManagers/SulphurousSeaBiome.cs#L63-L92), [tile composition](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Systems/Tile/BiomeTileCounterSystem.cs#L56-L73))

**Proven.** The active biome supplies `SulphuricWater`, a surface-background style, `BiomeHigh` scene priority, music, map/bestiary art, and a custom sky. `SpecialVisuals` activates and deactivates the sky; the sky then fades opacity and draws independently scrolling layers. ([scene properties and sky activation](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/BiomeManagers/SulphurousSeaBiome.cs#L15-L23), [special visuals](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/BiomeManagers/SulphurousSeaBiome.cs#L94-L108), [custom sky parallax and fade](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Skies/SulphurSeaSky.cs#L38-L113))

**Proven.** Abyss layers are separate `ModBiome`s. They combine a world-side position/depth predicate with minimum nearby layer-specific tile counts, and each layer selects another water style. ([base Abyss position test](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/BiomeManagers/AbyssLayer1Biome.cs#L15-L47), [layer-one activation and style](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/BiomeManagers/AbyssLayer1Biome.cs#L49-L81), [deeper layer example](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/BiomeManagers/AbyssLayer4Biome.cs#L24-L40))

## 3. Player acid damage, debuffs, breath, and authority

**Proven — acid meter.** In `UpdateBadLifeRegen`, Calamity increments `SulphWaterPoisoningLevel` while the player is in the Sulphurous Sea or Abyss layer one, is underwater, and lacks the listed exclusions or protection. Calamity's `IsUnderwater` is exactly `Collision.DrownCollision`, so the test is vanilla liquid-at-the-player's-breathing-area behavior, not a water-style query. ([meter predicate and damage](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerLifeRegen.cs#L144-L190), [`IsUnderwater`](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Utilities/PlayerUtils.cs#L329-L331))

**Proven — timing and hit.** The unmodified meter takes 720 ticks, or 12 seconds at 60 TPS, to fill and takes 150 ticks, or 2.5 seconds, to drain from full. Sulphurskin, the sulphurous armor set, and Corrosive Spine each halve accumulation; Abyss layer one multiplies it by `0.33`. At full, the meter resets and calls `Player.Hurt` for `min(25% of effective maximum life, 150)`. ([constants](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayer.cs#L1406-L1413), [multipliers and hit](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerLifeRegen.cs#L166-L190))

**Proven — not the similarly named debuff.** Environmental water does not apply the `SulphuricPoisoning` buff in this path. That is a separate combat debuff whose `ModBuff.Update` sets a flag; the general damage-over-time code then applies negative life regeneration. ([SulphuricPoisoning flags](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Buffs/DamageOverTime/SulphuricPoisoning.cs#L37-L45), [separate DoT application](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerLifeRegen.cs#L88-L106))

**Proven — execution context.** tModLoader documents `UpdateBadLifeRegen` as running on local, server, and remote clients. Calamity's acid-meter block contains no explicit `Main.myPlayer`, net-mode, or server-authority guard around the meter and `Player.Hurt`. By contrast, the Abyss depth routine explicitly checks `Main.myPlayer == Player.whoAmI`. ([tModLoader hook contract](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModPlayer.cs#L192-L198), [Abyss owner check](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerMiscEffects.cs#L3175-L3185))

**Inference.** Calamity relies on Terraria's player-damage synchronization to reconcile that direct hit. The inspected code does not establish a custom authoritative acid packet or server-only calculation. Apogee should not assume the pattern is duplication-proof without dedicated host/client, dedicated-server, latency, and reconnect tests.

## 4. Abyss pressure/depth versus Sulphurous Sea acid

**Proven.** The ambient Abyss system is not the Sulphurous acid meter. It computes a continuous depth ratio, raises darkness with depth, removes up to 120 defense before equipment reductions, and shortens the interval between breath deductions as depth increases. Equipment contributes to a capped breath-interval multiplier. At zero breath, it directly removes up to 12 life per update before resistance and invokes a special death path. ([depth, defense, and breath interval](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerMiscEffects.cs#L3184-L3253), [breath and zero-breath life loss](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerMiscEffects.cs#L3255-L3310))

**Proven.** Being in the Abyss but not underwater is also hazardous: while above 100 life, Calamity suppresses positive regeneration and subtracts 160 life-regeneration units, corresponding to 80 HP/s before Calamity's difficulty multiplier. This is labeled “air drowning in the Abyss” in source. ([air-drowning path](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerLifeRegen.cs#L385-L443))

**Proven.** `HadopelagicPressure` is a separate combat debuff, not the automatic depth-pressure calculation. It applies 40 negative life-regeneration units to players, or 20 HP/s, and while present it makes the Abyss breath interval 50% shorter, reduced to 20% shorter by the Abyssal Diving Suit. Abyss enemies such as Reaper Shark apply it for 300 ticks on damaging contact. ([debuff flag](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Buffs/DamageOverTime/HadopelagicPressure.cs#L12-L36), [player DoT](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerLifeRegen.cs#L115-L130), [breath acceleration](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerMiscEffects.cs#L3252-L3262), [enemy application](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/NPCs/Abyss/ReaperShark.cs#L712-L721))

## 5. NPC and projectile behavior

**Proven.** The environmental acid-meter implementation targets `Player` only. It does not scan, debuff, or damage NPCs or projectiles. `SulphuricPoisoning` can affect an NPC when some separate attack applies that buff, but that is unrelated to immersion in Sulphurous Sea water. ([player-only meter](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerLifeRegen.cs#L144-L190), [NPC combat-debuff flag](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/Buffs/DamageOverTime/SulphuricPoisoning.cs#L37-L45))

**Proven.** Water-dependent NPC spawning uses vanilla water state. For example, Reaper Shark requires both Abyss layer four and `spawnInfo.Water`. The water style itself exposes visual hooks, not NPC or projectile gameplay hooks. ([Reaper Shark spawn predicate](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/NPCs/Abyss/ReaperShark.cs#L718-L723), [tModLoader water-style API](https://github.com/tModLoader/tModLoader/blob/666f69962d3bdffde54fc14025f02634965b4e7c/patches/tModLoader/Terraria/ModLoader/ModWaterStyle.cs#L9-L96))

**Inference.** Because the physical liquid is vanilla water, ordinary NPC wet AI and projectile water slowdown/`ignoreWater` behavior remain vanilla unless an individual Calamity NPC or projectile overrides them. The sulphuric appearance alone supplies no acid immunity, corrosion, or projectile transformation semantics.

## 6. Pumps, buckets, fishing, and compatibility consequences

**Proven — fishing.** Sulphurous fishing is layered onto vanilla `FishingAttempt`. Calamity reads normal water-pool information, lava/honey flags, player position, and biome flags, then replaces catches or crates. Its `canSulphurFish` predicate is based on the player being near the relevant world edge or in the Sulphurous Sea/Abyss; it does not inspect an “acid” liquid ID. ([fishing predicate](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerFishing.cs#L100-L140), [crate replacement](https://github.com/CalamityTeam/CalamityModPublic/blob/1a8cebd27ec5615316b78f71973446b5528d2b78/CalPlayer/CalamityPlayerFishing.cs#L230-L238))

**Inference from the proven representation.** Pumps and buckets move or collect the underlying vanilla water, not “Sulphurous water.” No acid provenance is stored in the liquid tile. Water moved out of the biome remains ordinary water and loses the scene style and acid predicate; ordinary water moved into an active Sulphurous biome is drawn with the selected style and can satisfy the underwater hazard predicate. The same consequence applies to player-built pools and water introduced by other mods.

**Compatibility consequence.** Calamity gains full water behavior—flowing, settling, swimming, drowning, pumps, buckets, fishing, wet NPC spawns, and standard water/projectile interactions—but pays for it with biome-wide semantics. It cannot identify one pool as acid and another nearby pool as clean using `ModWaterStyle` alone. Its custom renderer IL hooks add presentation power but also create maintenance risk when Terraria/tModLoader renderer internals change.

## 7. What Apogee Wastes should learn or avoid

### Proven lessons

1. Calamity demonstrates a viable pattern for a **large, continuous, swimmable sea**: vanilla water provides all physical behavior; a biome-selected `ModWaterStyle` provides presentation; a `ModPlayer` meter provides the hazard.
2. The acid hazard is not encoded in the liquid. It is a player-state interpretation of `biome + underwater + equipment`.
3. Abyss depth pressure is an independent regional subsystem. Hadopelagic Pressure is an additional combat debuff, not the name of the ambient depth formula.
4. Fishing and aquatic spawning work because the liquid remains vanilla water and Calamity adds biome-aware overrides.

### Recommendation for Apogee

Keep the previously selected animated custom-tile acid pools for the localized Maw stomach-acid requirement. Apogee explicitly needs authored pools, exact locality, no recoloring of unrelated water, no bucket/pump transport, and no accidental acidification of player-built water. Calamity's approach conflicts with those requirements even though it is appropriate for an ocean-sized biome.

If Apogee later creates a genuinely continuous swimmable acid sea, then Calamity's architecture becomes relevant: use vanilla water plus a standard `ModWaterStyle`, a separately synchronized exposure meter, and explicit fishing/spawn overrides. Even then:

- do not treat the client-selected water style as damage authority;
- do not copy Calamity's renderer IL edits unless per-vertex coloration or safe-zone gradients justify the version-maintenance cost;
- keep acid exposure, depth pressure, breath depletion, and combat debuffs as separate named systems;
- prefer negative life regeneration for continuous damage and carefully owner-gate or server-authorize discrete `Player.Hurt` events;
- test player-built water, pumps, buckets, biome edges, overlapping scene effects, fishing, dedicated servers, and high-latency multiplayer explicitly.

## Resolution

Calamity's Sulphurous Sea does not overturn tModLoader's four-liquid limitation. It disguises and interprets vanilla water at biome scale. That is the key transferable technique—and the key reason it should **not** replace Apogee's local acid-tile design.
