Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$legacyPath = Join-Path $root 'Tools/New-NativeWorldTiles.ps1'
$legacy = Get-Content -Raw -LiteralPath $legacyPath
$failures = [Collections.Generic.List[string]]::new()

foreach ($forbiddenCall in @(
    "New-CorporateSheet 'Helix'",
    "New-CorporateWallSheet 'Helix'",
    "New-CorporateWallSheet 'Kessler'"
)) {
    if ($legacy -match [regex]::Escape($forbiddenCall)) { $failures.Add("legacy generator still invokes $forbiddenCall") }
}

$kesslerCalls = [regex]::Matches($legacy, "(?m)^\s*New-CorporateSheet\s+'Kessler'[^\r\n]*$")
foreach ($call in $kesslerCalls) {
    if ($call.Value -notmatch "'PrewarConcrete'\s*$") { $failures.Add("legacy generator owns an unexpected Kessler output: $($call.Value.Trim())") }
}
if ($legacy -match '(?m)^\s*New-NaturalSheet\s+') { $failures.Add('legacy generator still invokes natural terrain generation') }
if ($legacy -match '(?m)^\s*New-WallSheet\s+') { $failures.Add('legacy generator still invokes Wastes/Maw wall generation') }

foreach ($focusedGenerator in @(
    'Tools/New-WastesTerrainFamily.ps1',
    'Tools/New-MawTerrainFamily.ps1',
    'Tools/New-KesslerConstructionSet.ps1',
    'Tools/New-HelixConstructionSet.ps1'
)) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $focusedGenerator))) { $failures.Add("missing focused owner $focusedGenerator") }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'PASS: the legacy broad generator cannot invoke focused Kessler, Helix, Wastes, or Maw asset families.' -ForegroundColor Green
