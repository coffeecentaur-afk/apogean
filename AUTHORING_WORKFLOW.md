# Apogean Authoring Workflow

This is the production rule for adding visual and gameplay content. It prevents a mechanically valid placeholder from silently becoming the foundation for another unfinished family.

## One-family loop

1. **Specify the player promise.** State what the player sees, understands, does, and receives. Name the closest Terraria behavior that must remain familiar.
2. **Contract the engine behavior.** Record the owning tModLoader type, exact dimensions/framing, authority, lifetime, placement bounds, failure behavior, and test fixture.
3. **Build the smallest probe.** Author one tile, tree set, room, background composition, enemy, or boss state that can disprove the riskiest assumption.
4. **Run static gates.** Use `Tools/Invoke-ApogeanContentGate.ps1 -Profile <family>`. Static success permits an in-game render; it never approves appearance.
5. **Render the deterministic fixture.** Capture ordinary gameplay scale plus the family's failure cases. Use the same production draw/placement path whenever possible.
6. **Record the evidence.** Update `Tools/AuthoringStatus.json` as `fixture-pass`, `integrated`, `polished`, or `rejected`. A rejection keeps its useful technical proof but blocks dependent production work.
7. **Promote or replace.** Expand a family only after its probe passes. Replace rejected art at the same seam instead of layering compensating overlays over it.

## Evidence states

| State | Meaning |
| --- | --- |
| `specified` | Player experience and dependencies are agreed. |
| `contracted` | Engine, art, authority, failure, and fixture contracts are written. |
| `fixture-pass` | Static checks, clean build, and deterministic live fixture pass. |
| `integrated` | The production world/progression path passes, including reload and relevant multiplayer behavior. |
| `polished` | Final visual/audio/balance/documentation review is accepted. |
| `rejected` | A live failure or explicit review blocks promotion; retain the evidence and replace the candidate. |

## Family order

The default dependency order is terrain framing → native trees/vegetation → background rendering/routing → faction materials/furniture → complete structure templates → entities → boss encounters → quests/dialogue/progression. Design specifications may run ahead, but unfinished foundations do not gain production dependents.

Installed Codex skills enforce the focused contracts: `$tmodloader-atlas-authoring`, `$tmodloader-tree-authoring`, `$tmodloader-background-authoring`, `$tmodloader-structure-authoring`, `$tmodloader-entity-authoring`, `$tmodloader-boss-authoring`, `$tmodloader-quest-dialogue-authoring`, and `$apogean-content-direction`.

## Reference discipline

Other mods and games are studied for reusable principles: engine ownership, placement safety, encounter readability, functional silhouettes, objective clarity, co-op state, and environmental storytelling. Apogean does not copy source, assets, layouts, names, dialogue, timing, or recognizable encounter combinations. Research provenance and the verified/inferred boundary live in `RESEARCH_APOGEAN_CONTENT_WORKFLOWS.md`.

## Current commands

```powershell
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Status
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Tree
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Background
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Structure
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Boss
pwsh -File Tools/Invoke-ApogeanContentGate.ps1 -Profile Quest
```

An intentionally red gate is useful: it names what is still missing. Never weaken a production threshold merely to make the report green.

## Background lesson retained — 2026-09-04

Measure a source landmark against its actual Windows game capture. A 111-pixel truck became 150 pixels wide despite `Draw(scale:1)`, because Terraria applies forced minimum background zoom and temporarily logical screen dimensions. The scoped correction produced110 pixels without changing the user's zoom. `Tools/Test-WastesLandmarkScale.ps1` retains this red/green regression; `Art/Validation/WastesLandscapeV1/README.md` records the full bounded proof. Never infer native detail from texture dimensions or draw scale alone. Check composition behind real terrain as well as in an empty gallery, and keep art, coverage, routing and performance acceptance separate.
