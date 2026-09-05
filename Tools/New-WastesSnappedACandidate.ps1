param([string]$Root = (Split-Path -Parent $PSScriptRoot))

# Review-only export. This tool has deliberately no Promote switch and never writes
# Content/. Native assembly is an offline drawing-contract preview, not live evidence.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$destination = Join-Path $Root 'Art/Candidates/WastesSnappedA-v1'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$sourcePath = Join-Path $Root 'Art/Source/Trees/WastesSnappedA-components-source-v1.png'
$trunkPath = Join-Path $Root 'Content/Tiles/DeadForestTree.png'
if ((Get-FileHash -LiteralPath $trunkPath -Algorithm SHA256).Hash -ne 'F472C977973DE49B95CA4B6B4ED923113061F2C2597DD4672798F05B4EFA60CF') {
    throw 'The reviewed trunk input changed. Re-contract this candidate rather than silently changing its material.'
}
$source = [Drawing.Bitmap]::new($sourcePath)
$trunk = [Drawing.Bitmap]::new($trunkPath)
$tops = [Drawing.Bitmap]::new(246, 82)
$branches = [Drawing.Bitmap]::new(84, 126)
$palette = @('#211916', '#35261f', '#523827', '#735033', '#967041', '#ccb17d') | ForEach-Object { [Drawing.ColorTranslator]::FromHtml($_) }

function Quantize([Drawing.Color]$pixel) {
    # Source generation returned RGB with a pale simulated checkerboard, not alpha.
    # All six pieces are dark wood; the inspected backdrop has every channel >220.
    if ($pixel.A -lt 128 -or ($pixel.R -gt 220 -and $pixel.G -gt 220 -and $pixel.B -gt 220)) { return [Drawing.Color]::Transparent }
    $best = $palette[0]; $distance = [double]::MaxValue
    foreach ($color in $palette) {
        $d = [Math]::Pow($pixel.R - $color.R, 2) + [Math]::Pow($pixel.G - $color.G, 2) + [Math]::Pow($pixel.B - $color.B, 2)
        if ($d -lt $distance) { $best = $color; $distance = $d }
    }
    return $best
}

function Extract([int[]]$rectangle, [int]$width, [int]$height) {
    $piece = [Drawing.Bitmap]::new($width, $height)
    for ($y = 0; $y -lt $height; $y++) {
        for ($x = 0; $x -lt $width; $x++) {
            $sx = $rectangle[0] + [int][Math]::Floor(($x + 0.5) * $rectangle[2] / $width)
            $sy = $rectangle[1] + [int][Math]::Floor(($y + 0.5) * $rectangle[3] / $height)
            $piece.SetPixel($x, $y, (Quantize $source.GetPixel($sx, $sy)))
        }
    }
    return ,$piece
}

function Blit($g, $texture, [int]$x, [int]$y, [int]$sx, [int]$sy, [int]$w, [int]$h, [int]$scale = 1) {
    $g.DrawImage($texture, [Drawing.Rectangle]::new($x, $y, $w * $scale, $h * $scale), $sx, $sy, $w, $h, [Drawing.GraphicsUnit]::Pixel)
}

function Graphics-For($bitmap, [string]$background) {
    $g = [Drawing.Graphics]::FromImage($bitmap)
    $g.Clear([Drawing.ColorTranslator]::FromHtml($background))
    $g.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $g.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::Half
    $g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::None
    return ,$g
}

