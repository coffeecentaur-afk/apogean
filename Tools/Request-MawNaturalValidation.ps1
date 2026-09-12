param([Parameter(Mandatory)][ValidateSet('build','test','negative','properties','sand','seams','joins','natural','corners','night','capture','reload','release')][string]$Case,[switch]$Playable)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$prefix=if($Playable){'maw-playable'}else{'maw-natural'}
& (Join-Path $PSScriptRoot 'Publish-QARequest.ps1') -Request "$prefix-$Case"
Write-Output "Queued $prefix-$Case for gg/V3/SP only. Await actual consumption."
