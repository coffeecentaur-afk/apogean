param(
    [Parameter(Mandatory)][string]$LogPath,
    [string]$ExportDirectory = '',
    [ValidateSet('None','MissingSample','MissingRelease','WrongAsset','DrawFailure')][string]$Fault = 'None'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$lines = @(Get-Content -LiteralPath $LogPath | Where-Object { $_ -match 'WASTES (SCALE |V1 CAMERA:|MODULAR RESULT:)' })
switch ($Fault) {
    'MissingSample' { $lines = @($lines | Where-Object { $_ -notmatch 'SCALE SAMPLE: case=scale-depot; side=1;' }) }
    'MissingRelease' { $lines = @($lines | Where-Object { $_ -notmatch 'SCALE RESULT: case=scale-shell;' }) }
    'WrongAsset' { $lines = @($lines | ForEach-Object { $_ -replace 'asset=Checkpoint;', 'asset=Wrong;' }) }
    'DrawFailure' { $lines = @($lines | ForEach-Object { $_ -replace 'failed=False;', 'failed=True;' }) }
}
$cases = @{'scale-shell'='BrokenShell';'scale-depot'='MotorDepot';'scale-checkpoint'='Checkpoint'}
foreach ($case in $cases.Keys) {
    $samples = @($lines | Where-Object { $_ -match "WASTES SCALE SAMPLE: case=$case;" })
    if ($samples.Count -ne 2) { throw "MISSING_SAMPLE: $case" }
    foreach ($side in 0,1) {
        $name = if ($side -eq 0) {'Station'} else {$cases[$case]}
        $sample = @($samples | Where-Object { $_ -match "side=$side; asset=$name;" })
        if ($sample.Count -ne 1) { throw "WRONG_ASSET: $case side=$side" }
        if ($sample[0] -notmatch 'viewport=2560x1369; texture=512x460;' -or
            $sample[0] -notmatch 'failed=False; scope=ground-scale-only; production=False') {
            throw "DRAW_FAILURE: $case side=$side"
        }
    }
    if (@($lines | Where-Object { $_ -match "WASTES SCALE RESULT: case=$case; samples=[1-9][0-9]*; failures=0; artApproval=False; flightCoverage=False" }).Count -ne 1) {
        throw "MISSING_RELEASE: $case"
    }
}
if (@($lines | Where-Object { $_ -match 'WASTES MODULAR RESULT: case=ground; matrixChecks=[1-9][0-9]*; frameChecks=[1-9][0-9]*; failures=0;' }).Count -ne 1) {
    throw 'BASELINE_RETURN_UNPROVEN'
}
if ($ExportDirectory) {
    if ($Fault -ne 'None') { throw 'Do not export fault-injected evidence.' }
    $destination = (Resolve-Path -LiteralPath $ExportDirectory).Path
    $output = Join-Path $destination 'scale-telemetry.log'
    if (Test-Path -LiteralPath $output) { throw 'Refusing to overwrite captured evidence.' }
    [IO.File]::WriteAllLines($output, $lines)
}
Write-Output 'PASS: three acknowledged native scale pairs, release results, and restored baseline. Not art, pixel-color, flight, performance or production acceptance.'
