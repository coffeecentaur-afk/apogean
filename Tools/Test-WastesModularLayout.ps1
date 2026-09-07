param([ValidateSet('None','ShortDepth','PhaseJump','SocketShift')][string]$Mutation='None')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent $PSScriptRoot
$source=@('WastesCameraProjection','WastesParallaxContract','WastesModularLayout') | ForEach-Object {
    (Get-Content -Raw (Join-Path $repoRoot "Common/Backgrounds/$_.cs")) -replace '(?m)^using System;',''
}
Add-Type -TypeDefinition ("using System;`n"+($source -join "`n"))
$checks=0
foreach($viewport in @(@{Height=1080;Zoom=1.0},@{Height=1369;Zoom=4.0/3.0},@{Height=1440;Zoom=4.0/3.0})) {
    foreach($lift in @(-400,0,96,1200,1983.2,4200,4759.68)) {
        $cameraY=9584-$viewport.Height*.5-$lift
        foreach($layer in @(1,2)) {
            $top=[apogean.Common.Backgrounds.WastesModularLayout]::Top(649,$cameraY,$viewport.Height,$layer,$viewport.Zoom)
            $depth=if($layer -eq 1){1408}elseif($Mutation -eq 'ShortDepth'){1086}else{1915}
            if([apogean.Common.Backgrounds.WastesModularLayout]::BottomExposed($top,$depth,$viewport.Height)){throw 'DEPTH_CUTOFF'}
            if($layer -eq 2) {
                $socket=if($Mutation -eq 'SocketShift'){331}else{330}
                $cap=9584-(9584-3648)*.15
                $center=$cameraY+$viewport.Height*.5
                $expected=$viewport.Height*.5-48*$viewport.Zoom+(9584-[math]::Max($center,$cap))*.06+[math]::Max(0.0,[double]($cap-$center))*$viewport.Zoom
                if([Math]::Abs($top+$socket-$expected) -gt .01){throw 'SOCKET_SHIFT'}
            }
            $checks++
        }
    }
}
# Stable modulo at positive/negative world coordinates, including repeat edges.
foreach($layer in @(1,2)) {
    $period=if($layer -eq 1){2655}else{2268}
    $rate=[apogean.Common.Backgrounds.WastesParallaxContract]::Horizontal($layer)
    foreach($x in @(-150000,-100,-.1,0,.1,100,150000)) {
        $phase=[apogean.Common.Backgrounds.WastesModularLayout]::Phase($x,$layer)
        $next=[apogean.Common.Backgrounds.WastesModularLayout]::Phase($x+1,$layer)
        if($Mutation -eq 'PhaseJump'){$next+=10}
        $motion=($next-$phase+$period)%$period
        if($phase -lt 0 -or $phase -ge $period -or [Math]::Abs($motion-$rate) -gt .001){throw 'PHASE_JUMP'}
        $checks++
    }
}
# Wider existing-art helpers are deliberately not substitutes for authored depth.
if(-not [apogean.Common.Backgrounds.WastesModularLayout]::BottomExposed(-500,1086,1440)){throw 'NEGATIVE_DEPTH_MISSED'}
Write-Output "PASS: $checks modular camera/anchor/phase checks plus short-depth rejection. No cave-handoff or live-art approval."
