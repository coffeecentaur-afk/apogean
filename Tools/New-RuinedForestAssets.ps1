param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [Parameter(Mandatory = $true)]
    [string]$ExampleModRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex) {
    return [System.Drawing.ColorTranslator]::FromHtml($hex)
}

$wood = @(
    (C '#241b18'),
    (C '#3b2a22'),
    (C '#5a3e2b'),
    (C '#795735'),
    (C '#a17a45'),
    (C '#c2aa79')
)
$deadGrass = @(
    (C '#211b17'),
    (C '#3b3023'),
    (C '#59442a'),
    (C '#765b32'),
    (C '#9a7434'),
    (C '#b39456')
)
$deadFlower = @(
    (C '#211b18'),
    (C '#403127'),
    (C '#63452d'),
    (C '#815d34'),
    (C '#ad7f2f'),
    (C '#c5a45c')
)

function Is-GuidePixel([System.Drawing.Color]$color) {
    return $color.A -eq 0 -or ($color.R -eq 247 -and $color.G -eq 119 -and $color.B -eq 249)
}

function Save-RecoloredTemplate {
    param(
        [string]$Source,
        [string]$Destination,
        [System.Drawing.Color[]]$Palette,
        [int]$Seed,
        [switch]$TreeMask
    )

    $sourceBitmap = [System.Drawing.Bitmap]::new($Source)
    $output = [System.Drawing.Bitmap]::new(
        $sourceBitmap.Width,
        $sourceBitmap.Height,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

    try {
        for ($y = 0; $y -lt $sourceBitmap.Height; $y++) {
            for ($x = 0; $x -lt $sourceBitmap.Width; $x++) {
                $sourceColor = $sourceBitmap.GetPixel($x, $y)
                if (Is-GuidePixel $sourceColor) {
                    $output.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
                    continue
                }

                $hash = [Math]::Abs((($x + 17) * 73856093) -bxor (($y + 31) * 19349663) -bxor $Seed)
                $luminance = [int](($sourceColor.R * 3 + $sourceColor.G * 6 + $sourceColor.B) / 10)
                if ($luminance -lt 32) { $index = 0 }
                elseif ($luminance -lt 90) { $index = 1 }
                elseif ($luminance -lt 155) { $index = 2 }
                elseif ($luminance -lt 220) { $index = 3 }
                else { $index = 2 + ($hash % 2) }

                if ($TreeMask -and $hash % 43 -eq 0) { $index = 5 }
                elseif ($hash % 29 -eq 0) { $index = [Math]::Min(4, $index + 1) }

                $output.SetPixel($x, $y, $Palette[$index])
            }
        }

        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Destination) | Out-Null
        $output.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $sourceBitmap.Dispose()
        $output.Dispose()
    }
}

function New-TransparentBitmap([int]$width, [int]$height) {
    $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    return @{ Bitmap = $bitmap; Graphics = $graphics }
}

function Draw-BranchPath {
    param(
        [System.Drawing.Graphics]$Graphics,
        [System.Drawing.Point[]]$Points,
        [System.Drawing.Color[]]$Palette
    )

    $outline = [System.Drawing.Pen]::new($Palette[0], 7)
    $body = [System.Drawing.Pen]::new($Palette[2], 4)
    $highlight = [System.Drawing.Pen]::new($Palette[4], 1)
    try {
        foreach ($pen in @($outline, $body, $highlight)) {
            $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Flat
            $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Flat
            $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Miter
            $Graphics.DrawLines($pen, $Points)
        }
    }
    finally {
        $outline.Dispose()
        $body.Dispose()
        $highlight.Dispose()
    }
}

