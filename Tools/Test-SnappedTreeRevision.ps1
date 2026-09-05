param(
    [string]$CandidateDirectory = (Join-Path $PSScriptRoot '../Art/Candidates/WastesSnappedA-v2'),
    [string]$BaselineDirectory = (Join-Path $PSScriptRoot '../Art/Candidates/WastesSnappedA-v1')
)

# A focused art-export regression, not a live tree test or appearance approval.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$failures = [Collections.Generic.List[string]]::new()
$allowed = @('#211916', '#35261f', '#523827', '#735033', '#967041') | ForEach-Object { ([Drawing.ColorTranslator]::FromHtml($_)).ToArgb() }

foreach ($name in @('DeadForestTree.png', 'DeadForestTree_Tops.png', 'DeadForestTree_Branches.png')) {
    $candidate = [Drawing.Bitmap]::new((Resolve-Path (Join-Path $CandidateDirectory $name)).Path)
    $baseline = [Drawing.Bitmap]::new((Resolve-Path (Join-Path $BaselineDirectory $name)).Path)
    try {
        if ($candidate.Size -ne $baseline.Size) { $failures.Add("$name changed native dimensions"); continue }
        $badColor = 0; $alphaChanges = 0
        for ($y = 0; $y -lt $candidate.Height; $y++) {
            for ($x = 0; $x -lt $candidate.Width; $x++) {
                $p = $candidate.GetPixel($x, $y)
                if ($p.A -gt 0 -and ($p.A -ne 255 -or $p.ToArgb() -notin $allowed)) { $badColor++ }
                if ($p.A -ne $baseline.GetPixel($x, $y).A) { $alphaChanges++ }
            }
        }
        if ($badColor) { $failures.Add("$name has $badColor pixels outside the five opaque muted browns (or soft alpha)") }
        if ($name -ne 'DeadForestTree_Branches.png' -and $alphaChanges) { $failures.Add("$name changed $alphaChanges silhouette/socket pixels") }
        if ($name -ne 'DeadForestTree_Branches.png') { continue }
        for ($variant = 0; $variant -lt 3; $variant++) {
            $previousTop = -1; $previousBottom = -1
            # Ignore only the four pixels at the actual broken end. Intact bark
            # must be a solid connected strip with gradual one-pixel contour steps.
            for ($x = 20; $x -le 39; $x++) {
                $rows = @(0..39 | Where-Object { $candidate.GetPixel($x, $variant * 42 + $_).A -eq 255 })
                if ($rows.Count -lt 3) { $failures.Add("branch $variant intact column $x is missing or pinched"); continue }
                $top = $rows[0]; $bottom = $rows[-1]
                if ($rows.Count -ne $bottom - $top + 1) { $failures.Add("branch $variant column $x contains a false split") }
                if ($previousTop -ge 0 -and (($top - $previousTop) -notin @(0, 1) -or ($bottom - $previousBottom) -notin @(0, 1))) {
                    $failures.Add("branch $variant has a jagged intact contour at column $x")
                }
                $previousTop = $top; $previousBottom = $bottom
            }
            for ($x = 0; $x -lt 40; $x++) {
                for ($y = 0; $y -lt 40; $y++) {
                    $right = $candidate.GetPixel(42 + 39 - $x, $variant * 42 + $y)
                    $left = if ($y -ge 6) { $candidate.GetPixel($x, $variant * 42 + $y - 6) } else { [Drawing.Color]::Transparent }
                    if ($left.A -ne $right.A -or ($left.A -gt 0 -and $left.ToArgb() -ne $right.ToArgb())) {
                        $failures.Add("branch $variant mirror/pivot mismatch at $x,$y")
                    }
                }
            }
            # Left pivot (40,24); right pivot (0,30). Both remain seven pixels deep.
            foreach ($y in 21..27) {
                if ($candidate.GetPixel(39, $variant * 42 + $y).A -ne 255 -or $candidate.GetPixel(42, $variant * 42 + $y + 6).A -ne 255) {
                    $failures.Add("branch $variant loses its native attachment band")
                }
            }
        }
    }
    finally { $candidate.Dispose(); $baseline.Dispose() }
}

if ($failures.Count) {
    $failures | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'PASS: muted five-color wood, unchanged trunk/top silhouettes, solid smooth branch shafts, six-pixel mirrored pivots and intact sockets. Appearance and live behavior still require review.' -ForegroundColor Green
