param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex) {
    return [System.Drawing.ColorTranslator]::FromHtml($hex)
}

$palettes = @{
    Mawstone = @((C '#171416'), (C '#282223'), (C '#3f342c'), (C '#594329'), (C '#8b5d20'), (C '#c58a27'))
    OssuaryBone = @((C '#29231e'), (C '#594c3b'), (C '#8f7d5f'), (C '#c0ab7f'), (C '#ddc995'), (C '#f0dda7'))
    KesslerPlating = @((C '#18191b'), (C '#292c2f'), (C '#41454a'), (C '#5d5d5d'), (C '#8e3f29'), (C '#e36926'))
    HelixContainmentPanel = @((C '#252a2b'), (C '#50595a'), (C '#808b89'), (C '#b3bdb8'), (C '#e4e7df'), (C '#69c66f'))
    SentrixPanel = @((C '#080d13'), (C '#111d28'), (C '#1f3442'), (C '#31576b'), (C '#3c9bc1'), (C '#8edcf2'))
    KesslerRuinBlock = @((C '#201b19'), (C '#39312d'), (C '#50423a'), (C '#725242'), (C '#984a2b'), (C '#d06a2d'))
    HelixRuinBlock = @((C '#292c2c'), (C '#4c5351'), (C '#737d78'), (C '#9ca69f'), (C '#c6cac0'), (C '#70a86c'))
    SentrixRuinBlock = @((C '#090d12'), (C '#15212a'), (C '#273844'), (C '#3c5663'), (C '#397d96'), (C '#6eb2c9'))
    PrewarConcrete = @((C '#25231f'), (C '#3c3932'), (C '#585248'), (C '#756c5d'), (C '#958873'), (C '#b4a589'))
    MawResearchBlock = @((C '#1c1a18'), (C '#302b25'), (C '#4a4030'), (C '#675434'), (C '#916826'), (C '#c18a2c'))
}

function Save-RecoloredBlockSheet {
    param(
        [string]$Template,
        [string]$Destination,
        [System.Drawing.Color[]]$Palette,
        [int]$Seed,
        [ValidateSet('plain', 'amber-vein', 'bone-pore')]
        [string]$Detail = 'plain'
    )

    $source = [System.Drawing.Bitmap]::new($Template)
    $output = [System.Drawing.Bitmap]::new($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $pixel = $source.GetPixel($x, $y)
                if ($pixel.A -eq 0 -or ($pixel.R -eq 247 -and $pixel.G -eq 119 -and $pixel.B -eq 249)) {
                    $output.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
                    continue
                }

                $hash = [Math]::Abs((($x + 19) * 73856093) -bxor (($y + 37) * 19349663) -bxor $Seed)
                $luminance = [int](($pixel.R * 3 + $pixel.G * 6 + $pixel.B) / 10)
                if ($luminance -lt 38) { $index = 0 }
                elseif ($luminance -lt 72) { $index = 1 }
                elseif ($luminance -lt 108) { $index = 2 }
                elseif ($luminance -lt 150) { $index = 3 }
                elseif ($luminance -lt 205) { $index = 4 }
                else { $index = 5 }

                if ($Detail -eq 'amber-vein' -and $hash % 127 -lt 3) { $index = 5 }
                elseif ($Detail -eq 'bone-pore' -and $hash % 83 -lt 5) { $index = 0 + ($hash % 2) }
                elseif ($hash % 41 -eq 0) { $index = [Math]::Min(5, $index + 1) }
                $output.SetPixel($x, $y, $Palette[$index])
            }
        }

        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Destination) | Out-Null
        $output.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $source.Dispose()
        $output.Dispose()
    }
}

function New-Canvas([int]$width, [int]$height) {
    $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    return @{ Bitmap = $bitmap; Graphics = $graphics }
}

