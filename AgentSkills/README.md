# Apogean Codex Skills

This directory is the Git-versioned snapshot of the focused authoring skills used to build Apogean. Installed copies live under `%USERPROFILE%/.codex/skills/` so Codex can discover them.

The repository snapshot and installed copy must remain text-equivalent. After editing a skill, update both locations and run:

```powershell
pwsh -File Tools/Test-VersionedSkills.ps1
```

The skills divide work by engine contract rather than content theme: connected atlases, entities, trees, backgrounds, structures/furniture, bosses, quests/dialogue, and Apogean-specific direction. The project-wide workflow and evidence states are documented in `AUTHORING_WORKFLOW.md`.
