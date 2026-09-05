param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$ReferenceRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$shell = (Get-Process -Id $PID).Path
$validator = Join-Path $PSScriptRoot 'Test-TModLoaderAtlas.ps1'
# Native renderer exports, not copies of our own potentially broken atlas, are
# authoritative. Grass contains white engine-mask pixels at specific coordinates.
$materials = @(
    @('Soil', 'Dirt', 'Dirt', 'DirtUnsafe', 5, 5),
    @('Grass', 'Grass', 'Grass', 'GrassUnsafe', 11, 5),
    @('Stone', 'Stone', 'Stone', 'Stone', 10, 6),
    @('Sand', 'Sand', 'Sand', 'Sandstone', 11, 5),
    @('Ice', 'Ice', 'Ice', 'IceUnsafe', 11, 6),
    @('Snow', 'Snow', 'Snow', 'SnowUnsafe', 9, 5),
    @('Mud', 'Mud', 'Mud', 'MudUnsafe', 10, 6)
)
$failed = [Collections.Generic.List[string]]::new()
foreach ($material in $materials) {
    foreach ($wall in @($false, $true)) {
        $relative = if ($wall) { "Content/Walls/Wastes$($material[2])WallUnsafe.png" } else { "Content/Tiles/Wastes$($material[0]).png" }
        $reference = if ($wall) { "Vanilla-$($material[3])-Wall.png" } else { "Vanilla-$($material[1])-Tile.png" }
        $width = if ($wall) { 468 } else { 288 }
        $height = if ($wall) { 180 } elseif ($material[0] -eq 'Grass') { 1980 } else { 270 }
        $paletteLimit = if ($wall) { $material[5] } else { $material[4] }
        $arguments = @('-Atlas', (Join-Path $Root $relative), '-ReferenceAtlas', (Join-Path $ReferenceRoot $reference), '-ExpectedWidth', $width, '-ExpectedHeight', $height, '-MaximumOpaqueColors', $paletteLimit)
        if (-not $wall -and $material[0] -eq 'Grass') { $arguments += '-PreserveReferenceWhiteMask' }
        Write-Host "ATLAS: $relative"
        & $shell -NoProfile -ExecutionPolicy Bypass -File $validator @arguments
        if ($LASTEXITCODE -ne 0) { $failed.Add($relative) }
    }
}
if ($failed.Count -gt 0) {
    $failed | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'PASS: all 14 Wastes terrain atlases match native topology, dimensions, hard alpha, palette limits and grass white-mask locations. Slope/merge appearance still needs live inspection.' -ForegroundColor Green
