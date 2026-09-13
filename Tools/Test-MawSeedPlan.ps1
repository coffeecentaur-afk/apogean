#requires -Version 7.2
<#
.SYNOPSIS
Compiles the real, engine-free MawSeedPlan and checks deterministic anatomy.
.DESCRIPTION
Run in a fresh PowerShell process so Add-Type cannot reuse an older planner.
Always runs at least 128 distinct seeds plus smooth route/shoulder combinations.
Failures, including corrupt plans accepted by Validate, produce a nonzero exit.
Optional outputs are written only after the suite has finished; JSON includes
failures and the exact tested source hash. An SVG is a tile-coordinate diagram,
not a native render or a claim about movement, balance, or world integration.
.EXAMPLE
pwsh -NoProfile -File Tools/Test-MawSeedPlan.ps1
.EXAMPLE
pwsh -NoProfile -File Tools/Test-MawSeedPlan.ps1 -Seed 42 -PreviewPath C:/Temp/maw.svg -OutputPath C:/Temp/maw.json
#>
[CmdletBinding()]
param(
    [int]$Seed = 42,
    [ValidateRange(128, 4096)][int]$SeedCount = 128,
    [string]$PreviewPath = '',
    [Alias('SummaryPath')][string]$OutputPath = ''
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$sourcePath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../Common/WorldGeneration/MawSeedPlan.cs'))
$checksPath = Join-Path $PSScriptRoot 'MawSeedPlanChecks.cs.txt'
if ('apogean.Common.WorldGeneration.MawSeedPlan' -as [type]) {
    throw 'MawSeedPlan is already loaded. Run this script in a fresh pwsh -NoProfile -File process to test the current source.'
}
function Resolve-ReportPath([string]$Path, [string]$Extension) {
    if (-not $Path) { return '' }
    $resolved = [IO.Path]::GetFullPath($Path, $PWD.ProviderPath)
    if ([IO.Path]::GetExtension($resolved) -ne $Extension) {
        throw "Output must have the $Extension extension: $resolved"
    }
    if (-not [IO.Directory]::Exists([IO.Path]::GetDirectoryName($resolved))) {
        throw "Output directory must already exist: $resolved"
    }
    # Keep optional diagnostics from overwriting the preserved sketch artifacts.
    if ([IO.Path]::GetFileName($resolved) -like 'MawSketch*') {
        throw "The approved sketch artifacts are outside this tool's output scope: $resolved"
    }
    return $resolved
}
$PreviewPath = Resolve-ReportPath $PreviewPath '.svg'
$OutputPath = Resolve-ReportPath $OutputPath '.json'
$sourceBytes = [IO.File]::ReadAllBytes($sourcePath)
$sourceHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($sourceBytes))
$source = [Text.Encoding]::UTF8.GetString($sourceBytes).TrimStart([char]0xFEFF)
$checks = Get-Content -Raw -LiteralPath $checksPath
Add-Type -TypeDefinition ($source + "`n" + $checks)
$report = [apogean.Common.WorldGeneration.MawSeedPlanChecks]::Run($SeedCount)
$preview = $null
if ($PreviewPath) {
    try {
        $preview = [apogean.Common.WorldGeneration.MawSeedPlanChecks]::Svg($Seed, $report.Success, $sourceHash)
        # Validate the generated document before writing it.
        $null = [xml]$preview
    } catch {
        $report.AddFailure('preview', "seed=$Seed", $_.Exception.GetBaseException().Message)
    }
}
$sourceUnchanged = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash -eq $sourceHash
if (-not $sourceUnchanged) {
    $report.AddFailure('source-changed', 'run', 'MawSeedPlan.cs changed during this run; rerun in a fresh process.')
    $preview = $null
}
$summary = [ordered]@{
    SchemaVersion = 1
    Status = $(if ($report.Success) { 'PASS' } else { 'FAIL' })
    Source = $sourcePath
    SourceSha256 = $sourceHash
    SourceUnchanged = $sourceUnchanged
    FinishedUtc = [DateTime]::UtcNow.ToString('o')
    PreviewSeed = $Seed
    Scope = 'Pure plan only. Conservative 2x3 spatial reach with full 4x4 tooth hazards; no game, movement simulation, install, or visual acceptance.'
    Results = $report
}
if ($preview) { [IO.File]::WriteAllText($PreviewPath, $preview, [Text.UTF8Encoding]::new($false)) }
if ($OutputPath) { [IO.File]::WriteAllText($OutputPath, ($summary | ConvertTo-Json -Depth 12), [Text.UTF8Encoding]::new($false)) }
Write-Output ("{0}: {1} distinct seeds, {2}/{3} constructed cases, {4} deterministic pairs, {5} cell comparisons, {6}/{7} rejected corrupt plans, {8}/{9} rejected invalid inputs ({10:N2}s)." -f
    $summary.Status, $report.DistinctSeeds, $report.ConstructedCases, $report.Cases,
    $report.DeterministicPairs, $report.CellComparisons, $report.RejectedMutations,
    $report.MutationCases, $report.RejectedInputs, $report.InvalidInputCases, $report.Seconds)
foreach ($failure in $report.Failures) {
    Write-Output ("FAIL {0} ({1}): {2}: {3}" -f $failure.Check, $failure.Occurrences, $failure.FirstCase, $failure.Detail)
}
Write-Output "Source SHA256: $sourceHash"
Write-Output $summary.Scope
if (-not $report.Success) { throw "MawSeedPlan regression failed: $($report.Failures.Count) failure groups. Core source was not modified." }
