param([Parameter(Mandatory)][ValidateSet('build','test','day','night','capture','reload','release')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$path=Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanLiveValidation.request'
$stream=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read)
try { $bytes=[Text.Encoding]::UTF8.GetBytes("maw-material-$Case");$stream.Write($bytes,0,$bytes.Length) }
finally {$stream.Dispose()}
Write-Output "Queued maw-material-$Case for gg/V3/SP only. Await actual consumption."
