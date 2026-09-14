#requires -Version 7.2
<#
.SYNOPSIS
Pure V3 rounded lips/U-wall/clearance contract and frozen V1/V2 replay. No native build or world writes.
.DESCRIPTION
-Baseline deliberately fails on V2's unowned shoulder air.
Runtime contract (main-owned): preflight the complete new mask plus framing impact;
reject protected content and terrain crossing the local y=4 clearance ceiling.
These pure tests cannot certify that a native world's top edge is clear.
#>
[CmdletBinding()]
param([switch]$Baseline)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$path = Join-Path $PSScriptRoot '../Common/WorldGeneration/MawSeedPlan.cs'
$checks = Join-Path $PSScriptRoot 'MawSeedPlanV3Checks.cs.txt'
if ('apogean.Common.WorldGeneration.MawSeedPlan' -as [type]) { throw 'Use a fresh pwsh process.' }
$before = (Get-FileHash -LiteralPath $path).Hash
Add-Type -TypeDefinition ((Get-Content -Raw -LiteralPath $path) + "`n" + (Get-Content -Raw -LiteralPath $checks))
if ($Baseline) {
    Write-Output ([apogean.Common.WorldGeneration.MawSeedPlanV3Checks]::CaptureGoldens())
    Write-Output ([apogean.Common.WorldGeneration.MawSeedPlanV3Checks]::LegacyGap())
    throw 'Preserved V2 clearance negative control: expected failure.'
}
Write-Output ([apogean.Common.WorldGeneration.MawSeedPlanV3Checks]::Run())
if ((Get-FileHash -LiteralPath $path).Hash -ne $before) { throw 'Planner changed during pure tests; rerun.' }
Write-Output "Planner SHA256: $before"
