param([Parameter(Mandatory)][string]$LogPath,[Parameter(Mandatory)][string]$OutputPath)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
# Publish only named QA records, not Steam/account/system startup diagnostics.
$records = @(Get-Content -LiteralPath $LogPath | ForEach-Object {
    if ($_ -match '(WASTES (?:MODULAR (?:RESULT|SAMPLE)|V1 (?:CAMERA|PROJECTION|GROUND LOCK|SWEEP)):[^\r\n]*)$') {
        $Matches[1]
    }
})
if ($records.Count -eq 0) { throw 'No allowed Wastes QA telemetry records found' }
if ([IO.Path]::GetFullPath($LogPath) -eq [IO.Path]::GetFullPath($OutputPath)) { throw 'Preserve original log: choose a separate output path' }
[IO.File]::WriteAllLines([IO.Path]::GetFullPath($OutputPath),[string[]]$records)
Write-Output "Exported $($records.Count) QA-only records. Original log unchanged."
