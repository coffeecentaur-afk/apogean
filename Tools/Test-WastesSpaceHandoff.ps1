param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
# HISTORICAL opacity policy, superseded 2026-09-07 by Test-WastesHeightLock.ps1.
# Replay only against the old source revision; not an active current-code gate.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $ProjectRoot 'Common/Backgrounds/WastesCameraProjection.cs')
$checks = 0
# Same pure function multiplies the full color in the actual renderer.
foreach ($surface in 250,350,400,500,649,700) {
    $ground = ($surface-50)*16
    # Independent installed 1.4.4 Player.UpdateBiomes predicate: integer tile Y.
    $boundary = ([math]::Floor($surface * [double][float]0.35)+1)*16
    foreach ($layer in 0..2) {
        foreach ($offset in -720,0,720) {
            $player = $boundary-0.5
            $actual = [apogean.Common.Backgrounds.WastesCameraProjection]::LandOpacity($surface,$player+$offset,$player,$layer)
            if ($actual -ne 0) { throw "FAIL: terrestrial layer $layer persists in player Space: surface=$surface cameraOffset=$offset opacity=$actual" }
            $actual = [apogean.Common.Backgrounds.WastesCameraProjection]::LandOpacity($surface,$player,$player+$offset,$layer)
            if ($actual -ne 0) { throw "FAIL: terrestrial layer $layer persists in camera Space" }
            $checks += 2
        }
        foreach ($depth in $ground,($ground+400),($ground+1600)) {
            if ([apogean.Common.Backgrounds.WastesCameraProjection]::LandOpacity($surface,$depth,$depth,$layer) -ne 1) {
                throw "FAIL: layer $layer disappears at ground/shallow descent"
            }
            $checks++
        }
        $previous = 1.0
        foreach ($step in 0..1000) {
            $fraction = $step/1000.0
            $y = $ground-($ground-$boundary)*$fraction
            $actual = [apogean.Common.Backgrounds.WastesCameraProjection]::LandOpacity($surface,$y,$y,$layer)
            if ($actual -lt 0 -or $actual -gt 1 -or $actual -gt $previous+0.00001 -or [math]::Abs($actual-$previous) -gt .006) {
                throw "FAIL: layer $layer has abrupt/nonmonotone opacity at ascent $fraction"
            }
            if ($fraction -le 1/3 -and $actual -lt .9999) { throw 'FAIL: early ascent scenery lost' }
            if ($layer -ne 1 -and $fraction -le .699 -and $actual -ne 1) { throw 'FAIL: Far/Close fades too early' }
            if ($layer -eq 0 -and $fraction -eq .85 -and ($actual -lt .49 -or $actual -gt .51)) { throw 'FAIL: Far has no gradual late-ascent fade' }
            # No hidden elapsed-time state: descending revisits the same result.
            $again = [apogean.Common.Backgrounds.WastesCameraProjection]::LandOpacity($surface,$y,$y,$layer)
            if ($again -ne $actual) { throw 'FAIL: return path is stateful' }
            $previous = $actual
            $checks++
        }
    }
}
Write-Host "PASS: $checks real-policy presence/absence, offset-camera and continuous ascent checks. Not native render approval."
