param(
    [Parameter(Mandatory)][string]$CapturePath,
    [string]$AtlasPath = (Join-Path $PSScriptRoot '../Art/Candidates/MawToothCluster-v1/Native-v3/cluster-atlas.png')
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$shot=[Drawing.Bitmap]::new((Resolve-Path -LiteralPath $CapturePath).Path)
$atlas=[Drawing.Bitmap]::new((Resolve-Path -LiteralPath $AtlasPath).Path)
try {
    if($shot.Width-ne672 -or $shot.Height-ne592 -or $atlas.Width-notin@(288,416) -or $atlas.Height-notin@(72,144)){throw 'Expected fixed native orientation room and preview atlas region.'}
    $gaps=[Collections.Generic.List[string]]::new()
    $samples=0
    foreach($face in @(1,3)) {
        $column=if($face-eq1){0}else{63}
        $socketX=if($face-eq1){47}else{624}
        $top=if($face-eq1){176}else{304}
        for($row=0;$row-lt64;$row++) {
            $a=$atlas.GetPixel($face*72+[int][Math]::Floor($column/16)*18+$column%16,[int][Math]::Floor($row/16)*18+$row%16)
            if($a.A-ne255){continue}
            $samples++
            $c=$shot.GetPixel($socketX,$top+$row)
            # Named daylight fixture: blue/green scenery leaks between neutral
            # bone roots and brown Maw soil. Not a universal terrain detector.
            if($c.G-gt$c.R+20 -and $c.B-gt$c.R+20 -and $c.G-gt80){$gaps.Add("face=$face row=$row")}
        }
    }
    if($samples-lt20){throw 'Not enough root samples to certify the socket.'}
    if($gaps.Count-gt0){throw "WALL_SOCKET_GAP: $($gaps.Count)/$samples root rows expose scenery: $($gaps -join '; ')"}
    Write-Host "PASS: $samples wall-root rows meet terrain without daylight scenery gaps. Bounded native room only."
} finally {$shot.Dispose();$atlas.Dispose()}
