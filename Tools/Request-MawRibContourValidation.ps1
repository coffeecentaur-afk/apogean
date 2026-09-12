param([Parameter(Mandatory)][ValidateSet('build','pristine','reload','short','medium','long','inspect-on','inspect-off','capture','release')][string]$Case)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "maw-contour-$Case"
Write-Output 'Queued only; require fresh MAW RIB CONTOUR COMPLETE and preservation evidence. Separate specimen, no shallowV1 rebuild.'
