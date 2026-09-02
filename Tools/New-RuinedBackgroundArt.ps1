param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }

$palettes = @{
    Forest=@((C '#171515'),(C '#25211e'),(C '#37302a'),(C '#4b4035'),(C '#625344'),(C '#796654'),(C '#9a8061'),(C '#bd9b69'),(C '#c5792e'),(C '#e0ab4d'))
    Desert=@((C '#211916'),(C '#34251d'),(C '#4a3426'),(C '#64482f'),(C '#80603a'),(C '#9e7a47'),(C '#c09a5d'),(C '#ddbd7b'),(C '#955729'),(C '#d98b39'))
    Jungle=@((C '#111714'),(C '#1d2820'),(C '#2b3829'),(C '#3c4a34'),(C '#526047'),(C '#69745a'),(C '#8a8d69'),(C '#b0a77b'),(C '#88762d'),(C '#c49b38'))
    Snow=@((C '#1b2022'),(C '#2b3235'),(C '#3c474b'),(C '#526064'),(C '#6e7b7e'),(C '#8d9999'),(C '#b0b7b1'),(C '#d3d3c5'),(C '#80643f'),(C '#c2914d'))
    Corruption=@((C '#17131b'),(C '#251e2d'),(C '#382b42'),(C '#4c3a55'),(C '#63506b'),(C '#7a687e'),(C '#958299'),(C '#b6a3b4'),(C '#6f6031'),(C '#b08b3b'))
    Crimson=@((C '#1c1212'),(C '#2d1b1a'),(C '#422725'),(C '#593631'),(C '#714944'),(C '#895e55'),(C '#a77968'),(C '#c49a82'),(C '#84602f'),(C '#c58f3d'))
    Hallow=@((C '#1b1b26'),(C '#292a3b'),(C '#3c3e55'),(C '#545775'),(C '#6f7392'),(C '#8d90ae'),(C '#b1afc9'),(C '#d7d0dc'),(C '#7e6b3a'),(C '#c4a45d'))
    Ocean=@((C '#111a20'),(C '#1d2a32'),(C '#2b3c45'),(C '#3c515b'),(C '#526873'),(C '#6e8188'),(C '#91a0a2'),(C '#b8c0b9'),(C '#776036'),(C '#ba8d43'))
    Mushroom=@((C '#11171a'),(C '#1c262a'),(C '#29383c'),(C '#394c50'),(C '#4d6265'),(C '#657a78'),(C '#82928a'),(C '#a9aa96'),(C '#3d8191'),(C '#64bad0'))
    Underworld=@((C '#1c1210'),(C '#2e1c17'),(C '#44271d'),(C '#5c3524'),(C '#78492b'),(C '#955f35'),(C '#b97a44'),(C '#dca15f'),(C '#b84b25'),(C '#e07935'))
    Engraft=@((C '#151414'),(C '#24201e'),(C '#352b25'),(C '#4a392c'),(C '#624a32'),(C '#7e6037'),(C '#9f793a'),(C '#c6973e'),(C '#e0ad42'),(C '#ead49a'))
}

