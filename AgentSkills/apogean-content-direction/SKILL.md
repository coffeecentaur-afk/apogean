---
name: apogean-content-direction
description: Direct design and implementation decisions for the Apogean tModLoader project using its design bible, Wayfinder, progression, Wastes/Maw ecology, corporations, space-cowboy fantasy, and evidence-gated content workflow. Use whenever adding or revising Apogean gameplay, lore, art direction, quests, factions, bosses, structures, or progression.
---

# Apogean Content Direction

Read the repository's `CONTEXT.md`, `DESIGN_BIBLE.md`, `ACT1_ROADMAP.md`, `AUTHORING_WORKFLOW.md`, relevant ADRs/research, and current Wayfinder before proposing or implementing content. These are authoritative over remembered chat details.

## Direction

- Preserve Terraria's building, exploration, class, difficulty, and progression grammar. Apogean expands and reframes it.
- The neutral Wastes show a civilization picked clean; the Maw is the active amber-yellow biological threat.
- The player fantasy grows from wasteland lone rider to corporate kingmaker to post-Moon Lord spacefarer.
- Kessler, Helix, and Sentrix are durable institutions under a distant galactic quota system, never uncomplicated friends.
- Choices change alliance, dialogue, access, prices, and encounter route while keeping major content recoverable through raids or harder alternatives.
- World landmarks exist from creation and telegraph future progression even while sealed.
- Content quality grows from validated foundations: world placement, tile contracts, backgrounds, and structures before dependent NPCs or bosses.

## Development gate

Before coding a requested asset family, route to the relevant tModLoader authoring skill. Keep candidates outside production until their static and live evidence gates pass. A successful compiler run changes implementation status, not visual status.

For any substantial feature, update the Wayfinder using these states:

- `specified`: player experience and dependencies agreed;
- `contracted`: engine/art/test contracts recorded;
- `fixture-pass`: deterministic in-game proof accepted;
- `integrated`: production world/progression path accepted;
- `polished`: final art, sound, balance, multiplayer, and documentation accepted.

Do not advance a dependent branch while its foundation is below `fixture-pass`, unless the user explicitly chooses a throwaway prototype.

## Inspiration

Use references as a mechanics-and-language ledger, not a parts bin. Borderlands can inform explicit availability, objectives, reward ownership, optional discoveries, co-op state, redundant cues, and grim humor. Titanfall can inform functional Kessler silhouettes, rapid playable action-block prototypes, readable setup/commitment/counterplay, mobility tests, warning lines, missiles, and damaged-machine states. Terraria and mature mods can inform engine-compatible scale and progression. Produce original names, silhouettes, layouts, dialogue, timing, and encounter combinations.

Read [project-map.md](references/project-map.md) when selecting what to build next. Read the repository's research report on content workflows when translating an external reference.
