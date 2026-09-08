param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Atlas = ''
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# This narrow contract resolves this class's literal Texture override, not arbitrary
# C#. Without an override it follows ModWall's conventional class asset path.
# WastesDirtWallUnsafe's alpha was checked against installed Vanilla-DirtUnsafe-Wall.
# Different native wall styles can have different edge silhouettes; ExampleWall is
# not a universal per-pixel alpha reference for every natural-wall material.
$source = Get-Content -Raw -LiteralPath (Join-Path $Root 'Content/Walls/MawWallUnsafe.cs')
if (-not $Atlas) {
    if ($source -match 'override\s+string\s+Texture\s*=>\s*"apogean/([^"\r\n]+)"\s*;') {
        $Atlas = Join-Path $Root ($Matches[1] + '.png')
    } elseif ($source -match 'override\s+string\s+Texture') {
        throw 'Texture resolution changed: update this narrow contract rather than assuming a path.'
    } else {
        $Atlas = Join-Path $Root 'Content/Walls/MawWallUnsafe.png'
    }
}
$atlasPath = (Resolve-Path -LiteralPath $Atlas).Path
Write-Host "Checking structural wall atlas: $atlasPath"
& pwsh -NoProfile -File (Join-Path $Root 'Tools/Test-TModLoaderAtlas.ps1') `
    -Atlas $atlasPath -ReferenceAtlas (Join-Path $Root 'Content/Walls/WastesDirtWallUnsafe.png') `
    -ExpectedWidth 468 -ExpectedHeight 180 -MaximumOpaqueColors 16
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host 'MAW STRUCTURAL WALL: PASS (native dirt-wall topology; native scene review is separate).'
