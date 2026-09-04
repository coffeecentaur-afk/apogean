# Apogean project map

## Source-of-truth order

1. Current explicit user decision.
2. `DESIGN_BIBLE.md` and accepted ADRs.
3. `CONTEXT.md` terminology and invariants.
4. `ACT1_ROADMAP.md` sequencing and Wayfinder state.
5. Focused research reports and validation records.
6. Existing code, which may still be a placeholder or known defect.

When sources conflict, record the conflict before editing. Do not silently make placeholder code authoritative.

## Foundation sequence

1. World atlas, placement safety, and generation pass order.
2. Wastes/Maw tile, wall, tree, liquid, and conversion contracts.
3. Surface/underground/underworld background routing and render coverage.
4. Faction material families, furniture, templates, and terrain integration.
5. Ambient entities and combat readability.
6. Bosses, quests, dialogue, faction systems, and rewards.
7. Hardmode corporate arrivals, alliance war, Moon Lord transition, ship, and star chart.

This is dependency order, not a ban on design work. A later feature can be specified while its implementation waits.

## Review questions

- What does the player read, decide, and gain?
- What Terraria behavior remains familiar?
- Which progression state owns this content?
- Which world and multiplayer state is authoritative?
- What is the signature Apogean idea rather than a reference-game idea?
- Which deterministic fixture proves it?
- What evidence is needed to change its Wayfinder state?

## Evidence locations

- Human workflow: `AUTHORING_WORKFLOW.md`
- Machine status and blockers: `Tools/AuthoringStatus.json`
- Verified engine and inspiration research: `RESEARCH_APOGEAN_CONTENT_WORKFLOWS.md`
- Static family gates: `Tools/Invoke-ApogeanContentGate.ps1`