function New-Canvas([int]$width,[int]$height){
    $bitmap=[System.Drawing.Bitmap]::new($width,$height,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics=[System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::None
    $graphics.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
    @{Bitmap=$bitmap;Graphics=$graphics}
}

function Save-Canvas($canvas,[string]$relative){
    $path=Join-Path $Root $relative
    $directory=Split-Path -Parent $path
    if(-not(Test-Path -LiteralPath $directory)){New-Item -ItemType Directory -Path $directory -Force|Out-Null}
    $canvas.Bitmap.Save($path,[System.Drawing.Imaging.ImageFormat]::Png)
    $canvas.Graphics.Dispose();$canvas.Bitmap.Dispose()
}

function Seal-HorizontalSurfaceSeam([System.Drawing.Bitmap]$bitmap){
    # Surface backgrounds repeat horizontally. Match the terminal column exactly so
    # the camera never reveals a one-pixel split at the texture boundary.
    for($y=0;$y -lt $bitmap.Height;$y++){
        $bitmap.SetPixel($bitmap.Width-1,$y,$bitmap.GetPixel(0,$y))
    }
}

function Seal-UndergroundWrapStrip([System.Drawing.Bitmap]$bitmap){
    # Terraria's underground background renderer wraps the first 32-pixel strip at
    # x=128. Keep that repeated strip byte-identical after all landmarks are drawn.
    for($y=0;$y -lt $bitmap.Height;$y++){
        for($x=0;$x -lt 32;$x++){
            $bitmap.SetPixel(128+$x,$y,$bitmap.GetPixel($x,$y))
        }
    }
}

function Brush([System.Drawing.Color]$color){[System.Drawing.SolidBrush]::new($color)}

function Fill-Rect($g,[System.Drawing.Color]$color,[int]$x,[int]$y,[int]$w,[int]$h){
    $b=Brush $color
    try{$g.FillRectangle($b,$x,$y,$w,$h)}finally{$b.Dispose()}
}

function Fill-Poly($g,[System.Drawing.Color]$color,[System.Drawing.Point[]]$points){
    $b=Brush $color
    try{$g.FillPolygon($b,$points)}finally{$b.Dispose()}
}

function Stroke($g,[System.Drawing.Color]$color,[int]$width,[System.Drawing.Point[]]$points){
    $pen=[System.Drawing.Pen]::new($color,$width)
    try{$pen.StartCap='Flat';$pen.EndCap='Flat';$pen.LineJoin='Miter';$g.DrawLines($pen,$points)}finally{$pen.Dispose()}
}

function Draw-Landscape($g,[int]$width,[int]$height,[int]$horizon,[System.Drawing.Color[]]$p,[int]$seed,[int]$depth){
    $rand=[System.Random]::new($seed)
    $points=[Collections.Generic.List[System.Drawing.Point]]::new()
    $points.Add([Drawing.Point]::new(0,$height))
    $points.Add([Drawing.Point]::new(0,$horizon))
    $x=0;$y=$horizon
    while($x -lt $width){
        $x=[Math]::Min($width,$x+$rand.Next(42,86))
        $y=[Math]::Max([int]($height*0.24),[Math]::Min([int]($height*0.78),$horizon+$rand.Next(-55,56)))
        $points.Add([Drawing.Point]::new($x,$y))
    }
    $points.Add([Drawing.Point]::new($width,$height))
    Fill-Poly $g $p[$depth] $points.ToArray()
    for($band=1;$band -le 3;$band++){
        $line=[Collections.Generic.List[System.Drawing.Point]]::new()
        foreach($point in $points){if($point.Y -lt $height){$line.Add([Drawing.Point]::new($point.X,[Math]::Min($height-1,$point.Y+$band*18)))}}
        if($line.Count -gt 1){Stroke $g $p[[Math]::Min(7,$depth+$band)] 2 $line.ToArray()}
    }
}

function Draw-TerrainTexture([System.Drawing.Bitmap]$bitmap,$g,[System.Drawing.Color[]]$p,[int]$seed,[int]$depth,[int]$density){
    $rand=[System.Random]::new($seed)
    for($i=0;$i -lt $density;$i++){
        $x=$rand.Next(1,$bitmap.Width-8);$y=$rand.Next([int]($bitmap.Height*0.28),$bitmap.Height-4)
        if($bitmap.GetPixel($x,$y).A -eq 0){continue}
        $colorIndex=[Math]::Max(0,[Math]::Min(7,$depth+$rand.Next(-2,3)))
        $w=$rand.Next(2,12);$h=$rand.Next(1,5)
        Fill-Rect $g $p[$colorIndex] $x $y $w $h
        if($i%9 -eq 0){
            Stroke $g $p[[Math]::Max(0,$depth-2)] 1 @([Drawing.Point]::new($x,$y),[Drawing.Point]::new($x+$rand.Next(-8,9),$y+$rand.Next(5,16)),[Drawing.Point]::new($x+$rand.Next(-12,13),$y+$rand.Next(16,29)))
        }
    }
}

function Draw-DeadTree($g,[int]$x,[int]$ground,[int]$scale,[System.Drawing.Color[]]$p,[int]$lean){
    Stroke $g $p[0] ([Math]::Max(2,$scale+2)) @([Drawing.Point]::new($x,$ground),[Drawing.Point]::new($x+$lean,$ground-22*$scale),[Drawing.Point]::new($x+$lean-2*$scale,$ground-39*$scale))
    Stroke $g $p[5] $scale @([Drawing.Point]::new($x,$ground),[Drawing.Point]::new($x+$lean,$ground-22*$scale),[Drawing.Point]::new($x+$lean-2*$scale,$ground-39*$scale))
    Stroke $g $p[5] $scale @([Drawing.Point]::new($x+$lean,$ground-19*$scale),[Drawing.Point]::new($x-9*$scale,$ground-29*$scale),[Drawing.Point]::new($x-13*$scale,$ground-36*$scale))
    Stroke $g $p[4] $scale @([Drawing.Point]::new($x+$lean-1,$ground-24*$scale),[Drawing.Point]::new($x+10*$scale,$ground-31*$scale),[Drawing.Point]::new($x+13*$scale,$ground-38*$scale))
}

function Draw-Tower($g,[int]$x,[int]$ground,[int]$w,[int]$h,[System.Drawing.Color[]]$p,[bool]$broken){
    Fill-Rect $g $p[1] $x ($ground-$h) $w $h
    Fill-Rect $g $p[4] ($x+3) ($ground-$h+4) ($w-6) 3
    Fill-Rect $g $p[0] ($x+5) ($ground-$h+11) ($w-10) ($h-15)
    for($y=$ground-$h+15;$y -lt $ground-7;$y+=12){
        for($wx=$x+8;$wx -lt $x+$w-6;$wx+=9){Fill-Rect $g $p[8] $wx $y 3 4}
    }
    Fill-Rect $g $p[0] ($x-3) ($ground-$h-4) ($w+6) 5
    Stroke $g $p[4] 2 @([Drawing.Point]::new($x+[int]($w/2),$ground-$h-4),[Drawing.Point]::new($x+[int]($w/2),$ground-$h-28))
    if($broken){Fill-Poly $g ([Drawing.Color]::Transparent) @([Drawing.Point]::new($x+$w-14,$ground-$h),[Drawing.Point]::new($x+$w,$ground-$h),[Drawing.Point]::new($x+$w,$ground-$h+18))}
}

function Draw-Bridge($g,[int]$x,[int]$ground,[int]$length,[System.Drawing.Color[]]$p,[int]$drop){
    Fill-Rect $g $p[0] $x ($ground-10) $length 10
    Fill-Rect $g $p[5] $x ($ground-10) $length 3
    for($px=$x+12;$px -lt $x+$length;$px+=34){
        Fill-Rect $g $p[2] $px $ground 7 (24+$drop)
        Fill-Rect $g $p[0] ($px+2) $ground 3 (24+$drop)
    }
}

function Draw-BiomeLandmarks($g,[string]$biome,[int]$variant,[int]$width,[int]$height,[int]$ground,[System.Drawing.Color[]]$p,[int]$layer){
    $shift=$variant*73+$layer*29
    switch($biome){
        'Forest' {
            Draw-Tower $g (120+$shift) $ground 42 (105+$layer*9) $p $true
            Draw-Bridge $g (380-$shift) ($ground-12) 250 $p 35
            Draw-DeadTree $g (790-$shift) $ground (1+$layer) $p -4
        }
        'Desert' {
            Draw-Bridge $g (70+$shift) ($ground-8) 420 $p 54
            Draw-Tower $g (650-$shift) $ground 58 132 $p $true
            Fill-Poly $g $p[5] @([Drawing.Point]::new(520,$ground),[Drawing.Point]::new(720,$ground-36),[Drawing.Point]::new(860,$ground))
        }
        'Jungle' {
            Draw-Tower $g (420+$shift) $ground 96 92 $p $true
            for($i=0;$i -lt 5;$i++){Draw-DeadTree $g (70+$i*180+$shift%41) $ground (1+$layer) $p (($i%3)-1)*4}
            Stroke $g $p[8] 3 @([Drawing.Point]::new(370,$ground-96),[Drawing.Point]::new(520,$ground-128),[Drawing.Point]::new(650,$ground-83))
        }
        'Snow' {
            Draw-Tower $g (180+$shift) $ground 52 118 $p $true
            Draw-Bridge $g (470-$shift) ($ground-18) 330 $p 28
            Fill-Rect $g $p[7] 0 ($ground-4) $width 4
        }
        'Corruption' {
            for($i=0;$i -lt 7;$i++){Stroke $g $p[0] (5+$layer) @([Drawing.Point]::new($i*160+$shift%80,$ground),[Drawing.Point]::new($i*160+35,$ground-70-$i%3*25),[Drawing.Point]::new($i*160+15,$ground-130)) }
            Draw-Tower $g (520-$shift) $ground 50 110 $p $true
        }
        'Crimson' {
            for($i=0;$i -lt 6;$i++){Stroke $g $p[0] (7+$layer) @([Drawing.Point]::new($i*180+$shift%60,$ground),[Drawing.Point]::new($i*180+40,$ground-75),[Drawing.Point]::new($i*180+74,$ground-45))}
            Draw-Tower $g (275+$shift) $ground 74 95 $p $true
        }
        'Hallow' {
            for($i=0;$i -lt 5;$i++){Draw-Tower $g (80+$i*190+$shift%55) $ground 48 (90+($i%2)*45) $p ($i%2 -eq 0)}
            Stroke $g $p[9] 2 @([Drawing.Point]::new(0,$ground-25),[Drawing.Point]::new($width,$ground-42))
        }
        'Ocean' {
            Draw-Bridge $g (90+$shift) ($ground-8) 500 $p 65
            Draw-Tower $g (700-$shift) $ground 45 135 $p $true
            Stroke $g $p[8] 4 @([Drawing.Point]::new(540,$ground-12),[Drawing.Point]::new(620,$ground-120),[Drawing.Point]::new(720,$ground-120))
        }
        'Mushroom' {
            for($i=0;$i -lt 8;$i++){
                $mx=40+$i*130+$shift%40;$mh=35+($i%3)*18
                Fill-Rect $g $p[4] ($mx-3) ($ground-$mh) 7 $mh
                Fill-Poly $g $p[8] @([Drawing.Point]::new($mx-28,$ground-$mh),[Drawing.Point]::new($mx,$ground-$mh-22),[Drawing.Point]::new($mx+28,$ground-$mh))
            }
            Draw-Tower $g (450+$shift) $ground 64 90 $p $true
        }
        'Underworld' {
            for($i=0;$i -lt 9;$i++){Fill-Poly $g $p[2] @([Drawing.Point]::new($i*125,$ground),[Drawing.Point]::new($i*125+45,$ground-90-($i%3)*30),[Drawing.Point]::new($i*125+95,$ground))}
            Draw-Bridge $g (260+$shift) ($ground-20) 430 $p 50
            Draw-Tower $g (760-$shift) $ground 55 145 $p $true
        }
        'Engraft' {
            for($i=0;$i -lt 9;$i++){
                $sx=$i*125+$shift%50
                Stroke $g $p[0] (7+$layer) @([Drawing.Point]::new($sx,$ground),[Drawing.Point]::new($sx+25,$ground-65),[Drawing.Point]::new($sx+8,$ground-125))
                Stroke $g $p[8] 2 @([Drawing.Point]::new($sx+2,$ground),[Drawing.Point]::new($sx+27,$ground-65),[Drawing.Point]::new($sx+10,$ground-125))
            }
            Draw-Tower $g (430+$shift) $ground 70 110 $p $true
        }
    }
}

function New-Surface([string]$biome,[int]$variant,[string]$layer,[int]$width,[int]$height,[int]$horizon,[int]$depth){
    $canvas=New-Canvas $width $height
    $p=$palettes[$biome]
    $layerIndex=@{Far=0;Mid=1;Close=2}[$layer]
    Draw-Landscape $canvas.Graphics $width $height $horizon $p (([int][char]$biome[0])*101+$variant*1901+$layerIndex*7919) $depth
    Draw-BiomeLandmarks $canvas.Graphics $biome $variant $width $height ($horizon+20) $p $layerIndex
    Draw-TerrainTexture $canvas.Bitmap $canvas.Graphics $p (([int][char]$biome[0])*313+$variant*3571+$layerIndex*104729) $depth (650+$layerIndex*300)
    Seal-HorizontalSurfaceSeam $canvas.Bitmap
    Save-Canvas $canvas "Content/Backgrounds/$biome/V$($variant)_$layer.png"
}

function New-Underground([string]$biome,[int]$variant,[int]$index){
    # The vanilla four-slot contract alternates narrow transition strips (0/2)
    # with full cave panels (1/3).
    $height=if($index%2 -eq 0){16}else{96}
    $canvas=New-Canvas 160 $height
    $p=$palettes[$biome]
    Fill-Rect $canvas.Graphics $p[1] 0 0 160 $height
    for($x=0;$x -lt 160;$x+=16){
        Fill-Rect $canvas.Graphics $p[2] $x 0 2 $height
        if($height -gt 16){Fill-Rect $canvas.Graphics $p[3] ($x+3) (8+(($x+$variant*7+$index*11)%43)) 9 3}
    }
    if($height -gt 16){
        Draw-Bridge $canvas.Graphics (8+$variant*11) 66 128 $p 18
        Draw-Tower $canvas.Graphics (92-$variant*15) 66 30 48 $p $true
        if($biome -eq 'Engraft'){for($i=0;$i -lt 5;$i++){Stroke $canvas.Graphics $p[8] 2 @([Drawing.Point]::new($i*36,96),[Drawing.Point]::new($i*36+20,34),[Drawing.Point]::new($i*36+8,0))}}
        if($biome -eq 'Mushroom'){for($i=0;$i -lt 4;$i++){Fill-Poly $canvas.Graphics $p[8] @([Drawing.Point]::new($i*45,55),[Drawing.Point]::new($i*45+17,39),[Drawing.Point]::new($i*45+34,55))}}
        if($biome -eq 'Underworld'){Fill-Rect $canvas.Graphics $p[8] 0 83 160 13}
    }else{
        Fill-Rect $canvas.Graphics $p[4] 0 4 160 3
        for($x=8;$x -lt 160;$x+=24){Fill-Rect $canvas.Graphics $p[8] $x 8 7 3}
    }
    Seal-UndergroundWrapStrip $canvas.Bitmap
    Save-Canvas $canvas "Content/Backgrounds/$biome/Underground/V$($variant)_$index.png"
}

foreach($biome in $palettes.Keys){
    for($variant=0;$variant -lt 2;$variant++){
        New-Surface $biome $variant 'Far' 1024 408 205 4
        New-Surface $biome $variant 'Mid' 1024 600 315 3
        New-Surface $biome $variant 'Close' 952 480 265 1
        for($index=0;$index -lt 4;$index++){New-Underground $biome $variant $index}
    }
}

Write-Host 'Generated layered ruined surface and underground background families.'
