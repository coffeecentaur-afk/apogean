param([Parameter(Mandatory)][ValidateSet('build','test','day','night','capture','reload','release','save-and-quit')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$path = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanLiveValidation.request'
$request=if ($Case -eq 'save-and-quit') {'qa-save-and-quit'} else {"maw-fang-$Case"}
$stream = [IO.File]::Open($path, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::Read)
try { $bytes=[Text.Encoding]::UTF8.GetBytes($request); $stream.Write($bytes,0,$bytes.Length) } finally { $stream.Dispose() }
Write-Host "Queued $request. Only gg / Apogee Native Visual V3 / SP. Inspect native log for consumption; queueing is not proof."
