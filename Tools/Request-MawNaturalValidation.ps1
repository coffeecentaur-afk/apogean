param([Parameter(Mandatory)][ValidateSet('build','test','negative','properties','sand','seams','natural','corners','night','capture','reload','release')][string]$Case,[switch]$Playable)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$path=Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanLiveValidation.request'
$stream=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read)
$prefix=if($Playable){'maw-playable'}else{'maw-natural'}
try {$bytes=[Text.Encoding]::UTF8.GetBytes("$prefix-$Case");$stream.Write($bytes,0,$bytes.Length)}
finally {$stream.Dispose()}
Write-Output "Queued $prefix-$Case for gg/V3/SP only. Await actual consumption."
