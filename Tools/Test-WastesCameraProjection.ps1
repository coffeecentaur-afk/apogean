param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path $ProjectRoot 'Common/Backgrounds/WastesCameraProjection.cs')
$checks=0
function Assert-Projection([bool]$Condition,[string]$Message) {
    if(-not $Condition){throw "FAIL: $Message"}
    $script:checks++
}
foreach($height in 1080,1369,1440) {
    $width=if($height -eq 1080){1920}else{2560}
    $zoom=[single][math]::Max(1.0,[math]::Max($width/1920.0,$height/1080.0))
    # Live batch translation ~= .005px, not centered ZoomMatrix -426.67/-228.17.
    foreach($phase in 0,0.01,1,100,426,512,1024,2047.99) {
        $first=[math]::Floor(-$phase)
        $last=$first
        while($last+2048 -lt $width){$last+=2048}
        $left=[apogean.Common.Backgrounds.WastesCameraProjection]::LogicalCoordinate($first,$zoom)*$zoom
        $right=[apogean.Common.Backgrounds.WastesCameraProjection]::LogicalCoordinate(($last+2048),$zoom)*$zoom
        Assert-Projection ($left -le .1 -and $right -ge $width-.1) "both edges covered: ${width}x$height phase=$phase"
        $next=[apogean.Common.Backgrounds.WastesCameraProjection]::LogicalCoordinate(($first+2048),$zoom)*$zoom
        Assert-Projection ([math]::Abs($next-$left-2048) -lt .01) 'repeat stays 2048 physical pixels without gaps'
    }
    $camera=[single](9584-$height*.55)
    foreach($layer in 0,1,2) {
        $ground=[apogean.Common.Backgrounds.WastesCameraProjection]::Top(649,$camera,$height,1280,$layer)
        $flight=[apogean.Common.Backgrounds.WastesCameraProjection]::Top(649,($camera-1200),$height,1280,$layer)
        if($layer -eq 2) {
            $cap=9584-(9584-3648)*.15
            $center=$camera+$height*.5
            $expected=(9584-[math]::Max($center-1200,$cap))*.06+[math]::Max(0.0,[double]($cap-($center-1200)))-(9584-$center)*.06
            Assert-Projection ([math]::Abs($flight-$ground-$expected) -lt .02) 'Close depth response transitions to fixed-height flight exit'
        } else {
            Assert-Projection ([math]::Abs($flight-$ground) -le 72.1) "distant layer stays subtle: layer=$layer ${width}x$height"
        }
        Assert-Projection ($ground -eq [apogean.Common.Backgrounds.WastesCameraProjection]::Top(649,$camera,$height,1280,$layer)) 'stateless world datum'
        foreach($lift in -1200,-400,0,96,1200,2400,4200,8000) {
            $top=[apogean.Common.Backgrounds.WastesCameraProjection]::Top(649,($camera-$lift),$height,1280,$layer)
            if($layer -eq 0){Assert-Projection ([apogean.Common.Backgrounds.WastesCameraProjection]::CoveredBottom($top,1280,$height) -ge $height) 'far layer covers bottom after closer layers leave view'}
        }
    }
}
# Preserve original failure patterns as negative controls.
$oldLeft=(0-(-426.6667))/[single](4.0/3.0)*[single](4.0/3.0)
Assert-Projection ($oldLeft -gt 400) 'old centered inverse fails left-edge coverage'
Assert-Projection (6000*.06 -lt 1080) 'unbounded low parallax cannot provide the required high-flight exit'
Write-Host "PASS: $checks production projection/coverage/response checks including original-bug controls. Not GPU or art acceptance."
