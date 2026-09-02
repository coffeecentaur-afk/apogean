param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }
$p = @((C '#211916'), (C '#35261f'), (C '#523827'), (C '#735033'), (C '#967041'), (C '#ccb17d'), (C '#efdbae'), (C '#9f5d13'), (C '#d18a20'))

function New-Canvas([int]$width, [int]$height) {
    $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    return @{ Bitmap = $bitmap; Graphics = $graphics }
}

function Save-Canvas($canvas, [string]$path) {
    $canvas.Bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $canvas.Graphics.Dispose(); $canvas.Bitmap.Dispose()
}

function Draw-Path([System.Drawing.Graphics]$g, [System.Drawing.Point[]]$points, [int]$width = 5, [switch]$Bone) {
    $bodyColor = if ($Bone) { $p[5] } else { $p[3] }
    $highlightColor = if ($Bone) { $p[6] } else { $p[4] }
    $outline = [System.Drawing.Pen]::new($p[0], $width + 3)
    $body = [System.Drawing.Pen]::new($bodyColor, $width)
    $highlight = [System.Drawing.Pen]::new($highlightColor, 1)
    try {
        foreach ($pen in @($outline, $body, $highlight)) {
            $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Flat
            $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Flat
            $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Miter
            $g.DrawLines($pen, $points)
        }
    } finally { $outline.Dispose(); $body.Dispose(); $highlight.Dispose() }
}

function Add-AmberStrand([System.Drawing.Graphics]$g, [int]$x, [int]$y, [int]$length) {
    $pen = [System.Drawing.Pen]::new($p[7], 2)
    $tip = [System.Drawing.SolidBrush]::new($p[8])
    try {
        $g.DrawLine($pen, $x, $y, $x, $y + $length)
        $g.FillRectangle($tip, $x, $y + $length, 2, 2)
    } finally { $pen.Dispose(); $tip.Dispose() }
}

function New-TrunkSheet {
    $path = Join-Path $Root 'Content/Tiles/DeadForestTree.png'
    $temporaryPath = Join-Path $Root 'Content/Tiles/DeadForestTree.generated.png'
    $mask = [System.Drawing.Bitmap]::new($path)
    $out = [System.Drawing.Bitmap]::new($mask.Width, $mask.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($y = 0; $y -lt $mask.Height; $y++) {
            for ($x = 0; $x -lt $mask.Width; $x++) {
                if ($mask.GetPixel($x, $y).A -eq 0) { continue }
                $edge = $false
                foreach ($offset in @(@(-1,0),@(1,0),@(0,-1),@(0,1))) {
                    $nx = $x + $offset[0]; $ny = $y + $offset[1]
                    if ($nx -lt 0 -or $ny -lt 0 -or $nx -ge $mask.Width -or $ny -ge $mask.Height -or $mask.GetPixel($nx,$ny).A -eq 0) { $edge = $true; break }
                }
                $frameX = [int]($x / 22); $frameY = [int]($y / 22)
                $localX = $x % 22; $localY = $y % 22
                $hash = [Math]::Abs((($x + 7) * 73856093) -bxor (($y + 11) * 19349663))
                $color = if ($edge) { $p[0] } elseif ($hash % 13 -lt 3) { $p[2] } elseif ($hash % 11 -lt 2) { $p[4] } else { $p[3] }
                if ((($frameX + $frameY * 3) % 6) -eq 0 -and $localX -ge 7 -and $localX -le 9 -and $localY -ge 5 -and $localY -le 14) { $color = $p[5 + (($localY + $frameX) % 2)] }
                if ((($frameX * 5 + $frameY) % 9) -eq 0 -and $localX -eq 12 -and $localY -ge 9 -and $localY -le 14) { $color = $p[7 + ($localY % 2)] }
                if ((($frameX + $frameY) % 7) -eq 0 -and $localX -ge 5 -and $localX -le 10 -and $localY -ge 7 -and $localY -le 10) { $color = $p[1] }
                $out.SetPixel($x, $y, $color)
            }
        }
        $out.Save($temporaryPath, [System.Drawing.Imaging.ImageFormat]::Png)
    } finally { $mask.Dispose(); $out.Dispose() }
    Move-Item -LiteralPath $temporaryPath -Destination $path -Force
}