function Copy-FrameToTileSheet {
    param(
        [System.Drawing.Bitmap]$Frame,
        [System.Drawing.Bitmap]$Sheet,
        [int]$FrameIndex
    )

    for ($y = 0; $y -lt 64; $y++) {
        for ($x = 0; $x -lt 48; $x++) {
            $color = $Frame.GetPixel($x, $y)
            $destinationX = [int][Math]::Floor($x / 16.0) * 18 + ($x % 16)
            $destinationY = $FrameIndex * 72 + [int][Math]::Floor($y / 16.0) * 18 + ($y % 16)
            $Sheet.SetPixel($destinationX, $destinationY, $color)
        }
    }
}

function Draw-KesslerRack([System.Drawing.Graphics]$g, [int]$frame) {
    $dark = [System.Drawing.SolidBrush]::new((C '#17191b'))
    $steel = [System.Drawing.SolidBrush]::new((C '#34383b'))
    $mid = [System.Drawing.SolidBrush]::new((C '#55595b'))
    $rust = [System.Drawing.SolidBrush]::new((C '#8d3925'))
    $orange = [System.Drawing.SolidBrush]::new((C ($frame % 2 -eq 0 ? '#ef6a25' : '#7d2d20')))
    try {
        $g.FillRectangle($dark, 3, 2, 42, 61)
        $g.FillRectangle($steel, 6, 5, 36, 55)
        $g.FillRectangle($dark, 10, 8, 28, 48)
        $g.FillRectangle($mid, 18, 12, 12, 8)
        $g.FillRectangle($dark, 20, 13, 8, 5)
        $g.FillRectangle($mid, 14, 22, 20, 19)
        $g.FillRectangle($dark, 17, 24, 14, 13)
        $g.FillRectangle($rust, 10, 23 + ($frame % 2), 5, 24)
        $g.FillRectangle($rust, 33, 23 + (($frame + 1) % 2), 5, 24)
        $g.FillRectangle($mid, 15, 41, 7, 14)
        $g.FillRectangle($mid, 26, 41, 7, 14)
        $g.FillRectangle($orange, 7, 7, 4, 3)
        $g.FillRectangle($orange, 37, 7, 4, 3)
        $g.FillRectangle($dark, 2, 58, 44, 5)
        $g.FillRectangle($orange, 8 + $frame * 7, 59, 5, 2)
    }
    finally {
        $dark.Dispose(); $steel.Dispose(); $mid.Dispose(); $rust.Dispose(); $orange.Dispose()
    }
}

function Draw-HelixTank([System.Drawing.Graphics]$g, [int]$frame) {
    $dark = [System.Drawing.SolidBrush]::new((C '#263033'))
    $white = [System.Drawing.SolidBrush]::new((C '#dce3dc'))
    $gray = [System.Drawing.SolidBrush]::new((C '#7e8b87'))
    $glass = [System.Drawing.SolidBrush]::new((C '#315f49'))
    $green = [System.Drawing.SolidBrush]::new((C '#67c969'))
    $organism = [System.Drawing.Pen]::new((C '#172720'), 4)
    try {
        $g.FillRectangle($dark, 4, 1, 40, 63)
        $g.FillRectangle($white, 7, 3, 34, 7)
        $g.FillRectangle($white, 7, 54, 34, 7)
        $g.FillRectangle($gray, 7, 11, 5, 42)
        $g.FillRectangle($gray, 36, 11, 5, 42)
        $g.FillRectangle($glass, 12, 11, 24, 42)
        $bodyX = 24 + @(-2, 0, 2, 0)[$frame]
        $g.DrawLine($organism, $bodyX, 18, $bodyX, 42)
        $g.DrawLine($organism, $bodyX, 25, 16 + $frame, 34)
        $g.DrawLine($organism, $bodyX, 29, 32 - $frame, 38)
        $g.DrawLine($organism, $bodyX, 40, 18, 49 - ($frame % 2))
        $g.DrawLine($organism, $bodyX, 40, 31, 50 + ($frame % 2))
        $g.FillRectangle($green, 18 + $frame * 3, 56, 4, 2)
        $g.FillRectangle($green, 9, 6, 3, 2)
    }
    finally {
        $dark.Dispose(); $white.Dispose(); $gray.Dispose(); $glass.Dispose(); $green.Dispose(); $organism.Dispose()
    }
}

