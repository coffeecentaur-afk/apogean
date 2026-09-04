Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$referenceRoot = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences'
$tileReferencePath = Join-Path $referenceRoot 'Vanilla-GrayBrick-Tile.png'
$wallReferencePath = Join-Path $referenceRoot 'Vanilla-GrayBrick-Wall.png'
$failures = [Collections.Generic.List[string]]::new()

function Test-NativeTopology([string]$relative, [string]$referencePath, [int]$maximumColors) {
    if (-not (Test-Path -LiteralPath $referencePath)) { $failures.Add("missing native reference $referencePath"); return }
    $path = Join-Path $root $relative
    if (-not (Test-Path -LiteralPath $path)) { $failures.Add("missing $relative"); return }
    $bitmap = [Drawing.Bitmap]::new($path)
    $reference = [Drawing.Bitmap]::new($referencePath)
    try {
        if ($bitmap.Size -ne $reference.Size) { $failures.Add("$relative dimensions differ from native reference"); return }
        $alphaMismatch = 0
        $white = 0
        $soft = 0
        $colors = [Collections.Generic.HashSet[int]]::new()
        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                $pixel = $bitmap.GetPixel($x, $y)
                $source = $reference.GetPixel($x, $y)
                if (($pixel.A -gt 0) -ne ($source.A -gt 0)) { $alphaMismatch++ }
                if ($pixel.A -gt 0 -and $pixel.A -lt 255) { $soft++ }
                if ($pixel.A -eq 255) {
                    [void]$colors.Add($pixel.ToArgb())
                    if ($pixel.R -eq 255 -and $pixel.G -eq 255 -and $pixel.B -eq 255) { $white++ }
                }
            }
        }
        if ($alphaMismatch -gt 0) { $failures.Add("$relative has $alphaMismatch native-topology mismatches") }
        if ($soft -gt 0) { $failures.Add("$relative has $soft soft-alpha pixels") }
        if ($white -gt 0) { $failures.Add("$relative has $white opaque-white pixels") }
        if ($colors.Count -gt $maximumColors) { $failures.Add("$relative uses $($colors.Count) colors; maximum is $maximumColors") }
    }
    finally { $bitmap.Dispose(); $reference.Dispose() }
}

foreach ($name in @('HelixBlock', 'HelixTrim', 'HelixFloor', 'HelixGlass', 'HelixBeam', 'HelixContainmentPanel', 'HelixRuinBlock', 'MawResearchBlock')) {
    Test-NativeTopology "Content/Tiles/$name.png" $tileReferencePath 7
}
foreach ($name in @('HelixLaboratoryWall', 'HelixObservationWall')) {
    Test-NativeTopology "Content/Walls/$name.png" $wallReferencePath 7
}

$furnitureValidator = Join-Path ([Environment]::GetFolderPath('UserProfile')) '.codex/skills/tmodloader-structure-authoring/scripts/Test-FurnitureSheet.ps1'
if (-not (Test-Path -LiteralPath $furnitureValidator)) {
    $failures.Add("missing installed furniture validator $furnitureValidator")
}
else {
    & $furnitureValidator -Sheet (Join-Path $root 'Content/Tiles/HelixWorkbench.png') -ObjectWidthTiles 2 -ObjectHeightTiles 1 -CoordinateHeights 18
    & $furnitureValidator -Sheet (Join-Path $root 'Content/Tiles/HelixSymbioteTank.png') -ObjectWidthTiles 3 -ObjectHeightTiles 4 -AnimationFrames 4
}

$focusedGenerator = Get-Content -Raw -LiteralPath (Join-Path $root 'Tools/New-HelixConstructionSet.ps1')
$broadGenerator = Get-Content -Raw -LiteralPath (Join-Path $root 'Tools/New-NativeWorldTiles.ps1')
foreach ($contract in @('Vanilla-GrayBrick-Tile.png', 'Vanilla-GrayBrick-Wall.png', 'New-SymbioteTank')) {
    if ($focusedGenerator -notmatch [regex]::Escape($contract)) { $failures.Add("focused Helix generator missing $contract") }
}
foreach ($forbidden in @("New-CorporateSheet 'Helix'", "New-CorporateWallSheet 'Helix")) {
    if ($broadGenerator -match [regex]::Escape($forbidden)) { $failures.Add("broad generator can overwrite focused Helix assets via $forbidden") }
}
if (-not (Test-Path -LiteralPath (Join-Path $root 'Content/Diagnostics/HelixConstructionGallery.cs'))) {
    $failures.Add('missing dedicated Helix live construction gallery')
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'PASS: Helix structural tiles and walls preserve native topology, use bounded palettes, have one generator owner, and expose a dedicated live fixture.' -ForegroundColor Green
