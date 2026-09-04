---
name: tmodloader-entity-authoring
description: Design, implement, resize, or validate tModLoader NPC and item sprites, frame sheets, hitboxes, animation, movement silhouettes, and Terraria-scale readability. Use when enemies or drops are too tiny, frames disconnect, sprites flip or rotate badly, hitboxes mismatch, or a modded creature needs a disciplined concept-to-live-fixture workflow.
---

# tModLoader Entity Authoring

Build entities at gameplay scale. A detailed concept image is not a sprite sheet, and enlarging a weak sprite is not a redesign.

## Required workflow

1. Read the entity's combat role, AI, hitbox, frame count, sprite direction, and existing art source.
2. Compare its intended physical size with two vanilla entities visible during the same progression tier.
3. Define frame canvas, visible silhouette range, grounded baseline or flight anchor, default facing, palette, and animation purpose before drawing.
4. Add a failing static sheet contract with `scripts/Test-EntitySheet.ps1`.
5. Author one readable key frame first. Test it at 1x beside the reference entities before completing animation.
6. Build animation without moving the body anchor between frames unless the motion requires it. Keep feet/baseline stable for grounded actors.
7. Match `NPC.width` and `NPC.height` to fair collision, not transparent canvas size. Use draw offsets when art extends outside the hitbox.
8. Validate idle, pursuit, attack, hit, death, direction changes, slopes/platforms, lighting, and multiplayer synchronization in a deterministic arena.

## Readability rules

- Ambient particles and critters may be tiny; hostile silhouettes must communicate threat at ordinary zoom.
- Use a few large masses and a restrained palette. One-pixel noise cannot carry anatomy or faction identity.
- A four-frame sheet height must divide cleanly by four. Treat any remainder as a build-blocking asset defect.
- Inventory materials need a centered, recognizable silhouette and adequate occupied bounds; do not rely on tooltip scale.
- Flying enemies should bank or steer deliberately. Do not rotate the whole sprite like a missile unless that is the creature's concept.
- Test contact damage and draw size independently.

Read [entity-contracts.md](references/entity-contracts.md) before introducing a new enemy family.
