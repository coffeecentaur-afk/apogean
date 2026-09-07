Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$calculator = Join-Path $PSScriptRoot 'Get-WastesReclamationProfile.ps1'
$checks = 0
function Assert-Profile([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "FAIL: $Message" }
    $script:checks++
}
function Assert-InvalidCounts([long]$Eligible, [long]$Restored) {
    $rejected = $false
    try { $null = & $calculator -EligibleSurfaceUnits $Eligible -RestoredSurfaceUnits $Restored }
    catch { $rejected = $true }
    Assert-Profile $rejected "reject invalid counts: eligible=$Eligible restored=$Restored"
}

$unknown = & $calculator 0 0
Assert-Profile (-not $unknown.Available) 'zero eligible area is unavailable, not full recovery'
foreach ($layer in @('Global', 'Close', 'Mid', 'Far')) {
    Assert-Profile ($null -eq $unknown.$layer) "unknown $layer has no fabricated score"
}
Assert-InvalidCounts -1 0
Assert-InvalidCounts 1 -1
Assert-InvalidCounts 0 1
Assert-InvalidCounts 100 101

$previous = $null
foreach ($restored in 0..100) {
    $profile = & $calculator 100 $restored
    Assert-Profile $profile.Available "known denominator at $restored"
    Assert-Profile ($profile.Far -eq $profile.Global) "city follows global measure at $restored"
    Assert-Profile ($profile.Close -ge $profile.Mid -and $profile.Mid -ge $profile.Far) "Close leads Mid leads Far at $restored"
    foreach ($layer in @('Close', 'Mid', 'Far')) {
        $weight = $profile.$layer
        Assert-Profile ($weight -ge 0 -and $weight -le 1) "$layer bounded at $restored"
        if ($restored -eq 0) { Assert-Profile ($weight -eq 0) "$layer barren endpoint" }
        if ($restored -eq 100) { Assert-Profile ($weight -eq 1) "$layer full endpoint" }
        if ($restored -gt 0) {
            Assert-Profile ($weight -gt 0) "$layer starts before other layers finish"
            Assert-Profile ($weight -ge $previous.$layer) "$layer monotonic at $restored"
        }
    }
    $previous = $profile
}

$quarter = & $calculator 100 25
Assert-Profile ($quarter.Close -eq 0.578125 -and $quarter.Mid -eq 0.4375 -and $quarter.Far -eq 0.25) 'independent quarter-recovery values'
$ratio = & $calculator 1000 250
Assert-Profile ($ratio.Close -eq $quarter.Close -and $ratio.Mid -eq $quarter.Mid -and $ratio.Far -eq $quarter.Far) 'equal ratios produce equal profiles'
$lower = & $calculator 100 10
Assert-Profile ($lower.Close -lt $quarter.Close -and $lower.Mid -lt $quarter.Mid -and $lower.Far -lt $quarter.Far) 'reduced supplied restoration lowers every layer'
$maximum = & $calculator ([long]::MaxValue) ([long]::MaxValue)
Assert-Profile ($maximum.Global -eq 1 -and $maximum.Close -eq 1 -and $maximum.Mid -eq 1) 'large count endpoint does not overflow'
$tiny = & $calculator ([long]::MaxValue) 1
Assert-Profile ($tiny.Close -gt $tiny.Mid -and $tiny.Mid -gt $tiny.Far -and $tiny.Far -gt 0) 'tiny positive progress survives floating point arithmetic'
$repeat = & $calculator 100 25
Assert-Profile ($repeat.Close -eq $quarter.Close -and $repeat.Mid -eq $quarter.Mid -and $repeat.Far -eq $quarter.Far) 'calculator has no camera or previous-call state'

Write-Host "PASS: $checks offline profile assertions (including 4 invalid-count rejection cases)."
Write-Host 'Scope: proposed ratio/curve arithmetic only; no terrain counting, art coverage, save/network or live rendering proof.'
