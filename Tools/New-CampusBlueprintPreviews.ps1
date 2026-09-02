param([string]$Root = (Split-Path -Parent $PSScriptRoot))

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function C([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }

$colors = @{
    KesslerPlating=(C '#59504a'); KesslerPlatform=(C '#aa572f'); KesslerBulkheadWall=(C '#29282a'); KesslerWindowWall=(C '#46342e')
    KesslerChair=(C '#c26733'); KesslerTable=(C '#b55e31'); KesslerWorkbench=(C '#d06b31'); KesslerLight=(C '#ff512d'); KesslerConsole=(C '#e84428'); KesslerLocker=(C '#72574a'); KesslerPowerArmorRack=(C '#ef7a37')
    HelixContainmentPanel=(C '#cbd2ca'); HelixPlatform=(C '#8eb39a'); HelixLaboratoryWall=(C '#3a4542'); HelixObservationWall=(C '#315944')
    HelixChair=(C '#d9ddd6'); HelixTable=(C '#b8c4ba'); HelixWorkbench=(C '#a9b9ad'); HelixLight=(C '#68dc78'); HelixConsole=(C '#55b967'); HelixLocker=(C '#778b81'); HelixSymbioteTank=(C '#d6a33e')
    SentrixPanel=(C '#254b61'); SentrixPlatform=(C '#2c91b8'); SentrixDataWall=(C '#102838'); SentrixWindowWall=(C '#16475d')
    SentrixChair=(C '#4fc5e8'); SentrixTable=(C '#328baa'); SentrixWorkbench=(C '#3b9dbd'); SentrixLight=(C '#87ebff'); SentrixConsole=(C '#49c8ef'); SentrixLocker=(C '#315f75'); SentrixHologramCore=(C '#b0f3ff')
}

function Parse-Blueprint([string]$path) {
    $lines = Get-Content -LiteralPath $path
    $width = 0; $height = 0; $commands = [System.Collections.Generic.List[object]]::new()
    foreach ($raw in $lines) {
        $line = $raw.Trim()
        if ($line.Length -eq 0 -or $line.StartsWith('#')) { continue }
        $parts = $line -split '\s+'
        if ($parts[0] -eq 'size') { $width=[int]$parts[1]; $height=[int]$parts[2]; continue }
        if ($parts[0] -eq 'entrance') { continue }
        $commands.Add($parts)
    }
    return @{ Width=$width; Height=$height; Commands=$commands }
}

function Render-Blueprint([string]$name) {
    $path = Join-Path $Root "Content/Structures/Blueprints/$name.apstructure"
    $blueprint = Parse-Blueprint $path
    $scale = 4
    $bitmap = [System.Drawing.Bitmap]::new($blueprint.Width*$scale, $blueprint.Height*$scale, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    $g.Clear((C '#090b0d'))
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
    try {
        foreach ($parts in $blueprint.Commands) {
            $op = $parts[0]
            if ($op -eq 'clear' -or $op -eq 'erase') { continue }
            $asset = $parts[1]
            $x=[int]$parts[2]; $y=[int]$parts[3]; $w=[int]$parts[4]; $h=if($op -eq 'platform'){1}else{[int]$parts[5]}
            $color = $colors[$asset]
            if ($null -eq $color) { continue }
            $brush = [System.Drawing.SolidBrush]::new($color)
            try {
                if ($op -eq 'frame') {
                    $thickness=[int]$parts[6]
                    $g.FillRectangle($brush,$x*$scale,$y*$scale,$w*$scale,$thickness*$scale)
                    $g.FillRectangle($brush,$x*$scale,($y+$h-$thickness)*$scale,$w*$scale,$thickness*$scale)
                    $g.FillRectangle($brush,$x*$scale,$y*$scale,$thickness*$scale,$h*$scale)
                    $g.FillRectangle($brush,($x+$w-$thickness)*$scale,$y*$scale,$thickness*$scale,$h*$scale)
                } else {
                    $g.FillRectangle($brush,$x*$scale,$y*$scale,$w*$scale,$h*$scale)
                }
            } finally { $brush.Dispose() }
        }
        New-Item -ItemType Directory -Force -Path (Join-Path $Root 'Art/Preview') | Out-Null
        $bitmap.Save((Join-Path $Root "Art/Preview/$name-runtime-blueprint-v2.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    } finally { $g.Dispose(); $bitmap.Dispose() }
}

foreach ($name in @('KesslerCampus','HelixCampus','SentrixCampus')) { Render-Blueprint $name }
Write-Host 'Rendered the three runtime Campus blueprints.'
