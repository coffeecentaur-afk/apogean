param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$source = @('WastesCameraProjection','WastesParallaxContract','WastesModularLayout','WastesGroundProfile') | ForEach-Object {
    (Get-Content -Raw -LiteralPath (Join-Path $ProjectRoot "Common/Backgrounds/$_.cs")) -replace '(?m)^using System;', ''
}
Add-Type -TypeDefinition ("using System;`n" + ($source -join "`n"))
$rows = [int[]](0..280 | ForEach-Object { 599 + ($_ % 5)*12 })
$profile = [apogean.Common.Backgrounds.WastesGroundProfile]::new($rows)
$sampler = [Func[single,Nullable[single]]]{ param($x) $profile.GroundAt($x) }
$checks = 0
$seen = @{}
# Follow the SAME absolute sections across pans, seams and return trips.
foreach ($x in (0..240 | ForEach-Object { $_*512-1024 }) + (240..0 | ForEach-Object { $_*512-1024 })) {
    $scroll = $x*[double][single]0.2-620
    $first = [int][math]::Floor($scroll/2268)-1
    foreach ($cell in $first..($first+3)) {
        $actual = [apogean.Common.Backgrounds.WastesModularLayout]::CloseTop(649,9000,1440,1,$cell,$sampler)
        $anchor = ($cell*2268.0+724+620)/[double][single]0.2
        $expected = 720-48-330+($profile.GroundAt([single]$anchor)-9720)*.06
        if ([math]::Abs($actual-$expected) -gt .01) { throw 'FAIL: fixed section uses wrong terrain anchor' }
        if ($seen.ContainsKey($cell) -and $seen[$cell] -ne $actual) { throw 'FAIL: foreground section changes height while panning' }
        $seen[$cell] = $actual
        $flight = [apogean.Common.Backgrounds.WastesModularLayout]::CloseTop(649,8600,1440,1,$cell,$sampler)
        if ([math]::Abs($flight-$actual-24) -gt .01) { throw 'FAIL: low foreground motion does not use the farther-back depth plane' }
        $checks += 3
    }
}
$copy = [apogean.Common.Backgrounds.WastesGroundProfile]::new($profile.CopyRows())
$reloadSampler = [Func[single,Nullable[single]]]{ param($x) $copy.GroundAt($x) }
foreach ($cell in $seen.Keys) {
    if ([apogean.Common.Backgrounds.WastesModularLayout]::CloseTop(649,9000,1440,1,$cell,$reloadSampler) -ne $seen[$cell]) { throw 'FAIL: copied saved profile shifts a section' }
    $checks++
}
# Integration guard supplements arithmetic: the live call must use cell identity.
$renderer = Get-Content -Raw -LiteralPath (Join-Path $ProjectRoot 'Content/Backgrounds/WastesLandscapeV1Renderer.cs')
if ($renderer.Contains('TryGroundAt(centerX') -or -not $renderer.Contains('zoom, cell, SavedGroundAt)')) { throw 'FAIL: renderer bypasses fixed-section anchor' }
Write-Host "PASS: $checks fixed-section assertions plus live-call wiring guard. Synthetic terrain; native horizontal/diagonal proof still required."
