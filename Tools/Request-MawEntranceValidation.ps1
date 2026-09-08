param([Parameter(Mandatory)][ValidateSet('survey','surface','shallow')][string]$Case)
$ErrorActionPreference='Stop'
$path=Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanMawEntrance.request'
if(Test-Path -LiteralPath $path){throw 'A Maw entrance request is already pending.'}
if(-not (Test-Path -LiteralPath (Split-Path $path -Parent))){throw 'Captures directory must already exist.'}
[IO.File]::WriteAllText($path,$Case)
Write-Host "Requested $Case. Only gg in a disposable Apogee Arrival QA world can consume it. No terrain construction."
