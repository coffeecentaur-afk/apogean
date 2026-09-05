param(
    [string]$CaptureDirectory = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures'),
    [string]$EvidenceName = 'Apogean-ForestSpray',
    [ValidateRange(1,1440)][int]$MaximumAgeMinutes = 15,
    [switch]$Replay
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$result = Get-Content -LiteralPath (Join-Path $CaptureDirectory "$EvidenceName.json") -Raw | ConvertFrom-Json
if (-not $Replay -and ([datetime]::UtcNow - [datetime]$result.utc).TotalMinutes -gt $MaximumAgeMinutes) { throw 'FAIL: stale spray evidence; rerun forest-restoration-spray.' }
# Recompute from actual draw samples, not just the producer's pass flag. Missing
# draws during a partial fade are failures, not silently excluded observations.
$incoming = $outgoing = $mismatch = $missing = 0
foreach ($row in (Import-Csv -LiteralPath (Join-Path $CaptureDirectory "$EvidenceName.csv"))) {
    $engine = [double]::Parse($row.engineOpacity, [cultureinfo]::InvariantCulture)
    $draw = [double]::Parse($row.drawOpacity, [cultureinfo]::InvariantCulture)
    if ([double]::IsNaN($engine) -or [double]::IsInfinity($engine) -or [double]::IsNaN($draw) -or [double]::IsInfinity($draw)) {
        throw 'FAIL: non-finite opacity in the live trace.'
    }
    if ($engine -gt .001 -and $engine -lt .999) {
        if ($row.restored -eq 'True') { $outgoing++ } else { $incoming++ }
        if ($draw -lt 0) { $missing++ }
        elseif ([math]::Abs($draw - $engine) -gt .001) { $mismatch++ }
    }
}
if (-not $result.pass -or $incoming -eq 0 -or $outgoing -eq 0 -or $mismatch -ne 0 -or $missing -ne 0 -or
    -not $result.sawWaste -or -not $result.sawGreen -or $result.spawned -le 0 -or $result.tick -lt 1400 -or
    $result.forcedBackground -or $result.projectile -ne 'Terraria.ID.ProjectileID.PureSpray') {
    throw "FAIL: spray/fade check: incoming=$incoming outgoing=$outgoing mismatch=$mismatch missing=$missing; $($result | ConvertTo-Json -Compress)"
}
$scope = if ($Replay) { 'ARCHIVED TRACE REPLAY (not a new runtime test)' } else { 'fresh live evidence' }
Write-Host "PASS ($scope): native PureSpray restored the scene; $incoming incoming and $outgoing outgoing fade samples match, with no missing draws. This is not manual weapon-input or multiplayer proof."
