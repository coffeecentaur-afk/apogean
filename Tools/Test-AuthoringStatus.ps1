Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$path = Join-Path $PSScriptRoot 'AuthoringStatus.json'
$data = Get-Content -Raw -LiteralPath $path | ConvertFrom-Json
$failures = [Collections.Generic.List[string]]::new()
$allowedStates = [Collections.Generic.HashSet[string]]::new([string[]]$data.states, [StringComparer]::OrdinalIgnoreCase)
$ids = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

foreach ($family in $data.families) {
    if (-not $ids.Add([string]$family.id)) { $failures.Add("duplicate family id '$($family.id)'") }
    if (-not $allowedStates.Contains([string]$family.status)) { $failures.Add("$($family.id) has invalid status '$($family.status)'") }
    foreach ($field in @('static', 'build', 'liveFixture', 'production')) {
        if ([string]$family.$field -notin @('pending', 'pass', 'fail')) { $failures.Add("$($family.id) has invalid $field '$($family.$field)'") }
    }
    if ([string]$family.userReview -notin @('pending', 'accepted', 'rejected')) { $failures.Add("$($family.id) has invalid userReview '$($family.userReview)'") }
    if ($family.status -in @('fixture-pass', 'integrated', 'polished')) {
        foreach ($field in @('static', 'build', 'liveFixture')) {
            if ($family.$field -ne 'pass') { $failures.Add("$($family.id) cannot be $($family.status) while $field is $($family.$field)") }
        }
    }
    if ($family.status -in @('integrated', 'polished') -and $family.production -ne 'pass') {
        $failures.Add("$($family.id) cannot be $($family.status) without production evidence")
    }
    if ($family.status -eq 'polished' -and $family.userReview -ne 'accepted') {
        $failures.Add("$($family.id) cannot be polished without accepted user review")
    }
    if ($family.status -eq 'rejected' -and $family.userReview -ne 'rejected' -and $family.liveFixture -ne 'fail') {
        $failures.Add("$($family.id) is rejected without a rejected review or failed fixture")
    }
    if ($family.status -ne 'polished' -and @($family.blockers).Count -eq 0) {
        $failures.Add("$($family.id) is not polished but has no recorded blocker")
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host "PASS: $(@($data.families).Count) authoring families have evidence-consistent Wayfinder states." -ForegroundColor Green
