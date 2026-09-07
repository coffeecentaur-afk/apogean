param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('ApogeanHeightMutations-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path (Join-Path $scratch 'Common/Backgrounds'),(Join-Path $scratch 'Content/Backgrounds') | Out-Null
$files = @('Common/Backgrounds/WastesCameraProjection.cs','Common/Backgrounds/WastesModularLayout.cs','Common/Backgrounds/WastesParallaxContract.cs','Common/Backgrounds/WastesGroundProfile.cs','Content/Backgrounds/WastesLandscapeV1Renderer.cs')
$mutations = @(
    @('no ceiling','Common/Backgrounds/WastesCameraProjection.cs','Math.Max(center, LockCameraCenterY(surfaceTiles, layer))','center','Test-WastesHeightLock.ps1'),
    @('screen-following ceiling','Common/Backgrounds/WastesCameraProjection.cs','(cappedCenter - center) * gameZoom','0f','Test-WastesHeightLock.ps1'),
    @('ignores zoom','Common/Backgrounds/WastesCameraProjection.cs','(cappedCenter - center) * gameZoom','(cappedCenter - center)','Test-WastesHeightLock.ps1'),
    @('height fade returns','Common/Backgrounds/WastesCameraProjection.cs','return 1f;','return .5f;','Test-WastesHeightLock.ps1'),
    @('wrong section location','Common/Backgrounds/WastesModularLayout.cs','CloseWidth * .5','CloseWidth * .9','Test-WastesFixedSections.ps1'),
    @('renderer bypass','Content/Backgrounds/WastesLandscapeV1Renderer.cs','zoom, cell, SavedGroundAt)','zoom, 0, SavedGroundAt)','Test-WastesFixedSections.ps1')
)
foreach ($m in $mutations) {
    foreach ($f in $files) { Copy-Item -LiteralPath (Join-Path $ProjectRoot $f) -Destination (Join-Path $scratch $f) }
    $target = Join-Path $scratch $m[1]
    $original = Get-Content -Raw -LiteralPath $target
    if (-not $original.Contains($m[2])) { throw "Mutation anchor missing: $($m[0])" }
    # Mechanical mutation is confined to a newly created disposable source mirror.
    [IO.File]::WriteAllText($target,$original.Replace($m[2],$m[3]))
    $output = & pwsh -NoProfile -File (Join-Path $PSScriptRoot $m[4]) -ProjectRoot $scratch 2>&1
    if ($LASTEXITCODE -eq 0 -or ($output -join "`n") -notmatch 'FAIL:') { throw "Mutation not meaningfully rejected: $($m[0]); $output" }
    Write-Host "PASS: rejected $($m[0])"
}
Write-Host "PASS: six incorrect camera/anchor mutations rejected. Temporary evidence retained at $scratch"
