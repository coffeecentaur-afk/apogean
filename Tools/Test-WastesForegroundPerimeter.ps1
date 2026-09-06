Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$runner = (Get-Process -Id $PID).Path
$probe = Join-Path $PSScriptRoot 'Inspect-WastesForegroundPerimeter.ps1'
$qaTempBase = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$qaDirectory = Join-Path $qaTempBase ('ApogeanPerimeterProbe-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $qaDirectory | Out-Null
try {
    foreach ($case in @('dark-control','upper-fringe','lower-fringe','hidden-rgb')) {
        $inputPath = Join-Path $qaDirectory "$case.png"
        $outputPath = Join-Path $qaDirectory "$case.json"
        $bitmap = [Drawing.Bitmap]::new(8,480,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $bitmap.SetPixel(4,450,[Drawing.Color]::FromArgb(255,55,40,25))
            if ($case -eq 'upper-fringe') { $bitmap.SetPixel(4,10,[Drawing.Color]::FromArgb(255,160,155,150)) }
            if ($case -eq 'lower-fringe') { $bitmap.SetPixel(4,450,[Drawing.Color]::FromArgb(255,160,155,150)) }
            if ($case -eq 'hidden-rgb') { $bitmap.SetPixel(2,2,[Drawing.Color]::FromArgb(0,255,255,255)) }
            $bitmap.Save($inputPath,[Drawing.Imaging.ImageFormat]::Png)
        } finally { $bitmap.Dispose() }
        $trace = & $runner -NoProfile -File $probe -AssetPath $inputPath -ReportPath $outputPath -RequireNoReviewCandidates 2>&1
        $exitCode = $LASTEXITCODE
        $expectedCount = if ($case -like '*fringe') { 1 } else { 0 }
        if (($exitCode -ne 0) -ne ($expectedCount -eq 1)) { throw "Wrong CLI verdict: $case" }
        if ($exitCode -ne 0 -and "$trace" -notmatch 'PERIMETER_REVIEW_REQUIRED') { throw "Wrong failure reason: $case" }
        $report = Get-Content -Raw -LiteralPath $outputPath | ConvertFrom-Json
        if ($report.paleNeutralBoundaryCandidates -ne $expectedCount) { throw "Wrong opaque-edge count: $case" }
        $expectedBelow = if ($case -eq 'lower-fringe') { 1 } else { 0 }
        if ($report.candidatesBelowOld440RowProbe -ne $expectedBelow) { throw "Missed lower perimeter: $case" }
        $expectedHidden = if ($case -eq 'hidden-rgb') { 1 } else { 0 }
        if ($report.transparentWithNonzeroRgb -ne $expectedHidden) { throw "Conflated hidden RGB with opaque edge: $case" }
        Write-Output "PASS actual perimeter CLI: $case (diagnostic control, not art approval)"
    }
} finally {
    $resolved = [IO.Path]::GetFullPath($qaDirectory)
    $allowed = $qaTempBase.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar + 'ApogeanPerimeterProbe-'
    if (-not $resolved.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)) { throw 'Refusing unexpected temporary cleanup target' }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
