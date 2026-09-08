param([Parameter(Mandatory)][ValidateSet('view','save-quit')][string]$Case,
 [string]$SaveRoot=(Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader'))
$ErrorActionPreference='Stop'
$path=Join-Path $SaveRoot 'Captures/ApogeanArrivalSite.request'
if(Test-Path -LiteralPath $path){throw 'An arrival request is pending'}
[IO.File]::WriteAllText($path,$Case)
Write-Host "Requested $Case; requires gg in a new Apogee Arrival QA world. Never edits terrain."
