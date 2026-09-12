param([Parameter(Mandatory)][ValidateSet('build','step','mature','test','reload','joins','view','capture','release',
    'anatomy-build','anatomy-test','anatomy-reload','anatomy-view','anatomy-capture','anatomy-release',
    'anatomy-vines-build','anatomy-vines-grow','anatomy-vines-test','anatomy-vines-reload','anatomy-vines-view','anatomy-vines-probe','anatomy-vines-audit','anatomy-vines-solar',
    'anatomy-amber-build','anatomy-amber-test','anatomy-amber-reload','anatomy-amber-awake','anatomy-amber-dormant','anatomy-amber-sample')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "maw-fiber-$Case"
Write-Output "Queued maw-fiber-$Case for packed gg/V3/SP only. Await actual consumption."
