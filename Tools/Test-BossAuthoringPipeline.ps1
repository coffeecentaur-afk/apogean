Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$skillRoot = Join-Path ([Environment]::GetFolderPath('UserProfile')) '.codex/skills/tmodloader-boss-authoring'
$validator = Join-Path $skillRoot 'scripts/Test-BossEncounterSpec.ps1'
$example = Join-Path $skillRoot 'references/example-boss-spec.json'
if (-not (Test-Path -LiteralPath $validator) -or -not (Test-Path -LiteralPath $example)) {
    throw "Missing installed boss-authoring skill files under $skillRoot"
}
& $validator -Spec $example
Write-Host 'PASS: the boss authoring pipeline rejects incomplete encounter state cards before AI implementation.' -ForegroundColor Green
