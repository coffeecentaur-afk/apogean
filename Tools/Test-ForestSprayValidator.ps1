param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Synthetic validator tests only. These files never count as live game evidence.
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('Apogean-SprayValidator-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$runner = Join-Path $ProjectRoot 'Tools/Test-ForestSprayLive.ps1'
$checks = 0
foreach ($case in 'control','stale-outgoing','missing-draw','no-outgoing','false-report','stale-file','forced-preview','non-finite') {
    $rows = @(
        [pscustomobject]@{tick=12;living=0;wastes=169;restored='False';engineOpacity='.5';drawOpacity='.5'},
        [pscustomobject]@{tick=655;living=110;wastes=59;restored='True';engineOpacity='.5';drawOpacity='.5'}
    )
    $report = @{pass=$true;tick=1400;spawned=47;sawWaste=$true;sawGreen=$true;forcedBackground=$false;projectile='Terraria.ID.ProjectileID.PureSpray';utc=[datetime]::UtcNow}
    switch ($case) {
        'stale-outgoing' { $rows[1].drawOpacity='1' }
        'missing-draw' { $rows[1].drawOpacity='-1' }
        'no-outgoing' { $rows=@($rows[0]) }
        'false-report' { $report.pass=$false }
        'stale-file' { $report.utc=[datetime]::UtcNow.AddHours(-1) }
        'forced-preview' { $report.forcedBackground=$true }
        'non-finite' { $rows[1].drawOpacity='NaN' }
    }
    $report | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $scratch "$case.json")
    $rows | Export-Csv -LiteralPath (Join-Path $scratch "$case.csv") -NoTypeInformation
    $output = & pwsh -NoProfile -File $runner -CaptureDirectory $scratch -EvidenceName $case 2>&1
    $passed = $LASTEXITCODE -eq 0
    if ($passed -ne ($case -eq 'control')) { throw "FAIL validator case $case : $output" }
    $checks++
}
Write-Host "PASS: $checks validator checks through the real CLI, including stale outgoing opacity with a falsely green summary. Synthetic fixtures retained in $scratch; NOT live-render proof."
