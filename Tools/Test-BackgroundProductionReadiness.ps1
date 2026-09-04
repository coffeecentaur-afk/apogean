Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$failures = [Collections.Generic.List[string]]::new()
$biomes = @('Forest', 'Desert', 'Jungle', 'Snow', 'Corruption', 'Crimson', 'Hallow', 'Ocean', 'Mushroom')

foreach ($biome in $biomes) {
    foreach ($layer in @('Far', 'Mid', 'Close')) {
        $path = Join-Path $root "Content/Backgrounds/Diagnostics/HD/${biome}ConceptV0_${layer}.png"
        if (-not (Test-Path -LiteralPath $path)) { $failures.Add("missing $biome $layer"); continue }
        $bitmap = [Drawing.Bitmap]::new($path)
        try {
            if ($bitmap.Width -lt 2048 -or $bitmap.Height -lt 720) {
                $failures.Add("$biome $layer is $($bitmap.Width)x$($bitmap.Height); production target is at least 2048x720 native-detail source art")
            }
        }
        finally { $bitmap.Dispose() }
    }
}

$renderer = Get-Content -Raw -LiteralPath (Join-Path $root 'Content/Backgrounds/HighDefinitionSurfaceBackgroundRenderer.cs')
if ($renderer -match 'texture\.Height\s*-\s*1') {
    $failures.Add('renderer still stretches the final source row for vertical coverage; final layers need authored lower coverage')
}
foreach ($contract in @('HorizontalParallax', 'VerticalParallax', 'PositiveModulo', 'Main.screenPosition.X', 'Main.worldSurface')) {
    if ($renderer -notmatch [regex]::Escape($contract)) { $failures.Add("renderer missing $contract") }
}
if ($failures.Count -gt 0) {
    Write-Host 'BACKGROUND PRODUCTION READINESS: RED' -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'BACKGROUND PRODUCTION READINESS: STATIC PASS. Full live camera and routing matrix is still required.' -ForegroundColor Green
