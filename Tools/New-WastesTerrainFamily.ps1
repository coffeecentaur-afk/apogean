param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$CaptureRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanTileLabReferences'),
    [switch]$Promote
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Convert-HexColor([string]$hex) {
    [System.Drawing.ColorTranslator]::FromHtml($hex)
}

function Convert-ExactAtlas(
    [string]$sourceName,
    [string]$candidateRelativePath,
    [string]$productionRelativePath,
    [hashtable]$palette
) {
    $sourcePath = Join-Path $CaptureRoot $sourceName
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
        throw "Missing exported vanilla atlas: $sourcePath. Load the renderer fixture or run /apogean exportatlases first."
    }

    $source = [System.Drawing.Bitmap]::new($sourcePath)
    $output = [System.Drawing.Bitmap]::new($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $pixel = $source.GetPixel($x, $y)
                if ($pixel.A -eq 0) {
                    $output.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
                    continue
                }

                $key = '{0:X2}{1:X2}{2:X2}' -f $pixel.R, $pixel.G, $pixel.B
                if (-not $palette.ContainsKey($key)) {
                    throw "Unexpected source color #$key in $sourcePath at ($x,$y). Refusing to damage native atlas topology."
                }
                $output.SetPixel($x, $y, $palette[$key])
            }
        }

        $relativePath = if ($Promote) { $productionRelativePath } else { $candidateRelativePath }
        $outputPath = Join-Path $Root $relativePath
        $directory = Split-Path -Parent $outputPath
        if (-not (Test-Path -LiteralPath $directory)) {
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
        }
        $output.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $source.Dispose()
        $output.Dispose()
    }
}

function Palette([hashtable]$entries) {
    $result = @{}
    foreach ($key in $entries.Keys) { $result[$key] = Convert-HexColor $entries[$key] }
    $result
}

# Shared colors retain the exact renderer-approved Wastes soil and stone ramps.
# That continuity is what lets native dirt/stone merge pixels disappear at material seams.
$stoneTile = Palette @{
    '1F1C26'='#191A1B'; '433E4A'='#343331'; '58535F'='#484641'; '6A6871'='#5F5B53';
    '84818E'='#777167'; '9898A6'='#91897B'; '7B5549'='#493728'; '976B4B'='#654A30';
    'AD7F58'='#80613C'; 'C99B6D'='#A17C4B'
}
$stoneWall = Palette @{
    '141316'='#151513'; '1F1D22'='#23221E'; '29272C'='#302E29';
    '313034'='#3B3831'; '3D3C41'='#4A463D'; '46464C'='#5A5448'
}
$sandTile = Palette @{
    '4A3431'='#3A2D22'; '613F35'='#4F3A27'; '725138'='#674B2D'; '7F634E'='#80613D';
    'D4AA75'='#A5824D'; 'EEBE80'='#BF9858'; 'F7DB97'='#D3B16C'; 'FDEEA8'='#E2C986';
    '7D5C51'='#493728'; '976B4B'='#654A30'; 'AD7F58'='#80613C'
}
$sandWall = Palette @{
    '472925'='#33261C'; '5B2E23'='#48331F'; '6B3B2B'='#604526';
    '7B4B34'='#795C33'; '805D44'='#927344'
}
$iceTile = Palette @{
    '3B3657'='#242A2C'; '41406A'='#30383C'; '4A4E7D'='#3E494E'; '586CAE'='#536269';
    '6383C7'='#697A81'; '96C6DB'='#89989B'; '99B1BF'='#9AA5A4'; 'ABCACF'='#B0B8B2';
    'C4DFE0'='#C5C9BC'; 'E3F2F2'='#E0DFCC'; 'DFF4EE'='#E8E4D1'
}
$iceWall = Palette @{
    '211E29'='#1E2324'; '252431'='#282D2F'; '2A2A3A'='#31383B';
    '333850'='#3E494D'; '344061'='#4B5B60'; '475B66'='#617377'
}
$snowTile = Palette @{
    '3B2929'='#241D19'; '7B5549'='#493728'; '976B4B'='#654A30'; 'AD7F58'='#80613C';
    '5D6781'='#696B68'; '99B1BF'='#96988F'; 'ABCACF'='#AFB0A4';
    'C4DFE0'='#C8C6B8'; 'DFF4EE'='#E0DCC8'
}
$snowWall = Palette @{
    '464B61'='#555651'; '62717C'='#70736C'; '6E8187'='#85887F';
    '7E8F92'='#999B90'; '909F9C'='#B0AEA0'
}
$mudTile = Palette @{
    '241922'='#1A1713'; '382632'='#2A241A'; '3F2F37'='#372E22'; '4C3940'='#493A28';
    '5B4148'='#5C4730'; '7A5758'='#775D3D'; '433E4A'='#343331'; '58535F'='#484641';
    '6A6871'='#5F5B53'; '84818E'='#777167'
}
$mudWall = Palette @{
    '171117'='#17130F'; '241A22'='#241D16'; '282025'='#30261C';
    '31262B'='#3D3022'; '3A2C31'='#4C3B29'; '4E3B3D'='#634C32'
}

$specs = @(
    @('Vanilla-Stone-Tile.png', 'Content/Tiles/Diagnostics/WastesStoneCandidate.png', 'Content/Tiles/WastesStone.png', $stoneTile),
    @('Vanilla-Stone-Wall.png', 'Content/Walls/Diagnostics/WastesStoneWallCandidate.png', 'Content/Walls/WastesStoneWallUnsafe.png', $stoneWall),
    @('Vanilla-Sand-Tile.png', 'Content/Tiles/Diagnostics/WastesSandCandidate.png', 'Content/Tiles/WastesSand.png', $sandTile),
    @('Vanilla-Sandstone-Wall.png', 'Content/Walls/Diagnostics/WastesSandWallCandidate.png', 'Content/Walls/WastesSandWallUnsafe.png', $sandWall),
    @('Vanilla-Ice-Tile.png', 'Content/Tiles/Diagnostics/WastesIceCandidate.png', 'Content/Tiles/WastesIce.png', $iceTile),
    @('Vanilla-IceUnsafe-Wall.png', 'Content/Walls/Diagnostics/WastesIceWallCandidate.png', 'Content/Walls/WastesIceWallUnsafe.png', $iceWall),
    @('Vanilla-Snow-Tile.png', 'Content/Tiles/Diagnostics/WastesSnowCandidate.png', 'Content/Tiles/WastesSnow.png', $snowTile),
    @('Vanilla-SnowUnsafe-Wall.png', 'Content/Walls/Diagnostics/WastesSnowWallCandidate.png', 'Content/Walls/WastesSnowWallUnsafe.png', $snowWall),
    @('Vanilla-Mud-Tile.png', 'Content/Tiles/Diagnostics/WastesMudCandidate.png', 'Content/Tiles/WastesMud.png', $mudTile),
    @('Vanilla-MudUnsafe-Wall.png', 'Content/Walls/Diagnostics/WastesMudWallCandidate.png', 'Content/Walls/WastesMudWallUnsafe.png', $mudWall)
)

foreach ($spec in $specs) {
    Convert-ExactAtlas $spec[0] $spec[1] $spec[2] $spec[3]
}

$mode = if ($Promote) { 'production' } else { 'diagnostic candidate' }
Write-Host "Generated the five-material Wastes $mode family from exported vanilla atlas topology." -ForegroundColor Green
