param([Parameter(Mandatory)][ValidateSet('build','step','mature','test','reload','joins','view','capture','release',
    'anatomy-build','anatomy-test','anatomy-reload','anatomy-view','anatomy-capture','anatomy-release',
    'anatomy-vines-build','anatomy-vines-grow','anatomy-vines-test','anatomy-vines-reload','anatomy-vines-view','anatomy-vines-probe','anatomy-vines-audit',
    'anatomy-amber-build','anatomy-amber-test','anatomy-amber-reload','anatomy-amber-awake','anatomy-amber-dormant','anatomy-amber-sample')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$path=Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures/ApogeanLiveValidation.request'
$stream=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read)
try {$bytes=[Text.Encoding]::UTF8.GetBytes("maw-fiber-$Case");$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
Write-Output "Queued maw-fiber-$Case for packed gg/V3/SP only. Await actual consumption."
