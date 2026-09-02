---
status: accepted
---

# Generate bounded Apogee landmarks after vanilla final cleanup

Apogee is an additive Terraria expansion, not a total replacement for Terraria's world-generation pipeline. Early Maw carving was repeatedly overwritten or obstructed by later vanilla micro-biomes, natural objects, containers, hives, and Underworld structures. Replacing vanilla's producers wholesale, as a comprehensive world-generation overhaul can do, would make Apogee much less compatible with ordinary Terraria and other content mods.

Apogee therefore surveys, solves, reserves, and constructs its finite campaign atlas immediately after vanilla's `Final Cleanup` pass. The planner sees the completed world, rejects protected landmarks and containers, routes the Maw around completed structures, and only clears an explicit allowlist of natural terrain and cave clutter inside its accepted envelope. Campus and ruin passes then consume the saved atlas instead of choosing new locations independently.

## Consequences

- The same seed and mod set produce a deterministic saved atlas and validation hash.
- Vanilla landmarks and completed micro-biomes are evidence for placement, not disposable obstacles.
- Apogee can make bounded natural-terrain edits without adopting a Remnants-style replacement generator.
- `GenVars.structures` protects Apogee landmarks from cooperating passes that run later, but cannot stop a mod that directly rewrites tile memory or also assumes it owns the post-cleanup phase.
- Compatibility with major world-generation mods remains an explicit validation matrix item; it is never inferred from one successful vanilla world.
- Existing worlds are not retrofitted. The complete campaign requires a newly generated supported world.
