param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$source = @('WastesCameraProjection','WastesParallaxContract','WastesModularLayout') | ForEach-Object {
    (Get-Content -Raw -LiteralPath (Join-Path $ProjectRoot "Common/Backgrounds/$_.cs")) -replace '(?m)^using System;', ''
}
Add-Type -TypeDefinition ("using System;`n" + ($source -join "`n"))
$checks = 0
function Assert-Depth([bool]$condition, [string]$message) {
    if (-not $condition) { throw "FAIL: $message" }
    $script:checks++
}
$sampler = [Func[single,Nullable[single]]]{ param($x) [single]9584 }
$camera = 9584-1369*.5
$rest = [apogean.Common.Backgrounds.WastesModularLayout]::CloseTop(649,$camera,1369,(4.0/3),8,$sampler)
$step = [apogean.Common.Backgrounds.WastesModularLayout]::CloseTop(649,($camera-16),1369,(4.0/3),8,$sampler)
Assert-Depth ([math]::Abs($step-$rest-.96) -lt .01) "16px running rise moves foreground $($step-$rest)px; depth trial expects 0.96px, not terrain-speed movement"
Assert-Depth ([math]::Abs([apogean.Common.Backgrounds.WastesParallaxContract]::Horizontal(2)-.20) -lt .00001) 'Close horizontal response must be .20, ahead of .14 Mid'
Assert-Depth ([apogean.Common.Backgrounds.WastesParallaxContract]::Horizontal(2) -gt [apogean.Common.Backgrounds.WastesParallaxContract]::Horizontal(1)) 'Close must remain in front of Mid'

foreach ($surface in 250,500,649,700) {
    $ground=($surface-50)*16
    $space=([math]::Floor($surface * [double][single]0.35)+1)*16
    $cap=$ground-($ground-$space)*.15
    # Keep the former 15% handover as a regression sample, not a hard ceiling.
    foreach ($height in 1080,1369,1440) {
        foreach ($zoom in 1.0,(4.0/3),2.0) {
            foreach ($anchorOffset in -128,0,600) {
                $anchor=[single]($ground+$anchorOffset)
                $localSampler=[Func[single,Nullable[single]]]{ param($x) $anchor }
                foreach ($center in ($ground+400),$ground,($cap+.25),$cap,($cap-.25),($cap-32),($cap-5000)) {
                    $camera=$center-$height*.5
                    $top=[apogean.Common.Backgrounds.WastesModularLayout]::CloseTop($surface,$camera,$height,$zoom,8,$localSampler)
                    $span=$ground-$space
                    $u=[math]::Max(0.0,(($ground-$center)/$span-.1)/.9)
                    $area=if($u -lt 1){[math]::Pow($u,3)-.5*[math]::Pow($u,4)}else{$u-.5}
                    $extra=2*[math]::Max(0.0,$height+64-($height*.57-740-$height*.05*.012)-$span*.012)*1.6*$area
                    $expected=$height*.5-48*$zoom-330+($anchor-$center)*.06+$extra
                    Assert-Depth ([math]::Abs($top-$expected) -lt .02) "depth plane mismatch: surface=$surface height=$height zoom=$zoom anchor=$anchor center=$center actual=$top expected=$expected"
                    $again=[apogean.Common.Backgrounds.WastesModularLayout]::CloseTop($surface,$camera,$height,$zoom,8,$localSampler)
                    Assert-Depth ($again -eq $top) 'no spring, delayed catch-up, or visit-dependent placement'
                    if ($center -eq $cap-5000) { Assert-Depth ($top -gt $height) 'high flight must still leave the foreground below view' }
                }
                # Surface relief remains tied to immutable anchors, but is not
                # projected at tile speed merely because the camera runs sideways.
                $low=[apogean.Common.Backgrounds.WastesModularLayout]::Top($surface,($ground-$height*.5),$height,2,$zoom,$ground)
                $high=[apogean.Common.Backgrounds.WastesModularLayout]::Top($surface,($ground-$height*.5),$height,2,$zoom,($ground+160))
                Assert-Depth ([math]::Abs($high-$low-9.6) -lt .02) '160px terrain relief projects to 9.6px, not a wall-sized step'
            }
        }
    }
}
Write-Host "PASS: $checks Close-depth assertions. This is the user's farther-back parallax trial, not final comfort or production approval."
