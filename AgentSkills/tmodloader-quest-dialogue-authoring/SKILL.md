---
name: tmodloader-quest-dialogue-authoring
description: Specify, implement, or validate tModLoader quests, contracts, dialogue trees, faction reputation, co-op voting, reward ownership, retries, and persistent progression. Use when a quest chain, NPC conversation, alliance choice, side objective, raid, or world/player state needs a reliable Terraria-compatible contract.
---

# tModLoader Quest and Dialogue Authoring

Treat every quest as a state contract linking world evidence, player ownership, dialogue, objectives, rewards, failure, and the next visible action. Do not begin with prose alone.

## Workflow

1. Read the progression owner, world/player save systems, NPC interaction, multiplayer packet path, landmark record, and current story gate.
2. Write a versioned JSON quest spec and run `scripts/Test-QuestSpec.ps1` before implementation.
3. Mark each state as per-player, per-world, or derived. The server owns world mutations, completion, rewards, and alliance votes; clients present synchronized state.
4. Build one vertical slice: availability cue → acceptance dialogue → one observable objective → completion → recoverable reward → visible next action.
5. Make state legible through more than text. Pair dialogue/UI with a changed room, NPC, prop, signal, map marker, enemy, or access route.
6. Test full inventory, duplicate interaction, death, abandonment, missing landmark, reload, disconnect, late join, mixed player progress, and host migration or host override where applicable.
7. Add branches only after the linear recovery path is reliable. Every rejection, hostility state, and raid route must retain a way to obtain major class content.
8. Promote only after single player, host-and-play, dedicated server, and mixed-progress multiplayer evidence passes.

## Contracts

- Availability names the exact gate and how the player notices it.
- Objectives contain one concrete, server-observable verb. Split compound objectives into ordered stages.
- Rewards name owner, delivery fallback, duplicate protection, and full-inventory behavior.
- Dialogue choices state their scope and durable consequence; flavor is not stored as a world flag unless another system consumes it.
- World decisions define quorum, timeout, failed vote, offline-player policy, and explicit host override.
- Optional contracts add character, resources, alternate access, or completion goals without hiding campaign recovery.
- Keep content original. Borderlands can inform explicit objectives, reward texture, co-op ownership, and grim humor, but not names, dialogue, quest text, layouts, or recognizable mission scripts.

Read [quest-contracts.md](references/quest-contracts.md) before implementing a chain.
