param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
# HISTORICAL mutation suite for the retired opacity policy. Current gate:
# Test-WastesHeightMutations.ps1. This script requires its historical source.
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$sourcePath = Join-Path $ProjectRoot 'Common/Backgrounds/WastesCameraProjection.cs'
$original = Get-Content -Raw -LiteralPath $sourcePath
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('ApogeanSpaceControls-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path (Join-Path $scratch 'Common/Backgrounds') -Force | Out-Null
try {
    $variants = @(
        @('unchanged', '', '', 0),
        @('old-always-visible', 'float land = 1 - t * t * (3 - 2 * t);', 'float land = 1;', 1),
        @('always-hidden', 'float land = 1 - t * t * (3 - 2 * t);', 'float land = 0;', 1),
        @('abrupt-cut', 'float land = 1 - t * t * (3 - 2 * t);', 'float land = ascent < 1 ? 1 : 0;', 1),
        @('ignores-player', 'Math.Min(cameraCenterY, playerCenterY)', 'cameraCenterY', 1),
        @('ignores-camera', 'Math.Min(cameraCenterY, playerCenterY)', 'playerCenterY', 1)
    )
    foreach ($variant in $variants) {
        $mutated = $original
        if ($variant[1]) {
            if (-not $original.Contains($variant[1])) { throw "Missing mutation seam: $($variant[0])" }
            $mutated = $original.Replace($variant[1],$variant[2])
        }
        [IO.File]::WriteAllText((Join-Path $scratch 'Common/Backgrounds/WastesCameraProjection.cs'),$mutated)
        $output = & pwsh -NoProfile -File (Join-Path $ProjectRoot 'Tools/Test-WastesSpaceHandoff.ps1') -ProjectRoot $scratch 2>&1
        $failed = $LASTEXITCODE -ne 0
        if ($failed -ne [bool]$variant[3]) { throw "Wrong CLI verdict for $($variant[0]): $output" }
        if ($failed -and "$output" -notmatch 'FAIL:') { throw "Control failed for a non-assertion reason: $output" }
        Write-Host "PASS validator control: $($variant[0])"
    }
}
finally {
    $resolved = [IO.Path]::GetFullPath($scratch)
    $allowed = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\ApogeanSpaceControls-'
    if (-not $resolved.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected scratch cleanup target' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
