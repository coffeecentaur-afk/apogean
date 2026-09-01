param(
    [string]$GeneratedRoot = 'C:\Users\max_h\.codex\generated_images\01a05af6-a783-7c71-95d1-747436f4fdbc'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

function Ensure-Directory {
    param([string]$Path)
    New-Item -ItemType Directory -Force -Path $Path | Out-Null
}

function Save-NativeSprite {
    param(
        [string]$Source,
        [string]$Destination,
        [int]$Size
    )

    $sourceBitmap = [System.Drawing.Bitmap]::new($Source)
    try {
        $left = $sourceBitmap.Width
        $top = $sourceBitmap.Height
        $right = -1
        $bottom = -1

        for ($y = 0; $y -lt $sourceBitmap.Height; $y++) {
            for ($x = 0; $x -lt $sourceBitmap.Width; $x++) {
                if ($sourceBitmap.GetPixel($x, $y).A -eq 0) { continue }
                if ($x -lt $left) { $left = $x }
                if ($x -gt $right) { $right = $x }
                if ($y -lt $top) { $top = $y }
                if ($y -gt $bottom) { $bottom = $y }
            }
        }

        if ($right -lt $left -or $bottom -lt $top) {
            throw "No opaque pixels found in $Source"
        }

        $contentWidth = $right - $left + 1
        $contentHeight = $bottom - $top + 1
        $innerSize = $Size - 4
        $scale = [Math]::Min($innerSize / [double]$contentWidth, $innerSize / [double]$contentHeight)
        $drawWidth = [Math]::Max(1, [int][Math]::Round($contentWidth * $scale))
        $drawHeight = [Math]::Max(1, [int][Math]::Round($contentHeight * $scale))
        $drawX = [int][Math]::Floor(($Size - $drawWidth) / 2)
        $drawY = [int][Math]::Floor(($Size - $drawHeight) / 2)

        $output = [System.Drawing.Bitmap]::new($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $graphics = [System.Drawing.Graphics]::FromImage($output)
            try {
                $graphics.Clear([System.Drawing.Color]::Transparent)
                $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                $sourceRect = [System.Drawing.Rectangle]::new($left, $top, $contentWidth, $contentHeight)
                $destinationRect = [System.Drawing.Rectangle]::new($drawX, $drawY, $drawWidth, $drawHeight)
                $graphics.DrawImage($sourceBitmap, $destinationRect, $sourceRect, [System.Drawing.GraphicsUnit]::Pixel)
            }
            finally {
                $graphics.Dispose()
            }

            Ensure-Directory (Split-Path -Parent $Destination)
            $output.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $output.Dispose()
        }
    }
    finally {
        $sourceBitmap.Dispose()
    }
}

function Get-ColorMatrix {
    param([ValidateSet('Day', 'Night', 'Eclipse')][string]$Lighting)

    switch ($Lighting) {
        'Night' {
            return [System.Drawing.Imaging.ColorMatrix]::new([single[][]]@(
                [single[]]@(0.30, 0, 0, 0, 0),
                [single[]]@(0, 0.36, 0, 0, 0),
                [single[]]@(0, 0, 0.54, 0, 0),
                [single[]]@(0, 0, 0, 1, 0),
                [single[]]@(0.01, 0.015, 0.055, 0, 1)
            ))
        }
        'Eclipse' {
            return [System.Drawing.Imaging.ColorMatrix]::new([single[][]]@(
                [single[]]@(0.38, 0, 0, 0, 0),
                [single[]]@(0, 0.18, 0, 0, 0),
                [single[]]@(0, 0, 0.20, 0, 0),
                [single[]]@(0, 0, 0, 1, 0),
                [single[]]@(0.075, 0.004, 0.004, 0, 1)
            ))
        }
        default {
            return [System.Drawing.Imaging.ColorMatrix]::new()
        }
    }
}

function Save-BackgroundVariant {
    param(
        [string]$Source,
        [string]$Destination,
        [ValidateSet('Day', 'Night', 'Eclipse')][string]$Lighting
    )

    $sourceBitmap = [System.Drawing.Bitmap]::new($Source)
    try {
        $output = [System.Drawing.Bitmap]::new(960, 410, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $graphics = [System.Drawing.Graphics]::FromImage($output)
            $attributes = [System.Drawing.Imaging.ImageAttributes]::new()
            try {
                $graphics.Clear([System.Drawing.Color]::Black)
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                $attributes.SetColorMatrix((Get-ColorMatrix $Lighting))
                $destinationRect = [System.Drawing.Rectangle]::new(0, 0, $output.Width, $output.Height)
                $graphics.DrawImage($sourceBitmap, $destinationRect, 0, 0, $sourceBitmap.Width, $sourceBitmap.Height,
                    [System.Drawing.GraphicsUnit]::Pixel, $attributes)
            }
            finally {
                $attributes.Dispose()
                $graphics.Dispose()
            }

            Ensure-Directory (Split-Path -Parent $Destination)
            $output.Save($Destination, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $output.Dispose()
        }
    }
    finally {
        $sourceBitmap.Dispose()
    }
}

$weapons = @(
    @{ Name = 'RendHook'; File = 'exec-928396d7-b8c2-4d09-972a-70d0628dbad5.png'; Size = 56 },
    @{ Name = 'AmberSiphon'; File = 'exec-a97f50d4-d022-450b-876d-2224c57a387a.png'; Size = 52 },
    @{ Name = 'SinewBow'; File = 'exec-c9089f04-f7e5-4c8e-856c-9ece8e15c3fe.png'; Size = 52 },
    @{ Name = 'MawEffigy'; File = 'exec-0064eb33-6ac4-4533-ae7b-cec4479b8995.png'; Size = 48 }
)

foreach ($weapon in $weapons) {
    $source = Join-Path $GeneratedRoot $weapon.File
    $sourceCopy = Join-Path $projectRoot "Art/Source/Weapons/$($weapon.Name)-source.png"
    Ensure-Directory (Split-Path -Parent $sourceCopy)
    Copy-Item -LiteralPath $source -Destination $sourceCopy -Force
    Save-NativeSprite -Source $source -Destination (Join-Path $projectRoot "Content/Items/Weapons/$($weapon.Name).png") -Size $weapon.Size
}

$backgrounds = @(
    @{ Biome = 'Forest'; V0 = 'exec-cf62ea5b-6175-460e-8930-eff84aa41efe.png'; V1 = 'exec-7f996d97-c4dc-45ec-bb79-08070f1b58a0.png' },
    @{ Biome = 'Desert'; V0 = 'exec-9027704d-e2bb-4cea-bbc0-92b2b20817ba.png'; V1 = 'exec-70ca3eb2-2409-4d6e-aa79-90dd5e1bf576.png' },
    @{ Biome = 'Jungle'; V0 = 'exec-0fee420c-5160-47bf-a765-1cb1f783a3f4.png'; V1 = 'exec-1b3a6b61-92b8-4fe8-a8a5-ac35beae32bd.png' },
    @{ Biome = 'Snow'; V0 = 'exec-e5ceb9ac-b23d-47e3-b054-b694f60801a2.png'; V1 = 'exec-5a0787f6-34e8-455d-845e-0079acabb35e.png' },
    @{ Biome = 'Corruption'; V0 = 'exec-33ddc7f6-ca7b-4f61-b34d-a1b2cde554ff.png'; V1 = 'exec-dd18dd8f-c542-4048-8875-c61aa9a35f97.png' },
    @{ Biome = 'Crimson'; V0 = 'exec-ebc8c07f-39d4-4f1a-8a11-309d3e2f5989.png'; V1 = 'exec-bdba5d93-7131-4c4e-b58a-e49209808588.png' },
    @{ Biome = 'Hallow'; V0 = 'exec-7231fc10-a38c-4f7e-a08e-51b2dd913e53.png'; V1 = 'exec-3f703efb-94f2-4d28-a95b-833828065106.png' },
    @{ Biome = 'Ocean'; V0 = 'exec-af5b6831-c4b4-4bdd-9776-d66580d44ab7.png'; V1 = 'exec-5ea9b573-c7bb-44e6-84bf-87f571a5e916.png' },
    @{ Biome = 'Engraft'; V0 = 'exec-49e47457-054c-4189-8890-080e27a0838d.png'; V1 = 'exec-65bd809a-cb05-44c7-a54b-6a0e0683848c.png' }
)

foreach ($background in $backgrounds) {
    foreach ($variant in 0..1) {
        $generatedFile = if ($variant -eq 0) { $background.V0 } else { $background.V1 }
        $source = Join-Path $GeneratedRoot $generatedFile
        $sourceCopy = Join-Path $projectRoot "Art/Source/Backgrounds/$($background.Biome)/V$variant-Day-source.png"
        Ensure-Directory (Split-Path -Parent $sourceCopy)
        Copy-Item -LiteralPath $source -Destination $sourceCopy -Force

        foreach ($lighting in @('Day', 'Night', 'Eclipse')) {
            $destination = Join-Path $projectRoot "Content/Backgrounds/$($background.Biome)/V$($variant)_$lighting.png"
            Save-BackgroundVariant -Source $source -Destination $destination -Lighting $lighting
        }
    }
}

$transparent = [System.Drawing.Bitmap]::new(16, 16, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    Ensure-Directory (Join-Path $projectRoot 'Content/Backgrounds')
    $transparent.Save((Join-Path $projectRoot 'Content/Backgrounds/Transparent.png'), [System.Drawing.Imaging.ImageFormat]::Png)
}
finally {
    $transparent.Dispose()
}

Write-Host 'Imported native weapon sprites and 54 seeded/time-aware background textures.'
