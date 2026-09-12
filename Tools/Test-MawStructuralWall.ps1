param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Atlas = ''
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Narrow source contract, not a C# interpreter. The packed QA bank has its own
# map/runtime checks. Here only the production (packed-preview OFF) path is tested.
# WastesDirtWallUnsafe's alpha was checked against installed Vanilla-DirtUnsafe-Wall.
# Different native wall styles can have different edge silhouettes; ExampleWall is
# not a universal per-pixel alpha reference for every natural-wall material.
function Resolve-StructuralWall([string]$Source, [string]$Wrapper) {
    if ($Source -match 'override\s+string\s+Texture\s*=>\s*"apogean/(Content/Walls/[A-Za-z0-9_]+)"\s*;') {
        return $Matches[1] + '.png'
    }
    if ($Source -match 'override\s+string\s+Texture\s*=>\s*MawPackedPreview\.Texture\(\s*"soil"\s*,\s*true\s*,\s*"apogean/(Content/Walls/[A-Za-z0-9_]+)"\s*\)\s*;') {
        $fallback = $Matches[1]
        $normalized = $Wrapper -replace '\s',''
        $expected = 'internalstaticstringTexture(stringkey,boolwall,stringfallback)=>Enabled&&key!=null?(wall?MawTerrainStudies.WallTexture(key):"apogean/Content/Diagnostics/Materials/"+key+"/Tile"):fallback;'
        if (-not $normalized.Contains($expected)) {
            throw 'WALL_RESOLVER: preview fallback implementation changed; review its production branch.'
        }
        return $fallback + '.png'
    }
    if ($Source -match 'override\s+string\s+Texture') {
        throw 'WALL_RESOLVER: unsupported Texture expression; do not assume a path.'
    }
    return 'Content/Walls/MawWallUnsafe.png'
}
$source = Get-Content -Raw -LiteralPath (Join-Path $Root 'Content/Walls/MawWallUnsafe.cs')
$wrapper = Get-Content -Raw -LiteralPath (Join-Path $Root 'Content/Diagnostics/MawPackedPreview.cs')
# Regression controls run against this same resolver, including the wrapper body.
$literal = 'public override string Texture => "apogean/Content/Walls/MawDirtWallUnsafe";'
$packed = 'public override string Texture => MawPackedPreview.Texture("soil",true,"apogean/Content/Walls/MawDirtWallUnsafe");'
foreach ($inputSource in @($literal,$packed)) {
    if ((Resolve-StructuralWall $inputSource $wrapper) -ne 'Content/Walls/MawDirtWallUnsafe.png') { throw 'WALL_RESOLVER: valid path rejected' }
}
if ((Resolve-StructuralWall '' $wrapper) -ne 'Content/Walls/MawWallUnsafe.png') { throw 'WALL_RESOLVER: convention changed' }
foreach ($bad in @(
    @($packed.Replace('"soil"','"unknown"'),$wrapper),
    @($packed.Replace(',true,',',false,'),$wrapper),
    @($packed.Replace('MawPackedPreview.Texture','Unknown.Texture'),$wrapper),
    @($packed.Replace('Content/Walls/','../Content/Walls/'),$wrapper),
    @($packed,($wrapper.Replace(': fallback;',': "wrong";')))
)) {
    $rejected=$false
    try { $null=Resolve-StructuralWall $bad[0] $bad[1] } catch {
        if ($_.Exception.Message -notlike 'WALL_RESOLVER:*') { throw }; $rejected=$true
    }
    if (-not $rejected) { throw 'WALL_RESOLVER: malformed control accepted' }
}
Write-Host 'Structural-wall resolver: three paths / five rejected defects. Packed atlas acceptance remains separate.'
$resolved = Resolve-StructuralWall $source $wrapper
if (-not $Atlas) { $Atlas = Join-Path $Root $resolved }
$atlasPath = (Resolve-Path -LiteralPath $Atlas).Path
Write-Host "Checking structural wall atlas: $atlasPath"
& pwsh -NoProfile -File (Join-Path $Root 'Tools/Test-TModLoaderAtlas.ps1') `
    -Atlas $atlasPath -ReferenceAtlas (Join-Path $Root 'Content/Walls/WastesDirtWallUnsafe.png') `
    -ExpectedWidth 468 -ExpectedHeight 180 -MaximumOpaqueColors 16
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host 'MAW STRUCTURAL WALL: PASS (production fallback native dirt-wall topology; packed QA/native scene review is separate).'
