# Quest and dialogue contracts

## Required state card

- stable ID and schema version;
- availability gate and redundant player-facing cue;
- entry interaction and accepting player/world owner;
- ordered stages with one observable verb each;
- completion event and authority;
- reward owner, delivery, duplicate protection, and recovery;
- dialogue nodes, conditions, choices, effects, and fallback node;
- per-player versus per-world fields;
- reload, late-join, mixed-progress, and disconnect behavior;
- failure, abandonment, retry, and missing-landmark fallback;
- visible next action.

## Multiplayer decisions

For major Apogean alliances, all online players gather at the command interaction and receive the same vote. Record unanimity/quorum, timeout, decline behavior, and the host-only override interaction that appears after a failed vote. Individual trespass or attacks remain per-player hostility until the story contract explicitly promotes a world-level conflict.

## Evidence matrix

Test unavailable/available/active/complete/rejected states, every dialogue edge, full inventory, duplicate completion, reload at every stage, solo, two clients, late join, one disconnecting voter, failed vote, host override, and a missing or skipped landmark.

## Research basis

The schema is an Apogean inference. First-party Borderlands material supports treating level/mission synchronization, instanced reward ownership, and redundant loot cues as explicit production concerns; it does not define this schema. See `RESEARCH_APOGEAN_CONTENT_WORKFLOWS.md` for sources and boundaries.
