param(
    [ValidateSet('Status', 'Tree', 'Terrain', 'Background', 'Entity', 'Structure', 'Boss', 'Quest', 'All')]
    [string]$Profile = 'All',
    [switch]$Build
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$hostExecutable = (Get-Process -Id $PID).Path
$profiles = @{
    Status = @('Tools/Test-AuthoringStatus.ps1', 'Tools/Test-VersionedSkills.ps1', 'Tools/Test-GeneratorOwnership.ps1')
    Tree = @('Tools/Test-TreeProductionReadiness.ps1')
    Terrain = @('Tools/Test-WastesTerrainAtlases.ps1', 'Tools/Test-RigidPlantAtlas.ps1', 'Tools/Test-ReportedVisualRegressions.ps1', 'Tools/Test-SurfaceRegression.ps1')
    Background = @('Tools/Test-ForestRestoration.ps1', 'Tools/Test-BackgroundHdContracts.ps1', 'Tools/Test-BackgroundProductionReadiness.ps1')
    Entity = @('Tools/Test-ReportedVisualRegressions.ps1')
    Structure = @('Tools/Test-HelixConstructionSet.ps1', 'Tools/Test-WorldVisualIntegrity.ps1')
    Boss = @('Tools/Test-BossAuthoringPipeline.ps1')
    Quest = @('Tools/Test-QuestDialoguePipeline.ps1')
}

$selectedProfiles = if ($Profile -eq 'All') { @('Status', 'Tree', 'Terrain', 'Background', 'Entity', 'Structure', 'Boss', 'Quest') } else { @($Profile) }
$scripts = [Collections.Generic.List[string]]::new()
foreach ($selected in $selectedProfiles) {
    foreach ($script in $profiles[$selected]) {
        if (-not $scripts.Contains($script)) { $scripts.Add($script) }
    }
}

$failures = [Collections.Generic.List[string]]::new()
foreach ($script in $scripts) {
    $path = Join-Path $root $script
    Write-Host "GATE: $script" -ForegroundColor Cyan
    & $hostExecutable -NoProfile -ExecutionPolicy Bypass -File $path
    if ($LASTEXITCODE -ne 0) { $failures.Add($script) }
}

if ($Build) {
    Write-Host 'GATE: dotnet build --no-restore' -ForegroundColor Cyan
    & dotnet build $root --no-restore
    if ($LASTEXITCODE -ne 0) { $failures.Add('dotnet build --no-restore') }
}

Write-Host ''
if ($failures.Count -gt 0) {
    Write-Host "APOGEAN $Profile GATE: RED" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host "APOGEAN $Profile GATE: STATIC PASS" -ForegroundColor Green
Write-Host 'A static pass does not promote a visual family. Run and inspect its named live fixture, then update Tools/AuthoringStatus.json.'
