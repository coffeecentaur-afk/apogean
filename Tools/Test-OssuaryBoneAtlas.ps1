param(
    [string]$Atlas = (Join-Path (Split-Path -Parent $PSScriptRoot) 'Content/Tiles/OssuaryBone.png')
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$shell = (Get-Process -Id $PID).Path
& $shell -NoProfile -File (Join-Path $PSScriptRoot 'Test-TModLoaderAtlas.ps1') -Atlas $Atlas -ExpectedWidth 288 -ExpectedHeight 270 -MaximumOpaqueColors 16
if ($LASTEXITCODE -ne 0) { exit 1 }
Add-Type -AssemblyName System.Drawing
$bitmap = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $Atlas).Path)
try {
    $occupied = 0; $full = 0; $contoured = 0
    $masks = [Collections.Generic.HashSet[string]]::new()
    for ($row = 0; $row -lt 15; $row++) {
        for ($col = 0; $col -lt 16; $col++) {
            $mask = [Text.StringBuilder]::new(256)
            $visible = 0
            for ($dy = 0; $dy -lt 16; $dy++) {
                for ($dx = 0; $dx -lt 16; $dx++) {
                    $solid = $bitmap.GetPixel($col * 18 + $dx, $row * 18 + $dy).A -gt 0
                    [void]$mask.Append($(if ($solid) { '1' } else { '0' }))
                    if ($solid) { $visible++ }
                }
            }
            if ($visible -gt 0) {
                $occupied++
                [void]$masks.Add($mask.ToString())
                if ($visible -eq 256) { $full++ } else { $contoured++ }
            }
        }
    }
    Write-Host "BONE EDGE BASELINE: occupied=$occupied full=$full contoured=$contoured uniqueMasks=$($masks.Count)"
    # Necessary, not sufficient: the next organic structural material must have
    # exterior edge silhouettes. This does NOT assert every vanilla material
    # shares one alpha mask, or that a square connected block is invalid to tML.
    if ($contoured -eq 0 -or $masks.Count -lt 2) {
        Write-Host 'FAIL: all populated frame bodies are the same opaque square; no exterior/isolated edge silhouette is available.' -ForegroundColor Red
        exit 1
    }
    Write-Host 'PASS: edge diversity exists. Exact frame mapping, native merge/slope behavior and art approval still require separate evidence.' -ForegroundColor Green
}
finally { $bitmap.Dispose() }
