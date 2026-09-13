[CmdletBinding()]
param(
    [ValidateSet('Status', 'Tree', 'Terrain', 'MawMaterials', 'MawAnatomy', 'MawShallow', 'MawTeeth', 'MawCave', 'Performance', 'Persistence', 'Background', 'Entity', 'Structure', 'Boss', 'Quest', 'All')]
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
    Performance = @('Tools/Test-QAPerformanceStatistics.ps1', 'Tools/Test-QARequestPublication.ps1', 'Tools/Test-QASharedWallAssets.ps1')
    Persistence = @('Tools/Test-QAWorldPersistence.ps1', 'Tools/Test-QAHistoryCoverage.ps1', 'Tools/Test-QAExitDispatch.ps1')
    MawCave = @('Tools/Test-MawCaveDryMask.ps1', 'Tools/Test-MawCaveOverlayGuard.ps1', 'Tools/Test-MawEvidenceRenderFailures.ps1')
    MawShallow = @('Tools/Test-MawShallowTraversalPlan.ps1', 'Tools/Test-MawShallowMotionTrace.ps1', 'Tools/Test-MawShallowReturnInputs.ps1', 'Tools/Test-MawShallowReturnTrace.ps1', 'Tools/Test-MawShallowQaScope.ps1', 'Tools/Test-MawEvidenceRenderFailures.ps1', 'Tools/Test-CloseBackgroundDimensions.ps1')
    MawTeeth = @('Tools/Test-MawToothPlacement.ps1', 'Tools/Test-MawClusterPlayerCurves.ps1', 'Tools/Test-MawPlayerCurveValidators.ps1')
    MawAnatomy = @('Tools/Test-MawAnatomyPlan.ps1', 'Tools/Test-MawAnatomyCandidate.ps1', 'Tools/Test-MawAnatomyMutations.ps1', 'Tools/Test-MawHangingFiberCandidate.ps1', 'Tools/Test-MawAmberEmission.ps1', 'Tools/Test-MawAmberCompiler.ps1')
    MawMaterials = @('Tools/Test-PackedMawMaterials.ps1', 'Tools/Test-PackedMaterialMap.ps1', 'Tools/Test-PackedMaterialMutations.ps1', 'Tools/Test-QAAssetSnapshotMutations.ps1', 'Tools/Test-MawNativeSuite.ps1', 'Tools/Test-MawFiberGrowthPolicy.ps1')
    Terrain = @('Tools/Test-WastesTerrainAtlases.ps1', 'Tools/Test-MawStructuralWall.ps1', 'Tools/Test-MawFang.ps1', 'Tools/Test-MawFangValidator.ps1', 'Tools/Test-RigidPlantAtlas.ps1', 'Tools/Test-ReportedVisualRegressions.ps1', 'Tools/Test-SurfaceRegression.ps1')
    Background = @('Tools/Test-MaskedBackgroundExport.ps1', 'Tools/Test-WastesCityMask.ps1', 'Tools/Test-WastesHeightLock.ps1', 'Tools/Test-WastesForegroundDepth.ps1', 'Tools/Test-WastesRunningLiveValidator.ps1', 'Tools/Test-WastesTelemetryExport.ps1', 'Tools/Test-WastesRuinLayoutValidator.ps1', 'Tools/Test-WastesFixedSections.ps1', 'Tools/Test-WastesHeightMutations.ps1', 'Tools/Test-ForestRestoration.ps1', 'Tools/Test-BackgroundHdContracts.ps1', 'Tools/Test-BackgroundProductionReadiness.ps1')
    Entity = @('Tools/Test-ReportedVisualRegressions.ps1')
    Structure = @('Tools/Test-ArrivalPodNativePipeline.ps1', 'Tools/Test-ArrivalPodFooting.ps1', 'Tools/Test-ArrivalPodLiveValidator.ps1', 'Tools/Test-ArrivalSitePlanner.ps1', 'Tools/Test-ArrivalSiteMutations.ps1', 'Tools/Test-ArrivalSiteLiveValidator.ps1', 'Tools/Test-HelixConstructionSet.ps1', 'Tools/Test-WorldVisualIntegrity.ps1')
    Boss = @('Tools/Test-BossAuthoringPipeline.ps1')
    Quest = @('Tools/Test-QuestDialoguePipeline.ps1')
}
$profiles.Background += 'Tools/Test-CloseBackgroundDimensions.ps1'
$profiles.MawShallow += 'Tools/Test-QAHistoryCoverage.ps1'
$profiles.MawShallow += 'Tools/Test-MawRopeEvidence.ps1'
$profiles.MawShallow += 'Tools/Test-MawRopeClimbScope.ps1'
$profiles.MawShallow += 'Tools/Test-MawRopeClimbEvidence.ps1'
  $profiles.MawShallow += 'Tools/Test-MawRibContour.ps1'
  $profiles.Performance += 'Tools/Test-QACameraSweep.ps1'
  $profiles.Performance += 'Tools/Test-QAPerformanceEvidence.ps1'
  $profiles.Performance += 'Tools/Test-QABatchPlan.ps1'
  $profiles.Performance += 'Tools/Test-MawConversionEvidence.ps1'
  $profiles.Performance += 'Tools/Test-MawGrowthLoadPlan.ps1'
  $profiles.Performance += 'Tools/Test-MawGrowthEvidence.ps1'

$selectedProfiles = if ($Profile -eq 'All') { @('Status', 'Tree', 'Terrain', 'MawMaterials', 'MawAnatomy', 'MawShallow', 'MawTeeth', 'MawCave', 'Performance', 'Persistence', 'Background', 'Entity', 'Structure', 'Boss', 'Quest') } else { @($Profile) }
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
    Write-Host 'GATE: C# compile only; no tmod install' -ForegroundColor Cyan
    & $hostExecutable -NoProfile -File (Join-Path $PSScriptRoot 'Compile-ApogeanOnly.ps1')
    if ($LASTEXITCODE -ne 0) { $failures.Add('Compile-ApogeanOnly.ps1') }
}

Write-Host ''
if ($failures.Count -gt 0) {
    Write-Host "APOGEAN $Profile GATE: RED" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host "APOGEAN $Profile GATE: STATIC PASS" -ForegroundColor Green
Write-Host 'A static pass does not promote a visual family. Run and inspect its named live fixture, then update Tools/AuthoringStatus.json.'
