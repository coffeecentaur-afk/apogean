#requires -Version 7.2
<#
.SYNOPSIS
Compiles the real pure planner and verifies v1 replay plus opt-in v2 geometry.
.DESCRIPTION
No engine, install, world writes, or native visual claims. Run in a fresh pwsh.
-Baseline intentionally exits nonzero with the preserved v1 buried-wall defect;
the ordinary run requires v2 backing, finite ownership, hazardous branch-only
connectivity, independent surface walls, and the recorded v1 golden outputs.
#>
[CmdletBinding()]
param([switch]$Baseline)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$planPath = Join-Path $PSScriptRoot '../Common/WorldGeneration/MawSeedPlan.cs'
$checksPath = Join-Path $PSScriptRoot 'MawSeedPlanV2Checks.cs.txt'
if ('apogean.Common.WorldGeneration.MawSeedPlan' -as [type]) { throw 'Run in a fresh pwsh process to compile the current planner.' }
$before = (Get-FileHash -LiteralPath $planPath).Hash
Add-Type -TypeDefinition ((Get-Content -Raw -LiteralPath $planPath) + "`n" + (Get-Content -Raw -LiteralPath $checksPath))
if ($Baseline) { Write-Output ([apogean.Common.WorldGeneration.MawSeedPlanV2Checks]::BaselineSnapshots()) }
$result = [apogean.Common.WorldGeneration.MawSeedPlanV2Checks]::RootBacking($Baseline.IsPresent)
Write-Output $result
if ((Get-FileHash -LiteralPath $planPath).Hash -ne $before) { throw 'Planner changed during the test.' }
if ($result.StartsWith('FAIL')) { throw 'Buried rib backing regression failed (pure planner; no native lighting claim).' }
if (-not $Baseline) {
    Write-Output ([apogean.Common.WorldGeneration.MawSeedPlanV2Checks]::Compatibility())
    Write-Output ([apogean.Common.WorldGeneration.MawSeedPlanV2Checks]::Geometry())
    Write-Output ([apogean.Common.WorldGeneration.MawSeedPlanV2Checks]::Profiles())
}
if ((Get-FileHash -LiteralPath $planPath).Hash -ne $before) { throw 'Planner changed during the test.' }