function Save-TreeTops([string]$Destination) {
    $canvas = New-TransparentBitmap 246 82
    $bitmap = $canvas.Bitmap
    $graphics = $canvas.Graphics
    $podBrush = [System.Drawing.SolidBrush]::new($deadGrass[4])
    try {
        for ($variant = 0; $variant -lt 3; $variant++) {
            $left = $variant * 82
            $center = $left + 40
            $lean = @(-3, 2, 5)[$variant]
            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new($center, 79),
                [System.Drawing.Point]::new($center + $lean, 50),
                [System.Drawing.Point]::new($center + $lean + 2, 20),
                [System.Drawing.Point]::new($center + $lean + 7, 8)
            ) $wood
            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new($center + $lean, 54),
                [System.Drawing.Point]::new($center - 18, 37),
                [System.Drawing.Point]::new($center - 27, 21),
                [System.Drawing.Point]::new($center - 25, 11)
            ) $wood
            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new($center + $lean + 1, 44),
                [System.Drawing.Point]::new($center + 18, 31),
                [System.Drawing.Point]::new($center + 26, 16)
            ) $wood
            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new($center - 18, 37),
                [System.Drawing.Point]::new($center - 31, 34)
            ) $wood
            $graphics.FillRectangle($podBrush, $center - 29, 22 + $variant * 2, 3, 4)
            $graphics.FillRectangle($podBrush, $center + 25, 20 + $variant, 2, 3)
        }
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Destination) | Out-Null
        $bitmap.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $podBrush.Dispose()
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function Save-TreeBranches([string]$Destination) {
    $canvas = New-TransparentBitmap 84 126
    $bitmap = $canvas.Bitmap
    $graphics = $canvas.Graphics
    try {
        for ($row = 0; $row -lt 3; $row++) {
            $top = $row * 42
            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new(39, $top + 33),
                [System.Drawing.Point]::new(25, $top + 25 - $row * 2),
                [System.Drawing.Point]::new(13, $top + 13 + $row),
                [System.Drawing.Point]::new(9, $top + 5 + $row * 2)
            ) $wood
            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new(25, $top + 25 - $row * 2),
                [System.Drawing.Point]::new(15, $top + 27 + $row)
            ) $wood

            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new(44, $top + 33),
                [System.Drawing.Point]::new(58, $top + 25 - $row * 2),
                [System.Drawing.Point]::new(70, $top + 13 + $row),
                [System.Drawing.Point]::new(74, $top + 5 + $row * 2)
            ) $wood
            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new(58, $top + 25 - $row * 2),
                [System.Drawing.Point]::new(68, $top + 27 + $row)
            ) $wood
        }
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Destination) | Out-Null
        $bitmap.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function Save-Sapling([string]$Destination) {
    $canvas = New-TransparentBitmap 54 38
    $bitmap = $canvas.Bitmap
    $graphics = $canvas.Graphics
    try {
        for ($variant = 0; $variant -lt 3; $variant++) {
            $left = $variant * 18
            $center = $left + 8
            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new($center, 35),
                [System.Drawing.Point]::new($center + ($variant - 1), 21),
                [System.Drawing.Point]::new($center + $variant - 2, 7)
            ) $wood
            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new($center, 22),
                [System.Drawing.Point]::new($center - 5, 15)
            ) $wood
            Draw-BranchPath $graphics @(
                [System.Drawing.Point]::new($center, 17),
                [System.Drawing.Point]::new($center + 5, 11)
            ) $wood
        }
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Destination) | Out-Null
        $bitmap.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

$exampleBlock = Join-Path $ExampleModRoot 'Content/Tiles/ExampleBlock.png'
$exampleTree = Join-Path $ExampleModRoot 'Content/Tiles/Plants/ExampleTree.png'
$exampleWall = Join-Path $ExampleModRoot 'Content/Walls/ExampleWall.png'
foreach ($required in @($exampleBlock, $exampleTree, $exampleWall)) {
    if (-not (Test-Path -LiteralPath $required)) {
        throw "Missing official ExampleMod template: $required"
    }
}

Save-RecoloredTemplate $exampleBlock (Join-Path $Root 'Content/Tiles/DeadGrass.png') $deadGrass 1103
Save-RecoloredTemplate $exampleTree (Join-Path $Root 'Content/Tiles/DeadForestTree.png') $wood 2207 -TreeMask
Save-TreeBranches (Join-Path $Root 'Content/Tiles/DeadForestTree_Branches.png')
Save-TreeTops (Join-Path $Root 'Content/Tiles/DeadForestTree_Tops.png')
Save-Sapling (Join-Path $Root 'Content/Tiles/DeadForestSapling.png')
Save-RecoloredTemplate $exampleWall (Join-Path $Root 'Content/Walls/DeadGrassWallUnsafe.png') $deadGrass 3301
Save-RecoloredTemplate $exampleWall (Join-Path $Root 'Content/Walls/DeadFlowerWallUnsafe.png') $deadFlower 4409

Write-Host 'Generated native framed dead-grass, dead-tree, sapling, and unsafe wall assets.'
