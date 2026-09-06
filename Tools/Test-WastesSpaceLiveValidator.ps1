param([Parameter(Mandatory)][string]$LogPath,[Parameter(Mandatory)][string]$Viewport,
    [string]$ProjectRoot=(Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$original = Get-Content -Raw -LiteralPath $LogPath
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('ApogeanSpaceLogControls-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
try {
    $variants=@(
        @('unchanged',$original,0,''),
        @('visible-space',$original.Replace('factor=0.00000; submittedAlpha=0;','factor=1.00000; submittedAlpha=255;'),1,'VISIBLE_SPACE_SAMPLE'),
        @('missing-render',[regex]::Replace($original,'(?m)^.*WASTES LAND SAMPLE:.*\r?\n',''),1,'MISSING_DRAW_SAMPLE'),
        @('missing-completion',[regex]::Replace($original,'(?m)^.*WASTES LAND RESULT: case=space-descent;.*\r?\n',''),1,'MISSING_RESULT'),
        @('no-intermediates',[regex]::Replace($original,'partialFarChecks=\d+;','partialFarChecks=0;'),1,'MISSING_GRADUAL_FADE'),
        @('no-ground',[regex]::Replace($original,'groundChecks=\d+;','groundChecks=0;'),1,'MISSING_GROUND_PRESENCE')
    )
    foreach ($variant in $variants) {
        if ($variant[2] -eq 1 -and $variant[1] -eq $original) { throw "Mutation did not change input: $($variant[0])" }
        $path=Join-Path $scratch "$($variant[0]).log"
        [IO.File]::WriteAllText($path,$variant[1])
        $output=& pwsh -NoProfile -File (Join-Path $ProjectRoot 'Tools/Test-WastesSpaceLive.ps1') -LogPath $path -Viewport $Viewport 2>&1
        if (($LASTEXITCODE -ne 0) -ne [bool]$variant[2]) { throw "Wrong live-validator verdict for $($variant[0]): $output" }
        if ($variant[2] -eq 1 -and "$output" -notmatch $variant[3]) { throw "Control failed for the wrong reason: $($variant[0]): $output" }
        Write-Host "PASS live-validator control: $($variant[0])"
    }
}
finally {
    $resolved=[IO.Path]::GetFullPath($scratch)
    $allowed=[IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')+'\ApogeanSpaceLogControls-'
    if (-not $resolved.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected cleanup target' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
