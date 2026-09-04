Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$skillRoot = Join-Path ([Environment]::GetFolderPath('UserProfile')) '.codex/skills/tmodloader-quest-dialogue-authoring'
$validator = Join-Path $skillRoot 'scripts/Test-QuestSpec.ps1'
$example = Join-Path $skillRoot 'references/example-quest-spec.json'
if (-not (Test-Path -LiteralPath $validator) -or -not (Test-Path -LiteralPath $example)) {
    throw "Missing installed quest/dialogue authoring skill files under $skillRoot"
}
& $validator -Spec $example
Write-Host 'PASS: the quest/dialogue pipeline rejects incomplete ownership, reward, recovery, and co-op contracts before implementation.' -ForegroundColor Green
