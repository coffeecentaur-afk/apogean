param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $ProjectRoot 'Common/Biomes/ForestRestorationState.cs')
$policy = [apogean.Common.Biomes.ForestRestorationState]::new()
$checks = 0
function Assert-Policy([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "FAIL: $Message" }
    $script:checks++
}
Assert-Policy (-not $policy.IsLivingAt(1000)) 'fresh world is not silently restored'
$policy.Observe(1, 0, 0, 1000)
Assert-Policy (-not $policy.HasEvidence) 'one decorative grass tile is insufficient'
$policy.Observe(0, 180, 0, 1000)
Assert-Policy ($policy.HasEvidence -and -not $policy.UseLivingForest) 'Wastes sample'
$policy.Observe(64, 36, 0, 1000)
Assert-Policy (-not $policy.UseLivingForest) 'below entry threshold remains Wastes'
$policy.Observe(65, 35, 0, 1000)
Assert-Policy ($policy.IsLivingAt(1000)) 'exact entry threshold restores forest'
$policy.Observe(50, 50, 0, 1000)
Assert-Policy ($policy.UseLivingForest) 'mixed sample retains restored state'
$policy.Observe(36, 64, 0, 1000)
Assert-Policy ($policy.UseLivingForest) 'above exit threshold remains restored'
$policy.Observe(35, 65, 0, 1000)
Assert-Policy (-not $policy.UseLivingForest) 'exact exit threshold returns Wastes'
$policy.Observe(50, 50, 0, 1000)
Assert-Policy (-not $policy.UseLivingForest) 'mixed sample retains Wastes state'
$policy.Observe(180, 0, 0, 1000)
Assert-Policy ($policy.LivingFraction -eq 1 -and $policy.UseLivingForest) 'full restoration'
$policy.Observe(0, 0, 0, 1000)
Assert-Policy ($policy.IsLivingAt(1000)) 'flight above same area holds last valid scenery'
Assert-Policy ($policy.LivingCount -eq 0 -and $policy.WastesCount -eq 0) 'telemetry records empty sample'
Assert-Policy ($policy.IsLivingAt(1120)) 'local evidence inclusive radius'
Assert-Policy (-not $policy.IsLivingAt(1121)) 'distant camera cannot reuse cached green state'
$policy.Observe(0, 0, 0, 2000)
Assert-Policy (-not $policy.HasEvidence) 'teleport into unsampled terrain resets evidence'
$policy.Observe(100, 0, 0, 2000)
$policy.Observe(0, 0, 0, 2010)
$policy.Observe(0, 0, 0, 2020)
Assert-Policy (-not $policy.IsLivingAt(2121)) 'empty samples cannot drag evidence across world'
$policy.Observe(0, 0, 100, 2000)
Assert-Policy (-not $policy.UseLivingForest -and $policy.WastesCount -eq 100) 'legacy DeadGrass counts as Wastes'
$policy.Observe(-1, -2, -3, 2000)
Assert-Policy ($policy.LivingCount -eq 0 -and $policy.WastesCount -eq 0) 'defensive negative clamping'
$policy.Observe([int]::MaxValue, [int]::MaxValue, [int]::MaxValue, 2000)
Assert-Policy ($policy.WastesCount -eq 4294967294) 'sums do not overflow int'
Assert-Policy ([math]::Abs($policy.LivingFraction - (1.0/3.0)) -lt 0.000001) 'overflow-safe ratio'
$policy.Reset()
$policy.Observe(39, 0, 0, 1000)
Assert-Policy (-not $policy.HasEvidence) 'minimum sample boundary below'
$policy.Observe(40, 0, 0, 1000)
Assert-Policy ($policy.UseLivingForest) 'minimum sample boundary exact'
$policy.Observe(100, 0, 0, [double]::NaN)
Assert-Policy (-not $policy.HasEvidence) 'invalid position clears evidence'
$policy.Observe(100, 0, 0, [double]::PositiveInfinity)
Assert-Policy (-not $policy.HasEvidence) 'infinite position clears evidence'
$policy.Observe(100, 0, 0, 1000)
$policy.Reset()
Assert-Policy (-not $policy.HasEvidence -and $policy.LivingFraction -eq 0 -and $policy.WastesCount -eq 0) 'world reset clears all state'
Write-Host "PASS: $checks forest-restoration policy checks. This is not live rendering evidence."
