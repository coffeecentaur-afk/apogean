---
name: tmodloader-boss-authoring
description: Design, implement, rebalance, or validate tModLoader boss encounters, attack state machines, phase transitions, telegraphs, arena assumptions, multiplayer behavior, contact damage, loot, and progression gates. Use when a boss is repetitive, visually cluttered, unfair to a class, missile-like, desynchronized, over-heals, or lacks a signature mechanic.
---

# tModLoader Boss Authoring

Design an encounter as readable decisions over time. A longer attack list is not depth; the player must recognize intent, choose a response, and find a counter-window.

## Workflow

1. Read progression tier, expected gear/mobility, arena, multiplayer rules, boss lore, existing AI, projectiles/minions, loot, and player feedback.
2. Compare vanilla duration and mobility expectations at the same tier. Study reference encounters for abstract mechanics, then write an original encounter thesis.
3. Write a JSON encounter spec and validate it with `scripts/Test-BossEncounterSpec.ps1` before changing AI.
4. Build an explicit state machine. Each state owns entry initialization, timer, movement, attacks, exit condition, interruption rules, and network synchronization. Prefer compact synchronized `NPC.ai` state; pair every additional `SendExtraAI` field with matching `ReceiveExtraAI`, and update clients on authoritative transitions rather than every tick.
5. Implement one phase and its deterministic debug summon. Test the full loop before adding the next phase.
6. Validate every class and multiplayer size. Include melee access, ranger line-of-sight, mage projectile room, summoner target stability, revive/spectator behavior when the project uses it, and solo failure.
7. Measure duration, damage sources, projectile count, healing, minion population, and state time. Treat visual clutter and sound density as budgets.
8. Promote only after normal/expert/master, solo/host/client, despawn, death, loot bag, arena exit, and repeated-seed tests pass.

## Encounter rules

- Give each boss one signature rule that changes player decisions and belongs to its identity.
- Every damaging action has a readable telegraph, a dodge answer, and a recovery or counter-window.
- Contact damage follows intent. A hovering body can be harmless outside a charge while the charge hitbox deals contact damage.
- Recovery/healing phases expose a bounded risk-reward objective. Cap healing per phase and total encounter healing.
- Minions use population caps and phase-aware commands. Newly spawned minions join the current command state when intended.
- Arena assumptions follow the player's unlocked movement. Test a reasonable handmade arena, not a perfect developer chamber.
- Server or authoritative host owns projectile/minion spawns and persistent state; synchronize transitions and random choices.
- Treat telegraphs as presentation derived from synchronized start time, target, and seed. Clients may draw them, but clients do not choose their outcome.
- Reuse attacks only when their combination changes. Avoid a random bag of projectiles.
- Loot follows Terraria difficulty contracts: individual bags where appropriate, expert items, master relic/trophy/pet rules, and no unintended minion coin drops.

Read [boss-contracts.md](references/boss-contracts.md) before implementing a new boss or major phase.