function Draw-SentrixCore([System.Drawing.Graphics]$g, [int]$frame) {
    $black = [System.Drawing.SolidBrush]::new((C '#080d13'))
    $panel = [System.Drawing.SolidBrush]::new((C '#142735'))
    $cyan = [System.Drawing.SolidBrush]::new((C '#43bde7'))
    $white = [System.Drawing.SolidBrush]::new((C '#a7e8f5'))
    try {
        $g.FillRectangle($black, 8, 48, 32, 14)
        $g.FillRectangle($panel, 12, 44, 24, 14)
        $g.FillRectangle($black, 20, 34, 8, 13)
        $radius = 8 + ($frame % 2) * 2
        $g.FillRectangle($cyan, 24 - $radius, 21, $radius * 2, 2)
        $g.FillRectangle($cyan, 24 - $radius, 21 + $radius * 2 - 2, $radius * 2, 2)
        $g.FillRectangle($cyan, 24 - $radius, 23, 2, $radius * 2 - 4)
        $g.FillRectangle($cyan, 24 + $radius - 2, 23, 2, $radius * 2 - 4)
        $g.FillRectangle($white, 21 + $frame, 27, 6, 6)
        $g.FillRectangle($cyan, 6, 52 + $frame, 5, 2)
        $g.FillRectangle($cyan, 37, 55 - $frame, 5, 2)
        $g.FillRectangle($white, 17 + $frame * 4, 59, 3, 2)
    }
    finally {
        $black.Dispose(); $panel.Dispose(); $cyan.Dispose(); $white.Dispose()
    }
}

function Save-FixtureSheet([string]$Destination, [ValidateSet('Kessler', 'Helix', 'Sentrix')] [string]$Kind) {
    $sheet = [System.Drawing.Bitmap]::new(54, 288, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($frameIndex = 0; $frameIndex -lt 4; $frameIndex++) {
            $canvas = New-Canvas 48 64
            try {
                switch ($Kind) {
                    'Kessler' { Draw-KesslerRack $canvas.Graphics $frameIndex }
                    'Helix' { Draw-HelixTank $canvas.Graphics $frameIndex }
                    'Sentrix' { Draw-SentrixCore $canvas.Graphics $frameIndex }
                }
                Copy-FrameToTileSheet $canvas.Bitmap $sheet $frameIndex
            }
            finally {
                $canvas.Graphics.Dispose()
                $canvas.Bitmap.Dispose()
            }
        }
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Destination) | Out-Null
        $sheet.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $sheet.Dispose()
    }
}

$template = Join-Path $Root 'Content/Tiles/LockedBulkhead.png'
if (-not (Test-Path -LiteralPath $template)) {
    throw "Missing block-sheet template: $template"
}

$blockSpecs = @(
    @('Mawstone', 8101, 'amber-vein'),
    @('OssuaryBone', 8102, 'bone-pore'),
    @('KesslerPlating', 8201, 'plain'),
    @('HelixContainmentPanel', 8202, 'plain'),
    @('SentrixPanel', 8203, 'plain'),
    @('KesslerRuinBlock', 8301, 'plain'),
    @('HelixRuinBlock', 8302, 'plain'),
    @('SentrixRuinBlock', 8303, 'plain'),
    @('PrewarConcrete', 8304, 'plain'),
    @('MawResearchBlock', 8305, 'amber-vein')
)

foreach ($spec in $blockSpecs) {
    Save-RecoloredBlockSheet `
        -Template $template `
        -Destination (Join-Path $Root "Content/Tiles/$($spec[0]).png") `
        -Palette $palettes[$spec[0]] `
        -Seed $spec[1] `
        -Detail $spec[2]
}

Save-FixtureSheet (Join-Path $Root 'Content/Tiles/KesslerPowerArmorRack.png') 'Kessler'
Save-FixtureSheet (Join-Path $Root 'Content/Tiles/HelixSymbioteTank.png') 'Helix'
Save-FixtureSheet (Join-Path $Root 'Content/Tiles/SentrixHologramCore.png') 'Sentrix'

Write-Host 'Generated framed Maw, faction structure, ruin, and animated signature-fixture pixel art.'