function New-TreeTops {
    $canvas = New-Canvas 246 82
    $g = $canvas.Graphics
    for ($variant = 0; $variant -lt 3; $variant++) {
        $left = $variant * 82; $center = $left + 41; $lean = @(-5, 2, 6)[$variant]
        Draw-Path $g @([System.Drawing.Point]::new($center,81),[System.Drawing.Point]::new($center+$lean,55),[System.Drawing.Point]::new($center+$lean-2,31),[System.Drawing.Point]::new($center+$lean+4,8)) 7
        Draw-Path $g @([System.Drawing.Point]::new($center+$lean,58),[System.Drawing.Point]::new($center-18,45),[System.Drawing.Point]::new($center-31,29),[System.Drawing.Point]::new($center-36,12)) 5
        Draw-Path $g @([System.Drawing.Point]::new($center-19,44),[System.Drawing.Point]::new($center-35,47),[System.Drawing.Point]::new($center-39,40)) 3
        Draw-Path $g @([System.Drawing.Point]::new($center+$lean,48),[System.Drawing.Point]::new($center+20,39),[System.Drawing.Point]::new($center+33,23),[System.Drawing.Point]::new($center+35,9)) 5
        Draw-Path $g @([System.Drawing.Point]::new($center+20,39),[System.Drawing.Point]::new($center+37,43),[System.Drawing.Point]::new($center+40,35)) 3
        if ($variant -eq 1) { Draw-Path $g @([System.Drawing.Point]::new($center+$lean,33),[System.Drawing.Point]::new($center-5,17),[System.Drawing.Point]::new($center-13,7)) 4 -Bone }
        if ($variant -eq 2) { Draw-Path $g @([System.Drawing.Point]::new($center-17,45),[System.Drawing.Point]::new($center-26,31),[System.Drawing.Point]::new($center-22,18)) 3 -Bone }
        Add-AmberStrand $g ($center - 31) (29 + $variant) (8 + $variant * 2)
        Add-AmberStrand $g ($center + 33) (24 + $variant) (12 - $variant)
    }
    Save-Canvas $canvas (Join-Path $Root 'Content/Tiles/DeadForestTree_Tops.png')
}

function New-TreeBranches {
    $canvas = New-Canvas 84 126
    $g = $canvas.Graphics
    for ($row = 0; $row -lt 3; $row++) {
        $top = $row * 42
        Draw-Path $g @([System.Drawing.Point]::new(40,$top+37),[System.Drawing.Point]::new(27,$top+29),[System.Drawing.Point]::new(14,$top+17),[System.Drawing.Point]::new(7,$top+5+$row*2)) 4
        Draw-Path $g @([System.Drawing.Point]::new(27,$top+29),[System.Drawing.Point]::new(12,$top+31),[System.Drawing.Point]::new(7,$top+25)) 2
        Draw-Path $g @([System.Drawing.Point]::new(44,$top+37),[System.Drawing.Point]::new(57,$top+29),[System.Drawing.Point]::new(70,$top+17),[System.Drawing.Point]::new(77,$top+5+$row*2)) 4
        Draw-Path $g @([System.Drawing.Point]::new(57,$top+29),[System.Drawing.Point]::new(72,$top+31),[System.Drawing.Point]::new(77,$top+25)) 2
        if ($row -eq 1) { Draw-Path $g @([System.Drawing.Point]::new(57,$top+29),[System.Drawing.Point]::new(68,$top+18)) 2 -Bone }
        Add-AmberStrand $g (11 + $row * 2) ($top + 17) (5 + $row)
    }
    Save-Canvas $canvas (Join-Path $Root 'Content/Tiles/DeadForestTree_Branches.png')
}

function New-DeadTufts {
    $canvas = New-Canvas 54 18
    $g = $canvas.Graphics
    for ($variant = 0; $variant -lt 3; $variant++) {
        $left = $variant * 18; $base = $left + 8
        Draw-Path $g @([System.Drawing.Point]::new($base,16),[System.Drawing.Point]::new($base-1,9),[System.Drawing.Point]::new($base-5,4+$variant)) 1
        Draw-Path $g @([System.Drawing.Point]::new($base,16),[System.Drawing.Point]::new($base+2,10),[System.Drawing.Point]::new($base+6,6-$variant)) 1
        Draw-Path $g @([System.Drawing.Point]::new($base-3,16),[System.Drawing.Point]::new($base-6,11),[System.Drawing.Point]::new($base-7,8)) 1
        if ($variant -eq 2) { Add-AmberStrand $g ($base+5) 7 4 }
    }
    Save-Canvas $canvas (Join-Path $Root 'Content/Tiles/DeadTuft.png')
}

function New-RootWall {
    $canvas = New-Canvas 32 32
    $g = $canvas.Graphics
    $soil = [System.Drawing.SolidBrush]::new($p[2]); $dark = [System.Drawing.SolidBrush]::new($p[1])
    try {
        $g.FillRectangle($soil,0,0,32,32)
        for ($y=2;$y -lt 32;$y+=7) { $g.FillRectangle($dark, (($y*3)%11), $y, 8, 2) }
        Draw-Path $g @([System.Drawing.Point]::new(1,3),[System.Drawing.Point]::new(10,10),[System.Drawing.Point]::new(6,20),[System.Drawing.Point]::new(15,31)) 2
        Draw-Path $g @([System.Drawing.Point]::new(31,2),[System.Drawing.Point]::new(20,11),[System.Drawing.Point]::new(25,20),[System.Drawing.Point]::new(17,31)) 2
        Draw-Path $g @([System.Drawing.Point]::new(4,31),[System.Drawing.Point]::new(13,22),[System.Drawing.Point]::new(21,17),[System.Drawing.Point]::new(29,10)) 1
        Add-AmberStrand $g 13 12 5
    } finally { $soil.Dispose(); $dark.Dispose() }
    Save-Canvas $canvas (Join-Path $Root 'Content/Walls/DeadFlowerWallUnsafe.png')
}

New-TrunkSheet
New-TreeTops
New-TreeBranches
New-DeadTufts
New-RootWall
Write-Host 'Generated native-scale Wastes trunks, crowns, branches, root tufts, and tangled root walls.'
