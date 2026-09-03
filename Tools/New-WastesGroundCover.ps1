param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Convert-HexColor([string]$hex) {
    [System.Drawing.ColorTranslator]::FromHtml($hex)
}

$outline = Convert-HexColor '#211916'
$shadow = Convert-HexColor '#35261F'
$wood = Convert-HexColor '#59402A'
$ochre = Convert-HexColor '#8F6C3D'
$straw = Convert-HexColor '#B9924E'
$pale = Convert-HexColor '#D0B66F'
$amber = Convert-HexColor '#D78A19'

function New-Canvas([int]$width, [int]$height) {
    $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    return @{ Bitmap = $bitmap; Graphics = $graphics }
}

function Save-Canvas($canvas, [string]$relativePath) {
    $path = Join-Path $Root $relativePath
    $directory = Split-Path -Parent $path
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    try {
        $canvas.Bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $canvas.Graphics.Dispose()
        $canvas.Bitmap.Dispose()
    }
}

function Draw-Twig(
    [System.Drawing.Graphics]$graphics,
    [System.Drawing.Point[]]$points,
    [int]$bodyWidth = 1,
    [System.Drawing.Color]$bodyColor = $wood,
    [switch]$Highlight
) {
    $edgePen = [System.Drawing.Pen]::new($outline, $bodyWidth + 2)
    $bodyPen = [System.Drawing.Pen]::new($bodyColor, $bodyWidth)
    try {
        foreach ($pen in @($edgePen, $bodyPen)) {
            $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Flat
            $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Flat
            $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Miter
            $graphics.DrawLines($pen, $points)
        }
        if ($Highlight -and $points.Length -ge 2) {
            $highlightPen = [System.Drawing.Pen]::new($straw, 1)
            try {
                $highlightPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Flat
                $highlightPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Flat
                $graphics.DrawLine($highlightPen, $points[0], $points[1])
            }
            finally {
                $highlightPen.Dispose()
            }
        }
    }
    finally {
        $edgePen.Dispose()
        $bodyPen.Dispose()
    }
}

function Draw-Pod([System.Drawing.Graphics]$graphics, [int]$x, [int]$y) {
    $edgeBrush = [System.Drawing.SolidBrush]::new($outline)
    $podBrush = [System.Drawing.SolidBrush]::new($amber)
    $shineBrush = [System.Drawing.SolidBrush]::new($pale)
    try {
        $graphics.FillRectangle($edgeBrush, $x - 1, $y - 1, 5, 6)
        $graphics.FillRectangle($podBrush, $x, $y, 3, 4)
        $graphics.FillRectangle($shineBrush, $x, $y, 1, 1)
    }
    finally {
        $edgeBrush.Dispose()
        $podBrush.Dispose()
        $shineBrush.Dispose()
    }
}

function New-RootTufts {
    $canvas = New-Canvas 72 18
    $g = $canvas.Graphics
    for ($variant = 0; $variant -lt 4; $variant++) {
        $left = $variant * 18
        $baseX = $left + @(8, 9, 7, 10)[$variant]
        $tipY = @(5, 3, 6, 4)[$variant]
        Draw-Twig $g @([System.Drawing.Point]::new($baseX, 16), [System.Drawing.Point]::new($baseX - 2, 10), [System.Drawing.Point]::new($left + 3, $tipY)) 1 $ochre -Highlight
        Draw-Twig $g @([System.Drawing.Point]::new($baseX, 16), [System.Drawing.Point]::new($baseX + 2, 10), [System.Drawing.Point]::new($left + 15, $tipY + 2)) 1 $wood
        Draw-Twig $g @([System.Drawing.Point]::new($baseX - 1, 16), [System.Drawing.Point]::new($left + 2, 13)) 1 $shadow
        Draw-Twig $g @([System.Drawing.Point]::new($baseX + 1, 16), [System.Drawing.Point]::new($left + 16, 14)) 1 $shadow
        if ($variant -eq 1) {
            Draw-Twig $g @([System.Drawing.Point]::new($baseX - 1, 11), [System.Drawing.Point]::new($baseX + 1, 5)) 1 $straw
        }
        if ($variant -eq 3) {
            Draw-Pod $g ($left + 13) 8
        }
    }
    Save-Canvas $canvas 'Content/Tiles/DeadTuft.png'
}

