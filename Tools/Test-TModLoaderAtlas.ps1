param(
    [Parameter(Mandatory = $true)]
    [string]$Atlas,
    [string]$ReferenceAtlas,
    [int]$ExpectedWidth = 0,
    [int]$ExpectedHeight = 0,
    [int]$MaximumOpaqueColors = 16,
    [switch]$AllowOpaqueWhite,
    [switch]$AllowSoftAlpha
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$failures = [Collections.Generic.List[string]]::new()
$atlasPath = (Resolve-Path -LiteralPath $Atlas).Path
$bitmap = [Drawing.Bitmap]::new($atlasPath)
try {
    if ($ExpectedWidth -gt 0 -and $bitmap.Width -ne $ExpectedWidth) {
        $failures.Add("Width is $($bitmap.Width), expected $ExpectedWidth")
    }
    if ($ExpectedHeight -gt 0 -and $bitmap.Height -ne $ExpectedHeight) {
        $failures.Add("Height is $($bitmap.Height), expected $ExpectedHeight")
    }

    $colors = [Collections.Generic.HashSet[int]]::new()
    $opaque = 0
    $soft = 0
    $white = 0
    for ($y = 0; $y -lt $bitmap.Height; $y++) {
        for ($x = 0; $x -lt $bitmap.Width; $x++) {
            $pixel = $bitmap.GetPixel($x, $y)
            if ($pixel.A -gt 0 -and $pixel.A -lt 255) { $soft++ }
            if ($pixel.A -eq 255) {
                $opaque++
                [void]$colors.Add($pixel.ToArgb())
                if ($pixel.R -eq 255 -and $pixel.G -eq 255 -and $pixel.B -eq 255) { $white++ }
            }
        }
    }

    if ($opaque -eq 0) { $failures.Add('Atlas contains no opaque artwork') }
    if (-not $AllowSoftAlpha -and $soft -gt 0) { $failures.Add("Atlas contains $soft soft-alpha pixels") }
    if (-not $AllowOpaqueWhite -and $white -gt 0) { $failures.Add("Atlas contains $white opaque-white pixels") }
    if ($colors.Count -gt $MaximumOpaqueColors) {
        $failures.Add("Atlas uses $($colors.Count) opaque colors; maximum is $MaximumOpaqueColors")
    }

    if ($ReferenceAtlas) {
        $referencePath = (Resolve-Path -LiteralPath $ReferenceAtlas).Path
        $reference = [Drawing.Bitmap]::new($referencePath)
        try {
            if ($reference.Width -ne $bitmap.Width -or $reference.Height -ne $bitmap.Height) {
                $failures.Add("Reference is $($reference.Width)x$($reference.Height), atlas is $($bitmap.Width)x$($bitmap.Height)")
            }
            else {
                $alphaMismatches = 0
                for ($y = 0; $y -lt $bitmap.Height; $y++) {
                    for ($x = 0; $x -lt $bitmap.Width; $x++) {
                        $sourceVisible = $reference.GetPixel($x, $y).A -gt 0
                        $atlasVisible = $bitmap.GetPixel($x, $y).A -gt 0
                        if ($sourceVisible -ne $atlasVisible) { $alphaMismatches++ }
                    }
                }
                if ($alphaMismatches -gt 0) {
                    $failures.Add("Atlas changes native alpha topology at $alphaMismatches pixels")
                }
            }
        }
        finally { $reference.Dispose() }
    }

    if ($failures.Count -gt 0) {
        Write-Host "TMODLOADER ATLAS: FAIL ($($failures.Count) problems)" -ForegroundColor Red
        foreach ($failure in $failures) { Write-Host " - $failure" -ForegroundColor Red }
        exit 1
    }

    Write-Host "TMODLOADER ATLAS: PASS — $($bitmap.Width)x$($bitmap.Height), $opaque opaque pixels, $($colors.Count) colors" -ForegroundColor Green
}
finally {
    $bitmap.Dispose()
}
