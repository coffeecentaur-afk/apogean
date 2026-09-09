param([Parameter(Mandatory)][ValidateSet('build','test','day','night','capture','reload','release','reference')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$path = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanLiveValidation.request'
if (-not (Test-Path -LiteralPath (Split-Path -Parent $path))) { throw 'Captures directory must already exist.' }
# Atomic create prevents overwriting a pending request while Terraria is paused/unfocused.
$stream = [IO.File]::Open($path, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::Read)
try {
    $bytes = [Text.Encoding]::UTF8.GetBytes("maw-bone-$Case")
    $stream.Write($bytes, 0, $bytes.Length)
} finally { $stream.Dispose() }
Write-Host "Queued maw-bone-$Case. Only gg in Apogee Native Visual V3 single-player may consume it."
Write-Host 'Wait for consumption and inspect client.log before the next request. An unfocused game may pause; a queued request is not a test pass.'