function New-Bristles {
    $canvas = New-Canvas 54 36
    $g = $canvas.Graphics
    for ($variant = 0; $variant -lt 3; $variant++) {
        $left = $variant * 18
        $baseX = $left + @(8, 9, 7)[$variant]
        $lean = @(-2, 1, 3)[$variant]
        Draw-Twig $g @([System.Drawing.Point]::new($baseX, 34), [System.Drawing.Point]::new($baseX + $lean, 20), [System.Drawing.Point]::new($baseX + $lean, 6)) 1 $ochre -Highlight
        Draw-Twig $g @([System.Drawing.Point]::new($baseX + $lean, 14), [System.Drawing.Point]::new($left + 3, 9 - $variant)) 1 $wood
        Draw-Twig $g @([System.Drawing.Point]::new($baseX + $lean, 20), [System.Drawing.Point]::new($left + 14, 15 + $variant)) 1 $straw
        Draw-Twig $g @([System.Drawing.Point]::new($baseX, 34), [System.Drawing.Point]::new($left + 3, 31)) 1 $shadow
        Draw-Twig $g @([System.Drawing.Point]::new($baseX, 34), [System.Drawing.Point]::new($left + 14, 32)) 1 $shadow
        if ($variant -eq 2) {
            Draw-Twig $g @([System.Drawing.Point]::new($left + 13, 16), [System.Drawing.Point]::new($left + 15, 10)) 1 $pale
        }
    }
    Save-Canvas $canvas 'Content/Tiles/WastesBristle.png'
}

function New-RootShrubs {
    $canvas = New-Canvas 108 36
    $g = $canvas.Graphics
    for ($variant = 0; $variant -lt 3; $variant++) {
        $left = $variant * 36
        $center = $left + @(17, 18, 16)[$variant]
        Draw-Twig $g @([System.Drawing.Point]::new($center, 33), [System.Drawing.Point]::new($center - 8, 25), [System.Drawing.Point]::new($left + 4, 32)) 2 $wood -Highlight
        Draw-Twig $g @([System.Drawing.Point]::new($center, 33), [System.Drawing.Point]::new($center + 8, 25), [System.Drawing.Point]::new($left + 32, 32)) 2 $wood
        Draw-Twig $g @([System.Drawing.Point]::new($center - 7, 27), [System.Drawing.Point]::new($left + 8, 34)) 1 $ochre
        Draw-Twig $g @([System.Drawing.Point]::new($center + 7, 27), [System.Drawing.Point]::new($left + 27, 34)) 1 $ochre
        Draw-Twig $g @([System.Drawing.Point]::new($center, 30), [System.Drawing.Point]::new($center - 3, 18), [System.Drawing.Point]::new($left + 9, 8 + $variant)) 2 $ochre -Highlight
        Draw-Twig $g @([System.Drawing.Point]::new($center - 3, 18), [System.Drawing.Point]::new($left + 4, 16)) 1 $wood
        Draw-Twig $g @([System.Drawing.Point]::new($center - 1, 24), [System.Drawing.Point]::new($center + 9, 16), [System.Drawing.Point]::new($left + 30, 10 - $variant)) 2 $wood
        Draw-Twig $g @([System.Drawing.Point]::new($center + 9, 16), [System.Drawing.Point]::new($left + 31, 18)) 1 $straw
        if ($variant -eq 0) {
            Draw-Pod $g ($left + 5) 12
        }
        elseif ($variant -eq 1) {
            Draw-Pod $g ($left + 27) 7
        }
        else {
            Draw-Pod $g ($left + 29) 13
        }
    }
    Save-Canvas $canvas 'Content/Tiles/WastesRootShrub.png'
}

New-RootTufts
New-Bristles
New-RootShrubs
Write-Host 'Generated three native-scale Wastes ground-cover sheets.' -ForegroundColor Green