try {
    if ($source.Width -ne 1536 -or $source.Height -ne 1024) { throw 'Source crop contract requires the inspected 1536x1024 source.' }
    $caps = @(@(236, 29, 142, 523), @(678, 45, 144, 508), @(1138, 144, 169, 409))
    for ($frame = 0; $frame -lt 3; $frame++) {
        $height = @(58, 56, 40)[$frame]
        $piece = Extract $caps[$frame] 16 $height
        try {
            $fractureTop = @()
            for ($x = 0; $x -lt 16; $x++) {
                $first = $height
                for ($y = 0; $y -lt $height; $y++) {
                    if ($piece.GetPixel($x, $y).A -gt 0) { $first = $y; break }
                }
                $fractureTop += $first
            }
            for ($y = 0; $y -lt $height; $y++) {
                for ($x = 0; $x -lt 16; $x++) {
                    $pixel = $piece.GetPixel($x, $y)
                    $atlasY = 80 - $height + $y
                    # Generated microtexture was denser than the real trunk. Keep
                    # its fracture silhouette and exposed end, but use the actual
                    # bark pixels throughout the shaft instead of hiding a seam.
                    if ($atlasY -ge 64 -or ($pixel.A -gt 0 -and $y -gt $fractureTop[$x] + 2)) {
                        $pixel = $trunk.GetPixel($x + 2, $atlasY % 16)
                        if ($atlasY -lt 64 -and ($x -eq 0 -or $x -eq 15 -or $piece.GetPixel([Math]::Max(0, $x - 1), $y).A -eq 0 -or $piece.GetPixel([Math]::Min(15, $x + 1), $y).A -eq 0)) { $pixel = $palette[0] }
                    }
                    $tops.SetPixel($frame * 82 + 32 + $x, $atlasY, $pixel)
                }
            }
        }
        finally { $piece.Dispose() }
    }
    $stubs = @(@(112, 708, 352, 200), @(598, 708, 313, 200), @(1053, 708, 346, 200))
    for ($variant = 0; $variant -lt 3; $variant++) {
        $piece = Extract $stubs[$variant] 24 16
        try {
            for ($y = 0; $y -lt 16; $y++) {
                for ($x = 0; $x -lt 24; $x++) {
                    $pixel = $piece.GetPixel($x, $y)
                    # Trim the source's T-shaped mounting post into a short woody
                    # attachment centered on y=24 (right atlas uses y=30).
                    if ($x -ge 17 -and ($y -lt 7 -or $y -gt 13)) { $pixel = [Drawing.Color]::Transparent }
                    if ($pixel.A -gt 0 -and $x -gt 5) {
                        $pixel = $trunk.GetPixel(2 + ($x % 16), ($y + $variant * 3) % 16)
                    }
                    $branches.SetPixel(16 + $x, $variant * 42 + 14 + $y, $pixel)
                    # Installed renderer's right pivot is six pixels lower than
                    # the left pivot: (0,30) versus (40,24), not a naive mirror.
                    $branches.SetPixel(42 + 23 - $x, $variant * 42 + 20 + $y, $pixel)
                }
            }
        }
        finally { $piece.Dispose() }
    }
    $tops.Save((Join-Path $destination 'DeadForestTree_Tops.png'), [Drawing.Imaging.ImageFormat]::Png)
    $branches.Save((Join-Path $destination 'DeadForestTree_Branches.png'), [Drawing.Imaging.ImageFormat]::Png)
    Copy-Item -LiteralPath $trunkPath -Destination (Join-Path $destination 'DeadForestTree.png')

    # Atlas inspection board: native 22px trunk pitch / 20px drawn cells.
    $atlasBoard = [Drawing.Bitmap]::new(1120, 1110)
    $ag = Graphics-For $atlasBoard '#889aa4'
    $font = [Drawing.Font]::new('Consolas', 14)
    try {
        $ag.DrawString('ACTUAL CANDIDATE ATLASES / 4X NEAREST-NEIGHBOR', $font, [Drawing.Brushes]::Black, 12, 8)
        Blit $ag $trunk 10 44 0 0 176 264 4
        Blit $ag $tops 722 44 0 0 82 82 4
        Blit $ag $tops 722 385 82 0 82 82 4
        Blit $ag $tops 722 726 164 0 82 82 4
        $grid = [Drawing.Pen]::new([Drawing.Color]::FromArgb(90, 255, 255, 255))
        try {
            for ($row = 0; $row -lt 12; $row++) {
                for ($col = 0; $col -lt 8; $col++) {
                    $ag.DrawRectangle($grid, 10 + $col * 88, 44 + $row * 88, 80, 80)
                    $ag.DrawString("$col,$row", $font, [Drawing.Brushes]::White, 10 + $col * 88, 44 + $row * 88)
                }
            }
        }
        finally { $grid.Dispose() }
        $atlasBoard.Save((Join-Path $destination 'Atlas-inspection.png'), [Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $font.Dispose(); $ag.Dispose(); $atlasBoard.Dispose() }

    # GrowTree frame roles from the installed build: center root (88,132),
    # left root (44,132), right root (22,132), 22px pitch, 20px draw on 16px tiles.
    # DrawTrees: top origin (40,80) at (tileX*16+8,tileY*16+16).
    $scene = [Drawing.Bitmap]::new(520, 350)
    $sg = Graphics-For $scene '#8ca2ad'
    $labelFont = [Drawing.Font]::new('Consolas', 10)
    try {
        $baseY = 320
        for ($treeIndex = 0; $treeIndex -lt 4; $treeIndex++) {
            $centerX = @(62, 191, 321, 452)[$treeIndex]
            $height = @(6, 11, 16, 4)[$treeIndex]
            $topY = $baseY - $height * 16
            $variant = $treeIndex % 3
            for ($level = 1; $level -lt $height; $level++) {
                $fy = (($level * 7 + $treeIndex) % 3) * 22
                Blit $sg $trunk ($centerX - 10) ($topY + $level * 16) 0 $fy 20 20
            }
            # Native root cells, no separately scaled flare or overlay.
            Blit $sg $trunk ($centerX - 10) ($baseY - 16) 88 (132 + $variant * 22) 20 20
            Blit $sg $trunk ($centerX - 26) ($baseY - 16) 44 (132 + $variant * 22) 20 20
            Blit $sg $trunk ($centerX + 6) ($baseY - 16) 22 (132 + $variant * 22) 20 20
            if ($treeIndex -lt 3) {
                Blit $sg $tops ($centerX - 40) ($topY - 64) ($variant * 82) 0 80 80
                $leftY = $baseY - (@(3, 5, 8)[$variant]) * 16
                Blit $sg $trunk ($centerX - 10) $leftY 88 ($variant * 22) 20 20
                Blit $sg $branches ($centerX - 48) ($leftY - 12) 0 ($variant * 42) 40 40
                if ($treeIndex -gt 0) {
                    $rightY = $baseY - (@(2, 8, 12)[$variant]) * 16
                    Blit $sg $trunk ($centerX - 10) $rightY 66 (66 + $variant * 22) 20 20
                    Blit $sg $branches ($centerX + 8) ($rightY - 12) 42 ($variant * 42) 40 40
                }
            }
            else {
                # Real cut-state atlas frame rather than a resized complete tree.
                Blit $sg $trunk ($centerX - 10) $topY 0 198 20 20
            }
            $caption = if ($treeIndex -eq 3) { 'CUT STATE' } else { "$height TILES" }
            $sg.DrawString($caption, $labelFont, [Drawing.Brushes]::Black, $centerX - 35, 333)
        }
        $ground = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#4b3927'), 2)
        try { $sg.DrawLine($ground, 12, $baseY + 3, 508, $baseY + 3) } finally { $ground.Dispose() }
        $scene.Save((Join-Path $destination 'Native-assembly.png'), [Drawing.Imaging.ImageFormat]::Png)

        $board = [Drawing.Bitmap]::new(1100, 690)
        $bg = Graphics-For $board '#1f2930'
        $heading = [Drawing.Font]::new('Consolas', 17, [Drawing.FontStyle]::Bold)
        $small = [Drawing.Font]::new('Consolas', 11)
        $reference = [Drawing.Bitmap]::new((Join-Path $Root 'Art/Reference/2026-09-04-Wastes-Deadwood-Study.png'))
        try {
            $bg.DrawString('A / SNAPPED   -   NATIVE ASSET REVIEW', $heading, [Drawing.Brushes]::White, 22, 16)
            $bg.DrawString('Offline assembly from the candidate PNGs. Not installed. Live wind/chop tests still pending.', $small, [Drawing.Brushes]::LightGray, 22, 49)
            $bg.DrawString('APPROVED A STUDY', $small, [Drawing.Brushes]::White, 22, 88)
            $bg.DrawImage($reference, [Drawing.Rectangle]::new(22, 120, 174, 350), 235, 220, 174, 480, [Drawing.GraphicsUnit]::Pixel)
            $bg.DrawString('Study (resized)', $small, [Drawing.Brushes]::LightGray, 22, 478)
            $bg.DrawString('ACTUAL SPRITE SIZE / 1X', $small, [Drawing.Brushes]::White, 240, 88)
            Blit $bg $scene 240 120 0 0 520 350
            $bg.DrawString('6 / 11 / 16 native trunk cells + a chopped remainder', $small, [Drawing.Brushes]::LightGray, 240, 478)
            $bg.DrawString('FRACTURES + JOINTS / 4X', $small, [Drawing.Brushes]::White, 785, 88)
            $panelBrush = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#8ca2ad'))
            try { $bg.FillRectangle($panelBrush, 785, 120, 292, 350) } finally { $panelBrush.Dispose() }
            for ($frame = 0; $frame -lt 3; $frame++) {
                Blit $bg $tops (795 + $frame * 94) 126 ($frame * 82 + 30) 16 20 64 4
                Blit $bg $trunk (795 + $frame * 94) 382 0 0 20 20 4
            }
            $bg.DrawString('Continuous trunk material', $small, [Drawing.Brushes]::LightGray, 785, 478)
            $bg.DrawString('BRANCH CELLS / 3X    LEFT + RIGHT PIVOTS HAVE DIFFERENT VERTICAL OFFSETS', $small, [Drawing.Brushes]::White, 22, 523)
            for ($frame = 0; $frame -lt 3; $frame++) {
                Blit $bg $branches (30 + $frame * 340) 550 0 ($frame * 42) 40 40 3
                Blit $bg $branches (180 + $frame * 340) 550 42 ($frame * 42) 40 40 3
            }
            $board.Save((Join-Path $destination 'Native-comparison.png'), [Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $reference.Dispose(); $small.Dispose(); $heading.Dispose(); $bg.Dispose(); $board.Dispose() }
    }
    finally { $labelFont.Dispose(); $sg.Dispose(); $scene.Dispose() }
    Write-Host "Candidate-only PNGs exported to $destination; no Content asset changed."
}
finally { $source.Dispose(); $trunk.Dispose(); $tops.Dispose(); $branches.Dispose() }
