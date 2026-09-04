# Boss contracts

## Encounter thesis

In one sentence, state what the player learns to read and exploit. Name the signature rule and how it expresses the boss's lore.

## State card

Every AI state records:

- stable state ID and phase availability;
- entry setup and synchronized random choices;
- movement rule;
- telegraph duration and channels;
- damaging action and hitbox owner;
- player dodge answer;
- recovery/counter-window;
- exit condition and timeout;
- minion/projectile cleanup;
- host/client authority.

## Balance telemetry

Capture encounter duration, state time share, hits by source, maximum simultaneous projectiles, minion cap, healing per phase/encounter, melee uptime, and deaths by player count. Compare trends rather than balancing from one victory.

## Inspiration ledger

For each reference, record the abstract lesson only: such as telegraph hierarchy, vertical movement, contract pacing, shield interaction, or visual damage. Create original timing, geometry, art, names, and combinations.

## Evidence gate

An encounter needs a validated spec, deterministic summon, automated/static contracts, telemetry from representative equipment, and successful solo plus multiplayer live runs. A successful build is not encounter validation.

## Primary references

- tModLoader stable `ModNPC`: https://docs.tmodloader.net/docs/stable/class_mod_n_p_c.html
- tModLoader stable `NPC`: https://docs.tmodloader.net/docs/stable/class_n_p_c.html
- ExampleMod custom AI: https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/ExampleCustomAISlimeNPC.cs
- ExampleMod minion boss: https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/NPCs/MinionBoss
