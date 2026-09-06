param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$fixtureRoot = Join-Path ([IO.Path]::GetTempPath()) ('ApogeanReplacementProbe-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
$reference = Join-Path $fixtureRoot 'reference.png'
$probe = Join-Path $PSScriptRoot 'Test-BackgroundReplacement.ps1'
$runner = (Get-Process -Id $PID).Path
try {
    # Tiny diagnostic fixtures, never game art. Exercise the real CLI's exit code.
    $bitmap = [Drawing.Bitmap]::new(4,4)
    try {
        $bitmap.SetPixel(1,1,[Drawing.Color]::FromArgb(255,60,40,20))
        $bitmap.SetPixel(2,2,[Drawing.Color]::FromArgb(255,50,35,20))
        $bitmap.Save($reference,[Drawing.Imaging.ImageFormat]::Png)
    } finally { $bitmap.Dispose() }
    $cases = @('identical','dimensions','opaque','soft','outside','inside','eroded')
    foreach ($case in $cases) {
        $candidate = Join-Path $fixtureRoot ($case + '.png')
        $bitmap = if ($case -eq 'dimensions') { [Drawing.Bitmap]::new(5,4) } else { [Drawing.Bitmap]::new($reference) }
        try {
            switch ($case) {
                'opaque' { for ($y=0;$y -lt 4;$y++) { for ($x=0;$x -lt 4;$x++) { $bitmap.SetPixel($x,$y,[Drawing.Color]::White) } } }
                'soft' { $bitmap.SetPixel(1,1,[Drawing.Color]::FromArgb(128,60,40,20)) }
                'outside' { $bitmap.SetPixel(2,2,[Drawing.Color]::FromArgb(255,61,40,20)) }
                'inside' { $bitmap.SetPixel(1,1,[Drawing.Color]::FromArgb(255,61,40,20)) }
                'eroded' { $bitmap.SetPixel(1,1,[Drawing.Color]::Transparent) }
            }
            $bitmap.Save($candidate,[Drawing.Imaging.ImageFormat]::Png)
        } finally { $bitmap.Dispose() }
        $report = Join-Path $fixtureRoot ($case + '.json')
        & $runner -NoProfile -File $probe -ReferencePath $reference -CandidatePath $candidate -AllowedRectangles '1,1,1,1' -ReportPath $report -PreserveAlphaMask | Out-Null
        $code = $LASTEXITCODE
        $result = Get-Content -Raw -LiteralPath $report | ConvertFrom-Json
        $expected = switch ($case) {
            'dimensions' { 'DIMENSIONS_CHANGED' }
            'opaque' { 'NO_TRANSPARENT_PIXELS' }
            'soft' { 'SOFT_ALPHA' }
            'outside' { 'PIXELS_CHANGED_OUTSIDE_APPROVED_REGIONS' }
            'eroded' { 'ALPHA_MASK_CHANGED' }
            default { $null }
        }
        if ($expected) {
            if ($code -eq 0 -or $result.pass -or $result.failures -notcontains $expected) { throw "FALSE_PASS: $case" }
        } elseif ($code -ne 0 -or -not $result.pass) { throw "FALSE_FAILURE: $case" }
        Write-Output "PASS actual replacement-gate CLI: $case"
    }
} finally {
    $resolved = (Resolve-Path -LiteralPath $fixtureRoot).Path
    $expectedRoot = [IO.Path]::GetFullPath($fixtureRoot)
    if ($resolved -ne $expectedRoot -or -not ([IO.Path]::GetFileName($resolved)).StartsWith('ApogeanReplacementProbe-')) {
        throw 'Refuse cleanup outside the verified diagnostic fixture directory'
    }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
Write-Output 'PASS: two positive and five negative export-gate controls. No production assets changed.'
