Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanTelemetryExport-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
# Synthetic exporter-only input. Never publish this as engine evidence.
$expected=@(
    'WASTES V1 PROJECTION SAMPLE: case=diagonal-left; viewport=2560x1369; failed=False',
    'WASTES V1 PROJECTION: case=diagonal-left; failures=0;',
    'WASTES V1 CAMERA: case=ground;',
    'WASTES MODULAR RESULT: case=ground; failures=0;',
    'WASTES RUN MOTION SAMPLE: case=run-flat; simulatedCamera=True',
    'WASTES RUN MOTION RESULT: case=run-flat; failures=0;',
    'WASTES HEIGHT POSITION SAMPLE: case=space-ascent; failed=False',
    'WASTES CLOSE ANCHOR RESULT: case=ground; failures=0;'
)
$inputPath=Join-Path $scratch 'synthetic-input.log'
$outputPath=Join-Path $scratch 'synthetic-export.log'
$source=@('[startup] account=FAKE_PRIVATE_SENTINEL', '[startup] system=FAKE_SYSTEM_SENTINEL')
$source+=@($expected | ForEach-Object {"[12:00:00] [apogean]: $_"})
$source+='[startup] WASTES UNKNOWN RECORD: FAKE_UNKNOWN_SENTINEL'
[IO.File]::WriteAllLines($inputPath,[string[]]$source)
$before=(Get-FileHash -LiteralPath $inputPath).Hash
& (Join-Path $PSScriptRoot 'Export-WastesModularTelemetry.ps1') -LogPath $inputPath -OutputPath $outputPath
$actual=@(Get-Content -LiteralPath $outputPath)
foreach($record in $expected) {
    if($actual -cnotcontains $record) {throw "MISSING_ALLOWED_RECORD: $record"}
}
if(($actual -join "`n") -cne ($expected -join "`n")) {throw 'UNEXPECTED_RECORD_OR_CHANGED_PAYLOAD'}
if((Get-FileHash -LiteralPath $inputPath).Hash -ne $before) {throw 'SOURCE_LOG_CHANGED'}
$rejectedOverwrite=$false
try { & (Join-Path $PSScriptRoot 'Export-WastesModularTelemetry.ps1') -LogPath $inputPath -OutputPath $inputPath }
catch { if($_.Exception.Message -like 'Preserve original log:*') {$rejectedOverwrite=$true} else {throw} }
if(-not $rejectedOverwrite -or (Get-FileHash -LiteralPath $inputPath).Hash -ne $before) {throw 'SOURCE_OVERWRITE_NOT_BLOCKED'}
Write-Host 'PASS: eight diagnostic record types preserved exactly; private/unknown startup records excluded; source and overwrite protection preserved.'
Write-Host "Synthetic exporter-only fixtures: $scratch"
