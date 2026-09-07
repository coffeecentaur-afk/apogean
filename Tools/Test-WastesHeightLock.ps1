param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $ProjectRoot 'Common/Backgrounds/WastesCameraProjection.cs')
$checks = 0
function Assert-Height([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "FAIL: $Message" }
    $script:checks++
}
# Exercise the same altitude factor the renderer multiplies into submitted color.
foreach ($surface in 250,350,500,649,700) {
    $ground = ($surface - 50) * 16
    $space = ([math]::Floor($surface * [double][float]0.35) + 1) * 16
    foreach ($layer in 0..2) {
        foreach ($center in ($ground+1600),$ground,($space+500),$space,($space-500)) {
            foreach ($offset in -720,0,720) {
                $alpha = [apogean.Common.Backgrounds.WastesCameraProjection]::LandOpacity($surface,$center,$center+$offset,$layer)
                Assert-Height ($alpha -eq 1) "altitude must not fade layer=$layer surface=$surface center=$center; actual=$alpha"
            }
        }
    }
    foreach ($height in 720,1080,1369,1440,2160) {
        foreach ($zoom in 1.0,1.25,2.0) {
            foreach ($layer in 0,1) {
                # Lower flight ceilings, not a downward ground-composition offset.
                $fraction = if ($layer -eq 1) { .25 } else { .5 }
                $capCenter = $ground - ($ground-$space)*$fraction
                $actualCap = [apogean.Common.Backgrounds.WastesCameraProjection]::LockCameraCenterY($surface,$layer)
                Assert-Height ([math]::Abs($actualCap-$capCenter) -lt .01) "lower cap layer=$layer actual=$actualCap expected=$capCenter"
                $capCamera = $capCenter - $height*.5
                $capTop = [apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,$capCamera,$height,1792,$layer,$zoom)
                foreach ($lift in -400,-.25,0,.25,16,400,1200,4000) {
                    $camera = $capCamera-$lift
                    $top = [apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,$camera,$height,1792,$layer,$zoom)
                    $rate = if ($lift -gt 0) { $zoom } elseif ($layer -eq 0) { .012 } else { .03 }
                    $expected = $capTop + $lift*$rate
                    Assert-Height ([math]::Abs($top-$expected) -lt .015) "continuous cap/world lock layer=$layer lift=$lift height=$height zoom=$zoom actual=$top expected=$expected"
                    $again = [apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,$camera,$height,1792,$layer,$zoom)
                    Assert-Height ($top -eq $again) 'deterministic position after revisit/teleport'
                }
                $high = [apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,($capCamera-4000),$height,1792,$layer,$zoom)
                Assert-Height ($high -ge $height) 'scenery leaves through screen bottom, not alpha'
                $down = [apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,($ground+400-$height*.5),$height,1792,$layer,$zoom)
                $delta = $ground-($ground+400-$height*.5)-$height*.55
                $oldTop = $height*(.57+$layer*.025)-740+$delta*$(if($layer -eq 0){.012}else{.03})
                Assert-Height ([math]::Abs($down-$oldTop) -lt .015) 'ground/descent composition unchanged'
            }
            $close = [apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,($ground-4000),$height,1915,2,$zoom)
            $closeNext = [apogean.Common.Backgrounds.WastesCameraProjection]::Top($surface,($ground-4016),$height,1915,2,$zoom)
            Assert-Height ([math]::Abs($closeNext-$close-16*$zoom) -lt .015) 'existing Close world lock preserved'
        }
    }
}
Write-Host "PASS: $checks real-policy no-fade/capped-height checks. Arithmetic only, not native render approval."
